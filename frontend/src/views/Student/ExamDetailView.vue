<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  ArrowLeft, FileCheck, Clock, ListChecks, ShieldAlert, Globe, Maximize,
  MonitorCheck, Wifi, Puzzle, Server, Bot, CheckCircle2, AlertTriangle,
  XCircle, Loader2, PlayCircle, ChevronRight, Info, Lock, Users, BookOpen
} from 'lucide-vue-next'
import { mockExams, mockExamDetail, mockPreflightChecks, mockPreflightChecksHighRisk } from '@/data/exam.mock.js'

const router = useRouter()
const route = useRoute()

// State: 'detail' | 'preflight'
const screen = ref('detail')
const useHighRisk = ref(false)
const checksRunning = ref(false)
const checksComplete = ref(false)

const exam = computed(() => {
  const id = route.params.examId
  return mockExams.find(e => e.id === id) || mockExamDetail
})

const checks = computed(() => useHighRisk.value ? mockPreflightChecksHighRisk : mockPreflightChecks)

const riskScore = computed(() => {
  if (!checksComplete.value) return 0
  if (useHighRisk.value) return 82
  return 24
})

const riskLevel = computed(() => {
  if (riskScore.value >= 70) return 'danger'
  if (riskScore.value >= 40) return 'warning'
  return 'safe'
})

const canEnter = computed(() => checksComplete.value && riskScore.value < 70)

const checkStatuses = ref({})

const iconMap = { Globe, Maximize, MonitorCheck, Wifi, Puzzle, Server, Bot }

async function runPreflight() {
  checksRunning.value = true
  checksComplete.value = false
  checkStatuses.value = {}
  const items = checks.value
  for (let i = 0; i < items.length; i++) {
    checkStatuses.value[items[i].id] = 'checking'
    await new Promise(r => setTimeout(r, 400 + Math.random() * 300))
    checkStatuses.value[items[i].id] = items[i].status
  }
  checksRunning.value = false
  checksComplete.value = true
}

function goTake() {
  router.push(`/student/exams/${route.params.examId}/take`)
}

function formatDate(iso) {
  if (!iso) return '—'
  const d = new Date(iso)
  return d.toLocaleDateString('vi-VN', { day:'2-digit', month:'2-digit', year:'numeric' })
    + ' ' + d.toLocaleTimeString('vi-VN', { hour:'2-digit', minute:'2-digit' })
}
</script>

