<script setup>
import { ref, computed, watch } from 'vue'
import { Sparkles, X, Wand2, CheckCircle2, Loader2, ArrowRight, Clock } from 'lucide-vue-next'
import { aiApi } from '@/services/aiApi'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  templateId: { type: [Number, String], default: 0 },
  templateName: { type: String, default: 'Mẫu Giấy Khen' },
  currentHtml: { type: String, default: '' },
  currentCss: { type: String, default: '' }
})

const emit = defineEmits(['close', 'apply'])

const instruction = ref('')
const loading = ref(false)
const error = ref(null)
const result = ref(null)
const elapsedSeconds = ref(0)
const latestDesign = ref(null)
let requestVersion = 0
watch(() => props.isOpen, () => {
  requestVersion++
  result.value = null
  latestDesign.value = null
  error.value = null
})
watch(instruction, () => { requestVersion++; result.value = null })
const previewDoc = computed(() => result.value
  ? `<!doctype html><html><head><meta charset="utf-8"><style>${result.value.updatedCss}</style></head><body>${result.value.updatedHtml}</body></html>`
  : '')

async function handleGenerate() {
  if (!instruction.value.trim() || loading.value) return
  const version = ++requestVersion
  const base = latestDesign.value
  loading.value = true
  error.value = null
  result.value = null
  const startTime = Date.now()
  try {
    const res = await aiApi.editCertificateTemplate({
      templateId: Number(props.templateId) || 0,
      instruction: instruction.value.trim(),
      currentHtml: base?.updatedHtml ?? props.currentHtml,
      currentCss: base?.updatedCss ?? props.currentCss,
      mode: 'fast'
    })
    elapsedSeconds.value = Number(((Date.now() - startTime) / 1000).toFixed(1))
    if (version !== requestVersion || !props.isOpen) return
    result.value = res
    latestDesign.value = res
  } catch (err) {
    if (version === requestVersion) error.value = err.message || 'Không thể tạo bản thiết kế bằng AI. Vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}

function handleApply() {
  if (!result.value) return
  emit('apply', {
    updatedHtml: result.value.updatedHtml,
    updatedCss: result.value.updatedCss
  })
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <!-- Backdrop -->
        <div class="fixed inset-0 bg-black/60 backdrop-blur-sm" @click="emit('close')"></div>

        <!-- Modal Container -->
        <div class="relative w-full max-w-2xl lg-glass rounded-2xl border border-white/10 shadow-2xl flex flex-col overflow-hidden max-h-[90vh]">
          <!-- Header -->
          <div class="p-5 border-b border-(--border-default) flex items-center justify-between bg-gradient-to-r from-blue-600/10 via-indigo-600/10 to-transparent">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-indigo-500/30">
                <Wand2 :size="20" />
              </div>
              <div>
                <div class="flex items-center gap-2">
                  <h3 class="text-base font-bold text-heading">AI Trợ Lý Thiết Kế Mẫu Bằng Khen</h3>
                  <span class="px-2 py-0.5 text-[10px] font-extrabold tracking-wider uppercase rounded-full bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border border-emerald-500/30">
                    Phản hồi nhanh
                  </span>
                </div>
                <p class="text-xs text-muted mt-0.5">
                  Đang hỗ trợ chỉnh sửa: <span class="font-semibold text-heading">{{ templateName }}</span> (Mã #{{ templateId || 'Mới' }})
                </p>
              </div>
            </div>
            <button @click="emit('close')" class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input-hover) transition-colors cursor-pointer">
              <X :size="20" />
            </button>
          </div>

          <!-- Body -->
          <div class="p-5 overflow-y-auto space-y-4 text-sm flex-1">
            <!-- Instruction Input -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-wider mb-1.5">
                Nhập yêu cầu thiết kế tùy biến bằng ngôn ngữ tự nhiên:
              </label>
              <textarea
                v-model="instruction"
                rows="4"
                placeholder="Nhập bất kỳ ý tưởng thiết kế, màu sắc, phong cách (ví dụ: viền xanh dương chủ đề biển cả và con thuyền buồm, hoặc phong cách công nghệ tím neon, hoặc tối giản hiện đại...)"
                class="w-full px-3.5 py-2.5 rounded-xl border border-(--border-input) bg-(--surface-input) text-body placeholder:text-placeholder focus:outline-none focus:ring-2 focus:ring-(--lg-primary) text-xs resize-none"
              ></textarea>
            </div>

            <!-- Generate Button -->
            <div class="pt-2">
              <button
                @click="handleGenerate"
                :disabled="loading || !instruction.trim()"
                class="w-full py-2.5 rounded-xl bg-gradient-to-r from-blue-600 via-indigo-600 to-indigo-700 hover:from-blue-700 hover:to-indigo-800 text-white font-bold text-xs shadow-lg shadow-indigo-500/20 flex items-center justify-center gap-2 transition-all active:scale-[0.99] disabled:opacity-50 cursor-pointer"
              >
                <Sparkles v-if="!loading" :size="16" />
                <Loader2 v-else :size="16" class="animate-spin" />
                <span>{{ loading ? 'AI ĐANG THIẾT KẾ...' : 'THỰC THI CHỈNH SỬA BẰNG AI' }}</span>
              </button>
            </div>

            <!-- Error State -->
            <div v-if="error" class="p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs">
              {{ error }}
            </div>

            <!-- Result State -->
            <div v-if="result" class="p-4 rounded-xl surface-card border border-emerald-500/30 space-y-3 animate-fade-in">
              <div class="flex items-center justify-between text-xs">
                <div class="flex items-center gap-2 text-emerald-400 font-bold">
                  <CheckCircle2 :size="16" />
                  <span>Bản thiết kế mới đã sẵn sàng!</span>
                </div>
                <div class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 font-semibold">
                  <Clock :size="13" />
                  <span>Tốc độ phản hồi: {{ result.responseTimeSeconds || elapsedSeconds }}s</span>
                </div>
              </div>
              <p class="text-xs text-muted">{{ result.explanation }}</p>
              <iframe
                :srcdoc="previewDoc"
                sandbox=""
                title="Xem trước thiết kế AI"
                class="w-full h-80 border border-default rounded-lg surface-card"
              />
              <p class="text-xs text-label">Bạn có thể yêu cầu sửa tiếp trên bản xem trước này hoặc bấm áp dụng để đưa vào trình biên tập.</p>

              <div class="space-y-1 pt-1">
                <p class="text-[11px] font-bold text-heading">Các điểm đã cải tiến:</p>
                <ul class="space-y-1">
                  <li v-for="(change, idx) in result.changesSummary" :key="idx" class="text-xs text-body flex items-start gap-1.5">
                    <span class="text-emerald-400 font-bold">•</span>
                    <span>{{ change }}</span>
                  </li>
                </ul>
              </div>

              <button
                @click="handleApply"
                type="button"
                class="w-full py-2.5 mt-2 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-bold text-xs shadow-md shadow-emerald-500/20 flex items-center justify-center gap-2 transition-all cursor-pointer"
              >
                <span>ÁP DỤNG VÀO MẪU GIẤY KHEN</span>
                <ArrowRight :size="15" />
              </button>
            </div>
          </div>

          <!-- Footer -->
          <div class="p-4 border-t border-(--border-default) flex justify-end">
            <button @click="emit('close')" class="lg-button-secondary px-4 py-2 text-xs font-semibold">
              Đóng
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
