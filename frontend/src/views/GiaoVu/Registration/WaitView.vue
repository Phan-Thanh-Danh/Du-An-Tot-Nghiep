<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  CheckCircle2, 
  XCircle, 
  Bell, 
  Clock, 
  User, 
  ArrowRight,
  MoreVertical,
  ArrowLeftRight
} from 'lucide-vue-next'
import PageContainer from '../../../components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const waitlist = ref([
  { position: 1, student: 'Lê Thị Mai', studentCode: 'SV002', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 08:30', status: 'waiting' },
  { position: 2, student: 'Trần Văn Hoàng', studentCode: 'SV005', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 09:15', status: 'waiting' },
  { position: 3, student: 'Nguyễn Bích Liên', studentCode: 'SV008', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 10:00', status: 'confirmed' },
  { position: 4, student: 'Hoàng Minh Tuấn', studentCode: 'SV012', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 11:20', status: 'expired' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'waiting': return 'bg-amber-100 text-amber-700'
    case 'confirmed': return 'bg-emerald-100 text-emerald-700'
    case 'expired': return 'bg-rose-100 text-rose-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Danh sách hàng chờ (Waitlist)" 
    subtitle="Quản lý thứ tự đăng ký khi lớp học phần đã đầy sức chứa."
  >
    <div class="space-y-4">
      
      <!-- ── Search & Filter ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input 
            type="text" 
            placeholder="Tìm theo SV hoặc Lớp học phần..." 
            class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Lọc lớp
          </button>
        </div>
      </div>

      <!-- ── Waitlist Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">STT</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Sinh viên</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Lớp & Môn</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thời gian vào</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="item in waitlist" :key="item.position" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                <span class="text-sm font-black text-placeholder">#{{ item.position }}</span>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading">{{ item.student }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-0.5">{{ item.studentCode }}</p>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading leading-tight">{{ item.subject }}</p>
                <p class="text-[10px] font-bold text-link mt-1 uppercase tracking-tighter">{{ item.section }}</p>
              </td>
              <td class="px-4 py-4">
                <span class="text-xs font-medium text-label">{{ item.time }}</span>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest', getStatusBadge(item.status)]">
                  {{ item.status }}
                </span>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1">
                  <button v-if="item.status === 'waiting'" class="p-2 hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] rounded-lg text-placeholder transition-all" title="Xác nhận vào lớp">
                    <CheckCircle2 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all" title="Chuyển lớp khác">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] rounded-lg text-placeholder transition-all" title="Loại bỏ">
                    <XCircle :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Waitlist Info ── -->
      <div class="lg-card-glass p-4 border border-[var(--color-warning-bg)]/30">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--lg-warning)] shrink-0">
             <Bell :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-black text-heading">Quy trình tự động</h4>
            <p class="text-xs text-label mt-2 leading-relaxed">
              Hệ thống xử lý hàng chờ theo nguyên tắc <strong>FIFO (First In First Out)</strong>. Khi một sinh viên hủy môn hoặc Giáo vụ tăng sức chứa lớp, hệ thống sẽ tự động gửi thông báo cho sinh viên đầu hàng chờ. Sinh viên có <strong>24 giờ</strong> để xác nhận đăng ký trước khi lượt bị chuyển cho người tiếp theo.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
