const version = "0.0.2";

// ======================================
// Initialisation
// ======================================
const requestVerificationToken = document.querySelector(
  'input[name="__RequestVerificationToken"]'
)?.value;
const fileInput = document.getElementById("files");
const fileList = document.getElementById("files-list");
const fileCount = document.getElementById("files-count");
const fileSizeCount = document.getElementById("files-size-count");
const submitButton = document.getElementById("submit-form-group");
const errorSummary = document.getElementById("error-summary");

const MAX_FILE_SIZE_MB = 25;
const MAX_TOTAL_SIZE_MB = 100;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
const MAX_TOTAL_SIZE_BYTES = MAX_TOTAL_SIZE_MB * 1024 * 1024;

const filesMap = new Map();

// ======================================
// Event Handlers
// ======================================
document.addEventListener("DOMContentLoaded", () => {
  const fallbackList = document.getElementById("files-list-noscript");
  if (fallbackList) {
    fallbackList.remove();
  }

  updateButtonState();
  fetchAllFiles();
});

fileInput.addEventListener("change", (event) => {
  const files = Array.from(event.target.files);
  files.forEach(handleFileSelection);

  if (hasUploadStarted()) {
    startUploadProcess();
  }

  updateInterface();
  fileInput.value = "";
});

fileList.addEventListener("click", async (event) => {
  const target = event.target;
  if (!target) return;

  if (target.classList.contains("file-remove-link")) {
    event.preventDefault();
    removeFileFromFileList(target);
  } else if (target.classList.contains("file-retry-link")) {
    event.preventDefault();
    await retryFileUpload(target);
  } else if (target.classList.contains("file-download-link")) {
    event.preventDefault();
    await downloadSingleFile(target);
  }
});

submitButton.addEventListener("click", (event) => {
  const hasErrors = Array.from(filesMap.values()).some(
    (f) => f.status === "failed"
  );

  const hasErrorSummaryItems = errorSummary.querySelector("ul")?.children.length > 0;

  if (filesMap.size > 0 && (hasErrors || hasErrorSummaryItems)) {
    event.preventDefault();

    const firstErroredEntry = Array.from(filesMap.entries()).find(
      ([, entry]) => entry.status === "failed"
    );

    if (firstErroredEntry) {
      const [fileId] = firstErroredEntry;
      const targetElement = document.getElementById(fileId);
      if (targetElement) {
        targetElement.scrollIntoView({ behavior: "smooth", block: "center" });
        targetElement.focus({ preventScroll: true });
        return;
      }
    }

    if (hasErrorSummaryItems) {
      errorSummary.scrollIntoView({ behavior: "smooth", block: "start" });
      errorSummary.focus({ preventScroll: true });
    }
  } else if (filesMap.size > 0 && !hasUploadStarted()) {
    event.preventDefault();
    startUploadProcess();
  }
});

errorSummary.addEventListener("click", (event) => {
  const link = event.target;
  if (link.tagName !== "A") return;

  const href = link.getAttribute("href");
  if (!href?.startsWith("#")) return;

  const targetId = href.slice(1);
  const targetElement = document.getElementById(targetId);
  if (!targetElement) return;

  event.preventDefault();
  targetElement.scrollIntoView({ behavior: "smooth", block: "center" });
  targetElement.focus({ preventScroll: true });
});

// ======================================
// Core File Handling
// ======================================
function handleFileSelection(file) {
  const fileId = crypto.randomUUID();

  const entry = {
    attachmentId: null,
    file,
    status: "ready",
    uploadPercent: 0,
    errorMessage: null,
  };

  filesMap.set(fileId, entry);
  validateFileEntry(fileId, entry);

  renderFileToList(fileId);
  updateInterface();
}

function removeFileFromFileList(target) {
  const row = target.closest(".ofqual-file-list__item");
  if (!row) return;
  const fileId = row.id;
  const entry = filesMap.get(fileId);
  if (!entry) return;

  if (entry.status === "uploaded") {
    deleteSingleFile(fileId);
  }

  filesMap.delete(fileId);
  row.remove();
  updateInterface();
  announceToScreenReader(`File ${getDisplayName(entry)} removed.`, {
    polite: false,
  });
}

