const VM_KEYWORDS = [
  'virtualbox',
  'vmware',
  'parallels',
  'qemu',
  'swiftshader',
  'llvmpipe',
  'microsoft basic render driver',
  'virtual',
  'svga',
  'vbox',
]


export const PROCTORING_LIVE_VIOLATIONS_KEY = 'proctoring_live_violations'

function hasBrowserApi() {
  return typeof window !== 'undefined' && typeof document !== 'undefined'
}

function readJson(key, fallback) {
  try {
    const value = localStorage.getItem(key)
    return value ? JSON.parse(value) : fallback
  } catch {
    return fallback
  }
}

function normalizeText(value) {
  return String(value || '').trim().toLowerCase()
}

function includesAnyKeyword(value, keywords) {
  const text = normalizeText(value)
  return keywords.some((keyword) => text.includes(keyword))
}

function makeCheck({
  id,
  label,
  description,
  status = 'pass',
  risk = 0,
  reason = '',
  detail = '',
  icon = 'Globe',
  details = {},
}) {
  return { id, label, description, status, risk, reason, detail: detail || reason, icon, details }
}

export function getWebGLInfo() {
  if (!hasBrowserApi()) {
    return { supported: false, vendor: null, renderer: null }
  }

  try {
    const canvas = document.createElement('canvas')
    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl')

    if (!gl) {
      return { supported: false, vendor: null, renderer: null }
    }

    const debugInfo = gl.getExtension('WEBGL_debug_renderer_info')
    const vendor = debugInfo ? gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL) : gl.getParameter(gl.VENDOR)
    const renderer = debugInfo
      ? gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL)
      : gl.getParameter(gl.RENDERER)

    return {
      supported: true,
      vendor: vendor || null,
      renderer: renderer || null,
    }
  } catch {
    return { supported: false, vendor: null, renderer: null }
  }
}

export function detectVirtualMachine(webglInfo = getWebGLInfo()) {
  const combinedGpuText = `${webglInfo.vendor || ''} ${webglInfo.renderer || ''}`
  const vmMatched = includesAnyKeyword(combinedGpuText, VM_KEYWORDS)

  if (vmMatched) {
    return makeCheck({
      id: 'env_vm',
      label: 'Máy ảo / GPU ảo',
      description: 'Kiểm tra WebGL vendor/renderer',
      status: 'fail',
      risk: 70,
      reason: 'Phát hiện dấu hiệu máy ảo hoặc GPU ảo. Vui lòng dùng máy thật để làm bài thi.',
      icon: 'Server',
      details: webglInfo,
    })
  }

  if (!webglInfo.supported) {
    return makeCheck({
      id: 'env_vm',
      label: 'Máy ảo / GPU ảo',
      description: 'Kiểm tra WebGL vendor/renderer',
      status: 'warning',
      risk: 30,
      reason: 'Không truy vấn được WebGL/GPU. Vui lòng kiểm tra lại trình duyệt và driver đồ họa.',
      icon: 'Server',
      details: webglInfo,
    })
  }

  return makeCheck({
    id: 'env_vm',
    label: 'Máy ảo / GPU ảo',
    description: 'Kiểm tra WebGL vendor/renderer',
    status: 'pass',
    risk: 0,
    reason: 'Không phát hiện dấu hiệu máy ảo hoặc GPU ảo.',
    icon: 'Server',
    details: webglInfo,
  })
}

export function detectRemoteDesktop(webglInfo = getWebGLInfo()) {
  if (!hasBrowserApi()) {
    return makeCheck({
      id: 'env_remote_desktop',
      label: 'Remote Desktop / hiển thị',
      description: 'Kiểm tra cấu hình màn hình và GPU',
      status: 'warning',
      risk: 30,
      reason: 'Không thể kiểm tra môi trường hiển thị trong ngữ cảnh hiện tại.',
      icon: 'MonitorCheck',
    })
  }

  const signs = []
  const colorDepth = Number(window.screen?.colorDepth || 0)
  const devicePixelRatio = Number(window.devicePixelRatio || 1)
  const hardwareConcurrency = Number(navigator.hardwareConcurrency || 0)
  const width = Number(window.screen?.width || 0)
  const height = Number(window.screen?.height || 0)
  const renderer = normalizeText(webglInfo.renderer)

  if (colorDepth > 0 && colorDepth < 24) signs.push('Độ sâu màu màn hình thấp hơn 24-bit.')
  if (!webglInfo.supported) signs.push('Không truy vấn được GPU/WebGL.')
  if (hardwareConcurrency > 0 && hardwareConcurrency <= 2) signs.push('Số luồng CPU thấp bất thường.')
  if (devicePixelRatio < 0.75 || devicePixelRatio > 4) signs.push('Device pixel ratio bất thường.')
  if (renderer.includes('microsoft basic render driver')) signs.push('Renderer là Microsoft Basic Render Driver.')
  if ((width > 0 && width < 1024) || (height > 0 && height < 700)) signs.push('Độ phân giải màn hình thấp bất thường.')

  const risk = Math.min(70, signs.length * 20)
  const status = risk >= 70 ? 'fail' : risk > 0 ? 'warning' : 'pass'
  const reason =
    signs.length > 0
      ? 'Phát hiện dấu hiệu môi trường điều khiển từ xa hoặc cấu hình hiển thị không đạt yêu cầu.'
      : 'Cấu hình hiển thị đạt yêu cầu cơ bản.'

  return makeCheck({
    id: 'env_remote_desktop',
    label: 'Remote Desktop / hiển thị',
    description: 'Kiểm tra colorDepth, DPR, CPU và WebGL',
    status,
    risk,
    reason,
    icon: 'MonitorCheck',
    details: { colorDepth, devicePixelRatio, hardwareConcurrency, width, height, signs, webglInfo },
  })
}

