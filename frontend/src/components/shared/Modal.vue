<script setup lang="ts">
import { X, AlertTriangle, CircleCheck, Info } from 'lucide-vue-next'

export type ModalType = 'confirm' | 'delete' | 'info'

const props = defineProps<{
  open: boolean
  type?: ModalType
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  loading?: boolean
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

const icons = {
  delete: AlertTriangle,
  confirm: CircleCheck,
  info: Info,
}

const iconColors = {
  delete: 'text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-500/10',
  confirm: 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10',
  info: 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10',
}

const buttonColors = {
  delete: 'bg-red-600 hover:bg-red-700 dark:bg-red-500 dark:hover:bg-red-600 focus-visible:ring-red-500',
  confirm: 'bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 focus-visible:ring-blue-500',
  info: 'bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 focus-visible:ring-blue-500',
}
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="open" class="fixed inset-0 z-[90] flex items-center justify-center p-4" role="dialog" aria-modal="true" :aria-label="title">
        <div class="fixed inset-0 bg-black/40 dark:bg-black/60 backdrop-blur-sm" @click="emit('cancel')" />
        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 scale-95 translate-y-2"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 translate-y-2"
        >
          <div v-if="open" class="relative z-10 w-full max-w-md rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-xl p-6">
            <button
              type="button"
              :aria-label="'Đóng'"
              class="absolute top-4 right-4 rounded-lg p-1 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
              @click="emit('cancel')"
            >
              <X :size="18" aria-hidden="true" />
            </button>

            <div class="flex flex-col items-center text-center gap-3">
              <div v-if="type" :class="['flex h-12 w-12 items-center justify-center rounded-full', iconColors[type]]">
                <component :is="icons[type]" :size="24" aria-hidden="true" />
              </div>
              <div class="space-y-1">
                <h3 class="text-lg font-bold text-slate-900 dark:text-white">{{ title }}</h3>
                <p class="text-sm text-slate-500 dark:text-slate-400">{{ message }}</p>
              </div>
            </div>

            <div class="mt-6 flex items-center gap-3 justify-center">
              <button
                v-if="type !== 'info'"
                type="button"
                :aria-label="cancelLabel || 'Hủy bỏ'"
                class="inline-flex items-center justify-center rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 px-4 py-2.5 text-sm font-semibold text-slate-700 dark:text-slate-300 shadow-sm transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
                @click="emit('cancel')"
              >
                {{ cancelLabel || 'Hủy' }}
              </button>
              <button
                type="button"
                :disabled="loading"
                :aria-label="confirmLabel || 'Xác nhận'"
                :class="['inline-flex items-center justify-center gap-2 rounded-xl px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition-all duration-200 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed', type ? buttonColors[type] : buttonColors.confirm]"
                @click="emit('confirm')"
              >
                {{ confirmLabel || 'Xác nhận' }}
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
