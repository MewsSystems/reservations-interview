import { Text, Box } from "@radix-ui/themes";
import { useCallback } from "react";
import { toast } from "sonner";
import styled from "styled-components";

export interface InfoToastProps {
  toastId: string | number;
  message: string;
}

const BorderedInfoBox = styled(Box)`
  background-color: var(--blue-5);
  border-radius: var(--radius-4);
  border: 1px solid var(--indigo-9);
`;

/** An info toast */
export function InfoToast({ toastId, message }: InfoToastProps) {
  const closeToast = useCallback(() => toast.dismiss(toastId), [toastId]);

  return (
    <BorderedInfoBox onClick={closeToast} width="325" py="2" px="5">
      <Text size="5" align="left">
        {message}
      </Text>
    </BorderedInfoBox>
  );
}