export function detectHeadlessBrowser(webglInfo = getWebGLInfo()) {
  if (!hasBrowserApi()) {
    return makeCheck({
      id: 'env_headless',
      label: 'Headless browser',
      description: 'Kiểm tra webdriver, user agent và navigator',
      status: 'warning',
      risk: 30,
      reason: 'Không thể kiểm tra headless browser trong ngữ cảnh hiện tại.',
      icon: 'Globe',
    })
  }

  const signs = []
  const userAgent = navigator.userAgent || ''
  const pluginsLength = navigator.plugins?.length ?? 0
  const languagesLength = navigator.languages?.length ?? 0
  const webdriver = navigator.webdriver === true
  const hasHeadlessChrome = /HeadlessChrome/i.test(userAgent)

  if (webdriver) signs.push('navigator.webdriver = true.')
  if (hasHeadlessChrome) signs.push('User agent có HeadlessChrome.')
  if (pluginsLength === 0) signs.push('Không có navigator.plugins.')
  if (languagesLength === 0) signs.push('navigator.languages rỗng.')
  if (!webglInfo.supported) signs.push('Không có WebGL.')

  const hardBlock = webdriver || hasHeadlessChrome
  const risk = hardBlock ? 70 : signs.length >= 3 ? 55 : signs.length > 0 ? 20 : 0

  return makeCheck({
    id: 'env_headless',
    label: 'Headless browser',
    description: 'Kiểm tra webdriver, HeadlessChrome, plugins và languages',
    status: hardBlock ? 'fail' : signs.length >= 3 ? 'warning' : 'pass',
    risk,
    reason: hardBlock
      ? 'Phát hiện trình duyệt tự động/headless. Vui lòng dùng Chrome hoặc Edge thông thường.'
      : signs.length > 0
        ? 'Phát hiện một số dấu hiệu trình duyệt tự động. Vui lòng kiểm tra lại môi trường.'
        : 'Không phát hiện dấu hiệu headless browser.',
    icon: 'Globe',
    details: { userAgent, pluginsLength, languagesLength, webdriver, signs },
  })
}

export async function detectExamGuardAgent() {
  if (!hasBrowserApi()) {
    return makeCheck({
      id: 'env_agent',
      label: 'ExamGuard Agent',
      description: 'Kiểm tra Agent hệ điều hành',
      status: 'warning',
      risk: 30,
      reason: 'Không thể kiểm tra Agent trong ngữ cảnh hiện tại.',
      icon: 'ShieldAlert',
    })
  }

  try {
    const response = await fetch('https://127.0.0.1:17892/check', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sessionId: 'preflight', apiBaseUrl: window.location.origin })
    })

    if (!response.ok) {
      return makeCheck({
        id: 'env_agent',
        label: 'ExamGuard Agent',
        description: 'Kiểm tra Agent hệ điều hành',
        status: 'fail',
        risk: 100,
        reason: 'ExamGuard Agent trả về lỗi. Vui lòng kiểm tra lại.',
        icon: 'ShieldAlert',
      })
    }

    const data = await response.json()

    if (!data.safe) {
      return makeCheck({
        id: 'env_agent',
        label: 'ExamGuard Agent',
        description: 'Kiểm tra phần mềm cấm',
        status: 'fail',
        risk: 100,
        reason: data.message || 'Phát hiện phần mềm bị cấm (Remote Desktop / Extension AI). Vui lòng tắt và thử lại.',
        icon: 'ShieldAlert',
        details: { detectedApps: data.detectedApps }
      })
    }

    return makeCheck({
      id: 'env_agent',
      label: 'ExamGuard Agent',
      description: 'Kiểm tra Agent hệ điều hành',
      status: 'pass',
      risk: 0,
      reason: 'Agent đang chạy và không phát hiện phần mềm bị cấm.',
      icon: 'ShieldCheck',
    })

  } catch {
    return makeCheck({
      id: 'env_agent',
      label: 'ExamGuard Agent',
      description: 'Kiểm tra Agent hệ điều hành',
      status: 'fail',
      risk: 100,
      reason: 'Chưa khởi động ExamGuard Agent. Vui lòng tải về và chạy ứng dụng trước khi thi.',
      icon: 'ShieldAlert',
    })
  }
}

