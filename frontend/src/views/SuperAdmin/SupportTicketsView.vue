<script setup>
/**
 * SupportTicketsView.vue - Super Admin
 * Giao diện quản trị Ticket hỗ trợ kỹ thuật/học vụ.
 * Hiển thị danh sách ticket, lọc theo trạng thái/ưu tiên, xem chi tiết qua Drawer,
 * trả lời và đóng ticket.
 */
import { ref, computed } from 'vue'
import {
  MessageCircle,
  Search,
  Filter,
  Eye,
  X,
  Send,
  Clock,
  CheckCircle,
  AlertTriangle,
  RotateCcw,
  ChevronRight,
  User,
  Tag,
  Calendar,
  MessageSquare,
  ArrowUpCircle,
  ArrowDownCircle,
  MinusCircle,
  Info,
  LifeBuoy,
  Plus,
  XCircle
} from 'lucide-vue-next'

// --- Mock Data ---
const tickets = ref([
  {
    id: 'TK-2026-001',
    studentId: 'HE170001',
    studentName: 'Nguyễn Văn An',
    email: 'annv@fpt.edu.vn',
    campus: 'Cơ sở Hòa Lạc',
    category: 'Kỹ thuật',
    subject: 'Không thể truy cập video bài giảng',
    description: 'Em không thể xem video bài giảng môn PRN211 từ sáng nay. Trình duyệt báo lỗi "403 Forbidden" khi nhấn play. Em đã thử xóa cache và đổi trình duyệt nhưng vẫn bị lỗi tương tự.',
    priority: 'Cao',
    status: 'Mới',
    createdAt: '2026-06-12 08:30',
    updatedAt: '2026-06-12 08:30',
    assignedTo: null,
    replies: []
  },
  {
    id: 'TK-2026-002',
    studentId: 'HE170045',
    studentName: 'Trần Thị Bích',
    email: 'bichtt@fpt.edu.vn',
    campus: 'Cơ sở TP.HCM',
    category: 'Học vụ',
    subject: 'Yêu cầu cấp lại mật khẩu LMS',
    description: 'Em đã quên mật khẩu LMS và email khôi phục cũng không nhận được. Nhờ admin reset lại giúp em.',
    priority: 'Trung bình',
    status: 'Đang xử lý',
    createdAt: '2026-06-11 14:20',
    updatedAt: '2026-06-11 16:45',
    assignedTo: 'Admin Hệ thống',
    replies: [
      {
        id: 1,
        author: 'Admin Hệ thống',
        authorRole: 'admin',
        content: 'Em vui lòng cung cấp thêm MSSV và email đăng ký ban đầu để mình xác minh nhé.',
        createdAt: '2026-06-11 16:45'
      }
    ]
  },
  {
    id: 'TK-2026-003',
    studentId: 'HE170112',
    studentName: 'Lê Hoàng Long',
    email: 'longlh@fpt.edu.vn',
    campus: 'Cơ sở Đà Nẵng',
    category: 'Tài chính',
    subject: 'Sai số tiền công nợ học phí kỳ Summer 2026',
    description: 'Em đã đóng đủ học phí kỳ Summer 2026 qua chuyển khoản ngân hàng, nhưng trên hệ thống vẫn hiển thị em đang nợ 5.500.000đ. Em đã giữ biên lai giao dịch thành công.',
    priority: 'Cao',
    status: 'Đang xử lý',
    createdAt: '2026-06-10 09:15',
    updatedAt: '2026-06-11 10:30',
    assignedTo: 'Phòng Tài chính',
    replies: [
      {
        id: 1,
        author: 'Phòng Tài chính',
        authorRole: 'admin',
        content: 'Mình đã nhận được ticket và đang kiểm tra lại dữ liệu giao dịch. Em vui lòng gửi ảnh biên lai chuyển khoản vào email support@fpt.edu.vn nhé.',
        createdAt: '2026-06-10 14:00'
      },
      {
        id: 2,
        author: 'Lê Hoàng Long',
        authorRole: 'student',
        content: 'Dạ em đã gửi email kèm biên lai rồi ạ. Nhờ anh/chị kiểm tra giúp em.',
        createdAt: '2026-06-10 15:20'
      },
      {
        id: 3,
        author: 'Phòng Tài chính',
        authorRole: 'admin',
        content: 'Mình đã xác nhận giao dịch. Hệ thống sẽ được cập nhật trong vòng 24h. Em theo dõi thêm nhé.',
        createdAt: '2026-06-11 10:30'
      }
    ]
  },
  {
    id: 'TK-2026-004',
    studentId: 'HE170203',
    studentName: 'Phạm Minh Đức',
    email: 'ducpm@fpt.edu.vn',
    campus: 'Cơ sở Hòa Lạc',
    category: 'Kỹ thuật',
    subject: 'Bài thi online bị thoát giữa chừng',
    description: 'Em đang làm bài thi môn SWD392 thì bị mất kết nối internet khoảng 2 phút. Khi vào lại thì hệ thống báo "Bài thi đã kết thúc" dù thời gian vẫn còn. Nhờ admin kiểm tra và cho em thi lại.',
    priority: 'Khẩn cấp',
    status: 'Mới',
    createdAt: '2026-06-12 10:05',
    updatedAt: '2026-06-12 10:05',
    assignedTo: null,
    replies: []
  },
  {
    id: 'TK-2026-005',
    studentId: 'HE170089',
    studentName: 'Ngô Thùy Linh',
    email: 'linhngt@fpt.edu.vn',
    campus: 'Cơ sở TP.HCM',
    category: 'Học vụ',
    subject: 'Xin cấp giấy xác nhận sinh viên',
    description: 'Em cần giấy xác nhận đang là sinh viên để nộp hồ sơ xin visa du học trao đổi. Nhờ phòng đào tạo cấp cho em 2 bản tiếng Anh.',
    priority: 'Thấp',
    status: 'Đã giải quyết',
    createdAt: '2026-06-08 11:30',
    updatedAt: '2026-06-09 16:00',
    assignedTo: 'Phòng Đào tạo',
    replies: [
      {
        id: 1,
        author: 'Phòng Đào tạo',
        authorRole: 'admin',
        content: 'Giấy xác nhận đã được ký và đóng dấu. Em lên phòng Đào tạo tầng 2 nhận từ 8h-17h nhé.',
        createdAt: '2026-06-09 16:00'
      }
    ]
  },
  {
    id: 'TK-2026-006',
    studentId: 'HE170300',
    studentName: 'Vũ Quốc Huy',
    email: 'huyvq@fpt.edu.vn',
    campus: 'Cơ sở Hòa Lạc',
    category: 'Kỹ thuật',
    subject: 'Không nộp được bài tập qua LMS',
    description: 'Em upload file bài tập .zip khoảng 45MB thì hệ thống báo lỗi "File too large". Giới hạn upload hiện tại chỉ cho phép 20MB. Nhờ admin tăng giới hạn hoặc hướng dẫn cách khác để nộp.',
    priority: 'Trung bình',
    status: 'Đã đóng',
    createdAt: '2026-06-05 13:00',
    updatedAt: '2026-06-06 09:30',
    assignedTo: 'Admin Hệ thống',
    replies: [
      {
        id: 1,
        author: 'Admin Hệ thống',
        authorRole: 'admin',
        content: 'Mình đã tăng giới hạn upload lên 50MB cho toàn hệ thống. Em thử nộp lại nhé.',
        createdAt: '2026-06-05 17:00'
      },
      {
        id: 2,
        author: 'Vũ Quốc Huy',
        authorRole: 'student',
        content: 'Dạ em nộp được rồi ạ. Cảm ơn admin!',
        createdAt: '2026-06-06 09:30'
      }
    ]
  }
])

