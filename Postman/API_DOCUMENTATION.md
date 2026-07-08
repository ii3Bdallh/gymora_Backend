# Gymora API Documentation

## Overview

Complete API documentation for the Gymora ASP.NET Core 9.0 REST API.

**Base URL:** `https://localhost:7059` (local) / `https://api.gymora.com` (production)

## Architecture

| Component | Technology |
|-----------|-----------|
| Runtime | ASP.NET Core 9.0 |
| Authentication | JWT Bearer Token + Refresh Tokens |
| Identity | ASP.NET Core Identity (int PK) |
| ORM | Entity Framework Core + SQL Server |
| Validation | DataAnnotations |
| Background Jobs | Hangfire + MassTransit InMemory + EF Outbox |
| Caching | Redis (optional) |
| File/Video Storage | Bunny.net (Stream + Storage) |
| Push Notifications | Firebase Cloud Messaging |
| Payments | Paymob |
| Logging | Serilog (Console + File) |
| API Documentation | Swagger / OpenAPI |

---

## Controllers & Endpoints

### 1. AuthController (`/api/Auth`)

| # | Method | Route | Auth | Rate Limit | Description |
|---|--------|-------|------|------------|-------------|
| 1 | POST | `/api/Auth/register` | Anonymous | — | Register a new user |
| 2 | POST | `/api/Auth/confirm-email` | Anonymous | — | Confirm email with OTP |
| 3 | POST | `/api/Auth/resend-confirmation-email` | Anonymous | Ip_3Limit_5Min | Resend confirmation OTP |
| 4 | POST | `/api/Auth/login` | Anonymous | Ip_5Limit_1Min | Login with email/password |
| 5 | POST | `/api/Auth/login-google` | Anonymous | — | Login with Google ID token |
| 6 | POST | `/api/Auth/refresh-token` | Anonymous | — | Refresh JWT tokens |
| 7 | POST | `/api/Auth/logout` | Authorized | — | Logout and invalidate refresh token |
| 8 | POST | `/api/Auth/forgot-password` | Anonymous | Ip_3Limit_5Min | Request password reset OTP |
| 9 | POST | `/api/Auth/verify-otp` | Anonymous | Ip_10Limit_1Min | Verify password reset OTP |
| 10 | POST | `/api/Auth/reset-password` | Anonymous | Ip_10Limit_1Min | Reset password with verified OTP |
| 11 | GET | `/api/Auth/confirm-email` | Anonymous | Ip_10Limit_1Min | Confirm email via link (HTML) |
| 12 | POST | `/api/Auth/change-password` | Authorized | — | Change current user password |
| 13 | GET | `/api/Auth/get-user-profile` | Authorized | — | Get authenticated user profile |

### 2. SubscriptionPlanController (`/api/SubscriptionPlan`)

| # | Method | Route | Auth | Roles | Description |
|---|--------|-------|------|-------|-------------|
| 1 | POST | `/api/SubscriptionPlan` | Anonymous | — | Get paginated subscription plans |
| 2 | GET | `/api/SubscriptionPlan/{id}` | Anonymous | — | Get subscription plan by ID |
| 3 | POST | `/api/SubscriptionPlan/Create` | Authorized | SuperAdmin | Create a subscription plan |
| 4 | PUT | `/api/SubscriptionPlan/{id}` | Authorized | SuperAdmin | Update a subscription plan |
| 5 | DELETE | `/api/SubscriptionPlan/{id}` | Authorized | SuperAdmin | Soft-delete a subscription plan |
| 6 | POST | `/api/SubscriptionPlan/PlanPrices/Create` | Authorized | SuperAdmin | Create a plan price |
| 7 | DELETE | `/api/SubscriptionPlan/PlanPrices/{id}` | Authorized | SuperAdmin | Soft-delete a plan price |

### 3. TestController (`/api/Test`)

| # | Method | Route | Auth | Description |
|---|--------|-------|------|-------------|
| 1 | POST | `/api/Test/{parentId}/add-child` | Anonymous | Test transactional outbox pattern |
| 2 | POST | `/api/Test/send-notification` | Anonymous | Test FCM push notification |
| 3 | POST | `/api/Test/send-email` | Anonymous | Test SMTP email |

---

## Authentication

### JWT Token Format

```
Authorization: Bearer <jwt-access-token>
```

### Token Lifetime

| Token | Expiration | Configuration |
|-------|-----------|---------------|
| Access Token | 60 minutes | `Jwt:AccessTokenExpirationMinutes` |
| Refresh Token | 7 days | `Jwt:RefreshTokenExpirationDays` |

### Login Response

