# 📐 GoVoylo Backend Developer Guidelines

Welcome to the GoVoylo Engine. This project is built using **Clean Architecture** combined with **CQRS (Vertical Slices)**. The architecture is designed for extreme scale, 100% testability, and zero boilerplate duplication.

As a developer on this project, you must adhere to the strict rules outlined in this document.

---

## 🛑 Rule #1: The Dependency Direction Rule
Our architecture is organized like an onion. **Dependencies only point inward.**

```text
[ API (Web/Mobile/Agents Gateway) ]
        │
        ▼
[ Infrastructure (PostgreSQL/MongoDB/HTTP) ]
        │
        ▼
[ Application (Use Cases/MediatR/Validation) ]
        │
        ▼
[ Domain (Entities/Business Rules/Interfaces) ] ◄── CORE (Zero Dependencies)
```

### The Architectural Layer Constraints:
* **The Core Rule**: The `Domain` project has **zero** external references (no entity framework, no third-party packages, no databases). 
* **The Infrastructure Rule**: The application layer does *not* talk to PostgreSQL or MongoDB databases directly. It talks to interfaces inside the `Domain`.

---

## ⚡ Rule #2: Use Feature Folders (Vertical Slices)
Do **not** group files by technical type (e.g., do not put all DTOs or all Handlers in one massive shared folder). Group everything by its **Business Capability**.

### How to add a new business requirement:
Create a self-contained feature folder inside `Application/Features/`:

```text
Application/Features/Bookings/Commands/CreateBooking/
├── CreateBookingCommand.cs          <── The input parameters (Record)
├── CreateBookingCommandValidator.cs <── Input validation rules
└── CreateBookingCommandHandler.cs   <── The traffic cop (Orchestrator)
```

* **The Benefit**: If the Booking process breaks, you only edit files in *one* folder. You will never risk accidentally breaking the Payments system.

---

## 🚫 Rule #3: No Manual Try-Catch or Validation Boilerplate
We have automated the most exhausting cross-cutting concerns using **MediatR Pipeline Behaviors**. 

### 1. The Validation Automation
Do **not** write manual validation check logic (such as checking values using `if`) inside your command handlers or API controllers. Instead, write an `AbstractValidator<T>` next to your command.
* **How it works**: The global `ValidationBehavior` intercepts the request, runs your rules, and stops processing if validation fails.
* **The Benefit**: Your handlers remain perfectly clean and only execute when data is 100% verified.

### 2. The Logging Automation
Do **not** inject `ILogger` into every single handler just to write `"Starting handler X"`. 
* **How it works**: The global `LoggingBehavior` automatically prints execution logs and timing statistics for every request entering the system.

---

## 💎 Rule #4: Use Records for Data, Classes for State
To prevent random data modifications across different execution threads:

* **DTOs and Commands** must be declared as C# `record`. They are immutable data buckets. Once they are built, their properties are locked.
* **Domain Entities** (like `PaymentTransaction`) must be standard `class` objects with **private setters** (`public string Status { get; private set; }`). State can only change through explicit entity methods (e.g., `transaction.MarkAsFailed()`).

---

## 🛠️ Cheat Sheet: "How Do I Code This Requirement?"

| If you need to... | Write it in... | Example Component |
| :--- | :--- | :--- |
| Calculate taxes, percentages, or core fees | **Domain Entity** | `PaymentTransaction.CreateDebitCard()` |
| Save to DB or hit a third-party API | **Domain Interface** + **Infrastructure Class** | `IPaymentRepository` |
| Sequence a multi-step workflow | **Application Command Handler** | `ProcessPaymentCommandHandler` |
| Check if input formatting is valid | **Application Validator** | `ProcessPaymentCommandValidator` |
