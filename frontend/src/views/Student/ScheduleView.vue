<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import {
  CalendarDays, Clock, MapPin, User, Video, X,
  ChevronLeft, ChevronRight, ExternalLink, Bell,
  AlertTriangle, RefreshCw
} from 'lucide-vue-next'

const viewMode = ref('week')
const today = new Date()
const currentDate = ref(new Date(today))
const selectedEvent = ref(null)
const drawerOpen = ref(false)
useBodyScrollLock(drawerOpen)
const searchSubject = ref('')

import { studentDashboardMock } from '@/data/studentData.mock.js'

const mockSessions = computed(() => {
  const coursesList = studentDashboardMock.courses || []
  if (coursesList.length === 0) return []
  
  const sessions = []
  let idCounter = 1
  
  const todayVal = new Date()
  const dayOfWeek = todayVal.getDay()
  const monday = new Date(todayVal)
  monday.setDate(todayVal.getDate() - (dayOfWeek === 0 ? 6 : dayOfWeek - 1))
  
  const isIT = studentDashboardMock.student?.className?.includes('SE')
  
  coursesList.forEach((course, index) => {
    const teacherName = course.lecturer || 'Giảng viên phụ trách'
    const color = index === 0 ? 'blue' : index === 1 ? 'teal' : index === 2 ? 'violet' : 'amber'
    
    // Buổi 1
    const dayOffset1 = index * 2
    if (dayOffset1 < 7) {
      const date1 = new Date(monday)
      date1.setDate(monday.getDate() + dayOffset1)
      date1.setHours(7, 30, 0, 0)
      const endDate1 = new Date(date1)
      endDate1.setHours(9, 30)
      
      sessions.push({
        id: idCounter++,
        subject: course.name,
        code: course.code || course.id.toUpperCase(),
        teacher: teacherName,
        room: `P.${302 + index * 5}`,
        date: date1,
        endDate: endDate1,
        status: 'published',
        type: 'offline',
        color: color
      })
    }
    
    // Buổi 2
    const dayOffset2 = index * 2 + 3
    if (dayOffset2 < 7) {
      const date2 = new Date(monday)
      date2.setDate(monday.getDate() + dayOffset2)
      date2.setHours(13, 30, 0, 0)
      const endDate2 = new Date(date2)
      endDate2.setHours(15, 30)
      
      let status = 'published'
      let type = 'offline'
      let meetLink = ''
      if (index === 1) {
        status = 'online'
        type = 'online'
        meetLink = 'https://meet.google.com/abc-defg-hij'
      } else if (index === 2 && isIT) {
        status = 'cancelled'
        type = 'offline'
      }
      
      sessions.push({
        id: idCounter++,
        subject: course.name,
        code: course.code || course.id.toUpperCase(),
        teacher: teacherName,
        room: type === 'online' ? 'Online' : `P.${305 + index * 5}`,
        date: date2,
        endDate: endDate2,
        status: status,
        type: type,
        meetLink: meetLink,
        cancelReason: status === 'cancelled' ? 'Giảng viên bận công tác' : undefined,
        color: color
      })
    }
  })
  
  return sessions
})

const statusConfig = {
  published: { label: 'Published', cls: 'badge-published' },
  cancelled:  { label: 'Cancelled', cls: 'badge-cancelled' },
  makeup:     { label: 'Makeup', cls: 'badge-makeup' },
  online:     { label: 'Online', cls: 'badge-online' },
}

function fmt(d){ return d.toLocaleTimeString('vi-VN',{hour:'2-digit',minute:'2-digit'}) }

function goToday(){ currentDate.value = new Date(today) }

function prevPeriod(){
  const d = new Date(currentDate.value)
  if (viewMode.value === 'week') {
    d.setDate(d.getDate() - 7)
  } else {
    d.setMonth(d.getMonth() - 1)
  }
  currentDate.value = d
}

function nextPeriod(){
  const d = new Date(currentDate.value)
  if (viewMode.value === 'week') {
    d.setDate(d.getDate() + 7)
  } else {
    d.setMonth(d.getMonth() + 1)
  }
  currentDate.value = d
}

function openDrawer(ev){
  selectedEvent.value = ev
  drawerOpen.value = true
}
function closeDrawer(){ drawerOpen.value = false }

const uniqueSubjects = computed(() => [...new Set(mockSessions.value.map(s=>s.subject))])

const filteredSessions = computed(() => {
  if(!searchSubject.value) return mockSessions.value
  return mockSessions.value.filter(s=>s.subject===searchSubject.value)
})