async function startUploadProcess() {
  const readyGuids = [];

  for (const [fileId, file] of filesMap.entries()) {
    if (file.status === "ready" && file.file) {
      readyGuids.push(fileId);
    }
  }

  if (readyGuids.length === 0) return;
  updateFileCountProgress();
  await Promise.all(readyGuids.map(uploadSingleFile));
}

async function retryFileUpload(target) {
  const fileId = target.closest(".ofqual-file-list__item")?.id;
  if (!fileId) return;

  const entry = filesMap.get(fileId);
  if (!entry || !entry.file) return;

  const isValid = validateFileEntry(fileId, entry);
  if (!isValid) return;

  entry.status = "uploading";
  entry.uploadPercent = 0;
  entry.errorMessage = null;

  renderFileItem(fileId);
  updateInterface();
  await uploadSingleFile(fileId);
}

async function uploadSingleFile(fileId) {
  const entry = filesMap.get(fileId);
  if (!entry || !entry.file) return;

  entry.status = "uploading";
  renderFileItem(fileId);
  updateInterface();

  const formData = new FormData();
  formData.append("file", entry.file);

  const xhr = new XMLHttpRequest();
  xhr.open("POST", `${window.location.pathname}/upload`);
  xhr.setRequestHeader("RequestVerificationToken", requestVerificationToken);

  xhr.upload.addEventListener("progress", (event) => {
    const percent = Math.round((event.loaded / event.total) * 100);
    entry.uploadPercent = percent;
    renderFileItem(fileId);
  });

  xhr.onreadystatechange = () => {
    if (xhr.readyState !== XMLHttpRequest.DONE) return;

    if (xhr.status === 200) {
      try {
        const attachmentId = JSON.parse(xhr.responseText);
        entry.status = "uploaded";
        entry.uploadPercent = 100;
        entry.errorMessage = null;
        entry.attachmentId = attachmentId;
      } catch {
        entry.status = "failed";
        entry.errorMessage = "Upload succeeded but returned invalid response";
      }
    } else {
      entry.status = "failed";
      entry.errorMessage = xhr.responseText || "Upload failed";
    }

    renderFileItem(fileId);
    updateInterface();
  };

  xhr.onerror = () => {
    entry.status = "failed";
    entry.errorMessage = "Network error";
    renderFileItem(fileId);
    updateInterface();
  };

  xhr.send(formData);
}

async function downloadSingleFile(target) {
  const fileId = target.closest(".ofqual-file-list__item")?.id;
  if (!fileId) return;

  const entry = filesMap.get(fileId);
  if (!entry || !entry.attachmentId) return;

  try {
    const response = await fetch(
      `${window.location.pathname}/download/${entry.attachmentId}`,
      {
        method: "GET",
        headers: {
          RequestVerificationToken: requestVerificationToken,
        },
      }
    );

    if (!response.ok) {
      console.error("Failed to download file");
      return;
    }

    const blob = await response.blob();
    const fileName = entry.fileName || entry.file?.name || "download";

    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  } catch {
    console.error("Download failed");
  }
}

async function deleteSingleFile(fileId) {
  const entry = filesMap.get(fileId);
  if (!entry || !entry.attachmentId) return;

  try {
    const response = await fetch(
      `${window.location.pathname}/delete/${entry.attachmentId}`,
      {
        method: "POST",
        headers: {
          RequestVerificationToken: requestVerificationToken,
        },
      }
    );

    if (!response.ok) {
      const errorMessage = await response.text();
      entry.status = "failed";
      entry.errorMessage = errorMessage || "Failed to delete";
      renderFileItem(fileId);
    }
  } catch {
    entry.status = "failed";
    entry.errorMessage = "Failed to remove";
    renderFileItem(fileId);
  }
}

async function fetchAllFiles() {
  try {
    const response = await fetch(`${window.location.pathname}/list`, {
      method: "GET",
      headers: { RequestVerificationToken: requestVerificationToken },
    });

    if (!response.ok) return;

    const attachments = await response.json();

    for (const fileInfo of attachments) {
      const fileId = crypto.randomUUID();
      filesMap.set(fileId, {
        attachmentId: fileInfo.attachmentId,
        file: null,
        fileName: fileInfo.fieldName,
        fileSize: fileInfo.length,
        status: "uploaded",
        uploadPercent: 100,
      });

      renderFileToList(fileId);
    }

    updateInterface();
  } catch {
    console.error("Failed to fetch uploaded files.");
  }
}

