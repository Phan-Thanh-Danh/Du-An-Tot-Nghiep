<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Search,
  Users,
  XCircle,
  Download,
  Calendar,
  ArrowLeft,
  AlertCircle,
  UserCheck,
  CheckCircle2,
  Clock,
} from 'lucide-vue-next'
import { teacherApi } from '@/services/teacherApi'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'

const loading = ref(false)
const error = ref('')

const attendanceData = ref([])
const route = useRoute()
const router = useRouter()

const myClasses = ref([])
const selectedClassId = ref('')

const apiTotalSessions = ref(0)
const totalSessions = computed(() => apiTotalSessions.value > 0 ? apiTotalSessions.value : attendanceData.value.length ? Math.max(...attendanceData.value.map(s => s.present + s.absent)) : 0)
const avgAttendance = computed(() => {
  if (attendanceData.value.length === 0) return 0
  const total = attendanceData.value.reduce((s, st) => s + st.percent, 0)
  return Math.round(total / attendanceData.value.length)
})

const summaryStats = computed(() => {
  const absences = attendanceData.value.reduce((sum, student) => sum + student.absent, 0)
  const late = 0
  const excused = 0
  const risk = attendanceData.value.filter((student) => student.status === 'danger').length

  return [
    { label: 'Sĩ số', value: attendanceData.value.length, tone: 'primary' },
    { label: 'Tỷ lệ CC', value: `${avgAttendance.value}%`, tone: 'success' },
    { label: 'Lượt vắng', value: absences, tone: 'danger' },
    { label: 'Đi muộn', value: late, tone: 'warning' },
    { label: 'Có phép', value: excused, tone: 'info' },
    { label: 'Nguy cơ', value: risk, tone: 'danger' },
  ]
})

