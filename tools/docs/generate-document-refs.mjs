/**
 * Regenerate DOCUMENT_REFERENCES.csv from actual Markdown source links.
 *
 * Scans all .md files in the repo, extracts markdown links [...](path)
 * pointing to internal files, and writes the CSV.
 */

import fs from 'fs';
import path from 'path';

const ROOT = process.cwd();
const IGNORE_DIRS = new Set([
  'node_modules', '.git', '.cursor',
  'Backend/bin', 'Backend/obj',
  'frontend/node_modules', 'frontend/dist',
]);

const refs = [];
let fileCount = 0;

function walk(dir) {
  let entries;
  try {
    entries = fs.readdirSync(dir, { withFileTypes: true });
  } catch {
    return;
  }
  for (const entry of entries) {
    const full = path.join(dir, entry.name);
    const rel = path.relative(ROOT, full).replace(/\\/g, '/');
    if (entry.isDirectory()) {
      if (!IGNORE_DIRS.has(entry.name) && !rel.startsWith('.')) {
        walk(full);
      }
    } else if (entry.name.endsWith('.md')) {
      processFile(rel, full);
    }
  }
}

function processFile(relPath, fullPath) {
  fileCount++;
  const content = fs.readFileSync(fullPath, 'utf8');
  const lines = content.split('\n');

  // Regex to match [text](path) and ![alt](path) markdown links
  const linkRe = /\[(?![^\]]*:\/\/)[^\]]*\]\(([^)]+)\)/g;
  // Also match image links ![alt](path)
  const imgRe = /!\[(?![^\]]*:\/\/)[^\]]*\]\(([^)]+)\)/g;

  const allLinks = [];

  // Collect both types
  for (const line of lines) {
    const matches = [...line.matchAll(linkRe), ...line.matchAll(imgRe)];
    for (const m of matches) {
      allLinks.push({ target: m[1].trim(), line: lines.indexOf(line) + 1 });
    }
  }

  // Deduplicate by (target, line) within the same file
  const seen = new Set();
  for (const { target, line } of allLinks) {
    if (seen.has(`${target}:${line}`)) continue;
    seen.add(`${target}:${line}`);

    // Skip URLs, anchors, absolute paths (except repo-relative)
    if (target.startsWith('http://') || target.startsWith('https://')) continue;
    if (target.startsWith('#') || target.startsWith('mailto:')) continue;
    if (target.startsWith('file:///')) {
      console.warn(`[BROKEN_ABSOLUTE] ${relPath}:${line} — ${target}`);
      continue;
    }

    // Resolve relative path from the source file's directory
    const sourceDir = path.dirname(fullPath);
    const resolved = path.resolve(sourceDir, target);
    const relTarget = path.relative(ROOT, resolved).replace(/\\/g, '/');

    // Skip if outside repo
    if (relTarget.startsWith('..') || path.isAbsolute(relTarget)) continue;

    // Verify target exists
    if (!fs.existsSync(resolved)) {
      console.warn(`[BROKEN_LINK] ${relPath}:${line} — '${target}' resolves to '${relTarget}' but file does not exist`);
      continue;
    }

    refs.push({
      SourcePath: relPath,
      TargetPath: relTarget,
      Line: line,
      ReferenceType: 'Link',
    });
  }
}

// ── Main ──────────────────────────────────────────────────────────────────────

console.log('Scanning repository for Markdown file references...');
walk(ROOT);
console.log(`  Scanned ${fileCount} .md files`);

// Sort by SourcePath then Line
refs.sort((a, b) => {
  if (a.SourcePath < b.SourcePath) return -1;
  if (a.SourcePath > b.SourcePath) return 1;
  return a.Line - b.Line;
});

// Write CSV
const csvPath = path.join(ROOT, 'docs/governance/DOCUMENT_REFERENCES.csv');
const header = 'SourcePath,TargetPath,Line,ReferenceType';
const rows = refs.map(r =>
  `${r.SourcePath},${r.TargetPath},${r.Line},${r.ReferenceType}`
);
const csvContent = [header, ...rows, ''].join('\n');
fs.writeFileSync(csvPath, csvContent, 'utf8');

console.log(`  Found ${refs.length} references`);
console.log(`  Written to ${csvPath}`);
console.log('Done.');
