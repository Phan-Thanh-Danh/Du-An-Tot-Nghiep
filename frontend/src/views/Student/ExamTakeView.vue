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
import { useAuthStore } from '@/stores/auth'
import { examApi } from '@/services/examApi'
import { examProctoringHub } from '@/services/examProctoringHub'
import { requestScreenShare as webrtcRequestScreenShare, createStudentPeerConnection, stopScreenShare as webrtcStopScreenShare } from '@/services/webrtcScreenShare'
import {
  clearExamRuntimeStorage,
  createViolation,
  detectExamGuardAgent,
  loadViolationLog,
  saveViolationLog,
} from '@/utils/examSecurity'

const route = useRoute()
const router = useRouter()

const authStore = useAuthStore()
const STUDENT_ID = computed(() => authStore.user?.id || authStore.user?.userId || 0)
const STUDENT_NAME = computed(() => authStore.user?.fullName || 'Học sinh')
const PREFLIGHT_MAX_AGE_MS = 2 * 60 * 60 * 1000

const examId = String(route.params.examId)
const caThiId = Number(route.query.maCaThi || sessionStorage.getItem(`exam_ca_thi_${examId}`) || examId)
const exam = ref({ title: 'Đang tải...', durationMinutes: 45, totalQuestions: 0 })
const questions = ref([])
const maPhienThi = ref(null)
const maDeKiemTra = ref(null)

const answersKey = computed(() => `exam_answers_${examId}_${STUDENT_ID.value}`)
const flagsKey = computed(() => `exam_flags_${examId}_${STUDENT_ID.value}`)
const timeKey = computed(() => `exam_time_left_${examId}_${STUDENT_ID.value}`)
const lastSavedKey = computed(() => `exam_last_saved_at_${examId}_${STUDENT_ID.value}`)
const submittedKey = computed(() => `exam_submitted_${examId}_${STUDENT_ID.value}`)

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
const isSuspended = ref(false)

const browserCapabilities = ref({
  browserName: 'unknown',
  osName: 'unknown',
  fullscreen: false,
  keyboardLock: false,
  screenShare: false,
  secureContext: false,
  userAgent: '',
})

const examSoftLock = ref({
  visible: false,
  type: '',
  title: '',
  message: '',
  severity: 'high',
  canContinue: false,
  requireProctorUnlock: false,
  requireFullscreen: true,
  requireScreenShare: true,
  violationCount: 0,
})

const lockdownState = ref({
  fullscreenActive: false,
  keyboardLockActive: false,
  keyboardLockSupported: false,
  keyboardLockFailedReason: '',
  shortcutBlockMode: 'best-effort',
  lastFullscreenExitAt: null,
  lastWindowBlurAt: null,
  lastVisibilityHiddenAt: null,
})

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
let suppressFocusViolationUntil = 0
const lastViolationByType = new Map()

const currentQuestion = computed(() => questions.value[currentQuestionIndex.value] || questions.value[0])

const answeredCount = computed(() => questions.value.filter((question) => isAnswered(question)).length)
const flaggedCount = computed(() => Object.values(flagged.value).filter(Boolean).length)
const unansweredCount = computed(() => Math.max(questions.value.length - answeredCount.value, 0))
const progressPercent = computed(() => {
  if (!questions.value.length) return 0
  return Math.round((answeredCount.value / questions.value.length) * 100)
})
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
  return `${STUDENT_ID.value} • ${STUDENT_NAME.value} • ${currentTimestamp.value}`
})

function readJson(key, defaultValue) {
  try {
    const value = localStorage.getItem(key)
    return value ? JSON.parse(value) : defaultValue
  } catch {
    return defaultValue
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

function detectBrowserName(userAgent = navigator.userAgent) {
  const ua = userAgent.toLowerCase()
  if (ua.includes('coc_coc_browser') || ua.includes('coc_coc')) return 'Cốc Cốc'
  if (ua.includes('edg/')) return 'Microsoft Edge'
  if (ua.includes('firefox/')) return 'Firefox'
  if (ua.includes('chrome/') && !ua.includes('edg/')) return 'Chrome'
  if (ua.includes('safari/') && !ua.includes('chrome/')) return 'Safari'
  return 'Unknown'
}

function detectOsName(userAgent = navigator.userAgent) {
  const ua = userAgent.toLowerCase()
  if (ua.includes('windows')) return 'Windows'
  if (ua.includes('mac os') || ua.includes('macintosh')) return 'macOS'
  if (ua.includes('linux')) return 'Linux'
  if (ua.includes('x11')) return 'Linux/Unix'
  return 'Unknown'
}

function detectBrowserCapabilities() {
  const userAgent = navigator.userAgent
  browserCapabilities.value = {
    browserName: detectBrowserName(userAgent),
    osName: detectOsName(userAgent),
    fullscreen: Boolean(document.documentElement.requestFullscreen),
    keyboardLock: Boolean(navigator.keyboard?.lock),
    screenShare: Boolean(navigator.mediaDevices?.getDisplayMedia),
    secureContext: window.isSecureContext,
    userAgent,
  }
  lockdownState.value.keyboardLockSupported = browserCapabilities.value.keyboardLock
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
  if (localStorage.getItem(submittedKey.value) === 'true') {
    clearExamRuntimeStorage(examId, STUDENT_ID.value)
    return
  }

  const suspendedReason = localStorage.getItem(`exam_suspended_${examId}_${STUDENT_ID.value}`)
  if (suspendedReason) {
    isSuspended.value = true
    examSoftLock.value = {
        visible: true,
        type: 'SUSPENDED',
        title: 'ĐÌNH CHỈ THI',
        message: `Bạn đã bị giám thị đình chỉ thi. Lý do: ${suspendedReason}`,
        severity: 'critical',
        canContinue: false,
        requireProctorUnlock: false,
        requireFullscreen: false,
        requireScreenShare: false,
        violationCount: 99,
    }
    submitLocked = true
    return
  }

  const restoredAnswers = readJson(answersKey.value, null)
  const restoredFlags = readJson(flagsKey.value, null)
  const restoredTime = Number(localStorage.getItem(timeKey.value) || 0)
  const restoredViolations = loadViolationLog(examId, STUDENT_ID.value)
  const restoredSavedAt = localStorage.getItem(lastSavedKey.value) || ''

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
  localStorage.setItem(answersKey.value, JSON.stringify(answers.value))
  localStorage.setItem(flagsKey.value, JSON.stringify(flagged.value))
  localStorage.setItem(timeKey.value, String(timeLeftSeconds.value))
  localStorage.setItem(lastSavedKey.value, savedAt)
  saveViolationLog(examId, STUDENT_ID.value, violations.value)
  lastSavedAt.value = savedAt

  // Gửi API lưu tạm
  if (maPhienThi.value) {
    examApi.autoSaveAnswers({
      maPhienThi: maPhienThi.value,
      cauTraLoiJson: JSON.stringify(answers.value)
    }).catch(console.error)
  }
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
    studentId: STUDENT_ID.value,
    studentName: STUDENT_NAME.value,
    type,
    severity,
    message,
    details,
  })

  const violationId = Object.keys(violations.value).find(id => violations.value[id]?.handled === false)
  if (violationId && violations.value[violationId]) {
    violations.value[violationId].handled = true
  }

  violations.value = [violation, ...violations.value]
  saveViolationLog(examId, STUDENT_ID.value, violations.value)
  
  // Gửi qua Hub
  try {
    examProctoringHub.sendViolationLog(caThiId, STUDENT_ID.value, type, message + (details ? ' - ' + JSON.stringify(details) : ''))
  } catch(e) { console.warn(e) }
  
  // Gửi trực tiếp qua HTTP API (đảm bảo real-time)
  try {
    examApi.logViolation({
      maCaThi: caThiId,
      maHocSinh: STUDENT_ID.value,
      loaiViPham: type,
      mucDo: severity,
      moTa: message + (details ? ' - ' + JSON.stringify(details) : '')
    }).catch(() => {})
  } catch(e) {}
  
  return violation
}

