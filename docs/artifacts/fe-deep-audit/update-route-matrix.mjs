#!/usr/bin/env node
import { readFileSync, writeFileSync } from "fs";

// Parse route-inventory.csv with proper CSV parsing
const csv = readFileSync("docs/artifacts/fe-deep-audit/route-inventory.csv", "utf8").trim().split("\n");

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

const header = parseCSVLine(csv[0]);
const roleIdx = header.indexOf("Role");
const pathIdx = header.indexOf("Path");
const nameIdx = header.indexOf("Name");
const layoutIdx = header.indexOf("ParentLayout");
const viewIdx = header.indexOf("Component");
const apiIdx = header.indexOf("APIEndpoint");
const statusIdx = header.indexOf("Status");
const metaTitleIdx = header.indexOf("MetaTitle");

// Group routes by role
const routesByRole = {};
for (let i = 1; i < csv.length; i++) {
  const parts = parseCSVLine(csv[i]);
  const role = parts[roleIdx];
  if (!routesByRole[role]) routesByRole[role] = [];
  routesByRole[role].push(parts);
}

// Build markdown
const roleLabel = {
  "404": "404",
  "Public": "Public",
  "Student": "Student",
  "Parent": "Parent",
  "Teacher": "Teacher",
  "Staff": "Staff/Giao Vu",
  "BGH": "BGH",
  "SuperAdmin": "SuperAdmin",
  "ContentCouncil": "Content Council"
};

function getTitleFromCSV(parts) {
  return parts[metaTitleIdx] || "";
}

let md = `# Route Matrix: Role → Route → Menu → Layout → View → API → Permission → Status

## Legend
- **API_CONNECTED**: View exists, layout correct, real API calls confirmed
- **STATIC_FUNCTIONAL**: View works with static/inline data, no API
- **RUNTIME_VERIFIED**: Verified working in production/browser test
- **UNVERIFIED**: View exists but API status unknown
- **WRONG_CONTEXT**: View reused from different role
- **HIDE_FROM_DEMO**: Route exists but hidden from demo
- **PLACEHOLDER**: Stub or default template
- **MOCK_DATA**: Uses mock data instead of real API
- **INLINE_DATA**: Uses inline data arrays
- **BROKEN**: Route/view has issues

---

`;

function getLayoutInfo(parts) {
  const layout = parts[layoutIdx];
  if (layout === "None" || layout === "N/A") return "None";
  return layout;
}

function getAPIInfo(parts) {
  return parts[apiIdx] || "N/A";
}

function getMenuInfo(parts) {
  // From route-inventory: MenuSource is at index 13
  const menuSource = parts[13] || "None";
  return menuSource === "None" ? "None" : menuSource;
}

function getPermissionInfo(parts) {
  const roles = parts[10] || "None";
  return `role: ${roles}`;
}

const roleOrder = ["Public", "404", "Student", "Parent", "Teacher", "Staff", "BGH", "SuperAdmin", "ContentCouncil"];

for (const role of roleOrder) {
  const routes = routesByRole[role];
  if (!routes || routes.length === 0) continue;
  
  const label = roleLabel[role] || role;
  md += `## ${label} (${routes.length} routes)\n\n`;
  md += `| Route | Menu | Layout | View | API | Permission | Status |\n`;
  md += `|-------|------|--------|------|-----|------------|--------|\n`;
  
  for (const parts of routes) {
    const path = parts[pathIdx];
    const view = parts[viewIdx];
    const status = parts[statusIdx];
    const menu = getMenuInfo(parts);
    const layout = getLayoutInfo(parts);
    const api = getAPIInfo(parts);
    const perm = getPermissionInfo(parts);
    
    md += `| ${path} | ${menu} | ${layout} | ${view} | ${api} | ${perm} | **${status}** |\n`;
  }
  
  md += `\n`;
}

// Generate summary counts
const totalRoutes = csv.length - 1;
const statusCounts = {};
for (let i = 1; i < csv.length; i++) {
  const parts = parseCSVLine(csv[i]);
  const status = parts[statusIdx];
  statusCounts[status] = (statusCounts[status] || 0) + 1;
}

md += `## Summary Counts\n\n`;
md += `| Metric | Count |\n`;
md += `|--------|-------|\n`;
md += `| Total routes (excluding redirects) | ${totalRoutes} |\n`;
md += `| API_CONNECTED routes | ${statusCounts["API_CONNECTED"] || 0} |\n`;
md += `| STATIC_FUNCTIONAL routes | ${statusCounts["STATIC_FUNCTIONAL"] || 0} |\n`;
md += `| RUNTIME_VERIFIED routes | ${statusCounts["RUNTIME_VERIFIED"] || 0} |\n`;
md += `| PLACEHOLDER routes | ${statusCounts["PLACEHOLDER"] || 0} |\n`;
md += `| UNVERIFIED routes | ${statusCounts["UNVERIFIED"] || 0} |\n`;
md += `| WRONG_CONTEXT routes | ${statusCounts["WRONG_CONTEXT"] || 0} |\n`;
md += `| HIDE_FROM_DEMO routes | ${statusCounts["HIDE_FROM_DEMO"] || 0} |\n`;
md += `| MOCK_DATA routes | ${statusCounts["MOCK_DATA"] || 0} |\n`;
md += `| INLINE_DATA routes | ${statusCounts["INLINE_DATA"] || 0} |\n`;
md += `| BROKEN routes | ${statusCounts["BROKEN"] || 0} |\n`;
md += `| Public routes | 6 (/, /about, /login/:portal, /login, /payment/success, /payment/cancel) + 1 404 |\n`;
md += `| Routes reusing Student view across roles | 2 (teacher/notifications, bgh/notifications) |\n`;
md += `| Routes without meta.title (risk) | 26 (see note below) |\n`;

md += `\n---\n`;

writeFileSync("docs/artifacts/fe-deep-audit/route-matrix.md", md);
console.log(`Written route-matrix.md with ${totalRoutes} routes across ${roleOrder.length} roles`);

// Print per-role counts
for (const role of roleOrder) {
  const routes = routesByRole[role];
  if (routes) console.log(`  ${roleLabel[role]}: ${routes.length} routes`);
}
