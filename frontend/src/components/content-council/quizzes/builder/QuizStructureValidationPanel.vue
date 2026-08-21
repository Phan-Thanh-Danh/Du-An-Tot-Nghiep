<script setup lang="ts">
import { computed } from 'vue'
import { CheckCircle2, AlertTriangle, AlertCircle, Send } from 'lucide-vue-next'

const props = defineProps<{
  canSave: boolean
  canPublish: boolean
  errors: string[]
  warnings: string[]
}>()

const emit = defineEmits(['publish'])

const isPerfect = computed(() => props.canPublish && props.warnings.length === 0)
const hasErrors = computed(() => props.errors.length > 0)
const hasWarnings = computed(() => props.warnings.length > 0)

</script>

<template>
  <div class="mb-4">
    <!-- Perfect State -->
    <div v-if="isPerfect" class="flex items-center justify-between gap-3 p-3.5 bg-green-50 border border-green-200 rounded-xl text-green-800 shadow-sm">
      <div class="flex items-center gap-3">
        <CheckCircle2 class="w-5 h-5 shrink-0 text-green-600" />
        <span class="text-sm font-medium">Cấu trúc Quiz hợp lệ và đã sẵn sàng để xuất bản.</span>
      </div>
      <button 
        @click="emit('publish')" 
        class="px-3.5 py-1.5 bg-green-600 hover:bg-green-700 text-white font-semibold text-xs rounded-lg transition-colors shadow-sm flex items-center gap-1.5 shrink-0"
      >
        <Send class="w-3.5 h-3.5" />
        Xuất bản ngay
      </button>
    </div>

    <!-- Has Errors or Warnings -->
    <div v-else class="flex flex-col gap-2 p-4 rounded-xl border" :class="hasErrors ? 'bg-red-50 border-red-200' : 'bg-amber-50 border-amber-200'">
      <div class="flex items-center gap-2 mb-1">
        <AlertCircle v-if="hasErrors" class="w-5 h-5 text-red-600" />
        <AlertTriangle v-else class="w-5 h-5 text-amber-600" />
        <h3 class="font-bold text-sm" :class="hasErrors ? 'text-red-800' : 'text-amber-800'">
          {{ hasErrors ? 'Chưa thể xuất bản Quiz' : 'Cấu trúc Quiz có điểm cần lưu ý' }}
        </h3>
      </div>
      
      <ul class="space-y-1.5 ml-7 text-sm">
        <li v-for="(err, i) in errors" :key="'e'+i" class="text-red-700 list-disc list-outside">
          {{ err }}
        </li>
        <li v-for="(warn, i) in warnings" :key="'w'+i" class="text-amber-700 list-disc list-outside">
          {{ warn }}
        </li>
      </ul>
    </div>
  </div>
</template>
