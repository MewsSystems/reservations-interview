import { Notification } from "grommet";
import { Checkmark } from "grommet-icons";

export interface SuccessToastProps {
  show: boolean;
  onClose: () => void;
  /** Defaults to 'Success */
  title?: string;
  /** Defaults to 'Operation successful!'
   * Highly recommended to provide a contextual message
   */
  message?: string;
}

const DEFAULT_TITLE = "Success";
const DEFAULT_MESSAGE = "Operation successful!";

/** A successful toast with defaults.
 * Highly recommended to set a custom message
 */
export function SuccessToast({
  show,
  onClose,
  title,
  message,
}: SuccessToastProps) {
  if (!show) {
    return null;
  }

  const resolvedTitle = title || DEFAULT_TITLE;
  const resolvedMessage = message || DEFAULT_MESSAGE;

  return (
    <Notification
      toast
      title={resolvedTitle}
      message={resolvedMessage}
      onClose={onClose}
      status="normal"
      icon={<Checkmark />}
    />
  );
}
