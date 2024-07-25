import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";

declare var root: HTMLDivElement;

const reactRoot = ReactDOM.createRoot(root);
reactRoot.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
