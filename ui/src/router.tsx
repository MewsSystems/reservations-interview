import {
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { LoginPage } from "./staff/Login/LoginPage";
import { ProtectedRoute } from "./components/Guard/ProtectedRoute";
import { AnonymousRoute } from "./components/Guard/AnonymousRoute";
import { StaffReservationPage } from "./staff/Reservations/ReservationPage";
import { ImportRooms } from "./staff/ImportRooms/ImportRooms";

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
    path: "/login",
    getParentRoute: getRootRoute,
    component: () => <AnonymousRoute><LoginPage /></AnonymousRoute>
  }),
  createRoute({
    path: "/staff/reservations",
    getParentRoute: getRootRoute,
    component: () => <ProtectedRoute><StaffReservationPage /></ProtectedRoute>
  }),
  createRoute({
    path: "/staff/rooms",
    getParentRoute: getRootRoute,
    component: () =>
      <ProtectedRoute>
        <ImportRooms></ImportRooms>
      </ProtectedRoute>
  }),
];

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
