<script setup>
import { ref, computed, onMounted } from 'vue'
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
  CalendarDays,
  AlertCircle
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'

const route = useRoute()
const router = useRouter()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')))
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref(null)
const schedule = ref([])
const children = ref([])

const viewMode = ref('week')
const activeWeekIdx = ref(0)

const dayNames = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật']

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || {
    id: activeChildId.value,
    name: 'Học sinh',
    className: ''
  }
})

const scheduleByDay = computed(() => {
  const groups = {}
  schedule.value.forEach(item => {
    const dayIdx = Number(item.day)
    const dayName = dayIdx >= 1 && dayIdx <= 7 ? dayNames[dayIdx - 1] : `Ngày ${item.day}`
    if (!groups[dayName]) groups[dayName] = []
    groups[dayName].push(item)
  })
  return dayNames.map(name => ({
    day: name,
    items: groups[name] || []
  }))
})

const dayScheduleMap = computed(() => {
  const map = {}
  schedule.value.forEach(item => {
    const dayIdx = Number(item.day)
    const dayName = dayIdx >= 1 && dayIdx <= 7 ? dayNames[dayIdx - 1] : `Ngày ${item.day}`
    map[dayName] = true
  })
  return map
})

const selectedMonthDay = ref(new Date().getDate())
const today = new Date()
const currentYear = today.getFullYear()
const currentMonth = today.getMonth()

const monthDays = computed(() => {
  const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate()
  const firstDayOfWeek = new Date(currentYear, currentMonth, 1).getDay()
  const startOffset = firstDayOfWeek === 0 ? 6 : firstDayOfWeek - 1
  const days = []
  for (let i = 0; i < startOffset; i++) {
    days.push({ dayNumber: null, weekday: '', filler: true })
  }
  for (let i = 1; i <= daysInMonth; i++) {
    const date = new Date(currentYear, currentMonth, i)
    const dayIdx = date.getDay()
    const dayName = dayIdx === 0 ? 'Chủ Nhật' : dayNames[dayIdx - 1]
    days.push({
      dayNumber: i,
      weekday: dayName,
      hasClass: !!dayScheduleMap.value[dayName],
      hasExam: false,
      filler: false
    })
  }
  return days
})

const selectedDaySchedule = computed(() => {
  const date = new Date(currentYear, currentMonth, selectedMonthDay.value)
  const dayIdx = date.getDay()
  const dayName = dayIdx === 0 ? 'Chủ Nhật' : dayNames[dayIdx - 1]
  return scheduleByDay.value.find(g => g.day === dayName)?.items || []
})

