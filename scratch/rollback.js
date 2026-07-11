const fs = require('fs');
const path = require('path');

const rootDir = 'C:/Users/lapto/OneDrive/Desktop/Du-An-Tot-Nghiep';
const movePlanPath = path.join(rootDir, 'docs/governance/DOCUMENT_MOVE_PLAN.csv');

if (fs.existsSync(movePlanPath)) {
    const lines = fs.readFileSync(movePlanPath, 'utf8').split('\n').slice(1).filter(l => l.trim() !== '');
    let rolledBack = 0;
    
    for (let line of lines) {
        let inQuotes = false;
        let current = '';
        const cols = [];
        for (let char of line) {
            if (char === '"') inQuotes = !inQuotes;
            else if (char === ',' && !inQuotes) {
                cols.push(current);
                current = '';
            } else {
                current += char;
            }
        }
        cols.push(current);
        
        const currentPath = cols[1];
        const proposedPath = cols[2];
        const action = cols[5];

        if (action === 'MOVE' && currentPath !== proposedPath) {
            const absCurrent = path.join(rootDir, currentPath);
            const absProposed = path.join(rootDir, proposedPath);
            
            // Revert from proposedPath to currentPath
            if (fs.existsSync(absProposed)) {
                const targetDir = path.dirname(absCurrent);
                if (!fs.existsSync(targetDir)) {
                    fs.mkdirSync(targetDir, { recursive: true });
                }
                try {
                    fs.renameSync(absProposed, absCurrent);
                    rolledBack++;
                } catch(e) {
                    console.error("Error rolling back:", e.message);
                }
            }
        }
    }
    console.log(`Rolled back ${rolledBack} files.`);
} else {
    console.log("No move plan found.");
}
