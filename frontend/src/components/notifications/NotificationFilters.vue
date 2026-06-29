<script setup>

import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import { Search } from 'lucide-vue-next'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ filter: 'all', search: '' }),
  }
})
const emit = defineEmits(['update:modelValue'])

const setFilter = (filter) => {
  emit('update:modelValue', { ...props.modelValue, filter })
}

const updateSearch = (e) => {
  emit('update:modelValue', { ...props.modelValue, search: e.target.value })
}
</script>

<template>
  <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
    <div class="flex gap-2 p-1 bg-(--surface-card) rounded-lg border border-(--border-card)">
      <GlassButton
        size="sm"
        :variant="modelValue.filter === 'all' ? 'primary' : 'ghost'"
        @click="setFilter('all')"
      >
        Tất cả
      </GlassButton>
      <GlassButton
        size="sm"
        :variant="modelValue.filter === 'unread' ? 'primary' : 'ghost'"
        @click="setFilter('unread')"
      >
        Chưa đọc
      </GlassButton>
      <GlassButton
        size="sm"
        :variant="modelValue.filter === 'urgent' ? 'primary' : 'ghost'"
        @click="setFilter('urgent')"
      >
        Quan trọng
      </GlassButton>
    </div>

    <div class="relative w-full sm:w-64">
      <GlassInput
        type="text"
        placeholder="Tìm kiếm thông báo..."
        :value="modelValue.search"
        @input="updateSearch"
      />
      <Search class="absolute right-3 top-2.5 w-4 h-4 text-(--text-muted)" />
    </div>
  </div>
</template>
