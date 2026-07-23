---
name: backend-security-audit
description: Use this skill whenever the user asks to review, audit, scan, or check the security of a .NET / ASP.NET Core backend, or asks for a penetration-test-style report, vulnerability report, or security checklist for their API/backend project. Also trigger when the user mentions "security review", "pentest", "vulnerability scan", "OWASP", "harden my backend", or references a .NET/C# solution and wants to know if it's secure. This skill performs a DEFENSIVE, read-only audit (static code review + dependency scanning) and produces a Markdown report with findings, severity, and fixes. It does NOT write or execute exploit code, attack live systems, or attempt to break authentication — it only inspects code, config, and dependencies.
---

# Backend Security Audit (.NET / ASP.NET Core)

## Purpose & Boundaries

This skill turns Claude into a **defensive security reviewer** for a .NET backend. It reads the codebase, configuration, and dependency manifests, and produces a Markdown report of findings mapped to OWASP-style categories with concrete fixes.

**Hard boundaries (never cross these):**
- Never write, generate, or execute exploit code, payloads, or scripts designed to actually compromise a running system (no live SQLi payloads, no auth-bypass scripts, no port-scanning/attack tooling against a running target).
- Never attempt to connect to or attack a live/remote instance of the app. This is a **static, read-only, source-code + config review**, not live penetration testing.
- If the user pushes toward "actually try to break into it" / "write an exploit" / "attack the running server", explain that this skill only does static defensive review, and offer to keep going with that instead.
- It's fine and expected to run standard, non-destructive scanning tools (dependency vulnerability scanners, static analyzers) — those are inspection tools, not attack tools.

## Workflow

### Step 0: Collect business logic rules (if provided)

If the user gives you business rules/invariants (e.g. "a normal user can't apply more than 10% discount", "an order can't move to Shipped before it's Paid", "a user can only see their own invoices", "withdrawal amount can't exceed wallet balance"), capture them as an explicit list before starting the review. If the user hasn't given any, ask once: "في أي business rules محتاج أتأكد إنها مطبّقة صح في الكود؟" and proceed with whatever they give (even a partial list is useful) — don't block the rest of the audit waiting for this.

These rules get their own findings category (Step 3b below) in addition to the standard OWASP-style categories.

**Boundary reminder:** this step is about understanding rules so you can check the *code* enforces them everywhere they apply — it is not about crafting requests to send against a live server to see if the rule breaks in practice. Stay in static/source review, never live-test against a running instance.

### Step 1: Locate and understand the project
- Find the `.sln` / `.csproj` files to identify the project structure (Web API, MVC, Minimal API, Blazor Server, etc.)
- Identify the ASP.NET Core version and target framework (`net6.0`, `net8.0`, etc.)
- Identify the ORM/data layer (Entity Framework Core, Dapper, raw ADO.NET) — this changes which injection risks are relevant
- Identify the auth mechanism (ASP.NET Identity, JWT bearer, cookie auth, custom middleware, external IdP like Azure AD/Auth0)

### Step 2: Run non-destructive automated checks
These are inspection tools only — safe to run without asking:

```bash
# Outdated / vulnerable NuGet packages
dotnet list package --vulnerable --include-transitive

# Outdated packages generally
dotnet list package --outdated

# If available, static analysis
dotnet build /p:AnalysisLevel=latest /warnaserror-
```

If `dotnet` isn't available in the environment, note that in the report and rely on manual code review instead.

### Step 3: Manual code & config review

Go through `references/checklist.md` systematically. It covers, per OWASP-style category, exactly what to grep for and what "good" vs "bad" looks like in ASP.NET Core code. Read that file before doing the review — don't rely on memory for the specific patterns.

Key files to always check:
- `appsettings.json` / `appsettings.*.json` — look for secrets, connection strings, disabled security settings committed in plaintext
- `Program.cs` / `Startup.cs` — middleware order, CORS policy, HTTPS redirection, auth configuration
- Any `Controllers/` or `Endpoints/` — authorization attributes (`[Authorize]`), input validation, raw SQL usage
- `.csproj` files — package versions
- `Dockerfile` / `docker-compose.yml` if present — running as root, exposed ports, secrets baked into image

### Step 3b: Business logic rule verification (if rules were provided in Step 0)

For each rule the user gave you, trace it through the code statically:

1. **Find every entry point** that touches the relevant data/action — not just the "obvious" one. A discount rule might be enforced in the checkout controller but forgotten in an admin bulk-edit endpoint, a background job, a webhook handler, or an internal API called by another service. Grep broadly for the relevant model/field name across the whole solution, not just the folder you expect.
2. **Check where the rule is enforced**: is it in the domain model / service layer (good — enforced regardless of caller), or only in one controller's request validation (risky — easy to bypass via another path)?
3. **Check the validation is server-side and authoritative**: if the only enforcement is a client-side check (JS validation, a DTO attribute that can be omitted) or a comment saying "assume caller validated this", flag it.
4. **Check ordering/state-machine rules** (e.g. "order can't skip from Draft to Shipped"): look for an explicit state-transition guard (e.g. a switch/enum check) vs. code that just sets the new status without checking the current one.
5. **Note where you couldn't fully verify** a rule (e.g. logic split across services you don't have access to) — say so explicitly rather than assuming it's fine.

Write each rule's result as its own finding (see report template's "Business Logic" section), even if the verdict is "✅ enforced consistently everywhere I found it touched."

### Step 4: Write the findings into the report format

Use `references/report_template.md` as the structure. Every finding needs:
- **Severity**: Critical / High / Medium / Low / Info
- **Location**: file + line/area
- **Issue**: what's wrong, in plain terms
- **Risk**: what an attacker could realistically do with it
- **Fix**: concrete code-level or config-level fix (a real snippet, not just "validate input")

Order findings by severity, Critical first. If nothing is found in a category, say so explicitly ("✅ No issues found") rather than omitting the category — this shows the review was thorough, not skipped.

### Step 5: Deliver in chat as Markdown

Output the full report directly in the conversation as Markdown (the user wants it inline, not as a downloadable file). Keep it scannable: headers, tables for the findings summary, code blocks for fixes.

Close with a short prioritized action list: "If you only fix 3 things this week, fix these."

## Reference files
- `references/checklist.md` — the detailed OWASP-mapped review checklist with exact things to grep for in ASP.NET Core code (read this before Step 3)
- `references/report_template.md` — the exact Markdown structure to fill in for Step 4