<script setup>
import { ref } from 'vue'
import dayjs from 'dayjs'
import { Clock, User, Lock } from 'lucide-vue-next'
import { useSchedule } from '@/composables/useSchedule'

const { monthGrid, updateEvent } = useSchedule()
const emit = defineEmits(['edit', 'delete'])

const dayNames = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']

const draggedEvent = ref(null)
const dragOverDay = ref(null)
const dragOverWeekIdx = ref(null)
const dragOverDayIdx = ref(null)

const maxVisibleEvents = 4

function getEventDotColor(event) {
  return event.color || '#6366f1'
}

function getStatusBadge(status) {
  switch (status) {
    case 'published': return 'bg-green-500'
    case 'pending': return 'bg-amber-500'
    case 'draft': return 'bg-slate-400'
    default: return 'bg-slate-400'
  }
}

function formatTime(iso) {
  return dayjs(iso).format('HH:mm')
}

function handleEventClick(evt, e) {
  e.stopPropagation()
  emit('edit', evt)
}

// ── Drag-and-drop ──────────────────────────────────────
function canDrag(evt) {
  if (evt.status === 'published') return false
  if (evt.type === 'exam') return false
  return true
}

function isSameDay(date1, date2) {
  return dayjs(date1).format('YYYY-MM-DD') === dayjs(date2).format('YYYY-MM-DD')
}

function doTimesOverlap(startA, endA, startB, endB) {
  const aStart = dayjs(startA)
  const aEnd = dayjs(endA)
  const bStart = dayjs(startB)
  const bEnd = dayjs(endB)
  return aStart.isBefore(bEnd) && aEnd.isAfter(bStart)
}

function getDragReason(evt) {
  if (evt.status === 'published') return 'Đã công bố — không thể kéo'
  if (evt.type === 'exam') return 'Lịch thi — không thể kéo'
  return ''
}

function handleDragStart(evt, eventData) {
  if (!canDrag(eventData)) {
    evt.preventDefault()
    return
  }
  draggedEvent.value = eventData
  evt.dataTransfer.effectAllowed = 'move'
  evt.dataTransfer.setData('text/plain', String(eventData.id))
  evt.target.classList.add('opacity-50')
}

function handleDragEnd(evt) {
  draggedEvent.value = null
  dragOverDay.value = null
  dragOverWeekIdx.value = null
  dragOverDayIdx.value = null
  evt.target.classList.remove('opacity-50')
}

function handleDragOver(evt, weekIdx, dayIdx, day) {
  evt.preventDefault()
  evt.dataTransfer.dropEffect = 'move'
  dragOverWeekIdx.value = weekIdx
  dragOverDayIdx.value = dayIdx
  dragOverDay.value = day
}

function handleDragLeave() {
  dragOverWeekIdx.value = null
  dragOverDayIdx.value = null
  dragOverDay.value = null
}

function canDropOnDay(day, eventData) {
  if (!day || !day.date || !eventData) return { ok: false, reason: '' }
  if (day.isoDate === dayjs(eventData.start).format('YYYY-MM-DD')) return { ok: false, reason: 'Cùng ngày' }
  if (day.date.isBefore(dayjs(), 'day')) return { ok: false, reason: 'Không thể kéo vào quá khứ' }

  const eventStartTime = dayjs(eventData.start)
  const eventEndTime = dayjs(eventData.end)
  const duration = eventEndTime.diff(eventStartTime)

  const newStart = dayjs(day.isoDate).hour(eventStartTime.hour()).minute(eventStartTime.minute()).second(0)
  const newEnd = newStart.add(duration)

  const conflict = day.events.find(e => {
    if (e.id === eventData.id) return false
    if (e.teacher !== eventData.teacher) return false
    return doTimesOverlap(newStart.toISOString(), newEnd.toISOString(), e.start, e.end)
  })
  if (conflict) return { ok: false, reason: `Giảng viên đã có lịch "${conflict.title}" khung giờ này` }

  return { ok: true, reason: '' }
}

function handleDrop(evt, weekIdx, dayIdx, day) {
  evt.preventDefault()
  dragOverWeekIdx.value = null
  dragOverDayIdx.value = null
  dragOverDay.value = null

  const eventId = parseInt(evt.dataTransfer.getData('text/plain'), 10)
  const eventData = draggedEvent.value
  if (!eventData || eventData.id !== eventId) return

  const check = canDropOnDay(day, eventData)
  if (!check.ok) return

  const eventStartTime = dayjs(eventData.start)
  const eventEndTime = dayjs(eventData.end)
  const duration = eventEndTime.diff(eventStartTime)

  const newStart = dayjs(day.isoDate).hour(eventStartTime.hour()).minute(eventStartTime.minute()).second(0)
  const newEnd = newStart.add(duration)

  updateEvent(eventId, {
    start: newStart.toISOString(),
    end: newEnd.toISOString(),
  })
  draggedEvent.value = null
}

function isDragOver(weekIdx, dayIdx) {
  return dragOverWeekIdx.value === weekIdx && dragOverDayIdx.value === dayIdx
}

