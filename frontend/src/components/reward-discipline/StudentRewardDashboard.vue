<script setup>
import { ref, onMounted } from 'vue'
import { studentApi } from '@/services/studentApi'
import { unwrapApiData } from '@/services/apiClient'
import StudentRewardHeroCard from './StudentRewardHeroCard.vue'
import StudentRewardList from './StudentRewardList.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const rewards = ref([])
const loading = ref(false)
const error = ref('')

const mapReward = (item) => ({
  id: item.maKhenThuong ?? item.MaKhenThuong,
  tieuDe: item.danhHieuSnapshot ?? item.DanhHieuSnapshot ?? item.tenLoaiKhenThuong ?? item.TenLoaiKhenThuong ?? 'Khen thưởng',
  loaiKhenThuong: item.tenLoaiKhenThuong ?? item.TenLoaiKhenThuong ?? item.loaiKhenThuong ?? item.LoaiKhenThuong ?? 'Khen thưởng',
  hocKy: item.tenHocKySnapshot ?? item.TenHocKySnapshot ?? 'Chưa có học kỳ',
  xepHang: item.xepHang ?? item.XepHang ?? 'N/A',
  certificateStatus: (item.hasCertificate ?? item.HasCertificate) ? 'generated' : 'pending',
})

const fetchRewards = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await studentApi.getRewards({ pageIndex: 1, pageSize: 20 })
    const data = unwrapApiData(res)
    rewards.value = (data?.items ?? data?.Items ?? []).map(mapReward)
  } catch (err) {
    rewards.value = []
    error.value = err?.message || 'Không thể tải dữ liệu khen thưởng.'
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchRewards())
</script>

<template>
  <div class="p-6 h-full bg-(--surface-page) overflow-y-auto custom-scrollbar">
    <h1 class="text-2xl font-bold text-(--text-heading) mb-6">Khen thưởng & Thành tích</h1>
    
    <div class="space-y-8 max-w-5xl">
      <StudentRewardHeroCard 
        :rewardsCount="rewards.length" 
        :topRank="rewards[0]?.xepHang || 'N/A'" 
      />
      
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-6 animate-pulse">
        <GlassPanel v-for="i in 3" :key="i" class="h-48"></GlassPanel>
      </div>
      
      <div v-else-if="error" class="rounded-lg border border-(--border-default) bg-(--surface-card) p-4 text-sm text-(--color-danger-text)">
        {{ error }}
      </div>

      <StudentRewardList v-else :rewards="rewards" />
    </div>
  </div>
</template>
