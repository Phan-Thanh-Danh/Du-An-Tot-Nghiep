<script setup>
/**
 * ApprovalsRequestsView.vue - Super Admin
 * Giao diện duyệt đơn từ sinh viên (Module M13).
 * Workflow Stepper hiển thị quy trình duyệt đa bước.
 * Drawer xem chi tiết đơn, phê duyệt/từ chối với lý do.
 */
import { ref, computed } from 'vue'
import {
  Inbox,
  Search,
  Filter,
  RotateCcw,
  Eye,
  X,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Clock,
  Info,
  User,
  Calendar,
  FileText,
  Tag,
  Send,
  ArrowRight,
  MessageSquare,
  ChevronRight,
  CircleDot,
  Circle,
  Check
} from 'lucide-vue-next'

// --- Mock Data ---
const requestTypes = [
  { id: 'leave', name: 'Xin phép vắng mặt', icon: 'Calendar' },
  { id: 'retake', name: 'Xin thi lại / Phúc khảo', icon: 'FileText' },
  { id: 'withdraw', name: 'Rút môn học', icon: 'XCircle' },
  { id: 'transfer', name: 'Chuyển cơ sở', icon: 'ArrowRight' },
  { id: 'certificate', name: 'Cấp giấy xác nhận', icon: 'FileText' },
  { id: 'other', name: 'Khác', icon: 'MessageSquare' }
]

