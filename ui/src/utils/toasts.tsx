import { SuccessToast } from "../components/SuccessToast";
import { InfoToast } from "../components/InfoToast";
import { ErrorToast } from "../components/ErrorToast";
import { ExternalToast, toast } from "sonner";
import { useCallback } from "react";
import { HTTPError } from "ky";

const DEFAULT_TOAST_DURATION_MS = 2_250;

const DEFAULT_TOAST_OPTIONS: ExternalToast = {
  duration: DEFAULT_TOAST_DURATION_MS,
};

export function useShowSuccessToast(message: string) {
  return useCallback(
    () =>
      toast.custom(
        (t) => <SuccessToast toastId={t} message={message} />,
        DEFAULT_TOAST_OPTIONS,
      ),
    [message],
  );
}

export function useShowInfoToast(message: string) {
  return useCallback(
    () =>
      toast.custom(
        (t) => <InfoToast toastId={t} message={message} />,
        DEFAULT_TOAST_OPTIONS,
      ),
    [message],
  );
}

/** Non-hook version for use in catch blocks / imperative code */
export function showSuccessToast(message: string) {
  toast.custom(
    (t) => <SuccessToast toastId={t} message={message} />,
    DEFAULT_TOAST_OPTIONS,
  );
}

/** Non-hook version for use in catch blocks / imperative code */
export function showErrorToast(message: string) {
  toast.custom(
    (t) => <ErrorToast toastId={t} message={message} />,
    DEFAULT_TOAST_OPTIONS,
  );
}

/**
 * Parse structured `{ errors: string[] }` from an API error response and show
 * each error as a toast. Falls back to a generic message for non-HTTP or
 * unparseable errors. Returns true if structured errors were shown.
 */
export async function handleApiError(
  err: unknown,
  fallbackMessage: string,
): Promise<boolean> {
  if (err instanceof HTTPError) {
    try {
      const body = await err.response.json();
      if (body?.errors && Array.isArray(body.errors)) {
        body.errors.forEach((msg: string) => showErrorToast(msg));
        return true;
      }
    } catch {
      // response wasn't JSON — fall through
    }
  }
  showErrorToast(fallbackMessage);
  return false;
}
