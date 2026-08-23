<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue'
import {
  Bot,
  Sparkles,
  Send,
  X,
  RotateCcw,
  MessageCircle,
  Copy,
  Check,
  Maximize2,
  Minimize2,
  AlertCircle,
  Loader2,
  BookOpen,
  Brain,
  ChevronDown,
  Zap
} from 'lucide-vue-next'
import { aiApi } from '@/services/aiApi'
import { useAuthStore } from '@/stores/auth'
import { useAiAssistant } from '@/composables/useAiAssistant'

const authStore = useAuthStore()
const { isOpen, pendingPrompt, currentContext, toggle, close } = useAiAssistant()

// ── State ───────────────────────────────────────────────────
const conversationId = ref(null)
const input = ref('')
const isLoading = ref(false)
const isExpanded = ref(false)
const copiedIndex = ref(null)
const chatContainerRef = ref(null)
const textareaRef = ref(null)

const initialGreeting = {
  id: 'msg-0',
  role: 'bot',
  text: 'Xin chào! Tôi là trợ lý ảo học thuật của AET LMS. Tôi có thể giúp bạn giải đáp kiến thức bài học, quy chế học vụ và phương pháp ôn tập. Bạn cần hỗ trợ gì hôm nay?',
  timestamp: new Date(),
}

const messages = ref([{ ...initialGreeting }])

// ── Quick Prompts per Role ──────────────────────────────────
const rolePrompts = computed(() => {
  const role = authStore.user?.role || 'Student'

  if (role === 'Student' || role === 'hoc_sinh') {
    return [
      { label: '📚 Đăng ký môn học', prompt: 'Hãy hướng dẫn tôi các bước và quy định đăng ký môn học trong kỳ.' },
      { label: '⏱️ Quy chế điểm danh', prompt: 'Quy định về tỷ lệ chuyên cần và số buổi vắng tối đa cho phép là bao nhiêu?' },
      { label: '📊 Tính điểm GPA & Phúc khảo', prompt: 'Quy trình phúc khảo điểm số và công thức tính điểm trung bình học kỳ như thế nào?' },
      { label: '🔄 Quy chế học lại / thi lại', prompt: 'Khi nào sinh viên phải học lại hoặc được quyền đăng ký thi lại?' },
      { label: '🎫 Gửi đơn từ & Ticket hỗ trợ', prompt: 'Làm thế nào để gửi đơn xin nghỉ phép hoặc tạo ticket hỗ trợ học vụ?' },
    ]
  }

  if (role === 'Teacher' || role === 'giao_vien') {
    return [
      { label: '📝 Quy trình nhập & sửa điểm', prompt: 'Quy định và thời hạn nhập điểm thành phần cho lớp học phần là gì?' },
      { label: '💡 Gợi ý câu hỏi trắc nghiệm', prompt: 'Hãy gợi ý cách xây dựng ngân hàng câu hỏi trắc nghiệm phân hóa tốt cho sinh viên.' },
      { label: '🔓 Mở khóa điểm danh', prompt: 'Quy trình gửi yêu cầu mở khóa buổi điểm danh quá hạn cho phòng Giáo vụ.' },
    ]
  }

  if (role === 'AcademicStaff' || role === 'nhan_vien') {
    return [
      { label: '🏢 Tiêu chuẩn xếp phòng & ca', prompt: 'Các nguyên tắc phân bổ phòng học và tránh trùng lịch ca học.' },
      { label: '📑 Xử lý đơn từ học sinh', prompt: 'Quy trình thẩm định và phê duyệt các loại đơn học vụ phổ biến.' },
    ]
  }

  if (role === 'HoiDongQuanLyNoiDung') {
    return [
      { label: '📋 Rà soát đề cương Syllabus', prompt: 'Tiêu chuẩn rà soát chuẩn đầu ra (CLO) và ma trận liên kết bài học môn học.' },
    ]
  }

  return [
    { label: '⚙️ Kiểm tra hệ thống AI', prompt: 'Trạng thái mô hình AI local và khả năng hỗ trợ học vụ.' },
    { label: '❓ Hướng dẫn sử dụng LMS', prompt: 'Các chức năng chính của hệ thống AET LMS.' },
  ]
})

