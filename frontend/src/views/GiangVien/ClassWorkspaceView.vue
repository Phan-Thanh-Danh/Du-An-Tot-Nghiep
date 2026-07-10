<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  AlertCircle,
  ArrowLeft,
  BarChart3,
  CalendarClock,
  CheckCircle2,
  ClipboardCheck,
  ClipboardList,
  FileText,
  GraduationCap,
  Maximize,
  MessageSquare,
  Minimize,
  Play,
  Settings,
  UserCheck,
  UserX,
  Users,
  Video,
} from 'lucide-vue-next'

import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref('')
const isMicOn = ref(true)
const isCameraOn = ref(false)
const activeTab = ref('content')
const isAttendanceExpanded = ref(false)

const students = ref([])
const modules = ref([])

const classCode = computed(() => route.params.id || '')
const presentCount = computed(() => students.value.filter((student) => student.present).length)
const absentCount = computed(() => students.value.length - presentCount.value)
const completedModules = computed(
  () => modules.value.filter((module) => module.status === 'completed').length,
)
const progressPercent = computed(() =>
  modules.value.length ? Math.round((completedModules.value / modules.value.length) * 100) : 0,
)
const currentModule = computed(
  () => modules.value.find((module) => module.status === 'playing') || modules.value[0] || null,
)

async function loadWorkspace() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getTeacherClassWorkspace(route.params.id)
    students.value = (data?.students || []).map(s => ({
      id: s.maSinhVien ?? s.id,
      name: s.tenSinhVien ?? s.name ?? '',
      present: s.coMat ?? s.present ?? false,
    }))
    modules.value = (data?.modules || []).map(m => ({
      id: m.id,
      title: m.tieuDe ?? m.title ?? '',
      duration: m.thoiLuong ?? m.duration ?? '',
      status: m.trangThai ?? m.status ?? 'locked',
      type: m.loai ?? m.type ?? 'video',
    }))
  } catch (e) {
    error.value = e?.message || 'Không thể tải workspace.'
  } finally {
    loading.value = false
  }
}

const workspaceStats = computed(() => [
  {
    label: 'Sĩ số',
    value: students.value.length,
    hint: `Lớp ${classCode.value}`,
    icon: Users,
    tone: 'primary',
  },
  {
    label: 'Có mặt',
    value: presentCount.value,
    hint: `${absentCount.value} sinh viên vắng`,
    icon: UserCheck,
    tone: 'success',
  },
  {
    label: 'Tiến độ',
    value: `${progressPercent.value}%`,
    hint: `${completedModules.value}/${modules.value.length} nội dung`,
    icon: BarChart3,
    tone: 'violet',
  },
])

const quickActions = [
  {
    label: 'Điểm danh',
    icon: ClipboardCheck,
    action: () => {
      activeTab.value = 'attendance'
    },
  },
  {
    label: 'Nội dung học',
    icon: FileText,
    action: () => {
      activeTab.value = 'content'
    },
  },
  {
    label: 'Cài đặt',
    icon: Settings,
    action: () => {},
  },
]

function moduleVariant(status) {
  if (status === 'completed') return 'success'
  if (status === 'playing') return 'primary'
  return 'neutral'
}

function moduleLabel(status) {
  if (status === 'completed') return 'Hoàn thành'
  if (status === 'playing') return 'Đang học'
  return 'Chưa mở'
}

onMounted(() => { loadWorkspace() })
</script>

