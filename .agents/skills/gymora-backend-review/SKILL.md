---
name: gymora-backend-review
description: Reusable agent skill for reviewing Gymora backend controllers, DTOs, models, EF Core configurations, repositories, services, caching, and shared services prior to production deployment.
---

# Gymora Backend Production-Ready Review Guideline

This skill defines the step-by-step checklist and architecture trace guidelines to review the Gymora Backend, one controller at a time, prior to production deployment. 

When invoked with a target controller (e.g., `MembershipPlansController`), follow the instructions below to trace, inspect, and fix any issues across all layers rather than analyzing the controller in isolation.

---

## 1. Trace Workflow Overview
For the given controller, identify and trace all connected files across the layers:
1. **API Layer**: Controller (`Api/Controllers/{Name}Controller.cs`)
2. **Application Layer**: 
   * Service Interface (`Application/Interface/Service/I{Name}Service.cs`)
   * Service Implementation (`Application/Service/{Name}Service.cs`)
   * DTOs & Validators (`Application/DTO/Model/{Name}DTO.cs` or folder, and FluentValidation classes)
   * AutoMapper Profile (`Application/DTO/MapperConfig.cs`)
3. **Domain Layer**: 
   * Domain Models (`Domain/Model/{Name}.cs`)
   * Interfaces implemented (`IBaseGymEntity`, `IOnlyMeCanSee`, `IOnlyMeCanSeeAtGym`, `ICacheableEntity`)
4. **Infrastructure Layer**:
   * Repository Interface (`Application/Interface/Repo/I{Name}Repo.cs`)
   * Repository Implementation (`Infrastructure/Repo/{Name}Repo.cs`)
   * EF Core Configurations (`Infrastructure/Configurations/{Name}Config.cs`)
5. **Caching & Events**:
   * Cache key configuration and cache invalidation consumers (`Application/EventConsumer/CacheInvalidationConsumer.cs` or related events)

---

## 2. Layer-by-Layer Verification Checklist

### 2.1. DTOs & Validation Review
* **Request DTOs (Create/Update)**:
  * Validate that required fields have correct types (e.g., strings are not nullable unless optional).
  * Ensure a corresponding `AbstractValidator<T>` (FluentValidation) exists.
  * Verify validation rules: check for string length limits, non-negative range validations on IDs and prices, date range constraints, enum ranges, and nested DTO validation rule propagation.
* **Response DTOs**:
  * Verify that all navigation properties that need to be returned have corresponding response DTOs.
  * Navigation DTOs representing optional relationships must be nullable (e.g., `PlanPriceRDTO?`).
  * Ensure that the mapping configuration and repository query actually populate these related DTOs.

### 2.2. Domain Models & Database Configurations
* **Base Classes & Tenancy**:
  * Verify the model inherits from the correct base class (e.g., `BaseGymAuditableEntity`, `BaseGymEntity`, `BaseAuditableEntity`, etc.) matching its requirements.
  * Verify that `GymId` is used if the entity is gym-scoped (`IBaseGymEntity`).
  * Check for `[Searchable]` on fields queried via full-text search.
  * Check for `[Filterable(FilterType)]` on fields used in exact match/range queries.
* **EF Core Mappings & Indexes**:
  * Ensure correct configuration in `Infrastructure/Configurations/` for keys, foreign keys, and cascade paths (using `DeleteBehavior.Restrict` where loops/cycles are possible).
  * Check that composite indexes (like `(GymId, Status)`) are defined *only* if they are justified by actual query filters. Do not duplicate primary key or simple unique indexes.
* **Postman Sync on Schema/Filter Changes**:
  * If any modifications are made to model search/filter attributes (`[Searchable]`, `[Filterable]`), response DTO properties, or endpoint validations, ensure the corresponding Postman collection file in `Postman/` is updated.
  * Specifically, update the markdown table inside the description of the `GetPaged` endpoint, adjust request body structures, and run `python generate_collection.py` to rebuild `Gymora Complete Collection.postman_collection.json`.

### 2.3. Shared Interfaces Enforcement
* **`IOnlyMeCanSee`**:
  * Properties: `int CreatedById { get; set; }`
  * Enforcement: Ensure queries, updates, and deletes restrict access to the creator (`CreatedById == CurrentUserId`), unless bypassed by `SuperAdmin` role.
* **`IOnlyMeCanSeeAtGym`**:
  * Properties: `int CreatedByPersonId { get; set; }`
  * Enforcement: Ensure queries and operations restrict access to the person who created the record within the Gym context.
* **`IBaseGymEntity`**:
  * Properties: `int GymId { get; set; }`
  * Enforcement: Ensure that all actions enforce tenant separation (`GymId == CurrentGymId`). Clients must not be able to bypass this.
* **`ICacheableEntity`**:
  * Ensure caching is active only for models implementing `ICacheableEntity`.
  * Verify that the cache keys respect the scope: Gym-scoped entities must include `GymId`, and user-scoped entities must include `UserId` or `PersonId` in the cache key.

### 2.4. Service Layer Review
* **Design & Transactions**:
  * Ensure all business logic remains in the service layer, keeping controllers thin.
  * Ensure `CancellationToken` is passed and propagated to all async operations.
  * Check database transactions: operations that modify multiple entities/tables must use the unit of work transaction context. Read-only operations must not trigger transactions.
  * Ensure read/caching queries do not track entities (normally `AsNoTracking` / `trackChanges = false`).
* **Shared Service Reuse**:
  * Avoid duplicating logic. Specifically, check if the service requires the owner's active subscription or plan-related limits. If so, inject and use the `CurrentPlanService` instead of writing custom subscription checks.
