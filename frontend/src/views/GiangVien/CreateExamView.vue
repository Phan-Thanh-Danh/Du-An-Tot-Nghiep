<!-- CreateExamView.vue -->
<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { 
  ArrowLeft, Save, Send, Database, Clock, 
  Settings, Layers, Target, BookOpen, AlertCircle, Check, Plus
} from 'lucide-vue-next'

const router = useRouter()

const form = ref({
  name: '',
  description: '',
  type: 'Trắc nghiệm',
  duration: 60,
  passingScore: 5.0,
  shuffle: true,
  showResult: true,
  startTime: '',
  endTime: ''
})

function saveDraft() {
  alert('Đã lưu bản nháp thành công!')
  router.push('/teacher/exams')
}

function publish() {
  alert('Đã xuất bản đề thi!')
  router.push('/teacher/exams')
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800 max-w-6xl mx-auto">
    <!-- ── Header ── -->
    <div class="flex items-center justify-between bg-white p-6 rounded-[32px] border border-slate-100 shadow-sm sticky top-6 z-10">
      <div class="flex items-center gap-4">
        <router-link to="/teacher/exams" class="h-12 w-12 rounded-2xl bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-white hover:text-blue-600 hover:border-blue-200 transition-colors shadow-sm">
           <ArrowLeft :size="20" />
        </router-link>
        <div>
          <h1 class="text-xl font-black text-slate-900 tracking-tight">Tạo đề thi mới</h1>
          <p class="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Cấu hình & Chọn câu hỏi</p>
        </div>
      </div>
      <div class="flex items-center gap-3">
         <button @click="saveDraft" class="flex items-center gap-2 rounded-2xl bg-white px-6 py-3 border border-slate-200 shadow-sm hover:bg-slate-50 hover:text-blue-600 transition-colors font-bold text-sm text-slate-700">
            <Save :size="16" /> Lưu nháp
         </button>
         <button @click="publish" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-cyan-600 px-6 py-3 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all">
            <Send :size="16" /> Xuất bản
         </button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
       <!-- Main Form Area -->
       <div class="lg:col-span-2 space-y-8">
          
          <!-- Thông tin cơ bản -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
                   <BookOpen :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Thông tin cơ bản</h2>
             </div>

             <div class="space-y-6">
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Tên đề thi *</label>
                   <input v-model="form.name" type="text" placeholder="Ví dụ: Kiểm tra giữa kỳ môn..." class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-4 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
                </div>
                
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Mô tả / Hướng dẫn làm bài</label>
                   <textarea v-model="form.description" rows="4" placeholder="Nhập hướng dẫn cho sinh viên..." class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-4 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm resize-none"></textarea>
                </div>

                <div class="grid grid-cols-2 gap-4">
                   <div>
                      <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Loại đề thi</label>
                      <select v-model="form.type" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 px-5 py-4 text-sm font-bold text-slate-600 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm appearance-none cursor-pointer">
                         <option>Trắc nghiệm</option>
                         <option>Tự luận</option>
                         <option>Hỗn hợp</option>
                      </select>
                   </div>
                   <div>
                      <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Thời gian (Phút)</label>
                      <div class="relative">
                         <Clock :size="16" class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" />
                         <input v-model="form.duration" type="number" class="w-full rounded-[16px] border border-slate-200 bg-white pl-12 pr-5 py-4 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
                      </div>
                   </div>
                </div>
             </div>
          </div>

          <!-- Chọn câu hỏi -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center justify-between mb-6 pb-6 border-b border-slate-50">
                <div class="flex items-center gap-3">
                   <div class="h-10 w-10 rounded-xl bg-cyan-50 text-cyan-600 flex items-center justify-center">
                      <Database :size="18" />
                   </div>
                   <h2 class="text-lg font-black text-slate-900">Nguồn câu hỏi</h2>
                </div>
             </div>

             <div class="rounded-[24px] border-2 border-dashed border-slate-200 bg-slate-50 p-12 flex flex-col items-center justify-center text-center hover:border-blue-300 hover:bg-blue-50/50 transition-colors cursor-pointer group">
                <div class="h-16 w-16 rounded-full bg-white shadow-sm border border-slate-100 flex items-center justify-center text-blue-500 mb-4 group-hover:scale-110 transition-transform">
                   <Plus :size="24" />
                </div>
                <h3 class="text-sm font-bold text-slate-800">Thêm câu hỏi từ Thư viện</h3>
                <p class="text-xs font-medium text-slate-500 mt-2 max-w-[250px] leading-relaxed">Chọn các câu hỏi có sẵn từ kho tài nguyên hoặc tạo đề ngẫu nhiên theo cấu trúc.</p>
             </div>
          </div>

       </div>

       <!-- Sidebar Area -->
       <div class="lg:col-span-1 space-y-8">
          
          <!-- Cấu hình nâng cao -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-slate-100 text-slate-600 flex items-center justify-center">
                   <Settings :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Cấu hình thi</h2>
             </div>

             <div class="space-y-6">
                <!-- Toggle Item -->
                <div class="flex items-center justify-between">
                   <div>
                      <p class="text-sm font-bold text-slate-800">Đảo câu hỏi</p>
                      <p class="text-[10px] font-medium text-slate-400 mt-0.5">Xáo trộn vị trí câu</p>
                   </div>
                   <button @click="form.shuffle = !form.shuffle" :class="['w-12 h-7 rounded-full p-1 transition-colors relative', form.shuffle ? 'bg-blue-500' : 'bg-slate-200']">
                      <div :class="['h-5 w-5 rounded-full bg-white shadow-sm transition-transform', form.shuffle ? 'translate-x-5' : 'translate-x-0']"></div>
                   </button>
                </div>

                <!-- Toggle Item -->
                <div class="flex items-center justify-between pt-4 border-t border-slate-50">
                   <div>
                      <p class="text-sm font-bold text-slate-800">Xem điểm ngay</p>
                      <p class="text-[10px] font-medium text-slate-400 mt-0.5">Sau khi nộp bài</p>
                   </div>
                   <button @click="form.showResult = !form.showResult" :class="['w-12 h-7 rounded-full p-1 transition-colors relative', form.showResult ? 'bg-blue-500' : 'bg-slate-200']">
                      <div :class="['h-5 w-5 rounded-full bg-white shadow-sm transition-transform', form.showResult ? 'translate-x-5' : 'translate-x-0']"></div>
                   </button>
                </div>

                <div class="pt-4 border-t border-slate-50">
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Điểm đạt (Pass)</label>
                   <input v-model="form.passingScore" type="number" step="0.1" max="10" min="0" class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-3 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 transition-colors shadow-sm" />
                </div>
             </div>
          </div>

          <!-- Lịch thi -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
                   <Clock :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Mở & Đóng đề</h2>
             </div>

             <div class="space-y-4">
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Bắt đầu mở</label>
                   <input v-model="form.startTime" type="datetime-local" class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-3 text-sm font-bold text-slate-600 outline-none focus:border-amber-400 transition-colors shadow-sm" />
                </div>
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Tự động đóng</label>
                   <input v-model="form.endTime" type="datetime-local" class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-3 text-sm font-bold text-slate-600 outline-none focus:border-amber-400 transition-colors shadow-sm" />
                </div>
                
                <div class="mt-4 p-4 rounded-xl bg-slate-50 border border-slate-100 flex items-start gap-3">
                   <AlertCircle :size="16" class="text-slate-400 shrink-0 mt-0.5" />
                   <p class="text-xs font-medium text-slate-500 leading-relaxed">Nếu để trống, đề thi sẽ mở vĩnh viễn hoặc cho đến khi đóng thủ công.</p>
                </div>
             </div>
          </div>
       </div>
    </div>
  </div>
</template>

<style scoped>
</style>
