<script setup>
import { computed } from 'vue'
import { FileText } from 'lucide-vue-next'
import { sanitizeHtml } from '@/utils/htmlSanitizer'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})

const safeHtml = computed(() => {
  return sanitizeHtml(props.content.content || '')
})
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-6">
      <div class="p-2 bg-slate-100 text-slate-600 rounded-lg shrink-0">
        <FileText class="w-5 h-5" />
      </div>
      <div>
        <h3 class="text-lg font-bold text-slate-800">{{ content.title }}</h3>
      </div>
      <div v-if="content.status === 'draft'" class="ml-auto shrink-0">
        <span class="text-[10px] font-medium px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider">
          Bản nháp
        </span>
      </div>
    </div>

    <div class="bg-white border border-slate-200 rounded-xl p-6 sm:p-8">
      <div 
        v-if="safeHtml"
        class="prose prose-slate max-w-none text-slate-700 leading-relaxed"
        v-html="safeHtml"
      ></div>
      <div v-else class="text-slate-400 italic text-center py-8">
        Văn bản chưa có nội dung
      </div>
    </div>
  </div>
</template>
