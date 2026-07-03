<script setup>
import { ref } from 'vue'
import {
  Clock, Filter, AlertCircle, Send, Eye, X
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const schedules = ref([
  { id: 'PD-241', term: 'Học kỳ 1 - 2026', type: 'Chính quy', department: 'Khoa Công nghệ Thông tin', created: '10/05/2026', submitted: '12/05/2026', status: 'pending', metrics: { classes: 120, teachers: 45, hours: 360 } },
  { id: 'PD-242', term: 'Học kỳ 1 - 2026', type: 'Chất lượng cao', department: 'Khoa Công nghệ Thông tin', created: '11/05/2026', submitted: '12/05/2026', status: 'returned', metrics: { classes: 45, teachers: 20, hours: 150 }, note: 'Trùng lịch phòng thực hành máy Mac.' },
])

const selectedItem = ref(null)

const statusLabels = {
  pending: { label: 'Chờ duyệt', variant: 'warning' },
  returned: { label: 'Cần chỉnh sửa', variant: 'danger' }
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <Clock class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Lịch chờ duyệt</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Danh sách thời khóa biểu đang chờ Ban giám hiệu phê duyệt.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary"><Filter :size="15" class="mr-1" /> Lọc</GlassButton>
      </div>
    </div>

    <!-- Main Content Layout -->
    <div class="flex flex-1 min-h-0 gap-4 flex-col lg:flex-row">
      <!-- Left: List -->
      <div class="flex-1 surface-card border border-(--border-card) rounded-2xl p-4 flex flex-col gap-3 min-w-0 overflow-y-auto">
        <div v-for="item in schedules" :key="item.id"
             @click="selectedItem = item"
             class="surface-card border rounded-2xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden group flex flex-col lg:flex-row lg:items-center gap-4"
             :class="selectedItem?.id === item.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary) bg-(--lg-primary)/5' : 'border-(--border-card)'">

          <div class="absolute left-0 top-0 bottom-0 w-1" :class="item.status === 'pending' ? 'bg-amber-500' : 'bg-red-500'"></div>

          <!-- Info -->
          <div class="flex-1 pl-2">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-xs font-mono font-bold text-(--text-muted)">{{ item.id }}</span>
              <GlassBadge :variant="statusLabels[item.status].variant" size="sm">{{ statusLabels[item.status].label }}</GlassBadge>
            </div>
            <h3 class="font-bold text-(--text-heading) text-base">{{ item.term }}</h3>
            <p class="text-sm text-(--text-body) font-medium">{{ item.department }} - {{ item.type }}</p>
          </div>

          <!-- Metrics -->
          <div class="flex items-center gap-4 py-2 lg:py-0 border-y lg:border-y-0 lg:border-l border-(--border-default) lg:px-4 shrink-0">
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Lớp</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.classes }}</p>
             </div>
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Giảng viên</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.teachers }}</p>
             </div>
             <div class="text-center">
                <p class="text-[10px] uppercase text-(--text-muted) font-bold">Tiết/Tuần</p>
                <p class="font-bold text-(--text-heading)">{{ item.metrics.hours }}</p>
             </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-wrap lg:flex-nowrap gap-2 shrink-0 lg:pl-2" @click.stop>
             <GlassButton v-if="item.status === 'pending'" variant="danger" size="sm" class="flex-1 lg:flex-none justify-center">Thu hồi</GlassButton>
             <GlassButton v-if="item.status === 'returned'" variant="primary" size="sm" class="flex-1 lg:flex-none justify-center"><Send :size="14" class="mr-1"/>Gửi lại</GlassButton>
             <GlassButton variant="secondary" size="sm" class="flex-1 lg:flex-none justify-center" @click="selectedItem = item"><Eye :size="14" class="mr-1"/>Chi tiết</GlassButton>
          </div>
        </div>
      </div>

      <!-- Right: Detail Panel -->
      <div v-if="selectedItem" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
        <div class="surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
          <div class="p-4 border-b border-(--border-default) flex justify-between items-center bg-(--surface-input)">
            <h3 class="font-bold text-(--text-heading)">Chi tiết bộ TKB</h3>
            <button class="text-(--text-muted) hover:text-(--text-heading)" @click="selectedItem = null"><X :size="16" /></button>
          </div>

          <div class="p-4 flex-1 overflow-auto space-y-5">
            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Thông tin chung</p>
              <div class="space-y-2 text-sm text-(--text-body)">
                <div class="flex justify-between"><span class="text-(--text-muted)">Mã TKB:</span> <span class="font-mono font-bold">{{ selectedItem.id }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Học kỳ:</span> <span class="font-medium text-right">{{ selectedItem.term }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Đơn vị:</span> <span class="font-medium text-right">{{ selectedItem.department }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Trạng thái:</span>
                  <GlassBadge :variant="statusLabels[selectedItem.status].variant" size="sm">{{ statusLabels[selectedItem.status].label }}</GlassBadge>
                </div>
              </div>
            </div>

            <div v-if="selectedItem.note" class="p-3 bg-(--color-danger-bg) border border-(--color-danger-border) rounded-xl">
              <p class="text-xs font-bold text-(--color-danger-text) flex items-center gap-1 mb-1"><AlertCircle :size="14"/> Nhận xét từ BGH</p>
              <p class="text-sm text-(--color-danger-text) opacity-90">{{ selectedItem.note }}</p>
            </div>

            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Quy trình (Timeline)</p>
              <div class="relative pl-3 border-l-2 border-(--border-default) space-y-4 text-sm mt-2">
                 <div class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-[var(--text-muted)] mt-1.5"></div>
                   <p class="font-bold text-(--text-heading)">Tạo bản nháp</p>
                   <p class="text-xs text-(--text-muted)">{{ selectedItem.created }}</p>
                 </div>
                 <div class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-amber-400 mt-1.5 ring-2 ring-amber-100 dark:ring-amber-900"></div>
                   <p class="font-bold text-(--text-heading)">Gửi BGH duyệt</p>
                   <p class="text-xs text-(--text-muted)">{{ selectedItem.submitted }}</p>
                 </div>
                 <div v-if="selectedItem.status === 'returned'" class="relative">
                   <div class="absolute -left-4 w-2 h-2 rounded-full bg-red-500 mt-1.5 ring-2 ring-red-100 dark:ring-red-900"></div>
                   <p class="font-bold text-red-600 dark:text-red-400">Yêu cầu chỉnh sửa</p>
                   <p class="text-xs text-(--text-muted)">13/05/2026</p>
                 </div>
              </div>
            </div>
          </div>

          <div class="p-4 border-t border-(--border-default)">
            <GlassButton variant="secondary" class="w-full justify-center">Chỉnh sửa nội dung</GlassButton>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>
