export type ApiError = {
  messages: ErrorMessage[];
  type: FrontendErrorType;
};

export type BackendErrorType = 0 | 1 | 2 | 3 | 4 | 5;

export type ErrorMessage = {
  code: string;
  message: string;
  type: BackendErrorType;
  invalidField?: string | null;
};

export type FrontendErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "authentification"
  | "authorization";

export const BACKEND_TYPE_TO_FRONTEND: Record<
  BackendErrorType,
  FrontendErrorType
> = {
  0: "validation",
  1: "not_found",
  2: "failure",
  3: "conflict",
  4: "authentification",
  5: "authorization",
};

export class EnvelopeError extends Error {
  public readonly apiError: ApiError;
  public readonly type: FrontendErrorType;

  constructor(apiError: ApiError) {
    const firstMessage = apiError.messages[0].message ?? "Неизвестная ошибка";

    super(firstMessage);

    this.name = "EnvelopeError";
    this.apiError = apiError;
    this.type = apiError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  getMessages(): ErrorMessage[] {
    return this.apiError.messages;
  }

  getFirstMessage(): string {
    return this.apiError.messages[0].message ?? "Неизвестная ошибка";
  }

  getAllMessages(): string[] {
    return this.apiError.messages.map((msg) => msg.message);
  }

  getErrorType(): FrontendErrorType {
    return this.type;
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}
