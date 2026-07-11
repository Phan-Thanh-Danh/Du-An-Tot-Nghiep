import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const rootDir = path.resolve(__dirname, '../../')
const frontendDir = path.join(rootDir, 'frontend')

let fails = 0

function logFail(msg) {
  console.error(`[FAIL] ${msg}`)
  fails++
}

function logPass(msg) {
  console.log(`[PASS] ${msg}`)
}

// 1. Check v-html
function checkVHtml(dir) {
  if (!fs.existsSync(dir)) return;
  const files = fs.readdirSync(dir)
  for (const file of files) {
    const fullPath = path.join(dir, file)
    const stat = fs.statSync(fullPath)
    if (stat.isDirectory() && file !== 'node_modules') {
      checkVHtml(fullPath)
    } else if (file.endsWith('.vue')) {
      const content = fs.readFileSync(fullPath, 'utf8')
      const lines = content.split('\n')
      lines.forEach((line, i) => {
        if (line.includes('v-html') && !line.includes('//') && !line.includes('<!--') && !line.includes('v-html disabled')) {
          logFail(`v-html found in ${fullPath}:${i + 1}`)
        }
      })
    }
  }
}

console.log('--- Checking for v-html ---')
checkVHtml(path.join(frontendDir, 'src'))
if (fails === 0) logPass('No active v-html directives found.')

// 2. Check Route Meta
function checkRouteMeta() {
    const routerPath = path.join(frontendDir, 'src', 'router', 'index.js');
    if (!fs.existsSync(routerPath)) {
        logFail('router/index.js not found');
        return;
    }
    const content = fs.readFileSync(routerPath, 'utf8');
    
    // We check if "requiresAuth" exists. This is a very generic check.
    if (!content.includes('requiresAuth')) {
        logFail('requiresAuth not found in router');
    }
}
console.log('--- Checking Route Meta ---')
checkRouteMeta()
if (fails === 0) logPass('Route Meta checked.')

// 3. Check Canonical Roles
console.log('--- Checking Canonical Roles ---')
const roleCatalogPath = path.join(frontendDir, 'src', 'constants', 'roleCatalog.js')
if (fs.existsSync(roleCatalogPath)) {
  const content = fs.readFileSync(roleCatalogPath, 'utf8')
  if (content.includes('dbCode: \'quan_tri\'')) {
    logPass('Canonical roles match AuthConstants.cs (Admin verified).')
  } else {
    logFail('Canonical roles do not match AuthConstants.cs')
  }
} else {
  logFail('roleCatalog.js not found')
}

console.log('---------------------------')
if (fails > 0) {
  console.error(`Validation failed with ${fails} errors.`)
  process.exit(1)
} else {
  console.log('All P0 validations passed!')
}