const requests = ref([
  {
    id: 'REQ-2026-001',
    studentId: 'HE170001',
    studentName: 'Nguyễn Văn An',
    email: 'annv@fpt.edu.vn',
    campus: 'Cơ sở Hòa Lạc',
    type: 'retake',
    subject: 'Xin phúc khảo bài thi môn PRN211',
    description: 'Em nhận thấy kết quả thi cuối kỳ môn PRN211 không đúng với những gì em đã làm. Em xin phúc khảo bài thi viết (CK). Em đã nộp đủ bài và tham gia đầy đủ.',
    attachments: ['BienLai_PhucKhao.pdf'],
    priority: 'Cao',
    status: 'Chờ duyệt',
    createdAt: '2026-06-12 08:00',
    updatedAt: '2026-06-12 08:00',
    deadline: '2026-06-19',
    currentStep: 1,
    steps: [
      { id: 1, label: 'Sinh viên nộp đơn', status: 'completed', date: '2026-06-12 08:00', actor: 'Nguyễn Văn An' },
      { id: 2, label: 'Giáo vụ xem xét', status: 'current', date: null, actor: null },
      { id: 3, label: 'Trưởng bộ môn phê duyệt', status: 'pending', date: null, actor: null },
      { id: 4, label: 'Super Admin xác nhận', status: 'pending', date: null, actor: null }
    ],
    comments: []
  },
  {
    id: 'REQ-2026-002',
    studentId: 'HE170045',
    studentName: 'Trần Thị Bích',
    email: 'bichtt@fpt.edu.vn',
    campus: 'Cơ sở TP.HCM',
    type: 'withdraw',
    subject: 'Xin rút môn SWD392 kỳ Summer 2026',
    description: 'Em đang gặp vấn đề sức khỏe không thể tiếp tục học môn SWD392. Em đã trao đổi với giảng viên và được hướng dẫn nộp đơn rút môn. Em xin hoàn học phí theo quy chế.',
    attachments: ['GiayKhamBenh.pdf', 'XacNhanGV.pdf'],
    priority: 'Trung bình',
    status: 'Chờ duyệt',
    createdAt: '2026-06-11 14:30',
    updatedAt: '2026-06-11 16:00',
    deadline: '2026-06-18',
    currentStep: 2,
    steps: [
      { id: 1, label: 'Sinh viên nộp đơn', status: 'completed', date: '2026-06-11 14:30', actor: 'Trần Thị Bích' },
      { id: 2, label: 'Giáo vụ xem xét', status: 'completed', date: '2026-06-11 16:00', actor: 'GV Nguyễn Thị Lan' },
      { id: 3, label: 'Trưởng bộ môn phê duyệt', status: 'current', date: null, actor: null },
      { id: 4, label: 'Super Admin xác nhận', status: 'pending', date: null, actor: null }
    ],
    comments: [
      { id: 1, author: 'GV Nguyễn Thị Lan', role: 'staff', content: 'Đã xác minh tình trạng sức khỏe. Đề xuất duyệt theo quy chế hoàn phí tuần 3.', date: '2026-06-11 16:00' }
    ]
  },
  {
    id: 'REQ-2026-003',
    studentId: 'HE170112',
    studentName: 'Lê Hoàng Long',
    email: 'longlh@fpt.edu.vn',
    campus: 'Cơ sở Đà Nẵng',
    type: 'leave',
    subject: 'Xin phép nghỉ 3 ngày (12-14/06) vì lý do gia đình',
    description: 'Em xin phép vắng mặt từ ngày 12/06 đến 14/06/2026 do có việc gia đình khẩn cấp (tang lễ ông nội). Em sẽ bù bài tập và liên hệ giảng viên ngay khi quay lại.',
    attachments: ['DonXinPhep.pdf'],
    priority: 'Cao',
    status: 'Chờ duyệt',
    createdAt: '2026-06-11 20:30',
    updatedAt: '2026-06-12 07:00',
    deadline: '2026-06-15',
    currentStep: 3,
    steps: [
      { id: 1, label: 'Sinh viên nộp đơn', status: 'completed', date: '2026-06-11 20:30', actor: 'Lê Hoàng Long' },
      { id: 2, label: 'Giáo vụ xem xét', status: 'completed', date: '2026-06-12 07:00', actor: 'GV Phạm Thị Hoa' },
      { id: 3, label: 'Trưởng bộ môn phê duyệt', status: 'completed', date: '2026-06-12 08:15', actor: 'TS. Trần Minh' },
      { id: 4, label: 'Super Admin xác nhận', status: 'current', date: null, actor: null }
    ],
    comments: [
      { id: 1, author: 'GV Phạm Thị Hoa', role: 'staff', content: 'Xác nhận lý do chính đáng. Đề xuất phê duyệt.', date: '2026-06-12 07:00' },
      { id: 2, author: 'TS. Trần Minh', role: 'head', content: 'Đồng ý. SV cần cam kết hoàn thành bù bài tập trong 7 ngày.', date: '2026-06-12 08:15' }
    ]
  },
  {
    id: 'REQ-2026-004',
    studentId: 'HE170203',
    studentName: 'Phạm Minh Đức',
    email: 'ducpm@fpt.edu.vn',
    campus: 'Cơ sở Hòa Lạc',
    type: 'certificate',
    subject: 'Xin cấp giấy xác nhận SV (bản tiếng Anh)',
    description: 'Em cần 2 bản giấy xác nhận sinh viên bằng tiếng Anh để nộp hồ sơ xin visa du học trao đổi tại Nhật Bản (chương trình Summer Exchange 2026).',
    attachments: [],
    priority: 'Thấp',
    status: 'Chờ duyệt',
    createdAt: '2026-06-10 09:00',
    updatedAt: '2026-06-10 09:00',
    deadline: '2026-06-20',
    currentStep: 1,
    steps: [
      { id: 1, label: 'Sinh viên nộp đơn', status: 'completed', date: '2026-06-10 09:00', actor: 'Phạm Minh Đức' },
      { id: 2, label: 'Phòng đào tạo xử lý', status: 'current', date: null, actor: null },
      { id: 3, label: 'Super Admin xác nhận', status: 'pending', date: null, actor: null }
    ],
    comments: []
  },
  {
    id: 'REQ-2026-005',
    studentId: 'HE170089',
    studentName: 'Ngô Thùy Linh',
    email: 'linhngt@fpt.edu.vn',
    campus: 'Cơ sở TP.HCM',
    type: 'transfer',
    subject: 'Xin chuyển từ CS TP.HCM sang CS Hòa Lạc',
    description: 'Do gia đình chuyển về Hà Nội, em xin chuyển sang cơ sở Hòa Lạc từ kỳ Fall 2026. Em đã hoàn thành 4 kỳ tại CS TP.HCM với GPA 3.2/4.0.',
    attachments: ['DonChuyenCS.pdf', 'BangDiem4Ky.pdf'],
    priority: 'Trung bình',
    status: 'Chờ duyệt',
    createdAt: '2026-06-09 11:00',
    updatedAt: '2026-06-11 10:00',
    deadline: '2026-06-25',
    currentStep: 2,
    steps: [
      { id: 1, label: 'Sinh viên nộp đơn', status: 'completed', date: '2026-06-09 11:00', actor: 'Ngô Thùy Linh' },
      { id: 2, label: 'GV CS TP.HCM xem xét', status: 'completed', date: '2026-06-10 14:00', actor: 'GV Lê Văn Tùng' },
      { id: 3, label: 'GV CS Hòa Lạc xác nhận', status: 'current', date: null, actor: null },
      { id: 4, label: 'Trưởng khoa phê duyệt', status: 'pending', date: null, actor: null },
      { id: 5, label: 'Super Admin xác nhận', status: 'pending', date: null, actor: null }
    ],
    comments: [
      { id: 1, author: 'GV Lê Văn Tùng', role: 'staff', content: 'SV đạt yêu cầu chuyển cơ sở. GPA đạt chuẩn. Đề xuất duyệt.', date: '2026-06-10 14:00' }
    ]
  }
])

