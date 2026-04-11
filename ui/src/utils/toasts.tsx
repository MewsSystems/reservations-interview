import { SuccessToast } from "../components/SuccessToast";
import { InfoToast } from "../components/InfoToast";
import { ExternalToast, toast } from "sonner";
import { ReactNode, useCallback } from "react";

const DEFAULT_TOAST_DURATION_MS = 2_250;

const DEFAULT_TOAST_OPTIONS: ExternalToast = {
  duration: DEFAULT_TOAST_DURATION_MS,
};

function showCustomToast(renderer: (toastId: string | number) => ReactNode) {
  return toast.custom((t) => renderer(t), DEFAULT_TOAST_OPTIONS);
}

export function showInfoToast(message: string) {
  return showCustomToast((t) => <InfoToast toastId={t} message={message} />);
}

export function useShowSuccessToast(message: string) {
  return useCallback(
    () =>
      showCustomToast((t) => <SuccessToast toastId={t} message={message} />),
    [message],
  );
}

export function useShowInfoToast(message: string) {
  return useCallback(() => showInfoToast(message), [message]);
}
