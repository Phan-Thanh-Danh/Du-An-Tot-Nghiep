<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Plus, 
  UserPlus, 
  ArrowLeftRight, 
  Calendar,
  MoreHorizontal,
  ChevronDown,
  UserCheck,
  UserMinus
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const assignments = ref([
  { id: 'PC001', subject: 'Lập trình Java', class: 'SE1601', teacher: 'TS. Nguyễn Minh Khoa', sessions: 3, status: 'assigned' },
  { id: 'PC002', subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'ThS. Trần Thị Lan', sessions: 2, status: 'assigned' },
  { id: 'PC003', subject: 'Lập trình Web', class: 'SE1603', teacher: 'Chưa phân công', sessions: 3, status: 'unassigned' },
  { id: 'PC004', subject: 'Hệ quản trị CSDL', class: 'SE1604', teacher: 'ThS. Lê Văn Dũng', sessions: 3, status: 'assigned' },
  { id: 'PC005', subject: 'An toàn thông tin', class: 'SE1605', teacher: 'TS. Phạm Minh Tuấn', sessions: 2, status: 'assigned' },
])

const lecturers = [
  { id: 1, name: 'TS. Nguyễn Minh Khoa', load: '12/15', dept: 'CNTT' },
  { id: 2, name: 'ThS. Trần Thị Lan', load: '8/15', dept: 'CNTT' },
  { id: 3, name: 'TS. Phạm Minh Tuấn', load: '14/15', dept: 'ATTT' },
]

const getStatusBadge = (status) => {
  switch (status) {
    case 'assigned': return 'bg-green-100 text-green-700'
    case 'unassigned': return 'bg-red-100 text-red-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Phân công giảng viên" 
    subtitle="Quản lý gán giảng viên cho các lớp học phần trong học kỳ."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
        <Plus :size="18" /> Thêm phân công
      </button>
    </template>

    <div class="space-y-6">
      <!-- ── Stats Header ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600">
            <UserPlus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng phân công</p>
            <p class="text-2xl font-black text-slate-800">124</p>
          </div>
        </div>
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-orange-50 flex items-center justify-center text-orange-600">
            <UserMinus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Chưa gán GV</p>
            <p class="text-2xl font-black text-slate-800">12</p>
          </div>
        </div>
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-green-50 flex items-center justify-center text-green-600">
            <UserCheck :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hoàn tất</p>
            <p class="text-2xl font-black text-slate-800">90%</p>
          </div>
        </div>
      </div>

      <!-- ── Filter & Search ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo môn, lớp hoặc giảng viên..." 
            class="w-full bg-white border border-slate-100 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Bộ lọc
          </button>
          <div class="h-8 w-px bg-slate-200"></div>
          <button class="lg-icon-button bg-white border border-slate-100 p-2.5 text-slate-500">
            <Calendar :size="20" />
          </button>
        </div>
      </div>

      <!-- ── Assignment Table ── -->
      <div class="lg-table-shell">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Mã PC</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lớp & Môn</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Tiết/Tuần</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Trạng thái</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in assignments" :key="item.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <span class="text-xs font-black text-slate-800">{{ item.id }}</span>
              </td>
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ item.subject }}</p>
                <p class="text-[11px] font-bold text-blue-600 mt-1 uppercase">{{ item.class }}</p>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-[10px] font-black text-slate-500">
                    {{ item.teacher !== 'Chưa phân công' ? item.teacher.split(' ').pop().charAt(0) : '?' }}
                  </div>
                  <div>
                    <p :class="['text-sm font-bold', item.teacher === 'Chưa phân công' ? 'text-red-500 italic' : 'text-slate-700']">
                      {{ item.teacher }}
                    </p>
                    <p v-if="item.teacher !== 'Chưa phân công'" class="text-[10px] font-medium text-slate-400">Tải dạy: 12/15</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <span class="h-2 w-16 bg-slate-100 rounded-full overflow-hidden inline-flex">
                    <span :style="{ width: (item.sessions / 4) * 100 + '%' }" class="bg-blue-500 h-full"></span>
                  </span>
                  <span class="text-xs font-bold text-slate-700">{{ item.sessions }} tiết</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest', getStatusBadge(item.status)]">
                  {{ item.status === 'assigned' ? 'Đã gán' : 'Cần gán' }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Đổi giảng viên">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <MoreHorizontal :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Footer Info ── -->
      <div class="flex items-center justify-between px-4">
         <p class="text-xs font-medium text-slate-500">Hiển thị 5 / 124 bản ghi</p>
         <div class="flex items-center gap-1">
            <button v-for="p in 3" :key="p" :class="['h-8 w-8 rounded-lg flex items-center justify-center text-xs font-bold transition-all', p === 1 ? 'bg-blue-600 text-white shadow-md' : 'hover:bg-white text-slate-500']">
              {{ p }}
            </button>
         </div>
      </div>
    </div>
  </PageContainer>
</template>
