<script setup>
import { ref, computed } from 'vue'
import { ChevronDown, Check } from 'lucide-vue-next'
import { onClickOutside } from '@vueuse/core'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  options: {
    type: Array,
    required: true,
    // Expected format: [{ value: '...', label: '...' }]
  },
  label: String,
  placeholder: {
    type: String,
    default: 'Chọn một tùy chọn'
  },
  disabled: Boolean,
  required: Boolean,
  id: String,
  error: String,
})

const emit = defineEmits(['update:modelValue', 'change'])

const isOpen = ref(false)
const dropdownRef = ref(null)

onClickOutside(dropdownRef, () => {
  isOpen.value = false
})

const selectedOption = computed(() => {
  return props.options.find(opt => opt.value === props.modelValue)
})

const toggleDropdown = () => {
  if (!props.disabled) {
    isOpen.value = !isOpen.value
  }
}

const selectOption = (option) => {
  emit('update:modelValue', option.value)
  emit('change', option.value)
  isOpen.value = false
}

const selectId = computed(() => props.id || `lms-select-${Math.random().toString(36).substr(2, 9)}`)
</script>

<template>
  <div class="space-y-2 relative" ref="dropdownRef">
    <label
      v-if="label"
      :for="selectId"
      :class="['lg-label', { 'after:ml-1 after:text-red-600 after:content-[\'*\']': required }]"
      @click="toggleDropdown"
    >
      {{ label }}
    </label>

    <div 
      :id="selectId"
      class="lg-input flex items-center justify-between cursor-pointer select-none px-4 py-3 text-sm transition-all duration-200"
      :class="[
        isOpen ? 'ring-2 ring-(--focus-ring) border-(--focus-ring)' : '',
        disabled ? 'cursor-not-allowed opacity-60' : '',
        error ? 'error ring-2 ring-red-500/50 border-red-500' : ''
      ]"
      @click="toggleDropdown"
      tabindex="0"
      @keydown.enter="toggleDropdown"
      @keydown.space.prevent="toggleDropdown"
    >
      <span class="truncate flex-1 text-left" :class="selectedOption ? 'text-(--text-heading)' : 'text-(--text-placeholder)'">
        {{ selectedOption ? selectedOption.label : placeholder }}
      </span>
      <ChevronDown 
        class="w-4 h-4 text-(--text-muted) transition-transform duration-200" 
        :class="{ 'rotate-180': isOpen }" 
      />
    </div>

    <!-- Dropdown Menu -->
    <Transition
      enter-active-class="transition duration-100 ease-out"
      enter-from-class="transform scale-95 opacity-0"
      enter-to-class="transform scale-100 opacity-100"
      leave-active-class="transition duration-75 ease-in"
      leave-from-class="transform scale-100 opacity-100"
      leave-to-class="transform scale-95 opacity-0"
    >
      <div 
        v-if="isOpen" 
        class="absolute z-50 w-full mt-1 surface-dropdown border border-card rounded-lg shadow-xl overflow-hidden py-1"
      >
        <ul class="max-h-60 overflow-y-auto">
          <li 
            v-for="option in options" 
            :key="option.value"
            @click="selectOption(option)"
            class="px-4 py-2.5 text-sm cursor-pointer flex items-center justify-between hover:bg-(--surface-table-row-hover) transition-colors"
            :class="{
              'bg-(--surface-table-row-hover) text-(--text-heading) font-semibold': option.value === modelValue,
              'text-(--text-body)': option.value !== modelValue
            }"
          >
            <span class="truncate flex-1 text-left">{{ option.label }}</span>
            <Check v-if="option.value === modelValue" class="w-4 h-4 text-(--text-heading) shrink-0 ml-2" />
          </li>
        </ul>
      </div>
    </Transition>

    <p v-if="error" class="lg-error-text">
      {{ error }}
    </p>
  </div>
</template>

<style scoped>
/* Scrollbar styling for the dropdown */
ul::-webkit-scrollbar {
  width: 6px;
}
ul::-webkit-scrollbar-track {
  background: transparent;
}
ul::-webkit-scrollbar-thumb {
  background-color: rgba(156, 163, 175, 0.5);
  border-radius: 20px;
}
.dark ul::-webkit-scrollbar-thumb {
  background-color: rgba(71, 85, 105, 0.5);
}
</style>
