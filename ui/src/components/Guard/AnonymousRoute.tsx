import { useNavigate } from "@tanstack/react-router";
import { useEffect, ReactNode } from "react";
import { IsAuthenticated } from "../../utils/auth";
import { LoadingCard } from "../LoadingCard";

type AnonymousRouteProps = {
    children: ReactNode
}

export function AnonymousRoute({ children }: AnonymousRouteProps) {
    const navigate = useNavigate();
    useEffect(() => {
        if (IsAuthenticated()) {
            navigate({ to: '/', replace: true });
        }
    }, [IsAuthenticated, navigate]);

    if (IsAuthenticated()) {
        return <LoadingCard></LoadingCard>;
    }
    return children;
}