<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'
import LessonVideoPlayer from '@/components/learning/LessonVideoPlayer.vue'
import SlideHtmlPreview from '@/components/content-council/editor/content/SlideHtmlPreview.vue'
import { studentApi } from '@/services/studentApi'
import { examApi } from '@/services/examApi'
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
const apiQuizData = ref(null)  // object: { title, durationMinutes, passScore, totalScore, questions[] }
const quizAttempt = ref(null)
const quizHistory = ref(null)
const quizLoading = ref(false)
const quizSubmitting = ref(false)
const quizError = ref('')
let lessonLoadVersion = 0

function mapCourseLessons(rawLessons) {
  if (!rawLessons) return null
  return rawLessons.map(c => ({
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
      progressPercent: l.progressPercent ?? l.ProgressPercent ?? (l.status === 'completed' ? 100 : 0),
      type: l.type || l.Type,
      url: l.url || l.Url,
      allowSeek: l.allowSeek !== undefined ? l.allowSeek : (l.AllowSeek !== undefined ? l.AllowSeek : true),
    })),
  }))
}

// ── DYNAMIC COURSE LOAD LOGIC & REAL-TIME SYNC ──────────────
const route = useRoute()
const courseId = computed(() => route.params.courseId)

const apiCourse = ref(null)
const apiStats = ref(null)
const apiLessons = ref(null)
const isLoadingApi = ref(true)

async function fetchCourseDetail(silent = false) {
  const cId = courseId.value
  if (!cId) return
  try {
    if (!silent) isLoadingApi.value = true
    const res = await studentApi.getCourseDetail(cId)
    const isSuccess = res.success === true || res.Success === true
    if (isSuccess) {
      const data = res.data || res.Data || {}
      apiCourse.value = data.course || data.Course || null
      
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
        const mapped = mapCourseLessons(rawLessons)
        apiLessons.value = mapped
        
        // Đồng bộ thuộc tính allowSeek của bài học hiện tại nếu đang mở
        if (currentLesson.value && selectedLessonId.value && mapped) {
          const currentInMapped = mapped.flatMap(ch => ch.lessons).find(l => l.id === selectedLessonId.value)
          if (currentInMapped) {
            const isSeekAllowed = currentInMapped.allowSeek === false || currentInMapped.AllowSeek === false ? false : true
            if (currentLesson.value.allowSeek !== isSeekAllowed) {
              currentLesson.value.allowSeek = isSeekAllowed
            }
          }
        }
      } else {
        apiLessons.value = null
      }
    }
  } catch (err) {
    if (!silent) {
      console.error('Failed to load course details from API', err)
      apiCourse.value = null
      apiStats.value = null
      apiLessons.value = null
    }
  } finally {
    if (!silent) isLoadingApi.value = false
  }
}

watch(() => courseId.value, () => {
  fetchCourseDetail(false)
}, { immediate: true })

let syncBroadcastChannel = null
let syncPollInterval = null

function applyRealtimeSeekSync(eventData) {
  if (!eventData) return
  const eventCourse = String(eventData.courseCode || '').toUpperCase()
  const currentCourse = String(courseId.value || '').toUpperCase()
  if (eventCourse && currentCourse && eventCourse !== currentCourse) return

  const targetSeek = eventData.allowSeek

  // 1. Cập nhật ngay lập tức bài học đang phát hiện tại
  if (currentLesson.value) {
    if (!eventData.lessonId || String(currentLesson.value.id) === String(eventData.lessonId) || String(currentLesson.value.id) === `l${eventData.lessonId}`) {
      currentLesson.value.allowSeek = targetSeek
    }
  }

  // 2. Cập nhật tất cả các bài học trong danh sách chương trình
  if (apiLessons.value) {
    apiLessons.value.forEach(ch => {
      ch.lessons.forEach(l => {
        if (!eventData.lessonId || String(l.id) === String(eventData.lessonId) || String(l.id) === `l${eventData.lessonId}`) {
          l.allowSeek = targetSeek
        }
      })
    })
  }

  // 3. Tải lại ngầm để đảm bảo đồng bộ hoàn hảo với cơ sở dữ liệu
  fetchCourseDetail(true)
}

function handleStorageSync(e) {
  if (e.key === 'lms_seek_sync_event' && e.newValue) {
    try {
      const data = JSON.parse(e.newValue)
      applyRealtimeSeekSync(data)
    } catch (err) {}
  }
}

function handleVisibilitySync() {
  if (document.visibilityState === 'visible') {
    fetchCourseDetail(true)
  }
}

onMounted(() => {
  try {
    syncBroadcastChannel = new BroadcastChannel('lms_seek_sync')
    syncBroadcastChannel.onmessage = (event) => {
      applyRealtimeSeekSync(event.data)
    }
  } catch (e) {}

  window.addEventListener('storage', handleStorageSync)
  document.addEventListener('visibilitychange', handleVisibilitySync)
  window.addEventListener('focus', handleVisibilitySync)

  // Polling nhẹ nhàng mỗi 3.5 giây để luôn bắt kịp thay đổi từ Giảng viên
  syncPollInterval = setInterval(() => {
    fetchCourseDetail(true)
  }, 3500)
})

onBeforeUnmount(() => {
  if (syncBroadcastChannel) {
    syncBroadcastChannel.close()
    syncBroadcastChannel = null
  }
  if (syncPollInterval) {
    clearInterval(syncPollInterval)
    syncPollInterval = null
  }
  window.removeEventListener('storage', handleStorageSync)
  document.removeEventListener('visibilitychange', handleVisibilitySync)
  window.removeEventListener('focus', handleVisibilitySync)
})

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

const quizQuestions = computed(() => (apiQuizData.value?.questions && apiQuizData.value.questions.length > 0) ? apiQuizData.value.questions : [])
const quizInfo = computed(() => apiQuizData.value || null)

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

function getCleanChapterTitle(chapter) {
  if (!chapter) return ''
  const title = typeof chapter === 'string' ? chapter : (chapter.title || '')
  return title.replace(/^(Chương|Phần|Bài)\s*\d+\s*[:-]\s*/i, '').trim() || title
}

function formatChapterHeading(chapter) {
  if (!chapter) return ''
  const num = chapter.chapter || ''
  const cleanTitle = getCleanChapterTitle(chapter)
  if (!num) return cleanTitle
  if (!cleanTitle) return num
  return `${num}: ${cleanTitle}`
}

function formatLessonDuration(lesson) {
  if (!lesson) return '15:00'
  if (lesson.duration && lesson.duration !== '–' && lesson.duration !== '-' && lesson.duration !== '0:00') {
    return lesson.duration
  }
  if (lesson.durationSeconds && lesson.durationSeconds > 0) {
    const mins = Math.floor(lesson.durationSeconds / 60)
    const secs = lesson.durationSeconds % 60
    return `${mins}:${String(secs).padStart(2, '0')}`
  }
  return '15:00'
}

