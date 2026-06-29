<script setup>
import { ref } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { ArrowLeft, Send } from 'lucide-vue-next'

defineProps({
  record: { type: Object, required: true }
})
const emit = defineEmits(['back', 'submit'])
const lyDo = ref('')
const loading = ref(false)

const handleSubmit = () => {
  if (!lyDo.value.trim()) {
    alert('Vui lòng nhập lý do')
    return
  }
  loading.value = true
  setTimeout(() => {
    emit('submit', { lyDo: lyDo.value })
    loading.value = false
  }, 500)
}
</script>

<template>
  <div class="space-y-6 max-w-2xl mx-auto">
    <div class="flex items-center gap-4">
      <button @click="emit('back')" class="p-2 hover:bg-[var(--surface-hover)] rounded-full text-[var(--text-muted)]">
        <ArrowLeft class="w-5 h-5" />
      </button>
      <h2 class="text-xl font-bold text-[var(--text-heading)]">Gửi khiếu nại kỷ luật</h2>
    </div>

    <GlassPanel padding="normal" class="space-y-4">
      <div class="p-3 bg-[var(--surface-hover)] rounded-lg text-sm border-l-4 border-l-[var(--color-warning-border)]">
        Đang khiếu nại quyết định: <strong>{{ record.tieuDe }}</strong>
      </div>
      
      <div>
        <label class="block text-sm font-medium text-[var(--text-heading)] mb-1">Lý do khiếu nại / Kháng nghị</label>
        <textarea 
          v-model="lyDo"
          rows="5"
          class="w-full px-3 py-2 bg-[var(--surface-input)] border border-[var(--border-input)] rounded-lg focus:border-[var(--lg-primary)] outline-none text-[var(--text-body)] resize-none"
          placeholder="Trình bày rõ lý do..."
        ></textarea>
      </div>

      <div>
        <label class="block text-sm font-medium text-[var(--text-heading)] mb-1">Minh chứng đính kèm (Mock)</label>
        <div class="border-2 border-dashed border-[var(--border-card)] rounded-lg p-6 text-center text-[var(--text-muted)]">
          [Chức năng Upload Mock]
        </div>
      </div>

      <div class="flex justify-end gap-3 pt-4 border-t border-[var(--border-card)]">
        <GlassButton variant="ghost" @click="emit('back')">Hủy</GlassButton>
        <GlassButton variant="primary" :loading="loading" @click="handleSubmit">
          <template #leading><Send class="w-4 h-4" /></template> Gửi khiếu nại
        </GlassButton>
      </div>
    </GlassPanel>
  </div>
</template>
