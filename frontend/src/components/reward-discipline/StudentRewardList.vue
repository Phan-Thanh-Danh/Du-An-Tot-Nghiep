<script setup>
import StudentRewardCard from './StudentRewardCard.vue'

defineProps({
  rewards: { type: Array, default: () => [] }
})
</script>

<template>
  <div>
    <h3 class="text-lg font-bold text-(--text-heading) mb-4">Danh sách khen thưởng</h3>
    
    <div v-if="rewards.length === 0" class="text-center py-12 bg-(--surface-card) rounded-xl border border-(--border-card)">
      <div class="text-(--text-muted)">Bạn chưa có khen thưởng nào. Hãy tiếp tục cố gắng trong học kỳ này!</div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <StudentRewardCard v-for="r in rewards" :key="r.id" :reward="r" />
    </div>
  </div>
</template>
