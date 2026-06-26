<script setup lang="ts">
import { AlertCircle, CheckCircle2, X } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
  canPublish: boolean
  errors: string[]
  quizConfig: {
    questionCount: number
    totalScore: number
    durationMinutes: number
    passingScore: number | null
    minimumCorrectAnswers: number | null
    passMethod: 'score' | 'correct_answer_count'
  }
}>()

const emit = defineEmits(['close', 'confirm'])
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    
    <div class="relative bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden animate-fade-in-up">
      <!-- Error State -->
      <template v-if="!canPublish">
        <div class="px-6 py-5 border-b border-slate-100 flex items-start gap-4 bg-red-50/50">
          <div class="w-10 h-10 rounded-full bg-red-100 text-red-600 flex items-center justify-center shrink-0">
            <AlertCircle class="w-5 h-5" />
          </div>
          <div class="flex-1 min-w-0 pt-1">
            <h3 class="text-lg font-bold text-slate-800 mb-1">Chưa thể xuất bản Quiz</h3>
            <p class="text-sm text-slate-600">Bạn cần khắc phục các lỗi sau trước khi xuất bản:</p>
          </div>
          <button @click="emit('close')" class="text-slate-400 hover:text-slate-600 p-1 rounded-lg hover:bg-slate-100 transition-colors">
            <X class="w-5 h-5" />
          </button>
        </div>
        
        <div class="p-6 bg-slate-50">
          <ul class="space-y-3">
            <li v-for="(err, idx) in errors" :key="idx" class="flex items-start gap-2.5 text-sm text-red-700 bg-red-50 p-3 rounded-lg border border-red-100">
              <span class="w-1.5 h-1.5 rounded-full bg-red-500 mt-1.5 shrink-0"></span>
              <span class="leading-relaxed">{{ err }}</span>
            </li>
          </ul>
        </div>
        
        <div class="p-4 border-t border-slate-100 flex justify-end">
          <button 
            @click="emit('close')"
            class="px-5 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
          >
            Đóng
          </button>
        </div>
      </template>

      <!-- Ready State -->
      <template v-else>
        <div class="px-6 py-5 border-b border-slate-100 flex items-start gap-4">
          <div class="w-10 h-10 rounded-full bg-green-100 text-green-600 flex items-center justify-center shrink-0">
            <CheckCircle2 class="w-5 h-5" />
          </div>
          <div class="flex-1 min-w-0 pt-1">
            <h3 class="text-lg font-bold text-slate-800 mb-1">Xuất bản Quiz?</h3>
            <p class="text-sm text-slate-600">Quiz đã đáp ứng đủ các điều kiện.</p>
          </div>
          <button @click="emit('close')" class="text-slate-400 hover:text-slate-600 p-1 rounded-lg hover:bg-slate-100 transition-colors">
            <X class="w-5 h-5" />
          </button>
        </div>
        
        <div class="p-6 bg-slate-50">
          <p class="text-sm text-slate-600 mb-4">Sau khi xuất bản, Quiz sẽ sẵn sàng được mở hoặc gắn vào nội dung bài học. Thông số hiện tại:</p>
          
          <div class="bg-white rounded-lg border border-slate-200 p-4 space-y-2 text-sm">
            <div class="flex justify-between">
              <span class="text-slate-500">Số lượng câu hỏi:</span>
              <span class="font-bold text-slate-800">{{ quizConfig.questionCount }} câu</span>
            </div>
            <div class="flex justify-between">
              <span class="text-slate-500">Tổng điểm:</span>
              <span class="font-bold text-slate-800">{{ quizConfig.totalScore }} điểm</span>
            </div>
            <div class="flex justify-between">
              <span class="text-slate-500">Thời gian làm bài:</span>
              <span class="font-bold text-slate-800">{{ quizConfig.durationMinutes > 0 ? `${quizConfig.durationMinutes} phút` : 'Không giới hạn' }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-slate-500">Điều kiện đạt:</span>
              <span class="font-bold text-slate-800">
                <template v-if="quizConfig.passMethod === 'score'">
                  Từ {{ quizConfig.passingScore || 0 }} điểm
                </template>
                <template v-else>
                  Từ {{ quizConfig.minimumCorrectAnswers || 0 }} câu đúng
                </template>
              </span>
            </div>
          </div>
        </div>
        
        <div class="p-4 border-t border-slate-100 flex justify-end gap-3">
          <button 
            @click="emit('close')"
            class="px-5 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
          >
            Hủy
          </button>
          <button 
            @click="emit('confirm')"
            class="px-5 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
          >
            Xuất bản
          </button>
        </div>
      </template>

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
