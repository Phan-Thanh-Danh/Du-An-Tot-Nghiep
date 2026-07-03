<script setup>
import { ref, computed, onMounted } from 'vue'
import { mockSubjects, mockGradeSummary } from '@/data/studentData.mock.js'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const loading = ref(false)
const error = ref('')

const subjectsData = computed(() => ENABLE_MOCK_API ? mockSubjects : [])
const gradeSummaryData = computed(() => ENABLE_MOCK_API ? mockGradeSummary : {})

const metrics = computed(() => {
  const summary = gradeSummaryData.value || {}
  const earned = summary.totalCreditsEarned || 0
  const required = summary.totalCreditsRequired || 120
  const classification = summary.classification || 'Khá'
  
  return [
    { 
      label: 'GPA tích lũy', 
      value: summary.cumulativeGPA !== undefined ? String(summary.cumulativeGPA) : '3.2', 
      unit: '/4.0', 
      icon: 'TrendingUp', 
      tone: 'blue', 
      progress: Math.round(((summary.cumulativeGPA || 3.2) / 4.0) * 100), 
      hint: `Xếp loại ${classification}` 
    },
    { 
      label: 'Môn đã đạt', 
      value: String(summary.totalSubjectsPassed || 0), 
      unit: 'môn', 
      icon: 'CheckCircle2', 
      tone: 'green', 
      progress: subjectsData.value.length ? Math.round(((summary.totalSubjectsPassed || 0) / subjectsData.value.length) * 100) : 0, 
      hint: summary.totalSubjectsFailed ? `Bị rớt ${summary.totalSubjectsFailed} môn` : 'Không có môn rớt' 
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
      value: String(summary.riskAlertCount || 0), 
      unit: 'điểm', 
      icon: 'AlertTriangle', 
      tone: 'amber', 
      progress: summary.riskAlertCount ? 100 : 0, 
      hint: summary.riskAlertCount ? 'Có điểm cần phản hồi gấp' : 'Điểm số ổn định' 
    },
  ]
})

const rows = computed(() => {
  return subjectsData.value.map((item) => {
    let tone = 'blue'
    if (item.status === 'pass') tone = 'green'
    else if (item.status === 'fail') tone = 'red'
    else if (item.gpa === null || item.gpa === undefined) tone = 'amber'

    let icon = 'BookOpen'
    const codeUpper = item.code.toUpperCase()
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
    // Attempt to load real data in future
  } catch (e) {
    if (!ENABLE_MOCK_API) error.value = e?.message || 'Không thể tải dữ liệu.'
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
