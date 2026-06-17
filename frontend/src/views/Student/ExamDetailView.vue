<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  ArrowLeft, FileCheck, Clock, ListChecks, ShieldAlert, Globe, Maximize,
  MonitorCheck, Wifi, Puzzle, Server, Bot, CheckCircle2, AlertTriangle,
  XCircle, Loader2, PlayCircle, ChevronRight, Info, Lock, BookOpen
} from 'lucide-vue-next'
import { mockExams, mockExamDetail } from '@/data/studentData.mock.js'
import { runPreflightSecurityChecks } from '@/utils/examSecurity'

const router = useRouter()
const route = useRoute()

// State: 'detail' | 'preflight'
const screen = ref('detail')
const checksRunning = ref(false)
const checksComplete = ref(false)
const checks = ref([])
const preflightResult = ref(null)
const blockedReasons = ref([])

const exam = computed(() => {
  const id = route.params.examId
  return mockExams.find(e => e.id === id) || mockExamDetail
})

const examId = computed(() => String(route.params.examId || exam.value.id || mockExamDetail.id))
const riskScore = computed(() => preflightResult.value?.riskScore || 0)
const riskLevel = computed(() => preflightResult.value?.riskLevel || 'safe')
const canEnter = computed(() => Boolean(checksComplete.value && preflightResult.value?.canEnter))

const checkStatuses = ref({})

const iconMap = { Globe, Maximize, MonitorCheck, Wifi, Puzzle, Server, Bot }

async function runPreflight() {
  if (checksRunning.value) return

  checksRunning.value = true
  checksComplete.value = false
  checkStatuses.value = {}
  blockedReasons.value = []
  preflightResult.value = null

  const result = await runPreflightSecurityChecks()
  checks.value = result.checks
  preflightResult.value = result

  const items = result.checks
  for (let i = 0; i < items.length; i++) {
    checkStatuses.value[items[i].id] = 'checking'
    await new Promise(r => setTimeout(r, 260 + Math.random() * 180))
    checkStatuses.value[items[i].id] = items[i].status
  }

  blockedReasons.value = result.blockedReasons || []
  checksRunning.value = false
  checksComplete.value = true
}

function openPreflight() {
  screen.value = 'preflight'
}

watch(screen, (value) => {
  if (value === 'preflight' && !checksRunning.value && !checksComplete.value) {
    runPreflight()
  }
})

