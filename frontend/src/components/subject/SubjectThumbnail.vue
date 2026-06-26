<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  name: string
}>()

const gradients = [
  'from-blue-500 to-cyan-400',
  'from-violet-500 to-purple-400',
  'from-emerald-500 to-teal-400',
  'from-orange-500 to-amber-400',
  'from-pink-500 to-rose-400',
  'from-indigo-500 to-blue-400',
]

const init = computed(() => {
  const words = props.name.split(' ')
  if (words.length === 1) return words[0].slice(0, 2).toUpperCase()
  return (words[0][0] + words[words.length - 1][0]).toUpperCase()
})

const gradIdx = computed(() => {
  let hash = 0
  for (let i = 0; i < props.name.length; i++) {
    hash = props.name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return Math.abs(hash) % gradients.length
})
</script>

<template>
  <div class="relative h-32 bg-slate-100 dark:bg-slate-700 overflow-hidden group-hover:scale-105 transition-transform duration-500">
    <div
      :class="['absolute inset-0 bg-gradient-to-br', gradients[gradIdx]]"
    >
      <div class="absolute inset-0 bg-black/10" />
    </div>
    <div class="absolute inset-0 flex items-center justify-center">
      <span class="text-3xl font-bold text-white/90 tracking-wider select-none">
        {{ init }}
      </span>
    </div>
  </div>
</template>
