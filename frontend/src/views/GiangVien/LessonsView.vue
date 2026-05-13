<script setup>
import { ref } from 'vue'
import {
  Plus,
  GripVertical,
  PlayCircle,
  FileText,
  FileVideo,
  HelpCircle,
  ChevronRight,
  MoreVertical,
  Upload,
  Save,
  Eye,
  Trash2,
  CheckCircle2,
  Layout
} from 'lucide-vue-next'

// ── Mock Data ──────────────────────────────────────────────
const chapters = ref([
  {
    id: 1,
    title: 'Chương 1: Giới thiệu về Web Development',
    lessons: [
      { id: 101, title: 'Tổng quan về Frontend & Backend', type: 'text', duration: '10 min', status: 'published' },
      { id: 102, title: 'Cài đặt môi trường phát triển', type: 'video', duration: '15 min', status: 'published' },
    ]
  },
  {
    id: 2,
    title: 'Chương 2: HTML5 & Semantic Web',
    lessons: [
      { id: 201, title: 'Cấu trúc tài liệu HTML5', type: 'video', duration: '20 min', status: 'published' },
      { id: 202, title: 'Các thẻ Semantic phổ biến', type: 'pdf', duration: '5 pages', status: 'draft' },
      { id: 203, title: 'Bài trắc nghiệm cuối chương', type: 'quiz', duration: '10 câu', status: 'draft' },
    ]
  }
])

const activeLessonId = ref(101)
const activeLesson = ref({
  id: 101,
  title: 'Tổng quan về Frontend & Backend',
  type: 'text',
  content: 'Học về sự khác biệt giữa giao diện người dùng và xử lý phía máy chủ...',
  videoUrl: '',
  pdfUrl: '',
})

// ── Actions ───────────────────────────────────────────────
function selectLesson(lesson) {
  activeLessonId.value = lesson.id
  activeLesson.value = { ...lesson, content: 'Nội dung chi tiết của ' + lesson.title }
}

function addChapter() {
  const newId = chapters.value.length + 1
  chapters.value.push({
    id: newId,
    title: `Chương ${newId}: Chương mới`,
    lessons: []
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
      status: 'draft'
    })
    activeLessonId.value = newLessonId
  }
}
</script>

