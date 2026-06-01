<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Download, 
  History, 
  CheckCircle2, 
  XCircle, 
  Eye, 
  FileCheck,
  Calendar,
  ArrowUpRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const history = ref([
  { id: 'DON-998', student: 'Vũ Thị E', type: 'Cấp giấy xác nhận', result: 'approved', processedBy: 'Hệ thống tự động', date: '05/05/2026', note: 'Đã sinh PDF và gửi email cho SV.' },
  { id: 'DON-995', student: 'Phạm Văn F', type: 'Thi lại', result: 'rejected', processedBy: 'Nguyễn Bích L', date: '04/05/2026', note: 'Không đủ điều kiện dự thi (vắng quá 20%).' },
  { id: 'DON-990', student: 'Lê Hoàng G', type: 'Chuyển lớp', result: 'approved', processedBy: 'Phạm Minh D', date: '01/05/2026', note: 'Đã cập nhật enrollment sang lớp L04.' },
])

const getResultBadge = (result) => {
  switch (result) {
    case 'approved': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
    case 'rejected': return 'bg-rose-50 text-rose-600 border-rose-100'
    default: return 'bg-slate-50 text-slate-500 border-slate-100'
  }
}
</script>

<template>
  <PageContainer 
    title="Lịch sử xử lý đơn từ" 
    subtitle="Tra cứu và kiểm tra lại kết quả các đơn từ đã được giải quyết."
  >
    <div class="space-y-4">
      
      <!-- ── Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
               <input 
                 type="text" 
                 placeholder="Mã đơn hoặc tên sinh viên..." 
                 class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
               >
           </div>
            <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Calendar :size="18" /> Chọn thời gian
           </button>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
           <Download :size="18" /> Xuất báo cáo
        </button>
      </div>

      <!-- ── History Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Mã đơn</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Sinh viên & Loại đơn</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Kết quả</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Người xử lý</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="h in history" :key="h.id" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                <span class="text-xs font-black text-label uppercase tracking-tighter">{{ h.id }}</span>
              </td>
              <td class="px-4 py-4">
                <div>
                  <p class="text-sm font-black text-heading leading-tight">{{ h.type }}</p>
                  <p class="text-[11px] font-bold text-label mt-0.5">{{ h.student }} • {{ h.date }}</p>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <div :class="['h-6 w-6 rounded-full flex items-center justify-center border', getResultBadge(h.result)]">
                      <CheckCircle2 v-if="h.result === 'approved'" :size="14" />
                      <XCircle v-else :size="14" />
                   </div>
                    <span :class="['text-[10px] font-black uppercase tracking-widest', h.result === 'approved' ? 'text-success' : 'text-danger']">
                      {{ h.result === 'approved' ? 'Phê duyệt' : 'Từ chối' }}
                   </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                    <div class="h-6 w-6 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                      <ArrowUpRight :size="12" />
                   </div>
                    <span class="text-xs font-bold text-label">{{ h.processedBy }}</span>
                </div>
              </td>
              <td class="px-4 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 lg-button-ghost rounded-lg" title="Xem kết quả">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 lg-button-ghost rounded-lg" title="Xem timeline">
                    <History :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Empty State Mock ── -->
      <div v-if="history.length === 0" class="py-20 text-center">
         <div class="h-20 w-20 surface-solid rounded-full flex items-center justify-center text-placeholder mx-auto mb-4">
            <FileCheck :size="40" />
         </div>
         <h3 class="text-lg font-black text-heading">Không tìm thấy đơn đã xử lý</h3>
         <p class="text-sm text-label mt-2">Vui lòng điều chỉnh lại bộ lọc hoặc thời gian tìm kiếm.</p>
      </div>

    </div>
  </PageContainer>
</template>
