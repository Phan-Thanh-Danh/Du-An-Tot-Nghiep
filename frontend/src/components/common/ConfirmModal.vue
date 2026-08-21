<script setup lang="ts">
import { AlertTriangle, AlertOctagon, Info, CheckCircle2, X } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  isOpen: boolean
  title?: string
  message: string
  variant?: 'warning' | 'danger' | 'info' | 'success'
  confirmText?: string
  cancelText?: string
  isAlert?: boolean
}>(), {
  title: 'Xác nhận',
  variant: 'warning',
  confirmText: 'Đồng ý',
  cancelText: 'Hủy',
  isAlert: false
})

const emit = defineEmits(['update:isOpen', 'confirm', 'cancel'])

const handleConfirm = () => {
  emit('confirm')
  emit('update:isOpen', false)
}

const handleCancel = () => {
  emit('cancel')
  emit('update:isOpen', false)
}
</script>

<template>
  <Teleport to="body">
    <div v-if="isOpen" class="relative z-[100]" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <!-- Backdrop -->
      <div 
        class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity"
        @click="handleCancel"
      ></div>

      <!-- Dialog Container -->
      <div class="fixed inset-0 z-10 overflow-y-auto">
        <div class="flex min-h-full items-center justify-center p-4 text-center">
          <div class="relative transform overflow-hidden rounded-2xl bg-white text-left shadow-2xl transition-all w-full max-w-md border border-slate-100">
            
            <!-- Close button -->
            <button 
              @click="handleCancel" 
              class="absolute top-4 right-4 text-slate-400 hover:text-slate-600 transition-colors p-1.5 hover:bg-slate-100 rounded-full"
            >
              <X class="w-4 h-4" />
            </button>

            <div class="bg-white px-6 pt-6 pb-4">
              <div class="flex items-start gap-4">
                <!-- Icon badge -->
                <div 
                  class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl"
                  :class="{
                    'bg-amber-50 text-amber-600 border border-amber-200/60': variant === 'warning',
                    'bg-red-50 text-red-600 border border-red-200/60': variant === 'danger',
                    'bg-blue-50 text-blue-600 border border-blue-200/60': variant === 'info',
                    'bg-green-50 text-green-600 border border-green-200/60': variant === 'success',
                  }"
                >
                  <AlertTriangle v-if="variant === 'warning'" class="h-6 w-6" />
                  <AlertOctagon v-else-if="variant === 'danger'" class="h-6 w-6" />
                  <Info v-else-if="variant === 'info'" class="h-6 w-6" />
                  <CheckCircle2 v-else-if="variant === 'success'" class="h-6 w-6" />
                </div>

                <div class="pt-0.5">
                  <h3 class="text-lg font-bold text-slate-800 leading-snug" id="modal-title">
                    {{ title }}
                  </h3>
                  <p class="mt-2 text-sm text-slate-600 leading-relaxed whitespace-pre-line">
                    {{ message }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Footer buttons -->
            <div class="bg-slate-50/80 px-6 py-4 border-t border-slate-100 flex items-center justify-end gap-3">
              <button 
                v-if="!isAlert"
                type="button" 
                class="px-4 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-200 rounded-xl hover:bg-slate-100 transition-colors shadow-sm"
                @click="handleCancel"
              >
                {{ cancelText }}
              </button>

              <button 
                type="button" 
                class="px-5 py-2 text-sm font-semibold text-white rounded-xl shadow-sm transition-colors"
                :class="{
                  'bg-amber-600 hover:bg-amber-700': variant === 'warning',
                  'bg-red-600 hover:bg-red-700': variant === 'danger',
                  'bg-blue-600 hover:bg-blue-700': variant === 'info',
                  'bg-green-600 hover:bg-green-700': variant === 'success',
                }"
                @click="handleConfirm"
              >
                {{ isAlert ? (confirmText || 'Đóng') : confirmText }}
              </button>
            </div>

          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