<template>
  <div v-if="loading">
    <SkeletonDashboard :cards="4" :rows="4" class="p-4" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton variant="secondary" @click="loadWorkspace">Thử lại</GlassButton>
  </div>
  <div v-else class="teacher-workspace lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="workspace-header">
      <div class="header-main">
        <button
          type="button"
          class="back-button"
          aria-label="Quay lại chi tiết lớp"
          @click="router.push(`/teacher/classes/${route.params.id}/details`)"
        >
          <ArrowLeft :size="18" />
        </button>

        <div class="header-copy">
          <div class="header-kicker">
            <GraduationCap :size="15" />
            Workspace lớp học
          </div>
          <div>
            <h1>Lập trình Java - {{ classCode }}</h1>
            <p>Java OOP · Spring 2026 · Block 2</p>
          </div>
        </div>

        <div class="header-status">
          <GlassBadge variant="success">Đang diễn ra</GlassBadge>
          <span class="session-time">
            <CalendarClock :size="14" />
            Hôm nay · 07:30-09:30
          </span>
        </div>
      </div>

      <div class="quick-actions" aria-label="Thao tác nhanh">
        <GlassButton
          v-for="action in quickActions"
          :key="action.label"
          variant="secondary"
          size="sm"
          @click="action.action"
        >
          <template #leading>
            <component :is="action.icon" :size="15" />
          </template>
          {{ action.label }}
        </GlassButton>
      </div>
    </GlassPanel>

    <div class="workspace-grid" :class="{ 'attendance-expanded': isAttendanceExpanded }">
      <main class="workspace-main">
        <GlassPanel
          v-show="!isAttendanceExpanded"
          variant="flat"
          density="compact"
          class="session-panel"
        >
          <div class="section-title-row">
            <div>
              <p class="section-eyebrow">Buổi học hiện tại</p>
              <h2>Lớp học trực tuyến</h2>
            </div>
            <GlassBadge variant="info">Google Meet</GlassBadge>
          </div>

          <div class="meet-strip">
            <div class="meet-icon" aria-hidden="true">
              <Video :size="24" />
            </div>
            <div class="meet-copy">
              <p class="meet-title">{{ currentModule?.title ?? 'Chưa có nội dung' }}</p>
              <p class="meet-meta">
                {{ currentModule?.duration ?? '' }} · Mic {{ isMicOn ? 'bật' : 'tắt' }} · Camera
                {{ isCameraOn ? 'bật' : 'tắt' }}
              </p>
            </div>
            <a
              href="https://meet.google.com/new"
              target="_blank"
              rel="noopener noreferrer"
              class="meet-link"
            >
              <Video :size="16" />
              Tham gia
            </a>
          </div>
        </GlassPanel>

        <GlassPanel variant="flat" density="compact" class="content-panel">
          <div class="tab-bar">
            <button
              type="button"
              :class="['tab-button', activeTab === 'content' ? 'active' : '']"
              @click="activeTab = 'content'"
            >
              <FileText :size="16" />
              Nội dung
            </button>
            <button
              type="button"
              :class="['tab-button', activeTab === 'attendance' ? 'active' : '']"
              @click="activeTab = 'attendance'"
            >
              <ClipboardCheck :size="16" />
              Điểm danh
            </button>
          </div>

          <div v-if="activeTab === 'content'" class="content-stack">
            <div class="progress-card">
              <div>
                <p class="progress-label">Tiến độ nội dung</p>
                <p class="progress-value">
                  Hoàn thành {{ completedModules }}/{{ modules.length }} mục
                </p>
              </div>
              <div class="progress-track" aria-hidden="true">
                <span :style="{ width: `${progressPercent}%` }" />
              </div>
            </div>

            <div class="module-list">
              <article
                v-for="mod in modules"
                :key="mod.id"
                :class="['module-row', `module-${mod.status}`]"
              >
                <div class="module-status">
                  <CheckCircle2 v-if="mod.status === 'completed'" :size="15" />
                  <Play v-else-if="mod.status === 'playing'" :size="14" />
                  <ClipboardList v-else :size="14" />
                </div>
                <div class="module-copy">
                  <div class="module-title-row">
                    <h3>{{ mod.title }}</h3>
                    <GlassBadge :variant="moduleVariant(mod.status)">
                      {{ moduleLabel(mod.status) }}
                    </GlassBadge>
                  </div>
                  <p>{{ mod.duration }} · {{ mod.type === 'video' ? 'Video bài giảng' : 'Bài tập' }}</p>
                </div>
              </article>
            </div>
          </div>

          <div v-if="activeTab === 'attendance'" class="attendance-stack">
            <div class="attendance-toolbar">
              <div class="attendance-summary">
                <span class="mini-stat success">Có mặt: {{ presentCount }}</span>
                <span class="mini-stat danger">Vắng: {{ absentCount }}</span>
              </div>
              <button
                type="button"
                class="icon-button"
                :title="isAttendanceExpanded ? 'Thu nhỏ' : 'Phóng to thành trang riêng'"
                @click="isAttendanceExpanded = !isAttendanceExpanded"
              >
                <Minimize v-if="isAttendanceExpanded" :size="16" />
                <Maximize v-else :size="16" />
              </button>
            </div>

            <TableShell density="compact">
              <table>
                <thead>
                  <tr>
                    <th>Sinh viên</th>
                    <th>Trạng thái</th>
                    <th class="text-right">Cập nhật</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sv in students" :key="sv.id">
                    <td>
                      <div class="student-cell">
                        <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                        <span>
                          <strong>{{ sv.name }}</strong>
                          <small>{{ sv.id }}</small>
                        </span>
                      </div>
                    </td>
                    <td>
                      <GlassBadge :variant="sv.present ? 'success' : 'danger'">
                        {{ sv.present ? 'Có mặt' : 'Vắng mặt' }}
                      </GlassBadge>
                    </td>
                    <td>
                      <div class="attendance-actions">
                        <GlassButton
                          :variant="sv.present ? 'success' : 'ghost'"
                          size="sm"
                          @click="sv.present = true"
                        >
                          <template #leading>
                            <UserCheck :size="14" />
                          </template>
                          Có mặt
                        </GlassButton>
                        <GlassButton
                          :variant="!sv.present ? 'danger' : 'ghost'"
                          size="sm"
                          @click="sv.present = false"
                        >
                          <template #leading>
                            <UserX :size="14" />
                          </template>
                          Vắng
                        </GlassButton>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </TableShell>

            <div class="attendance-footer">
              <GlassButton variant="success" size="md">
                <template #leading>
                  <UserCheck :size="17" />
                </template>
                Lưu điểm danh
              </GlassButton>
            </div>
          </div>
        </GlassPanel>
      </main>

      <aside v-if="!isAttendanceExpanded" class="workspace-aside">
        <div class="stat-grid">
          <GlassPanel
            v-for="stat in workspaceStats"
            :key="stat.label"
            variant="flat"
            density="compact"
            class="stat-card"
          >
            <div :class="['stat-icon', `tone-${stat.tone}`]">
              <component :is="stat.icon" :size="17" />
            </div>
            <div>
              <p>{{ stat.label }}</p>
              <strong>{{ stat.value }}</strong>
              <span>{{ stat.hint }}</span>
            </div>
          </GlassPanel>
        </div>

        <GlassPanel variant="flat" density="compact" class="aside-panel">
          <div class="section-title-row">
            <div>
              <p class="section-eyebrow">Theo dõi nhanh</p>
              <h2>Cảnh báo lớp</h2>
            </div>
            <GlassBadge variant="warning">2 mục</GlassBadge>
          </div>
          <div class="notice-list">
            <div class="notice-item">
              <span class="notice-dot warning" />
              <p>2 sinh viên chưa điểm danh buổi hôm nay.</p>
            </div>
            <div class="notice-item">
              <span class="notice-dot info" />
              <p>Bài tập OOP sẽ mở sau khi hoàn tất chương 2.</p>
            </div>
          </div>
        </GlassPanel>

        <GlassPanel variant="flat" density="compact" class="aside-panel">
          <div class="section-title-row">
            <div>
              <p class="section-eyebrow">Trao đổi</p>
              <h2>Thảo luận lớp</h2>
            </div>
            <MessageSquare :size="17" class="muted-icon" />
          </div>
          <div class="chat-input">
            <MessageSquare :size="16" />
            <span>Thảo luận lớp học...</span>
          </div>
        </GlassPanel>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.teacher-workspace {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  color: var(--text-body);
}

