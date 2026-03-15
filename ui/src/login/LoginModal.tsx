import { useShowInfoToast } from "../utils/toasts";
import { Box, Button, Dialog, Separator, TextField } from "@radix-ui/themes";
import { useState } from "react";
import styled from "styled-components";

export interface Login {
    Login: string;
    Password: string;
}

interface LoginModalProps {
    onSubmit: (booking: Login) => void;
}

interface LoginFormProps {
    onSubmit: (booking: Login) => void;
}

/** Must be inside a Dialog.Root that container Dialog.Triggers elsewhere */
export function LoginModal({
    onSubmit,
}: LoginModalProps) {
    return (
        <Dialog.Content size="4">
            <Dialog.Title>Login</Dialog.Title>
            <Separator color="cyan" size="4" my="4" />
            <LoginForm onSubmit={onSubmit} />
        </Dialog.Content>
    );
}

const DimSlot = styled(TextField.Slot)`
  background-color: var(--gray-4);
  margin-right: 8px;
`;

const BottomRightBox = styled(Box)`
  position: absolute;
  bottom: 0;
  right: 0;
`;

function LoginForm({ onSubmit }: LoginFormProps) {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");

    const showNoInfoToast = useShowInfoToast("Missing login or password.");

    function handleSubmit(evt: React.MouseEvent<HTMLButtonElement>) {
        if (!login || !password) {
            showNoInfoToast();
            evt.preventDefault();
            return false;
        }

        onSubmit({
            Login: login,
            Password: password
        });
        return true;
    }

    return (
        <Box style={{ position: "relative", minHeight: 700 }}>
            <TextField.Root
                placeholder="... address@domain.tld ..."
                onChange={(evt) => setLogin(evt.target.value)}
                value={login}
                type="email"
                size="3"
                mb="4"
            >
                <DimSlot side="left" prefix="email">
                    Login (email)
                </DimSlot>
            </TextField.Root>
            <TextField.Root
                placeholder="... John ..."
                onChange={(evt) => setPassword(evt.target.value)}
                value={password}
                type="password"
                size="3"
                mb="4"
            >
                <DimSlot side="left" prefix="name">
                    Password
                </DimSlot>
            </TextField.Root>
            <BottomRightBox>
                <Dialog.Close>
                    <Button size="3" color="mint" mt="4" onClick={handleSubmit}>
                        Login
                    </Button>
                </Dialog.Close>
            </BottomRightBox>
        </Box>
    );
}
