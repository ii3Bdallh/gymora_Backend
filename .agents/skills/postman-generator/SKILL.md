---
name: postman-generator
description: |
  Use this skill whenever the user asks to generate, update, improve, or maintain a Postman Collection, Postman Environment, API documentation, endpoint descriptions, request examples, response examples, Postman tests, negative test cases, authentication configuration, multipart/form-data requests, integration test suites, multi-tenant/ownership security tests, cache verification tests, end-to-end user story flows, or any API testing assets for an ASP.NET Core Web API project. Trigger this even if the user says "flow", "integration test", "security test", "IDOR test", "cross-tenant test", or "Postman Flow" — those are all generated as a runnable Postman Collection (via Collection Runner chaining), not the visual Postman Flows canvas, because Postman Flows cannot currently be exported/imported reliably.

  This skill analyzes ASP.NET Core Controllers, Services, DTOs, Domain Models, Pagination DTOs, and custom attributes/base classes to automatically generate a production-ready Postman Collection v2.1 with Development, Staging, and Production environments, PLUS a companion Integration & Security Testing collection that verifies CRUD lifecycles, multi-tenant isolation, ownership isolation, caching, SuperAdmin overrides, and full business-flow user stories.
---

# Postman Generator (Collection + Integration/Security Testing)

## 1. Overview & Goal

Generate a complete production-ready Postman Collection directly from the ASP.NET Core source code, with no manual editing required, plus (when relevant — see Part B) a second collection that acts as an executable integration/security test suite.

The skill must infer everything from the codebase instead of guessing. Never invent endpoints, DTOs, or entities.

---

# PART A — Core API Collection

## 2. Prerequisites & Input Context

Analyze the following project folders, in this order:

```text
Api/Controllers
Application/Service
Application/DTO
Application/DTO/Model
Application/DTO/Pagintion
Domain/Model
```

## 3. Rules & Requirements

### General
- Never guess endpoints, invent DTOs, or invent entities. Infer everything from source code.
- Prefer XML comments whenever available; fall back to Service names if missing.
- Produce a Postman Collection v2.1 with Development, Staging, and Production environments.
- Always store entity IDs dynamically per scope using `last<EntityName>Id` (e.g., `lastUserId`, `lastProductId`).
- Use `pm.collectionVariables` or `pm.variables` so chaining flows seamlessly across folder executions without manual intervention.

### Controller Analysis
For every controller, detect: Controller Name, Route, HTTP Method, Request DTO, Response DTO, Route Parameters, Query Parameters, Body Parameters, Authorization, ProducesResponseType, Consumes, Multipart Requests.

### Endpoint Description
Generate using, in priority order: (1) XML Summary, (2) Service Method, (3) Endpoint Name.

### Service Analysis
Recognize common CRUD methods: `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `RestoreAsync`, `SearchAsync`, `GetAsync`, `GetByIdAsync`, `ChangeStatusAsync`. Generate readable descriptions automatically.

### DTO Analysis
Generate: Example Request, Example Response, Required Fields, Nullable Fields, Enum Values, Validation Rules, Default Values.

### Pagination
Detect `Application/DTO/Pagintion/PaginatedSearchReq` and `.../Filters/FilterRequest`. Never leave pagination request bodies empty — populate with realistic sample filters based on the corresponding Domain Model:

1. Scan the Domain Model for `[Searchable]` and `[Filterable]` properties.
2. **Search Term Mapping:** if `[Searchable]` properties exist (e.g. `Code`, `Name`), populate `searchTerm` with a realistic value.
3. **Filter Mapping:**
   - `[Filterable(FilterType.Exact)]` → populate `filters.exactFilters` with the property name as key and a list with a realistic value (enums as numeric/string).
   - `[Filterable(FilterType.Between)]` → populate `filters.betweenFilters` with the property name as key and a `{min, max}` object.

Example (Coupon: `Code`, `Name` searchable; `DiscountType` exact; `DiscountValue`, `ValidFrom`, `ValidTo` between):

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "SUMMER50",
  "orderBy": "Id",
  "orderDirection": "asc",
  "isActive": true,
  "filters": {
    "betweenFilters": {
      "DiscountValue": { "min": "10.0", "max": "50.0" },
      "ValidFrom": { "min": "2026-08-12T19:04:05Z", "max": "2026-09-12T19:04:05Z" }
    },
    "exactFilters": { "DiscountType": ["1"] }
  }
}
```

