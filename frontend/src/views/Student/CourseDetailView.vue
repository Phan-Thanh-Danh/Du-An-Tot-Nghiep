<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'
import LessonVideoPlayer from '@/components/learning/LessonVideoPlayer.vue'
import { studentApi } from '@/services/studentApi'
import {
  canStartLearning,
  getLockedReason,
  isLocked,
  LEARNING_ACCESS,
  needsEarlyLearningConfirm,
} from '@/utils/learningAccess.js'

const activeTab = ref('video')
const selectedLessonId = ref('')
const expandedChapters = ref({})
const quizAnswers = ref({})
const newComment = ref('')
const likedComments = ref({})
const accessNotice = ref(null)
const pendingEarlyLesson = ref(null)
const lessonProgressDrafts = ref({})
const quizSubmitted = ref(false)

// ── DYNAMIC COURSE LOAD LOGIC ─────────────────────────
const route = useRoute()
const courseId = computed(() => route.params.courseId)

const apiCourse = ref(null)
const apiStats = ref(null)
const apiLessons = ref(null)
const isLoadingApi = ref(true)

watch(() => courseId.value, async (newId) => {
  if (newId) {
    try {
      isLoadingApi.value = true
      const res = await studentApi.getCourseDetail(newId)
      const isSuccess = res.success === true || res.Success === true
      if (isSuccess) {
        const data = res.data || res.Data || {}
        apiCourse.value = data.course || data.Course || null
        
        // Cần format lại các key viết hoa từ C# sang camelCase cho Frontend dễ dùng
        const rawStats = data.stats || data.Stats || null
        if (rawStats) {
           apiStats.value = rawStats.map(s => ({
             label: s.label || s.Label,
             value: s.value || s.Value,
             unit: s.unit || s.Unit,
             icon: s.icon || s.Icon,
             tone: s.tone || s.Tone,
             progress: s.progress || s.Progress,
             hint: s.hint || s.Hint
           }))
        } else {
           apiStats.value = null
        }
        
        const rawLessons = data.lessons || data.Lessons || null
        if (rawLessons) {
          apiLessons.value = rawLessons.map(c => ({
            id: c.id || c.Id,
            chapter: c.chapter || c.Chapter,
            title: c.title || c.Title,
            description: c.description || c.Description,
            status: c.status || c.Status,
            badge: c.badge || c.Badge,
            tone: c.tone || c.Tone,
            icon: c.icon || c.Icon,
            meta: c.meta || c.Meta,
            progress: c.progress || c.Progress,
            lessons: (c.lessons || c.Lessons || []).map(l => ({
              id: l.id || l.Id,
              title: l.title || l.Title,
              duration: l.duration || l.Duration,
              status: l.status || l.Status,
              type: l.type || l.Type,
              url: l.url || l.Url
            }))
          }))
        } else {
          apiLessons.value = null
        }
      }
    } catch (err) {
      console.error('Failed to load course details from API', err)
      apiCourse.value = null
      apiStats.value = null
      apiLessons.value = null
    } finally {
      isLoadingApi.value = false
    }
  }
}, { immediate: true })

const courseInfo = computed(() => {
  if (!apiCourse.value) return null
  const c = apiCourse.value
  return {
    id: c.id || c.Id,
    title: c.title || c.Title,
    code: c.code || c.Code,
    teacher: c.teacher || c.Teacher,
    semester: c.semester || c.Semester,
    credits: c.credits || c.Credits,
    coverGradient: c.coverGradient || c.CoverGradient,
    description: c.description || c.Description,
  }
})

const courseStats = computed(() => apiStats.value || [])

const courseLessons = computed(() => apiLessons.value || [])

const apiQuiz = ref(null)
const apiComments = ref(null)

const quizQuestions = computed(() => (apiQuiz.value && apiQuiz.value.length > 0) ? apiQuiz.value : [])

const currentComments = computed(() => {
  return apiComments.value || []
})

const aiSummary = computed(() => null)



const learningLessons = computed(() => {
  const defaultSemesterIdx = 1
  const defaultBlockIdx = 1

  return courseLessons.value.map((chapter) => ({
    studentCurrentSemesterIndex: defaultSemesterIdx,
    studentCurrentBlockIndex: defaultBlockIdx,
    ...chapter,
    accessStatus: chapter.status === 'completed' ? LEARNING_ACCESS.COMPLETED : LEARNING_ACCESS.OFFICIAL,
    lessons: (chapter.lessons || []).map((lesson) => ({
      studentCurrentSemesterIndex: defaultSemesterIdx,
      studentCurrentBlockIndex: defaultBlockIdx,
      allowEarlyLearning: false,
      accessStatus: lesson.status === 'completed' ? LEARNING_ACCESS.COMPLETED : LEARNING_ACCESS.OFFICIAL,
      lessonType: lesson.type || 'video',
      ...lesson,
    })),
  }))
})