function selectChoice(questionId, choiceId) {
  if (isSuspended.value) return
  answers.value = {
    ...answers.value,
    [questionId]: choiceId,
  }
}

function toggleMultiChoice(questionId, choiceId) {
  if (isSuspended.value) return
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
  if (isSuspended.value) return
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
  if (currentQuestionIndex.value < questions.value.length - 1) {
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
    detectBrowserCapabilities()
    attachLockdownListeners()

    const stream = await requestExamScreenShare()

    await requestExamFullscreen()
    await lockExamKeyboard()

    if (!document.fullscreenElement) {
      throw new Error('Toàn màn hình chưa được kích hoạt ổn định.')
    }
    
    // 2. Gọi API bắt đầu thi
    const preflightRaw = sessionStorage.getItem(`exam_preflight_passed_${examId}`)
    let envScore = 0
    let isAgentActive = false
    if (preflightRaw) {
      try {
        const pf = JSON.parse(preflightRaw)
        envScore = pf.riskScore || 0
        // If there's an agent check in checks, parse it
        isAgentActive = pf.checks?.some(c => c.id === 'env_agent' && c.status === 'pass') || false
      } catch(e) {}
    }
    
    // Tạo fingerprint cơ bản
    const fingerprint = btoa(navigator.userAgent + screen.width + screen.height + screen.colorDepth).substring(0, 50)

    const session = await examApi.startExam({ 
      maCaThi: caThiId,
      envCheckScore: envScore,
      browserFingerprint: fingerprint,
      isAgentActive: isAgentActive
    })
    maPhienThi.value = session.maPhienThi
    maDeKiemTra.value = session.maDeKiemTra
    exam.value.durationMinutes = session.thoiGianLamBai || 45
    
    // 3. Lấy câu hỏi
    const quizResponse = await examApi.getExamQuestions(session.maPhienThi)
    questions.value = quizResponse || []
    exam.value.totalQuestions = questions.value.length
    
    // 4. Kết nối WebRTC Hub
    const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || ''
    await examProctoringHub.connect(token)

    // Lưu trữ peer connections theo connectionId của giám thị
    const studentPeerConnections = new Map()  // proctorConnectionId -> RTCPeerConnection
    const studentPendingIce = new Map()        // proctorConnectionId -> RTCIceCandidateInit[]

    function getStudentIceQueue(proctorConnId) {
      if (!studentPendingIce.has(proctorConnId)) studentPendingIce.set(proctorConnId, [])
      return studentPendingIce.get(proctorConnId)
    }

    async function flushStudentIceQueue(proctorConnId) {
      const pc = studentPeerConnections.get(proctorConnId)
      if (!pc || !pc.remoteDescription) return
      const queue = studentPendingIce.get(proctorConnId) || []
      if (import.meta.env.DEV && queue.length > 0)
        console.debug(`[Student] Flushing ${queue.length} ICE for proctor`, proctorConnId)
      while (queue.length > 0) {
        const c = queue.shift()
        try { await pc.addIceCandidate(new window.RTCIceCandidate(c)) }
        catch (e) { if (import.meta.env.DEV) console.warn('[Student] flush ICE error', e) }
      }
    }

    const initPeerAndSendOffer = async (proctorConnectionId) => {
      if (!proctorConnectionId) return

      if (!stream) {
        console.warn('[Student] Cannot create offer: screen stream is missing')
        return
      }

      if (studentPeerConnections.has(proctorConnectionId)) {
        if (import.meta.env.DEV) console.debug('[Student] Peer already exists for proctor', proctorConnectionId)
        return
      }

      const pc = createStudentPeerConnection(
        stream,
        // Student gửi ICE candidate về giám thị
        (candidate) => examProctoringHub.sendIceCandidate({
          maCaThi: caThiId,
          maHocSinh: STUDENT_ID.value,
          targetConnectionId: proctorConnectionId,
          candidate, // examProctoringHub sẽ chuẩn hóa qua toJSON()
        }),
        () => {} // negotiation callback
      )

      // Theo dõi trạng thái peer
      pc.onconnectionstatechange = () => {
        if (import.meta.env.DEV)
          console.debug('[Student] Peer connectionState:', pc.connectionState)
      }
      pc.oniceconnectionstatechange = () => {
        if (import.meta.env.DEV)
          console.debug('[Student] ICE state:', pc.iceConnectionState)
      }

      studentPeerConnections.set(proctorConnectionId, pc)

      try {
        const offer = await pc.createOffer()
        await pc.setLocalDescription(offer)

        await examProctoringHub.sendOffer({
          maCaThi: caThiId,
          maHocSinh: STUDENT_ID.value,
          targetConnectionId: proctorConnectionId,
          offer: { type: pc.localDescription.type, sdp: pc.localDescription.sdp },
        })
        if (import.meta.env.DEV) console.debug('[Student] Offer sent to proctor', proctorConnectionId)
      } catch (e) {
        console.error('[Student] Error creating offer', e)
      }
    }

    // WebRTC: Giám thị yêu cầu kết nối
    examProctoringHub.eventHandlers.onProctorRequestedConnections = async (payload) => {
      if (import.meta.env.DEV) console.debug('[Student] ProctorRequestedConnections', payload)
      await examProctoringHub.joinAsStudent(caThiId, STUDENT_ID.value)
      if (payload?.proctorConnectionId) {
        await initPeerAndSendOffer(payload.proctorConnectionId)
      }
    }

    // WebRTC: Giám thị phản hồi StudentConnectionIdBroadcast
    examProctoringHub.eventHandlers.onProctorAcknowledged = async (payload) => {
      if (import.meta.env.DEV) console.debug('[Student] ProctorAcknowledged', payload)
      if (payload?.proctorConnectionId) {
        await initPeerAndSendOffer(payload.proctorConnectionId)
      }
    }

    // Xử lý ICE từ giám thị — cần queue vì ICE có thể tới trước setRemoteDescription xong
    examProctoringHub.eventHandlers.onReceiveIceCandidate = async (iceDto) => {
      if (!iceDto?.candidate?.candidate) return

      // Bỏ qua own ICE
      if (iceDto.fromConnectionId === examProctoringHub.connectionId) {
        if (import.meta.env.DEV) console.debug('[Student] Skip own ICE candidate')
        return
      }

      const targetPc = studentPeerConnections.get(iceDto.fromConnectionId)
      const candidateInit = iceDto.candidate // đã là typed { candidate, sdpMid, sdpMLineIndex }

      if (!targetPc || !targetPc.remoteDescription) {
        if (import.meta.env.DEV) console.debug('[Student] ICE queued for proctor', iceDto.fromConnectionId)
        getStudentIceQueue(iceDto.fromConnectionId).push(candidateInit)
        return
      }

      try { await targetPc.addIceCandidate(new window.RTCIceCandidate(candidateInit)) }
      catch (e) { if (import.meta.env.DEV) console.warn('[Student] addIceCandidate error', e) }
    }

    // WebRTC: Nhận Answer từ giám thị
    examProctoringHub.eventHandlers.onReceiveAnswer = async (dto) => {
      if (!dto?.answer) {
        console.warn('[Student] ReceiveAnswer: invalid dto', dto)
        return
      }

      if (dto.fromConnectionId === examProctoringHub.connectionId) {
        if (import.meta.env.DEV) console.debug('[Student] Skip own answer')
        return
      }

      const proctorConnectionId = dto.fromConnectionId
      if (import.meta.env.DEV) console.debug('[Student] ReceiveAnswer from proctor', proctorConnectionId)

      const pc = studentPeerConnections.get(proctorConnectionId)
      if (!pc) {
        console.warn('[Student] Received answer but no peer connection exists for proctor', proctorConnectionId)
        return
      }

      try {
        const answerDesc = { type: dto.answer.type, sdp: dto.answer.sdp }
        await pc.setRemoteDescription(new window.RTCSessionDescription(answerDesc))
        await flushStudentIceQueue(proctorConnectionId)
      } catch (e) {
        console.error('[Student] Error setting remote answer', e)
      }
    }

    // Xử lý cảnh báo từ giám thị
    examProctoringHub.eventHandlers.onWarningReceived = (payload) => {
      const message = payload?.message || 'Giám thị đã gửi một nhắc nhở.'
      
      examSoftLock.value = {
        visible: true,
        type: 'PROCTOR_WARNING',
        title: 'Nhắc nhở từ giám thị',
        message: message,
        severity: 'high',
        canContinue: true,
        requireProctorUnlock: false,
        requireFullscreen: true,
        requireScreenShare: true,
        violationCount: examSoftLock.value.violationCount,
      }
      
      pushWarning('Nhắc nhở từ giám thị: ' + message, 'high')
      
      try {
        const audio = new Audio('/sound.mp3')
        audio.play().catch(() => {})
      } catch (err) { console.warn(err) }
    }

    // Xử lý mở khóa bài thi
    examProctoringHub.eventHandlers.onStudentUnlocked = () => {
      if (examSoftLock.value.requireProctorUnlock) {
        examSoftLock.value.requireProctorUnlock = false
        examSoftLock.value.canContinue = true
        examSoftLock.value.violationCount = 0
        fullscreenExitCount.value = 0
        tabSwitchCount.value = 0
        // Clear old violations so they don't count towards the next 3
        violations.value = []
        pushWarning('Giám thị đã mở khóa bài thi cho bạn. Vui lòng tiếp tục làm bài.', 'info')
      }
    }

    examProctoringHub.eventHandlers.onStudentSuspended = (payload) => {
      const reason = payload?.lyDo || 'Vi phạm quy chế thi'
      isSuspended.value = true
      
      examSoftLock.value = {
        visible: true,
        type: 'SUSPENDED',
        title: 'ĐÌNH CHỈ THI',
        message: `Bạn đã bị giám thị đình chỉ thi. Lý do: ${reason}`,
        severity: 'critical',
        canContinue: false,
        requireProctorUnlock: false,
        requireFullscreen: false,
        requireScreenShare: false,
        violationCount: 99,
      }
      
      pushWarning(`Bạn đã bị đình chỉ thi. Lý do: ${reason}`, 'critical')
      
      saveDraft()
      
      monitoringStatus.value = 'stopped'
      clearInterval(timerInterval)
      clearInterval(autosaveInterval)
      clearInterval(runtimeScanInterval)
      clearInterval(watermarkInterval)
      stopDevtoolsDetection()
      submitLocked = true
      cleanupScreenStream(false)
      
      localStorage.setItem(`exam_suspended_${examId}_${STUDENT_ID.value}`, reason)
    }

    await examProctoringHub.joinAsStudent(caThiId, STUDENT_ID.value)
    await examProctoringHub.screenShareStarted(caThiId, STUDENT_ID.value)

    attachScreenStream(stream)
    
    examStarted.value = true
    monitoringStatus.value = 'active'
    timeLeftSeconds.value = Number(exam.value.durationMinutes) * 60
    
    startTimer()
    startRuntimeMonitoring()
    saveDraft()
  } catch (error) {
    monitoringStatus.value = 'idle'
    cleanupScreenStream(false)
    startError.value = error?.message || error?.response?.data?.message || 'Bạn cần chia sẻ màn hình và bật toàn màn hình để bắt đầu bài thi.'
    if (document.fullscreenElement) {
      await document.exitFullscreen().catch(() => {})
    }
  }
}

