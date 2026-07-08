const FRONTEND = 'https://localhost:5173'
const CDP = 'http://localhost:9222'

const accounts = {
  parent: { email: 'p15test_parent01@lms.local', password: 'Test@123' },
  superadmin: { email: 'superadmin@lms.local', password: '123456' },
  student: { email: 'p12test_student011@lms.local', password: 'Test@123' },
  teacher: { email: 'p12test_teacher01@lms.local', password: 'Test@123' },
  staff: { email: 'p12test_staff01@lms.local', password: 'Test@123' },
  bgh: { email: 'p15test_bgh01@lms.local', password: 'Test@123' },
  content: { email: 'p15test_content01@lms.local', password: 'Test@123' },
}

const routes = [
  ...[
    '/parent/dashboard',
    '/parent/children/list',
    '/parent/children/overview',
    '/parent/learning/grades',
    '/parent/learning/schedule',
    '/parent/learning/attendance',
    '/parent/learning/alerts',
    '/parent/finance/tuition',
    '/parent/finance/payment',
    '/parent/finance/transactions',
    '/parent/finance/invoices',
    '/parent/notifications/system',
    '/parent/notifications/history',
    '/parent/profile/info',
    '/parent/profile/access-rights',
  ].map(route => ({ role: 'parent', route })),
  ...[
    '/super-admin/dashboard',
    '/super-admin/profile',
    '/super-admin/organizations',
    '/super-admin/users',
    '/super-admin/roles-permissions',
    '/super-admin/login-history',
    '/super-admin/training/semesters',
    '/super-admin/training/programs',
    '/super-admin/training/subjects',
    '/super-admin/training/courses',
    '/super-admin/training/exam-periods',
    '/super-admin/operations/schedules',
    '/super-admin/operations/schedules/approval',
    '/super-admin/operations/attendance-policy',
    '/super-admin/operations/registration-periods',
    '/super-admin/operations/pass-fail-rules',
    '/super-admin/finance/tuition-config',
    '/super-admin/finance/student-debts',
    '/super-admin/finance/payments',
    '/super-admin/finance/refunds',
    '/super-admin/support/tickets',
    '/super-admin/support/faq',
    '/super-admin/approvals/requests',
    '/super-admin/approvals/history',
    '/super-admin/approvals/reports',
    '/super-admin/rewards-discipline',
    '/super-admin/rewards/campaigns',
    '/super-admin/discipline/records',
    '/super-admin/discipline/appeals',
    '/super-admin/evaluations/config',
    '/super-admin/evaluations/results',
    '/super-admin/awards',
    '/super-admin/discipline',
    '/super-admin/reports/education-overview',
    '/super-admin/reports/learning',
    '/super-admin/reports/attendance',
    '/super-admin/reports/campus-comparison',
    '/super-admin/reports/export',
    '/super-admin/notifications/templates',
    '/super-admin/notifications/send',
    '/super-admin/notifications/history',
    '/super-admin/audit/logs',
    '/super-admin/security/alerts',
    '/super-admin/system/modules',
    '/super-admin/system/ai-automation',
  ].map(route => ({ role: 'superadmin', route })),
  { role: 'student', route: '/student/dashboard' },
  { role: 'teacher', route: '/teacher/dashboard' },
  { role: 'staff', route: '/staff/dashboard' },
  { role: 'bgh', route: '/bgh/dashboard' },
  { role: 'content', route: '/content-council/subjects' },
]