const currentLesson = ref({})

const flatLessons = computed(() =>
  learningLessons.value.flatMap((chapter) =>
    chapter.lessons.map((lesson) => ({
      chapter,
      lesson,
    }))
  )
)

const currentLessonIndex = computed(() =>
  flatLessons.value.findIndex((item) => item.lesson.id === selectedLessonId.value)
)

const previousLesson = computed(() => flatLessons.value[currentLessonIndex.value - 1] || null)
const nextLesson = computed(() => flatLessons.value[currentLessonIndex.value + 1] || null)

const miniStats = computed(() => {
  const stats = courseStats.value
  if (stats.length >= 4) {
    return [
      { label: 'Tiến độ', value: `${stats[0]?.value || 0}%` },
      { label: 'Bài học', value: `${stats[1]?.value || 0}${stats[1]?.unit || ''}` },
      { label: 'Bài tập', value: `${stats[2]?.value || 0} mục` },
      { label: 'Tài liệu', value: `${stats[3]?.value || 0} file` },
    ]
  }
  return [
    { label: 'Tiến độ', value: '--' },
    { label: 'Bài học', value: '--' },
    { label: 'Bài tập', value: '--' },
    { label: 'Tài liệu', value: '--' },
  ]
})

const currentLessonStatusLabel = computed(() => accessBadge[currentLesson.value.accessStatus] || 'Đang học')

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
  selectedLessonId.value = lesson.id
  expandedChapters.value[chapter.id] = true
  currentLesson.value = {
    ...lesson,
    ...lessonProgressDrafts.value[lesson.id],
    id: lesson.id,
    chapterId: chapter.id,
    chapterTitle: `${chapter.chapter}: ${chapter.title}`,
    title: lesson.title,
    duration: lesson.duration,
    durationSeconds: parseDurationSeconds(lesson.duration) || lesson.durationSeconds || 0,
  }
  activeTab.value = lesson.lessonType === 'quiz' ? 'quiz' : lesson.lessonType === 'assignment' ? 'document' : 'video'
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
  if (payload.lessonId === currentLesson.value.id) {
    currentLesson.value = {
      ...currentLesson.value,
      ...lessonProgressDrafts.value[payload.lessonId],
    }
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

function isQuestionLocked(index) {
  if (index === 0) return false
  for (let i = 0; i < index; i++) {
    const prevQId = mockQuizQuestions.value[i].id
    if (quizAnswers.value[prevQId] === undefined || quizAnswers.value[prevQId] === null) {
      return true
    }
  }
  return false
}

const isQuizFullyAnswered = computed(() => {
  return mockQuizQuestions.value.every(q => quizAnswers.value[q.id] !== undefined && quizAnswers.value[q.id] !== null)
})

function submitQuiz() {
  quizSubmitted.value = true
  if (currentLesson.value) {
    currentLesson.value.progressPercent = 100
    lessonProgressDrafts.value[currentLesson.value.id] = {
      progressPercent: 100,
      completedAt: new Date().toISOString()
    }
  }
}

function selectAnswer(qId, idx) {
  quizAnswers.value[qId] = idx
}

function toggleLike(cId) {
  likedComments.value[cId] = !likedComments.value[cId]
}

function navigateRelative(target) {
  if (!target) return
  selectLesson(target.chapter, target.lesson)
}

function resolveIcon(name) {
  return LucideIcons[name] || LucideIcons.Circle
}

const typeConfig = {
  video: { label: 'Video', icon: 'PlayCircle' },
  document: { label: 'Tài liệu', icon: 'FileText' },
  quiz: { label: 'Quiz', icon: 'ListChecks' },
  assignment: { label: 'Bài tập', icon: 'ClipboardList' },
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
  return typeConfig[lesson.lessonType]?.icon || 'PlayCircle'
}

function progressWidth(lesson) {
  return `${Math.max(0, Math.min(100, lessonProgressDrafts.value[lesson.id]?.progressPercent ?? lesson.progressPercent ?? 0))}%`
}

function lessonTypeLabel(lesson) {
  return typeConfig[lesson.lessonType]?.label || 'Bài học'
}

// Lắng nghe thay đổi của courseId để cập nhật bài học và reset quiz (đặt ở cuối để tránh lỗi ReferenceError)
watch(
  courseId,
  (newId) => {
    if (!newId) return
    quizAnswers.value = {}
    quizSubmitted.value = false
    accessNotice.value = null
    pendingEarlyLesson.value = null

    const lessons = learningLessons.value
    if (lessons && lessons.length > 0) {
      let foundLesson = null
      let foundChapter = null
      
      for (const ch of lessons) {
        const activeL = ch.lessons?.find(l => l.status === 'active' || l.status === 'learning')
        if (activeL) {
          foundLesson = activeL
          foundChapter = ch
          break
        }
      }
      
      if (!foundLesson) {
        foundChapter = lessons[0]
        foundLesson = lessons[0].lessons?.[0]
      }
      
      if (foundLesson && foundChapter) {
        expandedChapters.value = { [foundChapter.id]: true }
        selectedLessonId.value = foundLesson.id
        
        currentLesson.value = {
          videoUrl: '',
          durationSeconds: 1200,
          allowSeek: true,
          pauseOnBlur: true,
          minWatchPercentToComplete: 80,
          progressPercent: foundLesson.status === 'completed' ? 100 : (foundLesson.progressPercent || 0),
          documentTitle: foundLesson.url ? foundLesson.url.split('/').pop() : `${foundLesson.title}.pdf`,
          documentPages: 10,
          documentCurrentPage: 1,
          ...foundLesson,
          id: foundLesson.id,
          chapterId: foundChapter.id,
          chapterTitle: `${foundChapter.chapter}: ${foundChapter.title}`,
          title: foundLesson.title,
          duration: foundLesson.duration || '20:00',
        }
        
        activeTab.value = foundLesson.lessonType === 'quiz' ? 'quiz' : foundLesson.lessonType === 'assignment' ? 'document' : 'video'
      }
    }
  },
  { immediate: true }
)

// Auto-select first lesson when API data loads
watch(learningLessons, (lessons) => {
  if (lessons && lessons.length > 0 && !selectedLessonId.value) {
    const firstChapter = lessons[0]
    const firstLesson = firstChapter.lessons?.[0]
    if (firstLesson && firstChapter) {
      expandedChapters.value = { [firstChapter.id]: true }
      selectedLessonId.value = firstLesson.id
      currentLesson.value = {
        videoUrl: '',
        durationSeconds: 1200,
        allowSeek: true,
        pauseOnBlur: true,
        minWatchPercentToComplete: 80,
        progressPercent: firstLesson.progressPercent || 0,
        documentTitle: firstLesson.url ? firstLesson.url.split('/').pop() : `${firstLesson.title}.pdf`,
        documentPages: 10,
        documentCurrentPage: 1,
        ...firstLesson,
        id: firstLesson.id,
        chapterId: firstChapter.id,
        chapterTitle: `${firstChapter.chapter}: ${firstChapter.title}`,
        title: firstLesson.title,
        duration: firstLesson.duration || '20:00',
      }
      activeTab.value = firstLesson.lessonType === 'quiz' ? 'quiz' : firstLesson.lessonType === 'assignment' ? 'document' : 'video'
    }
  }
})

watch(() => currentLesson.value?.id, async (newLessonId) => {
  if (newLessonId && courseId.value) {
    if (currentLesson.value.lessonType === 'quiz') {
      try {
        const res = await studentApi.getLessonQuiz(courseId.value, newLessonId)
        apiQuiz.value = res.data || res.Data || []
      } catch (err) {
        console.error(err)
        apiQuiz.value = []
      }
    }

    try {
      const res = await studentApi.getLessonComments(courseId.value, newLessonId)
      apiComments.value = res.data || res.Data || []
    } catch (err) {
      console.error(err)
      apiComments.value = []
    }
  }
})
</script>

<template>
  <div class="course-player-page">
    <header class="course-header">
      <div class="course-header-actions">
        <router-link to="/student/courses" class="ghost-action">
          <component :is="resolveIcon('ArrowLeft')" :size="14" />
          Tất cả khóa học
        </router-link>
        <router-link to="/student/assignments" class="secondary-action">
          <component :is="resolveIcon('ClipboardList')" :size="14" />
          Bài tập
        </router-link>
      </div>

      <div class="course-header-main">
        <div class="course-eyebrow">
          <component :is="resolveIcon('BookOpenCheck')" :size="14" />
          {{ courseInfo?.code || '—' }} · {{ courseInfo?.semester || '—' }}
        </div>
        <h1>{{ courseInfo?.title || 'Chi tiết khóa học' }}</h1>
        <p>
          <component :is="resolveIcon('UserRound')" :size="14" />
          {{ courseInfo?.teacher || '—' }} · {{ courseInfo?.credits || '—' }} tín chỉ
        </p>
      </div>

      <div class="course-mini-stats" aria-label="Tổng quan khóa học">
        <div v-for="stat in miniStats" :key="stat.label" class="mini-stat">
          <span>{{ stat.label }}</span>
          <strong>{{ stat.value }}</strong>
        </div>
      </div>
    </header>

    <main class="learning-shell">
      <section class="lesson-column">
        <article class="lesson-panel">
          <div class="lesson-heading">
            <div class="lesson-title-block">
              <span class="chapter-chip">{{ currentLesson.chapterTitle }}</span>
              <h2>{{ currentLesson.title }}</h2>
              <div class="lesson-meta-row">
                <span>
                  <component :is="resolveIcon(typeConfig[currentLesson.lessonType]?.icon || 'PlayCircle')" :size="14" />
                  {{ lessonTypeLabel(currentLesson) }}
                </span>
                <span>
                  <component :is="resolveIcon('Clock3')" :size="14" />
                  {{ currentLesson.duration }}
                </span>
                <span>
                  <component :is="resolveIcon('Gauge')" :size="14" />
                  {{ currentLesson.progressPercent || 0 }}%
                </span>
              </div>
            </div>
            <span :class="['learning-access-badge', accessTone(currentLesson.accessStatus)]">
              {{ currentLessonStatusLabel }}
            </span>
          </div>

          <div class="lesson-tabs" aria-label="Nội dung bài học">
            <button
              v-for="tab in [
                { key: 'video', label: 'Video', icon: 'PlayCircle' },
                { key: 'document', label: 'Tài liệu', icon: 'FileText' },
                { key: 'quiz', label: 'Quiz', icon: 'ListChecks' },
                { key: 'discussion', label: 'Thảo luận', icon: 'MessagesSquare' },
              ]"
              :key="tab.key"
              type="button"
              :class="{ active: activeTab === tab.key }"
              @click="activeTab = tab.key"
            >
              <component :is="resolveIcon(tab.icon)" :size="14" />
              {{ tab.label }}
            </button>
          </div>

          <div class="lesson-content">
            <LessonVideoPlayer
              v-if="activeTab === 'video'"
              :key="currentLesson.id"
              :lesson="currentLesson"
              @progress="handleVideoProgress"
              @completed="handleVideoCompleted"
            />

            <div v-else-if="activeTab === 'document'" class="document-viewer">
              <div class="document-preview">
                <component :is="resolveIcon('FileText')" :size="36" />
                <strong>{{ currentLesson.documentTitle }}</strong>
                <span>Trang {{ currentLesson.documentCurrentPage }} / {{ currentLesson.documentPages }}</span>
              </div>
              <button type="button" class="secondary-action">
                <component :is="resolveIcon('ExternalLink')" :size="15" />
                Mở tài liệu
              </button>
            </div>

            <div v-else-if="activeTab === 'quiz'" class="quiz-view">
              <div
                v-for="(q, index) in quizQuestions"
                :key="q.id || q.Id"
                class="quiz-card"
                :class="{ 'opacity-50 pointer-events-none': isQuestionLocked(index) }"
              >
                <div class="flex items-center justify-between mb-2">
                  <p class="font-medium text-heading">Câu {{ index + 1 }}: {{ q.question || q.Text }}</p>
                  <span v-if="isQuestionLocked(index)" class="text-xs text-(--color-warning-text) font-semibold flex items-center gap-1">
                    <component :is="resolveIcon('Lock')" :size="12" /> Làm câu trước đó
                  </span>
                </div>
                <div class="quiz-options">
                  <button
                    v-for="(opt, idx) in q.options || q.Options"
                    :key="idx"
                    type="button"
                    :disabled="isQuestionLocked(index)"
                    :class="{ selected: quizAnswers[q.id || q.Id] === idx }"
                    @click="selectAnswer(q.id || q.Id, idx)"
                  >
                    <span>{{ ['A', 'B', 'C', 'D'][idx] }}</span>
                    {{ opt }}
                  </button>
                </div>
              </div>
              <button
                type="button"
                class="primary-action w-full justify-center"
                :disabled="!isQuizFullyAnswered"
                @click="submitQuiz"
              >
                <component :is="resolveIcon('Send')" :size="15" />
                Nộp bài Quiz
              </button>
              <p v-if="quizSubmitted" class="text-xs text-(--color-success-text) font-semibold text-center mt-3">
                Chúc mừng! Bạn đã hoàn thành bài Quiz. Tiến độ bài học đã được cập nhật thành 100%.
              </p>
            </div>

            <div v-else class="discussion-view">
              <div class="comment-composer">
                <div class="avatar">SV</div>
                <div>
                  <textarea
                    v-model="newComment"
                    rows="2"
                    placeholder="Nhập câu hỏi hoặc thảo luận về bài học..."
                  />
                  <button type="button" class="primary-action compact">
                    <component :is="resolveIcon('Send')" :size="12" />
                    Gửi
                  </button>
                </div>
              </div>

              <article v-for="comment in currentComments" :key="comment.id || comment.Id" class="comment-card">
                <div class="avatar comment-avatar">{{ comment.initials || comment.Initials }}</div>
                <div class="comment-body">
                  <div class="comment-author">
                    <strong>{{ comment.author || comment.Author }}</strong>
                    <span>{{ comment.time || comment.TimeAgo }}</span>
                  </div>
                  <p>{{ comment.content || comment.Content }}</p>
                  <button type="button" :class="{ liked: likedComments[comment.id || comment.Id] }" @click="toggleLike(comment.id || comment.Id)">
                    <component :is="resolveIcon('ThumbsUp')" :size="12" />
                    {{ (comment.likes !== undefined ? comment.likes : comment.Likes) + (likedComments[comment.id || comment.Id] ? 1 : 0) }}
                  </button>
                </div>
              </article>
            </div>
          </div>
        </article>

        <section class="lesson-body">
          <div>
            <span class="section-kicker">Nội dung bài học</span>
            <h3>{{ currentLesson.title }}</h3>
            <p>
              Bài học tập trung vào cách sử dụng {{ currentLesson.title.toLowerCase() }} trong bài toán thực tế,
              đi từ khái niệm, thao tác chính đến tình huống áp dụng trong thuật toán.
            </p>
          </div>
          <div class="completion-callout">
            <component :is="resolveIcon('ShieldCheck')" :size="16" />
            <span v-if="(currentLesson.progressPercent || 0) >= (currentLesson.minWatchPercentToComplete || 80)">
              Bạn đã đạt điều kiện hoàn thành bài học.
            </span>
            <span v-else>
              Cần xem tối thiểu {{ currentLesson.minWatchPercentToComplete || 80 }}% video để mở bài tiếp theo.
            </span>
          </div>
        </section>

        <nav class="lesson-nav" aria-label="Điều hướng bài học">
          <button type="button" class="secondary-action" :disabled="!previousLesson" @click="navigateRelative(previousLesson)">
            <component :is="resolveIcon('ArrowLeft')" :size="15" />
            Bài trước
          </button>

          <div v-if="nextLesson && isLocked(nextLesson.lesson)" class="next-lock-copy">
            {{ getLockedReason(nextLesson.lesson) }}
          </div>

          <button
            type="button"
            class="primary-action"
            :disabled="!nextLesson || isLocked(nextLesson.lesson)"
            @click="navigateRelative(nextLesson)"
          >
            Bài tiếp theo
            <component :is="resolveIcon('ArrowRight')" :size="15" />
          </button>
        </nav>
      </section>

      <aside class="course-side">
        <section class="outline-panel">
          <div class="side-heading">
            <h3>Course outline</h3>
            <span>{{ flatLessons.length }} bài</span>
          </div>

          <div class="chapter-list">
            <article v-for="chapter in learningLessons" :key="chapter.id" class="chapter-block">
              <button type="button" class="chapter-header" @click="toggleChapter(chapter.id)">
                <div>
                  <strong>{{ chapter.chapter }}</strong>
                  <span>{{ chapter.title }}</span>
                </div>
                <component :is="resolveIcon(expandedChapters[chapter.id] ? 'ChevronUp' : 'ChevronDown')" :size="15" />
              </button>

              <div v-if="expandedChapters[chapter.id]" class="lesson-list">
                <button
                  v-for="lesson in chapter.lessons"
                  :key="lesson.id"
                  type="button"
                  :class="['outline-lesson', { active: selectedLessonId === lesson.id, locked: isLocked(lesson) }]"
                  :title="isLocked(lesson) ? getLockedReason(lesson) : ''"
                  @click="selectLesson(chapter, lesson)"
                >
                  <component :is="resolveIcon(lessonIcon(lesson))" :size="14" />
                  <span class="outline-lesson-main">
                    <strong>{{ lesson.title }}</strong>
                    <small>
                      {{ lessonTypeLabel(lesson) }} · {{ lesson.duration }}
                      <template v-if="isLocked(lesson)"> · {{ getLockedReason(lesson) }}</template>
                    </small>
                    <span class="outline-progress" aria-hidden="true">
                      <span :style="{ width: progressWidth(lesson) }" />
                    </span>
                  </span>
                  <span :class="['learning-access-badge mini', accessTone(lesson.accessStatus)]">
                    {{ accessBadge[lesson.accessStatus] }}
                  </span>
                </button>
              </div>
            </article>
          </div>
        </section>

        <section v-if="aiSummary" class="side-card ai-card">
          <div class="side-heading">
            <h3>AI tóm tắt bài học</h3>
            <component :is="resolveIcon('Sparkles')" :size="16" />
          </div>
          <ul>
            <li v-for="point in aiSummary.keyTakeaways" :key="point">{{ point }}</li>
          </ul>
          <button type="button" class="secondary-action full">
            <component :is="resolveIcon('MessageSquare')" :size="15" />
            Hỏi AI về bài học
          </button>
        </section>

        <section class="side-card notes-card">
          <div class="side-heading">
            <h3>Ghi chú học tập</h3>
            <component :is="resolveIcon('PenLine')" :size="16" />
          </div>
          <textarea rows="7" placeholder="Ghi chú nhanh về bài học này..." />
          <button type="button" class="primary-action compact">
            <component :is="resolveIcon('Save')" :size="13" />
            Lưu ghi chú
          </button>
        </section>
      </aside>
    </main>

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
            <button type="button" class="course-primary-button" @click="confirmEarlyLesson">Tiếp tục học trước</button>
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.course-player-page {
  display: grid;
  gap: var(--section-gap);
  padding-bottom: 1rem;
}