function getStoredLessonProgress(lessonId) {
  if (!lessonId) return null
  if (lessonProgressDrafts.value[lessonId]) return lessonProgressDrafts.value[lessonId]
  try {
    const raw = localStorage.getItem(`lms_lesson_progress_${lessonId}`)
    if (raw) {
      const parsed = JSON.parse(raw)
      lessonProgressDrafts.value[lessonId] = parsed
      return parsed
    }
  } catch (e) {}
  return null
}

function activateLesson(chapter, lesson) {
  if (!canStartLearning(lesson) && lesson.accessStatus !== LEARNING_ACCESS.COMPLETED && lesson.accessStatus !== LEARNING_ACCESS.EARLY_COMPLETED) return
  expandedChapters.value[chapter.id] = true

  if (selectedLessonId.value === lesson.id && currentLesson.value?.id === lesson.id && currentLesson.value?.hasVideo !== undefined) {
    return
  }

  selectedLessonId.value = lesson.id
  const storedProg = getStoredLessonProgress(lesson.id)
  const initialProgress = storedProg?.progressPercent ?? (lesson.status === 'completed' ? 100 : (lesson.progressPercent || 0))
  const vUrl = lesson.url || lesson.videoUrl || lesson.UrlTapTin || ''
  const isVideoLesson = lesson.type === 'video' || lesson.lessonType === 'video' || Boolean(vUrl) || (!lesson.type && !lesson.lessonType)

  const isSeekAllowed = lesson.allowSeek === false || lesson.AllowSeek === false ? false : true
  currentLesson.value = {
    ...lesson,
    ...storedProg,
    hasVideo: isVideoLesson,
    hasDoc: lesson.type === 'document' || lesson.lessonType === 'assignment',
    hasQuiz: lesson.type === 'quiz' || lesson.lessonType === 'quiz',
    allowSeek: isSeekAllowed,
    pauseOnBlur: lesson.pauseOnBlur !== undefined ? lesson.pauseOnBlur : true,
    minWatchPercentToComplete: lesson.minWatchPercentToComplete || 80,
    progressPercent: initialProgress,
    videoUrl: vUrl,  // map url -> videoUrl cho LessonVideoPlayer
    id: lesson.id,
    chapterId: chapter.id,
    chapterTitle: formatChapterHeading(chapter),
    title: lesson.title,
    duration: formatLessonDuration(lesson),
    durationSeconds: parseDurationSeconds(lesson.duration) || lesson.durationSeconds || 0,
  }
  activeTab.value = lesson.lessonType === 'quiz' ? 'quiz' : lesson.lessonType === 'assignment' ? 'document' : 'video'
}

function parseDurationSeconds(duration) {
  if (!duration || !String(duration).includes(':')) return 0
  const [minutes, seconds] = String(duration).split(':').map(Number)
  return (minutes * 60) + (seconds || 0)
}

let lastApiProgressSavedAt = 0
async function handleVideoProgress(payload) {
  if (!payload) return
  const lessonId = payload.lessonId || currentLesson.value?.id
  const draft = {
    watchedSeconds: payload.currentTimeSeconds,
    maxWatchedSeconds: payload.maxWatchedSeconds,
    progressPercent: payload.progressPercent,
    completedAt: payload.completed ? new Date().toISOString() : null,
  }
  lessonProgressDrafts.value[lessonId] = draft
  try {
    localStorage.setItem(`lms_lesson_progress_${lessonId}`, JSON.stringify(draft))
  } catch (e) {}

  if (lessonId === currentLesson.value?.id) {
    currentLesson.value = {
      ...currentLesson.value,
      ...draft,
    }
  }
  if (payload.completed || (payload.progressPercent && payload.progressPercent >= 80)) {
    await handleVideoCompleted(payload)
  } else if (payload.progressPercent && payload.progressPercent > 0 && courseId.value && lessonId) {
    const now = Date.now()
    if (now - lastApiProgressSavedAt > 5000) {
      lastApiProgressSavedAt = now
      try {
        await studentApi.completeLesson(courseId.value, lessonId, payload.progressPercent)
      } catch (e) {}
    }
  }
}

const lessonCompletedItems = ref({})

const slideSeconds = ref(0)
let slideTimer = null

function startSlideTimer() {
  if (slideTimer) clearInterval(slideTimer)
  slideTimer = setInterval(() => {
    if (activeTab.value === 'slide' && currentLesson.value?.hasSlide) {
      slideSeconds.value += 1
      if (slideSeconds.value >= 60) {
        handleSlideCompleted('timer')
      }
    }
  }, 1000)
}

function handleSlideScroll(event) {
  const target = event?.target
  if (!target) return
  const isAtBottom = target.scrollHeight - target.scrollTop - target.clientHeight < 40
  if (isAtBottom) {
    handleSlideCompleted('scroll')
  }
}

async function handleSlideCompleted(reason = 'scroll') {
  if (!currentLesson.value) return
  const lessonId = currentLesson.value.id
  const items = getLessonCompletedItems(lessonId)
  if (!items.slide) {
    await updateLessonProgressAndSave('slide')
  }
}

watch(() => activeTab.value, (newTab) => {
  if (newTab === 'slide') {
    slideSeconds.value = 0
    startSlideTimer()
  } else {
    if (slideTimer) {
      clearInterval(slideTimer)
      slideTimer = null
    }
  }
})

function getLessonCompletedItems(lessonId) {
  if (!lessonId) return { video: false, slide: false, doc: false, quiz: false }
  if (lessonCompletedItems.value[lessonId]) {
    return lessonCompletedItems.value[lessonId]
  }

  const storageKey = `lms_lesson_items_${lessonId}`
  try {
    const saved = localStorage.getItem(storageKey)
    if (saved) {
      const parsed = JSON.parse(saved)
      lessonCompletedItems.value[lessonId] = parsed
      return parsed
    }
  } catch (e) {}

  const l = currentLesson.value
  const hasVideo = Boolean(l?.hasVideo || (l?.videoUrl && l?.videoUrl.trim() !== '') || l?.lessonType === 'video')
  const hasSlide = Boolean(l?.hasSlide || l?.slideHtml)
  const hasDoc = Boolean(l?.hasDoc || (l?.documentUrl && l?.documentUrl.trim() !== '') || l?.lessonType === 'assignment')
  const hasQuiz = Boolean(l?.hasQuiz || (apiQuizData.value?.questions?.length > 0) || l?.lessonType === 'quiz')

  const existingP = l?.progressPercent ?? (l?.status === 'completed' ? 100 : 0)
  const items = { video: false, slide: false, doc: false, quiz: false }
  if (existingP >= 100) {
    if (hasVideo) items.video = true
    if (hasSlide) items.slide = true
    if (hasDoc) items.doc = true
  }

  lessonCompletedItems.value[lessonId] = items
  return items
}

