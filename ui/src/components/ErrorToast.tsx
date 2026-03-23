import { Text, Box } from "@radix-ui/themes";
import { useCallback } from "react";
import { toast } from "sonner";
import styled from "styled-components";

export interface ErrorToastProps {
  toastId: string | number;
  messages: string[];
}

const BorderedErrorBox = styled(Box)`
  background-color: var(--red-5);
  border-radius: var(--radius-4);
  border: 1px solid var(--red-9);
`;

const ErrorList = styled.ul`
  margin: 4px 0 0 0;
  padding-left: 18px;
`;

/** An error toast */
export function ErrorToast({ toastId, messages }: ErrorToastProps) {
  const closeToast = useCallback(() => toast.dismiss(toastId), [toastId]);

  return (
    <BorderedErrorBox onClick={closeToast} width="325" py="2" px="5">
      {messages.length === 1 ? (
        <Text size="3" align="left">
          {messages[0]}
        </Text>
      ) : (
        <ErrorList>
          {messages.map((msg, i) => (
            <li key={i}>
              <Text size="2">{msg}</Text>
            </li>
          ))}
        </ErrorList>
      )}
    </BorderedErrorBox>
  );
}
