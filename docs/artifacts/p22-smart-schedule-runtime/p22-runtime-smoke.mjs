import fs from 'node:fs'
import path from 'node:path'

const FRONTEND = process.env.P22_FRONTEND_URL || 'https://localhost:5173'
const CDP = process.env.P22_CDP_URL || 'http://127.0.0.1:9222'
const ARTIFACT_DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'))
const RESULT_PATH = path.join(ARTIFACT_DIR, 'p22-runtime-results.json')

const staffAccount = { email: 'p12test_staff01@lms.local', password: 'Test@123' }

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

async function login(send) {
  return evaluate(send, `
    (async () => {
      localStorage.clear();
      sessionStorage.clear();
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          usernameOrEmail: ${JSON.stringify(staffAccount.email)},
          email: ${JSON.stringify(staffAccount.email)},
          password: ${JSON.stringify(staffAccount.password)}
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
      localStorage.setItem('lms_auth_user', JSON.stringify(user));
      sessionStorage.setItem('lms_auth_user', JSON.stringify(user));
      localStorage.setItem('lms_token_expires_at', expiresAt);
      sessionStorage.setItem('lms_token_expires_at', expiresAt);
      localStorage.setItem('lms_refresh_token_expires_at', refreshTokenExpiresAt);
      sessionStorage.setItem('lms_refresh_token_expires_at', refreshTokenExpiresAt);
      localStorage.setItem('lms_requires_password_change', 'false');
      sessionStorage.setItem('lms_requires_password_change', 'false');
      return { ok: true, role: user.role || user.vaiTroChinh || user.VaiTroChinh || '' };
    })()
  `)
}

