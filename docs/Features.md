# AliveMonitor Features

## 1. User Authentication & Onboarding

### Google Sign-Up/Sign-In (Only)
**Description:** Clients can only sign up and sign in via their Google account. There is no username/password registration or login.

**Sign-In Flow:**
1. Client visits AliveMonitor web app
2. Clicks "Sign in with Google"
3. Authenticates via Google OAuth 2.0
4. Upon successful authentication, backend issues a short-lived JWT access token and a refresh token
5. Redirected to Dashboard

**Sign-Out Flow:**
1. Client clicks "Sign out"
2. Frontend clears stored tokens
3. Redirected to Sign-In page

**Technical Details:**
- Google OAuth 2.0 is the **sole authentication method** (no email/password)
- Short-lived JWT access token (15-30 min) issued after successful Google authentication
- Refresh token issued alongside for seamless token renewal without re-login
- Tokens used for subsequent API calls
- User profile (name, email, avatar) derived from Google account
- On first sign-up, a client record is created with alert email defaulting to the Google account email
- **Multi-tenancy:** Each client's data is fully isolated — clients can only see their own endpoints and monitoring data

---

## 2. Endpoint Management

### Add Endpoint
**Description:** Clients can add new endpoints/services to monitor from the dashboard.

**Fields:**
- **Friendly Name** (required): Human-readable name for the service (e.g., "Production API", "Dev Server")
- **Endpoint URL** (required): Full URL to monitor (must be valid HTTP/HTTPS URL)
- **Monitoring Interval** (required): Custom interval in minutes (minimum: 1 minute)
- **Timeout** (optional): Maximum time in seconds to wait for a response (default: 30 seconds). If the endpoint does not respond within this time, it is considered unhealthy.
- **Custom Headers** (optional): List of key-value pairs (e.g., `Authorization: Bearer <token>`, `X-Api-Key: abc123`). These headers are included in every health check GET request for that endpoint. Useful for authenticated or protected APIs.
- **Health Check Rules** (optional):
  - Default: HTTP status code 200 AND response body's `status` property equals "healthy"
  - Customizable per endpoint: client can specify the expected HTTP status code and a JSON property name + expected value to check in the response body
- **Status** (required): Enabled/Disabled toggle (default: Disabled)

**Validation:**
- URL must be valid HTTP/HTTPS format
- Friendly name cannot be empty
- Monitoring interval must be at least 1 minute

**Behavior:**
- Newly added endpoints are **disabled by default** (monitoring does not start automatically)
- Client must manually enable the endpoint to start monitoring

### Edit Endpoint
**Description:** Clients can edit any field of an existing endpoint.

**Behavior:**
- All fields can be modified
- Changes take effect immediately
- If monitoring interval is changed while endpoint is enabled, the new interval applies to the next scheduled check

### Delete Endpoint
**Description:** Clients can delete endpoints they no longer want to monitor.

**Behavior:**
- Confirmation prompt before deletion
- Associated monitoring history is retained indefinitely (as per data retention policy)
- Active monitoring stops immediately upon deletion

### Enable/Disable Endpoint
**Description:** Clients can toggle monitoring on/off for each endpoint without deleting it.

**Behavior:**
- **Enabled**: Hangfire background job schedules health checks at the specified interval
- **Disabled**: No health checks are performed; endpoint remains in the system

---

## 3. Monitoring & Health Checks

### Health Check Execution
**Description:** When an endpoint is enabled, AliveMonitor periodically sends HTTP GET requests to verify health.

**Process:**
1. Hangfire job executes at the configured interval
2. Sends HTTP GET request to the endpoint URL (includes any custom headers configured for that endpoint)
3. Evaluates health based on the configured rules:
   - Default: HTTP 200 status code AND response body's `status` property equals "healthy"
   - Custom: client-specified expected HTTP status code and a JSON property name + expected value
4. Records result (timestamp, status code, response time, health status)
5. If unhealthy, triggers alert

**Health Status:**
- **Healthy**: Meets all configured health criteria
- **Unhealthy**: Fails one or more health criteria (e.g., non-200 status, wrong response body, timeout)

---

## 4. Alerts

### Architecture Overview

The alert system follows a **pluggable provider pattern**. An `IAlertService` interface defines the contract, and concrete implementations (e.g., `EmailAlertService`) are registered via DI. This allows adding new channels (Slack, SMS, webhook, etc.) in the future without modifying core logic.

```
IAlertService (interface)
  ├── EmailAlertService (implemented now)
  ├── SlackAlertService (future)
  ├── SmsAlertService (future)
  └── WebhookAlertService (future)
```

The active alert provider is configured in `appsettings.json`:
```json
{
  "Alerts": {
    "Provider": "Email",
    "ThrottleIntervalMinutes": 10,
    "Email": {
      "SmtpHost": "...",
      "SmtpPort": 587,
      "SenderAddress": "...",
      "SenderName": "AliveMonitor"
    }
  }
}
```

### Client Alert Configuration
**Description:** Each client configures their alert preferences in their profile/settings page.

**Fields:**
- **Alert Email** (required): Defaults to the client's Google account email on sign-up. Client can change it to any valid email address.

