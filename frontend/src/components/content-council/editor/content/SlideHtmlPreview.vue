<script setup lang="ts">
import { computed } from 'vue'
import SafeHtmlRenderer from '@/components/common/SafeHtmlRenderer.vue'

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
  <div class="slide-html-preview prose prose-slate dark:prose-invert max-w-none">
    <template v-for="block in blocks" :key="block.id">

      <!-- Header -->
      <SafeHtmlRenderer
        v-if="block.type === 'header'"
        :tag="`h${block.data.level || 2}`"
        :html="block.data.text"
        class="text-(--text-heading) font-bold"
        :class="{
          'text-3xl font-extrabold mt-8 mb-4': block.data.level === 1,
          'text-2xl font-bold mt-6 mb-3': block.data.level === 2,
          'text-xl font-bold mt-5 mb-2.5': block.data.level === 3,
          'text-lg font-semibold mt-4 mb-2': block.data.level === 4,
          'text-base font-semibold mt-3 mb-1.5': block.data.level === 5,
          'text-sm font-semibold uppercase tracking-wider text-slate-500 mt-2 mb-1': block.data.level === 6
        }"
      />

      <!-- Paragraph -->
      <SafeHtmlRenderer
        v-else-if="block.type === 'paragraph'"
        tag="p"
        :html="block.data.text"
        class="my-3 text-(--text-body) leading-relaxed"
      />

      <!-- Code Block -->
      <div v-else-if="block.type === 'code'" class="my-5 rounded-xl overflow-hidden border border-slate-700/60 bg-slate-900 text-slate-100 p-4 font-mono text-sm shadow-md">
        <pre class="overflow-x-auto leading-relaxed whitespace-pre-wrap"><code>{{ block.data.code }}</code></pre>
      </div>

      <!-- Raw HTML Block -->
      <SafeHtmlRenderer
        v-else-if="block.type === 'raw'"
        :html="block.data.html"
        class="my-4"
      />

      <!-- List -->
      <component
        v-else-if="block.type === 'list'"
        :is="block.data.style === 'ordered' ? 'ol' : 'ul'"
        class="my-4 pl-6 space-y-2 text-(--text-body)"
        :class="block.data.style === 'ordered' ? 'list-decimal' : 'list-disc'"
      >
        <SafeHtmlRenderer v-for="(item, index) in block.data.items" :key="index" tag="li" :html="typeof item === 'string' ? item : item.content" />
      </component>

      <!-- Table -->
      <div v-else-if="block.type === 'table'" class="my-6 overflow-x-auto">
        <table class="min-w-full divide-y divide-slate-200 dark:divide-slate-700 border border-slate-200 dark:border-slate-700 rounded-lg">
          <tbody class="divide-y divide-slate-200 dark:divide-slate-700 bg-white dark:bg-slate-800">
            <tr
              v-for="(row, rowIndex) in block.data.content"
              :key="rowIndex"
              :class="block.data.withHeadings && rowIndex === 0 ? 'bg-slate-50 dark:bg-slate-900/60 font-semibold text-(--text-heading)' : 'text-(--text-body)'"
            >
              <td
                v-for="(cell, cellIndex) in row"
                :key="cellIndex"
                class="px-4 py-3 text-sm border-r border-slate-100 dark:border-slate-700 last:border-r-0"
              >
                <SafeHtmlRenderer :html="cell" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Checklist -->
      <div v-else-if="block.type === 'checklist'" class="my-4 space-y-2">
        <div v-for="(item, index) in block.data.items" :key="index" class="flex items-start gap-3">
          <input type="checkbox" :checked="item.checked" disabled class="mt-1">
          <SafeHtmlRenderer tag="span" :html="item.text" class="text-(--text-body)" :class="{ 'line-through text-slate-400': item.checked }" />
        </div>
      </div>

      <!-- Quote -->
      <blockquote v-else-if="block.type === 'quote'" class="my-6 border-l-4 border-blue-500 pl-4 italic text-(--text-body) bg-slate-50 dark:bg-slate-800/50 py-2.5 pr-4 rounded-r-lg">
        <SafeHtmlRenderer :html="block.data.text" />
        <SafeHtmlRenderer v-if="block.data.caption" tag="footer" class="text-sm text-slate-500 mt-2 font-medium" :html="block.data.caption" />
      </blockquote>

      <!-- Image (both image tool and simple-image) -->
      <figure v-else-if="block.type === 'image' || block.type === 'simpleImage'" class="my-6">
        <img :src="block.data.file?.url || block.data.url" :alt="block.data.caption" class="max-w-full rounded-lg" :class="{ 'border border-slate-200 dark:border-slate-700': block.data.withBorder, 'bg-slate-100 dark:bg-slate-800': block.data.withBackground, 'w-full': block.data.stretched }">
        <SafeHtmlRenderer v-if="block.data.caption" tag="figcaption" class="text-center text-sm text-slate-500 mt-2" :html="block.data.caption" />
      </figure>

      <!-- Embed -->
      <div v-else-if="block.type === 'embed'" class="my-6 aspect-video w-full overflow-hidden rounded-xl border border-slate-200 dark:border-slate-700">
        <iframe
          :src="block.data.embed"
          class="w-full h-full"
          frameborder="0"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
        ></iframe>
        <SafeHtmlRenderer v-if="block.data.caption" tag="div" class="text-center text-xs text-slate-500 mt-1" :html="block.data.caption" />
      </div>

      <!-- Delimiter -->
      <hr v-else-if="block.type === 'delimiter'" class="my-8 border-t-2 border-slate-200 dark:border-slate-700 w-16 mx-auto text-center" />

      <!-- Warning -->
      <div v-else-if="block.type === 'warning'" class="my-6 p-4 bg-amber-50 dark:bg-amber-950/30 border-l-4 border-amber-500 text-amber-900 dark:text-amber-200 rounded-r-lg">
        <SafeHtmlRenderer tag="h4" class="font-bold mb-1" :html="block.data.title" />
        <SafeHtmlRenderer :html="block.data.message" />
      </div>

      <!-- Unsupported Block Fallback -->
      <div v-else class="my-4 p-4 border border-dashed border-slate-300 dark:border-slate-700 text-slate-400 text-sm bg-slate-50 dark:bg-slate-900 rounded">
        Unsupported block type: {{ block.type }}
      </div>

    </template>
  </div>
</template>
