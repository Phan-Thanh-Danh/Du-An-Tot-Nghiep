<script setup>
/**
 * AttendancePolicyView.vue - Super Admin
 * Giao diện quản trị chính sách chuyên cần, cấu hình quỹ vắng (Quota) và duyệt mở khóa điểm danh (Unlock Requests).
 * Tích hợp báo cáo Heatmap chuyên cần trực quan (Module M18) và Audit Logs bảo toàn dữ liệu.
 */
import { ref, computed } from 'vue'
import {
  Users,
  BookOpen,
  Clock,
  AlertTriangle,
  CheckCircle,
  Unlock,
  FileSpreadsheet,
  History,
  Save,
  X,
  Filter
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách Cơ sở
const campuses = ref([
  { id: 1, name: 'Cơ sở Hòa Lạc' },
  { id: 2, name: 'Cơ sở TP.HCM' },
  { id: 3, name: 'Cơ sở Đà Nẵng' }
])

// Danh sách Học kỳ
const semesters = ref(['Spring 2026', 'Summer 2026', 'Fall 2025'])

// Danh sách Môn học
const subjects = ref([
  { code: 'PRN211', name: 'Lập trình C# (.NET)' },
  { code: 'SWD392', name: 'Thiết kế & Kiến trúc phần mềm' },
  { code: 'PRN221', name: 'Lập trình ứng dụng Windows' },
  { code: 'SWE201c', name: 'Nhập môn kỹ nghệ phần mềm' }
])

// 1. Cấu hình quỹ vắng tối đa (Quota Config Table)
const quotaConfigs = ref([
  {
    id: 1,
    subjectCode: 'PRN211',
    subjectName: 'Lập trình C# (.NET)',
    semester: 'Spring 2026',
    totalSlots: 30,
    maxAbsentSlots: 6, // 20% của 30 slot
    lastUpdated: '2026-05-02 08:30:00'
  },
  {
    id: 2,
    subjectCode: 'SWD392',
    subjectName: 'Thiết kế & Kiến trúc phần mềm',
    semester: 'Spring 2026',
    totalSlots: 20,
    maxAbsentSlots: 4, // 20% của 20 slot
    lastUpdated: '2026-05-02 08:32:00'
  },
  {
    id: 3,
    subjectCode: 'PRN221',
    subjectName: 'Lập trình ứng dụng Windows',
    semester: 'Spring 2026',
    totalSlots: 30,
    maxAbsentSlots: 6,
    lastUpdated: '2026-05-02 08:35:00'
  },
  {
    id: 4,
    subjectCode: 'SWE201c',
    subjectName: 'Nhập môn kỹ nghệ phần mềm',
    semester: 'Spring 2026',
    totalSlots: 20,
    maxAbsentSlots: 4,
    lastUpdated: '2026-05-02 08:40:00'
  }
])

// 2. Yêu cầu xin mở khóa điểm danh (Attendance Unlock Requests)
const unlockRequests = ref([
  {
    id: 501,
    teacherName: 'Trần Văn Hùng',
    teacherEmail: 'hungtv@fpt.edu.vn',
    className: 'SE1701',
    subjectCode: 'PRN211',
    date: '2026-06-08',
    slotName: 'Slot 2 (09:45 - 11:15)',
    reason: 'Mất điện đột ngột tại phòng Lab cuối giờ nên không thể chốt điểm danh kịp lúc.',
    unlockCount: 0, // Số lần mở khóa trước đó (Giới hạn tối đa 1 lần duy nhất)
    status: 'Pending' // Pending, Approved, Rejected
  },
  {
    id: 502,
    teacherName: 'Nguyễn Thị Lan',
    teacherEmail: 'lannt@fpt.edu.vn',
    className: 'SE1702',
    subjectCode: 'SWD392',
    date: '2026-06-07',
    slotName: 'Slot 4 (13:30 - 15:00)',
    reason: 'Hệ thống LMS chập chờn lúc cuối ca, quên không nhấn nút lưu điểm danh.',
    unlockCount: 1, // Đã mở khóa 1 lần trước đó -> Sẽ bị chặn không cho duyệt nữa
    status: 'Pending'
  },
  {
    id: 503,
    teacherName: 'Lê Hoàng Long',
    teacherEmail: 'longlh@fpt.edu.vn',
    className: 'SE1703',
    subjectCode: 'PRN221',
    date: '2026-06-05',
    slotName: 'Slot 1 (08:00 - 09:30)',
    reason: 'Đi công tác khẩn cấp quên điểm danh bù ca dạy thay.',
    unlockCount: 0,
    status: 'Approved',
    approvedAt: '2026-06-06 09:00',
    unlockDuration: 20 // 20 phút
  }
])

// 3. Danh sách các buổi học quá hạn chưa điểm danh (UNCONFIRMED Sessions)
const unconfirmedSessions = ref([
  {
    id: 2001,
    className: 'SE1701',
    subjectCode: 'PRN211',
    teacherName: 'Trần Văn Hùng',
    date: '2026-06-08',
    slotName: 'Slot 2 (09:45 - 11:15)',
    status: 'Unlock Requested' // Có request đi kèm
  },
  {
    id: 2002,
    className: 'IA1701',
    subjectCode: 'SWE201c',
    teacherName: 'Phạm Văn Hải',
    date: '2026-06-08',
    slotName: 'Slot 3 (12:30 - 14:00)',
    status: 'Unconfirmed' // Không có request, cần admin nhắc nhở/giải quyết
  }
])

// 4. Sinh viên vượt ngưỡng quỹ vắng (Warning metrics)
const warnedStudents = ref([
  {
    id: 'HE170001',
    name: 'Nguyễn Văn An',
    className: 'SE1701',
    subjectCode: 'PRN211',
    absentSlots: 5,
    maxQuota: 6,
    absentPercentage: 83, // 83% của quỹ vắng
    status: 'Cảnh cáo'
  },
  {
    id: 'HE170102',
    name: 'Hoàng Minh Em',
    className: 'SE1702',
    subjectCode: 'SWD392',
    absentSlots: 4,
    maxQuota: 4,
    absentPercentage: 100, // 100% quỹ vắng
    status: 'Chờ cấm thi'
  },
  {
    id: 'HE170203',
    name: 'Lương Bảo Giang',
    className: 'SE1703',
    subjectCode: 'PRN221',
    absentSlots: 4,
    maxQuota: 6,
    absentPercentage: 66, // 66% của quỹ vắng
    status: 'Cảnh cáo'
  }
])

// 5. Nhật ký hoạt động điều chỉnh (Audit Logs)
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 10:15:30',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Cấu hình Quỹ vắng',
    details: 'Thay đổi Quỹ vắng PRN211 (Spring 2026): số buổi vắng từ 6 lên 7 buổi',
    reason: 'Phê duyệt điều chỉnh từ Trưởng bộ môn Phần mềm do tăng số ca bổ trợ.'
  },
  {
    id: 2,
    time: '2026-06-07 15:40:12',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Duyệt Mở khóa',
    details: 'Phê duyệt mở khóa điểm danh ca thi ID 503 cho giảng viên Lê Hoàng Long lớp SE1703',
    reason: 'Đã xác thực lý do đi công tác đột xuất có phép của Ban Đào tạo.'
  }
])

