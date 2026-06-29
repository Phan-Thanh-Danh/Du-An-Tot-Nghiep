<script setup>
import { ref, onMounted } from 'vue'
import ApplicationList from './ApplicationList.vue'
import ApplicationDetailPanel from './ApplicationDetailPanel.vue'
import { applicationsApi } from '@/services/applicationsApi'

const mode = ref('list') // list, detail, create, edit
const applications = ref([])
const loading = ref(false)
const selectedApp = ref(null)
const currentSchema = ref([])

const fetchApps = async () => {
  loading.value = true
  try {
    const res = await applicationsApi.getMyApplications({ pageSize: 50 })
    applications.value = res.items || []
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const handleSelect = async (app) => {
  selectedApp.value = app
  mode.value = 'detail'
  try {
    // Fetch schema if available
    if (app.mauDonId) {
      const tpl = await applicationsApi.getApplicationTemplateDetail(app.mauDonId)
      currentSchema.value = JSON.parse(tpl.cauHinhForm || '[]')
    }
  } catch { console.error('Failed to parse schema') }
}

onMounted(() => {
  fetchApps()
})
</script>

<template>
  <div class="h-full bg-[var(--surface-page)]">
    <!-- List Mode -->
    <div v-if="mode === 'list'" class="p-6 h-full flex flex-col">
      <div class="mb-6 flex justify-between items-end">
        <div>
          <h1 class="text-2xl font-bold text-[var(--text-heading)]">Đơn từ của tôi</h1>
          <p class="text-[var(--text-muted)] mt-1">Quản lý và theo dõi tiến độ xử lý đơn từ</p>
        </div>
      </div>
      <ApplicationList 
        :applications="applications" 
        :loading="loading" 
        @select="handleSelect" 
        @create="mode = 'create'" 
      />
    </div>

    <!-- Detail Mode -->
    <div v-else-if="mode === 'detail'" class="p-6 h-full">
      <ApplicationDetailPanel 
        :application="selectedApp"
        :schema="currentSchema"
        @back="mode = 'list'"
      />
    </div>

    <!-- Create/Edit Mode Placeholder -->
    <div v-else class="p-6 h-full flex justify-center items-center">
      <div class="text-center text-[var(--text-muted)]">
        <p>Giao diện {{ mode === 'create' ? 'Tạo đơn mới' : 'Chỉnh sửa đơn' }} đang được hoàn thiện...</p>
        <button class="mt-4 text-[var(--lg-primary)] hover:underline" @click="mode = 'list'">Quay lại</button>
      </div>
    </div>
  </div>
</template>
