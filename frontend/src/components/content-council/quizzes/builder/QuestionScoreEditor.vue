<script setup lang="ts">
import { ref, onMounted } from 'vue'

const props = defineProps<{
  initialScore: number
}>()

const emit = defineEmits(['save', 'cancel'])

const scoreInput = ref(props.initialScore)
const inputRef = ref<HTMLInputElement | null>(null)
const error = ref('')

onMounted(() => {
  if (inputRef.value) {
    inputRef.value.focus()
    inputRef.value.select()
  }
})

const handleSave = () => {
  if (!scoreInput.value || scoreInput.value <= 0) {
    error.value = 'Điểm số phải lớn hơn 0'
    return
  }
  if (scoreInput.value > 100) {
    error.value = 'Điểm số quá lớn'
    return
  }
  emit('save', Number(scoreInput.value))
}
</script>

<template>
  <div>
    <div class="flex items-center gap-2">
      <label class="text-sm font-medium text-slate-700">Điểm:</label>
      <input 
        ref="inputRef"
        type="number" 
        v-model="scoreInput"
        min="0.1" 
        max="100" 
        step="0.1"
        @keyup.enter="handleSave"
        class="w-24 border rounded-md px-2 py-1 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
        :class="error ? 'border-red-300' : 'border-slate-300'"
      />
      <button 
        @click="emit('cancel')" 
        class="px-2.5 py-1 text-xs font-medium text-slate-600 hover:bg-slate-200 bg-slate-100 rounded transition-colors"
      >
        Hủy
      </button>
      <button 
        @click="handleSave" 
        class="px-2.5 py-1 text-xs font-medium text-white hover:bg-blue-700 bg-blue-600 rounded shadow-sm transition-colors"
      >
        Lưu
      </button>
    </div>
    <p v-if="error" class="text-xs text-red-600 mt-1.5">{{ error }}</p>
  </div>
</template>
