#!/usr/bin/env node
import { readFileSync, writeFileSync, readdirSync, statSync } from "fs";
import { join, relative, sep, dirname, basename, resolve as pathResolve } from "path";
import { fileURLToPath } from "url";

const SRC = "frontend/src";
const OUT = "docs/artifacts/fe-deep-audit/component-inventory.csv";

const H = [
  "Component", "Category", "File", "Type",
  "Props", "Emits", "Slots", "Variants",
  "Accessibility", "LoadingState", "ErrorState",
  "Responsive", "DarkMode", "DuplicateGroup",
  "ImportedBy", "ImportedByCount", "USED", "DeadOrUsed", "Evidence"
];

const q = (v) => {
  const s = String(v ?? "").replace(/\n/g, " ").replace(/\r/g, "");
  if (s.includes(",") || s.includes('"'))
    return '"' + s.replace(/"/g, '""') + '"';
  return s;
};

// ── File discovery ──────────────────────────────────────────
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
        if (match) results.push(full);
      }
    }
  } catch (_) { /* skip */ }
  return results;
}

// ── Normalize path (relative to SRC) ────────────────────────
function normPath(absPath) {
  return relative(SRC, absPath).replace(/\\/g, "/");
}

// ── Resolve import relative to a file ───────────────────────
function resolveImport(importPath, importerAbsPath) {
  const importerDir = dirname(importerAbsPath);

  // Alias @ -> SRC
  if (importPath.startsWith("@/")) {
    const resolved = join(SRC, importPath.slice(2));
    return tryResolve(resolved);
  }

  // Relative ./ ../
  if (importPath.startsWith("./") || importPath.startsWith("../")) {
    const resolved = pathResolve(importerDir, importPath);
    return tryResolve(resolved);
  }

  // Probably a node_modules package — skip
  return null;
}

function tryResolve(basePath) {
  // Try exact path first
  if (checkFile(basePath)) return basePath;
  // Try with .vue extension
  if (checkFile(basePath + ".vue")) return basePath + ".vue";
  // Try /index.vue
  if (checkFile(basePath + "/index.vue")) return basePath + "/index.vue";
  // Try with .js/.ts extension
  if (checkFile(basePath + ".js")) return basePath + ".js";
  if (checkFile(basePath + ".ts")) return basePath + ".ts";
  // Try /index.js /index.ts
  if (checkFile(basePath + "/index.js")) return basePath + "/index.js";
  if (checkFile(basePath + "/index.ts")) return basePath + "/index.ts";
  return null;
}

const FILE_EXISTS_CACHE = new Map();
function checkFile(p) {
  if (FILE_EXISTS_CACHE.has(p)) return FILE_EXISTS_CACHE.get(p);
  try {
    statSync(p);
    FILE_EXISTS_CACHE.set(p, true);
    return true;
  } catch {
    FILE_EXISTS_CACHE.set(p, false);
    return false;
  }
}

// ── Extract imports from file content ───────────────────────
function extractImports(content, fileAbsPath) {
  const results = [];
  if (!content) return results;

  // Named/default static imports: import X from '...' or import { X } from '...'
  const staticRe = /import\s+(?:\*\s+as\s+)?\w+(?:\s*,\s*\{[^}]*\})?\s+from\s+['"]([^'"]+)['"]/g;
  let m;
  while ((m = staticRe.exec(content)) !== null) {
    results.push(m[1]);
  }

  // Named-only imports: import { X, Y } from '...'
  const namedRe = /import\s+\{[^}]+\}\s+from\s+['"]([^'"]+)['"]/g;
  while ((m = namedRe.exec(content)) !== null) {
    if (!results.includes(m[1])) results.push(m[1]);
  }

  // Dynamic imports: import('...')
  const dynRe = /import\s*\(\s*['"]([^'"]+)['"]\s*\)/g;
  while ((m = dynRe.exec(content)) !== null) {
    results.push(m[1]);
  }

  // Re-exports: export { default } from '...' or export * from '...'
  const exportRe = /export\s+(?:\{[^}]*\}|\*\s+from)\s+from\s+['"]([^'"]+)['"]/g;
  while ((m = exportRe.exec(content)) !== null) {
    results.push(m[1]);
  }

  // Deduplicate and resolve
  const seen = new Set();
  return results.filter(p => {
    if (seen.has(p)) return false;
    seen.add(p);
    return true;
  }).map(p => resolveImport(p, fileAbsPath)).filter(Boolean);
}

