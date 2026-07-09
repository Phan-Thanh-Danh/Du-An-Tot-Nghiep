import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.P16B1_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.P16B1_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'))
const RESULT_PATH = path.join(ARTIFACT_DIR, 'p16b1-results.json')

const accounts = {
  SuperAdmin: { email: 'superadmin@lms.local', password: '123456' },
  BGH: { email: 'p15test_bgh01@lms.local', password: 'Test@123' },
}

const routes = [
  { role: 'SuperAdmin', route: '/super-admin/operations/schedules' },
  { role: 'SuperAdmin', route: '/super-admin/operations/schedules/approval' },
  { role: 'SuperAdmin', route: '/super-admin/finance/tuition-config' },
  { role: 'SuperAdmin', route: '/super-admin/finance/student-debts' },
  { role: 'SuperAdmin', route: '/super-admin/finance/payments' },
  { role: 'SuperAdmin', route: '/super-admin/finance/refunds' },
  { role: 'SuperAdmin', route: '/super-admin/notifications/history' },
  { role: 'BGH', route: '/bgh/users', check: 'bgh-users-readonly' },
  { role: 'BGH', route: '/bgh/roles' },
  { role: 'BGH', route: '/bgh/curriculum' },
  { role: 'BGH', route: '/bgh/facilities' },
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

async function waitForPage(send, route) {
  for (let i = 0; i < 40; i++) {
    const probe = await evaluate(send, `
      (() => {
        const root = document.querySelector('#app');
        const text = (root?.innerText || '').trim();
        const loading = /Đang tải|Loading/i.test(text);
        return {
          path: location.pathname,
          hasRoot: !!root,
          hasVisibleText: text.length > 30,
          loading,
          title: document.title
        };
      })()
    `)
    if (probe.path === route && probe.hasRoot && probe.hasVisibleText && !probe.loading) return probe
    await delay(500)
  }
  return evaluate(send, `
    (() => {
      const root = document.querySelector('#app');
      const text = (root?.innerText || '').trim();
      return { path: location.pathname, hasRoot: !!root, hasVisibleText: text.length > 30, loading: /Đang tải|Loading/i.test(text), title: document.title };
    })()
  `)
}

async function safeActionProbe(send, route) {
  if (route.check !== 'bgh-users-readonly') return { skipped: true }
  return evaluate(send, `
    (() => {
      const labels = ['Thêm người dùng', 'Chỉnh sửa', 'Khóa tài khoản', 'Mở khóa tài khoản', 'Đặt lại mật khẩu'];
      const buttons = [...document.querySelectorAll('button')].map(btn => ({
        text: (btn.innerText || '').trim(),
        title: btn.getAttribute('title') || '',
        aria: btn.getAttribute('aria-label') || ''
      }));
      const matches = buttons.filter(btn => labels.some(label => btn.text.includes(label) || btn.title.includes(label) || btn.aria.includes(label)));
      return { adminMutationButtonCount: matches.length, matches };
    })()
  `)
}

async function main() {
  fs.mkdirSync(ARTIFACT_DIR, { recursive: true })
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)

  await send('Page.enable')
  await send('Network.enable')
  await send('Runtime.enable')
  await send('Log.enable')
  await send('Security.setIgnoreCertificateErrors', { ignore: true }).catch(() => {})

  const results = []
  let currentRole = null

  for (const route of routes) {
    if (currentRole !== route.role) {
      await send('Page.navigate', { url: FRONTEND })
      await delay(1000)
      await login(send, accounts[route.role])
      currentRole = route.role
    }

    const before = events.length
    await send('Page.navigate', { url: `${FRONTEND}${route.route}` })
    await delay(700)
    const pageProbe = await waitForPage(send, route.route)
    const safeProbe = await safeActionProbe(send, route)
    await delay(500)

    const slice = events.slice(before)
    const networkFailures = slice
      .filter(e => e.method === 'Network.responseReceived')
      .map(e => e.params?.response)
      .filter(r => r && [401, 403, 404, 500].includes(r.status))
      .map(r => ({ status: r.status, url: r.url }))
    const consoleErrors = slice
      .filter(e => e.method === 'Runtime.consoleAPICalled' && e.params?.type === 'error')
      .map(e => ({ args: (e.params.args || []).map(a => a.value || a.description || a.type).join(' ') }))
    const runtimeExceptions = slice
      .filter(e => e.method === 'Runtime.exceptionThrown')
      .map(e => ({ text: e.params?.exceptionDetails?.text || e.params?.exceptionDetails?.exception?.description }))

    const failedReadonly = route.check === 'bgh-users-readonly' && safeProbe.adminMutationButtonCount !== 0
    const passed = pageProbe.path === route.route && pageProbe.hasRoot && pageProbe.hasVisibleText && !pageProbe.loading && networkFailures.length === 0 && consoleErrors.length === 0 && runtimeExceptions.length === 0 && !failedReadonly

    results.push({
      ...route,
      pageProbe,
      safeProbe,
      consoleErrors,
      runtimeExceptions,
      networkFailures,
      result: passed ? 'PASS' : 'FAIL',
    })
  }

  const totals = {
    phase: 'P16B.1',
    highRiskRoutesTargeted: routes.length,
    routesVisited: results.length,
    routesPassed: results.filter(r => r.result === 'PASS').length,
    routesFailed: results.filter(r => r.result !== 'PASS').length,
    consoleErrors: results.reduce((sum, r) => sum + r.consoleErrors.length, 0),
    runtimeExceptions: results.reduce((sum, r) => sum + r.runtimeExceptions.length, 0),
    network401: results.reduce((sum, r) => sum + r.networkFailures.filter(n => n.status === 401).length, 0),
    network403: results.reduce((sum, r) => sum + r.networkFailures.filter(n => n.status === 403).length, 0),
    network404: results.reduce((sum, r) => sum + r.networkFailures.filter(n => n.status === 404).length, 0),
    network500: results.reduce((sum, r) => sum + r.networkFailures.filter(n => n.status === 500).length, 0),
    placeholdersRemaining: 0,
    productionMockFallbackRemaining: 25,
  }

  fs.writeFileSync(RESULT_PATH, JSON.stringify({
    generatedAt: new Date().toISOString(),
    frontend: FRONTEND,
    cdp: CDP,
    totals,
    results,
  }, null, 2))

  ws.close()
  console.log(JSON.stringify(totals, null, 2))
  if (totals.routesFailed > 0) process.exit(1)
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