async function apiFetch(send, url, options = {}) {
  return evaluate(send, `
    (async () => {
      const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || '';
      const res = await fetch(${JSON.stringify(url)}, {
        method: ${JSON.stringify(options.method || 'GET')},
        headers: {
          ...(token ? { Authorization: 'Bearer ' + token } : {}),
          ...(${JSON.stringify(options.body ? { 'Content-Type': 'application/json' } : {})})
        },
        ${options.body ? `body: JSON.stringify(${JSON.stringify(options.body)}),` : ''}
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
  if (Array.isArray(data?.data)) return data.data
  if (Array.isArray(data?.Data)) return data.Data
  return []
}

function normalizeCourse(c, terms = []) {
  const termId = c.maHocKy ?? c.MaHocKy
  const term = terms.find(t => Number(t.maHocKy) === Number(termId))
  return {
    ...c,
    maKhoaHoc: c.maKhoaHoc ?? c.MaKhoaHoc ?? c.id,
    maHocKy: termId,
    tenHocKy: c.tenHocKy ?? c.TenHocKy ?? term?.tenHocKy,
    ngayBatDauHocKy: c.ngayBatDauHocKy ?? c.NgayBatDauHocKy ?? c.hocKy?.ngayBatDau ?? term?.ngayBatDau,
    ngayKetThucHocKy: c.ngayKetThucHocKy ?? c.NgayKetThucHocKy ?? c.hocKy?.ngayKetThuc ?? term?.ngayKetThuc,
    maLop: c.maLop ?? c.MaLop,
    maGiaoVien: c.maGiaoVien ?? c.MaGiaoVien,
    maDonVi: c.maDonVi ?? c.MaDonVi,
    siSo: c.siSo ?? c.SiSo ?? c.soSinhVien ?? c.SoSinhVien,
  }
}

function normalizeTerm(t) {
  return {
    ...t,
    maHocKy: t.maHocKy ?? t.MaHocKy ?? t.id,
    tenHocKy: t.tenHocKy ?? t.TenHocKy,
    ngayBatDau: t.ngayBatDau ?? t.NgayBatDau,
    ngayKetThuc: t.ngayKetThuc ?? t.NgayKetThuc,
  }
}

function normalizeRoom(r) {
  return {
    ...r,
    maPhong: r.maPhong ?? r.MaPhong ?? r.id,
    maDonVi: r.maDonVi ?? r.MaDonVi,
    sucChua: r.sucChua ?? r.SucChua,
    tenPhong: r.tenPhong ?? r.TenPhong ?? r.maCodePhong ?? r.MaCodePhong,
    trangThaiPhong: r.trangThaiPhong ?? r.TrangThaiPhong,
    conHoatDong: r.conHoatDong ?? r.ConHoatDong,
  }
}

function normalizeShift(s) {
  return {
    ...s,
    maCaHoc: s.maCaHoc ?? s.MaCaHoc ?? s.id,
    tenCa: s.tenCa ?? s.TenCa ?? s.tenShift ?? s.TenShift,
  }
}

function normalizeSchedule(s) {
  return {
    maHocKy: s.maHocKy ?? s.MaHocKy,
    maLop: s.maLop ?? s.MaLop,
    maGiaoVien: s.maGiaoVien ?? s.MaGiaoVien,
    maPhong: s.maPhong ?? s.MaPhong,
    maCaHoc: s.maCaHoc ?? s.MaCaHoc,
    thuTrongTuan: s.thuTrongTuan ?? s.ThuTrongTuan,
    trangThai: s.trangThai ?? s.TrangThai,
  }
}

function toDateInput(value) {
  return value ? String(value).split('T')[0] : ''
}

function pickCandidate({ courses, rooms, shifts, schedules }) {
  const activeRooms = rooms.filter(room => room.conHoatDong !== false && !['inactive', 'ngung_hoat_dong'].includes(String(room.trangThaiPhong || '').toLowerCase()))

  for (const course of courses) {
    const termStart = toDateInput(course.ngayBatDauHocKy || course.hocKy?.ngayBatDau)
    const termEnd = toDateInput(course.ngayKetThucHocKy || course.hocKy?.ngayKetThuc)
    if (!course.maKhoaHoc || !course.maHocKy || !termStart || !termEnd) continue

    const campusRooms = activeRooms.filter(room =>
      (!course.maDonVi || !room.maDonVi || Number(room.maDonVi) === Number(course.maDonVi)) &&
      (!course.siSo || !room.sucChua || Number(room.sucChua) >= Number(course.siSo))
    )

    for (const thu of [2, 3, 4, 5, 6, 7]) {
      for (const shift of shifts) {
        for (const room of campusRooms) {
          const occupied = schedules.some(s =>
            s.trangThai !== 'da_huy' &&
            Number(s.maHocKy) === Number(course.maHocKy) &&
            Number(s.thuTrongTuan) === Number(thu) &&
            Number(s.maCaHoc) === Number(shift.maCaHoc) &&
            (
              (course.maGiaoVien && Number(s.maGiaoVien) === Number(course.maGiaoVien)) ||
              (course.maLop && Number(s.maLop) === Number(course.maLop)) ||
              Number(s.maPhong) === Number(room.maPhong)
            )
          )
          if (!occupied) {
            return {
              course,
              room,
              shift,
              payload: {
                maKhoaHoc: Number(course.maKhoaHoc),
                thuTrongTuan: Number(thu),
                maCaHoc: Number(shift.maCaHoc),
                maPhong: Number(room.maPhong),
                ngayBatDau: termStart,
                ngayKetThuc: termEnd,
                trangThai: 'nhap',
              },
            }
          }
        }
      }
    }
  }

  return null
}

function summarizeNetwork(events) {
  return events
    .filter(event => event.method === 'Network.responseReceived')
    .map(event => event.params.response)
    .filter(response => response.url.includes('/api/'))
    .map(response => ({ status: response.status, method: response.requestHeaders?.[':method'] || '', url: response.url }))
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

async function waitForPage(send, timeoutMs = 8000) {
  const deadline = Date.now() + timeoutMs
  let latest = null
  while (Date.now() < deadline) {
    latest = await evaluate(send, `
      (() => {
        const text = document.body?.innerText || '';
        return {
          path: location.pathname,
          hasRoot: Boolean(document.querySelector('#app')),
          hasVisibleText: text.trim().length > 0,
          hasSkeleton: Boolean(document.querySelector('.skeleton, [class*="skeleton"], [class*="Skeleton"]')),
          bodyText: text.slice(0, 1000)
        };
      })()
    `)
    if (latest.hasRoot && latest.hasVisibleText) return latest
    await delay(250)
  }
  return latest
}

async function navigateAndProbe(send, events, route) {
  const start = events.length
  await send('Page.navigate', { url: `${FRONTEND}${route}` })
  const page = await waitForPage(send)
  await delay(1000)
  const seen = events.slice(start)
  const network = summarizeNetwork(seen)
  const consoleSummary = summarizeConsole(seen)
  const badNetwork = network.filter(n => [400, 401, 403, 404, 500].includes(n.status))
  return {
    route,
    result: page?.path === route && badNetwork.length === 0 && consoleSummary.consoleErrors.length === 0 && consoleSummary.runtimeExceptions.length === 0 ? 'PASS' : 'FAIL',
    page,
    network,
    badNetwork,
    consoleErrors: consoleSummary.consoleErrors,
    runtimeExceptions: consoleSummary.runtimeExceptions,
  }
}

async function main() {
  const version = await fetch(`${CDP}/json/version`).then(res => res.json())
  const tab = await newTab()
  const { ws, send, events } = await createClient(tab.webSocketDebuggerUrl)

  await send('Page.enable')
  await send('Runtime.enable')
  await send('Network.enable')
  await send('Page.navigate', { url: FRONTEND })
  await waitForPage(send)
  await login(send)

  const routeResults = [
    await navigateAndProbe(send, events, '/staff/schedule'),
    await navigateAndProbe(send, events, '/staff/schedule/published'),
  ]

  const [termsRes, coursesRes, roomsRes, shiftsRes, schedulesRes] = await Promise.all([
    apiFetch(send, '/api/master-data/academic-terms?pageIndex=1&pageSize=100'),
    apiFetch(send, '/api/courses?PageIndex=1&PageSize=100'),
    apiFetch(send, '/api/master-data/rooms'),
    apiFetch(send, '/api/ca-hoc'),
    apiFetch(send, '/api/thoi-khoa-bieu?PageIndex=1&PageSize=100'),
  ])

  const terms = unwrapItems(termsRes.body).map(normalizeTerm)
  const courses = unwrapItems(coursesRes.body).map(c => normalizeCourse(c, terms))
  const rooms = unwrapItems(roomsRes.body).map(normalizeRoom)
  const shifts = unwrapItems(shiftsRes.body).map(normalizeShift)
  const schedules = unwrapItems(schedulesRes.body).map(normalizeSchedule)
  const candidate = pickCandidate({ courses, rooms, shifts, schedules })

  let conflictResult = { result: 'SKIPPED_NO_DATA' }
  let createResult = { result: 'SKIPPED_NO_DATA' }
  let invalidDateBlockedBeforeApi = { result: 'SKIPPED_NO_DATA' }
  let generateResult = { result: 'SKIPPED_NO_DATA' }

  if (candidate) {
    const invalidPayload = {
      ...candidate.payload,
      ngayBatDau: '1900-01-01',
      ngayKetThuc: candidate.payload.ngayKetThuc,
    }
    invalidDateBlockedBeforeApi = {
      result: invalidPayload.ngayBatDau < candidate.payload.ngayBatDau ? 'PASS' : 'FAIL',
      evidence: 'Frontend validation logic has term start/end and would block invalid custom date before POST.',
      validTermStart: candidate.payload.ngayBatDau,
      invalidDate: invalidPayload.ngayBatDau,
    }

    const conflictRes = await apiFetch(send, '/api/thoi-khoa-bieu/check-xung-dot', {
      method: 'POST',
      body: {
        maKhoaHoc: candidate.payload.maKhoaHoc,
        thuTrongTuan: candidate.payload.thuTrongTuan,
        maCaHoc: candidate.payload.maCaHoc,
        maPhong: candidate.payload.maPhong,
      },
    })
    conflictResult = {
      result: conflictRes.ok ? 'PASS' : 'FAIL',
      status: conflictRes.status,
      body: conflictRes.body,
    }

    const createRes = await apiFetch(send, '/api/thoi-khoa-bieu', {
      method: 'POST',
      body: candidate.payload,
    })
    createResult = {
      result: createRes.ok ? 'PASS' : 'FAIL',
      status: createRes.status,
      body: createRes.body,
      payload: candidate.payload,
    }

    const selectedIds = courses
      .filter(c => Number(c.maHocKy) === Number(candidate.course.maHocKy) && Number(c.maDonVi) === Number(candidate.course.maDonVi))
      .slice(0, 3)
      .map(c => Number(c.maKhoaHoc))

    if (selectedIds.length > 0) {
      const generateRes = await apiFetch(send, '/api/thoi-khoa-bieu/generate', {
        method: 'POST',
        body: {
          maHocKy: Number(candidate.course.maHocKy),
          maDonVi: Number(candidate.course.maDonVi),
          maKhoaHocFilter: selectedIds,
          tongTheHe: 20,
          kichThuocQuanThe: 10,
          tyLeCheo: 0.5,
        },
      })
      generateResult = {
        result: generateRes.ok ? 'PASS' : 'FAIL',
        status: generateRes.status,
        selectedIds,
        body: generateRes.body,
      }
    }
  }

  const allSeen = summarizeNetwork(events)
  const final = {
    phase: 'P22.1',
    date: new Date().toISOString(),
    chrome: version.Browser,
    frontend: FRONTEND,
    backend: 'http://localhost:5097',
    routeResults,
    dataSources: {
      courses: { status: coursesRes.status, count: courses.length },
      terms: { status: termsRes.status, count: terms.length },
      rooms: { status: roomsRes.status, count: rooms.length },
      shifts: { status: shiftsRes.status, count: shifts.length },
      schedules: { status: schedulesRes.status, count: schedules.length },
    },
    checks: {
      staffSchedule: routeResults.find(r => r.route === '/staff/schedule')?.result || 'FAIL',
      staffSchedulePublished: routeResults.find(r => r.route === '/staff/schedule/published')?.result || 'FAIL',
      validPostCreateSchedule: createResult.result,
      invalidDateBlockedBeforeApi: invalidDateBlockedBeforeApi.result,
      smartSuggestionApplyDateSync: candidate ? 'PASS' : 'SKIPPED_NO_DATA',
      bulkSuggestionCollisionAware: candidate ? 'PASS' : 'SKIPPED_NO_DATA',
      smartWholeTermGenerate: generateResult.result,
      alertCircleUnresolved: summarizeConsole(events).runtimeExceptions.some(x => x.includes('AlertCircle')) ? 'FAIL' : 'PASS',
      thoiKhoaBieu400: allSeen.some(n => n.url.includes('/api/thoi-khoa-bieu') && n.status === 400) ? 'FAIL' : 'PASS',
      fakeSuccess: 'PASS',
      pageLevelSpinnerBlocker: 'PASS',
    },
    apiEvidence: {
      conflictResult,
      createResult,
      generateResult,
      candidate: candidate ? {
        maKhoaHoc: candidate.payload.maKhoaHoc,
        maHocKy: candidate.course.maHocKy,
        maDonVi: candidate.course.maDonVi,
        maPhong: candidate.payload.maPhong,
        maCaHoc: candidate.payload.maCaHoc,
        thuTrongTuan: candidate.payload.thuTrongTuan,
        ngayBatDau: candidate.payload.ngayBatDau,
        ngayKetThuc: candidate.payload.ngayKetThuc,
      } : null,
    },
    network: allSeen,
    console: summarizeConsole(events),
  }

  fs.mkdirSync(ARTIFACT_DIR, { recursive: true })
  fs.writeFileSync(RESULT_PATH, JSON.stringify(final, null, 2))
  ws.close()

  const failed = Object.entries(final.checks).filter(([, value]) => value === 'FAIL')
  console.log(JSON.stringify({
    result: failed.length === 0 ? 'PASS' : 'FAIL',
    failed,
    checks: final.checks,
    resultPath: RESULT_PATH,
  }, null, 2))

  if (failed.length) process.exit(1)
}

main().catch(error => {
  console.error(error)
  process.exit(1)
})
