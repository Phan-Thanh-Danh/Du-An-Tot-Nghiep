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

const TRANSLATION_EXTENSION_SELECTORS = [
  '#goog-gt-tt',
  '.goog-te-banner-frame',
  '#goog-te-banner',
  'iframe[src*="translate.google"]',
  '#immersive-translate-popup',
  '[class*="immersive-translate"]',
  '[id*="immersive-translate"]',
  '[class*="deepl"]',
  '[id*="deepl"]',
]

const AI_WRITING_EXTENSION_SELECTORS = [
  '[data-gramm]',
  'grammarly-extension',
  '[class*="grammarly"]',
  '[id*="grammarly"]',
  '[class*="quillbot"]',
  '[id*="quillbot"]',
  '[class*="monica"]',
  '[id*="monica"]',
  '[class*="chatgpt"]',
  '[id*="chatgpt"]',
  '[class*="ai-assistant"]',
  '[id*="ai-assistant"]',
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

function findDomArtifacts(selectors) {
  if (!hasBrowserApi()) return []

  return selectors
    .map((selector) => {
      try {
        const node = document.querySelector(selector)
        return node ? { selector, tagName: node.tagName, id: node.id || '', className: String(node.className || '') } : null
      } catch {
        return null
      }
    })
    .filter(Boolean)
}

export function detectForbiddenExtensions() {
  const translationArtifacts = findDomArtifacts(TRANSLATION_EXTENSION_SELECTORS)
  const aiWritingArtifacts = findDomArtifacts(AI_WRITING_EXTENSION_SELECTORS)
  const artifacts = [...translationArtifacts, ...aiWritingArtifacts]

  if (artifacts.length > 0) {
    const category =
      translationArtifacts.length > 0 && aiWritingArtifacts.length > 0
        ? 'translation_ai'
        : translationArtifacts.length > 0
          ? 'translation'
          : 'ai_writing'

    return makeCheck({
      id: 'extensions_forbidden',
      label: 'Extension dịch / AI / viết',
      description: 'Quét DOM artifact của extension bị cấm',
      status: 'fail',
      risk: 70,
      reason: 'Phát hiện tiện ích có thể hỗ trợ dịch/AI/viết. Vui lòng tắt extension rồi kiểm tra lại.',
      icon: 'Puzzle',
      details: { category, artifacts },
    })
  }

  return makeCheck({
    id: 'extensions_forbidden',
    label: 'Extension dịch / AI / viết',
    description: 'Quét DOM artifact của extension bị cấm',
    status: 'pass',
    risk: 0,
    reason: 'Không phát hiện tiện ích dịch, AI hoặc viết văn bản trong DOM.',
    icon: 'Puzzle',
    details: { artifacts: [] },
  })
}

export function detectAdblockLikeExtensions() {
  return new Promise((resolve) => {
    if (!hasBrowserApi() || !document.body) {
      resolve(
        makeCheck({
          id: 'extensions_adblock',
          label: 'Adblock / Privacy',
          description: 'Kiểm tra bait element',
          status: 'warning',
          risk: 15,
          reason: 'Không thể kiểm tra tiện ích chặn nội dung ở thời điểm này.',
          icon: 'Puzzle',
        }),
      )
      return
    }

    const bait = document.createElement('div')
    bait.className = 'adsbox ad-banner ad-unit pub_300x250'
    bait.style.position = 'absolute'
    bait.style.left = '-9999px'
    bait.style.width = '300px'
    bait.style.height = '250px'
    bait.style.pointerEvents = 'none'
    bait.setAttribute('aria-hidden', 'true')
    document.body.appendChild(bait)

    window.setTimeout(() => {
      const style = window.getComputedStyle(bait)
      const hidden =
        bait.offsetHeight === 0 ||
        bait.offsetWidth === 0 ||
        style.display === 'none' ||
        style.visibility === 'hidden'

      bait.remove()

      resolve(
        hidden
          ? makeCheck({
              id: 'extensions_adblock',
              label: 'Adblock / Privacy',
              description: 'Kiểm tra bait element',
              status: 'warning',
              risk: 25,
              reason: 'Có tiện ích chặn nội dung có thể ảnh hưởng đến giám sát bài thi.',
              icon: 'Puzzle',
              details: { baitHidden: true },
            })
          : makeCheck({
              id: 'extensions_adblock',
              label: 'Adblock / Privacy',
              description: 'Kiểm tra bait element',
              status: 'pass',
              risk: 0,
              reason: 'Không phát hiện hành vi chặn nội dung qua bait element.',
              icon: 'Puzzle',
              details: { baitHidden: false },
            }),
      )
    }, 120)
  })
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
    detectForbiddenExtensions(),
    await detectAdblockLikeExtensions(),
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

export function saveViolationLog(examId, violations) {
  const normalizedViolations = Array.isArray(violations) ? violations : []
  localStorage.setItem(`exam_violations_${examId}`, JSON.stringify(normalizedViolations))

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

export function loadViolationLog(examId) {
  return readJson(`exam_violations_${examId}`, [])
}

export function clearExamRuntimeStorage(examId) {
  if (!examId) return

  localStorage.removeItem(`exam_answers_${examId}`)
  localStorage.removeItem(`exam_flags_${examId}`)
  localStorage.removeItem(`exam_time_left_${examId}`)
  localStorage.removeItem(`exam_violations_${examId}`)
  localStorage.removeItem(`exam_last_saved_at_${examId}`)
  localStorage.removeItem(`exam_submitted_${examId}`)
}
