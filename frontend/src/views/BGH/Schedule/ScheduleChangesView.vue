<script setup>
import { ref } from 'vue'
import { 
  ArrowLeftRight, 
  Search, 
  Filter, 
  User, 
  Calendar, 
  AlertCircle, 
  History, 
  CheckCircle2, 
  Clock, 
  MoreVertical,
  ArrowRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const changes = ref([
  { id: 'CHG-102', originalDate: '12/05/2026', class: 'SE1601', subject: 'Java Programming', originalTeacher: 'Nguyễn Văn A', replacementTeacher: 'Lê Thị B', makeupDate: '15/05/2026 (18:00)', reason: 'Giảng viên gốc ốm đột xuất', processor: 'Phạm Minh D' },
  { id: 'CHG-098', originalDate: '13/05/2026', class: 'SE1605', subject: 'English 4', originalTeacher: 'Trần Văn K', replacementTeacher: 'Chưa có', makeupDate: 'Bị hủy', reason: 'Lớp không đủ sĩ số tối thiểu', processor: 'Hệ thống tự động' },
  { id: 'CHG-085', originalDate: '11/05/2026', class: 'SE1610', subject: 'Database Design', originalTeacher: 'Hoàng Văn M', replacementTeacher: 'Hoàng Văn M', makeupDate: '14/05/2026 (07:30)', reason: 'Phòng học gốc bị sự cố điện', processor: 'Trần Thị H' },
])
</script>

<template>
  <PageContainer 
    title="Thay đổi & Dạy bù" 
    subtitle="Theo dõi toàn bộ các biến động lịch học phát sinh sau khi Thời khóa biểu đã được công bố."
  >
    <div class="space-y-6">
      
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Mã đơn, lớp hoặc tên giảng viên..." class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Loại thay đổi
           </button>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Calendar :size="18" /> Xem theo ngày
        </button>
      </div>

      <!-- ── Changes Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thông tin lớp học</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Sự thay đổi</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Ngày bù / Kết quả</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lý do & Xử lý</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="chg in changes" :key="chg.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-amber-50 flex items-center justify-center text-amber-600 border border-amber-100">
                    <AlertCircle :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-black text-slate-800 leading-tight">{{ chg.subject }}</p>
                    <p class="text-[11px] font-bold text-slate-400 mt-0.5">{{ chg.class }} • {{ chg.originalDate }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                   <div class="text-center">
                      <p class="text-[10px] font-black text-slate-400 uppercase mb-0.5">Gốc</p>
                      <p class="text-xs font-bold text-slate-700">{{ chg.originalTeacher }}</p>
                   </div>
                   <ArrowLeftRight :size="14" class="text-blue-400" />
                   <div class="text-center">
                      <p class="text-[10px] font-black text-blue-400 uppercase mb-0.5">Thay thế</p>
                      <p class="text-xs font-black text-blue-600">{{ chg.replacementTeacher }}</p>
                   </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                   <Clock :size="14" class="text-slate-300" />
                   <span :class="['text-xs font-black uppercase tracking-tighter', chg.makeupDate === 'Bị hủy' ? 'text-rose-500' : 'text-slate-600']">
                      {{ chg.makeupDate }}
                   </span>
                </div>
              </td>
              <td class="px-6 py-4">
                <p class="text-xs text-slate-600 font-medium leading-relaxed">{{ chg.reason }}</p>
                <div class="flex items-center gap-2 mt-2 text-[10px] font-bold text-slate-400">
                   <User :size="12" /> {{ chg.processor }}
                </div>
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <History :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Change Audit Hint ── -->
      <div class="lg-card-glass p-5 border-amber-100 bg-amber-50/10">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-amber-100 flex items-center justify-center text-amber-600 shrink-0">
               <CheckCircle2 :size="20" />
            </div>
            <div>
               <h4 class="text-sm font-black text-amber-900 uppercase tracking-wide">Quy trình ghi nhận thay đổi</h4>
               <p class="text-xs text-amber-700 mt-1 leading-relaxed">
                 Mọi thay đổi sau khi TKB đã công bố (Publish) đều phải được thực hiện qua đơn xin nghỉ/dạy bù từ Giảng viên và được Giáo vụ phê duyệt. BGH có thể xem lại lịch sử (Audit Log) để đối soát khi cần thiết.
               </p>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
