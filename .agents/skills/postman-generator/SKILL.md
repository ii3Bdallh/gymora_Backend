---
name: postman-generator
description: |
  Use this skill whenever the user asks to generate, update, improve, or maintain a Postman Collection, Postman Environment, API documentation, endpoint descriptions, request examples, response examples, Postman tests, negative test cases, authentication configuration, multipart/form-data requests, or API testing assets for an ASP.NET Core Web API project.

  This skill analyzes ASP.NET Core Controllers, Services, DTOs, Domain Models, Pagination DTOs, and custom attributes to automatically generate a production-ready Postman Collection v2.1 with Development, Staging, and Production environments.

  Supported features include:
  - Collection generation
  - Environment generation
  - Automatic endpoint documentation
  - Authentication detection
  - CRUD organization
  - Search & filtering generation
  - Pagination generation
  - File upload requests
  - Automatic request chaining
  - Postman test scripts
  - Negative test cases
  - Example requests & responses
---

# Postman Generator

## 1. Overview & Goal

Generate a complete production-ready Postman Collection directly from the ASP.NET Core source code.

The generated collection must require no manual editing.

The skill must infer API behavior from the codebase instead of guessing.

---

# 2. Prerequisites & Input Context

Analyze the following project folders.

```text
Api/Controllers
Application/Service
Application/DTO
Application/DTO/Model
Application/DTO/Pagintion
Domain/Model
```

Use them in the following order.

1. Controllers
2. Services
3. DTOs
4. Pagination DTOs
5. Domain Models

---

# 3. Rules & Requirements

The generator must follow these rules.

## General

- Never guess endpoints.
- Never invent DTOs.
- Never invent entities.
- Infer everything from source code.
- Prefer XML comments whenever available.
- Fall back to Service names if XML comments are missing.
- Produce a Postman Collection v2.1.
- Generate Development, Staging, and Production environments.
- The output must be immediately usable.
- Always store entity IDs dynamically per scope using `last<EntityName>Id` (e.g., `lastUserId`, `lastProductId`).
- Use `pm.collectionVariables` or `pm.variables` so that chaining flows seamlessly across folder executions without manual intervention.

---

## Controller Analysis

For every controller detect:

- Controller Name
- Route
- HTTP Method
- Request DTO
- Response DTO
- Route Parameters
- Query Parameters
- Body Parameters
- Authorization
- ProducesResponseType
- Consumes
- Multipart Requests

---

## Endpoint Description

Generate endpoint descriptions using:

1. XML Summary
2. Service Method
3. Endpoint Name

---

## Service Analysis

Read every Service.

Recognize common CRUD methods including:

- CreateAsync
- UpdateAsync
- DeleteAsync
- RestoreAsync
- SearchAsync
- GetAsync
- GetByIdAsync
- ChangeStatusAsync

Generate readable descriptions automatically.

---

## DTO Analysis

Generate:

- Example Request
- Example Response
- Required Fields
- Nullable Fields
- Enum Values
- Validation Rules
- Default Values

---

## Pagination

Detect:

`Application/DTO/Pagintion/PaginatedSearchReq`

Generate a clean pagination request body WITHOUT pre-filling the `filters` object with hardcoded attributes.

Example Request Body:

````json
{
  "pageNumber": 1,
  "pageSize": 10,
  "orderBy": "Id",
  "orderDirection": "asc",
  "filters": {
    "betweenFilters": {},
    "exactFilters": {}
  }
}
```
Note: Do NOT write populated filter properties directly into the request body. All available filterable attributes and domain model properties MUST be fully documented inside the Endpoint Description instead.

---

## Entity Mapping

Every controller must be mapped to its corresponding Domain Model.

Infer mapping using:

- Generic Services
- DTO Names
- AutoMapper
- Repository Types

Never inspect unrelated entities.

---

## Search

Detect:

```csharp
[Searchable]
````

Generate realistic search examples.

---

## Filtering

Detect:

```csharp
[Filterable]
```

Support:

- Exact
- Between

Generate filters automatically.