// --- Filter State ---
const searchQuery = ref('')
const filterStatus = ref('all')
const filterPriority = ref('all')
const filterCategory = ref('all')

const filteredTickets = computed(() => {
  return tickets.value.filter(t => {
    const matchSearch = searchQuery.value === '' ||
      t.id.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      t.studentName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      t.subject.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === 'all' || t.status === filterStatus.value
    const matchPriority = filterPriority.value === 'all' || t.priority === filterPriority.value
    const matchCategory = filterCategory.value === 'all' || t.category === filterCategory.value
    return matchSearch && matchStatus && matchPriority && matchCategory
  })
})

const resetFilters = () => {
  searchQuery.value = ''
  filterStatus.value = 'all'
  filterPriority.value = 'all'
  filterCategory.value = 'all'
}

// --- KPI ---
const totalTickets = computed(() => tickets.value.length)
const newTickets = computed(() => tickets.value.filter(t => t.status === 'Mới').length)
const inProgressTickets = computed(() => tickets.value.filter(t => t.status === 'Đang xử lý').length)
const resolvedTickets = computed(() => tickets.value.filter(t => t.status === 'Đã giải quyết' || t.status === 'Đã đóng').length)

// --- Drawer State ---
const isDrawerOpen = ref(false)
const selectedTicket = ref(null)
const newReplyText = ref('')