function detectFullscreenSupport() {
  const supported = hasBrowserApi() && Boolean(document.documentElement?.requestFullscreen)

  return makeCheck({
    id: 'browser_fullscreen',
    label: 'Fullscreen API',
    description: 'Kiểm tra khả năng chuyển toàn màn hình',
    status: supported ? 'pass' : 'fail',
    risk: supported ? 0 : 70,
    reason: supported
      ? 'Trình duyệt hỗ trợ chế độ toàn màn hình.'
      : 'Trình duyệt không hỗ trợ chế độ toàn màn hình. Vui lòng dùng Chrome hoặc Edge mới.',
    icon: 'Maximize',
  })
}

function detectScreenShareSupport() {
  const supported = hasBrowserApi() && Boolean(navigator.mediaDevices?.getDisplayMedia)

  return makeCheck({
    id: 'browser_screen_share',
    label: 'Chia sẻ màn hình',
    description: 'Kiểm tra Screen Capture API',
    status: supported ? 'pass' : 'fail',
    risk: supported ? 0 : 70,
    reason: supported
      ? 'Trình duyệt hỗ trợ chia sẻ màn hình.'
      : 'Trình duyệt không hỗ trợ chia sẻ màn hình. Vui lòng dùng Chrome hoặc Edge mới.',
    icon: 'MonitorCheck',
  })
}

export async function runPreflightSecurityChecks() {
  const webglInfo = getWebGLInfo()
  const checks = [
    detectHeadlessBrowser(webglInfo),
    detectVirtualMachine(webglInfo),
    detectRemoteDesktop(webglInfo),
    await detectExamGuardAgent(),
    detectFullscreenSupport(),
    detectScreenShareSupport(),
  ]

  const riskScore = Math.min(
    100,
    checks.reduce((total, check) => total + Number(check.risk || 0), 0),
  )
  const hasHardFail = checks.some((check) => check.status === 'fail' && Number(check.risk || 0) >= 70)
  const riskLevel = riskScore >= 70 ? 'danger' : riskScore >= 40 ? 'warning' : 'safe'
  const blockedReasons = checks
    .filter((check) => check.status === 'fail' || (riskScore >= 70 && Number(check.risk || 0) > 0))
    .map((check) => check.reason)

  return {
    checks,
    riskScore,
    riskLevel,
    canEnter: !hasHardFail && riskScore < 70,
    blockedReasons: [...new Set(blockedReasons)],
  }
}

export function createViolation({
  examId,
  studentId = 'PS12345',
  studentName = 'Nguyễn Văn A',
  type,
  severity,
  message,
  details = {},
}) {
  return {
    id: crypto.randomUUID?.() || `${Date.now()}-${Math.random()}`,
    examId,
    studentId,
    studentName,
    type,
    severity,
    message,
    details,
    timestamp: new Date().toISOString(),
  }
}

export function saveViolationLog(examId, studentId, violations) {
  const normalizedViolations = Array.isArray(violations) ? violations : []
  localStorage.setItem(`exam_violations_${examId}_${studentId}`, JSON.stringify(normalizedViolations))

  const current = readJson(PROCTORING_LIVE_VIOLATIONS_KEY, [])
  const merged = [...normalizedViolations, ...current]
  const uniqueById = []
  const seen = new Set()

  for (const item of merged) {
    if (!item?.id || seen.has(item.id)) continue
    seen.add(item.id)
    uniqueById.push(item)
  }

  localStorage.setItem(PROCTORING_LIVE_VIOLATIONS_KEY, JSON.stringify(uniqueById.slice(0, 100)))
}

export function loadViolationLog(examId, studentId) {
  return readJson(`exam_violations_${examId}_${studentId}`, [])
}

export function clearExamRuntimeStorage(examId, studentId) {
  if (!examId || !studentId) return

  localStorage.removeItem(`exam_answers_${examId}_${studentId}`)
  localStorage.removeItem(`exam_flags_${examId}_${studentId}`)
  localStorage.removeItem(`exam_time_left_${examId}_${studentId}`)
  localStorage.removeItem(`exam_violations_${examId}_${studentId}`)
  localStorage.removeItem(`exam_last_saved_at_${examId}_${studentId}`)
  localStorage.removeItem(`exam_submitted_${examId}_${studentId}`)
}