function wait(ms) {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

function waitForFullscreenState(timeoutMs = 1200) {
  return new Promise((resolve) => {
    if (document.fullscreenElement) {
      resolve(true)
      return
    }
    const timer = window.setTimeout(() => {
      document.removeEventListener('fullscreenchange', onChange)
      resolve(Boolean(document.fullscreenElement))
    }, timeoutMs)
    function onChange() {
      window.clearTimeout(timer)
      document.removeEventListener('fullscreenchange', onChange)
      resolve(Boolean(document.fullscreenElement))
    }
    document.addEventListener('fullscreenchange', onChange)
  })
}

async function requestExamFullscreen() {
  const root = document.documentElement
  if (!root.requestFullscreen) {
    throw new Error('Trình duyệt không hỗ trợ chế độ toàn màn hình.')
  }
  try {
    await root.requestFullscreen({ navigationUI: 'hide' })
  } catch (firstError) {
    console.warn('[ExamLockdown] requestFullscreen with options failed, retry basic', firstError)
    try {
      await root.requestFullscreen()
    } catch (retryError) {
      console.error('[ExamLockdown] requestFullscreen failed', retryError)
      throw new Error('Không thể bật toàn màn hình. Vui lòng cho phép fullscreen để vào phòng thi.', { cause: retryError })
    }
  }

  await waitForFullscreenState(1200)
  await wait(300)

  if (!document.fullscreenElement) {
    throw new Error('Toàn màn hình vừa bị thoát hoặc chưa được kích hoạt ổn định.')
  }

  isFullscreen.value = true
  lockdownState.value.fullscreenActive = true
  return true
}

async function requestExamScreenShare() {
  isScreenSharePickerOpen.value = true
  suppressFocusViolationUntil = Date.now() + 8000
  try {
    const stream = await webrtcRequestScreenShare()
    suppressFocusViolationUntil = Date.now() + 3000
    return stream
  } finally {
    isScreenSharePickerOpen.value = false
  }
}

function shouldIgnoreFocusViolation() {
  return isScreenSharePickerOpen.value || Date.now() < suppressFocusViolationUntil || examSoftLock.value.visible
}

async function lockExamKeyboard() {
  keyboardLockActive.value = false
  lockdownState.value.keyboardLockActive = false
  lockdownState.value.keyboardLockFailedReason = ''

  if (!navigator.keyboard?.lock) {
    lockdownState.value.keyboardLockSupported = false
    lockdownState.value.keyboardLockFailedReason = 'Trình duyệt không hỗ trợ Keyboard Lock API.'
    return false
  }

  if (!document.fullscreenElement) {
    lockdownState.value.keyboardLockFailedReason = 'Keyboard Lock chỉ hoạt động sau khi vào fullscreen.'
    return false
  }

  try {
    await navigator.keyboard.lock([
      'Escape', 'AltLeft', 'AltRight', 'ControlLeft', 'ControlRight',
      'MetaLeft', 'MetaRight', 'ShiftLeft', 'ShiftRight', 'Tab',
      'F4', 'F6', 'F11', 'KeyW', 'KeyR', 'KeyT', 'KeyN', 'KeyL',
      'KeyD', 'KeyP', 'KeyS', 'PageUp', 'PageDown', 'ArrowLeft', 'ArrowRight'
    ])
    keyboardLockActive.value = true
    lockdownState.value.keyboardLockActive = true
    lockdownState.value.keyboardLockSupported = true
    return true
  } catch (error) {
    console.warn('[ExamLockdown] Keyboard lock failed', error)
    keyboardLockActive.value = false
    lockdownState.value.keyboardLockActive = false
    lockdownState.value.keyboardLockFailedReason = error?.message || 'Không thể khóa phím nâng cao trên trình duyệt này.'
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
    webrtcStopScreenShare(screenStream.value)
    screenStream.value = null
  }

  if (markStopped && !submitLocked) {
    monitoringStatus.value = 'stopped'
    try {
      examProctoringHub.screenShareStopped(caThiId, STUDENT_ID.value)
    } catch (e) { console.warn(e) }
  }
}

function handleScreenTrackEnded() {
  if (!examStarted.value || submitLocked) return

  monitoringStatus.value = 'interrupted'
  isSuspended.value = true // Kích hoạt Hard Lock
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
    await requestExamScreenShare()
    isSuspended.value = false // Bỏ Hard Lock
    pushWarning('Chia sẻ màn hình đã được bật lại.', 'low')
  } catch (error) {
    monitoringStatus.value = 'interrupted'
    startError.value = error?.message || 'Không thể bật lại chia sẻ màn hình.'
  }
}

