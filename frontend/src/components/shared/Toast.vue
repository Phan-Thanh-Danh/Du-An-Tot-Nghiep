<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { CircleCheck, CircleX, AlertTriangle, Info, X } from 'lucide-vue-next'

export interface Toast {
  id: number
  type: 'success' | 'error' | 'warning' | 'info'
  message: string
}

const props = defineProps<{
  toasts: Toast[]
}>()

const emit = defineEmits<{
  dismiss: [id: number]
}>()

const icons = {
  success: CircleCheck,
  error: CircleX,
  warning: AlertTriangle,
  info: Info,
}

const colors = {
  success: 'border-emerald-200 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950 text-emerald-800 dark:text-emerald-200',
  error: 'border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950 text-red-800 dark:text-red-200',
  warning: 'border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950 text-amber-800 dark:text-amber-200',
  info: 'border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-950 text-blue-800 dark:text-blue-200',
}

const iconColors = {
  success: 'text-emerald-500 dark:text-emerald-400',
  error: 'text-red-500 dark:text-red-400',
  warning: 'text-amber-500 dark:text-amber-400',
  info: 'text-blue-500 dark:text-blue-400',
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none">
      <TransitionGroup
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-x-8 scale-95"
        enter-to-class="opacity-100 translate-x-0 scale-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-x-0 scale-100"
        leave-to-class="opacity-0 translate-x-8 scale-95"
      >
        <div
          v-for="toast in toasts"
          :key="toast.id"
          :class="['pointer-events-auto flex items-start gap-3 rounded-xl border p-4 shadow-lg', colors[toast.type]]"
          role="alert"
        >
          <component :is="icons[toast.type]" :size="20" class="shrink-0 mt-0.5" :class="iconColors[toast.type]" aria-hidden="true" />
          <p class="flex-1 text-sm font-medium">{{ toast.message }}</p>
          <button
            type="button"
            :aria-label="'Đóng thông báo'"
            class="shrink-0 rounded-lg p-1 opacity-60 hover:opacity-100 transition-opacity focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-current"
            @click="emit('dismiss', toast.id)"
          >
            <X :size="14" aria-hidden="true" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
