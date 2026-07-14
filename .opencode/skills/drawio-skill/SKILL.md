---
name: drawio-skill
description: Generate professional draw.io diagrams from repo-mapper output. Supports flowchart, architecture, sequence, system context, and ER diagrams. Always consumes repo-mapper Project Map first — never guesses architecture. Produces editable .drawio source files with consistent styling.
license: MIT
---

# Draw.io Skill

Generate accurate, clean draw.io diagrams grounded in real repository structure.

**Prerequisite:** `repo-mapper` must run first to produce a Project Map. This skill only accepts the Project Map as input — never invents components or flows.

---

## Pipeline (Mandatory)

```
repo-mapper output (Project Map)
  → Step 1: Read Project Map
  → Step 2: Choose diagram type
  → Step 3: Select template
  → Step 4: Populate with real names
  → Step 5: Style consistently
  → Step 6: Self-check
  → Step 7: Return .drawio + note
```

---

## Step 1 — Read Project Map

Extract from the Project Map:
- `purpose` — product name / goal
- `stack` — every technology with versions
- `modules[]` — each module's `name`, `role`, `boundary`, `key_files`, `entities`
- `flows[]` — each flow's `actor`, `steps`, `data_stores`
- `terms{}` — domain vocabulary (code→English)

---

## Step 2 — Choose Diagram Type

| If the Project Map shows…                          | Use template      |
|----------------------------------------------------|-------------------|
| A single business process with decisions           | `flowchart`       |
| Multiple modules across layers (UI → API → DB)     | `architecture`    |
| A request flowing through components over time     | `sequence`        |
| External actors around a system boundary           | `context`         |
| Entities with relationships (FK references)        | `er`              |

Rules:
- If multiple aspects are relevant, generate **one diagram per type** on separate pages.
- Never merge two diagram types into one canvas.
- Prefer `context` for high-level overview, `architecture` for detail.

---

## Step 3 — Select Template

Copy the matching `.drawio` from `templates/`. Each template provides:
- Correct XML structure (`mxfile` → `diagram` → `mxGraphModel` → `root`)
- Color palette consistent with this skill
- Pre-laid-out shapes and connectors

Do **not** write raw XML unless the template is fundamentally insufficient. If you must, reuse style strings from the templates.

---

## Step 4 — Populate with Real Names

- **Node labels:** use module `name` and `role` from the Project Map. Never translate code names to English unless the English form is also in the codebase.
- **Edge labels:** use `flow` step descriptions verbatim.
- **Actor labels:** use `actor` from flows; if that's a code name like `NguoiDung`, keep it.
- **Entity labels:** use the `term` mapping from the Project Map (code name + English in parentheses if helpful).

---

## Step 5 — Style Consistently

Apply these rules automatically:

| Element               | Fill        | Stroke      | Text        |
|-----------------------|-------------|-------------|-------------|
| UI / Presentation     | `#3B82F6`   | `#2563EB`   | `#FFFFFF`   |
| API / Controller      | `#8B5CF6`   | `#7C3AED`   | `#FFFFFF`   |
| Domain / Service      | `#F59E0B`   | `#D97706`   | `#FFFFFF`   |
| Data / Persistence    | `#10B981`   | `#059669`   | `#FFFFFF`   |
| Infrastructure        | `#64748B`   | `#475569`   | `#FFFFFF`   |
| External / Integration| `#EF4444`   | `#DC2626`   | `#FFFFFF`   |
| Actor (stick figure)  | —           | `#334155`   | `#334155`   |
| Group / Boundary      | `#F8FAFC`   | `#CBD5E1`   | `#475569`   |
| Swimlane header       | `#1E293B`   | `#0F172A`   | `#FFFFFF`   |

Connector defaults:
- Solid arrow `endArrow=blockThin;endFill=1;strokeWidth=2;edgeStyle=orthogonalEdgeStyle;rounded=1`
- Dashed / async: same but `dashed=1`

---

## Step 6 — Self-Check

- [ ] **No invented nodes** — every label maps to a real module/entity/flow from the Project Map
- [ ] **Arrow direction matches data/control flow** (UI → API → Service → DB, not reversed)
- [ ] **No overlapping** nodes or labels; minimum 40 px between shapes
- [ ] **Readable text** — all labels contrast against their fill color
- [ ] **Consistent style** — same-boundary nodes share the same fill color
- [ ] **Page fits** — all nodes within A4 portrait (827×1169) or landscape (1169×827)
- [ ] **Valid XML** — all `id`, `parent`, `source`, `target` attributes resolve
- [ ] **Template used** — or strong reason why raw XML was necessary

---

## Step 7 — Return

1. **`.drawio` source file** (always)
2. **Exported PNG** (only if requested, or when a visual preview adds significant value) — 2x DPI, transparent background
3. **Short explanation note** listing:
   - Which Project Map entries justified each node
   - Why this diagram type was chosen
   - Any assumptions or trade-offs made

---

## Reference: .drawio XML Quick Reference

```xml
<mxCell id="n2" value="Label" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#3B82F6;strokeColor=#2563EB;fontColor=#FFFFFF;" vertex="1" parent="1">
  <mxGeometry x="60" y="60" width="160" height="50" as="geometry" />
</mxCell>

<mxCell id="e3" style="endArrow=blockThin;endFill=1;strokeWidth=2;edgeStyle=orthogonalEdgeStyle;rounded=1;" edge="1" parent="1" source="n2" target="n4">
  <mxGeometry relative="1" as="geometry" />
</mxCell>

<mxCell id="s1" value="Swimlane Title" style="swimlane;whiteSpace=wrap;html=1;fillColor=#1E293B;strokeColor=#0F172A;fontColor=#FFFFFF;startSize=32;horizontal=1;" vertex="1" parent="1">
  <mxGeometry x="40" y="40" width="740" height="300" as="geometry" />
</mxCell>
```

Templates at `templates/` contain full working examples for every type. Copy and modify — don't write from scratch.