// ======================================
// File Rendering & UI
// ======================================
function renderFileToList(fileId) {
  const entry = filesMap.get(fileId);
  if (!entry) return;

  const list = document.querySelector(".ofqual-file-list");
  if (!list) return;

  const listItem = document.createElement("li");
  listItem.className = "ofqual-file-list__item";
  listItem.setAttribute("role", "listitem");
  listItem.setAttribute("id", fileId);

  list.appendChild(listItem);
  renderFileItem(fileId);
}

function renderFileItem(fileId) {
  const entry = filesMap.get(fileId);
  if (!entry) return;

  const row = document.querySelector(`.ofqual-file-list__item[id="${fileId}"]`);
  if (!row) return;

  row.classList.remove(
    "ofqual-file-list__item--error",
    "ofqual-file-list__item--preupload"
  );

  if (entry.status === "failed") {
    row.classList.add("ofqual-file-list__item--error");
  }

  if (
    entry.status === "ready" ||
    (entry.status === "failed" && entry.uploadPercent === 0)
  ) {
    row.classList.add("ofqual-file-list__item--preupload");
  }

  const percent = entry.uploadPercent || 0;
  let template = "";

  switch (entry.status) {
    case "uploaded":
      template = getUploadedTemplate(entry);
      break;
    case "uploading":
      template = getUploadingTemplate(entry, percent);
      break;
    case "failed":
      template = hasUploadStarted()
        ? getFailedTemplate(entry)
        : getPreUploadFailedTemplate(entry);
      break;
    case "ready":
    default:
      template = getReadyToUploadTemplate(entry);
      break;
  }

  row.innerHTML = template;
}

function getUploadingTemplate(entry, percent) {
  const name = getDisplayName(entry);
  const size = getDisplaySize(entry);

  return `
    <div class="ofqual-file-list__header">
      <span class="ofqual-file-list__name">
        ${name}
        <span class="govuk-visually-hidden"> - uploading</span>
      </span>
      <span class="ofqual-file-list__size">${size}</span>
    </div>
    <div class="ofqual-file-list__footer">
      <div class="ofqual-file-list__progress-wrapper ofqual-file-list__progress-wrapper--blue">
        <div class="ofqual-file-list__progress-bar ofqual-file-list__progress-bar--blue" style="width: ${percent}%"></div>
      </div>
      <a href="#" class="ofqual-file-list__action govuk-link file-remove-link">
        Cancel<span class="govuk-visually-hidden"> upload of ${name}</span>
      </a>
    </div>
    <div class="ofqual-file-list__status" aria-live="polite">Uploading ${percent}%</div>
  `;
}

function getUploadedTemplate(entry) {
  const name = getDisplayName(entry);
  const size = getDisplaySize(entry);

  return `
    <div class="ofqual-file-list__header">
      <a href="#" class="ofqual-file-list__name govuk-link file-download-link">
        ${name}
        <span class="govuk-visually-hidden"> - download file</span>
      </a>
      <span class="ofqual-file-list__size">${size}</span>
    </div>
    <div class="ofqual-file-list__footer">
      <div class="ofqual-file-list__progress-wrapper ofqual-file-list__progress-wrapper--green">
        <div class="ofqual-file-list__progress-bar ofqual-file-list__progress-bar--green" style="width: 100%;"></div>
      </div>
      <a href="#" class="ofqual-file-list__action govuk-link file-remove-link">
        Remove<span class="govuk-visually-hidden"> ${name}</span>
      </a>
    </div>
    <div class="ofqual-file-list__status" aria-live="polite">Upload complete</div>
  `;
}

