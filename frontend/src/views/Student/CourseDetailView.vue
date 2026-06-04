<script setup>
import { ref, computed } from 'vue'
import * as LucideIcons from 'lucide-vue-next'
import LessonVideoPlayer from '@/components/learning/LessonVideoPlayer.vue'
import {
  mockCourse, mockStats, mockLessons,
  mockCurrentLesson, mockQuizQuestions,
  mockComments, mockTimeline, mockAISummary,
} from '@/data/courseDetail.mock.js'
import {
  canStartLearning,
  getLockedReason,
  isLocked,
  LEARNING_ACCESS,
  needsEarlyLearningConfirm,
} from '@/utils/learningAccess.js'

// ── Tab & lesson state ──────────────────────────────────
const activeTab = ref('video')
const selectedLessonId = ref(mockCurrentLesson.id)
const selectedChapterId = ref(mockCurrentLesson.chapterId)
const expandedChapters = ref({ [mockCurrentLesson.chapterId]: true })
const quizAnswers = ref({})
const newComment = ref('')
const likedComments = ref({})
const accessNotice = ref(null)
const pendingEarlyLesson = ref(null)
const lessonProgressDrafts = ref({})

const lessonAccessOverrides = {
  'l1-1': {
    accessStatus: LEARNING_ACCESS.COMPLETED,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 1,
    lessonType: 'video',
    allowSeek: true,
    pauseOnBlur: true,
    minWatchPercentToComplete: 80,
    progressPercent: 100,
    maxWatchedSeconds: 1104,
  },
  'l1-2': {
    accessStatus: LEARNING_ACCESS.COMPLETED,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 1,
    lessonType: 'video',
    allowSeek: true,
    pauseOnBlur: true,
    minWatchPercentToComplete: 80,
    progressPercent: 100,
    maxWatchedSeconds: 1330,
  },
  'l1-3': {
    accessStatus: LEARNING_ACCESS.EARLY_COMPLETED,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 1,
    allowEarlyLearning: true,
    earlyScore: 8.5,
    attemptType: 'early',
  },
  'l2-1': {
    accessStatus: LEARNING_ACCESS.COMPLETED,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    lessonType: 'video',
    allowSeek: true,
    pauseOnBlur: true,
    minWatchPercentToComplete: 80,
    progressPercent: 95,
    maxWatchedSeconds: 1590,
  },
  'l2-2': {
    accessStatus: LEARNING_ACCESS.OFFICIAL,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    lessonType: 'video',
    allowSeek: false,
    pauseOnBlur: true,
    minWatchPercentToComplete: 80,
    progressPercent: 60,
    maxWatchedSeconds: 743,
  },
  'l2-3': {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    lockedReason: 'Cần xem video bài trước tối thiểu 80%.',
    prerequisiteProgress: 60,
    requiredProgress: 80,
  },
  'l2-4': {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    plannedSemesterIndex: 1,
    plannedBlockIndex: 2,
    lockedReason: 'Cần hoàn thành quiz chương 1.',
    prerequisiteProgress: 0,
    requiredProgress: 100,
  },
}

const chapterAccessOverrides = {
  ch1: { accessStatus: LEARNING_ACCESS.COMPLETED, plannedSemesterIndex: 1, plannedBlockIndex: 1 },
  ch2: { accessStatus: LEARNING_ACCESS.OFFICIAL, plannedSemesterIndex: 1, plannedBlockIndex: 2 },
  ch3: {
    accessStatus: LEARNING_ACCESS.LOCKED_PREREQUISITE,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 1,
    lockedReason: 'Hoàn thành 100% chương 2 để mở chương này.',
  },
  ch4: {
    accessStatus: LEARNING_ACCESS.EARLY_AVAILABLE,
    plannedSemesterIndex: 2,
    plannedBlockIndex: 2,
    allowEarlyLearning: true,
  },
}

