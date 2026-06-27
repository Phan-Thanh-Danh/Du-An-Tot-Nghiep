<script setup lang="ts">
import { AlertCircle, ArrowLeft } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="h-screen flex flex-col bg-slate-50 items-center justify-center p-6">
    <div class="max-w-md w-full bg-white p-8 rounded-2xl shadow-sm border border-slate-200 text-center">
      <div class="w-16 h-16 bg-red-50 text-red-500 rounded-full flex items-center justify-center mx-auto mb-6">
        <AlertCircle class="w-8 h-8" />
      </div>
      <h2 class="text-xl font-bold text-slate-800 mb-2">Không tìm thấy Quiz</h2>
      <p class="text-slate-600 mb-8">
        Quiz bạn đang tìm không tồn tại hoặc đã bị xóa khỏi hệ thống.
      </p>
      <button 
        @click="router.push({ name: 'content-council-quizzes' })"
        class="inline-flex items-center gap-2 px-5 py-2.5 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors"
      >
        <ArrowLeft class="w-4 h-4" />
        Quay lại danh sách Quiz
      </button>
    </div>
  </div>
</template>