async function resumeFullscreen() {
  try {
    await requestExamFullscreen()
  } catch (error) {
    console.warn(error)
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

async function scanForbiddenExtensionsRuntime() {
  if (!examStarted.value || submitLocked) return

  const result = await detectExamGuardAgent()
  if (result.status === 'fail') {
    addViolation(
      'FORBIDDEN_APP_RUNTIME',
      'critical',
      'Phát hiện phần mềm bị cấm trong lúc thi (Agent check failed)',
      result.details,
      { dedupeMs: 15000 },
    )
    pushWarning('Phát hiện phần mềm bị cấm trong lúc thi.', 'critical')
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
          { widthDiff, heightDiff, detector: 'viewport-delta' },
          { dedupeMs: 10000 },
        )
        pushWarning('Phát hiện dấu hiệu mở Developer Tools.', 'critical')
      }
    }, 1500)
  }
}

const restrictedShortcutRules = [
  {
    name: 'ESCAPE',
    match: e => e.key === 'Escape' || e.code === 'Escape',
    severity: 'critical',
    message: 'Thí sinh nhấn Escape trong lúc làm bài.',
  },
  {
    name: 'ALT_TAB',
    match: e => e.altKey && (e.key === 'Tab' || e.code === 'Tab'),
    severity: 'critical',
    message: 'Thí sinh cố dùng Alt+Tab để chuyển ứng dụng.',
  },
  {
    name: 'META_TAB',
    match: e => e.metaKey && (e.key === 'Tab' || e.code === 'Tab'),
    severity: 'critical',
    message: 'Thí sinh cố dùng Cmd/Meta+Tab để chuyển ứng dụng.',
  },
  {
    name: 'ALT_F4',
    match: e => e.altKey && (e.key === 'F4' || e.code === 'F4'),
    severity: 'critical',
    message: 'Thí sinh cố đóng cửa sổ bằng Alt+F4.',
  },
  {
    name: 'CTRL_ALT_T',
    match: e => e.ctrlKey && e.altKey && String(e.key).toLowerCase() === 't',
    severity: 'critical',
    message: 'Thí sinh cố mở terminal bằng Ctrl+Alt+T.',
  },
  {
    name: 'CMD_Q',
    match: e => e.metaKey && String(e.key).toLowerCase() === 'q',
    severity: 'critical',
    message: 'Thí sinh cố thoát ứng dụng bằng Cmd+Q.',
  },
  {
    name: 'CMD_M',
    match: e => e.metaKey && String(e.key).toLowerCase() === 'm',
    severity: 'high',
    message: 'Thí sinh cố thu nhỏ cửa sổ bằng Cmd+M.',
  },
  {
    name: 'CLOSE_TAB',
    match: e => (e.ctrlKey || e.metaKey) && String(e.key).toLowerCase() === 'w',
    severity: 'critical',
    message: 'Thí sinh cố đóng tab/cửa sổ bài thi.',
  },
  {
    name: 'RELOAD_PAGE',
    match: e =>
      (e.ctrlKey || e.metaKey) && String(e.key).toLowerCase() === 'r' ||
      e.key === 'F5' ||
      e.code === 'F5',
    severity: 'high',
    message: 'Thí sinh cố tải lại trang bài thi.',
  },
  {
    name: 'NEW_TAB',
    match: e => (e.ctrlKey || e.metaKey) && String(e.key).toLowerCase() === 't',
    severity: 'high',
    message: 'Thí sinh cố mở tab mới.',
  },
  {
    name: 'NEW_WINDOW',
    match: e => (e.ctrlKey || e.metaKey) && String(e.key).toLowerCase() === 'n',
    severity: 'high',
    message: 'Thí sinh cố mở cửa sổ mới.',
  },
  {
    name: 'ADDRESS_BAR_FOCUS',
    match: e =>
      ((e.ctrlKey || e.metaKey) && ['l', 'd'].includes(String(e.key).toLowerCase())) ||
      (e.altKey && String(e.key).toLowerCase() === 'd') ||
      e.key === 'F6' ||
      e.code === 'F6',
    severity: 'high',
    message: 'Thí sinh cố chuyển focus ra thanh địa chỉ.',
  },
  {
    name: 'PRINT_OR_SAVE',
    match: e => (e.ctrlKey || e.metaKey) && ['p', 's'].includes(String(e.key).toLowerCase()),
    severity: 'high',
    message: 'Thí sinh cố in hoặc lưu trang bài thi.',
  },
  {
    name: 'DEVTOOLS_SHORTCUT',
    match: e =>
      e.key === 'F12' ||
      e.code === 'F12' ||
      ((e.ctrlKey || e.metaKey) && e.shiftKey && ['i', 'j', 'c'].includes(String(e.key).toLowerCase())) ||
      (e.metaKey && e.altKey && String(e.key).toLowerCase() === 'i'),
    severity: 'critical',
    message: 'Thí sinh cố mở Developer Tools.',
  },
  {
    name: 'FULLSCREEN_TOGGLE_SHORTCUT',
    match: e =>
      e.key === 'F11' ||
      e.code === 'F11' ||
      (e.ctrlKey && e.metaKey && String(e.key).toLowerCase() === 'f'),
    severity: 'high',
    message: 'Thí sinh cố thay đổi chế độ toàn màn hình.',
  },
]

