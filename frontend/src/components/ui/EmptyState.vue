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
  <div class="lg-glass-soft rounded-[22px] px-5 py-8 text-center">
    <div class="relative mx-auto flex h-14 w-14 items-center justify-center">
      <span class="absolute inset-0 rounded-full bg-cyan-300/30 blur-2xl" />
      <span class="surface-input border-card relative flex h-12 w-12 items-center justify-center rounded-2xl border text-link shadow-sm">
        <slot name="icon">
          <Inbox :size="24" />
        </slot>
      </span>
    </div>
    <h3 class="mt-4 text-base font-bold text-heading">{{ title }}</h3>
    <p class="mx-auto mt-2 max-w-md text-sm leading-6 text-body">{{ description }}</p>
    <div v-if="$slots.default" class="mt-4">
      <slot />
    </div>
  </div>
</template>