// 6. Dữ liệu Heatmap báo cáo chuyên cần (Lưới 6 ngày học x 8 tuần kỳ Spring 2026)
// Biểu thị tỷ lệ chuyên cần: xanh đậm (>95%), xanh nhạt (90-95%), vàng (80-90%), đỏ (<80%)
const heatmapWeeks = ref([
  { week: 1, days: [98, 97, 96, 95, 98, 94] },
  { week: 2, days: [95, 96, 93, 94, 95, 90] },
  { week: 3, days: [94, 91, 90, 88, 92, 93] },
  { week: 4, days: [90, 89, 87, 85, 91, 88] },
  { week: 5, days: [92, 93, 89, 91, 93, 90] },
  { week: 6, days: [88, 86, 82, 80, 85, 83] }, // Tuần có xu hướng vắng nhiều
  { week: 7, days: [91, 94, 92, 89, 94, 93] },
  { week: 8, days: [89, 85, 78, 76, 82, 80]  } // Tuần chuẩn bị thi, nghỉ nhiều
])

// --- State Bộ lọc ---
const selectedCampus = ref('all')
const selectedSemester = ref('Spring 2026')
const selectedSubject = ref('all')
const activeSectionTab = ref('requests') // 'requests', 'quota', 'warning', 'unconfirmed'

// --- Thống kê KPI ---
const pendingRequestsCount = computed(() => unlockRequests.value.filter(r => r.status === 'Pending').length)
const unconfirmedCount = computed(() => unconfirmedSessions.value.filter(s => s.status === 'Unconfirmed').length)
const warnedStudentsCount = computed(() => warnedStudents.value.filter(s => s.status === 'Chờ cấm thi').length)
const systemAverageAttendance = ref(90.2) // % chuyên cần chung toàn hệ thống

// --- Bộ lọc các danh sách ---
const filteredUnlockRequests = computed(() => {
  return unlockRequests.value.filter(r => {
    const matchSubject = selectedSubject.value === 'all' || r.subjectCode === selectedSubject.value
    return matchSubject
  })
})