.workspace-header {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}

.header-main {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.back-button,
.icon-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: none;
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    color 0.2s ease,
    background 0.2s ease;
}

.back-button {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
}

.icon-button {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-sm);
}

.back-button:hover,
.icon-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.header-copy {
  display: flex;
  gap: 0.75rem;
  flex: 1;
  min-width: 0;
}

.header-kicker {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  height: 1.75rem;
  padding: 0 0.6rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.72rem;
  font-weight: 800;
  white-space: nowrap;
}

.header-copy h1 {
  margin: 0.3rem 0 0.15rem;
  color: var(--text-heading);
  font-size: clamp(1.25rem, 2vw, 1.65rem);
  line-height: 1.15;
  font-weight: 900;
  letter-spacing: 0;
}

.header-copy p,
.session-time,
.meet-meta,
.module-copy p,
.stat-card span,
.notice-item p,
.chat-input span {
  color: var(--text-muted);
}

.header-status {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.session-time {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.78rem;
  font-weight: 700;
}

.quick-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.workspace-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(18rem, 22rem);
  gap: 0.875rem;
  align-items: start;
}

.workspace-grid.attendance-expanded {
  grid-template-columns: minmax(0, 1fr);
}

.workspace-main,
.workspace-aside {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  min-width: 0;
}

.section-title-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 0.75rem;
}

.section-eyebrow,
.progress-label,
.stat-card p {
  margin: 0;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.section-title-row h2 {
  margin: 0.15rem 0 0;
  color: var(--text-heading);
  font-size: 1rem;
  line-height: 1.2;
  font-weight: 900;
}

.meet-strip {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.75rem;
}

.meet-icon,
.module-status,
.stat-icon,
.student-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: none;
}

.meet-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: var(--radius-lg);
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.meet-copy {
  min-width: 0;
  flex: 1;
}

.meet-title {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.95rem;
  font-weight: 850;
}

.meet-meta {
  margin: 0.15rem 0 0;
  font-size: 0.8rem;
}

