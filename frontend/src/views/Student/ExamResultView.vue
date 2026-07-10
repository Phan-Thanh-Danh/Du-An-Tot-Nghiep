<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'
import { examApi } from '@/services/examApi'

const route = useRoute()
const examResultId = String(route.params.examResultId || '1')

const resultData = ref(null)
const loading = ref(true)
const error = ref('')

const metrics = ref([])
const rows = ref([])
const timeline = ref([])

onMounted(async () => {
  try {
    const res = await examApi.getStudentExamResult(examResultId)
    const data = res?.data || res?.Data || res

    resultData.value = data

    metrics.value = [
      { label: 'Điểm', value: data.score || '0', unit: '/10', icon: 'Award', tone: 'green', progress: (data.score || 0) * 10, hint: 'Điểm tổng kết' },
      { label: 'Trạng thái', value: data.status || 'Hoàn thành', unit: '', icon: 'CheckCircle2', tone: 'teal', progress: 100, hint: 'Trạng thái phiên thi' }
    ]

    rows.value = [
      { 
        title: 'Tổng quan môn học', 
        description: `Môn: ${data.subjectName || 'N/A'}`, 
        badge: 'Đã cập nhật', 
        tone: 'blue', 
        icon: 'FileCheck2', 
        meta: [examResultId, data.status, new Date(data.submittedAt).toLocaleString('vi-VN')], 
        value: data.score, 
        valueHint: 'Điểm' 
      }
    ]

    timeline.value = [
      { title: 'Nộp bài', description: 'Đã nộp bài và có điểm lưu trên hệ thống', time: new Date(data.submittedAt).toLocaleTimeString('vi-VN'), tone: 'green' }
    ]
  } catch (err) {
    error.value = err.message || 'Lỗi lấy kết quả thi'
    // fallback empty state
    metrics.value = [
      { label: 'Lỗi', value: '-', unit: '', icon: 'AlertCircle', tone: 'red', progress: 0, hint: 'Không thể tải điểm' }
    ]
    rows.value = []
    timeline.value = []
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <StudentModulePage
    icon="FileCheck2"
    eyebrow="Đánh giá"
    :title="`Kết quả thi ${examResultId}`"
    subtitle="Đã nối API lấy kết quả điểm tự luận / trắc nghiệm."
    primary-title="Chi tiết kết quả"
    primary-description="Xem chi tiết điểm và nhật ký thi."
    timeline-title="Phiên thi"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Lịch thi', to: '/student/exams' }, { label: 'Bảng điểm', to: '/student/grades', primary: true }]"
  />
</template>
