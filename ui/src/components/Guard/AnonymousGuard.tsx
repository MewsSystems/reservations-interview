import { Guard } from "./Guard";

export function AnonymousGuard({ children }: { children: React.ReactNode }) {
    return <Guard shouldRender={false}>{children}</Guard>;
}
