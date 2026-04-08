import { AssetType, FileValidationResult, FileValidatorConfig } from "../types";

const KB = 1024;
const MB = KB * 1024;
const GB = MB * 1024;

export const fileValidators: Record<AssetType, FileValidatorConfig> = {
  video: {
    maxSize: 3 * GB,
    allowedExtensions: [".mp4", ".webm", ".mov", ".avi", ".mkv"],
    allowedTypes: [
      "video/mp4",
      "video/webm",
      "video/quicktime",
      "video/x-msvideo",
      "video/x-matroska",
    ],
    label: "Видео",
    description: "MP4, WebM, MOV до 3 Гб",
  },

  preview: {
    maxSize: 10 * MB,
    allowedExtensions: [".jpg", ".jpeg", ".png", ".webp"],
    allowedTypes: ["image/jpeg", "image/png", "image/webp"],
    label: "Превью",
    description: "JPG, PNG, WebP до 10 Мб",
  },
};

export function formatFileSize(bytes: number): string {
  if (bytes < KB) return `${bytes} Б`;
  if (bytes < MB) return `${(bytes / KB).toFixed(1)} КБ`;
  if (bytes < GB) return `${(bytes / MB).toFixed(1)} МБ`;
  return `${(bytes / GB).toFixed(2)} ГБ`;
}

function getFileExtension(fileName: string): string {
  const lastDot = fileName.lastIndexOf(".");
  if (lastDot === -1) return "";
  return fileName.slice(lastDot).toLowerCase();
}

export function validateFile(
  file: File,
  assetType: AssetType
): FileValidationResult {
  const config = fileValidators[assetType];

  if (file.size > config.maxSize) {
    return {
      valid: false,
      error: `Файл слишком большой. Максимальный размер: ${formatFileSize(
        config.maxSize
      )}`,
    };
  }

  if (file.size === 0) {
    return {
      valid: false,
      error: "Файл пустой",
    };
  }

  if (!config.allowedTypes.includes(file.type)) {
    return {
      valid: false,
      error: `Неподдерживаемый тип файла. Разрешены: ${config.allowedExtensions.join(
        ", "
      )}`,
    };
  }

  const extension = getFileExtension(file.name);
  if (!config.allowedExtensions.includes(extension)) {
    return {
      valid: false,
      error: `Неподдерживаемое расширение файла. Разрешены: ${config.allowedExtensions.join(
        ", "
      )}`,
    };
  }

  return { valid: true };
}

export function getAcceptString(assetType: AssetType): string {
  const config = fileValidators[assetType];
  return config.allowedTypes.join(",");
}

export function getValidatorConfig(assetType: AssetType): FileValidatorConfig {
  return fileValidators[assetType];
}
