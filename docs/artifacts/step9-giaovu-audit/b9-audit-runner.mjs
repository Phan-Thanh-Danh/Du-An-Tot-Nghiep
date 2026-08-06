import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.B9_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.B9_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = 'C:\\Users\\maita\\OneDrive\\Máy tính\\Du-An-Tot-Nghiep\\docs\\artifacts\\step9-giaovu-audit'
const RESULT_PATH = path.join(ARTIFACT_DIR, 'smoke-results-audit.json')

const STAFF = { email: 'p12test_staff01@lms.local', password: 'Test@123' }

const routes = [
  { route: '/staff/dashboard', group: '1-dashboard', note: 'Dashboard GiaoVu' },
  { route: '/staff/profile', group: '1-dashboard', note: 'StaffProfileView' },
  { route: '/staff/notices/send', group: '1-dashboard', note: 'SendNoticeView' },
  { route: '/staff/notices/history', group: '1-dashboard', note: 'NoticeHistoryView' },
  { route: '/staff/schedule', group: '2-schedule', note: 'ScheduleManagerView' },
  { route: '/staff/schedule/pending', group: '2-schedule', note: 'PendingSchedulesView' },
  { route: '/staff/schedule/published', group: '2-schedule', note: 'StaffPublishedSchedulesView' },
  { route: '/staff/conflicts', group: '2-schedule', note: 'ConflictCheckView' },
  { route: '/staff/shifts', group: '2-schedule', note: 'ShiftManagementView' },
  { route: '/staff/blocks', group: '2-schedule', note: 'BlockManagementView' },
  { route: '/staff/credit-mappings', group: '2-schedule', note: 'CreditMappingView' },
  { route: '/staff/buildings', group: '3-facilities', note: 'BuildingManagementView' },
  { route: '/staff/floors', group: '3-facilities', note: 'FloorManagementView' },
  { route: '/staff/rooms', group: '3-facilities', note: 'RoomManagementView' },
  { route: '/staff/assignments', group: '3-facilities', note: 'TeacherAssignmentView' },
  { route: '/staff/teaching-preferences', group: '3-facilities', note: 'TeachingPreferenceSummaryView' },
  { route: '/staff/academic-terms', group: '4-masterdata', note: 'AcademicTermManagementView' },
  { route: '/staff/subjects', group: '4-masterdata', note: 'SubjectManagementView' },
  { route: '/staff/courses', group: '4-masterdata', note: 'CourseManagementView' },
  { route: '/staff/registrations', group: '5-registration', note: 'RegistrationPeriodsView' },
  { route: '/staff/capacity', group: '5-registration', note: 'CapacityAdjustmentView' },
  { route: '/staff/course-status', group: '5-registration', note: 'CourseStatusView' },
  { route: '/staff/requests', group: '6-requests', note: 'PendingRequestsView' },
  { route: '/staff/requests/:id', group: '6-requests', note: 'RequestDetailView', resolveId: true },
  { route: '/staff/requests-history', group: '6-requests', note: 'RequestHistoryView' },
  { route: '/staff/workflow', group: '6-requests', note: 'WorkflowConfigView' },
  { route: '/staff/classes', group: '7-admin', note: 'ClassManagementView' },
  { route: '/staff/accounts', group: '7-admin', note: 'AccountManagementView' },
]

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

async function newTab() {
  let res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`, { method: 'PUT' })
  if (!res.ok) res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`)
  if (!res.ok) throw new Error(`tab: ${res.status}`)
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
  const send = (method, params = {}) => {
    const callId = ++id
    ws.send(JSON.stringify({ id: callId, method, params }))
    return new Promise((resolve, reject) => {
      pending.set(callId, { resolve, reject })
    })
  }
  return { ws, send, events }
}

