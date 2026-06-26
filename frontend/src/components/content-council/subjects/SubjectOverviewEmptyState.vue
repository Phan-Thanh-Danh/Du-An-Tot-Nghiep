<script setup>
import { FolderOpen } from 'lucide-vue-next'
</script>

<template>
  <div class="flex flex-col items-center justify-center py-16 px-4 text-center">
    <div class="w-16 h-16 lg-glass-soft rounded-full flex items-center justify-center mb-4 border border-card">
      <FolderOpen class="w-8 h-8 text-placeholder" />
    </div>
    <h3 class="text-lg font-bold text-heading mb-2">Chưa có nội dung môn học</h3>
    <p class="text-body max-w-sm mb-6">
      Môn học này chưa có chương hoặc bài học nào. Bạn có thể bắt đầu xây dựng cấu trúc ngay bây giờ.
    </p>
    <button 
      class="px-4 py-2 surface-card text-placeholder font-medium rounded-lg border border-card cursor-not-allowed transition-colors"
      title="Trình soạn nội dung sẽ được triển khai ở bước tiếp theo"
      disabled
    >
      Bắt đầu soạn nội dung
    </button>
  </div>
</template>
