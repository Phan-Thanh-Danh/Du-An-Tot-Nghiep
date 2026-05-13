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
  if (gpa >= 3.2) return 'text-emerald-600'
  if (gpa >= 2.5) return 'text-blue-600'
  return 'text-rose-600'
}
</script>

<template>
  <PageContainer 
    title="Báo cáo GPA hệ thống" 
    subtitle="Phân tích điểm trung bình tích lũy theo từng khoa, cơ sở và lớp học để đánh giá chất lượng sinh viên."
  >
    <template #actions>
      <div class="flex items-center gap-3">
         <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2 bg-white/50 border-slate-200">
            <FileText :size="18" /> PDF Report
         </button>
         <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2 bg-white border-slate-200 shadow-sm">
            <Download :size="18" /> Excel Data
         </button>
      </div>
    </template>

    <div class="space-y-6">
      
      <!-- ── Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input 
                type="text" 
                placeholder="Tìm khoa, ngành hoặc lớp..." 
                class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10"
              >
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold bg-white/50 border-slate-200">
              Kỳ Spring 2026
           </button>
        </div>
        <div class="flex items-center gap-2">
           <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest mr-2">Sắp xếp theo</span>
           <select class="bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option>GPA Trung bình (Cao - Thấp)</option>
              <option>Số lượng SV cảnh báo</option>
           </select>
        </div>
      </div>

      <!-- ── KPI Mini Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
         <div class="lg-card-glass p-6 bg-gradient-to-br from-indigo-50 to-blue-50 border-blue-100 flex items-center gap-5">
            <div class="h-12 w-12 rounded-2xl bg-white flex items-center justify-center text-indigo-600 shadow-sm border border-indigo-100">
               <Target :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-black text-indigo-600 uppercase tracking-widest">GPA Mục tiêu kỳ</p>
               <h3 class="text-2xl font-black text-slate-800 leading-tight">3.20</h3>
            </div>
         </div>
         <div class="lg-card-glass p-6 bg-gradient-to-br from-emerald-50 to-teal-50 border-emerald-100 flex items-center gap-5">
            <div class="h-12 w-12 rounded-2xl bg-white flex items-center justify-center text-emerald-600 shadow-sm border border-emerald-100">
               <TrendingUp :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-black text-emerald-600 uppercase tracking-widest">Tỷ lệ GPA >= 3.2</p>
               <h3 class="text-2xl font-black text-slate-800 leading-tight">42.5%</h3>
            </div>
         </div>
         <div class="lg-card-glass p-6 bg-gradient-to-br from-rose-50 to-amber-50 border-rose-100 flex items-center gap-5">
            <div class="h-12 w-12 rounded-2xl bg-white flex items-center justify-center text-rose-600 shadow-sm border border-rose-100">
               <Award :size="24" />
            </div>
            <div>
               <p class="text-[10px] font-black text-rose-600 uppercase tracking-widest">Thủ khoa kỳ (GPA)</p>
               <h3 class="text-2xl font-black text-slate-800 leading-tight">4.00</h3>
            </div>
         </div>
      </div>

      <!-- ── Data Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Khoa / Lớp / Cơ sở</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">GPA Trung bình</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Min / Max GPA</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Dưới ngưỡng (2.0)</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="stat in gpaStats" :key="stat.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-600 group-hover:text-white transition-all">
                    <Building2 v-if="stat.group.includes('Khoa')" :size="18" />
                    <Users v-else :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-black text-slate-800 leading-tight">{{ stat.group }}</p>
                    <p class="text-[10px] font-bold text-slate-400 mt-1 flex items-center gap-1">
                       <MapPin :size="10" /> {{ stat.campus }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                   <h3 :class="['text-lg font-black', getGpaColor(stat.avgGpa)]">{{ stat.avgGpa.toFixed(2) }}</h3>
                   <ArrowUpRight :size="14" class="text-slate-300" />
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-4">
                   <div class="text-center">
                      <p class="text-[9px] font-black text-slate-400 uppercase">Min</p>
                      <p class="text-xs font-bold text-slate-700">{{ stat.minGpa.toFixed(2) }}</p>
                   </div>
                   <div class="h-6 w-px bg-slate-100"></div>
                   <div class="text-center">
                      <p class="text-[9px] font-black text-emerald-400 uppercase">Max</p>
                      <p class="text-xs font-black text-emerald-600">{{ stat.maxGpa.toFixed(2) }}</p>
                   </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest border w-fit shadow-sm', stat.warningCount > 10 ? 'bg-rose-50 text-rose-600 border-rose-100' : 'bg-slate-50 text-slate-500 border-slate-100']">
                  {{ stat.warningCount }} Sinh viên
                </div>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all">
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
