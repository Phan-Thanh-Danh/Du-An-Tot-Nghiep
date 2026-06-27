<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  jsonData: {
    type: String,
    default: '{}'
  }
})

const blocks = computed(() => {
  try {
    const data = JSON.parse(props.jsonData)
    return data.blocks || []
  } catch (e) {
    return []
  }
})
</script>

<template>
  <div class="slide-html-preview prose prose-slate max-w-none">
    <template v-for="block in blocks" :key="block.id">
      
      <!-- Header -->
      <component 
        v-if="block.type === 'header'" 
        :is="`h${block.data.level || 2}`"
        v-html="block.data.text"
        class="mt-6 mb-4 font-bold text-slate-800"
      ></component>

      <!-- Paragraph -->
      <p 
        v-else-if="block.type === 'paragraph'" 
        v-html="block.data.text"
        class="my-3 text-slate-600 leading-relaxed"
      ></p>

      <!-- List -->
      <component 
        v-else-if="block.type === 'list'" 
        :is="block.data.style === 'ordered' ? 'ol' : 'ul'"
        class="my-4 pl-6 space-y-2 text-slate-600"
        :class="block.data.style === 'ordered' ? 'list-decimal' : 'list-disc'"
      >
        <li v-for="(item, index) in block.data.items" :key="index" v-html="item"></li>
      </component>

      <!-- Checklist -->
      <div v-else-if="block.type === 'checklist'" class="my-4 space-y-2">
        <div v-for="(item, index) in block.data.items" :key="index" class="flex items-start gap-3">
          <input type="checkbox" :checked="item.checked" disabled class="mt-1">
          <span v-html="item.text" class="text-slate-600" :class="{ 'line-through text-slate-400': item.checked }"></span>
        </div>
      </div>

      <!-- Quote -->
      <blockquote v-else-if="block.type === 'quote'" class="my-6 border-l-4 border-blue-500 pl-4 italic text-slate-700 bg-slate-50 py-2 pr-4">
        <div v-html="block.data.text"></div>
        <footer v-if="block.data.caption" class="text-sm text-slate-500 mt-2 font-medium" v-html="block.data.caption"></footer>
      </blockquote>

      <!-- Image -->
      <figure v-else-if="block.type === 'image'" class="my-6">
        <img :src="block.data.file?.url" :alt="block.data.caption" class="max-w-full rounded-lg" :class="{ 'border border-slate-200': block.data.withBorder, 'bg-slate-100': block.data.withBackground, 'w-full': block.data.stretched }">
        <figcaption v-if="block.data.caption" class="text-center text-sm text-slate-500 mt-2" v-html="block.data.caption"></figcaption>
      </figure>

      <!-- Delimiter -->
      <hr v-else-if="block.type === 'delimiter'" class="my-8 border-t-2 border-slate-200 w-16 mx-auto text-center" />

      <!-- Warning -->
      <div v-else-if="block.type === 'warning'" class="my-6 p-4 bg-orange-50 border-l-4 border-orange-500 text-orange-800 rounded-r-lg">
        <h4 class="font-bold mb-1" v-html="block.data.title"></h4>
        <div v-html="block.data.message"></div>
      </div>

      <!-- Unsupported Block Fallback -->
      <div v-else class="my-4 p-4 border border-dashed border-slate-300 text-slate-400 text-sm bg-slate-50 rounded">
        Unsupported block type: {{ block.type }}
      </div>
      
    </template>
  </div>
</template>
