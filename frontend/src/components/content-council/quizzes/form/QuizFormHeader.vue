<script setup lang="ts">
import { QuizFormMode } from '@/types/content-council/quizForm'

defineProps<{
  mode: QuizFormMode
}>()
</script>

<template>
  <div class="mb-6">
    <h1 class="text-2xl font-bold text-slate-800 tracking-tight">
      {{ mode === 'create' ? 'Tạo Quiz / Đề kiểm tra' : 'Chỉnh sửa Quiz / Đề kiểm tra' }}
    </h1>
    <p class="text-slate-500 mt-1">
      {{ mode === 'create' ? 'Thiết lập thông tin và quy tắc làm bài trước khi thêm câu hỏi.' : 'Cập nhật thông tin và cấu hình hoạt động của Quiz.' }}
    </p>
  </div>
</template>
