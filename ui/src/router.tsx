import {
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { StaffLoginPage } from "./staff/StaffLoginPage";
import { StaffReservationsPage } from "./staff/StaffReservationsPage";
import { ErrorPage } from "./components/ErrorPage";

const rootRoute = createRootRoute({
  component: Layout,
  // Rendered when no route matches (404)
  notFoundComponent: () => <ErrorPage statusCode={404} />,
  // Rendered when a route component or loader throws (5xx-class)
  errorComponent: ({ error }: { error: unknown }) => {
    const status = (error as any)?.response?.status ?? 500;
    const message = (error as any)?.message;
    return <ErrorPage statusCode={status} message={message} />;
  },
});

function getRootRoute() {
  return rootRoute;
}

const errorRoute = createRoute({
  path: "/error",
  getParentRoute: getRootRoute,
  validateSearch: (search: Record<string, unknown>) => {
    const parsed = Number(search.status);
    const status = Number.isFinite(parsed) && parsed >= 100 && parsed <= 599 ? parsed : 500;
    return {
      status,
      message: typeof search.message === "string" ? search.message : undefined,
    };
  },
  component: function ErrorRoute() {
    const { status, message } = errorRoute.useSearch();
    return <ErrorPage statusCode={status} message={message} />;
  },
});

const ROUTES = [
  createRoute({
    path: "/",
    getParentRoute: getRootRoute,
    component: LandingPage,
  }),
  createRoute({
    path: "/reservations",
    getParentRoute: getRootRoute,
    component: ReservationPage,
  }),
  createRoute({
    path: "/staff/login",
    getParentRoute: getRootRoute,
    component: StaffLoginPage,
  }),
  createRoute({
    path: "/staff/reservations",
    getParentRoute: getRootRoute,
    component: StaffReservationsPage,
  }),
  errorRoute,
];

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
