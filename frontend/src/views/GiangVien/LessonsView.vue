<script setup>
import { computed, ref, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  AlertCircle,
  BookOpen,
  ChevronLeft,
  Eye,
  FileText,
  FileVideo,
  HelpCircle,
  Lock,
  Unlock,
  PlayCircle,
  CheckCircle2,
  ListOrdered,
  Download,
  Info,
  Plus,
  Search,
  Check,
  Sparkles,
  Layers,
  Users,
  Film,
  Settings,
  ShieldCheck,
  ExternalLink,
  X,
  CheckSquare,
  Square
} from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const route = useRoute()
const courseId = computed(() => route.params.courseId || 'COM102')

const loading = ref(false)
const error = ref('')
const chapters = ref([])
const assignments = ref([])
const questionBank = ref([])
const loadingBank = ref(false)
const courseInfo = ref(null)
const activeTab = ref('curriculum') // 'curriculum' | 'assignments' | 'question_bank'
const isAllLocked = ref(false)
const togglingAll = ref(false)

const activeLessonId = ref(null)
const activeLesson = ref(null)
const toastMessage = ref('')
const bankSearch = ref('')
const bankDifficulty = ref('')

// --- Modal 1: Gán câu hỏi từ Ngân hàng vào 1 hoặc nhiều video bài học ---
const showAssignLessonModal = ref(false)
const selectedQuestionForModal = ref(null)
const targetLessonIds = ref([])
const assigningToLessons = ref(false)
const lessonSearchInModal = ref('')

function openChooseLessonModal(question) {
  selectedQuestionForModal.value = question
  targetLessonIds.value = []
  chapters.value.forEach(ch => {
    ch.lessons.forEach(l => {
      if (l.quizQuestions?.some(q => q.id === question.id)) {
        targetLessonIds.value.push(l.id)
      }
    })
  })
  if (targetLessonIds.value.length === 0 && activeLessonId.value) {
    targetLessonIds.value = [activeLessonId.value]
  }
  lessonSearchInModal.value = ''
  showAssignLessonModal.value = true
}

function toggleTargetLesson(lessonId) {
  const idx = targetLessonIds.value.indexOf(lessonId)
  if (idx > -1) {
    targetLessonIds.value.splice(idx, 1)
  } else {
    targetLessonIds.value.push(lessonId)
  }
}

async function confirmAssignToLessons() {
  if (!selectedQuestionForModal.value) return
  if (targetLessonIds.value.length === 0) {
    showToast('Vui lòng chọn ít nhất một bài học để gán câu hỏi!')
    return
  }
  assigningToLessons.value = true
  try {
    const q = selectedQuestionForModal.value
    let successCount = 0
    for (const lId of targetLessonIds.value) {
      await teacherApi.addQuizQuestionToLesson(lId, q.id)
      chapters.value.forEach(ch => {
        const foundL = ch.lessons.find(l => l.id === lId)
        if (foundL) {
          if (!foundL.quizQuestions) foundL.quizQuestions = []
          if (!foundL.quizQuestions.some(existing => existing.id === q.id)) {
            foundL.quizQuestions.push({
              id: q.id,
              question: q.question,
              options: q.options,
              answer: q.answer,
              explanation: q.explanation
            })
          }
        }
      })
      successCount++
    }
    showToast(`✅ Đã gán câu hỏi #${q.id} vào ${successCount} bài học thành công!`)
    showAssignLessonModal.value = false
  } catch (e) {
    console.error('Failed to assign to lessons:', e)
    showToast('Lỗi khi gán câu hỏi: ' + (e.message || ''))
  } finally {
    assigningToLessons.value = false
  }
}

const filteredModalChapters = computed(() => {
  const s = lessonSearchInModal.value.trim().toLowerCase()
  if (!s) return chapters.value
  return chapters.value
    .map(ch => ({
      ...ch,
      lessons: ch.lessons.filter(l => l.title.toLowerCase().includes(s) || ch.title.toLowerCase().includes(s))
    }))
    .filter(ch => ch.lessons.length > 0)
})

// --- Modal 2: Gán câu hỏi từ Ngân hàng vào Bài học đang xem (Active Lesson) ---
const showAddQuizToLessonModal = ref(false)
const selectedQuestionIdsForLesson = ref([])
const assigningQuestions = ref(false)
const modalBankSearch = ref('')
const modalBankDifficulty = ref('')

async function openAssignQuizModalForActiveLesson() {
  if (!activeLesson.value) {
    showToast('Vui lòng chọn một bài học trước!')
    return
  }
  await loadQuestionBank()
  selectedQuestionIdsForLesson.value = (activeLesson.value.quizQuestions || []).map(q => q.id)
  modalBankSearch.value = ''
  modalBankDifficulty.value = ''
  showAddQuizToLessonModal.value = true
}

function toggleQuestionSelection(qId) {
  const idx = selectedQuestionIdsForLesson.value.indexOf(qId)
  if (idx > -1) {
    selectedQuestionIdsForLesson.value.splice(idx, 1)
  } else {
    selectedQuestionIdsForLesson.value.push(qId)
  }
}

async function confirmAssignQuestionsToActiveLesson() {
  if (!activeLesson.value) return
  if (selectedQuestionIdsForLesson.value.length === 0) {
    showToast('Vui lòng chọn ít nhất một câu hỏi để gán!')
    return
  }
  assigningQuestions.value = true
  try {
    let count = 0
    for (const qId of selectedQuestionIdsForLesson.value) {
      const alreadyInLesson = activeLesson.value.quizQuestions?.some(q => q.id === qId)
      if (!alreadyInLesson) {
        await teacherApi.addQuizQuestionToLesson(activeLesson.value.id, qId)
        const bankQ = questionBank.value.find(q => q.id === qId)
        if (bankQ) {
          if (!activeLesson.value.quizQuestions) activeLesson.value.quizQuestions = []
          activeLesson.value.quizQuestions.push({
            id: bankQ.id,
            question: bankQ.question,
            options: bankQ.options,
            answer: bankQ.answer,
            explanation: bankQ.explanation
          })
        }
        count++
      }
    }
    showToast(`✅ Đã gán ${count > 0 ? count + ' câu hỏi mới' : 'các câu hỏi'} vào bài "${activeLesson.value.title}"!`)
    showAddQuizToLessonModal.value = false
  } catch (e) {
    console.error('Failed to assign questions:', e)
    showToast('Lỗi khi gán câu hỏi: ' + (e.message || ''))
  } finally {
    assigningQuestions.value = false
  }
}

