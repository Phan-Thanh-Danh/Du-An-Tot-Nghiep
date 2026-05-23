<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  CalendarClock, CheckCircle2, Award, ShieldCheck,
  FileCheck, Timer, Clock, MapPin, Users, BookOpen,
  PlayCircle, Eye, RotateCcw, ChevronRight, Filter,
  AlertTriangle, Lock
} from 'lucide-vue-next'
import { mockExams } from '@/data/exam.mock.js'

const router = useRouter()
const activeFilter = ref('all')

const filters = [
  { key: 'all', label: 'Tất cả' },
  { key: 'upcoming', label: 'Sắp diễn ra' },
  { key: 'open', label: 'Đang mở' },
  { key: 'completed', label: 'Đã hoàn thành' },
  { key: 'awaiting_result', label: 'Chờ điểm' },
]

const filteredExams = computed(() => {
  if (activeFilter.value === 'all') return mockExams
  return mockExams.filter(e => e.status === activeFilter.value)
})

const metrics = [
  { label: 'Sắp thi', value: mockExams.filter(e=>e.status==='upcoming').length, unit: 'ca', icon: CalendarClock, tone: 'blue', hint: 'Trong 7 ngày tới' },
  { label: 'Đang mở', value: mockExams.filter(e=>e.status==='open').length, unit: 'ca', icon: PlayCircle, tone: 'green', hint: 'Có thể vào thi ngay' },
  { label: 'Đã hoàn thành', value: mockExams.filter(e=>e.status==='completed').length, unit: 'bài', icon: CheckCircle2, tone: 'teal', hint: 'Kỳ hiện tại' },
  { label: 'Chờ điểm', value: mockExams.filter(e=>e.status==='awaiting_result').length, unit: 'bài', icon: Award, tone: 'violet', hint: 'Đang xử lý kết quả' },
]

const statusConfig = {
  upcoming:        { cls: 'badge-blue',   label: 'Sắp diễn ra' },
  open:            { cls: 'badge-green',  label: 'Đang mở' },
  in_progress:     { cls: 'badge-amber',  label: 'Đang làm bài' },
  completed:       { cls: 'badge-teal',   label: 'Đã hoàn thành' },
  awaiting_result: { cls: 'badge-violet', label: 'Chờ công bố điểm' },
  expired:         { cls: 'badge-red',    label: 'Đã quá hạn' },
}

const typeIcon = { multiple_choice: FileCheck, essay: BookOpen, mixed: Timer }

function formatDate(iso) {
  if (!iso) return '—'
  const d = new Date(iso)
  return d.toLocaleDateString('vi-VN', { day:'2-digit', month:'2-digit', year:'numeric' })
    + ' ' + d.toLocaleTimeString('vi-VN', { hour:'2-digit', minute:'2-digit' })
}

function goToExam(exam) {
  if (exam.status === 'completed' && exam.resultId) {
    router.push(`/student/exams/${exam.resultId}`)
  } else {
    router.push(`/student/exams/${exam.id}`)
  }
}
</script>

