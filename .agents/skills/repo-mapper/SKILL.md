---
name: repo-mapper
description: Analyze the current repository to build a concise, structured project map. Detects stack, modules, business flows, boundaries (UI/domain/data/infra), and domain terminology. Output is consumed by drawio-skill for accurate diagram generation. Always inspect real files — never invent architecture.
license: MIT
---

# Repo Mapper

Analyze the repository as the single source of truth and produce a structured **Project Map** that `drawio-skill` can consume.

---

## Workflow

### 1. Scan Repository Structure

Run from the repository root. Collect a top-3-level directory tree.

### 2. Detect Technology Stack

Read these files when they exist:

| File | What to extract |
|---|---|
| `package.json` | Framework, build tool, key dependencies |
| `composer.json` | PHP framework & packages |
| `pom.xml` / `build.gradle` | Java / Kotlin stack |
| `go.mod` | Go module & version |
| `Cargo.toml` | Rust crates |
| `Gemfile` | Ruby stack |
| `*.csproj` / `*.sln` | .NET target framework, packages |
| `requirements.txt` / `pyproject.toml` | Python stack |
| `Dockerfile` / `docker-compose.yml` | Deployment infra |
| `README.md` | Product purpose, setup instructions |
| `AGENTS.md` / `CLAUDE.md` | Agent conventions (if present) |

### 3. Identify Key Modules

Scan controller/service/route directories to build a module inventory:

- **Backend languages:** list Controllers (or routes), Services, Models/Entities
- **Frontend languages:** list Routes, Views/Pages, Components, Stores
- **Infrastructure:** list Database migrations, Config files, CI/CD

For each module note:
- Name (use the directory/namespace name from code)
- Role (auth, scheduling, finance, notifications, etc.)
- Key entities / models
- Primary controller or entry point

### 4. Map Business Flows

From the module inventory, infer:

```
1. Primary actor(s)        — who uses the system?
2. Main user journeys      — login → do work → logout
3. Data flow direction     — UI → API → Service → DB → response
4. Cross-cutting concerns  — auth, audit, notifications, logging
```

For each flow note:
- Source actor
- Trigger
- Main steps
- Data stores involved

### 5. Identify Boundaries

Classify each module into one of:

| Boundary | Description |
|---|---|
| **UI / Presentation** | Vue/React components, HTML templates, CSS |
| **API / Controller** | HTTP endpoints, route definitions |
| **Domain / Service** | Business logic, validators, workflows |
| **Data / Persistence** | DB models, repositories, migrations |
| **Infrastructure** | Auth middleware, logging, caching, config |
| **Integration** | External APIs, webhooks, email/SMS |

### 6. Extract Domain Terminology

List all domain-specific terms found in the codebase (entity names, route segments, store names, DTO names).

For a Vietnamese-context project, preserve the bilingual nature:
- Map Vietnamese names → English equivalents
- Use the **actual code names** (e.g., `MaNguoiDung`, `KhoaHoc`, `BuoiHoc`) not translations

---

## Output Format

Return a structured Project Map as **Markdown + JSON** block. Example:

```json
{
  "purpose": "LMS / Academic Management System",
  "stack": {
    "backend": {
      "language": "C#",
      "framework": "ASP.NET Core net10.0",
      "orm": "EF Core 10.0.x",
      "database": "SQL Server",
      "auth": "JWT Bearer"
    },
    "frontend": {
      "language": "JavaScript",
      "framework": "Vue 3",
      "build": "Vite",
      "state": "Pinia",
      "router": "Vue Router",
      "css": "Tailwind CSS v4"
    },
    "infra": ["Docker", "SignalR"]
  },
  "modules": [
    {
      "name": "Auth",
      "role": "Authentication & Authorization",
      "boundary": "api|domain",
      "key_files": ["Controllers/AuthController.cs", "Services/Auth/AuthService.cs"],
      "entities": ["NguoiDung", "VaiTro", "TokenLamMoi"]
    }
  ],
  "flows": [
    {
      "name": "Login",
      "actor": "NguoiDung (User)",
      "steps": [
        "POST /api/auth/login → validate credentials → issue JWT → return tokens"
      ],
      "data_stores": ["NguoiDung", "TokenLamMoi"]
    }
  ],
  "terms": {
    "NguoiDung": "User",
    "KhoaHoc": "Course",
    "ThoiKhoaBieu": "Timetable",
    "BuoiHoc": "Session"
  }
}
```

## Self-Check

- [ ] Every module name is directly from the codebase
- [ ] Every entity name is from an actual model file
- [ ] Every flow step corresponds to real controller/route
- [ ] Boundaries are correctly classified
- [ ] Stack versions match real config files
- [ ] No invented components or speculative architecture

## Notes

- This skill never generates diagrams — only text analysis.
- If the repo is ambiguous or lacks evidence, ask the user for clarification.
- Prefer breadth (all modules) over depth (one module's internals) for the first pass.
