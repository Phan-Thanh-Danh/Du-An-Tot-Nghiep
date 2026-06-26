<script setup>
import { File, ExternalLink, Download } from 'lucide-vue-next'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})

const isPdf = (url) => {
  return url && url.toLowerCase().endsWith('.pdf')
}
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-6">
      <div class="p-2 bg-blue-50 text-blue-600 rounded-lg shrink-0">
        <File class="w-5 h-5" />
      </div>
      <div class="flex-1 min-w-0">
        <h3 class="text-lg font-bold text-slate-800">{{ content.title }}</h3>
        <p v-if="content.description" class="text-sm text-slate-600 mt-1">{{ content.description }}</p>
      </div>
      <div v-if="content.status === 'draft'" class="shrink-0">
        <span class="text-[10px] font-medium px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider">
          Bản nháp
        </span>
      </div>
    </div>

    <!-- PDF Viewer -->
    <div v-if="isPdf(content.documentUrl)" class="bg-slate-100 rounded-xl overflow-hidden border border-slate-200 aspect-[1/1.4] max-h-[800px] w-full relative">
      <iframe 
        :src="content.documentUrl" 
        class="w-full h-full border-0"
        title="Trình xem tài liệu PDF"
      ></iframe>
    </div>

    <!-- Fallback / Non-PDF Document -->
    <div v-else class="flex items-center justify-between p-4 bg-slate-50 border border-slate-200 rounded-xl">
      <div class="flex items-center gap-3">
        <div class="p-3 bg-white border border-slate-200 rounded-lg">
          <File class="w-6 h-6 text-blue-500" />
        </div>
        <div>
          <h4 class="font-semibold text-slate-800">{{ content.title || 'Tài liệu chưa có tiêu đề' }}</h4>
          <p class="text-xs text-slate-500 mt-0.5">Vui lòng tải xuống hoặc mở trong tab mới để xem chi tiết</p>
        </div>
      </div>
      
      <a 
        v-if="content.documentUrl"
        :href="content.documentUrl" 
        target="_blank" 
        rel="noopener noreferrer"
        class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-blue-600 bg-white border border-blue-200 hover:bg-blue-50 hover:border-blue-300 rounded-lg transition-colors shadow-sm"
      >
        <ExternalLink class="w-4 h-4" />
        <span class="hidden sm:inline">Mở tài liệu</span>
      </a>
      <span v-else class="text-sm text-slate-400 italic">
        Chưa có file
      </span>
    </div>
  </div>
</template>
