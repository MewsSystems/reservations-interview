import {
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { StaffReservationsPage } from "./staff/StaffReservationsPage";
import { StaffRoomsPage } from "./staff/StaffRoomsPage";

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
    path: "/staff/reservations",
    getParentRoute: getRootRoute,
    component: StaffReservationsPage,
  }),
  createRoute({
    path: "/staff/rooms",
    getParentRoute: getRootRoute,
    component: StaffRoomsPage,
  }),
];

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
