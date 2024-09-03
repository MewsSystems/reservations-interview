import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { Toaster } from "sonner";
import { Theme } from "@radix-ui/themes";
import "@radix-ui/themes/styles.css";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

declare var root: HTMLDivElement;
const queryClient = new QueryClient();

const reactRoot = ReactDOM.createRoot(root);
reactRoot.render(
  <React.StrictMode>
    <Theme accentColor="mint" style={{ display: 'flex', flexDirection: 'row' }}>
      <QueryClientProvider client={queryClient}>
        <App />
        <Toaster />
      </QueryClientProvider>
    </Theme>
  </React.StrictMode>,
);
