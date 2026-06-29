<script setup>
import { ref, onMounted } from 'vue'
import { notificationsApi } from '@/services/notificationsApi'
import GlassPanel from '@/components/ui/GlassPanel.vue'

import { FileText } from 'lucide-vue-next'

const emit = defineEmits(['select'])
const templates = ref([])
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    const data = await notificationsApi.getNotificationTemplates({ pageSize: 100 })
    templates.value = data.items || []
  } catch (err) {
    console.error('Failed to load templates', err)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <GlassPanel variant="soft" padding="compact" class="h-64 flex flex-col">
    <div class="font-medium text-[var(--text-heading)] mb-2 flex items-center gap-2">
      <FileText class="w-4 h-4" /> Chọn Template
    </div>
    <div v-if="loading" class="flex-1 flex justify-center items-center">
      <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-[var(--lg-primary)]"></div>
    </div>
    <div v-else class="flex-1 overflow-y-auto space-y-2 custom-scrollbar pr-1">
      <div
        v-for="tpl in templates"
        :key="tpl.maMauThongBao"
        class="p-2 border border-[var(--border-card)] rounded-lg hover:border-[var(--lg-primary)] cursor-pointer transition-colors bg-[var(--surface-card)]"
        @click="emit('select', tpl)"
      >
        <div class="text-sm font-semibold truncate">{{ tpl.tenMau }}</div>
        <div class="text-xs text-[var(--text-muted)] line-clamp-1 mt-1">{{ tpl.moTa }}</div>
      </div>
      <div v-if="templates.length === 0" class="text-center text-sm text-[var(--text-muted)] py-4">
        Chưa có template nào
      </div>
    </div>
  </GlassPanel>
</template>