function saveLessonCompletedItems(lessonId, items) {
  if (!lessonId) return
  lessonCompletedItems.value[lessonId] = { ...items }
  const storageKey = `lms_lesson_items_${lessonId}`
  try {
    localStorage.setItem(storageKey, JSON.stringify(items))
  } catch (e) {}
}

function calculateLessonProgress(lessonId) {
  const items = getLessonCompletedItems(lessonId)
  const l = currentLesson.value
  const hasVideo = Boolean(l?.hasVideo || (l?.videoUrl && l?.videoUrl.trim() !== '') || l?.lessonType === 'video')
  const hasSlide = Boolean(l?.hasSlide || l?.slideHtml)
  const hasDoc = Boolean(l?.hasDoc || (l?.documentUrl && l?.documentUrl.trim() !== '') || l?.lessonType === 'assignment')
  const hasQuiz = Boolean(l?.hasQuiz || (apiQuizData.value?.questions?.length > 0) || l?.lessonType === 'quiz')

  let totalAvailable = 0
  let totalDone = 0

  if (hasVideo) {
    totalAvailable += 1
    if (items.video) totalDone += 1
  }
  if (hasSlide) {
    totalAvailable += 1
    if (items.slide) totalDone += 1
  }
  if (hasDoc) {
    totalAvailable += 1
    if (items.doc) totalDone += 1
  }
  if (hasQuiz) {
    totalAvailable += 1
    if (isQuizCompletedFromDb.value) totalDone += 1
  }

  if (totalAvailable === 0) return 100
  return Math.min(100, Math.round((totalDone / totalAvailable) * 100))
}

async function updateLessonProgressAndSave(itemType) {
  if (!currentLesson.value) return
  const lessonId = currentLesson.value.id
  const items = getLessonCompletedItems(lessonId)
  items[itemType] = true
  saveLessonCompletedItems(lessonId, items)

  const percent = calculateLessonProgress(lessonId)
  currentLesson.value.progressPercent = percent
  if (percent >= 100) {
    currentLesson.value.status = 'completed'
  }
  lessonProgressDrafts.value[lessonId] = {
    progressPercent: percent,
    completedAt: percent >= 100 ? new Date().toISOString() : null
  }

  if (courseId.value && lessonId) {
    try {
      await studentApi.completeLesson(courseId.value, lessonId, percent)
      await reloadCourseData()
    } catch (err) {
      console.error('Không thể lưu tiến độ bài học:', err)
    }
  }
}

async function handleVideoCompleted(payload) {
  const lessonId = payload?.lessonId || currentLesson.value?.id
  if (lessonId) {
    await updateLessonProgressAndSave('video')
  }
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
  const chapter = learningLessons.value?.find(c => c.id === id)
  if (!chapter?.lessons || chapter.lessons.length === 0) return
  expandedChapters.value[id] = !expandedChapters.value[id]
}

function isQuestionLocked(index) {
  if (index === 0) return false
  for (let i = 0; i < index; i++) {
    const prevQ = quizQuestions.value[i]
    const prevQId = prevQ?.id || prevQ?.Id
    const ans = quizAnswers.value[prevQId]
    const qType = prevQ?.type || prevQ?.Type
    if (qType === 'essay') {
      // Tự luận: phải có nội dung
      if (!ans || String(ans).trim() === '') return true
    } else if (qType === 'multiple') {
      // Chọn nhiều: phải chọn ít nhất 1 đáp án
      if (!Array.isArray(ans) || ans.length === 0) return true
    } else {
      if (ans === undefined || ans === null) return true
    }
  }
  return false
}

const isQuizFullyAnswered = computed(() => {
  return quizQuestions.value.every(q => {
    const qId = q.id || q.Id
    const ans = quizAnswers.value[qId]
    const qType = q.type || q.Type
    if (qType === 'essay') return Boolean(ans && String(ans).trim().length > 0)
    if (qType === 'multiple') return Array.isArray(ans) && ans.length > 0
    return ans !== undefined && ans !== null
  })
})

const quizResult = ref(null)

async function reloadCourseData() {
  if (!courseId.value) return
  try {
    const res = await studentApi.getCourseDetail(courseId.value)
    const isSuccess = res.success === true || res.Success === true
    if (isSuccess) {
      const data = res.data || res.Data || {}
      apiCourse.value = data.course || data.Course || null

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
      }

      const rawLessons = data.lessons || data.Lessons || null
      if (rawLessons) {
        apiLessons.value = mapCourseLessons(rawLessons)
      }
    }
  } catch (err) {
    console.error('Failed to reload course data', err)
  }
}

async function submitQuiz() {
  if (!quizAttempt.value || quizSubmitting.value) return

  quizSubmitting.value = true
  quizError.value = ''
  try {
    const answers = quizQuestions.value.map(q => {
      const answer = quizAnswers.value[q.id]
      const options = q.options || []
      const selectedIndexes = Array.isArray(answer) ? answer : (Number.isInteger(answer) ? [answer] : [])
      return {
        maCauHoi: Number(q.id),
        selectedOptionIds: selectedIndexes.map(index => {
          const option = options[index]
          return String(option?.id ?? option?.Id ?? String.fromCharCode(65 + index))
        }),
        essayText: q.type === 'essay' ? String(answer || '') : null,
      }
    })

    const response = await examApi.submitQuizAttempt(quizAttempt.value.maPhienThi, { answers })
    const score = response.diemCuoiCung ?? response.diemTuDong ?? 0
    quizResult.value = {
      total: response.tongSoCau ?? quizQuestions.value.length,
      correctCount: response.soCauDung ?? 0,
      score10: score,
      passScore: quizInfo.value?.passScore || 5,
      isPassed: response.ketQuaDat === true,
      details: response.chiTiet || {},
    }
    quizSubmitted.value = true
    quizHistory.value = await examApi.getQuizHistory(apiQuizData.value.quizId)

    if (quizResult.value.isPassed && currentLesson.value) {
      await updateLessonProgressAndSave('quiz')
    }
  } catch (err) {
    quizError.value = err?.message || 'Không thể nộp bài Quiz. Vui lòng thử lại.'
  } finally {
    quizSubmitting.value = false
  }
}

function mapAttemptQuestions(startResponse) {
  const rawQuestions = startResponse?.cauHoi || startResponse?.CauHoi || []
  return rawQuestions.map(q => {
    const questionType = q.loaiCauHoi || q.LoaiCauHoi
    const selectionType = q.kieuLuaChon || q.KieuLuaChon
    return {
      id: q.maCauHoi || q.MaCauHoi,
      text: q.noiDung || q.NoiDung,
      type: questionType === 'tu_luan'
        ? 'essay'
        : ((selectionType === 'chon_nhieu' || selectionType === 'multiple') ? 'multiple' : 'single'),
      options: q.luaChon || q.LuaChon || [],
    }
  })
}

