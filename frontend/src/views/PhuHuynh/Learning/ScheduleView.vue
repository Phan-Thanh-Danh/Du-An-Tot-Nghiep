<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Calendar,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  Video,
  MapPin,
  User,
  Clock,
  AlertOctagon,
  CalendarDays
} from 'lucide-vue-next'
import { childrenData, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'

const route = useRoute()
const router = useRouter()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

// Chế độ xem: 'week' hoặc 'month'
const viewMode = ref('week')

// Giả lập các tuần học
const weeks = [
  { id: 23, label: 'Tuần 23 (01/06 - 07/06)' },
  { id: 24, label: 'Tuần 24 (08/06 - 14/06)' },
  { id: 25, label: 'Tuần 25 (15/06 - 21/06)' }
]
const activeWeekIdx = ref(1) // Tuần hiện tại là Tuần 24

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
}

function prevWeek() {
  if (activeWeekIdx.value > 0) activeWeekIdx.value--
}

function nextWeek() {
  if (activeWeekIdx.value < weeks.length - 1) activeWeekIdx.value++
}

// Giả lập lịch tháng (June 2026)
const selectedMonthDay = ref(10) // Mặc định chọn ngày 10 (Thứ tư)
const monthDays = computed(() => {
  // June 2026 starts on Monday (1st)
  // We'll generate 30 days
  const days = []
  for (let i = 1; i <= 30; i++) {
    // Xác định xem ngày này có môn học hay thi gì không
    // Nguyễn Minh Quân: có lịch thi ngày 10/06, lịch học ngày 8, 9, 11, 12
    // Nguyễn Khánh Linh: có lịch thi ngày 11/06, lịch học ngày 8, 9, 12
    let hasClass = false
    let hasExam = false
    
    if (activeChildId.value === 1) {
      if (i === 10) hasExam = true
      else if ([8, 9, 11, 12].includes(i)) hasClass = true
    } else {
      if (i === 11) hasExam = true
      else if ([8, 9, 12].includes(i)) hasClass = true
    }

    days.push({
      dayNumber: i,
      weekday: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'][(i - 1) % 7],
      hasClass,
      hasExam
    })
  }
  return days
})

