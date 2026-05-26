<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { 
  MessageSquare, User, Search, Send, CheckCircle2, 
  Clock, MoreHorizontal, Filter, MessageCircle, HelpCircle
} from 'lucide-vue-next'

const popupStore = usePopupStore()

const questions = ref([
  { id: 1, student: 'Nguyễn Văn A', question: 'Thầy ơi, bài Assignment 1 có yêu cầu dùng Tailwind không ạ?', status: 'Pending', time: '10 phút trước' },
  { id: 2, student: 'Trần Thị B', question: 'Em không nộp được file .rar lên hệ thống, thầy xem giúp em.', status: 'Answered', time: '1 giờ trước' },
  { id: 3, student: 'Lê Hoàng C', question: 'Lịch thi giữa kỳ đã có chưa ạ?', status: 'Pending', time: '2 giờ trước' },
  { id: 4, student: 'Hoàng Văn D', question: 'Thưa thầy, cho em hỏi thêm về cách hoạt động của Vue Router?', status: 'Pending', time: '3 giờ trước' },
])

const selectedQ = ref(null)
const replyText = ref('')
const activeFilter = ref('all')

const filteredQuestions = computed(() => {
  if (activeFilter.value === 'pending') {
    return questions.value.filter(q => q.status === 'Pending')
  }
  return questions.value
})

const pendingCount = computed(() => questions.value.filter(q => q.status === 'Pending').length)

function openReply(q) {
  selectedQ.value = q
  replyText.value = ''
}

