<script setup>
import { CheckCircle2, AlertCircle, AlertTriangle, Info, X } from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const iconMap = {
  success: CheckCircle2,
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info,
}

const borderMap = {
  success: 'border-emerald-200/60 dark:border-emerald-500/20',
  error: 'border-red-200/60 dark:border-red-500/20',
  warning: 'border-amber-200/60 dark:border-amber-500/20',
  info: 'border-blue-200/60 dark:border-blue-500/20',
}

const bgMap = {
  success: 'bg-white/88 dark:bg-slate-950/82',
  error: 'bg-white/88 dark:bg-slate-950/82',
  warning: 'bg-white/88 dark:bg-slate-950/82',
  info: 'bg-white/88 dark:bg-slate-950/82',
}

const iconColorMap = {
  success: 'text-emerald-600 dark:text-emerald-400',
  error: 'text-red-600 dark:text-red-400',
  warning: 'text-amber-600 dark:text-amber-400',
  info: 'text-blue-600 dark:text-blue-400',
}
</script>

<template>
  <Teleport to="body">
    <div
      class="pointer-events-none fixed inset-0 z-[110] flex flex-col items-end gap-3 p-4 sm:p-6"
      aria-live="polite"
    >
      <TransitionGroup
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-x-8 scale-95"
        enter-to-class="opacity-100 translate-x-0 scale-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-x-0 scale-100"
        leave-to-class="opacity-0 translate-x-8 scale-95"
      >
        <div
          v-for="n in popupStore.notifications"
          :key="n.id"
          :class="[
            'pointer-events-auto relative flex w-full max-w-sm items-start gap-3 overflow-hidden rounded-2xl border p-4 shadow-[0_20px_50px_rgba(15,23,42,0.15)] backdrop-blur-2xl dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)]',
            borderMap[n.type],
            bgMap[n.type],
          ]"
          role="status"
        >
          <div
            :class="[
              'flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full',
              iconColorMap[n.type],
            ]"
          >
            <component :is="iconMap[n.type]" :size="18" />
          </div>
          <div class="min-w-0 flex-1">
            <p v-if="n.title" class="text-sm font-bold text-slate-900 dark:text-slate-100">
              {{ n.title }}
            </p>
            <p
              v-if="n.message"
              class="mt-0.5 text-sm leading-snug text-slate-600 dark:text-slate-400"
            >
              {{ n.message }}
            </p>
          </div>
          <button
            class="-mr-1 -mt-1 flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full text-slate-400 hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800 dark:hover:text-slate-300 transition-colors"
            aria-label="Đóng"
            @click="popupStore.dismiss(n.id)"
          >
            <X :size="14" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
