<script setup>
import { ref } from 'vue'
import { 
  Plus, 
  Search, 
  Calendar, 
  Clock, 
  Settings, 
  MoreVertical, 
  CheckCircle2, 
  AlertCircle,
  ArrowRight,
  ShieldCheck,
  Edit3,
  Trash2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const periods = ref([
  { id: 1, name: 'Đợt 1 - Học kỳ Spring 2026', semester: 'Spring 2026', openDate: '2026-01-15', closeDate: '2026-01-25', withdrawDeadline: '2026-02-10', maxCredits: 24, status: 'open' },
  { id: 2, name: 'Đợt bổ sung - Học kỳ Spring 2026', semester: 'Spring 2026', openDate: '2026-02-01', closeDate: '2026-02-05', withdrawDeadline: '2026-02-15', maxCredits: 12, status: 'draft' },
  { id: 3, name: 'Học kỳ Fall 2025', semester: 'Fall 2025', openDate: '2025-08-10', closeDate: '2025-08-25', withdrawDeadline: '2025-09-10', maxCredits: 24, status: 'closed' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'open': return 'bg-emerald-100 text-emerald-700 border-emerald-200'
    case 'closed': return 'bg-slate-100 text-slate-500 border-slate-200'
    case 'draft': return 'bg-blue-100 text-blue-700 border-blue-200'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Đợt đăng ký môn học" 
    subtitle="Quản lý thời gian, số tín chỉ tối đa và quy trình đăng ký cho sinh viên."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
        <Plus :size="18" /> Tạo đợt mới
      </button>
    </template>

    <div class="space-y-6">
      
      <!-- ── Search & Filter ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo tên đợt hoặc học kỳ..." 
            class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
      </div>

      <!-- ── Periods Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Đợt đăng ký</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thời gian</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Max Credits</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Trạng thái</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="period in periods" :key="period.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ period.name }}</p>
                <p class="text-[11px] font-bold text-blue-600 mt-1 uppercase tracking-tighter">{{ period.semester }}</p>
              </td>
              <td class="px-6 py-4">
                <div class="space-y-1">
                  <div class="flex items-center gap-2 text-xs font-bold text-slate-600">
                    <Clock :size="14" class="text-emerald-500" /> {{ period.openDate }} <ArrowRight :size="12" /> {{ period.closeDate }}
                  </div>
                  <div class="flex items-center gap-2 text-[10px] font-medium text-slate-400">
                    <AlertCircle :size="12" /> Hủy môn: {{ period.withdrawDeadline }}
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span class="text-sm font-black text-slate-800">{{ period.maxCredits }} TC</span>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border', getStatusBadge(period.status)]">
                  {{ period.status }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Chỉnh sửa">
                    <Edit3 :size="16" />
                  </button>
                  <button v-if="period.status === 'draft'" class="p-2 hover:bg-emerald-50 hover:text-emerald-600 rounded-lg text-slate-400 transition-all" title="Mở đăng ký">
                    <CheckCircle2 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <MoreVertical :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Important Rules ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="lg-card-glass p-6 border-blue-100 bg-blue-50/20">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-blue-100 flex items-center justify-center text-blue-600 shrink-0">
               <ShieldCheck :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-black text-blue-900">Quy tắc đăng ký</h4>
              <ul class="mt-2 space-y-2">
                <li class="text-xs text-blue-700 flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-blue-400 mt-1.5"></div>
                  Sinh viên chỉ có thể đăng ký khi đợt ở trạng thái <strong>OPEN</strong>.
                </li>
                <li class="text-xs text-blue-700 flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-blue-400 mt-1.5"></div>
                  Hệ thống tự động kiểm tra Tín chỉ tối đa đã cấu hình.
                </li>
              </ul>
            </div>
          </div>
        </div>

        <div class="lg-card-glass p-6 border-emerald-100 bg-emerald-50/20">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-emerald-100 flex items-center justify-center text-emerald-600 shrink-0">
               <Settings :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-black text-emerald-900">Chốt danh sách</h4>
              <p class="text-xs text-emerald-700 mt-2 leading-relaxed">
                Khi đợt đăng ký kết thúc, hệ thống sẽ tự động chốt danh sách và chuyển các lớp có sĩ số thấp sang trạng thái <strong>PENDING CANCEL</strong> để Giáo vụ xử lý.
              </p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
