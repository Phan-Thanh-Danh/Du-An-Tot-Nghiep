<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Eye, 
  RotateCcw, 
  Mail, 
  Smartphone, 
  Bell, 
  Users,
  CheckCircle2,
  XCircle,
  Clock,
  MoreVertical,
  ChevronRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const notices = ref([
  { id: 'NT-001', title: 'Thay đổi lịch học môn Java Lab 2', author: 'Phạm Minh D', target: 'Lớp L01, L02', channels: ['in-app', 'email'], date: '12/05 08:30', status: 'sent', recipients: 124 },
  { id: 'NT-002', title: 'Thông báo kết quả học bổng kỳ Fall', author: 'Nguyễn Bích L', target: 'Sinh viên đạt học bổng', channels: ['email', 'push'], date: '10/05 15:45', status: 'sent', recipients: 45 },
  { id: 'NT-003', title: 'Cảnh báo quá hạn đăng ký môn học', author: 'Hệ thống', target: 'SV chưa đăng ký', channels: ['push'], date: '08/05 09:00', status: 'failed', recipients: 312 },
  { id: 'NT-004', title: 'Lịch thi học kỳ Spring 2026', author: 'Trần Văn K', target: 'Toàn bộ SV Campus HCM', channels: ['in-app', 'email', 'push'], date: '15/05 08:00', status: 'scheduled', recipients: 4500 },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'sent': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
    case 'failed': return 'bg-rose-50 text-rose-600 border-rose-100'
    case 'scheduled': return 'bg-blue-50 text-blue-600 border-blue-100'
    default: return 'bg-slate-50 text-slate-500 border-slate-100'
  }
}

const getChannelIcon = (ch) => {
  switch (ch) {
    case 'in-app': return Bell
    case 'email': return Mail
    case 'push': return Smartphone
    default: return Bell
  }
}
</script>

<template>
  <PageContainer 
    title="Lịch sử thông báo" 
    subtitle="Xem lại danh sách và trạng thái gửi của các thông báo học vụ đã phát hành."
  >
    <div class="space-y-6">
      
      <!-- ── Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo tiêu đề hoặc người gửi..." 
            class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10"
          >
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Lọc theo kênh
        </button>
      </div>

      <!-- ── History Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div 
          v-for="nt in notices" 
          :key="nt.id"
          class="lg-card-glass p-6 group hover:shadow-xl hover:shadow-slate-200/50 transition-all border-slate-100"
        >
           <div class="flex items-start justify-between mb-5">
              <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest border shadow-sm', getStatusBadge(nt.status)]">
                 {{ nt.status }}
              </div>
              <button class="text-slate-300 hover:text-slate-500"><MoreVertical :size="18" /></button>
           </div>

           <h3 class="text-base font-black text-slate-800 leading-snug group-hover:text-blue-600 transition-colors">
              {{ nt.title }}
           </h3>
           
           <div class="mt-4 flex flex-col gap-3">
              <div class="flex items-center gap-2 text-xs font-bold text-slate-400">
                 <Users :size="14" /> {{ nt.target }}
              </div>
              <div class="flex items-center gap-2 text-xs font-bold text-slate-400">
                 <Clock :size="14" /> {{ nt.date }} • {{ nt.author }}
              </div>
           </div>

           <div class="mt-6 pt-5 border-t border-slate-50 flex items-center justify-between">
              <div class="flex items-center gap-2">
                 <div 
                   v-for="ch in nt.channels" 
                   :key="ch"
                   class="h-7 w-7 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400 border border-slate-100"
                   :title="ch"
                 >
                    <component :is="getChannelIcon(ch)" :size="14" />
                 </div>
                 <span class="text-[10px] font-black text-slate-300 uppercase tracking-widest ml-1">{{ nt.recipients }} recipients</span>
              </div>
              
              <div class="flex items-center gap-2">
                 <button v-if="nt.status === 'failed'" class="p-2 hover:bg-rose-50 text-rose-500 rounded-lg transition-colors" title="Thử lại">
                    <RotateCcw :size="16" />
                 </button>
                 <button class="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors flex items-center gap-1 text-xs font-black uppercase tracking-widest">
                    Detail <ChevronRight :size="14" />
                 </button>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Empty State ── -->
      <div v-if="notices.length === 0" class="py-24 text-center">
         <div class="h-20 w-20 bg-slate-50 rounded-3xl flex items-center justify-center text-slate-200 mx-auto mb-6">
            <Bell :size="40" />
         </div>
         <h3 class="text-xl font-black text-slate-800 tracking-tight">Chưa có thông báo nào được gửi</h3>
         <p class="text-sm text-slate-400 mt-2 max-w-xs mx-auto">Hãy bắt đầu bằng việc tạo một thông báo học vụ mới cho sinh viên hoặc giảng viên.</p>
      </div>

    </div>
  </PageContainer>
</template>