.course-header {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  align-items: start;
  gap: 0.75rem 1rem;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  background: var(--surface-card);
  color: var(--text-heading);
  padding: var(--card-padding-md);
  box-shadow: var(--lg-shadow-sm);
}

.course-eyebrow,
.lesson-meta-row,
.chapter-chip,
.section-kicker {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

.course-eyebrow {
  color: var(--text-link);
  font-size: 0.72rem;
  font-weight: 700;
}

.course-header h1,
.lesson-heading h2,
.lesson-body h3,
.side-heading h3 {
  margin: 0;
  color: inherit;
}

.course-header h1 {
  margin-top: 0.25rem;
  font-size: clamp(1.25rem, 2vw, 1.5rem);
  font-weight: 700;
  line-height: 1.2;
}

.course-header p {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  margin: 0.35rem 0 0;
  color: var(--text-label);
  font-size: 0.82rem;
  font-weight: 600;
}

.course-header-actions {
  display: flex;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 0.45rem;
}

.course-mini-stats {
  grid-column: 2;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.45rem;
}

.mini-stat {
  display: grid;
  gap: 0.1rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.mini-stat span {
  color: var(--text-muted);
  font-size: 0.66rem;
  font-weight: 700;
}

.mini-stat strong {
  color: var(--text-heading);
  font-size: 0.92rem;
  font-weight: 700;
}

.learning-shell {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
  gap: 1rem;
  align-items: start;
}

.lesson-column {
  display: grid;
  gap: 0.85rem;
  min-width: 0;
}

.lesson-panel,
.lesson-body,
.course-side,
.outline-panel,
.side-card {
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
}

.lesson-panel,
.lesson-body,
.outline-panel,
.side-card {
  border-radius: 20px;
}

.lesson-panel {
  overflow: hidden;
}

.lesson-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.8rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 0.85rem 0.9rem 0.7rem;
}

.lesson-title-block {
  min-width: 0;
}

.chapter-chip {
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.22rem 0.55rem;
  font-size: 0.68rem;
  font-weight: 850;
}

.lesson-heading h2 {
  margin-top: 0.5rem;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 700;
  line-height: 1.25;
}

.lesson-meta-row {
  flex-wrap: wrap;
  margin-top: 0.45rem;
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 600;
}

.lesson-meta-row span {
  display: inline-flex;
  align-items: center;
  gap: 0.28rem;
}

.lesson-tabs,
.side-tabs {
  display: flex;
  gap: 0.35rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.45rem;
}

.lesson-tabs button,
.side-tabs button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  min-height: 2rem;
  border: 0;
  border-radius: 11px;
  background: transparent;
  color: var(--text-label);
  cursor: pointer;
  padding: 0 0.7rem;
  font-size: 0.76rem;
  font-weight: 700;
}

