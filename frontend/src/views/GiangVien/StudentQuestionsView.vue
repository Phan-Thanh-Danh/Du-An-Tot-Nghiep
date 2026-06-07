<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  CheckCircle2,
  Clock,
  Filter,
  HelpCircle,
  MessageCircle,
  MessageSquare,
  Search,
  Send,
  User,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const popupStore = usePopupStore()

const questions = ref([
  { id: 1, student: 'Nguyễn Văn A', question: 'Thầy ơi, bài Assignment 1 có yêu cầu dùng Tailwind không ạ?', status: 'Pending', time: '10 phút trước' },
  { id: 2, student: 'Trần Thị B', question: 'Em không nộp được file .rar lên hệ thống, thầy xem giúp em.', status: 'Answered', time: '1 giờ trước' },
  { id: 3, student: 'Lê Hoàng C', question: 'Lịch thi giữa kỳ đã có chưa ạ?', status: 'Pending', time: '2 giờ trước' },
  { id: 4, student: 'Hoàng Văn D', question: 'Thưa thầy, cho em hỏi thêm về cách hoạt động của Vue Router?', status: 'Pending', time: '3 giờ trước' },
])

const selectedQ = ref(null)
const replyText = ref('')
const activeFilter = ref('all')

const filteredQuestions = computed(() => {
  if (activeFilter.value === 'pending') {
    return questions.value.filter(q => q.status === 'Pending')
  }
  return questions.value
})

const pendingCount = computed(() => questions.value.filter(q => q.status === 'Pending').length)
const answeredCount = computed(() => questions.value.filter(q => q.status === 'Answered').length)

const questionStats = computed(() => [
  { label: 'Tổng câu hỏi', value: questions.value.length, variant: 'neutral' },
  { label: 'Chưa trả lời', value: pendingCount.value, variant: 'warning' },
  { label: 'Đã trả lời', value: answeredCount.value, variant: 'success' },
  { label: 'Hôm nay', value: 2, variant: 'info' },
])

function openReply(q) {
  selectedQ.value = q
  replyText.value = ''
}

function sendReply() {
  if (selectedQ.value) {
    selectedQ.value.status = 'Answered'
    popupStore.success('Đã gửi phản hồi', `Phản hồi đã được gửi đến ${selectedQ.value.student}`)
    selectedQ.value = null
  }
}

function getStatusText(status) {
  return status === 'Pending' ? 'Chưa trả lời' : 'Đã trả lời'
}

function getStatusVariant(status) {
  return status === 'Pending' ? 'warning' : 'success'
}
</script>

