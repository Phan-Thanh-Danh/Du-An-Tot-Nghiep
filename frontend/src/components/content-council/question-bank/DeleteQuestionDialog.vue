<script setup lang="ts">
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { AlertTriangle, Trash2, X } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
  questionData: QuestionBankItem | null
}>()

const emit = defineEmits(['update:isOpen', 'confirm'])

const close = () => {
  emit('update:isOpen', false)
}

const confirm = () => {
  if (props.questionData?.usageCount === 0) {
    emit('confirm', props.questionData)
  }
}
</script>

<template>
  <div v-if="isOpen" class="relative z-[100]" aria-labelledby="modal-title" role="dialog" aria-modal="true">
    <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm transition-opacity" @click="close"></div>

    <div class="fixed inset-0 z-10 overflow-y-auto">
      <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
        <div class="relative transform overflow-hidden rounded-xl bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg">
          
          <div class="bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
            <div class="sm:flex sm:items-start">
              <div 
                class="mx-auto flex h-12 w-12 shrink-0 items-center justify-center rounded-full sm:mx-0 sm:h-10 sm:w-10"
                :class="questionData?.usageCount === 0 ? 'bg-red-100' : 'bg-amber-100'"
              >
                <Trash2 v-if="questionData?.usageCount === 0" class="h-6 w-6 text-red-600" aria-hidden="true" />
                <AlertTriangle v-else class="h-6 w-6 text-amber-600" aria-hidden="true" />
              </div>
              <div class="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left">
                <h3 class="text-lg font-bold leading-6 text-slate-900" id="modal-title">
                  {{ questionData?.usageCount === 0 ? 'Xóa câu hỏi?' : 'Không thể xóa câu hỏi' }}
                </h3>
                <div class="mt-2">
                  <p v-if="questionData?.usageCount === 0" class="text-sm text-slate-500">
                    Câu hỏi <span class="font-bold text-slate-800">{{ questionData.code }}</span> sẽ bị xóa vĩnh viễn khỏi ngân hàng câu hỏi. Bạn không thể hoàn tác hành động này.
                  </p>
                  <p v-else class="text-sm text-slate-500">
                    Câu hỏi <span class="font-bold text-slate-800">{{ questionData?.code }}</span> đang được sử dụng trong <span class="font-bold text-slate-800">{{ questionData?.usageCount }} Quiz</span>. Hãy sử dụng chức năng <strong>Vô hiệu hóa</strong> để ngăn câu hỏi xuất hiện trong các Quiz mới thay vì xóa.
                  </p>
                </div>
              </div>
            </div>
          </div>
          
          <div class="bg-slate-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
            <template v-if="questionData?.usageCount === 0">
              <button 
                type="button" 
                class="inline-flex w-full justify-center rounded-lg bg-red-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 sm:ml-3 sm:w-auto"
                @click="confirm"
              >
                Xóa câu hỏi
              </button>
              <button 
                type="button" 
                class="mt-3 inline-flex w-full justify-center rounded-lg bg-white px-3 py-2 text-sm font-semibold text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 hover:bg-slate-50 sm:mt-0 sm:w-auto"
                @click="close"
              >
                Hủy
              </button>
            </template>
            <template v-else>
              <button 
                type="button" 
                class="inline-flex w-full justify-center rounded-lg bg-white px-3 py-2 text-sm font-semibold text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 hover:bg-slate-50 sm:mt-0 sm:w-auto"
                @click="close"
              >
                Đóng
              </button>
            </template>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>