function getFailedTemplate(entry) {
  const name = getDisplayName(entry);
  const size = getDisplaySize(entry);

  return `
    <div class="ofqual-file-list__header">
      <span class="ofqual-file-list__name">
        ${name}
        <span class="govuk-visually-hidden"> - upload failed</span>
      </span>
      <span class="ofqual-file-list__size">${size}</span>
    </div>
    <div class="ofqual-file-list__footer">
      <div class="ofqual-file-list__progress-wrapper ofqual-file-list__progress-wrapper--red">
        <div class="ofqual-file-list__progress-bar ofqual-file-list__progress-bar--red" style="width: 100%"></div>
      </div>
      <div class="ofqual-file-list__actions">
        <a href="#" class="ofqual-file-list__action govuk-link file-retry-link">
          Retry<span class="govuk-visually-hidden"> upload of ${name}</span>
        </a>
        <a href="#" class="ofqual-file-list__action govuk-link file-remove-link">
          Remove<span class="govuk-visually-hidden"> ${name}</span>
        </a>
      </div>
    </div>
    <div class="ofqual-file-list__status" role="alert">${entry.errorMessage}</div>
  `;
}

function getPreUploadFailedTemplate(entry) {
  const name = getDisplayName(entry);
  const size = getDisplaySize(entry);

  return `
    <div class="ofqual-file-list__header">
      <span class="ofqual-file-list__name">
        ${name}
        <span class="govuk-visually-hidden"> - file validation failed</span>
      </span>
      <span class="ofqual-file-list__size">${size}</span>
    </div>
    <div class="ofqual-file-list__footer">
      <div class="ofqual-file-list__status" role="alert">${entry.errorMessage}</div>
      <div class="ofqual-file-list__actions">
        <a href="#" class="ofqual-file-list__action govuk-link file-remove-link">
          Remove<span class="govuk-visually-hidden"> ${name}</span>
        </a>
      </div>
    </div>
  `;
}

function getReadyToUploadTemplate(entry) {
  const name = getDisplayName(entry);
  const size = getDisplaySize(entry);

  return `
    <div class="ofqual-file-list__header">
      <span class="ofqual-file-list__name">${name}</span>
      <span class="ofqual-file-list__size">${size}</span>
    </div>
    <div class="ofqual-file-list__footer">
      <div class="ofqual-file-list__status" aria-live="polite">Ready to upload</div>
        <a href="#" class="ofqual-file-list__action govuk-link file-remove-link">
          Remove<span class="govuk-visually-hidden"> ${name}</span>
        </a>
    </div>
  `;
}

// ======================================
// Interface Updates
// ======================================
function updateButtonState() {
  const uploadingCount = Array.from(filesMap.values()).filter(
    (f) => f.status === "uploading"
  ).length;

  submitButton.disabled = uploadingCount > 0;

  if (uploadingCount > 0) {
    submitButton.innerText = "Uploading...";
  } else if (!hasUploadStarted()) {
    submitButton.innerText = "Upload files";
  } else {
    submitButton.innerText = "Submit files";
  }
}

function updateFileCountProgress() {
  fileCount.classList.remove("govuk-!-display-none");
  const total = filesMap.size;
  const totalUploaded = Array.from(filesMap.values()).filter(
    (f) => f.status === "uploaded"
  ).length;
  const filesToUpload = Array.from(filesMap.values()).filter(
    (f) => f.status === "ready" && f.file
  ).length;

  if (hasUploadStarted()) {
    fileCount.innerText = `${totalUploaded} of ${total} ${
      total === 1 ? "file" : "files"
    } uploaded`;
  } else {
    fileCount.innerText = `${filesToUpload} of ${total} ${
      total === 1 ? "file" : "files"
    } ready to upload`;
  }
}

function updateFileSizeCount() {
  const totalSizeBytes = getTotalSizeBytes();
  fileSizeCount.innerText = `${formatFileSize(
    totalSizeBytes
  )} of ${MAX_TOTAL_SIZE_MB}MB used`;
}

