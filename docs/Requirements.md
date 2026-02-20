# AliveMonitor Requirements

AliveMonitor is a lightweight application for monitoring the health of APIs and systems, designed for small to medium-sized environments. It provides real-time health checks, incident-based alerts, analytics, and a user-friendly dashboard for clients.

## Key Features
- Periodic health checks (configurable interval, default: 1 minute) with no per-client endpoint limit
- Supports GET requests with optional custom headers (e.g., Authorization, API keys)
- Health check rules: expected HTTP status code + optional JSON property check per endpoint
- Incident-based alert system: immediate email on first failure, throttled repeat alerts, and recovery notifications (pluggable provider pattern for future channels)
- Real-time dashboard status updates via SignalR
- Endpoint analytics: response time graphs, uptime percentages, incident history with custom date range picker
- Authentication exclusively via Google OAuth 2.0 (no username/password)
- Short-lived JWT access tokens with refresh tokens for API access
- Fully isolated multi-tenancy: each client sees only their own data
- PostgreSQL database (Azure Database for PostgreSQL in production)
- Azure cloud deployment with GitHub Actions CI/CD

## Monitoring
- Periodically sends GET requests to specified API endpoints (configurable interval, default 1 min)
- Supports optional custom headers per endpoint (Authorization, API keys, etc.)
- Health evaluated by expected HTTP status code and optional JSON property name + value
- No limit on endpoints per client
- Monitored endpoints managed via dashboard UI (add, edit, delete, enable/disable)
- Newly added endpoints are disabled by default
- Supports 6-50 users (client role only)
- Dashboard does not support multi-language or export features (CSV/PDF) at this time

## Notifications
- Incident-based alert lifecycle: first failure alert, throttled repeat alerts, and recovery alert
- Throttle interval configurable globally (default: 10 minutes)
- Notification includes issue details and a link to the dashboard
- Only email (SMTP) notifications supported initially

## Dashboard
- Endpoint list view with real-time status updates via SignalR
- Color-coded status indicators (green = healthy, red = unhealthy, gray = disabled)
- Search by name/URL and filter by status (All, Healthy, Unhealthy, Disabled)
- Endpoint detail/analytics view: response time graph, uptime %, check log, incident history
- Custom date range picker with quick presets (24h, 7d, 30d)
- Settings page: view profile (read-only), configure alert email
- Desktop-optimized (mobile responsiveness not a priority)
- Dark mode and accessibility features supported

## Security & Compliance
- Google OAuth 2.0 is the sole authentication method (no username/password)
- Short-lived JWT access tokens (15-30 min) with refresh tokens for seamless renewal
- Only one user role: client (no admin/observer)
- Fully isolated multi-tenancy: each client can only see their own endpoints and data
- Data retention: logs and monitoring data kept indefinitely
- No explicit compliance (e.g., GDPR) or audit log requirements at this stage

## Non-Technical Requirements
- Easy to use and lightweight
- Customizable branding (logo, colors) — configured globally via appsettings.json by deployer
- SMTP email configured globally in appsettings.json by deployer; all alerts sent from one address