const weekDays = computed(() => {
  const d = new Date(currentDate.value)
  const day = d.getDay()
  const diff = d.getDate() - day + (day===0?-6:1)
  d.setDate(diff)
  return Array.from({length:7},(_,i)=>{
    const dd = new Date(d)
    dd.setDate(d.getDate()+i)
    return dd
  })
})

function sessionsForDay(day){
  return filteredSessions.value.filter(s=>{
    const sd = s.date
    return sd.getFullYear()===day.getFullYear() && sd.getMonth()===day.getMonth() && sd.getDate()===day.getDate()
  })
}

function isToday(d){
  return d.getFullYear()===today.getFullYear() && d.getMonth()===today.getMonth() && d.getDate()===today.getDate()
}

const weekLabel = computed(()=>{
  const ds = weekDays.value
  const s = ds[0].toLocaleDateString('vi-VN',{day:'2-digit',month:'2-digit'})
  const e = ds[6].toLocaleDateString('vi-VN',{day:'2-digit',month:'2-digit',year:'numeric'})
  return `${s} – ${e}`
})

const monthDays = computed(()=>{
  const d = new Date(currentDate.value)
  const year = d.getFullYear(), month = d.getMonth()
  const first = new Date(year,month,1)
  const last = new Date(year,month+1,0)
  const days = []
  let startDow = first.getDay(); if(startDow===0)startDow=7
  for(let i=1;i<startDow;i++) days.push(null)
  for(let i=1;i<=last.getDate();i++) days.push(new Date(year,month,i))
  return days
})

const monthLabel = computed(()=>currentDate.value.toLocaleDateString('vi-VN',{month:'long',year:'numeric'}))

const metrics = computed(()=>{
  const total = filteredSessions.value.length
  const online = filteredSessions.value.filter(s=>s.status==='online').length
  const cancelled = filteredSessions.value.filter(s=>s.status==='cancelled').length
  const makeup = filteredSessions.value.filter(s=>s.status==='makeup').length
  return [
    {label:'Tổng buổi học',value:total,unit:'buổi',color:'blue'},
    {label:'Học online',value:online,unit:'buổi',color:'violet'},
    {label:'Đã huỷ',value:cancelled,unit:'buổi',color:'red'},
    {label:'Học bù',value:makeup,unit:'buổi',color:'amber'},
  ]
})
</script>

