<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import {
  CalendarDays, Clock, MapPin, User, Video, X,
  ChevronLeft, ChevronRight, ExternalLink, Bell,
  AlertTriangle, RefreshCw, CheckCircle2, Filter
} from 'lucide-vue-next'

const viewMode = ref('week')
const today = new Date()
const currentDate = ref(new Date(today))
const selectedEvent = ref(null)
const drawerOpen = ref(false)
useBodyScrollLock(drawerOpen)
const searchSubject = ref('')

const mockSessions = [
  { id: 1, subject: 'Cấu trúc dữ liệu & Giải thuật', code: 'CS301', teacher: 'TS. Nguyễn Minh Khoa', room: 'P.302', date: new Date(2026,4,26,7,30), endDate: new Date(2026,4,26,9,30), status: 'published', type: 'offline', color: 'blue' },
  { id: 2, subject: 'Toán rời rạc', code: 'MA201', teacher: 'ThS. Trần Thu Hà', room: 'P.105', date: new Date(2026,4,26,13,30), endDate: new Date(2026,4,26,15,30), status: 'published', type: 'offline', color: 'teal' },
  { id: 3, subject: 'Lập trình Web', code: 'CS402', teacher: 'KS. Lê Văn Tâm', room: 'Online', date: new Date(2026,4,27,8,0), endDate: new Date(2026,4,27,11,0), status: 'online', type: 'online', meetLink: 'https://meet.google.com/abc-defg-hij', color: 'violet' },
  { id: 4, subject: 'Kiến trúc máy tính', code: 'CS205', teacher: 'PGS. Phạm Thị Lan', room: 'P.201', date: new Date(2026,4,27,13,0), endDate: new Date(2026,4,27,15,0), status: 'cancelled', type: 'offline', cancelReason: 'Giảng viên bận công tác', color: 'red' },
  { id: 5, subject: 'Mạng máy tính', code: 'CS301', teacher: 'TS. Hoàng Đức Minh', room: 'P.401', date: new Date(2026,4,28,7,30), endDate: new Date(2026,4,28,9,30), status: 'makeup', type: 'offline', color: 'amber' },
  { id: 6, subject: 'Cấu trúc dữ liệu & Giải thuật', code: 'CS301', teacher: 'TS. Nguyễn Minh Khoa', room: 'P.302', date: new Date(2026,4,29,7,30), endDate: new Date(2026,4,29,9,30), status: 'published', type: 'offline', color: 'blue' },
  { id: 7, subject: 'Lập trình Web', code: 'CS402', teacher: 'KS. Lê Văn Tâm', room: 'Online', date: new Date(2026,4,30,8,0), endDate: new Date(2026,4,30,11,0), status: 'online', type: 'online', meetLink: 'https://zoom.us/j/12345678', color: 'violet' },
]

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

const uniqueSubjects = computed(() => [...new Set(mockSessions.map(s=>s.subject))])

