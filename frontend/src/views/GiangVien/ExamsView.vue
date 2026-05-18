<!-- ExamsView.vue -->
<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, FileEdit, Clock, HelpCircle, 
  ChevronRight, MoreVertical, Send, CheckCircle2,
  FileSignature, Calendar, Settings
} from 'lucide-vue-next'

const exams = ref([
  { id: 1, name: 'Thi giữa kỳ: Lập trình Web', duration: '90 phút', questions: 40, status: 'Published', date: '25/05/2026', type: 'Trắc nghiệm' },
  { id: 2, name: 'Thi kết thúc môn: Java Basic', duration: '120 phút', questions: 50, status: 'Draft', date: '15/06/2026', type: 'Hỗn hợp' },
  { id: 3, name: 'Quiz 2: Cấu trúc dữ liệu', duration: '15 phút', questions: 10, status: 'Published', date: '18/05/2026', type: 'Trắc nghiệm' },
])

const isConfigModalOpen = ref(false)
const configuringExam = ref(null)

const openConfigModal = (exam) => {
  configuringExam.value = { ...exam, shuffle: true, maxAttempts: 1, passScore: 5 }
  isConfigModalOpen.value = true
}

const saveConfig = () => {
  const idx = exams.value.findIndex(e => e.id === configuringExam.value.id)
  if (idx !== -1) {
    exams.value[idx] = { ...exams.value[idx], ...configuringExam.value }
  }
  isConfigModalOpen.value = false
}

