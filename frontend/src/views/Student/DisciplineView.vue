<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import RewardDisciplineMockBanner from '@/components/reward-discipline/RewardDisciplineMockBanner.vue'
import StudentDisciplineList from '@/components/reward-discipline/StudentDisciplineList.vue'
import StudentDisciplineDetail from '@/components/reward-discipline/StudentDisciplineDetail.vue'
import StudentDisciplineAppealForm from '@/components/reward-discipline/StudentDisciplineAppealForm.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const records = ref([])
const loading = ref(false)
const selectedRecord = ref(null)
const isAppealing = ref(false)

const fetchRecords = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getMyDisciplineRecords()
    records.value = res.items || []
  } finally {
    loading.value = false
  }
}

const handleSelect = (r) => { selectedRecord.value = r; isAppealing.value = false }
const handleBack = () => { selectedRecord.value = null; isAppealing.value = false }
const handleAppeal = () => { isAppealing.value = true }

const submitAppeal = async (payload) => {
  await rewardDisciplineMockService.submitDisciplineAppeal(selectedRecord.value.id, payload)
  selectedRecord.value.appealStatus = 'pending'
  isAppealing.value = false
}

onMounted(() => fetchRecords())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <RewardDisciplineMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6" v-if="!selectedRecord">Hồ sơ kỷ luật</h1>
    
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-6 animate-pulse">
      <GlassPanel v-for="i in 3" :key="i" class="h-48"></GlassPanel>
    </div>
    
    <div v-else>
      <StudentDisciplineAppealForm 
        v-if="selectedRecord && isAppealing" 
        :record="selectedRecord" 
        @back="isAppealing = false" 
        @submit="submitAppeal" 
      />
      <StudentDisciplineDetail 
        v-else-if="selectedRecord" 
        :record="selectedRecord" 
        @back="handleBack" 
        @appeal="handleAppeal" 
      />
      <StudentDisciplineList 
        v-else 
        :records="records" 
        @select="handleSelect" 
      />
    </div>
  </div>
</template>
