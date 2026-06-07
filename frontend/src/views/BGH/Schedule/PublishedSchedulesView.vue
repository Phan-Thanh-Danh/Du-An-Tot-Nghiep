<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Clock, 
  MapPin, 
  User, 
  Eye, 
  History, 
  ExternalLink,
  ChevronLeft,
  ChevronRight,
  Download
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const viewMode = ref('Week')
const schedules = ref([
  { id: 1, subject: 'Lập trình Mobile (React Native)', class: 'SE1601', teacher: 'Nguyễn Văn A', room: 'P.302', time: '07:30 - 10:30', day: 'Thứ 2', type: 'offline', status: 'published' },
  { id: 2, subject: 'Cơ sở dữ liệu NoSQL', class: 'SE1602', teacher: 'Trần Thị B', room: 'MS Teams', time: '13:30 - 15:30', day: 'Thứ 3', type: 'online', link: 'https://teams.microsoft.com/l/meetup-join/...', status: 'published' },
  { id: 3, subject: 'Web Frontend Advanced', class: 'SE1603', teacher: 'Lê Văn C', room: 'Lab 2', time: '08:30 - 11:30', day: 'Thứ 4', type: 'offline', status: 'published' },
])
</script>

<template>
  <PageContainer 
    title="Thời khóa biểu đã duyệt" 
    subtitle="Xem và theo dõi các bộ TKB đã được công bố chính thức cho sinh viên và giảng viên."
  >
    <template #actions>
      <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
         <Download :size="18" /> Xuất dữ liệu
      </button>
    </template>

    <div class="space-y-4">
      
      <!-- ── Calendar Toolbar ── -->
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3">
           <div class="flex items-center surface-solid rounded-xl p-1 border border-default">
              <button 
                v-for="mode in ['Day', 'Week', 'Month']" 
                :key="mode"
                @click="viewMode = mode"
                :class="[
                  'px-4 py-1.5 text-xs font-bold rounded-lg transition-all',
                  viewMode === mode ? 'surface-card text-link shadow-sm' : 'text-muted hover:text-heading'
                ]"
              >
                {{ mode }}
              </button>
           </div>
           <div class="flex items-center gap-2">
              <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted transition-colors">
                 <ChevronLeft :size="20" />
              </button>
              <span class="text-sm font-bold text-heading">12/05 - 18/05, 2026</span>
              <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted transition-colors">
                 <ChevronRight :size="20" />
              </button>
           </div>
        </div>

        <div class="flex items-center gap-3">
           <div class="relative">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm lớp, môn..." class="surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-xs font-bold outline-none focus:border-[var(--border-input-focus)]">
           </div>
           <button class="lg-icon-button surface-input border border-input p-2 text-muted">
              <Filter :size="18" />
           </button>
        </div>
      </div>

      <!-- ── Schedule List (Read-only for BGH) ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Lớp & Môn học</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Giảng viên</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Thời gian & Phòng</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default">Hình thức</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest border-b border-default text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="sch in schedules" :key="sch.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div :class="['h-9 w-9 rounded-xl flex items-center justify-center font-semibold text-[10px] uppercase shadow-sm border', sch.type === 'online' ? 'bg-[var(--color-info-bg)] text-link border-[var(--color-info-text)]/20' : 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20']">
                    {{ sch.class.slice(0, 2) }}
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ sch.subject }}</p>
                    <p class="text-[11px] font-bold text-muted mt-0.5">{{ sch.class }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                   <div class="h-6 w-6 rounded-full surface-solid flex items-center justify-center text-muted overflow-hidden">
                      <User :size="14" />
                   </div>
                   <span class="text-xs font-bold text-label">{{ sch.teacher }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex flex-col gap-1">
                   <div class="flex items-center gap-1.5 text-xs font-bold text-label">
                      <Clock :size="12" class="text-placeholder" /> {{ sch.day }}, {{ sch.time }}
                   </div>
                   <div class="flex items-center gap-1.5 text-[10px] font-bold text-muted">
                      <MapPin :size="12" class="text-placeholder" /> {{ sch.room }}
                   </div>
                </div>
              </td>
              <td class="px-4 py-3">
                <span :class="['px-2 py-0.5 rounded-lg text-[9px] font-semibold uppercase tracking-widest border', sch.type === 'online' ? 'bg-[var(--color-info-bg)] text-link border-[var(--color-info-text)]/20' : 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[var(--color-success-text)]/20']">
                  {{ sch.type }}
                </span>
                <div v-if="sch.type === 'online'" class="mt-1 flex items-center gap-1 text-[9px] font-semibold text-link uppercase cursor-pointer hover:underline">
                   <ExternalLink :size="10" /> Join Link
                </div>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-muted transition-all" title="Xem chi tiết">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-[var(--surface-input)] rounded-lg text-muted transition-all" title="Lịch sử duyệt">
                    <History :size="18" />
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