<template>
  <div class="h-[calc(100vh-140px)] flex flex-col lg:flex-row gap-6 pb-4">
    
    <!-- ── LEFT SIDEBAR: Syllabus / Chapters ── -->
    <aside class="w-full lg:w-[380px] flex flex-col lg-card-glass p-0 overflow-hidden border-slate-100">
      <div class="p-5 border-b border-slate-100 flex items-center justify-between bg-slate-50/30">
        <h2 class="font-bold text-slate-800 flex items-center gap-2">
          <Layout :size="18" class="text-indigo-600" />
          Chương trình học
        </h2>
        <button @click="addChapter" class="p-1.5 rounded-lg hover:bg-indigo-50 text-indigo-600 transition-colors" title="Thêm chương">
          <Plus :size="20" />
        </button>
      </div>

      <div class="flex-1 overflow-y-auto p-4 space-y-4 custom-scrollbar">
        <div v-for="chapter in chapters" :key="chapter.id" class="space-y-2">
          <div class="flex items-center justify-between group">
            <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest">{{ chapter.title }}</h3>
            <button class="opacity-0 group-hover:opacity-100 p-1 text-slate-400"><MoreVertical :size="14" /></button>
          </div>
          
          <div class="space-y-1.5">
            <div 
              v-for="lesson in chapter.lessons" 
              :key="lesson.id"
              @click="selectLesson(lesson)"
              :class="[
                'flex items-center gap-3 p-3 rounded-2xl cursor-pointer transition-all border',
                activeLessonId === lesson.id 
                  ? 'bg-indigo-600 text-white border-indigo-600 shadow-lg shadow-indigo-100 scale-[1.02]' 
                  : 'bg-white border-slate-50 hover:border-indigo-100 hover:bg-indigo-50/30 text-slate-600'
              ]"
            >
              <div class="flex-shrink-0">
                <PlayCircle v-if="lesson.type === 'video'" :size="18" />
                <FileText v-else-if="lesson.type === 'pdf'" :size="18" />
                <HelpCircle v-else-if="lesson.type === 'quiz'" :size="18" />
                <FileText v-else :size="18" />
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-bold truncate">{{ lesson.title }}</p>
                <p :class="['text-[10px]', activeLessonId === lesson.id ? 'text-indigo-100' : 'text-slate-400']">
                  {{ lesson.duration }} • {{ lesson.status }}
                </p>
              </div>
              <GripVertical :size="14" class="text-slate-300 group-hover:text-slate-400" />
            </div>
          </div>
          
          <button 
            @click="addLesson(chapter.id)"
            class="w-full py-2.5 mt-2 flex items-center justify-center gap-2 rounded-xl border border-dashed border-slate-200 text-slate-400 text-xs font-bold hover:border-indigo-300 hover:text-indigo-600 hover:bg-indigo-50/20 transition-all"
          >
            <Plus :size="14" /> Thêm bài học
          </button>
        </div>
      </div>
    </aside>

    <!-- ── RIGHT CONTENT: Lesson Editor / Details ── -->
    <main class="flex-1 flex flex-col lg-card-glass p-0 overflow-hidden border-slate-100">
      <!-- Toolbar -->
      <div class="p-4 border-b border-slate-100 flex items-center justify-between bg-white relative z-10">
        <div class="flex items-center gap-4">
          <div class="h-10 w-10 rounded-xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100">
             <Edit2 v-if="activeLesson" :size="20" />
          </div>
          <div>
            <h1 class="text-lg font-bold text-slate-800">{{ activeLesson?.title || 'Chọn bài học để bắt đầu' }}</h1>
            <p class="text-xs text-slate-400 font-medium uppercase tracking-wider">Loại nội dung: {{ activeLesson?.type || 'N/A' }}</p>
          </div>
        </div>
        
        <div class="flex items-center gap-2">
          <button class="lg-button-secondary py-2 px-4 text-xs font-bold">
            <Eye :size="16" /> Xem trước
          </button>
          <button class="lg-button-primary py-2 px-6 text-xs font-bold" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
            <Save :size="16" /> Lưu thay đổi
          </button>
        </div>
      </div>

      <!-- Editor Area -->
      <div class="flex-1 overflow-y-auto p-8 custom-scrollbar bg-slate-50/20">
        <div v-if="activeLessonId" class="max-w-4xl mx-auto space-y-8">
          
          <!-- Lesson Config -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-widest">Tiêu đề bài học</label>
              <input type="text" v-model="activeLesson.title" class="w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-[15px] outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all" />
            </div>
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-widest">Loại nội dung</label>
              <select v-model="activeLesson.type" class="w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-[15px] outline-none focus:border-indigo-400 transition-all">
                <option value="text">Văn bản (Text)</option>
                <option value="video">Video giảng dạy</option>
                <option value="pdf">Tài liệu PDF</option>
                <option value="quiz">Bài trắc nghiệm (Quiz)</option>
              </select>
            </div>
          </div>

          <!-- Content Specific Blocks -->
          <div class="space-y-6">
            
            <!-- Video Block -->
            <div v-if="activeLesson.type === 'video'" class="rounded-3xl border-2 border-dashed border-indigo-100 bg-indigo-50/30 p-10 text-center">
              <div class="mx-auto w-16 h-16 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 mb-4">
                <FileVideo :size="32" />
              </div>
              <h3 class="text-lg font-bold text-slate-700">Tải lên Video bài giảng</h3>
              <p class="text-sm text-slate-400 mb-6">Hỗ trợ định dạng MP4, MKV. Dung lượng tối đa 500MB.</p>
              <button class="inline-flex items-center gap-2 rounded-2xl bg-indigo-600 px-6 py-3 text-sm font-bold text-white shadow-lg hover:bg-indigo-700 transition-all active:scale-95">
                <Upload :size="18" /> Chọn File Video
              </button>
            </div>

            <!-- PDF Block -->
            <div v-if="activeLesson.type === 'pdf'" class="rounded-3xl border-2 border-dashed border-teal-100 bg-teal-50/30 p-10 text-center">
              <div class="mx-auto w-16 h-16 rounded-full bg-teal-100 flex items-center justify-center text-teal-600 mb-4">
                <FileText :size="32" />
              </div>
              <h3 class="text-lg font-bold text-slate-700">Tải lên tài liệu PDF</h3>
              <p class="text-sm text-slate-400 mb-6">Tài liệu học tập hoặc slide bài giảng cho sinh viên.</p>
              <button class="inline-flex items-center gap-2 rounded-2xl bg-teal-600 px-6 py-3 text-sm font-bold text-white shadow-lg hover:bg-teal-700 transition-all">
                <Upload :size="18" /> Chọn File PDF
              </button>
            </div>

            <!-- Text Block -->
            <div v-if="activeLesson.type === 'text'" class="space-y-2">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-widest">Nội dung văn bản</label>
              <textarea 
                v-model="activeLesson.content" 
                rows="10" 
                class="w-full rounded-2xl border border-slate-200 bg-white p-6 text-[15px] outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all leading-relaxed"
                placeholder="Nhập nội dung bài học tại đây..."
              ></textarea>
            </div>

            <!-- Quiz Block -->
            <div v-if="activeLesson.type === 'quiz'" class="lg-card-glass p-8 border-dashed border-2 border-slate-200">
               <div class="flex items-center justify-between mb-6">
                 <h3 class="text-lg font-bold text-slate-700 flex items-center gap-2"><HelpCircle class="text-indigo-500" /> Cấu trúc bài trắc nghiệm</h3>
                 <button class="text-sm font-bold text-indigo-600">+ Thêm câu hỏi</button>
               </div>
               <div class="py-12 text-center text-slate-400 italic text-sm">Chưa có câu hỏi nào trong bài này.</div>
            </div>

          </div>

          <!-- Bottom Actions -->
          <div class="pt-8 border-t border-slate-100 flex justify-between items-center">
             <button class="flex items-center gap-2 text-sm font-bold text-rose-500 hover:text-rose-700 transition-colors">
               <Trash2 :size="18" /> Xóa bài học này
             </button>
             <div class="flex items-center gap-4 text-xs font-bold text-slate-400 uppercase tracking-wider">
                <span class="flex items-center gap-1.5"><CheckCircle2 :size="14" class="text-emerald-500" /> Đã lưu tự động lúc 09:30</span>
             </div>
          </div>

        </div>

        <!-- No Selection State -->
        <div v-else class="h-full flex flex-col items-center justify-center text-center opacity-50">
           <div class="h-24 w-24 rounded-full bg-slate-100 flex items-center justify-center mb-4">
              <Layout :size="48" class="text-slate-300" />
           </div>
           <h3 class="text-xl font-bold text-slate-400">Chọn một bài học từ danh sách bên trái</h3>
           <p class="text-sm text-slate-300 mt-2">Hoặc thêm chương mới để bắt đầu xây dựng nội dung.</p>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import { Edit2 } from 'lucide-vue-next'
export default {
  components: {
    Edit2
  }
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

.scale-\[1\.02\] {
  transform: scale(1.02);
}
</style>
