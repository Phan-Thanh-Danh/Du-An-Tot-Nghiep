/**
 * Regenerate DOCUMENT_REFERENCES.csv from actual Markdown source links.
 *
 * Scans all .md files in the repo, extracts markdown links (inline, reference-style,
 * and HTML <a>/<img>), resolves them, validates targets exist, and writes
 * the CSV with a ValidationStatus column.
 *
 * Does NOT scan plain-text/backtick paths — only formal markdown link syntax.
 *
 * Exits non-zero when any broken reference is found.
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
let brokenCount = 0;

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

// ── Link extraction helpers ───────────────────────────────────────────────────

function extractInlineLinks(line) {
  const results = [];
  // Inline: [text](path) and ![alt](path) — captures optional title in quotes after path
  const inlineRe = /!?\[(?![^\]]*:\/\/)[^\]]*\]\(\s*([^\s()]+(?:\s+"[^"]*")?)\s*\)/g;
  let m;
  while ((m = inlineRe.exec(line)) !== null) {
    let target = m[1].trim();
    // Strip title ("..." or '...') at end
    target = target.replace(/\s+["'][^"']*["']$/, '');
    results.push(target);
  }
  return results;
}

function extractReferenceStyleLinks(line) {
  const results = [];
  // Reference definition: [label]: path
  const defRe = /^\[([^\]]+)\]:\s+(\S+)/gm;
  let m;
  while ((m = defRe.exec(line)) !== null) {
    results.push(m[2].trim());
  }
  return results;
}

function extractHTMLLinks(line) {
  const results = [];
  // <a href="..."> and <img src="...">
  const htmlRe = /<(?:a|img)\s[^>]*(?:href|src)\s*=\s*"([^"]+)"/gi;
  let m;
  while ((m = htmlRe.exec(line)) !== null) {
    results.push(m[1].trim());
  }
  return results;
}

function extractAllLinks(line) {
  return [
    ...extractInlineLinks(line),
    ...extractReferenceStyleLinks(line),
    ...extractHTMLLinks(line),
  ];
}

// ── Target normalization ──────────────────────────────────────────────────────

function stripFragment(target) {
  const hashIdx = target.indexOf('#');
  return hashIdx >= 0 ? target.slice(0, hashIdx) : target;
}

// ── File processing ───────────────────────────────────────────────────────────

function processFile(relPath, fullPath) {
  fileCount++;
  const content = fs.readFileSync(fullPath, 'utf8');
  const lines = content.split('\n');

  const seen = new Set();

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const rawTargets = extractAllLinks(line);
    const lineNum = i + 1;

    for (const rawTarget of rawTargets) {
      const dedupKey = `${rawTarget}:${lineNum}`;
      if (seen.has(dedupKey)) continue;
      seen.add(dedupKey);

      // Skip external URLs, anchors, mailto
      if (rawTarget.startsWith('http://') || rawTarget.startsWith('https://')) continue;
      if (rawTarget.startsWith('#') || rawTarget.startsWith('mailto:')) continue;

      // Strip fragment for resolution
      const target = stripFragment(rawTarget);

      // Check for file:/// absolute paths
      if (/^file:\/\/\/[cC]:/i.test(target)) {
        console.warn(`[BROKEN_ABSOLUTE] ${relPath}:${lineNum} — ${rawTarget}`);
        refs.push({
          SourcePath: relPath,
          TargetPath: rawTarget,
          Line: lineNum,
          ReferenceType: 'Link',
          ValidationStatus: 'BROKEN_ABSOLUTE',
        });
        brokenCount++;
        continue;
      }

      // Resolve relative path from the source file's directory
      const sourceDir = path.dirname(fullPath);
      const resolved = path.resolve(sourceDir, target);
      const relTarget = path.relative(ROOT, resolved).replace(/\\/g, '/');

      // Skip if outside repo
      if (relTarget.startsWith('..') || path.isAbsolute(relTarget)) {
        refs.push({
          SourcePath: relPath,
          TargetPath: rawTarget,
          Line: lineNum,
          ReferenceType: 'Link',
          ValidationStatus: 'OUTSIDE_REPO',
        });
        brokenCount++;
        continue;
      }

      // Verify target exists
      if (!fs.existsSync(resolved)) {
        console.warn(`[BROKEN_LINK] ${relPath}:${lineNum} — '${rawTarget}' resolves to '${relTarget}' but file does not exist`);
        refs.push({
          SourcePath: relPath,
          TargetPath: relTarget,
          Line: lineNum,
          ReferenceType: 'Link',
          ValidationStatus: 'BROKEN_TARGET',
        });
        brokenCount++;
        continue;
      }

      refs.push({
        SourcePath: relPath,
        TargetPath: relTarget,
        Line: lineNum,
        ReferenceType: 'Link',
        ValidationStatus: 'VALID',
      });
    }
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

// Write CSV with ValidationStatus column
const csvPath = path.join(ROOT, 'docs/governance/DOCUMENT_REFERENCES.csv');
const header = 'SourcePath,TargetPath,Line,ReferenceType,ValidationStatus';
const rows = refs.map(r =>
  `${r.SourcePath},${r.TargetPath},${r.Line},${r.ReferenceType},${r.ValidationStatus}`
);
const csvContent = [header, ...rows, ''].join('\n');
fs.writeFileSync(csvPath, csvContent, 'utf8');

const validCount = refs.filter(r => r.ValidationStatus === 'VALID').length;
console.log(`  ${validCount} valid, ${brokenCount} broken, ${refs.length} total references`);
console.log(`  Written to ${csvPath}`);
console.log(brokenCount > 0 ? `FAILED — ${brokenCount} broken reference(s) found.` : 'Done.');

if (brokenCount > 0) {
  process.exit(1);
}
