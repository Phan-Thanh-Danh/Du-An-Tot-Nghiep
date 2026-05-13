<script setup>
import { ref } from 'vue'
import { 
  CheckCircle2, 
  Search, 
  Filter, 
  Calendar, 
  XCircle, 
  RefreshCw, 
  History, 
  Bell,
  MoreVertical,
  ChevronDown
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const publishedSchedules = ref([
  { id: 1, subject: 'Lập trình Java', class: 'SE1601', teacher: 'Nguyễn Văn A', room: 'P.302', time: '07:30 - 10:30', day: 'Thứ 2', status: 'published' },
  { id: 2, subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'Trần Thị B', room: 'P.105', time: '13:30 - 15:30', day: 'Thứ 3', status: 'published' },
  { id: 3, subject: 'Hệ quản trị CSDL', class: 'SE1603', teacher: 'Lê Văn C', room: 'Lab 2', time: '08:30 - 11:30', day: 'Thứ 4', status: 'published' },
])
</script>

<template>
  <PageContainer 
    title="Lịch đã publish" 
    subtitle="Thời khóa biểu chính thức đã công bố. Mọi chỉnh sửa sẽ được lưu audit log và gửi thông báo cho GV/SV."
  >
    <div class="space-y-6">
      
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo môn, lớp, giảng viên..." 
            class="w-full bg-white border border-slate-100 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Bộ lọc nâng cao
          </button>
        </div>
      </div>

      <!-- ── Published Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thời gian</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Môn & Lớp</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Phòng</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in publishedSchedules" :key="item.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800">{{ item.day }}</p>
                <p class="text-xs font-bold text-blue-600 mt-0.5">{{ item.time }}</p>
              </td>
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ item.subject }}</p>
                <p class="text-[11px] font-bold text-slate-400 mt-1 uppercase tracking-tighter">{{ item.class }}</p>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-full bg-blue-50 flex items-center justify-center text-[10px] font-black text-blue-600 border border-blue-100">
                    {{ item.teacher.split(' ').pop().charAt(0) }}
                  </div>
                  <span class="text-sm font-bold text-slate-700">{{ item.teacher }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <span class="text-sm font-black text-slate-800">{{ item.room }}</span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-1">
                   <button class="p-2 hover:bg-orange-50 hover:text-orange-600 rounded-lg text-slate-400 transition-all" title="Hủy buổi / Thay đổi">
                      <XCircle :size="16" />
                   </button>
                   <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Lịch học bù">
                      <RefreshCw :size="16" />
                   </button>
                   <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                      <History :size="16" />
                   </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Summary Info ── -->
      <div class="flex flex-col md:flex-row items-center justify-between gap-6 px-4 py-2">
         <div class="flex items-center gap-4">
            <div class="flex items-center gap-2">
               <span class="h-3 w-3 rounded-full bg-green-500 shadow-sm shadow-green-200"></span>
               <span class="text-xs font-bold text-slate-600">342 Buổi học đã publish</span>
            </div>
            <div class="flex items-center gap-2 text-xs font-bold text-slate-400">
               <History :size="14" />
               Lần cuối cập nhật: 10:30 Hôm nay
            </div>
         </div>
         <button class="lg-button-secondary px-6 py-2.5 text-xs font-black uppercase tracking-widest flex items-center gap-2">
            <Bell :size="14" /> Gửi thông báo toàn bộ
         </button>
      </div>
    </div>
  </PageContainer>
</template>
