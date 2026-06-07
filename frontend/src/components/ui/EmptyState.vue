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
  <div class="empty-state">
    <div class="relative mx-auto flex h-12 w-12 items-center justify-center">
      <span class="empty-state-glow" />
      <span class="empty-state-icon">
        <slot name="icon">
          <Inbox :size="22" />
        </slot>
      </span>
    </div>
    <h3 class="mt-3 text-sm font-bold text-heading">{{ title }}</h3>
    <p class="mx-auto mt-1.5 max-w-md text-sm leading-6 text-body">{{ description }}</p>
    <div v-if="$slots.default" class="mt-3">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.empty-state {
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: var(--card-padding-lg);
  text-align: center;
  box-shadow: var(--lg-shadow-sm);
}

.empty-state-glow {
  position: absolute;
  inset: 0.25rem;
  border-radius: 9999px;
  background: var(--accent-cyan-soft);
  filter: blur(14px);
  opacity: 0.45;
}

.empty-state-icon {
  position: relative;
  display: flex;
  width: 2.5rem;
  height: 2.5rem;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  box-shadow: var(--lg-shadow-sm);
}
</style>