function getDropFeedback(day) {
  if (!draggedEvent.value || !day) return null
  const check = canDropOnDay(day, draggedEvent.value)
  return check
}
</script>

<template>
  <div class="lg-glass-strong rounded-2xl overflow-hidden border border-slate-200/70 dark:border-white/10 shadow-sm flex flex-col">
    <!-- Day names header -->
    <div class="grid grid-cols-7 border-b border-slate-200/70 dark:border-white/10 bg-white/30 dark:bg-white/5">
      <div
        v-for="dn in dayNames"
        :key="dn"
        class="px-1 py-3 text-center text-[11px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider"
      >
        {{ dn }}
      </div>
    </div>

    <!-- Calendar grid -->
    <div class="flex-1 overflow-y-auto custom-scroll" style="max-height: calc(100vh - 380px)">
      <div
        v-for="(week, wi) in monthGrid"
        :key="wi"
        class="grid grid-cols-7 border-b border-slate-100/60 dark:border-white/[0.04] last:border-b-0"
      >
        <div
          v-for="(day, di) in week"
          :key="di"
          @dragover="handleDragOver($event, wi, di, day)"
          @dragleave="handleDragLeave"
          @drop="handleDrop($event, wi, di, day)"
          class="min-h-[120px] p-2 border-r border-slate-100/60 dark:border-white/[0.04] last:border-r-0 transition-colors relative group"
          :class="[
            day.isToday ? 'bg-blue-50/50 dark:bg-blue-500/10' : '',
            !day.isCurrentMonth ? 'opacity-40' : '',
            isDragOver(wi, di) && getDropFeedback(day)?.ok ? 'bg-teal-50 dark:bg-teal-500/15 ring-2 ring-teal-400/40 ring-inset' : '',
            isDragOver(wi, di) && getDropFeedback(day) && !getDropFeedback(day).ok ? 'bg-red-50 dark:bg-red-500/10 ring-2 ring-red-400/30 ring-inset' : '',
          ]"
        >
          <!-- Day number -->
          <div
            class="inline-flex items-center justify-center w-7 h-7 text-xs font-bold rounded-full mb-1.5"
            :class="[
              day.isToday
                ? 'bg-teal-600 text-white shadow-sm'
                : day.isCurrentMonth ? 'text-slate-700 dark:text-slate-300' : 'text-slate-400 dark:text-slate-600',
            ]"
          >
            {{ day.dayNum }}
          </div>

          <!-- Drop feedback tooltip -->
          <div
            v-if="isDragOver(wi, di) && getDropFeedback(day) && !getDropFeedback(day).ok"
            class="absolute top-1 right-1 z-10 px-1.5 py-0.5 rounded-md bg-red-500 text-white text-[8px] font-bold whitespace-nowrap shadow-lg"
          >
            {{ getDropFeedback(day).reason }}
          </div>

          <!-- Events -->
          <div class="space-y-1">
            <div
              v-for="evt in day.events.slice(0, maxVisibleEvents)"
              :key="evt.id"
              :draggable="canDrag(evt)"
              @click="handleEventClick(evt, $event)"
              @dragstart="handleDragStart($event, evt)"
              @dragend="handleDragEnd"
              :title="!canDrag(evt) ? getDragReason(evt) : ''"
              class="px-2 py-1.5 rounded-lg text-[11px] leading-tight select-none transition-all hover:scale-[1.02] hover:shadow-md active:scale-[0.98]"
              :class="[
                canDrag(evt) ? 'cursor-grab active:cursor-grabbing' : 'cursor-default',
              ]"
              :style="{
                background: `${getEventDotColor(evt)}18`,
                color: getEventDotColor(evt),
                borderLeft: `3px solid ${getEventDotColor(evt)}`,
              }"
            >
              <!-- Line 1: dot + title + lock -->
              <div class="flex items-center gap-1.5 font-bold truncate">
                <span class="shrink-0 w-2 h-2 rounded-full" :class="getStatusBadge(evt.status)" />
                <span class="truncate">{{ evt.title }}</span>
                <Lock v-if="!canDrag(evt)" :size="10" class="shrink-0 opacity-50" title="Không thể kéo" />
              </div>
              <!-- Line 2: time + teacher -->
              <div class="flex items-center gap-2 mt-0.5 text-[10px] opacity-75 truncate">
                <span class="flex items-center gap-0.5 shrink-0">
                  <Clock :size="10" />
                  {{ formatTime(evt.start) }}–{{ formatTime(evt.end) }}
                </span>
                <span class="flex items-center gap-0.5 truncate">
                  <User :size="10" />
                  <span class="truncate">{{ evt.teacher }}</span>
                </span>
              </div>
            </div>
            <div
              v-if="day.events.length > maxVisibleEvents"
              class="text-[10px] font-bold text-slate-400 dark:text-slate-500 pl-1.5"
            >
              +{{ day.events.length - maxVisibleEvents }} nữa
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scroll::-webkit-scrollbar { width: 5px; height: 5px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 999px; }
.dark .custom-scroll::-webkit-scrollbar-thumb { background: rgba(148, 163, 184, 0.3); }
</style>