async function startQuizAttempt(quizId, expectedLessonId = currentLesson.value?.id) {
  const isStale = () => currentLesson.value?.id !== expectedLessonId
  quizLoading.value = true
  quizError.value = ''
  try {
    await examApi.getQuizAvailability(quizId)
    if (isStale()) return
    quizHistory.value = await examApi.getQuizHistory(quizId)
    if (isStale()) return
    const started = await examApi.startQuizAttempt(quizId)
    if (isStale()) return
    quizAttempt.value = started
    apiQuizData.value = {
      ...apiQuizData.value,
      questions: mapAttemptQuestions(started),
    }
  } catch (err) {
    quizAttempt.value = null
    quizError.value = err?.message || 'Quiz hiện chưa thể bắt đầu.'
  } finally {
    quizLoading.value = false
  }
}

async function retryQuiz() {
  quizAnswers.value = {}
  quizResult.value = null
  quizSubmitted.value = false
  if (apiQuizData.value?.quizId) {
    await startQuizAttempt(apiQuizData.value.quizId)
  }
}

async function openDocument() {
  const url = currentLesson.value?.documentUrl || currentLesson.value?.url || currentLesson.value?.urlTapTin
  if (url) {
    window.open(url, '_blank')
    await updateLessonProgressAndSave('doc')
  } else {
    alert('Chưa có file tài liệu đính kèm cho bài học này.')
  }
}

function isOptionSelected(q, idx) {
  const qId = q.id || q.Id
  const ans = quizAnswers.value[qId]
  const qType = q.type || q.Type
  if (qType === 'multiple') {
    return Array.isArray(ans) && ans.includes(idx)
  }
  return ans === idx
}

function selectAnswer(q, idx) {
  const qId = q.id || q.Id
  const qType = q.type || q.Type
  if (qType === 'multiple') {
    const current = Array.isArray(quizAnswers.value[qId]) ? [...quizAnswers.value[qId]] : []
    const existingIndex = current.indexOf(idx)
    if (existingIndex > -1) {
      current.splice(existingIndex, 1)
    } else {
      current.push(idx)
    }
    quizAnswers.value[qId] = current
  } else {
    quizAnswers.value[qId] = idx
  }
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
  if (!lesson) return '0%'
  const p = lessonProgressDrafts.value[lesson.id]?.progressPercent ?? lesson.progressPercent ?? (lesson.status === 'completed' ? 100 : 0)
  return `${Math.max(0, Math.min(100, p))}%`
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
    apiQuizData.value = null
    quizAttempt.value = null
    quizHistory.value = null
    quizError.value = ''
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
        
        const storedProg = getStoredLessonProgress(foundLesson.id)
        const initialProg = storedProg?.progressPercent ?? (foundLesson.status === 'completed' ? 100 : (foundLesson.progressPercent || 0))
        const fvUrl = foundLesson.url || foundLesson.videoUrl || ''
        const isSeekAllowed = foundLesson.allowSeek === false || foundLesson.AllowSeek === false ? false : true
        currentLesson.value = {
          ...foundLesson,
          ...storedProg,
          hasVideo: Boolean(fvUrl || foundLesson.type === 'video' || foundLesson.lessonType === 'video' || (!foundLesson.type && !foundLesson.lessonType)),
          hasDoc: foundLesson.type === 'document' || foundLesson.lessonType === 'assignment',
          hasQuiz: foundLesson.type === 'quiz' || foundLesson.lessonType === 'quiz',
          durationSeconds: 1200,
          allowSeek: isSeekAllowed,
          pauseOnBlur: true,
          minWatchPercentToComplete: 80,
          progressPercent: initialProg,
          documentTitle: foundLesson.url ? foundLesson.url.split('/').pop() : `${foundLesson.title}.pdf`,
          documentPages: 10,
          documentCurrentPage: 1,
          videoUrl: fvUrl,    // map url -> videoUrl cho LessonVideoPlayer
          id: foundLesson.id,
          chapterId: foundChapter.id,
          chapterTitle: formatChapterHeading(foundChapter),
          title: foundLesson.title,
          duration: formatLessonDuration(foundLesson),
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
      const storedProg = getStoredLessonProgress(firstLesson.id)
      const initialProg = storedProg?.progressPercent ?? (firstLesson.status === 'completed' ? 100 : (firstLesson.progressPercent || 0))
      const fvUrl = firstLesson.url || firstLesson.videoUrl || ''
      const isSeekAllowed = firstLesson.allowSeek === false || firstLesson.AllowSeek === false ? false : true
      currentLesson.value = {
        ...firstLesson,
        ...storedProg,
        hasVideo: Boolean(fvUrl || firstLesson.type === 'video' || firstLesson.lessonType === 'video' || (!firstLesson.type && !firstLesson.lessonType)),
        hasDoc: firstLesson.type === 'document' || firstLesson.lessonType === 'assignment',
        hasQuiz: firstLesson.type === 'quiz' || firstLesson.lessonType === 'quiz',
        durationSeconds: 1200,
        allowSeek: isSeekAllowed,
        pauseOnBlur: true,
        minWatchPercentToComplete: 80,
        progressPercent: initialProg,
        documentTitle: firstLesson.url ? firstLesson.url.split('/').pop() : `${firstLesson.title}.pdf`,
        documentPages: 10,
        documentCurrentPage: 1,
        videoUrl: fvUrl,       // map url -> videoUrl cho LessonVideoPlayer
        id: firstLesson.id,
        chapterId: firstChapter.id,
        chapterTitle: formatChapterHeading(firstChapter),
        title: firstLesson.title,
        duration: formatLessonDuration(firstLesson),
      }
      activeTab.value = firstLesson.lessonType === 'quiz' ? 'quiz' : firstLesson.lessonType === 'assignment' ? 'document' : 'video'
    }
  }
})

