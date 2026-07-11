#!/usr/bin/env node
import { parse } from "@babel/parser";
import { readFileSync, writeFileSync, existsSync } from "fs";
import { resolve, dirname, join, sep, basename } from "path";

const SRC = "frontend/src";
const src = readFileSync("frontend/src/router/index.js", "utf8");

// ── Normalize role names ────────────────────────────────────
const ROLE_MAP = {
  "Principal": "BGH",
  "AcademicStaff": "Staff",
  "HoiDongQuanLyNoiDung": "ContentCouncil"
};

function normalizeRole(r) {
  return ROLE_MAP[r] || r;
}

// ── Helper: get string value from AST node ──────────────────
function getStringValue(node) {
  if (node?.type === "StringLiteral") return node.value;
  if (node?.type === "TemplateLiteral" && node.quasis?.length === 1)
    return node.quasis[0].value.cooked;
  return null;
}

function getKeyName(prop) {
  if (prop?.type !== "ObjectProperty") return null;
  if (prop.key?.type === "Identifier") return prop.key.name;
  if (prop.key?.type === "StringLiteral") return prop.key.value;
  return null;
}

function getBoolValue(node) {
  if (node?.type === "BooleanLiteral") return node.value;
  return null;
}

// ── Parse route object from AST ObjectExpression ────────────
function parseRouteObject(node) {
  if (node?.type !== "ObjectExpression") return null;
  const route = {
    path: "", name: "", component: "", redirect: "",
    meta: { title: "", subtitle: "", section: "", requiresAuth: false, public: false, fullscreen: false, previewMode: false, role: null },
    children: [], isLayout: false, line: node.loc?.start?.line || 0
  };

  for (const prop of node.properties) {
    if (prop.type !== "ObjectProperty") continue;
    const key = getKeyName(prop);
    if (!key) continue;

    if (key === "path") { const v = getStringValue(prop.value); if (v !== null) route.path = v; }
    else if (key === "name") { const v = getStringValue(prop.value); if (v !== null) route.name = v; }
    else if (key === "redirect") {
      if (prop.value.type === "StringLiteral") route.redirect = prop.value.value;
      else route.redirect = "(function)";
    }
    else if (key === "component") {
      const fn = prop.value;
      if (fn?.type === "ArrowFunctionExpression" || fn?.type === "FunctionExpression") {
        // Arrow body is directly ImportExpression: () => import("...")
        if (fn.body?.type === "ImportExpression") {
          const p = getStringValue(fn.body.source);
          if (p) route.component = p.replace(/\\/g, "/");
        }
        // Block with return import()
        else if (fn.body?.type === "BlockStatement") {
          const ret = fn.body.body?.find(s => s?.type === "ReturnStatement");
          if (ret?.argument?.type === "ImportExpression") {
            const p = getStringValue(ret.argument.source);
            if (p) route.component = p.replace(/\\/g, "/");
          }
        }
      }
    }
    else if (key === "children" && prop.value?.type === "ArrayExpression") {
      route.isLayout = true;
      route.children = prop.value.elements.filter(e => e?.type === "ObjectExpression");
    }
    else if (key === "meta" && prop.value?.type === "ObjectExpression") {
      for (const mp of prop.value.properties) {
        if (mp.type !== "ObjectProperty") continue;
        const mk = getKeyName(mp);
        if (!mk) continue;
        if (mk === "title") route.meta.title = getStringValue(mp.value) || "";
        else if (mk === "subtitle") route.meta.subtitle = getStringValue(mp.value) || "";
        else if (mk === "section") route.meta.section = getStringValue(mp.value) || "";
        else if (mk === "requiresAuth") route.meta.requiresAuth = getBoolValue(mp.value) || false;
        else if (mk === "public") route.meta.public = getBoolValue(mp.value) || false;
        else if (mk === "fullscreen") route.meta.fullscreen = getBoolValue(mp.value) || false;
        else if (mk === "previewMode") route.meta.previewMode = getBoolValue(mp.value) || false;
        else if (mk === "role") {
          if (mp.value?.type === "StringLiteral") route.meta.role = normalizeRole(mp.value.value);
          else if (mp.value?.type === "ArrayExpression") {
            route.meta.role = mp.value.elements
              .filter(e => e?.type === "StringLiteral")
              .map(e => normalizeRole(e.value)).join("|");
          }
        }
      }
    }
    else if (key === "name") {
      // Skip - already handled above
    }
  }
  return route;
}

// ── Determine RecordType ────────────────────────────────────
function getRecordType(route) {
  if (route.path.startsWith("/:pathMatch")) return "CATCH_ALL";
  if (route.redirect && !route.isLayout) return "REDIRECT";
  if (route.isLayout) return "LAYOUT";
  if (route.meta.fullscreen) return "FULLSCREEN";
  return "PAGE";
}