// --- Filter State ---
const searchQuery = ref('')
const filterType = ref('all')
const filterPriority = ref('all')

const filteredRequests = computed(() => {
  return requests.value.filter(r => {
    const matchSearch = searchQuery.value === '' ||
      r.id.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.studentName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.subject.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === 'all' || r.type === filterType.value
    const matchPriority = filterPriority.value === 'all' || r.priority === filterPriority.value
    return matchSearch && matchType && matchPriority
  })
})

const resetFilters = () => {
  searchQuery.value = ''
  filterType.value = 'all'
  filterPriority.value = 'all'
}

// --- KPI ---
const totalRequests = computed(() => requests.value.length)
const urgentRequests = computed(() => requests.value.filter(r => r.priority === 'Cao' || r.priority === 'Khẩn cấp').length)
const nearDeadline = computed(() => {
  const today = new Date()
  return requests.value.filter(r => {
    const dl = new Date(r.deadline)
    const diff = (dl - today) / (1000 * 60 * 60 * 24)
    return diff <= 3 && diff >= 0
  }).length
})
const avgSteps = computed(() => {
  const total = requests.value.reduce((s, r) => s + r.steps.length, 0)
  return (total / requests.value.length).toFixed(1)
})

// --- Drawer State ---
const isDrawerOpen = ref(false)
const selectedRequest = ref(null)
const approvalComment = ref('')

const openDrawer = (req) => {
  selectedRequest.value = JSON.parse(JSON.stringify(req))
  approvalComment.value = ''
  isDrawerOpen.value = true
}
const closeDrawer = () => {
  isDrawerOpen.value = false
  selectedRequest.value = null
}

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => { showToast.value = false }, 4000)
}

// --- Handlers ---
const handleApprove = () => {
  if (!selectedRequest.value) return
  const original = requests.value.find(r => r.id === selectedRequest.value.id)
  if (!original) return

  const timeString = new Date().toLocaleString('vi-VN')

  // Advance current step
  const currentStepObj = original.steps.find(s => s.status === 'current')
  if (currentStepObj) {
    currentStepObj.status = 'completed'
    currentStepObj.date = timeString
    currentStepObj.actor = 'Super Admin'
  }

  // Advance next step or complete
  const nextStep = original.steps.find(s => s.status === 'pending')
  if (nextStep) {
    nextStep.status = 'current'
    original.currentStep++
  } else {
    original.status = 'Đã duyệt'
  }

  if (approvalComment.value.trim()) {
    original.comments.push({
      id: Date.now(),
      author: 'Super Admin',
      role: 'admin',
      content: approvalComment.value.trim(),
      date: timeString
    })
  }

  original.updatedAt = timeString

  if (original.status === 'Đã duyệt') {
    triggerToast(`Đơn ${original.id} đã được phê duyệt hoàn tất.`, 'success')
  } else {
    triggerToast(`Đã duyệt bước hiện tại cho đơn ${original.id}. Chuyển sang bước tiếp theo.`, 'info')
  }

  closeDrawer()
}

const handleReject = () => {
  if (!selectedRequest.value) return
  if (!approvalComment.value.trim()) {
    triggerToast('Vui lòng nhập lý do từ chối.', 'error')
    return
  }

  const original = requests.value.find(r => r.id === selectedRequest.value.id)
  if (!original) return

  const timeString = new Date().toLocaleString('vi-VN')
  original.status = 'Từ chối'
  original.updatedAt = timeString

  const currentStepObj = original.steps.find(s => s.status === 'current')
  if (currentStepObj) {
    currentStepObj.status = 'rejected'
    currentStepObj.date = timeString
    currentStepObj.actor = 'Super Admin'
  }

  original.comments.push({
    id: Date.now(),
    author: 'Super Admin',
    role: 'admin',
    content: `[TỪ CHỐI] ${approvalComment.value.trim()}`,
    date: timeString
  })

  triggerToast(`Đơn ${original.id} đã bị từ chối.`, 'error')
  closeDrawer()
}