watch(() => currentLesson.value?.id, async (newLessonId) => {
  if (newLessonId && courseId.value) {
    const loadVersion = ++lessonLoadVersion
    const isStale = () => loadVersion !== lessonLoadVersion || currentLesson.value?.id !== newLessonId
    // Reset quiz data khi chuyển bài
    apiQuizData.value = null
    quizAnswers.value = {}
    quizSubmitted.value = false
    quizResult.value = null
    quizAttempt.value = null
    quizHistory.value = null
    quizError.value = ''

    // Load content blocks (video, document, slide, quiz) từ API
    try {
      const contentRes = await studentApi.getLessonContent(courseId.value, newLessonId)
      if (isStale()) return
      const blocks = contentRes.data || contentRes.Data || []
      const isArr = Array.isArray(blocks)
      const videoBlock = isArr ? blocks.find(b => (b.Type || b.type) === 'video') : null
      const docBlock = isArr ? blocks.find(b => (b.Type || b.type) === 'tai_lieu' || (b.Type || b.type) === 'pdf' || (b.Type || b.type) === 'document') : null
      const slideBlock = isArr ? blocks.find(b => (b.Type || b.type) === 'slide_html') : null
      const quizBlock = isArr ? blocks.find(b => (b.Type || b.type) === 'quiz' || (b.Type || b.type) === 'trac_nghiem' || b.QuizId || b.quizId) : null

      const baseVideoUrl = currentLesson.value?.videoUrl || currentLesson.value?.url || ''
      const finalVideoUrl = videoBlock ? (videoBlock.VideoUrl || videoBlock.videoUrl || videoBlock.UrlTapTin || videoBlock.urlTapTin || baseVideoUrl) : baseVideoUrl
      const hasVid = Boolean(videoBlock || finalVideoUrl)
      const hasSl = Boolean(slideBlock)
      const hasD = Boolean(docBlock)
      const hasQ = Boolean(quizBlock) || currentLesson.value?.lessonType === 'quiz'

      const docUrl = docBlock ? (docBlock.DocumentUrl || docBlock.documentUrl || docBlock.UrlTapTin || docBlock.urlTapTin || '') : ''
      const docTitle = docBlock ? (docBlock.Title || docBlock.title || docUrl.split('/').pop() || currentLesson.value?.documentTitle) : currentLesson.value?.documentTitle
      const rawSlideData = slideBlock ? (slideBlock.NoiDungJson || slideBlock.noiDungJson || slideBlock.SlideHtml || slideBlock.slideHtml || '') : ''

      currentLesson.value = {
        ...currentLesson.value,
        hasVideo: hasVid,
        hasSlide: hasSl,
        hasDoc: hasD,
        hasQuiz: hasQ,
        slideJsonData: rawSlideData,
        videoUrl: finalVideoUrl,
        documentTitle: docTitle || '',
        documentUrl: docUrl || '',
        slideHtml: slideBlock ? (slideBlock.SlideHtml || slideBlock.slideHtml || slideBlock.NoiDungHtml || slideBlock.noiDungHtml || '') : undefined,
      }

      // Auto switch activeTab to the first available content block type
      if (hasVid) {
        activeTab.value = 'video'
      } else if (hasSl) {
        activeTab.value = 'slide'
      } else if (hasD) {
        activeTab.value = 'document'
      } else if (hasQ) {
        activeTab.value = 'quiz'
      }
    } catch (err) {
      console.error('Không thể tải content blocks:', err)
    }

    // Khôi phục trạng thái hoàn thành từng mục từ DB
    const existingP = currentLesson.value?.progressPercent ?? (currentLesson.value?.status === 'completed' ? 100 : 0)
    const items = getLessonCompletedItems(newLessonId)
    const hV = currentLesson.value?.hasVideo
    const hD = currentLesson.value?.hasDoc
    const hQ = currentLesson.value?.hasQuiz

    if (existingP >= 100) {
      if (hV) items.video = true
      if (hD) items.doc = true
    } else {
      if (!hV) items.video = false
      if (!hD) items.doc = false
      if (!hQ) items.quiz = false
    }

    if ((items.video && hV) || existingP > 0) {
      currentLesson.value = {
        ...currentLesson.value,
        progressPercent: existingP,
        watchedSeconds: items.video && hV ? (currentLesson.value.durationSeconds || 1200) : (currentLesson.value.watchedSeconds || 0),
        maxWatchedSeconds: items.video && hV ? (currentLesson.value.durationSeconds || 1200) : (currentLesson.value.maxWatchedSeconds || 0),
      }
    }

    // Load quiz cho bài học (thử gọi API getLessonQuiz cho mọi bài học có quiz)
    try {
      const res = await studentApi.getLessonQuiz(courseId.value, newLessonId)
      if (isStale()) return
      const raw = res.data || res.Data
      const quizId = raw?.quizId || raw?.QuizId
      if (raw && typeof raw === 'object' && !Array.isArray(raw) && quizId) {
        apiQuizData.value = { ...raw, quizId, questions: [] }
        await startQuizAttempt(quizId, newLessonId)
        if (isStale()) return
      } else if (Array.isArray(raw) && raw.length > 0) {
        apiQuizData.value = { questions: raw }
      } else {
        apiQuizData.value = null
      }
    } catch (err) {
      apiQuizData.value = null
    }

    // Load comments
    try {
      const res = await studentApi.getLessonComments(courseId.value, newLessonId)
      if (isStale()) return
      apiComments.value = res.data || res.Data || []
    } catch (err) {
      console.error(err)
      apiComments.value = []
    }
  }
})

const isQuizCompletedFromDb = computed(() => {
  const attempts = quizHistory.value?.lanLam || quizHistory.value?.LanLam || []
  return attempts.some(item => (item.trangThaiLuong || item.TrangThaiLuong) === 'da_dung')
})

async function handleResetCourseProgress() {
  if (!courseId.value) return
  if (!confirm(`Bạn có chắc chắn muốn reset tiến độ môn ${courseId.value} về 0% để test lại không?`)) return
  try {
    await studentApi.resetCourseProgress(courseId.value)
    if (apiLessons.value) {
      apiLessons.value.forEach(ch => {
        (ch.lessons || []).forEach(l => {
          try {
            localStorage.removeItem(`lms_lesson_items_${l.id}`)
          } catch (e) {}
        })
      })
    }
    lessonCompletedItems.value = {}
    lessonProgressDrafts.value = {}
    await reloadCourseData()
    alert(`Đã reset tiến độ môn ${courseId.value} về 0% thành công!`)
  } catch (err) {
    console.error('Không thể reset tiến độ:', err)
    alert('Có lỗi khi reset tiến độ.')
  }
}
</script>

