---
name: gymora-clean-architecture
description: Comprehensive guide for Gymora's Clean Architecture structure, base classes, DTOs, custom search/filter attributes, repositories, services, and implementing link/joining tables.
---

# Gymora Clean Architecture & Shared Services Guideline

This guide provides guidelines, rules, and examples for developing new features, models, DTOs, repositories, services, and joining (link) tables in Gymora following Clean Architecture rules and the established patterns of base classes.

---

## 1. Domain Layer Base Entities

All entities in `Domain/Model/` must inherit from the base classes located in [Domain/Model/Base/](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/Base):
- **`BaseEntity`**: Simple ID, contains `int Id` (PK) and `bool IsActive = true` (soft delete).
- **`BaseAuditableEntity`**: Extends `BaseEntity` with audit logs: `CreatedOn` and `CreatedById`.
- **`BaseGymEntity`**: Has `int GymId` and navigation property to `Gym`.
- **`BaseGymAuditableEntity`**: Extends `BaseGymEntity` with `CreatedOn`, `CreatedById`, and audit properties.
- **`BaseFileEntity`** / **`BaseAuditableFileEntity`**: Used for platform-wide entities containing stored file paths on Bunny Storage.
- **`BaseGymFileEntity`** / **`BaseGymAuditableFileEntity`**: Used for gym-owned entities containing stored file paths (implements `IBaseFileEntity` and `IBaseGymEntity`).

### Search and Filter Attributes
Decorate entity properties in the domain model to enable automatic search and filtering:
- **`[Searchable]`**: Marks properties (e.g. Name, Description) to be matched against `SearchTerm` in paged queries automatically.
- **`[Filterable(FilterType)]`**: Marks properties that support exact list checks or numeric/date range queries:
  - `FilterType.Exact`: Exact matching, IDs, or Enums (`ExactFilters` list in DTO).
  - `FilterType.Between`: Date and numeric ranges (`BetweenFilters` Min/Max in DTO).

---

## 2. Application Layer DTOs

Every entity `EntityName` has corresponding DTO records in `Application/DTO/Model/`. When using base entities, you **must use the matching base DTOs** from [Application/DTO/Base/](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Base):
- If the entity inherits from `BaseEntity`:
  - **`EntityNameCDTO` (Create DTO)**: Inherits from `BaseCDTO`.
  - **`EntityNameUDTO` (Update DTO)**: Inherits from `BaseUDTO`.
  - **`EntityNameRDTO` (Response DTO)**: Inherits from `BaseRDTO`.
- If the entity inherits from `BaseAuditableEntity` or `BaseGymAuditableEntity`:
  - **`EntityNameCDTO` (Create DTO)**: Inherits from `BaseAuditableCDTO`.
  - **`EntityNameUDTO` (Update DTO)**: Inherits from `BaseAuditableUDTO`.
  - **`EntityNameRDTO` (Response DTO)**: Inherits from `BaseAuditableRDTO`.
- If the entity inherits from `BaseGymEntity` or `BaseAuditableGymEntity`:
  - **`EntityNameCDTO` (Create DTO)**: Inherits from `BaseGymCDTO` (or `BaseGymAuditableCDTO`).
  - **`EntityNameUDTO` (Update DTO)**: Inherits from `BaseGymUDTO` (or `BaseGymAuditableUDTO`).
  - **`EntityNameRDTO` (Response DTO)**: Inherits from `BaseGymRDTO` (or `BaseGymAuditableRDTO`).
- If the entity inherits from `BaseGymFileEntity` or `BaseGymAuditableFileEntity`:
  - **`EntityNameCDTO` (Create DTO)**: Inherits from `BaseGymFCDTO` (or `BaseGymAuditableFCDTO`).
  - **`EntityNameUDTO` (Update DTO)**: Inherits from `BaseGymFUDTO` (or `BaseGymAuditableFUDTO`).
  - **`EntityNameRDTO` (Response DTO)**: Inherits from `BaseGymFRDTO` (or `BaseGymAuditableFRDTO`).

*Rule*: Never return raw Domain Model entities from endpoints or in DTOs. Map them to their corresponding `RDTO`s.

