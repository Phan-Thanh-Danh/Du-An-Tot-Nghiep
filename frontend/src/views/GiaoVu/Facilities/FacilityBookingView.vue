<script setup>
import { ref, onMounted } from 'vue'
import { Loader2, AlertCircle, Building, Calendar, Clock, CheckCircle2 } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'


const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')
const booking = ref(false)
const rooms = ref([])
const bookings = ref([])

const form = ref({
  roomId: '',
  date: '',
  startTime: '',
  endTime: '',
  purpose: '',
})

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

async function handleBook() {
  if (!form.value.roomId || !form.value.date || !form.value.startTime || !form.value.endTime) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ thông tin đặt phòng.')
    return
  }

  booking.value = true
  try {
    await staffApi.bookRoom({ ...form.value })
    popupStore.success('Đặt phòng thành công', `Phòng đã được đặt cho ${form.value.date}.`)
    bookings.value.unshift({
      id: 'B-' + Date.now(),
      roomName: rooms.value.find(r => r.id === form.value.roomId)?.name || form.value.roomId,
      date: form.value.date,
      time: `${form.value.startTime}-${form.value.endTime}`,
      purpose: form.value.purpose,
      status: 'pending',
    })
    form.value = { roomId: '', date: '', startTime: '', endTime: '', purpose: '' }
  } catch (e) {
    console.error(e)
  } finally {
    booking.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="facility-booking max-w-7xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Đặt phòng học</h1>
      <p class="text-(--text-body) mt-1">Đặt lịch sử dụng phòng học và cơ sở vật chất.</p>
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
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div class="lg-card-glass p-6 rounded-2xl space-y-5">
          <h3 class="text-lg font-semibold text-heading flex items-center gap-2">
            <Calendar :size="20" /> Đặt phòng mới
          </h3>

          <div>
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Chọn phòng</label>
            <select v-model="form.roomId" class="lg-input w-full px-4 py-2.5 text-sm font-bold">
              <option value="" disabled>-- Chọn phòng --</option>
              <option v-for="room in rooms" :key="room.id" :value="room.id">{{ room.name }} ({{ room.building }})</option>
            </select>
          </div>

          <div>
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Ngày</label>
            <input v-model="form.date" type="date" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Giờ bắt đầu</label>
              <input v-model="form.startTime" type="time" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
            </div>
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Giờ kết thúc</label>
              <input v-model="form.endTime" type="time" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
            </div>
          </div>

          <div>
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Mục đích</label>
            <input v-model="form.purpose" type="text" class="lg-input w-full px-4 py-2.5 text-sm font-bold" placeholder="VD: Thi cuối kỳ, Học bù..." />
          </div>

          <div class="flex items-center justify-end pt-2">
            <button
              :class="['lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2', booking ? 'opacity-45 pointer-events-none' : '']"
              :disabled="booking"
              @click="handleBook"
            >
              <span v-if="booking" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <CheckCircle2 v-else :size="16" />
              {{ booking ? 'Đang xử lý...' : 'Đặt phòng' }}
            </button>
          </div>
        </div>

        <div class="lg-card-glass p-6 rounded-2xl space-y-4">
          <h3 class="text-lg font-semibold text-heading flex items-center gap-2">
            <Clock :size="20" /> Lịch đặt gần đây
          </h3>

          <div v-if="bookings.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
            <Building :size="32" class="text-placeholder mb-2" />
            <p class="text-sm text-label">Chưa có lịch đặt nào.</p>
          </div>

          <div v-for="b in bookings" :key="b.id" class="surface-solid p-4 rounded-2xl flex items-center justify-between gap-4">
            <div>
              <p class="text-sm font-semibold text-heading">{{ b.roomName }}</p>
              <p class="text-xs text-label mt-1">{{ b.date }} · {{ b.time }}</p>
              <p class="text-xs text-placeholder mt-0.5">{{ b.purpose }}</p>
            </div>
            <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest',
              b.status === 'confirmed' ? 'bg-(--color-success-bg) text-(--lg-success)' : 'bg-(--color-warning-bg) text-(--lg-warning)']">
              {{ b.status === 'confirmed' ? 'Xác nhận' : 'Chờ duyệt' }}
            </span>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
