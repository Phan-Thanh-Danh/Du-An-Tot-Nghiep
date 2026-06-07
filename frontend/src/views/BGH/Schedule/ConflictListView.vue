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
    case 'critical': return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20'
    case 'major': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[var(--color-warning-text)]/20'
    case 'minor': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20'
    default: return 'surface-solid text-muted border-default'
  }
}
</script>

<template>
  <PageContainer 
    title="Xung đột lịch học" 
    subtitle="Giám sát các lỗi sắp xếp tài nguyên (giảng viên, phòng học, thời gian) trong toàn hệ thống."
  >
    <div class="space-y-4">
      
      <!-- ── Stats Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="surface-card border border-[var(--color-danger-text)]/20 bg-[var(--color-danger-bg)] rounded-2xl p-4 flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-danger-text)] shadow-sm border border-[var(--color-danger-text)]/20">
              <ShieldAlert :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-semibold text-[var(--color-danger-text)] uppercase tracking-widest">Nghiêm trọng</p>
              <h3 class="text-xl font-semibold text-heading leading-tight">02</h3>
           </div>
        </div>
        <div class="surface-card border border-[var(--color-warning-text)]/20 bg-[var(--color-warning-bg)] rounded-2xl p-4 flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-warning-text)] shadow-sm border border-[var(--color-warning-text)]/20">
              <AlertTriangle :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-semibold text-[var(--color-warning-text)] uppercase tracking-widest">Trung bình</p>
              <h3 class="text-xl font-semibold text-heading leading-tight">01</h3>
           </div>
        </div>
        <div class="surface-card border border-[var(--color-info-text)]/20 bg-[var(--color-info-bg)] rounded-2xl p-4 flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-info-text)] shadow-sm border border-[var(--color-info-text)]/20">
              <Zap :size="24" />
           </div>
           <div>
              <p class="text-[10px] font-semibold text-[var(--color-info-text)] uppercase tracking-widest">Lỗi nhẹ</p>
              <h3 class="text-xl font-semibold text-heading leading-tight">01</h3>
           </div>
        </div>
      </div>

      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm theo đối tượng, loại xung đột..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:border-[var(--border-input-focus)]">
           </div>
           <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
              <Filter :size="18" /> Lọc mức độ
           </button>
        </div>
        <button class="lg-button-primary py-2.5 px-5 text-sm font-semibold flex items-center gap-2">
           <Bell :size="18" /> Yêu cầu GV xử lý tất cả
        </button>
      </div>

      <!-- ── Conflict Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Loại xung đột</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Chi tiết & Đối tượng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Thời gian</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Mức độ</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="cf in conflicts" :key="cf.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-3">
                 <p class="text-sm font-semibold text-heading leading-tight">{{ cf.type }}</p>
                 <p class="text-[10px] font-bold text-muted mt-1 uppercase tracking-tighter">{{ cf.id }}</p>
              </td>
              <td class="px-4 py-3 max-w-xs">
                 <p class="text-xs text-body leading-relaxed font-medium mb-2">{{ cf.description }}</p>
                 <div class="flex items-center gap-2 text-[10px] font-semibold text-link uppercase">
                    <User v-if="cf.type.includes('giảng viên')" :size="12" />
                    <MapPin v-else :size="12" />
                    {{ cf.affected }}
                 </div>
              </td>
              <td class="px-4 py-3">
                 <div class="flex items-center gap-1.5 text-xs font-bold text-label">
                    <Clock :size="14" class="text-placeholder" /> {{ cf.time }}
                 </div>
              </td>
              <td class="px-4 py-3">
                <span :class="['px-2 py-0.5 rounded-lg text-[9px] font-semibold uppercase tracking-widest border', getSeverityClass(cf.severity)]">
                  {{ cf.severity }}
                </span>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-muted transition-all" title="Gửi thông báo nhắc nhở">
                    <Bell :size="18" />
                  </button>
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted transition-all">
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
