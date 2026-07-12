# FE Deep Audit Validation Results

**Date**: 2026-07-11 16:57:08  
**Scope**: Cross-file validation of all audit artifacts  
**Validator**: validate-audit.mjs

---

## Summary

| Metric | Count |
|--------|-------|
| Total checks | 26 |
| Pass | 26 |
| Fail | 0 |
| Warning | 0 |
| Pass rate | 100.0% |

**Verdict**: PASS

---

## Detailed Results

| # | Type | Check |
|---|------|-------|
| 1 | ✅ PASS | route-inventory.csv: 28 columns (expected 28) |
| 2 | ✅ PASS | route-inventory.csv: header matches expected |
| 3 | ✅ PASS | route-inventory.csv: 181 routes matches router (181) |
| 4 | ✅ PASS | route-inventory.csv: all 181 statuses valid |
| 5 | ✅ PASS | route-inventory.csv: no deprecated COMPLETE status |
| 6 | ✅ PASS | route-inventory.csv: all named routes unique |
| 7 | ✅ PASS | route-inventory.csv: all routes have Path |
| 8 | ✅ PASS | component-inventory.csv: 19 columns (expected 19) |
| 9 | ✅ PASS | component-inventory.csv: no wildcard file paths |
| 10 | ✅ PASS | component-inventory.csv: all USED values boolean |
| 11 | ✅ PASS | component-inventory.csv: all DeadOrUsed values valid |
| 12 | ✅ PASS | component-inventory.csv: USED/ImportedByCount consistent |
| 13 | ✅ PASS | component-inventory.csv: no DEAD components with imports |
| 14 | ✅ PASS | component-inventory.csv: all ImportedByCount are numeric |
| 15 | ✅ PASS | component-inventory.csv: 426 total components |
| 16 | ✅ PASS | ui-violations.csv: 11 columns (expected 11) |
| 17 | ✅ PASS | ui-violations.csv: all 32 VerificationMethod values valid |
| 18 | ✅ PASS | ui-violations.csv: all Severity values valid |
| 19 | ✅ PASS | ui-violations.csv: all 4 v-html entries classified as P0 |
| 20 | ✅ PASS | ui-violations.csv: File column properly separated from Line |
| 21 | ✅ PASS | ui-violations.csv: 32 total violations |
| 22 | ✅ PASS | Bidirectional check: all 145 router paths covered in CSV |
| 23 | ✅ PASS | Master report: exact route count 181 found |
| 24 | ✅ PASS | Master report: exact component count 426 found |
| 25 | ✅ PASS | Master report: exact violation count 32 found |
| 26 | ✅ PASS | Master report: exact API_CONNECTED count 124 found |

---

## Checks Per File

### route-inventory.csv
- Columns: 27 (181 routes)
- Statuses: STATIC_FUNCTIONAL: 13, PLACEHOLDER: 1, API_CONNECTED: 124, WRONG_CONTEXT: 2, UNVERIFIED: 33, HIDE_FROM_DEMO: 8

### component-inventory.csv
- Components: 426

### ui-violations.csv
- Violations: 32

### Master Report: All sections found

### Verdict: PASS