async function evaluate(send, expression) {
  const result = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true })
  if (result.exceptionDetails) {
    throw new Error(result.exceptionDetails.exception?.description || result.exceptionDetails.text)
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
      if (!res.ok || !token) throw new Error('login failed ' + res.status);
      const user = data.user || data?.data?.user || data?.Data?.User || {};
      localStorage.setItem('lms_access_token', token);
      sessionStorage.setItem('lms_access_token', token);
      localStorage.setItem('lms_refresh_token', data.refreshToken || '');
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_token_expires_at', data.expiresAt || '');
      localStorage.setItem('lms_requires_password_change', 'false');
      return { ok: true, role: user.role || '' };
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

function unwrapItems(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  if (Array.isArray(data?.records)) return data.records
  if (Array.isArray(data?.Records)) return data.Records
  if (Array.isArray(data?.data)) return data.data
  if (Array.isArray(data?.content)) return data.content
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

async function resolveRequestId(send) {
  // Request Detail cần id đơn — thử vài endpoint list
  const candidates = [
    '/api/admin/applications?PageIndex=1&PageSize=5',
    '/api/teacher/requests?PageIndex=1&PageSize=5',
    '/api/staff/requests?PageIndex=1&PageSize=5',
  ]
  for (const endpoint of candidates) {
    const res = await apiFetch(send, endpoint)
    if (!res.ok) continue
    const items = unwrapItems(res.body)
    const id = pickId(items[0] && items[0].items ? items[0].items[0] : items[0], ['maDonTu', 'MaDonTu', 'id', 'Id', 'maDon', 'MaDon', 'requestId', 'RequestId', 'maHoSo', 'MaHoSo'])
    if (id !== null && id !== undefined) return { ok: true, endpoint, id }
  }
  return { ok: false, endpoint: candidates[0] }
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
    apiCalls: apiResponses
      .filter(response => response.status < 400)
      .map(response => `${response.status} ${response.url.replace('https://localhost:5173', '').replace('https://localhost', '')}`),
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
      const loadingText = /dang tai|loading|Đang tải/i.test(text);
      const hasVisibleText = text.trim().length > 0;
      return { title: document.title || '', hasRoot, hasVisibleText, loadingText, path: location.pathname, len: text.length };
    })()
  `)
}

async function waitForPageProbe(send, timeoutMs) {
  const deadline = Date.now() + timeoutMs
  let latest = null
  while (Date.now() < deadline) {
    latest = await probePage(send).catch(error => ({ hasRoot: false, hasVisibleText: false, loadingText: false, probeError: error.message }))
    if (latest.hasRoot === true && latest.hasVisibleText === true && latest.loadingText !== true) {
      return latest
    }
    await delay(250)
  }
  return latest || { hasRoot: false, hasVisibleText: false, loadingText: false, probeError: 'timeout' }
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
  await delay(2000)

  await login(send, STAFF)
  await delay(800)

  const results = []

  for (const item of routes) {
    let targetRoute = item.route
    let resolveInfo = null

    if (item.resolveId) {
      resolveInfo = await resolveRequestId(send)
      if (resolveInfo.ok) {
        targetRoute = item.route.replace(':id', String(resolveInfo.id))
      } else {
        results.push({
          route: item.route, group: item.group, note: item.note,
          result: 'SKIP_NO_DATA', pageProbe: null, apiErrors: [], consoleErrors: [], runtimeExceptions: [],
          resolveInfo,
        })
        console.log(`[AUDIT] ${item.route} -> SKIP_NO_DATA (seed có 0 đơn)`)
        continue
      }
    }

    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${targetRoute}` })
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
      group: item.group,
      note: item.note,
      targetRoute,
      pageLoaded: Boolean(pageProbe.hasRoot && pageProbe.hasVisibleText),
      textLen: pageProbe.len || 0,
      apiCalls: network.apiCalls,
      apiErrors: network.apiErrors.map(error => `${error.status} ${error.url}`),
      consoleErrors: consoleSummary.consoleErrors,
      runtimeExceptions: consoleSummary.runtimeExceptions,
      result: pass ? 'PASS' : 'FAIL',
      resolveInfo: resolveInfo || null,
    })
    console.log(`[AUDIT ${item.group}] ${item.route} -> ${pass ? 'PASS' : 'FAIL'} (textLen=${pageProbe.len || 0}, apiCalls=${network.apiCalls.length})`)
  }

  ws.close()

  const totals = {
    routes: results.length,
    passed: results.filter(r => r.result === 'PASS').length,
    failed: results.filter(r => r.result === 'FAIL').length,
    skipped: results.filter(r => r.result === 'SKIP_NO_ID').length,
    consoleErrors: results.reduce((s, r) => s + r.consoleErrors.length, 0),
    runtimeExceptions: results.reduce((s, r) => s + r.runtimeExceptions.length, 0),
    network401: results.reduce((s, r) => s + r.apiErrors.filter(e => e.startsWith('401 ')).length, 0),
    network403: results.reduce((s, r) => s + r.apiErrors.filter(e => e.startsWith('403 ')).length, 0),
    network404: results.reduce((s, r) => s + r.apiErrors.filter(e => e.startsWith('404 ')).length, 0),
    network500: results.reduce((s, r) => s + r.apiErrors.filter(e => /^5\d\d /.test(e)).length, 0),
  }

  const output = {
    generatedAt: new Date().toISOString(),
    frontend: FRONTEND,
    cdp: CDP,
    browser: version.Browser,
    account: STAFF.email,
    totals,
    failSummaries: results.filter(r => r.result === 'FAIL').map(r => ({ route: r.route, apiErrors: r.apiErrors, consoleErrors: r.consoleErrors, runtimeExceptions: r.runtimeExceptions })),
    results,
  }

  fs.writeFileSync(RESULT_PATH, `${JSON.stringify(output, null, 2)}\n`)
  console.log(JSON.stringify({ totals, failed: output.failSummaries }, null, 2))
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})