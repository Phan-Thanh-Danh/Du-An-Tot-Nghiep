<script setup lang="ts">
import { AlertTriangle, Lock } from 'lucide-vue-next'
import { QuizStatus } from '@/types/content-council/quiz'
import { useRouter } from 'vue-router'

const props = defineProps<{
  status: QuizStatus
}>()

const router = useRouter()
</script>

<template>
  <div v-if="status === 'open' || status === 'closed'" class="mb-6 p-4 rounded-xl border flex items-start gap-3" :class="status === 'open' ? 'bg-amber-50 border-amber-200 text-amber-800' : 'bg-slate-50 border-slate-200 text-slate-800'">
    <div class="mt-0.5">
      <AlertTriangle v-if="status === 'open'" class="w-5 h-5 text-amber-600" />
      <Lock v-else class="w-5 h-5 text-slate-500" />
    </div>
    <div class="flex-1">
      <h3 class="font-bold mb-1" :class="status === 'open' ? 'text-amber-800' : 'text-slate-800'">
        {{ status === 'open' ? 'Quiz đang mở' : 'Quiz đã đóng' }}
      </h3>
      <p class="text-sm" :class="status === 'open' ? 'text-amber-700' : 'text-slate-600'">
        {{ status === 'open' ? 'Bạn cần đóng Quiz trước khi chỉnh sửa cấu hình.' : 'Hãy chuyển Quiz về bản nháp tại trang danh sách trước khi chỉnh sửa.' }}
      </p>
      <button 
        @click="router.push({ name: 'content-council-quizzes' })"
        class="mt-3 text-sm font-medium hover:underline flex items-center gap-1"
        :class="status === 'open' ? 'text-amber-700' : 'text-blue-600'"
      >
        Quay lại danh sách Quiz
      </button>
    </div>
  </div>
</template>
