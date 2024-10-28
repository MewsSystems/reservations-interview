import { Button, Dialog, TextField } from "@radix-ui/themes";
import { useState } from "react";

interface LoginModalProps {
  onSubmit: (code: string) => void;
}

/** Must be inside a Dialog.Root that container Dialog.Triggers elsewhere */
export function LoginModal({ onSubmit }: LoginModalProps) {
  const [passcode, setPasscode] = useState("");

    function handleSubmit(_event: React.MouseEvent<HTMLButtonElement>) {
        onSubmit(passcode);
        return true;
    }

  return (
    <Dialog.Content size="4">
      <Dialog.Title>Staff login</Dialog.Title>
      <TextField.Root
        placeholder="passcode"
        onChange={(evt) => setPasscode(evt.target.value)}
        value={passcode}
        type="password"
        size="3"
      ></TextField.Root>
      <Dialog.Close>
        <Button size="3" color="mint" mt="4" onClick={handleSubmit}>
          Submit
        </Button>
      </Dialog.Close>
    </Dialog.Content>
  );
}
