<script setup lang="ts">
import { ref, computed } from 'vue'
import { X, Check } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
  questionCount: number
  missingScore: number
}>()

const emit = defineEmits(['close', 'confirm'])

const strategy = ref<'fixed' | 'divide'>('fixed')
const fixedScore = ref(1)

const divideScore = computed(() => {
  if (props.questionCount === 0 || props.missingScore <= 0) return 0
  return Number((props.missingScore / props.questionCount).toFixed(2))
})

const handleConfirm = () => {
  let score = 1
  if (strategy.value === 'fixed') {
    score = fixedScore.value
  } else if (strategy.value === 'divide') {
    score = divideScore.value
  }
  
  if (score <= 0) score = 1
  emit('confirm', score)
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    
    <div class="relative bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden animate-fade-in-up flex flex-col max-h-[90vh]">
      <!-- Header -->
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between bg-slate-50 shrink-0">
        <h3 class="text-lg font-bold text-slate-800">Thêm {{ questionCount }} câu hỏi vào Quiz</h3>
        <button @click="emit('close')" class="text-slate-400 hover:text-slate-600 p-1 rounded-lg hover:bg-slate-200 transition-colors">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Body -->
      <div class="p-6 overflow-y-auto">
        <p class="text-sm text-slate-600 mb-4">Bạn vui lòng chọn cách phân bổ điểm cho các câu hỏi này:</p>
        
        <div class="space-y-4">
          <!-- Option 1: Fixed score -->
          <label class="flex items-start gap-3 p-4 rounded-xl border cursor-pointer transition-colors" :class="strategy === 'fixed' ? 'border-blue-500 bg-blue-50' : 'border-slate-200 hover:border-slate-300'">
            <div class="flex items-center h-5">
              <input type="radio" v-model="strategy" value="fixed" class="w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500">
            </div>
            <div class="flex-1">
              <span class="block text-sm font-bold text-slate-800">Mỗi câu cùng số điểm</span>
              <div v-if="strategy === 'fixed'" class="mt-3 flex items-center gap-2">
                <input 
                  type="number" 
                  v-model="fixedScore" 
                  min="0.1" 
                  step="0.1"
                  class="w-24 border border-slate-300 rounded-lg px-3 py-1.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none bg-white"
                >
                <span class="text-sm text-slate-600">điểm / câu</span>
              </div>
            </div>
          </label>

          <!-- Option 2: Divide remaining score -->
          <label class="flex items-start gap-3 p-4 rounded-xl border cursor-pointer transition-colors" :class="strategy === 'divide' ? 'border-blue-500 bg-blue-50' : 'border-slate-200 hover:border-slate-300'">
            <div class="flex items-center h-5">
              <input type="radio" v-model="strategy" value="divide" :disabled="missingScore <= 0" class="w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-500 disabled:opacity-50">
            </div>
            <div class="flex-1" :class="{'opacity-50': missingScore <= 0}">
              <span class="block text-sm font-bold text-slate-800">Chia đều theo số điểm còn lại</span>
              <p class="text-xs text-slate-500 mt-1">
                <template v-if="missingScore > 0">
                  Quiz còn thiếu {{ missingScore }} điểm. Mỗi câu sẽ được cộng <span class="font-bold text-slate-700">{{ divideScore }} điểm</span>.
                </template>
                <template v-else>
                  Quiz đã đạt đủ điểm cấu hình.
                </template>
              </p>
            </div>
          </label>
        </div>
      </div>
      
      <!-- Footer -->
      <div class="p-4 border-t border-slate-100 bg-slate-50 flex justify-end gap-3 shrink-0">
        <button 
          @click="emit('close')"
          class="px-5 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
        >
          Hủy
        </button>
        <button 
          @click="handleConfirm"
          class="px-5 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors shadow-sm flex items-center gap-2"
        >
          <Check class="w-4 h-4" />
          Thêm {{ questionCount }} câu
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