const filteredSessions = computed(() => {
  if(!searchSubject.value) return mockSessions
  return mockSessions.filter(s=>s.subject===searchSubject.value)
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
.schedule-page{padding:2rem;max-width:1400px;margin:0 auto;display:flex;flex-direction:column;gap:1.5rem}

.page-header{display:flex;justify-content:space-between;align-items:flex-start;gap:1rem;flex-wrap:wrap}
.eyebrow{display:flex;align-items:center;gap:.375rem;font-size:.7rem;font-weight:700;text-transform:uppercase;letter-spacing:.08em;color:#2563eb;margin-bottom:.4rem}
.page-title{font-size:1.875rem;font-weight:800;color:#0f172a;margin:0 0 .25rem;letter-spacing:-.02em}
.page-sub{font-size:.875rem;color:#64748b;margin:0}
.header-actions{display:flex;align-items:center;gap:.75rem}

.filter-select{padding:.5rem 1rem;border-radius:12px;border:1px solid rgba(148,163,184,.3);background:rgba(255,255,255,.7);color:#374151;font-size:.85rem;font-weight:500;cursor:pointer;backdrop-filter:blur(8px);outline:none}
.filter-select:focus{border-color:#2563eb}

.metrics-row{display:grid;grid-template-columns:repeat(4,1fr);gap:1rem}
.metric-card{background:rgba(255,255,255,.72);border:1px solid rgba(255,255,255,.5);border-radius:20px;padding:1.25rem;backdrop-filter:saturate(160%) blur(16px);box-shadow:0 4px 20px rgba(15,23,42,.07),inset 0 1px 0 rgba(255,255,255,.3);transition:transform .2s}
.metric-card:hover{transform:translateY(-2px)}
.metric-val{font-size:1.75rem;font-weight:800;color:#0f172a;line-height:1}
.metric-unit{font-size:.8rem;font-weight:500;color:#94a3b8;margin-left:4px}
.metric-lbl{font-size:.78rem;font-weight:600;color:#475569;margin-top:.25rem}
.metric-blue .metric-val{color:#2563eb}
.metric-violet .metric-val{color:#7c3aed}
.metric-red .metric-val{color:#dc2626}
.metric-amber .metric-val{color:#d97706}

.cal-controls{display:flex;align-items:center;gap:1rem;flex-wrap:wrap;background:rgba(255,255,255,.72);border:1px solid rgba(255,255,255,.5);border-radius:16px;padding:.875rem 1.25rem;backdrop-filter:saturate(160%) blur(16px);box-shadow:0 4px 20px rgba(15,23,42,.07)}
.view-toggle{display:flex;background:rgba(241,245,249,.8);border-radius:10px;padding:3px;gap:2px}
.toggle-btn{padding:.375rem .875rem;border-radius:8px;font-size:.8125rem;font-weight:600;color:#64748b;cursor:pointer;border:none;background:transparent;transition:all .15s}
.toggle-btn.active{background:#fff;color:#0f172a;box-shadow:0 1px 4px rgba(15,23,42,.1)}
.nav-group{display:flex;align-items:center;gap:.5rem;flex:1;justify-content:center}
.nav-btn{width:36px;height:36px;border-radius:10px;border:1px solid rgba(148,163,184,.3);background:rgba(255,255,255,.7);color:#475569;cursor:pointer;display:flex;align-items:center;justify-content:center;transition:all .15s}
.nav-btn:hover{border-color:#2563eb;color:#2563eb}
.period-label{font-size:.9rem;font-weight:700;color:#0f172a;min-width:200px;text-align:center}
.today-btn{padding:.5rem 1.25rem;border-radius:10px;background:#2563eb;color:#fff;font-size:.8125rem;font-weight:700;border:none;cursor:pointer;box-shadow:0 4px 14px rgba(37,99,235,.28);transition:all .15s}
.today-btn:hover{background:#1d4ed8;transform:translateY(-1px)}

.week-grid{display:grid;grid-template-columns:repeat(7,1fr);gap:.75rem}
.day-col{background:rgba(255,255,255,.6);border:1px solid rgba(255,255,255,.5);border-radius:16px;overflow:hidden;backdrop-filter:blur(12px);min-height:200px}
.day-header{padding:.625rem;text-align:center;background:rgba(248,250,252,.8);border-bottom:1px solid rgba(148,163,184,.12)}
.day-header.day-today{background:rgba(37,99,235,.08)}
.day-name{display:block;font-size:.68rem;font-weight:700;text-transform:uppercase;color:#94a3b8;letter-spacing:.06em}
.day-num{display:block;font-size:1.1rem;font-weight:800;color:#0f172a;margin-top:.1rem}
.day-today .day-num{color:#2563eb}
.day-events{padding:.5rem;display:flex;flex-direction:column;gap:.375rem}
.no-event{font-size:.7rem;color:#cbd5e1;text-align:center;padding:1rem}

.event-card{border-radius:10px;padding:.5rem .625rem;cursor:pointer;transition:all .15s;border:1px solid transparent}
.event-card:hover{transform:translateY(-1px);box-shadow:0 4px 12px rgba(15,23,42,.1)}
.ev-blue{background:rgba(37,99,235,.08);border-color:rgba(37,99,235,.2)}
.ev-teal{background:rgba(15,118,110,.08);border-color:rgba(15,118,110,.2)}
.ev-violet{background:rgba(124,58,237,.08);border-color:rgba(124,58,237,.2)}
.ev-amber{background:rgba(217,119,6,.08);border-color:rgba(217,119,6,.2)}
.ev-red{background:rgba(220,38,38,.06);border-color:rgba(220,38,38,.15)}
.ev-cancelled{opacity:.6;text-decoration-line:none}
.ev-time{font-size:.65rem;font-weight:600;color:#94a3b8}
.ev-name{font-size:.75rem;font-weight:700;color:#0f172a;line-height:1.3;margin:.15rem 0}
.ev-meta{display:flex;align-items:center;gap:.25rem;font-size:.65rem;color:#64748b}

.ev-badge{display:inline-block;padding:.15rem .5rem;border-radius:9999px;font-size:.6rem;font-weight:700;letter-spacing:.04em;margin-top:.25rem}
.ev-badge.lg{padding:.25rem .75rem;font-size:.75rem;margin-bottom:.5rem;display:inline-block}
.badge-published{background:rgba(22,163,74,.1);color:#15803d}
.badge-cancelled{background:rgba(220,38,38,.1);color:#b91c1c}
.badge-makeup{background:rgba(217,119,6,.1);color:#b45309}
.badge-online{background:rgba(124,58,237,.1);color:#6d28d9}

.month-grid{display:grid;grid-template-columns:repeat(7,1fr);gap:1px;background:rgba(148,163,184,.15);border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,.5)}
.month-dow{background:rgba(248,250,252,.9);padding:.5rem;text-align:center;font-size:.7rem;font-weight:700;text-transform:uppercase;letter-spacing:.06em;color:#94a3b8}
.month-cell{background:rgba(255,255,255,.6);min-height:100px;padding:.5rem;display:flex;flex-direction:column;gap:.25rem;transition:background .15s}
.month-cell:hover{background:rgba(255,255,255,.85)}
.cell-today{background:rgba(37,99,235,.04)!important}
.month-day-num{font-size:.8rem;font-weight:700;color:#475569}
.cell-today .month-day-num{color:#2563eb}
.month-ev{font-size:.65rem;font-weight:600;padding:.15rem .4rem;border-radius:6px;cursor:pointer;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.ev-dot-blue{background:rgba(37,99,235,.1);color:#1d4ed8}
.ev-dot-teal{background:rgba(15,118,110,.1);color:#0f766e}
.ev-dot-violet{background:rgba(124,58,237,.1);color:#6d28d9}
.ev-dot-amber{background:rgba(217,119,6,.1);color:#b45309}
.ev-dot-red{background:rgba(220,38,38,.08);color:#b91c1c}

.modal-overlay{position:fixed;inset:0;z-index:9998;background:rgba(15,23,42,.4);backdrop-filter:blur(6px);display:flex;align-items:center;justify-content:center;padding:1rem}
.modal-content{position:relative;z-index:9999;background:rgba(255,255,255,.95);backdrop-filter:saturate(180%) blur(24px);width:100%;max-height:90vh;border-radius:24px;box-shadow:0 24px 80px rgba(2,6,23,.32);overflow:hidden;border:1px solid rgba(255,255,255,.5);display:flex;flex-direction:column}
.modal-content.lg{max-width:560px}
.modal-header{padding:1.25rem 1.5rem;border-bottom:1px solid rgba(148,163,184,.15);display:flex;justify-content:space-between;align-items:flex-start;flex-shrink:0}
.modal-title{font-size:1.1rem;font-weight:800;color:#0f172a;margin:.25rem 0 .125rem;line-height:1.3}
.modal-code{font-size:.78rem;color:#64748b;margin:0}
.close-btn{width:36px;height:36px;border-radius:10px;border:1px solid rgba(148,163,184,.3);background:rgba(248,250,252,.8);color:#475569;cursor:pointer;display:flex;align-items:center;justify-content:center;transition:all .15s;flex-shrink:0}
.close-btn:hover{border-color:#dc2626;color:#dc2626}
.modal-body{flex:1;padding:1.5rem;display:flex;flex-direction:column;gap:1rem;overflow-y:auto}
.info-row{display:flex;align-items:center;gap:.75rem;font-size:.875rem;color:#374151}
.info-icon{color:#94a3b8;flex-shrink:0}
.text-violet{color:#7c3aed}
.meet-link{display:inline-flex;align-items:center;gap:.375rem;color:#7c3aed;font-weight:600;text-decoration:none;transition:color .15s}
.meet-link:hover{color:#5b21b6}
.cancel-notice,.makeup-notice,.online-notice{display:flex;align-items:flex-start;gap:.625rem;padding:.875rem 1rem;border-radius:12px;font-size:.8125rem;font-weight:500}
.cancel-notice{background:rgba(220,38,38,.06);color:#b91c1c;border:1px solid rgba(220,38,38,.15)}
.makeup-notice{background:rgba(217,119,6,.06);color:#b45309;border:1px solid rgba(217,119,6,.15)}
.online-notice{background:rgba(124,58,237,.06);color:#6d28d9;border:1px solid rgba(124,58,237,.15)}
.modal-footer{padding:1.25rem 1.5rem;border-top:1px solid rgba(148,163,184,.15);display:flex;gap:.75rem;justify-content:flex-end;flex-shrink:0}
.btn-secondary,.btn-primary{display:inline-flex;align-items:center;gap:.375rem;padding:.5rem 1.25rem;border-radius:10px;font-size:.8125rem;font-weight:700;cursor:pointer;border:none;transition:all .15s;text-decoration:none}
.btn-secondary{background:rgba(248,250,252,.9);color:#374151;border:1px solid rgba(148,163,184,.3)}
.btn-secondary:hover{border-color:#2563eb;color:#2563eb}
.btn-primary{background:#7c3aed;color:#fff;box-shadow:0 4px 14px rgba(124,58,237,.3)}
.btn-primary:hover{background:#6d28d9;transform:translateY(-1px)}

.modal-enter-active,.modal-leave-active{transition:all .3s cubic-bezier(0.16,1,.3,1)}
.modal-enter-from,.modal-leave-to{opacity:0;transform:scale(0.95)}

@media(max-width:900px){.metrics-row{grid-template-columns:repeat(2,1fr)}.week-grid{grid-template-columns:repeat(3,1fr)}}
@media(max-width:640px){.schedule-page{padding:1rem}.metrics-row{grid-template-columns:repeat(2,1fr);gap:.75rem}.week-grid{grid-template-columns:repeat(2,1fr)}.modal-content{margin:0 .5rem}}

/* ── Dark Mode ─────────────────────────────────────── */
:global(.dark) .page-title{color:#f1f5f9}
:global(.dark) .page-sub{color:#94a3b8}
:global(.dark) .eyebrow{color:#93c5fd}

:global(.dark) .filter-select{
  background:rgba(15,23,42,.6);
  border-color:rgba(255,255,255,.12);
  color:#e2e8f0;
  backdrop-filter:blur(12px);
}

:global(.dark) .metric-card{
  background:rgba(15,23,42,.55);
  border-color:rgba(255,255,255,.1);
  box-shadow:0 4px 20px rgba(2,6,23,.4),inset 0 1px 0 rgba(255,255,255,.06);
}
:global(.dark) .metric-val{color:#f1f5f9}
:global(.dark) .metric-lbl{color:#94a3b8}
:global(.dark) .metric-blue .metric-val{color:#93c5fd}
:global(.dark) .metric-violet .metric-val{color:#c4b5fd}
:global(.dark) .metric-red .metric-val{color:#fca5a5}
:global(.dark) .metric-amber .metric-val{color:#fcd34d}

:global(.dark) .cal-controls{
  background:rgba(15,23,42,.55);
  border-color:rgba(255,255,255,.1);
  box-shadow:0 4px 20px rgba(2,6,23,.4);
}
:global(.dark) .view-toggle{background:rgba(2,6,23,.4)}
:global(.dark) .toggle-btn{color:#94a3b8}
:global(.dark) .toggle-btn.active{background:rgba(30,41,59,.9);color:#f1f5f9;box-shadow:0 1px 4px rgba(2,6,23,.4)}
:global(.dark) .nav-btn{
  background:rgba(15,23,42,.6);
  border-color:rgba(255,255,255,.12);
  color:#94a3b8;
}
:global(.dark) .nav-btn:hover{border-color:#93c5fd;color:#93c5fd}
:global(.dark) .period-label{color:#f1f5f9}
:global(.dark) .today-btn{background:#2563eb;box-shadow:0 4px 14px rgba(37,99,235,.4)}

:global(.dark) .day-col{
  background:rgba(15,23,42,.5);
  border-color:rgba(255,255,255,.1);
}
:global(.dark) .day-header{
  background:rgba(2,6,23,.3);
  border-bottom-color:rgba(255,255,255,.08);
}
:global(.dark) .day-header.day-today{background:rgba(37,99,235,.18)}
:global(.dark) .day-name{color:#64748b}
:global(.dark) .day-num{color:#f1f5f9}
:global(.dark) .day-today .day-num{color:#93c5fd}
:global(.dark) .no-event{color:#334155}

:global(.dark) .ev-blue{background:rgba(37,99,235,.18);border-color:rgba(37,99,235,.35)}
:global(.dark) .ev-teal{background:rgba(15,118,110,.18);border-color:rgba(15,118,110,.35)}
:global(.dark) .ev-violet{background:rgba(124,58,237,.18);border-color:rgba(124,58,237,.35)}
:global(.dark) .ev-amber{background:rgba(217,119,6,.18);border-color:rgba(217,119,6,.35)}
:global(.dark) .ev-red{background:rgba(220,38,38,.15);border-color:rgba(220,38,38,.3)}
:global(.dark) .ev-time{color:#64748b}
:global(.dark) .ev-name{color:#e2e8f0}
:global(.dark) .ev-meta{color:#94a3b8}

:global(.dark) .badge-published{background:rgba(22,163,74,.18);color:#86efac}
:global(.dark) .badge-cancelled{background:rgba(220,38,38,.18);color:#fca5a5}
:global(.dark) .badge-makeup{background:rgba(217,119,6,.18);color:#fcd34d}
:global(.dark) .badge-online{background:rgba(124,58,237,.18);color:#c4b5fd}

:global(.dark) .month-grid{background:rgba(2,6,23,.3);border-color:rgba(255,255,255,.08)}
:global(.dark) .month-dow{background:rgba(15,23,42,.6);color:#64748b}
:global(.dark) .month-cell{background:rgba(15,23,42,.45)}
:global(.dark) .month-cell:hover{background:rgba(30,41,59,.7)}
:global(.dark) .cell-today{background:rgba(37,99,235,.15)!important}
:global(.dark) .month-day-num{color:#94a3b8}
:global(.dark) .cell-today .month-day-num{color:#93c5fd}
:global(.dark) .ev-dot-blue{background:rgba(37,99,235,.2);color:#93c5fd}
:global(.dark) .ev-dot-teal{background:rgba(15,118,110,.2);color:#5eead4}
:global(.dark) .ev-dot-violet{background:rgba(124,58,237,.2);color:#c4b5fd}
:global(.dark) .ev-dot-amber{background:rgba(217,119,6,.2);color:#fcd34d}
:global(.dark) .ev-dot-red{background:rgba(220,38,38,.15);color:#fca5a5}

:global(.dark) .modal-overlay{background:rgba(2,6,23,.6)}
:global(.dark) .modal-content{
  background:rgba(10,16,32,.92);
  border-color:rgba(255,255,255,.1);
  box-shadow:0 24px 80px rgba(2,6,23,.6);
}
:global(.dark) .modal-header{border-bottom-color:rgba(255,255,255,.08)}
:global(.dark) .modal-title{color:#f1f5f9}
:global(.dark) .modal-code{color:#64748b}
:global(.dark) .close-btn{
  background:rgba(30,41,59,.8);
  border-color:rgba(255,255,255,.12);
  color:#94a3b8;
}
:global(.dark) .info-row{color:#cbd5e1}
:global(.dark) .info-icon{color:#475569}
:global(.dark) .meet-link{color:#c4b5fd}
:global(.dark) .meet-link:hover{color:#a78bfa}
:global(.dark) .cancel-notice{background:rgba(220,38,38,.12);color:#fca5a5;border-color:rgba(220,38,38,.25)}
:global(.dark) .makeup-notice{background:rgba(217,119,6,.12);color:#fcd34d;border-color:rgba(217,119,6,.25)}
:global(.dark) .online-notice{background:rgba(124,58,237,.12);color:#c4b5fd;border-color:rgba(124,58,237,.25)}
:global(.dark) .modal-footer{background:rgba(15,23,42,.5);border-top-color:rgba(255,255,255,.08)}
:global(.dark) .btn-secondary{
  background:rgba(30,41,59,.7);
  border-color:rgba(255,255,255,.12);
  color:#cbd5e1;
}
:global(.dark) .btn-secondary:hover{border-color:#93c5fd;color:#93c5fd}
:global(.dark) .btn-primary{background:#7c3aed;box-shadow:0 4px 14px rgba(124,58,237,.4)}
</style>
