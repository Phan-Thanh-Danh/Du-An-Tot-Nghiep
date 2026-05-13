<script setup>
/**
 * LmsAlert.vue
 * Reusable Liquid Glass alert component
 * Types: success, error, warning, info
 * Props: type, title, icon, closeable
 */
import { ref } from 'vue'
import { AlertCircle, CheckCircle2, AlertTriangle, Info, X } from 'lucide-vue-next'

const props = defineProps({
  type: {
    type: String,
    default: 'info',
    validator: (v) => ['success', 'error', 'warning', 'info'].includes(v),
  },
  title: String,
  icon: Object,
  closeable: Boolean,
})

const isOpen = ref(true)

const typeConfig = {
  success: {
    icon: CheckCircle2,
    class: 'lg-alert-success',
  },
  error: {
    icon: AlertCircle,
    class: 'lg-alert-error',
  },
  warning: {
    icon: AlertTriangle,
    class: 'lg-alert-warning',
  },
  info: {
    icon: Info,
    class: 'lg-alert-info',
  },
}

const config = typeConfig[props.type]
const Icon = props.icon || config.icon
</script>

<template>
  <transition
    enter-active-class="transition-all duration-200"
    enter-from-class="opacity-0 -translate-y-2"
    enter-to-class="opacity-100 translate-y-0"
    leave-active-class="transition-all duration-200"
    leave-from-class="opacity-100 translate-y-0"
    leave-to-class="opacity-0 -translate-y-2"
  >
    <div v-if="isOpen" :class="['lg-alert', config.class]" role="alert">
      <Icon :size="18" class="shrink-0 mt-0.5" />
      <div class="flex-1">
        <p v-if="title" class="font-semibold">{{ title }}</p>
        <slot />
      </div>
      <button
        v-if="closeable"
        type="button"
        class="shrink-0 -mr-1 p-1 hover:opacity-70 transition"
        @click="isOpen = false"
      >
        <X :size="18" />
      </button>
    </div>
  </transition>
</template>

<style scoped>
/* Component-specific styles can go here if needed */
</style>
