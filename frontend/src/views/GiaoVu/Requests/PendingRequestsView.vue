<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  FileText, 
  Clock, 
  User, 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  UserPlus, 
  ArrowRight,
  MoreVertical,
  Flag,
  Timer
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const requests = ref([
  { id: 'DON-001', student: 'Nguyễn Văn A', type: 'Chuyển lớp', title: 'Xin chuyển từ L01 sang L02', date: '12/05/2026', reviewer: 'Phạm Minh D', sla: '2h 15m', status: 'under_review', priority: 'high' },
  { id: 'DON-002', student: 'Lê Thị B', type: 'Nghỉ học tạm thời', title: 'Xin bảo lưu kết quả học tập', date: '11/05/2026', reviewer: 'Chưa phân công', sla: '12h 40m', status: 'submitted', priority: 'medium' },
  { id: 'DON-003', student: 'Trần Văn C', type: 'Cấp giấy xác nhận', title: 'Xin giấy xác nhận SV làm thẻ ngân hàng', date: '13/05/2026', reviewer: 'Nguyễn Bích L', sla: '45m', status: 'under_review', priority: 'low' },
  { id: 'DON-004', student: 'Hoàng Thị D', type: 'Thi lại', title: 'Đơn xin thi lại môn Java', date: '10/05/2026', reviewer: 'Trần Văn K', sla: 'QUÁ HẠN', status: 'under_review', priority: 'high' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'submitted': return 'bg-blue-100 text-blue-700 border-blue-200'
    case 'under_review': return 'bg-amber-100 text-amber-700 border-amber-200'
    default: return 'bg-slate-100 text-slate-700 border-slate-200'
  }
}

const getPriorityColor = (priority) => {
  switch (priority) {
    case 'high': return 'text-rose-500'
    case 'medium': return 'text-amber-500'
    case 'low': return 'text-emerald-500'
    default: return 'text-slate-400'
  }
}
</script>

<template>
  <PageContainer 
    title="Đơn từ cần xử lý" 
    subtitle="Quản lý và phê duyệt các yêu cầu hành chính học vụ từ sinh viên."
  >
    <div class="space-y-4">
      
      <!-- ── Quick Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input 
            type="text" 
            placeholder="Tìm theo mã đơn, sinh viên hoặc nội dung..." 
            class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Lọc nâng cao
          </button>
        </div>
      </div>

      <!-- ── Requests Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default w-10">#</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Sinh viên & Loại đơn</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Người xử lý</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">SLA còn lại</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="req in requests" :key="req.id" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                 <Flag :size="16" :class="getPriorityColor(req.priority)" />
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl surface-solid flex items-center justify-center text-placeholder shrink-0">
                    <FileText :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-black text-heading leading-tight">{{ req.type }}</p>
                    <p class="text-[11px] font-bold text-label mt-0.5">{{ req.student }} • {{ req.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <User :size="14" class="text-placeholder" />
                  <span :class="['text-xs font-bold', req.reviewer === 'Chưa phân công' ? 'text-warning' : 'text-label']">
                    {{ req.reviewer }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <Timer :size="14" :class="req.sla === 'QUÁ HẠN' ? 'text-danger' : 'text-placeholder'" />
                  <span :class="['text-xs font-black uppercase tracking-tighter', req.sla === 'QUÁ HẠN' ? 'text-danger' : 'text-label']">
                    {{ req.sla }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border', getStatusBadge(req.status)]">
                  {{ req.status.replace('_', ' ') }}
                </span>
              </td>
              <td class="px-4 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <router-link :to="`/staff/requests/${req.id}`" class="p-2 lg-button-ghost rounded-lg" title="Xem chi tiết">
                    <ArrowRight :size="18" />
                  </router-link>
                  <button class="p-2 lg-button-ghost rounded-lg">
                    <MoreVertical :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── SLA Legend ── -->
      <div class="lg-card-glass p-5 border-danger bg-danger/5">
        <div class="flex items-start gap-4">
          <div class="h-10 w-10 rounded-2xl bg-danger/15 flex items-center justify-center text-danger shrink-0 shadow-sm border border-danger">
             <AlertCircle :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-black text-heading uppercase tracking-wide">Quy tắc xử lý đơn (SLA)</h4>
            <p class="text-xs text-body mt-1.5 leading-relaxed">
              Tất cả các đơn từ đều có thời hạn xử lý (SLA) quy định theo từng loại. Các đơn <strong>QUÁ HẠN</strong> sẽ được hệ thống tự động đẩy lên mức ưu tiên cao nhất và thông báo cho Trưởng phòng Giáo vụ.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
