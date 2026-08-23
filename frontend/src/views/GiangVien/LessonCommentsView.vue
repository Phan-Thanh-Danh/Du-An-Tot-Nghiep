<script setup>
import { computed, ref, onMounted, onBeforeUnmount, watch } from 'vue'
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
  RefreshCw,
  Reply,
  Search,
  Send,
  ThumbsUp,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { teacherApi } from '@/services/teacherApi'

const popupStore = usePopupStore()
const loading = ref(false)
const error = ref('')
const threads = ref([])
const subjects = ref([])
const availableLessons = ref([])
const selectedSubject = ref('')
const selectedLesson = ref('')
const statusFilter = ref('')
const searchQuery = ref('')
const replyingId = ref(null)
const replyTexts = ref({})
const hidingId = ref(null)
let pollTimer = null

const statusOptions = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'unreplied', label: 'Chưa phản hồi' },
  { value: 'replied', label: 'Đã phản hồi' }
]

const subjectOptions = computed(() => {
  const opts = [{ value: '', label: 'Tất cả môn học' }]
  subjects.value.forEach(s => {
    opts.push({
      value: String(s.id),
      label: `${s.name} (${s.code})`
    })
  })
  return opts
})

const lessonOptions = computed(() => {
  const opts = [{ value: '', label: 'Tất cả bài học' }]
  const seen = new Set()

  availableLessons.value.forEach(l => {
    if (l.title && !seen.has(l.title)) {
      seen.add(l.title)
      opts.push({ value: l.title, label: l.title })
    }
  })

  threads.value.forEach(t => {
    const title = t.baiHoc || t.lesson || t.tenBaiHoc || t.lessonTitle
    if (title && !seen.has(title)) {
      if (!selectedSubject.value || String(t.subjectId || t.maMonHoc) === String(selectedSubject.value)) {
        seen.add(title)
        opts.push({ value: title, label: title })
      }
    }
  })

  return opts
})

const filteredThreads = computed(() => {
  let list = threads.value

  if (selectedSubject.value) {
    list = list.filter(t => String(t.subjectId || t.maMonHoc || '') === String(selectedSubject.value))
  }

  if (selectedLesson.value) {
    list = list.filter(t => (t.baiHoc || t.lesson || t.tenBaiHoc || t.lessonTitle) === selectedLesson.value)
  }

  if (statusFilter.value === 'unreplied') {
    list = list.filter(t => (t.replies || []).length === 0 && !t.replied)
  } else if (statusFilter.value === 'replied') {
    list = list.filter(t => (t.replies || []).length > 0 || t.replied)
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(t => 
      (t.noiDung || t.content || '').toLowerCase().includes(q) ||
      (t.hoTen || t.author || t.studentName || '').toLowerCase().includes(q) ||
      (t.subjectName || t.monHoc || '').toLowerCase().includes(q) ||
      (t.baiHoc || t.lesson || t.tenBaiHoc || t.lessonTitle || '').toLowerCase().includes(q)
    )
  }

  return list
})

const commentStats = computed(() => [
  { label: 'Tổng thảo luận', value: threads.value.length, variant: 'neutral' },
  { label: 'Chưa phản hồi', value: threads.value.filter(thread => (thread.replies || []).length === 0 && !thread.replied).length, variant: 'warning' },
  { label: 'Đã phản hồi', value: threads.value.filter(thread => (thread.replies || []).length > 0 || thread.replied).length, variant: 'success' },
  { label: 'Hôm nay', value: threads.value.filter(t => {
    const raw = t.ngayTao || t.time || t.createdAt
    if (!raw) return false
    const d = new Date(raw)
    const now = new Date()
    return !isNaN(d) && d.toDateString() === now.toDateString()
  }).length, variant: 'info' },
])

function getThreadStatus(thread) {
  const hasReplies = (thread.replies || []).length > 0 || thread.replied
  return hasReplies ? 'Đã phản hồi' : 'Chưa phản hồi'
}

function getThreadVariant(thread) {
  const hasReplies = (thread.replies || []).length > 0 || thread.replied
  return hasReplies ? 'success' : 'warning'
}

async function loadTeacherSubjects() {
  try {
    const res = await teacherApi.getTeacherSubjects()
    const rawItems = Array.isArray(res) ? res : (res?.items ?? res?.data ?? [])
    const map = new Map()
    rawItems.forEach(item => {
      const sId = item.subjectId || item.SubjectId || item.id || item.Id
      const sCode = item.subjectCode || item.SubjectCode || item.code || ''
      const sName = item.subjectName || item.SubjectName || item.courseName || item.name || 'Môn học'
      if (sId && !map.has(sId)) {
        map.set(sId, { id: sId, code: sCode, name: sName })
      }
    })
    subjects.value = Array.from(map.values())
  } catch (e) {
    console.error('Lỗi khi tải danh sách môn học của giảng viên:', e)
  }
}

