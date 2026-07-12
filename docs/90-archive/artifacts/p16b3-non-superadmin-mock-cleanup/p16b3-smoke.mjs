import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.P16B3_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.P16B3_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'))
const RESULT_PATH = path.join(ARTIFACT_DIR, 'p16b3-results.json')

const accounts = {
  staff: { email: 'p12test_staff01@lms.local', password: 'Test@123' },
  teacher: { email: 'p12test_teacher01@lms.local', password: 'Test@123' },
  student: { email: 'p12test_student011@lms.local', password: 'Test@123' },
  content: { email: 'p15test_content01@lms.local', password: 'Test@123' },
}

const routes = [
  { role: 'staff', route: '/staff/accounts' },
  { role: 'teacher', route: '/teacher/class-attendance' },
  { role: 'student', route: '/student/exams/2/take', resolve: 'studentExamTake' },
  { role: 'student', route: '/student/exams/detail/2', resolve: 'studentExamDetail' },
  { role: 'student', route: '/student/support-tickets' },
  { role: 'content', route: '/content-council/question-bank' },
  { role: 'content', route: '/content-council/subjects/9/preview', resolve: 'subjectPreview' },
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
    return new Promise((resolve, reject) => pending.set(callId, { resolve, reject }))
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
      const expiresAt = data.expiresAt || data.ExpiresAt || data?.data?.expiresAt || data?.Data?.ExpiresAt || new Date(Date.now() + 3600000).toISOString();
      const refreshTokenExpiresAt = data.refreshTokenExpiresAt || data.RefreshTokenExpiresAt || data?.data?.refreshTokenExpiresAt || data?.Data?.RefreshTokenExpiresAt || expiresAt;
      localStorage.setItem('lms_access_token', token);
      sessionStorage.setItem('lms_access_token', token);
      localStorage.setItem('lms_refresh_token', data.refreshToken || data.RefreshToken || data?.data?.refreshToken || data?.Data?.RefreshToken || '');
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
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

async function apiFetch(send, url) {
  return evaluate(send, `
    (async () => {
      const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || '';
      const res = await fetch(${JSON.stringify(url)}, { headers: token ? { Authorization: 'Bearer ' + token } : {} });
      const text = await res.text();
      let body = null;
      try { body = text ? JSON.parse(text) : null; } catch { body = { raw: text }; }
      return { ok: res.ok, status: res.status, body };
    })()
  `)
}

function unwrapItems(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  if (Array.isArray(data?.records)) return data.records
  if (Array.isArray(data?.Records)) return data.Records
  return []
}

function pickId(item, fields) {
  for (const field of fields) {
    const value = item?.[field]
    if (value !== undefined && value !== null && value !== '') return value
  }
  return null
}

async function resolveRoute(send, route) {
  if (route.resolve === 'studentExamTake' || route.resolve === 'studentExamDetail') {
    const response = await apiFetch(send, '/api/exam/student/list')
    if (!response.ok) return { ...route, result: 'SKIPPED_NO_DATA', resolveStatus: response.status }
    const id = pickId(unwrapItems(response.body)[0], ['id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'])
    if (!id) return { ...route, result: 'SKIPPED_NO_DATA', resolveStatus: 'NO_DATA' }
    return {
      ...route,
      route: route.resolve === 'studentExamTake' ? `/student/exams/${id}/take` : `/student/exams/detail/${id}`,
      acceptedPaths: route.resolve === 'studentExamTake'
        ? [`/student/exams/${id}/take`, `/student/exams/detail/${id}`]
        : [`/student/exams/detail/${id}`],
      requestedRoute: route.route,
      resolvedId: id,
    }
  }

  if (route.resolve === 'subjectPreview') {
    const response = await apiFetch(send, '/api/master-data/subjects?pageSize=20')
    if (!response.ok) return { ...route, result: 'SKIPPED_NO_DATA', resolveStatus: response.status }
    const id = pickId(unwrapItems(response.body)[0], ['maMonHoc', 'MaMonHoc', 'id', 'Id'])
    if (!id) return { ...route, result: 'SKIPPED_NO_DATA', resolveStatus: 'NO_DATA' }
    return { ...route, route: `/content-council/subjects/${id}/preview`, requestedRoute: route.route, resolvedId: id }
  }

  return { ...route, requestedRoute: route.route }
}

function summarizeNetwork(events) {
  return events
    .filter(event => event.method === 'Network.responseReceived')
    .map(event => event.params.response)
    .filter(response => response.url.includes('/api/') && response.status >= 400)
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

  const results = []
  let currentRole = null

  for (const route of routes) {
    if (currentRole !== route.role) {
      await login(send, accounts[route.role])
      currentRole = route.role
      await delay(500)
    }

    const resolved = await resolveRoute(send, route)
    if (resolved.result === 'SKIPPED_NO_DATA') {
      results.push({
        role: route.role,
        requestedRoute: route.route,
        route: route.route,
        result: 'SKIPPED_NO_DATA',
        resolveStatus: resolved.resolveStatus,
        apiErrors: [],
        consoleErrors: [],
        runtimeExceptions: [],
      })
      continue
    }

    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${resolved.route}` })
    const pageProbe = await waitForPage(send)
    const seen = events.slice(start)
    const apiErrors = summarizeNetwork(seen)
    const consoleSummary = summarizeConsole(seen)
    const acceptedPaths = resolved.acceptedPaths || [resolved.route]
    const guardedRedirect =
      resolved.resolve === 'studentExamTake' &&
      pageProbe?.path === `/student/exams/detail/${resolved.resolvedId}`
    const pass =
      pageProbe?.hasRoot === true &&
      pageProbe?.hasVisibleText === true &&
      acceptedPaths.includes(pageProbe?.path) &&
      apiErrors.length === 0 &&
      consoleSummary.consoleErrors.length === 0 &&
      consoleSummary.runtimeExceptions.length === 0

    results.push({
      role: route.role,
      requestedRoute: resolved.requestedRoute,
      route: resolved.route,
      resolvedId: resolved.resolvedId,
      result: pass ? (guardedRedirect ? 'PASS_GUARDED_REDIRECT' : 'PASS') : 'FAIL',
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
    passCount: results.filter(result => result.result === 'PASS' || result.result === 'PASS_GUARDED_REDIRECT').length,
    failCount: results.filter(result => result.result === 'FAIL').length,
    skippedCount: results.filter(result => result.result === 'SKIPPED_NO_DATA').length,
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
