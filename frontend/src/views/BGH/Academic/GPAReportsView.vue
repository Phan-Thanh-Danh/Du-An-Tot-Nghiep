<script setup>
import { ref } from 'vue'
import { 
  Award, 
  Search, 
  Download, 
  TrendingUp, 
  ChevronRight, 
  Users, 
  Building2, 
  MapPin, 
  FileText, 
  ArrowUpRight,
  Target
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const gpaStats = ref([
  { id: 1, group: 'Khoa Công nghệ thông tin', avgGpa: 3.25, maxGpa: 3.98, minGpa: 1.2, warningCount: 12, campus: 'Cơ sở chính' },
  { id: 2, group: 'Khoa Kinh tế & Quản trị', avgGpa: 3.10, maxGpa: 4.0, minGpa: 1.5, warningCount: 8, campus: 'Cơ sở chính' },
  { id: 3, group: 'Khoa Ngôn ngữ Anh', avgGpa: 3.42, maxGpa: 3.95, minGpa: 2.1, warningCount: 3, campus: 'Cơ sở 2' },
  { id: 4, group: 'Lớp SE1601', avgGpa: 3.15, maxGpa: 3.90, minGpa: 1.8, warningCount: 4, campus: 'Cơ sở chính' },
])

const getGpaColor = (gpa) => {
  if (gpa >= 3.2) return 'text-[var(--color-success-text)]'
  if (gpa >= 2.5) return 'text-[var(--color-info-text)]'
  return 'text-[var(--color-danger-text)]'
}
</script>

<template>
  <PageContainer 
    title="Báo cáo GPA hệ thống" 
    subtitle="Phân tích điểm trung bình tích lũy theo từng khoa, cơ sở và lớp học để đánh giá chất lượng sinh viên."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <FileText :size="18" /> PDF Report
         </button>
         <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
            <Download :size="18" /> Excel Data
         </button>
      </div>
    </template>

    <div class="space-y-4">
      
      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input 
                type="text" 
                placeholder="Tìm khoa, ngành hoặc lớp..." 
                class="w-full surface-input border border-input rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-[var(--border-focus-ring)]"
              >
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
              Kỳ Spring 2026
           </button>
        </div>
        <div class="flex items-center gap-2">
           <span class="text-[10px] font-semibold text-muted uppercase tracking-widest mr-2">Sắp xếp theo</span>
           <select class="surface-input border border-input rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option>GPA Trung bình (Cao - Thấp)</option>
              <option>Số lượng SV cảnh báo</option>
           </select>
        </div>
      </div>

      <!-- ── KPI Mini Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shadow-sm border border-[var(--color-info-text)]/20">
               <Target :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-[var(--color-info-text)] uppercase tracking-widest">GPA Mục tiêu kỳ</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">3.20</h3>
            </div>
         </div>
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] shadow-sm border border-[var(--color-success-text)]/20">
               <TrendingUp :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-[var(--color-success-text)] uppercase tracking-widest">Tỷ lệ GPA >= 3.2</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">42.5%</h3>
            </div>
         </div>
         <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)] shadow-sm border border-[var(--color-warning-text)]/20">
               <Award :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-semibold text-[var(--color-warning-text)] uppercase tracking-widest">Thủ khoa kỳ (GPA)</p>
               <h3 class="text-xl font-semibold text-heading leading-tight">4.00</h3>
            </div>
         </div>
      </div>

      <!-- ── Data Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Khoa / Lớp / Cơ sở</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">GPA Trung bình</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Min / Max GPA</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Dưới ngưỡng (2.0)</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="stat in gpaStats" :key="stat.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl surface-solid flex items-center justify-center text-placeholder group-hover:text-link transition-all">
                    <Building2 v-if="stat.group.includes('Khoa')" :size="18" />
                    <Users v-else :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ stat.group }}</p>
                    <p class="text-[10px] font-bold text-muted mt-1 flex items-center gap-1">
                       <MapPin :size="10" /> {{ stat.campus }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <h3 :class="['text-lg font-semibold', getGpaColor(stat.avgGpa)]">{{ stat.avgGpa.toFixed(2) }}</h3>
                   <ArrowUpRight :size="14" class="text-placeholder" />
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-4">
                   <div class="text-center">
                      <p class="text-[9px] font-semibold text-muted uppercase">Min</p>
                      <p class="text-xs font-bold text-heading">{{ stat.minGpa.toFixed(2) }}</p>
                   </div>
                   <div class="h-6 w-px bg-[var(--border-default)]"></div>
                   <div class="text-center">
                      <p class="text-[9px] font-semibold text-[var(--color-success-text)] uppercase">Max</p>
                      <p class="text-xs font-semibold text-[var(--color-success-text)]">{{ stat.maxGpa.toFixed(2) }}</p>
                   </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border w-fit shadow-sm', stat.warningCount > 10 ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20' : 'surface-solid text-muted border-default']">
                  {{ stat.warningCount }} Sinh viên
                </div>
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

    </div>
  </PageContainer>
</template>