function handleRestrictedKeydown(event) {
  if (!examStarted.value || submitLocked) return
  const rule = restrictedShortcutRules.find(item => item.match(event))
  if (!rule) return

  event.preventDefault()
  event.stopPropagation()
  event.stopImmediatePropagation?.()

  softLockExamWithViolation({
    type: rule.name,
    severity: rule.severity,
    message: rule.message,
    details: {
      key: event.key,
      code: event.code,
      altKey: event.altKey,
      ctrlKey: event.ctrlKey,
      metaKey: event.metaKey,
      shiftKey: event.shiftKey,
      browser: browserCapabilities.value.browserName,
      os: browserCapabilities.value.osName,
      fullscreen: Boolean(document.fullscreenElement),
      keyboardLockActive: keyboardLockActive.value,
    },
  })
}

function handleWindowBlur() {
  if (!examStarted.value || submitLocked) return
  if (shouldIgnoreFocusViolation()) return

  lockdownState.value.lastWindowBlurAt = Date.now()
  softLockExamWithViolation({
    type: 'WINDOW_BLUR',
    severity: 'high',
    message: 'Cửa sổ bài thi bị mất focus. Có thể thí sinh đã chuyển ứng dụng hoặc dùng phím hệ thống.',
    details: {
      fullscreen: Boolean(document.fullscreenElement),
      visibilityState: document.visibilityState,
      browser: browserCapabilities.value.browserName,
      os: browserCapabilities.value.osName,
    },
  })
}

