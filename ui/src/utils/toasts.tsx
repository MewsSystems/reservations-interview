import { SuccessToast } from "../components/SuccessToast";
import { InfoToast } from "../components/InfoToast";
import { ExternalToast, toast } from "sonner";
import { useCallback } from "react";
import { ErrorToast } from "../components/ErrorToast";

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

export function useShowErrorToast() {
  return useCallback(
    (message: string) =>
      toast.custom(
        (t) => <ErrorToast toastId={t} message={message} />,
        DEFAULT_TOAST_OPTIONS,
      ),
    [],
  );
}