<template>
  <div class="student-questions-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <HelpCircle :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Teacher helpdesk</div>
          <h1 class="page-title">Câu hỏi sinh viên</h1>
          <p class="page-subtitle">
            Theo dõi câu hỏi mới, chọn một mục để phản hồi nhanh và cập nhật trạng thái xử lý.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge v-if="pendingCount > 0" variant="warning" size="md">
          {{ pendingCount }} chưa trả lời
        </GlassBadge>
        <GlassButton size="sm" variant="secondary" @click="activeFilter = 'pending'">
          <template #leading>
            <Filter :size="14" />
          </template>
          Lọc câu hỏi mới
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in questionStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>

      <div class="filters">
        <div class="segment">
          <button
            type="button"
            :class="['segment-button', activeFilter === 'all' && 'is-active']"
            @click="activeFilter = 'all'"
          >
            Tất cả
          </button>
          <button
            type="button"
            :class="['segment-button', activeFilter === 'pending' && 'is-active']"
            @click="activeFilter = 'pending'"
          >
            Chưa trả lời
          </button>
        </div>
        <label class="search-field">
          <Search :size="15" />
          <input type="text" placeholder="Tìm sinh viên, nội dung..." />
        </label>
      </div>
    </GlassPanel>

    <div class="workspace-grid">
      <GlassPanel variant="surface" density="none" class="question-list-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Hộp câu hỏi</h2>
              <p>{{ filteredQuestions.length }} mục đang hiển thị</p>
            </div>
            <GlassBadge variant="info" size="sm">SE1601</GlassBadge>
          </div>
        </template>

        <div v-if="filteredQuestions.length" class="question-list">
          <button
            v-for="q in filteredQuestions"
            :key="q.id"
            type="button"
            :class="['question-row', selectedQ?.id === q.id && 'is-selected']"
            @click="openReply(q)"
          >
            <span class="avatar">{{ q.student.split(' ').pop()[0] }}</span>
            <span class="question-content">
              <span class="row-topline">
                <strong>{{ q.student }}</strong>
                <span class="time-chip">
                  <Clock :size="12" />
                  {{ q.time }}
                </span>
              </span>
              <span class="question-text">{{ q.question }}</span>
              <span class="row-meta">
                <GlassBadge :variant="getStatusVariant(q.status)" size="sm">
                  {{ getStatusText(q.status) }}
                </GlassBadge>
                <span class="module-chip">WEB1013</span>
              </span>
            </span>
          </button>
        </div>

        <EmptyState
          v-else
          title="Không có câu hỏi"
          description="Các câu hỏi phù hợp bộ lọc sẽ xuất hiện tại đây."
        >
          <template #icon>
            <MessageCircle :size="22" />
          </template>
        </EmptyState>
      </GlassPanel>

      <GlassPanel v-if="selectedQ" variant="readable" density="compact" class="reply-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Phản hồi sinh viên</h2>
              <p>Soạn câu trả lời ngắn gọn, rõ hướng xử lý</p>
            </div>
            <GlassBadge :variant="getStatusVariant(selectedQ.status)" size="sm">
              {{ getStatusText(selectedQ.status) }}
            </GlassBadge>
          </div>
        </template>

        <div class="selected-question">
          <div class="selected-meta">
            <span class="avatar compact">{{ selectedQ.student.split(' ').pop()[0] }}</span>
            <div>
              <div class="student-name">{{ selectedQ.student }}</div>
              <div class="student-time">
                <Clock :size="12" />
                {{ selectedQ.time }}
              </div>
            </div>
          </div>
          <p>{{ selectedQ.question }}</p>
        </div>

        <div class="reply-history">
          <div class="history-item">
            <User :size="14" />
            <span>Câu hỏi đang chờ giảng viên phản hồi trong luồng hỗ trợ lớp học.</span>
          </div>
          <div v-if="selectedQ.status === 'Answered'" class="history-item success">
            <CheckCircle2 :size="14" />
            <span>Đã có phản hồi trước đó. Có thể gửi cập nhật bổ sung nếu cần.</span>
          </div>
        </div>

        <label class="reply-box">
          <span>Nội dung phản hồi</span>
          <textarea
            v-model="replyText"
            rows="6"
            placeholder="Viết câu trả lời của bạn tại đây..."
          />
        </label>

        <div class="reply-actions">
          <GlassButton variant="ghost" size="sm" @click="selectedQ = null">Huỷ</GlassButton>
          <GlassButton variant="primary" size="sm" @click="sendReply">
            <template #leading>
              <Send :size="14" />
            </template>
            Gửi câu trả lời
          </GlassButton>
        </div>
      </GlassPanel>

      <GlassPanel v-else variant="surface" density="compact" class="empty-detail">
        <div class="empty-detail-inner">
          <span class="empty-detail-icon">
            <MessageSquare :size="24" />
          </span>
          <h2>Chưa chọn câu hỏi</h2>
          <p>Chọn một câu hỏi trong danh sách để xem nội dung và phản hồi.</p>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.student-questions-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.header-main,
.header-actions,
.filters,
.panel-heading,
.selected-meta,
.row-topline,
.row-meta,
.stat-value-line,
.student-time,
.history-item {
  display: flex;
  align-items: center;
}

.header-main {
  gap: 0.875rem;
}

.header-icon,
.empty-detail-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.eyebrow,
.stat-label,
.module-chip,
.student-time,
.history-item,
.panel-heading p,
.page-subtitle {
  color: var(--text-muted);
}

.eyebrow {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  color: var(--text-heading);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  font-weight: 900;
}

.page-subtitle {
  margin: 0.25rem 0 0;
  max-width: 42rem;
  font-size: 0.875rem;
  line-height: 1.5;
}

.header-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.context-bar {
  align-items: stretch;
}

.mini-stats {
  display: grid;
  grid-template-columns: repeat(4, minmax(7rem, 1fr));
  gap: 0.625rem;
  flex: 1;
}

.mini-stat {
  min-width: 0;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
}

.stat-label {
  display: block;
  font-size: 0.6875rem;
  font-weight: 700;
}

.stat-value-line {
  justify-content: space-between;
  gap: 0.5rem;
  margin-top: 0.375rem;
}

.stat-value-line strong {
  color: var(--text-heading);
  font-size: 1.125rem;
}

