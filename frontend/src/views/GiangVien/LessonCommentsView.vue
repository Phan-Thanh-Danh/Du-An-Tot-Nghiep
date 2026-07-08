<script setup>
import { computed, ref, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  Clock,
  Filter,
  Loader2,
  MessageCircle,
  MoreHorizontal,
  Reply,
  Search,
  Send,
  ThumbsUp,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { teacherApi } from '@/services/teacherApi'

const popupStore = usePopupStore()
const loading = ref(false)
const error = ref('')
const threads = ref([])
const replyingId = ref(null)
const replyTexts = ref({})
const hidingId = ref(null)

const commentStats = computed(() => [
  { label: 'Tổng bình luận', value: threads.value.length, variant: 'neutral' },
  { label: 'Chưa phản hồi', value: threads.value.filter(thread => (thread.replies || []).length === 0).length, variant: 'warning' },
  { label: 'Đã phản hồi', value: threads.value.filter(thread => (thread.replies || []).length > 0).length, variant: 'success' },
  { label: 'Hôm nay', value: 2, variant: 'info' },
])

async function loadComments() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getLessonComments()
    threads.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch (e) {
    error.value = e?.message || 'Không thể tải bình luận.'
    threads.value = []
  } finally {
    loading.value = false
  }
}

async function replyToComment(thread) {
  const text = replyTexts.value[thread.id || thread.maBinhLuan]
  if (!text?.trim()) return
  replyingId.value = thread.id || thread.maBinhLuan
  try {
    await teacherApi.replyLessonComment(thread.id || thread.maBinhLuan, {
      noiDung: text,
      content: text,
    })
    if (!thread.replies) thread.replies = []
    thread.replies.push({
      id: Date.now(),
      user: 'Giảng viên',
      content: text,
      time: 'Vừa xong',
      isTeacher: true,
      laGiangVien: true,
    })
    replyTexts.value[thread.id || thread.maBinhLuan] = ''
    popupStore.success('Đã gửi phản hồi', 'Phản hồi của bạn đã được đăng.')
  } catch (e) {
    popupStore.error('Không thể gửi phản hồi', e?.message || 'Lỗi máy chủ.')
  } finally {
    replyingId.value = null
  }
}

async function hideComment(thread) {
  hidingId.value = thread.id || thread.maBinhLuan
  try {
    await teacherApi.hideLessonComment(thread.id || thread.maBinhLuan, { an: true, hidden: true })
    thread.biAn = true
    thread.hidden = true
    popupStore.success('Đã ẩn bình luận', 'Bình luận đã được ẩn khỏi sinh viên.')
  } catch (e) {
    popupStore.error('Không thể ẩn bình luận', e?.message || 'Lỗi máy chủ.')
  } finally {
    hidingId.value = null
  }
}

function getThreadStatus(thread) {
  return thread.replies && thread.replies.length > 0 ? 'Đã phản hồi' : 'Chưa phản hồi'
}

function getThreadVariant(thread) {
  return thread.replies && thread.replies.length > 0 ? 'success' : 'warning'
}

onMounted(() => { loadComments() })
</script>

