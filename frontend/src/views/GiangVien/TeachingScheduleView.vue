<template>
  <div class="space-y-6 pb-10">
    <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-heading">Lịch giảng dạy</h1>
        <p class="text-sm text-muted mt-1">Quản lý và theo dõi các ca dạy của bạn</p>
      </div>
      <div class="flex items-center gap-3">
        <select v-model="selectedTerm" @change="loadSchedule" :disabled="!terms || terms.length === 0" class="rounded-xl border border-input bg-surface-input px-4 py-2 text-sm font-bold text-body outline-none focus:border-(--accent-primary) transition-all cursor-pointer">
          <option v-if="!terms || terms.length === 0" value="null">Chưa có học kỳ có lịch giảng dạy</option>
          <option v-for="term in terms" :key="term.maHocKy" :value="term.maHocKy">
            {{ term.tenHocKy }} {{ term.isCurrent ? '(Hiện tại)' : '' }}
          </option>
        </select>
        <div class="flex items-center rounded-xl border border-card bg-surface-card p-1">
          <button v-for="mode in ['Today', 'Week', 'Term']" :key="mode"
                  @click="setMode(mode)"
                  :class="['px-4 py-1.5 rounded-lg text-xs font-bold transition-all', currentMode === mode ? 'bg-(--accent-primary) text-white shadow-md' : 'text-muted hover:text-body hover:bg-surface-hover']">
            {{ mode === 'Today' ? 'Hôm nay' : mode === 'Week' ? 'Tuần này' : 'Cả kỳ' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="flex flex-wrap items-center gap-3 lg-glass-soft rounded-2xl p-4">
      <div class="relative flex-1 min-w-[200px]">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
        <input type="text" v-model="searchQuery" placeholder="Tìm môn học, lớp, phòng..." 
               class="w-full rounded-xl border border-input bg-surface-input py-2 pl-9 pr-4 text-sm text-body outline-none focus:border-(--accent-primary) transition-colors" />
      </div>
      <select v-model="statusFilter" class="rounded-xl border border-input bg-surface-input px-4 py-2 text-sm text-body outline-none focus:border-(--accent-primary) transition-colors">
        <option value="">Tất cả trạng thái</option>
        <option value="chua_bat_dau">Sắp diễn ra</option>
        <option value="da_ket_thuc">Đã hoàn thành</option>
      </select>
    </div>

    <div v-if="loading" class="flex items-center justify-center min-h-[300px]">
      <div class="animate-spin w-8 h-8 border-2 border-(--accent-primary) border-t-transparent rounded-full"></div>
      <span class="ml-3 text-muted text-sm font-medium">Đang tải lịch...</span>
    </div>
    
    <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
      <AlertCircle :size="48" class="text-(--color-danger-text)" />
      <p class="text-(--color-danger-text) font-semibold">{{ error }}</p>
      <button @click="loadPage" class="rounded-xl bg-(--accent-primary) px-5 py-2.5 text-sm font-bold text-white hover:opacity-90 transition-all">Thử lại</button>
    </div>

    <div v-else-if="filteredSchedule.length === 0" class="flex flex-col items-center justify-center min-h-[300px] gap-4 lg-glass-soft rounded-3xl p-8 text-center">
      <div class="h-16 w-16 rounded-full bg-(--accent-primary)/10 flex items-center justify-center">
        <CalendarX :size="32" class="text-(--accent-primary)" />
      </div>
      <div>
        <p class="text-heading font-bold">Không có lịch dạy</p>
        <p class="text-muted text-sm mt-1 max-w-xs">Không tìm thấy ca dạy nào phù hợp với bộ lọc hoặc khoảng thời gian đã chọn.</p>
      </div>
      <button v-if="searchQuery || statusFilter" @click="resetFilters" class="mt-2 text-link text-sm font-bold hover:underline">Xóa bộ lọc</button>
    </div>

    <div v-else class="space-y-4">
      <div v-for="(group, date) in groupedSchedule" :key="date" class="lg-glass-soft rounded-2xl overflow-hidden border border-card">
        <div class="bg-surface-sidebar px-5 py-3 border-b border-card flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="h-8 w-8 rounded-lg bg-(--accent-primary)/10 text-(--accent-primary) flex items-center justify-center">
              <Calendar :size="16" />
            </div>
            <h2 class="text-sm font-bold text-heading">{{ formatDate(date) }}</h2>
          </div>
          <span class="text-xs font-semibold text-muted">{{ group.length }} ca dạy</span>
        </div>
        <div class="divide-y divide-(--border-card)">
          <div v-for="item in group" :key="item.maBuoiHoc" class="p-5 flex flex-col md:flex-row gap-5 items-start md:items-center hover:bg-surface-hover/50 transition-colors">
            <!-- Time -->
            <div class="flex flex-col items-center justify-center md:w-24 shrink-0 rounded-xl bg-surface-input border border-card py-2 px-3">
              <span class="text-lg font-black text-heading leading-none tracking-tight">{{ item.gioBatDau.substring(0,5) }}</span>
              <span class="text-[10px] font-bold text-muted mt-1 uppercase tracking-wider">{{ item.tenCa }}</span>
              <span class="text-xs font-semibold text-body mt-1">{{ item.gioKetThuc.substring(0,5) }}</span>
            </div>
            
            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <GlassBadge v-if="item.isSubstitute" variant="warning">Dạy thay</GlassBadge>
                <GlassBadge :variant="item.trangThaiBuoi === 'da_ket_thuc' ? 'neutral' : 'primary'">
                  {{ item.trangThaiBuoi === 'da_ket_thuc' ? 'Đã kết thúc' : 'Sắp diễn ra' }}
                </GlassBadge>
              </div>
              <h3 class="text-base font-bold text-heading truncate">{{ item.tenMonHoc }}</h3>
              <div class="flex flex-wrap items-center gap-x-4 gap-y-2 mt-2">
                <div class="flex items-center gap-1.5 text-xs text-body font-medium">
                  <Users :size="14" class="text-muted" /> Lớp {{ item.tenLop }}
                </div>
                <div class="flex items-center gap-1.5 text-xs text-body font-medium">
                  <MapPin :size="14" class="text-muted" /> Phòng {{ item.tenPhong }}
                </div>
                <div class="flex items-center gap-1.5 text-xs text-body font-medium">
                  <Clock :size="14" class="text-muted" /> Trạng thái DD: {{ item.trangThaiDiemDanh === 'chua_mo' ? 'Chưa mở' : item.trangThaiDiemDanh }}
                </div>
              </div>
            </div>
            
            <!-- Actions -->
            <div class="flex gap-2 w-full md:w-auto">
              <router-link :to="`/teacher/classes/${item.maKhoaHoc}`" class="flex-1 md:flex-none flex items-center justify-center gap-2 rounded-xl border border-card bg-surface-card px-4 py-2 text-xs font-bold text-body hover:text-link hover:border-(--accent-primary)/30 transition-all">
                <BookOpen :size="14" />
                <span>Lớp học</span>
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { teacherApi } from '@/services/teacherApi'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { Search, AlertCircle, CalendarX, Calendar, Users, MapPin, Clock, BookOpen } from 'lucide-vue-next'

const loading = ref(false)
const error = ref('')
const terms = ref([])
const selectedTerm = ref(null)
const currentMode = ref('Week')
const schedule = ref([])
const searchQuery = ref('')
const statusFilter = ref('')

const setMode = (mode) => {
  currentMode.value = mode
  loadSchedule()
}

const resetFilters = () => {
  searchQuery.value = ''
  statusFilter.value = ''
}

const loadTerms = async () => {
  try {
    const result = await teacherApi.getScheduleTerms()
    if (!Array.isArray(result)) {
      throw new TypeError('API /api/teacher/schedule/terms không trả về danh sách học kỳ hợp lệ.')
    }
    terms.value = result
    
    const current = terms.value.find(t => t.isCurrent)
    selectedTerm.value = current?.maHocKy ?? terms.value[0]?.maHocKy ?? null
    return true
  } catch (err) {
    console.error('Failed to load terms', err)
    error.value = err?.message || 'Không thể tải danh sách học kỳ'
    terms.value = []
    selectedTerm.value = null
    return false
  }
}

const loadSchedule = async () => {
  loading.value = true
  error.value = ''
  try {
    let params = {}
    if (selectedTerm.value) params.maHocKy = selectedTerm.value
    else return // No term, don't load schedule
    
    // Add date filters based on mode if necessary
    const now = new Date()
    
    if (currentMode.value === 'Today') {
      const y = now.getFullYear()
      const m = String(now.getMonth() + 1).padStart(2, '0')
      const d = String(now.getDate()).padStart(2, '0')
      const todayStr = `${y}-${m}-${d}`
      params.ngayTu = todayStr
      params.ngayDen = todayStr
    } else if (currentMode.value === 'Week') {
      const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
      const day = today.getDay()
      const diff = today.getDate() - day + (day === 0 ? -6 : 1) // adjust when day is sunday
      
      const monday = new Date(today)
      monday.setDate(diff)
      
      const sunday = new Date(today)
      sunday.setDate(diff + 6)
      
      const fmt = (dt) => {
        const y = dt.getFullYear()
        const m = String(dt.getMonth() + 1).padStart(2, '0')
        const d = String(dt.getDate()).padStart(2, '0')
        return `${y}-${m}-${d}`
      }
      
      params.ngayTu = fmt(monday)
      params.ngayDen = fmt(sunday)
    }
    // Term mode fetches all for the term

    const res = await teacherApi.getSchedule(params)
    if (!res || !Array.isArray(res.items)) {
      throw new TypeError('API lịch giảng dạy không trả về dữ liệu phân trang hợp lệ.')
    }
    schedule.value = res.items
  } catch (err) {
    error.value = err?.message || 'Không thể tải lịch giảng dạy'
    schedule.value = []
  } finally {
    loading.value = false
  }
}

const filteredSchedule = computed(() => {
  let result = schedule.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(s => 
      (s.tenMonHoc || '').toLowerCase().includes(q) ||
      (s.tenLop || '').toLowerCase().includes(q) ||
      (s.tenPhong || '').toLowerCase().includes(q)
    )
  }
  if (statusFilter.value) {
    result = result.filter(s => s.trangThaiBuoi === statusFilter.value)
  }
  return result
})

const groupedSchedule = computed(() => {
  const groups = {}
  filteredSchedule.value.forEach(item => {
    const date = item.ngayHoc
    if (!groups[date]) groups[date] = []
    groups[date].push(item)
  })
  return groups
})

const formatDate = (dateStr) => {
  const d = new Date(dateStr)
  return new Intl.DateTimeFormat('vi-VN', { weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric' }).format(d)
}

const loadPage = async () => {
  loading.value = true
  error.value = ''
  schedule.value = []
  const success = await loadTerms()
  if (success) {
    await loadSchedule()
  } else {
    loading.value = false
  }
}

onMounted(async () => {
  await loadPage()
})
</script>
