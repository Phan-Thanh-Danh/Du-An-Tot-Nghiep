<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  AlertTriangle,
  ChevronLeft,
  ChevronRight,
  Clock,
  ClipboardX,
  Flag,
  HelpCircle,
  Lock,
  Maximize,
  MonitorCheck,
  PlayCircle,
  Save,
  Send,
  ShieldAlert,
  Terminal,
  XCircle,
} from 'lucide-vue-next'
import { mockExams, mockQuestions } from '@/data/exam.mock.js'
import {
  clearExamRuntimeStorage,
  createViolation,
  detectForbiddenExtensions,
  loadViolationLog,
  saveViolationLog,
} from '@/utils/examSecurity'

const route = useRoute()
const router = useRouter()

const STUDENT_ID = 'PS12345'
const STUDENT_NAME = 'Nguyễn Văn A'
const PREFLIGHT_MAX_AGE_MS = 2 * 60 * 60 * 1000

const examId = String(route.params.examId || 'exam-ctdl-002')
const exam = computed(() => {
  return mockExams.find((item) => item.id === examId) || {
    id: examId,
    title: 'Bài thi trắc nghiệm',
    subject: 'Môn học mẫu',
    subjectCode: 'MOCK101',
    classCode: 'MOCK-K28A',
    durationMinutes: 45,
    totalQuestions: mockQuestions.length,
  }
})

const answersKey = `exam_answers_${examId}`
const flagsKey = `exam_flags_${examId}`
const timeKey = `exam_time_left_${examId}`
const lastSavedKey = `exam_last_saved_at_${examId}`
const submittedKey = `exam_submitted_${examId}`

const preflightReady = ref(false)
const examStarted = ref(false)
const isFullscreen = ref(false)
const monitoringStatus = ref('idle') // idle | starting | active | interrupted | stopped
const isScreenSharePickerOpen = ref(false)
const keyboardLockActive = ref(false)
const screenStream = ref(null)
const startError = ref('')
const showConfirmSubmit = ref(false)
const restoredDraft = ref(false)
const lastSavedAt = ref('')
const currentQuestionIndex = ref(0)
const answers = ref({})
const flagged = ref({})
const violations = ref([])
const warnings = ref([])
const timeLeftSeconds = ref(Number(exam.value.durationMinutes || 45) * 60)
const fullscreenExitCount = ref(0)
const tabSwitchCount = ref(0)
const currentTimestamp = ref('')

let timerInterval = null
let autosaveInterval = null
let runtimeScanInterval = null
let watermarkInterval = null
let devtoolsFallbackInterval = null
let devtoolsDetectorModule = null
let devtoolsListener = null
let streamRecoveryTimer = null
let screenTrack = null
let submitLocked = false
let lastBlurViolationAt = 0
let suppressFocusViolationUntil = 0
const lastViolationByType = new Map()

const currentQuestion = computed(() => mockQuestions[currentQuestionIndex.value] || mockQuestions[0])

const answeredCount = computed(() => mockQuestions.filter((question) => isAnswered(question)).length)
const flaggedCount = computed(() => Object.values(flagged.value).filter(Boolean).length)
const unansweredCount = computed(() => Math.max(mockQuestions.length - answeredCount.value, 0))
const progressPercent = computed(() => Math.round((answeredCount.value / mockQuestions.length) * 100))
const recentViolations = computed(() => violations.value.slice(0, 3))
const criticalViolationCount = computed(() =>
  violations.value.filter((item) => item.severity === 'critical' || item.severity === 'high').length,
)