const getStatusStyle = (status) => {
  return status === 'Published' 
    ? 'bg-emerald-50 text-emerald-600 border-emerald-100' 
    : 'bg-amber-50 text-amber-600 border-amber-100'
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-600 flex items-center justify-center text-white shadow-md shadow-blue-200">
           <FileSignature :size="32" />
        </div>
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Quản lý Đề thi</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Thiết kế và cấu hình các bộ đề thi trắc nghiệm & tự luận.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <router-link to="/teacher/exams/create" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-cyan-600 px-6 py-4 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all">
            <Plus :size="18" /> Tạo đề thi mới
         </router-link>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="flex items-center justify-between gap-4">
       <div class="flex items-center gap-2">
          <h2 class="text-lg font-black text-slate-800">Danh sách đề thi</h2>
          <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs font-black text-blue-700">{{ exams.length }}</span>
       </div>
       <div class="relative w-64">
          <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input type="text" placeholder="Tìm đề thi..." class="w-full rounded-[16px] border border-slate-200 bg-white pl-11 pr-4 py-3 text-sm font-medium outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
       </div>
    </div>

    <!-- Main List -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="(exam, index) in exams" :key="exam.id" 
           class="group rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm hover:shadow-xl hover:border-blue-200 transition-all flex flex-col animate-fade-in-up"
           :style="{ animationDelay: `${index * 100}ms` }">
        <div class="flex justify-between items-start mb-6">
           <div class="h-14 w-14 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-colors shadow-sm">
              <FileEdit :size="24" />
           </div>
           <div :class="['rounded-xl border px-3 py-1.5 text-[10px] font-black uppercase tracking-widest', getStatusStyle(exam.status)]">
              {{ exam.status === 'Published' ? 'Đã xuất bản' : 'Bản nháp' }}
           </div>
        </div>
        
        <div class="flex-1">
           <div class="inline-block rounded-lg bg-slate-100 px-2 py-1 text-[10px] font-black text-slate-500 uppercase tracking-widest mb-3">
              {{ exam.type }}
           </div>
           <h3 class="text-xl font-black text-slate-900 leading-tight group-hover:text-blue-700 transition-colors line-clamp-2 min-h-[56px]">{{ exam.name }}</h3>
           
           <div class="mt-6 flex flex-col gap-3">
              <div class="flex items-center gap-3">
                 <div class="flex items-center justify-center w-8 h-8 rounded-full bg-slate-50 text-slate-400">
                    <Calendar :size="14" />
                 </div>
                 <div>
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">Ngày diễn ra</p>
                    <p class="text-sm font-bold text-slate-700">{{ exam.date }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-6 mt-2 pt-4 border-t border-slate-50">
                 <div class="flex items-center gap-2">
                    <Clock :size="14" class="text-blue-400" />
                    <span class="text-sm font-bold text-slate-600">{{ exam.duration }}</span>
                 </div>
                 <div class="w-1 h-1 rounded-full bg-slate-300"></div>
                 <div class="flex items-center gap-2">
                    <HelpCircle :size="14" class="text-blue-400" />
                    <span class="text-sm font-bold text-slate-600">{{ exam.questions }} câu hỏi</span>
                 </div>
              </div>
           </div>
        </div>

        <div class="mt-8 pt-6 border-t border-slate-100 flex gap-3">
           <button @click="openConfigModal(exam)" class="flex-1 rounded-2xl bg-white border border-slate-200 py-3 text-sm font-bold text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors shadow-sm">
              Cấu hình
           </button>
           <button v-if="exam.status === 'Draft'" class="flex-1 rounded-2xl bg-blue-600 py-3 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:bg-blue-700 transition-colors flex items-center justify-center gap-2">
              <Send :size="16" /> Xuất bản
           </button>
           <button v-else class="flex-1 rounded-2xl bg-emerald-50 py-3 text-sm font-bold text-emerald-600 flex items-center justify-center gap-2 border border-emerald-100">
              <CheckCircle2 :size="16" /> Mở đề thi
           </button>
        </div>
      </div>
    </div>
  </div>

  <!-- Configure Exam Modal -->
  <Teleport to="body">
    <div v-if="isConfigModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="isConfigModalOpen = false"></div>
      <div class="relative bg-white rounded-3xl shadow-2xl w-full max-w-2xl overflow-hidden animate-fade-in-up">
        <div class="p-6 border-b border-slate-100 bg-slate-50/50 flex items-center justify-between">
          <div>
             <h3 class="text-xl font-bold text-slate-800">Cấu hình Đề thi</h3>
             <p class="text-sm text-slate-500 mt-1">Tùy chỉnh thông số và quy chế thi cho đề này.</p>
          </div>
          <div class="h-12 w-12 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 shadow-sm border border-blue-100/50">
             <Settings :size="24" />
          </div>
        </div>
        
        <div class="p-6 space-y-6 max-h-[60vh] overflow-y-auto">
           <div class="space-y-4">
             <h4 class="text-sm font-bold text-slate-800 uppercase tracking-wider flex items-center gap-2"><FileEdit :size="16" class="text-blue-500" /> Thông tin cơ bản</h4>
             <div>
               <label class="block text-sm font-bold text-slate-700 mb-1.5">Tên đề thi</label>
               <input v-model="configuringExam.name" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
             </div>
             <div class="grid grid-cols-2 gap-4">
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Thời gian làm bài</label>
                 <input v-model="configuringExam.duration" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Ngày diễn ra</label>
                 <input v-model="configuringExam.date" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
             </div>
           </div>

           <div class="space-y-4 pt-4 border-t border-slate-100">
             <h4 class="text-sm font-bold text-slate-800 uppercase tracking-wider flex items-center gap-2"><Settings :size="16" class="text-amber-500" /> Quy chế thi</h4>
             
             <div class="grid grid-cols-2 gap-6">
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Số lần làm bài tối đa</label>
                  <select v-model="configuringExam.maxAttempts" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all">
                    <option :value="1">1 lần</option>
                    <option :value="2">2 lần</option>
                    <option :value="3">3 lần</option>
                    <option :value="999">Không giới hạn</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Điểm đạt tối thiểu (Hệ số 10)</label>
                  <input v-model="configuringExam.passScore" type="number" min="0" max="10" step="0.5" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
             </div>
             
             <div class="flex items-center justify-between p-4 rounded-xl border border-slate-200 bg-slate-50 mt-2">
                <div>
                   <p class="text-sm font-bold text-slate-800">Đảo câu hỏi và đáp án</p>
                   <p class="text-xs font-medium text-slate-500 mt-0.5">Thứ tự câu hỏi và phương án sẽ hiển thị ngẫu nhiên cho mỗi sinh viên.</p>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" v-model="configuringExam.shuffle" class="sr-only peer">
                  <div class="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
                </label>
             </div>
           </div>
        </div>
        
        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3">
          <button @click="isConfigModalOpen = false" class="px-5 py-2.5 rounded-xl font-bold text-slate-500 hover:bg-slate-200 transition-colors">Đóng</button>
          <button @click="saveConfig" class="px-5 py-2.5 rounded-xl font-bold text-white bg-blue-600 hover:bg-blue-700 shadow-md shadow-blue-200 transition-all hover:-translate-y-0.5">Lưu cấu hình</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fade-in-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
