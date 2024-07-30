import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { Toaster } from "sonner";

declare var root: HTMLDivElement;

const reactRoot = ReactDOM.createRoot(root);
reactRoot.render(
  <React.StrictMode>
    <App />
    <Toaster />
  </React.StrictMode>
);
