# Gymora — Architecture & New Feature Guide

## 1. Solution Structure

```
gymora_Backend/
├── Api/                            # Presentation Layer
│   ├── Controllers/                # Auth, SubscriptionPlan, Test
│   ├── Middlewares/                 # ExceptionHandlingMiddleware
│   ├── Extensions/                 # DI, Serilog, ValidationHelper
│   └── Program.cs
├── Application/                    # Use-Case Layer
│   ├── DTO/                        # Auth/, CRUD/, Base/, Pagintion/, Errors/, Attribute/, Webhook/
│   │   ├── Result.cs               # Result<T> envelope
│   │   └── MapperConfig.cs         # AutoMapper profiles
│   ├── Interface/Service/          # IAuthService, IBaseService<T,R,C,U>, shared interfaces
│   ├── Interface/Repo/             # IBaseRepo<T>, IUnitOfWork, entity repos
│   ├── Service/base/               # BaseService<T,R,C,U> (abstract CRUD impl)
│   ├── Service/shared/             # Email, Notification, Bunny services
│   └── DependencyInjection/        # Service DI registrations
├── Domain/                         # Entity Layer (no deps)
│   ├── Model/Base/                 # BaseEntity (Id, IsActive), AuditableEntity
│   ├── Model/Auth/                 # ApplicationUser, ApplicationRole, RefreshToken
│   ├── Enum/                       # RoleType, FileType, BunnyUploadStatus
│   ├── Attributes/                 # Searchable, Filterable
│   └── Options/                    # Jwt, Bunny, Mail, Redis, etc.
├── Infrastructure/                 # Persistence & External Services
│   ├── Persistence/                # ApplicationDbContext, DesignTimeDbContextFactory
│   ├── Configurations/             # EF Core Fluent config per entity
│   ├── Repo/Base/                  # BaseRepo<T>, BaseOwnershipRepo<T>, UnitOfWork
│   ├── Repo/Entity/                # Feature-specific repo impls
│   ├── Extensions/                 # FilterExtension, SearchExtension
│   ├── Cache/QueryCache            # Attribute-based search/filter caching
│   ├── Seed/                       # IdentitySeeder
│   ├── Hangfire/                   # RecurringJobs, TokenCleanupJob
│   └── DependencyInjection/        # DbContext, Identity, JWT, MassTransit config
├── Shared/
├── IntegrationTests/
└── UnitTests/
```

## 2. Layers & Dependency Rule

```
Api → Application → Domain
  ↘    ↗
  Infrastructure (DbContext, Repos, Configs)
```

| Layer | Responsibility | Depends On |
|-------|---------------|------------|
| **Api** | HTTP, routing, middleware, Swagger | Application |
| **Application** | Business logic, DTOs, AutoMapper, service interfaces | Domain |
| **Domain** | Entities, enums, attributes, options | None |
| **Infrastructure** | EF Core, repos, Identity, JWT, MassTransit, external services | Application, Domain |

### Base Classes

```csharp
public abstract class BaseEntity {
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;   // soft-delete
}
public abstract class AuditableEntity : BaseEntity {
    public DateTime CreatedOn { get; set; }
    public int CreatedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
```

### DTO Hierarchy
```
BaseCDTO (empty)        BaseRDTO (Id)           BaseUDTO (empty)
  └─ BaseAuditableCDTO    └─ BaseAuditableRDTO    └─ BaseAuditableUDTO
```

### Response Envelope
```json
// Success                    // Error
{ "isSuccess": true,          { "isSuccess": false,
  "data": { ... } }             "error": { "code": "ERR", "message": "..." } }
```

## 3. Request Lifecycle

```
CORS → ExceptionHandling → StaticFiles → Routing → HTTPS → Auth (JWT) → Authorization → RateLimiter
                                                                                              │
                                                                                              ▼
Controller → validate ModelState → call Service → Repo (memory) → _unitOfWork.SaveChangesAsync()
                                                                                              │
On exception: ExceptionHandlingMiddleware maps: BadRequest→400, Unauthorized→401, Forbidden→403, NotFound→404, Exception→500
```

## 4. New Feature — 14 Steps

### Domain (2 files)

