<script setup>
/**
 * ApprovalsHistoryView.vue - Super Admin
 * Lịch sử duyệt đơn - Hiển thị tất cả đơn đã được xử lý (Đã duyệt/Từ chối).
 * Cho phép tìm kiếm, lọc theo loại đơn, trạng thái, và xem chi tiết timeline.
 */
import { ref, computed } from 'vue'
import {
  FolderCheck,
  Search,
  Filter,
  RotateCcw,
  Eye,
  X,
  CheckCircle,
  XCircle,
  Clock,
  Info,
  User,
  Calendar,
  FileText,
  Download,
  Check,
  CircleDot,
  Circle,
  ArrowRight
} from 'lucide-vue-next'

// --- Mock Data ---
const historyRecords = ref([
  {
    id: 'REQ-2026-H001',
    studentId: 'HE170050',
    studentName: 'Đỗ Thanh Tùng',
    campus: 'Cơ sở Hòa Lạc',
    type: 'retake',
    typeName: 'Xin thi lại / Phúc khảo',
    subject: 'Xin phúc khảo bài thi cuối kỳ môn MAD101',
    status: 'Đã duyệt',
    createdAt: '2026-06-01 10:00',
    resolvedAt: '2026-06-04 15:30',
    resolvedBy: 'Super Admin',
    processingDays: 3,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-06-01 10:00' },
      { label: 'Giáo vụ xem xét', status: 'completed', date: '2026-06-02 09:00' },
      { label: 'Trưởng BM phê duyệt', status: 'completed', date: '2026-06-03 14:00' },
      { label: 'Super Admin xác nhận', status: 'completed', date: '2026-06-04 15:30' }
    ],
    finalComment: 'Phúc khảo hợp lệ. Bài thi đã được chấm lại bởi hội đồng 2 giảng viên.'
  },
  {
    id: 'REQ-2026-H002',
    studentId: 'HE170088',
    studentName: 'Lý Minh Quang',
    campus: 'Cơ sở TP.HCM',
    type: 'withdraw',
    typeName: 'Rút môn học',
    subject: 'Xin rút môn PRN221 kỳ Spring 2026',
    status: 'Từ chối',
    createdAt: '2026-05-28 14:30',
    resolvedAt: '2026-06-01 10:00',
    resolvedBy: 'Super Admin',
    processingDays: 4,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-05-28 14:30' },
      { label: 'Giáo vụ xem xét', status: 'completed', date: '2026-05-29 09:00' },
      { label: 'Trưởng BM phê duyệt', status: 'rejected', date: '2026-06-01 10:00' }
    ],
    finalComment: 'Từ chối: Đã quá thời hạn rút môn theo quy chế (tuần 4). SV cần hoàn thành môn học.'
  },
  {
    id: 'REQ-2026-H003',
    studentId: 'HE170120',
    studentName: 'Nguyễn Bảo Ngọc',
    campus: 'Cơ sở Đà Nẵng',
    type: 'leave',
    typeName: 'Xin phép vắng mặt',
    subject: 'Xin nghỉ phép 2 ngày tham gia cuộc thi Hackathon',
    status: 'Đã duyệt',
    createdAt: '2026-05-25 08:00',
    resolvedAt: '2026-05-27 11:30',
    resolvedBy: 'Super Admin',
    processingDays: 2,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-05-25 08:00' },
      { label: 'Giáo vụ xem xét', status: 'completed', date: '2026-05-26 10:00' },
      { label: 'Super Admin xác nhận', status: 'completed', date: '2026-05-27 11:30' }
    ],
    finalComment: 'Đã duyệt. SV có giấy mời tham gia Hackathon cấp quốc gia, được tính hoạt động ngoại khóa.'
  },
  {
    id: 'REQ-2026-H004',
    studentId: 'HE170200',
    studentName: 'Trương Thị Mai',
    campus: 'Cơ sở Hòa Lạc',
    type: 'certificate',
    typeName: 'Cấp giấy xác nhận',
    subject: 'Xin cấp giấy xác nhận sinh viên (3 bản)',
    status: 'Đã duyệt',
    createdAt: '2026-05-20 09:15',
    resolvedAt: '2026-05-22 16:00',
    resolvedBy: 'Phòng Đào tạo',
    processingDays: 2,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-05-20 09:15' },
      { label: 'Phòng đào tạo xử lý', status: 'completed', date: '2026-05-22 16:00' }
    ],
    finalComment: 'Giấy xác nhận đã được in, ký và đóng dấu. SV lên phòng ĐT tầng 2 nhận.'
  },
  {
    id: 'REQ-2026-H005',
    studentId: 'HE170155',
    studentName: 'Hoàng Đức Hải',
    campus: 'Cơ sở TP.HCM',
    type: 'transfer',
    typeName: 'Chuyển cơ sở',
    subject: 'Xin chuyển sang CS Đà Nẵng từ kỳ Summer 2026',
    status: 'Đã duyệt',
    createdAt: '2026-05-15 11:00',
    resolvedAt: '2026-05-25 09:30',
    resolvedBy: 'Super Admin',
    processingDays: 10,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-05-15 11:00' },
      { label: 'GV CS HCM xem xét', status: 'completed', date: '2026-05-17 14:00' },
      { label: 'GV CS ĐN xác nhận', status: 'completed', date: '2026-05-20 10:00' },
      { label: 'Trưởng khoa phê duyệt', status: 'completed', date: '2026-05-23 16:00' },
      { label: 'Super Admin xác nhận', status: 'completed', date: '2026-05-25 09:30' }
    ],
    finalComment: 'SV đủ điều kiện chuyển cơ sở. GPA đạt chuẩn (3.1/4.0). Đã cập nhật hệ thống.'
  },
  {
    id: 'REQ-2026-H006',
    studentId: 'HE170078',
    studentName: 'Vũ Thị Hồng',
    campus: 'Cơ sở Hòa Lạc',
    type: 'other',
    typeName: 'Khác',
    subject: 'Xin gia hạn nộp đồ án tốt nghiệp thêm 2 tuần',
    status: 'Từ chối',
    createdAt: '2026-05-10 13:00',
    resolvedAt: '2026-05-14 09:00',
    resolvedBy: 'Super Admin',
    processingDays: 4,
    steps: [
      { label: 'SV nộp đơn', status: 'completed', date: '2026-05-10 13:00' },
      { label: 'GV hướng dẫn nhận xét', status: 'completed', date: '2026-05-12 10:00' },
      { label: 'Trưởng BM phê duyệt', status: 'completed', date: '2026-05-13 14:00' },
      { label: 'Super Admin xác nhận', status: 'rejected', date: '2026-05-14 09:00' }
    ],
    finalComment: 'Từ chối: SV không có lý do bất khả kháng. Thời hạn nộp ĐATN là chung cho toàn trường.'
  }
])

