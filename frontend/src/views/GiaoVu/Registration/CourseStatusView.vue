<script setup>
import { ref } from 'vue'
import { 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  MessageSquare, 
  RefreshCw, 
  Search, 
  Filter,
  ArrowRight,
  MoreVertical,
  Mail,
  Users
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingClasses = ref([
  { id: 'LHP003', subject: 'Lập trình Web', enrolled: 12, minEnroll: 15, teacher: 'Lê Văn C', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
  { id: 'LHP008', subject: 'Kỹ năng mềm', enrolled: 8, minEnroll: 20, teacher: 'Trần Thị H', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
])

const cancelledClasses = ref([
  { id: 'LHP012', subject: 'Triết học Mác-Lênin', enrolled: 5, minEnroll: 20, teacher: 'Nguyễn Văn K', status: 'cancelled', date: '12/05/2026' },
])
</script>

<template>
  <PageContainer 
    title="Hủy / Mở lại lớp" 
    subtitle="Xử lý các lớp học phần không đủ sĩ số tối thiểu hoặc cần đóng/mở lại theo nhu cầu."
  >
    <div class="space-y-8">
      
      <!-- ── Pending Cancellation ── -->
      <section class="space-y-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-black text-heading flex items-center gap-2">
            <AlertCircle :size="22" class="text-[var(--lg-danger)]" /> LỚP CHỜ HỦY (DƯỚI MIN ENROLL)
          </h3>
          <span class="px-2 py-0.5 rounded-lg bg-[var(--color-danger-bg)] text-[var(--lg-danger)] text-[10px] font-black uppercase tracking-widest">{{ pendingClasses.length }} Lớp</span>
        </div>

        <div class="grid grid-cols-1 gap-4">
          <div v-for="cls in pendingClasses" :key="cls.id" class="lg-card-glass p-4 flex flex-col md:flex-row md:items-center justify-between gap-4 group hover:border-[var(--color-danger-bg)]/50 transition-all">
            <div class="flex items-center gap-4">
              <div class="h-10 w-10 rounded-2xl bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--lg-danger)] border border-[var(--color-danger-bg)]/50 shrink-0">
                <Users :size="28" />
              </div>
              <div>
                <h4 class="text-base font-black text-heading">{{ cls.subject }}</h4>
                <div class="mt-1 flex items-center gap-3">
                  <span class="text-[10px] font-black text-link uppercase">{{ cls.id }}</span>
                  <span class="h-1 w-1 rounded-full bg-[var(--border-default)]"></span>
                  <span class="text-xs font-bold text-label">{{ cls.teacher }}</span>
                </div>
              </div>
            </div>

            <div class="flex flex-wrap items-center gap-4">
              <div class="px-4 border-x border-default">
                <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Sĩ số hiện tại</p>
                <p class="text-lg font-black text-[var(--lg-danger)]">{{ cls.enrolled }} <span class="text-placeholder font-medium">/ {{ cls.minEnroll }}</span></p>
              </div>

              <div class="flex items-center gap-2">
                <button class="lg-button-secondary px-4 py-2 text-xs font-bold text-[var(--lg-success)] hover:bg-[var(--color-success-bg)]">
                  <CheckCircle2 :size="16" /> Mở lại lớp
                </button>
                <button class="px-5 py-2.5 text-xs font-bold text-white bg-[var(--lg-danger)] hover:opacity-90 rounded-[18px] shadow-lg shadow-[var(--lg-danger)]/20 transition-all flex items-center gap-2">
                  <XCircle :size="16" /> Xác nhận hủy
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ── Recently Cancelled ── -->
      <section class="space-y-4 pt-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-black text-label flex items-center gap-2">
            <RefreshCw :size="20" /> LỊCH SỬ LỚP ĐÃ HỦY
          </h3>
        </div>

        <div class="lg-table-shell overflow-hidden opacity-80 hover:opacity-100 transition-opacity">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="surface-solid">
                <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Lớp học phần</th>
                <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Sĩ số lúc hủy</th>
                <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Ngày hủy</th>
                <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thông báo</th>
                <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="cls in cancelledClasses" :key="cls.id" class="group hover:bg-white/10 transition-colors">
                <td class="px-4 py-4">
                   <p class="text-sm font-black text-heading">{{ cls.subject }}</p>
                   <p class="text-[10px] font-bold text-placeholder mt-1">{{ cls.id }}</p>
                </td>
                <td class="px-4 py-4">
                   <span class="text-sm font-bold text-label">{{ cls.enrolled }} SV</span>
                </td>
                <td class="px-4 py-4">
                   <span class="text-xs font-medium text-label">{{ cls.date }}</span>
                </td>
                <td class="px-4 py-4">
                   <div class="flex items-center gap-1.5 text-[var(--lg-success)]">
                      <Mail :size="14" /> <span class="text-[10px] font-black uppercase tracking-widest">Đã gửi SV</span>
                   </div>
                </td>
                <td class="px-4 py-4 text-right">
                   <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder">
                      <MoreVertical :size="16" />
                   </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- ── Policy Note ── -->
      <div class="lg-card-glass p-4 border border-[var(--color-danger-bg)]/50">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-xl surface-card flex items-center justify-center text-[var(--lg-danger)] shadow-sm border border-[var(--color-danger-bg)]/50 shrink-0">
             <MessageSquare :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-black text-heading">Lưu ý khi hủy lớp</h4>
            <p class="text-xs text-label mt-2 leading-relaxed">
              Khi xác nhận hủy lớp, hệ thống sẽ tự động hoàn trả tín chỉ cho sinh viên, giải phóng phòng học và gửi thông báo qua Email/App. Đối với các sinh viên đã thanh toán học phí cho môn này, hệ thống sẽ tự động tạo <strong>Credit Note</strong> (phiếu khấu trừ) cho đợt đóng tiền tiếp theo.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
