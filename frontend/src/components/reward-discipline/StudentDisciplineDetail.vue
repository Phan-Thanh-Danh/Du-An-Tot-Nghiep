<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import DisciplineStatusBadge from './DisciplineStatusBadge.vue'
import RewardDisciplineTimeline from './RewardDisciplineTimeline.vue'
import { ArrowLeft, AlertTriangle } from 'lucide-vue-next'
import dayjs from 'dayjs'

defineProps({
  record: { type: Object, required: true }
})
const emit = defineEmits(['back', 'appeal'])
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center gap-4">
      <button @click="emit('back')" class="p-2 hover:bg-(--surface-hover) rounded-full text-(--text-muted)">
        <ArrowLeft class="w-5 h-5" />
      </button>
      <div>
        <h2 class="text-xl font-bold text-(--text-heading) flex items-center gap-3">
          Chi tiết kỷ luật
          <DisciplineStatusBadge :status="record.trangThai" />
        </h2>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 space-y-6">
        <GlassPanel padding="normal" class="border-t-4 border-t-(--color-danger-border)">
          <h3 class="text-lg font-semibold text-(--text-heading) mb-2">{{ record.tieuDe }}</h3>
          <p class="text-(--text-body) mb-4">{{ record.moTaCongKhai || record.moTa }}</p>
          
          <div class="grid grid-cols-2 gap-4 text-sm mt-6">
            <div>
              <div class="text-(--text-muted) mb-1">Mức độ kỷ luật</div>
              <div class="font-medium text-(--text-heading)">{{ record.mucDoKyLuat }}</div>
            </div>
            <div>
              <div class="text-(--text-muted) mb-1">Hình thức xử lý</div>
              <div class="font-medium text-(--text-heading)">{{ record.hinhThucXuLy }}</div>
            </div>
            <div>
              <div class="text-(--text-muted) mb-1">Ngày vi phạm</div>
              <div class="font-medium text-(--text-heading)">{{ dayjs(record.ngayViPham).format('DD/MM/YYYY') }}</div>
            </div>
            <div>
              <div class="text-(--text-muted) mb-1">Thời hạn hiệu lực</div>
              <div class="font-medium text-(--text-heading)">{{ record.thoiHanHieuLuc ? dayjs(record.thoiHanHieuLuc).format('DD/MM/YYYY') : 'Vô thời hạn' }}</div>
            </div>
          </div>
        </GlassPanel>

        <!-- Trạng thái khiếu nại -->
        <GlassPanel padding="normal" v-if="record.coTheKhieuNai || record.appealStatus">
          <div class="flex items-center gap-3">
            <AlertTriangle class="w-5 h-5 text-(--color-warning-text)" />
            <div class="flex-1">
              <h4 class="font-semibold text-(--text-heading)">Kháng nghị / Khiếu nại</h4>
              <p class="text-sm text-(--text-muted)" v-if="record.appealStatus === 'pending'">Yêu cầu khiếu nại của bạn đang được xem xét.</p>
              <p class="text-sm text-(--text-muted)" v-else-if="record.coTheKhieuNai">Bạn có quyền gửi khiếu nại nếu thấy quyết định chưa thỏa đáng.</p>
            </div>
            <GlassButton v-if="record.coTheKhieuNai && record.appealStatus !== 'pending'" variant="warning" size="sm" @click="emit('appeal')">
              Gửi khiếu nại
            </GlassButton>
            <DisciplineStatusBadge v-else-if="record.appealStatus" :status="record.appealStatus" />
          </div>
        </GlassPanel>
      </div>

      <div>
        <GlassPanel padding="normal">
          <h3 class="font-semibold text-(--text-heading) mb-4">Tiến trình</h3>
          <RewardDisciplineTimeline :events="record.timeline" />
        </GlassPanel>
      </div>
    </div>
  </div>
</template>
