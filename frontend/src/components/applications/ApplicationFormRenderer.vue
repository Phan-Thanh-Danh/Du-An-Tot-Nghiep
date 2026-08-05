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
      <label class="block text-sm font-medium text-(--text-heading)">
        {{ field.label }}
        <span v-if="field.required" class="text-(--color-danger-text)">*</span>
      </label>
      
      <!-- Fallback display if readonly -->
      <div v-if="readonly" class="p-3 bg-(--surface-hover) rounded-lg text-sm text-(--text-body) min-h-[40px]">
        {{ formData[field.key] || '---' }}
      </div>
      
      <template v-else>
        <!-- Text / Number -->
        <GlassInput 
          v-if="['text', 'number', 'date', 'email'].includes(field.type)"
          v-model="formData[field.key]"
          :type="field.type"
          :placeholder="field.placeholder"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
        />
        
        <!-- Textarea -->
        <textarea 
          v-else-if="field.type === 'textarea'"
          v-model="formData[field.key]"
          :placeholder="field.placeholder"
          rows="3"
          class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all resize-y"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
        ></textarea>
        
        <!-- Select -->
        <select 
          v-else-if="field.type === 'select'"
          v-model="formData[field.key]"
          class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
        >
          <option value="" disabled>{{ field.placeholder || 'Chọn...' }}</option>
          <option v-for="opt in field.options" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </option>
        </select>
        
        <!-- Multiselect -->
        <div v-else-if="field.type === 'multiselect'" class="space-y-2 pt-1">
          <template v-if="Array.isArray(field.options)">
            <label v-for="(opt, idx) in field.options" :key="opt.value ?? idx" class="flex items-center gap-2 cursor-pointer text-sm">
              <input 
                type="checkbox" 
                :value="opt.value ?? opt.Value" 
                v-model="formData[field.key]"
                class="w-4 h-4 rounded text-(--lg-primary) focus:ring-(--lg-primary) bg-(--surface-input) border-(--border-input)"
              />
              <span class="text-sm text-(--text-body)">{{ opt.label ?? opt.Label }}</span>
            </label>
          </template>
          <template v-else>
            <label v-for="(val, key) in field.options" :key="key" class="flex items-center gap-2 cursor-pointer text-sm">
              <input 
                type="checkbox" 
                :value="key" 
                v-model="formData[field.key]"
                class="w-4 h-4 rounded text-(--lg-primary) focus:ring-(--lg-primary) bg-(--surface-input) border-(--border-input)"
              />
              <span class="text-sm text-(--text-body)">{{ val }}</span>
            </label>
          </template>
        </div>

        <!-- Single Checkbox -->
        <label v-else-if="field.type === 'checkbox'" class="flex items-center gap-2 cursor-pointer">
          <input 
            type="checkbox" 
            v-model="formData[field.key]"
            class="w-4 h-4 rounded text-(--lg-primary) focus:ring-(--lg-primary) bg-(--surface-input) border-(--border-input)"
          />
          <span class="text-sm text-(--text-body)">{{ field.checkboxLabel || 'Xác nhận' }}</span>
        </label>
        
        <!-- Fallback -->
        <div v-else class="text-xs text-(--color-warning-text) italic p-2 border border-dashed border-(--color-warning-border) rounded">
          Trường '{{ field.type }}' chưa được frontend hỗ trợ
        </div>
      </template>

      <!-- Error message -->
      <p v-if="errors[field.key]" class="text-xs text-(--color-danger-text) mt-1">
        {{ errors[field.key] }}
      </p>
    </div>
  </div>
</template>