<template>
  <div class="exam-detail-page">
    <!-- Breadcrumb -->
    <div class="breadcrumb">
      <button class="back-btn" @click="screen === 'preflight' ? screen = 'detail' : router.push('/student/exams')">
        <ArrowLeft :size="16" />
        {{ screen === 'preflight' ? 'Quay lại chi tiết' : 'Danh sách bài thi' }}
      </button>
      <span class="breadcrumb-sep"><ChevronRight :size="14" /></span>
      <span class="breadcrumb-current">{{ screen === 'preflight' ? 'Kiểm tra môi trường' : 'Chi tiết bài thi' }}</span>
    </div>

    <!-- ═══ SCREEN: DETAIL ════════════════════════════════════════ -->
    <div v-if="screen === 'detail'" class="detail-layout">
      <!-- Main card -->
      <div class="detail-main glass-card">
        <div class="exam-hero">
          <div class="exam-hero-icon">
            <FileCheck :size="28" />
          </div>
          <div>
            <div class="hero-eyebrow">{{ exam.subjectCode }} · {{ exam.classCode }}</div>
            <h1 class="hero-title">{{ exam.title || mockExamDetail.title }}</h1>
            <p class="hero-subject">{{ exam.subject || mockExamDetail.subject }}</p>
          </div>
        </div>

        <!-- Stats row -->
        <div class="stats-row">
          <div class="stat-item">
            <Clock :size="18" class="stat-icon blue" />
            <div>
              <div class="stat-value">{{ exam.durationMinutes || mockExamDetail.durationMinutes }} phút</div>
              <div class="stat-label">Thời lượng</div>
            </div>
          </div>
          <div class="stat-item">
            <ListChecks :size="18" class="stat-icon violet" />
            <div>
              <div class="stat-value">{{ exam.totalQuestions || mockExamDetail.totalQuestions }} câu</div>
              <div class="stat-label">Câu hỏi</div>
            </div>
          </div>
          <div class="stat-item">
            <Lock :size="18" class="stat-icon amber" />
            <div>
              <div class="stat-value">{{ exam.maxAttempts || mockExamDetail.maxAttempts }} lần</div>
              <div class="stat-label">Số lần làm</div>
            </div>
          </div>
          <div class="stat-item">
            <BookOpen :size="18" class="stat-icon teal" />
            <div>
              <div class="stat-value">{{ exam.examTypeLabel || mockExamDetail.examTypeLabel }}</div>
              <div class="stat-label">Hình thức</div>
            </div>
          </div>
        </div>

        <!-- Time info -->
        <div class="time-card">
          <div class="time-row">
            <span class="time-label">Mở ca thi:</span>
            <span class="time-value">{{ formatDate(exam.openAt || mockExamDetail.openAt) }}</span>
          </div>
          <div class="time-row">
            <span class="time-label">Đóng ca thi:</span>
            <span class="time-value">{{ formatDate(exam.closeAt || mockExamDetail.closeAt) }}</span>
          </div>
          <div class="time-row">
            <span class="time-label">Giảng viên:</span>
            <span class="time-value">{{ exam.teacher || mockExamDetail.teacher }}</span>
          </div>
        </div>

        <!-- Rules -->
        <div class="rules-section">
          <div class="section-title"><ShieldAlert :size="16" class="text-amber" /> Quy định bài thi</div>
          <ul class="rules-list">
            <li v-for="(rule, i) in (exam.rules || mockExamDetail.rules)" :key="i" class="rule-item">
              <span class="rule-num">{{ i+1 }}</span>
              {{ rule }}
            </li>
          </ul>
        </div>

        <!-- Warning -->
        <div class="warning-banner">
          <Info :size="16" />
          <span>Bạn chỉ có thể nộp bài một lần. Hãy chắc chắn trước khi bắt đầu.</span>
        </div>

        <!-- CTA -->
        <div class="detail-actions">
          <button class="btn-primary-lg" @click="screen = 'preflight'">
            <PlayCircle :size="18" />
            Bắt đầu kiểm tra môi trường
          </button>
        </div>
      </div>

      <!-- Security notice sidebar -->
      <div class="detail-sidebar">
        <div class="sidebar-card glass-card">
          <div class="sidebar-title">
            <ShieldAlert :size="16" class="text-blue" />
            Môi trường thi an toàn
          </div>
          <ul class="security-list">
            <li class="security-item"><Maximize :size="14" /> Yêu cầu chế độ toàn màn hình</li>
            <li class="security-item"><MonitorCheck :size="14" /> Cần chia sẻ màn hình</li>
            <li class="security-item"><Globe :size="14" /> Chặn chuyển tab</li>
            <li class="security-item"><Bot :size="14" /> Kiểm tra AI / Dịch thuật</li>
            <li class="security-item"><Puzzle :size="14" /> Quét tiện ích bị cấm</li>
          </ul>
          <div class="sidebar-note">
            Tất cả các biện pháp trên là mô phỏng giao diện. Hệ thống thật sẽ có giám sát đầy đủ.
          </div>
        </div>

        <!-- Demo toggle -->
        <div class="demo-card glass-card">
          <div class="sidebar-title"><Info :size="14" /> Demo: Thay đổi kịch bản</div>
          <label class="toggle-row">
            <input type="checkbox" v-model="useHighRisk" class="toggle-input" />
            <span class="toggle-label">Mô phỏng risk score cao (bị chặn)</span>
          </label>
        </div>
      </div>
    </div>

    <!-- ═══ SCREEN: PREFLIGHT ══════════════════════════════════════ -->
    <div v-else class="preflight-layout">
      <div class="preflight-header glass-card">
        <div class="preflight-title-row">
          <div class="pf-icon"><ShieldAlert :size="22" /></div>
          <div>
            <h2 class="pf-title">Kiểm tra môi trường thi</h2>
            <p class="pf-sub">Hệ thống sẽ kiểm tra các điều kiện cần thiết trước khi vào phòng thi.</p>
          </div>
          <button
            v-if="!checksRunning"
            class="btn-run"
            @click="runPreflight"
          >
            <PlayCircle :size="15" />
            {{ checksComplete ? 'Kiểm tra lại' : 'Bắt đầu kiểm tra' }}
          </button>
          <div v-else class="checking-badge">
            <Loader2 :size="15" class="spin" /> Đang kiểm tra...
          </div>
        </div>
      </div>

      <!-- Risk Banner (nếu nguy hiểm) -->
      <div v-if="checksComplete && riskLevel === 'danger'" class="risk-banner danger">
        <XCircle :size="18" />
        <div>
          <strong>Không thể vào thi</strong>
          <span> – Vui lòng khắc phục các vấn đề bên dưới trước khi tiếp tục.</span>
        </div>
      </div>

      <!-- Checks list -->
      <div class="checks-grid">
        <div
          v-for="check in checks"
          :key="check.id"
          class="check-card glass-card"
          :class="`check--${checkStatuses[check.id] || 'idle'}`"
        >
          <div class="check-icon-wrap" :class="`icon-${checkStatuses[check.id] || 'idle'}`">
            <component :is="iconMap[check.icon] || Globe" :size="18" />
          </div>
          <div class="check-body">
            <div class="check-label">{{ check.label }}</div>
            <div class="check-desc">{{ check.description }}</div>
            <div v-if="checkStatuses[check.id] && checkStatuses[check.id] !== 'checking'" class="check-detail">
              {{ check.detail }}
            </div>
          </div>
          <div class="check-status-icon">
            <Loader2 v-if="checkStatuses[check.id] === 'checking'" :size="18" class="spin text-blue" />
            <CheckCircle2 v-else-if="checkStatuses[check.id] === 'pass'" :size="18" class="text-green" />
            <AlertTriangle v-else-if="checkStatuses[check.id] === 'warning'" :size="18" class="text-amber" />
            <XCircle v-else-if="checkStatuses[check.id] === 'fail'" :size="18" class="text-red" />
          </div>
        </div>
      </div>

      <!-- Risk Score & Action -->
      <div v-if="checksComplete" class="preflight-result glass-card">
        <div class="risk-score-section">
          <div class="risk-label">Risk Score</div>
          <div class="risk-score" :class="`risk-score--${riskLevel}`">
            {{ riskScore }}<span class="risk-max">/100</span>
          </div>
          <div class="risk-desc" :class="`risk-desc--${riskLevel}`">
            <span v-if="riskLevel==='safe'">✅ An toàn – Có thể vào thi</span>
            <span v-else-if="riskLevel==='warning'">⚠️ Cần chú ý – Khuyến nghị khắc phục trước khi thi</span>
            <span v-else>🚫 Rủi ro cao – Không thể vào thi</span>
          </div>
        </div>
        <button
          class="btn-enter"
          :disabled="!canEnter"
          @click="goTake"
        >
          <PlayCircle :size="18" />
          Vào phòng thi
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-detail-page { padding: 2rem; max-width: 1100px; margin: 0 auto; display: flex; flex-direction: column; gap: 1.25rem; }

