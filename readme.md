# Sample Interview App - Full Stack

## Reservation Management System

This repo is a barebones Reservation Management system that
allows guests to book rooms.

## Running the App

Ensure you have the following software installed:

- git
- dotnet (v8)
- node (v20)
- yarn (v4.3.1)
- caddy

<details>

<summary>MacOS Instructions</summary>

If you are on a mac, get homebrew so you can quickly install everything:

```sh
brew install git
brew install dotnet
dotnet --list-sdks # tested with sdk 8.0.104

# I recommend fnm to manage node
brew install fnm
fnm install 20
node --version # expect at least v20

# We recommend using corepack
corepack enable
which yarn

# if that doesn't work you can install yarn globally with npm
# npm i -g yarn

brew install caddy
```

</details>

In a terminal, install the project's dependencies in the backend and fronted portions:

```sh
cd api
dotnet build

cd ../ui
yarn install

cd ..
caddy adapt
```

Caddy's doc say to run `caddy adapt`, so that was included in the above code block.

With all that done, run the start shell script at the root of this repository to initiate
the api (dotnet), the router (caddy), and the ui (rspack dev server).

```
./start.sh
```

Then navigate to [port 4000](http://localhost:4000) to see the app.
If you go to [/api/swagger](http://localhost:4000/api/swagger) you can see
the interactive swagger docs to test the API.

> [!TIP]
> Sometimes the dotnet watcher gets confused, so we have concurrently configured to forward
> input to the dotnet watcher, so you can press `Ctrl + R` in the terminal to restart the api
> which usually fixes most problems. This only works because we put concurrently in 'raw' mode, so we
> lose the ability to name and color each output in a nice way, unfortunately.

**Seed some data**

Add 5 rooms using the api via [the swagger ui](http://localhost:4000/api/swagger) by
using the `POST /room` endpoint and the "try it out" mode. Feel free to add more data.

<details>
<summary>Expected Rooms JSON</summary>

You can use the `GET /room` to check if the DB has these saved:

```json
[
  {
    "number": 1,
    "state": 0
  },
  {
    "number": 2,
    "state": 0
  },
  {
    "number": 3,
    "state": 0
  },
  {
    "number": 4,
    "state": 0
  },
  {
    "number": 5,
    "state": 0
  }
]
```

</details>

## API

The API is a C# .NET 8 simple web api, living in api/

The API uses:

- SQLite
- Dapper
- Swagger
- CSharpier (VSCode extension for formatting)

## UI

The UI is a React + TS app, living in ui/

The UI uses:

- TypeScript (Types!)
- React (rendering library)
- Grommet (component library)
- TanStack Router (manual code style)
- RSBuild / RSpack (rust version of webpack)
- Prettier (VSCode extension for formatting)