**Behavior:**
- All endpoint alerts for that client are sent to their configured alert email
- Client can update their alert email at any time via the Settings page

---

### Incident State Machine

Each monitored endpoint tracks an **incident state** to manage alert lifecycle and prevent duplicate alerts.

**States:**
- **Healthy**: Endpoint is responding correctly. No active incident.
- **Incident Open**: Endpoint failed a health check. An incident has been created and the first alert sent.
- **Incident Ongoing**: Endpoint continues to fail. Throttled repeat alerts are sent.
- **Resolved**: Endpoint recovered. Recovery alert sent. Incident closed.

**State Transitions:**
```
[Healthy] --(health check fails)--> [Incident Open]
    - Create incident record (endpointId, openedAt, lastNotifiedAt)
    - Send IMMEDIATE alert

[Incident Open / Ongoing] --(health check fails again)--> [Incident Ongoing]
    - Check throttle: if (now - lastNotifiedAt) >= ThrottleIntervalMinutes
        → Send repeat failure alert, update lastNotifiedAt
    - Otherwise: skip alert, just log the failure

[Incident Open / Ongoing] --(health check succeeds)--> [Resolved]
    - Close incident (set resolvedAt)
    - Send RECOVERY alert
    - Transition back to [Healthy]

[Healthy] --(health check succeeds)--> [Healthy]
    - No action, log success
```

**Incident Record Schema:**
| Field             | Type     | Description                              |
|-------------------|----------|------------------------------------------|
| Id                | GUID     | Unique incident identifier               |
| EndpointId        | GUID     | FK to the monitored endpoint             |
| OpenedAt          | DateTime | When the incident was first detected     |
| LastNotifiedAt    | DateTime | When the last alert was sent              |
| ResolvedAt        | DateTime?| When the endpoint recovered (null=open)  |
| FailureCount      | int      | Number of consecutive failures           |

---

### Throttling Mechanism

**Description:** Prevents alert spam for ongoing failures.

**Configuration (appsettings.json):**
- `Alerts:ThrottleIntervalMinutes` — Global setting (default: 10 minutes)
- Applies to all endpoints uniformly

**Logic:**
1. On health check failure, check if an incident is already open for this endpoint
2. If **no open incident** → create incident, send alert immediately
3. If **open incident exists** → check `lastNotifiedAt`
   - If `(now - lastNotifiedAt) >= ThrottleIntervalMinutes` → send repeat alert, update `lastNotifiedAt`
   - Otherwise → skip alert (log check failure only)

---

### Email Alerts (Current Provider)

#### Failure Alert
**Trigger:** First failure or throttled repeat failure
**Subject:** `⚠️ AliveMonitor Alert: [Friendly Name] is Unhealthy`
**Body includes:**
- Endpoint friendly name
- Endpoint URL
- Timestamp of failure
- HTTP status code received (if applicable)
- Error details
- Number of consecutive failures (if repeat)
- Duration of downtime (if repeat)
- Link to dashboard

#### Recovery Alert
**Trigger:** Endpoint becomes healthy after an open incident
**Subject:** `✅ AliveMonitor: [Friendly Name] has Recovered`
**Body includes:**
- Endpoint friendly name
- Endpoint URL
- Timestamp of recovery
- Total downtime duration
- Link to dashboard

---

## 5. Dashboard

### Endpoint List View
**Description:** Main dashboard displays all endpoints with their current status.

**Displayed Information:**
- Friendly name
- Endpoint URL
- Current health status (Healthy/Unhealthy/Disabled)
- Last check timestamp
- Monitoring interval
- Actions: Edit, Delete, Enable/Disable toggle

**Features:**
- Real-time status updates via SignalR (WebSocket push from server)
- Quick enable/disable toggle
- Color-coded status indicators (green = healthy, red = unhealthy, gray = disabled)
- Search by endpoint name or URL
- Filter by status (All, Healthy, Unhealthy, Disabled)

### Endpoint Detail / Analytics View
**Description:** Clicking an endpoint opens a detail view with historical monitoring data.

**Displayed Information:**
- Response time graph over time (line chart)
- Uptime percentage (24h, 7d, 30d)
- Health check event log / timeline (last N checks with timestamps, status codes, response times)
- Incident history (past downtime events with duration)
- Current endpoint configuration

**Time Range:**
- Custom date range picker (client selects any start/end date)
- Quick presets: Last 24h, 7d, 30d

### Settings Page
**Description:** Client can view their profile and configure alert preferences.

**Displayed Information:**
- Profile info (name, email, avatar — read-only, derived from Google account)
- Alert email (editable, defaults to Google email on sign-up)

---

## Technical Constraints & Notes

- **No limit** on the number of endpoints per client
- **No role-based access**: All clients have the same permissions
- **Data retention**: All monitoring logs and history retained indefinitely
- **Customizable branding**: Dashboard supports custom logo and colors (configured globally via appsettings.json)
- **Dark mode and accessibility**: Supported in the UI
- **Desktop-optimized**: Mobile responsiveness is not a priority
- **Real-time**: SignalR used for pushing status updates to the dashboard

