<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import RewardDisciplineMockBanner from '@/components/reward-discipline/RewardDisciplineMockBanner.vue'
import DisciplineAppealTable from '@/components/reward-discipline/DisciplineAppealTable.vue'

const appeals = ref([])
const loading = ref(false)

const fetchAppeals = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getDisciplineAppeals()
    appeals.value = res.items || []
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchAppeals())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <RewardDisciplineMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Quản lý Khiếu nại kỷ luật</h1>
    
    <div v-if="loading" class="animate-pulse bg-[var(--surface-card)] h-64 rounded-xl"></div>
    <div v-else>
      <DisciplineAppealTable :appeals="appeals" />
    </div>
  </div>
</template>
