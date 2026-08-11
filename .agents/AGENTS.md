# Gymora Backend Architecture Guidelines

This document describes the Clean Architecture design patterns, conventions, and rules followed in this codebase. Always adhere to these patterns when creating new entities, DTOs, repositories, services, or controllers.

---

## 1. Clean Architecture Layers

- **Domain**: Contains business entities, enums, custom attributes (`[Searchable]`, `[Filterable]`), and domain events. Dependent on nothing.
- **Application**: Contains the business logic contracts, services, DTOs, AutoMapper profiles, caching interfaces, and exceptions. Depends only on **Domain**.
- **Infrastructure**: Contains Ef Core DbContext (`ApplicationDbContext`), database migrations, repository implementations, cache services, Hangfire background tasks, and seed data. Depends on **Application** and **Domain**.
- **Api**: The entry point. Contains controllers, middlewares, startup configuration, and API-specific configurations. Depends on all layers.

---

## 2. Domain Models & Base Entities

All domain models must reside in `Domain/Model/` and inherit from one of the base entities in [Domain/Model/Base/](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/Base/):

1. **`BaseEntity`**: For entities with simple IDs and soft delete capabilities.
   - Has `int Id` (Primary Key) and `bool IsActive = true`.
2. **`BaseAuditableEntity`**: Inherits `BaseEntity` and implements `IAuditableEntity`.
   - Has `DateTime CreatedOn`, `int CreatedById`, and `DateTime? ModifiedOn`.
3. **`BaseGymEntity`**: Inherits `BaseEntity` and implements `IBaseGymEntity`.
   - Has `int GymId`.
4. **`BaseGymAuditableEntity`**: Inherits `BaseGymEntity` and implements `IAuditableEntity`.
   - Has `DateTime CreatedOn`, `int CreatedById`, `DateTime? ModifiedOn`, and `int GymId`.
5. **`BaseFileEntity`** / **`BaseAuditableFileEntity`**: For platform-wide entities that store file paths or uploads.
6. **`BaseGymFileEntity`** / **`BaseGymAuditableFileEntity`**: For gym-owned entities that store file paths or uploads (implements `IBaseFileEntity` and `IBaseGymEntity`).

### Search and Filter Attributes
When defining properties in domain models, use:
- `[Searchable]` to make the field searchable using full-text search terms.
- `[Filterable(FilterType)]` (e.g., `FilterType.Exact`, `FilterType.Between`) to allow clients to filter the query on this field.

---

## 3. Data Transfer Objects (DTOs) & Mapping

Every entity `EntityName` should have three corresponding records in `Application/DTO/Model/EntityNameDTO.cs`:
1. **`EntityNameCDTO` (Create DTO)**: Inherits from `BaseCDTO` (or `BaseAuditableCDTO`). Defines fields required for creation.
2. **`EntityNameUDTO` (Update DTO)**: Inherits from `BaseUDTO` (or `BaseAuditableUDTO`). Defines fields allowed to be updated.
3. **`EntityNameRDTO` (Response DTO)**: Inherits from `BaseRDTO` (or `BaseAuditableRDTO`). Defines fields returned to clients (including `Id` and `IsActive`).

### AutoMapper Profile
Register mapping configurations in [Application/DTO/MapperConfig.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/MapperConfig.cs):
```csharp
CreateMap<EntityNameCDTO, EntityName>();
CreateMap<EntityNameUDTO, EntityName>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
CreateMap<EntityName, EntityNameRDTO>();
```

---

## 4. Repository Pattern

Repositories manage queries and persist data to EF Core.
- **Interfaces**: Define in `Application/Interface/Repo/` (e.g., `IEntityNameRepo : IBaseRepo<EntityName>`).
- **Implementations**: Implement in `Infrastructure/Repo/` inheriting from the base repositories:
  - `BaseRepo<T>`: Implements core CRUD, soft delete, search/filtering via `GetAllQuery` and `GetPageAsync`.
  - `BaseAuditableRepo<T>`: Extends `BaseRepo<T>` to automatically restrict data access based on the current user (`CreatedById == CurrentUserId`) if the user is not a `SuperAdmin`.

---

## 5. Service Layer

Business logic and orchestration happens in services.
- **Interfaces**: Define in `Application/Interface/Service/`.
  - Read-only services: `IEntityNameService : IBaseReadService<EntityName, EntityNameRDTO>`
  - Read/Write services: `IEntityNameService : IBaseService<EntityName, EntityNameRDTO, EntityNameCDTO, EntityNameUDTO>`
- **Implementations**: Implement in `Application/Service/` inheriting from base classes:
  - `BaseReadService<T, RDTO>`: Implements read queries, automated caching for `ICacheableEntity`, and authorization validation.
  - `BaseService<T, RDTO, CDTO, UDTO>`: Implements write operations (`AddAsync`, `UpdateAsync`, `DeleteAsync`), saving changes to database via Unit of Work, and publishes state change messages using MassTransit.

### Customizing Service Lifecycle Hooks
Do not override whole CRUD methods unless absolutely necessary. Instead, override the lifecycle hooks:
- `BeforeAddAsync(dto, cancellationToken)`
- `AfterMapAddAsync(entity, dto, cancellationToken)`
- `BeforeUpdateAsync(entity, dto, cancellationToken)`
- `AfterMapUpdateAsync(entity, dto, cancellationToken)`
- `AfterUpdateAsync(entity, cancellationToken)`
- `BeforeDeleteAsync(entity, cancellationToken)`
- `AfterDeleteAsync(entity, cancellationToken)`

---

## 6. Controller Layer

Controllers should be clean and thin, delegating work to services:
- Reside in `Api/Controllers/`.
- Use the primary constructor to inject `IEntityNameService` and `ILogger`.
- Use `[Authorize(Roles = ...)]` to secure endpoints.
- Wrap all responses using `Result<T>` (e.g., `return Ok(Result<EntityNameRDTO>.Success(data));` or `Result<PaginatedRes<EntityNameRDTO>>`).
