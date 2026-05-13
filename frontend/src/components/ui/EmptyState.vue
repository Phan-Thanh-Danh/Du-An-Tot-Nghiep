<script setup>
import { Inbox } from 'lucide-vue-next'

defineProps({
  title: {
    type: String,
    default: 'Chưa có dữ liệu',
  },
  description: {
    type: String,
    default: 'Nội dung sẽ hiển thị khi có dữ liệu học vụ phù hợp.',
  },
})
</script>

<template>
  <div class="lg-glass-soft rounded-[28px] px-6 py-10 text-center">
    <div class="relative mx-auto flex h-16 w-16 items-center justify-center">
      <span class="absolute inset-0 rounded-full bg-cyan-300/30 blur-2xl" />
      <span class="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-white/60 bg-white/70 text-sky-700 shadow-sm">
        <slot name="icon">
          <Inbox :size="26" />
        </slot>
      </span>
    </div>
    <h3 class="mt-4 text-base font-bold text-slate-950">{{ title }}</h3>
    <p class="mx-auto mt-2 max-w-md text-sm leading-6 text-slate-600">{{ description }}</p>
    <div v-if="$slots.default" class="mt-5">
      <slot />
    </div>
  </div>
</template>
