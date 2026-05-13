<script setup>
import { ref } from 'vue'
import { 
  Plus, 
  Search, 
  Filter, 
  Users, 
  UserCheck, 
  UserMinus, 
  AlertCircle,
  MoreVertical,
  Layers,
  Edit3,
  Power,
  TrendingUp
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const sections = ref([
  { id: 'LHP001', subject: 'Lập trình Java', teacher: 'Nguyễn Văn A', capacity: 45, enrolled: 45, waitlist: 12, minEnroll: 20, status: 'full' },
  { id: 'LHP002', subject: 'Cấu trúc dữ liệu', teacher: 'Trần Thị B', capacity: 45, enrolled: 38, waitlist: 0, minEnroll: 20, status: 'open' },
  { id: 'LHP003', subject: 'Lập trình Web', teacher: 'Lê Văn C', capacity: 40, enrolled: 12, waitlist: 0, minEnroll: 15, status: 'pending_cancel' },
  { id: 'LHP004', subject: 'Hệ quản trị CSDL', teacher: 'Phạm Minh D', capacity: 45, enrolled: 42, waitlist: 5, minEnroll: 20, status: 'open' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'open': return 'bg-emerald-100 text-emerald-700 border-emerald-200'
    case 'full': return 'bg-amber-100 text-amber-700 border-amber-200'
    case 'pending_cancel': return 'bg-rose-100 text-rose-700 border-rose-200'
    case 'cancelled': return 'bg-slate-100 text-slate-500 border-slate-200'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Lớp học phần" 
    subtitle="Quản lý danh sách các lớp mở trong học kỳ, theo dõi sĩ số và trạng thái lớp."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
        <Plus :size="18" /> Tạo lớp mới
      </button>
    </template>

    <div class="space-y-6">
      
      <!-- ── Quick Metrics ── -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
            <Layers :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Tổng số lớp</p>
             <p class="text-xl font-black text-slate-800">42</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <UserCheck :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Đã đăng ký</p>
             <p class="text-xl font-black text-slate-800">1,245</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
            <TrendingUp :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Đang Waitlist</p>
             <p class="text-xl font-black text-slate-800">84</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
            <AlertCircle :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Cần xử lý</p>
             <p class="text-xl font-black text-slate-800">5</p>
          </div>
        </div>
      </div>

      <!-- ── Search Bar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo mã lớp, môn học hoặc giảng viên..." 
            class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Bộ lọc
          </button>
        </div>
      </div>

      <!-- ── Sections Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Mã LHP</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Môn & Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Đăng ký / Sức chứa</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Trạng thái</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sec in sections" :key="sec.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <span class="text-xs font-black text-slate-800">{{ sec.id }}</span>
              </td>
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ sec.subject }}</p>
                <p class="text-[11px] font-bold text-slate-400 mt-1 uppercase tracking-tighter">{{ sec.teacher }}</p>
              </td>
              <td class="px-6 py-4">
                <div class="space-y-1.5">
                  <div class="flex items-center justify-between">
                    <span class="text-[11px] font-black text-slate-700">{{ sec.enrolled }} / {{ sec.capacity }}</span>
                    <span v-if="sec.waitlist > 0" class="text-[10px] font-bold text-amber-600">+{{ sec.waitlist }} waitlist</span>
                  </div>
                  <div class="h-1.5 w-32 bg-slate-100 rounded-full overflow-hidden">
                    <div 
                      :class="['h-full transition-all', sec.enrolled >= sec.capacity ? 'bg-amber-500' : 'bg-blue-500']"
                      :style="{ width: (sec.enrolled / sec.capacity) * 100 + '%' }"
                    ></div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border', getStatusBadge(sec.status)]">
                  {{ sec.status.replace('_', ' ') }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Sửa sức chứa">
                    <Edit3 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-rose-50 hover:text-rose-600 rounded-lg text-slate-400 transition-all" title="Hủy lớp">
                    <Power :size="16" />
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

    </div>
  </PageContainer>
</template>
