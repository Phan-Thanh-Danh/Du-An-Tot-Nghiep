<template>
  <div class="relative w-full" ref="containerRef">
    <div
      class="surface-input w-full rounded-lg border border-slate-300 flex items-center justify-between cursor-pointer min-h-[38px] px-3 py-1.5"
      :class="{
        'opacity-50 cursor-not-allowed bg-slate-50': disabled, 
        'border-blue-500 ring-1 ring-blue-500': isOpen && !disabled
      }"
      @click="toggleDropdown"
    >
      <input
        v-if="isOpen"
        ref="searchInputRef"
        type="text"
        v-model="searchQuery"
        class="w-full bg-transparent border-none outline-none focus:ring-0 p-0 text-sm"
        :placeholder="'Tìm kiếm...'"
        @blur="onBlur"
      />
      <span v-else class="text-sm text-slate-700 truncate block w-full">
        <span v-if="selectedLabel">{{ selectedLabel }}</span>
        <span v-else class="text-slate-400">{{ placeholder }}</span>
      </span>
      <ChevronDownIcon class="w-4 h-4 text-slate-400 ml-2 flex-shrink-0" />
    </div>

    <div
      v-if="isOpen"
      class="absolute z-50 w-full mt-1 bg-white border border-slate-200 rounded-lg shadow-lg max-h-60 overflow-y-auto"
      @mousedown.prevent
    >
      <div v-if="filteredOptions.length === 0" class="px-3 py-2 text-sm text-slate-500">
        Không tìm thấy kết quả
      </div>
      <div
        v-for="opt in filteredOptions"
        :key="opt.value"
        class="px-3 py-2 text-sm cursor-pointer hover:bg-slate-100"
        :class="{'bg-blue-50 text-blue-700 font-medium': opt.value === modelValue}"
        @click="selectOption(opt)"
      >
        {{ opt.label }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { ChevronDownIcon } from 'lucide-vue-next'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: null
  },
  options: {
    type: Array,
    default: () => []
  },
  placeholder: {
    type: String,
    default: '-- Chọn --'
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const isOpen = ref(false)
const searchQuery = ref('')
const searchInputRef = ref(null)

const selectedLabel = computed(() => {
  const selected = props.options.find(opt => opt.value === props.modelValue)
  return selected ? selected.label : ''
})

const filteredOptions = computed(() => {
  if (!searchQuery.value) return props.options
  const query = searchQuery.value.toLowerCase()
  return props.options.filter(opt => opt.label.toLowerCase().includes(query))
})

const toggleDropdown = async () => {
  if (props.disabled) return
  isOpen.value = true
  searchQuery.value = ''
  await nextTick()
  if (searchInputRef.value) {
    searchInputRef.value.focus()
  }
}

const onBlur = () => {
  // Tránh việc blur kích hoạt trước khi click chọn item
  setTimeout(() => {
    isOpen.value = false
  }, 150)
}

const selectOption = (opt) => {
  emit('update:modelValue', opt.value)
  emit('change', opt.value)
  isOpen.value = false
}
</script>