function updateFileErrorSummary() {
  const errorList = errorSummary.querySelector("ul");
  if (!errorList) return;

  const activeErrors = Array.from(filesMap.entries()).filter(
    ([, entry]) => entry.errorMessage
  );

  const totalSize = getTotalSizeBytes();
  const totalSizeErrorId = "__total_size_limit__";
  const totalSizeErrorMessage = `Total file size must not exceed ${MAX_TOTAL_SIZE_MB} MB`;

  const totalSizeErrorIndex = activeErrors.findIndex(
    ([id]) => id === totalSizeErrorId
  );
  if (totalSize > MAX_TOTAL_SIZE_BYTES) {
    if (totalSizeErrorIndex === -1) {
      activeErrors.push([
        totalSizeErrorId,
        { errorMessage: totalSizeErrorMessage },
      ]);
    }
  } else if (totalSizeErrorIndex !== -1) {
    activeErrors.splice(totalSizeErrorIndex, 1);
  }

  const existingAnchors = new Map(
    Array.from(errorList.children).map((li) => {
      const anchor = li.querySelector("a");
      const href = anchor?.getAttribute("href");
      return [href, li];
    })
  );

  for (const [fileId, entry] of activeErrors) {
    const href = fileId === totalSizeErrorId ? `#upload-section` : `#${fileId}`;
    if (!existingAnchors.has(href)) {
      const li = document.createElement("li");
      const a = document.createElement("a");
      a.href = href;
      a.textContent = entry.errorMessage;
      li.appendChild(a);
      errorList.appendChild(li);
    } else {
      const li = existingAnchors.get(href);
      const a = li?.querySelector("a");
      if (a && a.textContent !== entry.errorMessage) {
        a.textContent = entry.errorMessage;
      }
      existingAnchors.delete(href);
    }
  }

  for (const li of existingAnchors.values()) {
    li.remove();
  }

  if (activeErrors.length > 0) {
    errorSummary.classList.remove("govuk-!-display-none");
  } else {
    errorSummary.classList.add("govuk-!-display-none");
  }
}

function updateInterface() {
  updateFileSizeCount();
  updateFileCountProgress();
  updateButtonState();
  updateFileErrorSummary();
}

// ======================================
// Error Handling
// ======================================
function validateFileEntry(fileId, entry) {
  if (!entry.file) {
    entry.errorMessage = null;
    entry.status = "uploaded";
    return true;
  }

  const { name: fileName, size: fileSize, lastModified } = entry.file;
  let errorMessage = null;

  const allFiles = Array.from(filesMap.entries());
  const otherFiles = allFiles.filter(([id]) => id !== fileId);

  if (fileSize === 0) {
    errorMessage = "The selected file is empty";
  } else if (fileSize > MAX_FILE_SIZE_BYTES) {
    errorMessage = `The file must be smaller than ${MAX_FILE_SIZE_MB}MB`;
  } else {
    const isDuplicate = otherFiles.some(
      ([, otherEntry]) =>
        otherEntry.file &&
        otherEntry.file.name === fileName &&
        otherEntry.file.size === fileSize &&
        otherEntry.file.lastModified === lastModified
    );

    if (isDuplicate) {
      errorMessage = "This file is a duplicate of another file";
    }
  }

  entry.errorMessage = errorMessage;
  entry.status = errorMessage ? "failed" : "ready";
  return !errorMessage;
}

// ======================================
// Accessibility
// ======================================
function announceToScreenReader(message, options = {}) {
  const isPolite = options.polite || false;
  const regionId = "files-aria-status";

  let region = document.getElementById(regionId);

  if (!region) {
    region = document.createElement("div");
    region.id = regionId;
    region.className = "govuk-visually-hidden";
    region.setAttribute("aria-live", isPolite ? "polite" : "assertive");
    region.setAttribute("aria-atomic", "true");
    document.body.appendChild(region);
  } else {
    region.setAttribute("aria-live", isPolite ? "polite" : "assertive");
  }

  region.textContent = "";
  setTimeout(() => {
    region.textContent = message;
  }, 100);
}

// ======================================
// Helpers Functions
// ======================================
function formatFileSize(bytes) {
  if (bytes < 1000) return bytes + "B";
  if (bytes < 1000 * 1000) return (bytes / 1000).toFixed(1) + "KB";
  if (bytes < 1000 * 1000 * 1000)
    return (bytes / (1000 * 1000)).toFixed(1) + "MB";
  return (bytes / (1000 * 1000 * 1000)).toFixed(1) + "GB";
}

function getTotalSizeBytes() {
  let total = 0;
  for (const entry of filesMap.values()) {
    const size = entry.file?.size || entry.fileSize || 0;
    total += size;
  }
  return total;
}

function hasUploadStarted() {
  return Array.from(filesMap.values()).some(
    (f) => f.status === "uploading" || f.status === "uploaded"
  );
}

function getDisplayName(entry) {
  return entry.file?.name || entry.fileName || "Unnamed file";
}

function getDisplaySize(entry) {
  return formatFileSize(entry.file?.size || entry.fileSize || 0);
}
