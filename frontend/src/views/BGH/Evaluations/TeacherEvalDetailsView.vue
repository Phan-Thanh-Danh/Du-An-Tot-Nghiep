<script setup>
import { ref } from 'vue'
import { 
  ArrowLeft, 
  Star, 
  Download, 
  ShieldAlert, 
  User, 
  BookOpen, 
  MessageCircle,
  Award
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
         <button class="lg-button-primary py-2.5 px-5 text-sm font-semibold flex items-center gap-2">
            <Download :size="18" /> XUẤT BÁO CÁO PDF
         </button>
      </div>
    </template>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Criteria Breakdown ── -->
      <div class="xl:col-span-2 space-y-4">
        <div class="surface-card border border-card rounded-2xl p-5 relative overflow-hidden">
           <!-- Rating Badge Overlay -->
           <div class="absolute top-8 right-8 text-right">
              <div class="inline-flex items-center gap-2 bg-(--color-success-bg) text-(--color-success-text) px-4 py-2 rounded-2xl border border-(--color-success-text)/20 shadow-sm">
                 <Star :size="20" class="fill-(--color-success-text)" />
                 <span class="text-xl font-semibold">{{ teacher.avgRating }}</span>
              </div>
              <p class="text-[10px] font-semibold text-(--color-success-text) uppercase tracking-widest mt-2">Xếp loại: Xuất sắc</p>
           </div>

           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-10">Đánh giá theo tiêu chí</h4>
           
           <div class="space-y-8">
              <div v-for="item in teacher.criteria" :key="item.label">
                 <div class="flex items-center justify-between mb-3">
                    <span class="text-xs font-semibold text-label uppercase tracking-tighter">{{ item.label }}</span>
                    <div class="flex items-center gap-2">
                       <span class="text-sm font-semibold text-heading">{{ item.score.toFixed(2) }}</span>
                       <div class="flex gap-0.5">
                          <Star v-for="i in 5" :key="i" :size="10" :class="i <= Math.round(item.score) ? 'text-(--color-warning-text) fill-(--color-warning-text)' : 'text-placeholder'" />
                       </div>
                    </div>
                 </div>
                 <div class="h-2 w-full bg-(--surface-input) rounded-full overflow-hidden">
                    <div 
                      :style="{ width: `${(item.score / 5) * 100}%` }" 
                      class="h-full bg-(--lg-primary) rounded-full shadow-sm"
                    ></div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Semester History Chart Mock -->
        <div class="surface-card border border-card rounded-2xl p-8">
           <h4 class="text-sm font-semibold text-heading uppercase tracking-wide mb-8">Biến động điểm qua các học kỳ</h4>
           <div class="h-48 flex items-end justify-between px-10 relative">
              <div class="absolute inset-0 flex flex-col justify-between py-2 pointer-events-none opacity-5">
                 <div v-for="i in 4" :key="i" class="h-px w-full bg-(--border-default)"></div>
              </div>
              
              <div v-for="term in teacher.semesterHistory" :key="term.term" class="flex flex-col items-center gap-4 group">
                 <div 
                   :style="{ height: `${(term.score / 5) * 120}px` }" 
                   class="w-16 bg-(--color-info-bg) rounded-2xl group-hover:bg-(--lg-primary) transition-all border border-(--color-info-text)/20 relative"
                 >
                    <div class="absolute -top-8 left-1/2 -translate-x-1/2 text-xs font-semibold text-heading opacity-0 group-hover:opacity-100 transition-opacity">
                       {{ term.score }}
                    </div>
                 </div>
                 <span class="text-[10px] font-semibold text-muted uppercase tracking-widest">{{ term.term }}</span>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Teacher Context & AI Summary ── -->
      <div class="space-y-4">
        
        <!-- Teacher Profile Card -->
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
           <div class="h-24 w-24 rounded-2xl surface-card border border-card p-1 mx-auto mb-4 shadow-sm">
              <div class="h-full w-full rounded-2xl surface-solid flex items-center justify-center text-placeholder">
                 <User :size="48" />
              </div>
           </div>
           <h3 class="text-xl font-semibold text-heading">{{ teacher.name }}</h3>
           <p class="text-[10px] font-semibold text-link uppercase tracking-[0.2em] mt-1">{{ teacher.code }}</p>
           
           <div class="mt-8 pt-8 border-t border-default space-y-4 text-left">
              <div class="flex items-center gap-3">
                 <BookOpen :size="16" class="text-muted" />
                 <div>
                    <p class="text-[9px] font-semibold text-muted uppercase">Khoa phụ trách</p>
                    <p class="text-xs font-bold text-label">{{ teacher.dept }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-3">
                 <Award :size="16" class="text-(--color-warning-text)" />
                 <div>
                    <p class="text-[9px] font-semibold text-muted uppercase">Thành tích nổi bật</p>
                    <p class="text-xs font-bold text-label">Top 3 giảng viên yêu thích nhất kỳ</p>
                 </div>
              </div>
           </div>
        </div>

        <!-- AI Narrative Summary -->
        <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-5">
           <div class="flex items-center gap-3 mb-4">
              <div class="h-10 w-10 rounded-2xl bg-(--surface-card) text-(--color-info-text) flex items-center justify-center shadow-sm border border-(--color-info-text)/20">
                 <MessageCircle :size="20" />
              </div>
              <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Tổng hợp ý kiến AI</h4>
           </div>
           <p class="text-xs text-(--color-info-text) leading-relaxed font-medium italic">
             "Giảng viên có kiến thức chuyên môn cực kỳ vững chắc. Sinh viên đánh giá cao sự nhiệt tình hỗ trợ ngoài giờ lên lớp. Một số nhận xét nhỏ đề xuất giảng viên có thể chia sẻ thêm nhiều ví dụ thực tế từ doanh nghiệp."
           </p>
           <div class="mt-6 flex flex-wrap gap-2">
              <span class="px-2 py-1 rounded-lg bg-(--color-success-bg) text-(--color-success-text) text-[9px] font-semibold uppercase tracking-widest border border-(--color-success-text)/20">Nhiệt tình</span>
              <span class="px-2 py-1 rounded-lg bg-(--color-success-bg) text-(--color-success-text) text-[9px] font-semibold uppercase tracking-widest border border-(--color-success-text)/20">Chuyên môn cao</span>
              <span class="px-2 py-1 rounded-lg bg-(--color-info-bg) text-(--color-info-text) text-[9px] font-semibold uppercase tracking-widest border border-(--color-info-text)/20">Supportive</span>
           </div>
        </div>

        <!-- Critical Alerts Area -->
        <div v-if="teacher.avgRating < 3.5" class="surface-card border border-(--color-danger-text)/20 bg-(--color-danger-bg) rounded-2xl p-4">
           <div class="flex items-start gap-4 text-(--color-danger-text)">
              <ShieldAlert :size="24" class="shrink-0 mt-0.5" />
              <div>
                 <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Cảnh báo chất lượng</h4>
                 <p class="text-xs text-(--color-danger-text) mt-1 font-medium">Giảng viên có điểm đánh giá giảm mạnh so với kỳ trước. BGH cần lên lịch trao đổi chuyên môn.</p>
              </div>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
