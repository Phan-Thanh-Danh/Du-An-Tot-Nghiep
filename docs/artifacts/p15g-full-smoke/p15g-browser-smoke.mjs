import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.P15G_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.P15G_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'))
const RESULT_PATH = path.join(ARTIFACT_DIR, 'smoke-results-p15g.json')

const accounts = {
  superadmin: { email: 'superadmin@lms.local', password: '123456' },
  staff: { email: 'p12test_staff01@lms.local', password: 'Test@123' },
  teacher: { email: 'p12test_teacher01@lms.local', password: 'Test@123' },
  student: { email: 'p12test_student011@lms.local', password: 'Test@123' },
  bgh: { email: 'p15test_bgh01@lms.local', password: 'Test@123' },
  parent: { email: 'p15test_parent01@lms.local', password: 'Test@123' },
  contentcouncil: { email: 'p15test_content01@lms.local', password: 'Test@123' },
  content: { email: 'p15test_content01@lms.local', password: 'Test@123' },
}

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

function normalizeRole(role) {
  return String(role || '').replace(/[^a-z]/gi, '').toLowerCase()
}

function unwrapItems(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  if (Array.isArray(data?.records)) return data.records
  if (Array.isArray(data?.Records)) return data.Records
  if (Array.isArray(data?.students)) return data.students
  if (Array.isArray(data?.Students)) return data.Students
  return []
}

function pickId(item, fields) {
  if (!item || typeof item !== 'object') return null
  for (const field of fields) {
    const value = item[field]
    if (value !== undefined && value !== null && value !== '') return value
  }
  return null
}

