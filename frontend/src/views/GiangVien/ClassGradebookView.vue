<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  ArrowUpDown,
  Award,
  CheckCircle2,
  Download,
  Filter,
  Printer,
  Search,
  TrendingUp,
  Users,
  XCircle,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { teacherApi } from '@/services/teacherApi'

const classId = 1 // Hardcoded for demo/smoke
const gradebook = ref([])
const loading = ref(false)

const avgGPA = computed(() => {
  if (!gradebook.value.length) return 0
  const sum = gradebook.value.reduce((acc, sv) => acc + Number(sv.total || 0), 0)
  return (sum / gradebook.value.length).toFixed(2)
})
const passRate = computed(() => {
  if (!gradebook.value.length) return 0
  const passed = gradebook.value.filter((sv) => sv.status === 'Pass').length
  return Math.round((passed / gradebook.value.length) * 100)
})

const summaryStats = computed(() => {
  const passed = gradebook.value.filter((student) => student.status === 'Pass').length
  const failed = gradebook.value.filter((student) => student.status === 'Fail').length
  // Assuming 3 credits for the course for demo
  const totalCredits = gradebook.value.length * 3

  return [
    { label: 'Sinh viên', value: gradebook.value.length, tone: 'primary' },
    { label: 'GPA TB', value: avgGPA.value, tone: 'neutral' },
    { label: 'Đạt', value: passed, tone: 'success' },
    { label: 'Rớt', value: failed, tone: 'danger' },
    { label: 'Tín chỉ', value: totalCredits, tone: 'info' },
    { label: 'Hoàn thành', value: `${passRate.value}%`, tone: 'success' },
  ]
})

function statusVariant(status) {
  return status === 'Pass' ? 'success' : 'danger'
}

function statusLabel(status) {
  return status === 'Pass' ? 'Đạt' : 'Rớt'
}

async function loadGrades() {
  loading.value = true
  try {
    const res = await teacherApi.getTeacherClassGrades(classId)
    const items = res?.data || res?.Data || res || []
    gradebook.value = (Array.isArray(items) ? items : []).map(sv => ({
      id: sv.id,
      name: sv.name,
      gpa: Number(sv.total || 0).toFixed(1),
      status: Number(sv.total || 0) >= 5 ? 'Pass' : 'Fail',
      credits: 3 // fixed for demo
    }))
  } catch (error) {
    console.error("Lỗi khi tải bảng điểm:", error)
  } finally {
    loading.value = false
  }
}

onMounted(loadGrades)
</script>

<template>
  <div class="gradebook-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <div class="eyebrow">
          <Users :size="15" />
          SE1601 · Lập trình Java
        </div>
        <div>
          <h1>Sổ điểm lớp</h1>
          <p>Tổng hợp kết quả học tập và trạng thái hoàn thành môn học của lớp SE1601.</p>
        </div>
      </div>

      <div class="header-actions">
        <GlassButton variant="secondary" size="sm">
          <template #leading>
            <Printer :size="16" />
          </template>
          In bảng điểm
        </GlassButton>
        <GlassButton variant="primary" size="sm">
          <template #leading>
            <Download :size="16" />
          </template>
          Xuất bảng điểm
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="metric-strip">
        <div class="metric-card">
          <span class="metric-icon">
            <TrendingUp :size="17" />
          </span>
          <div>
            <p>GPA trung bình</p>
            <strong>{{ avgGPA }}<small>/10.0</small></strong>
            <div class="progress-track" aria-hidden="true">
              <span :style="{ width: `${avgGPA * 10}%` }" />
            </div>
          </div>
        </div>

        <div class="metric-card">
          <span class="metric-icon success">
            <CheckCircle2 :size="17" />
          </span>
          <div>
            <p>Tỷ lệ đạt</p>
            <strong>{{ passRate }}<small>% hoàn thành</small></strong>
            <div class="progress-track" aria-hidden="true">
              <span :style="{ width: `${passRate}%` }" />
            </div>
          </div>
        </div>
      </div>

      <div class="summary-strip">
        <div v-for="item in summaryStats" :key="item.label" :class="['summary-pill', item.tone]">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="table-panel">
      <div class="table-toolbar">
        <div>
          <h2>Bảng kết quả chi tiết</h2>
          <p>{{ gradebook.length }} sinh viên · Học kỳ Spring 2026 · Block 2</p>
        </div>
        <div class="filters">
          <label class="search-field">
            <Search :size="16" />
            <input type="text" placeholder="Tìm sinh viên..." />
          </label>
          <GlassButton variant="secondary" size="sm">
            <template #leading>
              <Filter :size="15" />
            </template>
            Trạng thái
          </GlassButton>
        </div>
      </div>

      <TableShell density="compact">
        <table>
          <thead>
            <tr>
              <th>Sinh viên</th>
              <th>MSSV</th>
              <th>Lớp</th>
              <th>Tín chỉ</th>
              <th>
                <span class="sortable-label">
                  GPA
                  <ArrowUpDown :size="12" />
                </span>
              </th>
              <th>Trạng thái</th>
              <th class="text-right">Ghi chú</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sv in gradebook" :key="sv.id">
              <td>
                <div class="student-cell">
                  <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                  <span>
                    <strong>{{ sv.name }}</strong>
                    <small>Java OOP</small>
                  </span>
                </div>
              </td>
              <td class="student-code">{{ sv.id }}</td>
              <td>
                <GlassBadge variant="primary">SE1601</GlassBadge>
              </td>
              <td class="credits-cell">{{ sv.credits }}</td>
              <td>
                <span class="gpa-cell">
                  <Award :size="15" />
                  <strong :class="Number(sv.gpa) < 5 ? 'failed' : 'passed'">{{ sv.gpa }}</strong>
                </span>
              </td>
              <td>
                <GlassBadge :variant="statusVariant(sv.status)">
                  <CheckCircle2 v-if="sv.status === 'Pass'" :size="11" />
                  <XCircle v-else :size="11" />
                  {{ statusLabel(sv.status) }}
                </GlassBadge>
              </td>
              <td class="note-cell">
                {{ sv.status === 'Pass' ? 'Đủ điều kiện hoàn thành' : 'Cần theo dõi học lại' }}
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>
  </div>
