<script setup>
defineProps({
  id: String,
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
})

const emit = defineEmits(['update:modelValue'])
</script>

<template>
  <label class="block space-y-2">
    <span v-if="label" class="text-sm font-semibold text-slate-700">{{ label }}</span>
    <input
      :id="id"
      :value="modelValue"
      :type="type"
      :placeholder="placeholder"
      :disabled="disabled"
      :class="['lg-input', error ? 'error' : '']"
      @input="emit('update:modelValue', $event.target.value)"
    />
    <span v-if="error" class="block text-sm font-semibold text-red-600">{{ error }}</span>
  </label>
</template>