const learningLessons = computed(() =>
  mockLessons.map((chapter) => ({
    studentCurrentSemesterIndex: 1,
    studentCurrentBlockIndex: 2,
    ...chapter,
    accessStatus: LEARNING_ACCESS.OFFICIAL,
    ...chapterAccessOverrides[chapter.id],
    lessons: chapter.lessons.map((lesson) => ({
      studentCurrentSemesterIndex: 1,
      studentCurrentBlockIndex: 2,
      allowEarlyLearning: false,
      accessStatus: lesson.status === 'completed' ? LEARNING_ACCESS.COMPLETED : LEARNING_ACCESS.OFFICIAL,
      ...lesson,
      ...lessonAccessOverrides[lesson.id],
    })),
  }))
)

const currentLesson = ref({ ...mockCurrentLesson })

function selectLesson(chapter, lesson) {
  accessNotice.value = null

  if (isLocked(lesson)) {
    accessNotice.value = {
      title: 'Bạn chưa đủ điều kiện mở bài này.',
      message: getLockedReason(lesson),
    }
    return
  }

  if (needsEarlyLearningConfirm(lesson)) {
    pendingEarlyLesson.value = { ...lesson, chapter }
    return
  }

  activateLesson(chapter, lesson)
}

function activateLesson(chapter, lesson) {
  if (!canStartLearning(lesson) && lesson.accessStatus !== LEARNING_ACCESS.COMPLETED && lesson.accessStatus !== LEARNING_ACCESS.EARLY_COMPLETED) return
  selectedChapterId.value = chapter.id
  selectedLessonId.value = lesson.id
  currentLesson.value = {
    ...mockCurrentLesson,
    ...lesson,
    ...lessonProgressDrafts.value[lesson.id],
    id: lesson.id,
    chapterId: chapter.id,
    chapterTitle: `${chapter.chapter}: ${chapter.title}`,
    title: lesson.title,
    duration: lesson.duration,
    durationSeconds: parseDurationSeconds(lesson.duration) || lesson.durationSeconds || mockCurrentLesson.durationSeconds,
  }
  activeTab.value = 'video'
}

function parseDurationSeconds(duration) {
  if (!duration || !String(duration).includes(':')) return 0
  const [minutes, seconds] = String(duration).split(':').map(Number)
  return (minutes * 60) + (seconds || 0)
}

function handleVideoProgress(payload) {
  lessonProgressDrafts.value[payload.lessonId] = {
    watchedSeconds: payload.currentTimeSeconds,
    maxWatchedSeconds: payload.maxWatchedSeconds,
    progressPercent: payload.progressPercent,
    completedAt: payload.completed ? new Date().toISOString() : null,
  }
}

function handleVideoCompleted(payload) {
  handleVideoProgress(payload)
}

function confirmEarlyLesson() {
  if (!pendingEarlyLesson.value) return
  const { chapter, ...lesson } = pendingEarlyLesson.value
  pendingEarlyLesson.value = null
  activateLesson(chapter, lesson)
}

function closeEarlyLessonModal() {
  pendingEarlyLesson.value = null
}

function toggleChapter(id) {
  expandedChapters.value[id] = !expandedChapters.value[id]
}

function selectAnswer(qId, idx) { quizAnswers.value[qId] = idx }
function toggleLike(cId) { likedComments.value[cId] = !likedComments.value[cId] }

function resolveIcon(name) { return LucideIcons[name] || LucideIcons.Circle }

const toneIcon = {
  blue: 'bg-blue-50 text-blue-700 ring-blue-100',
  green: 'bg-green-50 text-green-700 ring-green-100',
  orange: 'bg-orange-50 text-orange-700 ring-orange-100',
  violet: 'bg-violet-50 text-violet-700 ring-violet-100',
  amber: 'bg-amber-50 text-amber-700 ring-amber-100',
  slate: 'bg-slate-100 text-slate-500 ring-slate-200',
}
const toneBar = {
  blue: 'from-blue-600 to-cyan-500',
  green: 'from-green-600 to-teal-500',
  orange: 'from-orange-500 to-amber-400',
  violet: 'from-violet-600 to-indigo-500',
  amber: 'from-amber-500 to-orange-400',
  slate: 'from-slate-400 to-slate-300',
}
const toneDot = {
  blue: 'bg-blue-500', green: 'bg-green-500', orange: 'bg-orange-500',
  violet: 'bg-violet-500', amber: 'bg-amber-500', slate: 'bg-slate-400',
}
const statusBadge = {
  completed: 'bg-green-100 text-green-700',
  active: 'bg-blue-100 text-blue-700',
  locked: 'bg-slate-100 text-slate-500',
  upcoming: 'bg-amber-100 text-amber-700',
}

