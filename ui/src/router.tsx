import {
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { StaffPage } from "./staff/StaffPage";

const rootRoute = createRootRoute({
  component: Layout,
});

function getRootRoute() {
  return rootRoute;
}

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
    path: "/staff",
    getParentRoute: getRootRoute,
    component: StaffPage,
  }),
];

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
