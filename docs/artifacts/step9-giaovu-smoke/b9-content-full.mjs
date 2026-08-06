import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = 'https://localhost:5173'
const CDP = 'http://127.0.0.1:9222'
const ARTIFACT_DIR = 'C:\\Users\\maita\\OneDrive\\MÃ¡y tÃ­nh\\Du-An-Tot-Nghiep\\docs\\artifacts\\step9-giaovu-smoke'
const STAFF = { email: 'p12test_staff01@lms.local', password: 'Test@123' }

const routes = ['/staff/conflicts', '/staff/profile', '/staff/schedule/published']

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
  ws.addEventListener('message', event => {
    const message = JSON.parse(event.data)
    if (message.id && pending.has(message.id)) {
      const { resolve, reject } = pending.get(message.id)
      pending.delete(message.id)
      if (message.error) reject(new Error(message.error.message))
      else resolve(message.result)
    }
  })
  const send = (method, params = {}) => {
    const callId = ++id
    ws.send(JSON.stringify({ id: callId, method, params }))
    return new Promise((resolve, reject) => {
      pending.set(callId, { resolve, reject })
    })
  }
  return { ws, send }
}

async function evaluate(send, expression) {
  const result = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true })
  if (result.exceptionDetails) throw new Error(result.exceptionDetails.exception?.description || result.exceptionDetails.text)
  return result.result?.value
}

async function main() {
  const tab = await newTab()
  const { ws, send } = await createClient(tab.webSocketDebuggerUrl)
  await send('Page.enable')
  await send('Runtime.enable')
  await send('Security.enable')
  await send('Security.setIgnoreCertificateErrors', { ignore: true })

  await send('Page.navigate', { url: FRONTEND })
  await delay(2000)

  await evaluate(send, `
    (async () => {
      localStorage.clear();
      sessionStorage.clear();
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ usernameOrEmail: '${STAFF.email}', email: '${STAFF.email}', password: '${STAFF.password}' })
      });
      const data = await res.json().catch(() => ({}));
      const token = data.accessToken || data.token || data?.data?.accessToken || '';
      localStorage.setItem('lms_access_token', token);
      sessionStorage.setItem('lms_access_token', token);
      const user = data.user || data?.data?.user || {};
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_refresh_token', data.refreshToken || '');
      localStorage.setItem('lms_token_expires_at', data.expiresAt || '');
      localStorage.setItem('lms_requires_password_change', 'false');
      return Boolean(token);
    })()
  `)

  const report = []
  for (const route of routes) {
    await send('Page.navigate', { url: `${FRONTEND}${route}` })
    await delay(4500)
    const text = await evaluate(send, `document.body?.innerText || ''`)
    report.push({ route, snippet: text.replace(/\s+/g, ' '). })
    console.log(`\n===== ${route} =====`)
    console.log(report[report.length - 1].snippet)
  }

  const outPath = path.join(ARTIFACT_DIR, 'b9-content-dump.json')
  fs.writeFileSync(outPath, JSON.stringify(report, null, 2) + '\n')
  console.log(`\nwrote ${outPath}`)
  ws.close()
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})