const accessBadge = {
  [LEARNING_ACCESS.OFFICIAL]: 'Đang học',
  [LEARNING_ACCESS.EARLY_AVAILABLE]: 'Có thể học trước',
  [LEARNING_ACCESS.EARLY_COMPLETED]: 'Đã học trước',
  [LEARNING_ACCESS.LOCKED_PREREQUISITE]: 'Bị khóa',
  [LEARNING_ACCESS.FUTURE_LOCKED]: 'Chưa mở',
  [LEARNING_ACCESS.COMPLETED]: 'Đã hoàn thành',
}

function accessTone(status) {
  return {
    [LEARNING_ACCESS.OFFICIAL]: 'access-official',
    [LEARNING_ACCESS.EARLY_AVAILABLE]: 'access-early',
    [LEARNING_ACCESS.EARLY_COMPLETED]: 'access-early-done',
    [LEARNING_ACCESS.LOCKED_PREREQUISITE]: 'access-locked',
    [LEARNING_ACCESS.FUTURE_LOCKED]: 'access-future',
    [LEARNING_ACCESS.COMPLETED]: 'access-completed',
  }[status] || 'access-future'
}

function lessonIcon(lesson) {
  if (lesson.accessStatus === LEARNING_ACCESS.COMPLETED || lesson.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED) return 'CheckCircle2'
  if (isLocked(lesson)) return 'Lock'
  if (needsEarlyLearningConfirm(lesson)) return 'FastForward'
  return 'Play'
}
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-6">

    <!-- ══ HERO ══════════════════════════════════════════════ -->
    <div class="relative overflow-hidden rounded-[24px] bg-gradient-to-br from-blue-700 via-blue-600 to-cyan-500 p-4 text-white shadow-lg">
      <div class="pointer-events-none absolute right-0 top-0 h-64 w-64 rounded-full bg-white/10 blur-3xl" />
      <div class="relative flex flex-wrap items-start justify-between gap-4">
        <div>
          <div class="flex items-center gap-2 text-blue-200 text-xs font-semibold mb-2">
            <component :is="resolveIcon('BookOpenCheck')" :size="14" />
            Khóa học · {{ mockCourse.code }}
          </div>
          <h1 class="text-xl font-bold leading-tight">{{ mockCourse.title }}</h1>
          <p class="mt-1 text-blue-100 text-sm">{{ mockCourse.teacher }} · {{ mockCourse.semester }} · {{ mockCourse.credits }} tín chỉ</p>
        </div>
        <div class="flex gap-2">
          <router-link to="/student/courses" class="inline-flex items-center gap-1.5 rounded-xl bg-white/20 px-3 py-2 text-xs font-semibold hover:bg-white/30 transition-colors">
            <component :is="resolveIcon('ArrowLeft')" :size="13" /> Tất cả khóa học
          </router-link>
          <router-link to="/student/assignments" class="inline-flex items-center gap-1.5 rounded-xl bg-white px-3 py-2 text-xs font-semibold text-blue-700 hover:bg-blue-50 transition-colors">
            <component :is="resolveIcon('ClipboardList')" :size="13" /> Bài tập
          </router-link>
        </div>
      </div>
    </div>

    <!-- ══ STATS ════════════════════════════════════════════ -->
    <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <div v-for="s in mockStats" :key="s.label"
        class="rounded-[20px] border border-white/60 bg-white/80 p-4 shadow-sm backdrop-blur-md">
        <div class="flex items-center justify-between mb-3">
          <span class="text-xs font-semibold text-slate-500">{{ s.label }}</span>
          <div :class="['flex h-9 w-9 items-center justify-center rounded-xl ring-1', toneIcon[s.tone] || toneIcon.blue]">
            <component :is="resolveIcon(s.icon)" :size="16" />
          </div>
        </div>
        <div class="flex items-baseline gap-1 mb-3">
          <span class="text-xl font-bold text-slate-900">{{ s.value }}</span>
          <span class="text-xs text-slate-500">{{ s.unit }}</span>
        </div>
        <div class="h-1.5 overflow-hidden rounded-full bg-slate-200">
          <div :class="['h-full rounded-full bg-gradient-to-r', toneBar[s.tone] || toneBar.blue]" :style="{ width: s.progress + '%' }" />
        </div>
        <p class="mt-2 text-xs text-slate-400">{{ s.hint }}</p>
      </div>
    </div>

    <!-- ══ MAIN LAYOUT ══════════════════════════════════════ -->
    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_340px]">

      <!-- LEFT: Lesson Viewer + Course Outline -->
      <div class="space-y-4">

        <!-- LESSON VIEWER -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <!-- Header -->
          <div class="border-b border-slate-100 bg-white/60 px-5 py-4">
            <div class="flex items-center gap-2 mb-1">
              <span class="rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-semibold text-blue-700">Đang học</span>
              <span class="text-xs text-slate-400">{{ currentLesson.chapterTitle }}</span>
            </div>
            <h2 class="text-base font-bold text-slate-900">{{ currentLesson.title }}</h2>
          </div>

          <!-- Tabs -->
          <div class="flex gap-0 border-b border-slate-100 bg-white/40 px-5">
            <button v-for="tab in ['video','taiLieu','quiz','thaoLuan']" :key="tab"
              @click="activeTab = tab"
              :class="['px-4 py-3 text-sm font-semibold border-b-2 transition-colors', activeTab === tab ? 'border-blue-600 text-blue-700' : 'border-transparent text-slate-500 hover:text-slate-800']">
              {{ { video: '▶ Video', taiLieu: '📄 Tài liệu', quiz: '📝 Quiz', thaoLuan: '💬 Thảo luận' }[tab] }}
            </button>
          </div>

          <!-- Tab: Video -->
          <div v-if="activeTab === 'video'" class="p-5">
            <LessonVideoPlayer
              :key="currentLesson.id"
              :lesson="currentLesson"
              @progress="handleVideoProgress"
              @completed="handleVideoCompleted"
            />
          </div>

          <!-- Tab: Tài liệu -->
          <div v-else-if="activeTab === 'taiLieu'" class="p-5">
            <div class="flex h-44 items-center justify-center rounded-2xl border-2 border-dashed border-slate-200 bg-slate-50 mb-4">
              <div class="text-center">
                <component :is="resolveIcon('FileText')" :size="36" class="mx-auto text-slate-300 mb-2" />
                <p class="text-sm font-semibold text-slate-600">{{ currentLesson.documentTitle }}</p>
                <p class="text-xs text-slate-400 mt-1">Trang {{ currentLesson.documentCurrentPage }} / {{ currentLesson.documentPages }}</p>
              </div>
            </div>
            <button class="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors">
              <component :is="resolveIcon('ExternalLink')" :size="15" /> Mở tài liệu
            </button>
          </div>

          <!-- Tab: Quiz -->
          <div v-else-if="activeTab === 'quiz'" class="p-5 space-y-5">
            <div v-for="q in mockQuizQuestions" :key="q.id" class="rounded-2xl border border-slate-100 bg-slate-50/80 p-4">
              <p class="text-sm font-bold text-slate-900 mb-3">{{ q.question }}</p>
              <div class="space-y-2">
                <button v-for="(opt, idx) in q.options" :key="idx"
                  @click="selectAnswer(q.id, idx)"
                  :class="['flex w-full items-center gap-3 rounded-xl border px-3 py-2.5 text-sm text-left transition-all', quizAnswers[q.id] === idx ? 'border-blue-400 bg-blue-50 text-blue-800 font-semibold' : 'border-slate-200 bg-white text-slate-700 hover:border-blue-200']">
                  <span :class="['flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold shrink-0', quizAnswers[q.id] === idx ? 'bg-blue-600 text-white' : 'bg-slate-200 text-slate-500']">
                    {{ ['A','B','C','D'][idx] }}
                  </span>
                  {{ opt }}
                </button>
              </div>
            </div>
            <button class="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 transition-colors shadow-sm">
              <component :is="resolveIcon('Send')" :size="15" /> Nộp bài
            </button>
          </div>

          <!-- Tab: Thảo luận -->
          <div v-else-if="activeTab === 'thaoLuan'" class="p-5 space-y-4">
            <div class="flex gap-3">
              <div class="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-cyan-500 text-xs font-bold text-white">SV</div>
              <div class="flex-1">
                <textarea v-model="newComment" placeholder="Nhập câu hỏi hoặc thảo luận về bài học..." rows="2"
                  class="w-full resize-none rounded-xl border border-slate-200 bg-white/80 px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-400/20 transition-all" />
                <button class="mt-2 inline-flex items-center gap-1.5 rounded-xl bg-blue-600 px-3 py-2 text-xs font-semibold text-white hover:bg-blue-700 transition-colors">
                  <component :is="resolveIcon('Send')" :size="12" /> Gửi
                </button>
              </div>
            </div>
            <div v-for="c in mockComments" :key="c.id" class="rounded-2xl border border-slate-100 bg-white/70 p-4">
              <div class="flex gap-3">
                <div :class="['flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br text-xs font-bold text-white', c.avatarColor]">{{ c.initials }}</div>
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 mb-1">
                    <span class="text-sm font-bold text-slate-900">{{ c.author }}</span>
                    <span class="text-xs text-slate-400">{{ c.time }}</span>
                  </div>
                  <p class="text-sm text-slate-600">{{ c.content }}</p>
                  <div class="flex items-center gap-3 mt-2">
                    <button @click="toggleLike(c.id)" :class="['flex items-center gap-1 text-xs font-semibold transition-colors', likedComments[c.id] ? 'text-blue-600' : 'text-slate-400 hover:text-blue-500']">
                      <component :is="resolveIcon('ThumbsUp')" :size="12" /> {{ (c.likes + (likedComments[c.id] ? 1 : 0)) }}
                    </button>
                    <button class="text-xs font-semibold text-slate-400 hover:text-slate-600 transition-colors">Trả lời</button>
                  </div>
                  <div v-if="c.replies?.length" class="mt-3 space-y-2 pl-3 border-l-2 border-slate-100">
                    <div v-for="r in c.replies" :key="r.id" class="flex gap-2">
                      <div :class="['flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-gradient-to-br text-[10px] font-bold text-white', r.avatarColor]">{{ r.initials }}</div>
                      <div>
                        <div class="flex items-center gap-2 mb-0.5">
                          <span class="text-xs font-bold text-slate-800">{{ r.author }}</span>
                          <span v-if="r.isTeacher" class="rounded-full bg-violet-100 px-1.5 py-0.5 text-[10px] font-semibold text-violet-700">Giảng viên</span>
                          <span class="text-[10px] text-slate-400">{{ r.time }}</span>
                        </div>
                        <p class="text-xs text-slate-600">{{ r.content }}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COURSE OUTLINE -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 bg-white/60 px-5 py-4">
            <h3 class="text-base font-bold text-slate-900">Cấu trúc chương học</h3>
          </div>
          <div class="divide-y divide-slate-100">
            <div v-for="ch in learningLessons" :key="ch.id">
              <!-- Chapter Header -->
              <button @click="toggleChapter(ch.id)"
                :class="['flex w-full items-center gap-3 px-5 py-4 text-left transition-colors hover:bg-slate-50/80', ch.status === 'locked' ? 'opacity-60' : '']">
                <div :class="['flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ring-1', toneIcon[ch.tone] || toneIcon.slate]">
                  <component :is="resolveIcon(ch.icon)" :size="16" />
                </div>
                <div class="flex-1 min-w-0">
                  <div class="flex flex-wrap items-center gap-2 mb-0.5">
                    <span class="text-sm font-bold text-slate-900">{{ ch.chapter }}: {{ ch.title }}</span>
                    <span :class="['rounded-full px-2 py-0.5 text-[11px] font-semibold', statusBadge[ch.status] || statusBadge.upcoming]">{{ ch.badge }}</span>
                    <span :class="['learning-access-badge', accessTone(ch.accessStatus)]">{{ accessBadge[ch.accessStatus] }}</span>
                  </div>
                  <div class="flex items-center gap-3">
                    <span v-for="m in ch.meta" :key="m" class="text-xs text-slate-400">{{ m }}</span>
                    <span class="text-xs text-slate-400">Kỳ {{ ch.studentCurrentSemesterIndex }} · Block {{ ch.studentCurrentBlockIndex }} hiện tại</span>
                  </div>
                  <div v-if="ch.progress > 0" class="mt-2 h-1 overflow-hidden rounded-full bg-slate-200 w-32">
                    <div :class="['h-full rounded-full bg-gradient-to-r', toneBar[ch.tone] || toneBar.slate]" :style="{ width: ch.progress + '%' }" />
                  </div>
                </div>
                <component :is="resolveIcon(expandedChapters[ch.id] ? 'ChevronUp' : 'ChevronDown')" :size="16" class="text-slate-400 shrink-0" />
              </button>

              <!-- Lesson List -->
              <div v-if="expandedChapters[ch.id] && ch.lessons.length" class="bg-slate-50/50 px-5 pb-3 space-y-1">
                <button v-for="lesson in ch.lessons" :key="lesson.id"
                  @click="selectLesson(ch, lesson)"
                  :class="['lesson-access-row', isLocked(lesson) ? 'is-access-locked' : 'hover:bg-white cursor-pointer', selectedLessonId === lesson.id ? 'is-selected' : '']"
                  :aria-disabled="isLocked(lesson)"
                  :title="isLocked(lesson) ? getLockedReason(lesson) : ''">
                  <component :is="resolveIcon(lessonIcon(lesson))"
                    :size="14" :class="isLocked(lesson) ? 'text-slate-400' : needsEarlyLearningConfirm(lesson) ? 'text-violet-500' : lesson.accessStatus === LEARNING_ACCESS.COMPLETED || lesson.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED ? 'text-green-500' : 'text-blue-500'" />
                  <span class="flex-1 min-w-0">
                    <span :class="['block text-sm', selectedLessonId === lesson.id ? 'font-semibold text-blue-800' : 'text-slate-600']">{{ lesson.title }}</span>
                    <span v-if="isLocked(lesson)" class="block text-[11px] font-medium text-slate-400">{{ getLockedReason(lesson) }}</span>
                    <span v-else-if="needsEarlyLearningConfirm(lesson)" class="block text-[11px] font-medium text-violet-500">Nội dung kỳ sau, có thể học trước.</span>
                    <span v-else-if="lesson.accessStatus === LEARNING_ACCESS.EARLY_COMPLETED" class="block text-[11px] font-medium text-violet-500">Đã học trước · điểm {{ lesson.earlyScore }}/10</span>
                  </span>
                  <span :class="['learning-access-badge shrink-0', accessTone(lesson.accessStatus)]">{{ accessBadge[lesson.accessStatus] }}</span>
                  <span class="text-xs text-slate-400 shrink-0">{{ lesson.duration }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT SIDEBAR -->
      <div class="space-y-4">

        <!-- Learning Path / Timeline -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 px-5 py-4">
            <h3 class="text-base font-bold text-slate-900">Lộ trình học</h3>
          </div>
          <div class="px-5 py-4 space-y-0">
            <div v-for="item in mockTimeline" :key="item.id"
              class="relative border-l-2 border-slate-200 pl-5 pb-5 last:pb-0">
              <span :class="['absolute -left-[5px] top-1 h-2.5 w-2.5 rounded-full ring-4 ring-white', toneDot[item.tone] || toneDot.slate]" />
              <div class="flex items-start gap-2">
                <div class="flex-1">
                  <p class="text-sm font-bold text-slate-900">{{ item.title }}</p>
                  <p class="text-xs text-slate-500 mt-0.5">{{ item.description }}</p>
                  <span :class="['mt-1.5 inline-block rounded-full px-2 py-0.5 text-[11px] font-semibold', statusBadge[item.tone] || 'bg-slate-100 text-slate-500']">{{ item.time }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- AI Summary -->
        <div class="rounded-[24px] border border-violet-100 bg-gradient-to-br from-violet-50/80 to-white shadow-sm overflow-hidden">
          <div class="border-b border-violet-100 px-5 py-4 flex items-center gap-2">
            <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-violet-100 text-violet-700">
              <component :is="resolveIcon('Sparkles')" :size="16" />
            </div>
            <h3 class="text-sm font-bold text-slate-900">AI Tóm tắt bài học</h3>
          </div>
          <div class="px-5 py-4">
            <ul class="space-y-2">
              <li v-for="(pt, i) in mockAISummary.points" :key="i" class="flex gap-2 text-sm text-slate-600">
                <span class="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-violet-400"></span>
                {{ pt }}
              </li>
            </ul>
            <button class="mt-4 flex w-full items-center justify-center gap-2 rounded-xl border border-violet-200 bg-violet-50 px-4 py-2.5 text-sm font-semibold text-violet-700 hover:bg-violet-100 transition-colors">
              <component :is="resolveIcon('MessageSquare')" :size="15" /> Hỏi AI về bài học
            </button>
          </div>
        </div>

        <!-- Study Notes -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 px-5 py-4 flex items-center gap-2">
            <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-amber-50 text-amber-600">
              <component :is="resolveIcon('PenLine')" :size="16" />
            </div>
            <h3 class="text-sm font-bold text-slate-900">Ghi chú học tập</h3>
          </div>
          <div class="px-5 py-4">
            <textarea placeholder="Ghi chú nhanh về bài học này..." rows="4"
              class="w-full resize-none rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-700 outline-none focus:border-amber-400 focus:ring-2 focus:ring-amber-400/20 transition-all" />
            <button class="mt-2 inline-flex items-center gap-1.5 rounded-xl bg-amber-500 px-3 py-2 text-xs font-semibold text-white hover:bg-amber-600 transition-colors">
              <component :is="resolveIcon('Save')" :size="12" /> Lưu ghi chú
            </button>
          </div>
        </div>

      </div>
    </div>

    <section v-if="accessNotice" class="course-access-notice" role="status">
      <component :is="resolveIcon('ShieldAlert')" :size="16" />
      <div>
        <strong>{{ accessNotice.title }}</strong>
        <p>{{ accessNotice.message }}</p>
      </div>
      <button type="button" @click="accessNotice = null">Đóng</button>
    </section>

    <Teleport to="body">
      <div v-if="pendingEarlyLesson" class="course-modal-backdrop" @click.self="closeEarlyLessonModal">
        <section class="course-early-modal" role="dialog" aria-modal="true" aria-labelledby="course-early-title">
          <div class="course-modal-icon">
            <component :is="resolveIcon('FastForward')" :size="20" />
          </div>
          <h2 id="course-early-title">Bạn đang học trước lộ trình</h2>
          <p>
            Nội dung này thuộc Kỳ {{ pendingEarlyLesson.plannedSemesterIndex }} · Block {{ pendingEarlyLesson.plannedBlockIndex }}
            trong lộ trình tương lai. Bạn vẫn có thể học trước và kết quả sẽ được ghi nhận ở trạng thái học trước.
            Khi đến đúng kỳ/block, hệ thống sẽ áp dụng theo quy định của môn học.
          </p>
          <div class="course-modal-subject">
            <strong>{{ pendingEarlyLesson.title }}</strong>
            <span>{{ pendingEarlyLesson.chapter.chapter }} · {{ pendingEarlyLesson.chapter.title }}</span>
          </div>
          <div class="course-modal-actions">
            <button type="button" class="course-ghost-button" @click="closeEarlyLessonModal">Quay lại</button>
            <button type="button" class="course-primary-button" @click="confirmEarlyLesson">
              Tiếp tục học trước
            </button>
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.learning-access-badge {
  display: inline-flex;
  align-items: center;
  min-height: 1.35rem;
  border-radius: 999px;
  padding: 0.18rem 0.5rem;
  font-size: 0.66rem;
  font-weight: 850;
  line-height: 1;
  white-space: nowrap;
}

.access-official { color: var(--color-success-text); background: var(--color-success-bg); }
.access-early { color: #7c3aed; background: rgba(237, 233, 254, 0.82); }
.access-early-done { color: #6d28d9; background: rgba(237, 233, 254, 0.72); }
.access-locked { color: var(--color-warning-text); background: var(--color-warning-bg); }
.access-future { color: var(--text-placeholder); background: var(--surface-input); }
.access-completed { color: var(--text-link); background: color-mix(in srgb, var(--color-info-bg) 72%, transparent); }

:global(.dark) .access-early,
:global(.dark) .access-early-done {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.36);
}

.lesson-access-row {
  display: flex;
  width: 100%;
  align-items: center;
  gap: 0.75rem;
  border-radius: 0.75rem;
  padding: 0.625rem 0.75rem;
  text-align: left;
  transition:
    background-color 160ms ease,
    box-shadow 160ms ease;
}

.lesson-access-row.is-selected {
  background: var(--color-info-bg);
  box-shadow: 0 0 0 1px var(--border-input-focus);
}

.lesson-access-row.is-access-locked {
  cursor: not-allowed;
  background: color-mix(in srgb, var(--surface-input) 82%, transparent);
}

.course-access-notice {
  position: fixed;
  right: 1rem;
  bottom: 1rem;
  z-index: 40;
  display: flex;
  align-items: flex-start;
  gap: 0.65rem;
  max-width: min(26rem, calc(100vw - 2rem));
  border: 1px solid var(--border-card);
  border-radius: 18px;
  background: var(--surface-card-strong);
  color: var(--text-body);
  padding: 0.75rem;
  box-shadow: var(--lg-shadow-md);
  backdrop-filter: blur(18px) saturate(160%);
}

.course-access-notice strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.82rem;
}

.course-access-notice p {
  margin: 0.15rem 0 0;
  color: var(--text-label);
  font-size: 0.75rem;
}

.course-access-notice button {
  margin-left: auto;
  border: 0;
  background: transparent;
  color: var(--text-link);
  cursor: pointer;
  font-size: 0.72rem;
  font-weight: 850;
}

.course-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: grid;
  place-items: center;
  background: rgba(15, 23, 42, 0.44);
  padding: 1rem;
  backdrop-filter: blur(8px);
}

.course-early-modal {
  width: min(30rem, 100%);
  border: 1px solid var(--border-card);
  border-radius: 22px;
  background: var(--surface-modal);
  color: var(--text-body);
  padding: 1rem;
  box-shadow: var(--lg-shadow-lg);
}

.course-modal-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 16px;
  color: #7c3aed;
  background: rgba(237, 233, 254, 0.78);
}

