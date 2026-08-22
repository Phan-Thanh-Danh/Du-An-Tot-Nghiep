<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { Calendar, Clock, ChevronLeft, ChevronRight, Check, X } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: string | null
  label: string
  minDate?: Date | null
  disabled?: boolean
  placeholder?: string
  error?: string
  helperText?: string
  presetType?: 'open' | 'close'
  baseDate?: string | null
}>()

const emit = defineEmits(['update:modelValue'])

const isOpen = ref(false)
const containerRef = ref<HTMLElement | null>(null)

// Current view month & year in calendar
const currentMonth = ref(new Date().getMonth())
const currentYear = ref(new Date().getFullYear())

// Temporary selection inside popover
const selectedDate = ref<Date | null>(null)
const selectedHour = ref('08')
const selectedMinute = ref('00')

// Initialize from modelValue
const initFromValue = () => {
  if (props.modelValue) {
    const d = new Date(props.modelValue)
    if (!isNaN(d.getTime())) {
      selectedDate.value = new Date(d.getFullYear(), d.getMonth(), d.getDate())
      selectedHour.value = String(d.getHours()).padStart(2, '0')
      selectedMinute.value = String(d.getMinutes()).padStart(2, '0')
      currentMonth.value = d.getMonth()
      currentYear.value = d.getFullYear()
      return
    }
  }

  // Default to minDate if provided, or today
  const base = props.minDate ? new Date(props.minDate) : new Date()
  selectedDate.value = null
  selectedHour.value = '08'
  selectedMinute.value = '00'
  currentMonth.value = base.getMonth()
  currentYear.value = base.getFullYear()
}

watch(() => props.modelValue, initFromValue, { immediate: true })

const displayFormatted = computed(() => {
  if (!props.modelValue) return ''
  const d = new Date(props.modelValue)
  if (isNaN(d.getTime())) return ''
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  const hours = String(d.getHours()).padStart(2, '0')
  const minutes = String(d.getMinutes()).padStart(2, '0')
  return `${day}/${month}/${year} ${hours}:${minutes}`
})

const daysOfWeek = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']

const monthNames = [
  'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
  'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'
]

// Generate calendar cells for currentMonth & currentYear
const calendarDays = computed(() => {
  const year = currentYear.value
  const month = currentMonth.value
  
  const firstDayOfMonth = new Date(year, month, 1)
  const lastDayOfMonth = new Date(year, month + 1, 0)
  
  // 0 = Sunday, 1 = Monday -> map to Monday=0
  let firstDayIndex = firstDayOfMonth.getDay() - 1
  if (firstDayIndex === -1) firstDayIndex = 6

  const daysInMonth = lastDayOfMonth.getDate()

  const days: Array<{
    date: Date
    dayNumber: number
    isCurrentMonth: boolean
    isDisabled: boolean
    isSelected: boolean
    isToday: boolean
  }> = []

  const today = new Date()
  const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime()
  
  const minStart = props.minDate 
    ? new Date(props.minDate.getFullYear(), props.minDate.getMonth(), props.minDate.getDate()).getTime()
    : todayStart

  // Previous month trailing days
  const prevMonthLastDay = new Date(year, month, 0).getDate()
  for (let i = firstDayIndex - 1; i >= 0; i--) {
    const d = new Date(year, month - 1, prevMonthLastDay - i)
    days.push({
      date: d,
      dayNumber: d.getDate(),
      isCurrentMonth: false,
      isDisabled: true,
      isSelected: false,
      isToday: false
    })
  }

  // Current month days
  for (let i = 1; i <= daysInMonth; i++) {
    const d = new Date(year, month, i)
    const time = d.getTime()
    const isSelected = selectedDate.value ? d.toDateString() === selectedDate.value.toDateString() : false
    const isToday = time === todayStart
    const isDisabled = time < minStart

    days.push({
      date: d,
      dayNumber: i,
      isCurrentMonth: true,
      isDisabled,
      isSelected,
      isToday
    })
  }

  // Next month leading days to complete grid (42 cells)
  const remaining = 42 - days.length
  for (let i = 1; i <= remaining; i++) {
    const d = new Date(year, month + 1, i)
    days.push({
      date: d,
      dayNumber: i,
      isCurrentMonth: false,
      isDisabled: true,
      isSelected: false,
      isToday: false
    })
  }

  return days
})

