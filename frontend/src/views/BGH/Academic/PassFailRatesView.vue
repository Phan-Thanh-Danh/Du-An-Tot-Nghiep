<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  ChevronRight, 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  BookOpen, 
  Users, 
  ArrowUpRight,
  TrendingDown,
  Info
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const courseStats = ref([
  { id: 1, subject: 'Java Programming', class: 'SE1601', teacher: 'Nguyễn Văn A', total: 40, pass: 32, fail: 8, failRate: 20, reason: 'Điểm Final thấp (Trắc nghiệm)' },
  { id: 2, subject: 'Web Advanced', class: 'SE1602', teacher: 'Trần Thị B', total: 35, pass: 34, fail: 1, failRate: 2.8, reason: 'Vắng quá 20%' },
  { id: 3, subject: 'Cơ sở dữ liệu', class: 'SE1603', teacher: 'Lê Văn C', total: 42, pass: 30, fail: 12, failRate: 28.5, reason: 'Thiếu bài tập Lab' },
  { id: 4, subject: 'An toàn thông tin', class: 'SE1604', teacher: 'Hoàng Văn D', total: 38, pass: 38, fail: 0, failRate: 0, reason: 'N/A' },
])

const getFailRateColor = (rate) => {
  if (rate >= 20) return 'text-[var(--color-danger-text)] bg-[var(--color-danger-bg)] border-[var(--color-danger-text)]/20'
  if (rate >= 10) return 'text-[var(--color-warning-text)] bg-[var(--color-warning-bg)] border-[var(--color-warning-text)]/20'
  return 'text-[var(--color-success-text)] bg-[var(--color-success-bg)] border-[var(--color-success-text)]/20'
}
</script>

<template>
  <PageContainer 
    title="Tỷ lệ Pass / Fail môn học" 
    subtitle="Theo dõi và phân tích tỷ lệ qua môn, rớt môn để đánh giá độ khó và chất lượng giảng dạy."
  >
    <div class="space-y-4">
      
      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input 
                type="text" 
                placeholder="Tìm môn học, lớp hoặc giảng viên..." 
                class="w-full surface-input border border-input rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-[var(--border-focus-ring)]"
              >
           </div>
           <select class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option>Kỳ Spring 2026</option>
              <option>Kỳ Fall 2025</option>
           </select>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Lọc nâng cao
        </button>
      </div>

      <!-- ── Stats Summary ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
         <div class="surface-card border border-[var(--color-danger-text)]/20 rounded-2xl p-4 bg-[var(--color-danger-bg)] flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-danger-text)] shadow-sm border border-[var(--color-danger-text)]/20">
               <TrendingDown :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Môn học tỷ lệ rớt cao nhất</h4>
               <p class="text-xs text-[var(--color-danger-text)] mt-1 font-bold">Cấu trúc dữ liệu (32% Fail)</p>
            </div>
         </div>
         <div class="surface-card border border-[var(--color-success-text)]/20 rounded-2xl p-4 bg-[var(--color-success-bg)] flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-success-text)] shadow-sm border border-[var(--color-success-text)]/20">
               <ArrowUpRight :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Tỷ lệ Pass trung bình toàn trường</h4>
               <p class="text-xs text-[var(--color-success-text)] mt-1 font-bold">88.4% (Tăng 1.2% so với kỳ trước)</p>
            </div>
         </div>
      </div>

      <!-- ── Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Môn học & Lớp</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Giảng viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Sĩ số / Pass / Fail</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Tỷ lệ rớt</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Nguyên nhân chính</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="stat in courseStats" :key="stat.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] border border-[var(--color-info-text)]/20">
                    <BookOpen :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ stat.subject }}</p>
                    <p class="text-[11px] font-bold text-muted mt-0.5">{{ stat.class }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <User :size="14" class="text-placeholder" />
                   <span class="text-xs font-bold text-label">{{ stat.teacher }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex flex-col gap-1">
                   <div class="flex items-center gap-2">
                      <Users :size="12" class="text-placeholder" />
                      <span class="text-[10px] font-semibold text-muted">{{ stat.total }} SV</span>
                   </div>
                   <div class="flex items-center gap-3">
                      <span class="text-[10px] font-semibold text-[var(--color-success-text)]">{{ stat.pass }} Pass</span>
                      <span class="text-[10px] font-semibold text-[var(--color-danger-text)]">{{ stat.fail }} Fail</span>
                   </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border w-fit shadow-sm', getFailRateColor(stat.failRate)]">
                  {{ stat.failRate }}%
                </div>
              </td>
              <td class="px-4 py-4 max-w-[200px]">
                 <p class="text-[11px] text-muted font-medium leading-relaxed italic">{{ stat.reason }}</p>
              </td>
              <td class="px-4 py-4 text-right">
                <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all">
                  <ChevronRight :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Failure Causes Summary ── -->
      <div class="surface-card border border-card rounded-2xl p-6">
         <h4 class="text-xs font-semibold text-muted uppercase tracking-widest mb-4 flex items-center gap-2">
            <Info :size="16" /> Phân tích nguyên nhân rớt môn chính
         </h4>
         <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div v-for="(val, label) in { 'Điểm thi thấp': 45, 'Vắng quá 20%': 25, 'Thiếu bài tập Lab': 20, 'Khác': 10 }" :key="label" class="p-4 surface-solid rounded-2xl border border-default">
               <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1">{{ label }}</p>
               <div class="flex items-end gap-2">
                  <h3 class="text-xl font-semibold text-heading">{{ val }}%</h3>
                  <div class="h-1.5 flex-1 bg-[var(--surface-input)] rounded-full mb-1.5 overflow-hidden">
                     <div :style="{ width: `${val}%` }" class="h-full bg-[var(--lg-primary)] rounded-full"></div>
                  </div>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