const filteredQuotaConfigs = computed(() => {
  return quotaConfigs.value.filter(q => {
    const matchSubject = selectedSubject.value === 'all' || q.subjectCode === selectedSubject.value
    const matchSemester = selectedSemester.value === 'all' || q.semester === selectedSemester.value
    return matchSubject && matchSemester
  })
})

// --- State Modals ---

// Modal Duyệt Mở khóa
const isUnlockModalOpen = ref(false)
const selectedRequest = ref(null)
const unlockReason = ref('')
const unlockDuration = ref(20) // Mặc định 20 phút

// Modal Sửa Quota
const isQuotaModalOpen = ref(false)
const selectedQuota = ref(null)
const newAbsentQuota = ref(0)
const quotaChangeReason = ref('')

// State Export
const isExporting = ref(false)
const showToast = ref(false)
const toastMessage = ref('')

// --- Handlers ---

const triggerExport = () => {
  isExporting.value = true
  setTimeout(() => {
    isExporting.value = false
    toastMessage.value = `Xuất dữ liệu chuyên cần [${selectedSemester.value}] thành công: Attendance_Report_${new Date().toISOString().slice(0,10)}.xlsx`
    showToast.value = true
    setTimeout(() => {
      showToast.value = false
    }, 4000)
  }, 2000)
}

const openUnlockModal = (req) => {
  if (req.unlockCount >= 1) return // Ràng buộc nghiệp vụ: tối đa 1 lần
  selectedRequest.value = req
  unlockReason.value = ''
  unlockDuration.value = 20
  isUnlockModalOpen.value = true
}

const handleApproveUnlock = (isApproved) => {
  if (!selectedRequest.value) return

  const targetReq = unlockRequests.value.find(r => r.id === selectedRequest.value.id)
  if (!targetReq) return

  const timeString = new Date().toLocaleString('vi-VN')

  if (isApproved) {
    targetReq.status = 'Approved'
    targetReq.unlockCount += 1
    targetReq.approvedAt = timeString
    targetReq.unlockDuration = unlockDuration.value

    // Xóa khỏi danh sách UNCONFIRMED tạm thời hoặc đổi trạng thái
    const unconfirmed = unconfirmedSessions.value.find(s => s.className === targetReq.className && s.subjectCode === targetReq.subjectCode && s.date === targetReq.date)
    if (unconfirmed) {
      unconfirmed.status = 'Unlocked (Active)'
    }

    // Ghi Audit Log
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Duyệt Mở khóa',
      details: `Phê duyệt mở khóa ca học lớp ${targetReq.className} môn ${targetReq.subjectCode} trong ${unlockDuration.value} phút.`,
      reason: unlockReason.value || 'Giảng viên gửi yêu cầu xác thực lý do chính đáng'
    })
  } else {
    targetReq.status = 'Rejected'
    // Ghi Audit Log
    auditLogs.value.unshift({
      id: auditLogs.value.length + 1,
      time: timeString,
      actor: 'Super Admin (admin@fpt.edu.vn)',
      action: 'Từ chối Mở khóa',
      details: `Từ chối mở khóa ca học lớp ${targetReq.className} môn ${targetReq.subjectCode} của GV ${targetReq.teacherName}.`,
      reason: unlockReason.value || 'Lý do xin mở khóa không chính đáng hoặc trễ hạn quy định'
    })
  }

  isUnlockModalOpen.value = false
}

const openQuotaModal = (quota) => {
  selectedQuota.value = quota
  newAbsentQuota.value = quota.maxAbsentSlots
  quotaChangeReason.value = ''
  isQuotaModalOpen.value = true
}

const handleSaveQuota = () => {
  if (!selectedQuota.value) return

  const targetQuota = quotaConfigs.value.find(q => q.id === selectedQuota.value.id)
  if (!targetQuota) return

  const timeString = new Date().toLocaleString('vi-VN')
  const oldVal = targetQuota.maxAbsentSlots

  targetQuota.maxAbsentSlots = newAbsentQuota.value
  targetQuota.lastUpdated = timeString

  // Ghi Audit Log bắt buộc lưu giá trị cũ và mới
  auditLogs.value.unshift({
    id: auditLogs.value.length + 1,
    time: timeString,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    action: 'Cấu hình Quỹ vắng',
    details: `Sửa Quỹ vắng môn ${targetQuota.subjectCode} (${targetQuota.semester}): thay đổi giới hạn vắng từ ${oldVal} lên ${newAbsentQuota.value} buổi`,
    reason: quotaChangeReason.value || 'Điều chỉnh quy định học vụ môn học'
  })

  isQuotaModalOpen.value = false
}

