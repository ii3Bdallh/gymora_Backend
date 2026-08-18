---
name: gymora-backend-review
description: Reusable agent skill for reviewing Gymora backend controllers, DTOs, models, EF Core configurations, repositories, services, caching, and shared services prior to production deployment.
---

# Gymora Backend Production-Ready Review Guideline

This skill defines the step-by-step checklist and architecture trace guidelines to review the Gymora Backend, one controller at a time, prior to production deployment.

When invoked with a target controller (e.g., `OwnerSubscriptionController`), follow the instructions below to trace, inspect, and fix any issues across all layers rather than analyzing the controller in isolation.

---

## 1. Trace Workflow Overview

For the given controller, identify and trace all connected files across the layers:

1. **API Layer**: Controller (`Api/Controllers/{Name}Controller.cs`)
2. **Application Layer**:
   - Service Interface (`Application/Interface/Service/I{Name}Service.cs`)
   - Service Implementation (`Application/Service/{Name}Service.cs`)
   - DTOs & Validators (`Application/DTO/Model/{Name}DTO.cs` or folder, and FluentValidation classes)
   - AutoMapper Profile (`Application/DTO/MapperConfig.cs`)
3. **Domain Layer**:
   - Domain Models (`Domain/Model/{Name}.cs`)
   - Interfaces implemented (`IBaseGymEntity`, `IOnlyMeCanSee`, `IOnlyMeCanSeeAtGym`, `ICacheableEntity`)
4. **Infrastructure Layer**:
   - Repository Interface (`Application/Interface/Repo/I{Name}Repo.cs`)
   - Repository Implementation (`Infrastructure/Repo/{Name}Repo.cs`)
   - EF Core Configurations (`Infrastructure/Configurations/{Name}Config.cs`)
5. **Caching & Events**:
   - Cache key configuration and cache invalidation consumers (`Application/EventConsumer/...`)
6. **Tests**:
   - Controller Unit Tests (`UnitTests/Controllers/{Name}ControllerTests.cs`)
   - Service Unit Tests (`UnitTests/Services/{Name}ServiceTests.cs`)
7. **Postman & Automation**:
   - `FILTER_MAP` entry in `generate_collection.py`
   - Postman collection files in `Postman/`

---

## 2. Layer-by-Layer Verification Checklist

### 2.1. DTOs & Navigation Properties Review

- **Request DTOs (Create/Update)**:
  - Validate that required fields have correct types (e.g., strings are not nullable unless optional).
  - Check DataAnnotations attributes: string length limits, non-negative range validations on IDs and prices, date range constraints, enum ranges, and nested DTO validation rule propagation.
- **Response DTOs (`{Name}RDTO`)**:
  - **Mandatory Navigation DTOs**: Any entity with navigation properties MUST expose those navigation properties in its response DTO (`{Name}RDTO`) as nullable Response DTOs.
    - Example: `CouponRDTO? Coupon`, `PaymentRequestRDTO? PaymentRequest`, `SubscriptionPlanRDTO? Plan`, `PlanPriceRDTO? PlanPrice`.
  - Ensure AutoMapper profile in `Application/DTO/MapperConfig.cs` maps the entity to `{Name}RDTO` completely.

### 2.2. Domain Models & Database Configurations

- **Base Classes & Tenancy**:
  - Verify the model inherits from the correct base class (`BaseGymAuditableEntity`, `BaseGymEntity`, `BaseAuditableEntity`, etc.) matching its requirements.
  - Verify that `GymId` is used if the entity is gym-scoped (`IBaseGymEntity`).
  - Check for `[Searchable]` on fields queried via full-text search.
  - Check for `[Filterable(FilterType)]` (e.g. `FilterType.Exact`, `FilterType.Between`) on fields queried via exact match or range queries.
- **EF Core Mappings & Indexes**:
  - Ensure correct configuration in `Infrastructure/Configurations/` for keys, foreign keys, and cascade paths (using `DeleteBehavior.Restrict` where loops/cycles are possible).
  - Check that composite indexes (like `(GymId, Status)`) are defined _only_ if they are justified by actual query filters. Do not duplicate primary key or simple unique indexes.
- **Postman Sync on Schema/Filter Changes**:
  - If any modifications are made to model search/filter attributes (`[Searchable]`, `[Filterable]`), response DTO properties, or endpoint validations, ensure the corresponding Postman collection file in `Postman/` is updated.
  - Specifically, update the markdown table inside the description of the `GetPaged` endpoint, adjust request body structures, and run `python generate_collection.py` to rebuild `Gymora Complete Collection.postman_collection.json`.

### 2.3. Shared Interfaces Enforcement

- **`IOnlyMeCanSee`**:
  - Properties: `int CreatedById { get; set; }`
  - Enforcement: Ensure queries, updates, and deletes restrict access to the creator (`CreatedById == CurrentUserId`), unless bypassed by `SuperAdmin` role.
- **`IOnlyMeCanSeeAtGym`**:
  - Properties: `int CreatedByPersonId { get; set; }`
  - Enforcement: Ensure queries and operations restrict access to the person who created the record within the Gym context.
