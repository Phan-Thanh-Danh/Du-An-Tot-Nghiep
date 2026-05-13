<script setup>
import { ref } from 'vue'
import { 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  MessageSquare, 
  RefreshCw, 
  Search, 
  Filter,
  ArrowRight,
  MoreVertical,
  Mail,
  Users
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingClasses = ref([
  { id: 'LHP003', subject: 'Lập trình Web', enrolled: 12, minEnroll: 15, teacher: 'Lê Văn C', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
  { id: 'LHP008', subject: 'Kỹ năng mềm', enrolled: 8, minEnroll: 20, teacher: 'Trần Thị H', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
])

const cancelledClasses = ref([
  { id: 'LHP012', subject: 'Triết học Mác-Lênin', enrolled: 5, minEnroll: 20, teacher: 'Nguyễn Văn K', status: 'cancelled', date: '12/05/2026' },
])
</script>

<template>
  <PageContainer 
    title="Hủy / Mở lại lớp" 
    subtitle="Xử lý các lớp học phần không đủ sĩ số tối thiểu hoặc cần đóng/mở lại theo nhu cầu."
  >
    <div class="space-y-8">
      
      <!-- ── Pending Cancellation ── -->
      <section class="space-y-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-black text-slate-800 flex items-center gap-2">
            <AlertCircle :size="22" class="text-rose-500" /> LỚP CHỜ HỦY (DƯỚI MIN ENROLL)
          </h3>
          <span class="px-2 py-0.5 rounded-lg bg-rose-50 text-rose-600 text-[10px] font-black uppercase tracking-widest">{{ pendingClasses.length }} Lớp</span>
        </div>

        <div class="grid grid-cols-1 gap-4">
          <div v-for="cls in pendingClasses" :key="cls.id" class="lg-card-glass p-6 flex flex-col md:flex-row md:items-center justify-between gap-6 group hover:border-rose-200 transition-all">
            <div class="flex items-center gap-6">
              <div class="h-14 w-14 rounded-2xl bg-rose-50 flex items-center justify-center text-rose-500 border border-rose-100 shrink-0">
                <Users :size="28" />
              </div>
              <div>
                <h4 class="text-base font-black text-slate-800">{{ cls.subject }}</h4>
                <div class="mt-1 flex items-center gap-3">
                  <span class="text-[10px] font-black text-blue-600 uppercase">{{ cls.id }}</span>
                  <span class="h-1 w-1 rounded-full bg-slate-300"></span>
                  <span class="text-xs font-bold text-slate-500">{{ cls.teacher }}</span>
                </div>
              </div>
            </div>

            <div class="flex flex-wrap items-center gap-6">
              <div class="px-6 border-x border-slate-100">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Sĩ số hiện tại</p>
                <p class="text-lg font-black text-rose-600">{{ cls.enrolled }} <span class="text-slate-300 font-medium">/ {{ cls.minEnroll }}</span></p>
              </div>

              <div class="flex items-center gap-2">
                <button class="lg-button-secondary px-4 py-2 text-xs font-bold text-emerald-600 hover:bg-emerald-50">
                  <CheckCircle2 :size="16" /> Mở lại lớp
                </button>
                <button class="lg-button-primary px-5 py-2.5 text-xs font-bold bg-rose-600 shadow-lg shadow-rose-500/20">
                  <XCircle :size="16" /> Xác nhận hủy
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ── Recently Cancelled ── -->
      <section class="space-y-4 pt-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-black text-slate-400 flex items-center gap-2">
            <RefreshCw :size="20" /> LỊCH SỬ LỚP ĐÃ HỦY
          </h3>
        </div>

        <div class="lg-table-shell overflow-hidden opacity-80 hover:opacity-100 transition-opacity">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-50/50">
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lớp học phần</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Sĩ số lúc hủy</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Ngày hủy</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thông báo</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-for="cls in cancelledClasses" :key="cls.id" class="group hover:bg-white/50 transition-colors">
                <td class="px-6 py-4">
                   <p class="text-sm font-black text-slate-700">{{ cls.subject }}</p>
                   <p class="text-[10px] font-bold text-slate-400 mt-1">{{ cls.id }}</p>
                </td>
                <td class="px-6 py-4">
                   <span class="text-sm font-bold text-slate-600">{{ cls.enrolled }} SV</span>
                </td>
                <td class="px-6 py-4">
                   <span class="text-xs font-medium text-slate-500">{{ cls.date }}</span>
                </td>
                <td class="px-6 py-4">
                   <div class="flex items-center gap-1.5 text-emerald-500">
                      <Mail :size="14" /> <span class="text-[10px] font-black uppercase tracking-widest">Đã gửi SV</span>
                   </div>
                </td>
                <td class="px-6 py-4 text-right">
                   <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400">
                      <MoreVertical :size="16" />
                   </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- ── Policy Note ── -->
      <div class="lg-card-glass p-6 bg-rose-50/20 border-rose-100">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-xl bg-white flex items-center justify-center text-rose-500 shadow-sm border border-rose-100 shrink-0">
             <MessageSquare :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-black text-rose-900">Lưu ý khi hủy lớp</h4>
            <p class="text-xs text-rose-700 mt-2 leading-relaxed">
              Khi xác nhận hủy lớp, hệ thống sẽ tự động hoàn trả tín chỉ cho sinh viên, giải phóng phòng học và gửi thông báo qua Email/App. Đối với các sinh viên đã thanh toán học phí cho môn này, hệ thống sẽ tự động tạo <strong>Credit Note</strong> (phiếu khấu trừ) cho đợt đóng tiền tiếp theo.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