// Helper màu sắc cho ô Heatmap chuyên cần
const getHeatmapColorClass = (val) => {
  if (val >= 95) return 'bg-emerald-500 hover:bg-emerald-600 text-white'
  if (val >= 90) return 'bg-emerald-400/70 hover:bg-emerald-400 text-slate-800'
  if (val >= 80) return 'bg-amber-400 hover:bg-amber-500 text-slate-800'
  return 'bg-rose-500 hover:bg-rose-600 text-white'
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Quả cầu trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl bg-emerald-500 text-white border border-emerald-400 flex items-center gap-2 animate-in fade-in slide-in-from-bottom duration-300"
    >
      <CheckCircle class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Unlock class="w-8 h-8 text-primary" />
            Quỹ Vắng & Chuyên Cần
          </h1>
          <p class="text-sm text-muted mt-1">
            Quản trị chính sách điểm danh, thiết lập quota vắng môn học, duyệt yêu cầu mở khóa bổ sung và giám sát chuyên cần.
          </p>
        </div>

        <div class="flex items-center gap-2">
          <!-- Button Export Excel -->
          <button
            @click="triggerExport"
            :disabled="isExporting"
            class="lg-btn-primary px-4 py-2 text-sm font-bold flex items-center gap-2 disabled:opacity-50"
          >
            <span v-if="isExporting" class="lg-shell-loading-spinner w-4 h-4 border-2"></span>
            <FileSpreadsheet v-else class="w-4 h-4" />
            {{ isExporting ? 'Đang trích xuất...' : 'Export Báo Cáo' }}
          </button>
        </div>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- KPI 1 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Unlock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Chờ mở khóa</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ pendingRequestsCount }} ca</div>
          </div>
        </div>

        <!-- KPI 2 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <Clock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Chưa điểm danh</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ unconfirmedCount }} ca</div>
          </div>
        </div>

        <!-- KPI 3 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-red-500/10 flex items-center justify-center text-red-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Nguy cơ cấm thi</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ warnedStudentsCount }} HS</div>
          </div>
        </div>

        <!-- KPI 4 -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <Users class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Chuyên cần hệ thống</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ systemAverageAttendance }}%</div>
          </div>
        </div>
      </div>

      <!-- Lớp Biểu đồ nhiệt (Attendance Heatmap - Module M18) -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between border-b border-default pb-3 mb-4 gap-2">
          <div class="flex items-center gap-2">
            <BookOpen class="w-5 h-5 text-primary" />
            <div>
              <h3 class="font-extrabold text-heading text-sm">Biểu đồ nhiệt chuyên cần (Module M18)</h3>
              <p class="text-xs text-muted mt-0.5">Tỷ lệ hiện diện học tập theo tuần/ngày của kỳ Spring 2026</p>
            </div>
          </div>
          <div class="flex items-center gap-3 text-xs">
            <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded bg-emerald-500"></span> >95%</span>
            <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded bg-emerald-400/70"></span> 90-95%</span>
            <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded bg-amber-400"></span> 80-90%</span>
            <span class="flex items-center gap-1"><span class="w-2.5 h-2.5 rounded bg-rose-500"></span> &lt;80%</span>
          </div>
        </div>

        <!-- SVG/CSS Grid Heatmap -->
        <div class="overflow-x-auto">
          <div class="min-w-[760px] grid grid-cols-7 gap-2 pb-2">
            <!-- Labels -->
            <div class="font-bold text-xs text-muted text-center pt-2">Tuần học</div>
            <div v-for="d in 6" :key="d" class="font-bold text-xs text-muted text-center pt-2">Thứ {{ d + 1 }}</div>

            <!-- Heatmap Rows -->
            <template v-for="w in heatmapWeeks" :key="w.week">
              <div class="font-bold text-xs text-heading flex items-center justify-center border border-default bg-surface-card rounded-lg py-3">
                Tuần {{ w.week }}
              </div>
              <div 
                v-for="(dayVal, idx) in w.days" 
                :key="idx" 
                class="rounded-lg p-2.5 flex flex-col items-center justify-center transition-all duration-150 cursor-pointer shadow-sm text-xs font-bold border border-default/20"
                :class="getHeatmapColorClass(dayVal)"
              >
                <span>{{ dayVal }}%</span>
                <span class="text-[9px] font-normal opacity-80 mt-0.5">Hiện diện</span>
              </div>
            </template>
          </div>
        </div>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <Filter class="w-4 h-4 text-primary" />
          <h3 class="font-bold text-heading text-sm">Bộ lọc chính sách & chuyên cần</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <!-- Chọn Cơ sở -->
          <div>
            <select v-model="selectedCampus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả cơ sở đào tạo</option>
              <option v-for="camp in campuses" :key="camp.id" :value="camp.id">{{ camp.name }}</option>
            </select>
          </div>

          <!-- Lọc Học kỳ -->
          <div>
            <select v-model="selectedSemester" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả học kỳ</option>
              <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
            </select>
          </div>

          <!-- Lọc Môn học -->
          <div>
            <select v-model="selectedSubject" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả môn học</option>
              <option v-for="sub in subjects" :key="sub.code" :value="sub.code">
                [{{ sub.code }}] {{ sub.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- Tabbed Panels: Unlock Requests, Quota Config, Cảnh báo vắng -->
      <div class="mb-8">
        <!-- Navigation Tabs -->
        <div class="flex border-b border-default mb-4 overflow-x-auto gap-2">
          <button 
            @click="activeSectionTab = 'requests'"
            class="px-4 py-2.5 text-sm font-bold border-b-2 transition-colors flex items-center gap-2 whitespace-nowrap"
            :class="activeSectionTab === 'requests' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            <Unlock class="w-4 h-4" />
            Yêu cầu mở khóa điểm danh
            <span class="px-1.5 py-0.5 text-xs rounded-full bg-amber-500 text-white font-bold">{{ pendingRequestsCount }}</span>
          </button>
          
          <button 
            @click="activeSectionTab = 'quota'"
            class="px-4 py-2.5 text-sm font-bold border-b-2 transition-colors flex items-center gap-2 whitespace-nowrap"
            :class="activeSectionTab === 'quota' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            <BookOpen class="w-4 h-4" />
            Cấu hình Quỹ vắng môn học
          </button>

          <button 
            @click="activeSectionTab = 'unconfirmed'"
            class="px-4 py-2.5 text-sm font-bold border-b-2 transition-colors flex items-center gap-2 whitespace-nowrap"
            :class="activeSectionTab === 'unconfirmed' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            <Clock class="w-4 h-4" />
            Buổi học chưa điểm danh
            <span class="px-1.5 py-0.5 text-xs rounded-full bg-rose-500 text-white font-bold">{{ unconfirmedCount }}</span>
          </button>

          <button 
            @click="activeSectionTab = 'warning'"
            class="px-4 py-2.5 text-sm font-bold border-b-2 transition-colors flex items-center gap-2 whitespace-nowrap"
            :class="activeSectionTab === 'warning' ? 'border-primary text-primary' : 'border-transparent text-muted hover:text-heading'"
          >
            <AlertTriangle class="w-4 h-4" />
            Cảnh báo ngưỡng vắng mặt
          </button>
        </div>

        <!-- TAB CONTENT: 1. Unlock Requests -->
        <div v-if="activeSectionTab === 'requests'" class="lg-table-shell overflow-x-auto">
          <table class="min-w-full divide-y divide-default text-sm">
            <thead>
              <tr class="surface-table-header">
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Yêu cầu</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Giảng viên xin mở</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Lớp & Môn học</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Buổi học xin duyệt</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Số lần đã duyệt</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Hành động</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-if="filteredUnlockRequests.length === 0">
                <td colspan="7" class="px-4 py-12 text-center text-muted">
                  <div class="flex flex-col items-center gap-1.5">
                    <CheckCircle class="w-8 h-8 text-emerald-500" />
                    <span>Không có yêu cầu mở khóa nào cần phê duyệt.</span>
                  </div>
                </td>
              </tr>

              <tr v-for="req in filteredUnlockRequests" :key="req.id">
                <td class="px-4 py-3 font-semibold text-heading">#{{ req.id }}</td>
                <td class="px-4 py-3">
                  <div class="font-bold text-heading">{{ req.teacherName }}</div>
                  <div class="text-xs text-muted">{{ req.teacherEmail }}</div>
                </td>
                <td class="px-4 py-3">
                  <div class="font-bold text-heading">Lớp: {{ req.className }}</div>
                  <div class="text-xs text-primary font-semibold">Môn: {{ req.subjectCode }}</div>
                </td>
                <td class="px-4 py-3">
                  <div class="font-semibold text-body">{{ req.date }}</div>
                  <div class="text-xs text-muted">{{ req.slotName }}</div>
                </td>
                <td class="px-4 py-3 text-center">
                  <span 
                    class="font-bold text-xs"
                    :class="req.unlockCount >= 1 ? 'text-rose-500' : 'text-slate-500'"
                  >
                    {{ req.unlockCount }} / 1 lần
                  </span>
                </td>
                <td class="px-4 py-3 text-center">
                  <span 
                    class="lg-badge"
                    :class="req.status === 'Pending' ? 'lg-badge-warning' : 
                            req.status === 'Approved' ? 'lg-badge-success' : 'lg-badge-danger'"
                  >
                    {{ req.status === 'Pending' ? 'Đang chờ duyệt' : 
                       req.status === 'Approved' ? 'Đã duyệt' : 'Đã từ chối' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-center">
                  <div class="flex items-center justify-center gap-1.5">
                    <button
                      v-if="req.status === 'Pending'"
                      @click="openUnlockModal(req)"
                      :disabled="req.unlockCount >= 1"
                      class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"
                      :class="req.unlockCount >= 1 ? 'opacity-40 cursor-not-allowed' : ''"
                      :title="req.unlockCount >= 1 ? 'Quy chế chỉ cho phép mở khóa 1 lần duy nhất' : 'Duyệt mở khóa điểm danh'"
                    >
                      <Unlock class="w-3.5 h-3.5" />
                      Phê duyệt
                    </button>
                    <span v-else class="text-xs text-muted font-medium">Không thể thao tác</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- TAB CONTENT: 2. Quota Config -->
        <div v-if="activeSectionTab === 'quota'" class="lg-table-shell overflow-x-auto animate-in fade-in duration-200">
          <table class="min-w-full divide-y divide-default text-sm">
            <thead>
              <tr class="surface-table-header">
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Mã môn</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Tên môn học</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Học kỳ áp dụng</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Tổng số buổi</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Quỹ vắng tối đa (Hạn mức)</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Cập nhật lần cuối</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Hành động</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-if="filteredQuotaConfigs.length === 0">
                <td colspan="7" class="px-4 py-12 text-center text-muted">Không tìm thấy cấu hình môn học nào.</td>
              </tr>

              <tr v-for="q in filteredQuotaConfigs" :key="q.id">
                <td class="px-4 py-3 font-bold text-primary">{{ q.subjectCode }}</td>
                <td class="px-4 py-3 font-bold text-heading">{{ q.subjectName }}</td>
                <td class="px-4 py-3 text-body font-medium">{{ q.semester }}</td>
                <td class="px-4 py-3 text-center font-semibold">{{ q.totalSlots }} buổi</td>
                <td class="px-4 py-3 text-center">
                  <span class="lg-badge lg-badge-danger font-bold">{{ q.maxAbsentSlots }} buổi</span>
                  <span class="text-xs text-muted block mt-0.5">({{ Math.round((q.maxAbsentSlots/q.totalSlots)*100) }}% tổng tiết)</span>
                </td>
                <td class="px-4 py-3 text-center text-xs text-muted">{{ q.lastUpdated }}</td>
                <td class="px-4 py-3 text-center">
                  <button
                    @click="openQuotaModal(q)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1 mx-auto"
                  >
                    <Save class="w-3.5 h-3.5" />
                    Sửa Quota
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- TAB CONTENT: 3. UNCONFIRMED Sessions -->
        <div v-if="activeSectionTab === 'unconfirmed'" class="lg-table-shell overflow-x-auto animate-in fade-in duration-200">
          <table class="min-w-full divide-y divide-default text-sm">
            <thead>
              <tr class="surface-table-header">
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Môn học</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Lớp học</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Giảng viên</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Ngày dạy</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Ca dạy (Slot)</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Hành động can thiệp</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-if="unconfirmedSessions.length === 0">
                <td colspan="7" class="px-4 py-12 text-center text-muted">
                  <CheckCircle class="w-8 h-8 text-emerald-500 mx-auto mb-2" />
                  <span>Tất cả các ca học đã được điểm danh đầy đủ.</span>
                </td>
              </tr>

              <tr v-for="s in unconfirmedSessions" :key="s.id">
                <td class="px-4 py-3 font-bold text-heading">[{{ s.subjectCode }}]</td>
                <td class="px-4 py-3 font-bold text-primary">Lớp: {{ s.className }}</td>
                <td class="px-4 py-3 text-body font-medium">{{ s.teacherName }}</td>
                <td class="px-4 py-3 text-body">{{ s.date }}</td>
                <td class="px-4 py-3 text-body">{{ s.slotName }}</td>
                <td class="px-4 py-3 text-center">
                  <span 
                    class="lg-badge"
                    :class="s.status === 'Unlock Requested' ? 'lg-badge-warning' : 'lg-badge-danger'"
                  >
                    {{ s.status === 'Unlock Requested' ? 'Chờ mở khóa' : 'Chưa điểm danh' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-center">
                  <div class="flex items-center justify-center gap-1.5">
                    <span v-if="s.status === 'Unlock Requested'" class="text-xs text-muted italic">Xem tab Yêu cầu mở khóa</span>
                    <button
                      v-else
                      @click="toastMessage = 'Gửi email nhắc nhở thành công tới giảng viên ' + s.teacherName; showToast = true;"
                      class="lg-btn-secondary text-xs px-2.5 py-1.5"
                    >
                      Nhắc điểm danh
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- TAB CONTENT: 4. Cảnh báo vắng -->
        <div v-if="activeSectionTab === 'warning'" class="lg-table-shell overflow-x-auto animate-in fade-in duration-200">
          <table class="min-w-full divide-y divide-default text-sm">
            <thead>
              <tr class="surface-table-header">
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Mã sinh viên</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Tên sinh viên</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Lớp học</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Môn học</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Số buổi vắng / Quỹ vắng</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Tỷ lệ vắng lũy kế</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái cảnh báo</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="stu in warnedStudents" :key="stu.id">
                <td class="px-4 py-3 font-semibold text-heading">{{ stu.id }}</td>
                <td class="px-4 py-3 font-bold text-heading">{{ stu.name }}</td>
                <td class="px-4 py-3 text-body font-medium">{{ stu.className }}</td>
                <td class="px-4 py-3 font-semibold text-primary">{{ stu.subjectCode }}</td>
                <td class="px-4 py-3 text-center font-bold text-rose-500">
                  {{ stu.absentSlots }} / {{ stu.maxQuota }} buổi
                </td>
                <td class="px-4 py-3 min-w-[150px]">
                  <div class="flex items-center gap-2">
                    <div class="lg-progress-track w-24 h-2 flex-shrink-0">
                      <div 
                        class="lg-progress-fill" 
                        :class="stu.absentPercentage >= 100 ? 'bg-red-600' : 'bg-amber-500'"
                        :style="{ width: stu.absentPercentage + '%' }"
                      ></div>
                    </div>
                    <span class="text-xs font-bold">{{ stu.absentPercentage }}%</span>
                  </div>
                </td>
                <td class="px-4 py-3 text-center">
                  <span 
                    class="lg-badge"
                    :class="stu.status === 'Chờ cấm thi' ? 'lg-badge-danger animate-pulse' : 'lg-badge-warning'"
                  >
                    {{ stu.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Audit Logs Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal">
        <div class="flex items-center justify-between border-b border-default pb-3 mb-4">
          <div class="flex items-center gap-2">
            <History class="w-5 h-5 text-primary" />
            <h3 class="font-extrabold text-heading text-base">Nhật ký hoạt động (Audit Logs)</h3>
          </div>
          <span class="text-xs text-muted">Module M5 - Lưu vết chuyên cần toàn hệ thống</span>
        </div>

        <div class="space-y-3 max-h-60 overflow-y-auto pr-2">
          <div 
            v-for="log in auditLogs" 
            :key="log.id" 
            class="p-3 rounded-xl border border-default bg-surface-card flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-sm"
          >
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <span class="font-bold text-primary">{{ log.action }}</span>
                <span class="text-xs text-muted">{{ log.time }}</span>
              </div>
              <p class="text-body font-medium">{{ log.details }}</p>
              <div v-if="log.reason" class="text-xs text-muted italic">
                Lý do: {{ log.reason }}
              </div>
            </div>
            <div class="text-xs font-semibold text-muted text-right">
              {{ log.actor }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Duyệt Mở khóa (Unlock Attendance Modal) -->
    <div 
      v-if="isUnlockModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 lg-mobile-scrim"
    >
      <div class="lg-glass-strong surface-modal border border-default rounded-2xl w-full max-w-md p-6 shadow-2xl relative animate-in fade-in zoom-in-95 duration-200">
        <!-- Close button -->
        <button 
          @click="isUnlockModalOpen = false" 
          class="absolute top-4 right-4 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-3 border-b border-default pb-3 mb-4">
          <Unlock class="w-6 h-6 text-primary" />
          <h3 class="text-lg font-extrabold text-heading">Xét duyệt Mở khóa Điểm danh</h3>
        </div>

        <div class="space-y-3 mb-4 text-sm">
          <div class="p-3 bg-slate-50 dark:bg-slate-800/40 rounded-xl space-y-1">
            <div><strong>Giảng viên:</strong> {{ selectedRequest?.teacherName }}</div>
            <div><strong>Môn học:</strong> {{ selectedRequest?.subjectCode }} - <strong>Lớp:</strong> {{ selectedRequest?.className }}</div>
            <div><strong>Thời gian học:</strong> {{ selectedRequest?.date }} - {{ selectedRequest?.slotName }}</div>
            <div class="text-rose-500 font-bold mt-1 text-xs">
              Mỗi ca học chỉ được phép mở khóa bổ sung 01 lần duy nhất.
            </div>
          </div>

          <div class="text-xs text-body">
            <strong>Lý do của giảng viên:</strong>
            <p class="italic text-muted mt-1 p-2 bg-slate-50 dark:bg-slate-800/40 rounded border border-default/50">
              "{{ selectedRequest?.reason }}"
            </p>
          </div>

          <!-- Nhập thời gian mở khóa -->
          <div>
            <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
              Thời gian mở khóa (phút)
            </label>
            <select v-model="unlockDuration" class="w-full px-3 lg-control text-sm">
              <option :value="15">15 phút</option>
              <option :value="20">20 phút (Mặc định)</option>
              <option :value="30">30 phút</option>
            </select>
          </div>

          <!-- Nhập lý do phê duyệt -->
          <div>
            <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
              Nhập lý do phê duyệt / từ chối <span class="text-rose-500">*</span>
            </label>
            <textarea
              v-model="unlockReason"
              rows="3"
              placeholder="Nhập ghi chú hoặc lý do phê duyệt của bạn..."
              class="w-full p-2.5 text-sm lg-input"
            ></textarea>
          </div>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-2 mt-6">
          <button 
            @click="handleApproveUnlock(false)" 
            :disabled="!unlockReason.trim()"
            class="lg-btn-danger px-4 py-2 text-sm font-semibold border-0 text-white bg-rose-500 hover:bg-rose-600 disabled:opacity-50"
          >
            Từ chối duyệt
          </button>
          <button 
            @click="handleApproveUnlock(true)" 
            :disabled="!unlockReason.trim()"
            class="lg-btn-primary px-4 py-2 text-sm font-semibold disabled:opacity-50"
          >
            Phê duyệt mở
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Cấu hình Quỹ vắng (Quota Config Modal) -->
    <div 
      v-if="isQuotaModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 lg-mobile-scrim"
    >
      <div class="lg-glass-strong surface-modal border border-default rounded-2xl w-full max-w-md p-6 shadow-2xl relative animate-in fade-in zoom-in-95 duration-200">
        <!-- Close button -->
        <button 
          @click="isQuotaModalOpen = false" 
          class="absolute top-4 right-4 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-muted transition-colors"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-3 border-b border-default pb-3 mb-4">
          <BookOpen class="w-6 h-6 text-primary" />
          <h3 class="text-lg font-extrabold text-heading">Điều chỉnh Hạn mức Quỹ vắng</h3>
        </div>

        <div class="space-y-4 mb-4 text-sm">
          <div class="p-3 bg-slate-50 dark:bg-slate-800/40 rounded-xl space-y-1">
            <div><strong>Môn học:</strong> [{{ selectedQuota?.subjectCode }}] {{ selectedQuota?.subjectName }}</div>
            <div><strong>Học kỳ:</strong> {{ selectedQuota?.semester }}</div>
            <div><strong>Tổng số buổi của môn học:</strong> {{ selectedQuota?.totalSlots }} buổi</div>
          </div>

          <!-- Hạn mức vắng cũ và mới -->
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Hạn mức vắng cũ
              </label>
              <input
                :value="selectedQuota?.maxAbsentSlots + ' buổi'"
                type="text"
                disabled
                class="w-full px-3 lg-control text-sm bg-slate-100 dark:bg-slate-800 opacity-60 cursor-not-allowed"
              />
            </div>
            <div>
              <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
                Hạn mức vắng mới <span class="text-rose-500">*</span>
              </label>
              <input
                v-model.number="newAbsentQuota"
                type="number"
                min="0"
                :max="selectedQuota?.totalSlots"
                class="w-full px-3 lg-control text-sm"
              />
            </div>
          </div>

          <!-- Lý do thay đổi -->
          <div>
            <label class="block text-xs font-bold text-label uppercase tracking-wider mb-1">
              Lý do thay đổi <span class="text-rose-500">*</span>
            </label>
            <textarea
              v-model="quotaChangeReason"
              rows="3"
              placeholder="Nhập lý do thay đổi hạn mức vắng để ghi nhận lịch sử học vụ..."
              class="w-full p-2.5 text-sm lg-input"
            ></textarea>
          </div>
        </div>

        <!-- Buttons -->
        <div class="flex items-center justify-end gap-2 mt-6">
          <button 
            @click="isQuotaModalOpen = false" 
            class="lg-btn-secondary px-4 py-2 text-sm font-semibold"
          >
            Hủy bỏ
          </button>
          <button 
            @click="handleSaveQuota" 
            :disabled="newAbsentQuota < 0 || newAbsentQuota > (selectedQuota?.totalSlots || 0) || !quotaChangeReason.trim()"
            class="lg-btn-primary px-4 py-2 text-sm font-semibold disabled:opacity-50"
          >
            Lưu thay đổi
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Hiệu ứng chuyển động */
.animate-in {
  animation-duration: 200ms;
  animation-fill-mode: both;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>
