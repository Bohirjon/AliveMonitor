
## Backend/API
- ASP.NET Core Web API (.NET 10, C#)
- Entity Framework Core for database access
- PostgreSQL (Azure Database for PostgreSQL in production)
- Aspire for development orchestration
- Swagger for API documentation
- Google OAuth 2.0 as the sole authentication method (no username/password)
- JWT tokens (short-lived access + refresh tokens) issued after Google sign-in
- Only one user role: client (no admin/observer)
- Hangfire for background health checks and notifications (using PostgreSQL for job storage)
- Deployed on Azure cloud

## Real-Time
- SignalR for pushing live status updates from backend to dashboard (WebSocket)

## Frontend
- React (with TypeScript)
- React Router for navigation
- TanStack Query (React Query) for server state management and API caching
- Shadcn/ui component library + Tailwind CSS for styling
- Recharts for analytics charts (response time graphs, uptime charts)
- Axios for HTTP API calls
- Customizable branding (logo, colors)
- Supports dark mode and accessibility features
- Desktop-optimized (mobile responsiveness not a priority)

## CI/CD
- GitHub Actions for build, test, and deploy pipeline
- Deploys to Azure cloud
