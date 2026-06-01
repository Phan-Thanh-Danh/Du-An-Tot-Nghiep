<script setup>
import { ref } from 'vue'
import { 
  Settings2, 
  Plus, 
  ArrowRight, 
  CheckCircle2, 
  Clock, 
  Shield, 
  Zap, 
  FileJson,
  MoreVertical,
  Layers,
  Edit2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const workflows = ref([
  { id: 'WF-01', name: 'Nghỉ học / Bảo lưu', steps: 3, sla: '72h', auto: false, status: 'active' },
  { id: 'WF-02', name: 'Chuyển lớp học phần', steps: 2, sla: '48h', auto: true, status: 'active' },
  { id: 'WF-03', name: 'Cấp giấy xác nhận', steps: 1, sla: '24h', auto: true, status: 'active' },
  { id: 'WF-04', name: 'Thi lại / Hoãn thi', steps: 2, sla: '48h', auto: false, status: 'draft' },
])

const activeWorkflow = ref(workflows.value[1]) // Selecting 'Chuyển lớp' for the builder view
</script>

<template>
  <PageContainer 
    title="Cấu hình quy trình duyệt" 
    subtitle="Thiết lập các bước phê duyệt, vai trò và thời gian xử lý cho từng loại đơn từ."
  >
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <!-- ── Left: Workflow List ── -->
      <div class="space-y-4">
        <div class="flex items-center justify-between mb-2">
            <h4 class="text-xs font-black text-label uppercase tracking-widest">Loại đơn & Workflow</h4>
            <button class="text-[11px] font-black text-link uppercase tracking-widest flex items-center gap-1 hover:underline">
              <Plus :size="12" /> Thêm mới
           </button>
        </div>
        
        <div 
          v-for="wf in workflows" 
          :key="wf.id"
          @click="activeWorkflow = wf"
          :class="[
            'p-5 rounded-3xl border transition-all cursor-pointer group',
            activeWorkflow.id === wf.id 
              ? 'lg-glass-strong border-default' 
              : 'lg-card-glass hover:border-blue-200'
          ]"
        >
           <div class="flex items-start justify-between">
              <div>
                  <p :class="['text-sm font-black leading-tight', activeWorkflow.id === wf.id ? 'text-heading' : 'text-heading']">
                    {{ wf.name }}
                 </p>
                 <div class="flex items-center gap-3 mt-2">
                     <span :class="['text-[10px] font-black uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-label' : 'text-placeholder']">
                        <Layers :size="12" /> {{ wf.steps }} bước
                     </span>
                     <span :class="['text-[10px] font-black uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-label' : 'text-placeholder']">
                       <Clock :size="12" /> SLA {{ wf.sla }}
                    </span>
                 </div>
              </div>
               <div :class="['h-8 w-8 rounded-xl flex items-center justify-center transition-colors', activeWorkflow.id === wf.id ? 'surface-solid text-heading' : 'surface-solid text-placeholder']">
                 <ArrowRight :size="16" />
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Workflow Builder ── -->
      <div class="lg:col-span-2 space-y-4">
        
        <!-- Header Info -->
        <div class="lg-card-glass p-5 flex items-center justify-between">
           <div class="flex items-center gap-5">
               <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body border-default">
                 <Settings2 :size="28" />
              </div>
              <div>
                 <div class="flex items-center gap-3">
                     <h3 class="text-xl font-black text-heading">{{ activeWorkflow.name }}</h3>
                     <span class="lg-badge-success px-2 py-0.5 text-[10px] font-black uppercase tracking-widest">Đang áp dụng</span>
                 </div>
                  <p class="text-xs font-bold text-label mt-1 uppercase tracking-tighter">{{ activeWorkflow.id }} • Phiên bản v2.1.0</p>
              </div>
           </div>
           <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold flex items-center gap-2">
              <Edit2 :size="16" /> Chỉnh sửa
           </button>
        </div>

        <!-- Builder Visualizer -->
         <div class="lg-card-glass p-10 surface-solid relative overflow-hidden">
           
           <div class="flex flex-col items-center gap-12">
              
              <!-- Step 1 -->
               <div class="w-full max-w-sm p-4 lg-glass-soft border-default shadow-sm relative group hover:border-blue-300 transition-all">
                  <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-black shadow-md">1</div>
                  <h5 class="text-sm font-black text-heading uppercase tracking-wide">Tiếp nhận & Kiểm tra</h5>
                 <div class="mt-4 flex items-center justify-between">
                     <div class="flex items-center gap-2 text-[11px] font-bold text-label">
                        <Shield :size="14" class="text-link" /> Vai trò: Giáo vụ khoa
                     </div>
                     <span class="text-[10px] font-black text-warning uppercase tracking-widest bg-warning/10 px-2 py-1 rounded-lg">SLA: 24h</span>
                 </div>
              </div>

               <div class="h-10 w-0.5 border-default flex items-center justify-center">
                  <div class="h-2 w-2 rounded-full bg-link animate-bounce"></div>
               </div>

               <!-- Step 2 -->
                <div class="w-full max-w-sm p-4 lg-glass-soft rounded-3xl border border-blue-200 shadow-md relative group">
                  <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-black shadow-md">2</div>
                 <div class="flex items-center justify-between mb-4">
                     <h5 class="text-sm font-black text-heading uppercase tracking-wide">Phê duyệt chính</h5>
                     <Zap :size="18" class="text-warning fill-warning" title="Auto-Execute Available" />
                 </div>
                 <div class="flex items-center justify-between">
                     <div class="flex items-center gap-2 text-[11px] font-bold text-label">
                        <Shield :size="14" class="text-body" /> Trưởng phòng Giáo vụ
                     </div>
                     <span class="text-[10px] font-black text-warning uppercase tracking-widest bg-warning/10 px-2 py-1 rounded-lg">SLA: 48h</span>
                 </div>
              </div>

               <div class="h-10 w-0.5 border-default flex items-center justify-center">
                  <div class="h-2 w-2 rounded-full bg-link"></div>
              </div>

              <!-- Execution Step -->
               <div class="w-full max-w-sm p-5 surface-solid rounded-3xl border-2 border-dashed border-default relative">
                  <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-black shadow-md">
                    <CheckCircle2 :size="18" />
                 </div>
                 <div class="flex items-center gap-3">
                    <div class="h-10 w-10 rounded-xl lg-glass-soft flex items-center justify-center text-success shadow-sm">
                       <FileJson :size="20" />
                    </div>
                    <div>
                        <h5 class="text-xs font-black text-heading uppercase tracking-widest">Thực thi tự động</h5>
                        <p class="text-[10px] font-bold text-success mt-1 uppercase tracking-tighter">Update DB + Send PDF Email</p>
                    </div>
                 </div>
              </div>

           </div>

           <!-- Floating Tools -->
           <div class="absolute bottom-6 right-6 flex flex-col gap-3">
               <button class="h-10 w-10 lg-card-glass rounded-2xl border-default text-placeholder hover:text-link shadow-sm flex items-center justify-center transition-all">
                  <Plus :size="24" />
               </button>
               <button class="h-10 w-10 surface-solid rounded-2xl text-heading shadow-xl flex items-center justify-center hover:scale-110 transition-all">
                 <MoreVertical :size="24" />
              </button>
           </div>
        </div>

        <!-- Workflow Rules Summary -->
        <div class="lg-card-glass p-4 border-default bg-danger/5">
            <div class="flex items-start gap-4">
               <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body shrink-0">
                  <Shield :size="20" />
               </div>
               <div class="flex-1">
                  <h4 class="text-sm font-black text-heading uppercase tracking-wide">Quy tắc bảo mật Workflow</h4>
                  <p class="text-xs text-body mt-1 leading-relaxed">
                   Các workflow đã phát sinh đơn từ thực tế sẽ <strong>không được phép xóa</strong>. Bạn chỉ có thể đóng (archive) hoặc tạo phiên bản mới để thay đổi cấu trình mà không ảnh hưởng đến dữ liệu lịch sử.
                 </p>
              </div>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
