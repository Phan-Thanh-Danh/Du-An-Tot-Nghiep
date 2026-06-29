<script setup>
import { computed } from 'vue'
import GlassInput from '@/components/ui/GlassInput.vue'

const props = defineProps({
  schema: { type: Array, default: () => [] },
  modelValue: { type: Object, default: () => ({}) },
  readonly: { type: Boolean, default: false },
  errors: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})
</script>

<template>
  <div class="space-y-4">
    <div v-for="field in schema" :key="field.id || field.key" class="space-y-1">
      <label class="block text-sm font-medium text-[var(--text-heading)]">
        {{ field.label }}
        <span v-if="field.required" class="text-[var(--color-danger-text)]">*</span>
      </label>
      
      <!-- Fallback display if readonly -->
      <div v-if="readonly" class="p-3 bg-[var(--surface-hover)] rounded-lg text-sm text-[var(--text-body)] min-h-[40px]">
        {{ formData[field.key] || '---' }}
      </div>
      
      <template v-else>
        <!-- Text / Number -->
        <GlassInput 
          v-if="['text', 'number', 'date', 'email'].includes(field.type)"
          v-model="formData[field.key]"
          :type="field.type"
          :placeholder="field.placeholder"
          :class="{'border-[var(--color-danger-border)]': errors[field.key]}"
        />
        
        <!-- Textarea -->
        <textarea 
          v-else-if="field.type === 'textarea'"
          v-model="formData[field.key]"
          :placeholder="field.placeholder"
          rows="3"
          class="w-full px-3 py-2 bg-[var(--surface-input)] border border-[var(--border-input)] rounded-lg focus:ring-2 focus:ring-[var(--lg-primary)] outline-none text-[var(--text-body)] transition-all resize-y"
          :class="{'border-[var(--color-danger-border)]': errors[field.key]}"
        ></textarea>
        
        <!-- Select -->
        <select 
          v-else-if="field.type === 'select'"
          v-model="formData[field.key]"
          class="w-full h-10 px-3 bg-[var(--surface-input)] border border-[var(--border-input)] rounded-lg focus:ring-2 focus:ring-[var(--lg-primary)] outline-none text-[var(--text-body)] transition-all"
          :class="{'border-[var(--color-danger-border)]': errors[field.key]}"
        >
          <option value="" disabled>{{ field.placeholder || 'Chọn...' }}</option>
          <option v-for="opt in field.options" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </option>
        </select>
        
        <!-- Checkbox -->
        <label v-else-if="field.type === 'checkbox'" class="flex items-center gap-2 cursor-pointer">
          <input 
            type="checkbox" 
            v-model="formData[field.key]"
            class="w-4 h-4 rounded text-[var(--lg-primary)] focus:ring-[var(--lg-primary)] bg-[var(--surface-input)] border-[var(--border-input)]"
          />
          <span class="text-sm text-[var(--text-body)]">{{ field.checkboxLabel || 'Xác nhận' }}</span>
        </label>
        
        <!-- Fallback -->
        <div v-else class="text-xs text-[var(--color-warning-text)] italic p-2 border border-dashed border-[var(--color-warning-border)] rounded">
          Trường '{{ field.type }}' chưa được frontend hỗ trợ
        </div>
      </template>

      <!-- Error message -->
      <p v-if="errors[field.key]" class="text-xs text-[var(--color-danger-text)] mt-1">
        {{ errors[field.key] }}
      </p>
    </div>
  </div>
</template>
