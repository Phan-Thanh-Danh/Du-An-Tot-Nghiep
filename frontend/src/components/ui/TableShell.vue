<script setup>
defineProps({
  density: {
    type: String,
    default: 'default',
    validator: (value) => ['compact', 'default'].includes(value),
  },
  stickyHeader: Boolean,
  empty: Boolean,
})
</script>

<template>
  <div
    :class="[
      'lg-table-shell table-shell overflow-x-auto',
      density === 'compact' ? 'table-shell-compact' : 'table-shell-default',
      stickyHeader ? 'table-shell-sticky' : '',
      empty ? 'table-shell-empty' : '',
    ]"
  >
    <slot />
  </div>
</template>

<style scoped>
.table-shell {
  color: var(--text-body);
}

.table-shell :deep(table) {
  min-width: 100%;
}

.table-shell :deep(th) {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0;
}

.table-shell :deep(td) {
  color: var(--text-body);
  font-size: 0.84375rem;
}

.table-shell :deep(thead th) {
  position: relative;
}

.table-shell-sticky :deep(thead th) {
  position: sticky;
  top: 0;
  z-index: 1;
}

.table-shell-default :deep(th),
.table-shell-default :deep(td) {
  padding-inline: 0.875rem;
}

.table-shell-compact :deep(th),
.table-shell-compact :deep(td) {
  height: calc(var(--table-row-height) - 0.25rem);
  padding-block: 0.5rem;
  padding-inline: 0.75rem;
}

.table-shell-empty {
  min-height: 8rem;
}
</style>
