<script setup>
import { ref } from 'vue'
import { 
  Clock, 
  Search, 
  Filter, 
  Eye, 
  Undo2, 
  ArrowRight,
  MoreVertical,
  Calendar,
  User,
  ExternalLink
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingSchedules = ref([
  { id: 1, semester: 'Spring 2026', campus: 'Cơ sở chính', classCount: 45, scheduleCount: 320, requester: 'Trần Thị Giáo Vụ', date: '12/05/2026', status: 'pending' },
  { id: 2, semester: 'Spring 2026', campus: 'Cơ sở phụ', classCount: 12, scheduleCount: 84, requester: 'Lê Văn Giáo Vụ', date: '11/05/2026', status: 'pending' },
])

const getStatusBadge = (status) => {
  return 'bg-amber-100 text-amber-700 border border-amber-200'
}
</script>

<template>
  <PageContainer 
    title="Lịch chờ duyệt" 
    subtitle="Danh sách các thời khóa biểu đã gửi lên Ban giám hiệu chờ phê duyệt."
  >
    <div class="space-y-6">
      
      <!-- ── Search & Filter ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo học kỳ hoặc người gửi..." 
            class="w-full bg-white border border-slate-100 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Bộ lọc
          </button>
        </div>
      </div>

      <!-- ── Pending Cards ── -->
      <div class="grid grid-cols-1 gap-4">
        <div v-for="item in pendingSchedules" :key="item.id" class="lg-card-glass p-6 flex flex-col md:flex-row md:items-center justify-between gap-6 group hover:border-amber-200 transition-all">
          <div class="flex items-center gap-6">
            <div class="h-16 w-16 rounded-[24px] bg-amber-50 flex items-center justify-center text-amber-600 border border-amber-100 shrink-0">
              <Calendar :size="32" />
            </div>
            <div>
              <div class="flex items-center gap-3">
                <h3 class="text-lg font-black text-slate-800">TKB Học kỳ {{ item.semester }}</h3>
                <span :class="['px-2 py-0.5 rounded-full text-[10px] font-black uppercase tracking-widest', getStatusBadge(item.status)]">
                  Chờ duyệt
                </span>
              </div>
              <div class="mt-2 flex flex-wrap items-center gap-x-6 gap-y-1">
                <span class="flex items-center gap-1.5 text-xs font-bold text-slate-500">
                   <Clock :size="14" class="text-slate-400" /> {{ item.date }}
                </span>
                <span class="flex items-center gap-1.5 text-xs font-bold text-slate-500">
                   <User :size="14" class="text-slate-400" /> {{ item.requester }}
                </span>
                <span class="text-xs font-black text-blue-600 uppercase">{{ item.campus }}</span>
              </div>
            </div>
          </div>

          <div class="flex flex-col sm:flex-row items-center gap-6">
            <div class="flex items-center gap-8 text-center px-6 border-x border-slate-100">
              <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Số lớp</p>
                <p class="text-lg font-black text-slate-700">{{ item.classCount }}</p>
              </div>
              <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Số tiết</p>
                <p class="text-lg font-black text-slate-700">{{ item.scheduleCount }}</p>
              </div>
            </div>

            <div class="flex items-center gap-2">
              <button class="lg-button-secondary px-4 py-2 text-sm font-bold">
                <Undo2 :size="16" /> Thu hồi
              </button>
              <button class="lg-button-primary px-5 py-2.5 text-sm font-bold bg-blue-600 shadow-lg shadow-blue-500/20">
                <Eye :size="18" /> Chi tiết
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Workflow Note ── -->
      <div class="lg-glass-strong p-6 rounded-[24px] border-amber-100 bg-amber-50/30">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-full bg-white flex items-center justify-center text-amber-500 shadow-sm border border-amber-100 shrink-0">
             <ExternalLink :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-black text-amber-900">Thông tin quy trình</h4>
            <p class="text-xs text-amber-700 mt-1 leading-relaxed">
              Sau khi bạn gửi duyệt, Ban giám hiệu sẽ nhận được thông báo. Bạn không thể chỉnh sửa trực tiếp lịch học khi đang trong trạng thái chờ duyệt. Nếu cần chỉnh sửa khẩn cấp, vui lòng sử dụng nút <strong>"Thu hồi"</strong> để chuyển lịch về trạng thái Draft.
            </p>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>
</template>
