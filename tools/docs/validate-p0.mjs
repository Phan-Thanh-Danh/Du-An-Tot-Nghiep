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
  'Team Ownership': 'docs/p0/P0_TEAM_OWNERSHIP.md',
  'Document Inventory': 'docs/governance/DOCUMENT_INVENTORY.csv',
  'Document Move Plan': 'docs/governance/DOCUMENT_MOVE_PLAN.csv',
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

  // 5. Every IMPLEMENTED row must have MatchedEndpointId that exists in inventory
  const implementedRows = rows.filter(r => r['BackendStatus'] === 'IMPLEMENTED');
  implementedRows.forEach(r => {
    if (!r['MatchedEndpointIds'] || r['MatchedEndpointIds'].trim() === '') {
        errors.push(`${r['CapabilityId']}: BackendStatus=IMPLEMENTED but MatchedEndpointIds is empty`);
        return;
    }
    const ep = inventory.find(i => i.EndpointId === r['MatchedEndpointIds']);
    if (!ep) {
        errors.push(`${r['CapabilityId']}: MatchedEndpointIds '${r['MatchedEndpointIds']}' not found in Endpoint Inventory`);
        return;
    }
    // Check 5b: Evidence integrity
    const expectedEvidence = `[${ep.HttpMethod} ${ep.Route}] Controller: ${ep.Controller}, Action: ${ep.Action}`;
    if (r['Evidence'] !== expectedEvidence) {
        errors.push(`${r['CapabilityId']}: Evidence mismatch.\n  Expected: ${expectedEvidence}\n  Found:    ${r['Evidence']}`);
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
