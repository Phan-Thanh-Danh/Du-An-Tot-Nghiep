<script setup>
/**
 * SendNotificationView.vue - Super Admin
 * Giao diện soạn thảo và gửi thông báo đa kênh toàn hệ thống hoặc theo phạm vi cụ thể.
 * Tích hợp AI Smart Timing, Bulk Send Auto-Batching, Deduplication, Real-time Preview Modal,
 * và Dashboard theo dõi trạng thái gửi tin (Queued, Scheduled, Sent).
 */
import { ref, computed, watch } from 'vue'
import {
  Send,
  Calendar,
  Layers,
  Users,
  Eye,
  Trash2,
  AlertTriangle,
  CheckCircle,
  Clock,
  Sparkles,
  Info,
  X,
  FileText,
  Mail,
  MessageSquare,
  Bell,
  Check
} from 'lucide-vue-next'

// --- Mock Data cho Phạm vi nhận tin ---
const campuses = [
  { id: 'HN', name: 'Campus Hà Nội' },
  { id: 'HCM', name: 'Campus TP. Hồ Chí Minh' },
  { id: 'DN', name: 'Campus Đà Nẵng' }
]

const subCampuses = {
  HN: [
    { id: 'HN-HL', name: 'Cơ sở Hòa Lạc' },
    { id: 'HN-PX', name: 'Cơ sở Phạm Văn Đồng' }
  ],
  HCM: [
    { id: 'HCM-Q9', name: 'Cơ sở Quận 9' },
    { id: 'HCM-Q12', name: 'Cơ sở Quận 12' }
  ],
  DN: [
    { id: 'DN-NHS', name: 'Cơ sở Ngũ Hành Sơn' }
  ]
}

const classesMock = [
  { id: 'SE1701', name: 'Lớp SE1701 (Kỹ thuật phần mềm)' },
  { id: 'SE1702', name: 'Lớp SE1702 (Kỹ thuật phần mềm)' },
  { id: 'GD1602', name: 'Lớp GD1602 (Thiết kế đồ họa)' },
  { id: 'MC1801', name: 'Lớp MC1801 (Truyền thông đa phương tiện)' }
]

const eventTypes = [
  { code: 'GENERAL', name: 'Thông báo chung toàn hệ thống' },
  { code: 'ACADEMIC_ALERT', name: 'Cảnh báo học vụ & Lớp học' },
  { code: 'TUITION_BILL', name: 'Thông báo học phí & Công nợ' },
  { code: 'EXAM_REGISTRATION', name: 'Lịch đăng ký thi & Ca thi' }
]

// --- Mock Lịch sử gửi tin & Hàng chờ ---
const recentCampaigns = ref([
  {
    id: 'CAMP-001',
    title: 'Nhắc đóng học phí học kỳ Spring 2026',
    eventType: 'TUITION_BILL',
    scope: 'Toàn trường',
    channels: ['Email', 'SMS'],
    targetRoles: ['Student'],
    scheduledAt: '2026-06-22 19:30:00',
    status: 'Scheduled', // 'Queued' | 'Scheduled' | 'Sent' | 'Cancelled'
    sentCount: 12847,
    deliveredCount: 0,
    failedCount: 0,
    openRate: 0,
    createdAt: '2026-06-22 11:20:00'
  },
  {
    id: 'CAMP-002',
    title: 'Thông báo lịch bảo trì hệ thống LMS tối thứ Bảy',
    eventType: 'GENERAL',
    scope: 'Toàn trường',
    channels: ['Email', 'Push'],
    targetRoles: ['Student', 'Teacher', 'Staff'],
    scheduledAt: null,
    status: 'Sent',
    sentCount: 15420,
    deliveredCount: 15385,
    failedCount: 35,
    openRate: 68.5,
    createdAt: '2026-06-21 09:15:00'
  },
  {
    id: 'CAMP-003',
    title: 'Thay đổi phòng thi tốt nghiệp lớp SE1701',
    eventType: 'EXAM_REGISTRATION',
    scope: 'Lớp SE1701',
    channels: ['Push', 'SMS'],
    targetRoles: ['Student'],
    scheduledAt: null,
    status: 'Sent',
    sentCount: 32,
    deliveredCount: 32,
    failedCount: 0,
    openRate: 95.0,
    createdAt: '2026-06-20 14:05:00'
  }
])

