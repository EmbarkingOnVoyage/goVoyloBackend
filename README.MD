# 🚀 GoVoylo Developer Setup & Architectural Guide

Welcome to the **GoVoylo Backend** team! This project is built using **Clean Architecture** and follows a **CQRS (Command Query Responsibility Segregation)** pattern. It uses **Polyglot Persistence**, meaning we run two different database engines side-by-side:
1. **PostgreSQL**: Handles critical transaction data (Bookings, Payments) requiring strict relational consistency.
2. **MongoDB**: Handles high-volume analytical data (User Activity Tracking Logs, Telemetry).


## 🛠️ Part 1: Local Machine Database Setup

As a new developer, you do not need to manually install PostgreSQL or MongoDB onto your computer. Everything runs inside isolated **Docker Containers** using pre-configured images.

### Prerequisites
Before running the application, make sure you have the following software installed:
1. [.NET 10.0 SDK](https://microsoft.com)
2. [Docker Desktop](https://docker.com) *(Ensure it is opened and the status icon in the bottom-left corner is **Green**)*

### Step 1: Install the Entity Framework Core CLI Tools
Open your terminal (PowerShell or Command Prompt) and install the global database design tools by running:
```bash
dotnet tool install --global dotnet-ef
```
*(If it says it is already installed, you can safely skip this step).*

### Step 2: Spin Up the Database Containers
Navigate to the absolute root folder of the project (`C:\GITHUB\goVoyloBackend`) and run:
```bash
docker compose up -d
```
* **What this does:** Docker reads our `docker-compose.yml` file, automatically downloads the official PostgreSQL and MongoDB images, and starts them up in the background.
* **Verification:** Run `docker ps` in your terminal. You should see two active containers running: `govoyis_postgres` and `govoylo_mongodb`.

### Step 3: Apply the Database Schema Tables
Sync your newly created local PostgreSQL container with the team's latest structural tables by executing:
```bash
dotnet ef database update --project GoVoylo.Infrastructure --startup-project GoVoylo.Api
```
* **Verification:** Look for the word **`Done.`** at the bottom of the console stream. Your relational tables are now officially built!

## Part 2: Running the Project & Verifying Data

### 1. Launch the Live API
Run the Web API layer locally using the .NET CLI:
```bash
dotnet run --project GoVoylo.Api
```
Look at your terminal output to find the local port numbers your application is listening on (e.g., `http://localhost:5037`).

### 2. Run Automated Test Cases
To ensure that both databases, repository abstractions, and business rules are functional, run:
```bash
dotnet test
```
* **Note on Safety:** Our integration tests use **Database Transactions**. When a test runs, it inserts rows into your real local database to test queries, but automatically **Rolls Back (deletes)** the rows immediately after verifying. Your local database tables will always remain completely pristine and clutter-free!

## 🏗️ Part 3: Code Guidelines for Freshers

To keep our architecture clean and maintainable, you must adhere to our structural design layout rules.

### 1. The Core Rule of Dependencies (The "Castle Wall")

  [ GoVoylo.Api ] ──► [ GoVoylo.Application ] ──► [ GoVoylo.Domain ]
         │                        ▲
         ▼                        │
  [ GoVoylo.Infrastructure ] ─────┘

* **The Domain Layer** is the core inner shield. It must remain **100% pure C#**. You are strictly **forbidden** from installing database packages (like Entity Framework or MongoDB.Driver) or adding database attributes inside the Domain project.
* **The Application Layer** contains your feature business use cases. It can **never** reference the Infrastructure layer. It talks to databases strictly through abstract interfaces (`IPaymentRepository`).
* **The Infrastructure Layer** contains concrete structural mechanisms. This is the **only** layer where database connections, DbContext files, and drivers are permitted.


### 2. How to Add a New Feature (Step-by-Step Workflow)

If you are tasked with creating a new feature capability, execute your code in this exact linear order:

#### Step A: Define the Domain Interface
Create an abstraction contract inside your **`GoVoylo.Domain/Interfaces`** layer specifying how the database boundary operates using pure C# types:
```csharp
public interface INewFeatureRepository
{
    Task SaveDataAsync(NewEntity data, CancellationToken cancellationToken);
}
```

#### Step B: Implement the Concrete Infrastructure
Write the real database implementation inside **`GoVoylo.Infrastructure/Persistence/Repositories`**:
* For Relational database interactions, inject `ApplicationDbContext` and use Entity Framework.
* For Analytical data streams, inject `IConfiguration`, connect to a new collection using `IMongoCollection<T>`, and save flat documents into MongoDB.

#### Step C: Wire Up the Extensions Toggle
Open **`GoVoylo.Infrastructure/DependencyInjection.cs`** and register your concrete class mapping so the application can automatically locate it during startup:
```csharp
services.AddScoped<INewFeatureRepository, NewFeatureRepository>();
```

#### Step D: Build the CQRS Slices (Application Layer)
Navigate to your feature folder (e.g., `Features/Payments`). Do **not** create deeply nested subfolders for every request. Keep the layout flat and elegant:
* Group write-based changes under a **`Commands`** folder (`ProcessPaymentCommand.cs`).
* Group read-only views under a **`Queries`** folder (`GetPaymentByIdQuery.cs`).
* Put your DTO view contract models inside a **`Dtos`** folder (`PaymentDetailsDto.cs`).

#### Step E: Expose the Route (API Layer)
Keep your controllers completely thin. They should act solely as data gatekeepers, accepting requests, mapping parameters, and passing messages directly onto the **MediatR pipeline**:
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateFeatureCommand command)
{
    var id = await _mediator.Send(command);
    return Ok(id);
}
```

## 🔍 Part 4: Accessing Database Visualizers

If you want to view the stored data files on your machine through a graphical dashboard user interface:

### 🐘 Viewing PostgreSQL Data (via pgAdmin 4)
* **Host name/address:** `localhost`
* **Port:** `5433` *(Configured to 5433 to avoid port collisions with pre-existing local machine databases!)*
* **Maintenance database:** `govoyis_dev_db`
* **Username:** `govoylo_admin`
* **Password:** `secure_dev_password_2026`

### 🍃 Viewing MongoDB Analytics (via MongoDB Compass)
Connect via this direct unified connection string address string box:
```text
mongodb://govoylo_mongo_admin:secure_mongo_password_2026@localhost:27017/
```
Expand the directories to find **`govoylo_analytics_db`** ➡️ **`UserActivityLogs`** to verify your flat analytical JSON data documents stream smoothly!

