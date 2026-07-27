<template>
  <div class="proctor-page">
    <section class="proctor-header surface-card border-card">
      <div class="header-main">
        <div class="header-icon">
          <Monitor :size="24" />
        </div>
        <div>
          <p class="header-eyebrow">M4 Controlled Exam Environment</p>
          <h1>Dashboard Giám sát</h1>
          <p>Giám sát realtime không gian thi và màn hình thí sinh.</p>
        </div>
      </div>

      <div class="header-actions">
        <button
          type="button"
          class="ghost-action"
          :class="{ 'text-emerald-400': soundEnabled, 'text-slate-400': !soundEnabled }"
          @click="toggleSound"
          :title="soundEnabled ? 'Đang bật âm thanh cảnh báo (Click để tắt)' : 'Đã tắt âm thanh cảnh báo (Click để bật)'"
        >
          <Volume2 v-if="soundEnabled" :size="16" />
          <VolumeX v-else :size="16" />
          <span>{{ soundEnabled ? 'Âm cảnh báo ON' : 'Âm cảnh báo OFF' }}</span>
        </button>

        <button
          type="button"
          class="ghost-action"
          @click="endExamSession"
        >
          <LogOut :size="16" />
          Kết thúc ca thi
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

    <div v-if="loading" class="p-4">
      <ListSkeleton :rows="4" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[400px] gap-4">
      <AlertCircle :size="48" class="text-rose-400" />
      <p class="text-rose-600 font-semibold">{{ error }}</p>
      <button @click="router.push({ name: 'teacher-proctoring-sessions' })" class="btn-primary">Quay lại</button>
    </div>
    <template v-else-if="currentSession">
      <!-- DASHBOARD TOOLBAR -->
      <section class="dashboard-toolbar surface-card border-card">
        <div>
          <p class="section-eyebrow">{{ currentSession.subjectCode }} · {{ currentSession.classCode }}</p>
          <h2>Dashboard Giám sát</h2>
          <p>{{ currentSession.examTitle }} · Phòng: {{ currentSession.room }}</p>
        </div>
        <div class="dashboard-counters">
          <div>
            <span>Đang thi</span>
            <strong>{{ activeCount }} / {{ currentStudents.length }}</strong>
          </div>
          <div>
            <span>Cảnh báo mới</span>
            <strong class="text-rose-500">{{ unhandledViolations.length }}</strong>
          </div>
          <div>
            <span>Đã nộp bài</span>
            <strong class="text-teal-500">{{ submittedCount }}</strong>
          </div>
        </div>
        <button
          type="button"
          class="primary-action text-white"
          style="background: #10b981; border: none;"
          @click="startExamSession"
        >
          <Play :size="16" />
          Mở ca thi cho sinh viên
        </button>

        <button
          type="button"
          class="danger-action"
          :disabled="!isMonitoring"
          @click="suspendExamSession"
        >
          <ShieldAlert :size="16" />
          Tạm dừng ca thi
        </button>
      </section>

      <!-- ALERTS STRIP -->
      <section v-if="unhandledViolations.length > 0" class="alert-strip surface-card border-card">
        <div class="alert-strip-head">
          <div class="flex items-center gap-2">
            <AlertTriangle :size="18" class="text-rose-500" />
            <h3 class="font-bold text-rose-500 m-0">Cảnh báo vi phạm ({{ unhandledViolations.length }})</h3>
          </div>
          <button type="button" class="ghost-action" @click="clearAllAlerts">Đánh dấu đã xem tất cả</button>
        </div>
        <div class="alert-list">
          <div
            v-for="alert in unhandledViolations"
            :key="alert.id"
            class="alert-item"
            :class="{ critical: ['high', 'critical'].includes(alert.severity) }"
          >
            <ShieldAlert v-if="['high', 'critical'].includes(alert.severity)" :size="16" />
            <AlertCircle v-else :size="16" />
            <div>
              <strong>{{ alert.studentCode }} - {{ violationLabel(alert.type) }}</strong>
              <span>{{ formatViolationTime(alert.timestamp) }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- CONTROLS & FILTERS BAR -->
      <section class="students-controls-bar surface-card border-card">
        <div class="search-and-filters">
          <div class="search-box">
            <Search :size="16" class="search-icon" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo Mã SV, Họ tên..."
              class="search-input"
            />
          </div>

          <div class="filter-pills">
            <button
              type="button"
              class="filter-pill"
              :class="{ active: statusFilter === 'all' }"
              @click="statusFilter = 'all'"
            >
              Tất cả ({{ currentStudents.length }})
            </button>
            <button
              type="button"
              class="filter-pill"
              :class="{ active: statusFilter === 'in_progress' }"
              @click="statusFilter = 'in_progress'"
            >
              Đang thi ({{ activeCount }})
            </button>
            <button
              type="button"
              class="filter-pill"
              :class="{ active: statusFilter === 'violation' }"
              @click="statusFilter = 'violation'"
            >
              Cảnh báo ({{ unhandledViolations.length }})
            </button>
            <button
              type="button"
              class="filter-pill"
              :class="{ active: statusFilter === 'submitted' }"
              @click="statusFilter = 'submitted'"
            >
              Đã nộp bài ({{ submittedCount }})
            </button>
          </div>
        </div>

        <div class="view-switcher">
          <button
            type="button"
            class="switch-btn"
            :class="{ active: viewMode === 'table' }"
            @click="viewMode = 'table'"
            title="Hiển thị dạng Bảng danh sách"
          >
            <LayoutList :size="16" />
            <span>Bảng danh sách</span>
          </button>
          <button
            type="button"
            class="switch-btn"
            :class="{ active: viewMode === 'grid' }"
            @click="viewMode = 'grid'"
            title="Hiển thị dạng Lưới màn hình"
          >
            <LayoutGrid :size="16" />
            <span>Lưới màn hình</span>
          </button>
        </div>
      </section>

      <!-- TABLE VIEW (DEFAULT) -->
      <section v-if="viewMode === 'table'" class="students-table-container surface-card border-card">
        <div class="table-responsive">
          <table class="students-table">
            <thead>
              <tr>
                <th style="width: 50px;">STT</th>
                <th>Thí sinh</th>
                <th>Trạng thái thi</th>
                <th>Kết nối</th>
                <th>Màn hình chia sẻ</th>
                <th>Cảnh báo / Vi phạm</th>
                <th style="text-align: right; padding-right: 1.25rem;">Thao tác giám sát</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(student, index) in filteredStudents"
                :key="student.id || student.studentId"
                class="student-row"
                :class="{ 'has-alert-row': hasUnhandledViolation(student) }"
                @click="openStudentModal(student)"
              >
                <td class="text-center font-semibold text-slate-400">{{ index + 1 }}</td>
                <td>
                  <div class="student-info-cell">
                    <div class="student-avatar-chip">
                      {{ (student.name || 'SV').charAt(0).toUpperCase() }}
                    </div>
                    <div>
                      <span class="student-code-text">{{ student.studentCode }}</span>
                      <div class="student-name-text">{{ student.name }}</div>
                    </div>
                  </div>
                </td>
                <td>
                  <GlassBadge v-if="student.examStatus === 'in_progress'" variant="success">Đang thi</GlassBadge>
                  <GlassBadge v-else-if="student.examStatus === 'submitted'" variant="info">Đã nộp bài</GlassBadge>
                  <GlassBadge v-else-if="student.examStatus === 'suspended'" variant="danger">Bị đình chỉ</GlassBadge>
                  <GlassBadge v-else variant="default">{{ examStatusLabel(student.examStatus) }}</GlassBadge>
                </td>
                <td>
                  <span v-if="student.connectionId" class="connection-status-tag online">
                    <span class="dot"></span> Online
                  </span>
                  <span v-else class="connection-status-tag offline">
                    <span class="dot"></span> Offline
                  </span>
                </td>
                <td>
                  <span v-if="studentStreams[student.studentId]" class="stream-status-tag active">
                    <MonitorPlay :size="14" /> Đang phát live
                  </span>
                  <span v-else-if="student.streamStatus === 'streaming' || student.streamStatus === 'active'" class="stream-status-tag active">
                    <Monitor :size="14" /> Sẵn sàng
                  </span>
                  <span v-else class="stream-status-tag inactive">
                    <VideoOff :size="14" /> {{ streamLabel(student.streamStatus) }}
                  </span>
                </td>
                <td>
                  <div v-if="studentViolationCount(student) > 0" class="flex items-center gap-2">
                    <GlassBadge variant="danger" class="animate-pulse">
                      {{ studentViolationCount(student) }} Vi phạm
                    </GlassBadge>
                    <span class="text-xs text-rose-400 truncate max-w-[180px]" :title="latestViolationLabel(student)">
                      {{ latestViolationLabel(student) }}
                    </span>
                  </div>
                  <span v-else class="text-xs text-slate-400">Không có</span>
                </td>
                <td class="text-right" @click.stop>
                  <div class="row-actions">
                    <button
                      type="button"
                      class="btn-action-primary"
                      @click="requestScreenStream(student)"
                      title="Mở popup xem màn hình chia sẻ trực tiếp"
                    >
                      <MonitorPlay :size="14" />
                      <span>Xem màn hình</span>
                    </button>

                    <button
                      type="button"
                      class="btn-action-icon"
                      @click="showReminderDialog(student)"
                      title="Nhắc nhở thí sinh"
                    >
                      <MessageSquareWarning :size="14" />
                    </button>

                    <button
                      v-if="hasUnhandledViolation(student)"
                      type="button"
                      class="btn-action-icon text-teal-400"
                      @click="markStudentHandled(student)"
                      title="Đánh dấu đã xử lý vi phạm"
                    >
                      <Check :size="14" />
                    </button>

                    <button
                      type="button"
                      class="btn-action-icon text-rose-400 hover:text-rose-300"
                      @click="suspendStudent(student)"
                      title="Đình chỉ bài thi"
                    >
                      <ShieldAlert :size="14" />
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredStudents.length === 0">
                <td colspan="7" class="text-center py-8 text-slate-400">
                  Không tìm thấy thí sinh phù hợp
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- GRID VIEW -->
      <section v-else class="screen-grid">
        <article
          v-for="student in filteredStudents"
          :key="student.id"
          class="screen-card surface-card border-card"
          :class="{ 'has-alert': hasUnhandledViolation(student) }"
          @click="openStudentModal(student)"
        >
          <div class="video-container" style="aspect-ratio: 16/9; background: #000; border-radius: 12px; overflow: hidden; position: relative;">
            <video
              :ref="(el) => setVideoRef(el, student.studentId)"
              autoplay
              playsinline
              muted
              style="width: 100%; height: 100%; object-fit: cover;"
              v-show="studentStreams[student.studentId]"
            ></video>
            
            <div
              v-if="!studentStreams[student.studentId]"
              class="absolute inset-0 flex flex-col items-center justify-center text-slate-400 gap-2"
            >
              <VideoOff v-if="student.streamStatus === 'lost' || student.streamStatus === 'stopped'" :size="32" />
              <Loader2 v-else-if="student.streamStatus === 'reconnecting' || student.streamStatus === 'waiting'" :size="32" class="animate-spin" />
              <Monitor v-else :size="32" />
              <span class="text-xs font-semibold">{{ streamLabel(student.streamStatus) }}</span>
            </div>

            <!-- Overlay metrics -->
            <div class="absolute bottom-2 left-2 flex gap-1">
              <GlassBadge v-if="student.examStatus === 'submitted'" variant="success">Đã nộp</GlassBadge>
              <GlassBadge v-else-if="student.examStatus === 'suspended'" variant="danger">Đình chỉ</GlassBadge>
              <GlassBadge v-if="hasUnhandledViolation(student)" variant="danger">
                {{ studentViolationCount(student) }} Vi phạm
              </GlassBadge>
            </div>
          </div>

          <div class="screen-card-body">
            <span class="student-code">{{ student.studentCode }}</span>
            <h3>{{ student.name }}</h3>
            
            <div class="screen-meta">
              <div>
                <span>Trạng thái</span>
                <strong>{{ examStatusLabel(student.examStatus) }}</strong>
              </div>
              <div>
                <span>Cảnh báo gần nhất</span>
                <strong>{{ latestViolationLabel(student) }}</strong>
              </div>
            </div>

            <div class="screen-actions">
              <button type="button" @click.stop="requestScreenStream(student)" title="Yêu cầu Stream màn hình">
                <MonitorPlay :size="14" />
              </button>
              <button type="button" @click.stop="requestCameraStream(student)" title="Yêu cầu Camera">
                <Video :size="14" />
              </button>
              <button type="button" @click.stop="sendReminder(student)" title="Nhắc nhở">
                <MessageSquareWarning :size="14" />
              </button>
              <button type="button" @click.stop="suspendStudent(student)" title="Đình chỉ" class="text-rose-500">
                <ShieldAlert :size="14" />
              </button>
            </div>
          </div>
        </article>
      </section>
    </template>
    
    <!-- STUDENT MODAL -->
    <Teleport to="body">
      <div v-if="selectedStudent" class="student-modal-backdrop" @click="closeStudentModal">
        <div class="student-modal surface-card border-card" @click.stop>
          <button type="button" class="modal-close" @click="closeStudentModal">
            <X :size="18" />
          </button>
          
          <div class="modal-main">
            <div class="video-container large mb-4" style="aspect-ratio: 16/9; background: #000; border-radius: 12px; overflow: hidden; position: relative;">
              <video
                :ref="(el) => setModalVideoRef(el, selectedStudent?.studentId)"
                autoplay
                playsinline
                muted
                style="width: 100%; height: 100%; object-fit: contain;"
                v-show="studentStreams[selectedStudent?.studentId]"
              ></video>
              
              <div
                v-if="!studentStreams[selectedStudent?.studentId]"
                class="absolute inset-0 flex flex-col items-center justify-center text-slate-400 gap-2"
              >
                <VideoOff :size="48" />
                <span class="text-sm font-semibold">{{ streamLabel(selectedStudent?.streamStatus) }}</span>
              </div>
            </div>

            <div class="modal-actions">
              <button type="button" @click="requestScreenStream(selectedStudent)">
                <MonitorPlay :size="14" class="mr-1" inline /> Xem màn hình
              </button>
              <button type="button" @click="requestCameraStream(selectedStudent)">
                <Video :size="14" class="mr-1" inline /> Xem camera
              </button>
              <button type="button" @click="showReminderDialog(selectedStudent)">
                <MessageSquareWarning :size="14" class="mr-1" inline /> Nhắc nhở
              </button>
              <button type="button" @click="suspendStudent(selectedStudent)" class="danger">
                <ShieldAlert :size="14" class="mr-1" inline /> Đình chỉ
              </button>
            </div>
          </div>

          <aside class="modal-sidebar">
            <div class="modal-panel mb-4">
              <span class="student-code">{{ selectedStudent?.studentCode }}</span>
              <h2>{{ selectedStudent?.name }}</h2>
              <GlassBadge :variant="selectedStudent?.examStatus === 'in_progress' ? 'success' : 'warning'">
                {{ examStatusLabel(selectedStudent?.examStatus) }}
              </GlassBadge>
              
              <div style="margin-top: 10px; display: flex; flex-direction: column; gap: 4px;">
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 5px 0; border-bottom: 1px solid var(--border-default); font-size: 0.78rem;">
                  <span style="color: var(--text-label);">Vi phạm chưa xử lý</span>
                  <strong :class="studentViolationCount(selectedStudent) > 0 ? 'text-rose-500' : 'text-green-500'" style="font-size: 0.82rem;">
                    {{ studentViolationCount(selectedStudent) }} vi phạm
                  </strong>
                </div>
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 5px 0; font-size: 0.78rem;">
                  <span style="color: var(--text-label);">Trạng thái kết nối</span>
                  <strong :class="selectedStudent?.connectionId ? 'text-green-500' : 'text-slate-400'" style="font-size: 0.82rem;">
                    <span v-if="selectedStudent?.connectionId">&#9679; Online</span>
                    <span v-else>&#9675; Offline</span>
                  </strong>
                </div>
              </div>
              
              <button
                v-if="hasUnhandledViolation(selectedStudent)"
                type="button"
                class="primary-action w-full"
                @click="markStudentHandled(selectedStudent)"
              >
                Đánh dấu đã xử lý tất cả
              </button>
            </div>

            <div class="modal-panel timeline">
              <h3>Lịch sử hoạt động</h3>
              <div class="timeline-list">
                <div
                  v-for="log in sortedStudentLogs"
                  :key="log.timestamp + log.type"
                  class="timeline-item"
                  :class="{ handled: log.handled || log.type !== 'VIOLATION' }"
                >
                  <strong>{{ log.title || violationLabel(log.type) }}</strong>
                  <span>{{ log.message || 'Cảnh báo tự động' }}</span>
                  <small>{{ formatViolationTime(log.timestamp) }}</small>
                </div>
                <div v-if="!sortedStudentLogs.length" class="text-center text-slate-400 text-xs py-4">
                  Chưa có dữ liệu
                </div>
              </div>
            </div>
          </aside>
        </div>
      </div>
    </Teleport>

    <!-- REMINDER DIALOG -->
    <Teleport to="body">
      <div v-if="reminderDialog.visible" class="student-modal-backdrop" @click="reminderDialog.visible = false">
        <div class="reminder-dialog surface-card border-card" @click.stop>
          <h3 class="reminder-title">
            <MessageSquareWarning :size="18" />
            Nhắc nhở thí sinh
          </h3>
          <p class="reminder-student">{{ reminderDialog.student?.studentCode }} &mdash; {{ reminderDialog.student?.name }}</p>

          <div class="reminder-presets">
            <button
              v-for="preset in reminderPresets"
              :key="preset"
              type="button"
              class="preset-btn"
              :class="{ active: reminderDialog.message === preset }"
              @click="reminderDialog.message = preset"
            >{{ preset }}</button>
          </div>

          <div class="reminder-actions">
            <button type="button" class="ghost-btn" @click="reminderDialog.visible = false">Hủy</button>
            <button type="button" class="send-btn" @click="sendReminderConfirm">
              <MessageSquareWarning :size="14" /> Gửi nhắc nhở
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- REALTIME VIOLATION POPUP BANNER (CENTERED, THEME-ADAPTIVE, LARGER) -->
    <Teleport to="body">
      <div
        v-if="activeViolationAlert"
        class="violation-alert-backdrop"
        style="position: fixed; inset: 0; z-index: 9999999; display: flex; align-items: center; justify-content: center; background: rgba(0, 0, 0, 0.5); backdrop-filter: blur(6px);"
        @click.self="activeViolationAlert = null"
      >
        <div
          class="violation-alert-modal lg-glass-strong surface-card border-card"
          style="width: 100%; max-width: 540px; margin: 1.25rem; padding: 1.75rem; border-radius: 24px; position: relative; box-shadow: 0 20px 50px rgba(225, 29, 72, 0.35); border: 2px solid rgba(244, 63, 94, 0.4);"
        >
          <!-- Top Accent Warning Bar -->
          <div style="position: absolute; top: 0; left: 0; right: 0; height: 6px; background: linear-gradient(90deg, #e11d48, #f59e0b, #e11d48); border-top-left-radius: 24px; border-top-right-radius: 24px;"></div>

          <!-- Header -->
          <div class="flex items-start justify-between gap-4 mb-4">
            <div class="flex items-center gap-3">
              <div class="p-3 rounded-2xl bg-rose-500/15 text-rose-500 border border-rose-500/30">
                <ShieldAlert :size="28" />
              </div>
              <div>
                <h3 class="font-extrabold text-heading text-lg m-0 flex items-center gap-2" style="color: #f43f5e;">
                  🚨 CẢNH BÁO VI PHẠM THI!
                </h3>
                <span class="text-xs text-label font-medium">Thời gian: {{ formatViolationTime(activeViolationAlert.timestamp) }}</span>
              </div>
            </div>
            <button
              type="button"
              class="p-2 rounded-xl text-slate-400 hover:text-heading hover:bg-black/10 dark:hover:bg-white/10 transition-colors"
              @click="activeViolationAlert = null"
            >
              <X :size="22" />
            </button>
          </div>

          <!-- Content Body -->
          <div class="space-y-3 mb-5 p-4 rounded-2xl surface-input border-card" style="background: rgba(0, 0, 0, 0.04);">
            <div class="flex items-center justify-between text-sm">
              <span class="text-label font-medium">Thí sinh vi phạm:</span>
              <strong class="text-heading font-extrabold text-base text-amber-500 dark:text-amber-300">
                {{ activeViolationAlert.studentName || activeViolationAlert.studentCode }} ({{ activeViolationAlert.studentCode }})
              </strong>
            </div>

            <div class="flex items-center justify-between text-sm">
              <span class="text-label font-medium">Hành vi vi phạm:</span>
              <span class="px-3 py-1 rounded-xl text-rose-600 dark:text-rose-300 bg-rose-500/15 font-bold border border-rose-500/30 text-xs">
                {{ violationLabel(activeViolationAlert.type) }}
              </span>
            </div>

            <div v-if="activeViolationAlert.details" class="text-xs text-body pt-2 border-t border-card">
              <span class="text-label font-semibold">Mô tả chi tiết: </span>
              <span>{{ activeViolationAlert.details }}</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center gap-3">
            <button
              type="button"
              class="flex-1 py-3 px-4 rounded-xl bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-bold text-sm flex items-center justify-center gap-2 shadow-lg transition-all transform hover:-translate-y-0.5"
              @click="openViolationStudentModal(activeViolationAlert)"
            >
              <MonitorPlay :size="18" />
              Xem màn hình & Chi tiết
            </button>

            <button
              type="button"
              class="py-3 px-5 rounded-xl surface-input border-card text-heading font-semibold text-sm hover:bg-black/5 dark:hover:bg-white/10 transition-colors"
              @click="activeViolationAlert = null"
            >
              Đóng
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick, markRaw } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Monitor, Clock, LogOut, AlertCircle, ShieldAlert,
  AlertTriangle, VideoOff, Loader2, MonitorPlay, Video,
  MessageSquareWarning, X, LayoutList, LayoutGrid, Search, Check, Play,
  Volume2, VolumeX
} from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { usePopupStore } from '@/stores/popup'
import { useAuthStore } from '@/stores/auth'
import { teacherApi } from '@/services/teacherApi'
import { examProctoringHub } from '@/services/examProctoringHub'
import { createProctorPeerConnection } from '@/services/webrtcScreenShare'
import { useProctoringSession } from '@/composables/useProctoringSession'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const {
  loading,
  error,
  currentSession,
  currentStudents,
  loadSessionData,
  formatSessionTime,
  formatViolationTime,
  violationLabel,
  streamLabel,
  examStatusLabel,
} = useProctoringSession()