// ── Flatten route tree ──────────────────────────────────────
function flattenRoutes(elements, parentPath, parentRole) {
  const result = [];
  for (const el of elements) {
    const route = parseRouteObject(el);
    if (!route) continue;

    // Resolve full path
    const fullPath = route.path.startsWith("/") ? route.path :
      parentPath ? parentPath + "/" + route.path : route.path;

    // Determine role
    let role = route.meta.role || parentRole;
    if (!role) {
      if (route.meta.public) role = "Public";
      else if (route.path.startsWith("/:pathMatch")) role = "404";
      else if (!parentPath) role = "Public";
      else if (parentRole) role = parentRole;
      else role = "Public";
    }

    const recordType = getRecordType(route);

    // Skip empty-path redirect children
    if (route.path === "" && route.redirect) continue;

    const entry = {
      path: fullPath,
      name: route.name || "UNNAMED",
      component: route.component,
      redirect: route.redirect,
      meta: route.meta,
      isLayout: route.isLayout,
      recordType,
      role,
      line: route.line
    };
    result.push(entry);

    // Recurse children
    if (route.isLayout && route.children.length > 0) {
      result.push(...flattenRoutes(route.children, fullPath, role));
    }
  }
  return result;
}

// ── Parse router with Babel AST ─────────────────────────────
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
if (!routerCall) { console.error("createRouter not found"); process.exit(1); }

const routesProp = routerCall.arguments?.[0]?.properties?.find(
  p => p.type === "ObjectProperty" && p.key?.type === "Identifier" && p.key.name === "routes"
);
if (!routesProp) { console.error("routes property not found"); process.exit(1); }
if (routesProp.value.type !== "ArrayExpression") { console.error("routes is not an array"); process.exit(1); }

const allRoutes = flattenRoutes(routesProp.value.elements, "", "");

// ── Status assignment ───────────────────────────────────────

