<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ChevronLeft, PenTool } from 'lucide-vue-next'

const router = useRouter()

const goBack = () => {
  router.back()
}
</script>

<template>
  <div class="h-full flex flex-col p-6 max-w-4xl mx-auto w-full items-center justify-center">
    <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-12 w-full text-center flex flex-col items-center gap-6">
      <div class="w-16 h-16 bg-blue-50 text-blue-600 rounded-full flex items-center justify-center">
        <PenTool class="w-8 h-8" />
      </div>
      
      <div>
        <h1 class="text-2xl font-bold text-slate-800 mb-2">Quiz Builder</h1>
        <p class="text-slate-600 max-w-md mx-auto">
          Chức năng chọn, chấm điểm và sắp xếp câu hỏi sẽ được triển khai ở bước tiếp theo.
        </p>
      </div>
      
      <button 
        @click="goBack"
        class="mt-4 flex items-center gap-2 px-5 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition-colors"
      >
        <ChevronLeft class="w-4 h-4" />
        Quay lại
      </button>
    </div>
  </div>
</template>
