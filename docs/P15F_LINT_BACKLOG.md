# P15F Lint Backlog

> Date: 2026-07-08
> Scope: Release hardening triage only. No repo-wide lint refactor performed.

## Summary

| Item | Result |
|---|---:|
| `npm run lint` exit code | `1` |
| Oxlint | PASS, 0 warnings, 0 errors |
| ESLint files with findings | 121 |
| ESLint errors | 224 |
| Blocks `npm run build` | No |
| Raw log | `docs/artifacts/p15f-smoke/lint-p15f3.txt` |

## Main Backlog Groups

| File / Area | Error Type | Quick Fix | Risk | Blocks Build |
|---|---|---|---|---|
| `frontend/src/components/content-council/**/*.vue` | ESLint parser errors on TypeScript syntax inside Vue SFCs, e.g. `Unexpected token :`, `Unexpected token as`, `any is not defined`. | Configure ESLint parser for Vue + TypeScript SFCs or convert affected `<script setup lang="ts">` lint handling to the repo standard. | Medium: lint is noisy and hides real issues in content-council code. | No |
| `frontend/src/pages/content-council/**/*.vue` | Same TypeScript parser errors in page-level content-council SFCs. | Same parser/config fix; then rerun lint and fix remaining true unused vars. | Medium: content-council lint remains unreliable until parser is fixed. | No |
| `frontend/src/components/content-council/preview/*.vue` | Unused imports/props after parser reaches JS sections. | Remove unused imports or use props directly where intended. | Low: simple cleanup after parser config. | No |
| `frontend/src/components/content-council/subjects/*.vue` | `no-unused-vars` for imports/props. | Remove unused refs/computed/props or reference them intentionally. | Low. | No |
| `frontend/src/components/Schedule/MonthView.vue` | `isSameDay` unused. | Remove unused import/function. | Low. | No |
| `frontend/src/components/learning/LessonVideoPlayer.vue` | `pauseOnBlur` unused. | Remove unused variable or wire it to visibility behavior. | Low. | No |
| `frontend/src/components/reward-discipline/*.vue` | Unused imports/props. | Remove unused imports/props. | Low. | No |
| Misc Vue files listed in raw log | `no-unused-vars`, `no-undef`, `vue/no-v-text-v-html-on-component`. | Fix locally per file after parser config is corrected. | Low to Medium depending on component. | No |

## P15F Touched Files

P15F release hardening did not mass-fix unrelated lint. The P15F-touched files still build successfully. Any remaining lint work should be handled as a separate cleanup PR/task after ESLint understands Vue TypeScript SFC syntax.