<template>
  <div class="lesson-comments-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <MessageCircle :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Lesson discussion</div>
          <h1 class="page-title">Bình luận bài học</h1>
          <p class="page-subtitle">
            Theo dõi thread thảo luận dưới bài giảng, ưu tiên các bình luận chưa được giảng viên phản hồi.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge variant="warning" size="md">
          {{ threads.filter(thread => thread.replies.length === 0).length }} cần phản hồi
        </GlassBadge>
        <GlassButton size="sm" variant="secondary">
          <template #leading>
            <Filter :size="14" />
          </template>
          Lọc theo bài học
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in commentStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>

      <div class="filters">
        <label class="select-field">
          <BookOpen :size="15" />
          <select>
            <option>Tất cả bài học</option>
            <option>Bài 2: Cấu trúc HTML5</option>
            <option>Bài 5: Flexbox Layout</option>
          </select>
        </label>
        <label class="select-field">
          <Filter :size="15" />
          <select>
            <option>Tất cả trạng thái</option>
            <option>Chưa phản hồi</option>
            <option>Đã phản hồi</option>
          </select>
        </label>
        <label class="search-field">
          <Search :size="15" />
          <input type="text" placeholder="Tìm bình luận, sinh viên..." />
        </label>
      </div>
    </GlassPanel>

    <!-- Loading -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-16">
      <Loader2 :size="24" class="animate-spin text-muted mb-3" />
      <p class="text-sm font-medium text-muted">Đang tải bình luận...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-16">
      <AlertCircle :size="40" class="text-rose-400 mb-3" />
      <p class="text-sm font-semibold text-muted">{{ error }}</p>
      <GlassButton variant="primary" size="sm" class="mt-3" @click="loadComments">Thử lại</GlassButton>
    </div>

    <template v-else-if="threads.length">
      <div class="threads-shell">
        <GlassPanel
          v-for="thread in threads"
          :key="thread.id || thread.maBinhLuan"
          v-show="!thread.biAn && !thread.hidden"
          variant="surface"
          density="compact"
          class="thread-card"
          :clip="false"
        >
          <template #header>
            <div class="thread-header">
              <div class="lesson-chip">
                <BookOpen :size="15" />
                <span>{{ thread.tieuDeBaiHoc || thread.lesson }}</span>
              </div>
              <div class="thread-actions">
                <GlassBadge :variant="getThreadVariant(thread)" size="sm">
                  {{ getThreadStatus(thread) }}
                </GlassBadge>
                <button type="button" class="icon-button" :disabled="hidingId === (thread.id || thread.maBinhLuan)" @click="hideComment(thread)" aria-label="Ẩn bình luận">
                  <Loader2 v-if="hidingId === (thread.id || thread.maBinhLuan)" :size="14" class="animate-spin" />
                  <MoreHorizontal v-else :size="18" />
                </button>
              </div>
            </div>
          </template>

          <div class="thread-body">
            <div class="avatar">{{ (thread.hoTen || thread.user || '').split(' ').pop()[0] || '?' }}</div>
            <div class="thread-content">
              <div class="comment-topline">
                <strong>{{ thread.hoTen || thread.user }}</strong>
                <span>
                  <Clock :size="12" />
                  {{ thread.thoiGian || thread.time || '--' }}
                </span>
              </div>
              <p class="comment-text">{{ thread.noiDung || thread.content }}</p>

              <div class="comment-tools">
                <button type="button" @click="replyToComment(thread)" :disabled="replyingId === (thread.id || thread.maBinhLuan)">
                  <Reply :size="15" />
                  Phản hồi
                </button>
                <span>{{ (thread.replies || []).length }} phản hồi</span>
              </div>

              <div v-if="thread.replies && thread.replies.length > 0" class="reply-thread">
                <div v-for="reply in thread.replies" :key="reply.id || reply.maPhanHoi" class="reply-item">
                  <span class="avatar compact">{{ (reply.hoTen || reply.user || '').split(' ').pop()[0] || '?' }}</span>
                  <div class="reply-content">
                    <div class="reply-topline">
                      <strong>{{ reply.hoTen || reply.user }}</strong>
                      <GlassBadge v-if="reply.isTeacher || reply.laGiangVien" variant="primary" size="sm">
                        <CheckCircle2 :size="10" />
                        Giảng viên
                      </GlassBadge>
                      <span class="reply-time">
                        <Clock :size="11" />
                        {{ reply.thoiGian || reply.time || '--' }}
                      </span>
                    </div>
                    <p>{{ reply.noiDung || reply.content }}</p>
                  </div>
                </div>
              </div>

              <div class="quick-reply">
                <span class="teacher-avatar">GV</span>
                <label class="quick-reply-field">
                  <input v-model="replyTexts[thread.id || thread.maBinhLuan]" type="text" placeholder="Viết câu trả lời của bạn..." @keyup.enter="replyToComment(thread)" />
                  <button type="button" :disabled="replyingId === (thread.id || thread.maBinhLuan)" aria-label="Gửi phản hồi nhanh" @click="replyToComment(thread)">
                    <Loader2 v-if="replyingId === (thread.id || thread.maBinhLuan)" :size="14" class="animate-spin" />
                    <Send v-else :size="16" />
                  </button>
                </label>
              </div>
            </div>
          </div>
        </GlassPanel>
      </div>
    </template>

    <EmptyState
      v-else
      title="Chưa có bình luận"
      description="Bình luận dưới bài học sẽ xuất hiện tại đây để giảng viên phản hồi."
    >
      <template #icon>
        <MessageCircle :size="22" />
      </template>
    </EmptyState>
  </div>
</template>