<template>
  <div class="schedule-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><CalendarDays :size="15"/>Lịch học</div>
        <h1 class="page-title">Thời khóa biểu</h1>
        <p class="page-sub">Lịch học cá nhân theo tuần / tháng. Chỉ hiển thị lịch đã được phê duyệt.</p>
      </div>
      <div class="header-actions">
        <select v-model="searchSubject" class="filter-select">
          <option value="">Tất cả môn học</option>
          <option v-for="s in uniqueSubjects" :key="s" :value="s">{{ s }}</option>
        </select>
      </div>
    </div>

    <!-- Metrics -->
    <div class="metrics-row">
      <div v-for="m in metrics" :key="m.label" class="metric-card" :class="`metric-${m.color}`">
        <div class="metric-val">{{ m.value }}<span class="metric-unit">{{ m.unit }}</span></div>
        <div class="metric-lbl">{{ m.label }}</div>
      </div>
    </div>

    <!-- Calendar Controls -->
    <div class="cal-controls">
      <div class="view-toggle">
        <button :class="['toggle-btn',viewMode==='week'&&'active']" @click="viewMode='week'">Tuần</button>
        <button :class="['toggle-btn',viewMode==='month'&&'active']" @click="viewMode='month'">Tháng</button>
      </div>
      <div class="nav-group">
        <button class="nav-btn" @click="prevPeriod"><ChevronLeft :size="18"/></button>
        <span class="period-label">{{ viewMode==='week'?weekLabel:monthLabel }}</span>
        <button class="nav-btn" @click="nextPeriod"><ChevronRight :size="18"/></button>
      </div>
      <button class="today-btn" @click="goToday">Today</button>
    </div>

    <!-- Week View -->
    <div v-if="viewMode==='week'" class="week-grid">
      <div v-for="day in weekDays" :key="day.toISOString()" class="day-col">
        <div class="day-header" :class="isToday(day)&&'day-today'">
          <span class="day-name">{{ day.toLocaleDateString('vi-VN',{weekday:'short'}) }}</span>
          <span class="day-num">{{ day.getDate() }}</span>
        </div>
        <div class="day-events">
          <div v-for="ev in sessionsForDay(day)" :key="ev.id"
            class="event-card" :class="[`ev-${ev.color}`,ev.status==='cancelled'&&'ev-cancelled']"
            @click="openDrawer(ev)">
            <div class="ev-time">{{ fmt(ev.date) }} – {{ fmt(ev.endDate) }}</div>
            <div class="ev-name">{{ ev.subject }}</div>
            <div class="ev-meta">
              <MapPin :size="11"/>{{ ev.room }}
            </div>
            <span class="ev-badge" :class="statusConfig[ev.status]?.cls">{{ statusConfig[ev.status]?.label }}</span>
          </div>
          <div v-if="sessionsForDay(day).length===0" class="no-event">–</div>
        </div>
      </div>
    </div>

    <!-- Month View -->
    <div v-else class="month-grid">
      <div v-for="d in ['T2','T3','T4','T5','T6','T7','CN']" :key="d" class="month-dow">{{ d }}</div>
      <div v-for="(day,i) in monthDays" :key="i" class="month-cell" :class="day&&isToday(day)&&'cell-today'">
        <template v-if="day">
          <div class="month-day-num">{{ day.getDate() }}</div>
          <div v-for="ev in sessionsForDay(day)" :key="ev.id"
            class="month-ev" :class="`ev-dot-${ev.color}`"
            @click="openDrawer(ev)">
            {{ ev.subject.split(' ')[0] }}
          </div>
        </template>
      </div>
    </div>

    <!-- Event Detail Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="drawerOpen" class="modal-overlay" @click.self="closeDrawer">
          <div v-if="selectedEvent" class="modal-content lg">
            <div class="modal-header">
              <div>
                <span class="ev-badge lg" :class="statusConfig[selectedEvent.status]?.cls">{{ statusConfig[selectedEvent.status]?.label }}</span>
                <h2 class="modal-title">{{ selectedEvent.subject }}</h2>
                <p class="modal-code">{{ selectedEvent.code }}</p>
              </div>
              <button class="close-btn" @click="closeDrawer"><X :size="20"/></button>
            </div>

            <div class="modal-body">
              <div class="info-row"><Clock :size="16" class="info-icon"/><span>{{ fmt(selectedEvent.date) }} – {{ fmt(selectedEvent.endDate) }}</span></div>
              <div class="info-row"><MapPin :size="16" class="info-icon"/><span>{{ selectedEvent.room }}</span></div>
              <div class="info-row"><User :size="16" class="info-icon"/><span>{{ selectedEvent.teacher }}</span></div>
              <div v-if="selectedEvent.type==='online'" class="info-row">
                <Video :size="16" class="info-icon text-violet"/>
                <a :href="selectedEvent.meetLink" target="_blank" class="meet-link">
                  <ExternalLink :size="13"/>Tham gia buổi học online
                </a>
              </div>
              <div v-if="selectedEvent.status==='cancelled'" class="cancel-notice">
                <AlertTriangle :size="15"/>
                {{ selectedEvent.cancelReason || 'Buổi học bị huỷ.' }}
              </div>
              <div v-if="selectedEvent.status==='makeup'" class="makeup-notice">
                <RefreshCw :size="15"/>Đây là buổi học bù.
              </div>
              <div v-if="selectedEvent.type==='online'" class="online-notice">
                <Bell :size="15"/>Sẽ nhận thông báo 30 phút trước khi buổi học bắt đầu.
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn-secondary" @click="closeDrawer">Đóng</button>
              <a v-if="selectedEvent.type==='online'" :href="selectedEvent.meetLink" target="_blank" class="btn-primary">
                <Video :size="15"/>Mở link học
              </a>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.schedule-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.page-title {
  margin: 0.45rem 0 0.2rem;
  color: var(--text-heading);
  font-size: 1.35rem;
  font-weight: 900;
  line-height: 1.15;
}

