# DocumizeConnector

A custom **Microsoft 365 Copilot Connector** (formerly Microsoft Graph Connector) that indexes content from a self-hosted [Documize](https://www.documize.com/) knowledge base instance, making it searchable and accessible through Microsoft 365 Copilot and Microsoft Search.

## About

DocumizeConnector bridges your organization's self-hosted [Documize Community](https://github.com/documize/community) knowledge base with Microsoft 365 Copilot. By implementing the [Microsoft 365 Copilot Connectors SDK](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-overview), this connector crawls documents from a Documize instance via its REST API and ingests them into the Microsoft Graph — making your internal documentation discoverable through Microsoft Search, Copilot, and other Microsoft 365 surfaces.

**Key capabilities:**

- Authenticates against a Documize instance using Bearer token authentication
- Crawls spaces and documents from the Documize REST API
- Defines a schema for indexing document metadata (title, content, space, author, timestamps, etc.)
- Streams crawl items to the Microsoft Graph connector platform for indexing
- Supports full crawls via the Graph Connector Agent

---

## How It Works

This project is a C# gRPC service built on top of the Microsoft 365 Copilot Connectors SDK template. The connector agent communicates with it over a local gRPC channel during setup and crawl operations.

The three core service implementations are:

- **`ConnectorInfoServiceImpl`** — Declares the connector's unique ID, display name, and supported authentication types to the agent.
- **`ConnectionManagementServiceImpl`** — Handles connection validation. Authenticates against the Documize API using the provided credentials and validates the data source URL. Also defines the schema of properties to be indexed.
- **`ConnectorCrawlerServiceImpl`** — Performs the actual crawl. Queries the Documize REST API for spaces and documents, converts each document into a `CrawlItem`, and streams them back to the platform.

---

## Prerequisites

Before you begin, ensure you have the following:

1. **[Microsoft Graph Connector Agent](https://learn.microsoft.com/en-us/microsoftsearch/graph-connector-agent)** — Downloaded, installed, and configured on a Windows machine.
2. **[Visual Studio 2019 or later](https://visualstudio.microsoft.com/)** — With the **.NET SDK** (version compatible with the project's target framework).
3. **A running Documize instance** — The connector communicates with Documize over its REST API. You'll need:
   - The base URL of your Documize instance (e.g., `https://docs.yourcompany.com`)
   - A valid Documize user account and API credentials (email + password for Bearer token auth)
4. **Microsoft 365 tenant** — With permissions to register and manage Graph connectors (typically a Microsoft 365 admin account).

---

## Getting Started

### 1. Install the Extension

1. Open Visual Studio and go to **Extensions → Manage Extensions**.
2. Search for **GraphConnectorsTemplate** and download it.
3. Close and relaunch Visual Studio to complete installation.

### 2. Build and Run

1. Open the solution (`DocumizeConnector.slnx`) in Visual Studio.
2. Build the solution (**Build → Build Solution**).
3. Run the project. The gRPC server will start and listen for incoming requests from the Graph Connector Agent.

---

## Testing Locally

Use the **TestApp** utility bundled with the Microsoft Graph Connector Agent to validate your connector before deploying it.

> The TestApp does **not** create live connections or ingest data — it only tests the connector's service responses locally.

### Step 1 — Update `ConnectionInfo.json`

Located at: `C:\Program Files\Graph connector agent\TestApp\Config\ConnectionInfo.json`

```json
{
  "id": "DocumizeConnectorTest",
  "name": "DocumizeConnectorTest",
  "description": "Local test connection for Documize",
  "configuration": {
    "providerId": "<YOUR_PROVIDER_ID_FROM_ConnectorInfoServiceImpl.cs>",
    "scheduleSetting": {
      "fullSyncInterval": 30
    },
    "CredentialData": {
      "Path": "https://docs.yourcompany.com",
      "AuthenticationKind": "Basic",
      "CredentialDetails": {
        "loginId": "your-documize-email@example.com",
        "loginSecret": "your-documize-password"
      }
    },
    "ProviderParameters": null
  }
}
```

- Set `providerId` to the value found in `ConnectorInfoServiceImpl.cs`.
- Set `Path` to your Documize base URL.
- Set `loginId` and `loginSecret` to your Documize credentials.

### Step 2 — Update `CustomConnectorPortMap.json`

Located at: `C:\Program Files\Graph connector agent\CustomConnectorPortMap.json`

```json
{
  "<YOUR_PROVIDER_ID>": "30303"
}
```

Replace `<YOUR_PROVIDER_ID>` with the connector's unique ID and `30303` with the actual port from `ConnectorServer.cs` if different.

### Step 3 — Update `manifest.json`

Located at: `C:\Program Files\Graph connector agent\TestApp\Config\manifest.json`

```json
{
  "connectorId": "<YOUR_PROVIDER_ID>",
  "authTypes": ["Basic"]
}
```

### Step 4 — Run the TestApp

1. Make sure your connector project is running (gRPC server is up).
2. Run `GraphConnectorAgentTest.exe` from `C:\Program Files\Graph connector agent\TestApp\`.
3. Step through all available options (1–5) to validate each aspect of the connector:
   - Authentication validation
   - Custom configuration validation
   - Schema retrieval
   - Full crawl stream
   - Incremental crawl stream

---

## Deploying

Once local testing passes, follow Microsoft's guide to deploy and register the connector with your Microsoft 365 tenant:

- [Deploy your connector](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-hosting)

This involves registering the connector through the Microsoft 365 admin center and configuring a scheduled crawl.

---

## Project Structure

```
DocumizeConnector/
├── DocumizeConnector.slnx              # Visual Studio solution file
└── DocumizeConnector/
    ├── Connector/
    │   ├── ConnectorInfoServiceImpl.cs     # Declares connector identity & auth types
    │   ├── ConnectionManagementServiceImpl.cs  # Validates credentials & defines schema
    │   ├── ConnectorCrawlerServiceImpl.cs  # Crawls Documize API & streams items
    │   └── ConnectorServer.cs              # gRPC server setup & port configuration
    ├── Models/
    │   └── (Documize document models)  # Data models for Documize API responses
    └── Data/
        └── (Data loader classes)       # Documize REST API client / data fetcher
```

---

## References

- [Build a custom Copilot connector in C#](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-create) — The guide this project is based on.
- [Test your Copilot connector](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-test) — Local testing guide using the TestApp utility.
- [Deploy your connector](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-hosting) — Deployment guide for production.
- [Microsoft 365 Copilot Connectors SDK overview](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-overview)
- [Documize Community](https://github.com/documize/community) — The open-source knowledge base platform this connector indexes.
- [Documize REST API Reference](https://docs.documize.com/s/WtXNJ7dMOwABe2UK/api/d/WtWbRbdMOwABe2SK/authentication)

---

## License

This project is licensed under the **GNU General Public License v3.0**. See [LICENSE.txt](../LICENSE.txt) for details.
