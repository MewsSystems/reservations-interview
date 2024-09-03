import { IsAuthenticated } from "../../utils/auth";

type GuardProps = {
    children: React.ReactNode;
    shouldRender: boolean;
};

export function Guard({ children, shouldRender }: GuardProps) {
    const isAuthenticated = IsAuthenticated();

    if (isAuthenticated !== shouldRender) {
        return null;
    }
    return <>{children}</>;
}
