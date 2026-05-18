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
  Layout,
  BookOpen,
  Edit2
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
  <div class="space-y-8 pb-10 text-slate-800 animate-fade-in">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 flex items-center justify-center text-white shadow-md shadow-blue-200">
           <BookOpen :size="32" />
        </div>
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Bài học & Học liệu</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Xây dựng và quản lý nội dung bài giảng cho sinh viên.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <button @click="addChapter" class="flex items-center gap-2 rounded-2xl bg-white px-5 py-3 border border-slate-200 shadow-sm hover:bg-blue-50 hover:border-blue-200 hover:text-blue-600 transition-colors font-bold text-sm text-slate-700">
            <Plus :size="18" /> Thêm chương mới
         </button>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="flex flex-col lg:flex-row gap-8 h-[calc(100vh-260px)] min-h-[600px]">
      
      <!-- ── LEFT SIDEBAR: Syllabus / Chapters ── -->
      <aside class="w-full lg:w-[400px] flex flex-col bg-white rounded-[32px] border border-slate-100 shadow-sm overflow-hidden shrink-0">
        <div class="p-6 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
          <h2 class="font-black text-slate-800 flex items-center gap-2 text-lg">
            <Layout :size="20" class="text-blue-500" />
            Chương trình học
          </h2>
        </div>

        <div class="flex-1 overflow-y-auto p-5 space-y-6 custom-scrollbar">
          <div v-for="chapter in chapters" :key="chapter.id" class="space-y-3">
            <div class="flex items-center justify-between group px-1">
              <h3 class="text-[11px] font-black text-slate-400 uppercase tracking-widest">{{ chapter.title }}</h3>
              <button class="opacity-0 group-hover:opacity-100 p-1 text-slate-400 hover:text-blue-600 transition-colors"><MoreVertical :size="14" /></button>
            </div>
            
            <div class="space-y-2">
              <div 
                v-for="lesson in chapter.lessons" 
                :key="lesson.id"
                @click="selectLesson(lesson)"
                :class="[
                  'flex items-center gap-3 p-3.5 rounded-[20px] cursor-pointer transition-all border group relative overflow-hidden',
                  activeLessonId === lesson.id 
                    ? 'bg-blue-600 text-white border-blue-600 shadow-md shadow-blue-500/20 scale-[1.02]' 
                    : 'bg-white border-slate-100 hover:border-blue-200 hover:bg-blue-50/50 text-slate-600'
                ]"
              >
                <!-- Active Indicator Bar -->
                <div v-if="activeLessonId === lesson.id" class="absolute left-0 top-0 bottom-0 w-1 bg-cyan-300"></div>

                <div class="flex-shrink-0 h-10 w-10 flex items-center justify-center rounded-[14px]" :class="activeLessonId === lesson.id ? 'bg-white/20' : 'bg-slate-50 text-slate-400 group-hover:bg-blue-100 group-hover:text-blue-500'">
                  <PlayCircle v-if="lesson.type === 'video'" :size="18" />
                  <FileText v-else-if="lesson.type === 'pdf'" :size="18" />
                  <HelpCircle v-else-if="lesson.type === 'quiz'" :size="18" />
                  <FileText v-else :size="18" />
                </div>
                
                <div class="flex-1 min-w-0">
                  <p class="text-[13px] font-bold truncate transition-colors" :class="activeLessonId === lesson.id ? 'text-white' : 'text-slate-800 group-hover:text-blue-700'">{{ lesson.title }}</p>
                  <div class="flex items-center gap-2 mt-0.5">
                    <span :class="['text-[10px] font-semibold', activeLessonId === lesson.id ? 'text-blue-100' : 'text-slate-400']">{{ lesson.duration }}</span>
                    <span class="w-1 h-1 rounded-full" :class="activeLessonId === lesson.id ? 'bg-blue-300' : 'bg-slate-300'"></span>
                    <span :class="['text-[10px] font-bold uppercase tracking-wider', activeLessonId === lesson.id ? 'text-cyan-200' : 'text-slate-400']">{{ lesson.status }}</span>
                  </div>
                </div>
                
                <GripVertical :size="16" class="text-slate-300 group-hover:text-slate-400" :class="{'text-blue-300': activeLessonId === lesson.id}" />
              </div>
            </div>
            
            <button 
              @click="addLesson(chapter.id)"
              class="w-full py-3 mt-2 flex items-center justify-center gap-2 rounded-[20px] border-2 border-dashed border-slate-200 text-slate-400 text-xs font-bold hover:border-blue-300 hover:text-blue-600 hover:bg-blue-50/30 transition-all"
            >
              <Plus :size="16" /> Thêm bài học mới
            </button>
          </div>
        </div>
      </aside>

      <!-- ── RIGHT CONTENT: Lesson Editor / Details ── -->
      <main class="flex-1 flex flex-col bg-white rounded-[32px] border border-slate-100 shadow-sm overflow-hidden">
        
        <template v-if="activeLessonId">
          <!-- Toolbar -->
          <div class="p-6 border-b border-slate-100 flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-50/30 relative z-10">
            <div class="flex items-center gap-4">
              <div class="h-12 w-12 rounded-2xl bg-gradient-to-br from-blue-50 to-cyan-50 flex items-center justify-center text-blue-600 border border-blue-100 shadow-inner">
                 <Edit2 :size="20" />
              </div>
              <div>
                <h2 class="text-xl font-black text-slate-800">{{ activeLesson?.title || 'Đang chọn...' }}</h2>
                <p class="text-[11px] text-slate-400 font-bold uppercase tracking-widest mt-0.5 flex items-center gap-1.5">
                   LOẠI NỘI DUNG: <span class="text-blue-500">{{ activeLesson?.type || 'N/A' }}</span>
                </p>
              </div>
            </div>
            
            <div class="flex items-center gap-3">
              <button class="flex items-center gap-2 rounded-2xl bg-white px-5 py-2.5 border border-slate-200 shadow-sm hover:bg-slate-50 hover:text-slate-900 transition-colors font-bold text-sm text-slate-600">
                <Eye :size="16" /> Xem trước
              </button>
              <button class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 px-6 py-2.5 text-sm font-bold text-white shadow-md shadow-blue-200 hover:shadow-lg transition-all hover:-translate-y-0.5 active:translate-y-0">
                <Save :size="16" /> Lưu thay đổi
              </button>
            </div>
          </div>

          <!-- Editor Area -->
          <div class="flex-1 overflow-y-auto p-8 lg:p-10 custom-scrollbar">
            <div class="max-w-4xl mx-auto space-y-10">
              
              <!-- Lesson Config -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div class="space-y-3">
                  <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest">Tiêu đề bài học</label>
                  <input type="text" v-model="activeLesson.title" class="w-full rounded-[20px] border border-slate-200 bg-slate-50 px-5 py-3.5 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
                </div>
                <div class="space-y-3">
                  <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest">Loại nội dung</label>
                  <select v-model="activeLesson.type" class="w-full rounded-[20px] border border-slate-200 bg-slate-50 px-5 py-3.5 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all shadow-sm appearance-none cursor-pointer">
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
                <div v-if="activeLesson.type === 'video'" class="rounded-[32px] border-2 border-dashed border-blue-200 bg-blue-50/50 p-12 text-center transition-all hover:bg-blue-50">
                  <div class="mx-auto w-20 h-20 rounded-[24px] bg-white shadow-sm border border-blue-100 flex items-center justify-center text-blue-500 mb-6">
                    <FileVideo :size="36" stroke-width="1.5" />
                  </div>
                  <h3 class="text-xl font-black text-slate-800">Tải lên Video bài giảng</h3>
                  <p class="text-sm font-medium text-slate-500 mt-2 mb-8">Hỗ trợ định dạng MP4, MKV. Dung lượng tối đa 500MB.</p>
                  <button class="inline-flex items-center gap-2 rounded-2xl bg-white border border-blue-200 px-8 py-3.5 text-sm font-bold text-blue-600 shadow-sm hover:border-blue-300 hover:bg-blue-50 transition-all">
                    <Upload :size="18" /> Chọn File Video
                  </button>
                </div>

                <!-- PDF Block -->
                <div v-if="activeLesson.type === 'pdf'" class="rounded-[32px] border-2 border-dashed border-cyan-200 bg-cyan-50/50 p-12 text-center transition-all hover:bg-cyan-50">
                  <div class="mx-auto w-20 h-20 rounded-[24px] bg-white shadow-sm border border-cyan-100 flex items-center justify-center text-cyan-500 mb-6">
                    <FileText :size="36" stroke-width="1.5" />
                  </div>
                  <h3 class="text-xl font-black text-slate-800">Tải lên tài liệu PDF</h3>
                  <p class="text-sm font-medium text-slate-500 mt-2 mb-8">Tài liệu học tập hoặc slide bài giảng cho sinh viên.</p>
                  <button class="inline-flex items-center gap-2 rounded-2xl bg-white border border-cyan-200 px-8 py-3.5 text-sm font-bold text-cyan-600 shadow-sm hover:border-cyan-300 hover:bg-cyan-50 transition-all">
                    <Upload :size="18" /> Chọn File PDF
                  </button>
                </div>

                <!-- Text Block -->
                <div v-if="activeLesson.type === 'text'" class="space-y-3">
                  <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest">Nội dung văn bản</label>
                  <textarea 
                    v-model="activeLesson.content" 
                    rows="12" 
                    class="w-full rounded-[24px] border border-slate-200 bg-slate-50 p-6 text-[15px] font-medium text-slate-700 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all leading-relaxed shadow-inner resize-none"
                    placeholder="Nhập nội dung bài học tại đây..."
                  ></textarea>
                </div>

                <!-- Quiz Block -->
                <div v-if="activeLesson.type === 'quiz'" class="rounded-[32px] border border-slate-200 bg-slate-50 p-8 shadow-inner">
                   <div class="flex items-center justify-between mb-8 border-b border-slate-200 pb-6">
                     <div class="flex items-center gap-3">
                        <div class="h-10 w-10 rounded-xl bg-blue-100 text-blue-600 flex items-center justify-center">
                           <HelpCircle :size="20" />
                        </div>
                        <h3 class="text-lg font-black text-slate-800">Cấu trúc bài trắc nghiệm</h3>
                     </div>
                     <button class="text-sm font-bold text-blue-600 hover:bg-blue-50 px-4 py-2 rounded-xl transition-colors">+ Thêm câu hỏi</button>
                   </div>
                   <div class="py-16 text-center text-slate-400 text-sm font-medium">
                      <div class="w-16 h-16 rounded-full border-2 border-dashed border-slate-300 flex items-center justify-center mx-auto mb-4 opacity-50">
                         <HelpCircle :size="24" />
                      </div>
                      Chưa có câu hỏi nào trong bài trắc nghiệm này.
                   </div>
                </div>

              </div>

              <!-- Bottom Actions -->
              <div class="pt-8 flex justify-between items-center">
                 <button class="flex items-center gap-2 text-sm font-bold text-rose-500 hover:text-rose-700 transition-colors px-4 py-2 rounded-xl hover:bg-rose-50">
                   <Trash2 :size="18" /> Xóa bài học này
                 </button>
                 <div class="flex items-center gap-2 text-[11px] font-bold text-slate-400 uppercase tracking-wider bg-slate-50 px-4 py-2 rounded-xl">
                    <CheckCircle2 :size="14" class="text-emerald-500" /> Đã lưu tự động lúc 09:30
                 </div>
              </div>

            </div>
          </div>
        </template>

        <!-- No Selection State -->
        <div v-else class="h-full flex flex-col items-center justify-center text-center opacity-70 p-8">
           <div class="h-32 w-32 rounded-[32px] bg-slate-50 flex items-center justify-center mb-6 shadow-sm border border-slate-100">
              <Layout :size="48" class="text-slate-300" />
           </div>
           <h3 class="text-2xl font-black text-slate-600">Bắt đầu xây dựng bài giảng</h3>
           <p class="text-slate-400 mt-2 max-w-sm font-medium">Chọn một bài học từ danh sách bên trái hoặc nhấn "Thêm chương mới" để bắt đầu.</p>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in {
  animation: fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
