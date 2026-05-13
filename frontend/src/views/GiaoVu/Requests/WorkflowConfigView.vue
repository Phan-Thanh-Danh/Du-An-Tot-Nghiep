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
           <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest">Loại đơn & Workflow</h4>
           <button class="text-[11px] font-black text-blue-600 uppercase tracking-widest flex items-center gap-1 hover:underline">
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
              ? 'bg-blue-600 border-blue-600 shadow-lg shadow-blue-500/20' 
              : 'bg-white border-slate-100 hover:border-blue-200 shadow-sm'
          ]"
        >
           <div class="flex items-start justify-between">
              <div>
                 <p :class="['text-sm font-black leading-tight', activeWorkflow.id === wf.id ? 'text-white' : 'text-slate-800']">
                    {{ wf.name }}
                 </p>
                 <div class="flex items-center gap-3 mt-2">
                    <span :class="['text-[10px] font-black uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-blue-100' : 'text-slate-400']">
                       <Layers :size="12" /> {{ wf.steps }} bước
                    </span>
                    <span :class="['text-[10px] font-black uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-blue-100' : 'text-slate-400']">
                       <Clock :size="12" /> SLA {{ wf.sla }}
                    </span>
                 </div>
              </div>
              <div :class="['h-8 w-8 rounded-xl flex items-center justify-center transition-colors', activeWorkflow.id === wf.id ? 'bg-white/20 text-white' : 'bg-slate-50 text-slate-400']">
                 <ArrowRight :size="16" />
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Workflow Builder ── -->
      <div class="lg:col-span-2 space-y-6">
        
        <!-- Header Info -->
        <div class="lg-card-glass p-8 flex items-center justify-between">
           <div class="flex items-center gap-5">
              <div class="h-14 w-14 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600 border border-blue-100">
                 <Settings2 :size="28" />
              </div>
              <div>
                 <div class="flex items-center gap-3">
                    <h3 class="text-xl font-black text-slate-800">{{ activeWorkflow.name }}</h3>
                    <span class="px-2 py-0.5 rounded-lg bg-emerald-50 text-emerald-600 text-[10px] font-black uppercase tracking-widest border border-emerald-100">Đang áp dụng</span>
                 </div>
                 <p class="text-xs font-bold text-slate-400 mt-1 uppercase tracking-tighter">{{ activeWorkflow.id }} • Phiên bản v2.1.0</p>
              </div>
           </div>
           <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold flex items-center gap-2">
              <Edit2 :size="16" /> Chỉnh sửa
           </button>
        </div>

        <!-- Builder Visualizer -->
        <div class="lg-card-glass p-10 bg-slate-50/30 relative overflow-hidden">
           
           <div class="flex flex-col items-center gap-12">
              
              <!-- Step 1 -->
              <div class="w-full max-w-sm p-6 bg-white rounded-3xl border border-slate-200 shadow-sm relative group hover:border-blue-300 transition-all">
                 <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full bg-slate-800 text-white flex items-center justify-center text-xs font-black shadow-md">1</div>
                 <h5 class="text-sm font-black text-slate-800 uppercase tracking-wide">Tiếp nhận & Kiểm tra</h5>
                 <div class="mt-4 flex items-center justify-between">
                    <div class="flex items-center gap-2 text-[11px] font-bold text-slate-500">
                       <Shield :size="14" class="text-blue-500" /> Vai trò: Giáo vụ khoa
                    </div>
                    <span class="text-[10px] font-black text-amber-600 uppercase tracking-widest bg-amber-50 px-2 py-1 rounded-lg">SLA: 24h</span>
                 </div>
              </div>

              <div class="h-10 w-0.5 bg-blue-100 flex items-center justify-center">
                 <div class="h-2 w-2 rounded-full bg-blue-400 animate-bounce"></div>
              </div>

              <!-- Step 2 -->
              <div class="w-full max-w-sm p-6 bg-white rounded-3xl border border-blue-200 shadow-md relative group">
                 <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full bg-blue-600 text-white flex items-center justify-center text-xs font-black shadow-md">2</div>
                 <div class="flex items-center justify-between mb-4">
                    <h5 class="text-sm font-black text-blue-700 uppercase tracking-wide">Phê duyệt chính</h5>
                    <Zap :size="18" class="text-amber-500 fill-amber-500" title="Auto-Execute Available" />
                 </div>
                 <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2 text-[11px] font-bold text-slate-500">
                       <Shield :size="14" class="text-indigo-500" /> Trưởng phòng Giáo vụ
                    </div>
                    <span class="text-[10px] font-black text-amber-600 uppercase tracking-widest bg-amber-50 px-2 py-1 rounded-lg">SLA: 48h</span>
                 </div>
              </div>

              <div class="h-10 w-0.5 bg-blue-100 flex items-center justify-center">
                 <div class="h-2 w-2 rounded-full bg-blue-400"></div>
              </div>

              <!-- Execution Step -->
              <div class="w-full max-w-sm p-5 bg-emerald-50 rounded-3xl border-2 border-dashed border-emerald-200 relative">
                 <div class="absolute -left-3 top-1/2 -translate-y-1/2 h-8 w-8 rounded-full bg-emerald-500 text-white flex items-center justify-center text-xs font-black shadow-md">
                    <CheckCircle2 :size="18" />
                 </div>
                 <div class="flex items-center gap-3">
                    <div class="h-10 w-10 rounded-xl bg-white flex items-center justify-center text-emerald-600 shadow-sm">
                       <FileJson :size="20" />
                    </div>
                    <div>
                       <h5 class="text-xs font-black text-emerald-800 uppercase tracking-widest">Thực thi tự động</h5>
                       <p class="text-[10px] font-bold text-emerald-600 mt-1 uppercase tracking-tighter">Update DB + Send PDF Email</p>
                    </div>
                 </div>
              </div>

           </div>

           <!-- Floating Tools -->
           <div class="absolute bottom-6 right-6 flex flex-col gap-3">
              <button class="h-12 w-12 bg-white rounded-2xl border border-slate-200 text-slate-400 hover:text-blue-600 shadow-sm flex items-center justify-center transition-all">
                 <Plus :size="24" />
              </button>
              <button class="h-12 w-12 bg-slate-800 rounded-2xl text-white shadow-xl flex items-center justify-center hover:scale-110 transition-all">
                 <MoreVertical :size="24" />
              </button>
           </div>
        </div>

        <!-- Workflow Rules Summary -->
        <div class="lg-card-glass p-6 border-blue-100 bg-blue-50/10">
           <div class="flex items-start gap-4">
              <div class="h-10 w-10 rounded-2xl bg-blue-100 flex items-center justify-center text-blue-600 shrink-0">
                 <Shield :size="20" />
              </div>
              <div class="flex-1">
                 <h4 class="text-sm font-black text-blue-900 uppercase tracking-wide">Quy tắc bảo mật Workflow</h4>
                 <p class="text-xs text-blue-700 mt-1 leading-relaxed">
                   Các workflow đã phát sinh đơn từ thực tế sẽ <strong>không được phép xóa</strong>. Bạn chỉ có thể đóng (archive) hoặc tạo phiên bản mới để thay đổi cấu trình mà không ảnh hưởng đến dữ liệu lịch sử.
                 </p>
              </div>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
