<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import DisciplineStatusBadge from './DisciplineStatusBadge.vue'
import { ArrowLeft, UserX, AlertTriangle } from 'lucide-vue-next'
import dayjs from 'dayjs'

const props = defineProps({
  record: { type: Object, required: true }
})
const emit = defineEmits(['back'])
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between border-b border-(--border-card) pb-4">
      <div class="flex items-center gap-4">
        <button @click="emit('back')" class="p-2 hover:bg-(--surface-hover) rounded-full text-(--text-muted)">
          <ArrowLeft class="w-5 h-5" />
        </button>
        <div>
          <h2 class="text-xl font-bold text-(--text-heading) flex items-center gap-3">
            Hồ sơ: {{ record.maHoSo }}
            <DisciplineStatusBadge :status="record.trangThai" />
          </h2>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 space-y-6">
        <GlassPanel padding="normal" class="border-t-4 border-t-(--color-danger-border)">
          <h3 class="text-lg font-semibold text-(--text-heading) mb-4 flex items-center gap-2">
            <UserX class="w-5 h-5 text-(--color-danger-text)" /> Chi tiết vi phạm
          </h3>
          <div class="grid grid-cols-2 gap-4 text-sm mb-6 bg-(--surface-hover) p-4 rounded-lg">
            <div><span class="text-(--text-muted)">Sinh viên:</span> <span class="font-medium text-(--text-heading)">{{ record.hoTen }}</span></div>
            <div><span class="text-(--text-muted)">MSSV:</span> <span class="font-medium text-(--text-heading)">{{ record.maSinhVien }}</span></div>
            <div><span class="text-(--text-muted)">Tiêu đề:</span> <span class="font-medium text-(--text-heading)">{{ record.tieuDe }}</span></div>
            <div><span class="text-(--text-muted)">Ngày vi phạm:</span> <span class="font-medium text-(--text-heading)">{{ dayjs(record.ngayViPham).format('DD/MM/YYYY') }}</span></div>
            <div class="col-span-2"><span class="text-(--text-muted)">Căn cứ xử lý:</span> <span class="font-medium text-(--text-heading)">{{ record.canCuXuLy }}</span></div>
          </div>
          
          <h4 class="font-medium text-(--text-heading) mb-2">Mô tả vi phạm</h4>
          <p class="text-(--text-body) p-3 bg-(--surface-input) rounded-lg border border-(--border-input)">{{ record.moTa }}</p>
        </GlassPanel>
      </div>

      <div class="space-y-6">
        <GlassPanel padding="normal">
          <h3 class="font-semibold text-(--text-heading) mb-4">Mức độ & Hình thức</h3>
          <div class="p-3 bg-(--color-danger-bg) rounded-lg text-center mb-4">
            <div class="font-bold text-xl text-(--color-danger-text)">{{ record.mucDoKyLuat }}</div>
            <div class="text-sm mt-1 text-(--text-body)">{{ record.hinhThucXuLy }}</div>
          </div>
          <div v-if="record.appealCount > 0" class="flex items-center gap-2 text-(--color-warning-text) text-sm mt-4 p-3 bg-(--color-warning-bg) rounded-lg">
            <AlertTriangle class="w-4 h-4" />
            Đang có {{ record.appealCount }} khiếu nại chưa xử lý.
          </div>
        </GlassPanel>
      </div>
    </div>
  </div>
</template>
