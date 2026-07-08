<script setup>
import { ref, onMounted } from 'vue'
import { Search, Loader2, AlertCircle, Building, MapPin, Users } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'

const loading = ref(true)
const apiError = ref('')
const rooms = ref([])

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getRooms()
    rooms.value = Array.isArray(res) ? res : res?.items ?? res ?? []
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="facility-list max-w-7xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Danh sách cơ sở vật chất</h1>
      <p class="text-(--text-body) mt-1">Quản lý phòng học và cơ sở vật chất trong trường.</p>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-muted" :size="28" />
      <p class="text-sm text-muted">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
      <p class="text-xs text-muted">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="lg-glass-strong p-4 rounded-2xl">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) max-w-md">
          <Search :size="16" />
          <input type="text" placeholder="Tìm theo tên phòng hoặc tòa nhà..." class="bg-transparent border-none outline-none w-full" />
        </label>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="room in rooms" :key="room.id" class="lg-card-glass p-5 rounded-2xl hover:border-(--border-input-focus) transition-all">
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
                <Building :size="20" />
              </div>
              <div>
                <h3 class="text-base font-semibold text-heading">{{ room.name }}</h3>
                <p class="text-[11px] font-bold text-placeholder">{{ room.id }}</p>
              </div>
            </div>
            <span :class="['px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase tracking-widest',
              room.status === 'available' ? 'bg-(--color-success-bg) text-(--lg-success)' : 'bg-(--color-warning-bg) text-(--lg-warning)']">
              {{ room.status === 'available' ? 'Sẵn sàng' : 'Bảo trì' }}
            </span>
          </div>
          <div class="flex items-center gap-4 text-xs text-label">
            <span class="flex items-center gap-1.5">
              <MapPin :size="14" class="text-placeholder" /> {{ room.building }} - Tầng {{ room.floor }}
            </span>
            <span class="flex items-center gap-1.5">
              <Users :size="14" class="text-placeholder" /> {{ room.capacity }} SV
            </span>
          </div>
        </div>
      </div>

      <div v-if="rooms.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
        <div class="h-14 w-14 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
          <Building :size="24" class="text-placeholder" />
        </div>
        <p class="text-base font-semibold text-heading">Không có phòng học nào</p>
        <p class="text-sm text-label mt-1">Chưa có dữ liệu cơ sở vật chất.</p>
      </div>
    </template>
  </div>
</template>