**Step 1** — `Domain/Model/{Name}.cs`
```csharp
public class Product : BaseEntity {
    [Searchable] public string Title { get; set; }
    public string? Description { get; set; }
    [Filterable(FilterType.Between)] public decimal Price { get; set; }
    [Filterable(FilterType.Exact)] public int CategoryId { get; set; }
}
```
- `[Searchable]` — full-text search via `searchTerm`
- `[Filterable(Between)]` — range filter (min/max)
- `[Filterable(Exact)]` — IN-list filter

**Step 2** — `Infrastructure/Configurations/{Name}Config.cs`
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product> {
    public void Configure(EntityTypeBuilder<Product> builder) {
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.Title);
    }
}
```

### Application (8 files)

**Step 3** — `Application/DTO/CRUD/Create/{Name}CDTO.cs` (extends `BaseCDTO` or `BaseAuditableCDTO`)

**Step 4** — `Application/DTO/CRUD/Read/{Name}RDTO.cs` (extends `BaseRDTO` or `BaseAuditableRDTO`)

**Step 5** — `Application/DTO/CRUD/Update/{Name}UDTO.cs` (extends `BaseUDTO` or `BaseAuditableUDTO`)

**Step 6** — `Application/Interface/Repo/I{Name}Repo.cs`
```csharp
public interface IProductRepo : IBaseRepo<Product> { }
```

**Step 7** — `Application/Interface/Service/I{Name}Service.cs`
```csharp
public interface IProductService : IBaseService<Product, ProductRDTO, ProductCDTO, ProductUDTO> { }
```

**Step 8** — `Application/Service/{Name}Service.cs`
```csharp
public class ProductService(IProductRepo repo, IUnitOfWork uow, IMapper mapper)
    : BaseService<Product, ProductRDTO, ProductCDTO, ProductUDTO>(repo, uow, mapper), IProductService { }
```

**Step 9** — Edit `Application/DTO/MapperConfig.cs` — add after `// Script will Add After Here MapperConfig`:
```csharp
CreateMap<Product, ProductCDTO>().ReverseMap();
CreateMap<Product, ProductUDTO>().ReverseMap();
CreateMap<Product, ProductRDTO>().ReverseMap();
```

**Step 10** — Edit `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs` — add after `// Script will Add After Here DependencyInjectionService`:
```csharp
services.AddScoped<IProductService, ProductService>();
```

### Infrastructure (3 files)

**Step 11** — `Infrastructure/Repo/Entity/{Name}Repo.cs`
```csharp
public class ProductRepo(ApplicationDbContext ctx, ILogger<ProductRepo> log, QueryCache cache)
    : BaseRepo<Product>(ctx, log, cache), IProductRepo { }
```
> Use `BaseOwnershipRepo<T>` for `AuditableEntity` (auto-filters by `CreatedById`, blocks cross-user access, skips for SuperAdmin).

**Step 12** — Edit `Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs` — add after `// Script will Add After Here DependencyInjectionRepo`:
```csharp
services.AddScoped<IProductRepo, ProductRepo>();
```

**Step 13** — Edit `Infrastructure/Persistence/ApplicationDbContext.cs` — add after `// Script will Add After Here DbSet<Entity>`:
```csharp
public DbSet<Product> Product { get; set; }
```

### Api (1 file)

**Step 14** — `Api/Controllers/{Name}Controller.cs`
```csharp
[ApiController, Route("api/[controller]"), AllowAnonymous]
public class ProductController(ILogger<ProductController> log, IProductService svc) : ControllerBase {
    [HttpPost]       public async Task<IActionResult> GetPaged([FromBody] PaginatedSearchReq req)
        => Ok(Result<PaginatedRes<ProductRDTO>>.Success(await svc.GetPageAsync(req, true)));
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(Result<ProductRDTO>.Success(await svc.GetByIdAsync(id, true, ct)));
    [HttpPost("Create")] public async Task<IActionResult> Create([FromBody] ProductCDTO dto)
        => !ModelState.IsValid ? BadRequest(ModelState) : Ok(Result<ProductRDTO>.Success(await svc.AddAsync(dto)));
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, [FromBody] ProductUDTO dto)
        => !ModelState.IsValid ? BadRequest(ModelState) : Ok(Result<ProductRDTO>.Success(await svc.UpdateAsync(id, dto)));
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
        => Ok(Result<ProductRDTO>.Success(await svc.DeleteAsync(id)));
}
```

