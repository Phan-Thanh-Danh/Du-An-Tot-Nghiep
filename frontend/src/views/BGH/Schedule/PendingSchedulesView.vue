<script setup>
import { ref } from 'vue'
import { 
  CheckCircle2, 
  XCircle, 
  Eye, 
  Search, 
  Filter, 
  AlertTriangle, 
  Clock, 
  User,
  Building2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingSets = ref([
  { id: 'TKB-001', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa CNTT', classes: 86, slots: 420, conflicts: 0, sender: 'Phạm Minh D', date: '12/05/2026', status: 'pending_approval' },
  { id: 'TKB-002', semester: 'Spring 2026', campus: 'Cơ sở 2', dept: 'Khoa Kinh tế', classes: 42, slots: 215, conflicts: 3, sender: 'Nguyễn Bích L', date: '11/05/2026', status: 'pending_approval' },
  { id: 'TKB-003', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa Ngoại ngữ', classes: 65, slots: 310, conflicts: 1, sender: 'Trần Văn K', date: '13/05/2026', status: 'pending_approval' },
])
</script>

<template>
  <PageContainer 
    title="Thời khóa biểu chờ duyệt" 
    subtitle="Ban giám hiệu phê duyệt các bộ Thời khóa biểu do các Khoa/Phòng giáo vụ trình duyệt."
  >
    <div class="space-y-4">
      
      <!-- ── Filters ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input 
                type="text" 
                placeholder="Tìm theo học kỳ, khoa..." 
                class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-[var(--border-input-focus)]"
              >
           </div>
           <select class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none">
              <option>Spring 2026</option>
              <option>Fall 2025</option>
           </select>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Bộ lọc nâng cao
        </button>
      </div>

      <!-- ── Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Khoa / Bộ phận</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Học kỳ & CS</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Quy mô</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Xung đột</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Người gửi</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="set in pendingSets" :key="set.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] border border-[var(--color-info-text)]/20">
                    <Building2 :size="18" />
                  </div>
                  <p class="text-sm font-semibold text-heading leading-tight">{{ set.dept }}</p>
                </div>
              </td>
              <td class="px-4 py-3">
                <div>
                  <p class="text-xs font-semibold text-label leading-tight">{{ set.semester }}</p>
                  <p class="text-[10px] font-bold text-muted mt-0.5">{{ set.campus }}</p>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex flex-col gap-1">
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-tighter">Lớp: {{ set.classes }}</span>
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-tighter">Lịch: {{ set.slots }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div v-if="set.conflicts === 0" class="inline-flex items-center gap-1.5 rounded-lg border border-[var(--color-success-text)]/20 bg-[var(--color-success-bg)] px-2 py-1 text-[var(--color-success-text)]">
                   <CheckCircle2 :size="14" />
                   <span class="text-[10px] font-semibold uppercase tracking-widest">Sẵn sàng</span>
                </div>
                <div v-else class="inline-flex items-center gap-1.5 rounded-lg border border-[var(--color-danger-text)]/20 bg-[var(--color-danger-bg)] px-2 py-1 text-[var(--color-danger-text)]">
                   <AlertTriangle :size="14" />
                   <span class="text-[10px] font-semibold uppercase tracking-widest">{{ set.conflicts }} lỗi nghiêm trọng</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2 text-xs font-bold text-label">
                   <User :size="14" /> {{ set.sender }}
                </div>
                <p class="text-[10px] font-bold text-muted mt-1">{{ set.date }}</p>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-muted transition-all" title="Xem chi tiết TKB">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-success-bg)] hover:text-[var(--color-success-text)] rounded-lg text-muted transition-all" title="Phê duyệt">
                    <CheckCircle2 :size="18" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--color-danger-text)] rounded-lg text-muted transition-all" title="Từ chối">
                    <XCircle :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Approval Policy ── -->
      <div class="surface-card border border-[var(--color-info-text)]/20 bg-[var(--color-info-bg)] rounded-2xl p-4">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-info-text)] shrink-0 border border-[var(--color-info-text)]/20">
               <Clock :size="20" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Chính sách phê duyệt TKB</h4>
               <p class="text-xs text-[var(--color-info-text)] mt-1 leading-relaxed">
                 Hệ thống chỉ cho phép <strong>Duyệt & Publish</strong> bộ TKB khi số lượng xung đột nghiêm trọng (trùng phòng, trùng giảng viên) bằng 0. Nếu bộ TKB còn tồn tại xung đột, BGH vui lòng gửi yêu cầu Giáo vụ chỉnh sửa (Reject with comment).
               </p>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
