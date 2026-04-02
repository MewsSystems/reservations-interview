import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { Toaster } from "sonner";
import { Theme } from "@radix-ui/themes";
import "@radix-ui/themes/styles.css";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./staff/AuthContext";

declare var root: HTMLDivElement;
const queryClient = new QueryClient();

const reactRoot = ReactDOM.createRoot(root);
reactRoot.render(
  <React.StrictMode>
    <Theme accentColor="mint">
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <App />
          <Toaster />
        </AuthProvider>
      </QueryClientProvider>
    </Theme>
  </React.StrictMode>,
);
