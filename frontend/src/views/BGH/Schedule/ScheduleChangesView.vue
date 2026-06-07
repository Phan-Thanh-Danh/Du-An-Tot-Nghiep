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
  Eye
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
    <div class="space-y-4">
      
      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Mã đơn, lớp hoặc tên giảng viên..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-[var(--border-input-focus)]">
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
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Thông tin lớp học</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Sự thay đổi</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Ngày bù / Kết quả</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Lý do & Xử lý</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="chg in changes" :key="chg.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)] border border-[var(--color-warning-text)]/20">
                    <AlertCircle :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ chg.subject }}</p>
                    <p class="text-[11px] font-bold text-muted mt-0.5">{{ chg.class }} • {{ chg.originalDate }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                   <div class="text-center">
                      <p class="text-[10px] font-semibold text-muted uppercase mb-0.5">Gốc</p>
                      <p class="text-xs font-bold text-label">{{ chg.originalTeacher }}</p>
                   </div>
                   <ArrowLeftRight :size="14" class="text-link" />
                   <div class="text-center">
                      <p class="text-[10px] font-semibold text-link uppercase mb-0.5">Thay thế</p>
                      <p class="text-xs font-semibold text-link">{{ chg.replacementTeacher }}</p>
                   </div>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                   <Clock :size="14" class="text-placeholder" />
                   <span :class="['text-xs font-semibold uppercase tracking-tighter', chg.makeupDate === 'Bị hủy' ? 'text-[var(--color-danger-text)]' : 'text-label']">
                      {{ chg.makeupDate }}
                   </span>
                </div>
              </td>
              <td class="px-4 py-3">
                <p class="text-xs text-body font-medium leading-relaxed">{{ chg.reason }}</p>
                <div class="flex items-center gap-2 mt-2 text-[10px] font-bold text-muted">
                   <User :size="12" /> {{ chg.processor }}
                </div>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-muted transition-all">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted transition-all">
                    <History :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Change Audit Hint ── -->
      <div class="surface-card border border-[var(--color-warning-text)]/20 bg-[var(--color-warning-bg)] rounded-2xl p-5">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-warning-text)] shrink-0 border border-[var(--color-warning-text)]/20">
               <CheckCircle2 :size="20" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Quy trình ghi nhận thay đổi</h4>
               <p class="text-xs text-[var(--color-warning-text)] mt-1 leading-relaxed">
                 Mọi thay đổi sau khi TKB đã công bố (Publish) đều phải được thực hiện qua đơn xin nghỉ/dạy bù từ Giảng viên và được Giáo vụ phê duyệt. BGH có thể xem lại lịch sử (Audit Log) để đối soát khi cần thiết.
               </p>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
