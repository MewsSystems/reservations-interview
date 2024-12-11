import {
  createRootRoute,
  createRoute,
  createRouter,
  redirect,
} from "@tanstack/react-router";
import { Layout } from "./Layout";
import { LandingPage } from "./LandingPage";
import { ReservationPage } from "./reservations/ReservationPage";
import { LoginPage } from "./staff/LoginPage";
import { StaffPage } from "./staff/StaffPage";
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
    path: "/login",
    getParentRoute: getRootRoute,
    component: LoginPage,
  }),
  createRoute({
    path: "/staff",
    getParentRoute: getRootRoute,
    component: StaffPage,
    beforeLoad: async () => {
      const isAuthenticated = await IsAuthenticated()
      if (!isAuthenticated){
        throw redirect({
          to: "/login"
        });
      }
    }
  }),
];

async function IsAuthenticated() : Promise<boolean> {
  try {
      await ky.get("api/staff/check");
      return true;
  } catch (error) {
      console.log("Check error:", error);
      return false;
  }
}

const routeTree = rootRoute.addChildren(ROUTES);

export const router = createRouter({ routeTree });
