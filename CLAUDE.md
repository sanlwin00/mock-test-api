# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MockTestApi is a C# ASP.NET Core 8.0 web API for a mock test SaaS application. It provides authentication, subscription management, payment processing, and test-taking functionality using MongoDB as the database.

## Build and Development Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Run with specific profile
dotnet run --launch-profile https

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

The application runs on:
- HTTP: http://localhost:5290
- HTTPS: https://localhost:7203

## Architecture Overview

### Core Technologies
- **Framework**: ASP.NET Core 8.0 with minimal APIs
- **Database**: MongoDB with MongoDB.Driver
- **API Pattern**: Carter framework for module-based endpoint organization
- **Authentication**: JWT Bearer tokens
- **Background Jobs**: Hangfire with MongoDB storage
- **Logging**: Serilog with file, console, and Seq sinks
- **Rate Limiting**: AspNetCoreRateLimit
- **Payments**: Stripe integration
- **Email**: SendGrid, SMTP, and Brevo support
- **Mapping**: AutoMapper

### Project Structure

The codebase follows a clean architecture pattern:

```
/Endpoints/     - Carter modules defining API routes (AuthModule, MockTestModule, etc.)
/Services/      - Business logic layer with interface contracts
  /Interfaces/  - Service interface definitions
/Data/          - Data access layer (repositories)
  /Interfaces/  - Repository interface definitions
/Models/        - Domain models and DTOs
/Utils/         - Utility classes and helpers
/Templates/     - HTML email templates
/Mapping/       - AutoMapper profiles
```

### Key Domain Models

Based on `doc/datastructure.txt`, the main entities are:
- **User**: Authentication, profiles, subscriptions
- **Question**: Test questions with options, categories, explanations
- **Test**: Question sets with access control (free/paid)
- **MockTest**: User test sessions with results and progress saving
- **Payment**: Stripe payment processing with billing info
- **AuditLog**: Activity tracking across the system

### Configuration

Multiple environment-specific appsettings files:
- `appsettings.json` - Base configuration
- `appsettings.citizenshiptest.json` - Citizenship test environment
- `appsettings.raventest.json` - Raven test environment
- Development variants with `.dev.json` suffix

### Key Features

1. **Progress Saving**: Saves test progress mid-session
2. **Audit Trail**: Comprehensive logging across all modules for security and compliance
3. **Rate Limiting**: IP-based rate limiting for API protection
4. **Multi-tenancy**: Support for different test environments (citizenship, raven)
5. **Payment Integration**: Full Stripe payment processing with webhook support
6. **Email System**: Multiple email providers with template support
7. **Background Processing**: Hangfire for async task processing
8. **Scoped Question Fetch**: `GET /questions/{locale}?testId=xxx` returns only the questions belonging to a specific test, ordered by sequence. Falls back to all questions when `testId` is omitted (`Modules/TestAndQuestionModule.cs`). Implemented in `QuestionService.GetQuestionsByTestIdAsync` via `QuestionRepository.GetByIdsAsync`.

### CI / CD Pipeline

Both `deploy-prod.yml` and `deploy-uat.yml` use a **3-job pipeline** with explicit `needs:` dependencies:

| Stage | Job | What it does |
|---|---|---|
| 1 | `build` | `dotnet restore` + `dotnet build -c Release` |
| 2 | `test` | `dotnet test MockTestApi.Tests/...` — gates deploy |
| 3 | `deploy` | Generate appsettings from secrets → publish → FTP |

The `environment:` protection (production / uat) is on the `deploy` job only.

### Payment / Pricing Rules

**`POST /payments/create_session`** (`Services/PaymentService.cs`):
- **Price validation**: only `12.90` and `9.90` are accepted. Any other value throws `InvalidOperationException("Invalid price: {value}")`. Comparison uses `Math.Abs(p - price) < 0.001` to handle floating-point drift.
- **`PromoId`** (`StripeRequestDto.PromoId`): optional string sent by the frontend when the first-visit discount (`FIRST24`) is active. Logged via Serilog for audit; not used server-side to compute price (price whitelist is the enforcement mechanism).
- **`StripeRequestDto`**: `Currency` and `Product.Price` come from the frontend; backend does not store pricing config — it validates against the hardcoded allowed set.

**Allowed prices:**
| Value | Context |
|---|---|
| `12.90` | Base price (citizenshiptest tenant) |
| `9.90` | First-visit 24-hour discount (`promoId = "FIRST24"`) |

If additional prices or tenants are added, update the allowed-prices array in `PaymentService.CreateSession`.

### Authentication & Security

- JWT-based authentication with configurable expiration
- XSRF protection with custom headers
- Password hashing and secure storage
- Audit logging for all user actions
- Rate limiting to prevent abuse