<template>
  <div class="course-player-page" v-if="courseInfo">
    <header class="course-hero-banner">
      <div class="course-hero-top">
        <div class="course-hero-nav">
          <router-link to="/student/courses" class="hero-nav-btn">
            <component :is="resolveIcon('ArrowLeft')" :size="14" />
            <span>Tất cả khóa học</span>
          </router-link>
          <router-link to="/student/assignments" class="hero-nav-btn">
            <component :is="resolveIcon('ClipboardList')" :size="14" />
            <span>Bài tập</span>
          </router-link>
          <button type="button" class="hero-nav-btn hero-reset-btn" @click="handleResetCourseProgress">
            <component :is="resolveIcon('RotateCcw')" :size="13" />
            <span>Reset tiến độ (0%)</span>
          </button>
        </div>

        <div class="course-hero-semester-badge">
          <component :is="resolveIcon('Sparkles')" :size="12" />
          <span>{{ courseInfo?.semester || 'Học kỳ hiện tại' }}</span>
        </div>
      </div>

      <div class="course-hero-main-row">
        <div class="course-hero-info">
          <div class="course-hero-code-chip">
            <component :is="resolveIcon('BookOpenCheck')" :size="13" />
            <span>{{ courseInfo?.code || '—' }}</span>
            <span class="dot-separator">•</span>
            <span>{{ courseInfo?.credits || 3 }} tín chỉ</span>
          </div>
          <h1 class="course-hero-title">{{ courseInfo?.title || 'Chi tiết khóa học' }}</h1>
          <div class="course-hero-teacher">
            <component :is="resolveIcon('UserRound')" :size="14" />
            <span>Giảng viên: <strong>{{ courseInfo?.teacher || 'Chưa phân công' }}</strong></span>
          </div>
        </div>

        <div class="course-hero-stats-grid" aria-label="Tổng quan khóa học">
          <div v-for="stat in miniStats" :key="stat.label" class="hero-stat-card">
            <span class="hero-stat-label">{{ stat.label }}</span>
            <strong class="hero-stat-value">{{ stat.value }}</strong>
          </div>
        </div>
      </div>
    </header>

    <main class="learning-shell">
      <section class="lesson-column">
        <article class="lesson-panel">
          <div class="lesson-heading">
            <div class="lesson-title-block">
              <div class="lesson-badge-row">
                <span class="chapter-chip">
                  <component :is="resolveIcon('Bookmark')" :size="12" />
                  {{ currentLesson.chapterTitle }}
                </span>
                <span :class="['learning-access-badge', accessTone(currentLesson.accessStatus)]">
                  {{ currentLessonStatusLabel }}
                </span>
              </div>
              <h2 class="lesson-main-title">{{ currentLesson.title }}</h2>
              <div class="lesson-meta-pills">
                <span class="meta-pill">
                  <component :is="resolveIcon(typeConfig[currentLesson.lessonType]?.icon || 'PlayCircle')" :size="13" />
                  {{ lessonTypeLabel(currentLesson) }}
                </span>
                <span class="meta-pill">
                  <component :is="resolveIcon('Clock3')" :size="13" />
                  {{ currentLesson.duration }}
                </span>
                <span class="meta-pill">
                  <component :is="resolveIcon('Gauge')" :size="13" />
                  {{ currentLesson.progressPercent || 0 }}%
                </span>
              </div>
            </div>
          </div>

          <div class="lesson-tabs" aria-label="Nội dung bài học">
            <button
              v-for="tab in [
                { key: 'video', label: 'Video', icon: 'PlayCircle', done: Boolean(currentLesson.hasVideo && getLessonCompletedItems(currentLesson.id).video) },
                { key: 'slide', label: 'Slide', icon: 'PlaySquare', done: Boolean(currentLesson.hasSlide && getLessonCompletedItems(currentLesson.id).slide) },
                { key: 'document', label: 'Tài liệu', icon: 'FileText', done: Boolean(currentLesson.hasDoc && getLessonCompletedItems(currentLesson.id).doc) },
                { key: 'quiz', label: 'Quiz', icon: 'ListChecks', done: Boolean(currentLesson.hasQuiz && isQuizCompletedFromDb) },
                { key: 'discussion', label: 'Thảo luận', icon: 'MessagesSquare', done: false },
              ]"
              :key="tab.key"
              type="button"
              :class="{ active: activeTab === tab.key }"
              @click="activeTab = tab.key"
            >
              <component :is="resolveIcon(tab.done ? 'CheckCircle2' : tab.icon)" :size="14" :class="{ 'text-green-600 font-bold': tab.done }" />
              {{ tab.label }}
              <span v-if="tab.done" class="text-[10px] bg-green-100 text-green-700 px-1.5 py-0.2 rounded font-bold ml-1">✓</span>
            </button>
          </div>

          <div class="lesson-content">
            <div v-if="activeTab === 'video'">
              <div v-if="!currentLesson.hasVideo" class="p-8 text-center surface-card border border-card rounded-xl text-slate-500 my-4">
                <component :is="resolveIcon('PlayCircle')" :size="40" class="mx-auto text-slate-300 mb-2" />
                <p class="font-medium text-base text-slate-700">Bài học này không có Video</p>
                <p class="text-xs text-slate-400 mt-1">Vui lòng chọn tab Slide, Quiz hoặc Nội dung khác để tiếp tục học.</p>
              </div>
              <LessonVideoPlayer
                v-else
                :key="`${currentLesson.id}:${currentLesson.videoUrl || ''}`"
                :lesson="currentLesson"
                @progress="handleVideoProgress"
                @completed="handleVideoCompleted"
              />
            </div>

            <!-- Slide Tab -->
            <div v-else-if="activeTab === 'slide'" class="slide-viewer">
              <div v-if="!currentLesson.hasSlide" class="p-8 text-center surface-card border border-card rounded-xl text-slate-500 my-4 w-full">
                <component :is="resolveIcon('PlaySquare')" :size="40" class="mx-auto text-slate-300 mb-2" />
                <p class="font-medium text-base text-slate-700">Bài học này không có Slide HTML</p>
                <p class="text-xs text-slate-400 mt-1">Vui lòng chọn tab Video, Tài liệu hoặc Quiz để xem bài học.</p>
              </div>
              <template v-else>
                <div v-if="getLessonCompletedItems(currentLesson.id).slide" class="p-3 mb-3 rounded-xl bg-green-50 border border-green-200 text-green-800 flex items-center gap-2 text-xs font-bold w-full">
                  <component :is="resolveIcon('CheckCircle2')" :size="16" class="text-green-600 shrink-0" />
                  <span>✓ Bạn đã hoàn thành bài Slide này (Đã đọc 1 phút hoặc cuộn hết trang)</span>
                </div>
                <div v-else class="p-2.5 mb-3 rounded-xl bg-amber-50 border border-amber-200 text-amber-800 flex items-center justify-between text-xs font-medium w-full">
                  <div class="flex items-center gap-2">
                    <component :is="resolveIcon('Clock3')" :size="16" class="text-amber-600 shrink-0" />
                    <span>Xem slide đủ 1 phút ({{ Math.min(60, slideSeconds) }}/60s) hoặc cuộn xuống hết trang để hoàn thành</span>
                  </div>
                  <button type="button" @click="handleSlideCompleted('manual')" class="px-2.5 py-1 text-[11px] font-bold bg-amber-600 hover:bg-amber-700 text-white rounded-lg transition-colors">
                    Đã xem xong Slide
                  </button>
                </div>

                <div 
                  class="bg-white border border-slate-200 rounded-xl p-6 sm:p-8 max-h-[650px] overflow-y-auto shadow-inner"
                  @scroll="handleSlideScroll"
                >
                  <SlideHtmlPreview :jsonData="currentLesson.slideJsonData || '{}'" />
                </div>
              </template>
            </div>

            <div v-else-if="activeTab === 'document'" class="document-viewer">
              <div v-if="!currentLesson.hasDoc" class="p-8 text-center surface-card border border-card rounded-xl text-slate-500 my-4 w-full">
                <component :is="resolveIcon('FileText')" :size="40" class="mx-auto text-slate-300 mb-2" />
                <p class="font-medium text-base text-slate-700">Bài học này không có Tài liệu</p>
                <p class="text-xs text-slate-400 mt-1">Vui lòng chọn tab Slide, Quiz hoặc Nội dung khác để tiếp tục học.</p>
              </div>
              <template v-else>
                <div v-if="getLessonCompletedItems(currentLesson.id).doc" class="p-3 mb-3 rounded-xl bg-green-50 border border-green-200 text-green-800 flex items-center gap-2 text-xs font-bold w-full">
                  <component :is="resolveIcon('CheckCircle2')" :size="16" class="text-green-600 shrink-0" />
                  <span>✓ Bạn đã xem và tải tài liệu này (Đã ghi nhận tiến độ)</span>
                </div>
                <div class="document-preview">
                  <component :is="resolveIcon('FileText')" :size="36" />
                  <strong>{{ currentLesson.documentTitle || 'Tài liệu bài học' }}</strong>
                  <span>Trang {{ currentLesson.documentCurrentPage || 1 }} / {{ currentLesson.documentPages || 1 }}</span>
                </div>
                <button type="button" class="secondary-action" @click="openDocument">
                  <component :is="resolveIcon('ExternalLink')" :size="15" />
                  Mở tài liệu
                </button>
              </template>
            </div>

            <div v-else-if="activeTab === 'quiz'" class="quiz-view">
              <!-- Quiz info header -->
              <div v-if="quizInfo" class="quiz-info-header">
                <div class="quiz-stat">
                  <component :is="resolveIcon('Clock3')" :size="14" />
                  <span>{{ quizInfo.durationMinutes || 15 }} phút</span>
                </div>
                <div class="quiz-stat">
                  <component :is="resolveIcon('HelpCircle')" :size="14" />
                  <span>{{ quizQuestions.length }} câu</span>
                </div>
                <div class="quiz-stat">
                  <component :is="resolveIcon('Target')" :size="14" />
                  <span>Điểm đạt: {{ quizInfo.passScore || 5 }}/{{ quizInfo.totalScore || 10 }}</span>
                </div>
              </div>

              <!-- Previous completion notice banner -->
              <div v-if="isQuizCompletedFromDb && !quizSubmitted" class="quiz-result-banner mb-5 p-4 rounded-xl border bg-green-50 border-green-200 text-green-900 flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div class="p-2.5 rounded-lg shrink-0 bg-green-100 text-green-700">
                    <component :is="resolveIcon('CheckCircle2')" :size="24" />
                  </div>
                  <div>
                    <h3 class="text-base font-bold text-green-900 flex items-center gap-2">
                      ✓ Bạn đã nộp bài Quiz này trước đó
                    </h3>
                    <p class="text-xs mt-0.5 text-green-700">
                      Bạn có thể chọn đáp án và bấm Nộp bài Quiz bên dưới nếu muốn làm lại bài Quiz.
                    </p>
                  </div>
                </div>
              </div>

              <div v-if="quizError" class="mb-4 rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                {{ quizError }}
              </div>

              <!-- Auto grading result banner -->
              <div v-if="quizSubmitted && quizResult" class="quiz-result-banner mb-5 p-4 rounded-xl border flex items-center justify-between"
                :class="quizResult.isPassed ? 'bg-green-50 border-green-200 text-green-900' : 'bg-amber-50 border-amber-200 text-amber-900'">
                <div class="flex items-center gap-3">
                  <div class="p-2.5 rounded-lg shrink-0" :class="quizResult.isPassed ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'">
                    <component :is="resolveIcon(quizResult.isPassed ? 'CheckCircle2' : 'AlertCircle')" :size="24" />
                  </div>
                  <div>
                    <h3 class="text-base font-bold flex items-center gap-2">
                      Kết quả Quiz: <span class="text-lg font-extrabold">{{ quizResult.score10 }} / 10 điểm</span>
                      <span class="text-xs px-2.5 py-0.5 rounded-full font-bold ml-2" :class="quizResult.isPassed ? 'bg-green-200 text-green-800' : 'bg-amber-200 text-amber-800'">
                        {{ quizResult.isPassed ? 'ĐẠT (Hoàn thành)' : 'CHƯA ĐẠT' }}
                      </span>
                    </h3>
                    <p class="text-xs mt-1 text-slate-600">
                      Bạn trả lời đúng <strong>{{ quizResult.correctCount }}/{{ quizResult.total }}</strong> câu &middot; Điểm yêu cầu đạt: {{ quizResult.passScore }}/10 điểm
                    </p>
                  </div>
                </div>
              </div>

              <div
                v-for="(q, index) in quizQuestions"
                :key="q.id || q.Id"
                class="quiz-card"
                :class="{ 'opacity-50 pointer-events-none': isQuestionLocked(index) }"
              >
                <div class="flex items-center justify-between mb-2">
                  <p class="font-medium text-heading flex items-center gap-2">
                    Câu {{ index + 1 }}:
                    <span v-if="(q.type || q.Type) === 'multiple'" class="text-xs font-normal text-slate-500">(Chọn nhiều)</span>
                    {{ q.text || q.Text || q.question || q.Question }}
                  </p>
                  <span v-if="isQuestionLocked(index)" class="text-xs text-(--color-warning-text) font-semibold flex items-center gap-1">
                    <component :is="resolveIcon('Lock')" :size="12" /> Làm câu trước đó
                  </span>
                </div>

                <!-- Câu trắc nghiệm: radio/checkbox buttons -->
                <div class="quiz-options">
                  <button
                    v-for="(opt, idx) in q.options || q.Options"
                    :key="idx"
                    type="button"
                    :disabled="isQuestionLocked(index)"
                    :class="{ selected: isOptionSelected(q, idx) }"
                    @click="selectAnswer(q, idx)"
                  >
                    <span>{{ ['A', 'B', 'C', 'D'][idx] }}</span>
                    {{ typeof opt === 'object' ? (opt?.content || opt?.Content || opt?.text || opt?.Text || JSON.stringify(opt)) : opt }}
                  </button>
                </div>
              </div>
              <div v-if="quizQuestions.length === 0 && quizLoading" class="quiz-empty-state">
                <component :is="resolveIcon('HelpCircle')" :size="28" />
                <p>Đang tải câu hỏi...</p>
              </div>
              <div v-else-if="quizQuestions.length === 0 && apiQuizData && !quizError" class="quiz-empty-state">
                <component :is="resolveIcon('AlertCircle')" :size="28" />
                <p>Bài học này chưa có câu hỏi Quiz.</p>
              </div>
              <button
                v-if="quizQuestions.length > 0 && !quizSubmitted"
                type="button"
                class="primary-action w-full justify-center"
                :disabled="!isQuizFullyAnswered || quizSubmitting || quizLoading || !quizAttempt"
                @click="submitQuiz"
              >
                <component :is="resolveIcon('Send')" :size="15" />
                {{ quizSubmitting ? 'Đang chấm và lưu...' : 'Nộp bài Quiz' }}
              </button>
              <button
                v-else-if="quizSubmitted"
                type="button"
                class="primary-action w-full justify-center"
                :disabled="quizLoading"
                @click="retryQuiz"
              >
                <component :is="resolveIcon('RotateCcw')" :size="15" />
                {{ quizLoading ? 'Đang tạo lượt mới...' : 'Làm lại Quiz' }}
              </button>
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
              Bài học tập trung vào cách sử dụng {{ currentLesson?.title?.toLowerCase() || '' }} trong bài toán thực tế,
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
              <button
                type="button"
                class="chapter-header"
                :class="{ 'cursor-default': !chapter.lessons || chapter.lessons.length === 0 }"
                @click="toggleChapter(chapter.id)"
              >
                <div class="chapter-title-wrap">
                  <span class="chapter-num-badge">{{ chapter.chapter }}</span>
                  <strong class="chapter-main-title">{{ getCleanChapterTitle(chapter) }}</strong>
                </div>
                <component
                  v-if="chapter.lessons && chapter.lessons.length > 0"
                  :is="resolveIcon(expandedChapters[chapter.id] ? 'ChevronUp' : 'ChevronDown')"
                  :size="15"
                />
              </button>

              <div v-if="expandedChapters[chapter.id] && chapter.lessons && chapter.lessons.length > 0" class="lesson-list">
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
                      {{ lessonTypeLabel(lesson) }} · {{ formatLessonDuration(lesson) }}
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

  <div v-else class="flex flex-col items-center justify-center py-20 text-center">
    <p class="text-lg font-semibold text-muted">Không tìm thấy khóa học</p>
    <router-link to="/student/courses" class="mt-4 lg-button-primary px-4 py-2">Quay lại danh sách khóa học</router-link>
  </div>
