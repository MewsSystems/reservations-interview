import { Guard } from "./Guard";

export function AuthenticateGuard({ children }: { children: React.ReactNode }) {
    return <Guard shouldRender={true}>{children}</Guard>;
}
