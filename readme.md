# Requirements

- [Docker](https://www.docker.com/) or [Podman (recommended)](https://podman.io/)
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

# How to launch

1. Mark AppHost as startup project
2. Build and Run solution
3. Click the link of "api" project in the new browser tab that launched
4. Execute Login endpoint with following credentials:
    - Email: admin@example.com
    - Password: Password123%

# Remarks

> [!IMPORTANT]
> All of the configuration for the app is stored in appsettings.json in AppHost project for convenience

> [!IMPORTANT]
> Email client needs to be configured in appsettings.json or provided through env vars or user secrets

> [!CAUTION]
> In production scenarios secrets should be stored in a key vault (e.g. Hashicorp Vault or Keycloak)

> [!NOTE]
> All relevant endpoints are behind an Authentication layer

> [!NOTE]
> Sql Server Viewer can be accessed with DbGate from the Dashboard. It requires ConnectionString to be either "authdb" or "notificationdb" with port 5432
> Aspire will automagically translate those ConnectionStrings
