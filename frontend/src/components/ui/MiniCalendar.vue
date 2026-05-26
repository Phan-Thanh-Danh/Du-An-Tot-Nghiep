<script setup>
import { ref, computed } from 'vue'
import * as LucideIcons from 'lucide-vue-next'

const open = ref(false)
const currentDate = ref(new Date())
const viewDate = ref(new Date())
const menuRef = ref(null)

const daysInMonth = computed(() => {
  const year = viewDate.value.getFullYear()
  const month = viewDate.value.getMonth()
  const firstDay = new Date(year, month, 1).getDay()
  const totalDays = new Date(year, month + 1, 0).getDate()
  return { firstDay, totalDays }
})

const monthYear = computed(() => {
  return viewDate.value.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' })
})

const weekDays = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']

const todayStr = computed(() => currentDate.value.toDateString())

function prevMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() - 1, 1)
}

function nextMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() + 1, 1)
}

function isToday(day) {
  const d = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth(), day)
  return d.toDateString() === todayStr.value
}

function toggle() {
  open.value = !open.value
}
</script>

<template>
  <div ref="menuRef" class="relative">
    <button
      class="lg-icon-button flex h-8 w-8 items-center justify-center rounded-xl border border-white/50 dark:border-white/10 bg-white/45 dark:bg-slate-700/40 text-slate-500 dark:text-slate-400 shadow-sm backdrop-blur-xl hover:text-blue-700 dark:hover:text-blue-300 focus:outline-none"
      aria-label="Xem lịch"
      @click="toggle"
    >
      <LucideIcons.Calendar :size="15" />
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
        v-if="open"
        class="absolute right-0 top-[calc(100%+0.5rem)] z-[80] w-[280px] origin-top-right overflow-hidden rounded-[20px] border border-white/60 dark:border-white/10 bg-white/90 dark:bg-slate-900/85 p-3 shadow-[0_20px_50px_rgba(15,23,42,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
        @click.stop
      >
        <div class="flex items-center justify-between mb-3">
          <button class="flex h-7 w-7 items-center justify-center rounded-lg hover:bg-slate-100 dark:hover:bg-white/10 text-slate-500 dark:text-slate-400" @click="prevMonth">
            <LucideIcons.ChevronLeft :size="14" />
          </button>
          <span class="text-sm font-bold text-slate-700 dark:text-slate-200">{{ monthYear }}</span>
          <button class="flex h-7 w-7 items-center justify-center rounded-lg hover:bg-slate-100 dark:hover:bg-white/10 text-slate-500 dark:text-slate-400" @click="nextMonth">
            <LucideIcons.ChevronRight :size="14" />
          </button>
        </div>

        <div class="grid grid-cols-7 gap-0.5 text-center text-[11px] font-semibold">
          <div v-for="d in weekDays" :key="d" class="py-1 text-slate-400 dark:text-slate-500">{{ d }}</div>
          <div v-for="blank in daysInMonth.firstDay" :key="'b-' + blank" />
          <div
            v-for="day in daysInMonth.totalDays"
            :key="day"
            class="flex items-center justify-center rounded-lg py-1 text-xs font-medium transition-colors"
            :class="isToday(day)
              ? 'bg-blue-600 text-white shadow-sm'
              : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-white/10'"
          >
            {{ day }}
          </div>
        </div>

        <div class="mt-3 border-t border-slate-100/50 dark:border-white/10 pt-2 text-center text-[10px] font-semibold text-slate-400 dark:text-slate-500">
          Hôm nay: {{ currentDate.toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }) }}
        </div>
      </div>
    </Transition>
  </div>
</template>
