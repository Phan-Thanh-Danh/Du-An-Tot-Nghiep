<script setup>
import { ref, computed, useSlots, useId, watch, onUnmounted, nextTick, Fragment, Text, Comment } from 'vue'
import { ChevronDown, Check } from 'lucide-vue-next'
import { onClickOutside } from '@vueuse/core'

defineOptions({ inheritAttrs: false })

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  options: {
    type: Array,
    default: () => [],
    // Expected format: [{ value: '...', label: '...', disabled?: false }]
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
  teleport: {
    type: Boolean,
    default: true
  }
})

const isTest = typeof process !== 'undefined' && process.env?.NODE_ENV === 'test' || import.meta.env?.MODE === 'test'
const useTeleport = computed(() => props.teleport && !isTest)

const emit = defineEmits(['update:modelValue', 'change'])
const slots = useSlots()
const generatedId = useId()

const isOpen = ref(false)
const triggerRef = ref(null)
const menuRef = ref(null)
const dropdownStyle = ref({})

onClickOutside([triggerRef, menuRef], () => {
  isOpen.value = false
})

function updatePosition() {
  if (!triggerRef.value) return
  const rect = triggerRef.value.getBoundingClientRect()
  dropdownStyle.value = useTeleport.value
    ? {
        position: 'fixed',
        top: `${rect.bottom + 4}px`,
        left: `${rect.left}px`,
        width: `${rect.width}px`,
        zIndex: 999999
      }
    : {}
}

watch(isOpen, (newVal) => {
  if (newVal) {
    nextTick(() => {
      updatePosition()
    })
    window.addEventListener('scroll', updatePosition, true)
    window.addEventListener('resize', updatePosition)
  } else {
    window.removeEventListener('scroll', updatePosition, true)
    window.removeEventListener('resize', updatePosition)
  }
})

onUnmounted(() => {
  window.removeEventListener('scroll', updatePosition, true)
  window.removeEventListener('resize', updatePosition)
})

function flattenVNodes(nodes) {
  return nodes.flatMap((node) => {
    if (!node || node.type === Comment) return []
    if (node.type === Fragment) return flattenVNodes(Array.isArray(node.children) ? node.children : [])
    return [node]
  })
}

function vnodeText(value) {
  if (typeof value === 'string' || typeof value === 'number') return String(value)
  if (!Array.isArray(value)) return ''
  return value.map((child) => {
    if (typeof child === 'string' || typeof child === 'number') return String(child)
    if (child?.type === Text) return String(child.children ?? '')
    return vnodeText(child?.children)
  }).join('')
}

const normalizedOptions = computed(() => {
  if (props.options && props.options.length) return props.options

  const defaultSlot = slots.default?.()
  if (!defaultSlot) return []

  return flattenVNodes(defaultSlot)
    .filter((node) => node && node.type === 'option')
    .map((node) => ({
      value: node.props?.value ?? '',
      label: vnodeText(node.children).trim(),
      disabled: node.props?.disabled === '' || node.props?.disabled === true,
    }))
})

const selectedOption = computed(() => {
  return normalizedOptions.value.find(opt => String(opt.value) === String(props.modelValue))
})

const toggleDropdown = () => {
  if (!props.disabled) {
    isOpen.value = !isOpen.value
  }
}

const selectOption = (option) => {
  if (option.disabled) return
  emit('update:modelValue', option.value)
  emit('change', option.value)
  isOpen.value = false
}

const selectId = computed(() => props.id || `lms-select-${generatedId}`)
</script>

<template>
  <div class="min-w-[180px] space-y-1 relative">
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
      ref="triggerRef"
      class="lg-input flex items-center justify-between cursor-pointer select-none px-4 py-2.5 text-sm transition-all duration-150"
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
        class="w-4 h-4 text-(--text-muted) transition-transform duration-150" 
        :class="{ 'rotate-180': isOpen }" 
      />
    </div>

    <!-- Floating Dropdown Teleported to Body -->
    <Teleport to="body" :disabled="!useTeleport">
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
          ref="menuRef"
          class="surface-dropdown border border-card rounded-xl shadow-2xl overflow-hidden py-1 backdrop-blur-md"
          :class="{ 'absolute left-0 right-0 z-50 w-full mt-1': !useTeleport }"
          :style="useTeleport ? dropdownStyle : {}"
        >
          <ul class="max-h-60 overflow-y-auto">
            <li 
              v-for="option in normalizedOptions"
              :key="option.value"
              @click="selectOption(option)"
              class="px-4 py-2.5 text-sm cursor-pointer flex items-center justify-between hover:bg-(--surface-table-row-hover) transition-colors"
              :class="{
                'bg-(--surface-table-row-hover) text-(--text-heading) font-semibold': String(option.value) === String(modelValue),
                'text-(--text-body)': String(option.value) !== String(modelValue),
                'cursor-not-allowed opacity-50': option.disabled
              }"
            >
              <span class="truncate flex-1 text-left">{{ option.label }}</span>
              <Check v-if="String(option.value) === String(modelValue)" class="w-4 h-4 text-link shrink-0 ml-2" />
            </li>
          </ul>
        </div>
      </Transition>
    </Teleport>

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
