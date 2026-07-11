#!/usr/bin/env node
import { readFileSync, writeFileSync, readdirSync, statSync } from "fs";
import { join } from "path";

const ARTIFACTS = "docs/artifacts/fe-deep-audit";
const results = { pass: 0, fail: 0, warn: 0, details: [] };

function pass(msg) { results.pass++; results.details.push({ type: "PASS", msg }); }
function fail(msg) { results.fail++; results.details.push({ type: "FAIL", msg }); }
function warn(msg) { results.warn++; results.details.push({ type: "WARN", msg }); }

function parseCSVLine(line) {
  const parts = [];
  let current = "";
  let inQuotes = false;
  for (const ch of line) {
    if (ch === '"') { inQuotes = !inQuotes; }
    else if (ch === "," && !inQuotes) { parts.push(current.trim()); current = ""; }
    else { current += ch; }
  }
  parts.push(current.trim());
  return parts;
}

function loadCSV(filePath) {
  const text = readFileSync(filePath, "utf8").trim();
  const lines = text.split("\n");
  const header = parseCSVLine(lines[0]);
  const data = [];
  for (let i = 1; i < lines.length; i++) {
    if (lines[i].trim()) data.push(parseCSVLine(lines[i]));
  }
  return { header, data, raw: lines };
}


import { parse } from "@babel/parser";

function countRouterRoutes() {
  const src = readFileSync("frontend/src/router/index.js", "utf8");
  const ast = parse(src, { sourceType: "module", plugins: ["importAttributes", "jsx"] });

  function findCreateRouter(node) {
    if (node?.type === "CallExpression" && node.callee?.type === "Identifier" && node.callee.name === "createRouter") return node;
    for (const k of Object.keys(node || {})) {
      const v = node[k];
      if (v && typeof v === "object") {
        const arr = Array.isArray(v) ? v : [v];
        for (const item of arr) {
          if (item && typeof item === "object") { const f = findCreateRouter(item); if (f) return f; }
        }
      }
    }
    return null;
  }

  const routerCall = findCreateRouter(ast);
  if (!routerCall) return 0;
  
  const routesProp = routerCall.arguments?.[0]?.properties?.find(
    p => p.type === "ObjectProperty" && p.key?.type === "Identifier" && p.key.name === "routes"
  );
  if (!routesProp || routesProp.value.type !== "ArrayExpression") return 0;
  
  let count = 0;
  function countRoutes(elements) {
    for (const el of elements) {
      if (el?.type === "ObjectExpression") {
        const pathProp = el.properties.find(p => p.type === "ObjectProperty" && p.key?.name === "path");
        const redirectProp = el.properties.find(p => p.type === "ObjectProperty" && p.key?.name === "redirect");
        const pathVal = pathProp?.value?.value;
        // Skip empty-path redirect children
        if (!(pathVal === "" && redirectProp)) {
          count++;
        }
        const childrenProp = el.properties.find(p => p.type === "ObjectProperty" && p.key?.name === "children");
        if (childrenProp && childrenProp.value?.type === "ArrayExpression") {
          countRoutes(childrenProp.value.elements);
        }
      }
    }
  }
  
  countRoutes(routesProp.value.elements);
  return count;
}

function findAllFiles(dir, ext) {
  
  
  const results = [];
  try {
    const entries = readdirSync(dir, { withFileTypes: true });
    for (const entry of entries) {
      const full = join(dir, entry.name);
      if (entry.isDirectory() && !entry.name.startsWith(".") && entry.name !== "node_modules") {
        results.push(...findAllFiles(full, ext));
      } else if (entry.isFile()) {
        const match = Array.isArray(ext) ? ext.some(e => entry.name.endsWith(e)) : entry.name.endsWith(ext);
        if (match) results.push(full.replace(/\\/g, "/"));
      }
    }
  } catch (_) { }
  return results;
}

function countVueFiles() {
  const files = findAllFiles("frontend/src", ".vue");
  return files.length;
}

