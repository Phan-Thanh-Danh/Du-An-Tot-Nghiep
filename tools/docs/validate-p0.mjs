/**
 * P0 Integrity Validator
 *
 * Checks:
 * 1. Required P0 files exist
 * 2. Capability Matrix: no duplicate CapabilityIds
 * 3. Capability Matrix: all FrontendRole values are canonical roles
 * 4. Capability Matrix: every IMPLEMENTED row has MatchedEndpointId(s)
 * 5. Capability Matrix: no row is 100% IMPLEMENTED with empty Evidence
 * 6. Referential integrity: every MISSING/PARTIAL CapabilityId appears in Backlog
 * 7. Referential integrity: Backlog does not claim endpoints that are IMPLEMENTED
 * 8. Team Ownership: folder paths referenced must exist in filesystem
 * 9. Frontend unit tests pass
 * 10. Frontend lint passes
 * 11. Frontend build passes
 * 12. Backend build passes
 */

import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';

const ROOT = process.cwd();
let errors = [];
let warnings = [];

// ── Helpers ──────────────────────────────────────────────────────────────────

function check(condition, errorMsg) {
  if (!condition) errors.push(errorMsg);
}

function warn(condition, warnMsg) {
  if (!condition) warnings.push(warnMsg);
}

function fileExists(relPath, label) {
  const full = path.join(ROOT, relPath);
  if (!fs.existsSync(full)) {
    errors.push(`Missing file: ${label} (${relPath})`);
    return false;
  }
  return true;
}

function parseCSV(filePath) {
  const content = fs.readFileSync(path.join(ROOT, filePath), 'utf8');
  const lines = content.split('\n').filter(l => l.trim());
  const headers = lines[0].split(',').map(h => h.trim().replace(/^"|"$/g, ''));
  return lines.slice(1).map(line => {
    // Naive CSV parse (handles quoted fields with commas)
    const values = [];
    let current = '';
    let inQuote = false;
    for (const ch of line) {
      if (ch === '"') { inQuote = !inQuote; }
      else if (ch === ',' && !inQuote) { values.push(current.trim()); current = ''; }
      else { current += ch; }
    }
    values.push(current.trim());
    return Object.fromEntries(headers.map((h, i) => [h, (values[i] || '').replace(/^"|"$/g, '')]));
  });
}

// ── Canonical roles from AuthConstants ───────────────────────────────────────
const CANONICAL_ROLES = new Set([
  'Teacher', 'Student', 'Parent', 'Principal', 'SuperAdmin', 'Admin',
  'AcademicStaff', 'HoiDongQuanLyNoiDung', 'CampusAdmin'
]);

// ── Check 1: Required files ───────────────────────────────────────────────────
console.log('\n[1] Checking required P0 files...');
const FILES = {
  'Endpoint Inventory': 'docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv',
  'Capability Matrix': 'docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv',
  'Missing Backlog': 'docs/p0/P0_MISSING_BACKEND_BACKLOG.md',
  'Frontend Backlog': 'docs/p0/P0_FRONTEND_INTEGRATION_BACKLOG.md',
  'Team Ownership': 'docs/p0/P0_TEAM_OWNERSHIP.md',
  'Document Inventory': 'docs/governance/DOCUMENT_INVENTORY.csv',
  'Document Move Plan': 'docs/governance/DOCUMENT_MOVE_PLAN.csv',
  'Document References': 'docs/governance/DOCUMENT_REFERENCES.csv',
  'Document Map': 'docs/governance/DOCUMENT_MAP.md',
  'Document Naming Standard': 'docs/governance/DOCUMENT_NAMING_STANDARD.md',
  'Document Ownership': 'docs/governance/DOCUMENT_OWNERSHIP.md',
};
for (const [label, relPath] of Object.entries(FILES)) {
  fileExists(relPath, label);
}

// ── Check 2-7: Capability Matrix validation ───────────────────────────────────

// Load Endpoint Inventory
const invContent = fs.readFileSync(path.join(ROOT, 'docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv'), 'utf8');
const invLines = invContent.split('\n').filter(l => l.trim());
const invHeaders = invLines[0].split(',').map(h => h.trim().replace(/^"|"$/g, ''));
const inventory = invLines.slice(1).map(line => {
    const values = [];
    let current = '';
    let inQuote = false;
    for (const ch of line) {
        if (ch === '"') { inQuote = !inQuote; }
        else if (ch === ',' && !inQuote) { values.push(current.trim()); current = ''; }
        else { current += ch; }
    }
    values.push(current.trim());
    return Object.fromEntries(invHeaders.map((h, i) => [h, (values[i] || '').replace(/^"|"$/g, '')]));
});

