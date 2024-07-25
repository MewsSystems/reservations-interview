import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";

export default defineConfig({
  html: {
    title: "Reservations",
  },
  plugins: [pluginReact()],
  server: {
    port: 6002,
  },
});
