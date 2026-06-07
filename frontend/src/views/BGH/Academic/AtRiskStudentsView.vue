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
    case 'critical': return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20'
    case 'high': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[var(--color-warning-text)]/20'
    case 'medium': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20'
    default: return 'surface-solid text-muted border-default'
  }
}
</script>

<template>
  <PageContainer 
    title="Sinh viên có nguy cơ rớt môn" 
    subtitle="Hệ thống cảnh báo sớm (AI Early Warning) dựa trên dữ liệu điểm số, chuyên cần và tiến độ học tập."
  >
    <div class="space-y-4">
      
      <!-- ── AI Insight Banner ── -->
      <div class="surface-card border border-[var(--color-info-text)]/20 bg-[var(--color-info-bg)] rounded-2xl p-5 relative overflow-hidden">
         <div class="relative z-10 flex flex-col md:flex-row items-center justify-between gap-5">
            <div class="flex items-center gap-4">
               <div class="h-14 w-14 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center border border-[var(--color-info-text)]/20 shadow-sm">
                  <Brain :size="30" class="text-[var(--color-info-text)]" />
               </div>
               <div>
                  <h3 class="text-lg font-semibold tracking-tight text-heading">AI Academic Forecast</h3>
                  <p class="text-sm text-[var(--color-info-text)] mt-1 font-medium max-w-md">Hệ thống đã phân tích dữ liệu của 1,240 sinh viên và phát hiện 42 trường hợp có nguy cơ rớt môn cao trong kỳ này.</p>
               </div>
            </div>
            <div class="flex flex-wrap justify-center gap-3">
               <div class="px-4 py-3 surface-card rounded-2xl border border-[var(--color-info-text)]/20 text-center">
                  <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Độ chính xác</p>
                  <p class="text-lg font-semibold text-heading">94.2%</p>
               </div>
               <div class="px-4 py-3 surface-card rounded-2xl border border-[var(--color-info-text)]/20 text-center">
                  <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Cần can thiệp</p>
                  <p class="text-lg font-semibold text-heading">18 SV</p>
               </div>
            </div>
         </div>
      </div>

      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm tên sinh viên, mã số hoặc lớp..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-[var(--border-input-focus)]">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Mức độ rủi ro
           </button>
        </div>
        <button class="lg-button-primary py-2.5 px-4 text-sm font-semibold flex items-center gap-2">
           <Bell :size="18" /> Gửi cảnh báo cho Giảng viên
        </button>
      </div>

      <!-- ── Risk List ── -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
         <div 
           v-for="st in riskStudents" 
           :key="st.id" 
           class="surface-card border border-card rounded-2xl p-5 group hover:border-[var(--border-input-focus)] transition-all shadow-sm"
         >
            <div class="flex items-start justify-between mb-4">
               <div class="flex items-center gap-4">
                  <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-muted group-hover:bg-[var(--color-info-bg)] group-hover:text-[var(--color-info-text)] transition-all">
                     <User :size="28" />
                  </div>
                  <div>
                     <h4 class="text-lg font-semibold text-heading leading-tight group-hover:text-link transition-colors">{{ st.name }}</h4>
                     <p class="text-[11px] font-bold text-muted uppercase tracking-widest mt-1">{{ st.code }} • Lớp {{ st.class }}</p>
                  </div>
               </div>
               <div :class="['px-3 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getRiskBadge(st.risk)]">
                  {{ st.risk }}
               </div>
            </div>

            <div class="grid grid-cols-2 gap-4 mb-8">
               <div class="p-4 surface-solid rounded-2xl border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">Môn học hiện tại</p>
                  <p class="text-xs font-bold text-label">{{ st.subject }}</p>
               </div>
               <div class="p-4 surface-solid rounded-2xl border border-default">
                  <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mb-1.5">Điểm / Chuyên cần</p>
                  <div class="flex items-center justify-between">
                     <span class="text-sm font-semibold text-[var(--color-danger-text)]">{{ st.grade }}</span>
                     <span class="text-[10px] font-bold text-muted">{{ st.attendance }}% vắng</span>
                  </div>
               </div>
            </div>

            <div class="flex items-start gap-3 p-4 bg-[var(--color-danger-bg)] rounded-2xl border border-[var(--color-danger-text)]/20 mb-4">
               <Zap :size="16" class="text-[var(--color-danger-text)] shrink-0 mt-0.5" />
               <div>
                  <p class="text-[10px] font-semibold text-[var(--color-danger-text)] uppercase tracking-widest">Dự đoán của AI</p>
                  <p class="text-[11px] text-body font-medium leading-relaxed mt-1">{{ st.reason }}</p>
               </div>
            </div>

            <div class="flex items-center justify-between pt-6 border-t border-default">
               <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted" title="Xem lịch sử"><History :size="18" /></button>
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted" title="Gửi thông báo"><Bell :size="18" /></button>
               </div>
               <button class="text-xs font-semibold text-link uppercase tracking-widest flex items-center gap-1 hover:underline">
                  Xem chi tiết hồ sơ <ChevronRight :size="14" />
               </button>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
