import { ApiError, ErrorMessage } from "./errors";

export type RawEnvelope<T = unknown> = {
  result: T | null;
  errorList: ErrorMessage[] | null;
  isError: boolean;
  timeGenerated: string;
};

export type Envelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};