async function loadAttendance() {
  loading.value = true
  error.value = ''
  try {
    let classId = route.query.classId

    // Lấy danh sách lớp
    if (myClasses.value.length === 0) {
      const classesRes = await teacherApi.getTeacherClasses()
      const classesData = classesRes?.data?.data ?? classesRes?.data ?? classesRes?.Data ?? classesRes
      if (classesData && Array.isArray(classesData)) {
        myClasses.value = classesData
      }
    }

    if (!classId) {
      return // Dừng lại ở đây để hiển thị danh sách dạng Card
    }

    selectedClassId.value = classId

    const res = await teacherApi.getTeacherClassAttendance(classId)
    const data = res?.data?.data ?? res?.data ?? res?.Data ?? res
    
    if (data && data.students) {
      attendanceData.value = data.students
      apiTotalSessions.value = data.totalSessions ?? 0
    } else {
      throw new Error('Không có dữ liệu điểm danh')
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải điểm danh.'
    attendanceData.value = []
  } finally {
    loading.value = false
  }
}

const getStatusText = (status) => {
  const texts = {
    excellent: 'Xuất sắc',
    good: 'Ổn định',
    warning: 'Cần chú ý',
    danger: 'Nguy cơ'
  }
  return texts[status] || 'Ổn định'
}

const getStatusVariant = (status) => {
  const variants = {
    excellent: 'success',
    good: 'success',
    warning: 'warning',
    danger: 'danger'
  }
  return variants[status] || 'success'
}

const animateProgress = ref(false)

const onClassChange = () => {
  if (selectedClassId.value) {
    router.push({ query: { ...route.query, classId: selectedClassId.value } })
  }
}

watch(() => route.query.classId, (newId) => {
  if (!newId) {
    selectedClassId.value = ''
    attendanceData.value = []
    return
  }
  if (newId !== selectedClassId.value) {
    selectedClassId.value = newId
    loadAttendance()
  } else {
    loadAttendance()
  }
})

onMounted(() => {
  loadAttendance()
  setTimeout(() => { animateProgress.value = true }, 100)
})
</script>

<template>
  <div class="class-attendance-page lg-page-enter">
    <div v-if="loading">
      <GlassPanel variant="flat" density="compact" class="loading-panel">
        <p>Đang tải dữ liệu điểm danh...</p>
      </GlassPanel>
    </div>

    <div v-else-if="error">
      <GlassPanel variant="flat" density="compact" class="error-panel">
        <p>{{ error }}</p>
      </GlassPanel>
    </div>

    <!-- HIỂN THỊ DẠNG CARD KHI CHƯA CHỌN LỚP -->
    <template v-else-if="!route.query.classId">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-xl font-bold text-heading tracking-tight">Danh sách lớp học</h1>
          <p class="text-muted mt-1">Chọn lớp học để theo dõi chi tiết điểm danh và chuyên cần.</p>
        </div>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <TeacherClassCard
          v-for="cls in myClasses"
          :key="cls.classId || cls.id"
          :title="cls.className"
          subtitle="Lớp học phần"
        >
          <template #action>
            <router-link
              :to="{ query: { classId: cls.classId || cls.id } }"
              class="w-full flex justify-center items-center gap-2 group-hover:bg-(--accent-primary) group-hover:text-inverse transition-all bg-slate-100 px-4 py-2 rounded-xl text-xs font-bold"
            >
              Xem điểm danh
              <CheckCircle2 :size="14" />
            </router-link>
          </template>
        </TeacherClassCard>
        
        <div v-if="myClasses.length === 0" class="col-span-full py-12 text-center text-muted">
          Bạn chưa được phân công giảng dạy lớp nào.
        </div>
      </div>
    </template>

    <!-- HIỂN THỊ DẠNG BẢNG CHI TIẾT KHI ĐÃ CHỌN LỚP -->
    <template v-else>
      <GlassPanel variant="flat" density="compact" class="page-header">
        <div class="header-main">
          <router-link to="/teacher/class-attendance" class="back-link" aria-label="Quay lại danh sách lớp">
            <ArrowLeft :size="18" />
          </router-link>

          <div class="header-copy">
            <div class="context-tags">
              <GlassBadge variant="primary" v-if="selectedClassId">{{ myClasses.find(c => c.classId == selectedClassId)?.className || 'Lớp' }}</GlassBadge>
              <GlassBadge variant="neutral">HK hiện tại</GlassBadge>
              <GlassBadge variant="info">Sĩ số {{ attendanceData.length }}</GlassBadge>
            </div>
            <h1>Điểm danh theo lớp</h1>
            <p>Theo dõi chuyên cần, số buổi vắng và nguy cơ vượt quỹ vắng của sinh viên.</p>
          </div>
        </div>

        <div class="header-actions">
          <GlassButton variant="secondary" size="sm">
            <template #leading>
              <Calendar :size="16" />
            </template>
            Xem lịch sử
          </GlassButton>
          <GlassButton variant="primary" size="sm">
            <template #leading>
              <Download :size="16" />
            </template>
            Xuất báo cáo
          </GlassButton>
        </div>
      </GlassPanel>

      <GlassPanel variant="flat" density="compact" class="context-panel">
        <div class="summary-strip">
          <div v-for="item in summaryStats" :key="item.label" :class="['summary-pill', item.tone]">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </div>

        <div class="filters">
          <label class="select-shell">
            <Users :size="16" />
            <select v-model="selectedClassId" @change="onClassChange">
              <option v-for="cls in myClasses" :key="cls.classId || cls.id" :value="cls.classId || cls.id">
                Lớp {{ cls.className }}
              </option>
            </select>
          </label>

          <label class="input-shell">
            <Search :size="16" />
            <input type="text" placeholder="Tìm sinh viên, mã SV..." />
          </label>
        </div>
      </GlassPanel>

      <GlassPanel variant="flat" density="compact" class="table-panel">
        <div class="panel-title">
          <div>
            <h2>
              <Users :size="17" />
              Chi tiết chuyên cần
            </h2>
            <p>Danh sách theo dõi điểm danh từng sinh viên trong lớp {{ myClasses.find(c => c.classId == selectedClassId)?.className || '' }}.</p>
          </div>
          <GlassBadge variant="success">{{ totalSessions }} buổi học</GlassBadge>
        </div>

        <TableShell density="compact">
          <table>
            <thead>
              <tr>
                <th>Sinh viên</th>
                <th>MSSV</th>
                <th>Có mặt</th>
                <th>Vắng</th>
                <th>Đi muộn</th>
                <th>Có phép</th>
                <th>Tỷ lệ chuyên cần</th>
                <th>Trạng thái</th>
                <th class="text-right">Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sv in attendanceData" :key="sv.id">
                <td>
                  <div class="student-cell">
                    <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                    <span>
                      <strong>{{ sv.name }}</strong>
                      <small>Lập trình Java</small>
                    </span>
                  </div>
                </td>
                <td class="student-code">{{ sv.id }}</td>
                <td>
                  <span class="metric-cell success">
                    <CheckCircle2 :size="14" />
                    {{ sv.present }}/{{ totalSessions }}
                  </span>
                </td>
                <td>
                  <span :class="['metric-cell', sv.absent > 3 ? 'danger' : 'neutral']">
                    <XCircle :size="14" />
                    {{ sv.absent }}
                  </span>
                </td>
                <td>
                  <span class="metric-cell warning">
                    <Clock :size="14" />
                    0
                  </span>
                </td>
                <td>
                  <span class="metric-cell info">
                    <UserCheck :size="14" />
                    0
                  </span>
                </td>
                <td>
                  <div class="progress-cell">
                    <div class="progress-track" aria-hidden="true">
                      <span :style="{ width: animateProgress ? `${sv.percent}%` : '0%' }" />
                    </div>
                    <strong>{{ sv.percent }}%</strong>
                  </div>
                </td>
                <td>
                  <GlassBadge :variant="getStatusVariant(sv.status)">
                    <AlertCircle v-if="sv.status === 'danger' || sv.status === 'warning'" :size="11" />
                    <CheckCircle2 v-else :size="11" />
                    {{ getStatusText(sv.status) }}
                  </GlassBadge>
                </td>
                <td>
                  <div class="row-actions">
                    <GlassButton variant="ghost" size="sm">Chi tiết</GlassButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>

        <div class="table-footer">
          <span>Hiển thị 1-{{ attendanceData.length }} trong số 42 sinh viên</span>
          <div class="pagination">
            <button type="button">Trước</button>
            <button type="button" class="active">1</button>
            <button type="button">2</button>
            <button type="button">Sau</button>
          </div>
        </div>
      </GlassPanel>
    </template>
  </div>
</template>

<style scoped>
.class-attendance-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.loading-panel,
.error-panel {
  padding: 2rem;
  text-align: center;
}

.page-header,
.header-main,
.header-actions,
.context-panel,
.summary-strip,
.filters,
.panel-title,
.student-cell,
.metric-cell,
.progress-cell,
.row-actions,
.table-footer,
.pagination {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.panel-title,
.table-footer {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  align-items: flex-start;
  gap: 0.75rem;
  min-width: 0;
}

.back-link {
  display: inline-flex;
  width: 2.25rem;
  height: 2.25rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.back-link:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.header-copy {
  min-width: 0;
}

.context-tags,
.header-actions,
.summary-strip,
.filters,
.pagination {
  gap: 0.5rem;
  flex-wrap: wrap;
}

.context-tags {
  margin-bottom: 0.45rem;
}

.header-copy h1,
.panel-title h2 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

.header-copy h1 {
  font-size: 1.45rem;
  line-height: 1.15;
}

.panel-title h2 {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 1rem;
}

.header-copy p,
.panel-title p,
.summary-pill span,
.student-cell small,
.student-code,
.table-footer {
  color: var(--text-muted);
}

.header-copy p,
.panel-title p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.context-panel {
  align-items: center;
}

.summary-strip {
  justify-content: flex-start;
}

.summary-pill {
  display: grid;
  min-width: 5rem;
  gap: 0.05rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.primary,
.summary-pill.info {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.input-shell,
.select-shell {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.7rem;
  color: var(--text-placeholder);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.input-shell {
  width: min(20rem, 100%);
}

.select-shell {
  width: min(13rem, 100%);
}

.input-shell:focus-within,
.select-shell:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.input-shell input,
.select-shell select {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.select-shell select {
  appearance: none;
  cursor: pointer;
}

.input-shell input::placeholder {
  color: var(--text-placeholder);
}

.table-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.panel-title {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.student-cell {
  min-width: 13rem;
  gap: 0.65rem;
}

.student-avatar {
  display: inline-flex;
  width: 2rem;
  height: 2rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
  font-weight: 850;
}

.student-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.student-code {
  font-size: 0.8rem;
  font-weight: 750;
}

.metric-cell {
  gap: 0.35rem;
  color: var(--text-heading);
  font-size: 0.8rem;
  font-weight: 850;
  white-space: nowrap;
}

.metric-cell.success {
  color: var(--color-success-text);
}

.metric-cell.danger {
  color: var(--color-danger-text);
}

.metric-cell.warning {
  color: var(--color-warning-text);
}

.metric-cell.info {
  color: var(--text-link);
}

.progress-cell {
  min-width: 9rem;
  gap: 0.55rem;
}

.progress-track {
  width: 6rem;
  height: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  overflow: hidden;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
  transition: width 0.8s ease;
}

.progress-cell strong {
  color: var(--text-heading);
  font-size: 0.78rem;
  font-weight: 900;
}

.row-actions {
  justify-content: flex-end;
}

.table-footer {
  align-items: center;
  border-top: 1px solid var(--border-card);
  padding-top: 0.75rem;
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.pagination button {
  min-height: 2rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0 0.75rem;
  font-size: 0.76rem;
  font-weight: 850;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.pagination button:hover,
.pagination button.active {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

@media (max-width: 1024px) {
  .page-header,
  .context-panel,
  .panel-title,
  .table-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .filters,
  .header-actions,
  .row-actions {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .summary-strip,
  .filters,
  .pagination {
    display: grid;
    grid-template-columns: 1fr;
  }

  .summary-pill,
  .input-shell,
  .select-shell {
    width: 100%;
  }
}
</style>