const VALID_STATUSES = [
  "STATIC_FUNCTIONAL", "API_CONNECTED", "PLACEHOLDER",
  "HIDE_FROM_DEMO", "RUNTIME_VERIFIED", "WRONG_CONTEXT",
  "UNVERIFIED", "BROKEN", "MOCK_DATA", "INLINE_DATA"
];

const VALID_VERIFICATION = ["STATIC_CODE", "BUILD", "UNIT_TEST", "BROWSER", "NETWORK", "NOT_VERIFIED"];

// ═══════════════════════════════════════════════════════════
//  1. Validate route-inventory.csv
// ═══════════════════════════════════════════════════════════
function validateRouteInventory() {
  const filePath = `${ARTIFACTS}/route-inventory.csv`;
  const { header, data, raw } = loadCSV(filePath);

  // Check 28 columns
  if (header.length === 28) pass(`route-inventory.csv: ${header.length} columns (expected 28)`);
  else fail(`route-inventory.csv: ${header.length} columns (expected 28)`);

  // Check header names
  const expected = "Role,Path,RecordType,Name,ParentRoute,ParentLayout,Component,MetaTitle,MetaSubtitle,MetaSection,RequiresAuth,RequiredRoles,Fullscreen,PreviewMode,MenuSource,MenuVisible,BreadcrumbSource,PageTitleSource,Store,Composable,Service,APIEndpoint,DataType,RuntimeVerified,Status,UXRisk,TechnicalRisk,Evidence";
  if (raw[0].trim() === expected) pass("route-inventory.csv: header matches expected");
  else fail("route-inventory.csv: header mismatch");

  // Dynamic route count: count path entries in router
  const routerCount = countRouterRoutes();
  if (data.length === routerCount) pass(`route-inventory.csv: ${data.length} routes matches router (${routerCount})`);
  else fail(`route-inventory.csv: ${data.length} routes but router has ${routerCount} path entries`);

  // Validate statuses
  const statusIdx = header.indexOf("Status");
  const invalidStatuses = [];
  const statusCounts = {};
  for (const row of data) {
    const status = row[statusIdx];
    if (!VALID_STATUSES.includes(status)) invalidStatuses.push(status);
    statusCounts[status] = (statusCounts[status] || 0) + 1;
  }
  if (invalidStatuses.length === 0) pass(`route-inventory.csv: all ${data.length} statuses valid`);
  else fail(`route-inventory.csv: ${invalidStatuses.length} invalid statuses: ${[...new Set(invalidStatuses)].join(", ")}`);

  // No COMPLETE status
  if (statusCounts["COMPLETE"]) fail(`route-inventory.csv: ${statusCounts["COMPLETE"]} routes use deprecated COMPLETE status`);
  else pass("route-inventory.csv: no deprecated COMPLETE status");

  // No duplicate names (UNNAMED is allowed to repeat)
  const nameIdx = header.indexOf("Name");
  const namedRoutes = data.filter(r => r[nameIdx] !== "UNNAMED").map(r => r[nameIdx]);
  const dupes = namedRoutes.filter((n, i) => namedRoutes.indexOf(n) !== i);
  if (dupes.length === 0) pass("route-inventory.csv: all named routes unique");
  else fail(`route-inventory.csv: ${dupes.length} duplicate names: ${[...new Set(dupes)].join(", ")}`);

  // Check no empty Path
  const pathIdx = header.indexOf("Path");
  const emptyPaths = data.filter(r => !r[pathIdx]);
  if (emptyPaths.length === 0) pass("route-inventory.csv: all routes have Path");
  else fail(`route-inventory.csv: ${emptyPaths.length} routes missing Path`);

  return { data, header, statusCounts, total: data.length };
}

