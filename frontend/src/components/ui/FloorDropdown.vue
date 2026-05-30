<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { ChevronDown } from 'lucide-vue-next'

const props = defineProps({
  modelValue: {
    type: String,
    default: 'all',
  },
  label: {
    type: String,
    default: 'Lầu',
  },
  floors: {
    type: Array,
    default: () => ['1', '2', '3', '4', '5'],
  },
  allLabel: {
    type: String,
    default: 'Tất cả lầu',
  },
  disabled: Boolean,
})

const emit = defineEmits(['update:modelValue'])

const open = ref(false)
const triggerRef = ref(null)
const menuStyle = ref({})

const displayOptions = computed(() =>
  props.floors.map(f => ({
    value: f,
    label: `Lầu ${f}`,
  }))
)

const selectedLabel = computed(() => {
  if (props.modelValue === 'all') return props.allLabel
  const found = displayOptions.value.find(o => o.value === props.modelValue)
  return found ? found.label : props.modelValue
})

function toggle() {
  if (props.disabled) return
  open.value = !open.value
  if (!open.value) return
  nextTick(() => positionMenu())
}

function positionMenu() {
  if (!triggerRef.value) return
  const rect = triggerRef.value.getBoundingClientRect()
  menuStyle.value = {
    position: 'fixed',
    left: rect.left + 'px',
    top: rect.bottom + 6 + 'px',
    width: rect.width + 'px',
    minWidth: '140px',
    zIndex: 99999,
  }
}

function select(value) {
  emit('update:modelValue', value)
  open.value = false
}

function onDocumentClick(e) {
  if (!open.value) return
  if (triggerRef.value && !triggerRef.value.contains(e.target)) {
    open.value = false
  }
}

function onScroll() {
  if (open.value) positionMenu()
}

function onResize() {
  if (open.value) positionMenu()
}

onMounted(() => {
  document.addEventListener('click', onDocumentClick, true)
  document.addEventListener('scroll', onScroll, true)
  window.addEventListener('resize', onResize)
})

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick, true)
  document.removeEventListener('scroll', onScroll, true)
  window.removeEventListener('resize', onResize)
})
</script>

<template>
  <div ref="triggerRef" class="relative">
    <button
      type="button"
      :disabled="disabled"
      class="lg-input flex w-full items-center justify-between gap-2 px-4 py-2.5 text-sm font-bold text-left disabled:opacity-45 disabled:cursor-not-allowed"
      @click="toggle"
    >
      <span :class="modelValue === 'all' ? 'text-placeholder' : 'text-heading'">{{ selectedLabel }}</span>
      <ChevronDown
        :size="16"
        :class="open ? 'rotate-180' : ''"
        class="text-placeholder transition-transform duration-200 flex-shrink-0"
      />
    </button>

    <Teleport to="body">
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-2 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-2 scale-95"
      >
        <div
          v-if="open"
          :style="menuStyle"
          class="overflow-hidden rounded-[20px] border border-white/60 dark:border-white/10 bg-white/90 dark:bg-slate-900/85 p-1.5 shadow-[0_20px_50px_rgba(15,23,42,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <button
            class="flex w-full items-center rounded-xl px-3.5 py-2.5 text-sm font-bold text-left transition-all hover:bg-blue-50 dark:hover:bg-blue-600/15 hover:text-blue-700 dark:hover:text-blue-300"
            :class="modelValue === 'all' ? 'bg-blue-50 dark:bg-blue-600/15 text-blue-700 dark:text-blue-300' : 'text-slate-700 dark:text-slate-300'"
            role="menuitem"
            @click="select('all')"
          >
            {{ allLabel }}
          </button>
          <div class="my-0.5 border-t border-slate-100 dark:border-white/5"></div>
          <button
            v-for="opt in displayOptions"
            :key="opt.value"
            class="flex w-full items-center rounded-xl px-3.5 py-2.5 text-sm font-bold text-left transition-all hover:bg-blue-50 dark:hover:bg-blue-600/15 hover:text-blue-700 dark:hover:text-blue-300"
            :class="modelValue === opt.value ? 'bg-blue-50 dark:bg-blue-600/15 text-blue-700 dark:text-blue-300' : 'text-slate-700 dark:text-slate-300'"
            role="menuitem"
            @click="select(opt.value)"
          >
            {{ opt.label }}
          </button>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>
