<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Clock, ShieldAlert, AlertTriangle, CheckCircle, Flag,
  ChevronLeft, ChevronRight, Send, HelpCircle, Save,
  Eye, CornerDownRight
} from 'lucide-vue-next'
import { mockExams, mockQuestions } from '@/data/exam.mock.js'

const route = useRoute()
const router = useRouter()

const examId = String(route.params.examId || 'exam-ctdl-002')
const exam = computed(() => {
  return mockExams.find(e => e.id === examId) || {
    id: examId,
    title: 'Bài thi trắc nghiệm',
    subject: 'Môn học mẫu',
    subjectCode: 'MOCK101',
    durationMinutes: 45,
    totalQuestions: 12
  }
})

// Trạng thái làm bài
const currentQuestionIndex = ref(0)
const answers = ref({}) // qId -> choiceId
const flagged = ref({}) // qId -> boolean
const showConfirmSubmit = ref(false)

// Đồng hồ đếm ngược (giả lập phút và giây)
const timeLeftSeconds = ref(exam.value.durationMinutes * 60)
let timerInterval = null

const formattedTimeLeft = computed(() => {
  const mins = Math.floor(timeLeftSeconds.value / 60)
  const secs = timeLeftSeconds.value % 60
  return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`
})

const progressPercent = computed(() => {
  const answeredCount = Object.keys(answers.value).length
  return Math.round((answeredCount / mockQuestions.length) * 100)
})

onMounted(() => {
  // Bắt đầu đếm ngược
  timerInterval = setInterval(() => {
    if (timeLeftSeconds.value > 0) {
      timeLeftSeconds.value--
    } else {
      clearInterval(timerInterval)
      autoSubmit()
    }
  }, 1000)
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
})

// Điều hướng câu hỏi
const currentQuestion = computed(() => mockQuestions[currentQuestionIndex.value])

function nextQuestion() {
  if (currentQuestionIndex.value < mockQuestions.length - 1) {
    currentQuestionIndex.value++
  }
}

function prevQuestion() {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
  }
}

function selectChoice(questionId, choiceId) {
  answers.value[questionId] = choiceId
}

function toggleFlag(questionId) {
  flagged.value[questionId] = !flagged.value[questionId]
}

function autoSubmit() {
  submitExam()
}

function submitExam() {
  showConfirmSubmit.value = false
  // Chuyển hướng sang trang kết quả giả lập
  router.push(`/student/exams/result-${examId}`)
}
</script>

<template>
  <div class="exam-take-page">
    <!-- Header bài thi -->
    <header class="exam-header-strip glass-card">
      <div class="exam-title-meta">
        <div class="badge-exam">PHÒNG THI MÃ SỐ #{{ examId }}</div>
        <h1>{{ exam.title }}</h1>
        <p>{{ exam.subjectCode }} · {{ exam.subject }}</p>
      </div>

      <div class="exam-timer-block" :class="{ 'warning': timeLeftSeconds < 300 }">
        <Clock :size="20" class="timer-icon" />
        <div class="timer-values">
          <span class="timer-label">THỜI GIAN CÒN LẠI</span>
          <span class="timer-countdown">{{ formattedTimeLeft }}</span>
        </div>
      </div>
    </header>

    <!-- Thân giao diện chia 2 cột -->
    <div class="exam-take-layout">
      <!-- Cột chính: Nội dung câu hỏi -->
      <main class="question-main-panel glass-card">
        <header class="question-header">
          <span class="question-index">Câu hỏi {{ currentQuestionIndex + 1 }} / {{ mockQuestions.length }}</span>
          <span class="question-points">[{{ currentQuestion.points }} điểm]</span>
          
          <button 
            type="button" 
            class="flag-btn" 
            :class="{ 'active': flagged[currentQuestion.id] }"
            @click="toggleFlag(currentQuestion.id)"
          >
            <Flag :size="14" />
            {{ flagged[currentQuestion.id] ? 'Đã đánh dấu' : 'Đánh dấu xem sau' }}
          </button>
        </header>

        <!-- Đề bài -->
        <div class="question-content">
          <p class="question-text">{{ currentQuestion.content }}</p>
        </div>

        <!-- Các lựa chọn đáp án -->
        <div class="choices-container">
          <!-- Trắc nghiệm chọn 1 -->
          <div v-if="currentQuestion.type === 'single_choice'" class="choices-grid">
            <button
              v-for="choice in currentQuestion.choices"
              :key="choice.id"
              type="button"
              class="choice-card"
              :class="{ 'selected': answers[currentQuestion.id] === choice.id }"
              @click="selectChoice(currentQuestion.id, choice.id)"
            >
              <span class="choice-prefix">{{ choice.label }}</span>
              <span class="choice-text">{{ choice.text }}</span>
            </button>
          </div>

          <!-- Trắc nghiệm chọn nhiều -->
          <div v-else-if="currentQuestion.type === 'multiple_choice'" class="choices-grid">
            <button
              v-for="choice in currentQuestion.choices"
              :key="choice.id"
              type="button"
              class="choice-card"
              :class="{ 'selected': (answers[currentQuestion.id] || []).includes(choice.id) }"
              @click="() => {
                const currentArr = answers[currentQuestion.id] || [];
                if (currentArr.includes(choice.id)) {
                  answers[currentQuestion.id] = currentArr.filter(id => id !== choice.id);
                } else {
                  answers[currentQuestion.id] = [...currentArr, choice.id];
                }
              }"
            >
              <span class="choice-prefix-square">{{ choice.label }}</span>
              <span class="choice-text">{{ choice.text }}</span>
            </button>
          </div>

          <!-- Trả lời ngắn / Tự luận -->
          <div v-else class="text-answer-container">
            <label class="block text-sm font-semibold text-text-label mb-2">Nhập câu trả lời của bạn:</label>
            <textarea
              v-model="answers[currentQuestion.id]"
              rows="6"
              class="text-answer-input"
              placeholder="Viết câu trả lời tại đây..."
            ></textarea>
          </div>
        </div>

        <!-- Thanh điều hướng chân trang câu hỏi -->
        <footer class="question-actions">
          <button 
            type="button" 
            class="nav-btn" 
            :disabled="currentQuestionIndex === 0"
            @click="prevQuestion"
          >
            <ChevronLeft :size="16" />
            Câu trước
          </button>

          <button 
            type="button" 
            class="nav-btn" 
            :disabled="currentQuestionIndex === mockQuestions.length - 1"
            @click="nextQuestion"
          >
            Câu tiếp theo
            <ChevronRight :size="16" />
          </button>
        </footer>
      </main>

      <!-- Cột phụ: Bảng điều hướng câu hỏi & Nộp bài -->
      <aside class="exam-status-sidebar">
        <!-- Tiến độ làm bài -->
        <div class="sidebar-block glass-card">
          <h3>Tiến độ bài làm</h3>
          <div class="progress-bar-container">
            <div class="progress-info">
              <span>Đã làm: {{ Object.keys(answers).length }} / {{ mockQuestions.length }} câu</span>
              <strong>{{ progressPercent }}%</strong>
            </div>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: `${progressPercent}%` }" />
            </div>
          </div>
        </div>

        <!-- Bảng câu hỏi -->
        <div class="sidebar-block glass-card flex-1">
          <h3>Danh sách câu hỏi</h3>
          <div class="question-grid">
            <button
              v-for="(q, idx) in mockQuestions"
              :key="q.id"
              type="button"
              class="grid-question-btn"
              :class="{
                'active': idx === currentQuestionIndex,
                'answered': answers[q.id] !== undefined && answers[q.id] !== '',
                'flagged': flagged[q.id]
              }"
              @click="currentQuestionIndex = idx"
            >
              {{ idx + 1 }}
              <span v-if="flagged[q.id]" class="flag-dot"></span>
            </button>
          </div>
        </div>

        <!-- Nút nộp bài -->
        <div class="sidebar-submit-block">
          <button 
            type="button" 
            class="btn-submit-exam"
            @click="showConfirmSubmit = true"
          >
            <Send :size="16" />
            Nộp bài thi
          </button>
          <p class="submit-info-text">
            * Không yêu cầu nhập mật khẩu đề thi.
          </p>
        </div>
      </aside>
    </div>

    <!-- Modal xác nhận nộp bài -->
    <div v-if="showConfirmSubmit" class="confirm-modal-backdrop" @click.self="showConfirmSubmit = false">
      <div class="confirm-modal glass-card">
        <div class="modal-icon">
          <HelpCircle :size="28" />
        </div>
        <h2>Xác nhận nộp bài thi?</h2>
        <p>Bạn đã trả lời được {{ Object.keys(answers).length }} / {{ mockQuestions.length }} câu hỏi.</p>
        <p class="text-xs text-text-label mt-2">
          Sau khi nộp bài, bạn sẽ không thể chỉnh sửa đáp án của mình.
        </p>

        <div class="modal-actions">
          <button type="button" class="btn-cancel" @click="showConfirmSubmit = false">Quay lại làm bài</button>
          <button type="button" class="btn-confirm" @click="submitExam">Nộp bài ngay</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-take-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  max-width: 1200px;
  margin: 0 auto;
  padding-bottom: 2rem;
}

.glass-card {
  background: rgba(255, 255, 255, 0.72);
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 20px;
  backdrop-filter: saturate(160%) blur(16px);
  box-shadow: 0 8px 30px rgba(15, 23, 42, 0.08);
}

:global(.dark) .glass-card {
  background: rgba(20, 30, 48, 0.65);
  border: 1px solid rgba(148, 163, 184, 0.16);
  box-shadow: 0 12px 40px rgba(2, 6, 23, 0.4);
}

/* Header Strip */
.exam-header-strip {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  gap: 1rem;
}

.exam-title-meta h1 {
  margin: 0.2rem 0;
  font-size: 1.25rem;
  font-weight: 850;
  color: var(--text-heading);
}

.exam-title-meta p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--text-label);
}

.badge-exam {
  display: inline-block;
  font-size: 0.65rem;
  font-weight: 900;
  background: rgba(37, 99, 235, 0.12);
  color: #2563eb;
  padding: 0.2rem 0.5rem;
  border-radius: 6px;
  text-transform: uppercase;
}

:global(.dark) .badge-exam {
  background: rgba(96, 165, 250, 0.16);
  color: #60a5fa;
}

.exam-timer-block {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 1rem;
  background: rgba(37, 99, 235, 0.08);
  border: 1px solid rgba(37, 99, 235, 0.15);
  border-radius: 14px;
}

.exam-timer-block.warning {
  background: rgba(220, 38, 38, 0.08);
  border-color: rgba(220, 38, 38, 0.2);
  color: #dc2626;
}

.timer-icon {
  color: #2563eb;
}

.exam-timer-block.warning .timer-icon {
  color: #dc2626;
  animation: pulse 1s infinite alternate;
}

@keyframes pulse {
  from { opacity: 0.6; }
  to { opacity: 1; }
}

.timer-values {
  display: flex;
  flex-direction: column;
}

.timer-label {
  font-size: 0.58rem;
  font-weight: 800;
  color: var(--text-placeholder);
}

.timer-countdown {
  font-size: 1.15rem;
  font-weight: 900;
  color: var(--text-heading);
  font-variant-numeric: tabular-nums;
}

/* Layout */
.exam-take-layout {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 1rem;
  align-items: start;
}

.question-main-panel {
  display: flex;
  flex-direction: column;
  min-height: 25rem;
  padding: 1.5rem;
}

.question-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
  margin-bottom: 1.25rem;
}

.question-index {
  font-size: 0.9rem;
  font-weight: 850;
  color: var(--text-heading);
}

.question-points {
  font-size: 0.75rem;
  color: var(--text-link);
  font-weight: 800;
}

.flag-btn {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.3rem 0.6rem;
  border-radius: 8px;
  font-size: 0.72rem;
  font-weight: 800;
  cursor: pointer;
}

.flag-btn.active {
  background: rgba(217, 119, 6, 0.12);
  color: #d97706;
  border-color: rgba(217, 119, 6, 0.25);
}

.question-content {
  margin-bottom: 1.5rem;
}

.question-text {
  font-size: 0.98rem;
  line-height: 1.5;
  color: var(--text-heading);
  white-space: pre-line;
}

/* Choices Grid */
.choices-grid {
  display: grid;
  gap: 0.65rem;
}

.choice-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  text-align: left;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.75rem 1rem;
  border-radius: 12px;
  cursor: pointer;
  color: var(--text-body);
  transition: all 0.15s ease;
}

.choice-card:hover {
  background: var(--surface-card-strong);
  border-color: var(--border-input-focus);
}

.choice-card.selected {
  background: rgba(37, 99, 235, 0.08);
  border-color: #2563eb;
  color: #2563eb;
}

:global(.dark) .choice-card.selected {
  background: rgba(96, 165, 250, 0.12);
  border-color: #60a5fa;
  color: #60a5fa;
}

.choice-prefix {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.65rem;
  height: 1.65rem;
  border-radius: 50%;
  border: 1px solid var(--border-card);
  background: var(--surface-card-strong);
  font-weight: 900;
  font-size: 0.75rem;
  color: var(--text-label);
}

.choice-card.selected .choice-prefix {
  background: #2563eb;
  color: #ffffff;
  border-color: #2563eb;
}

:global(.dark) .choice-card.selected .choice-prefix {
  background: #60a5fa;
  color: #0f172a;
  border-color: #60a5fa;
}

.choice-prefix-square {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.65rem;
  height: 1.65rem;
  border-radius: 6px;
  border: 1px solid var(--border-card);
  background: var(--surface-card-strong);
  font-weight: 900;
  font-size: 0.75rem;
  color: var(--text-label);
}

.choice-card.selected .choice-prefix-square {
  background: #2563eb;
  color: #ffffff;
  border-color: #2563eb;
}

.choice-text {
  font-size: 0.85rem;
  font-weight: 650;
}

/* Text Answer Input */
.text-answer-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 12px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-heading);
  font-family: inherit;
  font-size: 0.875rem;
  outline: none;
}

.text-answer-input:focus {
  border-color: var(--border-input-focus);
}

.question-actions {
  display: flex;
  justify-content: space-between;
  border-top: 1px solid var(--border-card);
  padding-top: 1.25rem;
  margin-top: auto;
}

.nav-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.45rem 1rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.nav-btn:hover:not(:disabled) {
  background: var(--surface-card-strong);
  color: var(--text-heading);
}

.nav-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Sidebar */
.exam-status-sidebar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  height: 100%;
}

.sidebar-block {
  padding: 1.25rem;
}

.sidebar-block h3 {
  margin: 0 0 0.8rem;
  font-size: 0.85rem;
  font-weight: 900;
  color: var(--text-heading);
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.72rem;
  font-weight: 800;
  color: var(--text-label);
  margin-bottom: 0.4rem;
}

.progress-info strong {
  color: var(--text-heading);
}

.progress-track {
  height: 0.5rem;
  background: var(--surface-input);
  border-radius: 99px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #2563eb, #06b6d4);
  border-radius: inherit;
  transition: width 0.3s ease;
}

.question-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.5rem;
}

.grid-question-btn {
  position: relative;
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.grid-question-btn:hover {
  border-color: var(--border-input-focus);
}

.grid-question-btn.active {
  border-color: #2563eb;
  background: rgba(37, 99, 235, 0.08);
  color: #2563eb;
}

.grid-question-btn.answered {
  background: #2563eb;
  color: #ffffff;
  border-color: #2563eb;
}

:global(.dark) .grid-question-btn.answered {
  background: #60a5fa;
  color: #0f172a;
  border-color: #60a5fa;
}

.grid-question-btn.flagged {
  border-color: #d97706;
}

.flag-dot {
  position: absolute;
  top: 3px;
  right: 3px;
  width: 5px;
  height: 5px;
  background: #d97706;
  border-radius: 50%;
}

.sidebar-submit-block {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.btn-submit-exam {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  min-height: 2.75rem;
  border: 0;
  border-radius: 14px;
  background: linear-gradient(135deg, #1d4ed8, #2563eb);
  color: #ffffff;
  font-weight: 850;
  font-size: 0.85rem;
  cursor: pointer;
  box-shadow: 0 8px 24px rgba(37, 99, 235, 0.25);
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.btn-submit-exam:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 28px rgba(37, 99, 235, 0.35);
}

.submit-info-text {
  margin: 0;
  font-size: 0.65rem;
  color: var(--text-placeholder);
  text-align: center;
}

/* Modal */
.confirm-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: grid;
  place-items: center;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(8px);
  padding: 1rem;
}

.confirm-modal {
  width: min(28rem, 100%);
  padding: 1.5rem;
  text-align: center;
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes modal-enter {
  from { opacity: 0; transform: translateY(12px) scale(0.97); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3.25rem;
  height: 3.25rem;
  margin: 0 auto 1rem;
  border-radius: 16px;
  background: rgba(37, 99, 235, 0.1);
  color: #2563eb;
}

.confirm-modal h2 {
  margin: 0 0 0.5rem;
  font-size: 1.15rem;
  font-weight: 900;
  color: var(--text-heading);
}

.confirm-modal p {
  margin: 0.25rem 0;
  font-size: 0.825rem;
  color: var(--text-body);
}

.modal-actions {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 1.5rem;
}

.btn-cancel,
.btn-confirm {
  min-height: 2.35rem;
  padding: 0 1.25rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 850;
  cursor: pointer;
}

.btn-cancel {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.btn-cancel:hover {
  background: var(--surface-card-strong);
}

.btn-confirm {
  border: 0;
  background: #2563eb;
  color: #ffffff;
}

.btn-confirm:hover {
  background: #1d4ed8;
}

@media (max-width: 768px) {
  .exam-take-layout {
    grid-template-columns: 1fr;
  }
}
</style>
