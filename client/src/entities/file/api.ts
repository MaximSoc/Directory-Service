import { apiClient } from "@/shared/api/axios-instance";
import { AssetType, OwnerType } from "./types";
import { Envelope } from "@/shared/api/envelope";
import axios from "axios";

export type StartMultipartUploadRequest = {
  fileName: string;
  contentType: string;
  size: number;
  assetType: AssetType;
  contextId: string;
  context: OwnerType;
};

export type ChunkUploadUrl = {
  partNumber: number;
  uploadUrl: string;
};

export type StartMultipartUploadResponse = {
  mediaAssetId: string;
  uploadId: string;
  chunkUploadUrls: ChunkUploadUrl[];
  chunkSize: number;
};

export type PartETag = {
  partNumber: number;
  eTag: string;
};

export type CompleteMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
  partETags: PartETag[];
};

export type AbortMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
};

export const fileApi = {
  startMultipartUpload: async (
    request: StartMultipartUploadRequest
  ): Promise<StartMultipartUploadResponse> => {
    const response = await apiClient.post<
      Envelope<StartMultipartUploadResponse>
    >("/files/multipart/start", request);

    return response.data.result!;
  },

  uploadChunk: async (
    uploadUrl: string,
    chunk: Blob,
    signal?: AbortSignal
  ): Promise<string> => {
    const response = await axios.put(uploadUrl, chunk, {
      headers: {
        "Content-Type": "application/octet-stream",
      },
      signal,
    });

    const eTag = response.headers.etag?.replace(/"/g, "") || "";

    return eTag;
  },

  completeMultipartUpload: async (
    request: CompleteMultipartUploadRequest
  ): Promise<void> => {
    await apiClient.post("/files/multipart/complete", request);
  },

  abortMultipartUpload: async (
    request: AbortMultipartUploadRequest
  ): Promise<void> => {
    await apiClient.post("/files/multipart/abort", request);
  },
};
