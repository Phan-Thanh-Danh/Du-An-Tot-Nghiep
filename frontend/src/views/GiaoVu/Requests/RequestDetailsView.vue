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
    case 'completed': return 'bg-emerald-500 text-white'
    case 'current': return 'bg-blue-600 text-white ring-4 ring-blue-100'
    case 'pending': return 'bg-slate-200 text-slate-400'
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

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Request Content ── -->
      <div class="xl:col-span-2 space-y-6">
        
        <!-- Main Card -->
        <div class="lg-card-glass p-8">
           <div class="flex items-start justify-between mb-8">
              <div class="flex items-center gap-4">
                 <div class="h-12 w-12 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600 shadow-sm border border-blue-100">
                    <FileText :size="24" />
                 </div>
                 <div>
                    <h3 class="text-xl font-black text-slate-800 leading-tight">{{ request.title }}</h3>
                    <p class="text-sm font-bold text-slate-400 mt-1 uppercase tracking-tighter">{{ request.type }}</p>
                 </div>
              </div>
              <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400"><MoreVertical :size="20" /></button>
           </div>

           <div class="prose prose-slate max-w-none">
              <p class="text-sm text-slate-600 leading-relaxed font-medium bg-slate-50/50 p-6 rounded-3xl border border-slate-100">
                {{ request.content }}
              </p>
           </div>

           <!-- Evidence Files -->
           <div class="mt-8">
              <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-4">Minh chứng đính kèm</h4>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                 <div v-for="file in request.files" :key="file.name" class="flex items-center justify-between p-4 bg-white rounded-2xl border border-slate-100 group hover:border-blue-200 transition-all cursor-pointer">
                    <div class="flex items-center gap-3">
                       <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-500 transition-colors">
                          <FileText :size="20" />
                       </div>
                       <div>
                          <p class="text-xs font-bold text-slate-700 truncate max-w-[140px]">{{ file.name }}</p>
                          <p class="text-[10px] text-slate-400 uppercase font-black tracking-widest mt-0.5">{{ file.size }}</p>
                       </div>
                    </div>
                    <button class="p-2 text-slate-400 hover:text-blue-600"><Download :size="18" /></button>
                 </div>
              </div>
           </div>
        </div>

        <!-- Comments Section -->
        <div class="lg-card-glass p-8">
           <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-6 flex items-center gap-2">
              <MessageSquare :size="16" /> Thảo luận xử lý
           </h4>
           <div class="space-y-6">
              <div v-for="cm in request.comments" :key="cm.id" class="flex gap-4">
                 <div class="h-9 w-9 rounded-full bg-teal-500 flex items-center justify-center text-white text-[10px] font-black shrink-0 shadow-sm shadow-teal-200">PM</div>
                 <div class="flex-1">
                    <div class="flex items-center gap-2 mb-1">
                       <span class="text-xs font-black text-slate-800">{{ cm.user }}</span>
                       <span class="px-1.5 py-0.5 rounded-lg bg-teal-50 text-teal-600 text-[9px] font-black uppercase tracking-widest">{{ cm.role }}</span>
                       <span class="text-[10px] font-bold text-slate-400 ml-auto">{{ cm.date }}</span>
                    </div>
                    <p class="text-xs text-slate-600 leading-relaxed font-medium">{{ cm.text }}</p>
                 </div>
              </div>

              <!-- Input -->
              <div class="mt-8 relative">
                 <textarea 
                   placeholder="Nhập ghi chú hoặc hướng dẫn xử lý..."
                   class="w-full bg-slate-50 border border-slate-100 rounded-[24px] p-5 text-xs font-medium outline-none focus:ring-4 focus:ring-blue-500/10 h-32 resize-none"
                 ></textarea>
                 <button class="absolute bottom-4 right-4 bg-blue-600 text-white p-3 rounded-2xl shadow-lg shadow-blue-500/20 hover:scale-105 transition-all">
                    <Send :size="18" />
                 </button>
              </div>
           </div>
        </div>

      </div>

      <!-- ── Right: Workflow & Info ── -->
      <div class="space-y-6">
        
        <!-- Workflow Stepper -->
        <div class="lg-card-glass p-8 overflow-hidden relative">
           <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-8">Quy trình phê duyệt</h4>
           
           <div class="relative space-y-10">
              <!-- Vertical Line -->
              <div class="absolute left-[17px] top-2 bottom-2 w-0.5 bg-slate-100"></div>

              <div v-for="step in request.workflow" :key="step.step" class="relative flex gap-5 z-10">
                 <div :class="['h-9 w-9 rounded-full flex items-center justify-center text-xs font-black shrink-0 shadow-sm transition-all', getStepStatusClass(step.status)]">
                    <CheckCircle2 v-if="step.status === 'completed'" :size="16" />
                    <span v-else>{{ step.step }}</span>
                 </div>
                 <div>
                    <h5 :class="['text-sm font-black', step.status === 'pending' ? 'text-slate-400' : 'text-slate-800']">{{ step.label }}</h5>
                    <p class="text-[11px] font-bold text-slate-400 mt-0.5">{{ step.user }}</p>
                    <p v-if="step.date" class="text-[10px] font-black text-blue-600 uppercase tracking-tighter mt-1">{{ step.date }}</p>
                 </div>
              </div>
           </div>
        </div>

        <!-- Student Info Card -->
        <div class="lg-card-glass p-6 bg-slate-50/50">
           <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-6">Thông tin sinh viên</h4>
           <div class="space-y-4">
              <div class="flex items-center gap-4">
                 <div class="h-11 w-11 rounded-full bg-gradient-to-br from-blue-400 to-indigo-500 p-0.5">
                    <div class="h-full w-full rounded-full bg-white flex items-center justify-center text-indigo-600 font-black text-xs">NN</div>
                 </div>
                 <div>
                    <p class="text-sm font-black text-slate-800">{{ request.student }}</p>
                    <p class="text-[10px] font-bold text-slate-400 uppercase">{{ request.studentCode }}</p>
                 </div>
              </div>
              <div class="h-px bg-slate-100"></div>
              <div class="grid grid-cols-2 gap-4">
                 <div>
                    <p class="text-[9px] font-black text-slate-400 uppercase">Lớp chính</p>
                    <p class="text-xs font-bold text-slate-700">{{ request.class }}</p>
                 </div>
                 <div>
                    <p class="text-[9px] font-black text-slate-400 uppercase">Học kỳ</p>
                    <p class="text-xs font-bold text-slate-700">Spring 2026</p>
                 </div>
              </div>
           </div>
        </div>

        <!-- Main Actions -->
        <div class="space-y-3">
           <button class="w-full lg-button-primary bg-emerald-600 py-4 text-sm font-black shadow-lg shadow-emerald-500/20 flex items-center justify-center gap-2">
              <CheckCircle2 :size="20" /> DUYỆT ĐƠN (APPROVE)
           </button>
           <button class="w-full lg-button-secondary bg-white border-rose-100 text-rose-600 py-4 text-sm font-black hover:bg-rose-50 flex items-center justify-center gap-2">
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
