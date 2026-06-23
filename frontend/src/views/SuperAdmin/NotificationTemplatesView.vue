<script setup>
/**
 * NotificationTemplatesView.vue - Super Admin
 * Giao diện quản lý tập trung toàn bộ các mẫu tin nhắn (Notification Templates) của hệ thống LMS.
 * Thuộc module M16 – Notification Hub, tích hợp trình soạn thảo (Drawer), Placeholder Helper,
 * Real-time Preview giả lập thiết bị di động/HTML email, Test Send Modal, và Fallback Indicator.
 */
import { ref, computed } from 'vue'
import {
  Mail,
  MessageSquare,
  Bell,
  Plus,
  Filter,
  Pencil,
  Eye,
  Send,
  X,
  Save,
  AlertTriangle,
  CheckCircle,
  Info,
  History,
  Search,
  Globe,
  HelpCircle,
  Activity,
  FileText,
  RotateCcw,
  Check
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách các loại sự kiện (Event Types)
const eventTypes = ref([
  { code: 'GRADE_PUBLISHED', name: 'Công bộ điểm học kỳ', desc: 'Gửi khi giảng viên công bố điểm trung bình môn học.' },
  { code: 'ATTENDANCE_WARNING', name: 'Cảnh báo vắng học', desc: 'Gửi khi sinh viên chạm ngưỡng quỹ vắng tối đa.' },
  { code: 'PAYMENT_DUE', name: 'Nhắc đóng học phí', desc: 'Gửi trước kỳ hạn đóng học phí học kỳ mới.' },
  { code: 'EXAM_SCHEDULED', name: 'Thông báo lịch thi', desc: 'Gửi khi phòng khảo thí chốt ca thi và lịch thi.' }
])

// Danh sách các kênh (Channels)
const channels = ref(['Email', 'SMS', 'Push'])

// Danh sách biến placeholder hỗ trợ (Placeholder Helper) theo từng sự kiện
const placeholdersByEvent = {
  GRADE_PUBLISHED: [
    { code: '{{student_name}}', name: 'Tên sinh viên' },
    { code: '{{student_id}}', name: 'Mã sinh viên' },
    { code: '{{course_name}}', name: 'Tên môn học' },
    { code: '{{semester}}', name: 'Học kỳ' },
    { code: '{{grade}}', name: 'Điểm trung bình' },
    { code: '{{status}}', name: 'Đạt/Rớt (Pass/Fail)' }
  ],
  ATTENDANCE_WARNING: [
    { code: '{{student_name}}', name: 'Tên sinh viên' },
    { code: '{{student_id}}', name: 'Mã sinh viên' },
    { code: '{{course_name}}', name: 'Tên môn học' },
    { code: '{{absent_slots}}', name: 'Số buổi đã vắng' },
    { code: '{{max_slots}}', name: 'Hạn mức vắng tối đa' }
  ],
  PAYMENT_DUE: [
    { code: '{{student_name}}', name: 'Tên sinh viên' },
    { code: '{{student_id}}', name: 'Mã sinh viên' },
    { code: '{{semester}}', name: 'Học kỳ đóng phí' },
    { code: '{{amount}}', name: 'Số tiền học phí' },
    { code: '{{due_date}}', name: 'Hạn chót thanh toán' }
  ],
  EXAM_SCHEDULED: [
    { code: '{{student_name}}', name: 'Tên sinh viên' },
    { code: '{{course_name}}', name: 'Tên môn thi' },
    { code: '{{exam_date}}', name: 'Ngày thi' },
    { code: '{{exam_time}}', name: 'Ca thi (Slot)' },
    { code: '{{room_name}}', name: 'Phòng thi' }
  ]
}

// Biến giả lập phục vụ render Real-time Preview trực quan
const previewVariables = {
  student_name: 'Nguyễn Văn An',
  student_id: 'HE170001',
  course_name: 'Thiết kế & Kiến trúc phần mềm',
  semester: 'Spring 2026',
  grade: '8.5',
  status: 'Pass',
  absent_slots: '5',
  max_slots: '6',
  amount: '4,800,000',
  due_date: '30/06/2026',
  exam_date: '28/06/2026',
  exam_time: 'Slot 2 (09:45 - 11:15)',
  room_name: 'Phòng Lab 302 - Campus Hòa Lạc'
}

// Danh sách Templates hiện tại
const templates = ref([
  {
    id: 1,
    eventType: 'GRADE_PUBLISHED',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Công bố kết quả môn học {{course_name}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #2563eb;">Thông báo Kết Quả Học Tập</h2>\n  <p>Xin chào <strong>{{student_name}}</strong> ({{student_id}}),</p>\n  <p>Điểm số chính thức của môn học <strong>{{course_name}}</strong> trong học kỳ <strong>{{semester}}</strong> đã được công bố.</p>\n  <table style="width: 100%; border-collapse: collapse; margin-top: 15px;">\n    <tr style="background-color: #f8fafc;">\n      <td style="padding: 10px; border: 1px solid #e2e8f0;">Điểm trung bình</td>\n      <td style="padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; color: #16a34a;">{{grade}}</td>\n    </tr>\n    <tr>\n      <td style="padding: 10px; border: 1px solid #e2e8f0;">Trạng thái học vụ</td>\n      <td style="padding: 10px; border: 1px solid #e2e8f0; font-weight: bold;">{{status}}</td>\n    </tr>\n  </table>\n  <p style="margin-top: 20px; font-size: 13px; color: #64748b;">Vui lòng truy cập LMS để xem chi tiết bảng điểm thành phần.</p>\n</div>',
    status: 'Active',
    updatedAt: '2026-06-20 09:30:00'
  },
  {
    id: 2,
    eventType: 'GRADE_PUBLISHED',
    channel: 'Push',
    bodyTemplate: 'Điểm số môn {{course_name}} của bạn đã được công bố. Kết quả: {{grade}} ({{status}}). Xem chi tiết trên LMS ngay!',
    status: 'Active',
    updatedAt: '2026-06-20 09:32:00'
  },
  {
    id: 3,
    eventType: 'ATTENDANCE_WARNING',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Cảnh báo chuyên cần môn {{course_name}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #fca5a5; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #dc2626;">Cảnh Báo Vắng Mặt Vượt Giới Hạn</h2>\n  <p>Thân gửi <strong>{{student_name}}</strong>,</p>\n  <p>Hệ thống ghi nhận bạn đã vắng <strong>{{absent_slots}} buổi</strong> trên tổng số <strong>{{max_slots}} buổi học</strong> cho phép của môn <strong>{{course_name}}</strong>.</p>\n  <p style="color: #dc2626; font-weight: bold;">Cảnh báo: Nếu bạn vắng quá {{max_slots}} buổi học, bạn sẽ bị cấm thi môn học này theo quy chế đào tạo của nhà trường.</p>\n  <p>Vui lòng sắp xếp tham gia các buổi học tiếp theo đầy đủ để tránh ảnh hưởng đến kết quả học tập.</p>\n</div>',
    status: 'Active',
    updatedAt: '2026-06-18 15:45:00'
  },
  {
    id: 4,
    eventType: 'ATTENDANCE_WARNING',
    channel: 'SMS',
    bodyTemplate: 'LMS Attendance Warning: Sinh vien {{student_name}} da vang {{absent_slots}}/{{max_slots}} buoi mon {{course_name}}. Vui long di hoc day du de tranh bi cam thi.',
    status: 'Active',
    updatedAt: '2026-06-18 15:46:00'
  },
  {
    id: 5,
    eventType: 'PAYMENT_DUE',
    channel: 'Email',
    subjectTemplate: 'LMS Academic: Nhắc nợ học phí học kỳ {{semester}}',
    bodyTemplate: '<div style="font-family: sans-serif; padding: 20px; border: 1px solid #fde68a; border-radius: 12px; max-width: 600px;">\n  <h2 style="color: #d97706;">Thông Báo Học Phí Học Kỳ {{semester}}</h2>\n  <p>Xin chào <strong>{{student_name}}</strong>,</p>\n  <p>Hệ thống gửi thông báo nhắc bạn hoàn thành nghĩa vụ học phí cho học kỳ <strong>{{semester}}</strong>.</p>\n  <ul>\n    <li>Số tiền cần thanh toán: <strong>{{amount}} VNĐ</strong></li>\n    <li>Hạn cuối thanh toán: <strong style="color: #dc2626;">{{due_date}}</strong></li>\n  </ul>\n  <p>Vui lòng nộp học phí đúng kỳ hạn quy định để hệ thống tự động đăng ký và kích hoạt tài khoản học tập.</p>\n</div>',
    status: 'Draft',
    updatedAt: '2026-06-21 11:20:00'
  },
  {
    id: 6,
    eventType: 'PAYMENT_DUE',
    channel: 'SMS',
    bodyTemplate: 'LMS Nhac Hoc Phi: SV {{student_name}} ({{student_id}}) nop hoc phi ky {{semester}} truoc {{due_date}}. So tien: {{amount}} VND. Chi tiet tai ung dung LMS.',
    status: 'Draft',
    updatedAt: '2026-06-21 11:22:00'
  }
])

// Nhật ký hoạt động & kết xuất lỗi (Audit Logs)
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-22 10:15:30',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Cập nhật template',
    details: 'Cập nhật nội dung template Email sự kiện ATTENDANCE_WARNING',
    type: 'info'
  },
  {
    id: 2,
    time: '2026-06-21 14:05:12',
    actor: 'System (Handlebars Engine)',
    action: 'Lỗi render template',
    details: 'Lỗi biên dịch template SMS của sự kiện GRADE_PUBLISHED: Thiếu thẻ đóng ngoặc "}}"',
    type: 'error'
  },
  {
    id: 3,
    time: '2026-06-20 09:32:00',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Tạo template',
    details: 'Khởi tạo thành công template Push cho sự kiện GRADE_PUBLISHED',
    type: 'info'
  }
])