const currentTime = ref('')
let clockTimer = null

const viewMode = ref('table')
const searchQuery = ref('')
const statusFilter = ref('all')

const filteredStudents = computed(() => {
  return currentStudents.value.filter((student) => {
    if (searchQuery.value.trim()) {
      const q = searchQuery.value.trim().toLowerCase()
      const codeMatch = student.studentCode?.toLowerCase().includes(q)
      const nameMatch = student.name?.toLowerCase().includes(q)
      if (!codeMatch && !nameMatch) return false
    }
    if (statusFilter.value === 'in_progress') {
      return student.examStatus === 'in_progress'
    }
    if (statusFilter.value === 'submitted') {
      return student.examStatus === 'submitted'
    }
    if (statusFilter.value === 'suspended') {
      return student.examStatus === 'suspended'
    }
    if (statusFilter.value === 'violation') {
      return hasUnhandledViolation(student)
    }
    return true
  })
})

// Dùng Map thường (KHÔNG reactive) để tránh Vue proxy làm hỏng các method WebRTC gốc
const peerConnections = new Map()
const iceCandidateQueue = new Map()
const studentStreams = ref({})
const isMonitoring = ref(false)
const remoteVideoRefs = ref({})
const liveViolations = ref([])
const selectedStudent = ref(null)
const activeViolationAlert = ref(null)

