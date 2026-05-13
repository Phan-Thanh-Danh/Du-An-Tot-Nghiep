<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Hôm nay', value: '2', unit: 'tiết', icon: 'CalendarDays', tone: 'blue', progress: 45, hint: 'Tiếp theo 13:30' },
  { label: 'Tuần này', value: '14', unit: 'tiết', icon: 'Rows3', tone: 'teal', progress: 64, hint: '5 môn học phần' },
  { label: 'Đổi lịch', value: '1', unit: 'môn', icon: 'RefreshCw', tone: 'amber', progress: 20, hint: 'Cần theo dõi thông báo' },
  { label: 'Phòng học', value: '4', unit: 'phòng', icon: 'MapPin', tone: 'violet', progress: 50, hint: 'Theo cơ sở CS1' },
]

const rows = [
  { title: 'Cấu trúc dữ liệu & Giải thuật', description: 'Buổi lý thuyết đã hoàn thành trong sáng nay.', badge: 'Đã học', tone: 'slate', icon: 'CheckCircle2', meta: ['07:30 - 09:30', 'P.302', 'TS. Nguyễn Minh Khoa'], value: 'CS1', valueHint: 'Cơ sở' },
  { title: 'Toán rời rạc', description: 'Buổi học tiếp theo trong ngày, cần đến đúng phòng học.', badge: 'Sắp tới', tone: 'blue', icon: 'Clock3', meta: ['13:30 - 15:30', 'P.105', 'ThS. Trần Thu Hà'], value: '13:30', valueHint: 'Bắt đầu' },
  { title: 'Lập trình Web', description: 'Thực hành frontend, ưu tiên mang laptop cá nhân.', badge: 'Ngày mai', tone: 'teal', icon: 'Monitor', meta: ['08:00 - 11:00', 'Lab 04', 'KS. Lê Văn Tâm'], value: 'Lab', valueHint: 'Thực hành' },
]

const timeline = [
  { title: 'Lịch hôm nay', description: '2 tiết học được hiển thị dưới dạng timeline dễ quét.', time: 'Hôm nay', tone: 'blue' },
  { title: 'Bộ lọc tuần', description: 'Sẽ nối API schedule khi backend có controller.', time: 'dự kiến', tone: 'teal' },
  { title: 'Thông báo đổi phòng', description: 'Không hardcode thông báo thật nếu chưa có endpoint notification.', time: 'cần bổ sung', tone: 'amber' },
]
</script>

<template>
  <StudentModulePage
    icon="CalendarDays"
    eyebrow="Lịch học"
    title="Thời khóa biểu"
    subtitle="Màn lịch học đã được chuẩn hóa surface Liquid Glass và đang chờ API schedule."
    primary-title="Lịch học gần nhất"
    primary-description="Các buổi học được chia thành hàng rõ ràng, giữ nền solid để sinh viên đọc nhanh thời gian và phòng."
    timeline-title="Dòng thời gian"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Điểm danh', to: '/student/attendance' }, { label: 'Tạo yêu cầu đổi lịch', to: '/student/requests', primary: true }]"
  />
</template>