function handleVisibilityChange() {
  if (!examStarted.value || submitLocked) return
  if (shouldIgnoreFocusViolation()) return

  if (document.hidden) {
    lockdownState.value.lastVisibilityHiddenAt = Date.now()
    softLockExamWithViolation({
      type: 'TAB_OR_APP_SWITCH',
      severity: 'critical',
      message: 'Thí sinh rời khỏi tab/cửa sổ bài thi.',
      details: {
        visibilityState: document.visibilityState,
        fullscreen: Boolean(document.fullscreenElement),
        browser: browserCapabilities.value.browserName,
        os: browserCapabilities.value.osName,
      },
    })
  }
}

function handleFullscreenChange() {
  const active = Boolean(document.fullscreenElement)
  isFullscreen.value = active
  lockdownState.value.fullscreenActive = active

  if (active) {
    void lockExamKeyboard()
    return
  }

  unlockExamKeyboard()

  if (!examStarted.value || submitLocked) return
  if (shouldIgnoreFocusViolation()) return

  lockdownState.value.lastFullscreenExitAt = Date.now()
  fullscreenExitCount.value += 1

  softLockExamWithViolation({
    type: 'FULLSCREEN_EXIT',
    severity: 'critical',
    message: 'Thí sinh thoát khỏi chế độ toàn màn hình.',
    details: {
      count: fullscreenExitCount.value,
      browser: browserCapabilities.value.browserName,
      os: browserCapabilities.value.osName,
    },
  })
}

function handlePageHide() {
  if (!examStarted.value || submitLocked) return
  softLockExamWithViolation({
    type: 'PAGE_HIDE',
    severity: 'critical',
    message: 'Trang bài thi bị ẩn hoặc sắp rời khỏi.',
    details: {
      visibilityState: document.visibilityState,
      fullscreen: Boolean(document.fullscreenElement),
    },
  })
}

function handleWindowFocus() {
  if (!examStarted.value || submitLocked) return
  if (!document.fullscreenElement) {
    examSoftLock.value.visible = true
  }
}

function getLockdownViolationCount() {
  return violations.value.filter(v =>
    [
      'FULLSCREEN_EXIT',
      'TAB_OR_APP_SWITCH',
      'WINDOW_BLUR',
      'RESTRICTED_SHORTCUT',
      'ESCAPE',
      'ALT_TAB',
      'META_TAB',
      'CLOSE_TAB',
      'DEVTOOLS_SHORTCUT',
      'PAGE_HIDE',
    ].includes(v.type)
  ).length
}

function softLockExamWithViolation({ type, severity, message, details = {} }) {
  const violation = addViolation(type, severity, message, details, { dedupeMs: 1500 })
  if (!violation) return // Bỏ qua nếu bị dedupe

  const count = getLockdownViolationCount()
  const requireProctorUnlock = count >= 3

  examSoftLock.value = {
    visible: true,
    type,
    title: requireProctorUnlock
      ? 'Bài thi đã bị khóa tạm thời'
      : 'Phát hiện rời khỏi môi trường làm bài',
    message: requireProctorUnlock
      ? 'Bạn đã vi phạm quy định nhiều lần. Vui lòng chờ giám thị xử lý.'
      : message,
    severity,
    canContinue: false,
    requireProctorUnlock,
    requireFullscreen: true,
    requireScreenShare: true,
    violationCount: count,
  }

  window.setTimeout(() => {
    if (!examSoftLock.value.requireProctorUnlock) {
      examSoftLock.value.canContinue = true
    }
  }, 2000)
}

async function continueAfterSoftLock() {
  if (examSoftLock.value.requireProctorUnlock) {
    pushWarning('Bài thi đang bị khóa. Vui lòng chờ giám thị xử lý.', 'critical')
    return
  }

  if (!examSoftLock.value.canContinue) return

  // Tạm thời bỏ qua các vi phạm focus/blur do hiệu ứng chuyển đổi fullscreen của OS
  suppressFocusViolationUntil = Date.now() + 3000

  if (!document.fullscreenElement) {
    try {
      await requestExamFullscreen()
      await lockExamKeyboard()
    } catch (error) {
      console.warn(error)
      pushWarning('Bạn cần quay lại toàn màn hình để tiếp tục làm bài.', 'critical')
      return
    }
  }

  const hasLiveScreenTrack = Boolean(
    screenStream.value?.getVideoTracks?.().some(track => track.readyState === 'live')
  )

  if (!hasLiveScreenTrack) {
    pushWarning('Bạn cần bật lại chia sẻ màn hình để tiếp tục làm bài.', 'critical')
    return
  }

  examSoftLock.value.visible = false
}

function blockClipboard(event) {
  if (!examStarted.value || submitLocked) return
  event.preventDefault()
  addViolation('CLIPBOARD_ATTEMPT', 'medium', 'Thao tác sao chép/dán bị chặn trong bài thi', { eventType: event.type })
  pushWarning('Thao tác sao chép/dán bị chặn trong bài thi.', 'medium')
}

function blockContextMenu(event) {
  if (!examStarted.value || submitLocked) return
  event.preventDefault()
  addViolation('CONTEXT_MENU', 'low', 'Thao tác chuột phải bị chặn', { eventType: event.type })
}

