const version = "0.0.1";

// ======================================
// Initialisation
// ======================================
const requestVerificationToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
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
  if (filesMap.size > 0 && !hasUploadStarted()) {
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
  const fileSize = file.size;
  const fileName = file.name;

  const errorMessage =
    fileSize === 0
      ? "The selected file is empty"
      : fileSize > MAX_FILE_SIZE_BYTES
      ? `The selected file must be smaller than ${MAX_FILE_SIZE_MB}MB`
      : Array.from(filesMap.values()).some(
          (f) =>
            f.file &&
            f.file.name === fileName &&
            f.file.size === fileSize &&
            f.file.lastModified === file.lastModified
        )
      ? "The selected file has already been uploaded"
      : null;

  const status = errorMessage ? "failed" : "ready";

  filesMap.set(fileId, {
    file,
    status,
    uploadPercent: 0,
    errorMessage,
  });

  if (errorMessage) {
    showFileErrorMessageSummary(fileId);
  }

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
  clearFileErrorMessageSummary(fileId);
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

  entry.status = "uploading";
  entry.uploadPercent = 0;
  entry.errorMessage = null;
  renderFileItem(fileId);
  clearFileErrorMessageSummary(fileId);
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
  formData.append("fileId", fileId);

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
      entry.status = "uploaded";
      entry.uploadPercent = 100;
      entry.errorMessage = null;
      clearFileErrorMessageSummary(fileId);
    } else {
      entry.status = "failed";
      entry.errorMessage = xhr.responseText || "Upload failed";
      showFileErrorMessageSummary(fileId);
    }

    renderFileItem(fileId);
    updateInterface();
  };

  xhr.onerror = () => {
    entry.status = "failed";
    entry.errorMessage = "Network error";
    showFileErrorMessageSummary(fileId);
    renderFileItem(fileId);
    updateInterface();
  };

  xhr.send(formData);
}

async function downloadSingleFile(target) {
  const fileId = target.closest(".ofqual-file-list__item")?.id;
  if (!fileId) return;

  const response = await fetch(`${window.location.pathname}/download/${fileId}`, {
    method: "GET",
    headers: {
      RequestVerificationToken: requestVerificationToken,
    },
  });

  if (!response.ok) return;

  const blob = await response.blob();
  const entry = filesMap.get(fileId);
  const fileName = entry?.fileName || entry?.file?.name || "download";

  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

async function deleteSingleFile(fileId) {
  const formData = new FormData();
  formData.append("fileId", fileId);

  try {
    const response = await fetch(`${window.location.pathname}/delete`, {
      method: "POST",
      body: formData,
      headers: { RequestVerificationToken: requestVerificationToken },
    });

    if (!response.ok) {
      const errorMessage = await response.text();
      const entry = filesMap.get(fileId);
      if (entry) {
        entry.status = "failed";
        entry.errorMessage = errorMessage || "Failed to delete";
        renderFileItem(fileId);
      }
    }
  } catch {
    const entry = filesMap.get(fileId);
    if (entry) {
      entry.status = "failed";
      entry.errorMessage = "Failed to remove";
      renderFileItem(fileId);
    }
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

    for (const [fileId, fileInfo] of Object.entries(attachments)) {
      filesMap.set(fileId, {
        file: null,
        fileName: fileInfo.fileName,
        fileSize: fileInfo.length,
        status: "uploaded",
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
        ? getFailedTemplate(entry, percent)
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

function getFailedTemplate(entry, percent) {
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
        <div class="ofqual-file-list__progress-bar ofqual-file-list__progress-bar--red" style="width: ${percent}%"></div>
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
  const totalSizeBytes = getTotalSizeBytes();
  const uploadingCount = Array.from(filesMap.values()).filter(
    (f) => f.status === "uploading"
  ).length;
  const hasErrors = Array.from(filesMap.values()).some(
    (f) => f.status === "failed"
  );

  submitButton.disabled =
    hasErrors ||
    totalSizeBytes > MAX_TOTAL_SIZE_BYTES ||
    uploadingCount > 0;

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

function updateInterface() {
  updateFileSizeCount();
  updateFileCountProgress();
  updateButtonState();
}

// ======================================
// Error Summary
// ======================================
function showFileErrorMessageSummary(fileId) {
  const entry = filesMap.get(fileId);
  const errorList = errorSummary.querySelector("ul");
  if (!entry || !entry.errorMessage || !errorList) return;

  const existing = errorList.querySelector(`a[href="#${fileId}"]`);
  if (!existing) {
    errorList.insertAdjacentHTML(
      "beforeend",
      `<li><a href="#${fileId}">${entry.errorMessage}</a></li>`
    );
  }

  errorSummary.classList.remove("govuk-!-display-none");
}

function clearFileErrorMessageSummary(fileId) {
  const entry = filesMap.get(fileId);
  if (entry) entry.errorMessage = null;

  const errorList = errorSummary.querySelector("ul");
  if (!errorList) return;

  [...errorList.children].forEach((li) => {
    if (li.querySelector(`a[href="#${fileId}"]`)) {
      li.remove();
    }
  });

  if (errorList.children.length === 0) {
    errorSummary.classList.add("govuk-!-display-none");
  }
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
