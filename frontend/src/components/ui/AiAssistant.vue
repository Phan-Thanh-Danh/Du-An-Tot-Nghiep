<script setup>
import { ref, nextTick } from 'vue'
import * as LucideIcons from 'lucide-vue-next'

const open = ref(false)
const messages = ref([
  { role: 'bot', text: 'Xin chào! Tôi là trợ lý ảo của LMS. Bạn cần hỗ trợ gì?' },
])
const input = ref('')
const chatEndRef = ref(null)
const inputRef = ref(null)

const quickReplies = [
  'Hướng dẫn đăng ký môn học',
  'Cách xem thời khóa biểu',
  'Quy định học phí',
  'Liên hệ phòng giáo vụ',
]

const responses = {
  'hướng dẫn đăng ký môn học': 'Bạn vào mục **Đăng ký môn** trên sidebar, chọn kỳ học mong muốn và nhấn **Đăng ký**. Hạn đăng ký thường là 2 tuần đầu mỗi kỳ.',
  'cách xem thời khóa biểu': 'Vào **Thời khóa biểu** ở sidebar. Bạn có thể xem theo tuần hoặc theo tháng. Nhấn vào buổi học để xem chi tiết.',
  'quy định học phí': 'Học phí được tính theo số tín chỉ đăng ký. Vào **Học phí & Thanh toán** để xem chi tiết và thanh toán online.',
  'liên hệ phòng giáo vụ': 'Phòng Giáo vụ làm việc từ 7:30 - 17:00 các ngày trong tuần. Bạn có thể gửi ticket hỗ trợ qua mục **Hỗ trợ & Ticket**.',
}

function sendMessage(text) {
  const msg = text.trim()
  if (!msg) return
  messages.value.push({ role: 'user', text: msg })
  input.value = ''

  setTimeout(() => {
    const reply = generateReply(msg)
    messages.value.push({ role: 'bot', text: reply })
    scrollToEnd()
  }, 600)
  scrollToEnd()
}

function generateReply(msg) {
  const lower = msg.toLowerCase().trim()
  for (const [key, val] of Object.entries(responses)) {
    if (lower.includes(key)) return val
  }
  return 'Cảm ơn bạn đã hỏi! Để được hỗ trợ chi tiết hơn, vui lòng liên hệ Phòng Giáo vụ hoặc gửi ticket qua mục **Hỗ trợ & Ticket**.'
}

function handleQuickReply(reply) {
  sendMessage(reply)
}

function scrollToEnd() {
  nextTick(() => {
    chatEndRef.value?.scrollIntoView({ behavior: 'smooth' })
  })
}

function toggle() {
  open.value = !open.value
  if (open.value) {
    nextTick(() => inputRef.value?.focus())
  }
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-4 right-4 z-[150]">
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-4 scale-95"
      >
        <div
          v-if="open"
          class="absolute bottom-14 right-0 mb-2 w-[320px] origin-bottom-right overflow-hidden rounded-[20px] border border-white/60 dark:border-white/10 bg-white/92 dark:bg-slate-900/90 shadow-[0_30px_80px_rgba(15,23,42,0.2)] dark:shadow-[0_30px_80px_rgba(2,6,23,0.5)] backdrop-blur-2xl"
          @click.stop
        >
          <div class="flex items-center gap-2.5 border-b border-slate-100/50 dark:border-white/10 bg-gradient-to-r from-blue-600 to-cyan-600 px-3 py-2.5">
            <div class="flex h-7 w-7 items-center justify-center rounded-full bg-white/20">
              <LucideIcons.Bot :size="16" class="text-white" />
            </div>
            <div class="flex-1">
              <p class="text-xs font-bold text-white">Trợ lý ảo</p>
              <p class="text-[9px] font-medium text-white/70">Hỏi đáp nhanh về LMS</p>
            </div>
            <button class="flex h-6 w-6 items-center justify-center rounded-lg text-white/70 hover:bg-white/20 hover:text-white transition-colors" @click="toggle">
              <LucideIcons.X :size="13" />
            </button>
          </div>

          <div class="h-[280px] overflow-y-auto p-2.5 space-y-2.5">
            <div v-for="(msg, i) in messages" :key="i" class="flex" :class="msg.role === 'user' ? 'justify-end' : 'justify-start'">
              <div
                class="max-w-[85%] rounded-2xl px-3 py-2 text-[10px] leading-relaxed"
                :class="msg.role === 'user'
                  ? 'bg-blue-600 text-white rounded-br-md'
                  : 'bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-200 rounded-bl-md'"
              >
                {{ msg.text }}
              </div>
            </div>

            <div v-if="messages.length === 1" class="flex flex-wrap gap-1 pt-1">
              <button
                v-for="reply in quickReplies"
                :key="reply"
                class="rounded-full border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-2.5 py-1 text-[9px] font-semibold text-slate-600 dark:text-slate-400 hover:bg-blue-50 dark:hover:bg-blue-600/20 hover:text-blue-700 dark:hover:text-blue-300 hover:border-blue-200 dark:hover:border-blue-500/30 transition-all"
                @click="handleQuickReply(reply)"
              >
                {{ reply }}
              </button>
            </div>
            <div ref="chatEndRef" />
          </div>

          <div class="border-t border-slate-100/50 dark:border-white/10 p-2.5">
            <div class="flex items-center gap-1.5 rounded-xl border border-slate-200 dark:border-white/10 bg-white/60 dark:bg-slate-800/60 px-2.5 py-1.5 focus-within:border-blue-400 dark:focus-within:border-blue-500/50 focus-within:ring-2 focus-within:ring-blue-500/15 transition-all">
              <input
                ref="inputRef"
                v-model="input"
                type="text"
                placeholder="Nhập câu hỏi..."
                class="flex-1 bg-transparent text-[10px] text-slate-700 dark:text-slate-200 outline-none placeholder:text-slate-400 dark:placeholder:text-slate-500"
                @keydown.enter="sendMessage(input)"
              />
              <button
                class="flex h-6 w-6 items-center justify-center rounded-lg bg-blue-600 text-white hover:bg-blue-700 transition-colors disabled:opacity-40"
                :disabled="!input.trim()"
                @click="sendMessage(input)"
              >
                <LucideIcons.Send :size="10" />
              </button>
            </div>
          </div>
        </div>
      </Transition>

      <button
        class="relative flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-cyan-600 text-white shadow-xl shadow-blue-500/30 hover:scale-105 active:scale-95 transition-all duration-200 focus:outline-none"
        aria-label="Trợ lý ảo"
        @click="toggle"
      >
        <LucideIcons.MessageCircle :size="18" v-if="!open" />
        <LucideIcons.X :size="16" v-else />
      </button>
    </div>
  </Teleport>
</template>
