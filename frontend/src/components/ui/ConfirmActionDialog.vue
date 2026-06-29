<script setup>
import { computed, nextTick, ref, useId, watch } from 'vue'
import { AlertTriangle, X } from 'lucide-vue-next'
import GlassButton from './GlassButton.vue'
import GlassPanel from './GlassPanel.vue'

const props = defineProps({
  modelValue: Boolean,
  title: {
    type: String,
    default: 'Xác nhận thao tác',
  },
  message: {
    type: String,
    default: '',
  },
  confirmLabel: {
    type: String,
    default: 'Xác nhận',
  },
  cancelLabel: {
    type: String,
    default: 'Hủy',
  },
  variant: {
    type: String,
    default: 'primary',
    validator: (value) => ['primary', 'danger', 'success'].includes(value),
  },
  loading: Boolean,
  closeOnBackdrop: {
    type: Boolean,
    default: true,
  },
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const confirmButton = ref(null)
const dialogTitleId = `confirm-dialog-${useId()}`

const buttonVariant = computed(() => (props.variant === 'danger' ? 'danger' : props.variant))
const iconClass = computed(() => ({
  primary: 'confirm-dialog-icon-primary',
  danger: 'confirm-dialog-icon-danger',
  success: 'confirm-dialog-icon-success',
}[props.variant]))

function close() {
  if (props.loading) return
  emit('update:modelValue', false)
  emit('cancel')
}

function confirmAction() {
  if (props.loading) return
  emit('confirm')
}

watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return
    await nextTick()
    confirmButton.value?.$el?.focus?.()
  },
)
</script>

<template>
  <Teleport to="body">
    <div
      v-if="modelValue"
      class="confirm-dialog fixed inset-0 z-[var(--z-modal)] flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="dialogTitleId"
      @keydown.esc="close"
    >
      <div
        class="confirm-dialog-scrim absolute inset-0"
        aria-hidden="true"
        @click="closeOnBackdrop ? close() : undefined"
      />

      <GlassPanel
        variant="readable"
        density="comfortable"
        :clip="false"
        class="confirm-dialog-panel relative w-full max-w-lg"
      >
        <div class="flex items-start gap-3">
          <div :class="['confirm-dialog-icon flex h-11 w-11 shrink-0 items-center justify-center rounded-full', iconClass]">
            <AlertTriangle :size="22" aria-hidden="true" />
          </div>

          <div class="min-w-0 flex-1 space-y-2">
            <div class="flex items-start justify-between gap-3">
              <h2 :id="dialogTitleId" class="ui-section-title text-heading">
                {{ title }}
              </h2>
              <button
                type="button"
                class="lg-icon-button flex h-8 w-8 shrink-0 items-center justify-center"
                aria-label="Đóng hộp thoại xác nhận"
                :disabled="loading"
                @click="close"
              >
                <X :size="16" aria-hidden="true" />
              </button>
            </div>

            <p v-if="message" class="ui-body text-muted">
              {{ message }}
            </p>

            <slot />
          </div>
        </div>

        <div class="mt-5 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <GlassButton variant="secondary" :disabled="loading" @click="close">
            {{ cancelLabel }}
          </GlassButton>
          <GlassButton
            ref="confirmButton"
            :variant="buttonVariant"
            :loading="loading"
            @click="confirmAction"
          >
            {{ confirmLabel }}
          </GlassButton>
        </div>
      </GlassPanel>
    </div>
  </Teleport>
</template>

<style scoped>
.confirm-dialog {
  color: var(--text-body);
}

.confirm-dialog-scrim {
  background: color-mix(in srgb, var(--surface-app) 58%, transparent);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.confirm-dialog-panel {
  box-shadow: var(--lg-shadow-lg);
}

.confirm-dialog-icon {
  border: 1px solid var(--border-card);
}

.confirm-dialog-icon-primary {
  color: var(--color-info-text);
  background: var(--color-info-bg);
}

.confirm-dialog-icon-danger {
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
}

.confirm-dialog-icon-success {
  color: var(--color-success-text);
  background: var(--color-success-bg);
}
</style>
