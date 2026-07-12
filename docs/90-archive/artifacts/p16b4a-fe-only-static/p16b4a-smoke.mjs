/**
 * P16B.4A Targeted Browser Smoke
 * Scope: routes that changed status from FE_ONLY_STATIC in P16B.4A
 * Chrome CDP: http://127.0.0.1:9222
 * Frontend: https://localhost:5173
 * Backend: http://localhost:5097
 *
 * Run: node p16b4a-smoke.mjs
 */

import { writeFileSync } from 'fs'
import { resolve, dirname } from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url)
const __dir = dirname(__filename)

const FRONTEND = 'https://localhost:5173'
const CDP_URL = 'http://127.0.0.1:9222'

async function getCdpTarget() {
  const res = await fetch(`${CDP_URL}/json/list`)
  const targets = await res.json()
  const page = targets.find((t) => t.type === 'page')
  if (!page) throw new Error('No page target found in CDP')
  return page
}

async function sendCommand(ws, method, params = {}) {
  const id = Math.floor(Math.random() * 99999)
  return new Promise((resolve, reject) => {
    const timeout = setTimeout(() => reject(new Error(`Timeout: ${method}`)), 15000)
    const handler = (msg) => {
      const data = JSON.parse(msg.data ?? msg)
      if (data.id === id) {
        clearTimeout(timeout)
        ws.removeEventListener?.('message', handler)
        ws.off?.('message', handler)
        if (data.error) reject(new Error(JSON.stringify(data.error)))
        else resolve(data.result)
      }
    }
    ws.addEventListener ? ws.addEventListener('message', handler) : ws.on('message', handler)
    ws.send(JSON.stringify({ id, method, params }))
  })
}

