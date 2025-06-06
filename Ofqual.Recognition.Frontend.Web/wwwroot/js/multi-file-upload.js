const version = "0.0.1";

// ======================================
// Initialisation
// ======================================
const requestVerificationToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
const fileInput = document.getElementById("file-upload-input");
const fileList = document.getElementById("file-list");
const fileCount = document.getElementById("file-count");
const fileSizeCount = document.getElementById("file-size-count");
const uploadFilesButton = document.getElementById("upload-files-button");
const errorSummary = document.querySelector(".govuk-error-summary");

const MAX_FILE_SIZE_MB = 25;
const MAX_TOTAL_SIZE_MB = 100;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
const MAX_TOTAL_SIZE_BYTES = MAX_TOTAL_SIZE_MB * 1024 * 1024;

const filesMap = new Map();

// ======================================
// Event Handlers
// ======================================
document.addEventListener("DOMContentLoaded", () => {
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

uploadFilesButton.addEventListener("click", (event) => {
  if (!hasUploadStarted()) {
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
  const guid = crypto.randomUUID();
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

  filesMap.set(guid, {
    file,
    status,
    uploadPercent: 0,
    errorMessage,
  });

  if (errorMessage) {
    showFileErrorMessageSummary(guid);
  }

  renderFileToList(guid);
  updateInterface();
}

function removeFileFromFileList(target) {
  const row = target.closest(".ofqual-file-list__item");
  if (!row) return;
  const guid = row.id;
  const entry = filesMap.get(guid);
  if (!entry) return;

  if (entry.status === "uploaded") {
    deleteSingleFile(guid);
  }

  filesMap.delete(guid);
  clearFileErrorMessageSummary(guid);
  row.remove();
  updateInterface();
  announceToScreenReader(`File ${getDisplayName(entry)} removed.`, { polite: false });
}

async function startUploadProcess() {
  const readyGuids = [];

  for (const [guid, file] of filesMap.entries()) {
    if (file.status === "ready" && file.file) {
      readyGuids.push(guid);
    }
  }

  if (readyGuids.length === 0) return;
  updateFileCountProgress();
  await Promise.all(readyGuids.map(uploadSingleFile));
}

async function uploadSingleFile(guid) {
  const entry = filesMap.get(guid);
  if (!entry || !entry.file) return;

  entry.status = "uploading";
  renderFileItem(guid);
  updateInterface();

  const formData = new FormData();
  formData.append("document", entry.file);
  formData.append("guid", guid);

  const xhr = new XMLHttpRequest();
  xhr.open("POST", "./supportingDocument?handler=UploadSingleFile");
  xhr.setRequestHeader("RequestVerificationToken", requestVerificationToken);

  xhr.upload.addEventListener("progress", (event) => {
    const percent = Math.round((event.loaded / event.total) * 100);
    entry.uploadPercent = percent;
    renderFileItem(guid);
  });

  xhr.onreadystatechange = () => {
    if (xhr.readyState !== XMLHttpRequest.DONE) return;

    try {
      const response = JSON.parse(xhr.responseText);
      if (xhr.status !== 200) {
        entry.status = "failed";
        entry.errorMessage = response.errorMessage || "Upload failed";
        showFileErrorMessageSummary(guid);
      } else {
        entry.status = "uploaded";
        entry.uploadPercent = 100;
        entry.errorMessage = null;
        clearFileErrorMessageSummary(guid);
      }
    } catch {
      entry.status = "failed";
      entry.errorMessage = "Invalid server response";
      showFileErrorMessageSummary(guid);
    }

    renderFileItem(guid);
    updateInterface();
  };

  xhr.onerror = () => {
    entry.status = "failed";
    entry.errorMessage = "Network error";
    showFileErrorMessageSummary(guid);
    renderFileItem(guid);
    updateInterface();
  };

  xhr.send(formData);
}

async function downloadSingleFile(target) {
  const guid = target.closest(".ofqual-file-list__item")?.id;
  if (!guid) return;

  const response = await fetch(
    `./supportingDocument?handler=DownloadSingleFile&guid=${guid}`,
    {
      method: "GET",
      headers: {
        RequestVerificationToken: requestVerificationToken,
      },
    }
  );

  if (!response.ok) return;

  const blob = await response.blob();
  const entry = filesMap.get(guid);
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

async function retryFileUpload(target) {
  const guid = target.closest(".ofqual-file-list__item")?.id;
  if (!guid) return;
  const entry = filesMap.get(guid);
  if (!entry || !entry.file) return;

  entry.status = "uploading";
  entry.uploadPercent = 0;
  entry.errorMessage = null;
  renderFileItem(guid);
  clearFileErrorMessageSummary(guid);
  await uploadSingleFile(guid);
}

async function deleteSingleFile(guid) {
  const formData = new FormData();
  formData.append("guid", guid);

  try {
    const response = await fetch(
      "./supportingDocument?handler=DeleteSingleFile",
      {
        method: "POST",
        body: formData,
        headers: { RequestVerificationToken: requestVerificationToken },
      }
    );

    if (!response.ok) {
      const result = await response.json();
      const entry = filesMap.get(guid);
      if (entry) {
        entry.status = "failed";
        entry.errorMessage = result.errorMessage || "Failed to delete";
        renderFileItem(guid);
      }
    }
  } catch {
    const entry = filesMap.get(guid);
    if (entry) {
      entry.status = "failed";
      entry.errorMessage = "Failed to remove";
      renderFileItem(guid);
    }
  }
}

async function fetchAllFiles() {
  try {
    const response = await fetch("./supportingDocument?handler=AllFiles", {
      method: "GET",
      headers: { RequestVerificationToken: requestVerificationToken },
    });

    if (!response.ok) return;

    const result = await response.json();

    Object.entries(result).forEach(([guid, file]) => {
      filesMap.set(guid, {
        file: null,
        fileName: file.fileName,
        fileSize: file.length,
        status: "uploaded",
      });
      renderFileToList(guid);
    });

    updateInterface();
  } catch {
    console.error("Failed to fetch files.");
  }
}

// ======================================
// File Rendering & UI
// ======================================
function renderFileToList(guid) {
  const entry = filesMap.get(guid);
  if (!entry) return;

  const list = document.querySelector(".ofqual-file-list");
  if (!list) return;

  const listItem = document.createElement("li");
  listItem.className = "ofqual-file-list__item";
  listItem.setAttribute("role", "listitem");
  listItem.setAttribute("id", guid);

  list.appendChild(listItem);
  renderFileItem(guid);
}

function renderFileItem(guid) {
  const entry = filesMap.get(guid);
  if (!entry) return;

  const row = document.querySelector(`.ofqual-file-list__item[id="${guid}"]`);
  if (!row) return;

  row.classList.remove(
    "ofqual-file-list__item--error",
    "ofqual-file-list__item--preupload"
  );

  if (entry.status === "failed") {
    row.classList.add("ofqual-file-list__item--error");
  }

  if (entry.status === "ready" || (entry.status === "failed" && entry.uploadPercent === 0))
  {
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
  const totalFiles = filesMap.size;
  const uploadingCount = Array.from(filesMap.values()).filter((f) => f.status === "uploading").length;
  const hasErrors = Array.from(filesMap.values()).some((f) => f.status === "failed");

  uploadFilesButton.disabled =
    hasErrors ||
    totalFiles === 0 ||
    totalSizeBytes > MAX_TOTAL_SIZE_BYTES ||
    uploadingCount > 0;
  
  if (uploadingCount > 0) {
    uploadFilesButton.innerText = "Uploading...";
  } else if (!hasUploadStarted()) {
    uploadFilesButton.innerText = "Upload files";
  } else {
    uploadFilesButton.innerText = "Submit files";
  }
}

function updateFileCountProgress() {
  fileCount.classList.remove("govuk-!-display-none");
  const total = filesMap.size;
  const totalUploaded = Array.from(filesMap.values()).filter((f) => f.status === "uploaded").length;
  const filesToUpload = Array.from(filesMap.values()).filter((f) => f.status === "ready" && f.file).length;

  if (hasUploadStarted()) {
    fileCount.innerText = `${totalUploaded} of ${total} ${total === 1 ? "file" : "files"} uploaded`;
  } else {
    fileCount.innerText = `${filesToUpload} of ${total} ${total === 1 ? "file" : "files"} ready to upload`;
  }
}

function updateFileSizeCount() {
  const totalSizeBytes = getTotalSizeBytes();
  fileSizeCount.innerText = `${formatFileSize(totalSizeBytes)} of ${MAX_TOTAL_SIZE_MB}MB used`;
}

function updateInterface() {
  updateFileSizeCount();
  updateFileCountProgress();
  updateButtonState();
}

// ======================================
// Error Summary
// ======================================
function showFileErrorMessageSummary(guid) {
  const entry = filesMap.get(guid);
  const errorList = errorSummary.querySelector("ul");
  if (!entry || !entry.errorMessage || !errorList) return;

  const existing = errorList.querySelector(`a[href="#${guid}"]`);
  if (!existing) {
    errorList.insertAdjacentHTML(
      "beforeend",
      `<li><a href="#${guid}">${entry.errorMessage}</a></li>`
    );
  }

  errorSummary.classList.remove("govuk-!-display-none");
}

function clearFileErrorMessageSummary(guid) {
  const entry = filesMap.get(guid);
  if (entry) entry.errorMessage = null;

  const errorList = errorSummary.querySelector("ul");
  if (!errorList) return;

  [...errorList.children].forEach((li) => {
    if (li.querySelector(`a[href="#${guid}"]`)) {
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
  const regionId = options.regionId || "aria-status";
  const isPolite = options.polite || false;

  let region = document.getElementById(regionId);

  if (!region) {
    region = document.createElement("div");
    region.id = regionId;
    region.className = "govuk-visually-hidden";
    region.setAttribute("aria-live", isPolite ? "polite" : "assertive");
    region.setAttribute("aria-atomic", "true");
    document.body.appendChild(region);
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
  return Array.from(filesMap.values()).some((f) => f.status === "uploading" || f.status === "uploaded");
}

function getDisplayName(entry) {
  return entry.file?.name || entry.fileName || "Unnamed file";
}

function getDisplaySize(entry) {
  return formatFileSize(entry.file?.size || entry.fileSize || 0);
}