.lesson-tabs button.active,
.side-tabs button.active {
  background: var(--surface-card-strong);
  color: var(--text-link);
  box-shadow: 0 0 0 1px var(--border-card);
}

.lesson-content {
  padding: 0.75rem;
}

.document-viewer,
.quiz-view,
.discussion-view {
  display: grid;
  gap: 0.75rem;
}

.document-preview {
  display: grid;
  place-items: center;
  gap: 0.35rem;
  min-height: 14rem;
  border: 1px dashed var(--border-default);
  border-radius: 16px;
  background: var(--surface-input);
  color: var(--text-label);
  text-align: center;
}

.document-preview strong {
  color: var(--text-heading);
  font-size: 0.92rem;
}

.document-preview span {
  color: var(--text-placeholder);
  font-size: 0.78rem;
  font-weight: 700;
}

.quiz-card,
.comment-card,
.comment-composer {
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-input);
  padding: 0.75rem;
}

.quiz-card p,
.comment-card p,
.lesson-body p {
  margin: 0;
  color: var(--text-body);
  font-size: 0.84rem;
  line-height: 1.55;
}

.quiz-card p {
  color: var(--text-heading);
  font-weight: 850;
}

.quiz-options {
  display: grid;
  gap: 0.45rem;
  margin-top: 0.65rem;
}

