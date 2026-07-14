# Architecture Diagrams

This directory stores architecture diagrams for the LMS Academic Management System.

## Purpose

- Document system architecture, module relationships, deployment topology, and key design decisions.
- Diagrams are generated as `.drawio` (editable) source files via `drawio-skill`.
- Exported PNG images may be included for quick viewing in markdown documents.

## Naming Convention

```
docs/architecture/
├── <module-or-layer>-<version>.drawio   # editable source
├── <module-or-layer>-<version>.png      # exported view (optional)
└── README.md                            # this file
```

Examples:
- `system-context-v1.drawio`
- `backend-architecture-v1.drawio`
- `scheduling-workflow-v1.drawio`

## Process

1. Run `repo-mapper` to produce a Project Map of the current codebase.
2. Run `drawio-skill` with that Project Map to generate or update a diagram.
3. Save the `.drawio` file here.
4. If needed for docs, export PNG and commit both.