// --- State Bộ lọc ---
const searchKeyword = ref('')
const filterEvent = ref('all')
const filterChannel = ref('all')
const filterStatus = ref('all')

const filteredTemplates = computed(() => {
  return templates.value.filter(tpl => {
    const matchEvent = filterEvent.value === 'all' || tpl.eventType === filterEvent.value
    const matchChannel = filterChannel.value === 'all' || tpl.channel === filterChannel.value
    const matchStatus = filterStatus.value === 'all' || tpl.status === filterStatus.value

    const textToSearch = `${tpl.eventType} ${tpl.channel} ${tpl.subjectTemplate || ''} ${tpl.bodyTemplate}`.toLowerCase()
    const matchKeyword = !searchKeyword.value || textToSearch.includes(searchKeyword.value.toLowerCase())

    return matchEvent && matchChannel && matchStatus && matchKeyword
  })
})

const resetFilters = () => {
  searchKeyword.value = ''
  filterEvent.value = 'all'
  filterChannel.value = 'all'
  filterStatus.value = 'all'
}

// --- Thống kê KPI ---
const totalActiveTemplates = computed(() => templates.value.filter(t => t.status === 'Active').length)
const totalDraftTemplates = computed(() => templates.value.filter(t => t.status === 'Draft').length)
const totalSentTodayCount = ref(1485)