const filteredModalQuestionBank = computed(() => {
  let list = questionBank.value
  if (modalBankDifficulty.value) {
    list = list.filter(q => q.difficulty === modalBankDifficulty.value)
  }
  if (modalBankSearch.value && modalBankSearch.value.trim()) {
    const s = modalBankSearch.value.trim().toLowerCase()
    list = list.filter(q => (q.question || '').toLowerCase().includes(s))
  }
  return list
})

const lessonStats = computed(() => {
  const allLessons = chapters.value.flatMap(chapter => chapter.lessons)
  return [
    { label: 'Tổng bài học', value: allLessons.length, variant: 'neutral' },
    { label: 'Bài giảng Video', value: allLessons.filter(l => l.type === 'video').length, variant: 'info' },
    { label: 'Tài liệu PDF', value: allLessons.filter(l => l.type === 'pdf').length, variant: 'success' },
    { label: 'Bài đọc & Trắc nghiệm', value: allLessons.filter(l => l.type === 'text' || l.type === 'quiz').length, variant: 'warning' },
    { label: 'Bài tập môn học', value: assignments.value.length, variant: 'primary' },
  ]
})

function showToast(msg) {
  toastMessage.value = msg
  setTimeout(() => {
    if (toastMessage.value === msg) toastMessage.value = ''
  }, 3500)
}

function normalizeLessonType(rawType) {
  const t = String(rawType || '').toLowerCase()
  if (t.includes('video')) return 'video'
  if (t.includes('pdf')) return 'pdf'
  if (t.includes('quiz') || t.includes('trac_nghiem')) return 'quiz'
  if (t.includes('bai_tap') || t.includes('assignment')) return 'exercise'
  return 'text'
}

function parseChoices(choicesRaw) {
  if (!choicesRaw) return []
  if (Array.isArray(choicesRaw)) return choicesRaw
  try {
    const parsed = JSON.parse(choicesRaw)
    if (Array.isArray(parsed)) {
      return parsed.map(p => (typeof p === 'object' && p !== null ? (p.text || p.NoiDung || JSON.stringify(p)) : String(p)))
    }
  } catch (e) {}
  return String(choicesRaw).split('\n').filter(Boolean)
}

async function loadLessons() {
  loading.value = true
  error.value = ''
  try {
    const fn = teacherApi.getTeacherSubjectDetail || teacherApi.getSubjectLessonsDetail
    const res = await fn.call(teacherApi, courseId.value)
    const unwrapped = res?.data ?? res?.Data ?? res

    courseInfo.value = {
      code: unwrapped.code || unwrapped.Code || courseId.value,
      name: unwrapped.name || unwrapped.Name || 'Môn học',
      courseId: unwrapped.courseId || unwrapped.CourseId,
      className: unwrapped.className || unwrapped.ClassName || (unwrapped.classNames ? unwrapped.classNames.join(', ') : 'Lớp chuyên ngành'),
      studentCount: unwrapped.studentCount ?? unwrapped.StudentCount ?? 0,
      questionBankCount: unwrapped.questionBankCount ?? unwrapped.QuestionBankCount ?? 0
    }

    isAllLocked.value = unwrapped.isAllLocked ?? unwrapped.IsAllLocked ?? false

    assignments.value = (unwrapped.baiTaps || unwrapped.BaiTaps || []).map(b => ({
      id: b.id || b.Id,
      title: b.tieuDe || b.TieuDe || 'Bài tập',
      description: b.moTa || b.MoTa || '',
      deadline: b.hanNop || b.HanNop,
      maxAttempts: b.soLanNopToiDa || b.SoLanNopToiDa || 1,
      allowedFormats: b.dinhDangChoPhep || b.DinhDangChoPhep || 'PDF, DOCX, ZIP',
      gradingGuide: b.huongDanChamDiem || b.HuongDanChamDiem || '',
      status: b.trangThai || b.TrangThai || 'published'
    }))

    const items = Array.isArray(unwrapped.chuongHoc) ? unwrapped.chuongHoc : (Array.isArray(unwrapped.chapters) ? unwrapped.chapters : [])
    
    if (items && items.length > 0) {
      chapters.value = items.map(ch => ({
        id: ch.id,
        title: ch.tieuDe ?? ch.title ?? '',
        lessons: (ch.baiHoc ?? ch.lessons ?? []).map(l => ({
          id: l.id,
          title: l.tieuDe ?? l.title ?? '',
          type: normalizeLessonType(l.loai ?? l.type),
          duration: l.thoiLuong ?? l.duration ?? '15 phút',
          content: l.noiDung ?? l.content ?? ('Nội dung chi tiết của ' + (l.tieuDe ?? l.title ?? '')),
          fileUrl: l.urlTapTin ?? l.fileUrl ?? null,
          allowSeek: l.allowSeek !== false,
          quizQuestions: (l.quizQuestions || []).map(q => ({
            id: q.id,
            question: q.question,
            options: parseChoices(q.options),
            answer: q.answer,
            explanation: q.explanation
          }))
        })),
      }))
    } else {
      chapters.value = []
    }

    if (chapters.value.length && chapters.value[0].lessons.length) {
      const currentActive = chapters.value.flatMap(c => c.lessons).find(l => l.id === activeLessonId.value)
      selectLesson(currentActive || chapters.value[0].lessons[0])
    }
  } catch (e) {
    console.error('Error loading lessons detail:', e)
    error.value = e?.message || 'Không thể tải chi tiết bài học.'
    chapters.value = []
  } finally {
    loading.value = false
  }
}

async function loadQuestionBank() {
  if (questionBank.value.length > 0) return
  loadingBank.value = true
  try {
    const res = await teacherApi.getSubjectQuestionBank(courseId.value)
    const unwrapped = res?.data ?? res?.Data ?? res
    if (Array.isArray(unwrapped)) {
      questionBank.value = unwrapped.map(q => ({
        id: q.id,
        question: q.noiDung,
        type: q.loaiCauHoi,
        options: parseChoices(q.luaChon),
        answer: q.dapAnDung,
        difficulty: q.doKho || 'Trung bình',
        explanation: q.giaiThichDapAn
      }))
    }
  } catch (e) {
    console.error('Failed to load question bank:', e)
  } finally {
    loadingBank.value = false
  }
}

function selectLesson(lesson) {
  activeLessonId.value = lesson.id
  activeLesson.value = lesson
}

function broadcastSeekChange(payload) {
  try {
    const channel = new BroadcastChannel('lms_seek_sync')
    channel.postMessage(payload)
    channel.close()
  } catch (e) {}

  try {
    localStorage.setItem('lms_seek_sync_event', JSON.stringify({
      ...payload,
      _rand: Math.random()
    }))
  } catch (e) {}
}