// ═══════════════════════════════════════════════════════════
//  2. Validate component-inventory.csv
// ═══════════════════════════════════════════════════════════
function validateComponentInventory() {
  const filePath = `${ARTIFACTS}/component-inventory.csv`;
  const { header, data } = loadCSV(filePath);

  if (header.length === 19) pass(`component-inventory.csv: ${header.length} columns (expected 19)`);
  else fail(`component-inventory.csv: ${header.length} columns (expected 19)`);

  // Check no wildcards in File
  const fileIdx = header.indexOf("File");
  const wildcards = data.filter(r => r[fileIdx].includes("*"));
  if (wildcards.length === 0) pass("component-inventory.csv: no wildcard file paths");
  else fail(`component-inventory.csv: ${wildcards.length} wildcard entries`);

  // Check USED column is boolean
  const usedIdx = header.indexOf("USED");
  const nonBool = data.filter(r => r[usedIdx] !== "true" && r[usedIdx] !== "false");
  if (nonBool.length === 0) pass("component-inventory.csv: all USED values boolean");
  else fail(`component-inventory.csv: ${nonBool.length} non-boolean USED values`);

  // Check DeadOrUsed column
  const douIdx = header.indexOf("DeadOrUsed");
  const invalidDou = data.filter(r => r[douIdx] !== "USED" && r[douIdx] !== "DEAD");
  if (invalidDou.length === 0) pass("component-inventory.csv: all DeadOrUsed values valid");
  else fail(`component-inventory.csv: ${invalidDou.length} invalid DeadOrUsed values`);

  // Consistency: USED=false + ImportedByCount>0 should be impossible (if import count > 0, USED must be true)
  const impIdx = header.indexOf("ImportedByCount");
  const inconsistent = data.filter(r => r[usedIdx] === "false" && parseInt(r[impIdx] || "0") > 0);
  if (inconsistent.length === 0) pass("component-inventory.csv: USED/ImportedByCount consistent");
  else fail(`component-inventory.csv: ${inconsistent.length} DEAD components have ImportedByCount > 0: ${inconsistent.map(r => r[0]).join(", ")}`);

  // DEAD + ImportedByCount > 0 is a FAILURE
  const deadWithImports = data.filter(r => r[douIdx] === "DEAD" && parseInt(r[impIdx] || "0") > 0);
  if (deadWithImports.length === 0) pass("component-inventory.csv: no DEAD components with imports");
  else fail(`component-inventory.csv: ${deadWithImports.length} DEAD components have imports > 0`);

  // Check ImportedByCount is numeric
  const nonNumeric = data.filter(r => isNaN(parseInt(r[impIdx])));
  if (nonNumeric.length === 0) pass("component-inventory.csv: all ImportedByCount are numeric");
  else fail(`component-inventory.csv: ${nonNumeric.length} non-numeric ImportedByCount`);

  pass(`component-inventory.csv: ${data.length} total components`);
  return data.length;
}

// ═══════════════════════════════════════════════════════════
//  3. Validate ui-violations.csv
// ═══════════════════════════════════════════════════════════
function validateUIViolations() {
  const filePath = `${ARTIFACTS}/ui-violations.csv`;
  const { header, data } = loadCSV(filePath);

  if (header.length === 11) pass(`ui-violations.csv: ${header.length} columns (expected 11)`);
  else fail(`ui-violations.csv: ${header.length} columns (expected 11)`);

  // Check VerificationMethod column
  const vmIdx = header.indexOf("VerificationMethod");
  if (vmIdx === -1) { fail("ui-violations.csv: missing VerificationMethod column"); return 0; }

  const invalidVM = data.filter(r => !VALID_VERIFICATION.includes(r[vmIdx]));
  if (invalidVM.length === 0) pass(`ui-violations.csv: all ${data.length} VerificationMethod values valid`);
  else fail(`ui-violations.csv: ${invalidVM.length} invalid VerificationMethod: ${[...new Set(invalidVM.map(r => r[vmIdx]))].join(", ")}`);

  // Check Severity is P0/P1/P2/P3
  const sevIdx = header.indexOf("Severity");
  const invalidSev = data.filter(r => !/^P[0-3]$/.test(r[sevIdx]));
  if (invalidSev.length === 0) pass("ui-violations.csv: all Severity values valid");
  else fail(`ui-violations.csv: ${invalidSev.length} invalid Severity: ${[...new Set(invalidSev.map(r => r[sevIdx]))].join(", ")}`);

  // Check v-html = P0
  const probIdx = header.indexOf("Problem");
  const vhtmlEntries = data.filter(r => r[probIdx].toLowerCase().includes("v-html"));
  const vhtmlNotP0 = vhtmlEntries.filter(r => r[sevIdx] !== "P0");
  if (vhtmlNotP0.length === 0 && vhtmlEntries.length > 0) pass(`ui-violations.csv: all ${vhtmlEntries.length} v-html entries classified as P0`);
  else if (vhtmlEntries.length === 0) warn("ui-violations.csv: no v-html entries found");
  else fail(`ui-violations.csv: ${vhtmlNotP0.length} v-html entries NOT classified as P0`);

  // Check File column does not embed line number with colon
  const fileIdx = header.indexOf("File");
  const colonInFile = data.filter(r => r[fileIdx].includes(":"));
  if (colonInFile.length === 0) pass("ui-violations.csv: File column properly separated from Line");
  else fail(`ui-violations.csv: ${colonInFile.length} rows have colon in File column`);

  pass(`ui-violations.csv: ${data.length} total violations`);
  return data.length;
}

