<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  questionCount: number
  multipleChoiceCount: number
  essayCount: number
  currentScore: number
  configuredScore: number
}>()

const scoreDiff = computed(() => {
  return Number((props.currentScore - props.configuredScore).toFixed(2))
})

const progressPercentage = computed(() => {
  if (props.configuredScore === 0) return 0
  const p = (props.currentScore / props.configuredScore) * 100
  return Math.min(p, 100)
})

const isExactMatch = computed(() => scoreDiff.value === 0)
const isMissing = computed(() => scoreDiff.value < 0)
const isOver = computed(() => scoreDiff.value > 0)

</script>

<template>
  <div class="bg-white p-4 rounded-xl shadow-sm border border-slate-200">
    <div class="flex flex-col sm:flex-row justify-between gap-6">
      
      <!-- Left Stats -->
      <div class="flex-1 flex gap-6 sm:gap-12 text-sm">
        <div>
          <span class="block text-slate-500 mb-1">Số lượng câu hỏi</span>
          <span class="text-2xl font-bold text-slate-800">{{ questionCount }}</span>
          <span class="text-slate-500 ml-1">câu</span>
        </div>
        <div>
          <span class="block text-slate-500 mb-1">Phân loại</span>
          <div class="flex gap-4">
            <span class="font-medium text-slate-700">{{ multipleChoiceCount }} <span class="text-slate-400 font-normal">trắc nghiệm</span></span>
            <span class="font-medium text-slate-700">{{ essayCount }} <span class="text-slate-400 font-normal">tự luận</span></span>
          </div>
        </div>
      </div>

      <!-- Right Score Bar -->
      <div class="flex-1 max-w-sm">
        <div class="flex justify-between items-end mb-2">
          <div>
            <span class="block text-slate-500 text-sm mb-0.5">Tổng điểm hiện tại</span>
            <span class="text-2xl font-bold" :class="isExactMatch ? 'text-green-600' : isMissing ? 'text-amber-500' : 'text-red-500'">
              {{ currentScore }}<span class="text-base font-normal text-slate-500">/{{ configuredScore }}</span>
            </span>
          </div>
          <div class="text-sm font-medium pb-1" :class="isExactMatch ? 'text-green-600' : isMissing ? 'text-amber-600' : 'text-red-500'">
            <span v-if="isExactMatch">Khớp tổng điểm</span>
            <span v-else-if="isMissing">Thiếu {{ Math.abs(scoreDiff) }} điểm</span>
            <span v-else>Vượt {{ scoreDiff }} điểm</span>
          </div>
        </div>

        <div class="h-2 w-full bg-slate-100 rounded-full overflow-hidden" role="progressbar" :aria-valuenow="progressPercentage" aria-valuemin="0" aria-valuemax="100">
          <div 
            class="h-full transition-all duration-500" 
            :class="isExactMatch ? 'bg-green-500' : isMissing ? 'bg-amber-400' : 'bg-red-500'"
            :style="`width: ${progressPercentage}%`"
          ></div>
        </div>
      </div>

    </div>
  </div>
</template>