async function handleToggleSeek(lesson) {
  if (!lesson) return
  try {
    const res = await teacherApi.toggleLessonSeek(lesson.id)
    const newStatus = res?.allowSeek ?? !lesson.allowSeek
    lesson.allowSeek = newStatus

    broadcastSeekChange({
      type: 'LESSON_SEEK_CHANGED',
      courseCode: courseInfo.value?.code || courseId.value,
      lessonId: lesson.id,
      allowSeek: newStatus,
      timestamp: Date.now()
    })

    showToast(newStatus ? '🔓 Đã BẬT cho phép sinh viên tua video bài này' : '🔒 Đã KHÓA tua video của sinh viên cho bài này')
  } catch (e) {
    console.error('Failed to toggle seek:', e)
    showToast('Lỗi khi cập nhật cấu hình: ' + (e.message || ''))
  }
}

async function handleToggleSubjectSeekAll() {
  togglingAll.value = true
  try {
    const targetLock = !isAllLocked.value
    const res = await teacherApi.toggleSubjectSeekAll(courseId.value, targetLock)
    isAllLocked.value = res?.isLocked ?? targetLock
    
    chapters.value.forEach(ch => {
      ch.lessons.forEach(l => {
        if (l.type === 'video') {
          l.allowSeek = !isAllLocked.value
        }
      })
    })

    if (activeLesson.value && activeLesson.value.type === 'video') {
      activeLesson.value.allowSeek = !isAllLocked.value
    }

    broadcastSeekChange({
      type: 'SUBJECT_SEEK_ALL_CHANGED',
      courseCode: courseInfo.value?.code || courseId.value,
      isLocked: isAllLocked.value,
      allowSeek: !isAllLocked.value,
      timestamp: Date.now()
    })

    showToast(isAllLocked.value
      ? `🔒 Đã khóa tính năng tua video của sinh viên cho toàn bộ môn học!`
      : `🔓 Đã cho phép sinh viên tự do tua video toàn bộ môn học!`
    )
  } catch (e) {
    console.error('Failed to toggle all seek:', e)
    showToast('Lỗi khi cập nhật cấu hình: ' + (e.message || ''))
  } finally {
    togglingAll.value = false
  }
}

async function handleAddQuestionToLesson(question) {
  openChooseLessonModal(question)
}

const filteredQuestionBank = computed(() => {
  let list = questionBank.value
  if (bankDifficulty.value) {
    list = list.filter(q => q.difficulty === bankDifficulty.value)
  }
  if (bankSearch.value && bankSearch.value.trim()) {
    const s = bankSearch.value.trim().toLowerCase()
    list = list.filter(q => (q.question || '').toLowerCase().includes(s))
  }
  return list
})

function goBack() {
  router.push('/teacher/lessons')
}

watch(courseId, () => {
  loadLessons()
})

onMounted(() => {
  loadLessons()
})

function getLessonIcon(type) {
  if (type === 'video') return FileVideo
  if (type === 'pdf') return FileText
  if (type === 'quiz') return HelpCircle
  return FileText
}

function getTypeText(type) {
  if (type === 'video') return 'Video bài giảng'
  if (type === 'pdf') return 'Tài liệu PDF'
  if (type === 'quiz') return 'Trắc nghiệm'
  return 'Bài đọc'
}
</script>

