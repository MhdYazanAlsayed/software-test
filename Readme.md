# Project Title: Date Matcher API with Monitoring

## Description

A web API that finds all month–year pairs where a specific day of the month falls on a specific weekday (e.g., all Saturdays that are the 15th) within a given year range. Built-in logging and monitoring dashboard.

## Technologies

- .NET 9 / ASP.NET Core
- Entity Framework Core (SQL Server)
- xUnit for unit tests
- Docker
- Nginx

## How to Run

### Option 1 — Run with Docker (Recommended)

1. **Install Docker**

   Download and install Docker Desktop from the official website:

   https://www.docker.com/products/docker-desktop/

   Make sure Docker and Docker Compose are installed correctly.

---

2. **Create an environment file**
   Create a file named `.env` in the root of the project and add the following variables:

   ```
   MSSQL_SA_PASSWORD=MyP@ssw0rd
   MONITORING_PASSWORD=MyP@ssw0rd
   ```

   These variables are used for:
   - SQL Server SA password
   - Monitoring dashboard authentication

---

3. **Run the system**

   Execute the following command in the project root:

   ```bash
   docker compose -f docker-compose.prod.yaml --env-file .env up -d
   ```

   Docker will start the following services:
   - SQL Server
   - Date Matcher API
   - Nginx

---

