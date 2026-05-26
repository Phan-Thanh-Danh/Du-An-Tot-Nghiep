<script setup>
import { ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { 
  ArrowLeft, Search, Download, ExternalLink, 
  MessageSquare, Star, Save, CheckCircle2, AlertCircle,
  FileBox, FileDigit, Clock, Edit3, X
} from 'lucide-vue-next'

const popupStore = usePopupStore()

const submissions = ref([
  { id: 1, studentId: 'SV16001', name: 'Nguyễn Văn A', file: 'Asm1_NVA.zip', time: '18/05/2026 09:30', score: 8.5, comment: 'Tốt, giao diện sạch sẽ.', status: 'Graded' },
  { id: 2, studentId: 'SV16002', name: 'Trần Thị B', file: 'Asm1_Final_B.rar', time: '19/05/2026 14:15', score: null, comment: '', status: 'Pending' },
  { id: 3, studentId: 'SV16003', name: 'Lê Hoàng C', file: 'LHC_Asm1.pdf', time: '19/05/2026 23:55', score: 9.0, comment: 'Xuất sắc!', status: 'Graded' },
  { id: 4, studentId: 'SV16004', name: 'Phạm Minh D', file: 'asm_java_d.zip', time: '20/05/2026 01:20', score: null, comment: '', status: 'Late' },
])

const selectedSubmission = ref(null)

function selectGrading(sub) {
  selectedSubmission.value = { ...sub }
}

function saveGrade() {
  if (selectedSubmission.value) {
    const idx = submissions.value.findIndex(s => s.id === selectedSubmission.value.id)
    if (idx !== -1) {
      submissions.value[idx] = { ...selectedSubmission.value, status: 'Graded' }
    }
    selectedSubmission.value = null
    popupStore.success('Đã lưu điểm', 'Điểm và nhận xét đã được lưu thành công.')
  }
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <router-link to="/teacher/assignments" class="h-16 w-16 rounded-2xl bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-white hover:text-blue-600 hover:border-blue-200 transition-colors shadow-sm hover:shadow-md">
           <ArrowLeft :size="24" />
        </router-link>
        <div>
          <div class="flex items-center gap-3 mb-1">
             <span class="rounded-lg bg-blue-50 px-2 py-1 text-[10px] font-black uppercase tracking-widest text-blue-600 border border-blue-100">Assignment 1</span>
             <span class="rounded-lg bg-slate-50 px-2 py-1 text-[10px] font-black uppercase tracking-widest text-slate-500 border border-slate-200">SE1601</span>
          </div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Chấm bài tập</h1>
        </div>
      </div>
      <div class="relative z-10 flex gap-4">
         <div class="text-right">
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Hạn nộp</p>
            <p class="text-sm font-bold text-slate-800 flex items-center gap-1.5"><Clock :size="14" class="text-blue-500"/> 20/05/2026 23:59</p>
         </div>
         <div class="w-px bg-slate-200 hidden sm:block"></div>
         <div class="text-right">
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Đã chấm</p>
            <p class="text-sm font-bold text-slate-800"><span class="text-blue-600">2</span> / 4</p>
         </div>
      </div>
    </div>

    <!-- Alert Instruction -->
    <div class="rounded-[24px] bg-amber-50 border border-amber-100 p-5 flex gap-4 items-start shadow-sm">
       <div class="h-10 w-10 shrink-0 rounded-full bg-amber-100 text-amber-600 flex items-center justify-center">
          <AlertCircle :size="20" />
       </div>
       <div>
          <h4 class="text-sm font-bold text-amber-900">Lưu ý về bài nộp trễ</h4>
          <p class="text-xs font-medium text-amber-700 mt-1 leading-relaxed">Hệ thống đánh dấu các bài nộp sau ngày 20/05/2026 là "Nộp trễ". Giảng viên có thể xem xét trừ điểm tùy theo quy định của môn học.</p>
       </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      <!-- Left: List of Submissions -->
      <div class="xl:col-span-2 space-y-6">
        <div class="rounded-[32px] border border-slate-100 bg-white shadow-sm overflow-hidden p-6">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
            <h2 class="text-lg font-black text-slate-900 flex items-center gap-2">
               <Users :size="20" class="text-blue-500" /> Danh sách sinh viên
            </h2>
            <div class="relative w-full sm:w-72">
              <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm sinh viên, MSSV..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
            </div>
          </div>
          
          <div class="space-y-3">
             <div v-for="(sub, index) in submissions" :key="sub.id" 
                  @click="selectGrading(sub)"
                  class="group rounded-[24px] border transition-colors cursor-pointer p-4 animate-fade-in-up"
                  :style="{ animationDelay: `${index * 50}ms` }"
                  :class="selectedSubmission?.id === sub.id ? 'bg-blue-50 border-blue-200 shadow-sm' : 'bg-white border-slate-100 hover:border-blue-200 hover:bg-slate-50/50 hover:shadow-md'">
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                   <!-- Student Info -->
                   <div class="flex items-center gap-4 flex-1">
                      <div :class="['h-12 w-12 rounded-2xl flex items-center justify-center font-black text-sm transition-colors shadow-sm',
                                    selectedSubmission?.id === sub.id ? 'bg-blue-600 text-white' : 'bg-slate-50 border border-slate-100 text-slate-500 group-hover:bg-blue-100 group-hover:text-blue-600 group-hover:border-blue-200']">
                         {{ sub.name.split(' ').pop()[0] }}
                      </div>
                      <div>
                         <p class="text-sm font-bold text-slate-900 group-hover:text-blue-700 transition-colors">{{ sub.name }}</p>
                         <div class="flex items-center gap-2 mt-0.5">
                            <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ sub.studentId }}</span>
                            <div v-if="sub.status === 'Late'" class="h-1 w-1 rounded-full bg-rose-400"></div>
                            <span v-if="sub.status === 'Late'" class="text-[10px] font-black text-rose-500 uppercase tracking-widest flex items-center gap-1"><AlertCircle :size="10"/> Nộp trễ</span>
                         </div>
                      </div>
                   </div>

                   <!-- Submission Details -->
                   <div class="flex flex-col sm:items-end gap-1 flex-1">
                      <div class="flex items-center gap-1.5 text-xs font-bold text-slate-600 bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-100 w-max">
                         <FileBox :size="14" class="text-blue-400" />
                         <span class="truncate max-w-[120px]">{{ sub.file }}</span>
                      </div>
                      <span class="text-[10px] font-semibold text-slate-400 flex items-center gap-1"><Clock :size="10"/> {{ sub.time }}</span>
                   </div>

                   <!-- Score & Action -->
                   <div class="flex items-center justify-between sm:justify-end gap-6 sm:w-32 shrink-0 border-t sm:border-t-0 border-slate-100 pt-3 sm:pt-0">
                      <div class="flex flex-col items-center sm:items-end">
                         <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Điểm</span>
                         <span v-if="sub.score !== null" class="text-xl font-black text-emerald-600">{{ sub.score.toFixed(1) }}</span>
                         <span v-else class="text-xs font-bold text-slate-300 italic">--</span>
                      </div>
                      <div :class="['h-10 w-10 rounded-xl flex items-center justify-center transition-all shadow-sm', 
                                   selectedSubmission?.id === sub.id ? 'bg-blue-600 text-white shadow-blue-200' : 'bg-white border border-slate-200 text-slate-400 group-hover:text-blue-600 group-hover:border-blue-200 group-hover:bg-blue-50']">
                         <Edit3 :size="18" />
                      </div>
                   </div>
                </div>
             </div>
          </div>
        </div>
      </div>

      <!-- Right: Grading Panel -->
      <div class="xl:col-span-1">
        <div v-if="selectedSubmission" class="rounded-[32px] border border-slate-100 bg-white shadow-xl p-8 sticky top-6">
           <div class="flex items-center justify-between mb-8 pb-6 border-b border-slate-50">
              <div class="flex items-center gap-4">
                 <div class="h-14 w-14 rounded-2xl bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white shadow-md shadow-blue-200">
                    <FileDigit :size="24" />
                 </div>
                 <div>
                    <h2 class="text-xl font-black text-slate-900">Chấm điểm</h2>
                    <p class="text-sm font-bold text-slate-500">{{ selectedSubmission.name }}</p>
                 </div>
              </div>
              <button @click="selectedSubmission = null" class="h-8 w-8 flex items-center justify-center rounded-full bg-slate-50 text-slate-400 hover:bg-rose-50 hover:text-rose-500 transition-colors">
                 <X :size="16" />
              </button>
           </div>

           <div class="space-y-8 text-slate-800">
              <!-- File Attachment -->
              <div class="space-y-3">
                 <label class="text-[11px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2"><FileBox :size="14"/> File bài nộp</label>
                 <a href="#" class="group flex items-center justify-between rounded-[20px] bg-slate-50 border border-slate-200 p-4 hover:border-blue-300 hover:bg-white hover:shadow-md transition-all">
                    <div class="flex items-center gap-3">
                       <div class="h-10 w-10 rounded-xl bg-blue-100 text-blue-600 flex items-center justify-center">
                          <Download :size="18" />
                       </div>
                       <div>
                          <p class="text-sm font-bold text-slate-800 group-hover:text-blue-600 transition-colors">{{ selectedSubmission.file }}</p>
                          <p class="text-[10px] font-semibold text-slate-400 mt-0.5">{{ selectedSubmission.time }}</p>
                       </div>
                    </div>
                    <ExternalLink :size="16" class="text-slate-300 group-hover:text-blue-400" />
                 </a>
              </div>

              <!-- Score Input -->
              <div class="space-y-3">
                 <label class="text-[11px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2"><Star :size="14"/> Điểm số (0-10)</label>
                 <div class="relative">
                    <input 
                       type="number" 
                       v-model="selectedSubmission.score"
                       max="10" min="0" step="0.1"
                       placeholder="0.0"
                       class="w-full rounded-[24px] border border-slate-200 bg-white px-6 py-5 text-4xl font-black text-blue-600 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors text-center shadow-sm placeholder:text-slate-200"
                    />
                 </div>
              </div>

              <!-- Comment Input -->
              <div class="space-y-3">
                 <label class="text-[11px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2"><MessageSquare :size="14"/> Nhận xét</label>
                 <div class="relative group/textarea">
                    <textarea 
                       v-model="selectedSubmission.comment"
                       rows="4"
                       placeholder="Nhập phản hồi cho sinh viên..."
                       class="w-full rounded-[24px] border border-slate-200 bg-slate-50 p-5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors leading-relaxed resize-none shadow-sm group-hover/textarea:border-slate-300"
                    ></textarea>
                 </div>
              </div>

              <div class="pt-4">
                 <button @click="saveGrade" class="w-full rounded-[20px] bg-gradient-to-br from-blue-500 to-blue-600 py-4 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all flex items-center justify-center gap-2">
                    <CheckCircle2 :size="18" /> Lưu kết quả
                 </button>
              </div>
           </div>
        </div>

        <div v-else class="rounded-[32px] border-2 border-dashed border-slate-200 bg-slate-50/50 p-12 text-center flex flex-col items-center justify-center min-h-[500px] sticky top-6">
           <div class="h-24 w-24 rounded-full bg-white flex items-center justify-center text-slate-300 mb-6 shadow-sm border border-slate-100">
              <Edit3 :size="40" />
           </div>
           <h3 class="text-xl font-black text-slate-800">Chưa chọn bài</h3>
           <p class="text-sm font-medium text-slate-500 mt-2 max-w-[200px] leading-relaxed">Nhấp vào một sinh viên trong danh sách để bắt đầu chấm điểm.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fade-in-up 0.3s ease-out forwards;
}
</style>
