<script setup>
import { computed, ref, onMounted } from 'vue'
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  Eye,
  FileText,
  FileVideo,
  GripVertical,
  HelpCircle,
  Layout,
  MoreVertical,
  PlayCircle,
  Plus,
  Save,
  Trash2,
  Upload,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const chapters = ref([])

const activeLessonId = ref(null)
const activeLesson = ref(null)

const lessonStats = computed(() => {
  const allLessons = chapters.value.flatMap(chapter => chapter.lessons)
  return [
    { label: 'Tổng bài học', value: allLessons.length, variant: 'neutral' },
    { label: 'Đã xuất bản', value: allLessons.filter(lesson => lesson.status === 'published').length, variant: 'success' },
    { label: 'Bản nháp', value: allLessons.filter(lesson => lesson.status === 'draft').length, variant: 'warning' },
    { label: 'Chương', value: chapters.value.length, variant: 'info' },
  ]
})

async function loadLessons() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getTeacherClassDetail() // expects courseId param; use first course
    const items = data?.chuongHoc ?? data?.chapters ?? []
    chapters.value = items.map(ch => ({
      id: ch.id,
      title: ch.tieuDe ?? ch.title ?? '',
      lessons: (ch.baiHoc ?? ch.lessons ?? []).map(l => ({
        id: l.id,
        title: l.tieuDe ?? l.title ?? '',
        type: l.loai ?? l.type ?? 'text',
        duration: l.thoiLuong ?? l.duration ?? '',
        status: l.trangThai === 'published' || l.status === 'published' ? 'published' : 'draft',
      })),
    }))
    if (chapters.value.length && chapters.value[0].lessons.length) {
      selectLesson(chapters.value[0].lessons[0])
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải bài học.'
    chapters.value = []
  } finally {
    loading.value = false
  }
}

function selectLesson(lesson) {
  activeLessonId.value = lesson.id
  activeLesson.value = { ...lesson, content: lesson.content || 'Nội dung chi tiết của ' + lesson.title }
}

function addChapter() {
  const newId = chapters.value.length + 1
  chapters.value.push({
    id: newId,
    title: `Chương ${newId}: Chương mới`,
    lessons: [],
  })
}