function openViolationStudentModal(violation) {
  if (!violation) return
  const student = currentStudents.value.find(s => 
    String(s.studentId || s.id) === String(violation.studentId || violation.maHocSinh) || 
    (violation.studentCode && s.studentCode === violation.studentCode)
  )

  activeViolationAlert.value = null // Close banner

  if (student) {
    requestScreenStream(student) // Open student modal & stream!
  } else {
    popupStore.warning('Thông báo', `Thí sinh ${violation.studentCode || violation.studentId} không tìm thấy trong danh sách ca thi.`)
  }
}

const soundEnabled = ref(true)
let audioObj = null

function toggleSound() {
  soundEnabled.value = !soundEnabled.value
  if (soundEnabled.value) {
    popupStore.info('Âm thanh cảnh báo', 'Đã bật âm thanh cảnh báo vi phạm.')
    playViolationSound()
  } else {
    popupStore.info('Âm thanh cảnh báo', 'Đã tắt âm thanh cảnh báo.')
  }
}

function playViolationSound() {
  if (!soundEnabled.value) return

  try {
    if (!audioObj) {
      audioObj = new Audio('/sound.mp3')
    }
    audioObj.currentTime = 0
    const playPromise = audioObj.play()
    if (playPromise !== undefined) {
      playPromise.catch((err) => {
        console.warn('[Audio] /sound.mp3 failed, trying /Soud canh bao.mp3:', err)
        const altAudio = new Audio(encodeURI('/Soud canh bao.mp3'))
        altAudio.play().catch(() => playBeepFallback())
      })
    }
  } catch (e) {
    playBeepFallback()
  }
}

