<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
  Calendar,
  ArrowLeft
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()

const myClasses = ref([])
const selectedClassId = ref('')
const selectedClassName = ref('')

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

const getClassesList = async () => {
  loading.value = true
  try {
    const classesRes = await teacherApi.getTeacherClasses()
    const classesData = classesRes?.data?.data ?? classesRes?.data ?? classesRes?.Data ?? classesRes
    if (classesData && Array.isArray(classesData)) {
      myClasses.value = classesData
    }
  } catch (error) {
    console.error("Lỗi tải danh sách lớp:", error)
  } finally {
    loading.value = false
  }
}

async function loadGrades() {
  const classId = route.query.classId
  
  if (!classId) {
    selectedClassId.value = ''
    gradebook.value = []
    if (myClasses.value.length === 0) {
      await getClassesList()
    }
    return
  }

  selectedClassId.value = classId
  if (myClasses.value.length === 0) {
    await getClassesList()
  }
  const cls = myClasses.value.find(c => String(c.classId || c.id) === String(classId))
  selectedClassName.value = cls ? cls.className : `Lớp ${classId}`

  loading.value = true
  try {
    const res = await teacherApi.getTeacherClassGrades(classId)
    const extracted = res?.data?.items ?? res?.items ?? res?.data?.data ?? res?.data ?? res?.Data ?? res
    const items = Array.isArray(extracted) ? extracted : []
    gradebook.value = items.map(sv => ({
      id: sv.id,
      name: sv.name,
      gpa: Number(sv.total || 0).toFixed(1),
      status: Number(sv.total || 0) >= 5 ? 'Pass' : 'Fail',
      credits: 3 // fixed for demo
    }))
  } catch (error) {
    console.error("Lỗi khi tải bảng điểm:", error)
    gradebook.value = []
  } finally {
    loading.value = false
  }
}

const goToGrades = (id) => {
  router.push({ query: { ...route.query, classId: id } })
}

const goBack = () => {
  const q = { ...route.query }
  delete q.classId
  router.push({ query: q })
}

const exporting = ref(false)
const handleExport = async () => {
  if (!selectedClassId.value) return
  exporting.value = true
  try {
    const blob = await teacherApi.exportClassGrades(selectedClassId.value)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    // Use the class name for the file name, replacing spaces with underscores
    const safeName = (selectedClassName.value || `Lop_${selectedClassId.value}`).replace(/ /g, '_')
    a.download = `BangDiem_${safeName}.xlsx`
    document.body.appendChild(a)
    a.click()
    a.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error("Lỗi khi xuất bảng điểm:", error)
    alert("Không thể xuất bảng điểm lúc này. Vui lòng thử lại sau.")
  } finally {
    exporting.value = false
  }
}

watch(() => route.query.classId, () => {
  loadGrades()
})

onMounted(loadGrades)
</script>

<template>
  <div class="gradebook-page lg-page-enter">
    <div v-if="loading && !route.query.classId && myClasses.length === 0">
      <GlassPanel variant="flat" density="compact" class="loading-panel">
        <p>Đang tải danh sách lớp...</p>
      </GlassPanel>
    </div>
    
    <!-- CARD GRID VIEW (NO CLASS SELECTED) -->
    <template v-else-if="!route.query.classId">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-xl font-bold text-heading tracking-tight">Sổ điểm lớp học</h1>
          <p class="text-muted mt-1">Chọn lớp học để xem chi tiết kết quả học tập của sinh viên.</p>
        </div>
      </div>
      
      <div v-if="myClasses.length === 0" class="text-center p-8 bg-white rounded-xl shadow-sm border border-slate-200">
        <Users class="mx-auto h-12 w-12 text-slate-300 mb-3" />
        <p class="text-slate-500">Bạn chưa được phân công giảng dạy lớp nào.</p>
      </div>
      
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <TeacherClassCard
          v-for="cls in myClasses"
          :key="cls.classId || cls.id"
          :title="cls.className"
          :subtitle="cls.courseName || cls.subjectName || 'N/A'"
          :semester="cls.semester || 'Học kỳ hiện tại'"
          :studentsCount="cls.studentCount || cls.totalStudents"
        >
          <template #action>
            <button
              @click="goToGrades(cls.classId || cls.id)"
              class="w-full flex justify-center items-center gap-2 group-hover:bg-(--accent-primary) group-hover:text-inverse transition-all bg-slate-100 px-4 py-2 rounded-xl text-xs font-bold"
            >
              Xem sổ điểm
              <ArrowUpDown class="w-4 h-4 rotate-90" />
            </button>
          </template>
        </TeacherClassCard>
      </div>
    </template>
    
    <!-- DETAIL GRADEBOOK VIEW -->
    <template v-else>
      <div class="flex items-center gap-2 mb-2">
        <GlassButton variant="secondary" size="sm" @click="goBack" class="!px-2">
          <template #leading><ArrowLeft :size="16" /></template>
        </GlassButton>
        <span class="text-sm text-muted">Quay lại danh sách lớp</span>
      </div>

      <GlassPanel variant="flat" density="compact" class="page-header">
        <div class="header-copy">
          <div class="eyebrow">
            <Users :size="15" />
            {{ selectedClassName }}
          </div>
          <div>
            <h1>Sổ điểm lớp</h1>
            <p>Tổng hợp kết quả học tập và trạng thái hoàn thành môn học của {{ selectedClassName }}.</p>
          </div>
        </div>
  
        <div class="header-actions">
          <GlassButton variant="secondary" size="sm">
            <template #leading>
              <Printer :size="16" />
            </template>
            In bảng điểm
          </GlassButton>
          <GlassButton variant="primary" size="sm" :loading="exporting" @click="handleExport">
            <template #leading v-if="!exporting">
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
            <p>{{ gradebook.length }} sinh viên · Học kỳ hiện tại</p>
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
  
        <div v-if="loading" class="py-8 text-center text-muted">
          Đang tải dữ liệu điểm...
        </div>
        <TableShell v-else density="compact">
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
                    </span>
                  </div>
                </td>
                <td class="student-code">{{ sv.id }}</td>
                <td>
                  <GlassBadge variant="primary">{{ selectedClassName }}</GlassBadge>
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
    </template>
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
