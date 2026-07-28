<script setup>
import { computed, ref, onMounted } from 'vue'
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
  PlayCircle,
  CheckCircle2,
  ListOrdered,
  Download,
  Info
} from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const route = useRoute()
const courseId = route.params.courseId

const loading = ref(false)
const error = ref('')
const chapters = ref([])
const courseInfo = ref(null)

const activeLessonId = ref(null)
const activeLesson = ref(null)

const lessonStats = computed(() => {
  const allLessons = chapters.value.flatMap(chapter => chapter.lessons)
  return [
    { label: 'Tổng bài học', value: allLessons.length, variant: 'neutral' },
    { label: 'Bài giảng Video', value: allLessons.filter(l => l.type === 'video').length, variant: 'info' },
    { label: 'Tài liệu PDF', value: allLessons.filter(l => l.type === 'pdf').length, variant: 'success' },
    { label: 'Bài đọc & Trắc nghiệm', value: allLessons.filter(l => l.type === 'text' || l.type === 'quiz').length, variant: 'warning' },
  ]
})

async function loadLessons() {
  loading.value = true
  error.value = ''
  try {
    const idToFetch = courseId || '1'
    let data = null
    try {
      data = await teacherApi.getTeacherSubjectDetail(idToFetch)
    } catch (err) {
      data = await teacherApi.getTeacherClassDetail(idToFetch)
    }

    const unwrapped = data?.data ?? data?.Data ?? data
    courseInfo.value = {
      code: unwrapped?.code ?? unwrapped?.Code ?? unwrapped?.subjectCode ?? 'GEN101',
      name: unwrapped?.name ?? unwrapped?.Name ?? unwrapped?.courseName ?? 'Môn học',
      className: unwrapped?.className ?? unwrapped?.ClassName ?? 'Tất cả các lớp',
    }

    const items = Array.isArray(unwrapped?.chuongHoc) ? unwrapped.chuongHoc : (Array.isArray(unwrapped?.chapters) ? unwrapped.chapters : [])
    
    // Sample fallback data if DB has empty lessons for demo
    if (!items || items.length === 0) {
      chapters.value = [
        {
          id: 1,
          title: 'Chương 1: Phân tích & Tổng quan môn học',
          lessons: [
            { id: 101, title: 'Bài 1.1: Giới thiệu mục tiêu môn học', type: 'video', duration: '15 phút', content: 'Trong bài học này, giảng viên và sinh viên sẽ làm quen với phương pháp nghiên cứu, tổng quan kiến trúc và định hướng làm đồ án thực tế.', fileUrl: 'https://example.com/demo.mp4' },
            { id: 102, title: 'Bài 1.2: Tài liệu giáo trình & Quy định học tập', type: 'pdf', duration: '20 phút', content: 'Tài liệu hướng dẫn chi tiết về nội quy, thang điểm và lộ trình thực hành môn học.', fileUrl: 'https://example.com/syllabus.pdf' },
          ]
        },
        {
          id: 2,
          title: 'Chương 2: Thực hành & Làm bài trắc nghiệm',
          lessons: [
            { id: 201, title: 'Bài 2.1: Kiến thức nền tảng và bài tập đọc', type: 'text', duration: '30 phút', content: 'Kiến thức cốt lõi về quy trình làm việc nhóm, xây dựng sơ đồ hệ thống và tối ưu hóa hiệu năng.', fileUrl: null },
            { id: 202, title: 'Bài 2.2: Bài kiểm tra đánh giá kiến thức Chương 2', type: 'quiz', duration: '15 phút', content: 'Bộ câu hỏi trắc nghiệm kiểm tra mức độ nắm bắt kiến thức sau Chương 2.', quizQuestions: [
              { id: 1, question: 'Quy trình thiết kế giao diện theo chuẩn Liquid Glass chú trọng yếu tố nào?', options: ['Vị trí nút bấm', 'Màu sắc semantic tokens & hiệu ứng kính mờ trong suốt', 'Sử dụng ảnh gif', 'Văn bản dài'], answer: 1 },
              { id: 2, question: 'Thang điểm đánh giá quá trình học tập tính dựa trên tiêu chí nào?', options: ['Chuyên cần, Bài tập, Thi giữa kỳ, Thi cuối kỳ', 'Chỉ tính điểm thi cuối kỳ', 'Chỉ tính điểm danh', 'Không đánh giá'], answer: 0 }
            ]}
          ]
        }
      ]
    } else {
      chapters.value = items.map(ch => ({
        id: ch.id,
        title: ch.tieuDe ?? ch.title ?? '',
        lessons: (ch.baiHoc ?? ch.lessons ?? []).map(l => ({
          id: l.id,
          title: l.tieuDe ?? l.title ?? '',
          type: l.loai ?? l.type ?? 'text',
          duration: l.thoiLuong ?? l.duration ?? '',
          content: l.noiDung ?? l.content ?? ('Nội dung chi tiết của ' + (l.tieuDe ?? l.title ?? '')),
          fileUrl: l.urlTapTin ?? l.fileUrl ?? null,
          quizQuestions: l.quizQuestions || []
        })),
      }))
    }

    if (chapters.value.length && chapters.value[0].lessons.length) {
      selectLesson(chapters.value[0].lessons[0])
    }
  } catch (e) {
    console.error('Error loading lessons detail:', e)
    error.value = e?.message || 'Không thể tải chi tiết bài học.'
    chapters.value = []
  } finally {
    loading.value = false
  }
}

