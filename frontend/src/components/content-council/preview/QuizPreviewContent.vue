<script setup>
import { computed, ref } from 'vue'
import { HelpCircle, Clock, Target, CheckCircle } from 'lucide-vue-next'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})

const showAnswers = ref(false)

const questions = computed(() => Array.isArray(props.content?.questions) ? props.content.questions : [])
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-6">
      <div class="p-2 bg-green-50 text-green-600 rounded-lg shrink-0">
        <HelpCircle class="w-5 h-5" />
      </div>
      <div class="flex-1 min-w-0">
        <h3 class="text-lg font-bold text-slate-800">{{ content.title }}</h3>
        <p v-if="content.description" class="text-sm text-slate-600 mt-1">{{ content.description }}</p>
      </div>
      <div v-if="content.status === 'draft'" class="shrink-0">
        <span class="text-[10px] font-medium px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider">
          Bản nháp
        </span>
      </div>
    </div>

    <!-- Quiz Info Card -->
    <div class="bg-white border border-slate-200 rounded-xl overflow-hidden mb-6">
      <div class="grid grid-cols-2 sm:grid-cols-4 divide-x divide-y sm:divide-y-0 divide-slate-100">
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Thời gian</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <Clock class="w-4 h-4 text-blue-500" />
            <span>{{ content.duration || 15 }} phút</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Số câu hỏi</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <HelpCircle class="w-4 h-4 text-indigo-500" />
            <span>{{ content.questionCount || questions.length }} câu</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Điểm đạt</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <Target class="w-4 h-4 text-green-500" />
            <span>{{ content.passScore || 8 }}/10</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Quy tắc</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <CheckCircle class="w-4 h-4 text-orange-500" />
            <span>Phải đạt</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Council specific answers toggle -->
    <div class="flex items-center justify-between mb-4 bg-slate-50 p-3 rounded-lg border border-slate-200">
      <div class="flex items-center gap-2">
        <span class="text-xs font-bold px-2 py-1 bg-indigo-100 text-indigo-700 rounded uppercase">Quyền Hội Đồng</span>
        <span class="text-sm font-medium text-slate-600">Xem trước danh sách câu hỏi</span>
      </div>
      <label class="flex items-center gap-2 text-sm text-slate-700 cursor-pointer">
        <input 
          type="checkbox" 
          v-model="showAnswers"
          class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
        >
        Hiển thị đáp án đúng
      </label>
    </div>

    <!-- Questions Preview -->
    <div class="space-y-4">
      <div 
        v-for="(q, index) in questions"
        :key="q.id"
        class="bg-white border border-slate-200 rounded-xl p-5"
      >
        <h4 class="font-semibold text-slate-800 mb-4">
          <span class="text-slate-500 mr-2">Câu {{ index + 1 }}:</span>
          {{ q.text }}
          <span class="text-xs font-normal text-slate-400 ml-2">({{ q.type === 'single' ? 'Chọn 1' : 'Chọn nhiều' }})</span>
        </h4>
        
        <div class="space-y-2">
          <div 
            v-for="opt in q.options" 
            :key="opt.id"
            class="flex items-start gap-3 p-3 rounded-lg border transition-colors"
            :class="[
              showAnswers && opt.isCorrect 
                ? 'bg-green-50 border-green-200' 
                : 'bg-slate-50 border-slate-100'
            ]"
          >
            <div 
              class="w-5 h-5 rounded flex items-center justify-center text-xs font-medium shrink-0 mt-0.5"
              :class="[
                showAnswers && opt.isCorrect 
                  ? 'bg-green-500 text-white' 
                  : 'bg-white border border-slate-300 text-slate-500'
              ]"
            >
              {{ opt.id }}
            </div>
            <div class="text-sm" :class="showAnswers && opt.isCorrect ? 'text-green-800 font-medium' : 'text-slate-700'">
              {{ opt.text }}
            </div>
            <CheckCircle v-if="showAnswers && opt.isCorrect" class="w-4 h-4 text-green-600 ml-auto shrink-0 mt-0.5" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