async function newTab() {
  let res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`, { method: 'PUT' })
  if (!res.ok) res = await fetch(`${CDP}/json/new?${encodeURIComponent(FRONTEND)}`)
  if (!res.ok) throw new Error(`Cannot create CDP tab: ${res.status}`)
  return res.json()
}

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
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
    const msg = JSON.parse(event.data)
    if (msg.id && pending.has(msg.id)) {
      const { resolve, reject } = pending.get(msg.id)
      pending.delete(msg.id)
      if (msg.error) reject(new Error(msg.error.message))
      else resolve(msg.result)
      return
    }
    events.push(msg)
  })

  function send(method, params = {}) {
    const callId = ++id
    ws.send(JSON.stringify({ id: callId, method, params }))
    return new Promise((resolve, reject) => pending.set(callId, { resolve, reject }))
  }

  return { ws, send, events }
}

async function evaluate(send, expression) {
  return send('Runtime.evaluate', {
    expression,
    awaitPromise: true,
    returnByValue: true,
  })
}

async function login(send, account) {
  const expression = `
    (async () => {
      localStorage.clear();
      sessionStorage.clear();
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ usernameOrEmail: ${JSON.stringify(account.email)}, password: ${JSON.stringify(account.password)} })
      });
      const data = await res.json();
      if (!res.ok || !data.accessToken) throw new Error('login failed ' + res.status);
      localStorage.setItem('lms_access_token', data.accessToken);
      localStorage.setItem('lms_refresh_token', data.refreshToken || '');
      localStorage.setItem('lms_auth_user', JSON.stringify(data.user || {}));
      localStorage.setItem('lms_token_expires_at', data.expiresAt || '');
      localStorage.setItem('lms_refresh_token_expires_at', data.refreshTokenExpiresAt || '');
      localStorage.setItem('lms_requires_password_change', String(Boolean(data.requiresPasswordChange)));
      return { ok: true, role: data.user?.role };
    })()
  `
  return evaluate(send, expression)
}

function collectSince(events, start) {
  return events.slice(start)
}

async function main() {
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)
  await send('Page.enable')
  await send('Runtime.enable')
  await send('Network.enable')
  await send('Log.enable')
  await send('Page.navigate', { url: FRONTEND })
  await delay(1500)

  const loginCache = new Set()
  const results = []

  for (const item of routes) {
    if (!loginCache.has(item.role)) {
      await login(send, accounts[item.role])
      loginCache.add(item.role)
    }

    const start = events.length
    await send('Page.navigate', { url: `${FRONTEND}${item.route}` })
    await delay(2200)
    const seen = collectSince(events, start)
    const responses = seen
      .filter(e => e.method === 'Network.responseReceived')
      .map(e => e.params.response)
      .filter(r => r.url.includes('/api/'))
    const result = {
      role: item.role,
      route: item.route,
      api4xx5xx: responses.filter(r => r.status >= 400).map(r => ({ status: r.status, url: r.url })),
      consoleErrorDetails: seen
        .filter(e => e.method === 'Runtime.consoleAPICalled' && e.params.type === 'error')
        .map(e => e.params.args?.map(arg => arg.value || arg.description || '').join(' ')),
      runtimeExceptionDetails: seen
        .filter(e => e.method === 'Runtime.exceptionThrown')
        .map(e => e.params.exceptionDetails?.exception?.description || e.params.exceptionDetails?.text || ''),
    }
    result.consoleErrors = result.consoleErrorDetails.length
    result.runtimeExceptions = result.runtimeExceptionDetails.length
    result.pass = result.api4xx5xx.length === 0 && result.consoleErrors === 0 && result.runtimeExceptions === 0
    results.push(result)
  }

  ws.close()

  const totals = {
    routes: results.length,
    passed: results.filter(r => r.pass).length,
    consoleErrors: results.reduce((sum, r) => sum + r.consoleErrors, 0),
    runtimeExceptions: results.reduce((sum, r) => sum + r.runtimeExceptions, 0),
    network401: results.reduce((sum, r) => sum + r.api4xx5xx.filter(e => e.status === 401).length, 0),
    network403: results.reduce((sum, r) => sum + r.api4xx5xx.filter(e => e.status === 403).length, 0),
    network404: results.reduce((sum, r) => sum + r.api4xx5xx.filter(e => e.status === 404).length, 0),
    network500: results.reduce((sum, r) => sum + r.api4xx5xx.filter(e => e.status >= 500).length, 0),
  }

  console.log(JSON.stringify({ generatedAt: new Date().toISOString(), frontend: FRONTEND, totals, results }, null, 2))
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