// --- Helpers ---
const getTypeName = (type) => requestTypes.find(t => t.id === type)?.name || type
const getTypeBadgeClass = (type) => {
  const classes = {
    leave: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-200 dark:border-blue-500/20',
    retake: 'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-200 dark:border-violet-500/20',
    withdraw: 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-200 dark:border-rose-500/20',
    transfer: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/20',
    certificate: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-200 dark:border-emerald-500/20',
    other: 'bg-slate-500/10 text-slate-600 dark:text-slate-400 border-slate-200 dark:border-slate-500/20'
  }
  return classes[type] || classes.other
}

const getPriorityClass = (priority) => {
  switch (priority) {
    case 'Khẩn cấp': return 'text-rose-500 font-extrabold animate-pulse'
    case 'Cao': return 'text-orange-500 font-bold'
    case 'Trung bình': return 'text-amber-500 font-semibold'
    case 'Thấp': return 'text-sky-500 font-medium'
    default: return 'text-muted'
  }
}

const getDeadlineClass = (deadline) => {
  const today = new Date()
  const dl = new Date(deadline)
  const diff = (dl - today) / (1000 * 60 * 60 * 24)
  if (diff < 0) return 'text-rose-500 font-bold'
  if (diff <= 3) return 'text-amber-500 font-bold'
  return 'text-muted font-medium'
}