- **`IBaseGymEntity`**:
  - Properties: `int GymId { get; set; }`
  - Enforcement: Ensure that all actions enforce tenant separation (`GymId == CurrentGymId`). Clients must not be able to bypass this.
- **`ICacheableEntity`**:
  - Ensure caching is active only for models implementing `ICacheableEntity`.
  - Verify that the cache keys respect the scope: Gym-scoped entities must include `GymId`, and user-scoped entities must include `UserId` or `PersonId` in the cache key.

### 2.4. Service Layer Review

- **Design & Transactions**:
  - Ensure all business logic remains in the service layer, keeping controllers thin.
  - Ensure `CancellationToken` is passed and propagated to all async operations.
  - Check database transactions: operations that modify multiple entities/tables must use the unit of work transaction context. Read-only operations must not trigger transactions.
  - Ensure read/caching queries do not track entities (normally `AsNoTracking` / `trackChanges = false`).
- **Shared Service Reuse**:
  - Avoid duplicating logic. Specifically, check if the service requires the owner's active subscription or plan-related limits. If so, inject and use the `CurrentPlanService` instead of writing custom subscription checks.
- **Retrieval Splits**:
  - Utilize the base split retrieval pathways in the services:
    1. **`GetByIdAsync`**: Light-weight, minimal fields, no includes. Used for standard retrieval and validations (routes to repository's `GetByIdAsync`).
    2. **`GetByIdDetailsAsync`**: Detailed read with navigation properties/Includes. Used for details/show API endpoints (routes to repository's `GetByIdDetailsAsync`). This method does not accept `trackChanges` since it is strictly for read-only purposes.

- **Repository Review & Detailed Retrieval Split**:
  - For entities with navigation properties, the child repository (`Infrastructure/Repo/{Name}Repo.cs`) **MUST** override `Includes()` and `GetByIdDetailsAsync`:

    ```csharp
    protected override Func<IQueryable<EntityName>, IQueryable<EntityName>>? Includes()
    {
        return query => query.Include(x => x.Nav1).Include(x => x.Nav2);
    }

    public override async Task<EntityName?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(id, false, cancellationToken, Includes());
    }
    ```

- Enforce the two distinct retrieval pathways in EF Core:
  1. **`GetByIdAsync`**: Light-weight, minimal fields, no includes. Used for background checks, validation, and internal processing.
  2. **`GetByIdDetailsAsync`**: Detailed read with eager loading (`Includes()`). Used for API show/details endpoints.

### 2.4. Controller Layer Review

- **Endpoint Method Signatures**:
  - Every controller action/endpoint MUST return a wrapped `Result<T>` response with proper return type signatures:
    - `Task<ActionResult<Result<PaginatedRes<{Name}RDTO>>>>` for `GetPagedAsync`
    - `Task<ActionResult<Result<{Name}RDTO>>>` for `GetByIdAsync` and mutations.
- **`GetById` Endpoint Invocation**:
  - The `GetById` endpoint (`[HttpGet("{id}")]`) **MUST** invoke `service.GetByIdDetailsAsync(id, cancellationToken)` to return the detailed DTO with populated navigation properties.
- **Mandatory Authorization Scoping**:
  - System-wide roles: `[Authorize(Roles = $"{AppRole.SuperAdmin}")]` or `[Authorize]`.
  - Gym-scoped actions: `[GymAuthorize]` or `[GymAuthorize(GymRoleString.Owner)]`.

### 2.5. Unit & Integration Tests Sync

- Controller unit tests (`UnitTests/Controllers/{Name}ControllerTests.cs`) testing the `GetById` endpoint **MUST** mock `service.GetByIdDetailsAsync(id, cancellationToken)` instead of `GetByIdAsync`.
- Run `dotnet build` and `dotnet test` to ensure 100% compilation and passing test suite across all projects.

### 2.6. Postman & `generate_collection.py` Sync

- All `[Searchable]` and `[Filterable]` fields defined in the Domain Model **MUST** be reflected in `FILTER_MAP` inside [`generate_collection.py`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/generate_collection.py).
- Run `python generate_collection.py` to regenerate [`Postman/Gymora Complete Collection.postman_collection.json`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Postman/Gymora%20Complete%20Collection.postman_collection.json).

---

## 3. Review Process & Final Report

When a controller is submitted:

1. **Trace**: Inspect Controller -> Service -> DTO -> Domain Model -> Repo -> Config -> Tests -> Postman.
2. **Fix on the Fly**: Directly apply all required code modifications across layers.
3. **Build & Test**: Run `dotnet build` and `dotnet test`.
4. **Postman Regeneration**: Execute `python generate_collection.py`.
5. **Generate Report**: Present a clear summary with:
   - **Controller reviewed**: [Controller Name]
   - **Main files/layers inspected**: List of files traced.
   - **Changes made**: Bulleted list of code modifications.
   - **Remaining issues**: Warnings or unresolved queries.
   - **Final status**: `PASS`, `PASS WITH WARNINGS`, or `FAIL`.