function selectLesson(lesson) {
  activeLessonId.value = lesson.id
  activeLesson.value = lesson
}

function goBack() {
  router.push('/teacher/lessons')
}

function showAlert(msg) {
  if (typeof window !== 'undefined') {
    window.alert(msg)
  }
}

onMounted(() => { loadLessons() })

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
  <div v-if="loading" class="p-4">
    <ListSkeleton :rows="6" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadLessons">Thử lại</GlassButton>
  </div>
  <div v-else class="lessons-page space-y-4 pb-10">
    <!-- Header -->
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <GlassButton variant="secondary" size="sm" @click="goBack" class="shrink-0">
          <template #leading>
            <ChevronLeft :size="16" />
          </template>
          Danh sách môn học
        </GlassButton>
        <div class="min-w-0">
          <div class="eyebrow">Học liệu môn học</div>
          <h1 class="page-title">
            {{ courseInfo?.code }} - {{ courseInfo?.name }} (Lớp {{ courseInfo?.className }})
          </h1>
          <p class="page-subtitle">
            Theo dõi và chuẩn bị bài giảng theo đúng khung chương trình chuẩn.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge variant="info" size="md">
          <Lock :size="13" /> Quyền xem: Nội dung do Hội đồng chuyên môn biên soạn
        </GlassBadge>
      </div>
    </GlassPanel>

    <!-- Context bar (Stats) -->
    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in lessonStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>
    </GlassPanel>

    <!-- Content Shell -->
    <div class="authoring-shell">
      <!-- Left sidebar: Chapters & Lessons -->
      <aside class="chapters-panel">
        <GlassPanel variant="surface" density="none" class="panel-fill">
          <template #header>
            <div class="panel-heading">
              <div>
                <h2>Chương trình môn học</h2>
                <p>{{ chapters.length }} chương học</p>
              </div>
              <BookOpen :size="18" class="text-muted" />
            </div>
          </template>

          <div class="chapter-list custom-scrollbar">
            <section v-for="chapter in chapters" :key="chapter.id" class="chapter-block">
              <div class="chapter-heading">
                <h3>{{ chapter.title }}</h3>
              </div>

              <div class="lesson-list">
                <button
                  v-for="lesson in chapter.lessons"
                  :key="lesson.id"
                  type="button"
                  :class="['lesson-row', activeLessonId === lesson.id && 'is-active']"
                  @click="selectLesson(lesson)"
                >
                  <span class="lesson-icon">
                    <component :is="getLessonIcon(lesson.type)" :size="16" />
                  </span>
                  <span class="lesson-info">
                    <strong>{{ lesson.title }}</strong>
                    <span>
                      {{ lesson.duration }}
                      <i />
                      {{ getTypeText(lesson.type) }}
                    </span>
                  </span>
                  <GlassBadge :variant="lesson.type === 'video' ? 'info' : lesson.type === 'pdf' ? 'success' : 'neutral'" size="sm">
                    {{ getTypeText(lesson.type) }}
                  </GlassBadge>
                </button>
              </div>
            </section>
          </div>
        </GlassPanel>
      </aside>

      <!-- Right Main Content Viewer (Strictly Read-Only) -->
      <main class="editor-panel">
        <GlassPanel v-if="activeLessonId" variant="surface" density="none" class="panel-fill">
          <template #header>
            <div class="editor-toolbar">
              <div class="editor-title">
                <span class="editor-icon">
                  <PlayCircle :size="18" />
                </span>
                <div class="min-w-0">
                  <h2>{{ activeLesson?.title }}</h2>
                  <p>
                    LOẠI BÀI HỌC:
                    <GlassBadge variant="primary" size="sm">{{ getTypeText(activeLesson?.type) }}</GlassBadge>
                    <span class="ml-2 text-muted font-normal">Thời lượng: {{ activeLesson?.duration }}</span>
                  </p>
                </div>
              </div>

              <div class="editor-actions">
                <GlassBadge variant="info" size="sm">
                  <Info :size="12" /> Chế độ xem bài giảng
                </GlassBadge>
              </div>
            </div>
          </template>

          <div class="editor-body custom-scrollbar">
            <!-- Read Only Notice -->
            <div class="p-3 rounded-2xl bg-(--accent-primary-soft) border border-card flex items-center gap-3">
              <Lock :size="16" class="text-(--accent-primary) shrink-0" />
              <p class="text-xs text-heading font-medium">
                Nội dung bài giảng và tài liệu học tập này được quản lý tập trung bởi Hội đồng môn học. Giảng viên sử dụng nội dung này để giảng dạy trên lớp.
              </p>
            </div>

            <!-- Video Player Viewer -->
            <section v-if="activeLesson?.type === 'video'" class="form-section">
              <div class="section-title">
                <h3>Video Bài giảng</h3>
                <p>Nội dung video minh họa và bài giảng trực tuyến.</p>
              </div>
              <div class="upload-box surface-card border-card">
                <span class="upload-icon">
                  <FileVideo :size="32" class="text-(--accent-primary)" />
                </span>
                <h4 class="text-sm font-bold text-heading mt-2">{{ activeLesson?.title }}</h4>
                <p class="text-xs text-muted">Video bài giảng chuẩn định dạng MP4 / Full HD</p>
                <div class="mt-3 flex gap-2">
                  <GlassButton size="sm" variant="primary" @click="showAlert('Đang phát video bài giảng...')">
                    <template #leading>
                      <PlayCircle :size="15" />
                    </template>
                    Phát video
                  </GlassButton>
                </div>
              </div>
            </section>

            <!-- PDF Viewer -->
            <section v-else-if="activeLesson?.type === 'pdf'" class="form-section">
              <div class="section-title">
                <h3>Tài liệu PDF / Slide giảng dạy</h3>
                <p>Giáo trình, bài giảng và tài liệu đọc cho sinh viên.</p>
              </div>
              <div class="upload-box surface-card border-card">
                <span class="upload-icon">
                  <FileText :size="32" class="text-emerald-600" />
                </span>
                <h4 class="text-sm font-bold text-heading mt-2">{{ activeLesson?.title }}</h4>
                <p class="text-xs text-muted">Định dạng tài liệu: PDF (Đã kiểm duyệt)</p>
                <div class="mt-3 flex gap-2">
                  <GlassButton size="sm" variant="secondary" @click="showAlert('Đang mở xem trước tài liệu PDF...')">
                    <template #leading>
                      <Eye :size="15" />
                    </template>
                    Xem tài liệu
                  </GlassButton>
                </div>
              </div>
            </section>

            <!-- Text Content Reader -->
            <section v-else-if="activeLesson?.type === 'text'" class="form-section">
              <div class="section-title">
                <h3>Nội dung bài đọc & Hướng dẫn</h3>
                <p>Kiến thức lý thuyết và hướng dẫn thực hành.</p>
              </div>
              <div class="p-4 surface-card border border-card rounded-2xl leading-relaxed text-sm text-body whitespace-pre-line">
                {{ activeLesson?.content }}
              </div>
            </section>

            <!-- Quiz Questions Previewer -->
            <section v-else-if="activeLesson?.type === 'quiz'" class="form-section">
              <div class="section-title">
                <h3>Cấu trúc bài trắc nghiệm</h3>
                <p>Danh sách câu hỏi kiểm tra được biên soạn cho bài học này.</p>
              </div>
              
              <div v-if="activeLesson?.quizQuestions && activeLesson.quizQuestions.length" class="space-y-3">
                <div
                  v-for="(q, idx) in activeLesson.quizQuestions"
                  :key="q.id"
                  class="p-4 surface-card border border-card rounded-2xl space-y-2 text-xs"
                >
                  <div class="flex items-center gap-2">
                    <span class="font-bold text-heading">Câu {{ idx + 1 }}:</span>
                    <span class="font-semibold text-heading leading-normal">{{ q.question }}</span>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-2 pt-2">
                    <div
                      v-for="(opt, oIdx) in q.options"
                      :key="oIdx"
                      :class="[
                        'p-2 rounded-xl border text-xs font-medium transition-all',
                        oIdx === q.answer ? 'bg-(--color-success-bg) border-emerald-500/40 text-(--color-success-text)' : 'surface-input border-card text-muted'
                      ]"
                    >
                      <strong class="mr-1.5">{{ String.fromCharCode(65 + oIdx) }}.</strong> {{ opt }}
                      <span v-if="oIdx === q.answer" class="ml-2 font-bold">(Đáp án chuẩn)</span>
                    </div>
                  </div>
                </div>
              </div>
              <div v-else class="p-6 text-center surface-card border-card rounded-2xl">
                <HelpCircle :size="32" class="mx-auto text-muted/50 mb-2" />
                <p class="text-xs text-muted">Bộ câu hỏi trắc nghiệm đã được niêm phong bởi Hội đồng môn học.</p>
              </div>
            </section>
          </div>
        </GlassPanel>

        <GlassPanel v-else variant="surface" density="compact" class="panel-fill">
          <EmptyState
            title="Chọn bài học để xem nội dung"
            description="Chọn một bài học từ danh sách bên trái để xem nội dung chi tiết bài giảng."
          >
            <template #icon>
              <BookOpen :size="24" />
            </template>
          </EmptyState>
        </GlassPanel>
      </main>
    </div>
  </div>
