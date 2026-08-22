<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { X } from 'lucide-vue-next'
import { studentApi } from '@/services/studentApi'
import { unwrapApiData } from '@/services/apiClient'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const loading = ref(false)
const error = ref('')
const subjects = ref([])
const gradeSummary = ref({})

const showDetailModal = ref(false)
const detailLoading = ref(false)
const detailData = ref(null)
const detailCourseName = ref('')

onBeforeUnmount(() => {
  showDetailModal.value = false
  detailData.value = null
})

function formatGrade(value) {
  if (value === null || value === undefined) return '—'
  return Number(value).toFixed(2)
}

const flatColumns = computed(() => {
  if (!detailData.value) return []
  const gts = detailData.value.gradeTypes ?? detailData.value.GradeTypes ?? []
  
  const cols = []
  gts.forEach(gt => {
    const items = gt.items ?? gt.Items ?? []
    const typeWeight = gt.weight ?? gt.Weight ?? 0
    if (items.length === 0) {
      cols.push({
        gtCode: gt.code ?? gt.Code,
        gtName: gt.name ?? gt.Name,
        weight: typeWeight,
        itemName: '-',
        grade: null
      })
    } else {
      const itemWeight = (typeWeight / items.length)
      items.forEach(item => {
        cols.push({
          gtCode: gt.code ?? gt.Code,
          gtName: gt.name ?? gt.Name,
          weight: itemWeight,
          ...item
        })
      })
    }
  })
  return cols
})

