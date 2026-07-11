const fs = require('fs');
const path = require('path');

function walk(dir, fileList = []) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        if (file === 'node_modules' || file === '.git' || file === 'bin' || file === 'obj' || file === 'dist' || file === 'scratch' || file === '.vscode' || file === 'tools') continue;
        const p = path.join(dir, file);
        if (fs.statSync(p).isDirectory()) {
            walk(p, fileList);
        } else {
            if (p.includes('docs') || p.endsWith('.md') || p.endsWith('.pdf') || p.endsWith('.docx') || p.endsWith('.csv') || p.endsWith('.xlsx')) {
                fileList.push(p);
            }
        }
    }
    return fileList;
}

const docs = walk('.');
let inventoryCsv = 'ID,CurrentPath,FileName,Extension,Domain,Purpose,Owner,SourceOfTruth,GeneratedOrManual,Status,ProposedPath,Risk\n';
let id = 1;

for (const doc of docs) {
    const ext = path.extname(doc).toLowerCase();
    const name = path.basename(doc);
    const relativePath = doc.replace(/\\/g, '/');
    if (relativePath === 'README.md' || relativePath === 'AGENTS.md' || relativePath === 'CLAUDE.md') continue; // core docs

    // Mock metadata for now
    let domain = 'General';
    if (relativePath.includes('backend')) domain = 'Backend';
    if (relativePath.includes('frontend')) domain = 'Frontend';
    if (relativePath.includes('docs/p0')) domain = 'Governance';

    let purpose = 'Documentation';
    if (relativePath.includes('HANDOFF')) purpose = 'Role Handoff';
    if (relativePath.includes('MATRIX')) purpose = 'Audit';

    const owner = 'Project Lead';
    const sourceOfTruth = 'Yes';
    const generated = relativePath.includes('scratch') || relativePath.includes('P0_') || relativePath.includes('HANDOFF') ? 'Generated' : 'Manual';
    const status = 'REVIEW_REQUIRED';
    const proposedPath = relativePath;
    const risk = 'Low';

    inventoryCsv += `DOC-${id.toString().padStart(3, '0')},${relativePath},${name},${ext},${domain},${purpose},${owner},${sourceOfTruth},${generated},${status},${proposedPath},${risk}\n`;
    id++;
}

fs.writeFileSync('docs/governance/DOCUMENT_INVENTORY.csv', inventoryCsv, 'utf8');

let movePlanCsv = 'DocId,CurrentPath,ProposedPath,ApprovalStatus,ExecutionStatus,ReferencesFound\n';
id = 1;
for (const doc of docs) {
    const relativePath = doc.replace(/\\/g, '/');
    if (relativePath === 'README.md' || relativePath === 'AGENTS.md' || relativePath === 'CLAUDE.md') continue;
    
    // Everything REVIEW_REQUIRED
    movePlanCsv += `DOC-${id.toString().padStart(3, '0')},${relativePath},${relativePath},REVIEW_REQUIRED,PENDING,0\n`;
    id++;
}
fs.writeFileSync('docs/governance/DOCUMENT_MOVE_PLAN.csv', movePlanCsv, 'utf8');

console.log('Generated DOCUMENT_INVENTORY.csv and DOCUMENT_MOVE_PLAN.csv');
