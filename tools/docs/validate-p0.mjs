import fs from 'fs';
import path from 'path';

let fails = 0;
let endpointsParsed = 0;
let capabilitiesParsed = 0;

function logFail(msg) {
    console.error(`[FAIL] ${msg}`);
    fails++;
}
function logPass(msg) {
    console.log(`[PASS] ${msg}`);
}

const p0Dir = path.join(process.cwd(), 'docs/p0');

// 1. Endpoint Inventory
const invPath = path.join(p0Dir, 'P0_BACKEND_ENDPOINT_INVENTORY.csv');
if (!fs.existsSync(invPath)) {
    logFail('P0_BACKEND_ENDPOINT_INVENTORY.csv missing');
} else {
    const lines = fs.readFileSync(invPath, 'utf8').split('\n').filter(l => l.trim());
    endpointsParsed = lines.length - 1;
    const epIds = new Set();
    
    for (let i = 1; i < lines.length; i++) {
        // Simple CSV parse
        const parts = lines[i].split(',');
        const id = parts[0];
        const method = parts[3];
        const route = parts[4];
        
        if (epIds.has(id)) logFail(`Duplicate Endpoint ID: ${id}`);
        epIds.add(id);
        
        if (!['GET', 'POST', 'PUT', 'DELETE', 'PATCH'].includes(method)) logFail(`Invalid HTTP method: ${method} for ${id}`);
        if (!route.startsWith('/api/')) logFail(`Endpoint does not begin with /api/: ${route} for ${id}`);
    }
}

// 2. Capability Matrix
const capPath = path.join(p0Dir, 'P0_BACKEND_CAPABILITY_MATRIX.csv');
if (!fs.existsSync(capPath)) {
    logFail('P0_BACKEND_CAPABILITY_MATRIX.csv missing');
} else {
    const lines = fs.readFileSync(capPath, 'utf8').split('\n').filter(l => l.trim());
    capabilitiesParsed = lines.length - 1;
    const capIds = new Set();
    
    for (let i = 1; i < lines.length; i++) {
        // Regex to split by comma outside quotes
        const parts = lines[i].match(/(".*?"|[^",\s]+)(?=\s*,|\s*$)/g);
        if (!parts) continue;
        
        const id = parts[0];
        const beStatus = parts[3];
        const feStatus = parts[4];
        const role = parts[2];
        const evidence = parts[6] ? parts[6].replace(/(^"|"$)/g, '') : '';
        
        if (capIds.has(id)) logFail(`Duplicate Capability ID: ${id}`);
        capIds.add(id);
        
        const validStatuses = ['IMPLEMENTED', 'PARTIAL', 'MISSING', 'UNVERIFIED'];
        if (!validStatuses.includes(beStatus)) logFail(`Invalid BackendStatus: ${beStatus} for ${id}`);
        if (!validStatuses.includes(feStatus)) logFail(`Invalid FrontendStatus: ${feStatus} for ${id}`);
        
        if (beStatus === 'IMPLEMENTED') {
            if (!evidence.includes('Controller:') || (!evidence.includes('ServiceMethod:') && !evidence.includes('DIRECT_DB_CONTEXT'))) {
                logFail(`IMPLEMENTED capability ${id} missing required evidence (Controller/ServiceMethod)`);
            }
        }
        
        if (beStatus === 'PARTIAL' && !evidence.toLowerCase().includes('incomplete')) {
            logFail(`PARTIAL capability ${id} does not explain missing pieces in evidence`);
        }
    }
}

// 3. Security
const specPath = path.join(process.cwd(), 'frontend/src/components/common/__tests__/SafeHtmlRenderer.spec.js');
if (!fs.existsSync(specPath)) {
    logFail('Security tests for SafeHtmlRenderer missing');
}

// 4. Role Handoffs
const rolesDir = path.join(p0Dir, 'roles');
if (fs.existsSync(rolesDir)) {
    const roles = fs.readdirSync(rolesDir).filter(f => f.endsWith('_HANDOFF.md'));
    if (roles.length < 7) logFail(`Expected 7 role handoffs, found ${roles.length}`);
    roles.forEach(role => {
        const content = fs.readFileSync(path.join(rolesDir, role), 'utf8');
        if (!content.includes('Exact folder ownership')) logFail(`Handoff ${role} missing ownership`);
        if (!content.includes('Actual home route')) logFail(`Handoff ${role} missing home route`);
        if (!content.includes('Definition of Done')) logFail(`Handoff ${role} missing Definition of Done`);
    });
} else {
    logFail('roles/ directory missing');
}

console.log('--- P0 Validation Summary ---');
console.log(`Endpoints parsed: ${endpointsParsed}`);
console.log(`Capabilities parsed: ${capabilitiesParsed}`);
if (fails > 0) {
    console.error(`Validation failed with ${fails} errors.`);
    process.exit(1);
} else {
    console.log('All validations passed!');
}