.filters {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
}

.segment {
  display: inline-flex;
  gap: 0.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.1875rem;
}

.segment-button {
  border: 0;
  border-radius: calc(var(--radius-md) - 0.1875rem);
  background: transparent;
  color: var(--text-muted);
  padding: 0 0.625rem;
  font-size: 0.75rem;
  font-weight: 800;
  cursor: pointer;
}

.segment-button.is-active {
  background: var(--surface-card);
  color: var(--text-link);
  box-shadow: var(--lg-shadow-sm);
}

.search-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: min(18rem, 100%);
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.75rem;
}

.search-field input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.search-field input::placeholder,
.reply-box textarea::placeholder {
  color: var(--text-placeholder);
}

.workspace-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(20rem, 0.65fr);
  gap: 1rem;
  align-items: start;
}

.question-list-panel,
.reply-panel,
.empty-detail {
  min-width: 0;
}

.panel-heading {
  justify-content: space-between;
  gap: 0.75rem;
}

.panel-heading h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
}

.panel-heading p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.question-list {
  display: grid;
  gap: 0.625rem;
  padding: 0.75rem;
}

.question-row {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 0.75rem;
  width: 100%;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.75rem;
  text-align: left;
  cursor: pointer;
  transition: border-color 160ms ease, background 160ms ease, transform 160ms ease;
}

.question-row:hover,
.question-row.is-selected {
  border-color: var(--border-input-focus);
  background: var(--surface-input);
}

.question-row.is-selected {
  transform: translateY(-1px);
}

.avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.8125rem;
  font-weight: 900;
}

.avatar.compact {
  width: 2rem;
  height: 2rem;
}

.question-content {
  min-width: 0;
}

.row-topline {
  justify-content: space-between;
  gap: 0.75rem;
}

.row-topline strong,
.student-name {
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 900;
}

.time-chip,
.student-time {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  flex: 0 0 auto;
  color: var(--text-muted);
  font-size: 0.6875rem;
  font-weight: 700;
}

.question-text {
  display: -webkit-box;
  margin-top: 0.375rem;
  overflow: hidden;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.55;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.row-meta {
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.625rem;
}

.module-chip {
  border-radius: 999px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.1875rem 0.5rem;
  font-size: 0.6875rem;
  font-weight: 800;
}

.reply-panel,
.empty-detail {
  position: sticky;
  top: 1rem;
}

.selected-question {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 0.875rem;
}

.selected-meta {
  gap: 0.625rem;
}

.selected-question p {
  margin: 0.875rem 0 0;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 650;
  line-height: 1.65;
}

.reply-history {
  display: grid;
  gap: 0.5rem;
  margin-top: 0.875rem;
}

.history-item {
  gap: 0.5rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
  font-size: 0.75rem;
  font-weight: 650;
  line-height: 1.45;
}

.history-item.success {
  color: var(--color-success-text);
}

.reply-box {
  display: grid;
  gap: 0.5rem;
  margin-top: 0.875rem;
}

.reply-box span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 800;
}

.reply-box textarea {
  width: 100%;
  min-height: 9rem;
  resize: vertical;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-body);
  padding: 0.75rem;
  outline: 0;
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.6;
}

.reply-box textarea:focus,
.search-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.reply-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.625rem;
  margin-top: 0.875rem;
}

.empty-detail-inner {
  display: grid;
  min-height: 18rem;
  place-items: center;
  align-content: center;
  gap: 0.625rem;
  text-align: center;
}

.empty-detail-inner h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.empty-detail-inner p {
  margin: 0;
  max-width: 16rem;
  color: var(--text-muted);
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1.55;
}

@media (max-width: 1024px) {
  .page-header,
  .context-bar {
    align-items: flex-start;
    flex-direction: column;
  }

  .mini-stats {
    width: 100%;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filters,
  .search-field {
    width: 100%;
  }

  .workspace-grid {
    grid-template-columns: 1fr;
  }

  .reply-panel,
  .empty-detail {
    position: static;
  }
}

@media (max-width: 640px) {
  .mini-stats {
    grid-template-columns: 1fr;
  }

  .header-actions,
  .reply-actions,
  .segment {
    width: 100%;
  }

  .segment-button,
  .reply-actions :deep(.glass-button) {
    flex: 1;
  }

  .row-topline {
    align-items: flex-start;
    flex-direction: column;
    gap: 0.25rem;
  }
}
</style>