// ── Markdown Parser Helper ─────────────────────────────────
function renderMarkdown(rawText) {
  if (!rawText) return ''
  let html = rawText
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')

  // Code blocks ```...```
  html = html.replace(/```([\s\S]*?)```/g, (_, code) => {
    return `<pre class="my-2 p-2.5 rounded-xl bg-slate-900 text-slate-100 text-[11px] font-mono overflow-x-auto border border-white/10"><code>${code.trim()}</code></pre>`
  })

  // Inline code `...`
  html = html.replace(/`([^`]+)`/g, '<code class="px-1.5 py-0.5 rounded-md bg-blue-50 dark:bg-blue-950/60 text-blue-600 dark:text-blue-300 font-mono text-[11px] border border-blue-200/40 dark:border-blue-800/40">$1</code>')

  // Bold **...**
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong class="font-bold text-slate-900 dark:text-white">$1</strong>')

  // Italic *...*
  html = html.replace(/\*([^*]+)\*/g, '<em class="italic">$1</em>')

  // Bullet points
  html = html.replace(/^[*-]\s+(.*)$/gm, '<li class="ml-4 list-disc my-0.5">$1</li>')

  // Numbered lists
  html = html.replace(/^\d+\.\s+(.*)$/gm, '<li class="ml-4 list-decimal my-0.5">$1</li>')

  // Wrap list items
  html = html.replace(/(<li.*<\/li>)/s, '<ul class="my-1.5 space-y-0.5">$1</ul>')

  // Paragraphs / Linebreaks
  html = html.replace(/\n\n/g, '<br/><br/>').replace(/\n/g, '<br/>')

  return html
}

// ── Message Actions ────────────────────────────────────────
async function sendMessage(text) {
  const content = (text ?? input.value).trim()
  if (!content || isLoading.value) return

  const userMsgId = 'msg-' + Date.now()
  messages.value.push({
    id: userMsgId,
    role: 'user',
    text: content,
    timestamp: new Date(),
  })

  input.value = ''
  isLoading.value = true
  scrollToEnd()

  try {
    const payload = {
      message: content,
      conversationId: conversationId.value,
      courseId: currentContext.value?.courseId,
      lessonId: currentContext.value?.lessonId,
    }

    const res = await aiApi.chat(payload)
    if (res?.conversationId) {
      conversationId.value = res.conversationId
    }

    messages.value.push({
      id: 'bot-' + Date.now(),
      role: 'bot',
      text: res?.answer || 'Đã nhận phản hồi từ AI.',
      thinking: res?.thinking || null,
      showThinking: false,
      processingTimeMs: res?.processingTimeMs || null,
      model: res?.model,
      timestamp: new Date(),
    })
  } catch (err) {
    const errorMessage = err?.message || 'Không thể kết nối tới dịch vụ AI lúc này. Vui lòng thử lại sau.'
    messages.value.push({
      id: 'bot-err-' + Date.now(),
      role: 'bot',
      text: errorMessage,
      isError: true,
      timestamp: new Date(),
    })
  } finally {
    isLoading.value = false
    scrollToEnd()
  }
}

function handleQuickPrompt(promptText) {
  sendMessage(promptText)
}

function resetChat() {
  conversationId.value = null
  messages.value = [{ ...initialGreeting, id: 'msg-' + Date.now(), timestamp: new Date() }]
  scrollToEnd()
}

async function copyMessage(text, idx) {
  try {
    await navigator.clipboard.writeText(text)
    copiedIndex.value = idx
    setTimeout(() => {
      copiedIndex.value = null
    }, 2000)
  } catch {
    // Ignore copy error
  }
}

function scrollToEnd() {
  nextTick(() => {
    if (chatContainerRef.value) {
      chatContainerRef.value.scrollTop = chatContainerRef.value.scrollHeight
    }
  })
}

function handleKeydown(e) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    sendMessage(input.value)
  }
}

// ── Watch External Triggers ────────────────────────────────
watch(pendingPrompt, (newPrompt) => {
  if (newPrompt && newPrompt.trim()) {
    nextTick(() => {
      sendMessage(newPrompt)
      pendingPrompt.value = ''
    })
  }
})

watch(isOpen, (open) => {
  if (open) {
    scrollToEnd()
    nextTick(() => textareaRef.value?.focus())
  }
})
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-5 right-5 z-[150] font-sans">
      <!-- ── POPUP CHAT PANEL ────────────────────────────────── -->
      <Transition
        enter-active-class="transition-all duration-300 cubic-bezier(0.16, 1, 0.3, 1)"
        enter-from-class="opacity-0 translate-y-6 scale-90"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-6 scale-90"
      >
        <div
          v-if="isOpen"
          class="absolute bottom-16 right-0 mb-2 flex flex-col origin-bottom-right overflow-hidden rounded-[24px] border border-white/40 dark:border-white/10 bg-white/95 dark:bg-slate-900/95 shadow-[0_25px_60px_-15px_rgba(15,23,42,0.3)] dark:shadow-[0_25px_60px_-15px_rgba(0,0,0,0.7)] backdrop-blur-2xl transition-all duration-200"
          :class="isExpanded ? 'w-[440px] h-[640px]' : 'w-[360px] h-[520px]'"
          @click.stop
        >
          <!-- Top Header -->
          <div class="relative flex items-center justify-between border-b border-white/20 dark:border-white/10 bg-gradient-to-r from-blue-600 via-indigo-600 to-cyan-600 px-4 py-3 text-white shadow-sm">
            <div class="flex items-center gap-2.5">
              <div class="relative flex h-8 w-8 items-center justify-center rounded-xl bg-white/20 shadow-inner backdrop-blur-md">
                <Bot :size="18" class="text-white" />
                <span class="absolute -bottom-0.5 -right-0.5 h-2.5 w-2.5 rounded-full border-2 border-indigo-600 bg-emerald-400"></span>
              </div>
              <div>
                <div class="flex items-center gap-1.5">
                  <h3 class="text-xs font-bold leading-none tracking-wide text-white">Trợ lý AI AET</h3>
                  <span class="rounded-full bg-white/25 px-1.5 py-0.2 text-[9px] font-semibold text-white/90">Trực tuyến</span>
                </div>
                <p class="mt-0.5 text-[10px] text-white/80 font-medium">Hỗ trợ học tập & quy chế trực tuyến</p>
              </div>
            </div>

            <div class="flex items-center gap-1">
              <!-- Reset chat -->
              <button
                class="flex h-7 w-7 items-center justify-center rounded-lg text-white/80 hover:bg-white/20 hover:text-white transition-colors"
                title="Làm mới cuộc trò chuyện"
                @click="resetChat"
              >
                <RotateCcw :size="13" />
              </button>

              <!-- Expand/Collapse -->
              <button
                class="flex h-7 w-7 items-center justify-center rounded-lg text-white/80 hover:bg-white/20 hover:text-white transition-colors"
                :title="isExpanded ? 'Thu nhỏ' : 'Mở rộng'"
                @click="isExpanded = !isExpanded"
              >
                <Minimize2 :size="13" v-if="isExpanded" />
                <Maximize2 :size="13" v-else />
              </button>

              <!-- Close -->
              <button
                class="flex h-7 w-7 items-center justify-center rounded-lg text-white/80 hover:bg-white/20 hover:text-white transition-colors"
                title="Đóng cửa sổ"
                @click="close"
              >
                <X :size="15" />
              </button>
            </div>
          </div>

          <!-- Messages Container -->
          <div
            ref="chatContainerRef"
            class="flex-1 overflow-y-auto p-3.5 space-y-3.5 bg-slate-50/50 dark:bg-slate-950/40 text-slate-800 dark:text-slate-100"
          >
            <div
              v-for="(msg, idx) in messages"
              :key="msg.id || idx"
              class="flex flex-col"
              :class="msg.role === 'user' ? 'items-end' : 'items-start'"
            >
              <!-- Message Wrapper -->
              <div class="group relative flex max-w-[88%] gap-2" :class="msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'">
                <!-- Bot Avatar -->
                <div
                  v-if="msg.role === 'bot'"
                  class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-lg bg-gradient-to-tr from-blue-600 to-cyan-500 text-white shadow-sm mt-0.5"
                >
                  <Bot :size="13" />
                </div>

                <!-- Bubble Content -->
                <div
                  class="rounded-2xl px-3.5 py-2.5 text-[12px] leading-relaxed shadow-sm transition-all"
                  :class="[
                    msg.role === 'user'
                      ? 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-br-xs'
                      : msg.isError
                        ? 'bg-rose-50 dark:bg-rose-950/50 text-rose-800 dark:text-rose-200 border border-rose-200 dark:border-rose-900/50 rounded-bl-xs'
                        : 'bg-white dark:bg-slate-800/90 text-slate-800 dark:text-slate-100 border border-slate-200/60 dark:border-white/10 rounded-bl-xs'
                  ]"
                >
                  <div v-if="msg.isError" class="flex items-center gap-1.5 font-semibold text-rose-600 dark:text-rose-400 mb-1">
                    <AlertCircle :size="13" />
                    <span>Lỗi phản hồi</span>
                  </div>

                  <!-- Collapsible Thinking Process like ChatGPT / DeepSeek / Gemini -->
                  <div v-if="msg.thinking" class="mb-2 border-b border-slate-100 dark:border-slate-700/60 pb-2">
                    <button
                      type="button"
                      class="inline-flex items-center gap-1.5 px-2 py-1 rounded-md text-[11px] font-medium text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 bg-slate-100/90 dark:bg-slate-800/80 hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
                      @click="msg.showThinking = !msg.showThinking"
                    >
                      <Brain :size="12" class="text-indigo-500" />
                      <span>{{ msg.showThinking ? 'Thu gọn quá trình suy nghĩ' : '💭 Đã suy nghĩ (nhấn để xem)' }}</span>
                      <ChevronDown :size="12" class="transition-transform duration-200" :class="msg.showThinking ? 'rotate-180' : ''" />
                    </button>

                    <div
                      v-if="msg.showThinking"
                      class="mt-1.5 max-h-48 overflow-y-auto rounded-lg bg-slate-50 dark:bg-slate-900/90 p-2.5 text-[10.5px] leading-relaxed text-slate-600 dark:text-slate-300 font-mono whitespace-pre-wrap border border-slate-200/70 dark:border-slate-800 shadow-inner"
                    >
                      {{ msg.thinking }}
                    </div>
                  </div>

                  <!-- Bot markdown or User text -->
                  <div
                    v-if="msg.role === 'bot'"
                    class="prose prose-xs dark:prose-invert max-w-none text-[12px] leading-relaxed break-words"
                    v-html="renderMarkdown(msg.text)"
                  />
                  <div v-else class="whitespace-pre-wrap break-words">
                    {{ msg.text }}
                  </div>

                  <!-- Latency Badge -->
                  <div
                    v-if="msg.role === 'bot' && !msg.isError && msg.processingTimeMs"
                    class="mt-1.5 flex items-center gap-2 text-[9.5px] text-slate-400 dark:text-slate-500"
                  >
                    <span class="flex items-center gap-1">
                      <Zap :size="10" class="text-amber-500" />
                      <span>Phản hồi trong {{ (msg.processingTimeMs / 1000).toFixed(1) }}s</span>
                    </span>
                  </div>
                </div>

                <!-- Copy button on hover for bot -->
                <button
                  v-if="msg.role === 'bot' && !msg.isError"
                  class="opacity-0 group-hover:opacity-100 transition-opacity self-center p-1 rounded-md text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 hover:bg-slate-200/50 dark:hover:bg-slate-800"
                  title="Sao chép câu trả lời"
                  @click="copyMessage(msg.text, idx)"
                >
                  <Check :size="12" class="text-emerald-500" v-if="copiedIndex === idx" />
                  <Copy :size="12" v-else />
                </button>
              </div>
            </div>

            <!-- Typing Indicator -->
            <div v-if="isLoading" class="flex items-start gap-2 max-w-[88%]">
              <div class="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-lg bg-gradient-to-tr from-blue-600 to-cyan-500 text-white shadow-sm mt-0.5">
                <Bot :size="13" />
              </div>
              <div class="flex items-center gap-2 rounded-2xl rounded-bl-xs border border-slate-200/60 dark:border-white/10 bg-white dark:bg-slate-800 px-3.5 py-2.5 shadow-sm">
                <Brain :size="13" class="text-indigo-500 animate-pulse" />
                <span class="text-[11px] text-slate-500 dark:text-slate-400 font-medium">Đang suy nghĩ & xử lý...</span>
                <span class="flex items-center gap-0.5 text-blue-600 dark:text-blue-400">
                  <span class="inline-block h-1.5 w-1.5 animate-bounce rounded-full bg-current" style="animation-delay: 0ms"></span>
                  <span class="inline-block h-1.5 w-1.5 animate-bounce rounded-full bg-current" style="animation-delay: 150ms"></span>
                  <span class="inline-block h-1.5 w-1.5 animate-bounce rounded-full bg-current" style="animation-delay: 300ms"></span>
                </span>
              </div>
            </div>

            <!-- Quick Suggestions (when conversation is fresh) -->
            <div v-if="messages.length <= 2 && !isLoading" class="pt-2">
              <p class="mb-1.5 text-[10px] font-semibold uppercase tracking-wider text-slate-600 dark:text-slate-300 flex items-center gap-1">
                <Sparkles :size="11" class="text-amber-500" />
                Gợi ý câu hỏi nhanh:
              </p>
              <div class="flex flex-wrap gap-1.5">
                <button
                  v-for="(item, i) in rolePrompts"
                  :key="i"
                  class="rounded-xl border border-slate-200 dark:border-white/10 bg-white/80 dark:bg-slate-800/80 px-2.5 py-1.5 text-[11px] font-medium text-slate-700 dark:text-slate-300 shadow-xs hover:border-blue-400 dark:hover:border-blue-500 hover:bg-blue-50/70 dark:hover:bg-blue-950/40 hover:text-blue-700 dark:hover:text-blue-300 transition-all text-left"
                  @click="handleQuickPrompt(item.prompt)"
                >
                  {{ item.label }}
                </button>
              </div>
            </div>
          </div>

          <!-- Bottom Input Area -->
          <div class="border-t border-slate-200/60 dark:border-white/10 bg-white/90 dark:bg-slate-900/90 p-2.5">
            <div class="flex items-end gap-1.5 rounded-2xl border border-slate-200 dark:border-white/10 bg-slate-50 dark:bg-slate-950/60 px-3 py-2 focus-within:border-blue-500 focus-within:ring-2 focus-within:ring-blue-500/20 transition-all">
              <textarea
                ref="textareaRef"
                v-model="input"
                rows="1"
                placeholder="Nhập câu hỏi tự do cho AI (Enter để gửi)..."
                class="max-h-24 min-h-[22px] flex-1 resize-none bg-transparent text-[12px] text-slate-800 dark:text-slate-100 outline-none placeholder:text-slate-400 dark:placeholder:text-slate-500 leading-relaxed"
                :disabled="isLoading"
                @keydown="handleKeydown"
              />

              <button
                class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-xl bg-gradient-to-tr from-blue-600 to-indigo-600 text-white shadow-sm hover:opacity-90 active:scale-95 disabled:opacity-40 disabled:pointer-events-none transition-all"
                :disabled="!input.trim() || isLoading"
                title="Gửi câu hỏi"
                @click="sendMessage(input)"
              >
                <Loader2 v-if="isLoading" :size="13" class="animate-spin" />
                <Send v-else :size="12" />
              </button>
            </div>
            <p class="mt-1 text-center text-[9px] text-slate-500 dark:text-slate-400">
              AI có thể mắc lỗi. Vui lòng kiểm tra lại các thông tin học vụ quan trọng.
            </p>
          </div>
        </div>
      </Transition>

      <!-- ── FLOATING TRIGGER BUTTON ──────────────────────────── -->
      <button
        class="group relative flex h-12 w-12 items-center justify-center rounded-full bg-gradient-to-tr from-blue-600 via-indigo-600 to-cyan-500 text-white shadow-[0_10px_25px_-5px_rgba(37,99,235,0.5)] hover:scale-108 active:scale-95 transition-all duration-300 focus:outline-none"
        aria-label="Trợ lý ảo AI"
        @click="toggle"
      >
        <span class="absolute -inset-0.5 rounded-full bg-gradient-to-tr from-blue-400 to-cyan-300 opacity-0 group-hover:opacity-40 blur-sm transition-opacity"></span>
        <MessageCircle :size="20" v-if="!isOpen" class="relative transition-transform duration-200 group-hover:scale-110" />
        <X :size="18" v-else class="relative transition-transform duration-200 group-hover:rotate-90" />
      </button>
    </div>
  </Teleport>
</template>