### Entity Mapping
Map every controller to its corresponding Domain Model using Generic Services, DTO Names, AutoMapper, or Repository Types. Never inspect unrelated entities.

### Search & Filtering Detection
1. `[Searchable]` → included in full-text search; drives `searchTerm`.
2. `[Filterable(FilterType.Exact)]` → `filters.exactFilters`.
3. `[Filterable(FilterType.Between)]` → `filters.betweenFilters`.

### Sorting
Generate default `OrderBy`, preferring: `CreatedAt`, `CreatedOn`, `Name`, `Code`, `Id`.

### Authentication
Detect `[Authorize]` → configure `Bearer {{token}}`. Ignore `[AllowAnonymous]`.

### Domain Model & Filterable Attributes Documentation
For every endpoint (especially Search/Pagination), scan the corresponding Domain Model and build an **Attributes Reference Documentation** block inside the Postman endpoint description, listing every property (Type, Nullability, Validation) and highlighting which are `[Filterable]`/`[Searchable]` and how (exact vs. between). Use this format:

```markdown
### Endpoint Overview
Fetches a paginated list of entities with dynamic sorting and filtering.

### Available Domain Attributes & Filter Reference
| Attribute Name | Data Type    | Filter Support     | Description / Allowed Values   |
| :------------- | :----------- | :----------------- | :----------------------------- |
| `Id`           | `Guid / int` | Exact               | Unique Identifier              |
| `Name`         | `string`     | Searchable / Exact | Entity Name                    |
| `Status`       | `Enum / int` | Exact               | `1` = Active, `2` = Inactive   |
| `MaxOwnedGyms` | `int`        | Between             | Range filter via `min` / `max` |
| `CreatedAt`    | `DateTime`   | Between             | ISO Date range filter          |

### Filter Usage Guide
- **Exact Filters:** `"exactFilters": { "Status": ["1", "2"] }`
- **Between Filters:** `"betweenFilters": { "MaxOwnedGyms": { "min": "1", "max": "10" } }`
```

### File Upload
Detect `IFormFile`, `List<IFormFile>`, `multipart/form-data` → generate multipart requests.

### Environment Generation
Generate three environments — Development, Staging, Production — each with placeholder-only variables:
- `baseUrl`, `token`, `refreshToken`

Never save runtime values into environments.

### Automatic Request Chaining (Dynamic Entity ID Capture)
For every `POST` (Create) or `GET` (GetById/Search/List), generate a Tests-tab script that extracts the ID from common response paths (`id`, `data.id`, `result.id`, `data.items[0].id`, `items[0].id`) and stores it as `last<EntityName>Id` via `pm.collectionVariables.set(...)`, with an `pm.variables.set("lastEntityId", ...)` fallback for immediate-scope reuse. Reuse this ID in subsequent GET/PUT/DELETE requests for the same entity: `/api/<Entities>/{{last<EntityName>Id}}`.

### Tests
For every endpoint, verify: Status Code, JSON Response shape, Required Fields, Success Flag, Pagination, Validation Errors, Execution Time.

### Negative Test Cases
Generate additional requests for: Unauthorized, Forbidden, Validation Errors, Missing Fields, Invalid Route Parameters, Invalid Query Parameters, Invalid Filters, Duplicate Resources, Invalid File Uploads.

