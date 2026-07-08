<script setup>
import { ref, computed, onMounted } from 'vue'
import { studentApi } from '@/services/studentApi'
import { unwrapApiData } from '@/services/apiClient'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const loading = ref(false)
const error = ref('')
const subjects = ref([])
const gradeSummary = ref({})

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
  <StudentModulePage
    icon="BarChart2"
    eyebrow="Kết quả học tập"
    title="Bảng điểm"
    subtitle="Module điểm số hiện là UI dự kiến; frontend không tự bịa dữ liệu API."
    primary-title="Điểm theo môn học"
    primary-description="Bảng thông tin được tổ chức dạng solid card để dễ đối chiếu điểm, tín chỉ và trạng thái."
    timeline-title="Quy tắc điểm số"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem khóa học', to: '/student/courses' }, { label: 'Gửi yêu cầu', to: '/student/requests', primary: true }]"
  />
</template>
