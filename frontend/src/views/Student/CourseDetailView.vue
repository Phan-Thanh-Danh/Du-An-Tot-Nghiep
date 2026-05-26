<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'
import {
  mockCourse, mockStats, mockLessons,
  mockCurrentLesson, mockQuizQuestions,
  mockComments, mockTimeline, mockAISummary,
} from '@/data/courseDetail.mock.js'

const route = useRoute()
const courseId = String(route.params.courseId || mockCourse.id)

// ── Tab & lesson state ──────────────────────────────────
const activeTab = ref('video')
const selectedLessonId = ref(mockCurrentLesson.id)
const selectedChapterId = ref(mockCurrentLesson.chapterId)
const expandedChapters = ref({ [mockCurrentLesson.chapterId]: true })
const quizAnswers = ref({})
const newComment = ref('')
const likedComments = ref({})

const currentLesson = ref({ ...mockCurrentLesson })
const watchProgress = computed(() =>
  Math.round((currentLesson.value.watchedSeconds / currentLesson.value.totalSeconds) * 100)
)
const watchFormatted = computed(() => {
  const s = currentLesson.value.watchedSeconds
  return `${Math.floor(s / 60)}:${String(s % 60).padStart(2, '0')}`
})

function selectLesson(chapter, lesson) {
  if (lesson.status === 'locked') return
  selectedChapterId.value = chapter.id
  selectedLessonId.value = lesson.id
  currentLesson.value = {
    ...mockCurrentLesson,
    id: lesson.id,
    chapterId: chapter.id,
    chapterTitle: `${chapter.chapter}: ${chapter.title}`,
    title: lesson.title,
    duration: lesson.duration,
  }
  activeTab.value = 'video'
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
            <div class="relative flex h-52 w-full items-center justify-center rounded-2xl bg-gradient-to-br from-slate-800 to-slate-700 mb-4 overflow-hidden">
              <div class="absolute inset-0 flex items-center justify-center opacity-10">
                <component :is="resolveIcon('Film')" :size="80" class="text-white" />
              </div>
              <button class="relative flex h-10 w-10 items-center justify-center rounded-full bg-white/90 shadow-lg hover:scale-105 transition-transform">
                <component :is="resolveIcon('Play')" :size="24" class="text-blue-700 translate-x-0.5" />
              </button>
              <div class="absolute bottom-3 right-4 text-white/70 text-xs font-semibold">{{ watchFormatted }} / {{ currentLesson.duration }}</div>
            </div>
            <div class="mb-4">
              <div class="flex justify-between text-xs text-slate-500 mb-1">
                <span>Đã xem</span><span>{{ watchProgress }}%</span>
              </div>
              <div class="h-2 overflow-hidden rounded-full bg-slate-200">
                <div class="h-full rounded-full bg-gradient-to-r from-blue-600 to-cyan-500 transition-all" :style="{ width: watchProgress + '%' }" />
              </div>
            </div>
            <button class="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 transition-colors shadow-sm">
              <component :is="resolveIcon('Play')" :size="15" /> Tiếp tục học
            </button>
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
            <div v-for="ch in mockLessons" :key="ch.id">
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
                  </div>
                  <div class="flex items-center gap-3">
                    <span v-for="m in ch.meta" :key="m" class="text-xs text-slate-400">{{ m }}</span>
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
                  :class="['flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left transition-all', lesson.status === 'locked' ? 'opacity-50 cursor-not-allowed' : 'hover:bg-white cursor-pointer', selectedLessonId === lesson.id ? 'bg-blue-50 ring-1 ring-blue-200' : '']"
                  :disabled="lesson.status === 'locked'"
                  :title="lesson.status === 'locked' ? 'Cần hoàn thành bài trước' : ''">
                  <component :is="resolveIcon(lesson.status === 'completed' ? 'CheckCircle2' : lesson.status === 'active' ? 'Play' : 'Lock')"
                    :size="14" :class="lesson.status === 'completed' ? 'text-green-500' : lesson.status === 'active' ? 'text-blue-500' : 'text-slate-400'" />
                  <span :class="['flex-1 text-sm', selectedLessonId === lesson.id ? 'font-semibold text-blue-800' : 'text-slate-600']">{{ lesson.title }}</span>
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
  </div>
</template>
