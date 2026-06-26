<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { EllipsisVertical, Pencil, Eye, Copy, Archive } from 'lucide-vue-next'

defineProps<{
  subjectId: string
}>()

const emit = defineEmits<{
  edit: [id: string]
  preview: [id: string]
  copy: [id: string]
  archive: [id: string]
}>()

const open = ref(false)

const menuItems = [
  { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, event: 'edit' as const },
  { id: 'preview', label: 'Xem Preview', icon: Eye, event: 'preview' as const },
  { id: 'copy', label: 'Sao chép', icon: Copy, event: 'copy' as const },
  { id: 'archive', label: 'Lưu trữ', icon: Archive, event: 'archive' as const },
]

function handleItemClick(event: string, id: string) {
  emit(event as any, id)
  open.value = false
}

function handleClickOutside(e: MouseEvent) {
  if (open.value && !(e.target as HTMLElement).closest('[data-subject-menu]')) {
    open.value = false
  }
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onUnmounted(() => document.removeEventListener('click', handleClickOutside))
</script>

<template>
  <div class="relative" data-subject-menu>
    <button
      type="button"
      :aria-label="'Menu thao tác cho môn học'"
      :aria-expanded="open"
      class="inline-flex items-center justify-center h-8 w-8 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
      @click.stop="open = !open"
    >
      <EllipsisVertical :size="16" aria-hidden="true" />
    </button>
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0 scale-95 -translate-y-1"
      enter-to-class="opacity-100 scale-100 translate-y-0"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100 scale-100 translate-y-0"
      leave-to-class="opacity-0 scale-95 -translate-y-1"
    >
      <div
        v-if="open"
        class="absolute right-0 top-full mt-1 z-30 min-w-[160px] rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-lg py-1.5 overflow-hidden"
        role="menu"
        :aria-label="'Menu thao tác'"
      >
        <button
          v-for="item in menuItems"
          :key="item.id"
          type="button"
          role="menuitem"
          :class="[
            'w-full flex items-center gap-2.5 px-3.5 py-2 text-sm transition-colors duration-150 focus-visible:outline-none focus-visible:bg-slate-50 dark:focus-visible:bg-slate-700',
            item.id === 'archive' ? 'text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10' : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700',
          ]"
          @click="handleItemClick(item.event, subjectId)"
        >
          <component :is="item.icon" :size="16" aria-hidden="true" />
          {{ item.label }}
        </button>
      </div>
    </Transition>
  </div>
</template>
