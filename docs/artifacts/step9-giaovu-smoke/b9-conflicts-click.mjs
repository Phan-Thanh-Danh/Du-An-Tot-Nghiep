import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = 'https://localhost'
const CDP = 'http://127.0.0.1:9222'
const ARTIFACT_DIR = 'C:\\Users\\maita\\OneDrive\\Máy tính\\Du-An-Tot-Nghiep\\docs\\artifacts\\step9-giaovu-smoke'
const STAFF = { email: 'p12test_staff01@lms.local', password: 'Test@123' }

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

  await send('Page.navigate', { url: `${FRONTEND}/staff/conflicts` })
  await delay(4000)

  const before = await evaluate(send, `document.body?.innerText.replace(/\s+/g,' ').includes('Không tìm thấy xung đột nào')`)

  const clicked = await evaluate(send, `
    (() => {
      const buttons = [...document.querySelectorAll('button')];
      const labels = buttons.map(b => (b.textContent || '').trim()).filter(Boolean);
      const target = buttons.find(b => /Kiểm tra toàn hệ thống/.test(b.textContent));
      if (!target) return { clicked: false, labels };
      target.click();
      return { clicked: true, labels };
    })()
  `)

  await delay(6000)

  const text = await evaluate(send, `document.body?.innerText.replace(/\s+/g,' ')`)
  const i = text.indexOf('Kiểm tra xung đột Phát hiện và xử lý xung đ')
  const stats = i >= 0 ? text.slice(i, i + 500) : text.slice(Math.max(0, text.indexOf('TỔNG XUNG ĐỘT') - 80), Math.max(0, text.indexOf('TỔNG XUNG ĐỘT') - 80) + 600)
  const foundConflict = /TỔNG XUNG ĐỘT [1-9]/.test(text)

  const result = {
    clicked,
    initialEmptyState: before,
    conflictsPanelText: stats.slice(0, 500),
    conflictFoundAfterClick: foundConflict,
  }
  fs.writeFileSync(path.join(ARTIFACT_DIR, 'b9-conflicts-click.json'), JSON.stringify(result, null, 2) + '\n')
  console.log(JSON.stringify(result, null, 2))
  ws.close()
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})