// Cơ chế quét các template bị thiếu (Missing template channel cho từng Event Type)
const missingTemplates = computed(() => {
  const missing = []
  eventTypes.value.forEach(evt => {
    channels.value.forEach(chan => {
      const exists = templates.value.some(t => t.eventType === evt.code && t.channel === chan)
      if (!exists) {
        missing.push({
          eventType: evt.code,
          eventName: evt.name,
          channel: chan
        })
      }
    })
  })
  return missing
})

const totalMissingTemplatesCount = computed(() => missingTemplates.value.length)

// --- State Modals, Drawer & Form ---
const isDrawerOpen = ref(false)
const isTestModalOpen = ref(false)
const drawerMode = ref('create') // 'create' | 'edit'
const activeTab = ref('editor') // 'editor' | 'preview'

const currentTemplate = ref({
  id: null,
  eventType: '',
  channel: 'Email',
  subjectTemplate: '',
  bodyTemplate: '',
  status: 'Draft'
})

const testTargetTemplate = ref(null)
const testReceiver = ref('')
const isSendingTest = ref(false)

// --- Toast States ---
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

// --- Trình soạn thảo & Helper Placeholder ---
const lastFocusedInput = ref('body') // 'subject' | 'body'

const activePlaceholders = computed(() => {
  if (!currentTemplate.value.eventType) return []
  return placeholdersByEvent[currentTemplate.value.eventType] || []
})

// Chèn biến placeholder vào khung văn bản soạn thảo
const insertPlaceholder = (tag) => {
  if (lastFocusedInput.value === 'subject' && currentTemplate.value.channel === 'Email') {
    const text = currentTemplate.value.subjectTemplate || ''
    currentTemplate.value.subjectTemplate = text + tag
  } else {
    const text = currentTemplate.value.bodyTemplate || ''
    currentTemplate.value.bodyTemplate = text + tag
  }
  triggerToast(`Đã chèn biến ${tag} vào vùng soạn thảo.`, 'info')
}

// Kiểm tra lỗi cú pháp Handlebars thô (xác định có ngoặc {{ nhưng thiếu }})
const syntaxErrorText = computed(() => {
  const checkBrackets = (text) => {
    let openCount = (text.match(/\{\{/g) || []).length
    let closeCount = (text.match(/\}\}/g) || []).length
    if (openCount !== closeCount) {
      return 'Cảnh báo: Cú pháp biến {{...}} đang thiếu dấu đóng ngoặc hoặc mở ngoặc.'
    }
    return ''
  }

  const bodyErr = checkBrackets(currentTemplate.value.bodyTemplate || '')
  if (bodyErr) return bodyErr

  if (currentTemplate.value.channel === 'Email') {
    const subErr = checkBrackets(currentTemplate.value.subjectTemplate || '')
    if (subErr) return subErr
  }
  return ''
})

// Hàm render preview thay thế các biến Handlebars
const renderTemplate = (tplText) => {
  if (!tplText) return ''
  let result = tplText
  Object.keys(previewVariables).forEach(key => {
    const regex = new RegExp(`\\{\\{${key}\\}\\}`, 'g')
    result = result.replace(regex, previewVariables[key])
  })
  return result
}

const renderedPreviewBody = computed(() => {
  return renderTemplate(currentTemplate.value.bodyTemplate)
})

const renderedPreviewSubject = computed(() => {
  return renderTemplate(currentTemplate.value.subjectTemplate)
})

// --- Quy tắc Nghiệp vụ Validation ---
const isFormValid = computed(() => {
  if (!currentTemplate.value.eventType || !currentTemplate.value.channel) return false
  if (currentTemplate.value.channel === 'Email' && !currentTemplate.value.subjectTemplate) return false
  if (!currentTemplate.value.bodyTemplate) return false
  if (syntaxErrorText.value) return false
  return true
})

// Kiểm tra cặp EventType + Channel đã tồn tại chưa để tránh trùng lặp
const isDuplicateConfig = computed(() => {
  return templates.value.some(t => {
    if (drawerMode.value === 'create') {
      return t.eventType === currentTemplate.value.eventType && t.channel === currentTemplate.value.channel
    } else {
      return t.id !== currentTemplate.value.id && t.eventType === currentTemplate.value.eventType && t.channel === currentTemplate.value.channel
    }
  })
})

// --- Handlers ---

const openCreateDrawer = () => {
  drawerMode.value = 'create'
  activeTab.value = 'editor'
  currentTemplate.value = {
    id: null,
    eventType: eventTypes.value[0]?.code || '',
    channel: 'Email',
    subjectTemplate: '',
    bodyTemplate: '',
    status: 'Draft'
  }
  isDrawerOpen.value = true
}

const openEditDrawer = (tpl) => {
  drawerMode.value = 'edit'
  activeTab.value = 'editor'
  currentTemplate.value = JSON.parse(JSON.stringify(tpl))
  isDrawerOpen.value = true
}

const handleSaveTemplate = () => {
  if (!isFormValid.value) return
  if (isDuplicateConfig.value) {
    triggerToast('Lỗi: Cấu hình template cho kênh và sự kiện này đã tồn tại!', 'error')
    return
  }

  const timeString = new Date().toLocaleString('vi-VN')

  if (drawerMode.value === 'create') {
    const newId = templates.value.length ? Math.max(...templates.value.map(t => t.id)) + 1 : 1
    const newTpl = {
      ...currentTemplate.value,
      id: newId,
      updatedAt: timeString
    }
    templates.value.push(newTpl)

    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Tạo template',
      details: `Tạo mới template ${newTpl.channel} cho sự kiện ${newTpl.eventType} với trạng thái ${newTpl.status}`,
      type: 'info'
    })

    triggerToast(`Đã khởi tạo thành công template ${newTpl.channel} cho sự kiện ${newTpl.eventType}.`, 'success')
  } else {
    const index = templates.value.findIndex(t => t.id === currentTemplate.value.id)
    if (index !== -1) {
      templates.value[index] = {
        ...currentTemplate.value,
        updatedAt: timeString
      }

      auditLogs.value.unshift({
        id: auditLogs.value.length + 1,
        time: timeString,
        actor: 'Super Admin (admin@fpt.edu.vn)',
        action: 'Cập nhật template',
        details: `Cập nhật nội dung template ${currentTemplate.value.channel} của sự kiện ${currentTemplate.value.eventType}`,
        type: 'info'
      })

      triggerToast(`Đã lưu thay đổi cho template thành công.`, 'success')
    }
  }

  isDrawerOpen.value = false
}

