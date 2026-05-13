<script setup>
import { ref } from 'vue'
import { 
  MessageCircle, User, ThumbsUp, Reply, 
  MoreHorizontal, Search, BookOpen, Clock 
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
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Bình luận bài học</h1>
        <p class="text-slate-500 mt-1">Theo dõi các cuộc thảo luận của sinh viên dưới mỗi bài giảng.</p>
      </div>
      <div class="relative w-64">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm theo bài học..." class="w-full rounded-xl border border-slate-100 bg-white px-9 py-2 text-xs outline-none focus:border-indigo-300 shadow-sm" />
      </div>
    </div>

    <!-- Threads List -->
    <div class="space-y-6 max-w-5xl mx-auto">
      <div v-for="thread in threads" :key="thread.id" class="rounded-[32px] border border-slate-100 bg-white shadow-sm overflow-hidden p-8">
         <!-- Topic Info -->
         <div class="flex items-center gap-2 mb-6 border-b border-slate-50 pb-4">
            <BookOpen :size="16" class="text-indigo-500" />
            <span class="text-xs font-black text-slate-400 uppercase tracking-widest">{{ thread.lesson }}</span>
         </div>

         <!-- Parent Comment -->
         <div class="flex gap-4">
            <div class="h-12 w-12 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold">
               {{ thread.user.split(' ').pop()[0] }}
            </div>
            <div class="flex-1">
               <div class="flex items-center gap-3">
                  <h3 class="text-sm font-bold text-slate-800">{{ thread.user }}</h3>
                  <span class="text-[10px] text-slate-400">{{ thread.time }}</span>
               </div>
               <p class="mt-2 text-[15px] text-slate-600 leading-relaxed">{{ thread.content }}</p>
               
               <div class="mt-4 flex items-center gap-4">
                  <button class="flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-indigo-600 transition-colors">
                     <ThumbsUp :size="14" /> Thích
                  </button>
                  <button class="flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-indigo-600 transition-colors">
                     <Reply :size="14" /> Phản hồi
                  </button>
                  <button class="ml-auto text-slate-300 hover:text-slate-600 transition-colors"><MoreHorizontal :size="18" /></button>
               </div>

               <!-- Replies -->
               <div v-if="thread.replies.length > 0" class="mt-6 space-y-6 border-l-2 border-slate-50 pl-8">
                  <div v-for="reply in thread.replies" :key="reply.id" class="flex gap-4">
                     <div :class="['h-10 w-10 rounded-full flex items-center justify-center font-bold text-xs shadow-sm', reply.isTeacher ? 'bg-indigo-600 text-white' : 'bg-slate-100 text-slate-400']">
                        {{ reply.user.split(' ').pop()[0] }}
                     </div>
                     <div class="flex-1 p-4 rounded-2xl" :class="reply.isTeacher ? 'bg-indigo-50/50 border border-indigo-100/50' : 'bg-slate-50'">
                        <div class="flex items-center gap-3">
                           <h4 class="text-xs font-bold text-slate-800">{{ reply.user }}</h4>
                           <span v-if="reply.isTeacher" class="rounded-full bg-indigo-600 px-2 py-0.5 text-[8px] font-black text-white uppercase tracking-tighter">Giảng viên</span>
                           <span class="text-[9px] text-slate-400 ml-auto">{{ reply.time }}</span>
                        </div>
                        <p class="mt-1.5 text-sm text-slate-600 leading-relaxed">{{ reply.content }}</p>
                     </div>
                  </div>
               </div>

               <!-- Quick Reply Input -->
               <div class="mt-6 flex gap-3">
                  <div class="h-8 w-8 rounded-full bg-indigo-600 flex items-center justify-center text-white font-bold text-[10px] shrink-0">MK</div>
                  <input type="text" placeholder="Viết phản hồi..." class="flex-1 rounded-xl border border-slate-100 bg-slate-50 px-4 py-2 text-xs outline-none focus:border-indigo-300" />
               </div>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>
