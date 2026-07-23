# Backend Security Audit Report

**Project:** {name}
**Stack:** ASP.NET Core {version}, {ORM used}, {auth mechanism}
**Review type:** Static code + config + dependency review (no live attack testing)
**Date:** {date}

---

## Summary

| Severity | Count |
|---|---|
| Critical | X |
| High | X |
| Medium | X |
| Low | X |
| Info | X |

One or two sentences on overall posture — is this generally solid with a few gaps, or are there systemic issues.

---

## Findings

### 🔴 Critical

#### [C-1] {Title}
- **Location:** `path/to/file.cs:line`
- **Issue:** {plain-language description}
- **Risk:** {what an attacker could actually do}
- **Fix:**
```csharp
// concrete fix snippet
```

### 🟠 High
(same structure)

### 🟡 Medium
(same structure)

### 🔵 Low
(same structure)

### ⚪ Info / Best Practice
(same structure — things that aren't vulnerabilities but are worth improving)

---

## Business Logic Rules

For each rule the user provided:

#### [BL-1] {Rule as stated by the user}
- **Enforced at:** {file/layer(s) where it's checked}
- **Verdict:** ✅ Enforced consistently / ⚠️ Enforced in some paths but not others / ❌ Not enforced / ❓ Couldn't fully verify (why)
- **Details:** {which entry points were checked, which one(s) are missing the check if any}
- **Fix (if needed):**
```csharp
// concrete fix — move check to domain/service layer, add missing guard, etc.
```

## Categories with no issues found
✅ {Category name} — checked, no problems found.

---

## Dependency Scan Results
Output of `dotnet list package --vulnerable --include-transitive`, formatted as a table: package, current version, vulnerability/CVE, recommended version.

---

## Priority Action List
If you only fix 3 things this week:
1. {Highest severity, easiest to exploit}
2. {...}
3. {...}