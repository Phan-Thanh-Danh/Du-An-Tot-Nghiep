<script setup lang="ts">
import { AlertTriangle, Lock } from 'lucide-vue-next'
import { QuizStatus } from '@/types/content-council/quiz'

defineProps<{
  status: QuizStatus
}>()
</script>

<template>
  <div v-if="status === 'open' || status === 'closed'" class="mx-6 mt-6 p-4 rounded-xl border flex items-start gap-3" :class="status === 'open' ? 'bg-amber-50 border-amber-200 text-amber-800' : 'bg-slate-50 border-slate-200 text-slate-800'">
    <div class="mt-0.5">
      <AlertTriangle v-if="status === 'open'" class="w-5 h-5 text-amber-600" />
      <Lock v-else class="w-5 h-5 text-slate-500" />
    </div>
    <div class="flex-1">
      <h3 class="font-bold mb-1" :class="status === 'open' ? 'text-amber-800' : 'text-slate-800'">
        {{ status === 'open' ? 'Quiz đang mở và không thể chỉnh sửa cấu trúc' : 'Quiz đã đóng' }}
      </h3>
      <p class="text-sm" :class="status === 'open' ? 'text-amber-700' : 'text-slate-600'">
        {{ status === 'open' ? 'Bạn không thể thêm, sửa điểm, xóa hay sắp xếp lại các câu hỏi khi Quiz đang ở trạng thái mở.' : 'Hãy chuyển Quiz về bản nháp tại màn hình cấu hình trước khi chỉnh sửa.' }}
      </p>
    </div>
  </div>
</template>
