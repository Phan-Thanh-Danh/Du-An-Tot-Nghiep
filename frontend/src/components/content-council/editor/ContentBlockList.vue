<script setup lang="ts">
import { inject, computed } from 'vue'
import ContentBlockCard from './ContentBlockCard.vue'

const editor = inject<any>('curriculumEditor')
const contents = computed(() => editor.selectedLesson.value?.contents || [])
</script>

<template>
  <div class="space-y-3">
    <ContentBlockCard 
      v-for="content in contents" 
      :key="content.id" 
      :content="content" 
    />
  </div>
</template>
