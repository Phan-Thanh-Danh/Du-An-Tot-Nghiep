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
  if (rate >= 20) return 'text-rose-600 bg-rose-50 border-rose-100'
  if (rate >= 10) return 'text-amber-600 bg-amber-50 border-amber-100'
  return 'text-emerald-600 bg-emerald-50 border-emerald-100'
}
</script>

<template>
  <PageContainer 
    title="Tỷ lệ Pass / Fail môn học" 
    subtitle="Theo dõi và phân tích tỷ lệ qua môn, rớt môn để đánh giá độ khó và chất lượng giảng dạy."
  >
    <div class="space-y-6">
      
      <!-- ── Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input 
                type="text" 
                placeholder="Tìm môn học, lớp hoặc giảng viên..." 
                class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10"
              >
           </div>
           <select class="bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option>Kỳ Spring 2026</option>
              <option>Kỳ Fall 2025</option>
           </select>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Lọc nâng cao
        </button>
      </div>

      <!-- ── Stats Summary ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
         <div class="lg-card-glass p-6 border-rose-100 bg-rose-50/10 flex items-center gap-5">
            <div class="h-12 w-12 rounded-2xl bg-rose-100 flex items-center justify-center text-rose-600 shadow-sm border border-rose-200">
               <TrendingDown :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-black text-rose-900 uppercase tracking-wide">Môn học tỷ lệ rớt cao nhất</h4>
               <p class="text-xs text-rose-700 mt-1 font-bold">Cấu trúc dữ liệu (32% Fail)</p>
            </div>
         </div>
         <div class="lg-card-glass p-6 border-emerald-100 bg-emerald-50/10 flex items-center gap-5">
            <div class="h-12 w-12 rounded-2xl bg-emerald-100 flex items-center justify-center text-emerald-600 shadow-sm border border-emerald-200">
               <ArrowUpRight :size="24" />
            </div>
            <div>
               <h4 class="text-sm font-black text-emerald-900 uppercase tracking-wide">Tỷ lệ Pass trung bình toàn trường</h4>
               <p class="text-xs text-emerald-700 mt-1 font-bold">88.4% (Tăng 1.2% so với kỳ trước)</p>
            </div>
         </div>
      </div>

      <!-- ── Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Môn học & Lớp</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Sĩ số / Pass / Fail</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Tỷ lệ rớt</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Nguyên nhân chính</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="stat in courseStats" :key="stat.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 border border-blue-100">
                    <BookOpen :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-black text-slate-800 leading-tight">{{ stat.subject }}</p>
                    <p class="text-[11px] font-bold text-slate-400 mt-0.5">{{ stat.class }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                   <User :size="14" class="text-slate-300" />
                   <span class="text-xs font-bold text-slate-600">{{ stat.teacher }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex flex-col gap-1">
                   <div class="flex items-center gap-2">
                      <Users :size="12" class="text-slate-300" />
                      <span class="text-[10px] font-black text-slate-500">{{ stat.total }} SV</span>
                   </div>
                   <div class="flex items-center gap-3">
                      <span class="text-[10px] font-black text-emerald-600">{{ stat.pass }} Pass</span>
                      <span class="text-[10px] font-black text-rose-500">{{ stat.fail }} Fail</span>
                   </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest border w-fit shadow-sm', getFailRateColor(stat.failRate)]">
                  {{ stat.failRate }}%
                </div>
              </td>
              <td class="px-6 py-4 max-w-[200px]">
                 <p class="text-[11px] text-slate-500 font-medium leading-relaxed italic">{{ stat.reason }}</p>
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

      <!-- ── Failure Causes Summary ── -->
      <div class="lg-card-glass p-8">
         <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-6 flex items-center gap-2">
            <Info :size="16" /> Phân tích nguyên nhân rớt môn chính
         </h4>
         <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            <div v-for="(val, label) in { 'Điểm thi thấp': 45, 'Vắng quá 20%': 25, 'Thiếu bài tập Lab': 20, 'Khác': 10 }" :key="label" class="p-4 bg-slate-50/50 rounded-2xl border border-slate-100">
               <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">{{ label }}</p>
               <div class="flex items-end gap-2">
                  <h3 class="text-xl font-black text-slate-800">{{ val }}%</h3>
                  <div class="h-1.5 flex-1 bg-slate-200 rounded-full mb-1.5 overflow-hidden">
                     <div :style="{ width: `${val}%` }" class="h-full bg-blue-500 rounded-full"></div>
                  </div>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
