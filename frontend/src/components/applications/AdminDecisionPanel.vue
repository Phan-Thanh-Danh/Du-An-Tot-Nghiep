<script setup>
import { ref, computed } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { CheckCircle2, XCircle, AlertCircle } from 'lucide-vue-next'

const props = defineProps({
  application: { type: Object, required: true }
})
const emit = defineEmits(['approve', 'reject', 'request-supplement'])

const note = ref('')
const isProcessing = ref(false)
const formError = ref('')

const canDecide = computed(() => ['DANG_XEM_XET', 'DA_NOP'].includes(props.application.trangThai))

const handleAction = (type) => {
  formError.value = ''
  if (type === 'reject' || type === 'supplement') {
    if (!note.value.trim()) {
      formError.value = 'Vui lòng nhập lý do hoặc ghi chú phản hồi.'
      return
    }
  }
  
  isProcessing.value = true
  if (type === 'approve') emit('approve', { ghiChu: note.value })
  else if (type === 'reject') emit('reject', { lyDo: note.value })
  else emit('request-supplement', { noiDung: note.value })
  
  setTimeout(() => { isProcessing.value = false }, 1000)
}
</script>

<template>
  <GlassPanel padding="normal" class="space-y-4">
    <h3 class="font-semibold text-(--text-heading) flex items-center gap-2">
      <CheckCircle2 class="w-4 h-4" /> Quyết định
    </h3>

    <div v-if="!canDecide" class="p-3 bg-(--surface-hover) rounded-lg text-sm text-(--text-muted) text-center italic">
      Không thể thay đổi quyết định với trạng thái hiện tại.
    </div>

    <div v-else class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-(--text-heading) mb-1">Ghi chú / Lý do phản hồi</label>
        <textarea 
          v-model="note"
          rows="3"
          class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg focus:border-(--lg-primary) outline-none text-(--text-body) resize-none"
          placeholder="Nhập ghi chú cho sinh viên..."
        ></textarea>
        <p v-if="formError" class="mt-1 text-xs font-semibold text-(--color-danger-text)">
          {{ formError }}
        </p>
      </div>

      <div class="grid grid-cols-2 gap-2">
        <GlassButton variant="secondary" class="col-span-2" :loading="isProcessing" @click="handleAction('supplement')">
          <template #leading><AlertCircle class="w-4 h-4" /></template> Yêu cầu bổ sung
        </GlassButton>
        <GlassButton variant="danger" :loading="isProcessing" @click="handleAction('reject')">
          <template #leading><XCircle class="w-4 h-4" /></template> Từ chối
        </GlassButton>
        <GlassButton variant="success" :loading="isProcessing" @click="handleAction('approve')">
          <template #leading><CheckCircle2 class="w-4 h-4" /></template> Duyệt đơn
        </GlassButton>
      </div>
      <p class="text-[10px] text-(--color-warning-text) mt-1">Duyệt đơn có thể tự động chuyển trạng thái sang xử lý nghiệp vụ.</p>
    </div>
  </GlassPanel>
</template>
