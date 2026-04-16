# Sunroom CRM — Blazor Frontend

A full-featured customer relationship management application built with Blazor Web App (.NET 8), MudBlazor, and EF Core. This is the Blazor frontend for [Sunroom CRM](https://sunroomcrm.net), with a dual-mode data layer that runs either self-contained against its own SQL Server / EF Core backend or as a frontend-only client of the shared .NET 8 REST API.

## About Sunroom CRM

Sunroom CRM is a multi-frontend CRM platform designed to demonstrate the same business requirements implemented across multiple modern frameworks — all sharing a single .NET 8 REST API and SQL Server database. The project showcases how different frontend ecosystems approach the same real-world problems: authentication, CRUD operations, real-time data visualization, drag-and-drop workflows, role-based access control, and AI-powered features.

### The Full Stack

| Repository | Technology | Description |
|------------|------------|-------------|
| [sunroom-crm-dotnet](https://github.com/rvnminers-A-and-N/sunroom-crm-dotnet) | .NET 8, EF Core, SQL Server | Shared REST API with JWT auth, AI endpoints, and Docker support |
| [sunroom-crm-angular](https://github.com/rvnminers-A-and-N/sunroom-crm-angular) | Angular 21, Material, Vitest | Angular frontend with 100% test coverage |
| [sunroom-crm-react](https://github.com/rvnminers-A-and-N/sunroom-crm-react) | React 19, shadcn/ui, Vitest | React frontend with 100% test coverage |
| [sunroom-crm-vue](https://github.com/rvnminers-A-and-N/sunroom-crm-vue) | Vue 3, Vuetify 4, Vitest | Vue frontend with 100% test coverage |
| **sunroom-crm-blazor** (this repo) | Blazor Web App, .NET 8, MudBlazor | Blazor frontend with 100% test coverage |
| [sunroom-crm-laravel](https://github.com/rvnminers-A-and-N/sunroom-crm-laravel) | Laravel 13, Livewire 3 | Laravel full-stack implementation |

## Tech Stack

| Layer         | Technology                                          |
|---------------|-----------------------------------------------------|
| Framework     | Blazor Web App (.NET 8) with InteractiveServer + InteractiveWebAssembly render modes |
| UI            | MudBlazor 7.15 (Material Design)                    |
| Charts        | MudBlazor charts                                    |
| Data Layer    | EF Core 8 (Local) or HttpClient (Api) — switchable via `DataMode` |
| Database      | SQL Server (Local mode) or InMemory (tests)         |
| Auth          | Cookie authentication with BCrypt password hashing  |
| Unit Tests    | xUnit + FluentAssertions + bUnit + Moq + EF Core InMemory |
| E2E Tests     | Microsoft.Playwright 1.49 (Chromium, Firefox, WebKit) |
| Coverage      | Coverlet + ReportGenerator                          |
| CI/CD         | GitHub Actions                                      |
| Language      | C# 12 / .NET 8                                      |

## Features

- **Authentication** — Cookie-based login and registration with BCrypt-hashed passwords and minimal API endpoints
- **Contacts** — Full CRUD with search, tag filtering, pagination, and sorting
- **Companies** — Company management with associated contacts and deals
- **Deals** — List view and Kanban-style pipeline board with drag-and-drop between stages
- **Activities** — Activity log with timeline view linked to contacts and deals
- **Dashboard** — Overview with stat cards, gradient banner, and visualizations for pipeline value and recent activity
- **AI Features** — AI-powered natural language search, activity summarization, and deal insights
- **Admin Panel** — User management restricted to admin roles
- **Settings** — User profile editing and tag management
- **Responsive Layout** — MudLayout with collapsible navigation drawer and MudAppBar for mobile menus
- **Dual-Mode Data Layer** — Switch between local EF Core backend and the shared .NET REST API via configuration

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, container, or remote) for Local mode
- The [.NET API](https://github.com/rvnminers-A-and-N/sunroom-crm-dotnet) running on `http://localhost:5236` for Api mode

### Setup

```bash
git clone https://github.com/rvnminers-A-and-N/sunroom-crm-blazor.git
cd sunroom-crm-blazor
dotnet restore
dotnet run --project SunroomCrm.Blazor
```

The app runs at `https://localhost:5001` (or `http://localhost:5000`). On first run in Local mode, the database is created and seeded with an admin user, sample companies, contacts, deals, and activities.

### Switching Data Modes

Set `DataMode` in `appsettings.json` (or via environment variable `DataMode`):

- `Local` — Use the embedded EF Core / SQL Server backend (default)
- `Api` — Consume the shared .NET REST API at `ApiBaseUrl`

```json
{
  "DataMode": "Api",
  "ApiBaseUrl": "http://localhost:5236"
}
```

### Running the Shared API

The .NET API can be started via Docker Compose from the `sunroom-crm-dotnet` repo:

```bash
cd ../sunroom-crm-dotnet
cp .env.example .env   # Set SA_PASSWORD
docker compose up -d
```

## Available Commands

| Command                                                                                | Description                                  |
|----------------------------------------------------------------------------------------|----------------------------------------------|
| `dotnet run --project SunroomCrm.Blazor`                                               | Start the app on https://localhost:5001      |
| `dotnet build SunroomCrm.Blazor.sln`                                                   | Build the entire solution                    |
| `dotnet format SunroomCrm.Blazor.sln`                                                  | Format code with the .NET formatter          |
| `dotnet test SunroomCrm.Blazor.Tests/SunroomCrm.Blazor.Tests.csproj`                   | Run unit and integration tests               |
| `dotnet test SunroomCrm.Blazor.Tests.E2E/SunroomCrm.Blazor.Tests.E2E.csproj`           | Run Playwright end-to-end tests              |
| `dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings`          | Run tests with coverage collection           |

## Testing

### Unit Tests

164 tests across 22 test classes at **100% code coverage** (336/336 lines, 78/78 branches). Coverage thresholds are enforced in `.github/workflows/ci.yml` — the build fails if line coverage drops below 100%.

Tests use [xUnit](https://xunit.net/) with [FluentAssertions](https://fluentassertions.com/) for expressive assertions, [bUnit](https://bunit.dev/) for Razor component rendering, [Moq](https://github.com/devlooped/moq) for service mocking, and EF Core's InMemory provider for data-layer testing. API service tests use a `MockHttpMessageHandler` to assert request/response wiring without a live backend.

```bash
dotnet test SunroomCrm.Blazor.Tests/SunroomCrm.Blazor.Tests.csproj --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

### End-to-End Tests

51 Playwright tests across 11 test files, run against Chromium, Firefox, and WebKit:

- **Authentication** — login, registration, invalid credentials, and link navigation between login and register pages
- **Auth Guards** — unauthenticated redirects from dashboard, contacts, companies, and deals
- **Contacts** — list, create dialog, row click navigation, search field, and seeded data display
- **Companies** — list, create dialog, row click navigation, search field, and seeded data display
- **Deals** — list, create dialog, row click navigation, pipeline stage columns, and seeded data display
- **Activities** — list, create dialog, type filter, and seeded data display
- **Dashboard & Golden Path** — stat cards, gradient banner, and full register → dashboard → contacts → deals → pipeline journey
- **Navigation** — sidebar links to all sections and brand logo display
- **AI Panel** — page load, summarize section, and search section
- **Admin & Settings** — admin user access, admin link visibility, and settings page navigation
- **Cross-Browser** — login page and dashboard rendering verified across Chromium, Firefox, and WebKit

The `PlaywrightFixture` runs the entire Blazor app in-process with a fresh InMemory database per fixture, so the mocked E2E suite needs no external services. Setting `BLAZOR_E2E_API_URL` switches the fixture into integration mode that proxies to a live .NET API instead.

```bash
pwsh SunroomCrm.Blazor.Tests.E2E/bin/Debug/net8.0/playwright.ps1 install --with-deps
dotnet test SunroomCrm.Blazor.Tests.E2E/SunroomCrm.Blazor.Tests.E2E.csproj
```

## CI/CD Pipeline

GitHub Actions runs four jobs on every push and pull request to `main`:

**Lint & Build** — Runs `dotnet format --verify-no-changes` and a Release build of the entire solution.

**Unit & Integration Tests** — Runs the full xUnit suite with coverlet coverage collection, generates a ReportGenerator HTML report, appends a Markdown summary to the GitHub Actions job summary, and enforces the 100% line coverage gate.

**Playwright E2E (mocked)** — Installs all three Playwright browsers, then runs the full E2E suite against the in-process `PlaywrightFixture` with InMemory data. Gated behind the `RUN_E2E` repository variable.

**E2E Integration (Docker)** — Clones the .NET API repo, spins up SQL Server and the API via Docker Compose, then runs the full Playwright suite against the live stack with `BLAZOR_E2E_API_URL` pointed at the container. Gated behind the `RUN_E2E` repository variable.

## Architecture

```
SunroomCrm.Blazor/                # Server project (Blazor Web App host)
  Auth/                           # Cookie auth helpers and minimal API endpoints
  Components/                     # Server-rendered Razor components
    Layout/                       # MainLayout, NavMenu, AppBar
    Pages/                        # Auth, Contacts, Companies, Deals, Activities, Dashboard, Admin, AI, Settings
    Shared/                       # PageHeader, StatCard, EmptyState, ConfirmDialog, TagChip
  Data/                           # AppDbContext, SeedData, EF Core configuration
  Services/                       # Local data services (EF Core) and ServiceRegistration
  Theme/                          # SunroomTheme (MudBlazor palette and typography)
SunroomCrm.Blazor.Client/         # WebAssembly client project (interactive components)
  Pages/                          # WASM-rendered components (DealPipeline)
  Services/                       # Api data services (HttpClient + JwtDelegatingHandler)
SunroomCrm.Shared/                # Shared models and DTOs across all projects
SunroomCrm.Blazor.Tests/          # Unit and integration tests
  Unit/Services/Local/            # EF Core service tests with InMemory provider
  Unit/Services/Api/              # HttpClient service tests with MockHttpMessageHandler
  Unit/Components/                # bUnit Razor component tests
  Unit/DataLayer/                 # AppDbContext and configuration tests
  Integration/                    # Auth endpoint and SeedData integration tests
  Helpers/                        # Test utilities (MockHttpMessageHandler, TestDbContextFactory)
SunroomCrm.Blazor.Tests.E2E/      # Playwright end-to-end tests
  PlaywrightFixture.cs            # In-process app host with dual-mode (InMemory or live API)
  Tests/                          # 11 feature test files
.github/workflows/                # CI pipeline configuration
```

### Key Patterns

- **Blazor Web App** — Hybrid render modes: most pages use `InteractiveServer` for stateful server rendering with SignalR; the deals pipeline uses `InteractiveWebAssembly` for client-side drag-and-drop responsiveness
- **MudBlazor** — Material Design component library providing data tables, dialogs, forms, navigation drawer, app bar, snackbars, and responsive layout primitives. Providers live inside `MainLayout` so they share the interactive render context
- **Dual-Mode Data Layer** — `ServiceRegistration.AddDataServices()` switches between `Local*Service` (EF Core) and `Api*Service` (HttpClient) implementations of the same `IContactService`, `IDealService`, etc. interfaces based on `DataMode` configuration
- **Cookie Authentication** — Server-side auth with `LoginEndpoints` minimal API exposing `/api/account/login`, `/register`, and `/logout`. Login/register pages call these endpoints via JS interop so the cookie lands on the browser before redirect
- **JWT Delegating Handler** — In Api mode, `JwtDelegatingHandler` reads the auth token from the cookie and attaches a `Bearer` header to every outgoing API request
- **EF Core** — Code-first models with relationships configured in `OnModelCreating`. SeedData populates the InMemory or SQL Server database with an admin user, three users, tags, companies, contacts, deals, activities, and AI insights
- **bUnit** — Razor component tests render components in a test renderer with mocked services for fast, isolated UI testing
- **Playwright In-Process Fixture** — The E2E fixture builds a real `WebApplication` inside the test process, listens on a random port, and seeds an InMemory database. This eliminates the need for external `dotnet run` orchestration and gives every test a clean database

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