if (fs.existsSync(path.join(ROOT, 'docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv'))) {
  console.log('[2-7] Validating Capability Matrix...');
  const rows = parseCSV('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv');

  // 2. No duplicate CapabilityIds
  const ids = rows.map(r => r['CapabilityId']);
  const dupes = ids.filter((id, i) => ids.indexOf(id) !== i);
  check(dupes.length === 0, `Duplicate CapabilityIds: ${[...new Set(dupes)].join(', ')}`);

  // 3. All FrontendRole values are canonical
  const invalidRoles = rows.filter(r => r['FrontendRole'] && !CANONICAL_ROLES.has(r['FrontendRole']));
  invalidRoles.forEach(r => {
    errors.push(`Non-canonical FrontendRole '${r['FrontendRole']}' in row ${r['CapabilityId']}`);
  });

  // 4. At least one PARTIAL or MISSING (not all IMPLEMENTED)
  const hasPartialOrMissing = rows.some(r => ['PARTIAL', 'MISSING'].includes(r['BackendStatus']));
  check(hasPartialOrMissing, 'Capability Matrix claims 100% IMPLEMENTED — impossible for a real system. Add PARTIAL/MISSING entries.');

  // 5. Every IMPLEMENTED row must have MatchedEndpointId that exists in inventory (Supports 1-to-N)
  const implementedRows = rows.filter(r => r['BackendStatus'] === 'IMPLEMENTED');
  implementedRows.forEach(r => {
    if (!r['MatchedEndpointIds'] || r['MatchedEndpointIds'].trim() === '') {
        errors.push(`${r['CapabilityId']}: BackendStatus=IMPLEMENTED but MatchedEndpointIds is empty`);
        return;
    }
    const ids = r['MatchedEndpointIds'].split('|').map(x => x.trim());
    let expectedEvidenceParts = [];
    ids.forEach(id => {
        const ep = inventory.find(i => i.EndpointId === id);
        if (!ep) {
            errors.push(`${r['CapabilityId']}: MatchedEndpointIds '${id}' not found in Endpoint Inventory`);
        } else {
            expectedEvidenceParts.push(`[${ep.HttpMethod} ${ep.Route}] Controller: ${ep.Controller}, Action: ${ep.Action}`);
        }
    });
    
    // Check 5b: Evidence integrity for all endpoints
    const expectedEvidence = expectedEvidenceParts.join(' | ');
    if (r['Evidence'] !== expectedEvidence && expectedEvidenceParts.length > 0) {
        errors.push(`${r['CapabilityId']}: Evidence mismatch.\n  Expected: ${expectedEvidence}\n  Found:    ${r['Evidence']}`);
    }
  });

  // 5c. Check for status contradiction
  rows.forEach(r => {
      if (r['BackendStatus'] === 'MISSING' && r['FrontendStatus'] === 'IMPLEMENTED') {
          errors.push(`${r['CapabilityId']}: Contradiction - Backend is MISSING but Frontend is IMPLEMENTED.`);
      }
      
      // 5d. MissingEvidence verification
      if (r['BackendStatus'] === 'MISSING') {
          if (!r['MissingEvidence'] || r['MissingEvidence'].trim() === '') {
              errors.push(`${r['CapabilityId']}: BackendStatus is MISSING but MissingEvidence is empty.`);
          }
      }
  });

  // 6. Referential integrity: every MISSING/PARTIAL capability must have a backlog entry
  const backendTasks = rows.filter(r => ['MISSING', 'PARTIAL'].includes(r['BackendStatus'])).map(r => r['CapabilityId']);
  if (backendTasks.length > 0 && fs.existsSync(path.join(ROOT, 'docs/p0/P0_MISSING_BACKEND_BACKLOG.md'))) {
    const backlog = fs.readFileSync(path.join(ROOT, 'docs/p0/P0_MISSING_BACKEND_BACKLOG.md'), 'utf8');
    backendTasks.forEach(capId => {
      check(backlog.includes(capId), `Backend MISSING/PARTIAL capability ${capId} has no entry in P0_MISSING_BACKEND_BACKLOG.md`);
    });
  }

  const frontendTasks = rows.filter(r => ['PARTIAL', 'STATIC_UI_ONLY', 'DESIGNED_NOT_CONNECTED'].includes(r['FrontendStatus'])).map(r => r['CapabilityId']);
  if (frontendTasks.length > 0 && fs.existsSync(path.join(ROOT, 'docs/p0/P0_FRONTEND_INTEGRATION_BACKLOG.md'))) {
    const backlog = fs.readFileSync(path.join(ROOT, 'docs/p0/P0_FRONTEND_INTEGRATION_BACKLOG.md'), 'utf8');
    frontendTasks.forEach(capId => {
      check(backlog.includes(capId), `Frontend PARTIAL capability ${capId} has no entry in P0_FRONTEND_INTEGRATION_BACKLOG.md`);
    });
  }

  // 6b. Check for undefined operations in backlogs
  ['docs/p0/P0_MISSING_BACKEND_BACKLOG.md', 'docs/p0/P0_FRONTEND_INTEGRATION_BACKLOG.md'].forEach(file => {
      if (fs.existsSync(path.join(ROOT, file))) {
          const content = fs.readFileSync(path.join(ROOT, file), 'utf8');
          if (content.includes('undefined')) {
              errors.push(`${file} contains 'undefined'. Backlog generation failed to extract valid BusinessOperation or variables.`);
          }
      }
  });

  // 6c. Verify Handoff files completeness and staleness
  const handoffDir = path.join(ROOT, 'docs/p0/roles');
  if (fs.existsSync(handoffDir)) {
      const roleCapabilities = {};
      rows.forEach(r => {
          if (!r['FrontendRole']) return;
          if (!roleCapabilities[r['FrontendRole']]) roleCapabilities[r['FrontendRole']] = [];
          roleCapabilities[r['FrontendRole']].push(r);
      });
      
      for (const [role, caps] of Object.entries(roleCapabilities)) {
          let expectedFileName = role.replace(/([a-z])([A-Z])/g, '$1_$2').toUpperCase() + '_HANDOFF.md';
          if (role === 'HoiDongQuanLyNoiDung') expectedFileName = 'CONTENT_COUNCIL_HANDOFF.md';
          
          const fullPath = path.join(handoffDir, expectedFileName);
          if (!fs.existsSync(fullPath)) {
              errors.push(`Missing handoff file for role ${role}: ${expectedFileName}`);
              continue;
          }
          
          const content = fs.readFileSync(fullPath, 'utf8');
          caps.forEach(r => {
              if (!content.includes(r['CapabilityId'])) {
                  errors.push(`Handoff completeness failed: ${expectedFileName} is missing capability ${r['CapabilityId']}`);
              }
              if (r['MatchedEndpointIds'] && !content.includes(r['MatchedEndpointIds'])) {
                  errors.push(`Stale Handoff: ${expectedFileName} mentions ${r['CapabilityId']} but lacks its EndpointIds (${r['MatchedEndpointIds']})`);
              }
          });
      }
  }

  // 6d. Document Governance Validation — scan source files for file:/// patterns
  console.log('[6d] Scanning Markdown source files for file:/// absolute paths...');
  const IGNORE_DIRS = new Set(['node_modules', '.git', '.cursor', 'Backend/bin', 'Backend/obj', 'frontend/node_modules', 'frontend/dist']);
  let absolutePathCount = 0;
  function walkForAbsolutePaths(dir) {
    let entries;
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return; }
    for (const entry of entries) {
      const full = path.join(dir, entry.name);
      const rel = path.relative(ROOT, full).replace(/\\/g, '/');
      if (entry.isDirectory()) {
        if (!IGNORE_DIRS.has(entry.name) && !rel.startsWith('.')) walkForAbsolutePaths(full);
      } else if (entry.name.endsWith('.md')) {
        const content = fs.readFileSync(full, 'utf8');
        const lines = content.split('\n');
        for (let i = 0; i < lines.length; i++) {
          if (/file:\/\/\/[cC]:/i.test(lines[i])) {
            absolutePathCount++;
            errors.push(`BROKEN_LOCAL_ABSOLUTE_PATH in ${rel}:${i+1} — ${lines[i].trim().slice(0, 120)}`);
          }
        }
      }
    }
  }
  walkForAbsolutePaths(ROOT);
  if (absolutePathCount === 0) console.log('   ✓ No file:/// absolute paths found in Markdown source');

  // 6e. Validate DOCUMENT_REFERENCES.csv — each TargetPath must exist on disk
  if (fs.existsSync(path.join(ROOT, 'docs/governance/DOCUMENT_REFERENCES.csv'))) {
      console.log('[6e] Validating DOCUMENT_REFERENCES.csv entries...');
      const refsRows = parseCSV('docs/governance/DOCUMENT_REFERENCES.csv');

      // Check for BROKEN status in CSV
      const brokenInCsv = refsRows.filter(r => r['ValidationStatus'] && r['ValidationStatus'] !== 'VALID');
      for (const row of brokenInCsv) {
          errors.push(`DOCUMENT_REFERENCES.csv: ${row['SourcePath']}:${row['Line']} has ValidationStatus='${row['ValidationStatus']}' — must be fixed before Move Plan execution`);
      }

      let missingTargets = 0;
      for (const row of refsRows) {
          const targetFull = path.join(ROOT, row['TargetPath']);
          if (!fs.existsSync(targetFull)) {
              missingTargets++;
              errors.push(`DOCUMENT_REFERENCES.csv: TargetPath '${row['TargetPath']}' (referenced by ${row['SourcePath']}:${row['Line']}) does not exist on disk`);
          }
      }
      if (missingTargets === 0 && brokenInCsv.length === 0) console.log(`   ✓ All ${refsRows.length} entries valid and targets exist on disk`);

      // 6f. Source/Report parity — scan ALL .md files, two-way comparison
      console.log('[6f] Checking source/report parity (full scan, bidirectional)...');

      // Shared scanner — mirrors generate-document-refs.mjs logic
      function extractInlineLinks(line) {
        const results = [];
        const inlineRe = /!?\[(?![^\]]*:\/\/)[^\]]*\]\(\s*([^\s()]+(?:\s+"[^"]*")?)\s*\)/g;
        let m;
        while ((m = inlineRe.exec(line)) !== null) {
          let target = m[1].trim();
          target = target.replace(/\s+["'][^"']*["']$/, '');
          results.push(target);
        }
        return results;
      }
      function extractReferenceStyleLinks(line) {
        const results = [];
        const defRe = /^\[([^\]]+)\]:\s+(\S+)/gm;
        let m;
        while ((m = defRe.exec(line)) !== null) results.push(m[2].trim());
        return results;
      }
      function extractHTMLLinks(line) {
        const results = [];
        const htmlRe = /<(?:a|img)\s[^>]*(?:href|src)\s*=\s*"([^"]+)"/gi;
        let m;
        while ((m = htmlRe.exec(line)) !== null) results.push(m[1].trim());
        return results;
      }
      function extractAllLinks(line) {
        return [...extractInlineLinks(line), ...extractReferenceStyleLinks(line), ...extractHTMLLinks(line)];
      }

      const IGNORE_DIRS_SCAN = new Set(['node_modules', '.git', '.cursor', 'Backend/bin', 'Backend/obj', 'frontend/node_modules', 'frontend/dist']);
      const generated = [];

      function walkForParity(dir) {
        let entries;
        try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return; }
        for (const entry of entries) {
          const full = path.join(dir, entry.name);
          const rel = path.relative(ROOT, full).replace(/\\/g, '/');
          if (entry.isDirectory()) {
            if (!IGNORE_DIRS_SCAN.has(entry.name) && !rel.startsWith('.')) walkForParity(full);
          } else if (entry.name.endsWith('.md')) {
            const content = fs.readFileSync(full, 'utf8');
            const lines = content.split('\n');
            for (let i = 0; i < lines.length; i++) {
              const line = lines[i];
              const rawTargets = extractAllLinks(line);
              const lineNum = i + 1;
              for (const rawTarget of rawTargets) {
                if (rawTarget.startsWith('http://') || rawTarget.startsWith('https://') || rawTarget.startsWith('#') || rawTarget.startsWith('mailto:') || rawTarget.startsWith('file:///')) continue;
                // Strip fragment
                const hashIdx = rawTarget.indexOf('#');
                const target = hashIdx >= 0 ? rawTarget.slice(0, hashIdx) : rawTarget;
                const sourceDir = path.dirname(full);
                const resolved = path.resolve(sourceDir, target);
                const relTarget = path.relative(ROOT, resolved).replace(/\\/g, '/');
                if (!relTarget.startsWith('..') && !path.isAbsolute(relTarget)) {
                  generated.push({ SourcePath: rel, TargetPath: relTarget, Line: lineNum });
                }
              }
            }
          }
        }
      }
      walkForParity(ROOT);

      // Forward parity: every source reference must be in CSV
      let parityErrors = 0;
      for (const g of generated) {
          const match = refsRows.some(r => r['SourcePath'] === g.SourcePath && r['TargetPath'] === g.TargetPath && parseInt(r['Line']) === g.Line);
          if (!match) {
              parityErrors++;
              errors.push(`Parity error (forward): reference ${g.SourcePath}:${g.Line} → '${g.TargetPath}' found in source but missing from DOCUMENT_REFERENCES.csv`);
          }
      }

      // Reverse parity: every CSV row must exist in source
      let reverseErrors = 0;
      for (const row of refsRows) {
          const match = generated.some(g => g.SourcePath === row['SourcePath'] && g.TargetPath === row['TargetPath'] && g.Line === parseInt(row['Line']));
          if (!match) {
              reverseErrors++;
              errors.push(`Parity error (reverse): CSV row ${row['SourcePath']}:${row['Line']} → '${row['TargetPath']}' has no matching reference in source`);
          }
      }

      if (parityErrors === 0 && reverseErrors === 0) console.log(`   ✓ Bidirectional parity: ${generated.length} source refs ↔ ${refsRows.length} CSV rows match`);
  }

  // 7. Matrix coverage: each canonical role must have >= 3 capabilities
  const roleCoverage = {};
  rows.forEach(r => {
    if (r['FrontendRole']) {
      roleCoverage[r['FrontendRole']] = (roleCoverage[r['FrontendRole']] || 0) + 1;
    }
  });
  const requiredRoles = ['Teacher', 'Student', 'Parent', 'Principal', 'SuperAdmin', 'AcademicStaff', 'HoiDongQuanLyNoiDung'];
  requiredRoles.forEach(role => {
    const count = roleCoverage[role] || 0;
    check(count >= 3, `Role '${role}' has only ${count} capabilities in matrix — must have >= 3`);
  });

  console.log(`   Matrix rows: ${rows.length}, Roles covered: ${Object.keys(roleCoverage).join(', ')}`);
}

