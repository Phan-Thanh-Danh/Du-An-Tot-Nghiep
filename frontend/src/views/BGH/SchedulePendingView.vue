<script setup>
import { ref } from 'vue'
import {
  Clock, Filter, CheckCircle2, XCircle, Eye, X
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'

const schedules = ref([
  { id: 'PD-241', term: 'Học kỳ 1 - 2026', type: 'Chính quy', department: 'Khoa CNTT', created: '10/05/2026', submitter: 'Nguyễn Văn A (Giáo vụ)', status: 'pending', metrics: { classes: 120, teachers: 45, hours: 360 } },
  { id: 'PD-243', term: 'Học kỳ 1 - 2026', type: 'Chất lượng cao', department: 'Khoa Kinh tế', created: '11/05/2026', submitter: 'Trần Thị B (Giáo vụ)', status: 'pending', metrics: { classes: 80, teachers: 30, hours: 240 } },
])

const selectedItem = ref(null)
const confirmDialog = ref({ isOpen: false, action: null, message: '' })

function openConfirm(action, message) {
  confirmDialog.value = { isOpen: true, action, message }
}

function handleConfirm() {
  confirmDialog.value.isOpen = false
  selectedItem.value = null
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <Clock class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Duyệt Thời khóa biểu</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Phê duyệt thời khóa biểu từ các khoa trước khi công bố chính thức.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary"><Filter :size="15" class="mr-1" /> Bộ lọc</GlassButton>
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

          <div class="absolute left-0 top-0 bottom-0 w-1 bg-amber-500"></div>

          <!-- Info -->
          <div class="flex-1 pl-2">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-xs font-mono font-bold text-(--text-muted)">{{ item.id }}</span>
              <GlassBadge variant="warning" size="xs">Chờ duyệt</GlassBadge>
            </div>
            <h3 class="font-bold text-(--text-heading) text-base">{{ item.department }}</h3>
            <p class="text-sm text-(--text-body) font-medium">{{ item.term }} - {{ item.type }}</p>
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
          </div>

          <!-- Actions -->
          <div class="flex flex-wrap lg:flex-nowrap gap-2 shrink-0 lg:pl-2" @click.stop>
             <GlassButton variant="primary" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('approve', 'Phê duyệt thời khóa biểu này?')"><CheckCircle2 :size="14" class="mr-1"/>Duyệt</GlassButton>
             <GlassButton variant="danger" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('reject', 'Trả về giáo vụ yêu cầu chỉnh sửa?')"><XCircle :size="14" class="mr-1"/>Trả về</GlassButton>
          </div>
        </div>

        <div v-if="schedules.length === 0" class="flex-1 flex flex-col items-center justify-center text-(--text-muted) p-8">
          <CheckCircle2 :size="48" class="text-emerald-500/50 mb-4" />
          <p class="font-bold text-(--text-heading)">Không có dữ liệu chờ duyệt</p>
        </div>
      </div>

      <!-- Right: Detail Panel -->
      <div v-if="selectedItem" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
        <div class="surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
          <div class="p-4 border-b border-(--border-default) flex justify-between items-center bg-(--surface-input)">
            <h3 class="font-bold text-(--text-heading)">Chi tiết TKB</h3>
            <button class="text-(--text-muted) hover:text-(--text-heading)" @click="selectedItem = null"><X :size="16" /></button>
          </div>

          <div class="p-4 flex-1 overflow-auto space-y-5">
            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Thông tin chung</p>
              <div class="space-y-2 text-sm text-(--text-body)">
                <div class="flex justify-between"><span class="text-(--text-muted)">Học kỳ:</span> <span class="font-medium text-right">{{ selectedItem.term }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Đơn vị:</span> <span class="font-medium text-right">{{ selectedItem.department }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Người nộp:</span> <span class="font-medium text-right">{{ selectedItem.submitter }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Ngày nộp:</span> <span class="font-medium text-right">{{ selectedItem.created }}</span></div>
              </div>
            </div>

            <div class="p-3 bg-(--surface-input) rounded-xl border border-(--border-default)">
              <p class="text-xs font-bold text-(--text-heading) mb-2">Báo cáo kiểm tra xung đột hệ thống</p>
              <div class="flex items-center gap-2 text-emerald-600 dark:text-emerald-400 text-sm font-medium">
                <CheckCircle2 :size="16" /> Hoàn toàn hợp lệ (0 xung đột)
              </div>
            </div>

            <GlassButton variant="secondary" class="w-full justify-center text-link"><Eye :size="14" class="mr-1"/> Xem dữ liệu chi tiết</GlassButton>
          </div>
        </div>
      </div>
    </div>
  </div>
  <ConfirmActionDialog
    :is-open="confirmDialog.isOpen"
    title="Xác nhận phê duyệt"
    :message="confirmDialog.message"
    confirm-text="Đồng ý"
    @confirm="handleConfirm"
    @cancel="confirmDialog.isOpen = false"
  />
</template>
