<script setup>
import { ref } from 'vue'
import { Video } from 'lucide-vue-next'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})

const hasError = ref(false)
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-4">
      <div class="p-2 bg-red-50 text-red-600 rounded-lg shrink-0">
        <Video class="w-5 h-5" />
      </div>
      <div>
        <h3 class="text-lg font-bold text-slate-800">{{ content.title }}</h3>
        <p v-if="content.description" class="text-sm text-slate-600 mt-1">{{ content.description }}</p>
      </div>
      <div v-if="content.status === 'draft'" class="ml-auto shrink-0">
        <span class="text-[10px] font-medium px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider">
          Bản nháp
        </span>
      </div>
    </div>

    <div class="rounded-xl overflow-hidden bg-slate-900 aspect-video relative">
      <div v-if="hasError || !content.videoUrl" class="absolute inset-0 flex flex-col items-center justify-center text-slate-400">
        <Video class="w-12 h-12 mb-3 opacity-50" />
        <p class="font-medium text-slate-300">Video chưa sẵn sàng hoặc đường dẫn không hợp lệ</p>
      </div>
      <video 
        v-else
        :src="content.videoUrl" 
        controls 
        controlsList="nodownload"
        class="w-full h-full object-contain"
        @error="hasError = true"
      >
        Trình duyệt của bạn không hỗ trợ thẻ video.
      </video>
    </div>
  </div>
</template>