</template>

<style scoped>
.gradebook-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.page-header,
.context-panel,
.table-toolbar,
.header-actions,
.filters,
.summary-strip,
.metric-strip,
.sortable-label,
.student-cell,
.gpa-cell {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.table-toolbar {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-copy {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.5rem;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.header-copy h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  line-height: 1.15;
  font-weight: 900;
}

.header-copy p,
.table-toolbar p,
.metric-card p,
.summary-pill span,
.student-cell small,
.note-cell,
.student-code {
  color: var(--text-muted);
}

.header-copy p,
.table-toolbar p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-actions,
.filters,
.summary-strip,
.metric-strip {
  gap: 0.55rem;
  flex-wrap: wrap;
}

.context-panel {
  align-items: stretch;
}

.metric-strip {
  flex: 1;
}

.metric-card {
  display: flex;
  min-width: min(17rem, 100%);
  flex: 1;
  gap: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.75rem;
}

.metric-icon {
  display: inline-flex;
  width: 2.1rem;
  height: 2.1rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.metric-icon.success {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.metric-card div {
  min-width: 0;
  flex: 1;
}

.metric-card p {
  margin: 0;
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.metric-card strong {
  display: block;
  margin-top: 0.15rem;
  color: var(--text-heading);
  font-size: 1.2rem;
  font-weight: 900;
}

.metric-card small {
  margin-left: 0.25rem;
  color: var(--text-muted);
  font-size: 0.72rem;
  font-weight: 750;
}

.progress-track {
  width: 100%;
  height: 0.45rem;
  margin-top: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-card);
  overflow: hidden;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
}

.summary-strip {
  max-width: 28rem;
  justify-content: flex-end;
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

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.table-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.table-toolbar {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.table-toolbar h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.search-field {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  width: min(18rem, 100%);
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

.search-field:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-field input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.sortable-label {
  gap: 0.35rem;
  color: var(--text-link);
  white-space: nowrap;
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
}

.student-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.student-code,
.credits-cell,
.note-cell {
  font-size: 0.8rem;
  font-weight: 750;
}

.credits-cell {
  color: var(--text-heading);
}

.gpa-cell {
  min-width: 4.5rem;
  gap: 0.4rem;
  color: var(--text-link);
}

.gpa-cell strong {
  font-size: 0.95rem;
  font-weight: 900;
}

.gpa-cell strong.passed {
  color: var(--text-heading);
}

.gpa-cell strong.failed {
  color: var(--color-danger-text);
}

.note-cell {
  text-align: right;
  white-space: nowrap;
}

@media (max-width: 1024px) {
  .page-header,
  .context-panel,
  .table-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip {
    max-width: none;
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .header-actions,
  .filters,
  .summary-strip,
  .metric-strip {
    display: grid;
    grid-template-columns: 1fr;
  }

  .search-field,
  .summary-pill {
    width: 100%;
  }

  .note-cell {
    text-align: left;
  }
}
</style>