.quiz-options button {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-card);
  color: var(--text-label);
  cursor: pointer;
  padding: 0.55rem;
  text-align: left;
  font-size: 0.8rem;
  font-weight: 750;
}

.quiz-options button.selected {
  border-color: var(--border-input-focus);
  background: var(--color-info-bg);
  color: var(--text-heading);
}

.quiz-options span {
  display: grid;
  place-items: center;
  width: 1.35rem;
  height: 1.35rem;
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.68rem;
  font-weight: 700;
}

.comment-composer,
.comment-card {
  display: flex;
  gap: 0.7rem;
}

.comment-composer > div:last-child,
.comment-body {
  flex: 1;
  min-width: 0;
}

textarea {
  width: 100%;
  resize: none;
  border: 1px solid var(--border-input);
  border-radius: 13px;
  background: var(--surface-input);
  color: var(--text-heading);
  outline: 0;
  padding: 0.65rem;
  font-size: 0.82rem;
  font-weight: 650;
}

textarea::placeholder {
  color: var(--text-placeholder);
}

.avatar {
  display: grid;
  place-items: center;
  width: 2rem;
  height: 2rem;
  flex-shrink: 0;
  border-radius: 999px;
  background: linear-gradient(135deg, var(--lg-primary), var(--lg-cyan));
  color: var(--text-inverse);
  font-size: 0.7rem;
  font-weight: 900;
}

