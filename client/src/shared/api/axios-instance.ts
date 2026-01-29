import axios from "axios";
import { Envelope, RawEnvelope } from "./envelope";
import {
  ApiError,
  BackendErrorType,
  EnvelopeError,
  BACKEND_TYPE_TO_FRONTEND,
} from "./errors";

export const apiClient = axios.create({
  baseURL: "http://localhost:8080/api",
});

apiClient.interceptors.response.use(
  (response) => {
    const raw = response.data as RawEnvelope<unknown>;

    if (raw && raw.isError && raw.errorList && raw.errorList.length > 0) {
      const apiError: ApiError = {
        messages: raw.errorList,
        type:
          BACKEND_TYPE_TO_FRONTEND[raw.errorList[0].type as BackendErrorType] ??
          "failure",
      };
      throw new EnvelopeError(apiError);
    }

    const normalized: Envelope<unknown> = {
      result: raw.result,
      isError: raw.isError,
      timeGenerated: raw.timeGenerated,
      error: raw.errorList
        ? {
            messages: raw.errorList,
            type:
              BACKEND_TYPE_TO_FRONTEND[
                raw.errorList[0].type as BackendErrorType
              ] ?? "failure",
          }
        : null,
    };

    response.data = normalized;

    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const raw = error.response.data as RawEnvelope<unknown>;

      if (raw && raw.isError && raw.errorList && raw.errorList.length > 0) {
        const apiError: ApiError = {
          messages: raw.errorList,
          type:
            BACKEND_TYPE_TO_FRONTEND[
              raw.errorList[0].type as BackendErrorType
            ] ?? "failure",
        };
        throw new EnvelopeError(apiError);
      }
    }

    return Promise.reject(error);
  }
);
