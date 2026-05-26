<script setup>
import { ref } from 'vue'
import { 
  MessageCircle, User, ThumbsUp, Reply, 
  MoreHorizontal, Search, BookOpen, Clock, Send, CheckCircle2
} from 'lucide-vue-next'

const threads = ref([
  { 
    id: 1, 
    lesson: 'Bài 2: Cấu trúc HTML5',
    user: 'Lê Hoàng C',
    content: 'Tại sao chúng ta phải dùng thẻ <main> thay vì <div> hả thầy?',
    time: '2 giờ trước',
    replies: [
      { id: 101, user: 'TS. Nguyễn Minh Khoa', content: 'Thẻ <main> giúp các trình đọc màn hình và SEO nhận diện nội dung chính tốt hơn em nhé.', time: '1 giờ trước', isTeacher: true }
    ]
  },
  { 
    id: 2, 
    lesson: 'Bài 5: Flexbox Layout',
    user: 'Phạm Minh D',
    content: 'Em vẫn chưa phân biệt được justify-content và align-items.',
    time: '5 giờ trước',
    replies: []
  }
])
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background using radial gradient -->
      <div class="absolute -right-32 -top-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-600 flex items-center justify-center text-white shadow-md shadow-blue-200">
           <MessageCircle :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Thảo luận bài học</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Theo dõi và phản hồi các thắc mắc của sinh viên dưới bài giảng.</p>
        </div>
      </div>
      <div class="relative z-10 w-full md:w-80">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm kiếm câu hỏi, sinh viên..." class="w-full rounded-2xl border border-slate-200 bg-slate-50 px-11 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
      </div>
    </div>

    <!-- ── Threads List ── -->
    <div class="space-y-4 max-w-5xl mx-auto">
      <div v-for="thread in threads" :key="thread.id" 
           class="group rounded-2xl border border-slate-100 bg-white shadow-sm overflow-hidden p-5 hover:shadow-md hover:border-blue-100 transition-colors">
           
         <!-- Topic Info -->
         <div class="flex items-center justify-between mb-8 pb-4 border-b border-slate-50">
            <div class="flex items-center gap-3 px-4 py-2 rounded-xl bg-blue-50/50 border border-blue-100/50 text-blue-700">
               <BookOpen :size="16" />
               <span class="text-xs font-black uppercase tracking-widest">{{ thread.lesson }}</span>
            </div>
            <button class="text-slate-300 hover:text-blue-600 transition-colors p-2 rounded-lg hover:bg-blue-50"><MoreHorizontal :size="20" /></button>
         </div>

         <!-- Parent Comment -->
         <div class="flex gap-5">
            <div class="relative flex-shrink-0">
               <div class="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 font-bold text-lg shadow-inner group-hover:bg-blue-50 group-hover:text-blue-600 transition-colors">
                  {{ thread.user.split(' ').pop()[0] }}
               </div>
            </div>
            <div class="flex-1">
               <div class="flex items-center gap-3">
                  <h3 class="text-base font-bold text-slate-900">{{ thread.user }}</h3>
                  <div class="flex items-center gap-1.5 text-[11px] font-semibold text-slate-400">
                     <Clock :size="12" /> {{ thread.time }}
                  </div>
               </div>
               <p class="mt-2.5 text-base text-slate-700 leading-relaxed font-medium">{{ thread.content }}</p>
               
               <div class="mt-5 flex items-center gap-4">
                  <button class="flex items-center gap-2 text-sm font-bold text-slate-400 hover:text-blue-600 transition-colors">
                     <ThumbsUp :size="16" /> <span class="text-xs">Hữu ích</span>
                  </button>
                  <button class="flex items-center gap-2 text-sm font-bold text-slate-400 hover:text-blue-600 transition-colors">
                     <Reply :size="16" /> <span class="text-xs">Phản hồi</span>
                  </button>
               </div>

               <!-- Replies -->
               <div v-if="thread.replies.length > 0" class="mt-8 space-y-4 relative before:absolute before:inset-y-0 before:left-6 before:w-0.5 before:bg-slate-100 before:-z-10">
                  <div v-for="reply in thread.replies" :key="reply.id" class="flex gap-5 relative z-10">
                     <div class="flex-shrink-0">
                        <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center font-black text-sm shadow-sm ring-4 ring-white', 
                                    reply.isTeacher ? 'bg-gradient-to-br from-blue-500 to-blue-700 text-white shadow-blue-100' : 'bg-slate-100 text-slate-500']">
                           {{ reply.user.split(' ').pop()[0] }}
                        </div>
                     </div>
                     <div class="flex-1 p-5 rounded-[24px]" 
                          :class="reply.isTeacher ? 'bg-blue-50/50 border border-blue-100 shadow-sm relative overflow-hidden' : 'bg-slate-50 border border-slate-100'">
                        <div class="flex items-center gap-3 relative z-10">
                           <h4 class="text-sm font-bold text-slate-900">{{ reply.user }}</h4>
                           <span v-if="reply.isTeacher" class="flex items-center gap-1 rounded-full bg-blue-100 px-2.5 py-0.5 text-[9px] font-black text-blue-700 uppercase tracking-widest border border-blue-200">
                              <CheckCircle2 :size="10" /> Giảng viên
                           </span>
                           <span class="text-[10px] font-semibold text-slate-400 ml-auto flex items-center gap-1">
                              <Clock :size="10" /> {{ reply.time }}
                           </span>
                        </div>
                        <p class="mt-2 text-[15px] text-slate-700 leading-relaxed font-medium relative z-10">{{ reply.content }}</p>
                     </div>
                  </div>
               </div>

               <!-- Quick Reply Input -->
               <div class="mt-8 flex gap-4 items-start">
                  <div class="h-10 w-10 rounded-2xl bg-blue-600 flex items-center justify-center text-white font-bold text-xs shrink-0 shadow-sm shadow-blue-100">MK</div>
                  <div class="relative flex-1 group/input">
                     <input type="text" placeholder="Viết câu trả lời của bạn..." 
                            class="w-full rounded-[20px] border border-slate-200 bg-slate-50 pl-5 pr-12 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
                     <button class="absolute right-2 top-1/2 -translate-y-1/2 p-2 rounded-xl text-blue-500 hover:bg-blue-50 hover:text-blue-700 transition-colors">
                        <Send :size="18" />
                     </button>
                  </div>
               </div>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>