.comment-author {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.2rem;
}

.comment-author strong {
  color: var(--text-heading);
  font-size: 0.8rem;
}

.comment-author span {
  color: var(--text-placeholder);
  font-size: 0.68rem;
  font-weight: 750;
}

.comment-body button {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.45rem;
  border: 0;
  background: transparent;
  color: var(--text-placeholder);
  cursor: pointer;
  padding: 0;
  font-size: 0.72rem;
  font-weight: 850;
}

.comment-body button.liked {
  color: var(--text-link);
}

.lesson-body {
  display: grid;
  gap: 0.75rem;
  padding: 0.85rem 0.9rem;
}

.section-kicker {
  color: var(--text-link);
  font-size: 0.68rem;
  font-weight: 700;
}

.lesson-body h3 {
  margin-top: 0.3rem;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 700;
}

.completion-callout,
.next-lock-copy {
  display: inline-flex;
  align-items: flex-start;
  gap: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 0.55rem 0.65rem;
  font-size: 0.78rem;
  font-weight: 800;
  line-height: 1.4;
}

.lesson-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.6rem;
  border: 1px solid var(--border-card);
  border-radius: 18px;
  background: var(--surface-card);
  padding: 0.6rem;
}

.next-lock-copy {
  max-width: 24rem;
  margin-left: auto;
}

