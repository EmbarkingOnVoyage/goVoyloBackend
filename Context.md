# GoVoylo Clean Architecture Context

## Solution Blueprint
- **Style**: Clean Architecture with CQRS (MediatR), SOLID, TDD.
- **Pattern**: Vertical Slice / Feature Folders (`Features/FeatureName/Commands/`)
- **Naming**: File-Scoped Namespaces, PascalCase matching file names exactly.
- **Testing**: xUnit, FluentAssertions, NSubstitute. Three test projects inside root `tests/` folder.
- **Traffic & Security**: Built-in ASP.NET Core Rate Limiting (`HeavyTrafficPolicy`).

## Implemented Architecture State

### 1. GoVoylo.Domain
- `Common/BaseEntity.cs` (Id, CreatedAt)
- `Entities/BookingPayment.cs` (BookingReference, TotalAmount, Currency, PaymentStatus)
- `Entities/UserActivityLog.cs` (UserId, ActionType, PayloadJson, SourcePlatform)
- `Interfaces/IPaymentRepository.cs` (GetByReferenceAsync, SaveAsync)
- `Interfaces/IActivityLogRepository.cs` (LogActivityAsync)
- `Interfaces/ITravelSupplierClient.cs` (GetLiveOffersAsync)

### 2. GoVoylo.Application
- `Features/Payments/Dtos/PaymentResponseDto.cs`
- `Features/Payments/Commands/ProcessPayment/ProcessPaymentCommand.cs`
- `Features/Payments/Commands/ProcessPayment/ProcessPaymentCommandHandler.cs`
  - *Note*: Dynamically logs `request.SourceClient` straight to `IActivityLogRepository`.

### 3. GoVoylo.Infrastructure
- `Persistence/EntityFramework/ApplicationDbContext.cs` (EF Core, InMemory dev mapping for PostgreSQL target)
- `Persistence/Repositories/PaymentRepository.cs`
- `Persistence/Repositories/ActivityLogRepository.cs` (Console simulation placeholder for MongoDB target)
- `DependencyInjection.cs` (`AddInfrastructureServices` extension method)

### 4. GoVoylo.Api
- `Program.cs` (Explicit `public class Program` block structure, sets up MediatR, DI, and `AddRateLimiter`)
- `Controllers/PaymentsController.cs` (Uses `[EnableRateLimiting("HeavyTrafficPolicy")]`)

### 5. Automated Tests (`tests/` folder)
- `GoVoylo.Domain.UnitTests/Entities/BookingPaymentTests.cs` (Passed)
- `GoVoylo.Application.UnitTests/Features/Payments/Commands/ProcessPayment/ProcessPaymentCommandHandlerTests.cs` (Passed)
- `GoVoylo.Api.IntegrationTests/Controllers/PaymentsControllerTests.cs` (Uses `WebApplicationFactory<Program>`, Passed)