</template>

<style scoped>
.course-player-page {
  display: grid;
  gap: var(--section-gap);
  padding-bottom: 1rem;
}

.course-hero-banner {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 1.15rem;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 20px;
  background: linear-gradient(135deg, var(--surface-card) 0%, var(--surface-input) 100%);
  color: var(--text-heading);
  padding: 1.25rem 1.5rem;
  box-shadow: var(--lg-shadow-md);
  backdrop-filter: blur(var(--glass-blur)) saturate(140%);
}

.course-hero-banner::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, var(--lg-primary), var(--lg-cyan), var(--lg-secondary));
}

.course-hero-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.course-hero-nav {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.hero-nav-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.4rem 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-card);
  color: var(--text-heading);
  font-size: 0.78rem;
  font-weight: 700;
  text-decoration: none;
  transition: all 0.2s ease;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.hero-nav-btn:hover {
  border-color: var(--border-input-focus);
  background: var(--color-info-bg);
  color: var(--text-link);
  transform: translateY(-1px);
}

.hero-reset-btn {
  border-color: rgba(245, 158, 11, 0.35);
  background: rgba(245, 158, 11, 0.08);
  color: #f59e0b;
}

.hero-reset-btn:hover {
  border-color: #f59e0b;
  background: rgba(245, 158, 11, 0.16);
  color: #d97706;
}

