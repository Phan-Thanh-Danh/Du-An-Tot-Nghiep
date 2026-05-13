<script setup>
import { ref } from 'vue'
import { 
  CheckCircle2, 
  XCircle, 
  Eye, 
  Search, 
  Filter, 
  Calendar, 
  AlertTriangle, 
  Clock, 
  User, 
  ArrowRight,
  MessageSquare,
  Building2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingSets = ref([
  { id: 'TKB-001', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa CNTT', classes: 86, slots: 420, conflicts: 0, sender: 'Phạm Minh D', date: '12/05/2026', status: 'pending_approval' },
  { id: 'TKB-002', semester: 'Spring 2026', campus: 'Cơ sở 2', dept: 'Khoa Kinh tế', classes: 42, slots: 215, conflicts: 3, sender: 'Nguyễn Bích L', date: '11/05/2026', status: 'pending_approval' },
  { id: 'TKB-003', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa Ngoại ngữ', classes: 65, slots: 310, conflicts: 1, sender: 'Trần Văn K', date: '13/05/2026', status: 'pending_approval' },
])
</script>

<template>
  <PageContainer 
    title="Thời khóa biểu chờ duyệt" 
    subtitle="Ban giám hiệu phê duyệt các bộ Thời khóa biểu do các Khoa/Phòng giáo vụ trình duyệt."
  >
    <div class="space-y-6">
      
      <!-- ── Filters ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input 
                type="text" 
                placeholder="Tìm theo học kỳ, khoa..." 
                class="w-full bg-white border border-slate-200 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10"
              >
           </div>
           <select class="bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-xs font-bold outline-none">
              <option>Spring 2026</option>
              <option>Fall 2025</option>
           </select>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Bộ lọc nâng cao
        </button>
      </div>

      <!-- ── Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Khoa / Bộ phận</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Học kỳ & CS</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Quy mô</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Xung đột</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Người gửi</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="set in pendingSets" :key="set.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600">
                    <Building2 :size="18" />
                  </div>
                  <p class="text-sm font-black text-slate-800 leading-tight">{{ set.dept }}</p>
                </div>
              </td>
              <td class="px-6 py-4">
                <div>
                  <p class="text-xs font-black text-slate-700 leading-tight">{{ set.semester }}</p>
                  <p class="text-[10px] font-bold text-slate-400 mt-0.5">{{ set.campus }}</p>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex flex-col gap-1">
                   <span class="text-[10px] font-black text-slate-500 uppercase tracking-tighter">Lớp: {{ set.classes }}</span>
                   <span class="text-[10px] font-black text-slate-500 uppercase tracking-tighter">Lịch: {{ set.slots }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div v-if="set.conflicts === 0" class="flex items-center gap-1.5 text-emerald-600">
                   <CheckCircle2 :size="14" />
                   <span class="text-[10px] font-black uppercase tracking-widest">Sẵn sàng</span>
                </div>
                <div v-else class="flex items-center gap-1.5 text-rose-500">
                   <AlertTriangle :size="14" />
                   <span class="text-[10px] font-black uppercase tracking-widest">{{ set.conflicts }} lỗi nghiêm trọng</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2 text-xs font-bold text-slate-500">
                   <User :size="14" /> {{ set.sender }}
                </div>
                <p class="text-[10px] font-bold text-slate-400 mt-1">{{ set.date }}</p>
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Xem chi tiết TKB">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-emerald-50 hover:text-emerald-600 rounded-lg text-slate-400 transition-all" title="Phê duyệt">
                    <CheckCircle2 :size="18" />
                  </button>
                  <button class="p-2 hover:bg-rose-50 hover:text-rose-600 rounded-lg text-slate-400 transition-all" title="Từ chối">
                    <XCircle :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Approval Policy ── -->
      <div class="lg-card-glass p-6 border-blue-100 bg-blue-50/10">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-blue-100 flex items-center justify-center text-blue-600 shrink-0">
               <Clock :size="20" />
            </div>
            <div>
               <h4 class="text-sm font-black text-blue-900 uppercase tracking-wide">Chính sách phê duyệt TKB</h4>
               <p class="text-xs text-blue-700 mt-1 leading-relaxed">
                 Hệ thống chỉ cho phép <strong>Duyệt & Publish</strong> bộ TKB khi số lượng xung đột nghiêm trọng (trùng phòng, trùng giảng viên) bằng 0. Nếu bộ TKB còn tồn tại xung đột, BGH vui lòng gửi yêu cầu Giáo vụ chỉnh sửa (Reject with comment).
               </p>
            </div>
         </div>
      </div>

    </div>
  </PageContainer>
</template>
