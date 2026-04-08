import { useRef, useState } from "react";
import { AssetType, OwnerType, UploadProgress } from "../types";
import {
  ChunkUploadUrl,
  fileApi,
  PartETag,
  StartMultipartUploadResponse,
} from "../api";
import { isEnvelopeError } from "@/shared/api/errors";
import { validateFile } from "../lib/validators";

export type Props = {
  ownerId: string;
  ownerType: OwnerType;
  assetType: AssetType;
  onAbort?: () => void;
};

export function useFileUpload({
  ownerId,
  ownerType,
  assetType,
  onAbort,
}: Props) {
  const [uploadState, setUploadState] = useState<UploadProgress>({
    status: "idle",
    progress: 0,
    uploadedBytes: 0,
    totalBytes: 0,
  });

  const abortControllerRef = useRef<AbortController | null>(null);
  const currentUploadRef = useRef<{
    mediaAssetId: string;
    uploadId: string;
  } | null>(null);

  const abort = async () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    if (currentUploadRef.current) {
      try {
        await fileApi.abortMultipartUpload({
          mediaAssetId: currentUploadRef.current.mediaAssetId,
          uploadId: currentUploadRef.current.uploadId,
        });
      } catch (e) {
        console.error("Ошибка при отмене:", e);
      }
    }

    setUploadState({
      status: "idle",
      progress: 0,
      uploadedBytes: 0,
      totalBytes: 0,
    });
    currentUploadRef.current = null;

    onAbort?.();
  };

  const upload = async (file: File): Promise<string | undefined> => {
    const validation = validateFile(file, assetType);
    if (!validation.valid) {
      setUploadState({
        status: "failed",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
        error: validation.error,
      });
      return undefined;
    }

    try {
      abortControllerRef.current = new AbortController();

      setUploadState({
        status: "uploading",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
      });

      const uploadData: StartMultipartUploadResponse =
        await fileApi.startMultipartUpload({
          fileName: file.name,
          contentType: file.type,
          size: file.size,
          assetType,
          contextId: ownerId,
          context: ownerType,
        });

      const { mediaAssetId, uploadId, chunkUploadUrls, chunkSize } = uploadData;

      currentUploadRef.current = { mediaAssetId, uploadId };

      const partETags = await uploadChunks(
        file,
        chunkUploadUrls,
        chunkSize,
        abortControllerRef.current.signal
      );

      await fileApi.completeMultipartUpload({
        mediaAssetId,
        uploadId,
        partETags,
      });

      setUploadState({
        status: "completed",
        progress: 100,
        uploadedBytes: file.size,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
        mediaAssetId,
      });

      currentUploadRef.current = null;

      return mediaAssetId;
    } catch (error: unknown) {
      if (
        error instanceof Error &&
        (error.name === "CanceledError" || error.name === "AbortError")
      ) {
        return undefined;
      }

      const errorMessage = isEnvelopeError(error)
        ? error.getFirstMessage()
        : error instanceof Error
        ? error.message
        : "Ошибка загрузки файла";

      setUploadState((prev) => ({
        ...prev,
        status: "failed",
        error: errorMessage,
      }));
    }
  };

  const uploadChunks = async (
    file: File,
    chunks: ChunkUploadUrl[],
    chunkSize: number,
    signal: AbortSignal
  ): Promise<PartETag[]> => {
    const results: PartETag[] = [];

    for (let i = 0; i < chunks.length; i++) {
      const chunkInfo = chunks[i];
      const start = i * chunkSize;
      const end = Math.min(start + chunkSize, file.size);
      const chunk = file.slice(start, end);

      const eTag = await fileApi.uploadChunk(
        chunkInfo.uploadUrl,
        chunk,
        signal
      );

      results.push({
        partNumber: chunkInfo.partNumber,
        eTag,
      });

      const progress = Math.round(((i + 1) / chunks.length) * 100);
      const uploadedBytes = Math.min((i + 1) * chunkSize, file.size);

      setUploadState((prev) => ({ ...prev, progress, uploadedBytes }));
    }

    return results;
  };

  return {
    upload,
    abort,
    uploadState,
    isIdle: uploadState.status === "idle",
    isUploading: uploadState.status === "uploading",
    isCompleted: uploadState.status === "completed",
    isFailed: uploadState.status === "failed",
    error: uploadState.error,
  };
}