## 5. Code Generation Tool

The `AddFeature.dart` script automates all 14 steps:
- **Copy-Paste** (5 files): Controller, IRepo, IService, Service, Repo
- **Code-Generated** (5 files): CDTO, RDTO, UDTO, Model, Config
- **Add-To-File** (4 files): MapperConfig, Service DI, Repo DI, DbContext

```bash
dart AddFeature.dart Product      # Replace "Feature" → "Product" in all templates
dart AddFeature.dart               # Default: "User"
```

### Manual steps after generation:
```bash
dotnet ef migrations add Add{Name}Entity --startup-project ../Api
dotnet ef database update --startup-project ../Api
```
Then adjust controller auth (`[Authorize(Roles = "SuperAdmin")]`) and add custom business logic.

## 6. Key Patterns

| Pattern | Detail |
|---------|--------|
| **Soft Delete** | `BaseRepo.DeleteAsync` sets `IsActive = false`. Queries filter by `IsActive == true`. |
| **Ownership** | `BaseOwnershipRepo<T>` filters by `CreatedById`, blocks cross-user writes (except SuperAdmin). |
| **Unit of Work** | `SaveChangesAsync` called in **Service** layer only (not Repo). Enables transactions. |
| **AutoMapper** | 3 maps per entity: `CDTO↔Entity, UDTO↔Entity, RDTO↔Entity`. All in `MapperConfig.cs`. |
| **Validation** | DataAnnotations on DTOs. Custom file-type attributes: `PngFileOnly`, `SvgFileOnly`, `PdfFileOnly`, `Mp4FileOnly`, etc. ModelState checked in controllers. |
| **Exception → Status** | `BadRequestException`→400, `UnauthorizedException`→401, `ForbiddenException`→403, `NotFoundException`→404, unhandled→500. |
| **Pagination** | `POST` with body `{ pageNumber, pageSize, searchTerm, orderBy, orderDirection, isActive, filters }`. Max pageSize=50, max searchTerm=100. |
| **Rate Limiting** | `Ip_5Limit_1Min` (login), `Ip_3Limit_5Min` (OTP-sensitive), `Ip_10Limit_1Min` (OTP-verify). Use `[EnableRateLimiting("name")]`. |

## 7. Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| "Sequence contains no elements" | No `[Searchable]`/`[Filterable]` attributes | Add at least one |
| "No service for type I{Name}Repo" | Missing DI registration | Add to `InfrastructureServiceCollectionExtensions.cs` |
| "DbSet<{Name}> not found" | Missing DbSet in DbContext | Add after marker comment |
| "Missing map" on DTOs | Missing AutoMapper entries | Add 3 `CreateMap<>().ReverseMap()` calls |
| "Requires a primary key" | Entity doesn't extend `BaseEntity` | Extend `BaseEntity` or `AuditableEntity` |

## 8. File Checklist

| # | File | Action |
|---|------|--------|
| 1 | `Domain/Model/{Name}.cs` | Create |
| 2 | `Infrastructure/Configurations/{Name}Config.cs` | Create |
| 3 | `Application/DTO/CRUD/Create/{Name}CDTO.cs` | Create |
| 4 | `Application/DTO/CRUD/Read/{Name}RDTO.cs` | Create |
| 5 | `Application/DTO/CRUD/Update/{Name}UDTO.cs` | Create |
| 6 | `Application/Interface/Repo/I{Name}Repo.cs` | Create |
| 7 | `Application/Interface/Service/I{Name}Service.cs` | Create |
| 8 | `Application/Service/{Name}Service.cs` | Create |
| 9 | `Application/DTO/MapperConfig.cs` | Edit (add 3 maps) |
| 10 | `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs` | Edit (add service) |
| 11 | `Infrastructure/Repo/Entity/{Name}Repo.cs` | Create |
| 12 | `Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs` | Edit (add repo) |
| 13 | `Infrastructure/Persistence/ApplicationDbContext.cs` | Edit (add DbSet) |
| 14 | `Api/Controllers/{Name}Controller.cs` | Create |
| — | Migration | Run `dotnet ef migrations add` |
