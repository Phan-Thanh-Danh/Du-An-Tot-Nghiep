<script setup>
import { ref } from 'vue'
import { 
  AlertCircle, 
  Search, 
  Filter, 
  User, 
  TrendingDown, 
  Brain, 
  ChevronRight, 
  Bell, 
  History, 
  CheckCircle2,
  Clock,
  Zap
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const riskStudents = ref([
  { id: 1, name: 'Lê Hoàng Minh', code: 'SV2024005', class: 'SE1601', subject: 'Cấu trúc dữ liệu', grade: 3.5, attendance: 75, risk: 'critical', reason: 'Điểm Lab thấp & Vắng mặt liên tục (2 tuần).' },
  { id: 2, name: 'Nguyễn Thị Hoa', code: 'SV2024122', class: 'SE1602', subject: 'Java Programming', grade: 4.2, attendance: 90, risk: 'high', reason: 'Tiến độ hoàn thành Assignment chậm (>50% chưa nộp).' },
  { id: 3, name: 'Trần Văn Tú', code: 'SV2024089', class: 'SE1601', subject: 'Database Design', grade: 5.0, attendance: 82, risk: 'medium', reason: 'Điểm Quiz trung bình thấp (Dưới 4.5).' },
  { id: 4, name: 'Phạm Hồng Nam', code: 'SV2024201', class: 'SE1605', subject: 'Operating Systems', grade: 2.8, attendance: 65, risk: 'critical', reason: 'Vắng quá 20% & Điểm kiểm tra giữa kỳ < 3.0.' },
])

const getRiskBadge = (risk) => {
  switch (risk) {
    case 'critical': return 'bg-rose-600 text-white border-rose-600'
    case 'high': return 'bg-orange-500 text-white border-orange-500'
    case 'medium': return 'bg-amber-100 text-amber-700 border-amber-200'
    default: return 'bg-blue-100 text-blue-700 border-blue-200'
  }
}
</script>

<template>
  <PageContainer 
    title="Sinh viên có nguy cơ rớt môn" 
    subtitle="Hệ thống cảnh báo sớm (AI Early Warning) dựa trên dữ liệu điểm số, chuyên cần và tiến độ học tập."
  >
    <div class="space-y-6">
      
      <!-- ── AI Insight Banner ── -->
      <div class="lg-card-glass p-8 bg-gradient-to-r from-blue-600 via-indigo-600 to-violet-700 text-white relative overflow-hidden group">
         <div class="absolute -right-20 -top-20 h-80 w-80 bg-white/10 rounded-full blur-3xl group-hover:scale-110 transition-transform duration-1000"></div>
         <div class="relative z-10 flex flex-col md:flex-row items-center justify-between gap-8">
            <div class="flex items-center gap-6">
               <div class="h-20 w-20 rounded-3xl bg-white/20 backdrop-blur-md flex items-center justify-center border border-white/30 shadow-2xl">
                  <Brain :size="48" class="text-white" />
               </div>
               <div>
                  <h3 class="text-2xl font-black tracking-tight">AI Academic Forecast</h3>
                  <p class="text-blue-100 mt-1 font-medium opacity-90 max-w-md">Hệ thống đã phân tích dữ liệu của 1,240 sinh viên và phát hiện 42 trường hợp có nguy cơ rớt môn cao trong kỳ này.</p>
               </div>
            </div>
            <div class="flex flex-wrap justify-center gap-3">
               <div class="px-5 py-3 bg-white/10 rounded-2xl border border-white/20 text-center">
                  <p class="text-[10px] font-black uppercase tracking-widest opacity-60">Độ chính xác</p>
                  <p class="text-xl font-black">94.2%</p>
               </div>
               <div class="px-5 py-3 bg-white/10 rounded-2xl border border-white/20 text-center">
                  <p class="text-[10px] font-black uppercase tracking-widest opacity-60">Cần can thiệp</p>
                  <p class="text-xl font-black">18 SV</p>
               </div>
            </div>
         </div>
      </div>

      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm tên sinh viên, mã số hoặc lớp..." class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Mức độ rủi ro
           </button>
        </div>
        <button class="lg-button-primary bg-slate-800 py-2.5 px-6 text-sm font-black shadow-lg shadow-slate-200 flex items-center gap-2">
           <Bell :size="18" /> Gửi cảnh báo cho Giảng viên
        </button>
      </div>

      <!-- ── Risk List ── -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
         <div 
           v-for="st in riskStudents" 
           :key="st.id" 
           class="lg-card-glass p-8 group hover:border-blue-300 transition-all border-slate-100 shadow-sm"
         >
            <div class="flex items-start justify-between mb-6">
               <div class="flex items-center gap-4">
                  <div class="h-14 w-14 rounded-2xl bg-slate-100 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-all">
                     <User :size="28" />
                  </div>
                  <div>
                     <h4 class="text-lg font-black text-slate-800 leading-tight group-hover:text-blue-600 transition-colors">{{ st.name }}</h4>
                     <p class="text-[11px] font-bold text-slate-400 uppercase tracking-widest mt-1">{{ st.code }} • Lớp {{ st.class }}</p>
                  </div>
               </div>
               <div :class="['px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border shadow-sm', getRiskBadge(st.risk)]">
                  {{ st.risk }}
               </div>
            </div>

            <div class="grid grid-cols-2 gap-6 mb-8">
               <div class="p-4 bg-slate-50/50 rounded-2xl border border-slate-50">
                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1.5">Môn học hiện tại</p>
                  <p class="text-xs font-bold text-slate-700">{{ st.subject }}</p>
               </div>
               <div class="p-4 bg-slate-50/50 rounded-2xl border border-slate-50">
                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1.5">Điểm / Chuyên cần</p>
                  <div class="flex items-center justify-between">
                     <span class="text-sm font-black text-rose-600">{{ st.grade }}</span>
                     <span class="text-[10px] font-bold text-slate-500">{{ st.attendance }}% vắng</span>
                  </div>
               </div>
            </div>

            <div class="flex items-start gap-3 p-4 bg-rose-50/30 rounded-2xl border border-rose-100/50 mb-6">
               <Zap :size="16" class="text-rose-500 shrink-0 mt-0.5" />
               <div>
                  <p class="text-[10px] font-black text-rose-700 uppercase tracking-widest">Dự đoán của AI</p>
                  <p class="text-[11px] text-slate-600 font-medium leading-relaxed mt-1">{{ st.reason }}</p>
               </div>
            </div>

            <div class="flex items-center justify-between pt-6 border-t border-slate-50">
               <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400" title="Xem lịch sử"><History :size="18" /></button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400" title="Gửi thông báo"><Bell :size="18" /></button>
               </div>
               <button class="text-xs font-black text-blue-600 uppercase tracking-widest flex items-center gap-1 hover:underline">
                  Xem chi tiết hồ sơ <ChevronRight :size="14" />
               </button>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
