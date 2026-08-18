
name: gymora-backend-review
description: Comprehensive layer-by-layer architectural review, code refactoring, and test verification skill for Gymora backend modules.
---

# Gymora Backend Review & Refactoring Protocol

When assigned a controller name (e.g., `OwnerSubscriptionController`), execute the review sequentially across **7 Mandatory Phases**. You MUST not skip any phase, and your final output MUST include an explicit checklist showing the status of each layer.

---

## Phase 1: File Discovery & Path Mapping
Locate and list the exact paths before making changes:
- **API**: `Api/Controllers/{Name}Controller.cs`
- **Application**: 
  - `Application/Interface/Service/I{Name}Service.cs`
  - `Application/Service/{Name}Service.cs`
  - `Application/DTO/Model/{Name}DTO.cs` (and nested/related DTOs)
  - `Application/Validators/{Name}Validator.cs` (or inline FluentValidation)
  - `Application/DTO/MapperConfig.cs`
- **Domain**: 
  - `Domain/Model/{Name}.cs`
- **Infrastructure**: 
  - `Application/Interface/Repo/I{Name}Repo.cs`
  - `Infrastructure/Repo/{Name}Repo.cs`
  - `Infrastructure/Configurations/{Name}Config.cs`
- **Events & Cache**: 
  - `Application/EventConsumer/` (related invalidation consumers)
- **Tests**: 
  - `UnitTests/Controllers/{Name}ControllerTests.cs`
  - `UnitTests/Services/{Name}ServiceTests.cs`
- **Postman**: 
  - `generate_collection.py` & `Postman/` files

---

## Phase 2: Domain & Persistence Verification
Inspect and fix:
1. **Inheritance & Tenancy**: Validate `BaseGymAuditableEntity` / `BaseGymEntity` / `BaseAuditableEntity`. Enforce `IBaseGymEntity` (`GymId`) if gym-scoped.
2. **Attributes**: Validate `[Searchable]` on full-text search fields and `[Filterable(FilterType)]` on query filters.
3. **EF Core Configurations**:
   - Check Primary/Foreign keys, table names, and explicit delete behaviors (`DeleteBehavior.Restrict` on cycles).
   - Verify composite indexes exist only if supported by real filtering patterns.
4. **Shared Interfaces**:
   - `IOnlyMeCanSee` -> enforce `CreatedById == CurrentUserId` (bypassable only by SuperAdmin).
   - `IOnlyMeCanSeeAtGym` -> enforce `CreatedByPersonId`.
   - `ICacheableEntity` -> verify cache key scoping (`GymId`, `UserId`, or `PersonId`).

---

## Phase 3: DTOs & Mapping Rules
1. **Request DTOs**: Check DataAnnotations (`[Required]`, `[MaxLength]`, `[Range]`, non-negative IDs, enum validation).
2. **Response DTOs (`{Name}RDTO`)**:
   - **Navigation Properties**: If the domain model has navigation properties, the response DTO **MUST** expose them as nullable sub-DTOs (e.g., `CouponRDTO? Coupon`).
3. **AutoMapper (`MapperConfig.cs`)**: Verify bi-directional and explicit navigation property mappings.

---

## Phase 4: Service & Repository Layer Rules
1. **Repository Split**:
   - Repo MUST override `Includes()` and `GetByIdDetailsAsync`:
   - 

   ```C#
      protected override Func<IQueryable<EntityName>, IQueryable<EntityName>>? Includes()
       => query => query.Include(x => x.Nav1).Include(x => x.Nav2);

   public override async Task<EntityName?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
       => await base.GetByIdAsync(id, false, cancellationToken, Includes());

   ```

2. **Service Retrieval Pathways**:
    
    - `GetByIdAsync`: Light-weight, minimal fields, no tracking, no includes (for validation/internal checks).
        
    - `GetByIdDetailsAsync`: Full read with eager loading (strictly read-only, no `trackChanges`).
        
3. **Transactions & Plan Checks**:
    
    - Propagate `CancellationToken` to every async call.
        
    - Use UnitOfWork transactions for multi-entity write operations.
        
    - Inject `CurrentPlanService` for subscription/limit checks instead of ad-hoc logic.
        

## Phase 5: Controller & Authorization Review

1. **Method Signatures**:
    
    - `GetPagedAsync`: `Task<ActionResult<Result<PaginatedRes<{Name}RDTO>>>>`
        
    - `GetByIdAsync` / Mutations: `Task<ActionResult<Result<{Name}RDTO>>>`
        
2. **GetById Action**: Must call `_service.GetByIdDetailsAsync(id, cancellationToken)`.
    
3. **Authorization**: Verify `[Authorize]`, `[GymAuthorize]`, or role-specific attributes (`GymRoleString.Owner`, `AppRole.SuperAdmin`).
    

## Phase 6: Tests & Postman Sync

1. **Unit Tests**:
    
    - Controller unit tests for `GetById` MUST mock `service.GetByIdDetailsAsync(...)`.
        
    - Run tests: `dotnet test` (ensure 0 failures).
        
2. **Postman**:
    
    - Map all `[Searchable]` and `[Filterable]` fields inside `FILTER_MAP` in `generate_collection.py`.
        
    - Execute: `python generate_collection.py`.
        

## Phase 7: Verification Output Matrix (MANDATORY)

You MUST structure your final response using this exact schema:

Markdown

```
### 📋 Review Summary: [Target Controller]

| Layer / Component | File Checked | Status | Issues Fixed / Details |
| :--- | :--- | :--- | :--- |
| **Domain Model** | `Domain/Model/...` | [PASS/MODIFIED] | ... |
| **EF Configuration** | `Infrastructure/Configurations/...` | [PASS/MODIFIED] | ... |
| **Repository** | `Infrastructure/Repo/...` | [PASS/MODIFIED] | ... |
| **DTOs & Mapping** | `Application/DTO/...` | [PASS/MODIFIED] | ... |
| **Service Layer** | `Application/Service/...` | [PASS/MODIFIED] | ... |
| **Controller** | `Api/Controllers/...` | [PASS/MODIFIED] | ... |
| **Unit Tests** | `UnitTests/...` | [PASS/MODIFIED] | ... |
| **Postman Sync** | `generate_collection.py` | [PASS/MODIFIED] | ... |

### 🛠️ Detailed Code Modifications
- **[File Name]**: Explanation of change.

### 🧪 Test & Build Execution
- `dotnet build`: [Success/Fail]
- `dotnet test`: [Pass count / Fail count]
- `python generate_collection.py`: [Success/Fail]

### 🏁 Final Verdict: [PASS | PASS WITH WARNINGS | FAIL]
```