```json
{
  "isSuccess": true,
  "data": {
    "id": 1,
    "email": "ahmed.mohamed@example.com",
    "personName": "Ahmed Mohamed",
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "refreshtoken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "roles": ["User"],
    "refreshTokenExpirationDate": "2026-07-09T09:31:46Z"
  }
}
```

### Token Refresh

```
POST /api/Auth/refresh-token
{
  "refreshToken": "a1b2c3d4-...",
  "accessToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

## Roles

| Role | Enum Value | Description |
|------|-----------|-------------|
| `SuperAdmin` | 0 | Full system access |
| `User` | 1 | Standard authenticated user |

---

## Standard Response Envelope

### Success
```json
{
  "isSuccess": true,
  "data": { ... }
}
```

### Error
```json
{
  "isSuccess": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message"
  }
}
```

### Error Codes

| HTTP Status | Code | Source |
|------------|------|--------|
| 400 | `BAD_REQUEST` | `BadRequestException` middleware |
| 400 | `VALIDATION_ERROR` | `ModelState.IsValid` check |
| 401 | `UNAUTHORIZED` | JWT auth / `UnauthorizedException` |
| 403 | `FORBIDDEN` | `ForbiddenException` / role check |
| 404 | `NOT_FOUND` | `NotFoundException` |
| 429 | `RATE_LIMIT_EXCEEDED` | Rate limiter middleware |
| 500 | `INTERNAL_ERROR` | Unhandled exceptions |

---

## Rate Limiting

| Policy Name | Limit | Window | Endpoints |
|------------|-------|--------|-----------|
| `Ip_5Limit_1Min` | 5 requests | 1 minute | POST `/api/Auth/login` |
| `Ip_3Limit_5Min` | 3 requests | 5 minutes | POST resend-confirmation-email, POST forgot-password |
| `Ip_10Limit_1Min` | 10 requests | 1 minute | POST verify-otp, POST reset-password, GET confirm-email |

---

## Pagination

### Request

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": null,
  "orderBy": "Id",
  "orderDirection": "asc",
  "isActive": true,
  "createdById": null,
  "filters": null
}
```

### Response

```json
{
  "isSuccess": true,
  "data": {
    "totalCount": 42,
    "pageSize": 10,
    "pageNumber": 1,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true,
    "items": [...]
  }
}
```

### Advanced Filtering

```json
{
  "filters": {
    "betweenFilters": {
      "price": { "min": "100", "max": "500" }
    },
    "exactFilters": {
      "categoryId": ["1", "2", "3"]
    }
  }
}
```

### Constraints

| Field | Constraint |
|-------|-----------|
| `pageSize` | Max 50 |
| `searchTerm` | Max 100 characters |

---

## DTO Reference

### Auth DTOs

#### RegisterReqDto
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |
| `userName` | string | Yes | 3-100 characters |
| `password` | string | Yes | 8-100 chars, uppercase, lowercase, digit, special char |
| `phoneNumber` | string | No | Max 50 characters |

#### LoginReqDto
| Field | Type | Required |
|-------|------|----------|
| `email` | string | Yes |
| `password` | string | Yes |

#### LoginResDto
| Field | Type | Description |
|-------|------|-------------|
| `id` | int | User ID |
| `email` | string | User email |
| `personName` | string | Display name |
| `token` | string | JWT access token |
| `expiresIn` | int | Token lifetime in seconds |
| `refreshtoken` | string | Refresh token |
| `roles` | string[] | Assigned roles |
| `refreshTokenExpirationDate` | DateTime | Refresh token expiry |

#### RefreshTokenReqDto
| Field | Type | Required |
|-------|------|----------|
| `refreshToken` | string | Yes |
| `accessToken` | string | Yes |

#### LogoutRequest
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `refreshToken` | string | null | Token to invalidate |
| `logoutFromAllDevices` | bool | false | Invalidate all tokens |
| `userId` | int | (auto) | Set from JWT claim |

#### ConfirmEmailRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |
| `otp` | string | Yes | Exactly 5 digits |

#### ResendConfirmationRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |

#### ForgotPasswordRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |

#### VerifyOtpRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |
| `otp` | string | Yes | Max 6 characters |

#### ResetPasswordRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `email` | string | Yes | Email format |
| `otp` | string | Yes | Max 6 characters |
| `newPassword` | string | Yes | 8-100 chars, uppercase, lowercase, digit, special char |

#### ChangePasswordRequest
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `currentPassword` | string | Yes | — |
| `newPassword` | string | Yes | Min 8 characters |

#### GoogleLoginRequest
| Field | Type | Required |
|-------|------|----------|
| `idToken` | string | Yes |

