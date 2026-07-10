import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const RESULTS_FILE = path.join(__dirname, 'p16b5-results.json');

let results = [];
if (fs.existsSync(RESULTS_FILE)) {
    results = JSON.parse(fs.readFileSync(RESULTS_FILE, 'utf-8'));
}

export function logAction(route, role, action, selector, expectedApi, networkCalls, consoleErrors, runtimeErrors, result, notes) {
    const entry = {
        route,
        role,
        action,
        selector,
        expectedApi,
        networkCalls,
        consoleErrors,
        runtimeErrors,
        result,
        notes
    };
    results.push(entry);
    fs.writeFileSync(RESULTS_FILE, JSON.stringify(results, null, 2));
    console.log(`[LOG] Recorded ${action} on ${route} -> ${result}`);
}
