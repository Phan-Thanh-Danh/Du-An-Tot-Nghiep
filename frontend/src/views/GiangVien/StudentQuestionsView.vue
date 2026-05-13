<script setup>
import { ref } from 'vue'
import { 
  MessageSquare, User, Search, Send, CheckCircle2, 
  Clock, MoreHorizontal, Filter, MessageCircle 
} from 'lucide-vue-next'

const questions = ref([
  { id: 1, student: 'Nguyễn Văn A', question: 'Thầy ơi, bài Assignment 1 có yêu cầu dùng Tailwind không ạ?', status: 'Pending', time: '10 phút trước' },
  { id: 2, student: 'Trần Thị B', question: 'Em không nộp được file .rar lên hệ thống, thầy xem giúp em.', status: 'Answered', time: '1 giờ trước' },
  { id: 3, student: 'Lê Hoàng C', question: 'Lịch thi giữa kỳ đã có chưa ạ?', status: 'Pending', time: '2 giờ trước' },
])

const selectedQ = ref(null)
const replyText = ref('')

function openReply(q) {
  selectedQ.value = q
  replyText.value = ''
}

function sendReply() {
  if (selectedQ.value) {
    selectedQ.value.status = 'Answered'
    alert('Đã gửi phản hồi cho ' + selectedQ.value.student)
    selectedQ.value = null
  }
}
</script>

<template>
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Câu hỏi của học sinh</h1>
        <p class="text-slate-500 mt-1">Giải đáp thắc mắc và hỗ trợ sinh viên trong quá trình học tập.</p>
      </div>
      <div class="flex gap-2">
         <span class="rounded-full bg-rose-100 px-4 py-2 text-xs font-bold text-rose-600 border border-rose-200">
           2 câu hỏi mới
         </span>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <!-- Questions List -->
      <div class="xl:col-span-2 space-y-4">
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
           <div class="p-4 border-b border-slate-50 flex items-center justify-between">
              <div class="flex gap-2">
                 <button class="px-4 py-2 rounded-xl bg-indigo-600 text-white text-xs font-bold shadow-lg shadow-indigo-100">Tất cả</button>
                 <button class="px-4 py-2 rounded-xl bg-slate-50 text-slate-500 text-xs font-bold hover:bg-slate-100 transition-all">Chưa trả lời</button>
              </div>
              <div class="relative w-64">
                 <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                 <input type="text" placeholder="Tìm câu hỏi..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
              </div>
           </div>

           <div class="divide-y divide-slate-50">
              <div v-for="q in questions" :key="q.id" 
                   @click="openReply(q)"
                   :class="['p-6 cursor-pointer transition-all hover:bg-slate-50/80 group', selectedQ?.id === q.id ? 'bg-indigo-50/50' : '']">
                 <div class="flex items-start gap-4">
                    <div class="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs group-hover:bg-indigo-100 group-hover:text-indigo-600 transition-colors">
                       {{ q.student.split(' ').pop()[0] }}
                    </div>
                    <div class="flex-1 min-w-0">
                       <div class="flex justify-between items-start">
                          <p class="text-sm font-bold text-slate-800">{{ q.student }}</p>
                          <span class="text-[10px] font-bold text-slate-400">{{ q.time }}</span>
                       </div>
                       <p class="mt-1 text-sm text-slate-600 leading-relaxed">{{ q.question }}</p>
                       <div class="mt-3 flex items-center gap-3">
                          <span :class="['rounded-full px-2 py-0.5 text-[9px] font-black uppercase tracking-wider', q.status === 'Answered' ? 'bg-emerald-50 text-emerald-600 border border-emerald-100' : 'bg-rose-50 text-rose-600 border border-rose-100']">
                            {{ q.status === 'Answered' ? 'Đã trả lời' : 'Chưa trả lời' }}
                          </span>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>
      </div>

      <!-- Reply Panel -->
      <div class="xl:col-span-1">
         <div v-if="selectedQ" class="lg-card-glass p-6 bg-white border-slate-100 shadow-xl sticky top-6">
            <h2 class="text-lg font-bold text-slate-800 mb-6 flex items-center gap-2">
               <MessageSquare :size="20" class="text-indigo-600" /> Phản hồi sinh viên
            </h2>
            
            <div class="p-4 rounded-2xl bg-slate-50 border border-slate-100 mb-6">
               <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{{ selectedQ.student }} hỏi:</p>
               <p class="text-sm text-slate-700 italic">"{{ selectedQ.question }}"</p>
            </div>

            <div class="space-y-4">
               <textarea 
                  v-model="replyText"
                  rows="6"
                  placeholder="Nhập nội dung trả lời tại đây..."
                  class="w-full rounded-2xl border border-slate-200 p-4 text-sm outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all leading-relaxed"
               ></textarea>
               
               <div class="flex gap-2">
                  <button @click="selectedQ = null" class="flex-1 rounded-xl border border-slate-200 py-3 text-xs font-bold text-slate-500 hover:bg-slate-50 transition-all">Đóng</button>
                  <button @click="sendReply" class="flex-2 rounded-xl bg-indigo-600 py-3 text-xs font-bold text-white shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all flex items-center justify-center gap-2">
                     <Send :size="14" /> Gửi phản hồi
                  </button>
               </div>
            </div>
         </div>

         <div v-else class="h-64 rounded-[28px] border border-dashed border-slate-200 bg-slate-50/50 p-8 text-center flex flex-col items-center justify-center">
            <MessageCircle :size="40" class="text-slate-200 mb-3" />
            <p class="text-sm font-bold text-slate-400">Chọn câu hỏi để trả lời</p>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.flex-2 { flex: 2; }
</style>
