import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.P16B2_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.P16B2_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'))
const RESULT_PATH = path.join(ARTIFACT_DIR, 'p16b2-superadmin-results.json')

const routes = [
  '/super-admin/profile',
  '/super-admin/training/semesters',
  '/super-admin/training/programs',
  '/super-admin/training/subjects',
  '/super-admin/training/courses',
  '/super-admin/training/exam-periods',
  '/super-admin/operations/attendance-policy',
  '/super-admin/operations/registration-periods',
  '/super-admin/operations/pass-fail-rules',
  '/super-admin/support/tickets',
  '/super-admin/support/faq',
  '/super-admin/approvals/history',
  '/super-admin/evaluations/config',
  '/super-admin/evaluations/results',
  '/super-admin/reports/education-overview',
  '/super-admin/audit/logs',
  '/super-admin/security/alerts',
  '/super-admin/system/modules',
  '/super-admin/finance/tuition-config',
]

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

async function newTab() {
  let res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`, { method: 'PUT' })
  if (!res.ok) res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`)
  if (!res.ok) throw new Error(`Cannot create CDP tab: ${res.status}`)
  return res.json()
}

async function createClient(wsUrl) {
  const ws = new WebSocket(wsUrl)
  await new Promise((resolve, reject) => {
    ws.addEventListener('open', resolve, { once: true })
    ws.addEventListener('error', reject, { once: true })
  })

  let id = 0
  const pending = new Map()
  const events = []

  ws.addEventListener('message', event => {
    const message = JSON.parse(event.data)
    if (message.id && pending.has(message.id)) {
      const { resolve, reject } = pending.get(message.id)
      pending.delete(message.id)
      if (message.error) reject(new Error(message.error.message))
      else resolve(message.result)
      return
    }
    events.push(message)
  })

  function send(method, params = {}) {
    const callId = ++id
    ws.send(JSON.stringify({ id: callId, method, params }))
    return new Promise((resolve, reject) => {
      pending.set(callId, { resolve, reject })
    })
  }

  return { ws, send, events }
}

async function evaluate(send, expression) {
  const result = await send('Runtime.evaluate', {
    expression,
    awaitPromise: true,
    returnByValue: true,
  })
  if (result.exceptionDetails) {
    const description = result.exceptionDetails.exception?.description || result.exceptionDetails.text
    throw new Error(description)
  }
  return result.result?.value
}

async function login(send) {
  return evaluate(send, `
    (async () => {
      localStorage.clear();
      sessionStorage.clear();
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          usernameOrEmail: 'superadmin@lms.local',
          email: 'superadmin@lms.local',
          password: '123456'
        })
      });
      const data = await res.json().catch(() => ({}));
      const token = data.accessToken || data.token || data?.data?.accessToken || data?.Data?.AccessToken;
      if (!res.ok || !token) throw new Error('login failed ' + res.status);
      const user = data.user || data?.data?.user || data?.Data?.User || {};
      const expiresAt = data.expiresAt || data.ExpiresAt || data?.data?.expiresAt || data?.Data?.ExpiresAt || new Date(Date.now() + 3600000).toISOString();
      const refreshTokenExpiresAt = data.refreshTokenExpiresAt || data.RefreshTokenExpiresAt || data?.data?.refreshTokenExpiresAt || data?.Data?.RefreshTokenExpiresAt || expiresAt;
      localStorage.setItem('lms_access_token', token);
      sessionStorage.setItem('lms_access_token', token);
      localStorage.setItem('lms_refresh_token', data.refreshToken || data.RefreshToken || data?.data?.refreshToken || data?.Data?.RefreshToken || '');
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_token_expires_at', expiresAt);
      sessionStorage.setItem('lms_token_expires_at', expiresAt);
      localStorage.setItem('lms_refresh_token_expires_at', refreshTokenExpiresAt);
      sessionStorage.setItem('lms_refresh_token_expires_at', refreshTokenExpiresAt);
      localStorage.setItem('lms_requires_password_change', 'false');
      sessionStorage.setItem('lms_requires_password_change', 'false');
      return true;
    })()
  `)
}

function summarizeNetwork(events) {
  const apiResponses = events
    .filter(event => event.method === 'Network.responseReceived')
    .map(event => event.params.response)
    .filter(response => response.url.includes('/api/'))

  return apiResponses
    .filter(response => response.status >= 400)
    .map(response => ({ status: response.status, url: response.url }))
}

function summarizeConsole(events) {
  const consoleErrors = events
    .filter(event => event.method === 'Runtime.consoleAPICalled' && event.params.type === 'error')
    .map(event => event.params.args?.map(arg => arg.value || arg.description || '').join(' '))
    .filter(Boolean)

  const runtimeExceptions = events
    .filter(event => event.method === 'Runtime.exceptionThrown')
    .map(event => event.params.exceptionDetails?.exception?.description || event.params.exceptionDetails?.text || '')
    .filter(Boolean)

  return { consoleErrors, runtimeExceptions }
}

async function probePage(send) {
  return evaluate(send, `
    (() => {
      const text = document.body?.innerText || '';
      return {
        path: location.pathname,
        hasRoot: Boolean(document.querySelector('#app')),
        hasVisibleText: text.trim().length > 0,
        bodyText: text.slice(0, 500)
      };
    })()
  `)
}

async function waitForPage(send, timeoutMs = 5000) {
  const deadline = Date.now() + timeoutMs
  let latest = null
  while (Date.now() < deadline) {
    latest = await probePage(send)
    if (latest.hasRoot && latest.hasVisibleText) return latest
    await delay(250)
  }
  return latest
}

async function main() {
  const version = await fetch(`${CDP}/json/version`).then(res => res.json())
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)

  await send('Page.enable')
  await send('Runtime.enable')
  await send('Network.enable')
  await send('Page.navigate', { url: FRONTEND })
  await delay(1000)
  await login(send)

  const results = []

  for (const route of routes) {
    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${route}` })
    const pageProbe = await waitForPage(send)
    const seen = events.slice(start)
    const apiErrors = summarizeNetwork(seen)
    const consoleSummary = summarizeConsole(seen)
    const pass =
      pageProbe?.hasRoot === true &&
      pageProbe?.hasVisibleText === true &&
      pageProbe?.path === route &&
      apiErrors.length === 0 &&
      consoleSummary.consoleErrors.length === 0 &&
      consoleSummary.runtimeExceptions.length === 0

    results.push({
      role: 'SuperAdmin',
      route,
      result: pass ? 'PASS' : 'FAIL',
      pageLoaded: Boolean(pageProbe?.hasRoot && pageProbe?.hasVisibleText),
      apiErrors,
      consoleErrors: consoleSummary.consoleErrors,
      runtimeExceptions: consoleSummary.runtimeExceptions,
      pageProbe,
    })
  }

  ws.close()

  const output = {
    generatedAt: new Date().toISOString(),
    browser: version.Browser,
    frontend: FRONTEND,
    cdp: CDP,
    routeCount: results.length,
    passCount: results.filter(result => result.result === 'PASS').length,
    failCount: results.filter(result => result.result === 'FAIL').length,
    consoleErrors: results.reduce((sum, result) => sum + result.consoleErrors.length, 0),
    runtimeExceptions: results.reduce((sum, result) => sum + result.runtimeExceptions.length, 0),
    network401: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.status === 401).length, 0),
    network403: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.status === 403).length, 0),
    network404: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.status === 404).length, 0),
    network500: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.status >= 500).length, 0),
    results,
  }

  fs.writeFileSync(RESULT_PATH, `${JSON.stringify(output, null, 2)}\n`)
  console.log(JSON.stringify(output, null, 2))
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