function goTake() {
  if (!canEnter.value) return

  sessionStorage.setItem(`exam_preflight_passed_${examId.value}`, JSON.stringify({
    passed: true,
    passedAt: Date.now(),
    riskScore: riskScore.value,
    checks: checks.value,
  }))

  router.push(`/student/exams/${examId.value}/take`)
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
          <button class="btn-primary-lg" @click="openPreflight">
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
            Trình duyệt không thể phát hiện hoặc chặn mọi hình thức gian lận. Các kiểm tra hiện tại là lớp phòng vệ frontend cho MVP/demo.
          </div>
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
            {{ checksComplete ? 'Kiểm tra lại' : 'Đang chờ kiểm tra' }}
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

      <div v-if="checksComplete && riskLevel !== 'danger'" class="risk-banner" :class="riskLevel">
        <CheckCircle2 v-if="riskLevel === 'safe'" :size="18" />
        <AlertTriangle v-else :size="18" />
        <div>
          <strong>{{ riskLevel === 'safe' ? 'Môi trường đạt yêu cầu' : 'Có cảnh báo cần chú ý' }}</strong>
          <span>
            – {{ riskLevel === 'safe' ? 'Bạn có thể vào phòng thi.' : 'Bạn vẫn có thể vào thi nhưng nên khắc phục trước khi bắt đầu.' }}
          </span>
        </div>
      </div>

      <div v-if="checksComplete && blockedReasons.length" class="blocked-guidance glass-card">
        <h3>Lý do cần xử lý</h3>
        <ul>
          <li v-for="reason in blockedReasons" :key="reason">{{ reason }}</li>
        </ul>
        <div class="guidance-grid">
          <span>Tắt extension dịch/AI/viết.</span>
          <span>Không dùng Remote Desktop/TeamViewer/AnyDesk.</span>
          <span>Không dùng máy ảo.</span>
          <span>Mở Chrome/Edge bằng Guest Profile hoặc profile sạch.</span>
          <span>Sau đó nhấn Kiểm tra lại.</span>
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
              {{ check.reason || check.detail }}
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

      <div v-if="!checksRunning && !checksComplete" class="empty-preflight glass-card">
        <ShieldAlert :size="18" />
        <span>Hệ thống sẽ tự động quét môi trường trước khi vào phòng thi.</span>
      </div>

      <!-- Risk Score & Action -->
      <div v-if="checksComplete" class="preflight-result glass-card">
        <div class="risk-score-section">
          <div class="risk-label">Risk Score</div>
          <div class="risk-score" :class="`risk-score--${riskLevel}`">
            {{ riskScore }}<span class="risk-max">/100</span>
          </div>
          <div class="risk-desc" :class="`risk-desc--${riskLevel}`">
            <span v-if="riskLevel==='safe'">An toàn – Có thể vào thi</span>
            <span v-else-if="riskLevel==='warning'">Cần chú ý – Khuyến nghị khắc phục trước khi thi</span>
            <span v-else>Rủi ro cao – Không thể vào thi</span>
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
.exam-detail-page { padding: 2rem; max-width: 1100px; margin: 0 auto; display: flex; flex-direction: column; gap: 1.25rem; color: var(--text-body); }

/* Breadcrumb */
.breadcrumb { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; color: var(--text-muted); }
.back-btn { display: inline-flex; align-items: center; gap: 0.375rem; background: none; border: none; cursor: pointer; color: var(--text-link); font-size: 0.8125rem; font-weight: 600; padding: 0; }
.back-btn:hover { text-decoration: underline; }
.breadcrumb-sep { color: var(--text-placeholder); }
.breadcrumb-current { color: var(--text-heading); font-weight: 500; }

/* Glass base */
.glass-card {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 24px;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: var(--lg-shadow-sm), inset 0 1px 0 var(--glass-highlight);
}

/* ── Detail Layout ── */
.detail-layout { display: grid; grid-template-columns: 1fr 300px; gap: 1.25rem; align-items: start; }
.detail-main { padding: 2rem; display: flex; flex-direction: column; gap: 1.5rem; }

.exam-hero { display: flex; align-items: flex-start; gap: 1rem; }
.exam-hero-icon { width: 52px; height: 52px; border-radius: 14px; background: var(--accent-primary-soft); color: var(--text-link); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.hero-eyebrow { font-size: 0.75rem; font-weight: 700; color: var(--text-link); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.3rem; }
.hero-title { font-size: 1.5rem; font-weight: 800; color: var(--text-heading); margin: 0 0 0.25rem; letter-spacing: -0.02em; }
.hero-subject { font-size: 0.9rem; color: var(--text-muted); margin: 0; }

.stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; }
.stat-item { display: flex; align-items: center; gap: 0.625rem; padding: 0.875rem; background: var(--surface-solid); border-radius: 14px; border: 1px solid var(--border-default); }
.stat-icon { flex-shrink: 0; }
.stat-icon.blue { color: var(--text-link); } .stat-icon.violet { color: var(--accent-violet); } .stat-icon.amber { color: var(--color-warning-text); } .stat-icon.teal { color: var(--lg-secondary); }
.stat-value { font-size: 0.9375rem; font-weight: 700; color: var(--text-heading); }
.stat-label { font-size: 0.72rem; color: var(--text-placeholder); margin-top: 0.1rem; }

.time-card { background: var(--surface-solid); border-radius: 14px; padding: 1rem 1.25rem; border: 1px solid var(--border-default); display: flex; flex-direction: column; gap: 0.5rem; }
.time-row { display: flex; gap: 0.75rem; font-size: 0.875rem; }
.time-label { color: var(--text-muted); font-weight: 600; min-width: 100px; }
.time-value { color: var(--text-heading); font-weight: 500; }

.section-title { display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; font-weight: 700; color: var(--text-heading); margin-bottom: 0.75rem; }
.text-amber { color: var(--color-warning-text); } .text-blue { color: var(--text-link); } .text-green { color: var(--color-success-text); } .text-red { color: var(--color-danger-text); }
.rules-list { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 0.5rem; }
.rule-item { display: flex; align-items: flex-start; gap: 0.625rem; font-size: 0.875rem; color: var(--text-body); line-height: 1.5; }
.rule-num { flex-shrink: 0; width: 20px; height: 20px; border-radius: 50%; background: var(--accent-primary-soft); color: var(--text-link); font-size: 0.7rem; font-weight: 700; display: flex; align-items: center; justify-content: center; margin-top: 1px; }

.warning-banner { display: flex; align-items: center; gap: 0.625rem; padding: 0.875rem 1rem; background: var(--color-warning-bg); border: 1px solid color-mix(in srgb, var(--color-warning-text) 25%, transparent); border-radius: 12px; font-size: 0.875rem; color: var(--color-warning-text); }
.detail-actions { display: flex; justify-content: flex-end; }
.btn-primary-lg { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1.5rem; background: var(--text-link); color: var(--text-inverse); border: none; border-radius: 12px; font-size: 0.9375rem; font-weight: 700; cursor: pointer; box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 30%, transparent); transition: all 0.15s; }
.btn-primary-lg:hover { background: var(--lg-primary-dark); transform: translateY(-1px); box-shadow: 0 6px 20px color-mix(in srgb, var(--text-link) 40%, transparent); }

