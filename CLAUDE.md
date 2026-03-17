# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AliveMonitor is an API uptime monitoring tool with three clients (web, mobile) and a shared backend. Users sign in via Google OAuth, add endpoints to monitor, and receive alerts (email/Telegram) when endpoints go down. Real-time status updates are pushed via SignalR.

## Repository Structure

- `backend/` — .NET 10 Web API (C#), PostgreSQL, Hangfire, SignalR
- `frontend/` — React 19 + TypeScript (Vite), Tailwind CSS v4, TanStack Query
- `mobile/` — Flutter (Dart), Provider state management, GoRouter

## Build & Run Commands

### Backend (.NET 10)

```bash
# Local dev via .NET Aspire (orchestrates PostgreSQL + API + frontend proxy)
dotnet run --project backend/src/AliveMonitor.AppHost/AliveMonitor.AppHost.csproj

# Build
dotnet build backend/AliveMonitor.sln

# Run tests
dotnet test backend/AliveMonitor.sln

# EF Core migrations (from backend/ directory)
dotnet ef migrations add <Name> --project src/AliveMonitor.Infrastructure --startup-project src/AliveMonitor.Api
dotnet ef database update --project src/AliveMonitor.Infrastructure --startup-project src/AliveMonitor.Api
```

### Frontend (React + Vite)

```bash
cd frontend
npm install
npm run dev          # Dev server on :5173, proxies /api and /hubs to backend
npm run build        # TypeScript check + Vite production build
npm run lint         # ESLint
```

No test framework is configured for the frontend.

### Mobile (Flutter)

```bash
cd mobile
flutter pub get
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000/api   # Android emulator
flutter analyze
flutter test
```

Environment config files in `mobile/env/` (dev.json, prod.json). API URL and Google Client IDs are passed via `--dart-define`.

## Architecture

### Backend — Clean Architecture (3 layers)

- **AliveMonitor.Core** — Domain entities, DTOs, interfaces, enums. Zero external dependencies.
- **AliveMonitor.Infrastructure** — EF Core DbContext + repositories, all service implementations (health checks, auth, alerts, SSL, Telegram, SignalR notifier).
- **AliveMonitor.Api** — Controllers, SignalR hub, middleware, DI wiring. Serves the built frontend SPA from `wwwroot/`.
- **AliveMonitor.AppHost** — .NET Aspire orchestrator for local dev (spins up PostgreSQL, wires connection strings).

Key backend flows:
- **Health check scheduling**: Each monitored endpoint gets a Hangfire recurring job (`health-check-{id}`). `HealthCheckExecutor` runs the HTTP check, records a `HealthCheckLog`, updates endpoint status, opens/resolves `Incident`s, dispatches alerts, and notifies SignalR clients.
- **Auth**: Google ID token → backend validates → issues JWT access + refresh token pair. Refresh tokens are hashed and stored in DB.
- **Alerts**: `AlertDispatcher` → `CompositeAlertService` fans out to `EmailAlertService` and `TelegramAlertService` with throttle interval.

Database auto-migrates on startup (`MigrateAsync` in Program.cs). Connection string name: `alivemonitordb`.

### Frontend — React SPA

- **Routing**: React Router v7. `/dashboard`, `/endpoints/:id`, `/settings`. `ProtectedRoute` guards authenticated pages.
- **State**: Auth & theme in React Context. Server data via TanStack React Query (30s staleTime).
- **API layer**: Axios client (`frontend/src/api/client.ts`) with JWT interceptor and auto-refresh on 401.
- **Real-time**: SignalR singleton connection (`frontend/src/lib/signalr.ts`), `useSignalR` hook invalidates queries on `EndpointStatusChanged`.
- **UI**: Tailwind CSS + Radix UI primitives. CSS variables for theming in `theme.css`.
- **Path alias**: `@/` maps to `src/`.

### Mobile — Flutter

- **State**: Provider pattern (AuthProvider, EndpointProvider, TeamProvider, ThemeProvider).
- **HTTP**: Dio with interceptor for JWT auth + automatic token refresh on 401.
- **Navigation**: GoRouter with ShellRoute for bottom nav bar. Auth guard redirects to `/signin`.
- **Real-time**: `signalr_netcore` package connects to `/hubs/endpoint-status`.
- **Storage**: `flutter_secure_storage` for tokens and user profile.
- **Google Sign-In**: v7 API — `GoogleSignIn.instance` singleton, `authenticate()` method.

## API Convention

All REST endpoints are under `/api` prefix. SignalR hub at `/hubs/endpoint-status`. The frontend dev server proxies both `/api/*` and `/hubs/*` to the backend (configured in `vite.config.ts`).

## CI/CD

- `backend-ci.yml` — On push to backend paths: restore → build → test (.NET 10)
- `frontend-ci.yml` — On push to frontend paths: install → build (Node 22)
- `backend-deploy.yml` — Manual trigger: builds frontend into backend's `wwwroot/`, publishes .NET app, deploys to DigitalOcean droplet via SCP + SSH (systemd service)

## Key Configuration

- JWT settings in `appsettings.json` under `Jwt` section. Secret via environment variable.
- Google OAuth Client ID in `GoogleAuth:ClientId`.
- Alert settings (SMTP, Telegram bot token) in `Alerts` section.
- Frontend env vars: `VITE_API_URL`, `VITE_GOOGLE_CLIENT_ID` (in `.env` / `.env.production`).
- Hangfire dashboard available at `/hangfire` in development.