const prevMonth = () => {
  if (currentMonth.value === 0) {
    currentMonth.value = 11
    currentYear.value--
  } else {
    currentMonth.value--
  }
}

const nextMonth = () => {
  if (currentMonth.value === 11) {
    currentMonth.value = 0
    currentYear.value++
  } else {
    currentMonth.value++
  }
}

const selectDay = (day: typeof calendarDays.value[0]) => {
  if (day.isDisabled || !day.isCurrentMonth) return
  selectedDate.value = new Date(day.date)
}

const hoursList = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'))
const minutesList = ['00', '05', '10', '15', '20', '25', '30', '35', '40', '45', '50', '55']

const applyQuickPreset = (preset: string) => {
  const now = new Date()
  const base = props.baseDate ? new Date(props.baseDate) : now

  if (preset === 'now') {
    selectedDate.value = new Date(now.getFullYear(), now.getMonth(), now.getDate())
    selectedHour.value = String(now.getHours()).padStart(2, '0')
    selectedMinute.value = String(now.getMinutes()).padStart(2, '0')
  } else if (preset === 'tomorrow_morning') {
    const tom = new Date(now.getTime() + 24 * 60 * 60 * 1000)
    selectedDate.value = new Date(tom.getFullYear(), tom.getMonth(), tom.getDate())
    selectedHour.value = '08'
    selectedMinute.value = '00'
  } else if (preset === 'plus_3_days') {
    const d = new Date(base.getTime() + 3 * 24 * 60 * 60 * 1000)
    selectedDate.value = new Date(d.getFullYear(), d.getMonth(), d.getDate())
    selectedHour.value = '23'
    selectedMinute.value = '59'
  } else if (preset === 'plus_7_days') {
    const d = new Date(base.getTime() + 7 * 24 * 60 * 60 * 1000)
    selectedDate.value = new Date(d.getFullYear(), d.getMonth(), d.getDate())
    selectedHour.value = '23'
    selectedMinute.value = '59'
  }

  if (selectedDate.value) {
    currentMonth.value = selectedDate.value.getMonth()
    currentYear.value = selectedDate.value.getFullYear()
  }
}

const confirmSelection = () => {
  if (!selectedDate.value) {
    emit('update:modelValue', null)
    isOpen.value = false
    return
  }

  const year = selectedDate.value.getFullYear()
  const month = selectedDate.value.getMonth()
  const day = selectedDate.value.getDate()
  const h = parseInt(selectedHour.value, 10) || 0
  const m = parseInt(selectedMinute.value, 10) || 0

  const finalDate = new Date(year, month, day, h, m, 0)
  emit('update:modelValue', finalDate.toISOString())
  isOpen.value = false
}

const clearValue = (e: MouseEvent) => {
  e.stopPropagation()
  selectedDate.value = null
  emit('update:modelValue', null)
  isOpen.value = false
}

