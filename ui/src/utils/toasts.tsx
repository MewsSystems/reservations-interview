import { SuccessToast } from "../components/SuccessToast";
import { InfoToast } from "../components/InfoToast";
import { ExternalToast, toast } from "sonner";
import { useCallback } from "react";

const DEFAULT_TOAST_DURATION_MS = 2_250;

const DEFAULT_TOAST_OPTIONS: ExternalToast = {
  duration: DEFAULT_TOAST_DURATION_MS,
};

export function useShowSuccessToast() {
  return useCallback(
    (message: string) =>
      toast.custom(
        (t) => <SuccessToast toastId={t} message={message} />,
        DEFAULT_TOAST_OPTIONS,
      ),
    [],
  );
}

export function useShowInfoToast() {
  return useCallback(
    (message: string) =>
      toast.custom(
        (t) => <InfoToast toastId={t} message={message} />,
        DEFAULT_TOAST_OPTIONS,
      ),
    [],
  );
}