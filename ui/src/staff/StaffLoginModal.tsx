import { useState } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Box, Button, Dialog, Separator, Text, TextField } from "@radix-ui/themes";
import { useQueryClient } from "@tanstack/react-query";
import { staffLogin } from "./api";

interface StaffLoginModalProps {
  trigger: React.ReactNode;
}

export function StaffLoginModal({ trigger }: StaffLoginModalProps) {
  const [accessCode, setAccessCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsLoading(true);
    setError("");
    try {
      await staffLogin(accessCode);
      await queryClient.refetchQueries({ queryKey: ["staff", "auth"] });
      navigate({ to: "/staff" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setIsLoading(false);
    }
  }

  function handleOpenChange(open: boolean) {
    if (!open) {
      setAccessCode("");
      setError("");
    }
  }

  return (
    <Dialog.Root onOpenChange={handleOpenChange}>
      <Dialog.Trigger>{trigger}</Dialog.Trigger>
      <Dialog.Content size="3" style={{ maxWidth: 360 }}>
        <Dialog.Title>Staff Login</Dialog.Title>
        <Dialog.Description>Enter the shared access code to continue.</Dialog.Description>
        <Separator color="cyan" size="4" my="4" />
        <form onSubmit={handleSubmit}>
          <Text as="label" size="2" weight="medium" htmlFor="access-code">
            Access Code
          </Text>
          <TextField.Root
            id="access-code"
            type="password"
            placeholder="Enter access code"
            value={accessCode}
            onChange={(e) => {
              setAccessCode(e.target.value);
              setError("");
            }}
            mt="1"
            mb={error ? "1" : "4"}
            color={error ? "red" : undefined}
            required
          />
          {error && (
            <Text size="1" color="red" mb="4" as="p">
              {error}
            </Text>
          )}
          <Box style={{ display: "flex", justifyContent: "flex-end", gap: 8 }}>
            <Dialog.Close>
              <Button variant="soft" color="gray" type="button">
                Cancel
              </Button>
            </Dialog.Close>
            <Button type="submit" color="mint" loading={isLoading} disabled={!accessCode}>
              Login
            </Button>
          </Box>
        </form>
      </Dialog.Content>
    </Dialog.Root>
  );
}