const formattedTimeLeft = computed(() => {
  const mins = Math.floor(timeLeftSeconds.value / 60)
  const secs = timeLeftSeconds.value % 60
  return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`
})

const monitoringStatusLabel = computed(() => {
  if (monitoringStatus.value === 'active') return 'Đang giám sát'
  if (monitoringStatus.value === 'starting') return 'Đang khởi động'
  if (monitoringStatus.value === 'interrupted') return 'Gián đoạn'
  if (monitoringStatus.value === 'stopped') return 'Đã dừng'
  return 'Chưa bắt đầu'
})

const keyboardLockLabel = computed(() => {
  return keyboardLockActive.value ? 'Đang bật' : 'Theo dõi'
})

const watermarkText = computed(() => {
  return `${STUDENT_ID} • ${STUDENT_NAME} • ${currentTimestamp.value}`
})

function readJson(key, fallback) {
  try {
    const value = localStorage.getItem(key)
    return value ? JSON.parse(value) : fallback
  } catch {
    return fallback
  }
}

function isAnswered(question) {
  const value = answers.value[question.id]
  if (Array.isArray(value)) return value.length > 0
  return value !== undefined && value !== null && String(value).trim() !== ''
}

function formatTime(value) {
  if (!value) return '—'
  return new Date(value).toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

function updateWatermarkTimestamp() {
  currentTimestamp.value = new Date().toLocaleString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

function validatePreflightToken() {
  try {
    const raw = sessionStorage.getItem(`exam_preflight_passed_${examId}`)
    if (!raw) return false
    const payload = JSON.parse(raw)
    return payload?.passed === true && Date.now() - Number(payload.passedAt || 0) <= PREFLIGHT_MAX_AGE_MS
  } catch {
    return false
  }
}

function restoreDraft() {
  if (localStorage.getItem(submittedKey) === 'true') {
    clearExamRuntimeStorage(examId)
    return
  }

  const restoredAnswers = readJson(answersKey, null)
  const restoredFlags = readJson(flagsKey, null)
  const restoredTime = Number(localStorage.getItem(timeKey) || 0)
  const restoredViolations = loadViolationLog(examId)
  const restoredSavedAt = localStorage.getItem(lastSavedKey) || ''

  if (restoredAnswers && typeof restoredAnswers === 'object') {
    answers.value = restoredAnswers
    restoredDraft.value = true
  }

  if (restoredFlags && typeof restoredFlags === 'object') {
    flagged.value = restoredFlags
    restoredDraft.value = true
  }

  if (restoredTime > 0) {
    timeLeftSeconds.value = restoredTime
    restoredDraft.value = true
  }

  if (Array.isArray(restoredViolations)) {
    violations.value = restoredViolations
  }

  if (restoredSavedAt) {
    lastSavedAt.value = restoredSavedAt
  }
}

function saveDraft() {
  if (submitLocked) return

  const savedAt = new Date().toISOString()
  localStorage.setItem(answersKey, JSON.stringify(answers.value))
  localStorage.setItem(flagsKey, JSON.stringify(flagged.value))
  localStorage.setItem(timeKey, String(timeLeftSeconds.value))
  localStorage.setItem(lastSavedKey, savedAt)
  saveViolationLog(examId, violations.value)
  lastSavedAt.value = savedAt
}

function pushWarning(message, severity = 'high', action = '') {
  warnings.value.unshift({
    id: crypto.randomUUID?.() || `${Date.now()}-${Math.random()}`,
    message,
    severity,
    action,
    timestamp: new Date().toISOString(),
  })
  warnings.value = warnings.value.slice(0, 5)
}

function addViolation(type, severity, message, details = {}, options = {}) {
  const now = Date.now()
  const dedupeMs = options.dedupeMs ?? 0
  const lastAt = lastViolationByType.get(type) || 0

  if (dedupeMs > 0 && now - lastAt < dedupeMs) {
    return null
  }

  lastViolationByType.set(type, now)

  const violation = createViolation({
    examId,
    studentId: STUDENT_ID,
    studentName: STUDENT_NAME,
    type,
    severity,
    message,
    details,
  })

  violations.value = [violation, ...violations.value]
  saveViolationLog(examId, violations.value)
  return violation
}

function selectChoice(questionId, choiceId) {
  answers.value = {
    ...answers.value,
    [questionId]: choiceId,
  }
}

function toggleMultiChoice(questionId, choiceId) {
  const currentArr = Array.isArray(answers.value[questionId]) ? answers.value[questionId] : []
  const nextArr = currentArr.includes(choiceId)
    ? currentArr.filter((item) => item !== choiceId)
    : [...currentArr, choiceId]

  answers.value = {
    ...answers.value,
    [questionId]: nextArr,
  }
}

function updateTextAnswer(questionId, value) {
  answers.value = {
    ...answers.value,
    [questionId]: value,
  }
}

function toggleFlag(questionId) {
  flagged.value = {
    ...flagged.value,
    [questionId]: !flagged.value[questionId],
  }
}

function nextQuestion() {
  if (currentQuestionIndex.value < mockQuestions.length - 1) {
    currentQuestionIndex.value++
  }
}

function prevQuestion() {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
  }
}

async function startExamEnvironment() {
  if (monitoringStatus.value === 'starting') return

  startError.value = ''
  monitoringStatus.value = 'starting'

  try {
    await requestScreenShare()
    await enterExamFullscreen()
    examStarted.value = true
    monitoringStatus.value = 'active'
    startTimer()
    startRuntimeMonitoring()
    saveDraft()
  } catch (error) {
    monitoringStatus.value = 'idle'
    cleanupScreenStream(false)
    startError.value = error?.message || 'Bạn cần chia sẻ màn hình và bật toàn màn hình để bắt đầu bài thi.'
    if (document.fullscreenElement) {
      await document.exitFullscreen().catch(() => {})
    }
  }
}

async function enterExamFullscreen() {
  if (document.fullscreenElement) {
    isFullscreen.value = true
    await lockExamKeyboard()
    return
  }

  const root = document.documentElement
  if (!root.requestFullscreen) {
    throw new Error('Trình duyệt không hỗ trợ chế độ toàn màn hình.')
  }

  try {
    await root.requestFullscreen({ navigationUI: 'hide' })
  } catch {
    await root.requestFullscreen()
  }

  isFullscreen.value = Boolean(document.fullscreenElement)
  if (!document.fullscreenElement) {
    throw new Error('Không thể mở toàn màn hình. Vui lòng cho phép fullscreen để vào phòng thi.')
  }

  await lockExamKeyboard()
}

async function lockExamKeyboard() {
  if (!navigator.keyboard?.lock) {
    keyboardLockActive.value = false
    return false
  }

  try {
    await navigator.keyboard.lock([
      'AltLeft',
      'AltRight',
      'ControlLeft',
      'ControlRight',
      'MetaLeft',
      'MetaRight',
      'ShiftLeft',
      'ShiftRight',
      'Tab',
      'PageUp',
      'PageDown',
      'ArrowLeft',
      'ArrowRight',
      'Escape',
      'F4',
      'F6',
      'F11',
      'KeyC',
      'KeyD',
      'KeyI',
      'KeyJ',
      'KeyL',
      'KeyN',
      'KeyP',
      'KeyR',
      'KeyS',
      'KeyT',
      'KeyW',
    ])
    keyboardLockActive.value = true
    return true
  } catch {
    keyboardLockActive.value = false
    return false
  }
}

function unlockExamKeyboard() {
  try {
    navigator.keyboard?.unlock?.()
  } catch {
    // Browser support differs; cleanup is best-effort.
  }

  keyboardLockActive.value = false
}

async function requestScreenShare() {
  if (!navigator.mediaDevices?.getDisplayMedia) {
    throw new Error('Trình duyệt không hỗ trợ chia sẻ màn hình.')
  }

  isScreenSharePickerOpen.value = true
  suppressFocusViolationUntil = Date.now() + 3000

  try {
    const stream = await navigator.mediaDevices.getDisplayMedia({
      video: { frameRate: 5 },
      audio: false,
    })

    attachScreenStream(stream)
  } finally {
    isScreenSharePickerOpen.value = false
    suppressFocusViolationUntil = Date.now() + 2500
  }
}

function attachScreenStream(stream) {
  cleanupScreenStream(false)
  screenStream.value = stream
  screenTrack = stream.getVideoTracks()[0] || null

  if (screenTrack) {
    screenTrack.addEventListener('ended', handleScreenTrackEnded)
  }

  if (streamRecoveryTimer) {
    clearTimeout(streamRecoveryTimer)
    streamRecoveryTimer = null
  }

  monitoringStatus.value = 'active'
}

function cleanupScreenStream(markStopped = true) {
  if (screenTrack) {
    screenTrack.removeEventListener('ended', handleScreenTrackEnded)
    screenTrack = null
  }

  if (screenStream.value) {
    screenStream.value.getTracks().forEach((track) => track.stop())
    screenStream.value = null
  }

  if (markStopped && !submitLocked) {
    monitoringStatus.value = 'stopped'
  }
}

function handleScreenTrackEnded() {
  if (!examStarted.value || submitLocked) return

  monitoringStatus.value = 'interrupted'
  addViolation('SCREEN_STREAM_STOPPED', 'critical', 'Luồng chia sẻ màn hình bị ngắt')
  pushWarning('Luồng chia sẻ màn hình bị ngắt. Vui lòng bật lại trong vòng 10 giây.', 'critical', 'screen')

  if (streamRecoveryTimer) clearTimeout(streamRecoveryTimer)
  streamRecoveryTimer = window.setTimeout(() => {
    if (monitoringStatus.value !== 'active') {
      pushWarning('Luồng chia sẻ màn hình vẫn chưa được bật lại. Giám thị có thể đình chỉ bài thi.', 'critical', 'screen')
    }
  }, 10000)
}

async function restartScreenShare() {
  try {
    startError.value = ''
    monitoringStatus.value = 'starting'
    await requestScreenShare()
    pushWarning('Chia sẻ màn hình đã được bật lại.', 'low')
  } catch (error) {
    monitoringStatus.value = 'interrupted'
    startError.value = error?.message || 'Không thể bật lại chia sẻ màn hình.'
  }
}

async function resumeFullscreen() {
  try {
    await enterExamFullscreen()
  } catch {
    pushWarning('Không thể bật lại toàn màn hình. Vui lòng thử lại.', 'high', 'fullscreen')
  }
}

function startTimer() {
  if (timerInterval) clearInterval(timerInterval)

  timerInterval = window.setInterval(() => {
    if (!examStarted.value) return

    if (timeLeftSeconds.value > 0) {
      timeLeftSeconds.value--
    } else {
      clearInterval(timerInterval)
      timerInterval = null
      submitExam('timeout')
    }
  }, 1000)
}

function startRuntimeMonitoring() {
  if (!autosaveInterval) {
    autosaveInterval = window.setInterval(saveDraft, 30000)
  }

  if (!runtimeScanInterval) {
    runtimeScanInterval = window.setInterval(scanForbiddenExtensionsRuntime, 15000)
  }

  scanForbiddenExtensionsRuntime()
  startDevtoolsDetection()
}

function scanForbiddenExtensionsRuntime() {
  if (!examStarted.value || submitLocked) return

  const result = detectForbiddenExtensions()
  if (result.status === 'fail') {
    addViolation(
      'FORBIDDEN_EXTENSION_RUNTIME',
      'critical',
      'Phát hiện extension bị cấm trong lúc thi',
      result.details,
      { dedupeMs: 15000 },
    )
    pushWarning('Phát hiện extension bị cấm trong lúc thi.', 'critical')
  }
}

async function startDevtoolsDetection() {
  if (devtoolsDetectorModule || devtoolsFallbackInterval) return

  try {
    devtoolsDetectorModule = await import('devtools-detector')
    devtoolsListener = (isOpen, detail) => {
      if (!isOpen || !examStarted.value || submitLocked) return

      addViolation(
        'DEVTOOLS_OPENED',
        'critical',
        'Phát hiện dấu hiệu mở Developer Tools',
        detail || {},
        { dedupeMs: 10000 },
      )
      pushWarning('Phát hiện dấu hiệu mở Developer Tools.', 'critical')
    }
    devtoolsDetectorModule.addListener(devtoolsListener)
    devtoolsDetectorModule.setDetectDelay?.(1000)
    devtoolsDetectorModule.launch()
  } catch {
    devtoolsFallbackInterval = window.setInterval(() => {
      if (!examStarted.value || submitLocked) return

      const threshold = 160
      const widthDiff = window.outerWidth - window.innerWidth
      const heightDiff = window.outerHeight - window.innerHeight
      const suspected = widthDiff > threshold || heightDiff > threshold

      if (suspected) {
        addViolation(
          'DEVTOOLS_OPENED',
          'critical',
          'Phát hiện dấu hiệu mở Developer Tools',
          { widthDiff, heightDiff, detector: 'fallback' },
          { dedupeMs: 10000 },
        )
        pushWarning('Phát hiện dấu hiệu mở Developer Tools.', 'critical')
      }
    }, 1500)
  }
}

function handleFullscreenChange() {
  isFullscreen.value = Boolean(document.fullscreenElement)

  if (document.fullscreenElement && examStarted.value && !submitLocked) {
    void lockExamKeyboard()
  } else if (!document.fullscreenElement) {
    unlockExamKeyboard()
  }

  if (!examStarted.value || submitLocked) return

  if (!document.fullscreenElement) {
    fullscreenExitCount.value++
    addViolation('FULLSCREEN_EXIT', 'high', 'Học sinh thoát chế độ toàn màn hình', {
      count: fullscreenExitCount.value,
    })
    pushWarning('Bạn đã thoát toàn màn hình. Hành vi này đã được ghi nhận.', 'critical', 'fullscreen')

    if (fullscreenExitCount.value >= 3) {
      pushWarning('Bạn đã thoát toàn màn hình nhiều lần. Giám thị có thể đình chỉ bài thi.', 'critical', 'fullscreen')
    }
  }
}

function shouldIgnoreFocusViolation() {
  return isScreenSharePickerOpen.value || Date.now() < suppressFocusViolationUntil
}

function handleVisibilityChange() {
  if (!examStarted.value || submitLocked) return
  if (shouldIgnoreFocusViolation()) return

  if (document.hidden) {
    tabSwitchCount.value++
    addViolation('TAB_SWITCH', 'high', 'Học sinh rời khỏi tab thi', {
      count: tabSwitchCount.value,
      source: 'visibilitychange',
    })
  } else if (tabSwitchCount.value > 0) {
    pushWarning('Bạn đã rời khỏi tab thi. Hành vi này đã được ghi nhận.', 'high')
  }
}

function handleWindowBlur() {
  if (!examStarted.value || submitLocked || document.hidden) return
  if (shouldIgnoreFocusViolation()) return

  const now = Date.now()
  if (now - lastBlurViolationAt < 5000) return
  lastBlurViolationAt = now
  tabSwitchCount.value++
  addViolation('TAB_SWITCH', 'high', 'Học sinh rời khỏi cửa sổ thi', {
    count: tabSwitchCount.value,
    source: 'window.blur',
  })
}

function blockClipboard(event) {
  if (!examStarted.value || submitLocked) return

  event.preventDefault()
  addViolation('CLIPBOARD_ATTEMPT', 'medium', 'Thao tác sao chép/dán bị chặn trong bài thi', {
    eventType: event.type,
  })
  pushWarning('Thao tác sao chép/dán bị chặn trong bài thi.', 'medium')
}

function blockContextMenu(event) {
  if (!examStarted.value || submitLocked) return

  event.preventDefault()
  addViolation('CONTEXT_MENU', 'low', 'Thao tác chuột phải bị chặn', {
    eventType: event.type,
  })
}

function blockRestrictedShortcut(event) {
  if (!examStarted.value || submitLocked) return

  const shortcut = getRestrictedShortcut(event)
  if (!shortcut) return

  event.preventDefault()
  event.stopPropagation()

  const violation = addViolation(
    'KEYBOARD_SHORTCUT_ATTEMPT',
    shortcut.severity,
    shortcut.message,
    {
      shortcut: shortcut.name,
      key: event.key,
      code: event.code,
      altKey: event.altKey,
      metaKey: event.metaKey,
      ctrlKey: event.ctrlKey,
      shiftKey: event.shiftKey,
      repeat: event.repeat,
      keyboardLockActive: keyboardLockActive.value,
    },
    { dedupeMs: event.repeat ? 1200 : 0 },
  )

  if (violation) {
    pushWarning(shortcut.warning, shortcut.severity)
  }
}

function getRestrictedShortcut(event) {
  const key = String(event.key || '').toLowerCase()
  const code = String(event.code || '').toLowerCase()
  const ctrlOrMeta = event.ctrlKey || event.metaKey
  const isTabKey = key === 'tab' || code === 'tab'
  const isPageNavKey = key === 'pageup' || key === 'pagedown' || code === 'pageup' || code === 'pagedown'
  const isHorizontalArrow =
    key === 'arrowleft' || key === 'arrowright' || code === 'arrowleft' || code === 'arrowright'
  const isF4 = key === 'f4' || code === 'f4'
  const isF6 = key === 'f6' || code === 'f6'
  const isF11 = key === 'f11' || code === 'f11'
  const isEscape = key === 'escape' || code === 'escape'

  if (event.altKey && isTabKey) {
    return {
      name: 'ALT_TAB',
      severity: 'critical',
      message: 'Học sinh cố dùng Alt+Tab để chuyển cửa sổ',
      warning: 'Alt+Tab bị chặn trong bài thi.',
    }
  }

  if (event.metaKey && isTabKey) {
    return {
      name: 'META_TAB',
      severity: 'critical',
      message: 'Học sinh cố dùng phím hệ thống để chuyển cửa sổ',
      warning: 'Tổ hợp chuyển cửa sổ bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && isTabKey) {
    return {
      name: 'CTRL_TAB',
      severity: 'high',
      message: 'Học sinh cố dùng Ctrl+Tab để chuyển tab',
      warning: 'Ctrl+Tab bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && isPageNavKey) {
    return {
      name: 'CTRL_PAGE_TAB_NAV',
      severity: 'high',
      message: 'Học sinh cố dùng Ctrl+PageUp/PageDown để chuyển tab',
      warning: 'Tổ hợp chuyển tab bị chặn trong bài thi.',
    }
  }

  if (event.altKey && isHorizontalArrow) {
    return {
      name: 'ALT_HISTORY_NAV',
      severity: 'high',
      message: 'Học sinh cố dùng Alt+Arrow để điều hướng khỏi trang thi',
      warning: 'Tổ hợp điều hướng trình duyệt bị chặn trong bài thi.',
    }
  }

  if (event.altKey && isF4) {
    return {
      name: 'ALT_F4',
      severity: 'critical',
      message: 'Học sinh cố dùng Alt+F4 để đóng cửa sổ thi',
      warning: 'Alt+F4 bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && (key === 'w' || isF4)) {
    return {
      name: 'CLOSE_TAB',
      severity: 'critical',
      message: 'Học sinh cố đóng tab thi bằng bàn phím',
      warning: 'Tổ hợp đóng tab bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && ['t', 'n'].includes(key)) {
    return {
      name: 'NEW_TAB_OR_WINDOW',
      severity: 'high',
      message: 'Học sinh cố mở tab/cửa sổ mới bằng bàn phím',
      warning: 'Tổ hợp mở tab hoặc cửa sổ mới bị chặn trong bài thi.',
    }
  }

  if ((ctrlOrMeta && ['l', 'd'].includes(key)) || (event.altKey && key === 'd') || isF6) {
    return {
      name: 'ADDRESS_BAR_FOCUS',
      severity: 'high',
      message: 'Học sinh cố chuyển focus ra thanh địa chỉ trình duyệt',
      warning: 'Tổ hợp chuyển ra thanh địa chỉ bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && key === 'r') {
    return {
      name: 'REFRESH_PAGE',
      severity: 'high',
      message: 'Học sinh cố tải lại trang thi bằng bàn phím',
      warning: 'Tổ hợp tải lại trang bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && ['p', 's'].includes(key)) {
    return {
      name: 'PRINT_OR_SAVE',
      severity: 'high',
      message: 'Học sinh cố in hoặc lưu trang thi bằng bàn phím',
      warning: 'Tổ hợp in/lưu trang bị chặn trong bài thi.',
    }
  }

  if (ctrlOrMeta && event.shiftKey && ['i', 'j', 'c'].includes(key)) {
    return {
      name: 'DEVTOOLS_SHORTCUT',
      severity: 'critical',
      message: 'Học sinh cố mở Developer Tools bằng bàn phím',
      warning: 'Tổ hợp mở Developer Tools bị chặn trong bài thi.',
    }
  }

  if (isF11) {
    return {
      name: 'FULLSCREEN_TOGGLE',
      severity: 'high',
      message: 'Học sinh cố bật/tắt fullscreen bằng phím F11',
      warning: 'F11 bị chặn trong bài thi.',
    }
  }

  if (isEscape) {
    return {
      name: 'ESCAPE_FULLSCREEN',
      severity: 'high',
      message: 'Học sinh cố dùng Escape trong phòng thi toàn màn hình',
      warning: 'Phím Escape bị chặn trong bài thi.',
    }
  }

  return null
}

function handleBeforeUnload(event) {
  if (!examStarted.value || submitLocked) return

  addViolation(
    'KEYBOARD_SHORTCUT_ATTEMPT',
    'critical',
    'Học sinh cố đóng, tải lại hoặc rời trang thi',
    {
      shortcut: 'PAGE_UNLOAD_OR_REFRESH',
      keyboardLockActive: keyboardLockActive.value,
    },
    { dedupeMs: 1000 },
  )
  saveDraft()
  event.preventDefault()
  event.returnValue = ''
}

function buildSubmitPayload(reason) {
  return {
    examId,
    studentId: STUDENT_ID,
    studentName: STUDENT_NAME,
    answers: answers.value,
    flagged: flagged.value,
    violations: violations.value,
    submittedAt: new Date().toISOString(),
    timeLeftSeconds: timeLeftSeconds.value,
    submitReason: reason,
  }
}

async function submitExam(reason = 'manual') {
  if (submitLocked) return

  submitLocked = true
  showConfirmSubmit.value = false
  saveDraft()

  const payload = buildSubmitPayload(reason)
  sessionStorage.setItem('last_exam_submit_payload', JSON.stringify(payload))
  console.log('Mock exam submit payload:', payload)

  if (timerInterval) clearInterval(timerInterval)
  if (autosaveInterval) clearInterval(autosaveInterval)
  if (runtimeScanInterval) clearInterval(runtimeScanInterval)
  unlockExamKeyboard()
  cleanupScreenStream(false)
  monitoringStatus.value = 'stopped'
  localStorage.setItem(submittedKey, 'true')
  clearExamRuntimeStorage(examId)

  if (document.fullscreenElement) {
    await document.exitFullscreen().catch(() => {})
  }

  router.push(`/student/exams/result-${examId}`)
}

function stopDevtoolsDetection() {
  if (devtoolsFallbackInterval) {
    clearInterval(devtoolsFallbackInterval)
    devtoolsFallbackInterval = null
  }

  try {
    if (devtoolsDetectorModule && devtoolsListener) {
      devtoolsDetectorModule.removeListener(devtoolsListener)
      devtoolsDetectorModule.stop?.()
    }
  } catch {
    // Best-effort cleanup only.
  }
}

function addGlobalListeners() {
  document.addEventListener('fullscreenchange', handleFullscreenChange)
  document.addEventListener('visibilitychange', handleVisibilityChange)
  window.addEventListener('blur', handleWindowBlur)
  document.addEventListener('copy', blockClipboard)
  document.addEventListener('paste', blockClipboard)
  document.addEventListener('cut', blockClipboard)
  document.addEventListener('contextmenu', blockContextMenu)
  document.addEventListener('keydown', blockRestrictedShortcut, true)
  window.addEventListener('beforeunload', handleBeforeUnload)
}

function removeGlobalListeners() {
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  window.removeEventListener('blur', handleWindowBlur)
  document.removeEventListener('copy', blockClipboard)
  document.removeEventListener('paste', blockClipboard)
  document.removeEventListener('cut', blockClipboard)
  document.removeEventListener('contextmenu', blockContextMenu)
  document.removeEventListener('keydown', blockRestrictedShortcut, true)
  window.removeEventListener('beforeunload', handleBeforeUnload)
}

onMounted(() => {
  if (!validatePreflightToken()) {
    router.replace(`/student/exams/detail/${examId}`)
    return
  }

  preflightReady.value = true
  restoreDraft()
  updateWatermarkTimestamp()
  watermarkInterval = window.setInterval(updateWatermarkTimestamp, 1000)
  addGlobalListeners()
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
  if (autosaveInterval) clearInterval(autosaveInterval)
  if (runtimeScanInterval) clearInterval(runtimeScanInterval)
  if (watermarkInterval) clearInterval(watermarkInterval)
  if (streamRecoveryTimer) clearTimeout(streamRecoveryTimer)
  stopDevtoolsDetection()
  unlockExamKeyboard()
  removeGlobalListeners()
  cleanupScreenStream(false)
})
</script>

<template>
  <div v-if="!preflightReady" class="exam-take-page">
    <div class="glass-card loading-card">
      <Lock :size="18" />
      <span>Đang kiểm tra quyền vào phòng thi...</span>
    </div>
  </div>

  <div v-else class="exam-take-page">
    <div class="watermark-layer" aria-hidden="true">
      <span v-for="item in 42" :key="item">{{ watermarkText }}</span>
    </div>

    <header class="exam-header-strip glass-card">
      <div class="exam-title-meta">
        <div class="badge-exam">PHÒNG THI TRỰC TUYẾN #{{ examId }}</div>
        <h1>{{ exam.title }}</h1>
        <p>{{ exam.subjectCode }} · {{ exam.subject }} · {{ exam.classCode || 'CNTT-K28A' }}</p>
      </div>

      <div class="header-status-group">
        <div class="monitoring-pill" :class="`status-${monitoringStatus}`">
          <MonitorCheck :size="16" />
          <span>Chia sẻ màn hình: {{ monitoringStatus === 'active' ? 'Đang hoạt động' : monitoringStatusLabel }}</span>
        </div>
        <div class="exam-timer-block" :class="{ warning: timeLeftSeconds < 300 }">
          <Clock :size="20" class="timer-icon" />
          <div class="timer-values">
            <span class="timer-label">THỜI GIAN CÒN LẠI</span>
            <span class="timer-countdown">{{ formattedTimeLeft }}</span>
          </div>
        </div>
      </div>
    </header>

    <div v-if="restoredDraft" class="warning-banner info">
      <Save :size="16" />
      <span>Đã khôi phục bài làm tạm thời.</span>
    </div>

    <div v-if="startError" class="warning-banner critical">
      <XCircle :size="16" />
      <span>{{ startError }}</span>
    </div>

    <div v-for="warning in warnings" :key="warning.id" class="warning-banner" :class="warning.severity">
      <AlertTriangle :size="16" />
      <span>{{ warning.message }}</span>
      <button v-if="warning.action === 'fullscreen'" type="button" @click="resumeFullscreen">
        Bật lại toàn màn hình
      </button>
      <button v-else-if="warning.action === 'screen'" type="button" @click="restartScreenShare">
        Bật lại chia sẻ màn hình
      </button>
    </div>

    <div class="exam-take-layout" :class="{ 'is-locked': !examStarted }">
      <main class="question-main-panel glass-card">
        <header class="question-header">
          <span class="question-index">Câu hỏi {{ currentQuestionIndex + 1 }} / {{ mockQuestions.length }}</span>
          <span class="question-points">[{{ currentQuestion.points }} điểm]</span>

          <button
            type="button"
            class="flag-btn"
            :class="{ active: flagged[currentQuestion.id] }"
            :disabled="!examStarted"
            @click="toggleFlag(currentQuestion.id)"
          >
            <Flag :size="14" />
            {{ flagged[currentQuestion.id] ? 'Đã đánh dấu' : 'Đánh dấu xem sau' }}
          </button>
        </header>

        <div class="question-content">
          <p class="question-text">{{ currentQuestion.content }}</p>
        </div>

        <div class="choices-container">
          <div v-if="currentQuestion.type === 'single_choice'" class="choices-grid">
            <button
              v-for="choice in currentQuestion.choices"
              :key="choice.id"
              type="button"
              class="choice-card"
              :class="{ selected: answers[currentQuestion.id] === choice.id }"
              :disabled="!examStarted"
              @click="selectChoice(currentQuestion.id, choice.id)"
            >
              <span class="choice-prefix">{{ choice.label }}</span>
              <span class="choice-text">{{ choice.text }}</span>
            </button>
          </div>

          <div v-else-if="currentQuestion.type === 'multiple_choice'" class="choices-grid">
            <button
              v-for="choice in currentQuestion.choices"
              :key="choice.id"
              type="button"
              class="choice-card"
              :class="{ selected: (answers[currentQuestion.id] || []).includes(choice.id) }"
              :disabled="!examStarted"
              @click="toggleMultiChoice(currentQuestion.id, choice.id)"
            >
              <span class="choice-prefix-square">{{ choice.label }}</span>
              <span class="choice-text">{{ choice.text }}</span>
            </button>
          </div>

          <div v-else class="text-answer-container">
            <label class="text-answer-label">Nhập câu trả lời của bạn:</label>
            <textarea
              :value="answers[currentQuestion.id] || ''"
              rows="7"
              class="text-answer-input"
              placeholder="Viết câu trả lời tại đây..."
              :disabled="!examStarted"
              @input="updateTextAnswer(currentQuestion.id, $event.target.value)"
            />
          </div>
        </div>

        <footer class="question-actions">
          <button
            type="button"
            class="nav-btn"
            :disabled="currentQuestionIndex === 0"
            @click="prevQuestion"
          >
            <ChevronLeft :size="16" />
            Câu trước
          </button>

          <button
            type="button"
            class="nav-btn"
            :disabled="currentQuestionIndex === mockQuestions.length - 1"
            @click="nextQuestion"
          >
            Câu tiếp theo
            <ChevronRight :size="16" />
          </button>
        </footer>
      </main>

      <aside class="exam-status-sidebar">
        <div class="sidebar-block glass-card">
          <h3>Tiến độ bài làm</h3>
          <div class="progress-bar-container">
            <div class="progress-info">
              <span>Đã làm: {{ answeredCount }} / {{ mockQuestions.length }} câu</span>
              <strong>{{ progressPercent }}%</strong>
            </div>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: `${progressPercent}%` }" />
            </div>
          </div>
        </div>

        <div class="sidebar-block glass-card">
          <h3>Giám sát bài thi</h3>
          <div class="monitor-list">
            <div class="monitor-row">
              <span>Trạng thái</span>
              <strong>{{ monitoringStatusLabel }}</strong>
            </div>
            <div class="monitor-row">
              <span>Toàn màn hình</span>
              <strong>{{ isFullscreen ? 'Đang bật' : 'Chưa bật' }}</strong>
            </div>
            <div class="monitor-row">
              <span>Khóa phím chuyển tab</span>
              <strong>{{ keyboardLockLabel }}</strong>
            </div>
            <div class="monitor-row">
              <span>Vi phạm đã ghi nhận</span>
              <strong>{{ violations.length }}</strong>
            </div>
            <div class="monitor-row">
              <span>Cảnh báo nghiêm trọng</span>
              <strong>{{ criticalViolationCount }}</strong>
            </div>
            <div class="monitor-row">
              <span>Lưu tạm</span>
              <strong>{{ lastSavedAt ? `Đã lưu lúc ${formatTime(lastSavedAt)}` : 'Chưa lưu' }}</strong>
            </div>
          </div>
        </div>

        <div class="sidebar-block glass-card flex-1">
          <h3>Danh sách câu hỏi</h3>
          <div class="question-grid">
            <button
              v-for="(q, idx) in mockQuestions"
              :key="q.id"
              type="button"
              class="grid-question-btn"
              :class="{
                active: idx === currentQuestionIndex,
                answered: isAnswered(q),
                flagged: flagged[q.id],
              }"
              @click="currentQuestionIndex = idx"
            >
              {{ idx + 1 }}
              <span v-if="flagged[q.id]" class="flag-dot" />
            </button>
          </div>
        </div>

        <div class="sidebar-block glass-card">
          <h3>3 log gần nhất</h3>
          <div v-if="recentViolations.length" class="violation-mini-list">
            <div
              v-for="item in recentViolations"
              :key="item.id"
              class="violation-mini"
              :class="`severity-${item.severity}`"
            >
              <Terminal :size="13" />
              <div>
                <strong>{{ item.type }}</strong>
                <span>{{ item.message }}</span>
              </div>
            </div>
          </div>
          <p v-else class="empty-log">Chưa có vi phạm.</p>
        </div>

        <div class="sidebar-submit-block">
          <button
            type="button"
            class="btn-submit-exam"
            :disabled="!examStarted"
            @click="showConfirmSubmit = true"
          >
            <Send :size="16" />
            Nộp bài thi
          </button>
          <p class="submit-info-text">
            Sau khi nộp bài, bạn không thể chỉnh sửa đáp án.
          </p>
        </div>
      </aside>
    </div>

    <div v-if="!examStarted" class="start-overlay">
      <div class="start-card glass-card">
        <div class="start-icon">
          <ShieldAlert :size="30" />
        </div>
        <h2>Sẵn sàng bắt đầu bài thi</h2>
        <p>
          Khi bắt đầu, hệ thống sẽ chuyển sang toàn màn hình, yêu cầu chia sẻ màn hình và ghi nhận các hành vi bất thường.
        </p>
        <div class="start-checks">
          <span><Maximize :size="14" /> Toàn màn hình bắt buộc</span>
          <span><MonitorCheck :size="14" /> Chia sẻ màn hình bắt buộc</span>
          <span><ClipboardX :size="14" /> Chặn copy/paste/cut</span>
        </div>
        <button type="button" class="btn-start" :disabled="monitoringStatus === 'starting'" @click="startExamEnvironment">
          <PlayCircle :size="18" />
          {{ monitoringStatus === 'starting' ? 'Đang khởi động...' : 'Bắt đầu môi trường thi' }}
        </button>
        <p class="start-note">
          Browser không thể chặn tuyệt đối Print Screen hoặc Snipping Tool. Watermark được dùng để răn đe và truy vết nếu đề thi bị lộ.
        </p>
      </div>
    </div>

    <div v-if="showConfirmSubmit" class="confirm-modal-backdrop" @click.self="showConfirmSubmit = false">
      <div class="confirm-modal glass-card">
        <div class="modal-icon">
          <HelpCircle :size="28" />
        </div>
        <h2>Xác nhận nộp bài thi?</h2>
        <div class="submit-summary">
          <span>Đã làm: <strong>{{ answeredCount }}</strong></span>
          <span>Chưa làm: <strong>{{ unansweredCount }}</strong></span>
          <span>Đánh dấu: <strong>{{ flaggedCount }}</strong></span>
          <span>Vi phạm: <strong>{{ violations.length }}</strong></span>
        </div>
        <p>Sau khi nộp bài, bạn không thể chỉnh sửa đáp án.</p>

        <div class="modal-actions">
          <button type="button" class="btn-cancel" @click="showConfirmSubmit = false">Quay lại làm bài</button>
          <button type="button" class="btn-confirm" @click="submitExam('manual')">Nộp bài ngay</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-take-page {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: min(1280px, calc(100vw - 2rem));
  min-height: 100vh;
  margin: 0 auto;
  padding: 1rem 0 2rem;
  color: var(--text-body);
}

.glass-card {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 20px;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: var(--lg-shadow-sm);
}

.loading-card {
  display: inline-flex;
  align-items: center;
  gap: 0.625rem;
  padding: 1rem 1.25rem;
  color: var(--text-label);
  font-weight: 700;
}

.watermark-layer {
  position: fixed;
  inset: 0;
  z-index: 15;
  pointer-events: none;
  display: grid;
  grid-template-columns: repeat(6, minmax(160px, 1fr));
  gap: 2.5rem 1.5rem;
  padding: 2rem;
  opacity: 0.105;
  overflow: hidden;
}

.watermark-layer span {
  align-self: center;
  justify-self: center;
  color: var(--text-heading);
  font-size: 0.72rem;
  font-weight: 900;
  transform: rotate(-24deg);
  white-space: nowrap;
  user-select: none;
}

.exam-header-strip {
  position: relative;
  z-index: 20;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  gap: 1rem;
}

.exam-title-meta h1 {
  margin: 0.2rem 0;
  font-size: 1.25rem;
  font-weight: 850;
  color: var(--text-heading);
}

.exam-title-meta p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--text-label);
}

.badge-exam {
  display: inline-block;
  font-size: 0.65rem;
  font-weight: 900;
  background: var(--accent-primary-soft);
  color: var(--text-link);
  padding: 0.2rem 0.5rem;
  border-radius: 6px;
  text-transform: uppercase;
}

.header-status-group {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.monitoring-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  min-height: 2.45rem;
  padding: 0.55rem 0.8rem;
  border-radius: 14px;
  border: 1px solid var(--border-default);
  background: var(--surface-solid);
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 850;
}

.monitoring-pill.status-active {
  color: var(--color-success-text);
  background: var(--color-success-bg);
  border-color: color-mix(in srgb, var(--color-success-text) 24%, transparent);
}

.monitoring-pill.status-interrupted {
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
  border-color: color-mix(in srgb, var(--color-danger-text) 24%, transparent);
}

.exam-timer-block {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 1rem;
  background: var(--accent-primary-soft);
  border: 1px solid color-mix(in srgb, var(--text-link) 15%, transparent);
  border-radius: 14px;
}

.exam-timer-block.warning {
  background: var(--color-danger-bg);
  border-color: color-mix(in srgb, var(--color-danger-text) 20%, transparent);
  color: var(--color-danger-text);
}

.timer-icon {
  color: var(--text-link);
}

.exam-timer-block.warning .timer-icon {
  color: var(--color-danger-text);
  animation: pulse 1s infinite alternate;
}

@keyframes pulse {
  from { opacity: 0.6; }
  to { opacity: 1; }
}

.timer-values {
  display: flex;
  flex-direction: column;
}

.timer-label {
  font-size: 0.58rem;
  font-weight: 800;
  color: var(--text-placeholder);
}

.timer-countdown {
  font-size: 1.15rem;
  font-weight: 900;
  color: var(--text-heading);
  font-variant-numeric: tabular-nums;
}

.warning-banner {
  position: relative;
  z-index: 45;
  display: flex;
  align-items: center;
  gap: 0.625rem;
  min-height: 2.5rem;
  padding: 0.75rem 1rem;
  border-radius: 14px;
  border: 1px solid var(--border-default);
  background: var(--surface-card-strong);
  color: var(--text-label);
  font-size: 0.8125rem;
  font-weight: 750;
}

.warning-banner.info,
.warning-banner.low {
  color: var(--color-info-text);
  background: var(--color-info-bg);
  border-color: color-mix(in srgb, var(--color-info-text) 24%, transparent);
}

.warning-banner.medium {
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
  border-color: color-mix(in srgb, var(--color-warning-text) 24%, transparent);
}

.warning-banner.high,
.warning-banner.critical {
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
  border-color: color-mix(in srgb, var(--color-danger-text) 28%, transparent);
}

.warning-banner button {
  margin-left: auto;
  border: 1px solid currentColor;
  background: transparent;
  color: currentColor;
  border-radius: 10px;
  padding: 0.35rem 0.65rem;
  font-size: 0.72rem;
  font-weight: 850;
  cursor: pointer;
}

.exam-take-layout {
  position: relative;
  z-index: 20;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 300px;
  gap: 1rem;
  align-items: start;
}

.exam-take-layout.is-locked {
  filter: saturate(0.75);
}

.question-main-panel {
  display: flex;
  flex-direction: column;
  min-height: 30rem;
  padding: 1.5rem;
  user-select: none;
}

.question-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
  margin-bottom: 1.25rem;
}

.question-index {
  font-size: 0.9rem;
  font-weight: 850;
  color: var(--text-heading);
}

.question-points {
  font-size: 0.75rem;
  color: var(--text-link);
  font-weight: 800;
}

.flag-btn {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.3rem 0.6rem;
  border-radius: 8px;
  font-size: 0.72rem;
  font-weight: 800;
  cursor: pointer;
}

.flag-btn.active {
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  border-color: color-mix(in srgb, var(--color-warning-text) 25%, transparent);
}

.flag-btn:disabled,
.choice-card:disabled,
.btn-submit-exam:disabled,
.btn-start:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.question-content {
  margin-bottom: 1.5rem;
}

.question-text {
  font-size: 0.98rem;
  line-height: 1.5;
  color: var(--text-heading);
  white-space: pre-line;
}

.choices-grid {
  display: grid;
  gap: 0.65rem;
}

.choice-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  text-align: left;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.75rem 1rem;
  border-radius: 12px;
  cursor: pointer;
  color: var(--text-body);
  transition: all 0.15s ease;
}

.choice-card:hover:not(:disabled) {
  background: var(--surface-card-strong);
  border-color: var(--border-input-focus);
}

.choice-card.selected {
  background: var(--accent-primary-soft);
  border-color: var(--text-link);
  color: var(--text-link);
}

.choice-prefix,
.choice-prefix-square {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.65rem;
  height: 1.65rem;
  border: 1px solid var(--border-card);
  background: var(--surface-card-strong);
  font-weight: 900;
  font-size: 0.75rem;
  color: var(--text-label);
  flex-shrink: 0;
}

.choice-prefix {
  border-radius: 50%;
}

.choice-prefix-square {
  border-radius: 6px;
}

.choice-card.selected .choice-prefix,
.choice-card.selected .choice-prefix-square {
  background: var(--text-link);
  color: var(--text-inverse);
  border-color: var(--text-link);
}

.choice-text {
  font-size: 0.85rem;
  font-weight: 650;
}

.text-answer-label {
  display: block;
  margin-bottom: 0.5rem;
  color: var(--text-label);
  font-size: 0.85rem;
  font-weight: 800;
}

.text-answer-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 12px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-heading);
  font-family: inherit;
  font-size: 0.875rem;
  outline: none;
  resize: vertical;
  user-select: text;
}

.text-answer-input:focus {
  border-color: var(--border-input-focus);
}

.question-actions {
  display: flex;
  justify-content: space-between;
  border-top: 1px solid var(--border-card);
  padding-top: 1.25rem;
  margin-top: auto;
}

.nav-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.45rem 1rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.nav-btn:hover:not(:disabled) {
  background: var(--surface-card-strong);
  color: var(--text-heading);
}

.nav-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.exam-status-sidebar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  height: 100%;
}

.sidebar-block {
  padding: 1.25rem;
}

.sidebar-block h3 {
  margin: 0 0 0.8rem;
  font-size: 0.82rem;
  font-weight: 900;
  color: var(--text-heading);
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.progress-info,
.monitor-row {
  display: flex;
  justify-content: space-between;
  gap: 0.75rem;
  font-size: 0.72rem;
  font-weight: 800;
  color: var(--text-label);
  margin-bottom: 0.4rem;
}

.monitor-row {
  margin-bottom: 0;
  padding: 0.45rem 0;
  border-bottom: 1px solid var(--border-default);
}

.monitor-row:last-child {
  border-bottom: 0;
}

.progress-info strong,
.monitor-row strong {
  color: var(--text-heading);
  text-align: right;
}

.progress-track {
  height: 0.5rem;
  background: var(--surface-input);
  border-radius: 99px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--lg-primary), var(--lg-cyan));
  border-radius: inherit;
  transition: width 0.3s ease;
}

.question-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.5rem;
}

.grid-question-btn {
  position: relative;
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.grid-question-btn:hover {
  border-color: var(--border-input-focus);
}

.grid-question-btn.active {
  border-color: var(--text-link);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.grid-question-btn.answered {
  background: var(--text-link);
  color: var(--text-inverse);
  border-color: var(--text-link);
}

.grid-question-btn.flagged {
  border-color: var(--color-warning-text);
}

.flag-dot {
  position: absolute;
  top: 3px;
  right: 3px;
  width: 5px;
  height: 5px;
  background: var(--color-warning-text);
  border-radius: 50%;
}

.violation-mini-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.violation-mini {
  display: flex;
  gap: 0.45rem;
  padding: 0.55rem;
  border-radius: 12px;
  background: var(--surface-solid);
  border: 1px solid var(--border-default);
}

.violation-mini.severity-high,
.violation-mini.severity-critical {
  background: var(--color-danger-bg);
  border-color: color-mix(in srgb, var(--color-danger-text) 22%, transparent);
  color: var(--color-danger-text);
}

.violation-mini strong,
.violation-mini span {
  display: block;
}

.violation-mini strong {
  font-size: 0.65rem;
  font-weight: 900;
}

.violation-mini span {
  margin-top: 0.1rem;
  font-size: 0.68rem;
  font-weight: 650;
}

.empty-log {
  margin: 0;
  color: var(--text-placeholder);
  font-size: 0.75rem;
  font-weight: 650;
}

.sidebar-submit-block {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.btn-submit-exam {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  min-height: 2.75rem;
  border: 0;
  border-radius: 14px;
  background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-primary));
  color: var(--text-inverse);
  font-weight: 850;
  font-size: 0.85rem;
  cursor: pointer;
  box-shadow: 0 8px 24px color-mix(in srgb, var(--text-link) 25%, transparent);
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.btn-submit-exam:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 10px 28px color-mix(in srgb, var(--text-link) 35%, transparent);
}

.submit-info-text {
  margin: 0;
  font-size: 0.65rem;
  color: var(--text-placeholder);
  text-align: center;
}

.start-overlay {
  position: fixed;
  inset: 0;
  z-index: 90;
  display: grid;
  place-items: center;
  background: color-mix(in srgb, var(--surface-page) 72%, transparent);
  backdrop-filter: blur(10px);
  padding: 1rem;
}

.start-card {
  width: min(34rem, 100%);
  padding: 1.5rem;
  text-align: center;
}

.start-icon {
  display: grid;
  place-items: center;
  width: 3.5rem;
  height: 3.5rem;
  margin: 0 auto 1rem;
  border-radius: 18px;
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.start-card h2 {
  margin: 0 0 0.5rem;
  color: var(--text-heading);
  font-size: 1.25rem;
  font-weight: 900;
}

.start-card p {
  margin: 0;
  color: var(--text-body);
  font-size: 0.9rem;
  line-height: 1.55;
}

.start-checks {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
  margin: 1.25rem 0;
}

.start-checks span {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  min-height: 2.5rem;
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-solid);
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 850;
}

.btn-start {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  min-height: 3rem;
  border: 0;
  border-radius: 14px;
  background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-cyan));
  color: var(--text-inverse);
  font-size: 0.92rem;
  font-weight: 900;
  cursor: pointer;
}

.start-note {
  margin-top: 1rem !important;
  color: var(--text-muted) !important;
  font-size: 0.75rem !important;
}

.confirm-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: grid;
  place-items: center;
  background: color-mix(in srgb, var(--text-heading) 50%, transparent);
  backdrop-filter: blur(8px);
  padding: 1rem;
}

.confirm-modal {
  width: min(30rem, 100%);
  padding: 1.5rem;
  text-align: center;
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes modal-enter {
  from { opacity: 0; transform: translateY(12px) scale(0.97); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3.25rem;
  height: 3.25rem;
  margin: 0 auto 1rem;
  border-radius: 16px;
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.confirm-modal h2 {
  margin: 0 0 0.75rem;
  font-size: 1.15rem;
  font-weight: 900;
  color: var(--text-heading);
}

.confirm-modal p {
  margin: 0.75rem 0 0;
  font-size: 0.825rem;
  color: var(--text-body);
}

.submit-summary {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.5rem;
}

.submit-summary span {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  padding: 0.65rem 0.45rem;
  border-radius: 12px;
  background: var(--surface-solid);
  color: var(--text-label);
  font-size: 0.7rem;
  font-weight: 750;
}

.submit-summary strong {
  color: var(--text-heading);
  font-size: 1rem;
}

.modal-actions {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 1.5rem;
}

.btn-cancel,
.btn-confirm {
  min-height: 2.35rem;
  padding: 0 1.25rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.btn-cancel {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.btn-cancel:hover {
  background: var(--surface-card-strong);
}

.btn-confirm {
  border: 0;
  background: var(--text-link);
  color: var(--text-inverse);
}

.btn-confirm:hover {
  background: var(--lg-primary-dark);
}

@media (max-width: 900px) {
  .exam-header-strip {
    align-items: flex-start;
    flex-direction: column;
  }

  .header-status-group {
    justify-content: flex-start;
  }

  .exam-take-layout {
    grid-template-columns: 1fr;
  }

  .watermark-layer {
    grid-template-columns: repeat(3, minmax(160px, 1fr));
  }
}

@media (max-width: 640px) {
  .start-checks,
  .submit-summary {
    grid-template-columns: 1fr;
  }
}
</style>
