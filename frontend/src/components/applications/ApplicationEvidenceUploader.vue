<script setup>
import { ref } from 'vue'
import { Upload, X, File } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const props = defineProps({
  files: { type: Array, default: () => [] },
  disabled: { type: Boolean, default: false }
})

const emit = defineEmits(['upload', 'remove'])

const isDragging = ref(false)

const handleDrop = (e) => {
  if (props.disabled) return
  isDragging.value = false
  const droppedFiles = Array.from(e.dataTransfer.files)
  droppedFiles.forEach(f => emit('upload', f))
}

const handleFileSelect = (e) => {
  if (props.disabled) return
  const selected = Array.from(e.target.files)
  selected.forEach(f => emit('upload', f))
  e.target.value = ''
}
</script>

<template>
  <div class="space-y-4">
    <div 
      v-if="!disabled"
      class="border-2 border-dashed rounded-lg p-6 flex flex-col items-center justify-center transition-colors cursor-pointer"
      :class="isDragging ? 'border-(--lg-primary) bg-(--lg-primary) bg-opacity-10' : 'border-(--border-card) bg-(--surface-card) hover:border-(--lg-primary)'"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="handleDrop"
      @click="$refs.fileInput.click()"
    >
      <input type="file" ref="fileInput" class="hidden" @change="handleFileSelect" multiple />
      <div class="w-12 h-12 rounded-full bg-(--surface-hover) flex items-center justify-center mb-3">
        <Upload class="w-6 h-6 text-(--lg-primary)" />
      </div>
      <p class="text-(--text-heading) font-medium">Nhấn hoặc kéo thả file vào đây</p>
      <p class="text-sm text-(--text-muted) mt-1">Hỗ trợ PDF, JPG, PNG (tối đa 5MB)</p>
    </div>

    <div v-if="files.length > 0" class="space-y-2">
      <GlassPanel v-for="(file, idx) in files" :key="idx" padding="compact" class="flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="p-2 rounded bg-(--surface-hover)">
            <File class="w-4 h-4 text-(--lg-primary)" />
          </div>
          <div>
            <div class="text-sm font-medium text-(--text-body) line-clamp-1">{{ file.name || file.tenFile || 'Tài liệu minh chứng' }}</div>
            <div class="text-xs text-(--text-muted)" v-if="file.size">{{ (file.size / 1024 / 1024).toFixed(2) }} MB</div>
          </div>
        </div>
        <button 
          v-if="!disabled && file.id"
          @click.stop="emit('remove', file)" 
          class="p-2 hover:bg-(--surface-hover) rounded-full text-(--text-muted) hover:text-(--color-danger-text) transition-colors"
        >
          <X class="w-4 h-4" />
        </button>
      </GlassPanel>
    </div>
  </div>
</template>
