import {
  createRootRoute,
  createRoute,
  createRouter,
  redirect
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { StaffOverviewPage } from "./reservations/staff/ReservationOverview";
import ky from "ky";

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
      path: "/staff/overview",
      beforeLoad: requireStaffAuth,
      getParentRoute: getRootRoute,
      component: StaffOverviewPage,
  })
];

async function requireStaffAuth() {
    try {
        await ky.get("/api/staff/check");
    } catch {
        throw redirect({ to: "/" });
    }
}

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