</template>

<style scoped>
.lessons-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar,
.header-main,
.header-actions,
.panel-heading,
.stat-value-line,
.chapter-heading,
.lesson-row,
.lesson-info span,
.editor-toolbar,
.editor-title,
.editor-actions {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.panel-heading,
.editor-toolbar,
.chapter-heading {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon,
.editor-icon,
.lesson-icon,
.upload-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.header-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.eyebrow,
.page-subtitle,
.panel-heading p,
.stat-label,
.chapter-heading h3,
.lesson-info span,
.editor-title p,
.section-title p,
.upload-box p {
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
  max-width: 43rem;
  font-size: 0.875rem;
  line-height: 1.5;
}

.header-actions,
.editor-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
}

.mini-stats {
  display: grid;
  grid-template-columns: repeat(4, minmax(7rem, 1fr));
  gap: 0.625rem;
  width: 100%;
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
  margin-top: 0.375rem;
}

.stat-value-line strong {
  color: var(--text-heading);
  font-size: 1.125rem;
  font-weight: 900;
}

.authoring-shell {
  display: grid;
  grid-template-columns: minmax(18rem, 22rem) minmax(0, 1fr);
  gap: 1rem;
  min-height: 38rem;
}

.chapters-panel {
  grid-column: 1;
  min-width: 0;
}

.editor-panel {
  grid-column: 2;
  grid-row: 1;
  min-width: 0;
}

.panel-fill {
  height: 100%;
  min-width: 0;
}

.panel-heading h2,
.editor-title h2,
.section-title h3,
.upload-box h4 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

.panel-heading h2 {
  font-size: 0.9375rem;
}

.panel-heading p,
.editor-title p,
.section-title p,
.upload-box p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.chapter-list {
  display: grid;
  gap: 0.875rem;
  max-height: 36rem;
  overflow-y: auto;
  padding: 0.75rem;
}

.chapter-block {
  display: grid;
  gap: 0.5rem;
}

.chapter-heading {
  padding: 0 0.25rem;
}

.chapter-heading h3 {
  margin: 0;
  font-size: 0.6875rem;
  font-weight: 900;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.lesson-list {
  display: grid;
  gap: 0.5rem;
}

.lesson-row {
  width: 100%;
  min-width: 0;
  gap: 0.625rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  color: var(--text-body);
  cursor: pointer;
  padding: 0.625rem;
  text-align: left;
  transition: background 160ms ease, border-color 160ms ease, transform 160ms ease;
}

.lesson-row:hover,
.lesson-row.is-active {
  border-color: var(--border-input-focus);
  background: var(--surface-input);
}

.lesson-row.is-active {
  transform: translateY(-1px);
}

.lesson-icon,
.editor-icon {
  width: 2.125rem;
  height: 2.125rem;
  border-radius: var(--radius-md);
}

.lesson-info {
  min-width: 0;
  flex: 1;
}

.lesson-info strong {
  display: block;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.8125rem;
  font-weight: 900;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.lesson-info span {
  gap: 0.375rem;
  margin-top: 0.25rem;
  font-size: 0.6875rem;
  font-weight: 700;
}

.lesson-info i {
  width: 0.25rem;
  height: 0.25rem;
  border-radius: 999px;
  background: var(--border-default);
}

.editor-title {
  gap: 0.75rem;
  min-width: 0;
}

.editor-title h2 {
  overflow: hidden;
  font-size: 1rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.editor-title p {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.375rem;
  font-size: 0.6875rem;
  font-weight: 800;
}

.editor-body {
  display: grid;
  gap: 1rem;
  max-height: 36rem;
  overflow-y: auto;
  padding: 1rem;
}

.form-section {
  display: grid;
  gap: 0.875rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 1rem;
}

.upload-box {
  display: grid;
  place-items: center;
  gap: 0.5rem;
  border-radius: var(--radius-xl);
  border: 1px dashed var(--border-input);
  background: var(--surface-input);
  padding: 2rem;
  text-align: center;
}

.upload-icon {
  width: 3.5rem;
  height: 3.5rem;
  border-radius: var(--radius-lg);
}
</style>