<template>
  <div class="exams-page">
    <!-- Page Header -->
    <div class="page-header">
      <div class="header-left">
        <div class="page-eyebrow">
          <FileCheck :size="16" />
          <span>Đánh giá</span>
        </div>
        <h1 class="page-title">Thi / Kiểm tra</h1>
        <p class="page-subtitle">Danh sách ca thi, bài kiểm tra và kết quả của bạn trong kỳ hiện tại.</p>
      </div>
      <div class="header-actions">
        <button class="btn-secondary" @click="router.push('/student/grades')">
          <Award :size="16" />
          Xem bảng điểm
        </button>
      </div>
    </div>

    <!-- KPI Metrics -->
    <div class="metrics-grid">
      <div
        v-for="m in metrics" :key="m.label"
        class="metric-card"
        :class="`metric-${m.tone}`"
      >
        <div class="metric-icon">
          <component :is="m.icon" :size="22" />
        </div>
        <div class="metric-body">
          <div class="metric-value">{{ m.value }}<span class="metric-unit">{{ m.unit }}</span></div>
          <div class="metric-label">{{ m.label }}</div>
          <div class="metric-hint">{{ m.hint }}</div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="filter-bar">
      <div class="filter-label">
        <Filter :size="15" />
        <span>Lọc:</span>
      </div>
      <div class="filter-chips">
        <button
          v-for="f in filters" :key="f.key"
          class="filter-chip"
          :class="{ active: activeFilter === f.key }"
          @click="activeFilter = f.key"
        >
          {{ f.label }}
        </button>
      </div>
    </div>

    <!-- Exam Cards -->
    <div class="exam-list">
      <transition-group name="fade-up" tag="div" class="exam-grid">
        <div
          v-for="exam in filteredExams" :key="exam.id"
          class="exam-card"
          :class="`exam-card--${exam.status}`"
        >
          <!-- Card Header -->
          <div class="card-header">
            <div class="card-header-left">
              <div class="exam-type-icon" :class="`icon-${exam.status}`">
                <component :is="typeIcon[exam.examType] || FileCheck" :size="20" />
              </div>
              <div>
                <span class="exam-badge" :class="statusConfig[exam.status]?.cls">
                  {{ statusConfig[exam.status]?.label || exam.statusLabel }}
                </span>
                <span class="exam-type-badge">{{ exam.examTypeLabel }}</span>
              </div>
            </div>
            <div v-if="exam.status === 'open'" class="live-indicator">
              <span class="live-dot"></span>
              Đang mở
            </div>
          </div>

          <!-- Card Body -->
          <div class="card-body">
            <h3 class="exam-title">{{ exam.title }}</h3>
            <p class="exam-subject">{{ exam.subject }}</p>

            <div class="exam-meta-grid">
              <div class="meta-item">
                <BookOpen :size="14" class="meta-icon" />
                <span>{{ exam.subjectCode }} · {{ exam.classCode }}</span>
              </div>
              <div class="meta-item">
                <Clock :size="14" class="meta-icon" />
                <span>{{ exam.durationMinutes }} phút · {{ exam.totalQuestions }} câu</span>
              </div>
              <div class="meta-item">
                <CalendarClock :size="14" class="meta-icon" />
                <span>Mở: {{ formatDate(exam.openAt) }}</span>
              </div>
              <div class="meta-item">
                <Timer :size="14" class="meta-icon" />
                <span>Đóng: {{ formatDate(exam.closeAt) }}</span>
              </div>
              <div v-if="exam.room" class="meta-item">
                <MapPin :size="14" class="meta-icon" />
                <span>{{ exam.room }}</span>
              </div>
              <div class="meta-item">
                <Users :size="14" class="meta-icon" />
                <span>{{ exam.teacher }}</span>
              </div>
            </div>

            <div class="attempt-info">
              <Lock :size="12" />
              <span>{{ exam.attempts }}/{{ exam.maxAttempts }} lần làm bài</span>
            </div>
          </div>

          <!-- Card Footer -->
          <div class="card-footer">
            <template v-if="exam.status === 'upcoming'">
              <button class="btn-outline" @click="goToExam(exam)">
                <Eye :size="15" /> Xem chi tiết
              </button>
            </template>
            <template v-else-if="exam.status === 'open'">
              <button class="btn-outline" @click="goToExam(exam)">
                <Eye :size="15" /> Xem chi tiết
              </button>
              <button class="btn-primary" @click="goToExam(exam)">
                <PlayCircle :size="15" /> Bắt đầu kiểm tra
              </button>
            </template>
            <template v-else-if="exam.status === 'in_progress'">
              <button class="btn-primary" @click="goToExam(exam)">
                <RotateCcw :size="15" /> Tiếp tục làm bài
              </button>
            </template>
            <template v-else-if="exam.status === 'completed'">
              <button class="btn-outline" @click="goToExam(exam)">
                <Eye :size="15" /> Xem chi tiết
              </button>
              <button class="btn-teal" @click="router.push(`/student/exams/${exam.resultId || exam.id}`)">
                <Award :size="15" /> Xem kết quả
              </button>
            </template>
            <template v-else-if="exam.status === 'awaiting_result'">
              <button class="btn-outline" @click="goToExam(exam)">
                <Eye :size="15" /> Xem chi tiết
              </button>
              <span class="waiting-badge">
                <Timer :size="14" /> Đang chờ công bố
              </span>
            </template>
            <template v-else>
              <span class="expired-badge">
                <AlertTriangle :size="14" /> Đã hết hạn
              </span>
            </template>
          </div>
        </div>
      </transition-group>

      <div v-if="filteredExams.length === 0" class="empty-state">
        <FileCheck :size="48" class="empty-icon" />
        <p>Không có bài thi nào trong danh mục này.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ── Layout ─────────────────────────────────────────── */
