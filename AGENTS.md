# Repository Guidelines

## Project Structure & Module Organization
`frontend/` contains the Vue 3 + TypeScript app. Keep feature code under `frontend/src/modules/<domain>/`; shared routing, stores, layouts, and API clients live in `src/router/`, `src/stores/`, `src/layouts/`, and `src/services/`. Static assets belong in `frontend/public/` or `frontend/src/assets/`.

`backend/` is the .NET 10 solution in `backend/OpsMonitor.slnx`. Use `Api/` for controllers and startup, `Application/` for business services, `Contracts/` for DTOs, `Infrastructure/` for persistence and integrations, `Domain/` for core entities, and `Api.Tests/` for automated tests. Product docs, specs, and mockups live in `docs/`, `mockups/`, and `ReadMe/`.

## Build, Test, and Development Commands
- `dotnet build backend/OpsMonitor.slnx` builds the full backend solution.
- `dotnet run --project backend/Api/OpsMonitor.Api.csproj` starts the API on `http://localhost:5092`.
- `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj` runs backend xUnit tests.
- `npm --prefix frontend run dev` starts the Vite dev server.
- `npm --prefix frontend run build` type-checks and builds the frontend into `frontend/dist/`.
- `npm --prefix frontend test` runs Vitest once in `jsdom`.

## Coding Style & Naming Conventions
Follow `.editorconfig`: UTF-8, LF endings, final newline, 2 spaces by default, and 4 spaces for `*.cs`. Match the surrounding file style instead of reformatting unrelated code.

Use PascalCase for C# types, controllers, and Vue component filenames such as `WorkloadDashboardView.vue`. Keep DTOs and controllers suffixed clearly (`*Dtos.cs`, `*Controller.cs`). Frontend utilities use descriptive camelCase names such as `exportCsv.ts`.

## Testing Guidelines
Backend tests use xUnit and `WebApplicationFactory`; keep them in `backend/Api.Tests/*Tests.cs`. Frontend tests use Vitest and should live beside the feature in `frontend/src/**/__tests__/*.spec.ts`. No coverage gate is configured, so add focused tests for each behavior change rather than broad boilerplate.

## Commit & Pull Request Guidelines
Recent history follows Conventional Commits with scopes, for example `feat(frontend): ...`, `feat(api): ...`, `docs: ...`, and `chore: ...`. Keep commits small and area-specific.

PRs should include: a short problem/solution summary, verification commands you ran, linked docs or issues, and screenshots for visible `frontend/` changes. Call out config, auth, or database changes explicitly.

## Security & Configuration Tips
`backend/Api/Program.cs` currently uses mock bearer auth and `EnsureCreated()` for local development. Treat changes there as high-impact. Do not commit generated outputs such as `frontend/dist/`, `frontend/node_modules/`, or `backend/**/bin` and `obj/`; review incidental changes to `backend/Api/ops-monitor-dev.db` before committing.
