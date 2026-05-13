<script setup>
import { ref } from 'vue'
import { 
  ArrowLeft, 
  Star, 
  TrendingUp, 
  BarChart, 
  Download, 
  ShieldAlert, 
  User, 
  BookOpen, 
  Clock, 
  MessageCircle,
  Award,
  ChevronRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const teacher = ref({
  name: 'TS. Nguyễn Văn A',
  code: 'GV001',
  dept: 'Khoa Công nghệ thông tin',
  avgRating: 4.92,
  totalEvals: 86,
  status: 'excellent',
  criteria: [
    { label: 'Kiến thức chuyên môn', score: 4.95 },
    { label: 'Khả năng truyền đạt', score: 4.88 },
    { label: 'Tương tác sinh viên', score: 4.90 },
    { label: 'Công bằng đánh giá', score: 4.94 },
    { label: 'Hỗ trợ ngoài giờ', score: 4.82 },
  ],
  semesterHistory: [
    { term: 'Fall 2024', score: 4.75 },
    { term: 'Spring 2025', score: 4.82 },
    { term: 'Fall 2025', score: 4.92 },
  ]
})
</script>

<template>
  <PageContainer 
    :title="`Chi tiết đánh giá: ${teacher.name}`" 
    subtitle="Báo cáo phân tích chuyên sâu chất lượng giảng dạy của giảng viên qua các tiêu chí và thời gian."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <router-link to="/bgh/evaluations/ranking" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <ArrowLeft :size="18" /> Quay lại
         </router-link>
         <button class="lg-button-primary bg-indigo-600 py-2.5 px-5 text-sm font-black shadow-lg shadow-indigo-200 flex items-center gap-2">
            <Download :size="18" /> XUẤT BÁO CÁO PDF
         </button>
      </div>
    </template>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Criteria Breakdown ── -->
      <div class="xl:col-span-2 space-y-6">
        <div class="lg-card-glass p-8 relative overflow-hidden">
           <!-- Rating Badge Overlay -->
           <div class="absolute top-8 right-8 text-right">
              <div class="inline-flex items-center gap-2 bg-emerald-50 text-emerald-600 px-4 py-2 rounded-2xl border border-emerald-100 shadow-sm">
                 <Star :size="20" class="fill-emerald-500" />
                 <span class="text-2xl font-black">{{ teacher.avgRating }}</span>
              </div>
              <p class="text-[10px] font-black text-emerald-600 uppercase tracking-widest mt-2">Xếp loại: Xuất sắc</p>
           </div>

           <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-10">Đánh giá theo tiêu chí</h4>
           
           <div class="space-y-8">
              <div v-for="item in teacher.criteria" :key="item.label">
                 <div class="flex items-center justify-between mb-3">
                    <span class="text-xs font-black text-slate-600 uppercase tracking-tighter">{{ item.label }}</span>
                    <div class="flex items-center gap-2">
                       <span class="text-sm font-black text-slate-800">{{ item.score.toFixed(2) }}</span>
                       <div class="flex gap-0.5">
                          <Star v-for="i in 5" :key="i" :size="10" :class="i <= Math.round(item.score) ? 'text-amber-500 fill-amber-500' : 'text-slate-200'" />
                       </div>
                    </div>
                 </div>
                 <div class="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
                    <div 
                      :style="{ width: `${(item.score / 5) * 100}%` }" 
                      class="h-full bg-gradient-to-r from-blue-600 to-indigo-500 rounded-full shadow-sm"
                    ></div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Semester History Chart Mock -->
        <div class="lg-card-glass p-8">
           <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide mb-8">Biến động điểm qua các học kỳ</h4>
           <div class="h-48 flex items-end justify-between px-10 relative">
              <div class="absolute inset-0 flex flex-col justify-between py-2 pointer-events-none opacity-5">
                 <div v-for="i in 4" :key="i" class="h-px w-full bg-slate-800"></div>
              </div>
              
              <div v-for="term in teacher.semesterHistory" :key="term.term" class="flex flex-col items-center gap-4 group">
                 <div 
                   :style="{ height: `${(term.score / 5) * 120}px` }" 
                   class="w-16 bg-blue-100 rounded-2xl group-hover:bg-blue-600 transition-all border border-blue-200 relative"
                 >
                    <div class="absolute -top-8 left-1/2 -translate-x-1/2 text-xs font-black text-slate-800 opacity-0 group-hover:opacity-100 transition-opacity">
                       {{ term.score }}
                    </div>
                 </div>
                 <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ term.term }}</span>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Teacher Context & AI Summary ── -->
      <div class="space-y-6">
        
        <!-- Teacher Profile Card -->
        <div class="lg-card-glass p-8 text-center bg-slate-50/50">
           <div class="h-24 w-24 rounded-[32px] bg-white border border-slate-100 p-1 mx-auto mb-6 shadow-xl">
              <div class="h-full w-full rounded-[28px] bg-slate-50 flex items-center justify-center text-slate-300">
                 <User :size="48" />
              </div>
           </div>
           <h3 class="text-xl font-black text-slate-800">{{ teacher.name }}</h3>
           <p class="text-[10px] font-black text-blue-600 uppercase tracking-[0.2em] mt-1">{{ teacher.code }}</p>
           
           <div class="mt-8 pt-8 border-t border-slate-100 space-y-4 text-left">
              <div class="flex items-center gap-3">
                 <BookOpen :size="16" class="text-slate-400" />
                 <div>
                    <p class="text-[9px] font-black text-slate-400 uppercase">Khoa phụ trách</p>
                    <p class="text-xs font-bold text-slate-700">{{ teacher.dept }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-3">
                 <Award :size="16" class="text-amber-500" />
                 <div>
                    <p class="text-[9px] font-black text-slate-400 uppercase">Thành tích nổi bật</p>
                    <p class="text-xs font-bold text-slate-700">Top 3 giảng viên yêu thích nhất kỳ</p>
                 </div>
              </div>
           </div>
        </div>

        <!-- AI Narrative Summary -->
        <div class="lg-card-glass p-8 bg-indigo-50/20 border-indigo-100">
           <div class="flex items-center gap-3 mb-6">
              <div class="h-10 w-10 rounded-2xl bg-indigo-600 text-white flex items-center justify-center shadow-lg shadow-indigo-200">
                 <MessageCircle :size="20" />
              </div>
              <h4 class="text-sm font-black text-indigo-900 uppercase tracking-wide">Tổng hợp ý kiến AI</h4>
           </div>
           <p class="text-xs text-indigo-700/80 leading-relaxed font-medium italic">
             "Giảng viên có kiến thức chuyên môn cực kỳ vững chắc. Sinh viên đánh giá cao sự nhiệt tình hỗ trợ ngoài giờ lên lớp. Một số nhận xét nhỏ đề xuất giảng viên có thể chia sẻ thêm nhiều ví dụ thực tế từ doanh nghiệp."
           </p>
           <div class="mt-6 flex flex-wrap gap-2">
              <span class="px-2 py-1 rounded-lg bg-emerald-100 text-emerald-700 text-[9px] font-black uppercase tracking-widest">Nhiệt tình</span>
              <span class="px-2 py-1 rounded-lg bg-emerald-100 text-emerald-700 text-[9px] font-black uppercase tracking-widest">Chuyên môn cao</span>
              <span class="px-2 py-1 rounded-lg bg-blue-100 text-blue-700 text-[9px] font-black uppercase tracking-widest">Supportive</span>
           </div>
        </div>

        <!-- Critical Alerts Area -->
        <div v-if="teacher.avgRating < 3.5" class="lg-card-glass p-6 border-rose-100 bg-rose-50/20">
           <div class="flex items-start gap-4 text-rose-600">
              <ShieldAlert :size="24" class="shrink-0 mt-0.5" />
              <div>
                 <h4 class="text-sm font-black text-rose-900 uppercase tracking-wide">Cảnh báo chất lượng</h4>
                 <p class="text-xs text-rose-700 mt-1 font-medium">Giảng viên có điểm đánh giá giảm mạnh so với kỳ trước. BGH cần lên lịch trao đổi chuyên môn.</p>
              </div>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