#### GetUserProfileDto
| Field | Type | Description |
|-------|------|-------------|
| `email` | string? | User email |
| `personName` | string? | Display name |
| `phoneNumber` | string? | Phone number |
| `roles` | string[]? | Assigned roles |

### Subscription Plan DTOs

#### SubscriptionPlanCDTO (Create)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Plan name |
| `description` | string | No | Plan description |
| `maxOwnedGyms` | int | Yes | Max gyms |
| `maxCoachesPerGym` | int | Yes | Max coaches per gym |
| `maxMembersPerGym` | int | Yes | Max members per gym |
| `featuresJson` | string | No | JSON array of features |
| `prices` | PlanPriceCDTO[] | No | Plan prices |

#### SubscriptionPlanUDTO (Update)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Plan name |
| `description` | string | No | Plan description |
| `maxOwnedGyms` | int | Yes | Max gyms |
| `maxCoachesPerGym` | int | Yes | Max coaches per gym |
| `maxMembersPerGym` | int | Yes | Max members per gym |
| `featuresJson` | string | No | JSON array of features |
| `isActive` | bool | Yes | Active status |
| `prices` | PlanPriceUDTO[] | No | Plan prices |

#### SubscriptionPlanRDTO (Read)
| Field | Type | Description |
|-------|------|-------------|
| `id` | int | Plan ID |
| `name` | string | Plan name |
| `description` | string? | Plan description |
| `maxOwnedGyms` | int | Max gyms |
| `maxCoachesPerGym` | int | Max coaches per gym |
| `maxMembersPerGym` | int | Max members per gym |
| `featuresJson` | string? | JSON features |
| `isActive` | bool | Active status |
| `createdOn` | DateTime | Creation timestamp |
| `prices` | PlanPriceRDTO[] | Plan prices |

#### PlanPriceCDTO (Create)
| Field | Type | Description |
|-------|------|-------------|
| `planId` | int | Subscription Plan ID |
| `countryCode` | string | ISO country code |
| `currencyCode` | string | ISO currency code |
| `durationMonths` | int | Duration in months |
| `amount` | decimal | Price amount |

#### PlanPriceUDTO (Update)
| Field | Type | Description |
|-------|------|-------------|
| `countryCode` | string | ISO country code |
| `currencyCode` | string | ISO currency code |
| `durationMonths` | int | Duration in months |
| `amount` | decimal | Price amount |

#### PlanPriceRDTO (Read)
| Field | Type | Description |
|-------|------|-------------|
| `id` | int | Price ID |
| `planId` | int | Subscription Plan ID |
| `countryCode` | string | ISO country code |
| `currencyCode` | string | ISO currency code |
| `durationMonths` | int | Duration in months |
| `amount` | decimal | Price amount |
| `createdOn` | DateTime | Creation timestamp |

---

## Validation Rules Summary

### Password Policy (Identity)
| Rule | Value |
|------|-------|
| Min Length | 8 characters |
| Require Digit | Yes |
| Require Uppercase | Yes |
| Require Lowercase | Yes |
| Require Non-Alphanumeric | No |
| Max Failed Attempts | 5 |
| Lockout Duration | 15 minutes |

### Password Regex (Register & Reset)
```
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$
```
Must contain: at least one lowercase, one uppercase, one digit, one special character.

---

## Enums

### RoleType
| Value | Name |
|-------|------|
| 0 | SuperAdmin |
| 1 | User |

### FileType
| Value | Name |
|-------|------|
| 0 | Video |
| 1 | PDF |
| 2 | Audio |
| 3 | Document |
| 4 | Image |
| 5 | Other |

### BunnyUploadStatus
| Value | Name |
|-------|------|
| 0 | Queued |
| 1 | Processing |
| 2 | Encoding |
| 3 | Finished |
| 4 | ResolutionFinished |
| 5 | Failed |
| 6 | PresignedUploadStarted |
| 7 | PresignedUploadFinished |
| 8 | PresignedUploadFailed |
| 9 | CaptionsGenerated |
| 10 | TitleOrDescriptionGenerated |

---

## Middleware Pipeline

```
1. Serilog Logging
2. Swagger / SwaggerUI (Development only)
3. Hangfire Dashboard (/hangfire)
4. CORS (FrontendClient policy)
5. ExceptionHandlingMiddleware
6. Static Files
7. Routing
8. HTTPS Redirection
9. Authentication (JWT Bearer)
10. Authorization
11. Rate Limiter
12. MapControllers
13. Health Checks (/healthz)
```