<template>
  <div v-if="loading" class="p-6">
    <ListSkeleton :rows="6" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[350px] p-8 gap-4 surface-card rounded-3xl border border-card">
    <AlertCircle :size="48" class="text-rose-500" />
    <h3 class="text-base font-bold text-heading">Không thể tải nội dung môn học</h3>
    <p class="text-sm text-rose-500 text-center max-w-md">{{ error }}</p>
    <div class="flex items-center gap-3 mt-2">
      <GlassButton size="md" variant="secondary" @click="goBack">Quay lại danh sách</GlassButton>
      <GlassButton size="md" variant="primary" @click="loadLessons">Thử lại</GlassButton>
    </div>
  </div>
  <div v-else class="space-y-6 px-1 sm:px-2 pb-12">
    <!-- Toast notification -->
    <div
      v-if="toastMessage"
      class="fixed bottom-6 right-6 z-50 px-5 py-3.5 rounded-2xl bg-slate-900/95 text-white text-xs font-bold shadow-2xl flex items-center gap-3 border border-cyan-500/30 backdrop-blur-md animate-fade-in"
    >
      <Sparkles :size="18" class="text-cyan-400" />
      <span>{{ toastMessage }}</span>
    </div>

    <!-- Header Panel with generous padding -->
    <GlassPanel variant="soft" class="p-6 md:p-8 rounded-3xl" :clip="false">
      <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
        <div class="flex items-start gap-4">
          <GlassButton variant="secondary" size="sm" @click="goBack" class="shrink-0 mt-1">
            <template #leading>
              <ChevronLeft :size="16" />
            </template>
            Quay lại
          </GlassButton>
          <div class="space-y-1.5 min-w-0">
            <div class="text-xs font-bold text-(--accent-primary) uppercase tracking-wider flex items-center gap-2">
              <span>Học liệu & Bài giảng</span>
              <span class="inline-block w-1.5 h-1.5 rounded-full bg-cyan-400"></span>
              <span class="font-mono text-heading">{{ courseInfo?.code }}</span>
            </div>
            <h1 class="text-xl sm:text-2xl lg:text-3xl font-black text-heading leading-tight">
              {{ courseInfo?.name }}
            </h1>
            <div class="flex items-center gap-3 flex-wrap text-xs text-muted pt-1">
              <span class="inline-flex items-center gap-1.5 font-medium text-heading bg-(--accent-primary)/10 px-2.5 py-1 rounded-lg border border-(--accent-primary)/20">
                <Layers :size="14" class="text-(--accent-primary)" />
                Lớp phụ trách: <strong class="text-heading">{{ courseInfo?.className }}</strong>
              </span>
              <span class="inline-flex items-center gap-1.5 text-muted px-2 py-1">
                <Users :size="14" /> Sĩ số: <strong class="text-heading">{{ courseInfo?.studentCount }} sinh viên</strong>
              </span>
            </div>
          </div>
        </div>

        <!-- Master Actions: Toggle Seek for ALL videos -->
        <div class="flex items-center gap-3 flex-wrap justify-end">
          <button
            type="button"
            :disabled="togglingAll"
            @click="handleToggleSubjectSeekAll"
            :class="[
              'px-4 py-2.5 rounded-2xl text-xs font-bold flex items-center gap-2 transition-all shadow-md active:scale-95 border',
              isAllLocked
                ? 'bg-amber-600 hover:bg-amber-700 text-white border-amber-400/40'
                : 'bg-emerald-600 hover:bg-emerald-700 text-white border-emerald-400/40'
            ]"
            title="Nhấn để khóa hoặc cho phép sinh viên tua video toàn bộ môn học"
          >
            <component :is="isAllLocked ? Lock : Unlock" :size="15" />
            <span>{{ isAllLocked ? 'Khóa tua toàn bộ video SV' : 'Cho phép SV tua toàn bộ' }}</span>
          </button>
        </div>
      </div>
    </GlassPanel>

    <!-- Navigation Tabs & Stats Bar -->
    <GlassPanel variant="surface" class="p-4 md:p-5 rounded-2xl" :clip="false">
      <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
        <div class="flex items-center gap-2 flex-wrap">
          <button
            type="button"
            :class="[
              'px-4 py-2 rounded-xl text-xs font-bold transition-all flex items-center gap-2',
              activeTab === 'curriculum'
                ? 'bg-(--accent-primary) text-white shadow-sm'
                : 'surface-input border border-card text-muted hover:text-heading'
            ]"
            @click="activeTab = 'curriculum'"
          >
            <BookOpen :size="14" />
            Chương trình bài học ({{ chapters.reduce((acc, c) => acc + c.lessons.length, 0) }})
          </button>
          <button
            type="button"
            :class="[
              'px-4 py-2 rounded-xl text-xs font-bold transition-all flex items-center gap-2',
              activeTab === 'assignments'
                ? 'bg-(--accent-primary) text-white shadow-sm'
                : 'surface-input border border-card text-muted hover:text-heading'
            ]"
            @click="activeTab = 'assignments'"
          >
            <FileText :size="14" />
            Bài tập & Đồ án ({{ assignments.length }})
          </button>
          <button
            type="button"
            :class="[
              'px-4 py-2 rounded-xl text-xs font-bold transition-all flex items-center gap-2',
              activeTab === 'question_bank'
                ? 'bg-(--accent-primary) text-white shadow-sm'
                : 'surface-input border border-card text-muted hover:text-heading'
            ]"
            @click="activeTab = 'question_bank'; loadQuestionBank()"
          >
            <HelpCircle :size="14" />
            Ngân hàng câu hỏi ({{ courseInfo?.questionBankCount || questionBank.length }})
          </button>
        </div>

        <!-- Mini Stats Counter with proper spacing -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 w-full lg:w-auto">
          <div
            v-for="item in lessonStats.slice(0, 4)"
            :key="item.label"
            class="px-3.5 py-2 rounded-xl border border-card surface-input flex flex-col justify-center min-w-[100px]"
          >
            <span class="text-[11px] font-medium text-muted truncate">{{ item.label }}</span>
            <strong class="text-base font-black text-heading">{{ item.value }}</strong>
          </div>
        </div>
      </div>
    </GlassPanel>

    <!-- TAB 3: Question Bank Browser -->
    <div v-if="activeTab === 'question_bank'" class="space-y-4">
      <div class="surface-card border border-card rounded-2xl p-5 flex flex-col md:flex-row gap-4 items-center justify-between shadow-xs">
        <div class="relative flex-1 w-full">
          <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
          <input
            v-model="bankSearch"
            type="text"
            placeholder="Tìm kiếm câu hỏi trong ngân hàng theo từ khóa..."
            class="w-full pl-11 pr-4 py-2.5 rounded-xl surface-input border border-card text-xs text-heading focus:outline-none focus:border-(--accent-primary)"
          />
        </div>
        <div class="flex items-center gap-3 w-full md:w-auto">
          <select v-model="bankDifficulty" class="text-xs px-3.5 py-2.5 rounded-xl surface-input border border-card text-heading focus:outline-none">
            <option value="">Tất cả độ khó</option>
            <option value="Dễ">Dễ</option>
            <option value="Trung bình">Trung bình</option>
            <option value="Khó">Khó</option>
          </select>
        </div>
      </div>

      <div v-if="loadingBank" class="py-16 text-center text-xs text-muted surface-card border border-card rounded-2xl">
        <div class="animate-spin w-7 h-7 border-2 border-blue-600 border-t-transparent rounded-full mx-auto mb-3"></div>
        Đang truy vấn ngân hàng câu hỏi từ cơ sở dữ liệu...
      </div>
      <div v-else-if="filteredQuestionBank.length === 0" class="p-16 text-center surface-card border border-card rounded-2xl">
        <HelpCircle :size="42" class="mx-auto text-muted/40 mb-3" />
        <h3 class="text-sm font-bold text-heading">Không tìm thấy câu hỏi phù hợp</h3>
        <p class="text-xs text-muted mt-1">Chưa có câu hỏi nào trong ngân hàng môn học này hoặc từ khóa tìm kiếm không khớp.</p>
      </div>
      <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <div
          v-for="q in filteredQuestionBank"
          :key="q.id"
          class="p-5 surface-card border border-card rounded-2xl flex flex-col justify-between gap-4 shadow-sm hover:border-(--accent-primary)/40 transition-all"
        >
          <div>
            <div class="flex items-center justify-between gap-2 pb-3 border-b border-card">
              <span class="text-xs font-mono font-bold text-(--accent-primary)">Câu hỏi #{{ q.id }}</span>
              <GlassBadge :variant="q.difficulty === 'Dễ' ? 'success' : q.difficulty === 'Khó' ? 'danger' : 'warning'" size="sm">
                Độ khó: {{ q.difficulty }}
              </GlassBadge>
            </div>
            <h4 class="text-xs sm:text-sm font-bold text-heading mt-3 leading-relaxed">{{ q.question }}</h4>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mt-4">
              <div
                v-for="(opt, oIdx) in q.options"
                :key="oIdx"
                :class="[
                  'p-2.5 rounded-xl border text-xs font-medium',
                  (String(oIdx) === String(q.answer) || String.fromCharCode(65 + oIdx) === String(q.answer))
                    ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-600 dark:text-emerald-400 font-bold'
                    : 'surface-input border-card text-muted'
                ]"
              >
                <strong class="mr-1.5">{{ String.fromCharCode(65 + oIdx) }}.</strong> {{ opt }}
              </div>
            </div>
          </div>
          <div class="pt-3 border-t border-card flex items-center justify-between">
            <span class="text-xs text-muted">
              Đáp án: <strong class="text-emerald-600 dark:text-emerald-400 font-bold">{{ q.answer }}</strong>
            </span>
            <GlassButton size="sm" variant="primary" @click="openChooseLessonModal(q)">
              <template #leading>
                <Plus :size="13" />
              </template>
              Gán vào bài học...
            </GlassButton>
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 2: Assignments List -->
    <div v-else-if="activeTab === 'assignments'" class="space-y-4">
      <div v-if="assignments.length === 0" class="p-16 text-center surface-card border border-card rounded-2xl">
        <FileText :size="44" class="mx-auto text-muted/40 mb-3" />
        <h3 class="text-sm font-bold text-heading">Chưa có bài tập cho môn học này</h3>
        <p class="text-xs text-muted mt-1">Hội đồng bộ môn chưa tạo bài tập hoặc bài kiểm tra định kỳ cho môn này.</p>
      </div>
      <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <div
          v-for="bt in assignments"
          :key="bt.id"
          class="p-6 surface-card border border-card rounded-2xl flex flex-col justify-between gap-4 shadow-sm hover:border-(--accent-primary)/40 transition-all"
        >
          <div>
            <div class="flex items-center justify-between gap-2 pb-3 border-b border-card">
              <span class="text-xs font-mono font-bold text-(--accent-primary)">Mã BT #{{ bt.id }}</span>
              <GlassBadge variant="success" size="sm">Đã phát hành</GlassBadge>
            </div>
            <h3 class="text-base font-bold text-heading mt-3">{{ bt.title }}</h3>
            <p v-if="bt.description" class="text-xs text-muted mt-2 leading-relaxed">{{ bt.description }}</p>
            <div v-if="bt.gradingGuide" class="mt-4 p-3.5 surface-input border border-card rounded-xl text-xs text-body leading-relaxed">
              <strong class="text-heading block mb-1">Hướng dẫn chấm điểm:</strong>
              {{ bt.gradingGuide }}
            </div>
          </div>
          <div class="pt-3 border-t border-card flex items-center justify-between text-xs text-muted">
            <span>Định dạng nộp: <strong class="text-heading">{{ bt.allowedFormats }}</strong></span>
            <span>Số lần nộp tối đa: <strong class="text-heading">{{ bt.maxAttempts }} lần</strong></span>
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 1: Curriculum & Video Player Shell -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
      <!-- Left Sidebar: Chapters & Lessons (4 columns) -->
      <aside class="lg:col-span-4 surface-card border border-card rounded-2xl p-5 space-y-4 shadow-sm">
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <div>
            <h2 class="text-sm font-bold text-heading">Chương trình môn học</h2>
            <p class="text-xs text-muted">{{ chapters.length }} chương học · {{ chapters.reduce((acc, c) => acc + c.lessons.length, 0) }} bài</p>
          </div>
          <BookOpen :size="18" class="text-(--accent-primary)" />
        </div>

        <div v-if="chapters.length === 0" class="p-8 text-center text-xs text-muted">
          Chưa có bài học nào được cấu hình cho môn học này.
        </div>

        <div v-else class="space-y-4 max-h-[720px] overflow-y-auto pr-1">
          <div v-for="chapter in chapters" :key="chapter.id" class="space-y-2">
            <div class="px-2 py-1 text-[11px] font-bold text-muted uppercase tracking-wider bg-(--surface-page) rounded-lg">
              {{ chapter.title }}
            </div>

            <div class="space-y-1.5">
              <button
                v-for="lesson in chapter.lessons"
                :key="lesson.id"
                type="button"
                :class="[
                  'w-full p-3 rounded-xl border text-left transition-all flex items-center gap-3',
                  activeLessonId === lesson.id
                    ? 'bg-(--accent-primary)/10 border-(--accent-primary) shadow-sm'
                    : 'surface-input border-card hover:border-(--accent-primary)/30 text-muted'
                ]"
                @click="selectLesson(lesson)"
              >
                <div :class="[
                  'w-8 h-8 rounded-lg flex items-center justify-center shrink-0 border',
                  activeLessonId === lesson.id ? 'bg-(--accent-primary) text-white border-transparent' : 'surface-card border-card text-muted'
                ]">
                  <component :is="getLessonIcon(lesson.type)" :size="16" />
                </div>
                <div class="min-w-0 flex-1">
                  <div :class="['text-xs font-bold truncate', activeLessonId === lesson.id ? 'text-heading font-black' : 'text-body']">
                    {{ lesson.title }}
                  </div>
                  <div class="text-[11px] text-muted flex items-center gap-2 mt-0.5">
                    <span>{{ lesson.duration }}</span>
                    <span>·</span>
                    <span>{{ getTypeText(lesson.type) }}</span>
                  </div>
                </div>
                <div class="shrink-0 flex items-center gap-1">
                  <span v-if="lesson.type === 'video' && !lesson.allowSeek" title="Đang khóa tua video đối với sinh viên">
                    <Lock :size="13" class="text-amber-500" />
                  </span>
                  <GlassBadge :variant="lesson.type === 'video' ? 'info' : lesson.type === 'pdf' ? 'success' : 'neutral'" size="sm">
                    {{ getTypeText(lesson.type) }}
                  </GlassBadge>
                </div>
              </button>
            </div>
          </div>
        </div>
      </aside>

      <!-- Right Main: Media Player & Content Details (8 columns) -->
      <main class="lg:col-span-8 surface-card border border-card rounded-2xl p-6 md:p-8 space-y-6 shadow-sm">
        <div v-if="activeLesson" class="space-y-6">
          <!-- Lesson Header -->
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-card">
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <GlassBadge variant="primary" size="sm">{{ getTypeText(activeLesson.type) }}</GlassBadge>
                <span class="text-xs text-muted">Thời lượng: {{ activeLesson.duration }}</span>
                <span v-if="activeLesson.type === 'video'" :class="['text-xs font-bold inline-flex items-center gap-1 px-2 py-0.5 rounded-md', activeLesson.allowSeek ? 'bg-emerald-500/10 text-emerald-600' : 'bg-amber-500/10 text-amber-600']">
                  <component :is="activeLesson.allowSeek ? Unlock : Lock" :size="12" />
                  {{ activeLesson.allowSeek ? 'Sinh viên được tua' : 'Khóa tua SV' }}
                </span>
              </div>
              <h2 class="text-lg sm:text-xl font-bold text-heading pt-1">{{ activeLesson.title }}</h2>
            </div>

            <!-- Action buttons for active lesson -->
            <div class="flex items-center gap-2 flex-wrap">
              <GlassButton
                size="sm"
                variant="secondary"
                @click="openAssignQuizModalForActiveLesson"
              >
                <template #leading>
                  <HelpCircle :size="13" class="text-amber-500" />
                </template>
                + Gán Quiz vào bài này
              </GlassButton>
              <GlassButton
                v-if="activeLesson.type === 'video'"
                size="sm"
                :variant="activeLesson.allowSeek ? 'warning' : 'primary'"
                @click="handleToggleSeek(activeLesson)"
              >
                <template #leading>
                  <component :is="activeLesson.allowSeek ? Lock : Unlock" :size="13" />
                </template>
                {{ activeLesson.allowSeek ? 'Khóa tua bài này' : 'Mở tua bài này' }}
              </GlassButton>
            </div>
          </div>

          <!-- 1. VIDEO PLAYER (Teacher can seek freely!) -->
          <div v-if="activeLesson.type === 'video'" class="space-y-5">
            <div v-if="activeLesson.fileUrl" class="space-y-3">
              <div class="rounded-2xl overflow-hidden bg-black border border-card shadow-xl relative aspect-video flex items-center justify-center">
                <video
                  :key="activeLesson.fileUrl"
                  controls
                  playsinline
                  class="w-full h-full object-contain bg-black"
                  :src="activeLesson.fileUrl"
                  preload="auto"
                >
                  Trình duyệt không hỗ trợ phát video trực tiếp.
                </video>
              </div>
              <div class="flex items-center justify-between text-xs text-muted px-1">
                <span>Trạng thái luồng: <strong class="text-emerald-500">Đã kết nối Cloudflare R2</strong></span>
                <span class="italic">Giảng viên có quyền xem và tua video không giới hạn</span>
              </div>
            </div>
            <div v-else class="p-12 text-center surface-input border border-card rounded-2xl space-y-3">
              <Film :size="40" class="mx-auto text-(--accent-primary)" />
              <h4 class="text-sm font-bold text-heading">{{ activeLesson.title }}</h4>
              <p class="text-xs text-muted">Tệp video chưa được đính kèm hoặc đang xử lý mã hóa.</p>
            </div>

            <!-- Video summary/content -->
            <div v-if="activeLesson.content" class="p-5 surface-input border border-card rounded-2xl text-xs text-body leading-relaxed whitespace-pre-line">
              <strong class="text-heading block mb-2 text-sm">Tóm tắt nội dung bài giảng:</strong>
              {{ activeLesson.content }}
            </div>

            <!-- Quiz items attached to Video lesson -->
            <div class="space-y-4 pt-4 border-t border-card">
              <div class="flex items-center justify-between gap-3 flex-wrap">
                <h3 class="text-sm font-bold text-heading flex items-center gap-2">
                  <HelpCircle :size="16" class="text-amber-500" />
                  Câu hỏi trắc nghiệm kiểm tra bài học ({{ activeLesson.quizQuestions?.length || 0 }})
                </h3>
                <GlassButton size="xs" variant="primary" @click="openAssignQuizModalForActiveLesson">
                  <template #leading>
                    <Plus :size="12" />
                  </template>
                  Gán thêm câu hỏi từ Ngân hàng
                </GlassButton>
              </div>

              <div v-if="activeLesson.quizQuestions && activeLesson.quizQuestions.length" class="space-y-3">
                <div
                  v-for="(q, idx) in activeLesson.quizQuestions"
                  :key="q.id || idx"
                  class="p-4 surface-input border border-card rounded-xl space-y-2"
                >
                  <div class="text-xs font-bold text-heading">
                    <span class="text-(--accent-primary) mr-1">Câu {{ idx + 1 }}:</span> {{ q.question }}
                  </div>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                    <div
                      v-for="(opt, oIdx) in q.options"
                      :key="oIdx"
                      :class="[
                        'p-2 rounded-lg text-xs font-medium border',
                        (String(oIdx) === String(q.answer) || String.fromCharCode(65 + oIdx) === String(q.answer))
                          ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-600 dark:text-emerald-400 font-bold'
                          : 'surface-card border-card text-muted'
                      ]"
                    >
                      <strong class="mr-1">{{ String.fromCharCode(65 + oIdx) }}.</strong> {{ opt }}
                    </div>
                  </div>
                </div>
              </div>
              <div v-else class="p-6 text-center surface-input border border-dashed border-card rounded-2xl">
                <p class="text-xs text-muted mb-2">Bài giảng này chưa có câu hỏi trắc nghiệm nào.</p>
                <GlassButton size="xs" variant="secondary" @click="openAssignQuizModalForActiveLesson">
                  <template #leading>
                    <Plus :size="12" />
                  </template>
                  Mở Ngân hàng câu hỏi để gán
                </GlassButton>
              </div>
            </div>
          </div>

          <!-- 2. PDF DOCUMENT VIEWER -->
          <div v-else-if="activeLesson.type === 'pdf'" class="space-y-4">
            <div v-if="activeLesson.fileUrl" class="space-y-3">
              <div class="p-5 surface-input border border-card rounded-2xl flex items-center justify-between gap-4">
                <div class="flex items-center gap-3">
                  <FileText :size="32" class="text-emerald-500" />
                  <div>
                    <h4 class="text-sm font-bold text-heading">{{ activeLesson.title }}</h4>
                    <p class="text-xs text-muted">Tài liệu học tập chính thức định dạng PDF</p>
                  </div>
                </div>
                <a
                  :href="activeLesson.fileUrl"
                  target="_blank"
                  download
                  class="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold inline-flex items-center gap-2 transition-all shadow-xs"
                >
                  <Download :size="14" />
                  Tải tài liệu PDF
                </a>
              </div>
              <iframe
                :src="activeLesson.fileUrl"
                class="w-full h-[540px] rounded-2xl border border-card bg-white"
              ></iframe>
            </div>
            <div v-else class="p-12 text-center surface-input border border-card rounded-2xl">
              <FileText :size="40" class="mx-auto text-emerald-500 mb-2" />
              <p class="text-xs text-muted">Chưa có tệp PDF đính kèm cho bài học này.</p>
            </div>
          </div>

          <!-- 3. TEXT & QUIZ LESSON -->
          <div v-else class="space-y-5">
            <div class="p-6 surface-input border border-card rounded-2xl text-sm text-body leading-relaxed whitespace-pre-line">
              <strong class="text-heading block mb-3 text-base">Nội dung bài học:</strong>
              {{ activeLesson.content }}
            </div>

            <!-- Quiz items inside lesson if any -->
            <div v-if="activeLesson.quizQuestions && activeLesson.quizQuestions.length" class="space-y-3 pt-4 border-t border-card">
              <div class="flex items-center justify-between gap-3 flex-wrap">
                <h3 class="text-sm font-bold text-heading flex items-center gap-2">
                  <HelpCircle :size="16" class="text-amber-500" />
                  Câu hỏi trắc nghiệm kiểm tra bài học ({{ activeLesson.quizQuestions.length }})
                </h3>
                <GlassButton size="xs" variant="primary" @click="openAssignQuizModalForActiveLesson">
                  <template #leading>
                    <Plus :size="12" />
                  </template>
                  Gán thêm câu hỏi từ Ngân hàng
                </GlassButton>
              </div>
              <div class="space-y-3">
                <div
                  v-for="(q, idx) in activeLesson.quizQuestions"
                  :key="idx"
                  class="p-4 surface-card border border-card rounded-xl space-y-2"
                >
                  <div class="text-xs font-bold text-heading">
                    <span class="text-(--accent-primary) mr-1">Câu {{ idx + 1 }}:</span> {{ q.question }}
                  </div>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                    <div
                      v-for="(opt, oIdx) in q.options"
                      :key="oIdx"
                      :class="[
                        'p-2 rounded-lg text-xs font-medium border',
                        (String(oIdx) === String(q.answer) || String.fromCharCode(65 + oIdx) === String(q.answer))
                          ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-600 dark:text-emerald-400 font-bold'
                          : 'surface-input border-card text-muted'
                      ]"
                    >
                      <strong class="mr-1">{{ String.fromCharCode(65 + oIdx) }}.</strong> {{ opt }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="p-16 text-center text-xs text-muted">
          Chọn một bài học từ danh sách bên trái để xem nội dung bài giảng.
        </div>
      </main>
    </div>

    <!-- ========================================================================= -->
    <!-- MODAL 1: Chọn Video/Bài học để Gán câu hỏi từ Ngân hàng                 -->
    <!-- ========================================================================= -->
    <div
      v-if="showAssignLessonModal"
      class="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 overflow-y-auto animate-fade-in"
      @click.self="showAssignLessonModal = false"
    >
      <div class="surface-card border border-card rounded-3xl p-6 md:p-8 max-w-2xl w-full shadow-2xl space-y-5 animate-scale-up max-h-[90vh] flex flex-col">
        <!-- Header -->
        <div class="flex items-start justify-between gap-4 pb-4 border-b border-card shrink-0">
          <div>
            <div class="flex items-center gap-2 mb-1">
              <GlassBadge variant="primary" size="sm">Ngân hàng câu hỏi</GlassBadge>
              <GlassBadge :variant="selectedQuestionForModal?.difficulty === 'Dễ' ? 'success' : selectedQuestionForModal?.difficulty === 'Khó' ? 'danger' : 'warning'" size="sm">
                Độ khó: {{ selectedQuestionForModal?.difficulty }}
              </GlassBadge>
            </div>
            <h3 class="text-base sm:text-lg font-black text-heading leading-snug">
              Chọn Video bài giảng để gán câu hỏi #{{ selectedQuestionForModal?.id }}
            </h3>
          </div>
          <button
            type="button"
            @click="showAssignLessonModal = false"
            class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input) transition-all"
          >
            <X :size="18" />
          </button>
        </div>

        <!-- Question Preview -->
        <div class="p-4 rounded-2xl surface-input border border-card text-xs space-y-2 shrink-0">
          <strong class="text-heading block">{{ selectedQuestionForModal?.question }}</strong>
          <div class="flex items-center gap-2 text-muted text-[11px]">
            <span>Đáp án chuẩn: <strong class="text-emerald-500 font-bold">{{ selectedQuestionForModal?.answer }}</strong></span>
            <span>·</span>
            <span>Số lựa chọn: {{ selectedQuestionForModal?.options?.length || 0 }}</span>
          </div>
        </div>

        <!-- Search lessons -->
        <div class="relative shrink-0">
          <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted" />
          <input
            v-model="lessonSearchInModal"
            type="text"
            placeholder="Tìm bài học hoặc chương..."
            class="w-full text-xs pl-10 pr-4 py-2.5 rounded-xl surface-input border border-card text-heading placeholder:text-muted focus:outline-none focus:border-(--accent-primary)"
          />
        </div>

        <!-- Lesson List -->
        <div class="space-y-4 overflow-y-auto flex-1 pr-1 min-h-[220px]">
          <div v-if="filteredModalChapters.length === 0" class="p-8 text-center text-xs text-muted">
            Không tìm thấy bài học nào phù hợp.
          </div>
          <div v-for="ch in filteredModalChapters" :key="ch.id" class="space-y-2">
            <div class="text-[11px] font-bold text-muted uppercase tracking-wider px-2">
              {{ ch.title }}
            </div>
            <div class="space-y-1.5">
              <button
                v-for="l in ch.lessons"
                :key="l.id"
                type="button"
                :class="[
                  'w-full p-3 rounded-xl border text-left transition-all flex items-center justify-between gap-3',
                  targetLessonIds.includes(l.id)
                    ? 'bg-(--accent-primary)/10 border-(--accent-primary) shadow-xs'
                    : 'surface-input border-card hover:border-(--accent-primary)/40'
                ]"
                @click="toggleTargetLesson(l.id)"
              >
                <div class="flex items-center gap-3 min-w-0">
                  <component
                    :is="targetLessonIds.includes(l.id) ? CheckSquare : Square"
                    :size="18"
                    :class="targetLessonIds.includes(l.id) ? 'text-(--accent-primary)' : 'text-muted'"
                  />
                  <div class="min-w-0">
                    <div class="text-xs font-bold text-heading truncate">{{ l.title }}</div>
                    <div class="text-[11px] text-muted flex items-center gap-2 mt-0.5">
                      <span>{{ l.duration }}</span>
                      <span>·</span>
                      <span>{{ getTypeText(l.type) }}</span>
                    </div>
                  </div>
                </div>
                <div class="shrink-0 flex items-center gap-2">
                  <span
                    v-if="l.quizQuestions && l.quizQuestions.some(q => q.id === selectedQuestionForModal?.id)"
                    class="text-[11px] font-bold px-2 py-0.5 rounded-md bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                  >
                    ✓ Đã gán
                  </span>
                  <GlassBadge :variant="l.type === 'video' ? 'info' : 'neutral'" size="sm">
                    {{ l.quizQuestions?.length || 0 }} câu hỏi
                  </GlassBadge>
                </div>
              </button>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-between gap-3 pt-4 border-t border-card shrink-0">
          <span class="text-xs text-muted">
            Đã chọn: <strong class="text-heading">{{ targetLessonIds.length }}</strong> bài học
          </span>
          <div class="flex items-center gap-2">
            <GlassButton variant="secondary" size="sm" @click="showAssignLessonModal = false">
              Hủy
            </GlassButton>
            <GlassButton
              variant="primary"
              size="sm"
              :disabled="assigningToLessons || targetLessonIds.length === 0"
              @click="confirmAssignToLessons"
            >
              <template #leading>
                <Check :size="14" />
              </template>
              {{ assigningToLessons ? 'Đang gán...' : `Xác nhận gán vào (${targetLessonIds.length}) bài học` }}
            </GlassButton>
          </div>
        </div>
      </div>
    </div>

    <!-- ========================================================================= -->
    <!-- MODAL 2: Gán câu hỏi từ Ngân hàng vào Bài học đang xem (Active Lesson)  -->
    <!-- ========================================================================= -->
    <div
      v-if="showAddQuizToLessonModal"
      class="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 overflow-y-auto animate-fade-in"
      @click.self="showAddQuizToLessonModal = false"
    >
      <div class="surface-card border border-card rounded-3xl p-6 md:p-8 max-w-3xl w-full shadow-2xl space-y-5 animate-scale-up max-h-[90vh] flex flex-col">
        <!-- Header -->
        <div class="flex items-start justify-between gap-4 pb-4 border-b border-card shrink-0">
          <div>
            <div class="flex items-center gap-2 mb-1">
              <GlassBadge variant="primary" size="sm">{{ courseInfo?.code }}</GlassBadge>
              <span class="text-xs text-muted">Gán câu hỏi trắc nghiệm vào:</span>
            </div>
            <h3 class="text-base sm:text-lg font-black text-heading leading-snug">
              {{ activeLesson?.title }}
            </h3>
          </div>
          <button
            type="button"
            @click="showAddQuizToLessonModal = false"
            class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input) transition-all"
          >
            <X :size="18" />
          </button>
        </div>

        <!-- Search & Filter Controls -->
        <div class="flex flex-col sm:flex-row items-center gap-3 shrink-0">
          <div class="relative flex-1 w-full">
            <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted" />
            <input
              v-model="modalBankSearch"
              type="text"
              placeholder="Tìm kiếm nội dung câu hỏi trong ngân hàng..."
              class="w-full text-xs pl-10 pr-4 py-2.5 rounded-xl surface-input border border-card text-heading placeholder:text-muted focus:outline-none focus:border-(--accent-primary)"
            />
          </div>
          <select
            v-model="modalBankDifficulty"
            class="text-xs px-3.5 py-2.5 rounded-xl surface-input border border-card text-heading focus:outline-none shrink-0 w-full sm:w-auto"
          >
            <option value="">Tất cả độ khó</option>
            <option value="Dễ">Dễ</option>
            <option value="Trung bình">Trung bình</option>
            <option value="Khó">Khó</option>
          </select>
        </div>

        <!-- Question List -->
        <div class="space-y-3 overflow-y-auto flex-1 pr-1 min-h-[260px]">
          <div v-if="filteredModalQuestionBank.length === 0" class="p-12 text-center text-xs text-muted">
            Không tìm thấy câu hỏi phù hợp trong ngân hàng câu hỏi.
          </div>
          <div
            v-for="q in filteredModalQuestionBank"
            :key="q.id"
            :class="[
              'p-4 rounded-2xl border transition-all cursor-pointer flex flex-col gap-3',
              selectedQuestionIdsForLesson.includes(q.id)
                ? 'bg-(--accent-primary)/10 border-(--accent-primary) shadow-xs'
                : 'surface-input border-card hover:border-(--accent-primary)/40'
            ]"
            @click="toggleQuestionSelection(q.id)"
          >
            <div class="flex items-start justify-between gap-3">
              <div class="flex items-start gap-3 min-w-0">
                <component
                  :is="selectedQuestionIdsForLesson.includes(q.id) ? CheckSquare : Square"
                  :size="18"
                  :class="selectedQuestionIdsForLesson.includes(q.id) ? 'text-(--accent-primary)' : 'text-muted'"
                  class="shrink-0 mt-0.5"
                />
                <div>
                  <div class="flex items-center gap-2 mb-1">
                    <span class="text-xs font-mono font-bold text-(--accent-primary)">Câu #{{ q.id }}</span>
                    <GlassBadge :variant="q.difficulty === 'Dễ' ? 'success' : q.difficulty === 'Khó' ? 'danger' : 'warning'" size="sm">
                      {{ q.difficulty }}
                    </GlassBadge>
                    <span
                      v-if="activeLesson?.quizQuestions && activeLesson.quizQuestions.some(existing => existing.id === q.id)"
                      class="text-[10px] font-bold px-2 py-0.5 rounded-md bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                    >
                      ✓ Đã có trong bài
                    </span>
                  </div>
                  <h4 class="text-xs sm:text-sm font-bold text-heading leading-relaxed">{{ q.question }}</h4>
                </div>
              </div>
            </div>

            <!-- Choices Grid -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-1.5 pl-7">
              <div
                v-for="(opt, oIdx) in q.options"
                :key="oIdx"
                :class="[
                  'p-2 rounded-lg text-xs font-medium border',
                  (String(oIdx) === String(q.answer) || String.fromCharCode(65 + oIdx) === String(q.answer))
                    ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-600 dark:text-emerald-400 font-bold'
                    : 'surface-card border-card text-muted'
                ]"
              >
                <strong class="mr-1">{{ String.fromCharCode(65 + oIdx) }}.</strong> {{ opt }}
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-between gap-3 pt-4 border-t border-card shrink-0">
          <span class="text-xs text-muted">
            Đã chọn: <strong class="text-heading">{{ selectedQuestionIdsForLesson.length }}</strong> câu hỏi
          </span>
          <div class="flex items-center gap-2">
            <GlassButton variant="secondary" size="sm" @click="showAddQuizToLessonModal = false">
              Đóng
            </GlassButton>
            <GlassButton
              variant="primary"
              size="sm"
              :disabled="assigningQuestions || selectedQuestionIdsForLesson.length === 0"
              @click="confirmAssignQuestionsToActiveLesson"
            >
              <template #leading>
                <Check :size="14" />
              </template>
              {{ assigningQuestions ? 'Đang gán...' : `Gán các câu hỏi đã chọn (${selectedQuestionIdsForLesson.length})` }}
            </GlassButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
