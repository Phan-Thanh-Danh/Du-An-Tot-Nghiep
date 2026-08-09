<script setup>
import { computed } from 'vue'
import { Pencil, Trash2, Plus } from 'lucide-vue-next'
import GlassInput from '@/components/ui/GlassInput.vue'

const props = defineProps({
  schema: { type: Array, default: () => [] },
  modelValue: { type: Object, default: () => ({}) },
  readonly: { type: Boolean, default: false },
  editable: { type: Boolean, default: false },
  errors: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue', 'edit-field', 'delete-field', 'add-field'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})
</script>

<template>
  <div class="space-y-4">
    <div v-if="editable" class="flex justify-end mb-2">
      <button
        type="button"
        class="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-lg bg-(--lg-primary) text-white hover:opacity-90 transition-all"
        @click="emit('add-field')"
      >
        <Plus :size="14" />
        Thêm trường mới
      </button>
    </div>

    <div
      v-for="field in schema"
      :key="field.id || field.key"
      :class="[
        'space-y-1 relative transition-all duration-200',
        editable ? 'group/renderer p-3 border border-dashed border-transparent hover:border-(--lg-primary) hover:bg-(--surface-hover)/40 rounded-xl' : ''
      ]"
    >
      <!-- Action buttons for editable preview -->
      <div
        v-if="editable"
        class="absolute right-3 top-3 hidden group-hover/renderer:flex items-center gap-1 z-10 bg-(--surface-card) border border-(--border-card) shadow-sm rounded-lg p-1"
      >
        <button
          type="button"
          class="rounded p-1 text-(--text-placeholder) hover:bg-(--lg-primary)/10 hover:text-(--lg-primary)"
          title="Chỉnh sửa trường"
          @click="emit('edit-field', field)"
        >
          <Pencil :size="14" />
        </button>
        <button
          type="button"
          class="rounded p-1 text-(--color-danger-text) hover:bg-(--color-danger-bg)"
          title="Xóa trường"
          @click="emit('delete-field', field)"
        >
          <Trash2 :size="14" />
        </button>
      </div>

      <label class="block text-sm font-medium text-(--text-heading)">
        {{ field.label }}
        <span v-if="field.required" class="text-(--color-danger-text)">*</span>
      </label>

      <!-- Fallback display if readonly -->
      <div v-if="readonly" class="p-3 bg-(--surface-hover) rounded-lg text-sm text-(--text-body) min-h-[40px]">
        {{ formData[field.key] || '---' }}
      </div>

      <template v-else>
        <!-- Text / Number / Date / Email / Tel -->
        <GlassInput
          v-if="['text', 'number', 'date', 'email', 'tel'].includes(field.type)"
          v-model="formData[field.key]"
          :type="field.type"
          :placeholder="field.placeholder"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
          :disabled="editable"
        />

        <!-- Textarea -->
        <textarea
          v-else-if="field.type === 'textarea'"
          v-model="formData[field.key]"
          :placeholder="field.placeholder"
          rows="3"
          class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all resize-y"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
          :disabled="editable"
        ></textarea>

        <!-- Select -->
        <select
          v-else-if="field.type === 'select' || field.type === 'related_entity'"
          v-model="formData[field.key]"
          class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all"
          :class="{'border-(--color-danger-border)': errors[field.key]}"
          :disabled="field.readonly || editable"
        >
          <option value="" disabled>{{ field.placeholder || 'Chọn...' }}</option>
          <option v-for="(opt, idx) in field.options" :key="opt.value ?? opt.Value ?? idx" :value="opt.value ?? opt.Value">
            {{ opt.label ?? opt.Label }}
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
                :disabled="editable"
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
                :disabled="editable"
              />
              <span class="text-sm text-(--text-body)">{{ val }}</span>
            </label>
          </template>
        </div>

        <!-- Single Checkbox / Boolean -->
        <label v-else-if="field.type === 'checkbox' || field.type === 'boolean'" class="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            v-model="formData[field.key]"
            class="w-4 h-4 rounded text-(--lg-primary) focus:ring-(--lg-primary) bg-(--surface-input) border-(--border-input)"
            :disabled="editable"
          />
          <span class="text-sm text-(--text-body)">{{ field.checkboxLabel || 'Xác nhận' }}</span>
        </label>

        <!-- Student info / auto-filled text -->
        <div v-else-if="field.type === 'studentInfo' || field.readonly" class="p-3 bg-(--surface-hover) rounded-lg text-sm text-(--text-body) min-h-[40px]">
          {{ formData[field.key] || '—' }}
        </div>

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

    <div v-if="editable && schema.length" class="flex justify-center pt-2">
      <button
        type="button"
        class="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-lg border border-(--border-card) text-(--text-body) hover:border-(--lg-primary) hover:text-(--lg-primary) transition-all"
        @click="emit('add-field')"
      >
        <Plus :size="14" />
        Thêm trường mới
      </button>
    </div>
  </div>
</template>