// Lấy danh sách lịch học của ngày đang chọn trong lịch tháng
const selectedDaySchedule = computed(() => {
  const dayNameMap = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật']
  const weekdayIndex = (selectedMonthDay.value - 1) % 7
  const weekdayName = dayNameMap[weekdayIndex]
  
  // Trả về lịch học tương ứng trong data của học sinh
  const scheduleItem = currentChild.value.schedule.find(s => s.day === weekdayName)
  if (scheduleItem && scheduleItem.subject !== 'Không có lịch học') {
    return [scheduleItem]
  }
  return []
})

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div class="flex items-center gap-2">
        <button
          @click="goBack"
          class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
          title="Quay lại"
        >
          <ChevronLeft :size="18" />
        </button>
        <div>
          <h2 class="text-lg font-bold text-heading flex items-center gap-2">
            <Calendar :size="20" class="text-orange-600" />
            Thời khóa biểu của con
          </h2>
          <p class="text-xs text-body">Xem lịch lên lớp, phòng học, giảng viên và các mốc thi cử quan trọng</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild.name }}</span>
          </div>
          <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
        </button>

        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 translate-y-2 scale-95"
          enter-to-class="opacity-100 translate-y-0 scale-100"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0 scale-100"
          leave-to-class="opacity-0 translate-y-2 scale-95"
        >
          <div
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-[var(--lg-shadow-md)]"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-[var(--surface-card-hover)]"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── THAO TÁC / CHẾ ĐỘ XEM ── -->
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 lg-card-glass">
      <!-- Tabs Switcher -->
      <div class="flex p-1 surface-input border border-card rounded-xl">
        <button
          @click="viewMode = 'week'"
          class="flex items-center gap-1.5 px-4 py-1.5 rounded-lg text-xs font-bold transition"
          :class="viewMode === 'week' ? 'surface-elevated text-orange-600 shadow-sm' : 'text-label hover:text-orange-600'"
        >
          <Calendar :size="13" /> Theo Tuần
        </button>
        <button
          @click="viewMode = 'month'"
          class="flex items-center gap-1.5 px-4 py-1.5 rounded-lg text-xs font-bold transition"
          :class="viewMode === 'month' ? 'surface-elevated text-orange-600 shadow-sm' : 'text-label hover:text-orange-600'"
        >
          <CalendarDays :size="13" /> Theo Tháng
        </button>
      </div>

      <!-- Điều hướng Tuần (Chỉ hiển thị ở chế độ tuần) -->
      <div v-if="viewMode === 'week'" class="flex items-center gap-3">
        <button
          @click="prevWeek"
          :disabled="activeWeekIdx === 0"
          class="h-8 w-8 flex items-center justify-center border border-card surface-card rounded-lg text-label hover:text-orange-600 disabled:opacity-40 disabled:pointer-events-none transition"
        >
          <ChevronLeft :size="16" />
        </button>
        <span class="text-xs font-bold text-heading min-w-[150px] text-center">
          {{ weeks[activeWeekIdx].label }}
        </span>
        <button
          @click="nextWeek"
          :disabled="activeWeekIdx === weeks.length - 1"
          class="h-8 w-8 flex items-center justify-center border border-card surface-card rounded-lg text-label hover:text-orange-600 disabled:opacity-40 disabled:pointer-events-none transition"
        >
          <ChevronRight :size="16" />
        </button>
      </div>

      <!-- Chú thích lịch thi -->
      <div class="flex items-center gap-4 text-xs font-semibold">
        <div class="flex items-center gap-1.5">
          <span class="h-3.5 w-3.5 rounded-md bg-orange-100 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-950/20" />
          <span class="text-body text-[11px]">Lịch học thường</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="h-3.5 w-3.5 rounded-md bg-red-100 dark:bg-red-950/20 border border-red-200 dark:border-red-950/20" />
          <span class="text-red-700 dark:text-red-400 text-[11px] font-bold">Lịch Thi Học Kỳ</span>
        </div>
      </div>
    </div>

    <!-- ── CHẾ ĐỘ XEM THEO TUẦN (WEEK VIEW) ── -->
    <div v-if="viewMode === 'week'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-7 gap-4">
      <div
        v-for="item in currentChild.schedule"
        :key="item.day"
        class="lg-card-glass p-4 flex flex-col justify-between hover:border-orange-500/20 transition-all duration-300 relative group min-h-[200px]"
        :class="
          item.isExam ? 'border-red-200 dark:border-red-950/40 bg-red-50/20 dark:bg-red-950/5' :
          item.subject === 'Không có lịch học' ? 'opacity-60' : ''
        "
      >
        <!-- Top Day Header -->
        <div class="pb-2 border-b border-card mb-3">
          <h3 class="text-xs font-extrabold text-heading flex justify-between items-center">
            <span>{{ item.day }}</span>
            <span class="text-[10px] text-muted font-normal">{{ item.date }}</span>
          </h3>
        </div>

        <!-- Class details -->
        <div class="flex-1 space-y-2.5">
          <div v-if="item.subject === 'Không có lịch học'" class="h-full flex items-center justify-center py-8">
            <span class="text-[11px] text-muted italic font-medium">Nghỉ học</span>
          </div>
          <div v-else class="space-y-2">
            <!-- Badge Thi nếu có -->
            <span
              v-if="item.isExam"
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-red-100 dark:bg-red-950/30 text-[9px] font-bold text-red-700 dark:text-red-400"
            >
              <AlertOctagon :size="10" /> LỊCH THI HỌC KỲ
            </span>

            <p class="text-xs font-bold text-heading leading-tight group-hover:text-orange-600 transition-colors">
              {{ item.subject }}
            </p>

            <div class="space-y-1.5 text-[11px] text-body font-medium">
              <p class="flex items-center gap-1.5">
                <Clock :size="12" class="text-muted" />
                <span>{{ item.shift }}</span>
              </p>
              
              <p class="flex items-center gap-1.5">
                <component :is="item.room.includes('Online') ? Video : MapPin" :size="12" class="text-muted" />
                <span :class="item.room.includes('Online') ? 'text-blue-600 dark:text-blue-400 underline cursor-pointer' : ''">
                  {{ item.room }}
                </span>
              </p>

              <p class="flex items-center gap-1.5">
                <User :size="12" class="text-muted" />
                <span>{{ item.teacher }}</span>
              </p>
            </div>
          </div>
        </div>

        <!-- Online link button -->
        <div v-if="item.link" class="pt-3 border-t border-card mt-3">
          <a
            :href="item.link"
            target="_blank"
            class="w-full py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 dark:bg-blue-950/30 dark:text-blue-400 text-[10px] font-bold rounded-lg flex items-center justify-center gap-1 transition"
          >
            <Video :size="11" /> Vào học Online
          </a>
        </div>
      </div>
    </div>

    <!-- ── CHẾ ĐỘ XEM THEO THÁNG (MONTH VIEW) ── -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Calendar Grid (2/3 width) -->
      <div class="lg:col-span-2 lg-card-glass p-5 space-y-4">
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
            Tháng 6 - 2026
          </h3>
          <span class="text-[10px] text-muted font-semibold">Chọn ngày để xem chi tiết lịch học phía dưới</span>
        </div>

        <!-- Month calendar grid -->
        <div class="grid grid-cols-7 gap-2 text-center text-xs">
          <!-- Weekday Headers -->
          <div v-for="h in ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']" :key="h" class="py-1.5 font-bold text-muted text-[10px]">
            {{ h }}
          </div>

          <!-- Days -->
          <button
            v-for="d in monthDays"
            :key="d.dayNumber"
            @click="selectedMonthDay = d.dayNumber"
            class="py-2.5 rounded-xl border flex flex-col items-center justify-between gap-1 relative transition"
            :class="
              selectedMonthDay === d.dayNumber ? 'bg-orange-600 border-orange-600 text-white font-extrabold shadow-sm' :
              d.hasExam ? 'border-red-200 dark:border-red-950/40 bg-red-50/20 dark:bg-red-950/5 text-red-600' :
              d.hasClass ? 'border-card surface-card hover:bg-orange-50/40' :
              'border-transparent text-disabled opacity-40'
            "
          >
            <span>{{ d.dayNumber }}</span>
            <!-- Dot indicators -->
            <span class="flex gap-0.5">
              <span v-if="d.hasExam" class="h-1 w-1 rounded-full bg-red-500" />
              <span v-else-if="d.hasClass" class="h-1 w-1 rounded-full bg-orange-500" />
            </span>
          </button>
        </div>
      </div>

      <!-- Detail Card (1/3 width) -->
      <div class="lg-card-glass p-5 space-y-4 flex flex-col justify-between">
        <div>
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card mb-4">
            Lịch ngày {{ selectedMonthDay }}/06/2026
          </h3>

          <div v-if="selectedDaySchedule.length === 0" class="text-center py-12 text-muted italic text-xs">
            Không có lịch học hoặc lịch thi trong ngày này.
          </div>
          <div v-else class="space-y-4">
            <div
              v-for="(item, idx) in selectedDaySchedule"
              :key="idx"
              class="p-4 rounded-xl border space-y-3"
              :class="item.isExam ? 'border-red-200 bg-red-50/30 dark:border-red-950/20 dark:bg-red-950/5' : 'border-card'"
            >
              <div class="flex items-center justify-between">
                <span class="text-[10px] font-bold text-orange-600 bg-orange-50 dark:bg-orange-950/20 px-2 py-0.5 rounded">Lớp học phần</span>
                <span
                  v-if="item.isExam"
                  class="px-2 py-0.5 rounded bg-red-100 dark:bg-red-950/30 text-[9px] font-bold text-red-700 dark:text-red-400"
                >
                  Thi cử
                </span>
              </div>
              
              <h4 class="text-xs font-bold text-heading leading-tight">{{ item.subject }}</h4>

              <div class="space-y-1.5 text-[11px] text-body font-medium">
                <p class="flex items-center gap-1.5">
                  <Clock :size="12" class="text-muted" />
                  <span>{{ item.shift }}</span>
                </p>
                
                <p class="flex items-center gap-1.5">
                  <component :is="item.room.includes('Online') ? Video : MapPin" :size="12" class="text-muted" />
                  <span>{{ item.room }}</span>
                </p>

                <p class="flex items-center gap-1.5">
                  <User :size="12" class="text-muted" />
                  <span>{{ item.teacher }}</span>
                </p>
              </div>

              <!-- Online link button inside detail card -->
              <div v-if="item.link" class="pt-2">
                <a
                  :href="item.link"
                  target="_blank"
                  class="w-full py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 dark:bg-blue-950/30 dark:text-blue-400 text-[10px] font-bold rounded-lg flex items-center justify-center gap-1 transition"
                >
                  <Video :size="11" /> Vào học Online
                </a>
              </div>
            </div>
          </div>
        </div>

        <div class="text-[10px] text-muted leading-relaxed pt-4 border-t border-card">
          * Phụ huynh vui lòng nhắc nhở con chuẩn bị đầy đủ bài tập và đi học đúng giờ theo thời khóa biểu.
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
