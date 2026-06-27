<script setup>
import { nextTick, onUnmounted, ref, watch } from 'vue'
import { AlertTriangle, ArrowRight, LogOut } from 'lucide-vue-next'

const props = defineProps({
  show: { type: Boolean, default: false },
  expectedLabel: { type: String, default: '' },
  actualLabel: { type: String, default: '' },
  dashboardPath: { type: String, default: '' },
})

const emit = defineEmits(['go-to-dashboard', 'use-different-account'])

const modalRef = ref(null)
const previouslyFocused = ref(null)

const FOCUSABLE_SELECTOR =
  'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'

function getFocusable(el) {
  if (!el) return []
  return Array.from(el.querySelectorAll(FOCUSABLE_SELECTOR)).filter(
    (node) => !node.hasAttribute('disabled') && node.offsetParent !== null,
  )
}

function trapFocus(e) {
  if (e.key === 'Escape') {
    e.preventDefault()
    emit('use-different-account')
    return
  }

  if (e.key !== 'Tab') return

  const focusable = getFocusable(modalRef.value)
  if (focusable.length === 0) return

  const first = focusable[0]
  const last = focusable[focusable.length - 1]

  if (e.shiftKey) {
    if (document.activeElement === first) {
      e.preventDefault()
      last.focus()
    }
  } else {
    if (document.activeElement === last) {
      e.preventDefault()
      first.focus()
    }
  }
}

watch(
  () => props.show,
  async (isOpen) => {
    if (isOpen) {
      previouslyFocused.value = document.activeElement
      document.body.style.overflow = 'hidden'

      await nextTick()
      const focusable = getFocusable(modalRef.value)
      if (focusable.length > 0) {
        focusable[0].focus()
      }

      document.addEventListener('keydown', trapFocus)
    } else {
      document.body.style.overflow = ''
      document.removeEventListener('keydown', trapFocus)

      if (previouslyFocused.value?.focus) {
        previouslyFocused.value.focus()
      }
      previouslyFocused.value = null
    }
  },
)

onUnmounted(() => {
  document.body.style.overflow = ''
  document.removeEventListener('keydown', trapFocus)
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="show"
        ref="modalRef"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
        aria-labelledby="mismatch-title"
        aria-describedby="mismatch-description"
      >
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md bg-white rounded-2xl shadow-2xl overflow-hidden">
          <div class="p-6 space-y-4">
            <div class="flex items-center gap-3">
              <div class="w-12 h-12 rounded-full bg-[#ffedd5] flex items-center justify-center flex-shrink-0">
                <AlertTriangle class="w-6 h-6 text-[#c2410c]" aria-hidden="true" />
              </div>
              <div>
                <h2 id="mismatch-title" class="text-[17px] font-bold text-[#191c1e]">
                  Không gian không khớp
                </h2>
                <p id="mismatch-description" class="text-[13px] text-[#585f67] mt-0.5">
                  Tài khoản của bạn không thuộc không gian <strong>{{ expectedLabel }}</strong>.
                </p>
              </div>
            </div>

            <div class="bg-[#f8fafc] rounded-xl p-4 border border-[#e2e8f0]">
              <p class="text-[13px] text-[#444651]">
                Tài khoản này thuộc vai trò
                <span class="font-semibold text-[#191c1e]">{{ actualLabel }}</span>.
                Bạn có thể truy cập không gian phù hợp hoặc dùng tài khoản khác.
              </p>
            </div>

            <div class="flex flex-col gap-2 pt-2">
              <button
                type="button"
                class="w-full py-3 px-4 rounded-xl font-semibold text-[14px] text-white transition-all duration-300 flex items-center justify-center gap-2"
                :style="{
                  background: 'linear-gradient(90deg, #1d4ed8 0%, #2563eb 52%, #0891b2 100%)',
                }"
                @click="$emit('go-to-dashboard')"
              >
                <ArrowRight class="w-4 h-4" aria-hidden="true" />
                <span>Đi tới không gian {{ actualLabel }}</span>
              </button>

              <button
                type="button"
                class="w-full py-3 px-4 rounded-xl font-semibold text-[14px] text-[#585f67] bg-[#f1f5f9] hover:bg-[#e2e8f0] transition-colors flex items-center justify-center gap-2"
                @click="$emit('use-different-account')"
              >
                <LogOut class="w-4 h-4" aria-hidden="true" />
                <span>Dùng tài khoản khác</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