4. **Access the application**

   You can see the software test page using:

   [http://localhost:8081/](http://localhost:8081/)

---

### Option 2 — Run Locally (Without Docker)

1. **Install prerequisites**
   - .NET 9 SDK  
     https://dotnet.microsoft.com/download/dotnet/9.0

   - SQL Server  
     https://www.microsoft.com/en-us/sql-server/sql-server-downloads

---

2. **Configure the database connection**

   Update the connection string inside:

   ```
   Website.Api/appsettings.Development.json
   ```

   Example:

   ```json
   {
     "Monitoring": {
       "Password": "Your_Password"
     },
     "ConnectionStrings": {
       "Default": "Your_Connection_String"
     },
     "Origions": ["Your_CORS_Origins"]
   }
   ```

3. **Run the application**

   From the solution root:

   ```bash
   dotnet restore
   dotnet build
   dotnet run --project Website.Api
   ```

4. **Run tests**

   ```bash
   dotnet test Website.Tests
   ```

---

# Assumptions

During the implementation I made several assumptions and design decisions in order to keep the solution simple while still demonstrating scalability considerations.

### API-first approach

The solution is implemented as a REST API rather than a server-side rendered application.

This approach provides full control over the HTTP pipeline, allowing the system to capture detailed request and response information for monitoring purposes.

With server-side rendering, the response would typically be a full HTML page, which makes structured request/response logging less flexible.

Using an API-based approach makes the logging and monitoring system more precise and reusable.

---

### Separation of concerns in logging

The monitoring system separates two different types of logs:

- **Request Logs** – HTTP request/response information.
- **Entry Logs** – general application logs produced via `ILogger`.

This separation improves maintainability and allows each log type to evolve independently (different retention policies, storage strategies, etc.).

---

### Buffered in-memory logging

Logs are first stored in an **in-memory buffer** and periodically flushed to the database.

Advantages:

- Reduces the number of database writes
- Improves performance under higher request rates

Trade-off:

- Logs stored in memory may be lost if the application crashes before they are flushed.

In a large-scale production system this could be replaced with an asynchronous pipeline (e.g., message queues such as RabbitMQ or Kafka).

---

### Reverse proxy and protection layer

An **Nginx reverse proxy** is placed in front of the API.

Responsibilities:

- Serving the static HTML monitoring dashboard
- Acting as a protection layer for the API
- Applying rate limiting
- Allowing future horizontal scaling of API instances

This setup also enables running multiple API containers behind the reverse proxy to distribute load.

---

### Docker-based deployment

Docker is used to simplify deployment and environment consistency.

Benefits include:

- Easy setup of API and database services
- Reproducible environments
- Ability to scale horizontally by running multiple API containers

In the current architecture, Nginx can distribute traffic across multiple API instances.

---

### Testing scope

Due to time constraints, the current test suite focuses on **unit testing the core business service (`IFinderService`)**.

Integration tests for:

- API endpoints
- logging middleware
- monitoring components

were intentionally omitted for simplicity.

In a production system, integration and end-to-end tests would be added to verify the complete request pipeline.

# Documentation

## Projects

- `ObserveTool`: Reusable monitoring library (logging, dashboard, EF)
- `Website.Api`: Main API with date-matching endpoint
- `Website.Tests`: Unit tests

## 1. ObserveTool

It is a tool designed using C# classlibrary technology to make it easily reusable in other projects. Its main function is to capture logs and every request & response receive on the server in a database to query it later when needed.

## How to use it ?

At `Program.cs` you can use like this code :

```CSharp
var builder = WebApplication.CreateBuilder(args);

//Add my log service
builder.Services.AddInProcessMonitoring(connectionString, config =>
{
    config.AutoMigrateOnStartup = true;
    config.FlushIntervalSeconds = 30;
    config.Security.EnableAuth = true;
    config.Security.Password = "Password of basic authentication";
});

var app = builder.Build();

// Add the dashboard to see logs.
app.UseMonitorDashboard();

// Allow tool to save every request/response in the database.
app.UseInProcessMonitoring();

// Your core ..
```

## Architecture

```ObserveTool/
├── Contexts/
│   └── MonitoringDbContext.cs          # EF Core database context for logs
├── Extensions/
│   ├── DashboardExtensions.cs           # Adds dashboard services and endpoints
│   └── MonitoringExtensions.cs          # Registers monitoring services with DI
├── Helpers/
│   └── EmbeddedFileReader.cs            # Reads embedded UI files (dashboard)
├── Middlewares/
│   ├── BasicAuthenticationMiddleware.cs # Protects the dashboard with basic auth
│   └── RequestLoggingMiddleware.cs      # Captures HTTP request/response data
├── Models/
│   ├── RequestLog.cs                     # Entity for storing request details
│   └── LogEntry.cs                        # Represents a log entry (could be generic)
├── Options/
│   ├── MonitoringOptions.cs               # Configuration options (connection string, paths)
│   └── MonitoringSecurityOptions.cs       # Security settings (authentication, roles)
├── Services/
│   ├── Implementations/
│   │   ├── Buffers/
│   │   │   ├── InProcessLogBuffer.cs        # In-memory buffer for logs
│   │   │   └── InProcessLogRequestBuffer.cs # In-memory buffer for request logs
│   │   ├── Loggers/
│   │   │   └── Loggers.cs                    # Actual logging implementations
│   │   ├── BufferedLoggerProvider.cs        # ILoggerProvider that uses buffers
│   │   ├── LogCleanupService.cs             # Background service to clean old logs
│   │   └── LogFlushBackgroundService.cs     # Periodically flushes buffers to DB
│   └── Interfaces/
│       ├── ILogBuffer.cs                    # Contract for log buffering
│       ├── ILogEntryBuffer.cs                # For buffering generic log entries
│       ├── ILogRequestBuffer.cs               # For buffering request logs
│       └── IRequestLogger.cs                  # Contract for request logging
└── UI/
    └── index.html                           # Embedded dashboard frontend
```

## Important files

**1. DashboardExtensions.cs**

It is a file containing extension methods used to set up an HTML page with the necessary APIs to display logs to the user.
It uses BasicAuthMiddleware.cs to apply authentication on API and HTML pages.

**2. MonitoringExtensions.cs**

This file contains extension methods that add the necessary services to the project's DI container. It also configures the middleware that logs all API requests and responses.

**3. EmbeddedFileReader.cs**

This file embeds an HTML page inside a DLL file so that it can be read and returned to the browser to display the logs stored in the database.

**4. RequestLoggingMiddleware.cs**

This file contains Middleware that reads each Request and Response and adds it to the service, which is a Queue (we will explain it later), in order to write them to the database.

You may notice that we have ignored writing some files, such as images, and temporarily placed them inside a list called `IgnoredExtensions`.

```CSharp
private static readonly string[] IgnoredExtensions = new[]
{
    ".css", ".js", ".png", ".jpg", ".jpeg", ".gif",
    ".svg", ".ico", ".woff", ".woff2", ".ttf",
    ".map"
};
```

**5. LogEntryBuffer.cs & LogRequestBuffer.cs**

Both files represent queue management services that temporarily store logs. A separate service then retrieves a specific quantity of logs and saves them to the database in one batch.

Using this model improves performance and reduces database writes, but it carries the risk of losing logs stored in memory if the application crashes unexpectedly and they haven't yet been recorded in the database.

**6. RequestLogger.cs**

It's a very very simple service, just to use the `InProcessLogRequestBuffer` service.

**7. BufferedLoggerProvider.cs**

It is a file that inherits from ILoggerProvider in order to create a BufferedLogger entity when using the ILogger interface (which is the default interface for logging).

**8. BufferedLogger.cs**

It is a logger whose purpose is to store logs using the ILogEntryBuffer service instead of writing them to the console. The ILogEntryBuffer service then stores them in memory until they are stored in the database by a Job.

**9. LogFlushBackgroundService.cs**

This service is a background job configured by the client when registering and using the service. It retrieves all logs from memory and records them to the database at intervals determined by a variable `FlushIntervalSeconds`.

**10. LogCleanupService.cs**

This service deletes old logs that are more than a month old. Its purpose is to maintain the database and prevent data from growing uncontrollably. It's not the best way to do this, but it's currently the easiest and simplest.

## 2. Website.Api

Is a simple Restful API project to handle the Date finder algorithm.
It contains a simple service called `IFinderService` finds all month–year pairs.

**Improtant environment variables for developement**

In `appsettings.development.json` you can find :

```json
{
  "Monitoring": {
    "Password": "Your_Password"
  },
  "ConnectionStrings": {
    "Default": "Your_Connection_String"
  },
  "Origions": ["http://localhost:5500/"]
  // ...
}
```

## 3. Website.Tests

You can run the tests using the .NET CLI:

```bash
dotnet test Website.Tests
```

Or use your preferred IDE (Visual Studio, Rider, VS Code) to execute the tests.

**Note:**

The test is cover only the `IFinderService` for simplicity.