watch(selectedSubject, async (newSubjId) => {
  selectedLesson.value = ''
  availableLessons.value = []
  if (newSubjId) {
    try {
      const detail = await teacherApi.getTeacherSubjectDetail(newSubjId)
      const chuongs = detail?.chuongs || detail?.chapters || []
      const lessonsList = []
      chuongs.forEach(ch => {
        (ch.baiHocs || ch.lessons || []).forEach(l => {
          lessonsList.push({
            id: l.maBaiHoc || l.id,
            title: l.tieuDe || l.title || l.name
          })
        })
      })
      availableLessons.value = lessonsList
    } catch (e) {
      console.error('Lỗi khi tải bài học của môn:', e)
    }
  }
  loadComments(true)
})

async function loadComments(showLoading = true) {
  if (showLoading) loading.value = true
  error.value = ''
  try {
    const params = {}
    if (selectedSubject.value) params.subjectId = selectedSubject.value
    if (selectedLesson.value) params.lesson = selectedLesson.value
    const data = await teacherApi.getLessonComments(params)
    threads.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? data?.Data ?? [])
  } catch (e) {
    if (showLoading) {
      error.value = e?.message || 'Không thể tải bình luận.'
      threads.value = []
    }
  } finally {
    if (showLoading) loading.value = false
  }
}

async function replyToComment(thread) {
  const text = replyTexts.value[thread.id || thread.maBinhLuan]
  if (!text?.trim()) return
  replyingId.value = thread.id || thread.maBinhLuan
  try {
    await teacherApi.replyLessonComment(thread.id || thread.maBinhLuan, {
      noiDung: text.trim(),
      content: text.trim(),
    })
    popupStore.success('Đã gửi phản hồi', 'Phản hồi của bạn đã được gửi thành công.')
    replyTexts.value[thread.id || thread.maBinhLuan] = ''
    await loadComments(false)
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể gửi phản hồi.')
  } finally {
    replyingId.value = null
  }
}

async function hideComment(thread) {
  hidingId.value = thread.id || thread.maBinhLuan
  try {
    await teacherApi.hideLessonComment(thread.id || thread.maBinhLuan)
    popupStore.success('Đã ẩn', 'Bình luận đã được ẩn khỏi danh sách.')
    await loadComments(false)
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể ẩn bình luận.')
  } finally {
    hidingId.value = null
  }
}

function handleFocus() {
  loadComments(false)
}

onMounted(async () => { 
  await loadTeacherSubjects()
  await loadComments(true)
  pollTimer = setInterval(() => loadComments(false), 2500)
  window.addEventListener('focus', handleFocus)
})

onBeforeUnmount(() => {
  if (pollTimer) {
    clearInterval(pollTimer)
    pollTimer = null
  }
  window.removeEventListener('focus', handleFocus)
})
</script>

