import { useNavigate } from "@tanstack/react-router";
import { useEffect, ReactNode } from "react";
import { IsAuthenticated } from "../../utils/auth";
import { LoadingCard } from "../LoadingCard";

type ProtectedRouteProps = {
    children: ReactNode
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
    const navigate = useNavigate();
    useEffect(() => {
        if (!IsAuthenticated()) {
            navigate({ to: '/login', replace: true });
        }
    }, [IsAuthenticated, navigate]);

    if (!IsAuthenticated()) {
        return <LoadingCard></LoadingCard>;
    }
    return children;
}