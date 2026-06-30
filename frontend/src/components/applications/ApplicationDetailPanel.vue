<script setup>
import { computed } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ApplicationStatusBadge from './ApplicationStatusBadge.vue'
import ApplicationTimeline from './ApplicationTimeline.vue'
import ApplicationFormRenderer from './ApplicationFormRenderer.vue'
import ApplicationEvidenceUploader from './ApplicationEvidenceUploader.vue'
import { ArrowLeft, Clock, FileText, AlertTriangle } from 'lucide-vue-next'


const props = defineProps({
  application: { type: Object, required: true },
  schema: { type: Array, default: () => [] }
})
const emit = defineEmits(['back', 'edit', 'cancel', 'resubmit'])

const isSupplementRequired = computed(() => props.application.trangThai === 'YEU_CAU_BO_SUNG')
const canEdit = computed(() => ['NHAP', 'YEU_CAU_BO_SUNG'].includes(props.application.trangThai))
const canCancel = computed(() => ['NHAP', 'DA_NOP', 'YEU_CAU_BO_SUNG'].includes(props.application.trangThai))

const parsedFormData = computed(() => {
  try {
    return JSON.parse(props.application.duLieuForm || '{}')
  } catch {
    return {}
  }
})

const getSupplementNote = computed(() => {
  if (!isSupplementRequired.value) return ''
  // Tìm event YEU_CAU_BO_SUNG mới nhất trong timeline
  const ev = [...(props.application.lichSu || [])].reverse().find(x => x.loaiSuKien === 'YEU_CAU_BO_SUNG')
  return ev?.ghiChu || 'Vui lòng bổ sung thông tin theo yêu cầu.'
})
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <!-- Header -->
    <div class="flex items-center gap-4 border-b border-(--border-card) pb-4">
      <button @click="emit('back')" class="p-2 hover:bg-(--surface-hover) rounded-full transition-colors text-(--text-muted) hover:text-(--text-heading)">
        <ArrowLeft class="w-5 h-5" />
      </button>
      <div class="flex-1">
        <h2 class="text-xl font-bold text-(--text-heading) flex items-center gap-3">
          {{ application.tieuDe }}
          <ApplicationStatusBadge :status="application.trangThai" />
        </h2>
        <p class="text-sm text-(--text-muted) mt-1 flex gap-4">
          <span>Mã đơn: {{ application.maDon }}</span>
          <span>Loại: {{ application.tenLoaiDon || '---' }}</span>
        </p>
      </div>
      <div class="flex gap-2">
        <GlassButton v-if="canCancel" variant="ghost" @click="emit('cancel')">Hủy đơn</GlassButton>
        <GlassButton v-if="canEdit" variant="primary" @click="emit('edit')">Chỉnh sửa</GlassButton>
      </div>
    </div>

    <!-- Alert for YEU_CAU_BO_SUNG -->
    <div v-if="isSupplementRequired" class="p-4 bg-(--color-warning-bg) border border-(--color-warning-border) rounded-xl flex gap-3">
      <AlertTriangle class="w-5 h-5 text-(--color-warning-text) shrink-0 mt-0.5" />
      <div>
        <h4 class="font-semibold text-(--color-warning-text)">Yêu cầu bổ sung thông tin</h4>
        <p class="text-sm mt-1 text-(--text-body)">{{ getSupplementNote }}</p>
        <GlassButton variant="warning" size="sm" class="mt-3" @click="emit('edit')">Bổ sung ngay</GlassButton>
      </div>
    </div>

    <!-- Layout Split -->
    <div class="flex-1 grid grid-cols-1 lg:grid-cols-3 gap-6 overflow-hidden">
      <!-- Main Content -->
      <div class="lg:col-span-2 space-y-6 overflow-y-auto custom-scrollbar pr-2">
        <GlassPanel padding="normal">
          <h3 class="font-semibold text-(--text-heading) flex items-center gap-2 mb-4 border-b border-(--border-card) pb-2">
            <FileText class="w-4 h-4" /> Nội dung đơn
          </h3>
          <ApplicationFormRenderer 
            :schema="schema" 
            :model-value="parsedFormData"
            readonly 
          />
        </GlassPanel>

        <GlassPanel padding="normal">
          <h3 class="font-semibold text-(--text-heading) mb-4 border-b border-(--border-card) pb-2">Minh chứng đính kèm</h3>
          <ApplicationEvidenceUploader 
            :files="application.minhChung" 
            disabled 
          />
        </GlassPanel>
      </div>

      <!-- Sidebar -->
      <div class="space-y-6 overflow-y-auto custom-scrollbar pr-2">
        <GlassPanel padding="normal">
          <h3 class="font-semibold text-(--text-heading) mb-4 border-b border-(--border-card) pb-2 flex items-center gap-2">
            <Clock class="w-4 h-4" /> Lịch sử xử lý
          </h3>
          <ApplicationTimeline :events="application.lichSu" />
        </GlassPanel>
      </div>
    </div>
  </div>
</template>