:global(.dark) .course-modal-icon {
  color: #d8b4fe;
  background: rgba(88, 28, 135, 0.32);
}

.course-early-modal h2 {
  margin: 0.8rem 0 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
}

.course-early-modal p {
  margin: 0.55rem 0 0;
  color: var(--text-label);
  font-size: 0.85rem;
  line-height: 1.55;
}

.course-modal-subject {
  display: grid;
  gap: 0.2rem;
  margin-top: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--surface-input);
  padding: 0.65rem;
}

.course-modal-subject strong {
  color: var(--text-heading);
  font-size: 0.85rem;
}

.course-modal-subject span {
  color: var(--text-label);
  font-size: 0.76rem;
  font-weight: 700;
}

.course-modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.55rem;
  margin-top: 1rem;
}

.course-ghost-button,
.course-primary-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 2.35rem;
  border-radius: 12px;
  cursor: pointer;
  padding: 0 0.85rem;
  font-size: 0.8rem;
  font-weight: 850;
}

.course-ghost-button {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.course-primary-button {
  border: 0;
  background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-primary));
  color: #ffffff;
  box-shadow: 0 8px 20px rgba(37, 99, 235, 0.22);
}

@media (max-width: 640px) {
  .lesson-access-row {
    align-items: flex-start;
    flex-wrap: wrap;
  }

  .course-modal-actions {
    flex-direction: column-reverse;
  }

  .course-ghost-button,
  .course-primary-button {
    width: 100%;
  }
}
</style>