### CORS Policy
- **Name:** `FrontendClient`
- **Allowed Origins:** `http://localhost:3000`, `http://localhost:5059`
- **Methods:** All
- **Headers:** All
- **Credentials:** Allowed

---

## Services (Internal Architecture)

### Auth Service
- `IAuthService` → `AuthService`
- Register, Login, RefreshToken, Logout, ForgotPassword, VerifyOtp, ResetPassword, ChangePassword, LoginWithGoogle, GetUserProfile, ConfirmEmail, ResendConfirmationEmail

### Subscription Plan Service
- `ISubscriptionPlanService` → `SubscriptionPlanService`
- Full CRUD via `BaseService<T,R,C,U>` base class
- PlanPrice management (Add, Delete, GetById)

### Shared Services
| Interface | Implementation | Description |
|-----------|---------------|-------------|
| `IEmailService` | `EmailService` | SMTP email |
| `INotificationService` | `NotificationService` | FCM push |
| `IBunnyStorageService` | `BunnyStorageService` | Bunny file storage |
| `IBunnyStreamService` | `BunnyStreamService` | Bunny video streaming |
| `IBunnyCollectionService` | `BunnyCollectionService` | Bunny collection CRUD |
| `ICacheService` | (Redis) | Distributed caching |
| `IFileStorageService` | (Bunny) | File storage abstraction |
| `IBackgroundJobService` | (Hangfire) | Background jobs |
| `ICurrentUserService` | `CurrentUserService` | Current user context |
| `ICurrentGymService` | `CurrentGymService` | Current gym context |

---

## Background Jobs (Hangfire)

- **Dashboard:** `/hangfire`
- **Recurring Jobs:** Registered via `RecurringJobs.Register()` in Program.cs
- **Token Cleanup:** `TokenCleanupJob` removes expired refresh tokens

## MassTransit Events

- **Bus:** InMemory
- **Outbox:** Entity Framework Core Outbox (SQL Server)
- **Consumers:**
  - `NotificationConsumer` - Sends push notifications
  - `EmailConsumer` - Sends emails

---

## Configuration (appsettings.json)

### Required Sections

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=gymoraDb;..."
  },
  "Jwt": {
    "Issuer": "http://quizly.runasp.net",
    "Audience": "mobile-client",
    "SecretKey": "...",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Bunny": {
    "LibraryId": "...",
    "CdnHostName": "...",
    "StreamApiKey": "...",
    "StorageApiKey": "...",
    "PullZoneUrl": "...",
    "StorageName": "..."
  },
  "Mail": {
    "FromEmail": "...",
    "FromPassword": "..."
  },
  "FirebaseConfig": {
    "CredentialFilePath": "firebase-config.json"
  }
}
```

---

## Postman Collection

### Folders

1. **01 Authentication** - All auth workflows (register, login, profile, password mgmt)
2. **02 Subscription Plans** - Plan and PlanPrice CRUD
3. **03 Test & Utilities** - Development test endpoints
4. **04 Negative Test Cases** - Error and validation scenarios

### Collection Variables

| Variable | Auto-Set By |
|----------|------------|
| `accessToken` | Login, Login Google, Refresh Token |
| `refreshToken` | Login, Login Google, Refresh Token |
| `currentUserId` | Login, Login Google |
| `createdEntityId` | Create Subscription Plan |
| `createdPlanPriceId` | Create Plan Price |

### Automation Scripts

- **Login** → Automatically saves `accessToken`, `refreshToken`, `currentUserId` to collection variables
- **Refresh Token** → Automatically updates `accessToken` and `refreshToken`
- **Create Subscription Plan** → Automatically saves `createdEntityId`
- **Create Plan Price** → Automatically saves `createdPlanPriceId`

---

## Health Checks

**Endpoint:** `GET /healthz`

Returns the health status of the application.

---

## Notes & Known Gaps

1. **No API Versioning Detected** - The source code does not implement API versioning (no `ApiVersionAttribute` or versioned routes).
2. **No FluentValidation** - Validation uses DataAnnotations only; no FluentValidation assemblies/references were found.
3. **Test Endpoints** - The TestController endpoints are for development/debugging and should be disabled in production.
4. **Redis Not Configured** - The `appsettings.json` has empty Redis connection strings. Must be configured per environment.
5. **Paymob Service** - The `IPaymobService` interface exists but is not registered as a service in DI (commented out).
6. **No Controller for Topics** - The `TopicCDTO`, `TopicRDTO`, `TopicUDTO` DTOs exist with full validation, but no TopicController was found. The corresponding service and repo are also absent from the DI registrations. This suggests the Topic feature is incomplete or was removed.