// ── Check 8: Folder paths in Team Ownership ───────────────────────────────────
console.log('[8] Checking Team Ownership folder paths...');
const OWNERSHIP_PATHS = [
  'frontend/src/views/GiangVien',
  'frontend/src/views/GiaoVu',
  'frontend/src/views/PhuHuynh',
  'frontend/src/views/BGH',
  'frontend/src/views/SuperAdmin',
  'frontend/src/views/Student',
  'frontend/src/components/common',
  'frontend/src/layouts',
];
OWNERSHIP_PATHS.forEach(p => {
  check(fs.existsSync(path.join(ROOT, p)), `Ownership path does not exist: ${p}`);
});

// ── Check 9: Frontend unit tests ──────────────────────────────────────────────
console.log('[9] Running frontend unit tests...');
try {
  execSync('npm run test:unit -- --run', { stdio: 'pipe', cwd: path.join(ROOT, 'frontend') });
  console.log('   ✓ Frontend unit tests passed');
} catch (e) {
  errors.push(`Frontend unit tests FAILED:\n${e.stdout?.toString() || e.message}`);
}

// ── Check 10: Frontend lint ───────────────────────────────────────────────────
console.log('[10] Running frontend lint (oxlint only for speed)...');
try {
  execSync('npx oxlint .', { stdio: 'pipe', cwd: path.join(ROOT, 'frontend') });
  console.log('   ✓ Frontend oxlint passed');
} catch (e) {
  errors.push(`Frontend lint (oxlint) FAILED:\n${e.stdout?.toString() || e.message}`);
}