function playBeepFallback() {
  try {
    const AudioCtx = window.AudioContext || window.webkitAudioContext
    if (!AudioCtx) return
    const ctx = new AudioCtx()
    
    [0, 0.2, 0.4].forEach(delay => {
      const osc = ctx.createOscillator()
      const gain = ctx.createGain()
      osc.type = 'sawtooth'
      osc.frequency.setValueAtTime(880, ctx.currentTime + delay)
      osc.frequency.exponentialRampToValueAtTime(440, ctx.currentTime + delay + 0.15)
      
      gain.gain.setValueAtTime(0.3, ctx.currentTime + delay)
      gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + delay + 0.15)
      
      osc.connect(gain)
      gain.connect(ctx.destination)
      osc.start(ctx.currentTime + delay)
      osc.stop(ctx.currentTime + delay + 0.15)
    })
  } catch (err) {
    console.warn('[Audio] Beep fallback error:', err)
  }
}

const reminderDialog = ref({ visible: false, student: null, message: '' })
const reminderPresets = [
  'Vui lòng quay lại bài thi và tuân thủ quy định.',
  'Không được chuyển tab hoặc rời khỏi cửa sổ thi.',
  'Hãy giữ nàn hì̀nh, không có vấn đề gì đầu.',
  'Liên hệ giám thị nếu có vấn đề kỹ thuật.',
]