function selectChild(id) {
  activeChildId.value = id
  localStorage.setItem('parent_active_student_id', id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
  loading.value = true
  error.value = null
  parentApi.getChildSchedule(id).then(res => {
    schedule.value = res.data || []
  }).catch(e => {
    error.value = e.message
  }).finally(() => {
    loading.value = false
  })
}

function prevWeek() {
  if (activeWeekIdx.value > 0) activeWeekIdx.value--
}

function nextWeek() {
  if (activeWeekIdx.value < 3) activeWeekIdx.value++
}

function goBack() {
  router.push('/parent/dashboard')
}

onMounted(async () => {
  try {
    const [childrenRes, scheduleRes] = await Promise.all([
      parentApi.getChildren().catch(() => ({ data: [] })),
      parentApi.getChildSchedule(activeChildId.value)
    ])
    children.value = childrenRes.data || []
    schedule.value = scheduleRes.data || []
  } catch (e) {
    error.value = e.message || 'Không thể tải dữ liệu thời khóa biểu'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="space-y-6">
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="text-xs text-muted">Đang tải dữ liệu...</div>
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 gap-3">
      <AlertCircle :size="28" class="text-red-500" />
      <p class="text-xs text-red-600 font-semibold">{{ error }}</p>
      <button @click="goBack" class="px-3 py-1.5 text-xs font-bold rounded-lg surface-card border border-card text-label hover:text-orange-600">Quay lại</button>
    </div>
    <template v-else>
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
              class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-(--lg-shadow-md)"
            >
              <button
                v-for="child in children"
                :key="child.id"
                type="button"
                class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
                @click="selectChild(child.id)"
              >
                <span>{{ child.name }} ({{ child.className }})</span>
              </button>
            </div>
          </Transition>
        </div>
      </div>

      <!-- ── THAO TÁC / CHẾ ĐỘ XEM ── -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 lg-card-glass">
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

        <div v-if="viewMode === 'week'" class="flex items-center gap-3">
          <button
            @click="prevWeek"
            :disabled="activeWeekIdx === 0"
            class="h-8 w-8 flex items-center justify-center border border-card surface-card rounded-lg text-label hover:text-orange-600 disabled:opacity-40 disabled:pointer-events-none transition"
          >
            <ChevronLeft :size="16" />
          </button>
          <span class="text-xs font-bold text-heading min-w-[150px] text-center">
            Tuần lễ hiện tại
          </span>
          <button
            @click="nextWeek"
            :disabled="activeWeekIdx >= 3"
            class="h-8 w-8 flex items-center justify-center border border-card surface-card rounded-lg text-label hover:text-orange-600 disabled:opacity-40 disabled:pointer-events-none transition"
          >
            <ChevronRight :size="16" />
          </button>
        </div>

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
          v-for="group in scheduleByDay"
          :key="group.day"
          class="lg-card-glass p-4 flex flex-col justify-between hover:border-orange-500/20 transition-all duration-300 relative group min-h-[200px]"
          :class="group.items.length === 0 ? 'opacity-60' : ''"
        >
          <div class="pb-2 border-b border-card mb-3">
            <h3 class="text-xs font-extrabold text-heading flex justify-between items-center">
              <span>{{ group.day }}</span>
            </h3>
          </div>

          <div class="flex-1 space-y-4">
            <div v-if="group.items.length === 0" class="h-full flex items-center justify-center py-8">
              <span class="text-[11px] text-muted italic font-medium">Không có lịch</span>
            </div>
            <div v-else v-for="(item, idx) in group.items" :key="idx" class="space-y-2">
              <p class="text-xs font-bold text-heading leading-tight group-hover:text-orange-600 transition-colors">
                {{ item.subject }}
              </p>
              <div class="space-y-1.5 text-[11px] text-body font-medium">
                <p class="flex items-center gap-1.5">
                  <Clock :size="12" class="text-muted" />
                  <span>{{ item.time }}</span>
                </p>
                <p class="flex items-center gap-1.5">
                  <component :is="(item.room || '').toLowerCase().includes('online') ? Video : MapPin" :size="12" class="text-muted" />
                  <span :class="(item.room || '').toLowerCase().includes('online') ? 'text-orange-600 dark:text-orange-400 font-semibold' : ''">
                    {{ item.room }}
                  </span>
                </p>
                <p class="flex items-center gap-1.5">
                  <User :size="12" class="text-muted" />
                  <span>{{ item.teacher }}</span>
                </p>
              </div>
              <div v-if="idx < group.items.length - 1" class="border-t border-card/50 my-2"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── CHẾ ĐỘ XEM THEO THÁNG (MONTH VIEW) ── -->
      <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div class="lg:col-span-2 lg-card-glass p-5 space-y-4">
          <div class="flex items-center justify-between pb-3 border-b border-card">
            <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
              Tháng {{ currentMonth + 1 }} - {{ currentYear }}
            </h3>
            <span class="text-[10px] text-muted font-semibold">Chọn ngày để xem chi tiết lịch học phía dưới</span>
          </div>

          <div class="grid grid-cols-7 gap-2 text-center text-xs">
            <div v-for="h in ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']" :key="h" class="py-1.5 font-bold text-muted text-[10px]">
              {{ h }}
            </div>

            <template v-for="d in monthDays" :key="d.filler ? 'f-'+d.dayNumber : d.dayNumber">
              <div v-if="d.filler" class="py-2.5"></div>
              <button
                v-else
                @click="selectedMonthDay = d.dayNumber"
                class="py-2.5 rounded-xl border flex flex-col items-center justify-between gap-1 relative transition"
                :class="
                  selectedMonthDay === d.dayNumber ? 'bg-orange-600 border-orange-600 text-white font-extrabold shadow-sm' :
                  d.hasClass ? 'border-card surface-card hover:bg-orange-50/40' :
                  'border-transparent text-disabled opacity-40'
                "
              >
                <span>{{ d.dayNumber }}</span>
                <span v-if="d.hasClass" class="flex gap-0.5">
                  <span class="h-1 w-1 rounded-full bg-orange-500" />
                </span>
              </button>
            </template>
          </div>
        </div>

        <div class="lg-card-glass p-5 space-y-4 flex flex-col justify-between">
          <div>
            <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card mb-4">
              Lịch ngày {{ selectedMonthDay }}/{{ currentMonth + 1 }}/{{ currentYear }}
            </h3>

            <div v-if="selectedDaySchedule.length === 0" class="text-center py-12 text-muted italic text-xs">
              Không có lịch học hoặc lịch thi trong ngày này.
            </div>
            <div v-else class="space-y-4">
              <div
                v-for="(item, idx) in selectedDaySchedule"
                :key="idx"
                class="p-4 rounded-xl border space-y-3 border-card"
              >
                <div class="flex items-center justify-between">
                  <span class="text-[10px] font-bold text-orange-600 bg-orange-50 dark:bg-orange-950/20 px-2 py-0.5 rounded">Lớp học phần</span>
                </div>

                <h4 class="text-xs font-bold text-heading leading-tight">{{ item.subject }}</h4>

                <div class="space-y-1.5 text-[11px] text-body font-medium">
                  <p class="flex items-center gap-1.5">
                    <Clock :size="12" class="text-muted" />
                    <span>{{ item.time }}</span>
                  </p>
                  <p class="flex items-center gap-1.5">
                    <component :is="(item.room || '').toLowerCase().includes('online') ? Video : MapPin" :size="12" class="text-muted" />
                    <span :class="(item.room || '').toLowerCase().includes('online') ? 'text-orange-600 dark:text-orange-400 font-semibold' : ''">
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
          </div>

          <div class="text-[10px] text-muted leading-relaxed pt-4 border-t border-card">
            * Phụ huynh vui lòng nhắc nhở con chuẩn bị đầy đủ bài tập và đi học đúng giờ theo thời khóa biểu.
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
</style>