// ═══════════════════════════════════════════════════════════
//  4. Bidirectional route check
// ═══════════════════════════════════════════════════════════
function validateBidirectional(routeData) {
  const pathIdx = routeData.header.indexOf("Path");
  const roleIdx = routeData.header.indexOf("Role");
  const csvPaths = new Set(routeData.data.map(r => r[pathIdx].replace(/^"|"$/g, "")));

  // Check router paths match CSV paths
  const src = readFileSync("frontend/src/router/index.js", "utf8");
  const routerPaths = new Set();
  const pathRegex = /\bpath:\s*'([^']+)'/g;
  let m;
  while ((m = pathRegex.exec(src)) !== null) {
    const p = m[1];
    if (p) routerPaths.add(p);
  }

  // Subtract redirect-only empty paths and special cases
  const csvRoutesPresent = [];
  const csvRoutesMissing = [];

  for (const p of routerPaths) {
    if (p === "") continue; // redirect children
    // Check if this path appears in CSV (with parent path resolution)
    let found = false;
    for (const csvP of csvPaths) {
      if (csvP === p || csvP.endsWith("/" + p) || csvP.includes(p)) {
        found = true;
        break;
      }
    }
    if (found) csvRoutesPresent.push(p);
    else csvRoutesMissing.push(p);
  }

  if (csvRoutesMissing.length === 0) pass(`Bidirectional check: all ${routerPaths.size - 1} router paths covered in CSV`);
  else warn(`Bidirectional check: ${csvRoutesMissing.length} router paths not directly matched in CSV: ${csvRoutesMissing.slice(0, 10).join(", ")}`);
}

// ═══════════════════════════════════════════════════════════
//  5. Validate master report
// ═══════════════════════════════════════════════════════════