const activeCount = computed(() => {
  return currentStudents.value.filter((s) => s.examStatus === 'in_progress').length
})

const submittedCount = computed(() => {
  return currentStudents.value.filter((s) => s.examStatus === 'submitted').length
})

const unhandledViolations = computed(() => {
  return liveViolations.value.filter((v) => !v.handled)
})

const sortedStudentLogs = computed(() => {
  if (!selectedStudent.value) return []
  const logs = selectedStudent.value.logs || []
  const viols = violationsForStudent(selectedStudent.value).map(v => ({
    ...v,
    title: violationLabel(v.type),
    message: v.details || 'Hệ thống tự động ghi nhận'
  }))
  return [...logs, ...viols].sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
})

function updateTime() {
  currentTime.value = new Date().toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

onMounted(async () => {
  const sessionId = route.params.sessionId
  if (sessionId) {
    await loadSessionData(sessionId)
    if (currentSession.value) {
      await initializeHub(currentSession.value.id)
    }
  }
  updateTime()
  clockTimer = window.setInterval(updateTime, 1000)
})

onUnmounted(() => {
  if (clockTimer) window.clearInterval(clockTimer)
  cleanupWebRTC()
  examProctoringHub.disconnect()
})

// WebRTC & Hub Logic
function cleanupWebRTC() {
  // Stop all WebRTC tracks and close connections
  for (const stream of Object.values(studentStreams.value)) {
    if (stream) {
      stream.getTracks().forEach(track => track.stop())
    }
  }
  for (const pc of peerConnections.values()) {
    if (pc) pc.close()
  }
  peerConnections.clear()
  studentStreams.value = {}
  iceCandidateQueue.clear()
}

async function initializeHub(sessionId) {
  try {
    const authStore = useAuthStore()
    const token = authStore.accessToken || localStorage.getItem('token')
    if (!token) {
      console.warn('[Proctor] No auth token available, aborting initializeHub')
      return
    }

    console.log('[Proctor] Connecting to hub...')
    await examProctoringHub.connect(token)
    console.log('[Proctor] Hub connect call finished. isConnected:', examProctoringHub.isConnected)

    isMonitoring.value = true

    // Hàm tạo và gửi Offer cho thí sinh
    const createAndSendOffer = async (maHocSinh, targetConnectionId) => {
      let pc = peerConnections.get(maHocSinh)
      if (pc) {
        pc.close()
        peerConnections.delete(maHocSinh)
      }
      iceCandidateQueue.delete(maHocSinh)

      pc = createProctorPeerConnection(
        (candidate) => {
          examProctoringHub.sendIceCandidate({
            maCaThi: sessionId,
            maHocSinh,
            targetConnectionId,
            candidate
          })
        },
        (stream) => {
          studentStreams.value = { 
            ...studentStreams.value, 
            [maHocSinh]: markRaw(stream)
          }
        },
        async () => {
          if (import.meta.env.DEV) console.warn(`[Proctor] WebRTC reconnecting for student ${maHocSinh}...`)
          await createAndSendOffer(maHocSinh, targetConnectionId)
        }
      )
      peerConnections.set(maHocSinh, pc)

      try {
        const offer = await pc.createOffer({ offerToReceiveVideo: true, offerToReceiveAudio: false })
        await pc.setLocalDescription(offer)

        examProctoringHub.sendOffer({
          maCaThi: sessionId,
          maHocSinh,
          targetConnectionId,
          offer: { type: pc.localDescription.type, sdp: pc.localDescription.sdp }
        })
      } catch (err) {
        console.error(`Error creating offer for student ${maHocSinh}:`, err)
      }
    }

    // Lắng nghe Answer từ thí sinh
    examProctoringHub.eventHandlers.onReceiveAnswer = async (dto) => {
      const { maCaThi, maHocSinh, answer } = dto
      if (maCaThi != sessionId) return

      const pc = peerConnections.get(maHocSinh)
      if (pc) {
        try {
          await pc.setRemoteDescription(new RTCSessionDescription(answer))

          // Flush ICE queue now that remote description is set
          if (iceCandidateQueue.has(maHocSinh)) {
            const queue = iceCandidateQueue.get(maHocSinh)
            while (queue.length > 0) {
              const c = queue.shift()
              try { await pc.addIceCandidate(new RTCIceCandidate(c)) }
              catch (e) { console.warn(`Error flushing ICE for ${maHocSinh}`, e) }
            }
            iceCandidateQueue.delete(maHocSinh)
          }
        } catch (err) {
          console.error(`Error handling answer from student ${maHocSinh}:`, err)
        }
      }
    }

    examProctoringHub.eventHandlers.onReceiveIceCandidate = async (dto) => {
      const { maHocSinh, candidate } = dto
      const pc = peerConnections.get(maHocSinh)
      if (pc && candidate) {
        if (!pc.remoteDescription) {
          if (!iceCandidateQueue.has(maHocSinh)) {
            iceCandidateQueue.set(maHocSinh, [])
          }
          iceCandidateQueue.get(maHocSinh).push(candidate)
        } else {
          try {
            await pc.addIceCandidate(new RTCIceCandidate(candidate))
          } catch (e) {
            console.error(`Error adding ICE candidate from student ${maHocSinh}:`, e)
          }
        }
      }
    }
    
    examProctoringHub.eventHandlers.onStudentConnectionIdBroadcast = async (payload) => {
      console.log('[Proctor] onStudentConnectionIdBroadcast', payload)
      if (payload.maCaThi == sessionId) {
        const student = currentStudents.value.find(s => s.studentId === payload.maHocSinh || s.id === payload.maHocSinh)
        console.log('[Proctor] Found student:', student?.studentCode, 'maHocSinh:', payload.maHocSinh)
        if (student) {
          student.connectionId = payload.connectionId
        }
        await examProctoringHub.acknowledgeStudent(payload.connectionId)
        
        // Tạo offer ngay khi biết connectionId
        await createAndSendOffer(payload.maHocSinh, payload.connectionId)
      }
    }
    
    examProctoringHub.eventHandlers.onScreenShareStatusChanged = async (payload) => {
      console.log('[Proctor] onScreenShareStatusChanged', payload)
      const student = currentStudents.value.find(s => s.studentId === payload.maHocSinh || s.id === payload.maHocSinh)
      if (student) {
        student.streamStatus = payload.status
      }

      if ((payload.status === 'streaming' || payload.status === 'active') && student?.connectionId) {
        console.log('[Proctor] Student started sharing, sending new offer to connectionId:', student.connectionId)
        await createAndSendOffer(payload.maHocSinh, student.connectionId)
      } else if ((payload.status === 'streaming' || payload.status === 'active') && !student?.connectionId) {
        console.warn('[Proctor] Student streaming but no connectionId yet, will wait for StudentConnectionIdBroadcast')
      }

      if (payload.status === 'stopped' && studentStreams.value[payload.maHocSinh]) {
         const stream = studentStreams.value[payload.maHocSinh]
         stream.getTracks().forEach(track => track.stop())
         const newStreams = { ...studentStreams.value }
         delete newStreams[payload.maHocSinh]
         studentStreams.value = newStreams
         
         const pc = peerConnections.get(payload.maHocSinh)
         if (pc) {
           pc.close()
           peerConnections.delete(payload.maHocSinh)
         }
      }
    }

    examProctoringHub.eventHandlers.onViolationDetected = (payload) => {
      console.log('[Proctor] onViolationDetected received:', payload)
      const studentId = payload.studentId || payload.maHocSinh
      const studentCode = payload.studentCode
      const st = currentStudents.value.find(s => String(s.studentId || s.id) === String(studentId) || (studentCode && s.studentCode === studentCode))

      const normViolation = {
        ...payload,
        id: payload.id || Date.now() + Math.random(),
        type: payload.type || payload.loaiViPham,
        studentId: studentId,
        studentCode: studentCode || st?.studentCode,
        details: payload.details || payload.chiTiet || 'Cảnh báo tự động',
        timestamp: payload.timestamp || payload.thoiDiem || new Date().toISOString(),
        handled: false,
      }

      // Check if identical violation was received within 3 seconds
      const isDuplicate = liveViolations.value.some(v => 
        String(v.studentId || v.maHocSinh) === String(normViolation.studentId) &&
        v.type === normViolation.type &&
        Math.abs(new Date(v.timestamp).getTime() - new Date(normViolation.timestamp).getTime()) < 3000
      )

      if (isDuplicate) {
        console.warn('[Proctor] Ignored duplicate violation event:', normViolation)
        return
      }

      liveViolations.value.unshift(normViolation)
      popupStore.warning(
        'Cảnh báo vi phạm mới!',
        `${normViolation.studentCode || normViolation.studentId} - ${violationLabel(normViolation.type)}`
      )

      // 🚨 Hiển thị Popup Banner Cảnh báo nổi bật cho Giám thị
      activeViolationAlert.value = normViolation

      // Tự động ẩn popup sau 12 giây nếu không thao tác
      const alertId = normViolation.id
      setTimeout(() => {
        if (activeViolationAlert.value?.id === alertId) {
          activeViolationAlert.value = null
        }
      }, 12000)

      // 🔔 Phát âm thanh báo động vi phạm cho Giám thị
      playViolationSound()
    }

    // Khi SignalR tự reconnect, cần rejoin exam room để backend broadcast lại student connectionIds
    examProctoringHub.eventHandlers.onReconnected = async () => {
      console.log('[Proctor] Hub reconnected, rejoining exam room', sessionId)
      await examProctoringHub.joinExamRoom(parseInt(sessionId, 10)).catch(console.error)
    }

    if (examProctoringHub.isConnected) {
      console.log('[Proctor] Hub connected, auto-joining exam room', sessionId)
      await examProctoringHub.joinExamRoom(parseInt(sessionId, 10)).catch(console.error)
    }

    const violationsData = await teacherApi.getExamViolations(sessionId)
    const rawViolations = Array.isArray(violationsData) ? violationsData : (violationsData?.data?.items ?? violationsData?.data ?? violationsData?.items ?? [])
    liveViolations.value = rawViolations.map(v => {
      const studentId = v.studentId || v.maHocSinh
      const st = currentStudents.value.find(s => String(s.studentId || s.id) === String(studentId) || (v.studentCode && s.studentCode === v.studentCode))
      return {
        ...v,
        id: v.id || Date.now() + Math.random(),
        type: v.type || v.loaiViPham,
        studentId: studentId,
        studentCode: v.studentCode || st?.studentCode,
        details: v.details || v.chiTiet || 'Cảnh báo tự động',
        timestamp: v.timestamp || v.thoiDiem || v.ngayTao || new Date().toISOString(),
        handled: false
      }
    })

  } catch (err) {
    console.error('Hub error:', err)
    popupStore.error('Lỗi kết nối', 'Không thể kết nối server giám sát.')
  }
}

function setVideoRef(el, code) {
  if (el) {
    remoteVideoRefs.value[code] = el
    if (studentStreams.value[code] && el.srcObject !== studentStreams.value[code]) {
      el.srcObject = studentStreams.value[code]
    }
  }
}

function setModalVideoRef(el, code) {
  if (el) {
    remoteVideoRefs.value[`modal_${code}`] = el
    if (studentStreams.value[code] && el.srcObject !== studentStreams.value[code]) {
      el.srcObject = studentStreams.value[code]
    }
  }
}

// Actions
function requestScreenStream(student) {
  // Mở modal để xem stream
  openStudentModal(student)
  
  if (currentSession.value && student.connectionId) {
    // Chặn fullscreen video modal sau khi render
    nextTick(() => {
      const modalVideoEl = remoteVideoRefs.value[`modal_${student.studentId}`]
      if (modalVideoEl && studentStreams.value[student.studentId]) {
        modalVideoEl.requestFullscreen?.().catch(() => {})
      }
    })
  } else if (!student.connectionId) {
    popupStore.warning('Không thể gửi', 'Thí sinh chưa kết nối.')
  }
}

function requestCameraStream(student) {
  if (currentSession.value && student.connectionId) {
    if (typeof examProctoringHub.requestStream === 'function') {
      examProctoringHub.requestStream(currentSession.value.id, student.connectionId, 'camera').catch(() => {})
    } else {
      examProctoringHub.sendWarningToStudent(currentSession.value.id, student.connectionId, 'Yêu cầu mở camera').catch(() => {})
    }
    popupStore.info('Đã gửi yêu cầu', `Yêu cầu stream camera đến ${student.studentCode}`)
  } else {
    popupStore.warning('Không thể gửi', 'Thí sinh chưa kết nối.')
  }
}

async function startExamSession() {
  if (!confirm('Bạn có chắc chắn muốn mở ca thi cho sinh viên bắt đầu vào làm bài?')) return
  if (currentSession.value) {
    try {
      await teacherApi.startExamSession(currentSession.value.id)
      popupStore.success('Đã mở ca thi', 'Ca thi đã được mở thành công. Sinh viên có thể bấm "Vào làm"!')
    } catch (e) {
      popupStore.error('Lỗi', 'Không thể mở ca thi.')
    }
  }
}

async function endExamSession() {
  if (!confirm('Bạn có chắc chắn muốn kết thúc ca thi này?')) return

  if (currentSession.value) {
    try {
      await teacherApi.endExamSession(currentSession.value.id)
      cleanupWebRTC()
      isMonitoring.value = false
      router.push({ name: 'teacher-proctoring-sessions' })
    } catch (e) {
      popupStore.error('Lỗi', 'Không thể kết thúc ca thi.')
    }
  }
}

async function suspendExamSession() {
  if (!confirm('Tạm dừng toàn bộ ca thi? Học sinh sẽ không thể làm bài tiếp.')) return
  if (currentSession.value) {
    try {
      await teacherApi.suspendExamSession(currentSession.value.id)
      popupStore.warning('Đã tạm dừng', 'Toàn bộ ca thi đã bị tạm dừng.')
    } catch (e) {
      popupStore.error('Lỗi', 'Không thể tạm dừng ca thi.')
    }
  }
}

// Violations & Modal
function violationsForStudent(student) {
  if (!student) return []
  const sId = String(student.studentId || student.id || '')
  const sCode = String(student.studentCode || '').toLowerCase()

  return liveViolations.value.filter((v) => {
    const vId = String(v.studentId || v.maHocSinh || v.id || '')
    const vCode = String(v.studentCode || '').toLowerCase()
    return (sId && vId && sId === vId) || (sCode && vCode && sCode === vCode)
  })
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

function clearAllAlerts() {
  liveViolations.value.forEach((v) => { v.handled = true })
}

function openStudentModal(student) {
  selectedStudent.value = student
}

function closeStudentModal() {
  selectedStudent.value = null
}

function sendReminder(student) {
  showReminderDialog(student)
}

function showReminderDialog(student) {
  reminderDialog.value = {
    visible: true,
    student,
    message: reminderPresets[0],
  }
}

function sendReminderConfirm() {
  const { student, message } = reminderDialog.value
  if (!message.trim()) return

  if (student?.connectionId && currentSession.value) {
    examProctoringHub.sendWarningToStudent(currentSession.value.id, student.connectionId, message)
  }

  if (!student.logs) student.logs = []
  student.logs.unshift({
    type: 'PROCTOR_MESSAGE',
    message,
    timestamp: new Date().toISOString(),
  })
  popupStore.info('Dã gửi nhắc nhở', `${student.studentCode} · ${message}`)
  reminderDialog.value.visible = false
}

async function suspendStudent(student) {
  const reason = window.prompt(`Lý do đình chỉ thí sinh ${student.studentCode}?`, 'Vi phạm quy chế thi')
  if (!reason) return

  if (currentSession.value) {
    try {
      await examProctoringHub.suspendStudent(
        currentSession.value.id, 
        student.studentId || student.id, 
        student.connectionId || '', 
        reason
      )
    } catch (e) {
      console.error('Error suspending student:', e)
    }
  }

  student.examStatus = 'suspended'
  student.streamStatus = 'stopped'
  if (!student.logs) student.logs = []
  student.logs.unshift({
    type: 'SUSPENDED',
    message: `Đình chỉ: ${reason}`,
    timestamp: new Date().toISOString(),
  })
  popupStore.warning('Đã đình chỉ', `${student.studentCode} đã được chuyển sang trạng thái bị đình chỉ.`)
}

function markStudentHandled(student) {
  violationsForStudent(student).forEach((violation) => {
    violation.handled = true
  })
  if (!student.logs) student.logs = []
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
.alert-strip {
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
.student-code {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.65rem;
  font-weight: 850;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
.student-code {
  color: var(--text-link);
}

.proctor-header h1,
.dashboard-toolbar h2,
.screen-card h3,
.modal-panel h2 {
  margin: 0.1rem 0;
  color: var(--text-heading);
  font-weight: 850;
}
.proctor-header h1, .dashboard-toolbar h2 { font-size: 1.05rem; }
.screen-card h3 { font-size: 0.9rem; margin-top: 0; }
.modal-panel h2 { font-size: 1.05rem; }

.proctor-header p,
.dashboard-toolbar p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.78rem;
  font-weight: 600;
}

.time-chip,
.ghost-action,
.danger-action,
.primary-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 850;
  cursor: pointer;
}

.time-chip {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.65rem 0.8rem;
  cursor: default;
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
.danger-action,
.primary-action {
  min-height: 2.4rem;
  border: 1px solid var(--border-card);
  padding: 0 0.8rem;
}

.ghost-action {
  background: var(--surface-input);
  color: var(--text-label);
}

.danger-action {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
  border-color: transparent;
}
.danger-action:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.primary-action {
  border: 0;
  background: var(--text-link);
  color: var(--text-inverse);
}

.dashboard-toolbar {
  padding: 1rem;
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

.dashboard-counters div,
.modal-facts span {
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.55rem;
}

.dashboard-counters span,
.screen-meta span {
  color: var(--text-muted);
  font-size: 0.68rem;
  font-weight: 800;
  display: block;
}

.dashboard-counters strong,
.screen-meta strong,
.modal-facts strong {
  display: block;
  color: var(--text-heading);
  font-weight: 900;
}

.alert-strip {
  padding: 1rem;
}

.alert-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.alert-item {
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.65rem;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 750;
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

.screen-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}

.screen-card {
  border-radius: 18px;
  padding: 1rem;
  box-shadow: var(--lg-shadow-sm);
  cursor: pointer;
  transition: transform 0.18s ease;
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
  flex-direction: column;
  align-items: flex-start;
  gap: 0.2rem;
}

.screen-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.45rem;
  margin-top: 0.75rem;
  width: 100%;
}

.screen-actions,
.modal-actions {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.4rem;
  margin-top: 0.8rem;
  width: 100%;
}

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
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Modal Styles */
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
  .dashboard-toolbar,
  .alert-strip-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .student-modal,
  .dashboard-toolbar {
    grid-template-columns: 1fr;
  }

  .dashboard-counters,
  .screen-meta {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

/* ── Modal facts rows ─────────────────────────────────────────────────── */
.modal-fact-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid var(--border-default);
  gap: 12px;
}
.modal-fact-row:last-child { border-bottom: none; }
.modal-fact-row > span { color: var(--text-label); font-size: 0.8rem; }
.modal-fact-row > strong { font-size: 0.9rem; display: flex; align-items: center; gap: 4px; }

/* ── Reminder dialog ─────────────────────────────────────────────────── */
.reminder-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 9999;
  width: min(480px, 95vw);
  padding: 28px 28px 24px;
  border-radius: 16px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.reminder-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 1rem;
  font-weight: 700;
  color: var(--text-heading);
  margin: 0;
}

.reminder-student {
  font-size: 0.82rem;
  color: var(--text-label);
  margin: 0;
}

.reminder-presets {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.preset-btn {
  font-size: 0.75rem;
  padding: 5px 10px;
  border-radius: 20px;
  border: 1px solid var(--border-input);
  background: transparent;
  color: var(--text-body);
  cursor: pointer;
  transition: all 0.15s;
  text-align: left;
}
.preset-btn:hover { background: var(--surface-input); }
.preset-btn.active {
  background: var(--lg-primary, #2563eb);
  color: #fff;
  border-color: transparent;
}

.reminder-textarea {
  width: 100%;
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  border-radius: 10px;
  padding: 10px 12px;
  color: var(--text-body);
  font-size: 0.85rem;
  resize: vertical;
  outline: none;
  transition: border 0.15s;
  box-sizing: border-box;
}
.reminder-textarea:focus { border-color: var(--border-input-focus); }

.reminder-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.ghost-btn {
  padding: 8px 18px;
  border-radius: 8px;
  background: transparent;
  border: 1px solid var(--border-default);
  color: var(--text-label);
  cursor: pointer;
  font-size: 0.85rem;
  transition: background 0.15s;
}
.ghost-btn:hover { background: var(--surface-input); }

.send-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 20px;
  border-radius: 8px;
  background: var(--lg-primary, #2563eb);
  color: #fff;
  border: none;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 600;
  transition: opacity 0.15s;
}
.send-btn:hover { opacity: 0.88; }

/* CONTROLS & FILTERS BAR */
.students-controls-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.75rem 1rem;
  border-radius: 16px;
  flex-wrap: wrap;
}

.search-and-filters {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
  flex: 1;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
  min-width: 240px;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  color: var(--text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 0.5rem 0.75rem 0.5rem 2.25rem;
  border-radius: 12px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-heading);
  font-size: 0.82rem;
  outline: none;
  transition: border-color 0.2s;
}

.search-input:focus {
  border-color: var(--text-link);
}

.filter-pills {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  flex-wrap: wrap;
}

.filter-pill {
  padding: 0.35rem 0.75rem;
  border-radius: 10px;
  font-size: 0.75rem;
  font-weight: 600;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.filter-pill:hover {
  color: var(--text-heading);
}

.filter-pill.active {
  background: var(--accent-primary-soft, rgba(37, 99, 235, 0.15));
  border-color: var(--text-link);
  color: var(--text-link);
  font-weight: 700;
}

.view-switcher {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  background: var(--surface-input);
  padding: 0.25rem;
  border-radius: 12px;
  border: 1px solid var(--border-card);
}

.switch-btn {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.75rem;
  border-radius: 9px;
  font-size: 0.75rem;
  font-weight: 700;
  border: none;
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.switch-btn:hover {
  color: var(--text-heading);
}

.switch-btn.active {
  background: var(--surface-card);
  color: var(--text-heading);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
}

/* STUDENTS TABLE */
.students-table-container {
  border-radius: 18px;
  overflow: hidden;
  box-shadow: var(--lg-shadow-sm);
}

.table-responsive {
  width: 100%;
  overflow-x: auto;
}

.students-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 0.82rem;
}

.students-table th {
  padding: 0.85rem 1rem;
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-muted);
  font-size: 0.72rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 1px solid var(--border-card);
}

.student-row {
  border-bottom: 1px solid var(--border-card);
  cursor: pointer;
  transition: background 0.15s ease;
}

.student-row:hover {
  background: rgba(255, 255, 255, 0.04);
}

.student-row.has-alert-row {
  background: rgba(239, 68, 68, 0.06);
}

.student-row.has-alert-row:hover {
  background: rgba(239, 68, 68, 0.1);
}

.students-table td {
  padding: 0.75rem 1rem;
  vertical-align: middle;
}

.student-info-cell {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.student-avatar-chip {
  width: 2.2rem;
  height: 2.2rem;
  border-radius: 10px;
  background: var(--accent-primary-soft, rgba(37, 99, 235, 0.2));
  color: var(--text-link);
  font-weight: 850;
  font-size: 0.85rem;
  display: grid;
  place-items: center;
  flex-shrink: 0;
}

.student-code-text {
  font-size: 0.68rem;
  font-weight: 850;
  color: var(--text-link);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.student-name-text {
  font-weight: 700;
  color: var(--text-heading);
}

.connection-status-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.connection-status-tag .dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
}

.connection-status-tag.online {
  color: #10b981;
}

.connection-status-tag.online .dot {
  background: #10b981;
  box-shadow: 0 0 6px #10b981;
}

.connection-status-tag.offline {
  color: var(--text-muted);
}

.connection-status-tag.offline .dot {
  background: #94a3b8;
}

.stream-status-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.stream-status-tag.active {
  color: #3b82f6;
}

.stream-status-tag.inactive {
  color: var(--text-muted);
}

.row-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.4rem;
}

.btn-action-primary {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.85rem;
  border-radius: 10px;
  background: var(--text-link);
  color: #fff;
  font-size: 0.75rem;
  font-weight: 700;
  border: none;
  cursor: pointer;
  transition: opacity 0.2s, transform 0.1s;
}

.btn-action-primary:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn-action-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: 8px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  cursor: pointer;
  transition: all 0.15s;
}

.btn-action-icon:hover {
  background: rgba(255, 255, 255, 0.1);
  color: var(--text-heading);
}

/* REALTIME VIOLATION MODAL ANIMATION */
.violation-alert-backdrop {
  animation: fadeInBackdrop 0.25s ease-out forwards;
}

.violation-alert-modal {
  animation: zoomInModal 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes fadeInBackdrop {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes zoomInModal {
  from { opacity: 0; transform: scale(0.88); }
  to { opacity: 1; transform: scale(1); }
}
</style>
