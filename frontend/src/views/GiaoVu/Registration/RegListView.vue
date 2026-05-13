<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Download, 
  UserPlus, 
  ArrowLeftRight, 
  Eye, 
  Trash2, 
  CheckCircle2, 
  XCircle,
  MoreVertical,
  History
} from 'lucide-vue-next'
import PageContainer from '../../../components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const enrollments = ref([
  { id: 1, student: 'Nguyễn Văn Nam', studentCode: 'SV001', section: 'LHP001', subject: 'Lập trình Java', status: 'enrolled', type: 'new', prereq: 'pass', date: '15/01/2026' },
  { id: 2, student: 'Lê Thị Mai', studentCode: 'SV002', section: 'LHP001', subject: 'Lập trình Java', status: 'waitlist', type: 'new', prereq: 'pass', date: '16/01/2026' },
  { id: 3, student: 'Trần Minh Tâm', studentCode: 'SV003', section: 'LHP002', subject: 'Cấu trúc dữ liệu', status: 'enrolled', type: 'retake', prereq: 'pass', date: '15/01/2026' },
  { id: 4, student: 'Phạm Hoàng Anh', studentCode: 'SV004', section: 'LHP003', subject: 'Lập trình Web', status: 'dropped', type: 'new', prereq: 'fail', date: '17/01/2026' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'enrolled': return 'bg-emerald-100 text-emerald-700'
    case 'waitlist': return 'bg-amber-100 text-amber-700'
    case 'dropped': return 'bg-rose-100 text-rose-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Danh sách đăng ký" 
    subtitle="Theo dõi và xử lý các yêu cầu đăng ký môn học của sinh viên."
  >
    <template #actions>
      <div class="flex items-center gap-3">
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
          <Download :size="18" /> Export
        </button>
        <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
          <UserPlus :size="18" /> Ghép HS thủ công
        </button>
      </div>
    </template>

    <div class="space-y-6">
      
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo SV, Mã SV hoặc Môn học..." 
            class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Lọc nâng cao
          </button>
        </div>
      </div>

      <!-- ── Enrollment Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Sinh viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lớp & Môn</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Prereq</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Trạng thái</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="en in enrollments" :key="en.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800">{{ en.student }}</p>
                <p class="text-[11px] font-bold text-slate-400 mt-0.5">{{ en.studentCode }}</p>
              </td>
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ en.subject }}</p>
                <div class="flex items-center gap-2 mt-1">
                  <span class="text-[10px] font-black text-blue-600 bg-blue-50 px-1.5 py-0.5 rounded">{{ en.section }}</span>
                  <span v-if="en.type === 'retake'" class="text-[9px] font-bold text-orange-600 border border-orange-200 px-1.5 py-0.5 rounded uppercase tracking-tighter">Học lại</span>
                </div>
              </td>
              <td class="px-6 py-4">
                 <div v-if="en.prereq === 'pass'" class="text-emerald-500 flex items-center gap-1.5">
                    <CheckCircle2 :size="16" /> <span class="text-[10px] font-black uppercase tracking-widest">Hợp lệ</span>
                 </div>
                 <div v-else class="text-rose-500 flex items-center gap-1.5">
                    <XCircle :size="16" /> <span class="text-[10px] font-black uppercase tracking-widest">Thiếu ĐK</span>
                 </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest', getStatusBadge(en.status)]">
                  {{ en.status }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Chuyển lớp">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button class="p-2 hover:bg-rose-50 hover:text-rose-600 rounded-lg text-slate-400 transition-all" title="Hủy đăng ký">
                    <Trash2 :size="16" />
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