* **Retrieval Splits**:
  * Utilize the base split retrieval pathways in the services:
    1. **`GetByIdAsync`**: Light-weight, minimal fields, no includes. Used for standard retrieval and validations (routes to repository's `GetByIdAsync`).
    2. **`GetByIdDetailsAsync`**: Detailed read with navigation properties/Includes. Used for details/show API endpoints (routes to repository's `GetByIdDetailsAsync`). This method does not accept `trackChanges` since it is strictly for read-only purposes.

### 2.5. Repository Review
* **Retrieval Splits**:
  * Enforce two distinct retrieval pathways declared in the repository base interfaces and classes:
    1. **`GetByIdAsync`**: Light-weight, minimal fields, no includes. Declared in `IBaseRepo<T>` and implemented in `BaseRepo<T>`.
    2. **`GetByIdDetailsAsync`**: Detailed read with necessary navigation properties/Includes. Declared in `IBaseRepo<T>` and implemented in `BaseRepo<T>` to throw `NotImplementedException` by default. Child repositories must override this method to define custom detailed includes if details are needed. This method does not accept a `trackChanges` parameter because it is strictly read-only.
  * Optimize EF Core queries: avoid N+1 queries, eliminate unused `.Include(...)` calls, and avoid materializing queries too early (keep as `IQueryable` before paginating or projecting).
  * Ensure proper tracking rules: `trackChanges` should be `false` for reads and `true` only for updates/deletes when required.

### 2.6. Caching & Invalidation
* **Cache Management**:
  * Verify the cache key matches the entity's access scope (e.g. `CacheKeyGenerator.ById<T>`).
  * Ensure cache invalidation occurs immediately after Create, Update, or Delete (e.g., via MassTransit `EntityChangedEvent` consumed by `CacheInvalidationConsumer`).
  * Prevent cache leakages: ensure global users cannot see gym cache, and different gyms/users don't share keys.

### 2.7. Exceptions & Error Handling
* Avoid throwing generic exceptions. Utilize existing business exceptions under `Application/DTO/Errors/Exceptions`.
* If a new business exception is needed:
  1. Define it inheriting from a base exception under `Application/DTO/Errors/Exceptions`.
  2. Map it to the appropriate HTTP status code inside `Api/Middlewares/ExceptionHandlingMiddleware.cs`.

### 2.8. Security, Rate Limiting & Auth Flow Review
* **Controller Response Wrapper**:
  * Every controller action/endpoint must return a wrapped `Result` or `Result<T>` response (e.g., `Task<ActionResult<Result<T>>>` or `Task<ActionResult<Result>>`). Direct un-wrapped DTOs, primitive types, or raw objects must not be returned.
* **Mandatory Authorization Scoping**:
  * Verify that every controller class or action endpoint enforces authorization. No action should be left unsecured unless explicitly marked with `[AllowAnonymous]`.
  * Platform/Application-wide actions targeting system roles (e.g., `SuperAdmin`, standard `User`) must use the standard ASP.NET Core `[Authorize]` attribute (e.g., `[Authorize(Roles = $"{AppRole.SuperAdmin}")]`, `[Authorize]`, or `[AllowAnonymous]`).
  * Gym/Tenant-scoped actions must use the custom `[GymAuthorize]` filter attribute (e.g., `[GymAuthorize]` or `[GymAuthorize(GymRoleString.Owner)]`) to validate gym membership and gym-specific roles.
* **Rate Limiting**:
  * Ensure rate limiting policies (e.g., `[EnableRateLimiting("Ip_5Limit_1Min")]`) are applied to all public endpoints (login, registration) and sensitive flows (OTP verification/resending, password resets, and changes).
* **Claims Validation & Body Spoofing**:
  * For authenticated endpoints, never fetch the user ID (`UserId`) from the request body if it can be spoofed by a malicious user. Strictly extract it from the authenticated context (`User` claims or `CurrentUser` helper).
* **Token Rotation (Security Context Matching)**:
  * Validate user ID ownership match between the access token and the refresh token during token rotation or switch-gym flows to prevent token substitution attacks.
* **Gym Context Membership Persistence**:
  * During token rotation (or SwitchGym), check that the user still has access to the target gym (e.g., via `_gymAccessRepo.GetGymAccessAsync`). If they are kicked out or deactivated, clear the gym context (reset claims/gym fields to 0 or null) to revoke their access claims dynamically.
* **Password Policy Consistency**:
  * Ensure password validation complexity regex (requiring uppercase, lowercase, digit, and special character) is applied consistently across registration, password resets, and change password requests.

---

## 3. Review Process & Final Report

When a controller is submitted:
1. **Locate & Search**: Trace all related files starting from the controller.
2. **Verify & Inspect**: Check each file against the layer checklists.
3. **Fix on the Fly**: If you find incorrect implementations, missing FluentValidation rules, wrong mapping profiles, missing repository Includes, or incorrect caching configurations, **modify the code directly** to align with the rest of Gymora's conventions.
4. **Compile & Test**: Build the project and run tests (`dotnet test`) to verify changes do not break existing logic.
5. **Generate Report**: Provide a concise summary with the following fields:
   * **Controller reviewed**: [Controller Name]
   * **Main files/layers inspected**: List of files traced.
   * **Changes made**: Bulleted list of code modifications.
   * **Remaining issues**: List of warnings or unresolved queries.
   * **Final status**: `PASS` (no issues), `PASS WITH WARNINGS` (working but has warnings), or `FAIL` (build/test errors, or unresolved security/tenant leaks).
