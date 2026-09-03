# FOUNDATION_DEBT_FRONTEND_LINT

## Baseline recorded for Task 8.0

- ESLint baseline: 340 errors.
- Scope: existing frontend-wide debt; it is not introduced by Task 7C.
- Evidence retained: frontend build passed, `test:unit` passed (36/36), and Oxlint reported zero errors.
- Decision: do not run `eslint --fix` and do not change frontend files during Task 7C closure.
- Owner phase: defer remediation to Phase 8.8.

This record removes the existing ESLint baseline from the Task 7C commit gate only while the Task 7C diff contains no frontend source file.
