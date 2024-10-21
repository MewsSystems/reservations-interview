import { Text, Box } from "@radix-ui/themes";
import { useCallback } from "react";
import { toast } from "sonner";
import styled from "styled-components";

export interface ErrorToastProps {
  toastId: string | number;
  message: string;
}

const BorderedSuccessBox = styled(Box)`
  background-color: var(--red-5);
  border-radius: var(--radius-4);
  border: 1px solid var(--red-9);
`;

/** A successful toast */
export function ErrorToast({ toastId, message }: ErrorToastProps) {
  const closeToast = useCallback(() => toast.dismiss(toastId), [toastId]);

  return (
    <BorderedSuccessBox onClick={closeToast} width="325" py="2" px="5">
      <Text size="5" align="left">
        {message}
      </Text>
    </BorderedSuccessBox>
  );
}
