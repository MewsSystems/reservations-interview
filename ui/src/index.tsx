import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { Toaster } from "sonner";
import { Theme } from "@radix-ui/themes";
import "@radix-ui/themes/styles.css";
import { QueryClient, QueryClientProvider, QueryCache, MutationCache } from "@tanstack/react-query";
import { router } from "./router";

declare var root: HTMLDivElement;

function navigateToError(error: unknown) {
  const status: number = (error as any)?.response?.status ?? 500;
  if (status >= 400) {
    router.navigate({
      to: "/error",
      search: { status, message: (error as any)?.message },
    });
  }
}

const queryClient = new QueryClient({
  queryCache: new QueryCache({ onError: navigateToError }),
  mutationCache: new MutationCache({ onError: navigateToError }),
});

const reactRoot = ReactDOM.createRoot(root);
reactRoot.render(
  <React.StrictMode>
    <Theme accentColor="mint">
      <QueryClientProvider client={queryClient}>
        <App />
        <Toaster />
      </QueryClientProvider>
    </Theme>
  </React.StrictMode>,
);