function validateMasterReport() {
  try {
    const master = readFileSync("docs/FE_DEEP_AUDIT_MASTER.md", "utf8");
    const routeCsv = loadCSV("docs/artifacts/fe-deep-audit/route-inventory.csv");
    const compCsv = loadCSV("docs/artifacts/fe-deep-audit/component-inventory.csv");
    const uiCsv = loadCSV("docs/artifacts/fe-deep-audit/ui-violations.csv");
    
    // Check master counts exactly match CSV
    let allFound = true;
    
    const routeCount = routeCsv.data.length;
    if (master.includes(routeCount.toString() + " routes") || master.includes("Total routes**: " + routeCount)) {
      pass("Master report: exact route count " + routeCount + " found");
    } else {
      fail("Master report: does not contain exact route count " + routeCount);
      allFound = false;
    }
    
    const compCount = compCsv.data.length;
    if (master.includes(compCount.toString() + " components") || master.includes("Total components**: " + compCount)) {
      pass("Master report: exact component count " + compCount + " found");
    } else {
      fail("Master report: does not contain exact component count " + compCount);
      allFound = false;
    }
    
    const violCount = uiCsv.data.length;
    if (master.includes(violCount.toString() + " violations") || master.includes("Total**: " + violCount)) {
      pass("Master report: exact violation count " + violCount + " found");
    } else {
      fail("Master report: does not contain exact violation count " + violCount);
      allFound = false;
    }
    
    // API_CONNECTED check
    const apiCount = routeCsv.data.filter(r => r[24] === "API_CONNECTED").length;
    if (master.includes("API_CONNECTED | " + apiCount) || master.includes(apiCount + " API_CONNECTED")) {
      pass("Master report: exact API_CONNECTED count " + apiCount + " found");
    } else {
      fail("Master report: does not contain exact API_CONNECTED count " + apiCount);
      allFound = false;
    }
    
    // Check old hardcoded metrics are gone
    if (master.includes("157 components") && compCount !== 157) {
      fail("Master report: found old metric '157 components'");
      allFound = false;
    }
    if (master.includes("173 routes") && routeCount !== 173) {
      fail("Master report: found old metric '173 routes'");
      allFound = false;
    }
    if (master.includes("35 violations") && violCount !== 35) {
      fail("Master report: found old metric '35 violations'");
      allFound = false;
    }

    return allFound;
  } catch (e) {
    fail("Master report validation error: " + e.message);
    return false;
  }
}


// ═══════════════════════════════════════════════════════════
//  Run all validations
// ═══════════════════════════════════════════════════════════
console.log("Running FE Deep Audit Validation...\n");

const invResult = validateRouteInventory();
const compCount = validateComponentInventory();
const violCount = validateUIViolations();
validateBidirectional(invResult);
const masterOK = validateMasterReport();

// ═══════════════════════════════════════════════════════════
//  Write validation-results.md
// ═══════════════════════════════════════════════════════════
const timestamp = new Date().toISOString().replace("T", " ").substring(0, 19);

let verdict;
const totalChecks = results.details.length;
if (results.fail === 0 && results.warn === 0) verdict = "PASS";
else if (results.fail === 0 && results.warn > 0) verdict = "PASS_WITH_WARNINGS";
else verdict = "FAIL";

let md = `# FE Deep Audit Validation Results

**Date**: ${timestamp}  
**Scope**: Cross-file validation of all audit artifacts  
**Validator**: validate-audit.mjs

---

## Summary

| Metric | Count |
|--------|-------|
| Total checks | ${totalChecks} |
| Pass | ${results.pass} |
| Fail | ${results.fail} |
| Warning | ${results.warn} |
| Pass rate | ${((results.pass / totalChecks) * 100).toFixed(1)}% |

**Verdict**: ${verdict}

---

## Detailed Results

| # | Type | Check |
|---|------|-------|
`;

results.details.forEach((d, i) => {
  const icon = d.type === "PASS" ? "✅" : d.type === "FAIL" ? "❌" : "⚠️";
  md += `| ${i + 1} | ${icon} ${d.type} | ${d.msg} |\n`;
});

md += `\n---\n\n## Checks Per File\n\n`;
md += `### route-inventory.csv\n`;
md += `- Columns: 27 (${invResult.total} routes)\n`;
md += `- Statuses: ${Object.entries(invResult.statusCounts).map(([k, v]) => `${k}: ${v}`).join(", ")}\n\n`;
md += `### component-inventory.csv\n`;
md += `- Components: ${compCount}\n\n`;
md += `### ui-violations.csv\n`;
md += `- Violations: ${violCount}\n\n`;
md += `### Master Report: ${masterOK ? "All sections found" : "Some sections missing"}\n\n`;
md += `### Verdict: ${verdict}\n`;

writeFileSync(`${ARTIFACTS}/validation-results.md`, md);
console.log(`\nWritten validation-results.md`);
console.log(`Results: ${results.pass} pass, ${results.fail} fail, ${results.warn} warn`);
console.log(`Verdict: ${verdict}`);
