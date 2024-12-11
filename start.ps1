# Run our three programs concurrently with some formatting
npx concurrently -ir --default-input-target 0 `
  "cd api & dotnet watch" `
  "cd ui & npm start" `
  "caddy run"
  