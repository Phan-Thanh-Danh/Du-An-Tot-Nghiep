<template>
  <div class="proctor-page">
    <section class="proctor-header surface-card border-card">
      <div class="header-main">
        <div class="header-icon">
          <Monitor :size="24" />
        </div>
        <div>
          <p class="header-eyebrow">M4 Controlled Exam Environment</p>
          <h1>{{ pageTitle }}</h1>
          <p>{{ pageSubtitle }}</p>
        </div>
      </div>

      <div class="header-actions">
        <button
          v-if="viewState === 'attendance' || viewState === 'dashboard'"
          type="button"
          class="ghost-action"
          @click="goBack"
        >
          <LogOut :size="16" />
          {{ viewState === 'dashboard' ? 'Quay lại điểm danh' : 'Quay lại danh sách ca' }}
        </button>

        <button
          v-if="viewState === 'dashboard'"
          type="button"
          class="ghost-action"
          :class="{ active: soundEnabled }"
          @click="soundEnabled = !soundEnabled"
        >
          <Bell v-if="soundEnabled" :size="16" />
          <BellOff v-else :size="16" />
          {{ soundEnabled ? 'Âm cảnh báo bật' : 'Âm cảnh báo tắt' }}
        </button>

        <div class="time-chip">
          <Clock :size="18" />
          <div>
            <span>Thời gian thực</span>
            <strong>{{ currentTime }}</strong>
          </div>
        </div>
      </div>
    </section>

    <template v-if="viewState === 'sessions'">
      <div v-if="loading" class="p-4">
        <ListSkeleton :rows="4" />
      </div>
      <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[400px] gap-4">
        <AlertCircle :size="48" class="text-rose-400" />
        <p class="text-rose-600 font-semibold">{{ error }}</p>
        <button @click="loadSessions" class="btn-primary">Thử lại</button>
      </div>
      <template v-else>
        <section class="stats-grid">
        <div class="stat-card surface-card border-card">
          <span>Tổng ca</span>
          <strong>{{ sessionStats.total }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đang điểm danh</span>
          <strong>{{ sessionStats.attendance }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đang canh thi</span>
          <strong>{{ sessionStats.monitoring }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đã kết thúc</span>
          <strong>{{ sessionStats.ended }}</strong>
        </div>
      </section>

      <section class="session-grid">
        <article
          v-for="session in assignedExamSessions"
          :key="session.id"
          class="session-card surface-card border-card"
        >
          <div class="session-card-top">
            <div>
              <p class="session-code">{{ session.subjectCode }} · {{ session.classCode }}</p>
              <h2>{{ session.examTitle }}</h2>
              <p>{{ formatSessionTime(session) }} · {{ session.room }}</p>
            </div>
            <GlassBadge :variant="sessionBadgeVariant(session.status)">
              {{ sessionStatusLabel(session.status) }}
            </GlassBadge>
          </div>

          <div class="session-metrics">
            <div>
              <span>Tổng thí sinh</span>
              <strong>{{ session.totalStudents }}</strong>
            </div>
            <div>
              <span>Đã điểm danh</span>
              <strong>{{ attendanceCountForSession(session.id) }}</strong>
            </div>
            <div>
              <span>Đang thi</span>
              <strong>{{ activeCountForSession(session.id) }}</strong>
            </div>
            <div>
              <span>Đã nộp</span>
              <strong>{{ submittedCountForSession(session.id) }}</strong>
            </div>
            <div>
              <span>Vi phạm</span>
              <strong>{{ violationCountForSession(session.id) }}</strong>
            </div>
          </div>

          <button
            type="button"
            class="primary-action"
            :class="{ muted: session.status === 'ended' }"
            @click="openSession(session)"
          >
            <PlayCircle v-if="session.status !== 'ended'" :size="16" />
            <FileCheck2 v-else :size="16" />
            {{ sessionActionLabel(session) }}
          </button>
        </article>
      </section>
      </template>
    </template>

    <template v-else-if="viewState === 'attendance' && currentSession">
      <section class="attendance-shell surface-card border-card">
        <div class="section-toolbar">
          <div>
            <p class="section-eyebrow">{{ currentSession.subjectCode }} · {{ currentSession.classCode }}</p>
            <h2>Điểm danh thí sinh dự thi</h2>
            <p>{{ currentSession.examTitle }} · {{ formatSessionTime(currentSession) }} · {{ currentSession.room }}</p>
          </div>
          <button
            type="button"
            class="primary-action"
            :disabled="attendanceStats.present === 0"
            @click="startMonitoring"
          >
            <LayoutGrid :size="16" />
            Bắt đầu canh thi
          </button>
        </div>

        <div class="stats-grid compact">
          <div class="stat-card surface-input border-card">
            <span>Tổng</span>
            <strong>{{ attendanceStats.total }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Có mặt</span>
            <strong>{{ attendanceStats.present }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Vắng mặt</span>
            <strong>{{ attendanceStats.absent }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Rủi ro pre-flight</span>
            <strong>{{ attendanceStats.risk }}</strong>
          </div>
        </div>

        <div class="student-table">
          <div class="student-row table-head">
            <span>MSSV</span>
            <span>Họ tên</span>
            <span>Điểm danh</span>
            <span>Pre-flight</span>
            <span>Stream</span>
            <span>Hành động</span>
          </div>
          <div
            v-for="student in currentStudents"
            :key="student.id"
            class="student-row"
          >
            <strong>{{ student.studentCode }}</strong>
            <span>{{ student.name }}</span>
            <GlassBadge :variant="attendanceBadgeVariant(student.attendanceStatus)">
              {{ attendanceLabel(student.attendanceStatus) }}
            </GlassBadge>
            <span :class="['status-text', student.preflightStatus]">
              {{ preflightLabel(student.preflightStatus) }}
            </span>
            <span>{{ streamLabel(student.streamStatus) }}</span>
            <div class="row-actions">
              <button type="button" @click="setAttendance(student, 'present')">Có mặt</button>
              <button type="button" @click="setAttendance(student, 'absent')">Vắng mặt</button>
              <button type="button" @click="setAttendance(student, 'exempted')">Miễn thi</button>
            </div>
          </div>
        </div>
      </section>
    </template>

    <template v-else-if="viewState === 'dashboard' && currentSession">
      <section class="dashboard-toolbar surface-card border-card">
        <div>
          <p class="section-eyebrow">{{ currentSession.subjectCode }} · {{ currentSession.classCode }}</p>
          <h2>Dashboard giám thị</h2>
          <p>Theo dõi thí sinh, trạng thái stream và vi phạm trong ca thi.</p>
        </div>
        <div class="dashboard-counters">
          <div>
            <span>Đang thi</span>
            <strong>{{ activeMonitoringStudents.length }}</strong>
          </div>
          <div>
            <span>Cảnh báo</span>
            <strong>{{ currentRealtimeViolations.length }}</strong>
          </div>
          <div>
            <span>Đã nộp</span>
            <strong>{{ closedStudents.filter((student) => student.examStatus === 'submitted').length }}</strong>
          </div>
        </div>
        <button type="button" class="danger-action" @click="finishSession">
          <SquareX :size="16" />
          Kết thúc ca canh thi
        </button>
      </section>

      <section class="alert-strip surface-card border-card">
        <div class="alert-strip-head">
          <div>
            <p class="section-eyebrow">Cảnh báo realtime</p>
            <h3>Violation từ phòng thi</h3>
          </div>
          <button type="button" class="ghost-action" @click="loadLiveViolations">
            <RefreshCw :size="15" />
            Làm mới
          </button>
        </div>
        <div v-if="currentRealtimeViolations.length" class="alert-list">
          <div
            v-for="violation in currentRealtimeViolations.slice(0, 4)"
            :key="violation.id"
            class="alert-item"
            :class="{ critical: violation.severity === 'critical' || violation.severity === 'high' }"
          >
            <AlertTriangle :size="14" />
            <div>
              <strong>{{ violationLabel(violation.type) }}</strong>
              <span>{{ violation.studentId }} · {{ formatViolationTime(violation.timestamp) }}</span>
            </div>
          </div>
        </div>
        <div v-else class="empty-alert">
          Chưa có vi phạm nào trong ca thi.
        </div>
      </section>

      <section class="screen-grid">
        <article
          v-for="student in activeMonitoringStudents"
          :key="student.id"
          class="screen-card surface-card border-card"
          :class="{ 'has-alert': hasUnhandledViolation(student) }"
          @click="openStudentModal(student)"
        >
          <div class="placeholder-screen">
            <video v-if="studentStreams[student.studentId]" :srcObject.prop="studentStreams[student.studentId]" autoplay playsinline muted></video>
            <span v-else>Đang chờ màn hình...</span>
          </div>
          <div class="screen-card-body">
            <div>
              <p class="student-code">{{ student.studentCode }}</p>
              <h3>{{ student.name }}</h3>
            </div>
            <GlassBadge v-if="hasUnhandledViolation(student)" variant="danger">Cảnh báo</GlassBadge>
          </div>
          <div class="screen-meta">
            <span>Stream: <strong>{{ streamLabel(student.streamStatus) }}</strong></span>
            <span>Bài thi: <strong>{{ examStatusLabel(student.examStatus) }}</strong></span>
            <span>Vi phạm: <strong>{{ studentViolationCount(student) }}</strong></span>
            <span>Gần nhất: <strong>{{ latestViolationLabel(student) }}</strong></span>
          </div>
          <div class="screen-actions" @click.stop>
            <button type="button" @click="openStudentModal(student)">Phóng to</button>
            <button type="button" @click="sendReminder(student)">Nhắc nhở</button>
            <button type="button" @click="suspendStudent(student)">Đình chỉ</button>
            <button type="button" @click="markStudentHandled(student)">Đã xử lý</button>
          </div>
        </article>
      </section>

      <section v-if="closedStudents.length" class="closed-section surface-card border-card">
        <div class="section-toolbar compact-toolbar">
          <div>
            <p class="section-eyebrow">Đã rời grid đang thi</p>
            <h3>Đã nộp bài / Đã đình chỉ</h3>
          </div>
        </div>
        <div class="closed-grid">
          <article
            v-for="student in closedStudents"
            :key="student.id"
            class="closed-card"
            @click="openStudentModal(student)"
          >
            <div class="placeholder-screen compact">
              <video v-if="studentStreams[student.studentId]" :srcObject.prop="studentStreams[student.studentId]" autoplay playsinline muted></video>
              <span v-else>Đã đóng màn hình</span>
            </div>
            <div>
              <strong>{{ student.studentCode }}</strong>
              <span>{{ student.name }}</span>
              <GlassBadge :variant="student.examStatus === 'suspended' ? 'danger' : 'success'">
                {{ examStatusLabel(student.examStatus) }}
              </GlassBadge>
            </div>
          </article>
        </div>
      </section>
    </template>

    <Teleport to="body" v-if="!isUnmounting">
      <div v-if="selectedStudent" class="student-modal-backdrop" @click.self="selectedStudent = null">
        <section class="student-modal surface-card border-card">
          <button type="button" class="modal-close" @click="selectedStudent = null">
            <X :size="18" />
          </button>
          <div class="modal-screen">
            <div class="placeholder-screen large">
              <video v-if="studentStreams[selectedStudent.studentId]" :srcObject.prop="studentStreams[selectedStudent.studentId]" autoplay playsinline muted></video>
              <span v-else>Đang chờ màn hình...</span>
            </div>
          </div>
          <aside class="modal-panel">
            <p class="section-eyebrow">Chi tiết thí sinh</p>
            <h2>{{ selectedStudent.name }}</h2>
            <p>{{ selectedStudent.studentCode }} · {{ currentSession?.classCode }}</p>

            <div class="modal-facts">
              <span>Stream <strong>{{ streamLabel(selectedStudent.streamStatus) }}</strong></span>
              <span>Bài thi <strong>{{ examStatusLabel(selectedStudent.examStatus) }}</strong></span>
              <span>Vi phạm <strong>{{ studentViolationCount(selectedStudent) }}</strong></span>
              <span>Gần nhất <strong>{{ latestViolationLabel(selectedStudent) }}</strong></span>
            </div>

            <div class="timeline">
              <h3>Violation timeline</h3>
              <div v-if="violationsForStudent(selectedStudent).length" class="timeline-list">
                <div
                  v-for="item in violationsForStudent(selectedStudent)"
                  :key="item.id"
                  class="timeline-item"
                  :class="{ handled: item.handled }"
                >
                  <strong>{{ violationLabel(item.type) }}</strong>
                  <span>{{ item.message || 'Ghi nhận vi phạm' }}</span>
                  <small>{{ formatViolationTime(item.timestamp) }}</small>
                </div>
              </div>
              <p v-else class="empty-alert">Chưa có vi phạm.</p>
            </div>

            <div class="modal-actions">
              <button type="button" @click="sendReminder(selectedStudent)">Nhắc nhở</button>
              <button type="button" class="danger" @click="suspendStudent(selectedStudent)">Đình chỉ</button>
              <button type="button" @click="markStudentHandled(selectedStudent)">Đánh dấu xử lý</button>
            </div>
          </aside>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { computed, onMounted, onBeforeUnmount, onUnmounted, ref } from 'vue'
import {
  AlertCircle,
  AlertTriangle,
  Bell,
  BellOff,
  Clock,
  FileCheck2,
  LayoutGrid,
  LogOut,
  Monitor,
  PlayCircle,
  RefreshCw,
  SquareX,
  X,
} from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { usePopupStore } from '@/stores/popup'
import { teacherApi } from '@/services/teacherApi'
import { examProctoringHub } from '@/services/examProctoringHub'
import { createProctorPeerConnection } from '@/services/webrtcScreenShare'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const loading = ref(false)
const error = ref('')
const popupStore = usePopupStore()
const viewState = ref('sessions')
const currentTime = ref('')
const selectedSessionId = ref('')
const selectedStudent = ref(null)
const soundEnabled = ref(true)
const liveViolations = ref([])
const examStudents = ref([])
const studentStreams = ref({})
const peerConnections = new Map()
let clockTimer = null
let violationTimer = null
const isUnmounting = ref(false)

const violationLabelMap = {
  TAB_SWITCH: 'Rời tab',
  FULLSCREEN_EXIT: 'Thoát toàn màn hình',
  CLIPBOARD_ATTEMPT: 'Copy/Paste',
  CONTEXT_MENU: 'Chuột phải',
  DEVTOOLS_OPENED: 'Developer Tools',
  FORBIDDEN_EXTENSION_RUNTIME: 'Extension bị cấm',
  KEYBOARD_SHORTCUT_ATTEMPT: 'Phím tắt bị cấm',
  SCREEN_STREAM_STOPPED: 'Mất stream màn hình',
}

const assignedExamSessions = ref([])

const currentSession = computed(() => {
  return assignedExamSessions.value.find((session) => session.id === selectedSessionId.value) || null
})

const currentStudents = computed(() => {
  return currentSession.value ? examStudents.value : []
})

const presentStudents = computed(() => {
  return currentStudents.value.filter((student) => student.attendanceStatus === 'present')
})

const activeMonitoringStudents = computed(() => {
  return presentStudents.value.filter((student) => !['submitted', 'suspended'].includes(student.examStatus))
})

const closedStudents = computed(() => {
  return presentStudents.value.filter((student) => ['submitted', 'suspended'].includes(student.examStatus))
})

const attendanceStats = computed(() => {
  const total = currentStudents.value.length
  const present = currentStudents.value.filter((student) => student.attendanceStatus === 'present').length
  const exempted = currentStudents.value.filter((student) => student.attendanceStatus === 'exempted').length
  const risk = currentStudents.value.filter((student) => student.preflightStatus === 'risk').length
  return {
    total,
    present,
    absent: total - present - exempted,
    risk,
  }
})

const sessionStats = computed(() => {
  return {
    total: assignedExamSessions.value.length,
    attendance: assignedExamSessions.value.filter((session) => session.status === 'attendance').length,
    monitoring: assignedExamSessions.value.filter((session) => session.status === 'monitoring').length,
    ended: assignedExamSessions.value.filter((session) => session.status === 'ended').length,
  }
})

const pageTitle = computed(() => {
  if (viewState.value === 'attendance') return 'Điểm danh thí sinh dự thi'
  if (viewState.value === 'dashboard') return 'Dashboard giám thị'
  return 'Ca canh thi được phân công'
})

const pageSubtitle = computed(() => {
  if (viewState.value === 'dashboard') return 'Theo dõi thí sinh, trạng thái stream và vi phạm trong ca thi.'
  if (viewState.value === 'attendance') return 'Xác nhận thí sinh có mặt trước khi đưa vào grid giám sát.'
  return 'Chọn ca thi/lớp thi để điểm danh và mở màn hình giám thị realtime.'
})

const currentRealtimeViolations = computed(() => {
  const codes = new Set(currentStudents.value.map((student) => student.studentCode))
  return liveViolations.value
    .filter((violation) => codes.has(violation.studentId || violation.studentCode))
    .filter((violation) => !violation.handled)
})

async function loadSessions() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getExams()
    const rawSessions = Array.isArray(data) ? data : (data?.data?.items ?? data?.data ?? data?.items ?? [])
    assignedExamSessions.value = rawSessions
    if (selectedSessionId.value) {
      const studentsData = await teacherApi.getExamStudents(selectedSessionId.value)
      examStudents.value = Array.isArray(studentsData) ? studentsData : (studentsData?.data?.items ?? studentsData?.data ?? studentsData?.items ?? [])
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải ca thi.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSessions()
  updateTime()
  loadLiveViolations()
  clockTimer = window.setInterval(updateTime, 1000)
  violationTimer = window.setInterval(loadLiveViolations, 3000)
  setupHubAndWebRTC()
})

onBeforeUnmount(() => {
  isUnmounting.value = true
})

onUnmounted(() => {
  try {
    if (clockTimer) window.clearInterval(clockTimer)
    if (violationTimer) window.clearInterval(violationTimer)
    
    // Stop all WebRTC tracks and close connections
    for (const stream of Object.values(studentStreams.value)) {
      if (stream) {
        stream.getTracks().forEach(track => track.stop())
      }
    }
    for (const pc of peerConnections.values()) {
      pc.close()
    }
    peerConnections.clear()
    studentStreams.value = {}

    // Disconnect hub
    examProctoringHub.disconnect()
  } catch (e) {
    console.error('Lỗi khi unmount ProctoringView:', e)
  }
})

async function setupHubAndWebRTC() {
  try {
    const token = authStore.token
    if (!token) return

    await examProctoringHub.connect(token)

    examProctoringHub.eventHandlers.onReceiveOffer = async (dto) => {
      const { maCaThi, maHocSinh, targetConnectionId, offer } = dto
      if (maCaThi != currentSession.value?.id) return

      let pc = peerConnections.get(maHocSinh)
      if (!pc) {
        pc = createProctorPeerConnection(
          (candidate) => {
            examProctoringHub.sendIceCandidate({
              maCaThi,
              maHocSinh,
              targetConnectionId: dto.fromConnectionId,
              candidate
            })
          },
          (stream) => {
            studentStreams.value[maHocSinh] = stream
          }
        )
        peerConnections.set(maHocSinh, pc)
      }

      try {
        await pc.setRemoteDescription(new RTCSessionDescription(offer))
        const answer = await pc.createAnswer()
        await pc.setLocalDescription(answer)

        examProctoringHub.sendAnswer({
          maCaThi,
          maHocSinh,
          targetConnectionId: dto.fromConnectionId,
          answer
        })
      } catch (err) {
        console.error(`Error handling offer from student ${maHocSinh}:`, err)
      }
    }

    examProctoringHub.eventHandlers.onReceiveIceCandidate = async (dto) => {
      const { maHocSinh, candidate } = dto
      const pc = peerConnections.get(maHocSinh)
      if (pc && candidate) {
        try {
          await pc.addIceCandidate(new RTCIceCandidate(candidate))
        } catch (e) {
          console.error(`Error adding ICE candidate from student ${maHocSinh}:`, e)
        }
      }
    }
    
    examProctoringHub.eventHandlers.onStudentConnectionIdBroadcast = async (payload) => {
      if (payload.maCaThi == currentSession.value?.id) {
        await examProctoringHub.acknowledgeStudent(payload.connectionId)
      }
    }
    
    examProctoringHub.eventHandlers.onScreenShareStatusChanged = (payload) => {
      if (payload.status === 'stopped' && studentStreams.value[payload.maHocSinh]) {
         const stream = studentStreams.value[payload.maHocSinh]
         stream.getTracks().forEach(track => track.stop())
         delete studentStreams.value[payload.maHocSinh]
         
         const pc = peerConnections.get(payload.maHocSinh)
         if (pc) {
           pc.close()
           peerConnections.delete(payload.maHocSinh)
         }
      }
    }

  } catch (e) {
    console.error('Error setting up ExamProctoringHub', e)
  }
}

function updateTime() {
  currentTime.value = new Date().toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

async function loadLiveViolations() {
  if (!selectedSessionId.value) return
  try {
    const data = await teacherApi.getExamViolations(selectedSessionId.value)
    liveViolations.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch {
    liveViolations.value = []
  }
}

function formatSessionTime(session) {
  const start = new Date(session.startTime)
  const end = new Date(session.endTime)
  return `${start.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`
}

function sessionStatusLabel(status) {
  if (status === 'scheduled') return 'Sắp diễn ra'
  if (status === 'attendance') return 'Đang điểm danh'
  if (status === 'monitoring') return 'Đang canh thi'
  return 'Đã kết thúc'
}

function sessionBadgeVariant(status) {
  if (status === 'monitoring') return 'success'
  if (status === 'attendance') return 'warning'
  if (status === 'ended') return 'neutral'
  return 'primary'
}

function sessionActionLabel(session) {
  if (session.status === 'ended') return 'Xem biên bản'
  if (session.status === 'monitoring') return 'Mở dashboard giám thị'
  return 'Vào điểm danh'
}

async function openSession(session) {
  selectedSessionId.value = session.id
  try {
    const studentsData = await teacherApi.getExamStudents(session.id)
    examStudents.value = Array.isArray(studentsData) ? studentsData : (studentsData?.data?.items ?? studentsData?.data ?? studentsData?.items ?? [])
  } catch {
    examStudents.value = []
  }
  
  if (examProctoringHub.isConnected) {
    examProctoringHub.joinExamRoom(session.id).catch(console.error)
  }

  if (session.status === 'monitoring') {
    viewState.value = 'dashboard'
  } else {
    viewState.value = 'attendance'
    if (session.status === 'scheduled') session.status = 'attendance'
  }
}

function goBack() {
  if (viewState.value === 'dashboard') {
    viewState.value = 'attendance'
    return
  }

  if (selectedSessionId.value && examProctoringHub.isConnected) {
    examProctoringHub.leaveExamRoom(selectedSessionId.value).catch(console.error)
  }

  selectedSessionId.value = ''
  selectedStudent.value = null
  viewState.value = 'sessions'
}

function setAttendance(student, status) {
  student.attendanceStatus = status
  if (status !== 'present') {
    student.streamStatus = 'waiting'
    student.examStatus = 'not_started'
  }
}

async function startMonitoring() {
  if (!currentSession.value || attendanceStats.value.present === 0) {
    popupStore.warning('Chưa thể bắt đầu', 'Cần điểm danh ít nhất 1 thí sinh có mặt.')
    return
  }

  try {
    const attendancePayload = {
      maCaThi: currentSession.value.id,
      danhSachDiemDanh: currentStudents.value.map(s => {
        let mappedStatus = 'cho_thi';
        if (s.attendanceStatus === 'present') mappedStatus = 'co_mat';
        if (s.attendanceStatus === 'absent') mappedStatus = 'vang_mat';
        return {
          maHocSinh: s.studentId || s.id,
          trangThaiDiemDanh: mappedStatus,
          ghiChu: ''
        };
      })
    }
    
    await teacherApi.batchExamAttendance(attendancePayload)
    await teacherApi.startExamSession(currentSession.value.id)

    currentSession.value.status = 'monitoring'
    presentStudents.value.forEach((student, index) => {
      if (!['submitted', 'suspended'].includes(student.examStatus)) {
        student.examStatus = index === presentStudents.value.length - 1 && presentStudents.value.length >= 3
          ? 'submitted'
          : 'in_progress'
      }
      student.streamStatus = student.examStatus === 'submitted' ? 'stopped' : index === 1 ? 'reconnecting' : 'streaming'
    })
    viewState.value = 'dashboard'
    popupStore.success('Đã bắt đầu canh thi', `${presentStudents.value.length} thí sinh có mặt được đưa vào grid giám sát.`)
  } catch (error) {
    popupStore.error('Lỗi', 'Không thể bắt đầu ca thi: ' + (error?.response?.data?.message || error.message || 'Lỗi không xác định'))
  }
}

async function finishSession() {
  if (!currentSession.value) return
  const confirmed = window.confirm('Kết thúc ca canh thi này?')
  if (!confirmed) return

  try {
    await teacherApi.createBienBan({ maCaThi: currentSession.value.id, trangThai: 'ended' })
  } catch {
    // proceed even if API fails
  }

  currentSession.value.status = 'ended'
  presentStudents.value.forEach((student) => {
    if (student.examStatus === 'in_progress') student.examStatus = 'submitted'
    if (student.streamStatus === 'streaming' || student.streamStatus === 'reconnecting') student.streamStatus = 'stopped'
  })
  selectedStudent.value = null
  viewState.value = 'sessions'
  popupStore.success('Đã kết thúc ca', 'Biên bản ca thi đã được cập nhật.')
}

function attendanceCountForSession(sessionId) {
  const students = selectedSessionId.value === sessionId ? examStudents.value : []
  return students.filter((student) => student.attendanceStatus === 'present').length
}

function activeCountForSession(sessionId) {
  const students = selectedSessionId.value === sessionId ? examStudents.value : []
  return students.filter((student) => student.examStatus === 'in_progress').length
}

function submittedCountForSession(sessionId) {
  const students = selectedSessionId.value === sessionId ? examStudents.value : []
  return students.filter((student) => student.examStatus === 'submitted').length
}

function violationCountForSession(sessionId) {
  const students = selectedSessionId.value === sessionId ? examStudents.value : []
  const codes = new Set(students.map((student) => student.studentCode))
  return liveViolations.value.filter((violation) => codes.has(violation.studentId || violation.studentCode) && !violation.handled).length
}

function violationsForStudent(student) {
  if (!student) return []
  return liveViolations.value.filter((violation) => (violation.studentId || violation.studentCode) === student.studentCode)
}

function studentViolationCount(student) {
  return violationsForStudent(student).filter((violation) => !violation.handled).length
}

function latestViolationForStudent(student) {
  return violationsForStudent(student).filter((violation) => !violation.handled)[0] || null
}

function latestViolationLabel(student) {
  const latest = latestViolationForStudent(student)
  return latest ? violationLabel(latest.type) : 'Không có'
}

function hasUnhandledViolation(student) {
  const latest = latestViolationForStudent(student)
  return Boolean(latest && ['high', 'critical'].includes(latest.severity)) || studentViolationCount(student) > 0
}

function violationLabel(type) {
  return violationLabelMap[type] || type || 'Cảnh báo'
}

function streamLabel(status) {
  if (status === 'streaming') return 'Đang truyền'
  if (status === 'lost') return 'Mất tín hiệu'
  if (status === 'reconnecting') return 'Đang kết nối lại'
  if (status === 'stopped') return 'Đã dừng'
  return 'Chưa kết nối'
}

function examStatusLabel(status) {
  if (status === 'in_progress') return 'Đang làm'
  if (status === 'submitted') return 'Đã nộp bài'
  if (status === 'suspended') return 'Bị đình chỉ'
  return 'Chưa bắt đầu'
}

function attendanceLabel(status) {
  if (status === 'present') return 'Có mặt'
  if (status === 'exempted') return 'Miễn thi'
  return 'Vắng mặt'
}

function attendanceBadgeVariant(status) {
  if (status === 'present') return 'success'
  if (status === 'exempted') return 'neutral'
  return 'warning'
}

function preflightLabel(status) {
  if (status === 'pass') return 'Đạt'
  if (status === 'risk') return 'Rủi ro'
  return 'Chưa kiểm tra'
}

function formatViolationTime(value) {
  if (!value) return '--:--:--'
  return new Date(value).toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

function openStudentModal(student) {
  selectedStudent.value = student
}

function sendReminder(student) {
  const message = window.prompt('Nội dung nhắc nhở', 'Vui lòng quay lại bài thi và tuân thủ quy định.')
  if (!message) return

  student.logs.unshift({
    type: 'PROCTOR_MESSAGE',
    message,
    timestamp: new Date().toISOString(),
  })
  popupStore.info('Đã gửi nhắc nhở', `${student.studentCode} · ${message}`)
}

function suspendStudent(student) {
  const confirmed = window.confirm(`Đình chỉ thí sinh ${student.studentCode}?`)
  if (!confirmed) return

  student.examStatus = 'suspended'
  student.streamStatus = 'stopped'
  student.logs.unshift({
    type: 'SUSPENDED',
    message: 'Giám thị đình chỉ thí sinh',
    timestamp: new Date().toISOString(),
  })
  popupStore.warning('Đã đình chỉ', `${student.studentCode} đã được chuyển sang trạng thái bị đình chỉ.`)
}

function markStudentHandled(student) {
  violationsForStudent(student).forEach((violation) => {
    violation.handled = true
  })
  student.logs.unshift({
    type: 'VIOLATION_HANDLED',
    message: 'Giám thị đánh dấu xử lý cảnh báo',
    timestamp: new Date().toISOString(),
  })
  popupStore.success('Đã xử lý', `Cảnh báo của ${student.studentCode} đã được đánh dấu xử lý.`)
}
</script>

<style scoped>
.proctor-page {
  display: flex;
  min-height: calc(100vh - 132px);
  flex-direction: column;
  gap: 1rem;
  color: var(--text-body);
}

.border-card {
  border: 1px solid var(--border-card);
}

.proctor-header,
.dashboard-toolbar,
.attendance-shell,
.alert-strip,
.closed-section {
  border-radius: 18px;
  box-shadow: var(--lg-shadow-sm);
}

.proctor-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem;
}

.header-main,
.header-actions,
.section-toolbar,
.alert-strip-head,
.screen-card-body {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.header-icon {
  display: grid;
  width: 2.8rem;
  height: 2.8rem;
  place-items: center;
  border-radius: 14px;
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.header-eyebrow,
.section-eyebrow,
.session-code {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.65rem;
  font-weight: 850;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.proctor-header h1,
.dashboard-toolbar h2,
.attendance-shell h2,
.session-card h2,
.modal-panel h2 {
  margin: 0.1rem 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 850;
}

.proctor-header p,
.dashboard-toolbar p,
.attendance-shell p,
.session-card p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.78rem;
  font-weight: 600;
}

.time-chip,
.ghost-action,
.primary-action,
.danger-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 850;
}

.time-chip {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.65rem 0.8rem;
}

.time-chip span {
  display: block;
  color: var(--text-muted);
  font-size: 0.58rem;
  text-transform: uppercase;
}

.time-chip strong {
  display: block;
  color: var(--text-heading);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
}

.ghost-action,
.primary-action,
.danger-action {
  min-height: 2.4rem;
  border: 1px solid var(--border-card);
  padding: 0 0.8rem;
  cursor: pointer;
}

.ghost-action {
  background: var(--surface-input);
  color: var(--text-label);
}

.ghost-action.active {
  color: var(--text-link);
}

.primary-action {
  border: 0;
  background: var(--text-link);
  color: var(--text-inverse);
}

.primary-action:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.primary-action.muted {
  background: var(--surface-input);
  color: var(--text-label);
}

.danger-action {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.stats-grid.compact {
  margin-top: 1rem;
}

.stat-card {
  min-height: 4.2rem;
  border-radius: 16px;
  padding: 0.8rem 1rem;
}

.stat-card span,
.session-metrics span,
.dashboard-counters span,
.screen-meta span {
  color: var(--text-muted);
  font-size: 0.68rem;
  font-weight: 800;
}

.stat-card strong {
  display: block;
  color: var(--text-heading);
  font-size: 1.35rem;
  font-weight: 900;
}

.session-grid,
.screen-grid,
.closed-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}

.session-card,
.screen-card {
  border-radius: 18px;
  padding: 1rem;
  box-shadow: var(--lg-shadow-sm);
}

.session-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.session-metrics {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 0.45rem;
  margin: 1rem 0;
}

.session-metrics div,
.dashboard-counters div,
.modal-facts span {
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.55rem;
}

.session-metrics strong,
.dashboard-counters strong,
.screen-meta strong,
.modal-facts strong {
  display: block;
  color: var(--text-heading);
  font-weight: 900;
}

.attendance-shell,
.dashboard-toolbar,
.alert-strip,
.closed-section {
  padding: 1rem;
}

.student-table {
  margin-top: 1rem;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 16px;
}

.student-row {
  display: grid;
  grid-template-columns: 0.8fr 1.3fr 0.8fr 0.9fr 0.9fr 1.55fr;
  align-items: center;
  gap: 0.75rem;
  border-top: 1px solid var(--border-default);
  padding: 0.75rem;
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 700;
}

.student-row:first-child {
  border-top: 0;
}

.table-head {
  background: var(--surface-input);
  color: var(--text-muted);
  font-size: 0.68rem;
  font-weight: 900;
  text-transform: uppercase;
}

.row-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.row-actions button,
.screen-actions button,
.modal-actions button {
  min-height: 2rem;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.68rem;
  font-weight: 850;
  cursor: pointer;
}

.row-actions button {
  padding: 0 0.55rem;
}

.status-text.pass {
  color: var(--color-success-text);
}

.status-text.risk {
  color: var(--color-danger-text);
}

.dashboard-toolbar {
  display: grid;
  grid-template-columns: 1fr auto auto;
  align-items: center;
  gap: 1rem;
}

.dashboard-counters {
  display: grid;
  grid-template-columns: repeat(3, 7rem);
  gap: 0.5rem;
}

.alert-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.alert-item,
.empty-alert {
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.65rem;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 750;
}

.alert-item {
  display: flex;
  gap: 0.55rem;
}

.alert-item.critical {
  border-color: color-mix(in srgb, var(--color-danger-text) 32%, transparent);
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.alert-item strong,
.alert-item span {
  display: block;
}

.screen-card {
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, transform 0.18s ease;
}

.screen-card:hover {
  transform: translateY(-2px);
}

.screen-card.has-alert {
  border-color: color-mix(in srgb, var(--color-danger-text) 56%, var(--border-card));
  animation: alert-blink 1.8s ease-in-out infinite;
}

.screen-card-body {
  margin-top: 0.8rem;
}

.student-code {
  color: var(--text-link);
  font-size: 0.68rem;
  font-weight: 900;
}

.screen-card h3 {
  margin: 0.1rem 0 0;
  color: var(--text-heading);
  font-size: 0.9rem;
  font-weight: 850;
}

.screen-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.45rem;
  margin-top: 0.75rem;
}

.screen-actions,
.modal-actions {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.4rem;
  margin-top: 0.8rem;
}

.closed-card {
  display: grid;
  grid-template-columns: 8rem 1fr;
  gap: 0.75rem;
  align-items: center;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--surface-input);
  padding: 0.75rem;
  cursor: pointer;
}

.closed-card strong,
.closed-card span {
  display: block;
}

.closed-card strong {
  color: var(--text-heading);
}

.closed-card span {
  margin-bottom: 0.45rem;
  color: var(--text-muted);
  font-size: 0.76rem;
  font-weight: 700;
}

.placeholder-screen {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 12rem;
  border: 1px dashed var(--border-card);
  border-radius: 14px;
  background: var(--surface-input);
  color: var(--text-placeholder);
  font-size: 0.78rem;
  font-weight: 750;
}

.placeholder-screen.compact {
  min-height: 5rem;
}

.placeholder-screen.large {
  min-height: 28rem;
}

.student-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: grid;
  place-items: center;
  background: rgba(3, 7, 18, 0.72);
  padding: 1.5rem;
}

.student-modal {
  position: relative;
  display: grid;
  width: min(1180px, 96vw);
  max-height: 92vh;
  grid-template-columns: 1fr 340px;
  gap: 1rem;
  overflow: auto;
  border-radius: 20px;
  padding: 1rem;
}

.modal-close {
  position: absolute;
  top: 0.8rem;
  right: 0.8rem;
  z-index: 1;
  display: grid;
  width: 2.1rem;
  height: 2.1rem;
  place-items: center;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-label);
  cursor: pointer;
}

.modal-panel {
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-input);
  padding: 1rem;
}

.modal-facts {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.5rem;
  margin: 1rem 0;
}

.timeline h3 {
  margin: 0 0 0.55rem;
  color: var(--text-heading);
  font-size: 0.86rem;
}

.timeline-list {
  display: flex;
  max-height: 15rem;
  flex-direction: column;
  gap: 0.45rem;
  overflow-y: auto;
}

.timeline-item {
  border: 1px solid color-mix(in srgb, var(--color-danger-text) 25%, transparent);
  border-radius: 12px;
  background: var(--color-danger-bg);
  padding: 0.65rem;
  color: var(--color-danger-text);
  font-size: 0.72rem;
}

.timeline-item.handled {
  border-color: var(--border-default);
  background: var(--surface-card);
  color: var(--text-muted);
}

.timeline-item strong,
.timeline-item span,
.timeline-item small {
  display: block;
}

.modal-actions {
  grid-template-columns: 1fr;
}

.modal-actions .danger {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

@keyframes alert-blink {
  0%, 100% {
    box-shadow: 0 0 0 color-mix(in srgb, var(--color-danger-text) 0%, transparent);
  }
  50% {
    box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-danger-text) 16%, transparent);
  }
}

@media (max-width: 980px) {
  .proctor-header,
  .header-main,
  .header-actions,
  .section-toolbar,
  .alert-strip-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .stats-grid,
  .dashboard-toolbar,
  .student-modal {
    grid-template-columns: 1fr;
  }

  .dashboard-counters,
  .session-metrics,
  .screen-meta {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .student-row {
    grid-template-columns: 1fr;
  }

  .table-head {
    display: none;
  }
}
</style>