.exams-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* ── Header ─────────────────────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}
.page-eyebrow {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #2563eb;
  margin-bottom: 0.5rem;
}
.page-title {
  font-size: 1.875rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 0.375rem;
  letter-spacing: -0.02em;
}
.page-subtitle {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
}
.header-actions { display: flex; gap: 0.75rem; align-items: flex-start; padding-top: 0.25rem; }

/* ── Metrics ─────────────────────────────────────────── */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}
.metric-card {
  background: rgba(255,255,255,0.72);
  border: 1px solid rgba(255,255,255,0.5);
  border-radius: 20px;
  padding: 1.25rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: 0 4px 20px rgba(15,23,42,.07), inset 0 1px 0 rgba(255,255,255,.3);
  transition: transform 0.2s, box-shadow 0.2s;
}
.metric-card:hover { transform: translateY(-2px); box-shadow: 0 8px 30px rgba(15,23,42,.10), inset 0 1px 0 rgba(255,255,255,.3); }
.metric-icon { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.metric-blue .metric-icon { background: rgba(37,99,235,.12); color: #2563eb; }
.metric-green .metric-icon { background: rgba(22,163,74,.12); color: #16a34a; }
.metric-teal .metric-icon { background: rgba(15,118,110,.12); color: #0f766e; }
.metric-violet .metric-icon { background: rgba(124,58,237,.12); color: #7c3aed; }
.metric-value { font-size: 1.5rem; font-weight: 800; color: #0f172a; line-height: 1; }
.metric-unit { font-size: 0.8rem; font-weight: 500; color: #64748b; margin-left: 3px; }
.metric-label { font-size: 0.8rem; font-weight: 600; color: #475569; margin-top: 0.2rem; }
.metric-hint { font-size: 0.7rem; color: #94a3b8; margin-top: 0.1rem; }

/* ── Filter ─────────────────────────────────────────── */
.filter-bar { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.filter-label { display: flex; align-items: center; gap: 0.375rem; font-size: 0.8125rem; font-weight: 600; color: #64748b; }
.filter-chips { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.filter-chip {
  padding: 0.375rem 0.875rem;
  border-radius: 9999px;
  font-size: 0.8125rem;
  font-weight: 500;
  border: 1px solid rgba(148,163,184,.3);
  background: rgba(255,255,255,.6);
  color: #475569;
  cursor: pointer;
  transition: all 0.15s;
}
.filter-chip:hover { border-color: #2563eb; color: #2563eb; background: rgba(37,99,235,.06); }
.filter-chip.active { background: #2563eb; color: #fff; border-color: #2563eb; box-shadow: 0 2px 8px rgba(37,99,235,.25); }

/* ── Exam Cards Grid ────────────────────────────────── */
.exam-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 1.25rem; }
.exam-list { width: 100%; }

.exam-card {
  background: rgba(255,255,255,.72);
  border: 1px solid rgba(255,255,255,.5);
  border-radius: 24px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: 0 4px 20px rgba(15,23,42,.07), inset 0 1px 0 rgba(255,255,255,.25);
  transition: transform 0.2s, box-shadow 0.2s;
}
.exam-card:hover { transform: translateY(-3px); box-shadow: 0 12px 36px rgba(15,23,42,.12), inset 0 1px 0 rgba(255,255,255,.25); }
.exam-card--open { border-color: rgba(22,163,74,.25); }
.exam-card--upcoming { border-color: rgba(37,99,235,.2); }

/* Card sections */
.card-header {
  padding: 1rem 1.25rem 0.75rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid rgba(148,163,184,.12);
}
.card-header-left { display: flex; align-items: center; gap: 0.75rem; }
.exam-type-icon { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; }
.icon-open { background: rgba(22,163,74,.12); color: #16a34a; }
.icon-upcoming { background: rgba(37,99,235,.12); color: #2563eb; }
.icon-completed { background: rgba(15,118,110,.12); color: #0f766e; }
.icon-awaiting_result { background: rgba(124,58,237,.12); color: #7c3aed; }
.icon-expired { background: rgba(220,38,38,.1); color: #dc2626; }

/* Badges */
.exam-badge, .exam-type-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.02em;
  margin-right: 0.35rem;
}
.badge-blue    { background: rgba(37,99,235,.1); color: #1d4ed8; }
.badge-green   { background: rgba(22,163,74,.12); color: #15803d; }
.badge-amber   { background: rgba(217,119,6,.12); color: #b45309; }
.badge-teal    { background: rgba(15,118,110,.12); color: #0f766e; }
.badge-violet  { background: rgba(124,58,237,.1); color: #6d28d9; }
.badge-red     { background: rgba(220,38,38,.1); color: #b91c1c; }
.exam-type-badge { background: rgba(148,163,184,.15); color: #475569; }

.live-indicator {
  display: flex; align-items: center; gap: 0.4rem;
  font-size: 0.7rem; font-weight: 700; color: #16a34a;
}
.live-dot {
  width: 7px; height: 7px; border-radius: 50%; background: #16a34a;
  animation: pulse-dot 1.4s ease-in-out infinite;
}
@keyframes pulse-dot { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.5;transform:scale(1.4)} }

/* Card body */
.card-body { padding: 1rem 1.25rem; flex: 1; display: flex; flex-direction: column; gap: 0.875rem; }
.exam-title { font-size: 1rem; font-weight: 700; color: #0f172a; margin: 0; line-height: 1.4; }
.exam-subject { font-size: 0.8125rem; color: #64748b; margin: 0; }

.exam-meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.375rem; }
.meta-item { display: flex; align-items: center; gap: 0.375rem; font-size: 0.78rem; color: #64748b; }
.meta-icon { color: #94a3b8; flex-shrink: 0; }

.attempt-info {
  display: flex; align-items: center; gap: 0.375rem;
  font-size: 0.75rem; color: #94a3b8;
  padding-top: 0.25rem;
  border-top: 1px solid rgba(148,163,184,.12);
}

/* Card footer */
.card-footer {
  padding: 0.875rem 1.25rem;
  border-top: 1px solid rgba(148,163,184,.12);
  display: flex; align-items: center; gap: 0.625rem; flex-wrap: wrap;
}

/* Buttons */
.btn-primary, .btn-outline, .btn-teal, .btn-secondary {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.5rem 1rem; border-radius: 10px;
  font-size: 0.8125rem; font-weight: 600;
  cursor: pointer; border: none;
  transition: all 0.15s; white-space: nowrap;
}
.btn-primary { background: #2563eb; color: #fff; box-shadow: 0 4px 14px rgba(37,99,235,.3); }
.btn-primary:hover { background: #1d4ed8; transform: translateY(-1px); box-shadow: 0 6px 20px rgba(37,99,235,.35); }
.btn-outline { background: rgba(255,255,255,.7); color: #374151; border: 1px solid rgba(148,163,184,.35); }
.btn-outline:hover { border-color: #2563eb; color: #2563eb; }
.btn-teal { background: rgba(15,118,110,.1); color: #0f766e; border: 1px solid rgba(15,118,110,.2); }
.btn-teal:hover { background: rgba(15,118,110,.15); }
.btn-secondary { background: rgba(255,255,255,.7); color: #374151; border: 1px solid rgba(148,163,184,.35); }
.btn-secondary:hover { border-color: #2563eb; color: #2563eb; }

.waiting-badge {
  display: inline-flex; align-items: center; gap: 0.35rem;
  font-size: 0.78rem; font-weight: 600; color: #7c3aed;
  background: rgba(124,58,237,.08); padding: 0.4rem 0.75rem;
  border-radius: 9999px;
}
.expired-badge {
  display: inline-flex; align-items: center; gap: 0.35rem;
  font-size: 0.78rem; font-weight: 600; color: #dc2626;
  background: rgba(220,38,38,.08); padding: 0.4rem 0.75rem;
  border-radius: 9999px;
}

/* Empty state */
.empty-state { text-align: center; padding: 4rem 2rem; color: #94a3b8; }
.empty-icon { margin: 0 auto 1rem; opacity: 0.3; display: block; }

/* Transitions */
.fade-up-enter-active { animation: glass-fade-up 0.28s cubic-bezier(0.16,1,0.3,1) both; }
@keyframes glass-fade-up {
  from { opacity:0; transform: translateY(12px) scale(0.985); }
  to   { opacity:1; transform: translateY(0) scale(1); }
}

/* ── Responsive ─────────────────────────────────────── */
@media (max-width: 900px) {
  .metrics-grid { grid-template-columns: repeat(2, 1fr); }
  .exam-grid { grid-template-columns: 1fr; }
}
@media (max-width: 640px) {
  .exams-page { padding: 1rem; }
  .metrics-grid { grid-template-columns: repeat(2, 1fr); gap: 0.75rem; }
  .page-header { flex-direction: column; }
  .exam-meta-grid { grid-template-columns: 1fr; }
}
</style>