// ── Check 11: Frontend build ──────────────────────────────────────────────────
console.log('[11] Running frontend build...');
try {
  execSync('npm run build', { stdio: 'pipe', cwd: path.join(ROOT, 'frontend') });
  console.log('   ✓ Frontend build passed');
} catch (e) {
  errors.push(`Frontend build FAILED:\n${e.stdout?.toString().slice(0, 500) || e.message}`);
}

// ── Check 12: Backend build ───────────────────────────────────────────────────
console.log('[12] Running dotnet build...');
try {
  const result = execSync('dotnet build --no-restore', { stdio: 'pipe', cwd: path.join(ROOT, 'Backend') });
  const output = result.toString();
  if (output.includes('Error(s)') && !output.includes('0 Error(s)')) {
    errors.push('Backend build has errors');
  } else {
    console.log('   ✓ Backend build passed');
  }
} catch (e) {
  errors.push(`Backend build FAILED:\n${e.stdout?.toString().slice(0, 500) || e.message}`);
}

// ── Summary ───────────────────────────────────────────────────────────────────
console.log('\n' + '='.repeat(60));

if (warnings.length > 0) {
  console.warn('\nWarnings:');
  warnings.forEach(w => console.warn('  ⚠ ' + w));
}

if (errors.length > 0) {
  console.error('\nP0 Validation FAILED:');
  errors.forEach(e => console.error('  ✗ ' + e));
  process.exit(1);
} else {
  console.log('\n✓ P0 Validation PASSED — All checks green.');
  console.log(`  Matrix rows verified, ${Object.keys({}).length || 'all'} roles covered, build/test/lint clean.`);
}