// ── Analyze Vue file content ────────────────────────────────
function analyzeVue(content, filePath) {
  const displayName = basename(filePath).replace(/\.vue$/, "");
  const relPath = normPath(filePath);
  const parts = relPath.split("/");
  const viewsIdx = parts.indexOf("views");
  const compsIdx = parts.indexOf("components");
  const layoutsIdx = parts.indexOf("layouts");
  let category = "";
  if (viewsIdx !== -1 && viewsIdx + 1 < parts.length) category = parts.slice(viewsIdx + 1, -1).join("/") || parts[viewsIdx + 1];
  else if (compsIdx !== -1 && compsIdx + 1 < parts.length) category = parts.slice(compsIdx + 1, -1).join("/") || parts[compsIdx + 1];
  else if (layoutsIdx !== -1 && layoutsIdx + 1 < parts.length) category = parts.slice(layoutsIdx + 1, -1).join("/") || parts[layoutsIdx + 1];
  else category = parts.slice(-2, -1)[0] || "";

  const propsMatch = content.match(/defineProps\s*\([^)]*\)/s);
  // Also match props: { ... } in Options API
  const propsOpts = content.match(/props\s*:\s*\{[^}]*\}/s);
  
  const props = (propsMatch || propsOpts || [""])[0].substring(0, 200);
  const emits = (content.match(/defineEmits\s*\([^)]*\)/s) || [""])[0].substring(0, 150);
  const slots = (content.match(/<slot\s/g) || []).length;
  const hasAria = /aria-/.test(content);
  const hasLoading = /\bloading\b/.test(content) || /\bisLoading\b/.test(content) || /\b:loading\b/.test(content);
  const hasError = /\berror\b/.test(content) || /\bisError\b/.test(content) || /\bhasError\b/.test(content);
  const hasResponsive = /sm:|md:|lg:|xl:|2xl:/.test(content);
  const hasDarkMode = /dark:/.test(content);
  const hasEmptyState = /v-if\s*=/.test(content) && /(empty|noData|noResults|no-records)/i.test(content);

  return {
    displayName,
    relPath,
    category,
    props: props || "",
    emits: emits || "",
    slots: String(slots),
    accessibility: hasAria ? "aria-attributes-detected" : "none",
    loadingState: hasLoading ? "loading-prop-detected" : "none",
    errorState: hasError ? "error-prop-detected" : "none",
    responsive: hasResponsive ? "responsive-classes-detected" : "none",
    darkMode: hasDarkMode ? "dark-mode-classes-detected" : "none",
  };
}

// ── Main ─────────────────────────────────────────────────────
console.log("Scanning Vue files...");
const vueFiles = findAllFiles(SRC, ".vue");
console.log(`Found ${vueFiles.length} Vue files`);

// Build file map: relPath -> absPath
const allVueByPath = {};
const allVueByDisplayName = {};
for (const f of vueFiles) {
  const rp = normPath(f);
  allVueByPath[rp] = f;
  const dn = basename(f).replace(/\.vue$/, "");
  if (!allVueByDisplayName[dn]) allVueByDisplayName[dn] = [];
  allVueByDisplayName[dn].push(f);
}

// Log duplicates
for (const [dn, files] of Object.entries(allVueByDisplayName)) {
  if (files.length > 1) {
    console.log(`  Duplicate basename "${dn}": ${files.map(f => normPath(f)).join(", ")}`);
  }
}

// Analyze each Vue file
const components = [];
for (const f of vueFiles) {
  try {
    const content = readFileSync(f, "utf8");
    const info = analyzeVue(content, f);
    components.push(info);
  } catch (e) {
    console.error(`  Error reading ${f}: ${e.message}`);
  }
}

// Collect all source files for import scanning
const allSrcFiles = [...vueFiles];
const jsTsFiles = findAllFiles(SRC, [".js", ".ts"]);
allSrcFiles.push(...jsTsFiles);
console.log(`Total files scanned: ${allSrcFiles.length} (${vueFiles.length} vue, ${jsTsFiles.length} js/ts)`);

