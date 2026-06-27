<script setup>
import { PlaySquare } from 'lucide-vue-next'
import SlideHtmlPreview from '../editor/content/SlideHtmlPreview.vue'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-6">
      <div class="p-2 bg-orange-50 text-orange-600 rounded-lg shrink-0">
        <PlaySquare class="w-5 h-5" />
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

    <div class="bg-white border border-slate-200 rounded-xl p-6 sm:p-8">
      <SlideHtmlPreview :jsonData="content.content" />
    </div>
  </div>
</template>
