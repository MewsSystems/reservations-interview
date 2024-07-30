import { useCallback } from "react";
import { toast } from "sonner";

const useCloseToast = (toastId: string | number) =>
  useCallback(() => toast.dismiss(toastId), [toastId]);

export interface SuccessToastProps {
  toastId: string | number;
  message: string;
}

/** A successful toast */
function SuccessToast({ toastId, message }: SuccessToastProps) {
  const closeToast = useCloseToast(toastId);

  return (
    <div
      title="Click to dismiss"
      className="toast toast-bottom toast-end cursor-pointer"
      onClick={closeToast}
    >
      <div className="alert alert-success">
        <span>{message}</span>
      </div>
    </div>
  );
}

const DEFAULT_TOAST_DURATION_MS = 2_250;

export function useShowToast(message: string) {
  return useCallback(
    () =>
      toast.custom((t) => <SuccessToast toastId={t} message={message} />, {
        duration: DEFAULT_TOAST_DURATION_MS,
      }),
    [message]
  );
}