const openDrawer = (ticket) => {
  selectedTicket.value = JSON.parse(JSON.stringify(ticket))
  newReplyText.value = ''
  isDrawerOpen.value = true
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  selectedTicket.value = null
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
const handleSendReply = () => {
  if (!newReplyText.value.trim() || !selectedTicket.value) return

  const timeString = new Date().toLocaleString('vi-VN')
  const newReply = {
    id: Date.now(),
    author: 'Super Admin',
    authorRole: 'admin',
    content: newReplyText.value.trim(),
    createdAt: timeString
  }

  // Update trong danh sách gốc
  const original = tickets.value.find(t => t.id === selectedTicket.value.id)
  if (original) {
    original.replies.push(newReply)
    original.updatedAt = timeString
    if (original.status === 'Mới') {
      original.status = 'Đang xử lý'
      original.assignedTo = 'Super Admin'
    }
  }
  selectedTicket.value.replies.push(newReply)
  newReplyText.value = ''
  triggerToast('Đã gửi phản hồi thành công.')
}

const handleCloseTicket = () => {
  if (!selectedTicket.value) return
  const original = tickets.value.find(t => t.id === selectedTicket.value.id)
  if (original) {
    original.status = 'Đã đóng'
    original.updatedAt = new Date().toLocaleString('vi-VN')
  }
  selectedTicket.value.status = 'Đã đóng'
  triggerToast(`Ticket ${selectedTicket.value.id} đã được đóng.`)
}

const handleResolveTicket = () => {
  if (!selectedTicket.value) return
  const original = tickets.value.find(t => t.id === selectedTicket.value.id)
  if (original) {
    original.status = 'Đã giải quyết'
    original.updatedAt = new Date().toLocaleString('vi-VN')
  }
  selectedTicket.value.status = 'Đã giải quyết'
  triggerToast(`Ticket ${selectedTicket.value.id} đã được đánh dấu giải quyết.`, 'success')
}

// --- Helpers ---
const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'Mới': return 'lg-badge-info'
    case 'Đang xử lý': return 'lg-badge-warning'
    case 'Đã giải quyết': return 'lg-badge-success'
    case 'Đã đóng': return 'lg-badge-violet'
    default: return ''
  }
}

const getPriorityIcon = (priority) => {
  switch (priority) {
    case 'Khẩn cấp': return { icon: ArrowUpCircle, class: 'text-rose-500 animate-pulse' }
    case 'Cao': return { icon: ArrowUpCircle, class: 'text-orange-500' }
    case 'Trung bình': return { icon: MinusCircle, class: 'text-amber-500' }
    case 'Thấp': return { icon: ArrowDownCircle, class: 'text-sky-500' }
    default: return { icon: MinusCircle, class: 'text-muted' }
  }
}

