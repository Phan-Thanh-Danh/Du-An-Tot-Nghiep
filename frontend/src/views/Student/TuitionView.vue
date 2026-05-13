<script setup>
import StudentModulePage from '@/components/SinhVien/StudentModulePage.vue'

const metrics = [
  { label: 'Kỳ 2', value: 'Đã nộp', icon: 'BadgeCheck', tone: 'green', progress: 100, hint: 'Không còn công nợ demo' },
  { label: 'Tổng phí', value: '12.5', unit: 'tr', icon: 'WalletCards', tone: 'blue', progress: 75, hint: 'Dữ liệu minh họa' },
  { label: 'Giao dịch', value: '4', unit: 'lần', icon: 'ReceiptText', tone: 'teal', progress: 60, hint: 'Chưa nối API tài chính' },
  { label: 'Hoàn phí', value: '0', unit: 'yêu cầu', icon: 'Undo2', tone: 'slate', progress: 0, hint: 'Chưa phát sinh' },
]

const rows = [
  { title: 'Học phí kỳ 2/2024-2025', description: 'Khoản học phí chính theo số tín chỉ đã đăng ký.', badge: 'Đã thanh toán', tone: 'green', icon: 'CreditCard', meta: ['Hạn 30/06/2025', 'Biên lai FE demo'], value: '12.500.000đ', valueHint: 'Đã nộp' },
  { title: 'Lệ phí thư viện', description: 'Khoản phụ thu học vụ theo quy định dự kiến.', badge: 'Hoàn tất', tone: 'teal', icon: 'Library', meta: ['Kỳ 2', 'Không còn nợ'], value: '200.000đ', valueHint: 'Đã nộp' },
  { title: 'Yêu cầu hoàn phí', description: 'Luồng này cần API tài chính và phân quyền trước khi thao tác thật.', badge: 'Dự kiến', tone: 'amber', icon: 'Undo2', meta: ['Cần bổ sung', 'Không gọi API'], value: '0', valueHint: 'Yêu cầu' },
]

const timeline = [
  { title: 'Thanh toán hoàn tất', description: 'Tình trạng thanh toán hiện dùng dữ liệu FE demo.', time: 'Kỳ 2', tone: 'green' },
  { title: 'Không hardcode dữ liệu nhạy cảm', description: 'Không đưa giao dịch thật hoặc thông tin cá nhân vào component.', time: 'Security rule', tone: 'amber' },
  { title: 'Cần API tài chính', description: 'Invoice/transaction/refund là nhóm endpoint dự kiến.', time: 'cần bổ sung', tone: 'blue' },
]
</script>

<template>
  <StudentModulePage
    icon="CreditCard"
    eyebrow="Tài chính"
    title="Học phí & Thanh toán"
    subtitle="Màn tài chính hiện là UI dự kiến, không hardcode giao dịch hoặc thông tin nhạy cảm."
    primary-title="Khoản thu và giao dịch"
    primary-description="Các dòng tài chính dùng solid surface, rõ trạng thái và không lạm dụng hiệu ứng glass."
    timeline-title="Lưu ý tài chính"
    :metrics="metrics"
    :rows="rows"
    :timeline="timeline"
    :actions="[{ label: 'Tạo ticket tài chính', to: '/student/support-tickets', primary: true }]"
  />
</template>