const getStepStatusIcon = (status) => {
  switch (status) {
    case 'completed': return { icon: Check, class: 'bg-emerald-500 text-white' }
    case 'current': return { icon: CircleDot, class: 'bg-primary text-white animate-pulse' }
    case 'rejected': return { icon: XCircle, class: 'bg-rose-500 text-white' }
    default: return { icon: Circle, class: 'bg-surface-input text-muted border border-default' }
  }
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast -->
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
      <!-- Page Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Inbox class="w-8 h-8 text-primary" />
            Đơn Cần Duyệt
          </h1>
          <p class="text-sm text-muted mt-1">
            Xem xét và phê duyệt các đơn từ sinh viên theo quy trình duyệt đa bước (Workflow Stepper).
          </p>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <Inbox class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng đơn chờ</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalRequests }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đơn ưu tiên cao</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ urgentRequests }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Clock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Sắp đến hạn (≤3 ngày)</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ nearDeadline }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <ArrowRight class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Bước duyệt TB</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ avgSteps }} bước</div>
          </div>
        </div>
      </div>

      <!-- Filter Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Bộ lọc đơn từ</h3>
          </div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline">
            <RotateCcw class="w-3.5 h-3.5" />
            Xóa bộ lọc
          </button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
            <input v-model="searchQuery" type="text" placeholder="Tìm theo mã đơn, tên SV, tiêu đề..." class="w-full pl-9 pr-3 lg-control text-sm" />
          </div>
          <div>
            <select v-model="filterType" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả loại đơn</option>
              <option v-for="t in requestTypes" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
          </div>
          <div>
            <select v-model="filterPriority" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả mức ưu tiên</option>
              <option value="Khẩn cấp">Khẩn cấp</option>
              <option value="Cao">Cao</option>
              <option value="Trung bình">Trung bình</option>
              <option value="Thấp">Thấp</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Request Cards List -->
      <div class="space-y-3 mb-8">
        <div v-if="filteredRequests.length === 0" class="lg-glass-soft lg-card lg-density-spacious text-center">
          <Inbox class="w-10 h-10 text-muted mx-auto mb-3" />
          <p class="text-sm text-muted font-semibold">Không có đơn từ nào chờ duyệt.</p>
        </div>

        <div
          v-for="req in filteredRequests"
          :key="req.id"
          class="lg-glass-soft lg-card lg-card-hover lg-density-normal cursor-pointer transition-all duration-200"
          @click="openDrawer(req)"
        >
          <div class="flex flex-col lg:flex-row lg:items-center gap-4">
            <!-- Left: Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-2 flex-wrap">
                <span class="text-xs font-extrabold text-primary">{{ req.id }}</span>
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full border" :class="getTypeBadgeClass(req.type)">
                  {{ getTypeName(req.type) }}
                </span>
                <span class="text-[10px] font-bold" :class="getPriorityClass(req.priority)">
                  {{ req.priority }}
                </span>
              </div>
              <h4 class="font-bold text-heading text-sm mb-1 truncate">{{ req.subject }}</h4>
              <div class="flex items-center gap-3 text-[10px] text-muted">
                <span class="flex items-center gap-1"><User class="w-3 h-3" /> {{ req.studentName }} ({{ req.studentId }})</span>
                <span class="flex items-center gap-1"><Calendar class="w-3 h-3" /> {{ req.createdAt }}</span>
                <span class="flex items-center gap-1" :class="getDeadlineClass(req.deadline)"><Clock class="w-3 h-3" /> Hạn: {{ req.deadline }}</span>
              </div>
            </div>

            <!-- Right: Mini Stepper -->
            <div class="flex items-center gap-1 flex-shrink-0">
              <template v-for="(step, idx) in req.steps" :key="step.id">
                <div
                  class="w-6 h-6 rounded-full flex items-center justify-center flex-shrink-0"
                  :class="getStepStatusIcon(step.status).class"
                  :title="step.label"
                >
                  <component :is="getStepStatusIcon(step.status).icon" class="w-3.5 h-3.5" />
                </div>
                <div v-if="idx < req.steps.length - 1" class="w-4 h-0.5 rounded-full" :class="step.status === 'completed' ? 'bg-emerald-500' : 'bg-surface-input'"></div>
              </template>
              <ChevronRight class="w-5 h-5 text-muted ml-2" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Drawer - Request Detail -->
    <Transition name="drawer">
      <div v-if="isDrawerOpen" class="fixed inset-0 z-[100] flex justify-end">
        <div class="absolute inset-0 bg-slate-950/40 backdrop-blur-sm" @click="closeDrawer"></div>

        <div class="relative w-full max-w-2xl lg-glass-strong shadow-2xl flex flex-col h-full overflow-hidden">
          <!-- Drawer Header -->
          <div class="flex items-center justify-between px-5 py-4 border-b border-default flex-shrink-0">
            <div class="flex items-center gap-3">
              <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
                <FileText class="w-5 h-5" />
              </div>
              <div>
                <h2 class="text-sm font-extrabold text-heading">{{ selectedRequest?.id }}</h2>
                <p class="text-[10px] text-muted">Chi tiết đơn từ & Quy trình duyệt</p>
              </div>
            </div>
            <button @click="closeDrawer" class="lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default text-muted hover:text-heading">
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Drawer Body -->
          <div class="flex-1 overflow-y-auto px-5 py-4 space-y-5" v-if="selectedRequest">
            <!-- Request Info -->
            <div class="lg-glass-soft p-4 rounded-xl border border-default space-y-3">
              <div class="flex items-center gap-2 flex-wrap mb-2">
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full border" :class="getTypeBadgeClass(selectedRequest.type)">
                  {{ getTypeName(selectedRequest.type) }}
                </span>
                <span class="text-[10px] font-bold" :class="getPriorityClass(selectedRequest.priority)">
                  Ưu tiên: {{ selectedRequest.priority }}
                </span>
              </div>

              <h3 class="font-extrabold text-heading text-sm">{{ selectedRequest.subject }}</h3>

              <div class="grid grid-cols-2 gap-3 text-xs">
                <div><span class="text-muted block">Sinh viên</span><span class="font-bold text-heading">{{ selectedRequest.studentName }} ({{ selectedRequest.studentId }})</span></div>
                <div><span class="text-muted block">Cơ sở</span><span class="font-bold text-heading">{{ selectedRequest.campus }}</span></div>
                <div><span class="text-muted block">Ngày tạo</span><span class="font-bold text-heading">{{ selectedRequest.createdAt }}</span></div>
                <div><span class="text-muted block">Hạn xử lý</span><span class="font-bold" :class="getDeadlineClass(selectedRequest.deadline)">{{ selectedRequest.deadline }}</span></div>
              </div>

              <div class="pt-2 border-t border-default/50">
                <p class="text-xs text-body leading-relaxed">{{ selectedRequest.description }}</p>
              </div>

              <div v-if="selectedRequest.attachments.length" class="flex items-center gap-2 pt-2">
                <FileText class="w-3.5 h-3.5 text-muted" />
                <span class="text-[10px] text-muted font-semibold">Đính kèm:</span>
                <span
                  v-for="file in selectedRequest.attachments"
                  :key="file"
                  class="text-[10px] text-link font-bold hover:underline cursor-pointer"
                >{{ file }}</span>
              </div>
            </div>

            <!-- Workflow Stepper (Vertical) -->
            <div>
              <h4 class="text-xs font-bold text-label uppercase mb-3 flex items-center gap-1.5">
                <ArrowRight class="w-4 h-4 text-primary" />
                Quy trình duyệt (Workflow Stepper)
              </h4>

              <div class="relative ml-3">
                <div
                  v-for="(step, idx) in selectedRequest.steps"
                  :key="step.id"
                  class="flex items-start gap-3 relative"
                  :class="idx < selectedRequest.steps.length - 1 ? 'pb-6' : ''"
                >
                  <!-- Vertical line -->
                  <div
                    v-if="idx < selectedRequest.steps.length - 1"
                    class="absolute left-3.5 top-7 w-0.5 bottom-0"
                    :class="step.status === 'completed' ? 'bg-emerald-500' : step.status === 'rejected' ? 'bg-rose-500' : 'bg-surface-input'"
                  ></div>

                  <!-- Step icon -->
                  <div
                    class="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 z-10"
                    :class="getStepStatusIcon(step.status).class"
                  >
                    <component :is="getStepStatusIcon(step.status).icon" class="w-3.5 h-3.5" />
                  </div>

                  <!-- Step content -->
                  <div class="flex-1 min-w-0 pt-0.5">
                    <div class="text-xs font-bold text-heading">{{ step.label }}</div>
                    <div class="text-[10px] text-muted mt-0.5">
                      <span v-if="step.date">{{ step.date }} · {{ step.actor }}</span>
                      <span v-else-if="step.status === 'current'" class="text-primary font-semibold">Đang chờ xử lý...</span>
                      <span v-else>Chưa đến lượt</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Comments History -->
            <div v-if="selectedRequest.comments.length">
              <h4 class="text-xs font-bold text-label uppercase mb-3 flex items-center gap-1.5">
                <MessageSquare class="w-4 h-4 text-primary" />
                Nhận xét từ các bước duyệt ({{ selectedRequest.comments.length }})
              </h4>

              <div class="space-y-2.5">
                <div
                  v-for="comment in selectedRequest.comments"
                  :key="comment.id"
                  class="p-3 rounded-lg bg-surface-card border border-default/30 text-xs"
                >
                  <div class="flex items-center justify-between mb-1.5">
                    <span class="font-bold text-heading">{{ comment.author }}
                      <span class="lg-badge text-[9px] ml-1" :class="comment.role === 'admin' ? 'lg-badge-primary' : comment.role === 'head' ? 'lg-badge-violet' : 'lg-badge-info'">
                        {{ comment.role === 'admin' ? 'Admin' : comment.role === 'head' ? 'Trưởng BM' : 'Giáo vụ' }}
                      </span>
                    </span>
                    <span class="text-[9px] text-muted">{{ comment.date }}</span>
                  </div>
                  <p class="text-body leading-relaxed">{{ comment.content }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Drawer Footer - Approval Actions -->
          <div class="flex-shrink-0 px-5 py-4 border-t border-default bg-surface-card" v-if="selectedRequest && selectedRequest.status === 'Chờ duyệt'">
            <div class="mb-3">
              <label class="block text-xs font-bold text-label mb-1.5 uppercase">Nhận xét / Lý do</label>
              <textarea
                v-model="approvalComment"
                rows="2"
                placeholder="Nhập nhận xét hoặc lý do từ chối (bắt buộc khi từ chối)..."
                class="w-full px-3 py-2 lg-control text-sm resize-none"
              ></textarea>
            </div>
            <div class="flex items-center justify-end gap-2">
              <button
                @click="handleReject"
                class="lg-btn-danger text-xs px-4 py-2 font-bold flex items-center gap-1.5"
              >
                <XCircle class="w-4 h-4" />
                Từ chối
              </button>
              <button
                @click="handleApprove"
                class="lg-btn-primary text-xs px-4 py-2 font-bold flex items-center gap-1.5"
              >
                <CheckCircle class="w-4 h-4" />
                Phê duyệt bước hiện tại
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.drawer-enter-active,
.drawer-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.drawer-enter-active > div:last-child,
.drawer-leave-active > div:last-child {
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}
.drawer-enter-from > div:last-child,
.drawer-leave-to > div:last-child {
  transform: translateX(100%);
}
</style>