function handleBeforeUnload(event) {
  if (!examStarted.value || submitLocked) return
  addViolation(
    'KEYBOARD_SHORTCUT_ATTEMPT',
    'critical',
    'Học sinh cố đóng, tải lại hoặc rời trang thi',
    { shortcut: 'PAGE_UNLOAD_OR_REFRESH', keyboardLockActive: keyboardLockActive.value },
    { dedupeMs: 1000 },
  )
  saveDraft()
  event.preventDefault()
  event.returnValue = ''
}

function attachLockdownListeners() {
  window.addEventListener('keydown', handleRestrictedKeydown, true)
  document.addEventListener('keydown', handleRestrictedKeydown, true)
  document.addEventListener('fullscreenchange', handleFullscreenChange, true)
  document.addEventListener('visibilitychange', handleVisibilityChange, true)
  window.addEventListener('blur', handleWindowBlur, true)
  window.addEventListener('focus', handleWindowFocus, true)
  window.addEventListener('pagehide', handlePageHide, true)
  window.addEventListener('beforeunload', handleBeforeUnload, true)
  window.addEventListener('contextmenu', blockContextMenu, true)
  window.addEventListener('copy', blockClipboard, true)
  window.addEventListener('cut', blockClipboard, true)
  window.addEventListener('paste', blockClipboard, true)
  window.addEventListener('selectstart', blockClipboard, true)
}

function detachLockdownListeners() {
  window.removeEventListener('keydown', handleRestrictedKeydown, true)
  document.removeEventListener('keydown', handleRestrictedKeydown, true)
  document.removeEventListener('fullscreenchange', handleFullscreenChange, true)
  document.removeEventListener('visibilitychange', handleVisibilityChange, true)
  window.removeEventListener('blur', handleWindowBlur, true)
  window.removeEventListener('focus', handleWindowFocus, true)
  window.removeEventListener('pagehide', handlePageHide, true)
  window.removeEventListener('beforeunload', handleBeforeUnload, true)
  window.removeEventListener('contextmenu', blockContextMenu, true)
  window.removeEventListener('copy', blockClipboard, true)
  window.removeEventListener('cut', blockClipboard, true)
  window.removeEventListener('cut', blockClipboard, true)
  window.removeEventListener('paste', blockClipboard, true)
  window.removeEventListener('selectstart', blockClipboard, true)
}

function stopDevtoolsDetection() {
  if (devtoolsDetectorModule && devtoolsListener) {
    devtoolsDetectorModule.removeListener(devtoolsListener)
    devtoolsDetectorModule.stop()
  }
  if (devtoolsFallbackInterval) {
    clearInterval(devtoolsFallbackInterval)
    devtoolsFallbackInterval = null
  }
}

async function submitExam(reason = 'manual') {
  if (submitLocked || isSuspended.value) return
  submitLocked = true
  monitoringStatus.value = 'stopped'
  clearInterval(timerInterval)
  clearInterval(autosaveInterval)
  clearInterval(runtimeScanInterval)
  clearInterval(watermarkInterval)
  stopDevtoolsDetection()
  
  if (reason === 'timeout') {
    pushWarning('Đã hết thời gian làm bài. Hệ thống đang tự động nộp bài.', 'critical')
  }
  
  try {
    localStorage.setItem(submittedKey.value, 'true')
    clearExamRuntimeStorage(examId, STUDENT_ID.value)
    router.replace(`/student/exams/results/${examId}`)
  } catch (error) {
    submitLocked = false
    console.warn(error)
    pushWarning('Không thể nộp bài thi. Vui lòng thử lại.', 'critical')
  }
}

