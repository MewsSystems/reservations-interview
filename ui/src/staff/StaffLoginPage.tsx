import { useEffect, useState } from "react";
import { useRouter } from "@tanstack/react-router";
import {
  Box,
  Button,
  Card,
  Flex,
  Heading,
  Separator,
  TextField,
} from "@radix-ui/themes";
import { useAuth } from "./AuthContext";
import { handleApiError, showErrorToast } from "../utils/toasts";
import styled from "styled-components";

const DimSlot = styled(TextField.Slot)`
  background-color: var(--gray-4);
  margin-right: 8px;
`;

export function StaffLoginPage() {
  const router = useRouter();
  const { isAuthed, login } = useAuth();
  const [accessCode, setAccessCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isAuthed === true) {
      router.navigate({ to: "/staff" });
    }
  }, [isAuthed, router]);

  async function handleSubmit(evt: React.FormEvent) {
    evt.preventDefault();
    if (!accessCode.trim()) {
      showErrorToast("Access code is required.");
      return;
    }

    setIsLoading(true);
    try {
      await login(accessCode);
      router.navigate({ to: "/staff" });
    } catch (err) {
      await handleApiError(err, "Login failed. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <Flex align="center" justify="center" pt="9">
      <Card size="4" style={{ width: 360 }}>
        <Heading size="6" mb="2">
          Staff Login
        </Heading>
        <Separator color="mint" size="4" mb="4" />
        <form onSubmit={handleSubmit}>
          <Box mb="4">
            <TextField.Root
              placeholder="Enter access code..."
              type="password"
              size="3"
              value={accessCode}
              onChange={(e) => setAccessCode(e.target.value)}
              autoFocus
            >
              <DimSlot side="left">Code</DimSlot>
            </TextField.Root>
          </Box>
          <Flex justify="end">
            <Button
              size="3"
              color="mint"
              type="submit"
              disabled={isLoading}
            >
              {isLoading ? "Logging in..." : "Login"}
            </Button>
          </Flex>
        </form>
      </Card>
    </Flex>
  );
}