Example:

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "orderBy": "Id",
  "orderDirection": "asc",
  "filters": {
    "betweenFilters": {
      "MaxOwnedGyms": {
        "min": "1",
        "max": "10"
      }
    },
    "exactFilters": {
      "Status": ["1", "2"]
    }
  }
}
```

Enums should use enum values.

Numeric fields should use realistic ranges.

Dates should use ISO format.

---

## Sorting

Generate default OrderBy.

Prefer:

- CreatedAt
- CreatedOn
- Name
- Code
- Id

---

## Authentication

Detect:

```csharp
[Authorize]
```

Automatically configure:

Bearer {{token}}

Ignore:

```csharp
[AllowAnonymous]
```

---

## Domain Models & Filterable Attributes Documentation

For every endpoint (especially Search/Pagination endpoints), the generator MUST scan the corresponding `Domain/Model` entity and dynamically build an **Attributes Reference Documentation** inside the Postman Endpoint Description.

### Endpoint Description Requirements:

In addition to the XML summary and Service method, include a clear documentation block mapping all attributes from the Domain Model:

1. **All Domain Model Attributes:** List every property found in the `Domain/Model` along with its Type, Nullability, and Validation requirements.
2. **Filterable Attributes:** Highlight properties decorated with `[Filterable]` or `[Searchable]`, indicating whether they support `exactFilters` or `betweenFilters`.

### Example Format inside Postman Endpoint Description:

````markdown
### Endpoint Overview

Fetches a paginated list of entities with dynamic sorting and filtering.

---

### Available Domain Attributes & Filter Reference

| Attribute Name | Data Type    | Filter Support     | Description / Allowed Values   |
| :------------- | :----------- | :----------------- | :----------------------------- |
| `Id`           | `Guid / int` | Exact              | Unique Identifier              |
| `Name`         | `string`     | Searchable / Exact | Entity Name                    |
| `Status`       | `Enum / int` | Exact              | `1` = Active, `2` = Inactive   |
| `MaxOwnedGyms` | `int`        | Between            | Range filter via `min` / `max` |
| `CreatedAt`    | `DateTime`   | Between            | ISO Date range filter          |

---

### Filter Usage Guide (For Request Body)

- **Exact Filters (`exactFilters`):** Pass attribute name with array of string/enum values. Example: `"exactFilters": { "Status": ["1", "2"] }`
- **Between Filters (`betweenFilters`):** Pass attribute name with `min` and `max`. Example: `"betweenFilters": { "MaxOwnedGyms": { "min": "1", "max": "10" } }`

## File Upload

Detect:

- IFormFile
- List<IFormFile>
- multipart/form-data

Generate multipart requests.

---

## Environment Generation

Generate three environments.

Development

Variables:

- baseUrl
- token
- refreshToken

Staging

Variables:

- baseUrl
- token
- refreshToken

Production

Variables:

- baseUrl
- token
- refreshToken

Generate placeholders only.

Never save runtime values.

---

## Automatic Request Chaining (Dynamic Entity ID Capture)

Automatically extract created/fetched IDs and save them using **entity-specific post-response scripts** (Tests tab in Postman).

### Naming Convention Rule:

Dynamic variable names MUST strictly follow the pattern: `last<EntityName>Id` (e.g., `lastUserId`, `lastProductId`, `lastGymId`).

### Extraction Logic & Test Script Generation:

For every `POST` (Create) or `GET` (GetById / Search / List) endpoint, generate a Post-response Test script in Postman:

1. Identify the Entity name from Controller or Route (e.g., `UsersController` -> `User`).
2. Parse response to locate ID in common paths:
   - `id`
   - `data.id`
   - `result.id`
   - `data.items[0].id`
   - `items[0].id`
3. Store the extracted ID into a collection variable or `pm.variables`:

```javascript
// Example auto-generated script for Post/Create or Get endpoint:
if (pm.response.code === 200 || pm.response.code === 201) {
  var responseData = pm.response.json();
  var extractedId =
    responseData.id ||
    (responseData.data && responseData.data.id) ||
    (responseData.result && responseData.result.id) ||
    (responseData.data &&
      responseData.data.items &&
      responseData.data.items[0] &&
      responseData.data.items[0].id) ||
    (responseData.items && responseData.items[0] && responseData.items[0].id);

  if (extractedId) {
    pm.collectionVariables.set("last<EntityName>Id", extractedId);
    // Fallback for immediate scope execution
    pm.variables.set("lastEntityId", extractedId);
  }
}
```
````

Route & Request Re-use:
For subsequent endpoints related to the same Entity:

GET /api/<Entities>/{{{last<EntityName>Id}}}

PUT /api/<Entities>/{{{last<EntityName>Id}}}

DELETE /api/<Entities>/{{{last<EntityName>Id}}}

## Fallback route parameter syntax if specific entity ID is missing: {{lastEntityId}}.

## Tests

Generate tests for every endpoint.

Verify:

- Status Code
- JSON Response
- Required Fields
- Success Flag
- Pagination
- Validation Errors
- Execution Time

---

## Negative Test Cases

Generate additional requests for:

- Unauthorized
- Forbidden
- Validation Errors
- Missing Fields
- Invalid Route Parameters
- Invalid Query Parameters
- Invalid Filters
- Duplicate Resources
- Invalid File Uploads

---

# 4. Execution Process

Execute the following steps in order.

1. Scan Controllers.
2. Scan Services.
3. Scan DTOs.
4. Scan Pagination DTOs.
5. Map Controllers to Domain Models.
6. Generate endpoint descriptions.
7. Generate request examples.
8. Generate response examples.
9. Generate pagination requests.
10. Generate filters.
11. Generate search examples.
12. Generate environments.
13. Configure authentication.
14. Generate Postman tests.
15. Generate request chaining.
16. Generate negative test cases.
17. Organize collection folders.
18. Produce final Collection v2.1.

---

# 5. Output

The skill must generate:

- Postman Collection v2.1
- Development Environment
- Staging Environment
- Production Environment
- Endpoint Descriptions
- Controller Descriptions
- Folder Descriptions
- Request Examples
- Response Examples
- Authentication Configuration
- Pagination Requests
- Search Examples
- Filter Examples
- Sorting Examples
- Multipart Requests
- Automatic Request Chaining
- Postman Tests
- Negative Test Cases

No manual editing should be required after generation.