function sendReply() {
  if (selectedQ.value) {
    selectedQ.value.status = 'Answered'
    popupStore.success('Đã gửi phản hồi', `Phản hồi đã được gửi đến ${selectedQ.value.student}`)
    selectedQ.value = null
  }
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background using radial gradient instead of expensive blur -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white shadow-md shadow-blue-200">
           <HelpCircle :size="32" />
        </div>
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Hỏi đáp & Hỗ trợ</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Giải đáp thắc mắc của sinh viên trong quá trình học tập.</p>
        </div>
      </div>
      <div class="relative z-10 flex items-center gap-3">
         <div v-if="pendingCount > 0" class="flex items-center gap-2 rounded-2xl bg-rose-50 px-5 py-3 border border-rose-100 shadow-sm">
            <div class="h-2 w-2 rounded-full bg-rose-500 animate-pulse shadow-[0_0_8px_rgba(244,63,94,0.8)]"></div>
            <span class="text-sm font-bold text-rose-600">{{ pendingCount }} câu hỏi mới</span>
         </div>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      <!-- Questions List -->
      <div class="xl:col-span-2 space-y-6">
        <div class="rounded-[32px] border border-slate-100 bg-white shadow-sm overflow-hidden p-6">
           <!-- Filters & Search -->
           <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
              <div class="flex gap-2 p-1 rounded-[16px] bg-slate-50 border border-slate-100">
                 <button 
                    @click="activeFilter = 'all'"
                    :class="['px-6 py-2.5 rounded-[12px] text-xs font-bold transition-colors', activeFilter === 'all' ? 'bg-white text-blue-600 shadow-sm border border-slate-100' : 'text-slate-500 hover:text-blue-600 hover:bg-blue-50/50']"
                 >Tất cả</button>
                 <button 
                    @click="activeFilter = 'pending'"
                    :class="['px-6 py-2.5 rounded-[12px] text-xs font-bold transition-colors', activeFilter === 'pending' ? 'bg-white text-blue-600 shadow-sm border border-slate-100' : 'text-slate-500 hover:text-blue-600 hover:bg-blue-50/50']"
                 >Chưa trả lời</button>
              </div>
              <div class="relative w-full sm:w-72">
                 <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
                 <input type="text" placeholder="Tìm sinh viên, nội dung..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3 text-xs font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
              </div>
           </div>

           <!-- List -->
           <div class="space-y-4">
              <div v-for="q in filteredQuestions" :key="q.id" 
                   @click="openReply(q)"
                   :class="[
                     'p-5 rounded-[24px] cursor-pointer transition-colors group border', 
                     selectedQ?.id === q.id ? 'bg-blue-50 border-blue-200 shadow-sm' : 'bg-white border-slate-100 hover:border-blue-200 hover:bg-slate-50/50'
                   ]">
                 <div class="flex items-start gap-4">
                    <div :class="['h-12 w-12 rounded-2xl flex flex-shrink-0 items-center justify-center font-bold text-sm transition-colors', 
                                  selectedQ?.id === q.id ? 'bg-blue-600 text-white' : 'bg-slate-50 border border-slate-100 text-slate-500 group-hover:bg-blue-100 group-hover:text-blue-600']">
                       {{ q.student.split(' ').pop()[0] }}
                    </div>
                    <div class="flex-1 min-w-0 pt-0.5">
                       <div class="flex justify-between items-start">
                          <p class="text-sm font-bold text-slate-900">{{ q.student }}</p>
                          <span class="flex items-center gap-1 text-[10px] font-semibold text-slate-400">
                             <Clock :size="10" /> {{ q.time }}
                          </span>
                       </div>
                       <p class="mt-1.5 text-[15px] text-slate-600 font-medium leading-relaxed line-clamp-2">{{ q.question }}</p>
                       <div class="mt-3 flex items-center gap-3">
                          <span v-if="q.status === 'Pending'" class="flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[10px] font-black uppercase tracking-widest bg-rose-50 text-rose-600 border border-rose-100">
                             <div class="h-1.5 w-1.5 rounded-full bg-rose-500 animate-pulse"></div> Chưa trả lời
                          </span>
                          <span v-else class="flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[10px] font-black uppercase tracking-widest bg-emerald-50 text-emerald-600 border border-emerald-100">
                             <CheckCircle2 :size="12" /> Đã trả lời
                          </span>
                       </div>
                    </div>
                 </div>
              </div>
              <div v-if="filteredQuestions.length === 0" class="py-12 text-center text-slate-500 font-medium">
                 Không có câu hỏi nào.
              </div>
           </div>
        </div>
      </div>

      <!-- Reply Panel -->
      <div class="xl:col-span-1">
         <div v-if="selectedQ" class="rounded-[32px] bg-white border border-slate-100 shadow-lg p-8 sticky top-6">
            <h2 class="text-xl font-black text-slate-900 mb-6 flex items-center gap-2">
               <MessageSquare :size="24" class="text-blue-600" /> Phản hồi sinh viên
            </h2>
            
            <div class="p-5 rounded-[24px] bg-slate-50/80 border border-slate-100 mb-6 relative overflow-hidden">
               <div class="absolute -left-2 top-0 bottom-0 w-1 bg-blue-400 rounded-r-full"></div>
               <div class="flex items-center gap-2 mb-2">
                 <User :size="14" class="text-slate-400" />
                 <p class="text-xs font-bold text-slate-500 uppercase tracking-widest">{{ selectedQ.student }} hỏi:</p>
               </div>
               <p class="text-sm font-medium text-slate-800 leading-relaxed italic">"{{ selectedQ.question }}"</p>
            </div>

            <div class="space-y-5">
               <div class="relative group/textarea">
                 <textarea 
                    v-model="replyText"
                    rows="6"
                    placeholder="Viết câu trả lời của bạn tại đây..."
                    class="w-full rounded-[24px] border border-slate-200 bg-slate-50 p-5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors leading-relaxed resize-none shadow-sm"
                 ></textarea>
               </div>
               
               <div class="flex gap-3">
                  <button @click="selectedQ = null" class="flex-1 rounded-[16px] border border-slate-200 bg-white py-3.5 text-xs font-bold text-slate-600 hover:bg-slate-50 hover:text-slate-900 transition-colors shadow-sm">Huỷ</button>
                  <button @click="sendReply" class="flex-[2] rounded-[16px] bg-gradient-to-br from-blue-500 to-blue-600 py-3.5 text-sm font-bold text-white shadow-md shadow-blue-200 hover:shadow-lg transition-all flex items-center justify-center gap-2">
                     <Send :size="16" /> Gửi câu trả lời
                  </button>
               </div>
            </div>
         </div>

         <div v-else class="h-80 rounded-[32px] border-2 border-dashed border-slate-200 bg-slate-50/50 p-8 text-center flex flex-col items-center justify-center sticky top-6 opacity-60">
            <div class="h-24 w-24 rounded-full bg-white shadow-sm flex items-center justify-center mb-4">
               <MessageCircle :size="40" class="text-slate-300" />
            </div>
            <h3 class="font-black text-lg text-slate-600 mb-1">Chưa chọn câu hỏi</h3>
            <p class="text-xs font-medium text-slate-400">Chọn một câu hỏi từ danh sách bên trái để phản hồi</p>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  0% {
    opacity: 0;
    transform: translateY(10px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in-up {
  opacity: 0;
  animation: fade-in-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
  will-change: opacity, transform;
}
.delay-100 { animation-delay: 100ms; }
.delay-200 { animation-delay: 200ms; }
</style>