function traceApiEndpoint(componentPath) {
  if (!componentPath || componentPath.includes("redirect")) return null;
  const absPath = resolve(process.cwd(), "frontend/src/router", componentPath);
  if (!existsSync(absPath)) return null;
  
  const content = readFileSync(absPath, "utf8");
  const importRe = /import\s+.*?\s+from\s+['"]([^'"]+)['"]/g;
  let m;
  const importsToTrace = [];
  while ((m = importRe.exec(content)) !== null) {
    const p = m[1];
    if (p.includes("service") || p.includes("api") || p.includes("store") || p.includes("composable")) {
      importsToTrace.push(p);
    }
  }

  for (const imp of importsToTrace) {
    let resolved = null;
    if (imp.startsWith("@/")) {
      resolved = join(process.cwd(), "frontend/src", imp.slice(2));
    } else if (imp.startsWith("./") || imp.startsWith("../")) {
      resolved = resolve(dirname(absPath), imp);
    } else {
      continue;
    }
    
    const exts = ["", ".js", ".ts"];
    let finalPath = null;
    for (const ext of exts) {
      if (existsSync(resolved + ext)) {
        finalPath = resolved + ext;
        break;
      }
    }
    
    if (finalPath) {
      const fileContent = readFileSync(finalPath, "utf8");
      // Find exact API endpoint (no wildcard, no interpolation that breaks it)
      const endpointRe = /(?:get|post|put|delete|patch)\s*\(\s*`?(['"])(.*?)\1|api(?:Request)?.*?\(\s*`?(['"])(.*?)\3/gi;
      let em;
      while ((em = endpointRe.exec(fileContent)) !== null) {
        const endpoint = em[2] || em[4];
        if (endpoint && endpoint.includes("/api/") && !endpoint.includes("*") && !endpoint.includes("${")) {
          return { endpoint, service: basename(finalPath, ".js") };
        }
      }
      
      // Fallback: look for any literal string containing /api/
      const literalRe = /['"`](\/api\/[a-zA-Z0-9/_-]+)['"`]/g;
      let litM;
      while ((litM = literalRe.exec(fileContent)) !== null) {
        const endpoint = litM[1];
        if (endpoint && endpoint.includes("/api/") && !endpoint.includes("*") && !endpoint.includes("${")) {
          return { endpoint, service: basename(finalPath, ".js") };
        }
      }
    }
  }
  return null;
}

function getStatus(r) {
  // Static / placeholder checks
  if (r.component?.includes("AboutView")) return { status: "PLACEHOLDER", endpoint: "", service: "" };
  if (r.component?.includes("PortalLandingView") || r.component?.includes("NotFoundView") ||
      r.component?.includes("PaymentSuccessView") || r.component?.includes("PaymentCancelView")) {
    return { status: "STATIC_FUNCTIONAL", endpoint: "", service: "" };
  }
  if (r.redirect && !r.isLayout) return { status: "STATIC_FUNCTIONAL", endpoint: "", service: "" };
  if (r.isLayout) return { status: "STATIC_FUNCTIONAL", endpoint: "", service: "" };
  
  if (r.component?.includes("Student/NotificationsView") && r.role !== "Student") {
    return { status: "WRONG_CONTEXT", endpoint: "", service: "" };
  }
  
  if (r.component && (
    r.component.includes("FinanceMonitorView") ||
    r.component.includes("TuitionConfigView") ||
    r.component.includes("SupportTicketsView") ||
    r.component.includes("LearningReportView") ||
    r.component.includes("AttendanceReportView")
  )) {
    return { status: "HIDE_FROM_DEMO", endpoint: "", service: "" };
  }
  
  // Dynamic API tracing
  const traced = traceApiEndpoint(r.component);
  if (traced) {
    return { status: "API_CONNECTED", endpoint: traced.endpoint, service: traced.service };
  }

  // Default: UNVERIFIED
  return { status: "UNVERIFIED", endpoint: "", service: "" };
}


// ── Build CSV ───────────────────────────────────────────────
const H = [
  "Role","Path","RecordType","Name","ParentRoute","ParentLayout","Component",
  "MetaTitle","MetaSubtitle","MetaSection","RequiresAuth","RequiredRoles",
  "Fullscreen","PreviewMode","MenuSource","MenuVisible","BreadcrumbSource",
  "PageTitleSource","Store","Composable","Service","APIEndpoint","DataType",
  "RuntimeVerified","Status","UXRisk","TechnicalRisk","Evidence"
];

// Sort by path for consistency
allRoutes.sort((a, b) => a.path.localeCompare(b.path));

const q = (v) => {
  const s = String(v ?? "");
  if (s.includes(",") || s.includes('"') || s.includes("\n"))
    return '"' + s.replace(/"/g, '""') + '"';
  return s;
};

const rows = [];
for (const r of allRoutes) {
  const si = getStatus(r);
  const status = si.status;
  const endpoint = si.endpoint;
  const service = si.service;

  const hasTitle = !!r.meta.title;
  const isMenuVisible = hasTitle && !r.redirect && r.name !== "UNNAMED";
  const menuSource = r.meta.section ? "menuGroups.js" : isMenuVisible ? "meta.title" : "None";

  const dataType = endpoint ? "Real API" : "Static UI";
  const runtimeVerified = "STATIC_CODE";

  // Evidence
  let evidence = "";
  if (r.isLayout) evidence = `Layout for ${r.role}`;
  else if (status === "API_CONNECTED") evidence = `${service}: ${endpoint}`;
  else if (status === "UNVERIFIED") evidence = "No API endpoint evidence traced";
  else if (status === "STATIC_FUNCTIONAL") evidence = r.redirect ? "Redirect route" : "Static view — no API";
  else if (status === "PLACEHOLDER") evidence = "Placeholder view";
  else if (status === "HIDE_FROM_DEMO") evidence = "Hidden from demo";
  else if (status === "WRONG_CONTEXT") evidence = "Uses component from wrong role context";
  else evidence = status;

  rows.push([
    r.role, r.path, r.recordType, r.name,
    "N/A", "None",
    r.component || (r.redirect ? "redirect (function)" : ""),
    r.meta.title, r.meta.subtitle, r.meta.section,
    String(!!r.meta.requiresAuth), r.role || "None",
    String(!!r.meta.fullscreen), String(!!r.meta.previewMode),
    menuSource, String(isMenuVisible),
    isMenuVisible ? "route.meta.title" : "None",
    isMenuVisible ? "route.meta.title" : "None",
    "", "", service, endpoint, dataType,
    runtimeVerified, status, "Low", "Low",
    `${r.recordType} ${r.role} route ${r.path} (line ${r.line})`
  ]);
}

const headerLine = H.map(q).join(",");
const dataLines = rows.map(r => r.map(q).join(","));
const csv = [headerLine, ...dataLines].join("\n");
writeFileSync("docs/artifacts/fe-deep-audit/route-inventory.csv", csv, "utf8");

// ── Summary ──────────────────────────────────────────────────
const byType = {}, byRole = {}, byStatus = {};
for (const r of rows) {
  byType[r[2]] = (byType[r[2]] || 0) + 1;
  byRole[r[0]] = (byRole[r[0]] || 0) + 1;
  byStatus[r[24]] = (byStatus[r[24]] || 0) + 1;
}

console.log(`Written route-inventory.csv: ${rows.length} routes, ${csv.length} bytes`);
console.log("\nBy RecordType:");
for (const [k, v] of Object.entries(byType).sort()) console.log(`  ${k}: ${v}`);
console.log("\nBy Role:");
for (const [k, v] of Object.entries(byRole).sort()) console.log(`  ${k}: ${v}`);
console.log("\nBy Status:");
for (const [k, v] of Object.entries(byStatus).sort()) console.log(`  ${k}: ${v}`);
console.log(`\nAPI_CONNECTED: ${rows.filter(r => r[24] === "API_CONNECTED").length}`);
console.log(`UNVERIFIED: ${rows.filter(r => r[24] === "UNVERIFIED").length}`);
console.log(`Browser-verified: ${rows.filter(r => r[23] === "BROWSER").length}`);
console.log(`Unnamed: ${rows.filter(r => r[3] === "UNNAMED").length}`);