const openTestModal = (tpl) => {
  testTargetTemplate.value = tpl
  testReceiver.value = tpl.channel === 'Email' ? 'test_student@fpt.edu.vn' : tpl.channel === 'SMS' ? '0912345678' : 'HE170001'
  isTestModalOpen.value = true
}

const handleSendTest = () => {
  if (!testReceiver.value.trim()) {
    triggerToast('Vui lòng nhập thông tin người nhận thử nghiệm!', 'error')
    return
  }

  isSendingTest.value = true

  setTimeout(() => {
    isSendingTest.value = false
    isTestModalOpen.value = false

    const timeString = new Date().toLocaleString('vi-VN')

    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Gửi thử nghiệm',
      details: `Gửi thử thành công template ${testTargetTemplate.value.channel} [Sự kiện: ${testTargetTemplate.value.eventType}] tới: ${testReceiver.value}`,
      type: 'info'
    })

    triggerToast(`Gửi thử nghiệm thành công tới ${testReceiver.value}!`, 'success')
  }, 1500)
}

const getEventName = (code) => {
  const e = eventTypes.value.find(evt => evt.code === code)
  return e ? e.name : code
}

const getEventDesc = (code) => {
  const e = eventTypes.value.find(evt => evt.code === code)
  return e ? e.desc : ''
}
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
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Bell class="w-8 h-8 text-primary" />
            Template Thông Báo
          </h1>
          <p class="text-sm text-muted mt-1">
            Quản lý tập trung toàn bộ các mẫu tin nhắn phục vụ hệ thống Notification Hub. Thiết lập nội dung cho Email, SMS và Push.
          </p>
        </div>

        <div>
          <button
            @click="openCreateDrawer"
            class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2"
          >
            <Plus class="w-4.5 h-4.5" />
            Tạo Mẫu Mới
          </button>
        </div>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- KPI 1 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đang hoạt động</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalActiveTemplates }} mẫu</div>
          </div>
        </div>

        <!-- KPI 2 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <FileText class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Bản nháp (Draft)</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalDraftTemplates }} bản nháp</div>
          </div>
        </div>

        <!-- KPI 3 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4" :class="totalMissingTemplatesCount > 0 ? 'border-rose-300 dark:border-rose-900/50' : ''">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center" :class="totalMissingTemplatesCount > 0 ? 'bg-rose-500/15 text-rose-500 animate-pulse' : 'bg-slate-500/10 text-slate-500'">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Kênh thiếu template</div>
            <div class="text-2xl font-bold mt-0.5 text-heading" :class="totalMissingTemplatesCount > 0 ? 'text-rose-500 font-extrabold' : ''">
              {{ totalMissingTemplatesCount }} kênh
            </div>
          </div>
        </div>

        <!-- KPI 4 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Activity class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã gửi hôm nay</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalSentTodayCount }} tin</div>
          </div>
        </div>
      </div>

      <!-- Fallback Alert Indicator (Báo động các kênh còn thiếu template) -->
      <div v-if="totalMissingTemplatesCount > 0" class="lg-alert lg-alert-warning mb-6">
        <div class="flex items-start gap-2.5">
          <AlertTriangle class="w-5.5 h-5.5 flex-shrink-0 mt-0.5" />
          <div class="space-y-1">
            <h4 class="font-extrabold text-sm text-amber-700 dark:text-amber-400">Cơ chế Fallback đang kích hoạt cho các kênh sau:</h4>
            <p class="text-xs leading-relaxed text-slate-700 dark:text-slate-300">
              Có {{ totalMissingTemplatesCount }} sự kiện chưa cấu hình đầy đủ kênh gửi. Hệ thống sẽ tự động dùng văn bản thô thuần túy (plain text) mặc định để dự phòng:
            </p>
            <div class="flex flex-wrap gap-2 mt-2">
              <span 
                v-for="(item, idx) in missingTemplates" 
                :key="idx"
                class="px-2 py-1 rounded bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-200 dark:border-rose-900/50 text-[10px] font-bold"
              >
                {{ item.eventName }} &rarr; Kênh: {{ item.channel }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Tìm kiếm & Lọc template</h3>
          </div>
          <button 
            @click="resetFilters" 
            class="text-xs text-link font-bold flex items-center gap-1 hover:underline"
          >
            <RotateCcw class="w-3.5 h-3.5" />
            Reset bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <!-- Tìm kiếm từ khóa -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Từ khóa nội dung</label>
            <div class="relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input 
                type="text" 
                v-model="searchKeyword" 
                placeholder="Tìm tiêu đề, nội dung..."
                class="w-full pl-9 pr-3 lg-control text-sm"
              />
            </div>
          </div>

          <!-- Lọc Sự kiện -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Loại sự kiện</label>
            <select v-model="filterEvent" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả sự kiện</option>
              <option v-for="evt in eventTypes" :key="evt.code" :value="evt.code">
                {{ evt.name }}
              </option>
            </select>
          </div>

          <!-- Lọc Kênh gửi -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Kênh gửi (Channel)</label>
            <select v-model="filterChannel" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả kênh</option>
              <option v-for="chan in channels" :key="chan" :value="chan">{{ chan }}</option>
            </select>
          </div>

          <!-- Lọc Trạng thái -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Trạng thái</label>
            <select v-model="filterStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Active">Đang hoạt động (Active)</option>
              <option value="Draft">Bản nháp (Draft)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bảng Danh sách Template (Template Table) -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Sự kiện kích hoạt</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Kênh gửi</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Mẫu tiêu đề & Nội dung tóm tắt</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Cập nhật lúc</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredTemplates.length === 0">
              <td colspan="6" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <Bell class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy template nào phù hợp với bộ lọc.</span>
                </div>
              </td>
            </tr>

            <tr v-for="tpl in filteredTemplates" :key="tpl.id" class="transition-colors">
              <!-- Loại sự kiện -->
              <td class="px-4 py-4 min-w-[200px]">
                <div class="font-extrabold text-heading text-sm">{{ getEventName(tpl.eventType) }}</div>
                <div class="text-[10px] text-primary font-bold tracking-wider mt-0.5">{{ tpl.eventType }}</div>
                <p class="text-[11px] text-muted mt-1 leading-normal max-w-[250px]">{{ getEventDesc(tpl.eventType) }}</p>
              </td>

              <!-- Kênh gửi -->
              <td class="px-4 py-4 text-center">
                <div class="flex flex-col items-center justify-center gap-1.5">
                  <span 
                    class="w-8 h-8 rounded-full flex items-center justify-center border"
                    :class="{
                      'bg-sky-500/10 text-sky-500 border-sky-300': tpl.channel === 'Email',
                      'bg-amber-500/10 text-amber-500 border-amber-300': tpl.channel === 'SMS',
                      'bg-violet-500/10 text-violet-500 border-violet-300': tpl.channel === 'Push'
                    }"
                  >
                    <Mail v-if="tpl.channel === 'Email'" class="w-4 h-4" />
                    <MessageSquare v-else-if="tpl.channel === 'SMS'" class="w-4 h-4" />
                    <Bell v-else class="w-4 h-4" />
                  </span>
                  <span class="text-xs font-bold">{{ tpl.channel }}</span>
                </div>
              </td>

              <!-- Nội dung mẫu tóm tắt -->
              <td class="px-4 py-4 max-w-md">
                <div class="space-y-1">
                  <!-- Nếu là Email thì có tiêu đề thư -->
                  <div v-if="tpl.channel === 'Email'" class="text-xs font-bold text-heading truncate">
                    <span class="text-muted font-normal mr-1">Tiêu đề:</span>
                    {{ tpl.subjectTemplate }}
                  </div>
                  <!-- Nội dung body -->
                  <div class="text-xs text-muted truncate leading-relaxed">
                    <span class="text-muted font-normal mr-1">Nội dung:</span>
                    {{ tpl.bodyTemplate.replace(/<[^>]*>/g, '') }}
                  </div>
                </div>
              </td>

              <!-- Cập nhật -->
              <td class="px-4 py-4 text-center text-xs text-muted">
                {{ tpl.updatedAt }}
              </td>

              <!-- Trạng thái -->
              <td class="px-4 py-4 text-center">
                <span 
                  class="lg-badge"
                  :class="tpl.status === 'Active' ? 'lg-badge-success' : tpl.status === 'Draft' ? 'lg-badge-warning' : 'lg-badge-danger'"
                >
                  {{ tpl.status === 'Active' ? 'Đang chạy' : 'Bản nháp' }}
                </span>
              </td>

              <!-- Hành động -->
              <td class="px-4 py-4 text-center">
                <div class="flex items-center justify-center gap-2">
                  <!-- Sửa -->
                  <button
                    @click="openEditDrawer(tpl)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                    title="Chỉnh sửa template"
                  >
                    <Pencil class="w-3.5 h-3.5" />
                    Sửa
                  </button>

                  <!-- Gửi thử -->
                  <button
                    @click="openTestModal(tpl)"
                    class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"
                    title="Gửi thử nghiệm"
                  >
                    <Send class="w-3.5 h-3.5" />
                    Test gửi
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Trình Soạn Thảo Drawer (Template Editor Drawer) -->
      <div 
        v-if="isDrawerOpen" 
        class="fixed inset-0 z-[100] flex justify-end bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
      >
        <!-- Khung Drawer trượt từ bên phải -->
        <div class="w-full lg:max-w-5xl xl:max-w-6xl h-screen bg-surface-modal lg-glass-strong border-l border-default shadow-2xl flex flex-col animate-in slide-in-from-right duration-300">
          
          <!-- Drawer Header -->
          <div class="p-5 border-b border-default flex items-center justify-between">
            <div>
              <h2 class="text-lg font-extrabold text-heading flex items-center gap-2.5">
                <Pencil class="w-5.5 h-5.5 text-primary" />
                {{ drawerMode === 'create' ? 'Thiết lập Template thông báo mới' : 'Hiệu chỉnh mẫu thông báo' }}
              </h2>
              <p class="text-xs text-muted mt-1">Soạn thảo nội dung, chèn biến động Handlebars và kiểm tra kết xuất hiển thị.</p>
            </div>
            
            <button 
              @click="isDrawerOpen = false"
              class="lg-icon-button p-2 bg-surface-card rounded-lg border border-default text-muted hover:text-heading"
            >
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Drawer Navigation Tabs (Chỉ hiển thị trên mobile) -->
          <div class="px-5 py-2 border-b border-default bg-surface-table flex gap-4 lg:hidden">
            <button 
              @click="activeTab = 'editor'"
              class="px-4 py-2 text-xs font-bold border-b-2 transition-colors flex items-center gap-2"
              :class="activeTab === 'editor' ? 'border-primary text-primary font-extrabold' : 'border-transparent text-muted hover:text-heading'"
            >
              <Pencil class="w-4 h-4" />
              Soạn thảo nội dung
            </button>
            <button 
              @click="activeTab = 'preview'"
              class="px-4 py-2 text-xs font-bold border-b-2 transition-colors flex items-center gap-2"
              :class="activeTab === 'preview' ? 'border-primary text-primary font-extrabold' : 'border-transparent text-muted hover:text-heading'"
            >
              <Eye class="w-4 h-4" />
              Xem trước thực tế (Preview)
            </button>
          </div>

          <!-- Drawer Body -->
          <div class="flex-1 overflow-y-auto p-5">
            <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
              
              <!-- Cột 1: Form Soạn thảo -->
              <div 
                :class="[
                  activeTab === 'editor' ? 'block' : 'hidden lg:block',
                  'lg:col-span-7 space-y-4'
                ]"
              >
                <h3 class="font-extrabold text-sm text-primary border-b border-default pb-1 flex items-center gap-1.5">
                  <FileText class="w-4 h-4" />
                  Cấu hình Soạn thảo
                </h3>

                <!-- Cặp sự kiện & kênh gửi -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-bold text-label mb-1.5 uppercase">Loại sự kiện</label>
                    <select 
                      v-model="currentTemplate.eventType"
                      :disabled="drawerMode === 'edit'"
                      class="w-full px-3 lg-control text-sm"
                    >
                      <option v-for="evt in eventTypes" :key="evt.code" :value="evt.code">
                        {{ evt.name }}
                      </option>
                    </select>
                  </div>

                  <div>
                    <label class="block text-xs font-bold text-label mb-1.5 uppercase">Kênh truyền tải</label>
                    <select 
                      v-model="currentTemplate.channel"
                      :disabled="drawerMode === 'edit'"
                      class="w-full px-3 lg-control text-sm"
                    >
                      <option v-for="chan in channels" :key="chan" :value="chan">
                        {{ chan }}
                      </option>
                    </select>
                  </div>
                </div>

                <!-- Trùng lặp Warning -->
                <p 
                  v-if="isDuplicateConfig" 
                  class="text-xs text-rose-500 font-bold flex items-center gap-1 animate-pulse"
                >
                  <AlertTriangle class="w-4 h-4 flex-shrink-0" />
                  <span>Đã tồn tại template cho sự kiện và kênh này. Vui lòng chọn kênh khác hoặc sửa bản hiện tại!</span>
                </p>

                <!-- Tiêu đề (Email chỉ có tiêu đề) -->
                <div v-if="currentTemplate.channel === 'Email'">
                  <label class="block text-xs font-bold text-label mb-1.5 uppercase">Tiêu đề thư mẫu (Subject)</label>
                  <input 
                    type="text"
                    v-model="currentTemplate.subjectTemplate"
                    @focus="lastFocusedInput = 'subject'"
                    placeholder="Nhập tiêu đề thư gửi tới người học..."
                    class="w-full px-3 lg-control text-sm font-semibold"
                  />
                </div>

                <!-- Nội dung mẫu (Body) -->
                <div>
                  <label class="block text-xs font-bold text-label mb-1.5 uppercase">Thân bài mẫu (Body Template)</label>
                  <textarea
                    v-model="currentTemplate.bodyTemplate"
                    @focus="lastFocusedInput = 'body'"
                    rows="12"
                    placeholder="Soạn nội dung tin nhắn. Bạn có thể sử dụng các thẻ HTML nếu gửi email, hoặc plain text..."
                    class="w-full px-3 py-2 lg-control text-xs font-mono leading-relaxed"
                  ></textarea>
                  <div class="flex items-center justify-between mt-1.5">
                    <span class="text-[10px] text-muted font-bold">Cú pháp render: Handlebars Engine ({{ currentTemplate.bodyTemplate?.length || 0 }} ký tự)</span>
                    <span v-if="syntaxErrorText" class="text-[10px] text-rose-500 font-extrabold flex items-center gap-1 animate-pulse">
                      <AlertTriangle class="w-3.5 h-3.5" />
                      {{ syntaxErrorText }}
                    </span>
                  </div>
                </div>

                <!-- Placeholder Helper Area -->
                <div class="lg-glass-soft p-3.5 rounded-xl border border-default">
                  <div class="flex items-center gap-1.5 mb-2.5">
                    <HelpCircle class="w-4 h-4 text-primary" />
                    <h4 class="text-xs font-extrabold text-heading">Vùng Trợ Giúp Placeholder (Click để chèn nhanh)</h4>
                  </div>
                  
                  <p class="text-[10px] text-muted leading-normal mb-3">
                    Chọn loại sự kiện để hiển thị các biến động tương ứng của cơ sở dữ liệu. Click vào thẻ để tự động chèn vào khu soạn thảo đang chọn.
                  </p>

                  <div class="flex flex-wrap gap-2">
                    <button
                      v-for="ph in activePlaceholders"
                      :key="ph.code"
                      @click="insertPlaceholder(ph.code)"
                      class="px-2.5 py-1 rounded bg-surface-card hover:bg-slate-200 dark:hover:bg-slate-800 border border-default text-[11px] font-bold text-heading transition-all duration-150 flex items-center gap-1"
                    >
                      <span>{{ ph.code }}</span>
                      <span class="text-[9px] text-muted font-normal">({{ ph.name }})</span>
                    </button>
                    <span v-if="activePlaceholders.length === 0" class="text-xs text-muted italic">Vui lòng chọn loại sự kiện để tải các biến.</span>
                  </div>
                </div>

                <!-- Trạng thái hoạt động -->
                <div>
                  <label class="block text-xs font-bold text-label mb-1.5 uppercase">Trạng thái phát hành</label>
                  <div class="flex gap-4 text-xs font-bold mt-1">
                    <label class="flex items-center gap-2 cursor-pointer">
                      <input type="radio" value="Active" v-model="currentTemplate.status" class="text-primary focus:ring-primary" />
                      <span class="text-heading">Kích hoạt (Active)</span>
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer">
                      <input type="radio" value="Draft" v-model="currentTemplate.status" class="text-primary focus:ring-primary" />
                      <span class="text-muted">Lưu nháp (Draft)</span>
                    </label>
                  </div>
                </div>
              </div>

              <!-- Cột 2: Real-time Preview Panel -->
              <div 
                :class="[
                  activeTab === 'preview' ? 'flex' : 'hidden lg:flex',
                  'lg:col-span-5 flex-col space-y-4 lg:border-l lg:border-default lg:pl-6 w-full'
                ]"
              >
                <h3 class="font-extrabold text-sm text-primary border-b border-default pb-1 flex items-center gap-1.5">
                  <Eye class="w-4 h-4" />
                  Xem trước hiển thị thực tế
                </h3>

                <p class="text-[10px] text-muted leading-relaxed">
                  Render nội dung tin nhắn thời gian thực bằng cách tự động thay thế các placeholder Handlebars bằng thông tin mẫu của học sinh <strong>Nguyễn Văn An (HE170001)</strong>.
                </p>

                <!-- Phân loại hiển thị theo Kênh -->
                
                <!-- 1. EMAIL PREVIEW (Giả lập màn hình web/Desktop email) -->
                <div v-if="currentTemplate.channel === 'Email'" class="w-full flex flex-col min-h-[350px]">
                  <div class="w-full bg-slate-200 dark:bg-slate-800 rounded-t-xl px-4 py-2 border-b border-default flex items-center gap-2 text-xs">
                    <Globe class="w-4.5 h-4.5 text-muted" />
                    <span class="font-bold text-muted">HTML Email Client Mockup</span>
                  </div>
                  <div class="w-full lg-glass bg-white dark:bg-slate-950 p-4 rounded-b-xl border border-t-0 overflow-y-auto max-h-[500px]">
                    <div class="text-xs border-b border-default pb-3.5 mb-4 text-slate-800 dark:text-slate-200 space-y-1">
                      <div><span class="font-bold text-muted mr-1">Người gửi:</span> <span class="font-bold text-heading">LMS Academic &lt;no-reply@fpt.edu.vn&gt;</span></div>
                      <div><span class="font-bold text-muted mr-1">Tiêu đề thư:</span> <span class="font-bold text-primary">{{ renderedPreviewSubject || '(Chưa nhập tiêu đề)' }}</span></div>
                    </div>
                    <!-- Content Body -->
                    <div v-html="renderedPreviewBody" class="prose dark:prose-invert max-w-full text-xs text-slate-800 dark:text-slate-100"></div>
                  </div>
                </div>

                <!-- 2. MOBILE PREVIEW (Giả lập màn hình điện thoại di động) -->
                <div v-else class="w-full flex flex-col items-center justify-start py-4">
                  <!-- Vỏ Điện thoại di động Mockup -->
                  <div class="w-[280px] h-[480px] rounded-[36px] border-[6px] border-slate-700 dark:border-slate-800 bg-slate-900 shadow-2xl relative flex flex-col p-3 overflow-hidden">
                    <!-- Tai thỏ / Notch -->
                    <div class="absolute top-0 left-1/2 -translate-x-1/2 w-28 h-4.5 bg-slate-700 dark:border-slate-800 rounded-b-xl z-20 flex items-center justify-center">
                      <span class="w-1.5 h-1.5 rounded-full bg-slate-900 mr-1.5"></span>
                      <span class="w-8 h-1 bg-slate-950 rounded-full"></span>
                    </div>

                    <!-- Màn hình điện thoại -->
                    <div class="flex-1 rounded-[26px] bg-slate-100 dark:bg-slate-950 p-3 flex flex-col overflow-hidden relative z-10 pt-6">
                      <div class="flex items-center justify-between text-[9px] text-muted font-bold mb-3 px-1">
                        <span>12:25 PM</span>
                        <div class="flex items-center gap-1">
                          <span>5G</span>
                          <span class="w-4 h-2.5 rounded bg-slate-400 dark:bg-slate-700"></span>
                        </div>
                      </div>

                      <!-- MOCK PUSH NOTIFICATION SURFACES -->
                      <div v-if="currentTemplate.channel === 'Push'" class="w-full p-2.5 rounded-xl bg-white/90 dark:bg-slate-900/90 border border-default shadow-md space-y-1.5">
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-1.5">
                            <span class="w-4 h-4 rounded-md bg-primary flex items-center justify-center text-[9px] font-bold text-white">LMS</span>
                            <span class="text-[10px] font-extrabold text-heading">LMS Academic Portal</span>
                          </div>
                          <span class="text-[8px] text-muted">Bây giờ</span>
                        </div>
                        <div class="text-[10px] leading-relaxed text-body text-heading font-medium">
                          {{ renderedPreviewBody || '(Trống nội dung thông báo)' }}
                        </div>
                      </div>

                      <!-- MOCK SMS MESSAGE CHAT VIEW -->
                      <div v-else-if="currentTemplate.channel === 'SMS'" class="flex-1 flex flex-col justify-end space-y-2">
                        <div class="text-center text-[8px] text-muted py-1.5 uppercase font-bold">Hôm nay 12:25 PM</div>
                        
                        <div class="self-start max-w-[85%] bg-slate-200 dark:bg-slate-800 text-slate-800 dark:text-slate-100 p-2.5 rounded-2xl rounded-tl-none text-[10px] leading-relaxed font-semibold">
                          {{ renderedPreviewBody || '(Trống nội dung SMS)' }}
                        </div>
                        
                        <!-- Khung nhập nhắn tin dưới cùng điện thoại -->
                        <div class="mt-4 pt-2 border-t border-default/45 flex items-center gap-1.5">
                          <div class="flex-1 bg-white dark:bg-slate-900 rounded-full px-3 py-1 text-[8px] text-muted border">iMessage / Tin nhắn</div>
                          <span class="w-5 h-5 rounded-full bg-emerald-500 flex items-center justify-center text-white"><Check class="w-3 h-3" /></span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </div>

          <!-- Drawer Footer -->
          <div class="p-5 border-t border-default flex items-center justify-end gap-3 bg-surface-table">
            <button
              @click="isDrawerOpen = false"
              class="lg-btn-secondary px-4 py-2 text-sm font-bold"
            >
              Hủy bỏ
            </button>
            <button
              @click="handleSaveTemplate"
              :disabled="!isFormValid || isDuplicateConfig"
              class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <Save class="w-4 h-4" />
              Lưu cấu hình
            </button>
          </div>

        </div>
      </div>

      <!-- Test Send Modal (Modal Gửi Thử Nghiệm) -->
      <div 
        v-if="isTestModalOpen" 
        class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
      >
        <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
          <!-- Close button -->
          <button 
            @click="isTestModalOpen = false"
            class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
          >
            <X class="w-4.5 h-4.5" />
          </button>

          <!-- Header -->
          <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
            <div class="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
              <Send class="w-5.5 h-5.5" />
            </div>
            <div>
              <h3 class="text-base font-extrabold text-heading">Gửi Thử Nghiệm Template</h3>
              <p class="text-xs text-muted mt-0.5">Mô phỏng gửi tin nhắn thật để kiểm tra luồng truyền tải và định dạng.</p>
            </div>
          </div>

          <!-- Nhập thông tin -->
          <div class="space-y-4 mb-5">
            <div class="p-3.5 rounded-lg bg-surface-card border border-default/50 text-xs space-y-1.5">
              <div><span class="font-bold text-muted mr-1">Sự kiện:</span> <span class="font-bold text-heading">{{ getEventName(testTargetTemplate?.eventType) }}</span></div>
              <div><span class="font-bold text-muted mr-1">Kênh:</span> <span class="font-bold text-heading text-primary">{{ testTargetTemplate?.channel }}</span></div>
            </div>

            <div>
              <label class="block text-xs font-bold text-label mb-2 uppercase">
                {{ testTargetTemplate?.channel === 'Email' ? 'Địa chỉ Email nhận thử' : testTargetTemplate?.channel === 'SMS' ? 'Số điện thoại nhận SMS thử' : 'Mã sinh viên nhận Push' }}
              </label>
              
              <input 
                type="text" 
                v-model="testReceiver"
                :placeholder="testTargetTemplate?.channel === 'Email' ? 'nhanvien@fpt.edu.vn' : '09xxxxxxxx'"
                class="w-full px-3 lg-control text-sm font-semibold"
              />
              <span v-if="!testReceiver.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Bắt buộc điền thông tin để thực hiện gửi thử</span>
            </div>
          </div>

          <!-- Footer Actions -->
          <div class="flex items-center justify-end gap-2.5">
            <button
              @click="isTestModalOpen = false"
              :disabled="isSendingTest"
              class="lg-btn-secondary px-4 py-2 text-sm font-bold"
            >
              Hủy
            </button>
            <button
              @click="handleSendTest"
              :disabled="!testReceiver.trim() || isSendingTest"
              class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <span v-if="isSendingTest" class="lg-shell-loading-spinner w-4 h-4 border-2"></span>
              <Send v-else class="w-4 h-4" />
              {{ isSendingTest ? 'Đang gửi thử...' : 'Gửi thử nghiệm' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Khung Audit Logs & Render Errors -->
      <div class="lg-glass-soft lg-card lg-density-normal">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <History class="w-5 h-5 text-primary" />
          <div>
            <h3 class="font-extrabold text-heading text-sm">Nhật ký Template & Lỗi kết xuất (Audit Logs)</h3>
            <p class="text-xs text-muted mt-0.5">Ghi nhận chi tiết hoạt động sửa đổi nội dung và các lỗi render trong quá trình vận hành.</p>
          </div>
        </div>

        <div class="space-y-3.5">
          <div 
            v-for="log in auditLogs" 
            :key="log.id" 
            class="flex flex-col sm:flex-row sm:items-start justify-between p-3 rounded-lg bg-surface-card border border-default/30 text-xs gap-3 hover:bg-surface-card-hover transition-colors"
            :class="log.type === 'error' ? 'border-rose-300 bg-rose-500/5' : ''"
          >
            <div class="space-y-1">
              <div class="flex flex-wrap items-center gap-2">
                <span class="font-bold text-heading">{{ log.actor }}</span>
                <span 
                  class="lg-badge py-0.5 px-2 text-[9px] font-extrabold"
                  :class="log.type === 'error' ? 'lg-badge-danger animate-pulse' : 'lg-badge-info'"
                >
                  {{ log.action }}
                </span>
              </div>
              <p class="text-body font-medium leading-relaxed" :class="log.type === 'error' ? 'text-rose-600 dark:text-rose-400 font-bold' : ''">
                {{ log.details }}
              </p>
            </div>
            
            <div class="text-[10px] text-muted font-semibold whitespace-nowrap self-end sm:self-start">
              {{ log.time }}
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Mockup Mobile and Email Customization */
textarea {
  resize: vertical;
}
input[type="radio"] {
  width: 1rem;
  height: 1rem;
  border: 1px solid var(--border-input);
  background-color: var(--surface-input);
}
</style>