// --- Filter State ---
const searchQuery = ref('')
const filterType = ref('all')
const filterStatus = ref('all')

const filteredHistory = computed(() => {
  return historyRecords.value.filter(r => {
    const matchSearch = searchQuery.value === '' ||
      r.id.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.studentName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.subject.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === 'all' || r.type === filterType.value
    const matchStatus = filterStatus.value === 'all' || r.status === filterStatus.value
    return matchSearch && matchType && matchStatus
  })
})

const resetFilters = () => {
  searchQuery.value = ''
  filterType.value = 'all'
  filterStatus.value = 'all'
}

// --- KPI ---
const totalProcessed = computed(() => historyRecords.value.length)
const approvedCount = computed(() => historyRecords.value.filter(r => r.status === 'Đã duyệt').length)
const rejectedCount = computed(() => historyRecords.value.filter(r => r.status === 'Từ chối').length)
const avgProcessingDays = computed(() => {
  const total = historyRecords.value.reduce((s, r) => s + r.processingDays, 0)
  return (total / historyRecords.value.length).toFixed(1)
})

// --- Detail Modal ---
const isDetailOpen = ref(false)
const selectedRecord = ref(null)

const openDetail = (record) => {
  selectedRecord.value = record
  isDetailOpen.value = true
}

// --- Helpers ---
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

