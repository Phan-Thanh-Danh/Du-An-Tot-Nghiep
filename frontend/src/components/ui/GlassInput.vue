<script setup>
import { computed, useId } from 'vue'

const props = defineProps({
  id: String,
  name: String,
  modelValue: {
    type: [String, Number],
    default: '',
  },
  label: String,
  placeholder: String,
  type: {
    type: String,
    default: 'text',
  },
  disabled: Boolean,
  error: String,
  autocomplete: String,
  inputmode: String,
  required: Boolean,
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['sm', 'md', 'lg'].includes(value),
  },
})

const emit = defineEmits(['update:modelValue'])

const generatedId = useId()
const inputId = computed(() => props.id || `glass-input-${generatedId}`)
const errorId = computed(() => `${inputId.value}-error`)

const sizeClass = computed(() => ({
  sm: 'glass-input-sm',
  md: 'glass-input-md',
  lg: 'glass-input-lg',
}[props.size]))
</script>

<template>
  <div class="block space-y-1.5">
    <label v-if="label" :for="inputId" class="lg-label">
      {{ label }}
    </label>
    <div
      :class="[
        'lg-input flex items-center gap-2',
        sizeClass,
        error ? 'lg-input-error glass-input-error' : '',
        disabled ? 'cursor-not-allowed opacity-60' : '',
      ]"
    >
      <slot name="prefix" />
      <input
        :id="inputId"
        :name="name"
        :value="modelValue"
        :type="type"
        :placeholder="placeholder"
        :disabled="disabled"
        :autocomplete="autocomplete"
        :inputmode="inputmode"
        :required="required"
        :aria-invalid="error ? 'true' : undefined"
        :aria-describedby="error ? errorId : undefined"
        class="glass-input-control min-w-0 flex-1 bg-transparent text-heading outline-none placeholder:text-placeholder disabled:cursor-not-allowed"
        @input="emit('update:modelValue', $event.target.value)"
      />
      <slot name="suffix" />
    </div>
    <p v-if="error" :id="errorId" role="alert" class="lg-error-text">
      {{ error }}
    </p>
  </div>
</template>

<style scoped>
.glass-input-sm {
  min-height: var(--control-height-sm);
  padding: 0 0.625rem;
}

.glass-input-md {
  min-height: var(--control-height-md);
  padding: 0 0.75rem;
}

.glass-input-lg {
  min-height: var(--control-height-lg);
  padding: 0 0.875rem;
}

.glass-input-control {
  font-size: 0.84375rem;
}

.glass-input-sm .glass-input-control {
  font-size: 0.8125rem;
}

.glass-input-lg .glass-input-control {
  font-size: 0.875rem;
}

.glass-input-error {
  background: var(--color-danger-bg);
  box-shadow:
    0 0 0 3px color-mix(in srgb, var(--color-danger-text) 14%, transparent),
    var(--lg-shadow-sm);
}
</style>
