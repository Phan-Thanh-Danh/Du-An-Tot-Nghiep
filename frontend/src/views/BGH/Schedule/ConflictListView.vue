<script setup>
import { ref } from 'vue'
import { 
  AlertTriangle, 
  Search, 
  Filter, 
  Bell, 
  User, 
  MapPin, 
  Clock, 
  ArrowRight,
  MoreVertical,
  ShieldAlert,
  Zap
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const conflicts = ref([
  { id: 'CFL-001', type: 'Trùng giảng viên', description: 'TS. Nguyễn Văn A bị xếp lịch tại P.302 và Lab 1 cùng lúc.', severity: 'critical', affected: 'Nguyễn Văn A', time: 'Thứ 2, 07:30 - 10:30', campus: 'Cơ sở chính' },
  { id: 'CFL-002', type: 'Trùng phòng học', description: 'Phòng A.201 đang có 2 lớp SE1601 và SE1602 xếp chồng.', severity: 'critical', affected: 'Phòng A.201', time: 'Thứ 4, 13:30 - 16:30', campus: 'Cơ sở chính' },
  { id: 'CFL-003', type: 'Phòng bảo trì', description: 'Lab 5 đang trong kế hoạch nâng cấp thiết bị.', severity: 'major', affected: 'Lab 5', time: 'Cả tuần', campus: 'Cơ sở 2' },
  { id: 'CFL-004', type: 'Giờ học không hợp lệ', description: 'Lớp SE1603 có tiết học kết thúc sau 22:00.', severity: 'minor', affected: 'SE1603', time: 'Thứ 6, 19:30 - 22:30', campus: 'Cơ sở chính' },
])

const getSeverityClass = (severity) => {
  switch (severity) {
    case 'critical': return 'bg-rose-50 text-rose-600 border-rose-100'
    case 'major': return 'bg-amber-50 text-amber-600 border-amber-100'
    case 'minor': return 'bg-blue-50 text-blue-600 border-blue-100'
    default: return 'bg-slate-50 text-slate-500 border-slate-100'
  }
}
</script>

<template>
  <PageContainer 
    title="Xung đột lịch học" 
    subtitle="Giám sát các lỗi sắp xếp tài nguyên (giảng viên, phòng học, thời gian) trong toàn hệ thống."
  >
    <div class="space-y-6">
      
      <!-- ── Stats Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="lg-card-glass p-6 border-rose-100 bg-rose-50/10 flex items-center gap-5">
           <div class="h-12 w-12 rounded-2xl bg-rose-100 flex items-center justify-center text-rose-600 shadow-sm border border-rose-200">
              <ShieldAlert :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-black text-rose-600 uppercase tracking-widest">Nghiêm trọng</p>
              <h3 class="text-2xl font-black text-rose-900 leading-tight">02</h3>
           </div>
        </div>
        <div class="lg-card-glass p-6 border-amber-100 bg-amber-50/10 flex items-center gap-5">
           <div class="h-12 w-12 rounded-2xl bg-amber-100 flex items-center justify-center text-amber-600 shadow-sm border border-amber-200">
              <AlertTriangle :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-black text-amber-600 uppercase tracking-widest">Trung bình</p>
              <h3 class="text-2xl font-black text-amber-900 leading-tight">01</h3>
           </div>
        </div>
        <div class="lg-card-glass p-6 border-blue-100 bg-blue-50/10 flex items-center gap-5">
           <div class="h-12 w-12 rounded-2xl bg-blue-100 flex items-center justify-center text-blue-600 shadow-sm border border-blue-200">
              <Zap :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-black text-blue-600 uppercase tracking-widest">Lỗi nhẹ</p>
              <h3 class="text-2xl font-black text-blue-900 leading-tight">01</h3>
           </div>
        </div>
      </div>

      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm theo đối tượng, loại xung đột..." class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Lọc mức độ
           </button>
        </div>
        <button class="lg-button-primary bg-slate-800 py-2.5 px-5 text-sm font-black shadow-lg shadow-slate-200 flex items-center gap-2">
           <Bell :size="18" /> Yêu cầu GV xử lý tất cả
        </button>
      </div>

      <!-- ── Conflict Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Loại xung đột</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Chi tiết & Đối tượng</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thời gian</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Mức độ</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="cf in conflicts" :key="cf.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                 <p class="text-sm font-black text-slate-800 leading-tight">{{ cf.type }}</p>
                 <p class="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-tighter">{{ cf.id }}</p>
              </td>
              <td class="px-6 py-4 max-w-xs">
                 <p class="text-xs text-slate-600 leading-relaxed font-medium mb-2">{{ cf.description }}</p>
                 <div class="flex items-center gap-2 text-[10px] font-black text-blue-600 uppercase">
                    <User v-if="cf.type.includes('giảng viên')" :size="12" />
                    <MapPin v-else :size="12" />
                    {{ cf.affected }}
                 </div>
              </td>
              <td class="px-6 py-4">
                 <div class="flex items-center gap-1.5 text-xs font-bold text-slate-500">
                    <Clock :size="14" class="text-slate-300" /> {{ cf.time }}
                 </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2 py-0.5 rounded-lg text-[9px] font-black uppercase tracking-widest border', getSeverityClass(cf.severity)]">
                  {{ cf.severity }}
                </span>
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Gửi thông báo nhắc nhở">
                    <Bell :size="18" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <ArrowRight :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </div>
  </PageContainer>
</template>
