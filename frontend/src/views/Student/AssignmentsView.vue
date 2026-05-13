<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Chưa nộp', value: '3', unit: 'bài', icon: 'AlertCircle', tone: 'orange', progress: 42, hint: '1 bài cần xử lý trong hôm nay' },
  { label: 'Đã nộp', value: '12', unit: 'bài', icon: 'CheckCircle2', tone: 'green', progress: 80, hint: 'Tỷ lệ hoàn thành ổn định' },
  { label: 'Đang chấm', value: '2', unit: 'bài', icon: 'Clock3', tone: 'blue', progress: 25, hint: 'Chờ phản hồi từ giảng viên' },
  { label: 'Điểm TB', value: '8.4', unit: '/10', icon: 'Star', tone: 'violet', progress: 84, hint: 'Dữ liệu FE demo' },
]

const rows = [
  {
    title: 'Bài tập CTDL - Tuần 10',
    description: 'Cài đặt cây nhị phân tìm kiếm và phân tích độ phức tạp.',
    badge: 'Gấp',
    tone: 'orange',
    icon: 'ClipboardList',
    meta: ['CTDL101', 'Hạn hôm nay 23:59', 'Tối đa 2 lần nộp'],
    value: 'Chưa nộp',
    valueHint: 'Cần bổ sung API',
    to: '/student/assignments/demo',
  },
  {
    title: 'Bài tập lớn Lập trình Web',
    description: 'Thiết kế module frontend có state loading, error, empty và responsive.',
    badge: 'Đang làm',
    tone: 'blue',
    icon: 'Code2',
    meta: ['LTW301', 'Hạn 12/05/2025', 'File .zip hoặc repo link'],
    value: '65%',
    valueHint: 'Tiến độ dự kiến',
    to: '/student/assignments/web',
  },
  {
    title: 'Báo cáo thực hành CSDL',
    description: 'Chuẩn hóa truy vấn SQL, mô hình quan hệ và ảnh chụp kết quả.',
    badge: 'Sắp tới',
    tone: 'teal',
    icon: 'Database',
    meta: ['HQTCSDL401', 'Hạn 15/05/2025', 'Chưa mở nộp'],
    value: 'Chưa mở',
    valueHint: 'Theo lịch học vụ',
  },
]

const timeline = [
  { title: 'Ưu tiên bài CTDL', description: 'Bài này đang là hạng mục gần hạn nhất trong FE demo.', time: 'Hôm nay', tone: 'orange' },
  { title: 'Chờ API submissions', description: 'Nút nộp bài thật sẽ được bật khi backend có endpoint.', time: 'cần bổ sung', tone: 'blue' },
  { title: 'Giữ form dễ đọc', description: 'Dropzone và rubric dùng surface solid, không dùng glass quá mờ.', time: 'UI rule', tone: 'teal' },
]
</script>

<template>
  <StudentModulePage
    icon="ClipboardList"
    eyebrow="Học tập"
    title="Bài tập"
    subtitle="Module bài tập hiện là UI dự kiến; chưa gọi endpoint assignment/submission vì backend chưa có controller."
    primary-title="Danh sách bài tập"
    primary-description="Bố cục ưu tiên scan nhanh theo hạn nộp, môn học và trạng thái."
    timeline-title="Nhắc việc"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Xem khóa học', to: '/student/courses' }, { label: 'Tạo ticket hỗ trợ', to: '/student/support-tickets', primary: true }]"
  />
</template>
