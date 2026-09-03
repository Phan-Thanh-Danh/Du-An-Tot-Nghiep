<script setup>
import { ref, computed, onMounted } from 'vue'
import { studentApi } from '@/services/studentApi.js'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { unwrapApiData } from '@/services/apiClient.js'

const assignmentList = ref([])
const loading = ref(true)
const selectedCourse = ref('')  // Lọc theo tên môn học (CourseCode/Course)

onMounted(async () => {
  try {
    const res = await studentApi.getAssignments()
    const raw = unwrapApiData(res)
    assignmentList.value = Array.isArray(raw) ? raw : (res?.data ?? res?.Data ?? [])
  } catch (err) {
    console.error('Error fetching assignments:', err)
  } finally {
    loading.value = false
  }
})

// Danh sách các khóa học/môn học duy nhất (để hiển thị dropdown)
const courseOptions = computed(() => {
  const seen = new Set()
  const options = []
  for (const item of assignmentList.value || []) {
    const key = item.courseId || item.course
    if (key && !seen.has(key)) {
      seen.add(key)
      options.push({
        value: String(item.courseId || item.course),
        label: item.courseCode ? `${item.courseCode} – ${item.course}` : item.course,
      })
    }
  }
  return options.sort((a, b) => a.label.localeCompare(b.label, 'vi'))
})

// Danh sách bài tập sau khi lọc theo khóa học
const filteredList = computed(() => {
  const list = assignmentList.value || []
  if (!selectedCourse.value) return list
  return list.filter(item => {
    const key = String(item.courseId || item.course)
    return key === selectedCourse.value
  })
})

const rows = computed(() => {
  return filteredList.value.map((item) => {
    let tone = item.variant || 'blue'

    let icon = 'ClipboardList'
    const courseLower = (item.course || '').toLowerCase()
    if (courseLower.includes('web')) icon = 'Code2'
    else if (courseLower.includes('csdl') || courseLower.includes('cơ sở dữ liệu')) icon = 'Database'
    else if (courseLower.includes('marketing') || courseLower.includes('seo')) icon = 'TrendingUp'
    else if (courseLower.includes('thiết kế') || courseLower.includes('màu sắc') || courseLower.includes('hình')) icon = 'Palette'

    const meta = [
      item.courseCode ? `${item.courseCode} – ${item.course}` : item.course,
      `Hạn: ${item.deadline}`,
    ]
    if (item.priority) {
      meta.push(`Độ ưu tiên: ${item.priority === 'high' ? 'Cao' : item.priority === 'medium' ? 'Trung bình' : 'Thấp'}`)
    }

    return {
      title: item.title,
      description: `Bài tập môn ${item.course}. Sinh viên hoàn thành đúng yêu cầu và nộp bài trước thời hạn.`,
      badge: item.status,
      tone: tone,
      icon: icon,
      meta: meta,
      value: item.status,
      valueHint: item.deadline,
      to: item.id ? `/student/assignments/${item.id}` : undefined,
    }
  })
})

const metrics = computed(() => {
  const list = filteredList.value

  const unpaid = list.filter(item => item.status === 'Chưa nộp' || item.status === 'Sắp đến hạn' || item.status === 'Quá hạn').length
  const paid = list.filter(item => item.status === 'Đã nộp' || item.status === 'Hoàn thành').length
  const grading = list.filter(item => item.status === 'Đang chấm' || item.status === 'Chờ chấm').length

  return [
    {
      label: 'Chưa nộp',
      value: String(unpaid),
      unit: 'bài',
      icon: 'AlertCircle',
      tone: 'orange',
      progress: list.length ? Math.round((unpaid / list.length) * 100) : 0,
      hint: unpaid > 0 ? `Cần chú ý` : 'Không có bài gấp'
    },
    {
      label: 'Đã nộp',
      value: String(paid),
      unit: 'bài',
      icon: 'CheckCircle2',
      tone: 'green',
      progress: list.length ? Math.round((paid / list.length) * 100) : 0,
      hint: 'Tỷ lệ hoàn thành ổn định'
    },
    {
      label: 'Đang chấm',
      value: String(grading),
      unit: 'bài',
      icon: 'Clock3',
      tone: 'blue',
      progress: list.length ? Math.round((grading / list.length) * 100) : 0,
      hint: 'Chờ phản hồi từ giảng viên'
    },
    {
      label: 'Tổng bài tập',
      value: String(list.length),
      unit: 'bài',
      icon: 'ClipboardList',
      tone: 'violet',
      progress: assignmentList.value.length ? Math.round((list.length / assignmentList.value.length) * 100) : 0,
      hint: selectedCourse.value ? 'Bài tập của môn đã chọn' : 'Tất cả môn học'
    },
  ]
})

const timeline = computed(() => {
  return filteredList.value.map(item => {
    let tone = 'blue'
    if (item.priority === 'high') tone = 'orange'
    else if (item.status === 'Quá hạn') tone = 'red'
    else if (item.status === 'Đã nộp') tone = 'teal'

    return {
      title: item.title,
      description: `Môn: ${item.course} - Trạng thái: ${item.status}`,
      time: item.deadline.split(' · ')[0] || item.deadline,
      tone: tone
    }
  })
})
</script>

<template>
  <StudentModulePage
    icon="ClipboardList"
    eyebrow="Học tập"
    title="Bài tập"
    subtitle="Quản lý và theo dõi bài tập các môn học"
    primary-title="Danh sách bài tập"
    primary-description="Bố cục ưu tiên scan nhanh theo hạn nộp, môn học và trạng thái."
    timeline-title="Nhắc việc"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem khóa học', to: '/student/courses' }, { label: 'Tạo ticket hỗ trợ', to: '/student/support-tickets', primary: true }]"
  >
    <template #filters>
      <!-- Bộ lọc theo khóa học / môn học -->
      <LmsSelect
        v-model="selectedCourse"
        :options="courseOptions"
        placeholder="Tất cả môn học"
        :searchable="courseOptions.length > 5"
        class="min-w-[200px]"
      />
    </template>
  </StudentModulePage>
</template>