.course-side {
  position: sticky;
  top: 0.75rem;
  display: grid;
  gap: 0.75rem;
  overflow: visible;
  border: 0;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
  backdrop-filter: none;
}

.side-tabs {
  border-bottom-color: var(--border-card);
}

.outline-panel,
.side-card {
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
}

.side-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.75rem 0.8rem 0.55rem;
}

.side-heading h3 {
  color: var(--text-heading);
  font-size: 0.92rem;
  font-weight: 700;
}

.side-heading span,
.side-heading svg {
  color: var(--text-placeholder);
  font-size: 0.75rem;
  font-weight: 800;
}

.chapter-list {
  max-height: min(61vh, 38rem);
  overflow: auto;
  padding: 0 0.5rem 0.6rem;
}

.chapter-block {
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
}

.chapter-block + .chapter-block {
  margin-top: 0.5rem;
}

.chapter-header {
  display: flex;
  width: 100%;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  border: 0;
  background: transparent;
  color: var(--text-heading);
  cursor: pointer;
  padding: 0.6rem 0.65rem;
  text-align: left;
}

.chapter-header strong,
.chapter-header span {
  display: block;
}

.chapter-header strong {
  color: var(--text-link);
  font-size: 0.68rem;
  font-weight: 700;
}

.chapter-header span {
  margin-top: 0.15rem;
  color: var(--text-heading);
  font-size: 0.8rem;
  font-weight: 700;
  line-height: 1.3;
}

.lesson-list {
  display: grid;
  gap: 0.35rem;
  border-top: 1px solid var(--border-card);
  padding: 0.4rem;
}

.outline-lesson {
  display: flex;
  align-items: flex-start;
  gap: 0.45rem;
  width: 100%;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  background: transparent;
  color: var(--text-label);
  cursor: pointer;
  padding: 0.45rem;
  text-align: left;
}

.outline-lesson:hover,
.outline-lesson.active {
  border-color: var(--border-input-focus);
  background: var(--color-info-bg);
}