onMounted(() => {
  if (!validatePreflightToken()) {
    router.replace(`/student/exams/detail/${examId}`)
    return
  }
  
  const previousStudentKey = `exam_last_student_${examId}`
  const lastStudent = localStorage.getItem(previousStudentKey)
  if (lastStudent && lastStudent !== String(STUDENT_ID.value)) {
    clearExamRuntimeStorage(examId, lastStudent)
    violations.value = []
    examSoftLock.value = {
      isLocked: false,
      requireProctorUnlock: false,
      canContinue: true,
      violationCount: 0
    }
    fullscreenExitCount.value = 0
    tabSwitchCount.value = 0
  }
  localStorage.setItem(previousStudentKey, String(STUDENT_ID.value))

  preflightReady.value = true
  restoreDraft()
  updateWatermarkTimestamp()
  watermarkInterval = window.setInterval(updateWatermarkTimestamp, 1000)
  // attachLockdownListeners is called in startExamEnvironment
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
  if (autosaveInterval) clearInterval(autosaveInterval)
  if (runtimeScanInterval) clearInterval(runtimeScanInterval)
  if (watermarkInterval) clearInterval(watermarkInterval)
  if (streamRecoveryTimer) clearTimeout(streamRecoveryTimer)
  stopDevtoolsDetection()
  unlockExamKeyboard()
  detachLockdownListeners()
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

    <div class="exam-soft-lock-overlay" v-if="examSoftLock.visible">
      <div class="lock-modal glass-card">
        <div class="lock-icon" :class="examSoftLock.severity">
          <AlertTriangle :size="32" />
        </div>
        <h2 class="lock-title">{{ examSoftLock.title }}</h2>
        <p class="lock-message">{{ examSoftLock.message }}</p>
        <div class="lock-details">
          <p>Mức độ: <strong>{{ examSoftLock.severity.toUpperCase() }}</strong></p>
          <p>Số lần vi phạm: <strong>{{ examSoftLock.violationCount }}</strong></p>
        </div>
        <div class="lock-actions">
          <button
            class="btn-primary"
            :disabled="!examSoftLock.canContinue || examSoftLock.requireProctorUnlock"
            @click="continueAfterSoftLock"
          >
            {{
              examSoftLock.requireProctorUnlock
                ? 'Đã khóa bài thi'
                : (examSoftLock.canContinue ? 'Tôi hiểu và tiếp tục làm bài' : 'Vui lòng chờ...')
            }}
          </button>
        </div>
      </div>
    </div>

    <!-- Hard Lock Overlay khi mất chia sẻ màn hình -->
    <div class="exam-soft-lock-overlay" v-if="isSuspended">
      <div class="lock-modal glass-card">
        <div class="lock-icon critical">
          <MonitorCheck :size="32" />
        </div>
        <h2 class="lock-title">Đã Khóa Bài Thi</h2>
        <p class="lock-message">Màn hình của bạn đã dừng chia sẻ. Bài thi đã bị khóa tạm thời để đảm bảo tính công bằng.</p>
        <div class="lock-actions">
          <button class="btn-primary" @click="restartScreenShare">
            Bật lại Toàn bộ màn hình
          </button>
        </div>
      </div>
    </div>

    <div class="exam-take-layout" :class="{ 'is-locked': !examStarted || examSoftLock.visible || isSuspended }">
      <main class="question-main-panel glass-card">
        <template v-if="currentQuestion">
          <header class="question-header">
            <span class="question-index">Câu hỏi {{ currentQuestionIndex + 1 }} / {{ questions.length }}</span>
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
            :disabled="currentQuestionIndex === questions.length - 1"
            @click="nextQuestion"
          >
            Câu tiếp theo
            <ChevronRight :size="16" />
          </button>
        </footer>
        </template>
        <template v-else>
          <div class="empty-state p-4 text-center">
            <p>Đang tải câu hỏi hoặc đề thi không có câu hỏi nào.</p>
          </div>
        </template>
      </main>

      <aside class="exam-status-sidebar">
        <div class="sidebar-block glass-card">
          <h3>Tiến độ bài làm</h3>
          <div class="progress-bar-container">
            <div class="progress-info">
              <span>Đã làm: {{ answeredCount }} / {{ questions.length }} câu</span>
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
              v-for="(q, idx) in questions"
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
        
        <div v-if="startError" class="warning-banner critical" style="margin-bottom: 20px;">
          <XCircle :size="16" />
          <span>{{ startError }}</span>
        </div>
        
        <div v-if="startError && startError.includes('ExamGuard Agent')" class="agent-download-section">
          <p class="agent-download-desc">Vui lòng tải và chạy ứng dụng ExamGuard Agent (không cần cài đặt):</p>
          <div class="agent-download-links">
            <a href="https://github.com/Phan-Thanh-Danh/Remote-Desktop/raw/main/publish/win-x64/ExamGuard.Agent.exe" target="_blank" class="agent-btn win">
              <Download :size="16" /> Windows (x64)
            </a>
            <a href="https://github.com/Phan-Thanh-Danh/Remote-Desktop/raw/main/publish/osx-arm64/ExamGuard.Agent" target="_blank" class="agent-btn mac">
              <Download :size="16" /> macOS (Apple Silicon)
            </a>
          </div>
          <p class="agent-note">Sau khi chạy ứng dụng, hãy ấn "Bắt đầu làm bài" để quét lại.</p>
        </div>
        <p>
          Khi bắt đầu, hệ thống sẽ chuyển sang toàn màn hình, yêu cầu chia sẻ màn hình và ghi nhận các hành vi bất thường.
        </p>
        <div class="start-checks">
          <span><Maximize :size="14" /> Toàn màn hình bắt buộc</span>
          <span><MonitorCheck :size="14" /> Chia sẻ màn hình bắt buộc</span>
          <span><ClipboardX :size="14" /> Chặn copy/paste/bôi đen</span>
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
  pointer-events: none;
  opacity: 0.6;
}

/* Chống bôi đen (Text selection) */
.exam-take-layout {
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
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

.start-checks span {
  display: flex;
  align-items: center;
  gap: 8px;
}

.agent-download-section {
  background: var(--surface-card, #f8fafc);
  border: 1px solid var(--border-card, #e2e8f0);
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 20px;
  text-align: left;
}
.agent-download-desc {
  font-size: 14px;
  margin-bottom: 12px;
  font-weight: 500;
  color: var(--text-heading, #1e293b);
}
.agent-download-links {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
}
.agent-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: white;
  text-decoration: none;
  transition: opacity 0.2s;
}
.agent-btn:hover { opacity: 0.9; }
.agent-btn.win { background: #0078d7; }
.agent-btn.mac { background: #333333; }
.agent-note {
  font-size: 12px;
  color: var(--text-body, #64748b);
  margin: 0;
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

.exam-soft-lock-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.85);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 99999;
  padding: 1rem;
}

.lock-modal {
  background: var(--surface-card);
  padding: 2.5rem;
  border-radius: 20px;
  max-width: 480px;
  width: 100%;
  text-align: center;
  border: 1px solid var(--border-card);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
  animation: slide-up 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slide-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.lock-icon {
  display: inline-flex;
  padding: 1rem;
  border-radius: 50%;
  margin-bottom: 1.5rem;
}

.lock-icon.critical {
  background: rgba(220, 38, 38, 0.1);
  color: #ef4444;
}

.lock-icon.high {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.lock-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--text-heading);
  margin: 0 0 1rem;
}

.lock-message {
  font-size: 0.95rem;
  color: var(--text-body);
  margin: 0 0 1.5rem;
  line-height: 1.5;
}

.lock-details {
  display: flex;
  justify-content: center;
  gap: 1.5rem;
  padding: 1rem;
  background: var(--surface-solid);
  border-radius: 12px;
  margin-bottom: 2rem;
  font-size: 0.85rem;
  color: var(--text-label);
}

.lock-details strong {
  display: block;
  font-size: 1.1rem;
  color: var(--text-heading);
  margin-top: 0.25rem;
}

.lock-actions .btn-primary {
  width: 100%;
  padding: 1rem;
  border: none;
  border-radius: 12px;
  background: var(--text-link);
  color: var(--text-inverse);
  font-weight: 800;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.2s;
}

.lock-actions .btn-primary:hover:not(:disabled) {
  background: var(--lg-primary-dark);
}

.lock-actions .btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background: var(--surface-input);
  color: var(--text-label);
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