const ROUTES_TO_SMOKE = [
  // CONNECT_EXISTING_API — connected this phase
  {
    role: 'SuperAdmin',
    route: '/super-admin/approvals/requests',
    label: 'AdminApplicationsQueue (connected)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'SuperAdmin',
    route: '/super-admin/approvals/reports',
    label: 'AdminApplicationReports (connected)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'SuperAdmin',
    route: '/super-admin/notifications/send',
    label: 'SendNotificationView (connected)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'BGH',
    route: '/bgh/profile',
    label: 'BGH ProfileView (connected account API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'BGH',
    route: '/bgh/notifications',
    label: 'BGH NotificationsView (connected notification API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Staff',
    route: '/staff/conflicts',
    label: 'ConflictCheckView (connected API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Staff',
    route: '/staff/schedule/pending',
    label: 'PendingSchedulesView (connected drafts API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Staff',
    route: '/staff/requests-history',
    label: 'RequestHistoryView (connected applications API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Staff',
    route: '/staff/notices/send',
    label: 'SendNoticeView (connected notifications API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Staff',
    route: '/staff/notices/history',
    label: 'NoticeHistoryView (connected notifications API)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  {
    role: 'Student',
    route: '/student/requests',
    label: 'Student RequestsView/StudentApplicationsHome (connected)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'PASS_LOAD_ONLY_ACTIONS_PENDING',
  },
  // WRAPPER_API_BACKED — reclassified without code change
  {
    role: 'SuperAdmin',
    route: '/super-admin/rewards-discipline',
    label: 'RewardDisciplineView wrapper (API backed via AwardsView)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'SuperAdmin',
    route: '/super-admin/rewards/campaigns',
    label: 'RewardCampaignsView wrapper (API backed)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'SuperAdmin',
    route: '/super-admin/discipline/records',
    label: 'DisciplineRecordsView wrapper (API backed)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'SuperAdmin',
    route: '/super-admin/discipline/appeals',
    label: 'DisciplineAppealsView wrapper (API backed)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'Staff',
    route: '/staff/dashboard',
    label: 'Staff Dashboard wrapper (API backed via useStaffDashboard)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'Student',
    route: '/student/exams',
    label: 'ExamsView wrapper (API backed via studentExamService)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'Student',
    route: '/student/notifications',
    label: 'Student NotificationsView wrapper',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'Teacher',
    route: '/teacher/notifications',
    label: 'Teacher NotificationsView wrapper',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'Parent',
    route: '/parent/dashboard',
    label: 'Parent DashboardWrapper (API backed)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
  {
    role: 'ContentCouncil',
    route: '/content-council/subjects',
    label: 'ContentCouncil SubjectListPage (API backed)',
    prevStatus: 'FE_ONLY_STATIC',
    newStatus: 'WRAPPER_API_BACKED',
  },
]

const LOGIN_CREDENTIALS = {
  SuperAdmin: { username: 'superadmin', password: 'Test@123' },
  BGH: { username: 'bghtest01', password: 'Test@123' },
  Staff: { username: 'stafftest01', password: 'Test@123' },
  Teacher: { username: 'teachertest01', password: 'Test@123' },
  Student: { username: 'studenttest01', password: 'Test@123' },
  Parent: { username: 'p15test_parent01@lms.local', password: 'Test@123' },
  ContentCouncil: { username: 'contentcounciltest01', password: 'Test@123' },
}

async function smokeRoute(ws, route) {
  const url = `${FRONTEND}${route.route}`
  const consoleErrors = []
  const networkErrors = []

  // Navigate to route
  await sendCommand(ws, 'Page.navigate', { url })
  await new Promise((r) => setTimeout(r, 3000))

  // Collect console errors
  const consoleListener = (msg) => {
    try {
      const data = JSON.parse(msg.data ?? msg)
      if (data.method === 'Runtime.consoleAPICalled' && data.params?.type === 'error') {
        const text = data.params.args?.map((a) => a.value || a.description || '').join(' ')
        if (text) consoleErrors.push(text)
      }
    } catch (_) {}
  }
  ws.addEventListener ? ws.addEventListener('message', consoleListener) : ws.on('message', consoleListener)

  // Enable Console domain
  await sendCommand(ws, 'Runtime.enable')

  // Evaluate for runtime errors and page title
  let evalResult
  try {
    evalResult = await sendCommand(ws, 'Runtime.evaluate', {
      expression: `({
        title: document.title,
        hasContent: document.body?.innerText?.trim().length > 100,
        hasError: !!document.querySelector('.error-state, [data-error], .error-banner'),
        url: window.location.href,
        routerPath: window.location.pathname
      })`,
      returnByValue: true,
    })
  } catch (e) {
    evalResult = { result: { value: { error: e.message } } }
  }

  ws.removeEventListener?.('message', consoleListener)
  ws.off?.('message', consoleListener)

  const pageInfo = evalResult?.result?.value || {}
  const pass =
    !networkErrors.some((e) => /40[134]|500/.test(e)) &&
    consoleErrors.length === 0 &&
    pageInfo.routerPath !== '/login'

  return {
    route: route.route,
    label: route.label,
    role: route.role,
    prevStatus: route.prevStatus,
    newStatus: route.newStatus,
    result: pass ? 'PASS' : 'FAIL',
    url: pageInfo.url || url,
    title: pageInfo.title || '',
    hasContent: pageInfo.hasContent || false,
    consoleErrors,
    networkErrors,
  }
}

async function main() {
  console.log('P16B.4A targeted smoke starting...')
  console.log(`CDP: ${CDP_URL}  Frontend: ${FRONTEND}`)

  let ws
  try {
    const { WebSocket } = await import('ws').catch(() => ({
      WebSocket: globalThis.WebSocket,
    }))

    const target = await getCdpTarget()
    ws = new WebSocket(target.webSocketDebuggerUrl)

    await new Promise((resolve, reject) => {
      ws.on ? ws.on('open', resolve) : (ws.onopen = resolve)
      ws.on ? ws.on('error', reject) : (ws.onerror = reject)
      setTimeout(reject, 10000)
    })

    await sendCommand(ws, 'Target.setDiscoverTargets', { discover: true })
    await sendCommand(ws, 'Page.enable')
    await sendCommand(ws, 'Network.enable')

    const results = []
    let passCount = 0
    let failCount = 0

    for (const route of ROUTES_TO_SMOKE) {
      console.log(`  → ${route.role} ${route.route}`)
      const result = await smokeRoute(ws, route)
      results.push(result)
      if (result.result === 'PASS') {
        passCount++
        console.log(`    ✓ PASS  [${result.newStatus}]`)
      } else {
        failCount++
        console.log(`    ✗ FAIL  consoleErrors=${result.consoleErrors.length} networkErrors=${result.networkErrors.length}`)
        if (result.consoleErrors.length) console.log('    console:', result.consoleErrors.slice(0, 2))
      }
    }

    ws.close ? ws.close() : ws.terminate?.()

    const summary = {
      phase: 'P16B.4A',
      date: new Date().toISOString(),
      frontend: FRONTEND,
      backend: 'http://localhost:5097',
      totalRoutes: results.length,
      pass: passCount,
      fail: failCount,
      results,
    }

    const outPath = resolve(__dir, 'p16b4a-results.json')
    writeFileSync(outPath, JSON.stringify(summary, null, 2), 'utf-8')
    console.log(`\nDone: ${passCount}/${results.length} PASS, ${failCount} FAIL`)
    console.log(`Results: ${outPath}`)

    if (failCount > 0) process.exit(1)
  } catch (err) {
    console.error('Smoke runner error:', err.message)
    if (ws) ws.close ? ws.close() : ws.terminate?.()
    process.exit(1)
  }
}

main()
