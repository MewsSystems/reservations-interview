import { useState } from "react";
import { Button, Dialog, Separator, TextField } from "@radix-ui/themes";
import { useNavigate } from "@tanstack/react-router";
import { staffLogin } from "./api";
import { useAuth } from "../utils/auth";
import { useShowErrorToast } from "../utils/toasts";

interface StaffLoginDialogProps {
  children: React.ReactNode;
}

export function StaffLoginDialog({ children }: StaffLoginDialogProps) {
  const [accessCode, setAccessCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const showError = useShowErrorToast();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);

    try {
      const token = await staffLogin(accessCode);
      login(token);
      setOpen(false);
      setAccessCode("");
      navigate({ to: "/staff/reservations" });
    } catch {
      showError(["Invalid access code"]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger>{children}</Dialog.Trigger>
      <Dialog.Content size="3" style={{ maxWidth: 400 }}>
        <Dialog.Title>Staff Login</Dialog.Title>
        <Dialog.Description>
          Enter your staff access code
        </Dialog.Description>
        <Separator color="cyan" size="4" my="4" />
        <form onSubmit={handleSubmit}>
          <TextField.Root
            placeholder="Access code"
            type="password"
            value={accessCode}
            onChange={(e) => setAccessCode(e.target.value)}
            size="3"
            mb="4"
          />
          <Button size="3" color="mint" type="submit" disabled={loading || !accessCode}>
            {loading ? "Logging in..." : "Login"}
          </Button>
        </form>
      </Dialog.Content>
    </Dialog.Root>
  );
}