const handleClickOutside = (event: MouseEvent) => {
  if (containerRef.value && !containerRef.value.contains(event.target as Node)) {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <div class="flex-1" ref="containerRef">
    <label class="block text-sm font-medium text-slate-700 dark:text-slate-200 mb-1.5">
      {{ label }}
    </label>

    <!-- Trigger wrapper with relative positioning so popover is placed directly below input -->
    <div class="relative">
      <!-- Trigger Button / Display -->
      <div 
        @click="!disabled && (isOpen = !isOpen)"
        class="flex items-center justify-between px-3.5 py-2.5 bg-white dark:bg-slate-900 border rounded-xl cursor-pointer transition-all shadow-xs"
        :class="[
          disabled ? 'bg-slate-50 dark:bg-slate-800 text-slate-400 cursor-not-allowed border-slate-200 dark:border-slate-700' : 'hover:border-blue-400 text-slate-800 dark:text-slate-100',
          error ? 'border-red-400 ring-2 ring-red-100 dark:ring-red-950' : 'border-slate-300 dark:border-slate-700',
          isOpen ? 'border-blue-500 ring-2 ring-blue-100 dark:ring-blue-900' : ''
        ]"
      >
        <div class="flex items-center gap-2.5 min-w-0">
          <Calendar class="w-4 h-4 shrink-0" :class="displayFormatted ? 'text-blue-600 dark:text-blue-400' : 'text-slate-400'" />
          <span class="text-sm font-semibold truncate" v-if="displayFormatted">
            {{ displayFormatted }}
          </span>
          <span class="text-sm text-slate-400 select-none" v-else>
            {{ placeholder || 'Bấm chọn ngày & giờ...' }}
          </span>
        </div>

        <div class="flex items-center gap-1">
          <button 
            v-if="displayFormatted && !disabled"
            @click="clearValue"
            type="button"
            class="p-1 text-slate-400 hover:text-red-500 rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            title="Xóa thời gian"
          >
            <X class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      <!-- Interactive Calendar Popover directly below input (mt-1.5, z-[9999] important) -->
      <div 
        v-if="isOpen"
        class="absolute top-full left-0 mt-1.5 z-[9999] w-[330px] bg-white dark:bg-slate-900 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-700 p-4 animate-fade-in-up"
        style="z-index: 9999 !important;"
      >
        <!-- Popover Header -->
        <div class="flex items-center justify-between mb-3">
          <h4 class="font-bold text-slate-800 dark:text-slate-100 text-sm">
            {{ monthNames[currentMonth] }} năm {{ currentYear }}
          </h4>
          <div class="flex items-center gap-1">
            <button 
              type="button"
              @click="prevMonth"
              class="p-1.5 text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
            >
              <ChevronLeft class="w-4 h-4" />
            </button>
            <button 
              type="button"
              @click="nextMonth"
              class="p-1.5 text-slate-500 hover:text-slate-800 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
            >
              <ChevronRight class="w-4 h-4" />
            </button>
          </div>
        </div>

        <!-- Quick Shortcuts -->
        <div class="flex items-center gap-1.5 mb-3 flex-wrap">
          <template v-if="presetType === 'open'">
            <button 
              type="button"
              @click="applyQuickPreset('now')"
              class="px-2.5 py-1 bg-slate-100 dark:bg-slate-800 hover:bg-blue-50 dark:hover:bg-blue-950/50 hover:text-blue-600 dark:hover:text-blue-400 text-slate-600 dark:text-slate-300 rounded text-xs font-medium transition-colors"
            >
              Hiện tại
            </button>
            <button 
              type="button"
              @click="applyQuickPreset('tomorrow_morning')"
              class="px-2.5 py-1 bg-slate-100 dark:bg-slate-800 hover:bg-blue-50 dark:hover:bg-blue-950/50 hover:text-blue-600 dark:hover:text-blue-400 text-slate-600 dark:text-slate-300 rounded text-xs font-medium transition-colors"
            >
              Sáng mai (08:00)
            </button>
          </template>
          <template v-else-if="presetType === 'close'">
            <button 
              type="button"
              @click="applyQuickPreset('plus_3_days')"
              class="px-2.5 py-1 bg-blue-50 dark:bg-blue-950/50 text-blue-700 dark:text-blue-300 border border-blue-200 dark:border-blue-800 rounded text-xs font-medium hover:bg-blue-100 dark:hover:bg-blue-900 transition-colors"
            >
              +3 ngày (Tối thiểu)
            </button>
            <button 
              type="button"
              @click="applyQuickPreset('plus_7_days')"
              class="px-2.5 py-1 bg-slate-100 dark:bg-slate-800 hover:bg-blue-50 dark:hover:bg-blue-950/50 hover:text-blue-600 dark:hover:text-blue-400 text-slate-600 dark:text-slate-300 rounded text-xs font-medium transition-colors"
            >
              +7 ngày
            </button>
          </template>
        </div>

        <!-- Days Grid Header -->
        <div class="grid grid-cols-7 gap-1 text-center mb-1">
          <span 
            v-for="d in daysOfWeek" 
            :key="d" 
            class="text-[11px] font-bold text-slate-400 uppercase py-1"
          >
            {{ d }}
          </span>
        </div>

        <!-- Days Grid Cells -->
        <div class="grid grid-cols-7 gap-1 text-center mb-4">
          <button
            v-for="(day, idx) in calendarDays"
            :key="idx"
            type="button"
            :disabled="day.isDisabled || !day.isCurrentMonth"
            @click="selectDay(day)"
            class="h-8 rounded-lg text-xs font-medium flex items-center justify-center transition-all relative"
            :class="[
              !day.isCurrentMonth ? 'text-slate-300 dark:text-slate-600 pointer-events-none' : '',
              day.isDisabled && day.isCurrentMonth ? 'text-slate-300 dark:text-slate-600 bg-slate-50 dark:bg-slate-800/40 cursor-not-allowed pointer-events-none line-through' : '',
              !day.isDisabled && day.isCurrentMonth && !day.isSelected ? 'hover:bg-blue-50 dark:hover:bg-blue-950/50 text-slate-700 dark:text-slate-200 hover:text-blue-600 dark:hover:text-blue-400 hover:font-bold' : '',
              day.isSelected ? 'bg-blue-600 text-white font-bold shadow-md shadow-blue-500/20' : '',
              day.isToday && !day.isSelected ? 'border border-blue-400 font-bold text-blue-600 dark:text-blue-400' : ''
            ]"
          >
            {{ day.dayNumber }}
          </button>
        </div>

        <!-- Time Picker Section -->
        <div class="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between mb-4">
          <div class="flex items-center gap-1.5 text-xs font-semibold text-slate-700 dark:text-slate-300">
            <Clock class="w-4 h-4 text-slate-400" />
            <span>Chọn giờ:</span>
          </div>
          <div class="flex items-center gap-1.5">
            <!-- Hours Dropdown -->
            <select 
              v-model="selectedHour" 
              class="bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg px-2 py-1 text-xs font-bold text-slate-800 dark:text-slate-100 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option v-for="h in hoursList" :key="h" :value="h">{{ h }}</option>
            </select>
            <span class="text-xs font-bold text-slate-400">:</span>
            <!-- Minutes Dropdown -->
            <select 
              v-model="selectedMinute" 
              class="bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg px-2 py-1 text-xs font-bold text-slate-800 dark:text-slate-100 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option v-for="m in minutesList" :key="m" :value="m">{{ m }}</option>
            </select>
          </div>
        </div>

        <!-- Footer Buttons -->
        <div class="flex items-center justify-between pt-2 border-t border-slate-100 dark:border-slate-800">
          <button 
            type="button"
            @click="isOpen = false"
            class="px-3 py-1.5 text-xs font-medium text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
          >
            Hủy
          </button>
          <button 
            type="button"
            @click="confirmSelection"
            class="px-4 py-1.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-lg shadow-sm shadow-blue-500/20 transition-all flex items-center gap-1"
          >
            <Check class="w-3.5 h-3.5" /> Xác nhận
          </button>
        </div>
      </div>
    </div>

    <p v-if="error" class="mt-1.5 text-xs text-red-600 font-medium">{{ error }}</p>
    <p v-else-if="helperText" class="mt-1.5 text-xs text-slate-500 dark:text-slate-400">{{ helperText }}</p>
  </div>
</template>

<style scoped>
.animate-fade-in-up {
  animation: popIn 0.2s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes popIn {
  from {
    opacity: 0;
    transform: translateY(4px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
