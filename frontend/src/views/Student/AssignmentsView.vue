<script setup>
import { ref, computed, onMounted } from 'vue'
import { studentApi } from '@/services/studentApi.js'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const assignmentList = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await studentApi.getAssignments()
    if (res.success) {
      assignmentList.value = res.data
    }
  } catch (err) {
    console.error('Error fetching assignments:', err)
  } finally {
    loading.value = false
  }
})

const rows = computed(() => {
  if (!assignmentList.value) return []
  return assignmentList.value.map((item) => {
    let tone = item.variant || 'blue'
    
    let icon = 'ClipboardList'
    const courseLower = item.course.toLowerCase()
    if (courseLower.includes('web')) icon = 'Code2'
    else if (courseLower.includes('csdl')) icon = 'Database'
    else if (courseLower.includes('marketing') || courseLower.includes('seo')) icon = 'TrendingUp'
    else if (courseLower.includes('thiết kế') || courseLower.includes('màu sắc') || courseLower.includes('hình')) icon = 'Palette'

    const meta = [
      item.course,
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
  const list = assignmentList.value || []
  
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
      label: 'Điểm TB', 
      value: '8.2', 
      unit: '/10', 
      icon: 'Star', 
      tone: 'violet', 
      progress: 82, 
      hint: 'GPA học kỳ hiện tại' 
    },
  ]
})

const timeline = computed(() => {
  const list = assignmentList.value || []
  return list.map(item => {
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
  />
</template>