const getCategoryBadgeClass = (cat) => {
  switch (cat) {
    case 'Kỹ thuật': return 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-200 dark:border-blue-500/20'
    case 'Học vụ': return 'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-200 dark:border-violet-500/20'
    case 'Tài chính': return 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-200 dark:border-emerald-500/20'
    default: return 'bg-slate-100 text-slate-600 border-slate-200'
  }
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs -->
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
            <LifeBuoy class="w-8 h-8 text-primary" />
            Ticket Hỗ Trợ
          </h1>
          <p class="text-sm text-muted mt-1">
            Tiếp nhận, phân loại và xử lý các yêu cầu hỗ trợ kỹ thuật, học vụ, tài chính từ sinh viên và giảng viên.
          </p>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <MessageCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng Ticket</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalTickets }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-blue-500/10 flex items-center justify-center text-blue-500">
            <Plus class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Mới tiếp nhận</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ newTickets }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Clock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đang xử lý</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ inProgressTickets }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã giải quyết</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ resolvedTickets }}</div>
          </div>
        </div>
      </div>

      <!-- Filter Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Bộ lọc & Tìm kiếm ticket</h3>
          </div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline">
            <RotateCcw class="w-3.5 h-3.5" />
            Xóa bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          <!-- Search -->
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo mã, tên, tiêu đề..."
              class="w-full pl-9 pr-3 lg-control text-sm"
            />
          </div>

          <!-- Status -->
          <div>
            <select v-model="filterStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Mới">Mới</option>
              <option value="Đang xử lý">Đang xử lý</option>
              <option value="Đã giải quyết">Đã giải quyết</option>
              <option value="Đã đóng">Đã đóng</option>
            </select>
          </div>

          <!-- Priority -->
          <div>
            <select v-model="filterPriority" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả mức ưu tiên</option>
              <option value="Khẩn cấp">Khẩn cấp</option>
              <option value="Cao">Cao</option>
              <option value="Trung bình">Trung bình</option>
              <option value="Thấp">Thấp</option>
            </select>
          </div>

          <!-- Category -->
          <div>
            <select v-model="filterCategory" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả danh mục</option>
              <option value="Kỹ thuật">Kỹ thuật</option>
              <option value="Học vụ">Học vụ</option>
              <option value="Tài chính">Tài chính</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Ticket Table -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Mã Ticket</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Người gửi</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Tiêu đề</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Danh mục</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Ưu tiên</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Cập nhật</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredTickets.length === 0">
              <td colspan="8" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <LifeBuoy class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy ticket nào phù hợp với bộ lọc.</span>
                </div>
              </td>
            </tr>

            <tr
              v-for="ticket in filteredTickets"
              :key="ticket.id"
              class="transition-colors cursor-pointer hover:bg-surface-card-hover"
              @click="openDrawer(ticket)"
            >
              <td class="px-4 py-3.5">
                <span class="font-extrabold text-primary text-xs">{{ ticket.id }}</span>
              </td>
              <td class="px-4 py-3.5">
                <div class="font-bold text-heading text-xs">{{ ticket.studentName }}</div>
                <div class="text-[10px] text-muted">{{ ticket.studentId }} · {{ ticket.campus }}</div>
              </td>
              <td class="px-4 py-3.5 max-w-[280px]">
                <div class="font-semibold text-heading text-xs truncate">{{ ticket.subject }}</div>
                <div class="text-[10px] text-muted mt-0.5 flex items-center gap-1">
                  <MessageSquare class="w-3 h-3 flex-shrink-0" />
                  {{ ticket.replies.length }} phản hồi
                </div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  class="text-[10px] font-bold px-2 py-0.5 rounded-full border"
                  :class="getCategoryBadgeClass(ticket.category)"
                >
                  {{ ticket.category }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-1">
                  <component
                    :is="getPriorityIcon(ticket.priority).icon"
                    class="w-4 h-4"
                    :class="getPriorityIcon(ticket.priority).class"
                  />
                  <span class="text-[10px] font-bold" :class="getPriorityIcon(ticket.priority).class">
                    {{ ticket.priority }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="lg-badge text-[10px]" :class="getStatusBadgeClass(ticket.status)">
                  {{ ticket.status }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-[10px] text-muted font-medium">{{ ticket.updatedAt }}</span>
              </td>
              <td class="px-4 py-3.5 text-center" @click.stop>
                <button
                  @click="openDrawer(ticket)"
                  class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1 mx-auto"
                  title="Xem chi tiết ticket"
                >
                  <Eye class="w-3.5 h-3.5" />
                  Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Drawer - Ticket Detail -->
    <Transition name="drawer">
      <div v-if="isDrawerOpen" class="fixed inset-0 z-[100] flex justify-end">
        <!-- Scrim -->
        <div class="absolute inset-0 bg-slate-950/40 backdrop-blur-sm" @click="closeDrawer"></div>

        <!-- Drawer Panel -->
        <div class="relative w-full max-w-xl lg-glass-strong shadow-2xl flex flex-col h-full overflow-hidden">
          <!-- Drawer Header -->
          <div class="flex items-center justify-between px-5 py-4 border-b border-default flex-shrink-0">
            <div class="flex items-center gap-3">
              <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
                <MessageCircle class="w-5 h-5" />
              </div>
              <div>
                <h2 class="text-sm font-extrabold text-heading">{{ selectedTicket?.id }}</h2>
                <p class="text-[10px] text-muted">Chi tiết ticket hỗ trợ</p>
              </div>
            </div>
            <button @click="closeDrawer" class="lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default text-muted hover:text-heading">
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Drawer Body -->
          <div class="flex-1 overflow-y-auto px-5 py-4 space-y-4" v-if="selectedTicket">
            <!-- Ticket Info Card -->
            <div class="lg-glass-soft p-4 rounded-xl border border-default space-y-3">
              <h3 class="font-extrabold text-heading text-sm">{{ selectedTicket.subject }}</h3>

              <div class="grid grid-cols-2 gap-3 text-xs">
                <div class="flex items-center gap-2">
                  <User class="w-3.5 h-3.5 text-muted flex-shrink-0" />
                  <div>
                    <span class="text-muted block">Người gửi</span>
                    <span class="font-bold text-heading">{{ selectedTicket.studentName }}</span>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <Tag class="w-3.5 h-3.5 text-muted flex-shrink-0" />
                  <div>
                    <span class="text-muted block">Danh mục</span>
                    <span class="font-bold text-heading">{{ selectedTicket.category }}</span>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <Calendar class="w-3.5 h-3.5 text-muted flex-shrink-0" />
                  <div>
                    <span class="text-muted block">Tạo lúc</span>
                    <span class="font-bold text-heading">{{ selectedTicket.createdAt }}</span>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <Clock class="w-3.5 h-3.5 text-muted flex-shrink-0" />
                  <div>
                    <span class="text-muted block">Trạng thái</span>
                    <span class="lg-badge text-[10px]" :class="getStatusBadgeClass(selectedTicket.status)">{{ selectedTicket.status }}</span>
                  </div>
                </div>
              </div>

              <div class="pt-2 border-t border-default/50">
                <p class="text-xs text-body leading-relaxed">{{ selectedTicket.description }}</p>
              </div>
            </div>

            <!-- Replies Timeline -->
            <div>
              <h4 class="text-xs font-bold text-label uppercase mb-3 flex items-center gap-1.5">
                <MessageSquare class="w-4 h-4 text-primary" />
                Lịch sử trao đổi ({{ selectedTicket.replies.length }})
              </h4>

              <div v-if="selectedTicket.replies.length === 0" class="text-center py-6 text-muted text-xs">
                <MessageCircle class="w-6 h-6 mx-auto mb-2 text-muted" />
                Chưa có phản hồi nào cho ticket này.
              </div>

              <div v-else class="space-y-3">
                <div
                  v-for="reply in selectedTicket.replies"
                  :key="reply.id"
                  class="flex gap-3"
                  :class="reply.authorRole === 'admin' ? 'flex-row-reverse' : ''"
                >
                  <div
                    class="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 text-white text-[10px] font-bold"
                    :class="reply.authorRole === 'admin' ? 'bg-gradient-to-br from-blue-600 to-cyan-500' : 'bg-gradient-to-br from-violet-500 to-purple-600'"
                  >
                    {{ reply.author.charAt(0) }}
                  </div>
                  <div
                    class="flex-1 p-3 rounded-xl text-xs border max-w-[85%]"
                    :class="reply.authorRole === 'admin'
                      ? 'bg-primary/5 border-primary/20 text-heading'
                      : 'bg-surface-card border-default text-body'"
                  >
                    <div class="flex items-center justify-between mb-1.5">
                      <span class="font-bold text-heading text-[11px]">{{ reply.author }}</span>
                      <span class="text-[9px] text-muted">{{ reply.createdAt }}</span>
                    </div>
                    <p class="leading-relaxed">{{ reply.content }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Drawer Footer - Reply Box -->
          <div class="flex-shrink-0 px-5 py-4 border-t border-default bg-surface-card" v-if="selectedTicket && selectedTicket.status !== 'Đã đóng'">
            <!-- Action Buttons -->
            <div class="flex items-center gap-2 mb-3" v-if="selectedTicket.status !== 'Đã giải quyết'">
              <button
                @click="handleResolveTicket"
                class="lg-btn-success text-xs px-3 py-1.5 flex items-center gap-1 font-bold"
              >
                <CheckCircle class="w-3.5 h-3.5" />
                Đánh dấu giải quyết
              </button>
              <button
                @click="handleCloseTicket"
                class="lg-btn-secondary text-xs px-3 py-1.5 flex items-center gap-1"
              >
                <XCircle class="w-3.5 h-3.5" />
                Đóng ticket
              </button>
            </div>

            <!-- Reply input -->
            <div class="flex gap-2">
              <textarea
                v-model="newReplyText"
                rows="2"
                placeholder="Nhập phản hồi cho ticket này..."
                class="flex-1 px-3 py-2 lg-control text-sm resize-none"
              ></textarea>
              <button
                @click="handleSendReply"
                :disabled="!newReplyText.trim()"
                class="lg-btn-primary px-4 py-2 self-end disabled:opacity-40 disabled:cursor-not-allowed"
                title="Gửi phản hồi"
              >
                <Send class="w-4 h-4" />
              </button>
            </div>
          </div>

          <div v-else-if="selectedTicket && selectedTicket.status === 'Đã đóng'" class="flex-shrink-0 px-5 py-4 border-t border-default bg-surface-card">
            <div class="flex items-center gap-2 text-xs text-muted font-semibold">
              <CheckCircle class="w-4 h-4 text-emerald-500" />
              Ticket này đã được đóng và không thể phản hồi thêm.
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