<style scoped>
.lesson-comments-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar,
.header-main,
.header-actions,
.filters,
.thread-header,
.thread-actions,
.lesson-chip,
.stat-value-line,
.thread-body,
.comment-topline,
.comment-topline span,
.comment-tools,
.reply-item,
.reply-topline,
.reply-time,
.quick-reply,
.search-field,
.select-field {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.thread-header {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon {
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

.eyebrow {
  color: var(--text-muted);
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
  max-width: 43rem;
  color: var(--text-muted);
  font-size: 0.875rem;
  line-height: 1.5;
}

.header-actions,
.filters {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
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
  color: var(--text-muted);
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

.search-field,
.select-field {
  gap: 0.5rem;
  min-width: min(18rem, 100%);
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.75rem;
}

.select-field {
  min-width: 11.5rem;
}

.search-field input,
.select-field select,
.quick-reply-field input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.select-field select {
  appearance: none;
  cursor: pointer;
}

.search-field input::placeholder,
.quick-reply-field input::placeholder {
  color: var(--text-placeholder);
}

.search-field:focus-within,
.select-field:focus-within,
.quick-reply-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.threads-shell {
  display: grid;
  gap: 0.875rem;
  max-width: 68rem;
  width: 100%;
  margin: 0 auto;
}

.thread-card {
  min-width: 0;
}

.lesson-chip {
  min-width: 0;
  gap: 0.5rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.4375rem 0.625rem;
  font-size: 0.75rem;
  font-weight: 900;
}

.lesson-chip span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.thread-actions {
  flex: 0 0 auto;
  gap: 0.5rem;
}

.icon-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-muted);
  cursor: pointer;
}

.thread-body {
  align-items: flex-start;
  gap: 0.875rem;
}

.avatar,
.teacher-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.8125rem;
  font-weight: 900;
}

.avatar.compact,
.teacher-avatar {
  width: 2rem;
  height: 2rem;
  font-size: 0.75rem;
}

.thread-content {
  min-width: 0;
  flex: 1;
}

.comment-topline {
  justify-content: space-between;
  gap: 0.75rem;
}

.comment-topline strong,
.reply-topline strong {
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 900;
}

.comment-topline span,
.reply-time,
.comment-tools,
.comment-tools button {
  gap: 0.25rem;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 700;
}

.comment-text {
  margin: 0.5rem 0 0;
  color: var(--text-body);
  font-size: 0.9375rem;
  font-weight: 650;
  line-height: 1.65;
}

.comment-tools {
  flex-wrap: wrap;
  gap: 0.875rem;
  margin-top: 0.75rem;
}

.comment-tools button {
  display: inline-flex;
  align-items: center;
  border: 0;
  background: transparent;
  cursor: pointer;
  padding: 0;
}

.comment-tools button:hover,
.icon-button:hover {
  color: var(--text-link);
}

.reply-thread {
  display: grid;
  gap: 0.625rem;
  margin-top: 0.875rem;
  padding-left: 0.75rem;
  border-left: 1px solid var(--border-card);
}

.reply-item {
  align-items: flex-start;
  gap: 0.75rem;
}

.reply-content {
  min-width: 0;
  flex: 1;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.75rem;
}

.reply-topline {
  flex-wrap: wrap;
  gap: 0.5rem;
}

.reply-time {
  margin-left: auto;
}

.reply-content p {
  margin: 0.5rem 0 0;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.6;
}

.quick-reply {
  align-items: flex-start;
  gap: 0.625rem;
  margin-top: 0.875rem;
}

.teacher-avatar {
  background: var(--color-info-bg);
  color: var(--color-info-text);
}

.quick-reply-field {
  display: flex;
  align-items: center;
  flex: 1;
  min-width: 0;
  height: 2.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  padding: 0 0.375rem 0 0.75rem;
}

.quick-reply-field button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  width: 2rem;
  height: 2rem;
  border: 0;
  border-radius: var(--radius-md);
  background: var(--surface-card);
  color: var(--text-link);
  cursor: pointer;
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

  .select-field {
    flex: 1;
    min-width: 12rem;
  }
}

@media (max-width: 640px) {
  .mini-stats {
    grid-template-columns: 1fr;
  }

  .header-actions,
  .select-field,
  .thread-actions,
  .quick-reply-field {
    width: 100%;
  }

  .thread-header,
  .comment-topline {
    align-items: flex-start;
    flex-direction: column;
  }

  .thread-actions {
    justify-content: space-between;
  }

  .thread-body {
    gap: 0.625rem;
  }

  .reply-time {
    margin-left: 0;
  }
}
</style>
