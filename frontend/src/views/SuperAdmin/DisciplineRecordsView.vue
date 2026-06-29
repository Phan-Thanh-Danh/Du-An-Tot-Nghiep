<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import RewardDisciplineMockBanner from '@/components/reward-discipline/RewardDisciplineMockBanner.vue'
import DisciplineRecordTable from '@/components/reward-discipline/DisciplineRecordTable.vue'
import DisciplineRecordDetail from '@/components/reward-discipline/DisciplineRecordDetail.vue'

const records = ref([])
const loading = ref(false)
const selectedRecord = ref(null)

const fetchRecords = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getDisciplineRecords()
    records.value = res.items || []
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchRecords())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <RewardDisciplineMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6" v-if="!selectedRecord">Quản lý Hồ sơ Kỷ luật</h1>
    
    <div v-if="loading" class="animate-pulse bg-[var(--surface-card)] h-64 rounded-xl"></div>
    <div v-else>
      <DisciplineRecordDetail 
        v-if="selectedRecord" 
        :record="selectedRecord" 
        @back="selectedRecord = null" 
      />
      <DisciplineRecordTable 
        v-else 
        :records="records" 
        @select="r => selectedRecord = r" 
      />
    </div>
  </div>
</template>