// ── Build import graph ──────────────────────────────────────
// map: relPath (importer) -> [relPath (importee)]
const importEdges = {};

console.log("Extracting imports...");
for (const f of allSrcFiles) {
  try {
    const content = readFileSync(f, "utf8");
    const importerRel = normPath(f);
    const importedPaths = extractImports(content, f);

    for (const resolvedAbs of importedPaths) {
      const impRel = normPath(resolvedAbs);
      if (!allVueByPath[impRel]) continue; // not a Vue file (could be JS/TS, skip for now)
      
      if (!importEdges[impRel]) importEdges[impRel] = [];
      if (!importEdges[impRel].includes(importerRel)) {
        importEdges[impRel].push(importerRel);
      }
    }
  } catch (e) {
    // skip files that can't be read
  }
}

// ── Find route components ───────────────────────────────────
function findRouteComponents() {
  const routeComps = new Set();
  try {
    const csv = readFileSync("docs/artifacts/fe-deep-audit/route-inventory.csv", "utf8");
    const lines = csv.split("\n").slice(1);
    const routerDir = join(SRC, "router");
    for (const line of lines) {
      if (!line.trim()) continue;
      const cols = line.split(",");
      // Column 6 (0-indexed) = Component path (relative to router dir)
      const comp = (cols[6] || "").replace(/"/g, "");
      if (comp && !comp.includes("redirect")) {
        const absRouter = pathResolve(routerDir, comp);
        const relFromSrc = normPath(absRouter);
        if (allVueByPath[relFromSrc]) {
          routeComps.add(relFromSrc);
        }
      }
    }
  } catch (_) { /* ignore */ }
  return routeComps;
}

const routeComponents = findRouteComponents();
console.log(`Route components: ${routeComponents.size}`);

// ── Build duplicate groups ──────────────────────────────────
const dupGroups = {};
for (const [dn, files] of Object.entries(allVueByDisplayName)) {
  if (files.length > 1) {
    for (const f of files) {
      const rp = normPath(f);
      dupGroups[rp] = `${dn} (${files.length}x)`;
    }
  }
}

// ── Build CSV rows ──────────────────────────────────────────
const rows = [];

for (const c of components) {
  const importedBy = importEdges[c.relPath] || [];
  const importedByCount = importedBy.length;
  const isRouteComp = routeComponents.has(c.relPath);
  const used = importedByCount > 0 || isRouteComp;
  const deadOrUsed = used ? "USED" : "DEAD";

  let evidence = "";
  if (isRouteComp) evidence = "Route component";
  else if (importedByCount > 0) evidence = `Imported by ${importedByCount} files`;
  else evidence = "No imports found";

  const dupGroup = dupGroups[c.relPath] || "";

  rows.push([
    c.relPath, c.category, c.relPath, "vue",
    c.props, c.emits, c.slots, "",
    c.accessibility, c.loadingState, c.errorState,
    c.responsive, c.darkMode, dupGroup,
    importedBy.slice(0, 30).join("; "), String(importedByCount),
    String(used), deadOrUsed, evidence
  ]);
}

// Sort by relPath for consistency
rows.sort((a, b) => a[0].localeCompare(b[0]));

// Write CSV
const headerLine = H.map(q).join(",");
const dataLines = rows.map(r => r.map(q).join(","));
const csv = [headerLine, ...dataLines].join("\n");
writeFileSync(OUT, csv, "utf8");

// Summary
const used = rows.filter(r => r[16] === "true");
const dead = rows.filter(r => r[16] === "false");
console.log(`\nWritten component-inventory.csv: ${rows.length} components, ${csv.length} bytes`);
console.log(`  USED: ${used.length}`);
console.log(`  DEAD: ${dead.length}`);

// Verify unique paths
const paths = rows.map(r => r[0]);
const uniquePaths = new Set(paths);
if (uniquePaths.size !== paths.length) {
  console.error(`  ERROR: ${paths.length - uniquePaths.size} duplicate paths in output!`);
} else {
  console.log(`  All ${uniquePaths.size} paths are unique`);
}
