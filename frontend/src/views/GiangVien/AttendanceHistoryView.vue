<script setup>
import { ref, computed } from 'vue'
import { 
  Search, Calendar, Filter, Download, 
  ChevronRight, ArrowUpDown, Clock, Users,
  CheckCircle2, AlertTriangle, BookOpen, X,
  Edit3, Save, AlertCircle
} from 'lucide-vue-next'

const attendanceHistory = ref([
  { id: 1, date: '12/05/2026', className: 'SE1601 - Java', absences: 3, total: 30, time: '07:30', room: 'A201' },
  { id: 2, date: '11/05/2026', className: 'SS1402 - Web', absences: 5, total: 32, time: '12:30', room: 'B305' },
  { id: 3, date: '10/05/2026', className: 'SE1601 - Java', absences: 1, total: 30, time: '07:30', room: 'A201' },
  { id: 4, date: '08/05/2026', className: 'SA1709 - DB', absences: 0, total: 25, time: '09:45', room: 'C102' },
])

// Mock data for student list per session
const sessionStudentsMock = ref({
  1: [
    { id: 'SV16001', name: 'Nguyễn Văn Anh', status: 'Present', time: '07:25', note: '' },
    { id: 'SV16002', name: 'Trần Thị Bình', status: 'Present', time: '07:28', note: '' },
    { id: 'SV16003', name: 'Lê Hoàng Cường', status: 'Late', time: '07:42', note: 'Kẹt xe đường Cộng Hòa' },
    { id: 'SV16004', name: 'Phạm Minh Danh', status: 'Absent', time: '--', note: 'Không lý do' },
    { id: 'SV16005', name: 'Đỗ Thùy Dương', status: 'Present', time: '07:18', note: '' },
    { id: 'SV16006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16007', name: 'Vũ Thị Giang', status: 'Absent', time: '--', note: 'Có phép (Bị ốm sốt)' },
    { id: 'SV16008', name: 'Lê Minh Hải', status: 'Present', time: '07:29', note: '' },
    { id: 'SV16009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Nghỉ không phép' },
    { id: 'SV16010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '07:24', note: '' }
  ],
  2: [
    { id: 'SV14001', name: 'Nguyễn Văn Anh', status: 'Present', time: '12:20', note: '' },
    { id: 'SV14002', name: 'Trần Thị Bình', status: 'Absent', time: '--', note: 'Nghỉ có phép' },
    { id: 'SV14003', name: 'Lê Hoàng Cường', status: 'Present', time: '12:25', note: '' },
    { id: 'SV14004', name: 'Phạm Minh Danh', status: 'Absent', time: '--', note: 'Nghỉ không phép' },
    { id: 'SV14005', name: 'Đỗ Thùy Dương', status: 'Late', time: '12:45', note: 'Hỏng xe giữa đường' },
    { id: 'SV14006', name: 'Nguyễn Tiến Đạt', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14007', name: 'Vũ Thị Giang', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14008', name: 'Lê Minh Hải', status: 'Present', time: '12:15', note: '' },
    { id: 'SV14009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '12:22', note: '' }
  ],
  3: [
    { id: 'SV16001', name: 'Nguyễn Văn Anh', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16002', name: 'Trần Thị Bình', status: 'Present', time: '07:25', note: '' },
    { id: 'SV16003', name: 'Lê Hoàng Cường', status: 'Present', time: '07:27', note: '' },
    { id: 'SV16004', name: 'Phạm Minh Danh', status: 'Present', time: '07:29', note: '' },
    { id: 'SV16005', name: 'Đỗ Thùy Dương', status: 'Present', time: '07:15', note: '' },
    { id: 'SV16006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '07:20', note: '' },
    { id: 'SV16007', name: 'Vũ Thị Giang', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16008', name: 'Lê Minh Hải', status: 'Present', time: '07:24', note: '' },
    { id: 'SV16009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Đi muộn quá giờ' },
    { id: 'SV16010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '07:21', note: '' }
  ],
  4: [
    { id: 'SV17001', name: 'Nguyễn Văn Anh', status: 'Present', time: '09:35', note: '' },
    { id: 'SV17002', name: 'Trần Thị Bình', status: 'Present', time: '09:40', note: '' },
    { id: 'SV17003', name: 'Lê Hoàng Cường', status: 'Present', time: '09:42', note: '' },
    { id: 'SV17004', name: 'Phạm Minh Danh', status: 'Present', time: '09:45', note: '' },
    { id: 'SV17005', name: 'Đỗ Thùy Dương', status: 'Present', time: '09:30', note: '' },
    { id: 'SV17006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '09:33', note: '' },
    { id: 'SV17007', name: 'Vũ Thị Giang', status: 'Present', time: '09:38', note: '' },
    { id: 'SV17008', name: 'Lê Minh Hải', status: 'Present', time: '09:41', note: '' },
    { id: 'SV17009', name: 'Phạm Thanh Hương', status: 'Present', time: '09:43', note: '' },
    { id: 'SV17010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '09:44', note: '' }
  ]
})

// Search & Filter History
const historySearch = ref('')
const historyDate = ref('')

const filteredHistory = computed(() => {
  return attendanceHistory.value.filter(item => {
    const matchSearch = item.className.toLowerCase().includes(historySearch.value.toLowerCase()) ||
                        item.room.toLowerCase().includes(historySearch.value.toLowerCase())
    
    let matchDate = true
    if (historyDate.value) {
      const parts = historyDate.value.split('-')
      const formattedDate = `${parts[2]}/${parts[1]}/${parts[0]}`
      matchDate = item.date === formattedDate
    }
    
    return matchSearch && matchDate
  })
})

// Selected Session Detail state
const selectedSession = ref(null)
const isDetailModalOpen = ref(false)
const activeStudents = ref([])
const activeSearchQuery = ref('')
const activeFilterStatus = ref('')
const isEditing = ref(false)

const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

function triggerToast(msg, type = 'success') {
  toast.value.message = msg
  toast.value.type = type
  toast.value.show = true
  setTimeout(() => {
    toast.value.show = false
  }, 4000)
}

function openSessionDetails(session) {
  selectedSession.value = { ...session }
  const list = sessionStudentsMock.value[session.id] || []
  activeStudents.value = JSON.parse(JSON.stringify(list))
  activeSearchQuery.value = ''
  activeFilterStatus.value = ''
  isEditing.value = false
  isDetailModalOpen.value = true
}

const filteredStudents = computed(() => {
  return activeStudents.value.filter(sv => {
    const matchSearch = sv.name.toLowerCase().includes(activeSearchQuery.value.toLowerCase()) ||
                        sv.id.toLowerCase().includes(activeSearchQuery.value.toLowerCase())
    const matchStatus = !activeFilterStatus.value || sv.status === activeFilterStatus.value
    return matchSearch && matchStatus
  })
})

const sessionStats = computed(() => {
  const total = activeStudents.value.length
  const present = activeStudents.value.filter(s => s.status === 'Present').length
  const late = activeStudents.value.filter(s => s.status === 'Late').length
  const absent = activeStudents.value.filter(s => s.status === 'Absent').length
  
  return { total, present, late, absent }
})

function changeStatus(index, status) {
  if (!isEditing.value) return
  activeStudents.value[index].status = status
  if (status === 'Absent') {
    activeStudents.value[index].time = '--'
  } else if (status === 'Present' && activeStudents.value[index].time === '--') {
    activeStudents.value[index].time = '07:30' // Fallback checkin time
  }
}

function saveAttendanceChanges() {
  if (selectedSession.value) {
    const idx = attendanceHistory.value.findIndex(h => h.id === selectedSession.value.id)
    if (idx !== -1) {
      const absencesCount = activeStudents.value.filter(s => s.status === 'Absent').length
      attendanceHistory.value[idx].absences = absencesCount
      attendanceHistory.value[idx].total = activeStudents.value.length
    }
    
    sessionStudentsMock.value[selectedSession.value.id] = JSON.parse(JSON.stringify(activeStudents.value))
    isEditing.value = false
    triggerToast('Đã cập nhật lịch sử điểm danh thành công!', 'success')
  }
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800 relative">
    
    <!-- Toast Component -->
    <Transition name="toast-slide">
      <div v-if="toast.show" 
           :class="['fixed top-6 right-6 z-[100] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl border backdrop-blur-md transition-all duration-300', 
                    toast.type === 'success' ? 'bg-emerald-500/90 border-emerald-400 text-white' : 'bg-rose-500/90 border-rose-400 text-white']">
        <CheckCircle2 v-if="toast.type === 'success'" :size="20" />
        <AlertCircle v-else :size="20" />
        <p class="text-sm font-bold tracking-wide">{{ toast.message }}</p>
      </div>
    </Transition>

    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-emerald-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white shadow-md shadow-emerald-200">
           <Calendar :size="32" />
        </div>
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Lịch sử điểm danh</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Xem lại nhật ký điểm danh của các buổi học đã diễn ra.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <button class="flex items-center gap-2 rounded-2xl bg-white px-5 py-3 border border-slate-200 shadow-sm hover:bg-slate-50 hover:text-emerald-600 transition-colors font-bold text-sm text-slate-700 active:scale-95">
            <Download :size="18" /> Xuất báo cáo
         </button>
      </div>
    </div>

    <!-- Quick Stats & Filters -->
    <div class="flex flex-col xl:flex-row gap-6">
       <!-- Stats -->
       <div class="grid grid-cols-2 md:grid-cols-4 xl:w-1/2 gap-4">
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 mb-3">
                <BookOpen :size="20" />
             </div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Tổng số buổi</p>
             <p class="text-2xl font-black text-slate-800">24</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-emerald-50 flex items-center justify-center text-emerald-600 mb-3">
                <CheckCircle2 :size="20" />
             </div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Đi học đúng</p>
             <p class="text-2xl font-black text-slate-800">92%</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm col-span-2 flex flex-col justify-center">
             <div class="flex items-center gap-3 mb-2">
                  <div class="h-10 w-10 rounded-xl bg-rose-50 flex items-center justify-center text-rose-600">
                     <AlertTriangle :size="20" />
                  </div>
                  <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Cần lưu ý</p>
             </div>
             <p class="text-sm font-bold text-slate-800 mt-1">Lớp SS1402 có tỷ lệ vắng cao (15%)</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 rounded-[32px] bg-white border border-slate-100 p-6 shadow-sm flex flex-col justify-center">
          <p class="text-sm font-bold text-slate-800 mb-4 flex items-center gap-2"><Filter :size="16" class="text-blue-500" /> Bộ lọc dữ liệu</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input v-model="historySearch" type="text" placeholder="Tìm kiếm lớp học, mã phòng..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors text-slate-800" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
              <Calendar :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
              <input v-model="historyDate" type="date" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors text-slate-800 cursor-pointer" />
            </div>
          </div>
       </div>
    </div>

    <!-- History Table -->
    <div class="rounded-[32px] border border-slate-100 bg-white shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50 border-b border-slate-100">
              <th class="px-8 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">
                 <div class="flex items-center gap-2 hover:text-blue-600 cursor-pointer transition-colors w-max">Ngày học <ArrowUpDown :size="14" /></div>
              </th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Lớp học</th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Thời gian</th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-center">Tình trạng</th>
              <th class="px-8 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in filteredHistory" :key="item.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-4">
                   <div class="h-12 w-12 rounded-2xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-500 group-hover:bg-blue-50 group-hover:text-blue-600 group-hover:border-blue-100 transition-colors shadow-sm">
                      <Calendar :size="20" />
                   </div>
                   <div>
                      <p class="text-sm font-bold text-slate-900">{{ item.date }}</p>
                      <p class="text-xs font-semibold text-slate-400 mt-0.5">Học kỳ Fall</p>
                   </div>
                </div>
              </td>
              <td class="px-6 py-5">
                <p class="text-sm font-bold text-slate-900 group-hover:text-blue-700 transition-colors">{{ item.className }}</p>
                <div class="flex items-center gap-1.5 mt-1">
                   <span class="rounded bg-slate-100 px-1.5 py-0.5 text-[10px] font-bold text-slate-500 uppercase">Phòng</span>
                   <span class="text-xs font-bold text-slate-600">{{ item.room }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2 text-sm text-slate-600 font-bold bg-slate-50 px-3 py-1.5 rounded-xl w-max border border-slate-100">
                   <Clock :size="14" class="text-blue-500" /> {{ item.time }}
                </div>
              </td>
              <td class="px-6 py-5 text-center">
                 <div v-if="item.absences === 0" class="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-3 py-1.5 border border-emerald-100 text-emerald-700">
                    <CheckCircle2 :size="14" />
                    <span class="text-xs font-bold">Đầy đủ ({{item.total}})</span>
                 </div>
                 <div v-else class="inline-flex items-center gap-1.5 rounded-full bg-rose-50 px-3 py-1.5 border border-rose-100 text-rose-700">
                    <Users :size="14" />
                    <span class="text-xs font-bold">Vắng {{ item.absences }}/{{item.total}}</span>
                 </div>
              </td>
              <td class="px-8 py-5 text-right">
                <button @click="openSessionDetails(item)" class="h-10 w-10 inline-flex items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-400 hover:text-blue-600 hover:border-blue-200 hover:bg-blue-50 transition-colors shadow-sm group-hover:shadow-md">
                   <ChevronRight :size="20" />
                </button>
              </td>
            </tr>
            <tr v-if="filteredHistory.length === 0">
              <td colspan="5" class="text-center py-10">
                <div class="flex flex-col items-center justify-center text-slate-350">
                  <Calendar :size="48" class="text-slate-200 mb-2" />
                  <p class="text-sm font-bold">Không tìm thấy lịch sử điểm danh</p>
                  <p class="text-xs">Vui lòng thử lại với từ khóa hoặc bộ lọc khác.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="bg-slate-50/80 px-8 py-5 border-t border-slate-100 flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-widest">
            <Users :size="14" class="text-slate-400" /> Hiển thị {{ filteredHistory.length }} bản ghi
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-blue-500 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
         </div>
      </div>
    </div>

    <!-- Attendance Details Dialog Modal -->
    <Teleport to="body">
      <div v-if="isDetailModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <!-- Backdrop with blur -->
        <div @click="isDetailModalOpen = false" class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity duration-300"></div>

        <!-- Modal Card -->
        <div class="relative w-full max-w-4xl bg-white rounded-[32px] shadow-2xl border border-slate-100 flex flex-col max-h-[85vh] overflow-hidden transform transition-all duration-300 scale-100 animate-modal-in">
          
          <!-- Modal Header -->
          <div class="p-6 md:p-8 border-b border-slate-100 flex justify-between items-center relative overflow-hidden shrink-0">
            <div class="absolute -right-16 -top-16 h-36 w-36 bg-gradient-to-tr from-emerald-50 to-emerald-100/30 rounded-full blur-2xl pointer-events-none" />
            
            <div class="relative z-10 flex items-center gap-4">
              <div class="h-12 w-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center border border-emerald-100/50">
                 <Calendar :size="24" />
              </div>
              <div v-if="selectedSession">
                 <h3 class="text-xl md:text-2xl font-black text-slate-850">Chi Tiết Điểm Danh</h3>
                 <p class="text-xs font-semibold text-slate-400 mt-0.5">
                   Lớp: <span class="text-slate-700 font-bold">{{ selectedSession.className }}</span> • 
                   Ngày: <span class="text-slate-700 font-bold">{{ selectedSession.date }}</span> • 
                   Giờ: <span class="text-slate-700 font-bold">{{ selectedSession.time }}</span> • 
                   Phòng: <span class="text-slate-700 font-bold">{{ selectedSession.room }}</span>
                 </p>
              </div>
            </div>
            <button @click="isDetailModalOpen = false" class="h-10 w-10 rounded-full bg-slate-50 flex items-center justify-center text-slate-450 hover:bg-rose-50 hover:text-rose-500 hover:rotate-90 transition-all duration-300 relative z-10">
               <X :size="18" />
            </button>
          </div>

          <!-- Modal Stats Row -->
          <div class="px-6 py-4 md:px-8 border-b border-slate-50 bg-slate-50/40 grid grid-cols-4 gap-4 shrink-0">
            <div class="rounded-xl bg-white border border-slate-100 p-3 text-center shadow-sm">
              <p class="text-[9px] font-black text-slate-400 uppercase tracking-wider">Tổng số</p>
              <p class="text-base font-black text-slate-700 mt-0.5">{{ sessionStats.total }} SV</p>
            </div>
            <div class="rounded-xl bg-emerald-50/50 border border-emerald-100 p-3 text-center shadow-sm">
              <p class="text-[9px] font-black text-emerald-600 uppercase tracking-wider">Có mặt</p>
              <p class="text-base font-black text-emerald-700 mt-0.5">{{ sessionStats.present }} SV</p>
            </div>
            <div class="rounded-xl bg-amber-50/50 border border-amber-100 p-3 text-center shadow-sm">
              <p class="text-[9px] font-black text-amber-600 uppercase tracking-wider">Đi muộn</p>
              <p class="text-base font-black text-amber-700 mt-0.5">{{ sessionStats.late }} SV</p>
            </div>
            <div class="rounded-xl bg-rose-50/50 border border-rose-100 p-3 text-center shadow-sm">
              <p class="text-[9px] font-black text-rose-600 uppercase tracking-wider">Vắng mặt</p>
              <p class="text-base font-black text-rose-700 mt-0.5">{{ sessionStats.absent }} SV</p>
            </div>
          </div>

          <!-- Modal Body Toolbar -->
          <div class="px-6 py-4 md:px-8 border-b border-slate-50 bg-white flex flex-col sm:flex-row gap-3 justify-between items-center shrink-0">
            <!-- Search student inside session -->
            <div class="relative w-full sm:w-64">
              <Search :size="14" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
              <input 
                v-model="activeSearchQuery"
                type="text" 
                placeholder="Tìm sinh viên, MSSV..." 
                class="w-full rounded-xl border border-slate-200 bg-slate-50/60 pl-9 pr-4 py-2.5 text-xs font-semibold outline-none focus:border-emerald-450 focus:bg-white transition-all text-slate-800" 
              />
            </div>
            
            <!-- Quick Filter Status -->
            <div class="flex items-center gap-2 w-full sm:w-auto shrink-0 justify-end">
              <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wide mr-1">Lọc:</span>
              <button 
                @click="activeFilterStatus = ''"
                :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all', !activeFilterStatus ? 'bg-slate-900 text-white shadow-sm' : 'bg-slate-50 border border-slate-100 text-slate-500 hover:bg-slate-100']"
              >
                Tất cả
              </button>
              <button 
                @click="activeFilterStatus = 'Present'"
                :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all', activeFilterStatus === 'Present' ? 'bg-emerald-600 text-white shadow-sm' : 'bg-slate-50 border border-slate-100 text-slate-500 hover:bg-emerald-50 hover:text-emerald-700']"
              >
                Có mặt
              </button>
              <button 
                @click="activeFilterStatus = 'Late'"
                :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all', activeFilterStatus === 'Late' ? 'bg-amber-600 text-white shadow-sm' : 'bg-slate-50 border border-slate-100 text-slate-500 hover:bg-amber-50 hover:text-amber-700']"
              >
                Muộn
              </button>
              <button 
                @click="activeFilterStatus = 'Absent'"
                :class="['px-3 py-1.5 rounded-lg text-xs font-bold transition-all', activeFilterStatus === 'Absent' ? 'bg-rose-600 text-white shadow-sm' : 'bg-slate-50 border border-slate-100 text-slate-500 hover:bg-rose-50 hover:text-rose-700']"
              >
                Vắng
              </button>
            </div>
          </div>

          <!-- Modal Body Student List (Scrollable) -->
          <div class="overflow-y-auto p-6 md:p-8 bg-slate-50/20 flex-1">
            <div class="rounded-2xl border border-slate-100 bg-white overflow-hidden shadow-sm">
              <table class="w-full text-left border-collapse">
                <thead>
                  <tr class="bg-slate-50/40 border-b border-slate-100 text-[10px] font-black uppercase tracking-wider text-slate-450">
                    <th class="px-6 py-4">Sinh viên</th>
                    <th class="px-6 py-4">Mã số SV</th>
                    <th class="px-6 py-4">Giờ điểm danh</th>
                    <th class="px-6 py-4 text-center">Trạng thái</th>
                    <th class="px-6 py-4">Ghi chú</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-50 text-xs">
                  <tr v-for="(sv, idx) in filteredStudents" :key="sv.id" class="hover:bg-slate-50/30 transition-colors">
                    <td class="px-6 py-4">
                      <div class="flex items-center gap-3">
                        <div class="h-9 w-9 rounded-xl bg-slate-50 font-black text-slate-550 flex items-center justify-center border border-slate-100 shrink-0 uppercase shadow-inner">
                          {{ sv.name.split(' ').pop()[0] }}
                        </div>
                        <span class="font-bold text-slate-800">{{ sv.name }}</span>
                      </div>
                    </td>
                    <td class="px-6 py-4 font-bold text-slate-500 uppercase tracking-wide">
                      {{ sv.id }}
                    </td>
                    <td class="px-6 py-4">
                      <div class="flex items-center gap-1 text-slate-600 font-semibold">
                        <Clock :size="12" class="text-slate-400" />
                        <span>{{ sv.time }}</span>
                      </div>
                    </td>
                    
                    <!-- Status Badge (Toggleable in edit mode) -->
                    <td class="px-6 py-4 text-center">
                      <div v-if="!isEditing" class="inline-block">
                        <span v-if="sv.status === 'Present'" class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2.5 py-1 text-[10px] font-bold text-emerald-700 border border-emerald-100">
                          Có mặt
                        </span>
                        <span v-else-if="sv.status === 'Late'" class="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2.5 py-1 text-[10px] font-bold text-amber-700 border border-amber-100">
                          Đi muộn
                        </span>
                        <span v-else class="inline-flex items-center gap-1 rounded-full bg-rose-50 px-2.5 py-1 text-[10px] font-bold text-rose-700 border border-rose-100">
                          Vắng mặt
                        </span>
                      </div>
                      
                      <!-- Editable Button Group -->
                      <div v-else class="inline-flex rounded-lg border border-slate-200 bg-white p-0.5 shadow-sm w-max mx-auto">
                        <button 
                          @click="changeStatus(idx, 'Present')"
                          :class="['px-2.5 py-1 text-[10px] font-bold rounded-md transition-colors', sv.status === 'Present' ? 'bg-emerald-500 text-white shadow-sm' : 'text-slate-500 hover:bg-slate-50']"
                        >
                          Có
                        </button>
                        <button 
                          @click="changeStatus(idx, 'Late')"
                          :class="['px-2.5 py-1 text-[10px] font-bold rounded-md transition-colors', sv.status === 'Late' ? 'bg-amber-500 text-white shadow-sm' : 'text-slate-500 hover:bg-slate-50']"
                        >
                          Muộn
                        </button>
                        <button 
                          @click="changeStatus(idx, 'Absent')"
                          :class="['px-2.5 py-1 text-[10px] font-bold rounded-md transition-colors', sv.status === 'Absent' ? 'bg-rose-500 text-white shadow-sm' : 'text-slate-500 hover:bg-slate-50']"
                        >
                          Vắng
                        </button>
                      </div>
                    </td>

                    <!-- Note column -->
                    <td class="px-6 py-4">
                      <div v-if="!isEditing" class="text-slate-500 italic max-w-[180px] truncate" :title="sv.note">
                        {{ sv.note || '--' }}
                      </div>
                      <input 
                        v-else
                        v-model="sv.note"
                        type="text"
                        placeholder="Nhập lý do..."
                        class="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-xs outline-none focus:border-emerald-450 bg-slate-50/50 focus:bg-white transition-colors text-slate-800"
                      />
                    </td>
                  </tr>
                  
                  <tr v-if="filteredStudents.length === 0">
                    <td colspan="5" class="text-center py-8">
                      <div class="flex flex-col items-center justify-center text-slate-350">
                        <Users :size="32" class="text-slate-200 mb-1" />
                        <p class="text-xs font-bold">Không tìm thấy sinh viên nào</p>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Modal Footer -->
          <div class="p-6 border-t border-slate-100 flex justify-between items-center bg-white shrink-0">
            <!-- Left secondary action -->
            <button 
              type="button"
              class="rounded-xl border border-slate-250 bg-white px-5 py-3 text-xs font-bold text-slate-600 hover:bg-slate-50 transition-all flex items-center gap-1.5 active:scale-95 shadow-sm"
            >
              <Download :size="16" /> Xuất file buổi này
            </button>
            
            <!-- Right main actions -->
            <div class="flex gap-2">
              <button 
                v-if="!isEditing"
                @click="isEditing = true"
                class="rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-600 px-6 py-3 text-xs font-bold text-white shadow-md shadow-emerald-100 hover:shadow-lg hover:shadow-emerald-200 hover:-translate-y-0.5 transition-all flex items-center gap-1.5 active:scale-95"
              >
                <Edit3 :size="16" /> Chỉnh sửa điểm danh
              </button>
              
              <template v-else>
                <button 
                  @click="isEditing = false"
                  class="rounded-xl border border-slate-200 px-5 py-3 text-xs font-bold text-slate-500 hover:bg-slate-50 transition-all active:scale-95 shadow-sm"
                >
                  Hủy
                </button>
                <button 
                  @click="saveAttendanceChanges"
                  class="rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-600 px-6 py-3 text-xs font-bold text-white shadow-md shadow-emerald-100 hover:shadow-lg hover:shadow-emerald-200 hover:-translate-y-0.5 transition-all flex items-center gap-1.5 active:scale-95"
                >
                  <Save :size="16" /> Lưu thay đổi
                </button>
              </template>
            </div>
          </div>

        </div>
      </div>
    </Teleport>

  </div>
</template>

<style scoped>
/* Modal slide/fade animations */
@keyframes modal-in {
  from {
    opacity: 0;
    transform: scale(0.96) translateY(15px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}
.animate-modal-in {
  animation: modal-in 0.35s cubic-bezier(0.16, 1, 0.3, 1) both;
}

/* Toast animations */
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from {
  transform: translateY(-20px) scale(0.9);
  opacity: 0;
}
.toast-slide-leave-to {
  transform: translateY(20px) scale(0.9);
  opacity: 0;
}

.text-slate-850 {
  color: #1e293b;
}
.text-slate-550 {
  color: #64748b;
}
.text-slate-450 {
  color: #94a3b8;
}
.text-slate-350 {
  color: #cbd5e1;
}
.focus\:border-emerald-450:focus {
  border-color: #10b981;
}
</style>
