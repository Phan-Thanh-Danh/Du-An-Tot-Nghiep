<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import RewardDisciplineMockBanner from './RewardDisciplineMockBanner.vue'
import StudentRewardHeroCard from './StudentRewardHeroCard.vue'
import StudentRewardList from './StudentRewardList.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const rewards = ref([])
const loading = ref(false)

const fetchRewards = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getMyRewards()
    rewards.value = res.items || []
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchRewards())
</script>

<template>
  <div class="p-6 h-full bg-(--surface-page) overflow-y-auto custom-scrollbar">
    <RewardDisciplineMockBanner />
    <h1 class="text-2xl font-bold text-(--text-heading) mb-6">Khen thưởng & Thành tích</h1>
    
    <div class="space-y-8 max-w-5xl">
      <StudentRewardHeroCard 
        :rewardsCount="rewards.length" 
        :topRank="rewards[0]?.xepHang || 'N/A'" 
      />
      
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-6 animate-pulse">
        <GlassPanel v-for="i in 3" :key="i" class="h-48"></GlassPanel>
      </div>
      
      <StudentRewardList v-else :rewards="rewards" />
    </div>
  </div>
</template>