### 2.1. Shared User Context & Pagination Models
For querying, filtering, and user authentication context:
- **`CurrentUser`** (from [CurrentUser.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Model/CurrentUser.cs)): Injected into services to retrieve currently authenticated user credentials (`UserId`, `CurrentGymId`, `PlatformRole`, `IsSuperAdmin`, etc.).
- **`PaginatedSearchReq`** (from [PaginatedSearchReq.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Pagintion/PaginatedSearchReq.cs)): Base request class representing parameters for searching, filtering, and page index parameters.
- **`PaginatedRes<T>`** (from [PaginatedRes.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Pagintion/PaginatedRes.cs)): Generic container wrapper returned from paged list queries.

---

## 3. EF Core Entity Configuration Extensions

When configuring EF Core mappings inside `Infrastructure/Configurations/`, use the extension methods from [ConfigurationExtensions.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Extensions/ConfigurationExtensions.cs) to automatically map base class relationships and indexes:
- **`builder.ConfigureAuditing()`**: Sets up relationship to `ApplicationUser` for `CreatedById` and adds indices for auditable entities.
- **`builder.ConfigureFileAuditing()`**: Sets up auditing for file entities.
- **`builder.ConfigureGymAuditing()`**: Configures auditing relations and properties targeting gym entities.
- **`builder.ConfigureGymOwned()`**: Standardizes the mapping between gym-owned entities (`BaseGymEntity`) and `GymId`, applying cascade delete behavior.

---

## 4. Repositories and Services

### Repository Pattern
- Interfaces go in `Application/Interface/Repo/` (e.g. `IEntityNameRepo : IBaseRepo<EntityName>`).
- Implementations go in `Infrastructure/Repo/` (e.g. `EntityNameRepo : BaseRepo<EntityName>`).
- Use `BaseAuditableRepo<T>` to automatically restrict queries to only the current user's resources unless they are `SuperAdmin`.

### Service Layer
- Interfaces go in `Application/Interface/Service/`.
- Implementations go in `Application/Service/` (inheriting from `BaseService` or `BaseReadService`).
- If the entity is gym-owned, inherit from `BaseGymService` or `BaseAuditableGymService` to automatically validate gym boundaries.
- If the entity is gym-owned and contains file uploads, inherit from `BaseGymFileService` or `BaseGymAuditableFileService` to automatically integrate file uploads (via `IStorageService`) and check gym access rights.
- Override lifecycle hooks (e.g., `BeforeAddAsync`, `AfterMapAddAsync`) rather than overriding entire CRUD operations.

---

## 5. Shared Utility Services

The workspace includes several shared utility service interfaces located in `Application/Interface/Service/Shared/`:

### `CurrentPlanService`
- Used to check the owner's active subscription status (e.g., `Active`, `Grace`, or fallback to `Free`) and limits.
- Exposes slot capability checks:
  - `HasAvailableGymSlotAsync`: Checks if `CurrentGymCount < MaxOwnedGyms`.
  - `HasAvailableMemberSlotAsync`: Checks if `CurrentMemberCount < MaxMembers`.
  - `HasAvailableCoachSlotAsync`: Checks if `CurrentCoachCount < MaxCoaches`.
- Returns `CurrentPlanResult` where `Subscription` is mapped to `OwnerSubscriptionRDTO` to keep layer separation clean.