// --- Danh sách các mẫu Template đã tạo sẵn (mock đồng bộ từ trang Template thông báo) ---
const availableTemplates = [
  {
    id: 1,
    name: '[Email] Kết quả môn học (GRADE_PUBLISHED)',
    eventType: 'ACADEMIC_ALERT',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Công bố kết quả môn học {{course_name}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #2563eb;">Thông báo Kết Quả Học Tập</h2>\n  <p>Xin chào <strong>{{student_name}}</strong> ({{student_id}}),</p>\n  <p>Điểm số chính thức của môn học <strong>{{course_name}}</strong> trong học kỳ <strong>{{semester}}</strong> đã được công bố.</p>\n  <table style="width: 100%; border-collapse: collapse; margin-top: 15px;">\n    <tr style="background-color: #f8fafc;">\n      <td style="padding: 10px; border: 1px solid #e2e8f0;">Điểm trung bình</td>\n      <td style="padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; color: #16a34a;">{{grade}}</td>\n    </tr>\n    <tr>\n      <td style="padding: 10px; border: 1px solid #e2e8f0;">Trạng thái học vụ</td>\n      <td style="padding: 10px; border: 1px solid #e2e8f0; font-weight: bold;">{{status}}</td>\n    </tr>\n  </table>\n  <p style="margin-top: 20px; font-size: 13px; color: #64748b;">Vui lòng truy cập LMS để xem chi tiết bảng điểm.</p>\n</div>'
  },
  {
    id: 2,
    name: '[Push] Công bố điểm số (GRADE_PUBLISHED)',
    eventType: 'ACADEMIC_ALERT',
    channel: 'Push',
    subjectTemplate: '',
    bodyTemplate: 'Điểm số môn {{course_name}} của bạn đã được công bố. Kết quả: {{grade}} ({{status}}). Xem chi tiết trên LMS ngay!'
  },
  {
    id: 3,
    name: '[Email] Cảnh báo chuyên cần (ATTENDANCE_WARNING)',
    eventType: 'ACADEMIC_ALERT',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Cảnh báo chuyên cần môn {{course_name}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #fca5a5; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #dc2626;">Cảnh Báo Vắng Mặt Vượt Giới Hạn</h2>\n  <p>Thân gửi <strong>{{student_name}}</strong>,</p>\n  <p>Hệ thống ghi nhận bạn đã vắng <strong>{{absent_slots}} buổi</strong> trên tổng số <strong>{{max_slots}} buổi học</strong> cho phép của môn <strong>{{course_name}}</strong>.</p>\n  <p style="color: #dc2626; font-weight: bold;">Cảnh báo: Nếu bạn vắng quá {{max_slots}} buổi học, bạn sẽ bị cấm thi môn học này.</p>\n</div>'
  },
  {
    id: 4,
    name: '[SMS] Cảnh báo chuyên cần (ATTENDANCE_WARNING)',
    eventType: 'ACADEMIC_ALERT',
    channel: 'SMS',
    subjectTemplate: '',
    bodyTemplate: 'LMS Attendance Warning: Sinh vien {{student_name}} da vang {{absent_slots}}/{{max_slots}} buoi mon {{course_name}}. Vui long di hoc day du.'
  },
  {
    id: 5,
    name: '[Email] Nhắc đóng học phí (PAYMENT_DUE)',
    eventType: 'TUITION_BILL',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Nhắc nợ học phí học kỳ {{semester}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #fde68a; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #d97706;">Thông Báo Học Phí Học Kỳ {{semester}}</h2>\n  <p>Xin chào <strong>{{student_name}}</strong>,</p>\n  <p>LMS gửi thông báo nhắc bạn hoàn thành nghĩa vụ học phí: <strong>{{amount}} VNĐ</strong> trước ngày <strong style="color: #dc2626;">{{due_date}}</strong>.</p>\n</div>'
  },
  {
    id: 6,
    name: '[SMS] Nhắc nợ học phí (PAYMENT_DUE)',
    eventType: 'TUITION_BILL',
    channel: 'SMS',
    subjectTemplate: '',
    bodyTemplate: 'LMS Nhac Hoc Phi: SV {{student_name}} ({{student_id}}) nop hoc phi ky {{semester}} truoc {{due_date}}. So tien: {{amount}} VND.'
  }
]

const selectedTemplateId = ref('')

const applyTemplate = () => {
  if (!selectedTemplateId.value) {
    return
  }
  const tpl = availableTemplates.find(t => t.id === Number(selectedTemplateId.value))
  if (tpl) {
    notificationForm.value.eventType = tpl.eventType
    notificationForm.value.channels = [tpl.channel]
    notificationForm.value.title = tpl.subjectTemplate || ''
    notificationForm.value.body = tpl.bodyTemplate || ''
    triggerToast(`Đã áp dụng mẫu thông báo: ${tpl.name.split('] ')[1]}`, 'success')
  }
}

// --- State Form ---
const notificationForm = ref({
  title: '',
  body: '',
  eventType: 'GENERAL',
  channels: ['Email'], // Email, SMS, Push
  scope: 'all', // 'all' | 'campus' | 'subcampus' | 'class'
  selectedCampus: 'HN',
  selectedSubCampus: 'HN-HL',
  selectedClass: 'SE1701',
  targetRoles: ['Student'], // Student, Teacher, Staff
  isScheduled: false,
  scheduledDate: '',
  scheduledTime: ''
})

// AI Smart Timing State
const useAISmartTiming = ref(false)
const aiRecommendationText = ref('')

// Preview State
const isPreviewOpen = ref(false)
const previewActiveChannel = ref('Email') // Email | SMS | Push

// Toast Notification
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success') // 'success' | 'error' | 'info'

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 4000)
}

// --- Tính toán Phạm vi người nhận & Số lượng giả lập ---
const estimatedRecipientCount = computed(() => {
  let count = 0
  const scope = notificationForm.value.scope
  const roles = notificationForm.value.targetRoles

  // Tính số lượng cơ bản dựa trên vai trò
  let multiplier = 0
  if (roles.includes('Student')) multiplier += 0.8
  if (roles.includes('Teacher')) multiplier += 0.15
  if (roles.includes('Staff')) multiplier += 0.05

  if (scope === 'all') {
    count = 15420
  } else if (scope === 'campus') {
    const campus = notificationForm.value.selectedCampus
    if (campus === 'HN') count = 7500
    else if (campus === 'HCM') count = 5600
    else count = 2320
  } else if (scope === 'subcampus') {
    const sub = notificationForm.value.selectedSubCampus
    if (sub === 'HN-HL') count = 4800
    else if (sub === 'HN-PX') count = 2700
    else if (sub === 'HCM-Q9') count = 3800
    else if (sub === 'HCM-Q12') count = 1800
    else count = 2320
  } else if (scope === 'class') {
    count = 32
  }

  return Math.round(count * multiplier)
})

// --- Quy tắc Nghiệp vụ Validation ---
const isFormValid = computed(() => {
  if (!notificationForm.value.title.trim()) return false
  if (!notificationForm.value.body.trim()) return false
  if (notificationForm.value.channels.length === 0) return false
  if (notificationForm.value.targetRoles.length === 0) return false
  if (notificationForm.value.isScheduled) {
    if (!notificationForm.value.scheduledDate || !notificationForm.value.scheduledTime) return false
  }
  return true
})

// Kiểm tra chống trùng lặp (Deduplication Check)
const showDeduplicationWarning = ref(false)
watch(
  () => notificationForm.value.body,
  (newVal) => {
    if (newVal.trim().length > 10) {
      // Giả lập kiểm tra trùng lặp: Nếu trùng lặp một phần nội dung với tin gần đây
      const isDuplicate = recentCampaigns.value.some(c => 
        c.status === 'Sent' && 
        (c.title.toLowerCase().includes(notificationForm.value.title.toLowerCase()) || 
         newVal.toLowerCase().includes('bảo trì') || 
         newVal.toLowerCase().includes('học phí')) &&
        (new Date() - new Date(c.createdAt)) < 5 * 60 * 1000 // dưới 5 phút
      )
      showDeduplicationWarning.value = isDuplicate
    } else {
      showDeduplicationWarning.value = false
    }
  }
)

// Cảnh báo Bulk Send (số lượng nhận lớn hơn 500)
const isBulkSend = computed(() => {
  return estimatedRecipientCount.value > 500
})

// --- AI Smart Timing Recommendation Generator ---
watch(
  [() => notificationForm.value.eventType, () => notificationForm.value.targetRoles],
  () => {
    const roles = notificationForm.value.targetRoles
    const event = notificationForm.value.eventType

    if (roles.includes('Student') && event === 'TUITION_BILL') {
      aiRecommendationText.value = 'Học sinh có xu hướng mở tin báo đóng tiền nhiều nhất vào lúc 19:30 hàng ngày. Đề xuất: 19:30 tối nay.'
    } else if (roles.includes('Teacher')) {
      aiRecommendationText.value = 'Giảng viên mở thông báo học vụ nhiều nhất vào đầu giờ sáng làm việc. Đề xuất: 08:30 sáng mai.'
    } else {
      aiRecommendationText.value = 'Mức độ hoạt động cao của tệp người nhận được ghi nhận vào chiều muộn. Đề xuất: 16:15 hôm nay.'
    }
  },
  { immediate: true }
)

// Xử lý nút AI Smart Timing
const toggleAISmartTiming = () => {
  useAISmartTiming.value = !useAISmartTiming.value
  if (useAISmartTiming.value) {
    notificationForm.value.isScheduled = true
    const now = new Date()
    // Giả lập thiết lập giờ vàng
    const tomorrow = new Date(now)
    tomorrow.setDate(now.getDate() + 1)
    
    const year = tomorrow.getFullYear()
    const month = String(tomorrow.getMonth() + 1).padStart(2, '0')
    const day = String(tomorrow.getDate()).padStart(2, '0')
    
    notificationForm.value.scheduledDate = `${year}-${month}-${day}`
    
    const event = notificationForm.value.eventType
    const roles = notificationForm.value.targetRoles
    if (roles.includes('Student') && event === 'TUITION_BILL') {
      notificationForm.value.scheduledTime = '19:30'
    } else if (roles.includes('Teacher')) {
      notificationForm.value.scheduledTime = '08:30'
    } else {
      notificationForm.value.scheduledTime = '16:15'
    }
    triggerToast('AI đã tự động phân tích hành vi và cấu hình lịch gửi tối ưu!', 'info')
  } else {
    notificationForm.value.isScheduled = false
  }
}

// --- Preview Variables Render ---
const renderPreviewContent = (text) => {
  if (!text) return ''
  // Thay thế các biến động Handlebars giả lập nếu có
  return text
    .replace(/\{\{student_name\}\}/g, 'Nguyễn Văn An')
    .replace(/\{\{student_id\}\}/g, 'HE170001')
    .replace(/\{\{semester\}\}/g, 'Spring 2026')
    .replace(/\{\{due_date\}\}/g, '30/06/2026')
}

// --- Submit Handlers ---
const handleSendNotification = () => {
  if (!isFormValid.value) return

  const now = new Date()
  const timeString = now.toLocaleString('vi-VN')

  let scheduledAtString = null
  let finalStatus = 'Sent'

  if (notificationForm.value.isScheduled) {
    scheduledAtString = `${notificationForm.value.scheduledDate} ${notificationForm.value.scheduledTime}:00`
    finalStatus = 'Scheduled'
  }

  let scopeLabel = 'Toàn trường'
  if (notificationForm.value.scope === 'campus') {
    scopeLabel = campuses.find(c => c.id === notificationForm.value.selectedCampus)?.name || 'Cơ sở'
  } else if (notificationForm.value.scope === 'subcampus') {
    const campusId = notificationForm.value.selectedCampus
    const subId = notificationForm.value.selectedSubCampus
    scopeLabel = subCampuses[campusId]?.find(s => s.id === subId)?.name || 'Cơ sở thành viên'
  } else if (notificationForm.value.scope === 'class') {
    scopeLabel = `Lớp ${notificationForm.value.selectedClass}`
  }

  const newCampaign = {
    id: `CAMP-0${recentCampaigns.value.length + 1}`,
    title: notificationForm.value.title,
    eventType: notificationForm.value.eventType,
    scope: scopeLabel,
    channels: [...notificationForm.value.channels],
    targetRoles: [...notificationForm.value.targetRoles],
    scheduledAt: scheduledAtString,
    status: finalStatus,
    sentCount: estimatedRecipientCount.value,
    deliveredCount: finalStatus === 'Sent' ? Math.round(estimatedRecipientCount.value * 0.99) : 0,
    failedCount: 0,
    openRate: 0,
    createdAt: timeString
  }

  recentCampaigns.value.unshift(newCampaign)
  
  if (finalStatus === 'Scheduled') {
    triggerToast(`Đã lên lịch chiến dịch thành công lúc ${scheduledAtString}!`, 'success')
  } else {
    triggerToast(`Chiến dịch gửi thông báo đã được đưa vào hàng chờ chuyển đi!`, 'success')
  }

  // Reset form
  notificationForm.value.title = ''
  notificationForm.value.body = ''
  notificationForm.value.isScheduled = false
  useAISmartTiming.value = false
}

const handleSaveDraft = () => {
  if (!notificationForm.value.title.trim()) {
    triggerToast('Vui lòng điền tiêu đề để lưu bản nháp!', 'error')
    return
  }
  triggerToast('Đã lưu nội dung soạn thảo vào kho bản nháp của hệ thống.', 'success')
}

// Hủy chiến dịch đã lên lịch
const cancelCampaign = (campaign) => {
  campaign.status = 'Cancelled'
  triggerToast(`Đã hủy lịch phát sóng của chiến dịch ${campaign.id}!`, 'info')
}

// Thống kê Analytics mini
const totalSentAllTime = computed(() => {
  return recentCampaigns.value
    .filter(c => c.status === 'Sent')
    .reduce((sum, c) => sum + c.sentCount, 0)
})

const avgDeliveryRate = computed(() => {
  const sentCampaigns = recentCampaigns.value.filter(c => c.status === 'Sent')
  if (sentCampaigns.length === 0) return 0
  const totalSent = sentCampaigns.reduce((sum, c) => sum + c.sentCount, 0)
  const totalDelivered = sentCampaigns.reduce((sum, c) => sum + c.deliveredCount, 0)
  return ((totalDelivered / totalSent) * 100).toFixed(1)
})
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
          <Send class="w-8 h-8 text-primary" />
          Gửi Thông Báo Hệ Thống
        </h1>
        <p class="text-sm text-muted mt-1">
          Thiết lập nội dung và điều phối gửi thông báo đa kênh (Email, SMS, Push) tới toàn trường hoặc các phân cấp đơn vị cụ thể.
        </p>
      </div>

      <!-- Analytics Dashboard mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
            <Send class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tổng số đã gửi</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ totalSentAllTime.toLocaleString('vi-VN') }} tin</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tỷ lệ chuyển thành công</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ avgDeliveryRate }}%</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <Mail class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tỷ lệ mở Email (TB)</div>
            <div class="text-xl font-bold mt-0.5 text-heading">72.4%</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Clock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Đang chờ gửi</div>
            <div class="text-xl font-bold mt-0.5 text-heading">
              {{ recentCampaigns.filter(c => c.status === 'Scheduled' || c.status === 'Queued').length }} chiến dịch
            </div>
          </div>
        </div>
      </div>

      <!-- Bố cục 2 cột chính -->
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        
        <!-- Cột Trái: Soạn thảo & Cấu hình đối tượng (7/12) -->
        <div class="lg:col-span-7 space-y-6">
          <div class="lg-glass-soft lg-card lg-density-normal">
            <div class="flex items-center gap-2 mb-4 border-b border-default pb-3.5">
              <FileText class="w-5 h-5 text-primary" />
              <h2 class="text-base font-extrabold text-heading">Thiết lập chiến dịch gửi thông báo</h2>
            </div>

            <div class="space-y-4">
              <!-- Chọn Template có sẵn -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Sử dụng Mẫu Template có sẵn (Tùy chọn)</label>
                <select 
                  v-model="selectedTemplateId" 
                  @change="applyTemplate"
                  class="w-full px-3 lg-control text-sm font-semibold border-primary/45 bg-primary/5 text-primary"
                >
                  <option value="">-- Soạn thảo tự do (Không dùng template) --</option>
                  <option v-for="tpl in availableTemplates" :key="tpl.id" :value="tpl.id">
                    {{ tpl.name }}
                  </option>
                </select>
                <p class="text-[10px] text-muted mt-1 leading-normal">
                  Hệ thống tự động điền Tiêu đề, Nội dung, Loại sự kiện và Kênh tương ứng theo template đã thiết lập.
                </p>
              </div>

              <!-- Chọn Sự kiện thông báo -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Loại sự kiện thông báo</label>
                <select v-model="notificationForm.eventType" class="w-full px-3 lg-control text-sm">
                  <option v-for="evt in eventTypes" :key="evt.code" :value="evt.code">
                    {{ evt.name }}
                  </option>
                </select>
              </div>

              <!-- Chọn kênh gửi -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Kênh truyền tải tin nhắn</label>
                <div class="flex flex-wrap gap-4 mt-2">
                  <label class="flex items-center gap-2 cursor-pointer text-xs font-bold">
                    <input type="checkbox" value="Email" v-model="notificationForm.channels" class="rounded text-primary focus:ring-primary" />
                    <span class="flex items-center gap-1">
                      <Mail class="w-4 h-4 text-sky-500" />
                      Email (SendGrid)
                    </span>
                  </label>
                  <label class="flex items-center gap-2 cursor-pointer text-xs font-bold">
                    <input type="checkbox" value="SMS" v-model="notificationForm.channels" class="rounded text-primary focus:ring-primary" />
                    <span class="flex items-center gap-1">
                      <MessageSquare class="w-4 h-4 text-amber-500" />
                      SMS (Twilio)
                    </span>
                  </label>
                  <label class="flex items-center gap-2 cursor-pointer text-xs font-bold">
                    <input type="checkbox" value="Push" v-model="notificationForm.channels" class="rounded text-primary focus:ring-primary" />
                    <span class="flex items-center gap-1">
                      <Bell class="w-4 h-4 text-violet-500" />
                      Push Notification (SignalR)
                    </span>
                  </label>
                </div>
              </div>

              <!-- Tiêu đề (Email cần) -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Tiêu đề thông báo (Subject)</label>
                <input 
                  type="text"
                  v-model="notificationForm.title"
                  placeholder="Nhập tiêu đề thư gửi (Bắt buộc với Email)..."
                  class="w-full px-3 lg-control text-sm font-semibold"
                />
              </div>

              <!-- Nội dung tin nhắn (Body) -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Nội dung thông báo (Message Body)</label>
                <textarea
                  v-model="notificationForm.body"
                  rows="8"
                  placeholder="Nhập nội dung thông báo. Bạn có thể sử dụng các biến placeholder như {{student_name}}, {{student_id}}..."
                  class="w-full px-3 py-2.5 lg-control text-sm font-sans leading-relaxed"
                ></textarea>
                <div class="flex items-center justify-between mt-1 text-[10px] text-muted font-bold">
                  <span>Sử dụng Markdown/HTML cơ bản đối với Email.</span>
                  <span>{{ notificationForm.body.length }} ký tự</span>
                </div>
              </div>

              <!-- PHẠM VI NHẬN TIN -->
              <div class="lg-glass p-4 rounded-xl border border-default space-y-3.5">
                <div class="flex items-center gap-1.5 border-b border-default pb-2">
                  <Layers class="w-4 h-4 text-primary" />
                  <h3 class="text-xs font-extrabold text-heading uppercase">Cấu hình đối tượng & Phạm vi nhận tin</h3>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <!-- Chọn vai trò -->
                  <div>
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Vai trò người nhận</label>
                    <div class="flex flex-wrap gap-3 mt-1.5">
                      <label class="flex items-center gap-1.5 cursor-pointer text-xs font-bold">
                        <input type="checkbox" value="Student" v-model="notificationForm.targetRoles" class="rounded text-primary focus:ring-primary" />
                        Sinh viên
                      </label>
                      <label class="flex items-center gap-1.5 cursor-pointer text-xs font-bold">
                        <input type="checkbox" value="Teacher" v-model="notificationForm.targetRoles" class="rounded text-primary focus:ring-primary" />
                        Giảng viên
                      </label>
                      <label class="flex items-center gap-1.5 cursor-pointer text-xs font-bold">
                        <input type="checkbox" value="Staff" v-model="notificationForm.targetRoles" class="rounded text-primary focus:ring-primary" />
                        Nhân sự
                      </label>
                    </div>
                  </div>

                  <!-- Chọn phạm vi -->
                  <div>
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Phạm vi cơ sở</label>
                    <select v-model="notificationForm.scope" class="w-full px-3 lg-control text-xs">
                      <option value="all">Toàn hệ thống (Toàn trường)</option>
                      <option value="campus">Theo Cụm cơ sở (Campus)</option>
                      <option value="subcampus">Theo Cơ sở thành viên</option>
                      <option value="class">Theo Lớp học cụ thể</option>
                    </select>
                  </div>
                </div>

                <!-- Lọc phụ thuộc Scope -->
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t border-default/50" v-if="notificationForm.scope !== 'all'">
                  <div v-if="notificationForm.scope === 'campus' || notificationForm.scope === 'subcampus'">
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Chọn Cụm cơ sở</label>
                    <select v-model="notificationForm.selectedCampus" class="w-full px-3 lg-control text-xs">
                      <option v-for="c in campuses" :key="c.id" :value="c.id">{{ c.name }}</option>
                    </select>
                  </div>

                  <div v-if="notificationForm.scope === 'subcampus'">
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Chọn cơ sở thành viên</label>
                    <select v-model="notificationForm.selectedSubCampus" class="w-full px-3 lg-control text-xs">
                      <option v-for="s in subCampuses[notificationForm.selectedCampus]" :key="s.id" :value="s.id">
                        {{ s.name }}
                      </option>
                    </select>
                  </div>

                  <div v-if="notificationForm.scope === 'class'">
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Chọn Lớp học phần</label>
                    <select v-model="notificationForm.selectedClass" class="w-full px-3 lg-control text-xs">
                      <option v-for="c in classesMock" :key="c.id" :value="c.id">{{ c.name }}</option>
                    </select>
                  </div>
                </div>

                <!-- Ước lượng số lượng nhận -->
                <div class="p-2.5 rounded-lg bg-surface-card border border-default/50 flex items-center justify-between text-xs">
                  <div class="flex items-center gap-2 text-muted">
                    <Users class="w-4 h-4 text-primary" />
                    <span>Số lượng người nhận ước tính:</span>
                  </div>
                  <span class="font-extrabold text-primary text-sm">{{ estimatedRecipientCount.toLocaleString('vi-VN') }} người nhận</span>
                </div>
              </div>

              <!-- ĐIỀU KHIỂN THỜI GIAN GỬI -->
              <div class="lg-glass p-4 rounded-xl border border-default space-y-4">
                <div class="flex items-center justify-between border-b border-default pb-2">
                  <div class="flex items-center gap-1.5">
                    <Calendar class="w-4 h-4 text-primary" />
                    <h3 class="text-xs font-extrabold text-heading uppercase">Lịch phát sóng & Gửi tin</h3>
                  </div>

                  <label class="flex items-center gap-2 cursor-pointer text-xs font-bold">
                    <input type="checkbox" v-model="notificationForm.isScheduled" class="rounded text-primary focus:ring-primary" />
                    Lên lịch gửi trong tương lai
                  </label>
                </div>

                <!-- AI Smart Timing Suggestion Card -->
                <div class="p-3.5 rounded-xl bg-violet-500/5 border border-violet-500/20 space-y-2">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-1.5 text-xs font-bold text-violet-600 dark:text-violet-400">
                      <Sparkles class="w-4.5 h-4.5 animate-pulse" />
                      AI Smart Timing Recommendation
                    </div>
                    <button 
                      @click="toggleAISmartTiming"
                      class="px-2.5 py-1 rounded bg-violet-600 hover:bg-violet-700 text-white text-[10px] font-extrabold transition-all"
                    >
                      {{ useAISmartTiming ? 'Tắt gợi ý' : 'Áp dụng giờ AI' }}
                    </button>
                  </div>
                  <p class="text-xs leading-relaxed text-slate-700 dark:text-slate-300">
                    {{ aiRecommendationText }}
                  </p>
                </div>

                <!-- Input chọn lịch gửi -->
                <div class="grid grid-cols-2 gap-4" v-if="notificationForm.isScheduled">
                  <div>
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Ngày gửi</label>
                    <input type="date" v-model="notificationForm.scheduledDate" class="w-full px-3 lg-control text-xs" />
                  </div>
                  <div>
                    <label class="block text-[11px] font-bold text-label mb-1.5 uppercase">Giờ gửi</label>
                    <input type="time" v-model="notificationForm.scheduledTime" class="w-full px-3 lg-control text-xs" />
                  </div>
                </div>
              </div>

              <!-- Cảnh báo Bulk Send & Trùng lặp -->
              <div v-if="isBulkSend || showDeduplicationWarning" class="space-y-2">
                <!-- Cảnh báo Bulk Send -->
                <div v-if="isBulkSend" class="lg-alert lg-alert-info p-3 flex items-start gap-2.5">
                  <Info class="w-5 h-5 flex-shrink-0 text-sky-500 mt-0.5" />
                  <div class="text-xs">
                    <span class="font-bold text-sky-700 dark:text-sky-400">Cơ chế Bulk Send kích hoạt:</span>
                    Số lượng người nhận lớn hơn 500. Hệ thống sẽ tự động chia nhỏ chiến dịch này thành các đợt 100 tin/lần gửi để đảm bảo an toàn băng thông.
                  </div>
                </div>

                <!-- Cảnh báo trùng lặp -->
                <div v-if="showDeduplicationWarning" class="lg-alert lg-alert-warning p-3 flex items-start gap-2.5">
                  <AlertTriangle class="w-5 h-5 flex-shrink-0 text-amber-500 mt-0.5" />
                  <div class="text-xs">
                    <span class="font-bold text-amber-700 dark:text-amber-400">Cảnh báo chống trùng lặp (Deduplication):</span>
                    Hệ thống ghi nhận có một thông báo tương tự đã được gửi đi trong vòng 5 phút qua. Vui lòng kiểm tra lại để tránh làm phiền người dùng.
                  </div>
                </div>
              </div>

              <!-- Nút Action gửi/lưu -->
              <div class="flex items-center justify-end gap-3 pt-3 border-t border-default">
                <button 
                  @click="handleSaveDraft"
                  class="lg-btn-secondary text-xs px-4 py-2.5 font-bold"
                >
                  Lưu bản nháp
                </button>

                <button
                  @click="isPreviewOpen = true"
                  class="lg-btn-secondary text-xs px-4 py-2.5 font-bold flex items-center gap-1.5"
                >
                  <Eye class="w-4 h-4" />
                  Xem trước tin
                </button>

                <button
                  @click="handleSendNotification"
                  :disabled="!isFormValid"
                  class="lg-btn-primary text-xs px-5 py-2.5 font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <Send class="w-4 h-4" />
                  {{ notificationForm.isScheduled ? 'Lên lịch gửi' : 'Gửi ngay lập tức' }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Cột Phải: Theo dõi & Báo cáo kết quả trực tuyến (5/12) -->
        <div class="lg:col-span-5 space-y-6">
          <!-- Recent Campaigns & Queue -->
          <div class="lg-glass-soft lg-card lg-density-normal">
            <div class="flex items-center justify-between mb-4 border-b border-default pb-3.5">
              <div class="flex items-center gap-2">
                <Clock class="w-5 h-5 text-primary" />
                <h2 class="text-base font-extrabold text-heading">Hàng đợi phát sóng gần đây</h2>
              </div>
            </div>

            <div class="space-y-4">
              <div 
                v-for="camp in recentCampaigns" 
                :key="camp.id"
                class="p-4 rounded-xl border border-default bg-surface-card hover:bg-surface-card-hover transition-colors space-y-2.5"
              >
                <div class="flex items-start justify-between gap-2">
                  <div class="space-y-1">
                    <span class="text-[9px] font-bold text-muted tracking-wider uppercase bg-surface-input px-2 py-0.5 rounded border border-default">
                      {{ camp.id }}
                    </span>
                    <h3 class="text-xs font-extrabold text-heading leading-normal">
                      {{ camp.title }}
                    </h3>
                  </div>

                  <span 
                    class="lg-badge"
                    :class="{
                      'lg-badge-success': camp.status === 'Sent',
                      'lg-badge-warning': camp.status === 'Scheduled',
                      'lg-badge-danger': camp.status === 'Cancelled',
                      'lg-badge-info': camp.status === 'Queued'
                    }"
                  >
                    {{ camp.status === 'Sent' ? 'Đã gửi' : camp.status === 'Scheduled' ? 'Đã lên lịch' : camp.status === 'Cancelled' ? 'Đã hủy' : 'Đang chờ' }}
                  </span>
                </div>

                <div class="text-[11px] text-muted space-y-1">
                  <div class="flex items-center gap-1.5">
                    <span class="font-bold">Đối tượng:</span> 
                    <span>{{ camp.scope }} &middot; {{ camp.targetRoles.join(', ') }}</span>
                  </div>
                  <div class="flex items-center gap-1.5" v-if="camp.scheduledAt">
                    <span class="font-bold">Giờ phát:</span> 
                    <span class="text-primary font-semibold">{{ camp.scheduledAt }}</span>
                  </div>
                  <div class="flex items-center gap-1.5">
                    <span class="font-bold">Kênh:</span> 
                    <span class="flex items-center gap-1.5">
                      <span v-for="chan in camp.channels" :key="chan" class="inline-flex items-center gap-0.5 bg-surface-input px-1.5 py-0.5 rounded text-[10px] font-bold border border-default">
                        <Mail v-if="chan === 'Email'" class="w-3 h-3 text-sky-500" />
                        <MessageSquare v-else-if="chan === 'SMS'" class="w-3 h-3 text-amber-500" />
                        <Bell v-else class="w-3 h-3 text-violet-500" />
                        {{ chan }}
                      </span>
                    </span>
                  </div>
                </div>

                <!-- Báo cáo kết quả gửi -->
                <div class="pt-2.5 border-t border-default/50 space-y-2" v-if="camp.status === 'Sent'">
                  <div class="flex items-center justify-between text-[11px] font-bold">
                    <span class="text-muted">Đã chuyển đổi thành công:</span>
                    <span class="text-emerald-500">{{ camp.deliveredCount }} / {{ camp.sentCount }} tin</span>
                  </div>
                  <div class="w-full bg-slate-200 dark:bg-slate-800 h-1.5 rounded-full overflow-hidden">
                    <div class="bg-emerald-500 h-full" :style="{ width: `${(camp.deliveredCount / camp.sentCount) * 100}%` }"></div>
                  </div>
                </div>

                <!-- Nút hủy gửi nếu là Scheduled -->
                <div class="pt-2 flex justify-end" v-if="camp.status === 'Scheduled'">
                  <button 
                    @click="cancelCampaign(camp)"
                    class="lg-btn-secondary text-[10px] px-2.5 py-1.5 text-rose-500 border-rose-200 hover:bg-rose-500/10 hover:text-rose-600 font-bold flex items-center gap-1"
                  >
                    <Trash2 class="w-3 h-3" />
                    Hủy lịch phát sóng
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- Preview Modal (Giả lập kết xuất) -->
    <div 
      v-if="isPreviewOpen"
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative flex flex-col max-h-[90vh]">
        <!-- Close Button -->
        <button 
          @click="isPreviewOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-center gap-3 mb-4 border-b border-default pb-3.5">
          <div class="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
            <Eye class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Xem trước kết xuất thông báo</h3>
            <p class="text-xs text-muted mt-0.5">Kiểm tra hiển thị giả lập trên từng thiết bị của người nhận tin.</p>
          </div>
        </div>

        <!-- Tab Selector Kênh Preview -->
        <div class="flex gap-2 border-b border-default mb-4 pb-2 text-xs font-bold">
          <button 
            v-for="chan in notificationForm.channels"
            :key="chan"
            @click="previewActiveChannel = chan"
            class="px-4 py-2 border-b-2 transition-colors flex items-center gap-1.5"
            :class="previewActiveChannel === chan ? 'border-primary text-primary font-extrabold' : 'border-transparent text-muted hover:text-heading'"
          >
            <Mail v-if="chan === 'Email'" class="w-4 h-4 text-sky-500" />
            <MessageSquare v-else-if="chan === 'SMS'" class="w-4 h-4 text-amber-500" />
            <Bell v-else class="w-4 h-4 text-violet-500" />
            {{ chan }}
          </button>
        </div>

        <!-- Body Preview Content -->
        <div class="flex-1 overflow-y-auto min-h-[300px] flex items-center justify-center bg-slate-50 dark:bg-slate-900/50 rounded-lg p-4">
          
          <!-- Case Email -->
          <div v-if="previewActiveChannel === 'Email'" class="w-full bg-white dark:bg-slate-950 rounded-xl border border-default p-4 shadow-sm text-xs text-slate-800 dark:text-slate-200">
            <div class="border-b border-default pb-3 mb-4 space-y-1">
              <div><span class="font-bold text-muted mr-1">Tiêu đề:</span> <span class="font-bold text-primary">{{ notificationForm.title || '(Chưa nhập tiêu đề)' }}</span></div>
              <div><span class="font-bold text-muted mr-1">Người gửi:</span> <span class="font-bold text-heading">LMS Academic Portal &lt;no-reply@fpt.edu.vn&gt;</span></div>
            </div>
            <div class="prose dark:prose-invert max-w-full text-slate-800 dark:text-slate-100 leading-relaxed whitespace-pre-line">
              {{ renderPreviewContent(notificationForm.body) || '(Trống nội dung thông báo)' }}
            </div>
          </div>

          <!-- Case SMS / Push Mobile View -->
          <div v-else class="w-[240px] h-[380px] rounded-[30px] border-[5px] border-slate-700 bg-slate-950 relative flex flex-col p-2 overflow-hidden shadow-xl">
            <!-- Notch -->
            <div class="absolute top-0 left-1/2 -translate-x-1/2 w-20 h-3.5 bg-slate-700 rounded-b-lg z-20 flex items-center justify-center"></div>

            <!-- Screen content -->
            <div class="flex-1 rounded-[22px] bg-slate-100 dark:bg-slate-950 p-2 flex flex-col pt-5">
              <div class="flex items-center justify-between text-[8px] text-muted font-bold mb-3 px-1">
                <span>12:25 PM</span>
                <span class="w-3.5 h-2 rounded bg-slate-400 dark:bg-slate-700"></span>
              </div>

              <!-- Push UI mockup -->
              <div v-if="previewActiveChannel === 'Push'" class="w-full p-2 rounded-lg bg-white/95 dark:bg-slate-900/95 border border-default shadow space-y-1 text-left">
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-1">
                    <span class="w-3 h-3 rounded bg-primary flex items-center justify-center text-[7px] text-white font-bold">LMS</span>
                    <span class="text-[9px] font-extrabold text-heading">LMS Portal</span>
                  </div>
                  <span class="text-[7px] text-muted">Bây giờ</span>
                </div>
                <div class="text-[9px] leading-normal text-heading font-medium truncate">
                  {{ notificationForm.title || 'Thông báo mới' }}
                </div>
                <div class="text-[8px] leading-normal text-muted line-clamp-3">
                  {{ renderPreviewContent(notificationForm.body) || '(Trống nội dung)' }}
                </div>
              </div>

              <!-- SMS Chat UI mockup -->
              <div v-else-if="previewActiveChannel === 'SMS'" class="flex-1 flex flex-col justify-end space-y-2 text-left">
                <div class="text-center text-[7px] text-muted py-1 uppercase font-bold">Hôm nay 12:25 PM</div>
                
                <div class="self-start max-w-[85%] bg-slate-200 dark:bg-slate-800 text-slate-800 dark:text-slate-100 p-2 rounded-xl rounded-tl-none text-[8px] leading-relaxed font-semibold whitespace-pre-line">
                  {{ renderPreviewContent(notificationForm.body) || '(Trống nội dung SMS)' }}
                </div>
                
                <!-- Chat input -->
                <div class="mt-2 pt-1 border-t border-default/45 flex items-center gap-1">
                  <div class="flex-1 bg-white dark:bg-slate-900 rounded-full px-2 py-0.5 text-[7px] text-muted border">iMessage / Tin nhắn</div>
                  <span class="w-4 h-4 rounded-full bg-emerald-500 flex items-center justify-center text-white"><Check class="w-2.5 h-2.5" /></span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="mt-4 pt-3 border-t border-default flex justify-end">
          <button 
            @click="isPreviewOpen = false"
            class="lg-btn-primary px-4 py-2 text-xs font-bold"
          >
            Đóng xem trước
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Mobile Phone and Textarea styles */
textarea {
  resize: vertical;
}
input[type="checkbox"] {
  border: 1px solid var(--border-input);
  background-color: var(--surface-input);
}
</style>
