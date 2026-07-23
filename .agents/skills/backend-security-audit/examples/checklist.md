# ASP.NET Core Security Review Checklist

For each category: what to search for, what good/bad looks like, and typical fix.

## 1. Injection (SQL / NoSQL / Command)
**Grep for:** `FromSqlRaw(`, `ExecuteSqlRaw(`, string concatenation into SQL (`"SELECT * FROM " + userInput`), `SqlCommand` with string-built queries, `Process.Start(` with user input.
- **Bad:** `context.Users.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'")`
- **Good:** `context.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}")` or parameterized `SqlParameter`, or plain LINQ (`context.Users.Where(u => u.Name == name)`)
- EF Core LINQ queries are generally safe by default — flag only raw SQL / string interpolation into SQL.

## 2. Broken Authentication
**Grep for:** custom password hashing (anything not using `ASP.NET Core Identity`, `PasswordHasher<T>`, or a vetted library like BCrypt.Net), JWT validation config.
- Check JWT setup: `TokenValidationParameters` — is `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` all `true`? Is the signing key hardcoded in source or config committed to git?
- Check for missing `[Authorize]` on controllers/actions that should require auth — look for endpoints that touch user data with `[AllowAnonymous]` or no attribute at all.
- Password/credential comparison: must be constant-time. Flag `password == storedPassword` string comparisons.

## 3. Broken Access Control (IDOR)
**Grep for:** actions that take an `id` parameter and fetch by id without checking it belongs to the current user, e.g. `GetOrder(int id) => _db.Orders.Find(id)` with no ownership check against `User.Identity`.
- **Good pattern:** `_db.Orders.Where(o => o.Id == id && o.UserId == currentUserId)`
- Check role-based checks use `[Authorize(Roles = "Admin")]` correctly, not just checking a client-supplied role claim without validating the token.

## 4. Security Misconfiguration
- `appsettings.json`: any connection strings, API keys, or secrets committed in plaintext? Should be in `appsettings.Development.json` (gitignored), User Secrets, Azure Key Vault, or environment variables.
- `Program.cs`: is `app.UseHttpsRedirection()` present? Is `app.UseHsts()` present for production?
- CORS: check `AddCors` policy — flag `.AllowAnyOrigin().AllowCredentials()` together (invalid/dangerous combo), or overly broad `AllowAnyOrigin()` for an authenticated API.
- Exception handling: is `app.UseDeveloperExceptionPage()` active outside `Development` environment? This leaks stack traces.
- Check `launchSettings.json` and Docker/CI configs for secrets.

## 5. Sensitive Data Exposure
- Are passwords/PII logged? Grep `_logger.Log*` calls near password/token/SSN variables.
- Are API responses over-serializing (e.g., returning the full `User` entity including `PasswordHash` field) instead of DTOs?
- Is HTTPS enforced everywhere (`RequireHttps` on cookies, HSTS)?

## 6. XML External Entities (XXE) — if any XML parsing
**Grep for:** `XmlDocument`, `XDocument.Load`, `XmlReader` — ensure `DtdProcessing.Prohibit` or `XmlResolver = null` is set.

## 7. Insecure Deserialization
**Grep for:** `BinaryFormatter`, `JavaScriptSerializer`, or `JsonConvert.DeserializeObject<object>` / non-typed deserialization of user-controlled input. `BinaryFormatter` is obsolete and dangerous — flag any use as Critical.

## 8. Using Components with Known Vulnerabilities
- Result of `dotnet list package --vulnerable` from Step 2 goes here directly — list each vulnerable package, version, and the CVE if shown.

## 9. Insufficient Logging & Monitoring
- Is there any logging of auth failures, access-control failures? (`ILogger` calls around `[Authorize]` failures, failed logins)
- Is Application Insights / Serilog / another structured logger configured at all?

## 10. Rate Limiting / DoS
- Check for `Microsoft.AspNetCore.RateLimiting` middleware or any throttling on auth endpoints (`/login`, `/register`, password reset). Missing rate limiting on login = brute-force risk (flag as Medium/High depending on sensitivity).

## 11. CSRF (if using cookie auth)
- If auth is cookie-based (not pure JWT bearer in header), check for antiforgery tokens (`[ValidateAntiForgeryToken]`, `services.AddAntiforgery()`). Pure stateless JWT-in-header APIs are generally not CSRF-vulnerable — note this instead of flagging it needlessly.

## 12. File Upload Handling (if applicable)
- Check file type validation (not just extension — check content/magic bytes), file size limits, and that uploaded files aren't stored in a web-servable path with execute permissions.