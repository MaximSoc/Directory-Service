export type AssetType = "video" | "preview";

export type OwnerType = "department" | "location" | "position";

export type UploadStatus =
  | "idle"
  | "uploading"
  | "completing"
  | "completed"
  | "failed";

export type FileStatus =
  | "uploading"
  | "uploaded"
  | "processing"
  | "ready"
  | "failed"
  | "deleted";

export type UploadProgress = {
  status: UploadStatus;
  progress: number;
  uploadedBytes: number;
  totalBytes: number;
  fileName?: string;
  fileSize?: number;
  error?: string;
  mediaAssetId?: string;
};

export type FileInfo = {
  id: string;
  url: string | null;
  status: FileStatus;
};

export type FileValidatorConfig = {
  maxSize: number;
  allowedExtensions: string[];
  allowedTypes: string[];
  label: string;
  description: string;
};

export type FileValidationResult =
  | { valid: true }
  | { valid: false; error: string };