/* Breadcrumb */
.breadcrumb { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; color: #64748b; }
.back-btn { display: inline-flex; align-items: center; gap: 0.375rem; background: none; border: none; cursor: pointer; color: #2563eb; font-size: 0.8125rem; font-weight: 600; padding: 0; }
.back-btn:hover { text-decoration: underline; }
.breadcrumb-sep { color: #cbd5e1; }
.breadcrumb-current { color: #0f172a; font-weight: 500; }

/* Glass base */
.glass-card {
  background: rgba(255,255,255,.72);
  border: 1px solid rgba(255,255,255,.5);
  border-radius: 24px;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: 0 8px 30px rgba(15,23,42,.08), inset 0 1px 0 rgba(255,255,255,.25);
}

/* ── Detail Layout ── */
.detail-layout { display: grid; grid-template-columns: 1fr 300px; gap: 1.25rem; align-items: start; }
.detail-main { padding: 2rem; display: flex; flex-direction: column; gap: 1.5rem; }

.exam-hero { display: flex; align-items: flex-start; gap: 1rem; }
.exam-hero-icon { width: 52px; height: 52px; border-radius: 14px; background: rgba(37,99,235,.1); color: #2563eb; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.hero-eyebrow { font-size: 0.75rem; font-weight: 700; color: #2563eb; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.3rem; }
.hero-title { font-size: 1.5rem; font-weight: 800; color: #0f172a; margin: 0 0 0.25rem; letter-spacing: -0.02em; }
.hero-subject { font-size: 0.9rem; color: #64748b; margin: 0; }

.stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; }
.stat-item { display: flex; align-items: center; gap: 0.625rem; padding: 0.875rem; background: rgba(248,250,252,.7); border-radius: 14px; border: 1px solid rgba(148,163,184,.15); }
.stat-icon { flex-shrink: 0; }
.stat-icon.blue { color: #2563eb; } .stat-icon.violet { color: #7c3aed; } .stat-icon.amber { color: #d97706; } .stat-icon.teal { color: #0f766e; }
.stat-value { font-size: 0.9375rem; font-weight: 700; color: #0f172a; }
.stat-label { font-size: 0.72rem; color: #94a3b8; margin-top: 0.1rem; }

.time-card { background: rgba(248,250,252,.7); border-radius: 14px; padding: 1rem 1.25rem; border: 1px solid rgba(148,163,184,.15); display: flex; flex-direction: column; gap: 0.5rem; }
.time-row { display: flex; gap: 0.75rem; font-size: 0.875rem; }
.time-label { color: #64748b; font-weight: 600; min-width: 100px; }
.time-value { color: #0f172a; font-weight: 500; }

.section-title { display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; font-weight: 700; color: #0f172a; margin-bottom: 0.75rem; }
.text-amber { color: #d97706; } .text-blue { color: #2563eb; } .text-green { color: #16a34a; } .text-red { color: #dc2626; }
.rules-list { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 0.5rem; }
.rule-item { display: flex; align-items: flex-start; gap: 0.625rem; font-size: 0.875rem; color: #374151; line-height: 1.5; }
.rule-num { flex-shrink: 0; width: 20px; height: 20px; border-radius: 50%; background: rgba(37,99,235,.1); color: #2563eb; font-size: 0.7rem; font-weight: 700; display: flex; align-items: center; justify-content: center; margin-top: 1px; }

.warning-banner { display: flex; align-items: center; gap: 0.625rem; padding: 0.875rem 1rem; background: rgba(245,158,11,.08); border: 1px solid rgba(245,158,11,.25); border-radius: 12px; font-size: 0.875rem; color: #92400e; }
.detail-actions { display: flex; justify-content: flex-end; }
.btn-primary-lg { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1.5rem; background: #2563eb; color: #fff; border: none; border-radius: 12px; font-size: 0.9375rem; font-weight: 700; cursor: pointer; box-shadow: 0 4px 14px rgba(37,99,235,.3); transition: all 0.15s; }
.btn-primary-lg:hover { background: #1d4ed8; transform: translateY(-1px); box-shadow: 0 6px 20px rgba(37,99,235,.4); }

/* Sidebar */
.detail-sidebar { display: flex; flex-direction: column; gap: 1rem; }
.sidebar-card, .demo-card { padding: 1.25rem; }
.sidebar-title { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; font-weight: 700; color: #0f172a; margin-bottom: 0.875rem; }
.security-list { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 0.5rem; }
.security-item { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; color: #475569; }
.sidebar-note { margin-top: 0.875rem; padding-top: 0.75rem; border-top: 1px solid rgba(148,163,184,.15); font-size: 0.72rem; color: #94a3b8; line-height: 1.5; }
.toggle-row { display: flex; align-items: flex-start; gap: 0.5rem; cursor: pointer; font-size: 0.8125rem; color: #475569; }
.toggle-input { margin-top: 2px; cursor: pointer; accent-color: #2563eb; }
.toggle-label { line-height: 1.4; }

/* ── Preflight Layout ── */
.preflight-layout { display: flex; flex-direction: column; gap: 1rem; }
.preflight-header { padding: 1.25rem 1.5rem; }
.preflight-title-row { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.pf-icon { width: 44px; height: 44px; border-radius: 12px; background: rgba(37,99,235,.1); color: #2563eb; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.pf-title { font-size: 1.125rem; font-weight: 700; color: #0f172a; margin: 0 0 0.2rem; }
.pf-sub { font-size: 0.8125rem; color: #64748b; margin: 0; }
.btn-run { margin-left: auto; display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.5rem 1rem; background: #2563eb; color: #fff; border: none; border-radius: 10px; font-size: 0.8125rem; font-weight: 600; cursor: pointer; box-shadow: 0 3px 10px rgba(37,99,235,.3); transition: all 0.15s; }
.btn-run:hover { background: #1d4ed8; }
.checking-badge { margin-left: auto; display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.8125rem; color: #2563eb; font-weight: 600; }

.risk-banner { display: flex; align-items: center; gap: 0.75rem; padding: 1rem 1.25rem; border-radius: 14px; font-size: 0.875rem; animation: glass-fade-up 0.28s ease both; }
.risk-banner.danger { background: rgba(220,38,38,.08); border: 1px solid rgba(220,38,38,.25); color: #991b1b; }

.checks-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 0.875rem; }
.check-card { padding: 1rem 1.25rem; display: flex; align-items: flex-start; gap: 0.875rem; transition: transform 0.2s; }
.check-card:hover { transform: translateY(-2px); }
.check-icon-wrap { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.icon-pass { background: rgba(22,163,74,.1); color: #16a34a; }
.icon-warning { background: rgba(217,119,6,.1); color: #d97706; }
.icon-fail { background: rgba(220,38,38,.1); color: #dc2626; }
.icon-checking, .icon-idle { background: rgba(37,99,235,.08); color: #2563eb; }
.check-body { flex: 1; }
.check-label { font-size: 0.875rem; font-weight: 700; color: #0f172a; }
.check-desc { font-size: 0.75rem; color: #64748b; margin-top: 0.125rem; }
.check-detail { font-size: 0.75rem; color: #475569; margin-top: 0.375rem; padding: 0.375rem 0.625rem; background: rgba(248,250,252,.8); border-radius: 8px; animation: glass-fade-up 0.2s ease both; }
.check--fail .check-detail { color: #991b1b; background: rgba(254,226,226,.5); }
.check--warning .check-detail { color: #92400e; background: rgba(254,243,199,.5); }
.check-status-icon { flex-shrink: 0; }

.preflight-result { padding: 1.5rem; display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
.risk-score-section { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.risk-label { font-size: 0.8125rem; font-weight: 600; color: #64748b; }
.risk-score { font-size: 2rem; font-weight: 900; letter-spacing: -0.03em; }
.risk-score--safe { color: #16a34a; }
.risk-score--warning { color: #d97706; }
.risk-score--danger { color: #dc2626; }
.risk-max { font-size: 1rem; color: #94a3b8; font-weight: 400; }
.risk-desc { font-size: 0.875rem; font-weight: 600; }
.risk-desc--safe { color: #15803d; }
.risk-desc--warning { color: #b45309; }
.risk-desc--danger { color: #b91c1c; }
.btn-enter { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1.5rem; background: #2563eb; color: #fff; border: none; border-radius: 12px; font-size: 0.9375rem; font-weight: 700; cursor: pointer; box-shadow: 0 4px 14px rgba(37,99,235,.3); transition: all 0.15s; }
.btn-enter:disabled { background: #94a3b8; box-shadow: none; cursor: not-allowed; }
.btn-enter:not(:disabled):hover { background: #1d4ed8; transform: translateY(-1px); }

/* Animation */
@keyframes glass-fade-up { from { opacity:0; transform: translateY(12px) scale(0.985); } to { opacity:1; transform: translateY(0) scale(1); } }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Responsive */
@media (max-width: 900px) {
  .detail-layout { grid-template-columns: 1fr; }
  .stats-row { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 640px) {
  .exam-detail-page { padding: 1rem; }
  .stats-row { grid-template-columns: 1fr 1fr; }
  .checks-grid { grid-template-columns: 1fr; }
  .preflight-result { flex-direction: column; align-items: flex-start; }
}
</style>