.outline-lesson.locked {
  cursor: not-allowed;
  opacity: 0.72;
}

.outline-lesson svg {
  flex-shrink: 0;
  margin-top: 0.1rem;
  color: var(--text-link);
}

.outline-lesson.locked svg {
  color: var(--text-placeholder);
}

.outline-lesson-main {
  flex: 1;
  min-width: 0;
}

.outline-lesson-main strong {
  display: block;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.77rem;
  font-weight: 700;
  line-height: 1.28;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.outline-lesson-main small {
  display: block;
  overflow: hidden;
  margin-top: 0.15rem;
  color: var(--text-placeholder);
  font-size: 0.66rem;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.outline-progress {
  display: block;
  height: 0.25rem;
  overflow: hidden;
  margin-top: 0.35rem;
  border-radius: 999px;
  background: color-mix(in srgb, var(--surface-card-strong) 78%, transparent);
}

.outline-progress span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--lg-primary), var(--lg-cyan));
}

.side-card {
  padding: 0 0.8rem 0.8rem;
}

.ai-card ul {
  display: grid;
  gap: 0.5rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.ai-card li {
  position: relative;
  padding-left: 0.8rem;
  color: var(--text-label);
  font-size: 0.8rem;
  font-weight: 700;
  line-height: 1.45;
}

.ai-card li::before {
  position: absolute;
  top: 0.55rem;
  left: 0;
  width: 0.35rem;
  height: 0.35rem;
  border-radius: 999px;
  background: var(--lg-accent);
  content: "";
}

.notes-card textarea {
  min-height: 11rem;
}

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

.learning-access-badge.mini {
  display: none;
  padding-inline: 0.4rem;
  font-size: 0.58rem;
}

.access-official { color: var(--color-success-text); background: var(--color-success-bg); }
.access-early { color: var(--accent-violet); background: var(--accent-violet-soft); }
.access-early-done { color: var(--accent-violet); background: var(--accent-violet-soft); }
.access-locked { color: var(--color-warning-text); background: var(--color-warning-bg); }
.access-future { color: var(--text-placeholder); background: var(--surface-input); }
.access-completed { color: var(--text-link); background: color-mix(in srgb, var(--color-info-bg) 72%, transparent); }

.ghost-action,
.primary-action,
.secondary-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.38rem;
  min-height: 2.15rem;
  border-radius: 12px;
  cursor: pointer;
  padding: 0 0.75rem;
  font-size: 0.78rem;
  font-weight: 850;
  text-decoration: none;
  transition: transform 160ms ease, opacity 160ms ease;
}

.ghost-action {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.primary-action {
  border: 0;
  background: var(--accent-primary);
  color: var(--text-inverse);
  box-shadow: var(--lg-shadow-sm);
}

.course-header .primary-action {
  background: var(--accent-primary);
  color: var(--text-inverse);
  box-shadow: var(--lg-shadow-sm);
}

.secondary-action {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.primary-action.compact,
.secondary-action.compact {
  min-height: 1.95rem;
  padding-inline: 0.65rem;
  font-size: 0.72rem;
}

.secondary-action.full {
  width: 100%;
  margin-top: 0.8rem;
}

.primary-action:disabled,
.secondary-action:disabled {
  cursor: not-allowed;
  opacity: 0.52;
  transform: none;
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
  background: color-mix(in srgb, var(--text-heading) 44%, transparent);
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
  color: var(--accent-violet);
  background: var(--accent-violet-soft);
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
  background: var(--accent-primary);
  color: var(--text-inverse);
  box-shadow: var(--lg-shadow-sm);
}

@media (max-width: 1180px) {
  .learning-shell {
    grid-template-columns: 1fr;
  }

  .course-side {
    position: static;
  }

  .chapter-list {
    max-height: none;
  }
}

@media (max-width: 760px) {
  .course-header {
    grid-template-columns: 1fr;
  }

  .course-mini-stats {
    grid-column: 1;
  }

  .course-header-actions,
  .lesson-nav {
    flex-wrap: wrap;
  }

  .course-mini-stats {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .lesson-heading {
    flex-direction: column;
  }

  .lesson-tabs {
    overflow-x: auto;
  }

  .next-lock-copy {
    order: 3;
    width: 100%;
    max-width: none;
    margin-left: 0;
  }
}

@media (max-width: 520px) {
  .course-mini-stats {
    grid-template-columns: 1fr;
  }

  .course-header-actions > *,
  .lesson-nav > button,
  .course-modal-actions > * {
    width: 100%;
  }

  .outline-lesson {
    flex-wrap: wrap;
  }

  .outline-lesson-main {
    flex-basis: calc(100% - 2rem);
  }
}
</style>
