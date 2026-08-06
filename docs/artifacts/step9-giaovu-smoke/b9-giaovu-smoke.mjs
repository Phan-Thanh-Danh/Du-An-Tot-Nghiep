import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.B9_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.B9_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = 'C:\\Users\\maita\\OneDrive\\Máy tính\\Du-An-Tot-Nghiep\\docs\\artifacts\\step9-giaovu-smoke'
const RESULT_PATH = path.join(ARTIFACT_DIR, 'smoke-results-b9.json')

const STAFF = { email: 'p12test_staff01@lms.local', password: 'Test@123' }

const routes = [
  { route: '/staff/dashboard', note: 'Dashboard GiaoVu (staffApi.dashboard)' },
  { route: '/staff/conflicts', note: 'ConflictCheckView - conflict detection client-side' },
  { route: '/staff/profile', note: 'StaffProfileView - account/me, profile, change-password' },
  { route: '/staff/schedule/published', note: 'StaffPublishedSchedulesView - published list + cancel' },
  { route: '/staff/rooms', note: 'RoomManagementView - master-data rooms' },
  { route: '/staff/schedule', note: 'ScheduleManagerView - schedule CRUD' },
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

async function login(send, account) {
  return evaluate(send, `
    (async () => {
      localStorage.clear();
      sessionStorage.clear();
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          usernameOrEmail: ${JSON.stringify(account.email)},
          email: ${JSON.stringify(account.email)},
          password: ${JSON.stringify(account.password)}
        })
      });
      const data = await res.json().catch(() => ({}));
      const token = data.accessToken || data.token || data?.data?.accessToken || data?.Data?.AccessToken;
      if (!res.ok || !token) throw new Error('login failed ' + res.status + ' for ' + ${JSON.stringify(account.email)});
      const user = data.user || data?.data?.user || data?.Data?.User || {};
      localStorage.setItem('lms_access_token', token);
      sessionStorage.setItem('lms_access_token', token);
      localStorage.setItem('lms_refresh_token', data.refreshToken || '');
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_token_expires_at', data.expiresAt || '');
      localStorage.setItem('lms_requires_password_change', 'false');
      return { ok: true, role: user.role || user.vaiTroChinh || user.VaiTroChinh || '' };
    })()
  `)
}

function collectEvents(events, start) {
  return events.slice(start)
}

function summarizeNetwork(events) {
  const apiResponses = events
    .filter(event => event.method === 'Network.responseReceived')
    .map(event => event.params.response)
    .filter(response => response.url.includes('/api/'))

  return {
    apiErrors: apiResponses
      .filter(response => response.status >= 400)
      .map(response => ({ status: response.status, url: response.url })),
  }
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
      const hasRoot = Boolean(document.querySelector('#app'));
      const loadingText = /dang tai|loading/i.test(text);
      const hasVisibleText = text.trim().length > 0;
      return { title: document.title || '', hasRoot, hasVisibleText, loadingText, path: location.pathname };
    })()
  `)
}

async function waitForPageProbe(send, timeoutMs) {
  const deadline = Date.now() + timeoutMs
  let latest = null

  while (Date.now() < deadline) {
    latest = await probePage(send).catch(error => ({
      hasRoot: false,
      hasVisibleText: false,
      loadingText: false,
      probeError: error.message,
    }))

    if (latest.hasRoot === true && latest.hasVisibleText === true && latest.loadingText !== true) {
      return latest
    }

    await delay(250)
  }

  return latest || {
    hasRoot: false,
    hasVisibleText: false,
    loadingText: false,
    probeError: 'page probe timed out',
  }
}

async function main() {
  const version = await fetch(`${CDP}/json/version`).then(res => res.json())
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)

  await send('Page.enable')
  await send('Runtime.enable')
  await send('Network.enable')
  await send('Log.enable')
  await send('Security.enable')
  await send('Security.setIgnoreCertificateErrors', { ignore: true })
  await send('Page.navigate', { url: FRONTEND })
  await delay(1500)

  await login(send, STAFF)
  await delay(800)

  const results = []

  for (const item of routes) {
    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${item.route}` })
    const pageProbe = await waitForPageProbe(send, 8000)

    const seen = collectEvents(events, start)
    const network = summarizeNetwork(seen)
    const consoleSummary = summarizeConsole(seen)

    const pass =
      pageProbe.hasRoot === true &&
      pageProbe.hasVisibleText === true &&
      network.apiErrors.length === 0 &&
      consoleSummary.consoleErrors.length === 0 &&
      consoleSummary.runtimeExceptions.length === 0

    results.push({
      route: item.route,
      note: item.note,
      pageLoaded: Boolean(pageProbe.hasRoot && pageProbe.hasVisibleText),
      apiErrors: network.apiErrors.map(error => `${error.status} ${error.url}`),
      consoleErrors: consoleSummary.consoleErrors,
      runtimeExceptions: consoleSummary.runtimeExceptions,
      result: pass ? 'PASS' : 'FAIL',
      pageProbe,
    })
    console.log(`[B9] ${item.route} -> ${pass ? 'PASS' : 'FAIL'}`)
    if (!pass) {
      console.log(JSON.stringify(results[results.length - 1], null, 2))
    }
  }

  ws.close()

  const totals = {
    routes: results.length,
    passed: results.filter(result => result.result === 'PASS').length,
    failed: results.filter(result => result.result === 'FAIL').length,
    consoleErrors: results.reduce((sum, result) => sum + result.consoleErrors.length, 0),
    runtimeExceptions: results.reduce((sum, result) => sum + result.runtimeExceptions.length, 0),
    network401: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.startsWith('401 ')).length, 0),
    network403: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.startsWith('403 ')).length, 0),
    network404: results.reduce((sum, result) => sum + result.apiErrors.filter(error => error.startsWith('404 ')).length, 0),
    network500: results.reduce((sum, result) => sum + result.apiErrors.filter(error => /^5\d\d /.test(error)).length, 0),
  }

  const output = {
    generatedAt: new Date().toISOString(),
    frontend: FRONTEND,
    cdp: CDP,
    browser: version.Browser,
    account: STAFF.email,
    totals,
    results,
  }

  fs.writeFileSync(RESULT_PATH, `${JSON.stringify(output, null, 2)}\n`)
  console.log(JSON.stringify({ totals }, null, 2))
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