/* Sidebar */
.detail-sidebar { display: flex; flex-direction: column; gap: 1rem; }
.sidebar-card, .demo-card { padding: 1.25rem; }
.sidebar-title { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; font-weight: 700; color: var(--text-heading); margin-bottom: 0.875rem; }
.security-list { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 0.5rem; }
.security-item { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8125rem; color: var(--text-label); }
.sidebar-note { margin-top: 0.875rem; padding-top: 0.75rem; border-top: 1px solid var(--border-default); font-size: 0.72rem; color: var(--text-placeholder); line-height: 1.5; }
.toggle-row { display: flex; align-items: flex-start; gap: 0.5rem; cursor: pointer; font-size: 0.8125rem; color: var(--text-label); }
.toggle-input { margin-top: 2px; cursor: pointer; accent-color: var(--text-link); }
.toggle-label { line-height: 1.4; }

/* ── Preflight Layout ── */
.preflight-layout { display: flex; flex-direction: column; gap: 1rem; }
.preflight-header { padding: 1.25rem 1.5rem; }
.preflight-title-row { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.pf-icon { width: 44px; height: 44px; border-radius: 12px; background: var(--accent-primary-soft); color: var(--text-link); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.pf-title { font-size: 1.125rem; font-weight: 700; color: var(--text-heading); margin: 0 0 0.2rem; }
.pf-sub { font-size: 0.8125rem; color: var(--text-muted); margin: 0; }
.btn-run { margin-left: auto; display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.5rem 1rem; background: var(--text-link); color: var(--text-inverse); border: none; border-radius: 10px; font-size: 0.8125rem; font-weight: 600; cursor: pointer; box-shadow: 0 3px 10px color-mix(in srgb, var(--text-link) 30%, transparent); transition: all 0.15s; }
.btn-run:hover { background: var(--lg-primary-dark); }
.checking-badge { margin-left: auto; display: inline-flex; align-items: center; gap: 0.4rem; font-size: 0.8125rem; color: var(--text-link); font-weight: 600; }

.risk-banner { display: flex; align-items: center; gap: 0.75rem; padding: 1rem 1.25rem; border-radius: 14px; font-size: 0.875rem; animation: glass-fade-up 0.28s ease both; }
.risk-banner.danger { background: var(--color-danger-bg); border: 1px solid color-mix(in srgb, var(--color-danger-text) 25%, transparent); color: var(--color-danger-text); }
.risk-banner.safe { background: var(--color-success-bg); border: 1px solid color-mix(in srgb, var(--color-success-text) 25%, transparent); color: var(--color-success-text); }
.risk-banner.warning { background: var(--color-warning-bg); border: 1px solid color-mix(in srgb, var(--color-warning-text) 25%, transparent); color: var(--color-warning-text); }

.blocked-guidance { padding: 1rem 1.25rem; display: flex; flex-direction: column; gap: 0.75rem; }
.blocked-guidance h3 { margin: 0; font-size: 0.9rem; font-weight: 850; color: var(--text-heading); }
.blocked-guidance ul { margin: 0; padding-left: 1.1rem; display: flex; flex-direction: column; gap: 0.35rem; color: var(--color-danger-text); font-size: 0.8125rem; font-weight: 650; }
.guidance-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.5rem; }
.guidance-grid span { display: flex; align-items: center; min-height: 2.1rem; padding: 0.45rem 0.65rem; border-radius: 10px; background: var(--surface-solid); border: 1px solid var(--border-default); color: var(--text-label); font-size: 0.75rem; font-weight: 700; }

.checks-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 0.875rem; }
.check-card { padding: 1rem 1.25rem; display: flex; align-items: flex-start; gap: 0.875rem; transition: transform 0.2s; }
.check-card:hover { transform: translateY(-2px); }
.check-icon-wrap { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.icon-pass { background: var(--color-success-bg); color: var(--color-success-text); }
.icon-warning { background: var(--color-warning-bg); color: var(--color-warning-text); }
.icon-fail { background: var(--color-danger-bg); color: var(--color-danger-text); }
.icon-checking, .icon-idle { background: var(--accent-primary-soft); color: var(--text-link); }
.check-body { flex: 1; }
.check-label { font-size: 0.875rem; font-weight: 700; color: var(--text-heading); }
.check-desc { font-size: 0.75rem; color: var(--text-muted); margin-top: 0.125rem; }
.check-detail { font-size: 0.75rem; color: var(--text-label); margin-top: 0.375rem; padding: 0.375rem 0.625rem; background: var(--surface-solid); border-radius: 8px; animation: glass-fade-up 0.2s ease both; }
.check--fail .check-detail { color: var(--color-danger-text); background: var(--color-danger-bg); }
.check--warning .check-detail { color: var(--color-warning-text); background: var(--color-warning-bg); }
.check-status-icon { flex-shrink: 0; }

.empty-preflight { padding: 1rem 1.25rem; display: flex; align-items: center; gap: 0.625rem; color: var(--text-muted); font-size: 0.85rem; font-weight: 650; }

.preflight-result { padding: 1.5rem; display: flex; align-items: center; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
.risk-score-section { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.risk-label { font-size: 0.8125rem; font-weight: 600; color: var(--text-muted); }
.risk-score { font-size: 2rem; font-weight: 900; letter-spacing: -0.03em; }
.risk-score--safe { color: var(--color-success-text); }
.risk-score--warning { color: var(--color-warning-text); }
.risk-score--danger { color: var(--color-danger-text); }
.risk-max { font-size: 1rem; color: var(--text-placeholder); font-weight: 400; }
.risk-desc { font-size: 0.875rem; font-weight: 600; }
.risk-desc--safe { color: var(--color-success-text); }
.risk-desc--warning { color: var(--color-warning-text); }
.risk-desc--danger { color: var(--color-danger-text); }
.btn-enter { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1.5rem; background: var(--text-link); color: var(--text-inverse); border: none; border-radius: 12px; font-size: 0.9375rem; font-weight: 700; cursor: pointer; box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 30%, transparent); transition: all 0.15s; }
.btn-enter:disabled { background: var(--text-placeholder); box-shadow: none; cursor: not-allowed; }
.btn-enter:not(:disabled):hover { background: var(--lg-primary-dark); transform: translateY(-1px); }

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
