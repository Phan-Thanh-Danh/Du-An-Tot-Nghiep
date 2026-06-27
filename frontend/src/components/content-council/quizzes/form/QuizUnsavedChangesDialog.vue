<script setup lang="ts">
import { AlertTriangle } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits(['close', 'confirm'])
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    
    <div class="relative bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden animate-fade-in-up">
      <div class="p-6 text-center flex flex-col items-center">
        <div class="w-12 h-12 rounded-full bg-amber-100 text-amber-600 flex items-center justify-center mb-4">
          <AlertTriangle class="w-6 h-6" />
        </div>
        <h3 class="text-lg font-bold text-slate-800 mb-2">Bạn có thay đổi chưa lưu</h3>
        <p class="text-slate-600 text-sm">
          Các thay đổi trong Quiz sẽ bị mất nếu bạn rời khỏi trang. Bạn có chắc chắn muốn rời đi?
        </p>
      </div>
      
      <div class="p-4 border-t border-slate-100 bg-slate-50 flex gap-3">
        <button 
          @click="emit('close')"
          class="flex-1 px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
        >
          Ở lại trang
        </button>
        <button 
          @click="emit('confirm')"
          class="flex-1 px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors shadow-sm"
        >
          Rời đi
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in-up {
  animation: fadeInUp 0.2s ease-out forwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
