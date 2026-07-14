# Diagrams

This directory stores general diagrams (flowcharts, sequence diagrams, ER diagrams, process maps) for the LMS Academic Management System.

## Purpose

- Document business processes, data flows, user journeys, and entity relationships.
- All diagrams are generated as `.drawio` (editable) source files via `drawio-skill`.
- Exported PNG images may be included for quick viewing.

## Naming Convention

```
docs/diagrams/
├── <topic>-<type>-<version>.drawio   # editable source
├── <topic>-<type>-<version>.png      # exported view (optional)
└── README.md                         # this file
```

Types: `flowchart`, `sequence`, `er`, `context`, `architecture`

Examples:
- `login-flow-sequence-v1.drawio`
- `course-allocation-flowchart-v1.drawio`
- `database-er-diagram-v1.drawio`

## Process

1. Run `repo-mapper` to produce a Project Map of the current codebase.
2. Run `drawio-skill` with that Project Map to generate or update a diagram.
3. Save the `.drawio` file here.
4. If needed for docs, export PNG and commit both.