<template>
  <div class="lesson-comments-page w-full space-y-4">
    <!-- Header -->
    <GlassPanel variant="soft" density="compact" class="page-header w-full" :clip="false">
      <div class="header-main flex items-center justify-between w-full">
        <div class="flex items-center gap-3">
          <span class="header-icon">
            <MessageCircle :size="20" />
          </span>
          <div class="min-w-0">
            <div class="eyebrow">Interactive Hub</div>
            <h1 class="page-title">Hỏi đáp & Thảo luận bài học</h1>
            <p class="page-subtitle">
              Hỏi đáp, trao đổi thắc mắc trực tiếp giữa sinh viên và giảng viên theo từng môn và bài giảng.
            </p>
          </div>
        </div>
        <GlassButton
          variant="ghost"
          size="sm"
          class="shrink-0"
          :disabled="loading"
          @click="loadComments(true)"
        >
          <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
          Làm mới
        </GlassButton>
      </div>
    </GlassPanel>

    <!-- Context bar (Stats & Filters) -->
    <GlassPanel variant="surface" density="compact" class="context-bar w-full flex flex-col gap-3" :clip="false">
      <!-- Mini stats row -->
      <div class="mini-stats grid grid-cols-2 sm:grid-cols-4 gap-3 w-full">
        <div v-for="item in commentStats" :key="item.label" class="mini-stat flex items-center justify-between p-3 rounded-xl border border-card surface-input">
          <div>
            <span class="text-[11px] font-bold text-muted uppercase tracking-wider block">{{ item.label }}</span>
            <strong class="text-lg font-black text-heading block mt-0.5">{{ item.value }}</strong>
          </div>
          <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
        </div>
      </div>

      <!-- Filters row -->
      <div class="filters-row flex flex-wrap items-center gap-3 w-full pt-1">
        <!-- Lọc Môn học -->
        <div class="flex-1 min-w-[200px] sm:min-w-[240px]">
          <LmsSelect
            v-model="selectedSubject"
            placeholder="Tất cả môn học"
            :options="subjectOptions"
            searchable
          />
        </div>
        <!-- Lọc Bài học -->
        <div class="flex-1 min-w-[200px] sm:min-w-[240px]">
          <LmsSelect
            v-model="selectedLesson"
            placeholder="Tất cả bài học"
            :options="lessonOptions"
            searchable
          />
        </div>
        <!-- Lọc Trạng thái -->
        <div class="w-44">
          <LmsSelect
            v-model="statusFilter"
            placeholder="Tất cả trạng thái"
            :options="statusOptions"
          />
        </div>
        <!-- Tìm kiếm -->
        <label class="search-field flex-1 min-w-[220px] flex items-center gap-2 px-3 py-2 border border-input rounded-xl surface-input">
          <Search :size="15" class="text-placeholder shrink-0" />
          <input v-model="searchQuery" type="text" placeholder="Tìm kiếm nội dung, sinh viên..." class="w-full bg-transparent outline-none text-xs text-heading placeholder:text-muted" />
        </label>
      </div>
    </GlassPanel>

    <!-- Loading -->
    <div v-if="loading && threads.length === 0" class="flex flex-col items-center justify-center py-16">
      <Loader2 :size="24" class="animate-spin text-muted mb-3" />
      <p class="text-sm font-medium text-muted">Đang tải bình luận...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error && threads.length === 0" class="flex flex-col items-center justify-center py-16">
      <AlertCircle :size="40" class="text-rose-400 mb-3" />
      <p class="text-sm font-semibold text-muted">{{ error }}</p>
      <GlassButton variant="primary" size="sm" class="mt-3" @click="loadComments">Thử lại</GlassButton>
    </div>

    <!-- Threads List (Full-width, Compact, High Efficiency) -->
    <template v-else-if="filteredThreads.length">
      <div class="threads-shell w-full flex flex-col gap-3">
        <GlassPanel
          v-for="thread in filteredThreads"
          :key="thread.id || thread.maBinhLuan"
          v-show="!thread.biAn && !thread.hidden"
          variant="surface"
          density="compact"
          class="thread-card w-full transition-all duration-200 hover:border-blue-300 dark:hover:border-blue-700"
          :clip="false"
        >
          <!-- Thread Header Bar -->
          <div class="thread-header-bar flex items-center justify-between gap-3 pb-2.5 mb-2.5 border-b border-card">
            <div class="flex items-center gap-2 min-w-0 flex-1">
              <span v-if="thread.subjectName || thread.monHoc" class="shrink-0 px-2 py-0.5 rounded-md bg-blue-500/10 text-blue-700 dark:text-blue-300 font-bold text-[11px] border border-blue-200/40 dark:border-blue-800/40">
                {{ thread.subjectName || thread.monHoc }} <span v-if="thread.subjectCode">({{ thread.subjectCode }})</span>
              </span>
              <div class="flex items-center gap-1.5 min-w-0 text-muted text-xs">
                <BookOpen :size="13" class="shrink-0" />
                <span class="text-heading font-semibold text-xs truncate max-w-md">
                  {{ thread.lessonTitle || thread.tieuDeBaiHoc || thread.lesson || thread.baiHoc }}
                </span>
              </div>
            </div>

            <div class="flex items-center gap-2 shrink-0">
              <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md surface-input border border-card text-[11px] font-bold text-muted">
                <ThumbsUp :size="12" class="text-blue-500" />
                {{ thread.likes || 0 }}
              </span>
              <GlassBadge :variant="getThreadVariant(thread)" size="sm">
                {{ getThreadStatus(thread) }}
              </GlassBadge>
              <button 
                type="button" 
                class="icon-button" 
                :disabled="hidingId === (thread.id || thread.maBinhLuan)" 
                title="Ẩn bình luận này"
                @click="hideComment(thread)" 
                aria-label="Ẩn bình luận"
              >
                <Loader2 v-if="hidingId === (thread.id || thread.maBinhLuan)" :size="13" class="animate-spin" />
                <MoreHorizontal v-else :size="15" />
              </button>
            </div>
          </div>

          <!-- Thread Main Content -->
          <div class="thread-main-row flex items-start gap-3 w-full">
            <!-- Student Avatar -->
            <div class="w-8 h-8 rounded-full bg-gradient-to-tr from-blue-600 to-cyan-500 text-white font-bold text-xs flex items-center justify-center shrink-0 shadow-sm">
              {{ (thread.hoTen || thread.author || thread.user || '').split(' ').pop()[0] || 'S' }}
            </div>

            <!-- Content Column -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2 mb-1">
                <div class="flex items-center gap-2">
                  <strong class="text-heading font-bold text-xs sm:text-sm">{{ thread.hoTen || thread.author || thread.user }}</strong>
                </div>
                <span class="text-[11px] text-muted flex items-center gap-1">
                  <Clock :size="11" />
                  {{ thread.thoiGian || thread.time || '--' }}
                </span>
              </div>

              <!-- Question text -->
              <p class="text-xs sm:text-sm text-body leading-relaxed whitespace-pre-line break-words font-medium">
                {{ thread.noiDung || thread.content }}
              </p>

              <!-- Replies list (if any) -->
              <div v-if="thread.replies && thread.replies.length > 0" class="mt-2.5 space-y-2 pt-2 border-t border-card/60">
                <div 
                  v-for="reply in thread.replies" 
                  :key="reply.id || reply.maPhanHoi || reply.maBinhLuan" 
                  class="p-2.5 rounded-xl surface-input border border-card flex items-start gap-2.5"
                >
                  <div class="w-6 h-6 rounded-full bg-teal-600 text-white font-bold text-[10px] flex items-center justify-center shrink-0 mt-0.5 shadow-sm">
                    {{ (reply.hoTen || reply.author || reply.user || '').split(' ').pop()[0] || 'G' }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center justify-between gap-2 mb-0.5">
                      <div class="flex items-center gap-1.5 flex-wrap">
                        <strong class="text-heading font-bold text-xs">{{ reply.hoTen || reply.author || reply.user }}</strong>
                        <span v-if="reply.role === 'giao_vien' || reply.role === 'Teacher' || reply.isTeacher || reply.laGiangVien" class="px-1.5 py-0.2 rounded text-[10px] font-bold bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300">
                          Giảng viên
                        </span>
                      </div>
                      <span class="text-[10px] text-muted flex items-center gap-1">
                        <Clock :size="10" />
                        {{ reply.thoiGian || reply.time || '--' }}
                      </span>
                    </div>
                    <p class="text-xs text-body leading-relaxed whitespace-pre-line break-words">
                      {{ reply.noiDung || reply.content }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Quick Reply input row -->
              <div class="mt-2.5 flex items-center gap-2">
                <div class="w-6 h-6 rounded-full bg-blue-600 text-white font-bold text-[10px] flex items-center justify-center shrink-0">
                  GV
                </div>
                <div class="flex-1 flex items-center gap-1.5 surface-input border border-input rounded-xl px-3 py-1.5 focus-within:border-blue-500 focus-within:ring-2 focus-within:ring-blue-500/20 transition-all">
                  <input 
                    v-model="replyTexts[thread.id || thread.maBinhLuan]" 
                    type="text" 
                    placeholder="Viết câu trả lời của bạn... (Nhấn Enter để gửi)" 
                    class="w-full bg-transparent outline-none text-xs text-heading placeholder:text-muted"
                    @keyup.enter="replyToComment(thread)" 
                  />
                  <button 
                    type="button" 
                    class="p-1 rounded-lg text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30 transition-all disabled:opacity-30 disabled:cursor-not-allowed"
                    :disabled="replyingId === (thread.id || thread.maBinhLuan) || !replyTexts[thread.id || thread.maBinhLuan]?.trim()" 
                    title="Gửi phản hồi (Enter)"
                    aria-label="Gửi phản hồi nhanh" 
                    @click="replyToComment(thread)"
                  >
                    <Loader2 v-if="replyingId === (thread.id || thread.maBinhLuan)" :size="14" class="animate-spin" />
                    <Send v-else :size="14" />
                  </button>
                </div>
              </div>

            </div>
          </div>
        </GlassPanel>
      </div>
    </template>

    <EmptyState
      v-else
      title="Chưa có bình luận"
      description="Bình luận dưới bài học từ các môn học của bạn sẽ xuất hiện tại đây để giảng viên phản hồi."
    >
      <template #icon>
        <MessageCircle :size="22" />
      </template>
    </EmptyState>
  </div>
</template>

<style scoped>
.lesson-comments-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
  color: var(--text-body);
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
  color: var(--text-muted);
  font-size: 0.875rem;
  line-height: 1.5;
}

.icon-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.875rem;
  height: 1.875rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.15s ease;
}

.icon-button:hover {
  color: var(--text-link);
  border-color: var(--border-input-focus);
}
</style>
