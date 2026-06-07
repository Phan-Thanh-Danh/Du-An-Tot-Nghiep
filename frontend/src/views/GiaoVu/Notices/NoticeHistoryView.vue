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
    case 'sent': return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[var(--color-success-text)]/20'
    case 'failed': return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20'
    case 'scheduled': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20'
    default: return 'surface-solid text-muted border-default'
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
    <div class="space-y-4">
      
      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input 
            type="text" 
            placeholder="Tìm theo tiêu đề hoặc người gửi..." 
            class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
          >
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Lọc theo kênh
        </button>
      </div>

      <!-- ── History Grid ── -->
      <div class="space-y-3">
        <div 
          v-for="nt in notices" 
          :key="nt.id"
           class="surface-card border border-card rounded-2xl p-4 group hover:border-[var(--border-input-focus)] transition-all"
        >
           <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div class="min-w-0 flex-1">
              <div class="flex items-center gap-2 mb-2">
                <span class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">{{ nt.id }}</span>
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getStatusBadge(nt.status)]">
                   {{ nt.status }}
                </div>
              </div>

              <h3 class="text-base font-semibold text-heading leading-snug group-hover:text-link transition-colors">
                {{ nt.title }}
             </h3>

             <div class="mt-3 flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-x-4">
                 <div class="flex items-center gap-2 text-xs font-bold text-label">
                    <Users :size="14" /> {{ nt.target }}
                 </div>
                 <div class="flex items-center gap-2 text-xs font-bold text-label">
                   <Clock :size="14" /> {{ nt.date }} • {{ nt.author }}
                </div>
             </div>
            </div>

            <div class="flex items-center justify-between gap-4 lg:min-w-[320px]">
              <div class="flex items-center gap-2">
                 <div 
                   v-for="ch in nt.channels" 
                   :key="ch"
                    class="h-7 w-7 rounded-lg surface-solid flex items-center justify-center text-placeholder border-default"
                   :title="ch"
                 >
                    <component :is="getChannelIcon(ch)" :size="14" />
                 </div>
                  <span class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">{{ nt.recipients }} recipients</span>
              </div>
              
              <div class="flex items-center gap-2">
                  <button v-if="nt.status === 'failed'" class="p-2 lg-button-ghost text-[var(--color-danger-text)] rounded-lg transition-colors" title="Thử lại">
                    <RotateCcw :size="16" />
                 </button>
                  <button class="p-2 lg-button-ghost text-link rounded-lg transition-colors flex items-center gap-1 text-xs font-semibold uppercase tracking-widest">
                    Detail <ChevronRight :size="14" />
                 </button>
                  <button class="p-2 lg-button-ghost text-placeholder hover:text-heading rounded-lg transition-colors"><MoreVertical :size="18" /></button>
              </div>
           </div>
           </div>
        </div>
      </div>

      <!-- ── Empty State ── -->
      <div v-if="notices.length === 0" class="py-24 text-center">
         <div class="h-20 w-20 surface-solid rounded-3xl flex items-center justify-center text-placeholder mx-auto mb-4">
            <Bell :size="40" />
         </div>
         <h3 class="text-xl font-semibold text-heading tracking-tight">Chưa có thông báo nào được gửi</h3>
         <p class="text-sm text-label mt-2 max-w-xs mx-auto">Hãy bắt đầu bằng việc tạo một thông báo học vụ mới cho sinh viên hoặc giảng viên.</p>
      </div>

    </div>
  </PageContainer>
</template>
