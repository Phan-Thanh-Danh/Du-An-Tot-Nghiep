<script setup>
import MockBanner from '@/components/ui/MockBanner.vue'
import { ref, onMounted } from 'vue'
import AdminApplicationFilters from './AdminApplicationFilters.vue'
import AdminApplicationTable from './AdminApplicationTable.vue'
import AdminApplicationDetail from './AdminApplicationDetail.vue'
import { applicationsApi } from '@/mocks/applicationsMockService'

const applications = ref([])
const loading = ref(false)
const selectedApp = ref(null)

const fetchApps = async (query = {}) => {
  loading.value = true
  try {
    const res = await applicationsApi.getAdminApplications({ pageSize: 50, ...query })
    applications.value = res.items || []
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const handleFilter = (filters) => {
  fetchApps(filters)
}

onMounted(() => fetchApps())
</script>

<template>
  <div class="h-full bg-[var(--surface-page)]">
    <MockBanner />
    <div v-if="!selectedApp" class="p-6 h-full flex flex-col">
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-[var(--text-heading)]">Hàng đợi đơn từ</h1>
        <p class="text-[var(--text-muted)] mt-1">Quản lý và tiếp nhận các đơn từ được gửi lên</p>
      </div>

      <AdminApplicationFilters @filter="handleFilter" />
      <AdminApplicationTable 
        :applications="applications" 
        :loading="loading" 
        @select="selectedApp = $event" 
      />
    </div>
    
    <div v-else class="p-6 h-full">
      <AdminApplicationDetail :application="selectedApp" @back="selectedApp = null; fetchApps()" />
    </div>
  </div>
</template>
