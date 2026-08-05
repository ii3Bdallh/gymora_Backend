---
name: gymora-clean-architecture
description: Guidelines and instructions for working with Gymora's clean architecture base entities, DTOs, repositories, services, and shared subscription/access validation logic.
---

# Gymora Clean Architecture & Shared Services Guideline

This skill provides context and instructions for developing new features, models, DTOs, and services in Gymora following Clean Architecture rules and the established patterns of base classes.

## 1. Domain Layer Base Entities

All entities in `Domain/Model/` must inherit from the base classes located in [Domain/Model/Base/](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/Base):
- **`BaseEntity`**: Simple ID, contains `Id` (PK) and `IsActive` (soft delete).
- **`BaseAuditableEntity`**: Extends `BaseEntity` with audit logs: `CreatedOn` and `CreatedById`.
- **`BaseGymEntity`**: Has `GymId` and navigation property to `Gym`.
- **`BaseGymAuditableEntity`**: Extends `BaseGymEntity` with `CreatedOn`, `CreatedById`, and audit properties.
- **`BaseFileEntity`** / **`BaseAuditableFileEntity`**: Used for entities containing stored file paths on Bunny Storage.

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

*Rule*: Never return raw Domain Model entities from endpoints or in DTOs. Map them to their corresponding `RDTO`s.

---

## 2.1. Shared User Context & Pagination Models

For querying, filtering, and user authentication context:
- **`CurrentUser`** (from [CurrentUser.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Model/CurrentUser.cs)): Injected into services to retrieve currently authenticated user credentials (`UserId`, `CurrentGymId`, `PlatformRole`, `IsSuperAdmin`, etc.).
- **`PaginatedSearchReq`** (from [PaginatedSearchReq.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Pagintion/PaginatedSearchReq.cs)): Base request class representing parameters for searching, filtering, and page index parameters.
- **`PaginatedRes<T>`** (from [PaginatedRes.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Pagintion/PaginatedRes.cs)): Generic container wrapper returned from paged list queries.

---

## 2.2. EF Core Entity Configuration Extensions

When configuring EF Core mappings inside `Infrastructure/Configurations/`, use the extension methods from [ConfigurationExtensions.cs](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Extensions/ConfigurationExtensions.cs) to automatically map base class relationships and indexes:
- **`builder.ConfigureAuditing()`**: Sets up relationship to `ApplicationUser` for `CreatedById` and adds indices for auditable entities.
- **`builder.ConfigureFileAuditing()`**: Sets up auditing for file entities.
- **`builder.ConfigureGymAuditing()`**: Configures auditing relations and properties targeting gym entities.
- **`builder.ConfigureGymOwned()`**: Standardizes the mapping between gym-owned entities (`BaseGymEntity`) and `GymId`, applying cascade delete behavior.

---

## 3. Repositories and Services

### Repository Pattern
- Interfaces go in `Application/Interface/Repo/` (e.g. `IEntityNameRepo : IBaseRepo<EntityName>`).
- Implementations go in `Infrastructure/Repo/` (e.g. `EntityNameRepo : BaseRepo<EntityName>`).
- Use `BaseAuditableRepo<T>` to automatically restrict queries to only the current user's resources unless they are `SuperAdmin`.

### Service Layer
- Interfaces go in `Application/Interface/Service/`.
- Implementations go in `Application/Service/` (inheriting from `BaseService` or `BaseReadService`).
- Override lifecycle hooks (e.g., `BeforeAddAsync`, `AfterMapAddAsync`) rather than overriding entire CRUD operations.

---

## 4. Shared Services for Gym Access & Subscriptions

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

---

## 5. Other Shared Services

The workspace includes several shared utility service interfaces located in `Application/Interface/Service/Shared/`:

### `IBackgroundJobService`
- **Purpose**: Abstracts Hangfire background job scheduler.
- **Methods**:
  - `EnqueueAsync(jobName, job)`: Queue fire-and-forget task.
  - `ScheduleAsync(jobName, job, delay)`: Schedule a task delayed by a `TimeSpan`.
  - `RecurringAsync(jobName, job, cronExpression)`: Schedule a recurring task using Cron expression.

### `IBunnyCollectionService`
- **Purpose**: Manages video collections on Bunny Stream.
- **Methods**:
  - `GetCollectionAsync(collectionId)`: Fetch collection details.
  - `CreateCollectionAsync(dto)`: Create a new collection.
  - `UpdateCollectionAsync(collectionId, dto)`: Update metadata.
  - `DeleteCollectionAsync(collectionId)`: Delete collection.

### `IBunnyStreamService`
- **Purpose**: Integrates video streaming upload and playback via Bunny Stream.
- **Methods**:
  - `GenerateUrlToUploadVideoToBunnyStream(title, collectionIdentifier)`: Request secure upload URL and video GUID.
  - `GenerateUrlToAccessFileAsync(videoGuid)`: Returns direct link to playback.
  - `GetVideoDetails(VideoId, LibraryId)`: Query stream processing status.
  - `DeleteVideoFromBunnyStream(videoGuid)`: Delete a single stream.
  - `DeleteVideosByCollectionIdAsync(collectionId)`: Bulk deletes all collection videos.

### `ICacheService`
- **Purpose**: Generic cache abstraction for application caching.
- **Methods**:
  - `GetAsync<T>(key)`: Get cached object.
  - `SetAsync<T>(key, value, absoluteExpiration)`: Set cache value with expiration.
  - `RemoveAsync(key)`: Invalidate cache key.

### `IEmailService`
- **Purpose**: Asynchronous email delivery service.
- **Methods**:
  - `SendEmailAsync(toEmail, subject, body)`: Send an email message.

### `INotificationService`
- **Purpose**: Coordinates pushes and notifications.
- **Methods**:
  - `SendNotificationAsync(userId, notification)`: Sends notification to a single user's device.
  - `SendNotificationListAsync(userIds, notification)`: Multicasts to multiple user IDs.
  - `SendToTopicAsync(topic, notification)`: Broadcasts to all users subscribed to a specific topic.

### `IStorageService`
- **Purpose**: Manages file storage (public or signed private files) on Bunny Storage.
- **Methods**:
  - `UploadFileToStorageAsync(file, isPublic, entityType)`: Uploads a file (e.g. logo, Receipt) and returns the StoredFilePath.
  - `GetFileAccessUrl(storedFileName, isPublic)`: Resolves file URL (Signed URL for private files).
  - `DeleteFileFromStorageAsync(storedFileName)`: Deletes file.
  - `DeleteCollectionFromStorageAsync(collectionPath)`: Deletes folder from storage.

