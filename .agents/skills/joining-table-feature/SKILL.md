---
name: joining-table-feature
description: Guidelines and step-by-step example of implementing a new joining table feature linking multiple entities under clean architecture.
---

# Gymora Joining Table / Link Feature Guide

This guide provides a comprehensive example and pattern for creating a new feature in the Gymora backend that acts as a joining (link) table between two or more entities, using **`CoachAssignment`** as the reference implementation.

---

## 1. Domain Model Design

When linking entities (for example, linking a member and a coach under a specific Gym), the model should inherit from the appropriate base entity class:

- Inherit from `BaseGymEntity` (if gym-owned) or `BaseEntity` (if system-wide).
- Define foreign key properties and their navigation properties.

### Example: [CoachAssignment.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/CoachAssignment.cs)
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

---

## 2. EF Core Entity Configuration

Define entity mappings in `Infrastructure/Configurations/`. 

> [!IMPORTANT]
> To prevent **multiple cascade paths or cycles** in SQL Server when a single table links to the same target table multiple times (e.g. `Member`, `Coach`, and `AssignedBy` all referencing `GymPerson`), configure the foreign keys with `DeleteBehavior.Restrict` instead of cascade.

### Example: [CoachAssignmentConfig.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Configurations/CoachAssignmentConfig.cs)
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

---

## 3. Data Transfer Objects (DTOs) & Pagination Requests

Define data transfer schemas in `Application/DTO/Model/`:
1. **Pagination Requests**: Inherit from `PaginatedSearchReq` to support customized filtering (e.g. filtering assignments by MemberId or CoachId).
2. **Create DTO (CDTO)**: Inherit from `BaseGymCDTO` and define fields required for creation.
3. **Response DTO (RDTO)**: Inherit from `BaseGymRDTO` and include mapped RDTO properties of the linked entities.

### Example: [TraineesDTOs.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Model/TraineesDTOs.cs)
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Pagintion;

namespace Application.DTO.Model;

// Request to get trainees assigned to a specific coach
public class GetAssignedMemberForCoachPagedReq : PaginatedSearchReq
{
    [Required(ErrorMessage = "Coach Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid CoachId")]
    public int CoachId { get; set; }

    [Required(ErrorMessage = "Gym Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
    public int GymId { get; set; }
}

// Request to get coaches assigned to a specific member
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

---

## 4. Repository Layer

Implement query overrides in the repository to eager load (`Include`) related entities and handle custom request filters.

### Interface: [ICoachAssignmentRepo.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Interface/Repo/ICoachAssignmentRepo.cs)
```csharp
using Domain.Model;
using Application.Interface.Repo;

namespace Application.Interface.Repo;

public interface ICoachAssignmentRepo : IBaseRepo<CoachAssignment>
{
}
```

### Implementation: [CoachAssignmentRepo.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Repo/CoachAssignmentRepo.cs)
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
            // Eagerly load all navigation properties
            return query => query.Include(x => x.Coach).Include(x => x.Member).Include(x => x.AssignedBy);
        }

        public override IQueryable<CoachAssignment> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            var query = base.GetAllQuery(searchReq, trackChanges, cancellationToken, include);

            // Handle custom pagination requests to filter query by relational keys
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

---

## 5. Service Layer

Implement validation rules (such as permission boundaries, model ownership, and checking whether related entities exist and are active) in service lifecycle hooks.

### Interface: [ICoachAssignmentService.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Interface/Service/ITraineeCoachService.cs)
```csharp
using Application.DTO.Model;
using Domain.Model;

namespace Application.Interface.Service;

public interface ICoachAssignmentService : IBaseService<CoachAssignment, CoachAssignmentRDTO, CoachAssignmentCDTO, CoachAssignmentUDTO>
{
}
```

### Implementation: [CoachAssignmentService.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Service/CoachAssignmentService.cs)
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

public class CoachAssignmentService : BaseService<CoachAssignment, CoachAssignmentRDTO, CoachAssignmentCDTO, CoachAssignmentUDTO>, ICoachAssignmentService
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
        // 1. Validate Gym Scope ownership
        if (CurrentGymId != dto.GymId)
            throw new ForbiddenException("You are not authorized to perform this action.");

        // 2. Validate related entity existence and roles
        GymPerson? member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, cancellationToken);
        if (member is null)
            throw new NotFoundException($"Member with ID {dto.MemberId} was not found.");

        GymPerson? coach = await _gymPersonRepo.GetByIdAsync(dto.CoachStaffId, false, cancellationToken);
        if (coach is null || coach.PersonType == PersonType.Member)
            throw new NotFoundException($"Coach with ID {dto.CoachStaffId} was not found.");
    }
}
```

---

## 6. Controller Layer

Controllers expose endpoints mapping HTTP requests to service calls. Since base pagination searches filter by body models using `[HttpPost]`, custom pagination endpoints are structured in the same way.

### Example: [CoachAssignmentController.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Api/Controllers/CoachAssignmentController.cs)
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