.meet-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  min-height: 2.25rem;
  padding: 0 0.85rem;
  border-radius: var(--radius-md);
  background: var(--accent-primary);
  color: var(--text-inverse);
  font-size: 0.8rem;
  font-weight: 850;
  text-decoration: none;
  white-space: nowrap;
}

.tab-bar {
  display: flex;
  gap: 0.35rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.45rem;
}

.tab-button {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  min-height: 2.15rem;
  padding: 0 0.8rem;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  background: transparent;
  color: var(--text-label);
  font-size: 0.82rem;
  font-weight: 800;
  transition:
    background 0.2s ease,
    border-color 0.2s ease,
    color 0.2s ease;
}

.tab-button:hover,
.tab-button.active {
  border-color: var(--border-card);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.content-stack,
.attendance-stack,
.module-list,
.notice-list {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.content-stack,
.attendance-stack {
  padding-top: 0.75rem;
}

.progress-card {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(8rem, 16rem);
  gap: 0.75rem;
  align-items: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.75rem;
}

.progress-value {
  margin: 0.15rem 0 0;
  color: var(--text-heading);
  font-weight: 850;
}

.progress-track {
  height: 0.5rem;
  border-radius: 999px;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  overflow: hidden;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
}

.module-row {
  display: flex;
  gap: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.8rem;
}

.module-playing {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
}

.module-status {
  width: 1.9rem;
  height: 1.9rem;
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
}

.module-completed .module-status {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.module-locked .module-status {
  color: var(--text-placeholder);
}

.module-copy {
  flex: 1;
  min-width: 0;
}

.module-title-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
}

.module-copy h3 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9rem;
  line-height: 1.35;
  font-weight: 850;
}

.module-copy p {
  margin: 0.25rem 0 0;
  font-size: 0.78rem;
}

.attendance-toolbar,
.attendance-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.attendance-summary {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.mini-stat {
  display: inline-flex;
  min-height: 1.75rem;
  align-items: center;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  padding: 0 0.65rem;
  font-size: 0.76rem;
  font-weight: 850;
}

.mini-stat.success {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.mini-stat.danger {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.student-cell {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  min-width: 12rem;
}

.student-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.85rem;
}

.student-cell small {
  display: block;
  color: var(--text-placeholder);
  font-size: 0.72rem;
  font-weight: 750;
}

.attendance-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.4rem;
}

.attendance-footer {
  justify-content: flex-end;
  border-top: 1px solid var(--border-card);
  padding-top: 0.75rem;
}

.stat-grid {
  display: grid;
  gap: 0.65rem;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.stat-icon {
  width: 2.1rem;
  height: 2.1rem;
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-link);
}

.tone-success {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.tone-violet {
  background: var(--accent-violet-soft);
  color: var(--accent-violet);
}

.stat-card strong {
  display: block;
  margin-top: 0.1rem;
  color: var(--text-heading);
  font-size: 1.08rem;
  font-weight: 900;
}

.stat-card span {
  display: block;
  margin-top: 0.05rem;
  font-size: 0.75rem;
  font-weight: 650;
}

.aside-panel {
  min-width: 0;
}

.notice-item {
  display: flex;
  gap: 0.6rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.65rem;
}

.notice-item p {
  margin: 0;
  font-size: 0.8rem;
  line-height: 1.45;
}

.notice-dot {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 999px;
  margin-top: 0.35rem;
  flex: none;
}

.notice-dot.warning {
  background: var(--color-warning-text);
}

.notice-dot.info {
  background: var(--text-link);
}

.muted-icon {
  color: var(--text-placeholder);
}

.chat-input {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  min-height: 2.5rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.75rem;
  cursor: pointer;
}

.chat-input svg {
  color: var(--text-placeholder);
}

@media (max-width: 1024px) {
  .workspace-grid {
    grid-template-columns: 1fr;
  }

  .workspace-aside {
    display: grid;
    grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  }

  .stat-grid {
    grid-column: 1 / -1;
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .header-main,
  .header-copy,
  .meet-strip,
  .progress-card,
  .module-title-row,
  .attendance-toolbar {
    align-items: stretch;
    flex-direction: column;
  }

  .header-main {
    position: relative;
  }

  .back-button {
    position: absolute;
    top: 0;
    right: 0;
  }

  .header-copy {
    padding-right: 2.75rem;
  }

  .header-status,
  .quick-actions,
  .attendance-actions {
    justify-content: flex-start;
  }

  .meet-link {
    width: 100%;
  }

  .workspace-aside,
  .stat-grid {
    grid-template-columns: 1fr;
  }
}
</style>
