<script setup>
import { ref } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import { Send, AlertTriangle } from 'lucide-vue-next'

const emit = defineEmits(['send'])

const type = ref('TUITION')
const form = ref({
  maHocKy: '',
  ghiChu: ''
})
const loading = ref(false)

const handleSend = () => {
  emit('send', { type: type.value, payload: { ...form.value } })
}
</script>

<template>
  <GlassPanel variant="strong">
    <template #header>
      <h2 class="text-lg font-semibold flex items-center gap-2">
        <AlertTriangle class="w-5 h-5 text-[var(--color-warning-text)]" />
        Phát hành thông báo chuyên biệt
      </h2>
    </template>

    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium mb-1">Loại thông báo</label>
        <select v-model="type" class="w-full h-10 px-3 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)]">
          <option value="TUITION">Nhắc học phí</option>
          <option value="ACADEMIC">Cảnh báo học vụ</option>
          <option value="URGENT">Khẩn cấp toàn trường</option>
        </select>
      </div>

      <div v-if="type === 'TUITION'">
        <label class="block text-sm font-medium mb-1">Mã học kỳ</label>
        <GlassInput v-model="form.maHocKy" type="number" placeholder="Ví dụ: 20261" />
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Ghi chú bổ sung</label>
        <textarea v-model="form.ghiChu" class="w-full p-2 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)]"></textarea>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-end mt-4">
        <GlassButton variant="warning" @click="handleSend" :loading="loading">
          <template #leading><Send class="w-4 h-4" /></template>
          Phát hành
        </GlassButton>
      </div>
    </template>
  </GlassPanel>
</template>