### `GymAccessRepo`
- Used to validate and switch user context between gyms.
- Exposes `GetGymAccessAsync` to check if a user has access to a specific gym (handles Owner and Staff/Member checks, verifying that the gym is active, the user's access status is active, and the owner has a valid, compliant subscription).
- Exposes `CanJoinGymAsync` to verify if a gym has enough capacity (slots) to allow a new member or coach to join, based on the owner's plan limit.

### `IBackgroundJobService`
- Abstracts Hangfire background job scheduler: `EnqueueAsync`, `ScheduleAsync`, and `RecurringAsync`.

### `ICacheService`
- Generic cache abstraction for application caching: `GetAsync<T>`, `SetAsync<T>`, and `RemoveAsync`.

### `IEmailService`
- Asynchronous email delivery service: `SendEmailAsync(toEmail, subject, body)`.

### `INotificationService`
- Coordinates pushes and notifications: `SendNotificationAsync`, `SendNotificationListAsync`, and `SendToTopicAsync`.

### `IStorageService`
- Manages Bunny Storage uploads/deletions: `UploadFileToStorageAsync`, `GetFileAccessUrl`, `DeleteFileFromStorageAsync`, and `DeleteCollectionFromStorageAsync`.

---

## 6. Implementing a Joining Table / Link Feature

This section provides a comprehensive pattern for creating a new feature in the Gymora backend that acts as a joining (link) table between two or more entities, using **`CoachAssignment`** as the reference implementation.

### 6.1. Domain Model Design
When linking entities, inherit from the appropriate base entity class.
*Example: [CoachAssignment.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/CoachAssignment.cs)*
```csharp
using Domain.Model.Base;
using System;

namespace Domain.Model
{
    public class CoachAssignment : BaseGymEntity
    {
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;

        public int CoachStaffId { get; set; }
        public GymPerson Coach { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public int AssignedById { get; set; }
        public GymPerson AssignedBy { get; set; } = null!;

        public DateTime? EndedAt { get; set; }
    }
}
```

### 6.2. EF Core Entity Configuration
Define entity mappings in `Infrastructure/Configurations/`.

> [!IMPORTANT]
> To prevent **multiple cascade paths or cycles** in SQL Server when a single table links to the same target table multiple times (e.g. `Member`, `Coach`, and `AssignedBy` all referencing `GymPerson`), configure the foreign keys with `DeleteBehavior.Restrict` instead of cascade delete.

*Example: [CoachAssignmentConfig.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Configurations/CoachAssignmentConfig.cs)*
```csharp
using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class CoachAssignmentConfiguration : IEntityTypeConfiguration<CoachAssignment>
    {
        public void Configure(EntityTypeBuilder<CoachAssignment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymOwned(); // Automatically configures Gym relation and cascade delete

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent SQL Server cascade path cycles

            builder.HasOne(x => x.Coach)
                .WithMany()
                .HasForeignKey(x => x.CoachStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedBy)
                .WithMany()
                .HasForeignKey(x => x.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite index to prevent duplicate active/historical assignments
            builder.HasIndex(x => new { x.MemberId, x.CoachStaffId })
                .IsUnique();
        }
    }
}
```

### 6.3. Data Transfer Objects (DTOs)
Define data transfer schemas in `Application/DTO/Model/`:
1. **Pagination Requests**: Inherit from `PaginatedSearchReq` to support customized filtering.
2. **Create DTO (CDTO)**: Inherit from `BaseGymCDTO` and define fields required for creation.
3. **Response DTO (RDTO)**: Inherit from `BaseGymRDTO` and include mapped RDTO properties of the linked entities.

*Example: [TraineesDTOs.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Model/TraineesDTOs.cs)*
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Pagintion;

namespace Application.DTO.Model;

public class GetAssignedMemberForCoachPagedReq : PaginatedSearchReq
{
    [Required(ErrorMessage = "Coach Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid CoachId")]
    public int CoachId { get; set; }

    [Required(ErrorMessage = "Gym Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
    public int GymId { get; set; }
}

public class GetAssignCoachForMemberPagedReq : PaginatedSearchReq
{
    [Required(ErrorMessage = "Member Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Gym Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
    public int GymId { get; set; }
}

public record CoachAssignmentRDTO : BaseGymRDTO
{
    public int MemberId { get; set; }
    public GymPersonRDTO? Member { get; set; }

    public int CoachStaffId { get; set; }
    public GymPersonRDTO? CoachStaff { get; set; }

    public int AssignedById { get; set; }
    public GymPersonRDTO? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public record CoachAssignmentCDTO : BaseGymCDTO
{
    [Required(ErrorMessage = "Member Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Coach Staff Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid CoachStaffId")]
    public int CoachStaffId { get; set; }
}

public record CoachAssignmentUDTO : BaseGymUDTO { }
```

Don't forget to register mapping configurations in `Application/DTO/MapperConfig.cs`:
```csharp
CreateMap<CoachAssignmentCDTO, CoachAssignment>();
CreateMap<CoachAssignmentUDTO, CoachAssignment>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
CreateMap<CoachAssignment, CoachAssignmentRDTO>();
```

### 6.4. Repository Layer
Implement query overrides in the repository to eager load (`Include`) related entities and handle custom request filters.

*Example: [CoachAssignmentRepo.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Repo/CoachAssignmentRepo.cs)*
```csharp
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class CoachAssignmentRepo(ApplicationDbContext context, ILogger<CoachAssignmentRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<CoachAssignment>(context, logger, queryCache, currentUser), ICoachAssignmentRepo
    {
        protected override Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? Includes()
        {
            return query => query.Include(x => x.Coach).Include(x => x.Member).Include(x => x.AssignedBy);
        }

        public override IQueryable<CoachAssignment> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            var query = base.GetAllQuery(searchReq, trackChanges, cancellationToken, include);

            if (searchReq is GetAssignedMemberForCoachPagedReq coachReq)
            {
                query = query.Where(x => x.CoachStaffId == coachReq.CoachId);
            }
            else if (searchReq is GetAssignCoachForMemberPagedReq memberReq)
            {
                query = query.Where(x => x.MemberId == memberReq.MemberId);
            }

            return query;
        }

        public override async Task<PaginatedRes<CoachAssignment>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            include ??= Includes();
            return await base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override async Task<CoachAssignment?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }
    }
}
```

### 6.5. Service Layer
Implement validation rules in service lifecycle hooks.

*Example: [CoachAssignmentService.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Service/CoachAssignmentService.cs)*
```csharp
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class CoachAssignmentService : BaseGymService<CoachAssignment, CoachAssignmentRDTO, CoachAssignmentCDTO, CoachAssignmentUDTO>, ICoachAssignmentService
{
    private readonly IGymPersonRepo _gymPersonRepo;

    public CoachAssignmentService(
        ICoachAssignmentRepo repo,
        IGymPersonRepo gymPersonRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        IPublishEndpoint publishEndpoint,
        CurrentUser currentUser,
        ILogger<CoachAssignmentService> logger)
        : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
    {
        _gymPersonRepo = gymPersonRepo;
    }

    protected override async Task BeforeAddAsync(CoachAssignmentCDTO dto, CancellationToken cancellationToken)
    {
        await base.BeforeAddAsync(dto, cancellationToken);

        GymPerson? member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, cancellationToken);
        if (member is null)
            throw new NotFoundException($"Member with ID {dto.MemberId} was not found.");

        GymPerson? coach = await _gymPersonRepo.GetByIdAsync(dto.CoachStaffId, false, cancellationToken);
        if (coach is null || coach.PersonType == PersonType.Member)
            throw new NotFoundException($"Coach with ID {dto.CoachStaffId} was not found.");
    }
}
```

### 6.6. Controller Layer
Expose endpoints mapping HTTP requests to service calls.

*Example: [CoachAssignmentController.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Api/Controllers/CoachAssignmentController.cs)*
```csharp
using System.Threading;
using System.Threading.Tasks;
using Api.Filters;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoachAssignmentController(
    ICoachAssignmentService coachAssignmentService,
    ILogger<CoachAssignmentController> logger)
    : ControllerBase
{
    [HttpPost("get-assigned-members-for-coach")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetAssignedMembers([FromBody] GetAssignedMemberForCoachPagedReq req)
    {
        logger.LogInformation("Fetching all assigned members for coach");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned members for coach");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-assigned-coaches-for-member")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetAssignedCoaches([FromBody] GetAssignCoachForMemberPagedReq req)
    {
        logger.LogInformation("Fetching all assigned coaches for member");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned coaches for member");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-gym-coach-assignments")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetGymCoachAssignments([FromBody] PaginatedSearchReq searchReq)
    {
        logger.LogInformation("Fetching all assigned coaches for member");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(searchReq, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned coaches for member");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("coach-assignments")]
    public async Task<IActionResult> AssignCoach([FromBody] CoachAssignmentCDTO dto, CancellationToken ct)
    {
        var result = await coachAssignmentService.AddAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, Result<CoachAssignmentRDTO>.Success(result));
    }
}
```