.page-sub {
  margin: 0;
  color: var(--text-body);
  font-size: 0.82rem;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.filter-select {
  min-height: 2.35rem;
  border: 1px solid var(--border-input);
  border-radius: 12px;
  background: var(--surface-input);
  color: var(--text-label);
  cursor: pointer;
  outline: none;
  padding: 0 0.8rem;
  font-size: 0.82rem;
  font-weight: 700;
}

.filter-select:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.metrics-row {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.metric-card,
.cal-controls,
.day-col,
.month-grid {
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
}

.metric-card {
  border-radius: 16px;
  padding: 0.8rem;
}

.metric-val {
  color: var(--text-heading);
  font-size: 1.1rem;
  font-weight: 900;
  line-height: 1;
}

.metric-unit {
  margin-left: 0.25rem;
  color: var(--text-placeholder);
  font-size: 0.72rem;
  font-weight: 700;
}

.metric-lbl {
  margin-top: 0.25rem;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 800;
}

.metric-blue { box-shadow: inset 3px 0 0 var(--accent-primary), var(--lg-shadow-sm); }
.metric-violet { box-shadow: inset 3px 0 0 var(--accent-violet), var(--lg-shadow-sm); }
.metric-red { box-shadow: inset 3px 0 0 var(--color-danger-text), var(--lg-shadow-sm); }
.metric-amber { box-shadow: inset 3px 0 0 var(--color-warning-text), var(--lg-shadow-sm); }

.cal-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
  border-radius: 18px;
  padding: 0.65rem;
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
}

.view-toggle {
  display: flex;
  gap: 0.2rem;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.2rem;
}

.toggle-btn,
.nav-btn,
.today-btn,
.btn-secondary,
.btn-primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 850;
  transition: transform 160ms ease, border-color 160ms ease, color 160ms ease, background 160ms ease;
}

.toggle-btn {
  min-height: 1.95rem;
  border: 0;
  background: transparent;
  color: var(--text-placeholder);
  padding: 0 0.75rem;
}

.toggle-btn.active {
  background: var(--surface-card-strong);
  color: var(--text-link);
  box-shadow: var(--lg-shadow-sm);
}

.nav-group {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  flex: 1;
}

.nav-btn {
  width: 2.2rem;
  height: 2.2rem;
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-label);
}

.nav-btn:hover {
  border-color: var(--border-input-focus);
  color: var(--text-link);
}

.period-label {
  min-width: 12rem;
  color: var(--text-heading);
  font-size: 0.86rem;
  font-weight: 850;
  text-align: center;
}

.today-btn,
.btn-primary {
  min-height: 2.2rem;
  border: 0;
  background: var(--accent-primary);
  color: var(--text-inverse);
  padding: 0 0.9rem;
  text-decoration: none;
}

.today-btn:hover,
.btn-primary:hover {
  transform: translateY(-1px);
}

.week-grid {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 0.65rem;
}

.day-col {
  min-height: 13rem;
  overflow: hidden;
  border-radius: 16px;
}

.day-header {
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.55rem;
  text-align: center;
}

.day-header.day-today {
  background: var(--accent-primary-soft);
}

.day-name {
  display: block;
  color: var(--text-placeholder);
  font-size: 0.65rem;
  font-weight: 850;
  text-transform: uppercase;
}

.day-num {
  display: block;
  margin-top: 0.1rem;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.day-today .day-num {
  color: var(--text-link);
}

.day-events {
  display: flex;
  flex-direction: column;
  gap: 0.38rem;
  padding: 0.45rem;
}

.no-event {
  color: var(--text-placeholder);
  font-size: 0.7rem;
  padding: 0.9rem;
  text-align: center;
}

.event-card {
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  cursor: pointer;
  padding: 0.5rem;
  transition: transform 160ms ease, box-shadow 160ms ease;
}

.event-card:hover {
  transform: translateY(-1px);
  box-shadow: var(--lg-shadow-sm);
}

.ev-blue { border-left: 3px solid var(--accent-primary); }
.ev-teal { border-left: 3px solid var(--accent-cyan); }
.ev-violet { border-left: 3px solid var(--accent-violet); }
.ev-amber { border-left: 3px solid var(--color-warning-text); }
.ev-red { border-left: 3px solid var(--color-danger-text); }
.ev-cancelled { opacity: 0.72; }

.ev-time {
  color: var(--text-placeholder);
  font-size: 0.64rem;
  font-weight: 750;
}

.ev-name {
  margin: 0.15rem 0;
  color: var(--text-heading);
  font-size: 0.74rem;
  font-weight: 850;
  line-height: 1.3;
}

.ev-meta {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--text-label);
  font-size: 0.65rem;
}

.ev-badge {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  margin-top: 0.25rem;
  padding: 0.16rem 0.48rem;
  font-size: 0.6rem;
  font-weight: 850;
}

.ev-badge.lg {
  margin-bottom: 0.5rem;
  padding: 0.25rem 0.75rem;
  font-size: 0.74rem;
}

