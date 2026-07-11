import fs from 'fs';
import { execSync } from 'child_process';

let errors = [];

function checkFile(path, name) {
    if (!fs.existsSync(path)) {
        errors.push(`Missing file: ${name} (${path})`);
        return false;
    }
    return true;
}

// 1. Check basic P0 files
checkFile('docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv', 'Endpoint Inventory');
checkFile('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv', 'Capability Matrix');
checkFile('docs/p0/P0_MISSING_BACKEND_BACKLOG.md', 'Missing Backlog');
checkFile('docs/governance/DOCUMENT_INVENTORY.csv', 'Document Inventory');
checkFile('docs/governance/DOCUMENT_MOVE_PLAN.csv', 'Document Move Plan');

// 2. Validate capability matrix content
if (checkFile('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv', 'Capability Matrix')) {
    const csv = fs.readFileSync('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv', 'utf8');
    const lines = csv.split('\n').filter(Boolean).slice(1);
    let hasPartialOrMissing = false;
    for (const line of lines) {
        const parts = line.split(',');
        const beStatus = parts[3];
        if (beStatus === 'PARTIAL' || beStatus === 'MISSING') hasPartialOrMissing = true;
    }
    if (!hasPartialOrMissing) {
        errors.push("Capability matrix claims 100% IMPLEMENTED backend, which is a false semantic truth. At least one capability must be PARTIAL or MISSING in P0.");
    }
}

// 3. Validate missing backlog doesn't contain false tasks
if (checkFile('docs/p0/P0_MISSING_BACKEND_BACKLOG.md', 'Missing Backlog')) {
    const content = fs.readFileSync('docs/p0/P0_MISSING_BACKEND_BACKLOG.md', 'utf8');
    if (content.includes('Teacher bulk attendance') || content.includes('lacks bulk POST action')) {
        errors.push("Missing backlog contains a false claim: 'Teacher bulk attendance' is actually implemented.");
    }
}

// 4. Test safe HTML renderer mitigation
console.log('Running SafeHtmlRenderer unit tests...');
try {
    execSync('npm run test:unit -- --run src/components/common/__tests__/SafeHtmlRenderer.spec.js', { stdio: 'inherit', cwd: 'frontend' });
} catch (e) {
    errors.push('SafeHtmlRenderer unit tests failed.');
}

// 5. Test canonical roles
console.log('Running roleCatalog unit tests...');
try {
    execSync('npm run test:unit -- --run src/constants/__tests__/roleCatalog.spec.js', { stdio: 'inherit', cwd: 'frontend' });
} catch (e) {
    errors.push('roleCatalog unit tests failed.');
}

// 6. Test Backend Build
console.log('Running dotnet build...');
try {
    execSync('dotnet build', { stdio: 'inherit', cwd: 'Backend' });
} catch (e) {
    errors.push('dotnet build failed.');
}

if (errors.length > 0) {
    console.error('P0 Validation Failed with the following errors:');
    errors.forEach(e => console.error('- ' + e));
    process.exit(1);
} else {
    console.log('P0 Validation Passed! Semantic truth and security constraints verified.');
}
