<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import RewardDisciplineMockBanner from '@/components/reward-discipline/RewardDisciplineMockBanner.vue'
import RewardCampaignTable from '@/components/reward-discipline/RewardCampaignTable.vue'
import RewardCampaignDetail from '@/components/reward-discipline/RewardCampaignDetail.vue'

const campaigns = ref([])
const loading = ref(false)
const selectedCampaign = ref(null)

const fetchCampaigns = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getRewardCampaigns()
    campaigns.value = res.items || []
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchCampaigns())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <RewardDisciplineMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6" v-if="!selectedCampaign">Quản lý Đợt khen thưởng</h1>
    
    <div v-if="loading" class="animate-pulse bg-[var(--surface-card)] h-64 rounded-xl"></div>
    <div v-else>
      <RewardCampaignDetail 
        v-if="selectedCampaign" 
        :campaign="selectedCampaign" 
        @back="selectedCampaign = null" 
      />
      <RewardCampaignTable 
        v-else 
        :campaigns="campaigns" 
        @select="c => selectedCampaign = c" 
      />
    </div>
  </div>
</template>
