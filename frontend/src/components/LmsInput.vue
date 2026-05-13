<script setup>
/**
 * LmsInput.vue
 * Reusable Liquid Glass input component
 * Props: modelValue, type, placeholder, label, error, icon, disabled, required
 */
import { computed } from 'vue'

const props = defineProps({
  id: String,
  modelValue: {
    type: [String, Number],
    default: '',
  },
  type: {
    type: String,
    default: 'text',
  },
  placeholder: String,
  label: String,
  error: String,
  disabled: Boolean,
  required: Boolean,
  icon: Object, // lucide icon component
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v),
  },
})

const emit = defineEmits(['update:modelValue', 'focus', 'blur'])

const paddingClasses = {
  sm: 'px-3 py-2 text-xs',
  md: 'px-4 py-3 text-sm',
  lg: 'px-5 py-4 text-base',
}

const inputClass = computed(() => [
  'lg-input',
  paddingClasses[props.size],
  {
    'error': props.error,
    'cursor-not-allowed opacity-60': props.disabled,
  },
])

const inputId = computed(() => props.id || `lms-input-${String(props.label || props.placeholder || 'field').toLowerCase().replace(/\s+/g, '-')}`)
const errorId = computed(() => `${inputId.value}-error`)
</script>

<template>
  <div class="space-y-2">
    <label
      v-if="label"
      :for="inputId"
      :class="['text-sm font-semibold text-slate-700', { 'after:ml-1 after:text-red-600 after:content-[\'*\']': required }]"
    >
      {{ label }}
    </label>

    <div class="relative">
      <component
        v-if="icon"
        :is="icon"
        :size="18"
        class="absolute left-3.5 top-1/2 -translate-y-1/2 shrink-0 text-slate-400 pointer-events-none"
      />

      <input
        :id="inputId"
        :value="modelValue"
        :type="type"
        :placeholder="placeholder"
        :disabled="disabled"
        :class="[inputClass, { 'pl-10': icon }]"
        :aria-describedby="error ? errorId : undefined"
        :aria-invalid="Boolean(error)"
        :required="required"
        @input="emit('update:modelValue', $event.target.value)"
        @focus="emit('focus')"
        @blur="emit('blur')"
      />
    </div>

    <p v-if="error" :id="errorId" class="text-sm font-medium text-red-600">
      {{ error }}
    </p>
  </div>
</template>

<style scoped>
/* Component-specific styles can go here if needed */
</style>