async function openDetail(item) {
  detailCourseName.value = item.name || item.Name
  detailData.value = null
  showDetailModal.value = true
  detailLoading.value = true
  try {
    const res = await studentApi.getGradeDetail(item.courseId ?? item.CourseId, item.semesterId ?? item.SemesterId)
    detailData.value = res?.data ?? res?.Data ?? res
  } catch (error) {
    console.error('Lỗi khi tải chi tiết điểm:', error)
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  showDetailModal.value = false
  detailData.value = null
}

const metrics = computed(() => {
  const summary = gradeSummary.value || {}
  const earned = summary.totalCreditsEarned ?? summary.totalTinChiDaDat ?? 0
  const required = summary.totalCreditsRequired ?? summary.totalTinChiYeuCau ?? 120
  const classification = summary.classification ?? summary.xepLoai ?? 'Khá'

  return [
    {
      label: 'GPA tích lũy',
      value: summary.cumulativeGPA !== undefined ? String(summary.cumulativeGPA ?? summary.gpaTichLuy) : '3.2',
      unit: '/4.0',
      icon: 'TrendingUp',
      tone: 'blue',
      progress: Math.round((((summary.cumulativeGPA ?? summary.gpaTichLuy) || 3.2) / 4.0) * 100),
      hint: `Xếp loại ${classification}`
    },
    {
      label: 'Môn đã đạt',
      value: String(summary.totalSubjectsPassed ?? summary.soMonDaDat ?? 0),
      unit: 'môn',
      icon: 'CheckCircle2',
      tone: 'green',
      progress: subjects.value.length ? Math.round((((summary.totalSubjectsPassed ?? summary.soMonDaDat) || 0) / subjects.value.length) * 100) : 0,
      hint: (summary.totalSubjectsFailed ?? summary.soMonRot) ? `Bị rớt ${summary.totalSubjectsFailed ?? summary.soMonRot} môn` : 'Không có môn rớt'
    },
    {
      label: 'Tín chỉ',
      value: String(earned),
      unit: `/${required}`,
      icon: 'BadgeCheck',
      tone: 'teal',
      progress: Math.round((earned / required) * 100),
      hint: 'Theo tiến độ học tập'
    },
    {
      label: 'Cần rà soát',
      value: String(summary.riskAlertCount ?? summary.soCanhBao ?? 0),
      unit: 'điểm',
      icon: 'AlertTriangle',
      tone: 'amber',
      progress: (summary.riskAlertCount ?? summary.soCanhBao) ? 100 : 0,
      hint: (summary.riskAlertCount ?? summary.soCanhBao) ? 'Có điểm cần phản hồi gấp' : 'Điểm số ổn định'
    },
  ]
})

const rows = computed(() => {
  return subjects.value.map((item) => {
    let tone = 'blue'
    if (item.status === 'pass') tone = 'green'
    else if (item.status === 'fail') tone = 'red'
    else if (item.gpa === null || item.gpa === undefined) tone = 'amber'

    let icon = 'BookOpen'
    const codeUpper = (item.code || '').toUpperCase()
    if (codeUpper.startsWith('GD')) icon = 'Palette'
    else if (codeUpper.startsWith('MR') || codeUpper.startsWith('MKT')) icon = 'TrendingUp'
    else if (codeUpper.startsWith('WEB') || codeUpper.startsWith('LTW') || codeUpper.startsWith('NET') || codeUpper.startsWith('SDLC')) icon = 'Code2'
    else if (codeUpper.startsWith('DB') || codeUpper.startsWith('HQT')) icon = 'Database'

    const process = item.processScore !== null && item.processScore !== undefined ? String(item.processScore) : 'chưa có'
    const midterm = item.midtermScore !== null && item.midtermScore !== undefined ? String(item.midtermScore) : 'chưa có'
    const final = item.finalScore !== null && item.finalScore !== undefined ? String(item.finalScore) : 'chưa có'

    return {
      title: item.name,
      description: `Điểm quá trình: ${process}, giữa kỳ: ${midterm}, cuối kỳ: ${final}. Ghi chú: ${item.note || 'Không có ghi chú.'}`,
      badge: item.statusLabel || (item.status === 'pass' ? 'Đạt' : 'Chưa đạt'),
      tone: tone,
      icon: icon,
      meta: [item.code, `${item.credits} tín chỉ`, item.semester || 'Kỳ này'],
      value: item.gpa !== null && item.gpa !== undefined ? String(item.gpa) : 'chưa có',
      valueHint: item.letterGrade || 'Đang tính',
      onClick: () => openDetail(item),
    }
  })
})

const timeline = [
  { title: 'Bảng điểm ưu tiên solid', description: 'Dữ liệu điểm không dùng surface quá trong suốt để giữ readability.', time: 'Design rule', tone: 'blue' },
  { title: 'Yêu cầu sửa điểm', description: 'Luồng này là dự kiến, cần API grade-change-request.', time: 'cần bổ sung', tone: 'amber' },
  { title: 'Không tự bịa contract', description: 'Frontend chỉ trình bày demo cho đến khi backend có controller.', time: 'API rule', tone: 'teal' },
]

onMounted(async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await studentApi.getGrades()
    const data = unwrapApiData(response) || {}
    subjects.value = data.subjects ?? data.Subjects ?? data.items ?? data.Items ?? []
    gradeSummary.value = data.summary ?? data.Summary ?? data.gradeSummary ?? data.GradeSummary ?? {}
  } catch (e) {
    error.value = e?.message || 'Không thể tải dữ liệu.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="grades-view-root">
    <StudentModulePage
      icon="BarChart2"
      eyebrow="Kết quả học tập"
      title="Bảng điểm"
      subtitle="Xem bảng điểm quá trình và điểm thi chi tiết."
      primary-title="Điểm theo môn học"
      primary-description="Bảng thông tin được tổ chức dạng solid card để dễ đối chiếu điểm, tín chỉ và trạng thái."
      timeline-title="Quy tắc điểm số"
      :metrics="metrics"
      :rows="rows"
      :timeline="timeline"
      :actions="[{ label: 'Xem khóa học', to: '/student/courses' }, { label: 'Gửi yêu cầu', to: '/student/requests', primary: true }]"
    />

    <Teleport to="body">
      <div v-if="showDetailModal" class="modal-overlay" @click.self="closeDetail" @keydown.esc="closeDetail">
        <GlassPanel variant="readable" density="comfortable" :clip="false" class="detail-modal">
          <div class="detail-header">
            <div>
              <h2>Chi tiết điểm</h2>
              <p>{{ detailCourseName }}</p>
            </div>
            <button class="lg-icon-button detail-close" @click="closeDetail" aria-label="Đóng">
              <X :size="16" />
            </button>
          </div>

          <div v-if="detailLoading" class="py-8 text-center text-muted">
            Đang tải chi tiết điểm...
          </div>

          <template v-else-if="detailData">
            <div class="detail-types-table-wrapper" style="overflow-x: auto; margin-bottom: 2rem; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); background: white;">
              <TableShell density="comfortable" style="width: max-content; min-width: 100%;">
                <table class="detail-table" style="color: #333;">
                  <thead>
                    <tr style="background-color: #f1f5f9;">
                      <th rowspan="2" class="sticky-col text-center" style="left: 0; min-width: 40px; z-index: 4; border-right: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; background: #f8fafc;">#</th>
                      <th rowspan="2" class="sticky-col text-left" style="left: 40px; min-width: 120px; z-index: 4; border-right: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; background: #f8fafc;">Mã sinh viên</th>
                      <th rowspan="2" class="sticky-col text-left" style="left: 160px; min-width: 180px; z-index: 4; border-right: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; background: #f8fafc;">Họ và tên</th>
                      
                      <th v-for="(gt, index) in (detailData.gradeTypes ?? detailData.GradeTypes)" :key="'gt-' + index" 
                          :colspan="(gt.items ?? gt.Items)?.length || 1" 
                          class="text-center font-medium text-slate-700"
                          style="border-bottom: 1px solid #e2e8f0; padding-bottom: 8px;">
                        {{ gt.name ?? gt.Name }} <br/><span class="text-xs text-slate-500">({{ gt.weight ?? gt.Weight }}%)</span>
                      </th>
                      
                      <th rowspan="2" class="text-center font-bold text-slate-700" style="min-width: 80px; border-bottom: 1px solid #e2e8f0;">Tổng kết</th>
                      <th rowspan="2" class="text-center font-bold text-slate-700" style="min-width: 100px; border-bottom: 1px solid #e2e8f0;">Trạng thái</th>
                    </tr>
                    <tr style="background-color: #f8fafc;">
                      <th v-for="(col, index) in flatColumns" :key="'col-' + index" class="text-center text-slate-600 font-medium" style="border-bottom: 1px solid #e2e8f0;">
                        {{ col.itemName ?? col.ItemName }} <br/>
                        <span class="text-xs text-slate-500">({{ (col.weight || 0).toFixed(1).replace('.0', '') }}%)</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody style="background: white;">
                    <tr class="hover:bg-slate-50 transition-colors">
                      <td class="sticky-col text-center" style="left: 0; border-right: 1px solid #f1f5f9; background: white;">1</td>
                      <td class="sticky-col text-left font-medium" style="left: 40px; border-right: 1px solid #f1f5f9; background: white;">
                        {{ (detailData.studentId ?? detailData.StudentId) }}
                      </td>
                      <td class="sticky-col text-left font-medium" style="left: 160px; border-right: 1px solid #f1f5f9; background: white;">
                        {{ (detailData.studentName ?? detailData.StudentName) }}
                      </td>
                      <td v-for="(col, index) in flatColumns" :key="'td-' + index" class="text-center">
                        <span :class="['detail-item-grade font-semibold text-[1.05rem]', (col.grade ?? col.Grade) === null ? 'text-slate-400' : 'text-slate-800']">
                          {{ (col.grade ?? col.Grade) === null ? '—' : formatGrade(col.grade ?? col.Grade) }}
                        </span>
                      </td>
                      <td class="text-center font-bold text-slate-800">
                        {{ formatGrade(detailData.gpaMonHoc ?? detailData.GpaMonHoc) }}
                      </td>
                      <td class="text-center">
                        <span :class="(detailData.trangThai ?? detailData.TrangThai) === 'Đạt' ? 'text-success font-medium' : 'text-danger font-medium'">
                          {{ detailData.trangThai ?? detailData.TrangThai }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </TableShell>
            </div>
          </template>
          <div v-else class="py-8 text-center text-muted">
            Không thể tải chi tiết điểm.
          </div>
        </GlassPanel>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal, 9999);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: color-mix(in srgb, var(--surface-app) 58%, transparent);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.detail-modal {
  width: 95vw;
  max-width: 1600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--lg-shadow-lg);
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.detail-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-heading);
}

.detail-header p {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  color: var(--text-muted);
}

.detail-close {
  color: var(--text-muted);
}

.detail-close:hover {
  color: var(--text-heading);
}

.detail-table {
  width: 100%;
  border-collapse: collapse;
}
.detail-table th, .detail-table td {
  padding: 0.75rem 1rem;
  border-right: 1px solid var(--border-card, #e2e8f0);
  border-bottom: 1px solid var(--border-card, #e2e8f0);
  white-space: nowrap;
}
.detail-table th.sticky-col, .detail-table td.sticky-col {
  position: sticky;
  left: 0;
  background-color: var(--surface-card, #f8fafc);
  z-index: 10;
  border-right: 2px solid var(--border-card, #e2e8f0);
}
.detail-table thead th {
  background-color: var(--surface-card, #f8fafc);
  font-weight: 600;
}
</style>