## 4. Part A Execution Process
1. Scan Controllers → 2. Scan Services → 3. Scan DTOs → 4. Scan Pagination DTOs → 5. Map Controllers to Domain Models → 6. Generate endpoint descriptions → 7. Generate request examples → 8. Generate response examples → 9. Generate pagination requests → 10. Generate filters → 11. Generate search examples → 12. Generate environments → 13. Configure authentication → 14. Generate Postman tests → 15. Generate request chaining → 16. Generate negative test cases → 17. Organize collection folders → 18. Produce final Collection v2.1.

---

# PART B — Integration, Security & Flow Testing Suite

Trigger this part whenever the user asks for integration tests, a "flow", security/isolation tests, IDOR/cross-tenant tests, caching verification, or end-to-end user story tests. **Important:** Postman's visual Flows canvas currently has no reliable export/import (Postman's own team has confirmed flow files reference workspace-local resource IDs and break on export/import). So instead of generating a `.postman_flow` file, this skill generates a **second, separate, fully executable Postman Collection** that achieves the same outcome using `postman.setNextRequest()` chaining in the Tests tab, runnable end-to-end via the Collection Runner or Newman with zero manual steps.

## 5. Detecting the Isolation Pattern (project-agnostic)

Before generating, scan the Domain Models and their base repository classes/interfaces for the isolation pattern actually used in *this* project — do not assume fixed class names. Look for:
- A **tenant-scoping base class/repository** that automatically filters queries by a tenant column (e.g. `GymId`, `WorkspaceId`, `TenantId`). In Gymora this is `BaseGymRepo<T>` filtering on `GymId` for entities inheriting `BaseGymEntity`. In another project (e.g. ClassHub) the equivalent may be a workspace/RBAC service such as `AccessControlService` filtering by `WorkspaceId`/`WorkspaceRole` — detect and use whatever the actual project implements.
- A **creator/ownership-scoping base class/interface** that filters by the creating user (e.g. Gymora's `BaseAuditableRepo<T>` + `IOnlyMeCanSee` filtering by `CreatedById`).
- Whether these filters are bypassed for a privileged role (e.g. `SuperAdmin`).

Use the concrete Gymora class names below as the worked example, but substitute the project's real class/interface names when generating for a different codebase.

## 6. Mandatory Auth & Tenant Setup

Every generated integration collection must be self-contained — it logs itself in, it doesn't assume you already have tokens:

1. **Login Initiation:** Every CRUD sequence, security test, or user story must begin with a `Login` request (`POST /api/Auth/Login`) using credentials read from environment variables (e.g. `testUserA_email`/`testUserA_password`), storing the returned token into a **collection variable** (`tokenA`), never into the environment.
2. **Switch Tenant Context:** For tenant-owned entities, immediately follow Login with a `Switch Gym`/`Switch Workspace` request (whatever the project's real endpoint is) using the target tenant ID, before any CRUD/business calls.
3. **Multi-user Security Flows:** Log in sequentially as User A and User B (and SuperAdmin where relevant), capturing `tokenA`, `tokenB`, `superAdminToken` as separate collection variables, and switch each to their respective tenant.

Add these to the environment (placeholders only, never real credentials):
- `testUserA_email`, `testUserA_password`, `testUserB_email`, `testUserB_password`, `superAdmin_email`, `superAdmin_password`

## 7. Collection Structure

### Folder 1: `01 - CRUD Verification`
Per entity: Create (assert `201`/`200`, extract `last<EntityName>Id`) → Read by ID (assert `200`, validate fields) → Update (assert `200`) → Delete (assert `200`) → Read again (assert `404`, confirming soft delete).

### Folder 2: `02 - Security & Isolation Boundaries`

**Test A — Tenant Boundary (different tenant):**
1. POST with `tokenA` (Tenant A) → save `lastGymResourceId`.
2. GET `/api/<Entities>/{{lastGymResourceId}}` with `tokenB` (Tenant B, different tenant entirely).
3. Assert `404 Not Found` (tenant filter excludes the row) — note in the test description *why* it's 404 and not 403, since that's a deliberate repository-level behavior, not a bug.

**Test B — Ownership Boundary (same tenant, different user):**
1. POST with `tokenA` → save `lastPrivateResourceId`.
2. GET/PUT/DELETE `/api/<Entities>/{{lastPrivateResourceId}}` with `tokenB`, where User B belongs to the *same* tenant as User A but did not create the resource.
3. Assert `404 Not Found` or `403 Forbidden`.

Keep Test A and Test B as separate cases even for entities that combine both interfaces (tenant + ownership) — a same-tenant/different-user failure and a different-tenant failure have different root causes and should be diagnosable independently.

**Test C — Privileged Override (must succeed, not just "not fail"):**
1. GET `/api/<Entities>/{{lastGymResourceId}}` with `superAdminToken`.
2. Assert `200 OK` with the correct resource body. This positive check matters as much as the two negative ones — without it you can't tell whether isolation is "working correctly" or just "blocking everyone including admins."

### Folder 3: `03 - Caching Verification` (only if `ICacheableEntity`/equivalent caching marker is detected)
1. GET (cache miss, note response time) → 2. Same GET (cache hit, notably faster or has a cache-hit indicator) → 3. PUT/DELETE to mutate the resource → 4. GET again (must be a fresh cache miss, reflecting the mutation — this is the step that actually catches cache-invalidation bugs).

### Folder 4: `04 - Creative User Stories (End-to-End Flows)`
Construct realistic multi-step business journeys from the actual controllers available, e.g.:
- **Member Subscription & Approval:** Create member → query active plans → subscribe → submit payment → admin approves payment → poll/GET subscription → assert status `Active`.
- **Capacity Limit:** Create a class session with capacity 1 → register Member A (assert success) → register Member B (assert `400` with a capacity error).

Only generate stories the underlying endpoints actually support — never invent an endpoint to complete a story.

## 8. Chaining & Branching Implementation

Use `postman.setNextRequest()` in the **Tests** tab to jump between requests without duplicating them:

```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.collectionVariables.set("lastEntityId", jsonData.data.id);
    postman.setNextRequest("Read Entity by ID");
} else {
    postman.setNextRequest("Cleanup Request");
}
```

Rules:
- Never hardcode routes, bodies, or tokens inside a script if the equivalent request already exists in the collection — reference it, don't reimplement it.
- Parameterize everything through `{{baseUrl}}`, `{{tokenA}}` / `{{tokenB}}` / `{{superAdminToken}}`, and `{{last<EntityName>Id}}`.

## 9. Part B Execution Process
1. Detect the project's real isolation pattern (Section 5) → 2. Generate Login + Switch Tenant setup requests → 3. Generate `01 - CRUD Verification` per entity → 4. Generate `02 - Security & Isolation Boundaries` (Tests A/B/C) per tenant-owned or ownership-owned entity → 5. Generate `03 - Caching Verification` where applicable → 6. Generate `04 - Creative User Stories` from real endpoint combinations → 7. Wire chaining via `setNextRequest` → 8. Add the new test-user environment variables → 9. Produce the second Collection v2.1, clearly named e.g. `<ProjectName> - Integration & Security Tests`, separate from the Part A collection.

---

# 10. Combined Output

For a full request, the skill produces:
- **Collection A (API Collection):** all endpoints, environments (Dev/Staging/Prod), endpoint + controller + folder descriptions, request/response examples, auth config, pagination/search/filter/sorting examples, multipart requests, automatic ID chaining, tests, negative test cases.
- **Collection B (Integration & Security Tests):** CRUD Verification, Security & Isolation Boundaries (tenant + ownership + privileged-override), Caching Verification (if applicable), Creative User Stories — fully runnable via Collection Runner/Newman with no manual setup.

No manual editing should be required after generation, other than pasting real test-user credentials into the environment once.