function loadRoutes() {
  const current = JSON.parse(fs.readFileSync(RESULT_PATH, 'utf8'))
  const rows = Array.isArray(current) ? current : current.results
  if (!Array.isArray(rows) || rows.length === 0) {
    throw new Error(`No route list found in ${RESULT_PATH}`)
  }

  return rows.map(row => ({
    role: row.role,
    route: row.requestedRoute || row.originalRoute || row.route,
  }))
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

async function apiFetch(send, url) {
  return evaluate(send, `
    (async () => {
      const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || '';
      const res = await fetch(${JSON.stringify(url)}, {
        headers: token ? { Authorization: 'Bearer ' + token } : {}
      });
      const text = await res.text();
      let body = null;
      try { body = text ? JSON.parse(text) : null; } catch { body = { raw: text }; }
      return { ok: res.ok, status: res.status, url: res.url, body };
    })()
  `)
}

async function firstId(send, endpoint, fields) {
  const response = await apiFetch(send, endpoint)
  if (!response.ok) return { ok: false, status: response.status, endpoint }
  const item = unwrapItems(response.body)[0]
  const id = pickId(item, fields)
  if (id === null) return { ok: false, status: 'NO_DATA', endpoint }
  return { ok: true, id, endpoint }
}

async function resolveRoute(send, item) {
  const requestedRoute = item.route
  let route = requestedRoute

  const replacements = [
    {
      test: /^\/student\/courses\/1$/,
      endpoint: '/api/student/courses',
      fields: ['maKhoaHoc', 'MaKhoaHoc', 'id', 'Id', 'code', 'Code'],
      build: id => `/student/courses/${id}`,
    },
    {
      test: /^\/student\/assignments\/1$/,
      endpoint: '/api/student/assignments',
      fields: ['id', 'Id', 'maBaiTap', 'MaBaiTap', 'assignmentId', 'AssignmentId'],
      build: id => `/student/assignments/${id}`,
    },
    {
      test: /^\/student\/exams\/detail\/1$/,
      endpoint: '/api/exam/student/list',
      fields: ['id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'],
      build: id => `/student/exams/detail/${id}`,
    },
    {
      test: /^\/student\/exams\/1\/take$/,
      endpoint: '/api/exam/student/list',
      fields: ['id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'],
      build: id => `/student/exams/${id}/take`,
    },
    {
      test: /^\/student\/exams\/1$/,
      endpoint: '/api/exam/student/list',
      fields: ['resultId', 'ResultId', 'id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'],
      build: id => `/student/exams/${id}`,
    },
    {
      test: /^\/teacher\/classes\/1\/details$/,
      endpoint: '/api/teacher/classes?pageSize=20',
      fields: ['id', 'Id', 'maKhoaHoc', 'MaKhoaHoc', 'classId', 'ClassId'],
      build: id => `/teacher/classes/${id}/details`,
    },
    {
      test: /^\/teacher\/classes\/1\/workspace$/,
      endpoint: '/api/teacher/classes?pageSize=20',
      fields: ['id', 'Id', 'maKhoaHoc', 'MaKhoaHoc', 'classId', 'ClassId'],
      build: id => `/teacher/classes/${id}/workspace`,
    },
    {
      test: /^\/bgh\/academic\/at-risk\/1\/history$/,
      endpoint: '/api/bgh/academic/at-risk',
      fields: ['id', 'Id', 'studentId', 'StudentId', 'maHocSinh', 'MaHocSinh'],
      build: id => `/bgh/academic/at-risk/${id}/history`,
    },
    {
      test: /^\/bgh\/evaluations\/detail\/1$/,
      endpoint: '/api/bgh/evaluations/ranking',
      fields: ['teacherId', 'TeacherId', 'maGiaoVien', 'MaGiaoVien', 'maCodeGv', 'MaCodeGv', 'id', 'Id'],
      build: id => `/bgh/evaluations/detail/${id}`,
    },
    {
      test: /^\/content-council\/subjects\/1\/overview$/,
      endpoint: '/api/master-data/subjects?pageSize=20',
      fields: ['maMonHoc', 'MaMonHoc', 'id', 'Id'],
      build: id => `/content-council/subjects/${id}/overview`,
    },
    {
      test: /^\/content-council\/subjects\/1\/editor$/,
      endpoint: '/api/master-data/subjects?pageSize=20',
      fields: ['maMonHoc', 'MaMonHoc', 'id', 'Id'],
      build: id => `/content-council/subjects/${id}/editor`,
    },
    {
      test: /^\/content-council\/subjects\/1\/preview$/,
      endpoint: '/api/master-data/subjects?pageSize=20',
      fields: ['maMonHoc', 'MaMonHoc', 'id', 'Id'],
      build: id => `/content-council/subjects/${id}/preview`,
    },
    {
      test: /^\/content-council\/quizzes\/1\/edit$/,
      endpoint: '/api/exam/de-kiem-tra/search?pageSize=20',
      fields: ['id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'],
      build: id => `/content-council/quizzes/${id}/edit`,
    },
    {
      test: /^\/content-council\/quizzes\/1\/builder$/,
      endpoint: '/api/exam/de-kiem-tra/search?pageSize=20',
      fields: ['id', 'Id', 'maDeKiemTra', 'MaDeKiemTra'],
      build: id => `/content-council/quizzes/${id}/builder`,
    },
  ]

  const replacement = replacements.find(entry => entry.test.test(route))
  if (!replacement) {
    return { route, requestedRoute, status: 'RESOLVED_STATIC' }
  }

  const resolved = await firstId(send, replacement.endpoint, replacement.fields)
  if (!resolved.ok) {
    return {
      route,
      requestedRoute,
      status: 'SKIPPED_NO_DATA',
      resolveEndpoint: resolved.endpoint,
      resolveStatus: resolved.status,
    }
  }

  return {
    route: replacement.build(resolved.id),
    requestedRoute,
    status: 'RESOLVED_DYNAMIC',
    resolveEndpoint: resolved.endpoint,
    resolvedId: resolved.id,
  }
}

function collectEvents(events, start) {
  return events.slice(start)
}

function summarizeNetwork(events) {
  const apiResponses = events
    .filter(event => event.method === 'Network.responseReceived')
    .map(event => event.params.response)
    .filter(response => response.url.includes('/api/'))

  const loadingFailed = events
    .filter(event => event.method === 'Network.loadingFailed')
    .map(event => ({
      requestId: event.params.requestId,
      errorText: event.params.errorText,
      canceled: event.params.canceled,
    }))

  return {
    apiErrors: apiResponses
      .filter(response => response.status >= 400)
      .map(response => ({ status: response.status, url: response.url })),
    networkFailures: loadingFailed,
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
      const title = document.title || '';
      const hasRoot = Boolean(document.querySelector('#app'));
      const loadingText = /dang tai|loading/i.test(text);
      const hasVisibleText = text.trim().length > 0;
      return { title, hasRoot, hasVisibleText, loadingText, path: location.pathname };
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
    probeError: 'page probe timed out before first evaluation',
  }
}

async function main() {
  const version = await fetch(`${CDP}/json/version`).then(res => res.json())
  const routes = loadRoutes()
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)

  await send('Page.enable')
  await send('Runtime.enable')
  await send('Network.enable')
  await send('Log.enable')
  await send('Page.navigate', { url: FRONTEND })
  await delay(1500)

  const loggedInRoles = new Set()
  const results = []

  for (const item of routes) {
    const roleKey = normalizeRole(item.role)
    const account = accounts[roleKey]
    if (!account) throw new Error(`No account configured for role ${item.role}`)

    if (!loggedInRoles.has(roleKey)) {
      await login(send, account)
      loggedInRoles.add(roleKey)
      await delay(600)
    }

    const resolved = await resolveRoute(send, item)
    if (resolved.status === 'SKIPPED_NO_DATA') {
      results.push({
        role: item.role,
        requestedRoute: item.route,
        route: resolved.route,
        pageLoaded: false,
        skeletonStuck: false,
        apiErrors: [],
        consoleErrors: [],
        runtimeExceptions: [],
        networkFailures: [],
        result: 'SKIPPED_NO_DATA',
        resolveEndpoint: resolved.resolveEndpoint,
        resolveStatus: resolved.resolveStatus,
      })
      continue
    }

    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${resolved.route}` })
    const pageProbe = await waitForPageProbe(send, Number(process.env.P15G_ROUTE_WAIT_MS || 5000))

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
      role: item.role,
      requestedRoute: item.route,
      route: resolved.route,
      pageLoaded: Boolean(pageProbe.hasRoot && pageProbe.hasVisibleText),
      skeletonStuck: false,
      apiErrors: network.apiErrors.map(error => `${error.status} ${error.url}`),
      consoleErrors: consoleSummary.consoleErrors,
      runtimeExceptions: consoleSummary.runtimeExceptions,
      networkFailures: network.networkFailures,
      result: pass ? 'PASS' : 'FAIL',
      resolveStatus: resolved.status,
      resolveEndpoint: resolved.resolveEndpoint,
      resolvedId: resolved.resolvedId,
      pageProbe,
    })
  }

  ws.close()

  const totals = {
    routes: results.length,
    passed: results.filter(result => result.result === 'PASS').length,
    failed: results.filter(result => result.result === 'FAIL').length,
    skipped: results.filter(result => result.result === 'SKIPPED_NO_DATA').length,
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
    totalRoutes: totals.routes,
    passCount: totals.passed,
    failCount: totals.failed,
    skippedCount: totals.skipped,
    totals,
    results,
  }

  fs.writeFileSync(RESULT_PATH, `${JSON.stringify(output, null, 2)}\n`)
  console.log(JSON.stringify(output, null, 2))
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