.badge-published { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-cancelled { background: var(--color-danger-bg); color: var(--color-danger-text); }
.badge-makeup { background: var(--color-warning-bg); color: var(--color-warning-text); }
.badge-online { background: var(--accent-violet-soft); color: var(--accent-violet); }

.month-grid {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 1px;
  overflow: hidden;
  border-radius: 16px;
}

.month-dow {
  background: var(--surface-input);
  color: var(--text-placeholder);
  padding: 0.5rem;
  font-size: 0.68rem;
  font-weight: 850;
  text-align: center;
  text-transform: uppercase;
}

.month-cell {
  display: flex;
  min-height: 6.25rem;
  flex-direction: column;
  gap: 0.25rem;
  background: var(--surface-card);
  padding: 0.5rem;
}

.month-cell:hover {
  background: var(--surface-input);
}

.cell-today {
  background: var(--accent-primary-soft) !important;
}

.month-day-num {
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 850;
}

.cell-today .month-day-num {
  color: var(--text-link);
}

.month-ev {
  overflow: hidden;
  border-radius: 7px;
  padding: 0.16rem 0.4rem;
  font-size: 0.65rem;
  font-weight: 760;
  text-overflow: ellipsis;
  white-space: nowrap;
  cursor: pointer;
}

.ev-dot-blue { background: var(--accent-primary-soft); color: var(--text-link); }
.ev-dot-teal { background: var(--accent-cyan-soft); color: var(--accent-cyan); }
.ev-dot-violet { background: var(--accent-violet-soft); color: var(--accent-violet); }
.ev-dot-amber { background: var(--color-warning-bg); color: var(--color-warning-text); }
.ev-dot-red { background: var(--color-danger-bg); color: var(--color-danger-text); }

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 9998;
  display: flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, var(--lg-bg-mid) 58%, transparent);
  padding: 1rem;
  backdrop-filter: blur(6px);
}

.modal-content {
  position: relative;
  z-index: 9999;
  display: flex;
  width: 100%;
  max-height: 90vh;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 22px;
  background: var(--surface-modal);
  box-shadow: var(--lg-shadow-lg);
}

.modal-content.lg {
  max-width: 34rem;
}

.modal-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  flex-shrink: 0;
  border-bottom: 1px solid var(--border-card);
  padding: 1rem;
}

.modal-title {
  margin: 0.25rem 0 0.1rem;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
  line-height: 1.3;
}

.modal-code {
  margin: 0;
  color: var(--text-placeholder);
  font-size: 0.78rem;
}

.close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 2.25rem;
  height: 2.25rem;
  border: 1px solid var(--border-input);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  cursor: pointer;
}

.close-btn:hover {
  border-color: var(--color-danger-text);
  color: var(--color-danger-text);
}

.modal-body {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 0.85rem;
  overflow-y: auto;
  padding: 1rem;
}

.info-row {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  color: var(--text-label);
  font-size: 0.85rem;
}

.info-icon {
  flex-shrink: 0;
  color: var(--text-placeholder);
}

.text-violet,
.meet-link {
  color: var(--accent-violet);
}

.meet-link {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-weight: 800;
  text-decoration: none;
}

.cancel-notice,
.makeup-notice,
.online-notice {
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
  border-radius: 12px;
  padding: 0.75rem 0.85rem;
  font-size: 0.8rem;
  font-weight: 750;
}

.cancel-notice { background: var(--color-danger-bg); color: var(--color-danger-text); border: 1px solid color-mix(in srgb, var(--color-danger-text) 18%, transparent); }
.makeup-notice { background: var(--color-warning-bg); color: var(--color-warning-text); border: 1px solid color-mix(in srgb, var(--color-warning-text) 18%, transparent); }
.online-notice { background: var(--accent-violet-soft); color: var(--accent-violet); border: 1px solid color-mix(in srgb, var(--accent-violet) 18%, transparent); }

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.65rem;
  flex-shrink: 0;
  border-top: 1px solid var(--border-card);
  padding: 1rem;
}

.btn-secondary,
.btn-primary {
  min-height: 2.25rem;
  border: 0;
  padding: 0 0.9rem;
}

.btn-secondary {
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-label);
}

.btn-secondary:hover {
  border-color: var(--border-input-focus);
  color: var(--text-link);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.24s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: scale(0.97);
}

@media(max-width: 900px) {
  .metrics-row {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .week-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media(max-width: 640px) {
  .metrics-row,
  .week-grid {
    grid-template-columns: 1fr;
  }

  .period-label {
    min-width: auto;
  }

  .nav-group {
    order: 3;
    width: 100%;
  }
}
</style>
