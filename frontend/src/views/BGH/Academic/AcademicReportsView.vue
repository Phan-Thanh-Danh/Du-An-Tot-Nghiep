<script setup>
import { ref } from 'vue'
import { 
  FileSearch, 
  Search, 
  Filter, 
  BarChart, 
  PieChart, 
  Table as TableIcon, 
  Download, 
  Printer, 
  Share2, 
  Calendar, 
  Building2, 
  BookOpen,
  ChevronDown,
  ExternalLink
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const reports = ref([
  { id: 'RPT-001', name: 'Báo cáo chất lượng học tập kỳ Spring 2026', type: 'Toàn trường', date: '12/05/2026', status: 'ready' },
  { id: 'RPT-002', name: 'Phân tích tỷ lệ Incomplete môn đồ án', type: 'Theo môn học', date: '10/05/2026', status: 'ready' },
  { id: 'RPT-003', name: 'Thống kê điểm trung bình Khoa CNTT', type: 'Theo khoa', date: '08/05/2026', status: 'generating' },
])

const activeTab = ref('Class')
</script>

<template>
  <PageContainer 
    title="Báo cáo học tập chi tiết" 
    subtitle="Công cụ phân tích và kết xuất báo cáo đa chiều theo lớp, môn học và cơ sở đào tạo."
  >
    <div class="space-y-8">
      
      <!-- ── Report Generator Controls ── -->
      <div class="lg-card-glass p-8 bg-blue-50/20 border-blue-100">
         <div class="flex items-center gap-4 mb-8">
            <div class="h-12 w-12 rounded-2xl bg-blue-600 text-white flex items-center justify-center shadow-lg shadow-blue-500/20">
               <FileSearch :size="24" />
            </div>
            <div>
               <h3 class="text-xl font-black text-slate-800">Trình tạo báo cáo</h3>
               <p class="text-xs text-slate-400 mt-0.5 font-bold uppercase tracking-widest">Tùy chỉnh các thông số để xuất dữ liệu</p>
            </div>
         </div>

         <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div class="space-y-2">
               <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Loại báo cáo</label>
               <div class="relative">
                  <select class="w-full bg-white border border-slate-200 rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option>Báo cáo theo Lớp</option>
                     <option>Báo cáo theo Môn học</option>
                     <option>Báo cáo theo Cơ sở</option>
                  </select>
                  <ChevronDown :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
               </div>
            </div>
            <div class="space-y-2">
               <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Học kỳ</label>
               <div class="relative">
                  <select class="w-full bg-white border border-slate-200 rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option>Spring 2026</option>
                     <option>Fall 2025</option>
                  </select>
                  <Calendar :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
               </div>
            </div>
            <div class="space-y-2">
               <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Cơ sở (Campus)</label>
               <div class="relative">
                  <select class="w-full bg-white border border-slate-200 rounded-2xl px-5 py-3.5 text-sm font-bold outline-none appearance-none cursor-pointer">
                     <option>Tất cả cơ sở</option>
                     <option>Cơ sở Hồ Chí Minh</option>
                     <option>Cơ sở Đà Nẵng</option>
                  </select>
                  <Building2 :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
               </div>
            </div>
            <div class="flex items-end">
               <button class="w-full lg-button-primary py-3.5 text-sm font-black flex items-center justify-center gap-2">
                  <BarChart :size="18" /> TẠO BÁO CÁO
               </button>
            </div>
         </div>
      </div>

      <!-- ── Analysis Content (Mock Tabbed View) ── -->
      <div class="space-y-6">
         <div class="flex items-center justify-between border-b border-slate-100 pb-2">
            <div class="flex gap-8">
               <button 
                 v-for="tab in ['Class', 'Subject', 'Campus']" 
                 :key="tab"
                 @click="activeTab = tab"
                 :class="['pb-4 text-xs font-black uppercase tracking-widest relative transition-all', activeTab === tab ? 'text-blue-600' : 'text-slate-400 hover:text-slate-600']"
               >
                  Báo cáo {{ tab }}
                  <div v-if="activeTab === tab" class="absolute bottom-0 left-0 right-0 h-1 bg-blue-600 rounded-full"></div>
               </button>
            </div>
            <div class="flex items-center gap-2">
               <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-colors"><Printer :size="18" /></button>
               <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-colors"><Share2 :size="18" /></button>
            </div>
         </div>

         <!-- Visual Preview Cards -->
         <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            <div 
              v-for="rpt in reports" 
              :key="rpt.id" 
              class="lg-card-glass p-6 group hover:border-blue-300 transition-all border-slate-100 bg-white/50"
            >
               <div class="flex items-start justify-between mb-4">
                  <div :class="['h-10 w-10 rounded-xl flex items-center justify-center shadow-sm', rpt.status === 'generating' ? 'bg-amber-100 text-amber-600 animate-pulse' : 'bg-blue-50 text-blue-600 border border-blue-100']">
                     <FileSearch v-if="rpt.status === 'ready'" :size="20" />
                     <Clock v-else :size="20" />
                  </div>
                  <span :class="['text-[9px] font-black uppercase tracking-widest px-2 py-1 rounded-lg border', rpt.status === 'ready' ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100']">
                     {{ rpt.status }}
                  </span>
               </div>
               
               <h4 class="text-sm font-black text-slate-800 leading-snug group-hover:text-blue-600 transition-colors">{{ rpt.name }}</h4>
               <p class="text-[10px] font-bold text-slate-400 mt-2 uppercase tracking-widest">{{ rpt.type }} • {{ rpt.date }}</p>
               
               <div class="mt-6 pt-5 border-t border-slate-100 flex items-center justify-between">
                  <div class="flex gap-2">
                     <button class="text-[10px] font-black text-slate-400 hover:text-blue-600 uppercase">View</button>
                     <button class="text-[10px] font-black text-slate-400 hover:text-blue-600 uppercase">Export</button>
                  </div>
                  <button class="text-slate-300 hover:text-blue-600"><ExternalLink :size="16" /></button>
               </div>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