.course-hero-semester-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: 999px;
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  color: var(--text-link);
  font-size: 0.72rem;
  font-weight: 800;
  box-shadow: var(--lg-shadow-sm);
}

.course-hero-main-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.75rem;
  flex-wrap: wrap;
}

.course-hero-info {
  flex: 1 1 340px;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.course-hero-code-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  width: fit-content;
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.02em;
}

.course-hero-code-chip .dot-separator {
  color: var(--text-placeholder);
}

.course-hero-title {
  margin: 0;
  font-size: clamp(1.4rem, 2.2vw, 1.85rem);
  font-weight: 900;
  letter-spacing: -0.02em;
  line-height: 1.25;
  color: var(--text-heading);
}

.course-hero-teacher {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--text-label);
  font-size: 0.82rem;
  font-weight: 600;
}

.course-hero-teacher strong {
  color: var(--text-heading);
  font-weight: 750;
}

.course-hero-stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(85px, 1fr));
  gap: 0.65rem;
  flex-shrink: 0;
}

.hero-stat-card {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.2rem;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--surface-card);
  padding: 0.55rem 0.85rem;
  min-width: 85px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
  transition: border-color 0.2s ease;
}

.hero-stat-card:hover {
  border-color: var(--border-input-focus);
}

.hero-stat-label {
  color: var(--text-placeholder);
  font-size: 0.68rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.hero-stat-value {
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
  line-height: 1.2;
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
  flex-direction: column;
  gap: 0.65rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 1.15rem 1.35rem 0.95rem;
}

.lesson-title-block {
  min-width: 0;
  width: 100%;
}

.lesson-badge-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  width: 100%;
}

.chapter-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.65rem;
  font-size: 0.72rem;
  font-weight: 800;
}

.lesson-main-title {
  margin: 0.45rem 0 0;
  color: var(--text-heading);
  font-size: 1.25rem;
  font-weight: 850;
  line-height: 1.35;
  letter-spacing: -0.01em;
}

.lesson-meta-pills {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  flex-wrap: wrap;
  margin-top: 0.55rem;
}

.meta-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.25rem 0.6rem;
  border-radius: 8px;
  background: var(--surface-input);
  border: 1px solid var(--border-card);
  color: var(--text-label);
  font-size: 0.74rem;
  font-weight: 700;
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
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  width: 100%;
  border: 0;
  background: transparent;
  padding: 0.75rem 0.85rem;
  text-align: left;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.chapter-title-wrap {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}

.chapter-num-badge {
  color: var(--text-link);
  font-size: 0.68rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.chapter-main-title {
  color: var(--text-heading);
  font-size: 0.88rem;
  font-weight: 750;
  line-height: 1.3;
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
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.78rem;
  font-weight: 700;
  line-height: 1.35;
  word-break: break-word;
  white-space: normal;
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

.quiz-info-header {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
  padding: 0.65rem 0.85rem;
  background: var(--surface-input);
  border: 1px solid var(--border-card);
  border-radius: 12px;
  margin-bottom: 1rem;
}

.quiz-stat {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--text-label);
}

.quiz-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem;
  color: var(--text-placeholder);
  font-size: 0.85rem;
  font-weight: 600;
  text-align: center;
  border: 1px dashed var(--border-card);
  border-radius: 12px;
  margin-bottom: 1rem;
}

.quiz-essay-block {
  display: grid;
  gap: 0.35rem;
}

.quiz-essay-textarea {
  width: 100%;
  min-height: 8rem;
  padding: 0.65rem 0.75rem;
  border: 1px solid var(--border-input);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-body);
  font-size: 0.88rem;
  line-height: 1.55;
  resize: vertical;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
  font-family: inherit;
}

.quiz-essay-textarea:focus {
  outline: none;
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.quiz-essay-textarea:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.quiz-option-correct {
  background: #f0fdf4 !important;
  border-color: #22c55e !important;
  color: #15803d !important;
  font-weight: 600;
}

.quiz-option-wrong {
  background: #fef2f2 !important;
  border-color: #f87171 !important;
  color: #991b1b !important;
  text-decoration: line-through;
}
</style>
