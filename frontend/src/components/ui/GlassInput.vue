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
})

const emit = defineEmits(['update:modelValue'])

const generatedId = useId()
const inputId = computed(() => props.id || `glass-input-${generatedId}`)
const errorId = computed(() => `${inputId.value}-error`)
</script>

<template>
  <div class="block space-y-2">
    <label v-if="label" :for="inputId" class="lg-label">
      {{ label }}
    </label>
    <div
      :class="[
        'lg-input flex min-h-11 items-center gap-2 px-3.5',
        error ? 'border-red-300 bg-red-50/70 shadow-[0_0_0_4px_rgba(220,38,38,0.10)]' : '',
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
        class="min-w-0 flex-1 bg-transparent text-sm text-heading outline-none placeholder:text-slate-400 dark:placeholder:text-slate-500 disabled:cursor-not-allowed"
        @input="emit('update:modelValue', $event.target.value)"
      />
      <slot name="suffix" />
    </div>
    <p v-if="error" :id="errorId" role="alert" class="lg-error-text">
      {{ error }}
    </p>
  </div>
</template>