const getStepIcon = (status) => {
  switch (status) {
    case 'completed': return { icon: Check, class: 'bg-emerald-500 text-white' }
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

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Page Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <FolderCheck class="w-8 h-8 text-primary" />
            Lịch Sử Duyệt Đơn
          </h1>
          <p class="text-sm text-muted mt-1">
            Tra cứu toàn bộ lịch sử xử lý đơn từ sinh viên đã được phê duyệt hoặc từ chối.
          </p>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <FolderCheck class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng đã xử lý</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalProcessed }}</div>
          </div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã phê duyệt</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ approvedCount }}</div>
          </div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <XCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã từ chối</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ rejectedCount }}</div>
          </div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Clock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">TB thời gian xử lý</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ avgProcessingDays }} ngày</div>
          </div>
        </div>
      </div>

      <!-- Filter Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Bộ lọc lịch sử</h3>
          </div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline">
            <RotateCcw class="w-3.5 h-3.5" /> Xóa bộ lọc
          </button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
            <input v-model="searchQuery" type="text" placeholder="Tìm theo mã đơn, tên SV..." class="w-full pl-9 pr-3 lg-control text-sm" />
          </div>
          <div>
            <select v-model="filterType" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả loại đơn</option>
              <option value="leave">Xin phép vắng mặt</option>
              <option value="retake">Xin thi lại / Phúc khảo</option>
              <option value="withdraw">Rút môn học</option>
              <option value="transfer">Chuyển cơ sở</option>
              <option value="certificate">Cấp giấy xác nhận</option>
              <option value="other">Khác</option>
            </select>
          </div>
          <div>
            <select v-model="filterStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Đã duyệt">Đã duyệt</option>
              <option value="Từ chối">Từ chối</option>
            </select>
          </div>
        </div>
      </div>

      <!-- History Table -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Mã đơn</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Sinh viên</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Tiêu đề</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Loại đơn</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Kết quả</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Xử lý</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Ngày hoàn tất</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredHistory.length === 0">
              <td colspan="8" class="px-4 py-12 text-center text-muted">
                <FolderCheck class="w-8 h-8 mx-auto mb-2 text-muted" />
                <span>Không tìm thấy bản ghi nào.</span>
              </td>
            </tr>
            <tr v-for="record in filteredHistory" :key="record.id" class="transition-colors hover:bg-surface-card-hover">
              <td class="px-4 py-3.5"><span class="font-extrabold text-primary text-xs">{{ record.id }}</span></td>
              <td class="px-4 py-3.5">
                <div class="font-bold text-heading text-xs">{{ record.studentName }}</div>
                <div class="text-[10px] text-muted">{{ record.studentId }} · {{ record.campus }}</div>
              </td>
              <td class="px-4 py-3.5 max-w-[240px]">
                <div class="font-semibold text-heading text-xs truncate">{{ record.subject }}</div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full border" :class="getTypeBadgeClass(record.type)">
                  {{ record.typeName }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="lg-badge text-[10px]" :class="record.status === 'Đã duyệt' ? 'lg-badge-success' : 'lg-badge-danger'">
                  {{ record.status }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-xs font-bold text-heading">{{ record.processingDays }} ngày</span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-[10px] text-muted font-medium">{{ record.resolvedAt }}</span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <button @click="openDetail(record)" class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1 mx-auto">
                  <Eye class="w-3.5 h-3.5" /> Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <div v-if="isDetailOpen && selectedRecord" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-2xl lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative max-h-[90vh] overflow-y-auto">
        <button @click="isDetailOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default">
          <X class="w-5 h-5" />
        </button>

        <div class="mb-5 pb-4 border-b border-default">
          <div class="flex items-center gap-2 mb-2">
            <span class="text-xs font-extrabold text-primary">{{ selectedRecord.id }}</span>
            <span class="lg-badge text-[10px]" :class="selectedRecord.status === 'Đã duyệt' ? 'lg-badge-success' : 'lg-badge-danger'">
              {{ selectedRecord.status }}
            </span>
          </div>
          <h2 class="text-lg font-extrabold text-heading">{{ selectedRecord.subject }}</h2>
          <div class="flex items-center gap-4 text-xs text-muted mt-2">
            <span>SV: {{ selectedRecord.studentName }} ({{ selectedRecord.studentId }})</span>
            <span>CS: {{ selectedRecord.campus }}</span>
            <span>Xử lý: {{ selectedRecord.processingDays }} ngày</span>
          </div>
        </div>

        <!-- Timeline -->
        <div class="mb-5">
          <h4 class="text-xs font-bold text-label uppercase mb-3 flex items-center gap-1.5">
            <ArrowRight class="w-4 h-4 text-primary" />
            Quy trình xử lý
          </h4>
          <div class="relative ml-3 space-y-0">
            <div v-for="(step, idx) in selectedRecord.steps" :key="idx" class="flex items-start gap-3 relative" :class="idx < selectedRecord.steps.length - 1 ? 'pb-5' : ''">
              <div v-if="idx < selectedRecord.steps.length - 1" class="absolute left-3.5 top-7 w-0.5 bottom-0" :class="step.status === 'completed' ? 'bg-emerald-500' : 'bg-rose-500'"></div>
              <div class="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 z-10" :class="getStepIcon(step.status).class">
                <component :is="getStepIcon(step.status).icon" class="w-3.5 h-3.5" />
              </div>
              <div class="flex-1 pt-0.5">
                <div class="text-xs font-bold text-heading">{{ step.label }}</div>
                <div class="text-[10px] text-muted">{{ step.date }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Final Comment -->
        <div class="lg-alert" :class="selectedRecord.status === 'Đã duyệt' ? 'lg-alert-success' : 'lg-alert-error'">
          <div class="flex gap-2">
            <component :is="selectedRecord.status === 'Đã duyệt' ? CheckCircle : XCircle" class="w-5 h-5 flex-shrink-0 mt-0.5 text-current opacity-90" />
            <div class="text-xs font-bold leading-relaxed text-current">
              <strong>Kết luận:</strong> {{ selectedRecord.finalComment }}
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end pt-4 border-t border-default mt-5">
          <button @click="isDetailOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Đóng</button>
        </div>
      </div>
    </div>
  </div>
</template>