function addLesson(chapterId) {
  const chapter = chapters.value.find(c => c.id === chapterId)
  if (chapter) {
    const newLessonId = Date.now()
    chapter.lessons.push({
      id: newLessonId,
      title: 'Bài học mới',
      type: 'text',
      duration: '0 min',
      status: 'draft',
    })
    activeLessonId.value = newLessonId
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
  if (type === 'video') return 'Video'
  if (type === 'pdf') return 'Tài liệu'
  if (type === 'quiz') return 'Quiz'
  return 'Bài đọc'
}

function getStatusText(status) {
  return status === 'published' ? 'Đã xuất bản' : 'Bản nháp'
}

function getStatusVariant(status) {
  return status === 'published' ? 'success' : 'neutral'
}
</script>

<template>
  <div v-if="loading" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải bài học...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadLessons">Thử lại</GlassButton>
  </div>
  <div v-else class="lessons-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <BookOpen :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Course authoring</div>
          <h1 class="page-title">Bài học & học liệu</h1>
          <p class="page-subtitle">
            Xây dựng chương trình học, chọn bài học để chỉnh nội dung và trạng thái xuất bản.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge variant="info" size="md">WEB1013</GlassBadge>
        <GlassButton size="sm" variant="primary" @click="addChapter">
          <template #leading>
            <Plus :size="14" />
          </template>
          Thêm chương
        </GlassButton>
      </div>
    </GlassPanel>

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

    <div class="authoring-shell">
      <aside class="chapters-panel">
        <GlassPanel variant="surface" density="none" class="panel-fill">
          <template #header>
            <div class="panel-heading">
              <div>
                <h2>Chương trình học</h2>
                <p>{{ chapters.length }} chương đang soạn</p>
              </div>
              <Layout :size="18" />
            </div>
          </template>

          <div class="chapter-list custom-scrollbar">
            <section v-for="chapter in chapters" :key="chapter.id" class="chapter-block">
              <div class="chapter-heading">
                <h3>{{ chapter.title }}</h3>
                <button type="button" aria-label="Mở thao tác chương">
                  <MoreVertical :size="14" />
                </button>
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
                  <GlassBadge :variant="getStatusVariant(lesson.status)" size="sm">
                    {{ getStatusText(lesson.status) }}
                  </GlassBadge>
                  <GripVertical :size="15" class="drag-icon" />
                </button>
              </div>

              <button type="button" class="add-lesson-button" @click="addLesson(chapter.id)">
                <Plus :size="15" />
                Thêm bài học mới
              </button>
            </section>
          </div>
        </GlassPanel>
      </aside>

      <main class="editor-panel">
        <GlassPanel v-if="activeLessonId" variant="surface" density="none" class="panel-fill">
          <template #header>
            <div class="editor-toolbar">
              <div class="editor-title">
                <span class="editor-icon">
                  <PlayCircle :size="18" />
                </span>
                <div class="min-w-0">
                  <h2>{{ activeLesson?.title || 'Đang chọn...' }}</h2>
                  <p>
                    LOẠI NỘI DUNG:
                    <GlassBadge variant="primary" size="sm">{{ getTypeText(activeLesson?.type) }}</GlassBadge>
                  </p>
                </div>
              </div>

              <div class="editor-actions">
                <GlassButton size="sm" variant="secondary">
                  <template #leading>
                    <Eye :size="14" />
                  </template>
                  Xem trước
                </GlassButton>
                <GlassButton size="sm" variant="primary">
                  <template #leading>
                    <Save :size="14" />
                  </template>
                  Lưu thay đổi
                </GlassButton>
              </div>
            </div>
          </template>

          <div class="editor-body custom-scrollbar">
            <section class="form-section">
              <div class="section-title">
                <h3>Thông tin bài học</h3>
                <p>Thiết lập tiêu đề, loại nội dung và trạng thái hiển thị.</p>
              </div>

              <div class="form-grid">
                <label class="field">
                  <span>Tiêu đề bài học</span>
                  <input v-model="activeLesson.title" type="text" />
                </label>
                <label class="field">
                  <span>Loại nội dung</span>
                  <select v-model="activeLesson.type">
                    <option value="text">Văn bản (Text)</option>
                    <option value="video">Video giảng dạy</option>
                    <option value="pdf">Tài liệu PDF</option>
                    <option value="quiz">Bài trắc nghiệm (Quiz)</option>
                  </select>
                </label>
              </div>
            </section>

            <section class="form-section">
              <div class="section-title">
                <h3>Nội dung</h3>
                <p>Soạn hoặc đính kèm học liệu tương ứng với loại bài học.</p>
              </div>

              <div v-if="activeLesson.type === 'video'" class="upload-box">
                <span class="upload-icon">
                  <FileVideo :size="28" />
                </span>
                <h4>Tải lên video bài giảng</h4>
                <p>Hỗ trợ MP4, MKV. Dung lượng tối đa 500MB.</p>
                <GlassButton size="sm" variant="secondary">
                  <template #leading>
                    <Upload :size="14" />
                  </template>
                  Chọn File Video
                </GlassButton>
              </div>

              <div v-else-if="activeLesson.type === 'pdf'" class="upload-box">
                <span class="upload-icon">
                  <FileText :size="28" />
                </span>
                <h4>Tải lên tài liệu PDF</h4>
                <p>Tài liệu học tập hoặc slide bài giảng cho sinh viên.</p>
                <GlassButton size="sm" variant="secondary">
                  <template #leading>
                    <Upload :size="14" />
                  </template>
                  Chọn File PDF
                </GlassButton>
              </div>

              <label v-else-if="activeLesson.type === 'text'" class="field">
                <span>Nội dung văn bản</span>
                <textarea
                  v-model="activeLesson.content"
                  rows="12"
                  placeholder="Nhập nội dung bài học tại đây..."
                />
              </label>

              <div v-else-if="activeLesson.type === 'quiz'" class="quiz-box">
                <div class="quiz-heading">
                  <span class="upload-icon compact">
                    <HelpCircle :size="20" />
                  </span>
                  <div>
                    <h4>Cấu trúc bài trắc nghiệm</h4>
                    <p>Chưa có câu hỏi nào trong bài trắc nghiệm này.</p>
                  </div>
                  <GlassButton size="sm" variant="secondary">Thêm câu hỏi</GlassButton>
                </div>
              </div>
            </section>

            <div class="bottom-actions">
              <GlassButton size="sm" variant="danger">
                <template #leading>
                  <Trash2 :size="14" />
                </template>
                Xóa bài học này
              </GlassButton>
              <div class="autosave-chip">
                <CheckCircle2 :size="14" />
                Đã lưu tự động lúc 09:30
              </div>
            </div>
          </div>
        </GlassPanel>

        <GlassPanel v-else variant="surface" density="compact" class="panel-fill">
          <EmptyState
            title="Bắt đầu xây dựng bài giảng"
            description="Chọn một bài học từ danh sách hoặc thêm chương mới để bắt đầu."
          >
            <template #icon>
              <Layout :size="22" />
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
.editor-actions,
.quiz-heading,
.bottom-actions,
.autosave-chip {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.panel-heading,
.editor-toolbar,
.chapter-heading,
.bottom-actions {
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
.upload-box p,
.quiz-heading p {
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
  grid-template-columns: minmax(0, 1fr) minmax(20rem, 26rem);
  gap: 1rem;
  min-height: 38rem;
}

.chapters-panel {
  grid-column: 2;
  min-width: 0;
}

.editor-panel {
  grid-column: 1;
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
.upload-box h4,
.quiz-heading h4 {
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
.upload-box p,
.quiz-heading p {
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

.chapter-heading button {
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  padding: 0.25rem;
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

.drag-icon {
  flex: 0 0 auto;
  color: var(--text-muted);
}

.add-lesson-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
  width: 100%;
  border: 1px dashed var(--border-input);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  color: var(--text-muted);
  cursor: pointer;
  padding: 0.625rem;
  font-size: 0.75rem;
  font-weight: 800;
}

.add-lesson-button:hover {
  border-color: var(--border-input-focus);
  color: var(--text-link);
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

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.875rem;
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 800;
}

.field input,
.field select,
.field textarea {
  width: 100%;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-body);
  outline: 0;
  padding: 0.75rem;
  font-size: 0.875rem;
  font-weight: 650;
}

.field select {
  appearance: none;
  cursor: pointer;
}

.field textarea {
  min-height: 14rem;
  resize: vertical;
  line-height: 1.65;
}

.field input:focus,
.field select:focus,
.field textarea:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.field textarea::placeholder {
  color: var(--text-placeholder);
}

.upload-box,
.quiz-box {
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
  width: 3.25rem;
  height: 3.25rem;
  border-radius: var(--radius-lg);
}

.upload-icon.compact {
  width: 2.25rem;
  height: 2.25rem;
}

.quiz-heading {
  width: 100%;
  align-items: flex-start;
  gap: 0.75rem;
  text-align: left;
}

.quiz-heading > div {
  flex: 1;
}

.autosave-chip {
  gap: 0.375rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--color-success-text);
  padding: 0.5rem 0.75rem;
  font-size: 0.75rem;
  font-weight: 800;
}

.custom-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: var(--border-default) transparent;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 0.375rem;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: var(--border-default);
  border-radius: 999px;
}

@media (max-width: 1180px) {
  .authoring-shell {
    grid-template-columns: 1fr;
  }

  .chapters-panel,
  .editor-panel {
    grid-column: auto;
    grid-row: auto;
  }

  .chapter-list,
  .editor-body {
    max-height: none;
  }
}

@media (max-width: 768px) {
  .page-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .mini-stats,
  .form-grid {
    grid-template-columns: 1fr;
  }

  .header-actions,
  .editor-actions,
  .bottom-actions {
    width: 100%;
  }

  .editor-toolbar,
  .bottom-actions {
    align-items: flex-start;
    flex-direction: column;
  }

  .editor-actions :deep(.glass-button),
  .header-actions :deep(.glass-button) {
    width: 100%;
  }
}
</style>
