<script setup>
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { 
  ArrowLeft, 
  CheckCircle2, 
  XCircle, 
  UserPlus, 
  Download, 
  FileText, 
  Clock, 
  History, 
  MessageSquare,
  ShieldCheck,
  Send,
  MoreVertical
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const route = useRoute()
const requestId = route.params.id || 'DON-001'

// ── Mock Data ────────────────────────────────────────────────
const request = ref({
  id: requestId,
  student: 'Nguyễn Văn Nam',
  studentCode: 'SV2024001',
  class: 'SE1601',
  type: 'Chuyển lớp học phần',
  title: 'Xin chuyển từ L01 sang L02 môn Java',
  content: 'Do lịch làm việc part-time bị thay đổi, em xin phép được chuyển sang lớp L02 học vào tối thứ 4 để đảm bảo việc học tập.',
  files: [
    { name: 'Xac-nhan-cong-ty.pdf', size: '1.2MB' },
    { name: 'Bang-diem-ky-truoc.jpg', size: '800KB' }
  ],
  status: 'under_review',
  currentStep: 2,
  workflow: [
    { step: 1, label: 'Gửi đơn', user: 'Sinh viên', date: '12/05 08:30', status: 'completed' },
    { step: 2, label: 'Kiểm tra hồ sơ', user: 'Giáo vụ (Phạm Minh D)', date: '12/05 10:15', status: 'current' },
    { step: 3, label: 'Phê duyệt', user: 'Trưởng phòng Giáo vụ', date: null, status: 'pending' },
    { step: 4, label: 'Thực thi kết quả', user: 'Hệ thống tự động', date: null, status: 'pending' }
  ],
  comments: [
    { id: 1, user: 'Phạm Minh D', role: 'Giáo vụ', text: 'Vui lòng kiểm tra lại sĩ số lớp L02 trước khi duyệt chuyển.', date: '12/05 10:20' }
  ]
})

const getStepStatusClass = (status) => {
  switch (status) {
    case 'completed': return 'bg-(--color-success-bg) text-(--color-success-text) border border-(--color-success-text)/20'
    case 'current': return 'bg-(--color-info-bg) text-(--color-info-text) border border-(--color-info-text)/25 ring-4 ring-(--color-info-bg)'
    case 'pending': return 'surface-solid text-muted border border-default'
    default: return ''
  }
}
</script>

<template>
  <PageContainer 
    :title="`Chi tiết đơn: ${request.id}`" 
    :subtitle="request.type"
  >
    <template #actions>
      <router-link to="/staff/requests" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
        <ArrowLeft :size="18" /> Quay lại
      </router-link>
    </template>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <!-- ── Left: Request Content ── -->
      <div class="xl:col-span-2 space-y-4">
        
        <!-- Main Card -->
        <div class="surface-card border border-card rounded-2xl p-6">
           <div class="flex items-start justify-between mb-6">
              <div class="flex items-center gap-4">
                  <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body shadow-sm border-default">
                     <FileText :size="24" />
                  </div>
                  <div>
                     <h3 class="text-xl font-semibold text-heading leading-tight">{{ request.title }}</h3>
                     <p class="text-sm font-bold text-label mt-1 uppercase tracking-tighter">{{ request.type }}</p>
                  </div>
               </div>
               <button class="p-2 lg-button-ghost rounded-lg"><MoreVertical :size="20" /></button>
           </div>

           <div class="prose prose-slate max-w-none">
               <p class="text-sm text-body leading-relaxed font-medium surface-solid p-4 rounded-2xl border-default">
                {{ request.content }}
              </p>
           </div>

           <!-- Evidence Files -->
           <div class="mt-6">
               <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4">Minh chứng đính kèm</h4>
               <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div v-for="file in request.files" :key="file.name" class="flex items-center justify-between p-4 lg-list-item rounded-2xl border-default group hover:border-(--border-input-focus) transition-all cursor-pointer">
                     <div class="flex items-center gap-3">
                        <div class="h-10 w-10 rounded-xl surface-solid flex items-center justify-center text-placeholder transition-colors">
                           <FileText :size="20" />
                        </div>
                        <div>
                           <p class="text-xs font-bold text-heading truncate max-w-[140px]">{{ file.name }}</p>
                           <p class="text-[10px] text-placeholder uppercase font-semibold tracking-widest mt-0.5">{{ file.size }}</p>
                        </div>
                     </div>
                     <button class="p-2 lg-button-ghost"><Download :size="18" /></button>
                  </div>
               </div>
           </div>
        </div>

        <!-- Comments Section -->
        <div class="surface-card border border-card rounded-2xl p-6">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4 flex items-center gap-2">
               <MessageSquare :size="16" /> Thảo luận xử lý
            </h4>
            <div class="space-y-4">
               <div v-for="cm in request.comments" :key="cm.id" class="flex gap-4">
                  <div class="h-9 w-9 rounded-full surface-solid flex items-center justify-center text-heading text-[10px] font-semibold shrink-0">PM</div>
                  <div class="flex-1">
                     <div class="flex items-center gap-2 mb-1">
                        <span class="text-xs font-semibold text-heading">{{ cm.user }}</span>
                        <span class="px-1.5 py-0.5 rounded-lg lg-badge-info text-[9px] font-semibold uppercase tracking-widest">{{ cm.role }}</span>
                        <span class="text-[10px] font-semibold text-label ml-auto">{{ cm.date }}</span>
                     </div>
                     <p class="text-xs text-body leading-relaxed font-medium">{{ cm.text }}</p>
                  </div>
               </div>

              <!-- Input -->
              <div class="mt-8 relative">
                  <textarea 
                    placeholder="Nhập ghi chú hoặc hướng dẫn xử lý..."
                    class="w-full surface-input border border-input rounded-2xl p-4 text-xs font-medium outline-none h-28 resize-none"
                  ></textarea>
                  <button class="absolute bottom-4 right-4 lg-button-primary p-3 rounded-2xl hover:scale-105 transition-all">
                    <Send :size="18" />
                 </button>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Right: Workflow & Info ── -->
      <div class="space-y-4">
        
        <!-- Workflow Stepper -->
        <div class="surface-card border border-card rounded-2xl p-5 overflow-hidden relative">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-6">Quy trình phê duyệt</h4>
            
            <div class="relative space-y-8">
               <!-- Vertical Line -->
               <div class="absolute left-[17px] top-2 bottom-2 w-0.5 border-default"></div>

              <div v-for="step in request.workflow" :key="step.step" class="relative flex gap-5 z-10">
                 <div :class="['h-9 w-9 rounded-full flex items-center justify-center text-xs font-semibold shrink-0 shadow-sm transition-all', getStepStatusClass(step.status)]">
                    <CheckCircle2 v-if="step.status === 'completed'" :size="16" />
                    <span v-else>{{ step.step }}</span>
                 </div>
                 <div>
                     <h5 :class="['text-sm font-semibold', step.status === 'pending' ? 'text-placeholder' : 'text-heading']">{{ step.label }}</h5>
                     <p class="text-[11px] font-bold text-label mt-0.5">{{ step.user }}</p>
                     <p v-if="step.date" class="text-[10px] font-semibold text-link uppercase tracking-tighter mt-1">{{ step.date }}</p>
                 </div>
              </div>
           </div>
        </div>

        <!-- Student Info Card -->
        <div class="surface-card border border-card rounded-2xl p-4">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4">Thông tin sinh viên</h4>
            <div class="space-y-4">
               <div class="flex items-center gap-4">
                  <div class="h-11 w-11 rounded-full bg-(--color-info-bg) text-(--color-info-text) border border-(--color-info-text)/20 flex items-center justify-center font-semibold text-xs">
                     NN
                  </div>
                  <div>
                     <p class="text-sm font-semibold text-heading">{{ request.student }}</p>
                     <p class="text-[10px] font-semibold text-label uppercase">{{ request.studentCode }}</p>
                  </div>
               </div>
               <div class="h-px border-default"></div>
               <div class="grid grid-cols-2 gap-4">
                  <div>
                     <p class="text-[9px] font-semibold text-label uppercase">Lớp chính</p>
                     <p class="text-xs font-bold text-heading">{{ request.class }}</p>
                  </div>
                  <div>
                     <p class="text-[9px] font-semibold text-label uppercase">Học kỳ</p>
                     <p class="text-xs font-bold text-heading">Spring 2026</p>
                  </div>
               </div>
            </div>
         </div>

         <!-- Main Actions -->
         <div class="space-y-3">
            <button class="w-full lg-button-primary py-4 text-sm font-semibold flex items-center justify-center gap-2">
               <CheckCircle2 :size="20" /> DUYỆT ĐƠN (APPROVE)
            </button>
            <button class="w-full lg-btn-danger py-4 text-sm font-semibold flex items-center justify-center gap-2">
              <XCircle :size="20" /> TỪ CHỐI (REJECT)
           </button>
           <button class="w-full lg-button-secondary py-3 text-xs font-bold flex items-center justify-center gap-2">
              <UserPlus :size="16" /> CHUYỂN NGƯỜI XỬ LÝ
           </button>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
