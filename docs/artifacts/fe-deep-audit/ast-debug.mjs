import { readFileSync, statSync } from "fs";
import { dirname, resolve as pathResolve, join } from "path";

const SRC = "frontend/src";
const cwd = process.cwd();
const layoutFile = join(cwd, SRC, "components/SinhVien/Layout_SinhVien.vue");

const content = readFileSync(layoutFile, "utf8");
const re = /import\s+(?:\*\s+as\s+)?\w+(?:\s*,\s*\{[^}]*\})?\s+from\s+['"]([^'"]+)['"]/g;
let m;
const foundImports = [];
while ((m = re.exec(content)) !== null) {
  foundImports.push(m[1]);
}
console.log("Found imports in Layout_SinhVien:", foundImports);

for (const impPath of foundImports) {
  let resolved;
  if (impPath.startsWith("./") || impPath.startsWith("../")) {
    const importerDir = dirname(layoutFile);
    resolved = pathResolve(importerDir, impPath);
  } else if (impPath.startsWith("@/")) {
    resolved = join(cwd, SRC, impPath.slice(2));
  } else continue;

  const exists = (() => { try { statSync(resolved); return true; } catch { return false; } })();
  const norm = resolved.replace(/\\/g, "/").replace(/^.*?frontend\/src\//, "");
  console.log("  " + impPath + " -> " + norm + " (" + (exists ? "EXISTS" : "NOT FOUND") + ")");
}

// Verify the component's relPath
const sidebarFile = join(cwd, SRC, "components/SinhVien/AppSidebar.vue");
const sidebarNorm = sidebarFile.replace(/\\/g, "/").replace(/^.*?frontend\/src\//, "");
console.log("\nAppSidebar norm:", sidebarNorm);

// Verify allVueByPath would find it
// This simulates what the generator does
const allVueByPath = {};
allVueByPath[sidebarNorm] = sidebarFile;
console.log("allVueByPath lookup:", allVueByPath[sidebarNorm] ? "FOUND" : "NOT FOUND");
