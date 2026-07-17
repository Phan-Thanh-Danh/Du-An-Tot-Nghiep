<script setup>
import { ref, computed, onMounted } from 'vue'
import { Building, Calendar, Clock, CheckCircle2, ShieldAlert, AlertTriangle, RefreshCw } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'

const popupStore = usePopupStore()
const loading = ref(true)
const forbidden = ref(false)
const error = ref(null)
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

function mapBooking(b) {
  return {
    id: b.id ?? b.Id,
    roomId: b.roomId ?? b.RoomId,
    roomName: b.roomName ?? b.RoomName ?? `Phòng ${b.roomId ?? b.RoomId}`,
    startTime: b.startTime ?? b.StartTime,
    endTime: b.endTime ?? b.EndTime,
    purpose: b.purpose ?? b.Purpose ?? 'Đặt phòng',
    status: b.status ?? b.Status ?? 'cho_duyet',
    requesterName: b.requesterName ?? b.RequesterName ?? 'Giáo vụ',
  }
}

async function loadData() {
  loading.value = true
  error.value = null
  forbidden.value = false
  try {
    const [roomsRes, bookingsRes] = await Promise.all([
      staffApi.getRooms({ pageSize: 100 }),
      staffApi.getBookings()
    ])
    
    const rRaw = roomsRes?.items ?? roomsRes?.data ?? roomsRes ?? []
    rooms.value = Array.isArray(rRaw) ? rRaw : (rRaw.items ?? [])
    
    const bRaw = bookingsRes?.items ?? bookingsRes?.data ?? bookingsRes ?? []
    const bList = Array.isArray(bRaw) ? bRaw : (bRaw.items ?? bRaw.Data ?? [])
    bookings.value = bList.map(mapBooking)
  } catch (err) {
    console.error(err)
    if (err?.statusCode === 403) {
      forbidden.value = true
    } else {
      error.value = err?.message || 'Không thể tải danh sách phòng và lịch đặt.'
    }
  } finally {
    loading.value = false
  }
}

const hasOverlap = computed(() => {
  if (!form.value.roomId || !form.value.date || !form.value.startTime || !form.value.endTime) return false
  
  const newStart = new Date(`${form.value.date}T${form.value.startTime}`)
  const newEnd = new Date(`${form.value.date}T${form.value.endTime}`)
  
  if (newStart >= newEnd) return false
  
  return bookings.value.some(b => {
    if (b.status === 'da_huy' || Number(b.roomId) !== Number(form.value.roomId)) return false
    
    const bStart = new Date(b.startTime)
    const bEnd = new Date(b.endTime)
    
    return newStart < bEnd && newEnd > bStart
  })
})

async function handleBook() {
  if (!form.value.roomId || !form.value.date || !form.value.startTime || !form.value.endTime) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ thông tin đặt phòng.')
    return
  }
  
  const newStart = new Date(`${form.value.date}T${form.value.startTime}`)
  const newEnd = new Date(`${form.value.date}T${form.value.endTime}`)
  
  if (newStart >= newEnd) {
    popupStore.warning('Thời gian không hợp lệ', 'Giờ bắt đầu phải trước giờ kết thúc.')
    return
  }

  if (hasOverlap.value) {
    popupStore.error('Xung đột lịch đặt', 'Phòng này đã có lịch đặt trong khoảng thời gian bạn chọn.')
    return
  }

  booking.value = true
  try {
    const payload = {
      roomId: Number(form.value.roomId),
      purpose: form.value.purpose || 'Đặt phòng học vụ',
      startTime: newStart.toISOString(),
      endTime: newEnd.toISOString(),
      attendees: 30,
      maDonVi: 1,
      requesterId: 1
    }
    await staffApi.bookRoom(payload)
    popupStore.success('Đặt phòng thành công', `Phòng đã được đặt và đang chờ duyệt.`)
    form.value = { roomId: '', date: '', startTime: '', endTime: '', purpose: '' }
    await loadData()
  } catch (err) {
    console.error(err)
    popupStore.error('Thao tác thất bại', err?.message || 'Không thể tạo lịch đặt phòng.')
  } finally {
    booking.value = false
  }
}

function formatBookingTime(startTime, endTime) {
  if (!startTime || !endTime) return ''
  const start = new Date(startTime)
  const end = new Date(endTime)
  const pad = (n) => String(n).padStart(2, '0')
  return `${pad(start.getHours())}:${pad(start.getMinutes())} - ${pad(end.getHours())}:${pad(end.getMinutes())}`
}

function formatBookingDate(startTime) {
  if (!startTime) return ''
  const d = new Date(startTime)
  const pad = (n) => String(n).padStart(2, '0')
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="facility-booking max-w-7xl mx-auto space-y-6 p-4 md:p-6">
    <!-- Page Header -->
    <div class="border-b border-(--border-default) pb-4">
      <div class="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-(--text-link) mb-1">
        <Building :size="14"/>
        Cơ sở vật chất
      </div>
      <h1 class="text-3xl font-extrabold text-(--text-heading) tracking-tight">Đặt phòng học</h1>
      <p class="text-sm text-(--text-muted) mt-1">Đăng ký và quản lý việc đặt phòng học, phòng thi, phòng thực hành.</p>
    </div>

    <!-- 5 States Container -->
    <div v-if="loading" class="space-y-4 py-8">
      <LoadingSkeleton :lines="5" />
    </div>

    <!-- Forbidden State -->
    <div v-else-if="forbidden" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
      <ShieldAlert :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
      <h3 class="text-lg font-bold text-(--text-heading)">Không có quyền truy cập</h3>
      <p class="text-sm text-(--text-muted) mt-1">Tài khoản của bạn không được phân quyền sử dụng chức năng đặt phòng học.</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
      <AlertTriangle :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
      <h3 class="text-lg font-bold text-(--text-heading)">Đã xảy ra lỗi</h3>
      <p class="text-sm text-(--text-muted) mt-1 mb-4">{{ error }}</p>
      <GlassButton variant="secondary" @click="loadData">
        <template #leading><RefreshCw :size="14" /></template>
        Thử lại
      </GlassButton>
    </div>

    <!-- Success State -->
    <template v-else>
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Booking Form -->
        <GlassPanel variant="flat" class="p-6 flex flex-col justify-between h-full border border-(--border-default)">
          <div class="space-y-5">
            <h3 class="text-lg font-bold text-(--text-heading) flex items-center gap-2 pb-2 border-b border-(--border-default)">
              <Calendar :size="20" class="text-blue-500" /> Đặt phòng mới
            </h3>

            <!-- Room Select -->
            <div>
              <label class="text-[10px] font-black text-(--text-muted) uppercase tracking-widest mb-1.5 block">Chọn phòng học</label>
              <select v-model="form.roomId" class="text-xs bg-(--surface-input) border border-(--border-input) text-(--text-body) rounded-lg w-full px-3 py-2.5 outline-none focus:ring-1 focus:ring-(--border-focus)">
                <option value="" disabled>-- Chọn phòng trống --</option>
                <option v-for="room in rooms" :key="room.maPhong" :value="room.maPhong">
                  {{ room.tenPhong || room.maCodePhong }} (Cơ sở: {{ room.maDonVi }}) - Sức chứa: {{ room.sucChua || 0 }}
                </option>
              </select>
            </div>

            <!-- Date picker -->
            <div>
              <label class="text-[10px] font-black text-(--text-muted) uppercase tracking-widest mb-1.5 block">Ngày đặt</label>
              <GlassInput v-slot="scope" class="w-full">
                <input v-model="form.date" v-bind="scope" type="date" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
              </GlassInput>
            </div>

            <!-- Time pickers -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[10px] font-black text-(--text-muted) uppercase tracking-widest mb-1.5 block">Giờ bắt đầu</label>
                <GlassInput v-slot="scope" class="w-full">
                  <input v-model="form.startTime" v-bind="scope" type="time" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
                </GlassInput>
              </div>
              <div>
                <label class="text-[10px] font-black text-(--text-muted) uppercase tracking-widest mb-1.5 block">Giờ kết thúc</label>
                <GlassInput v-slot="scope" class="w-full">
                  <input v-model="form.endTime" v-bind="scope" type="time" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
                </GlassInput>
              </div>
            </div>

            <!-- Purpose -->
            <div>
              <label class="text-[10px] font-black text-(--text-muted) uppercase tracking-widest mb-1.5 block">Mục đích sử dụng</label>
              <GlassInput v-model="form.purpose" placeholder="VD: Tổ chức thi cuối kỳ, học bù môn Java..." class="w-full" />
            </div>

            <!-- Overlap Warning -->
            <div v-if="hasOverlap" class="p-3 bg-red-500/10 text-red-600 dark:text-red-400 rounded-lg text-xs font-semibold flex items-start gap-2 border border-red-500/20">
              <AlertTriangle :size="16" class="shrink-0 mt-0.5" />
              <span>Xung đột lịch: Phòng học đã được đặt bởi người khác trong khoảng thời gian này. Vui lòng chọn giờ hoặc phòng khác.</span>
            </div>
          </div>

          <div class="flex items-center justify-end pt-6 border-t border-(--border-default) mt-6">
            <GlassButton
              variant="primary"
              :loading="booking"
              :disabled="booking || hasOverlap"
              @click="handleBook"
            >
              <template #leading><CheckCircle2 :size="16" /></template>
              Đặt phòng học
            </GlassButton>
          </div>
        </GlassPanel>

        <!-- Recent Bookings List -->
        <GlassPanel variant="flat" class="p-6 flex flex-col h-full border border-(--border-default)">
          <h3 class="text-lg font-bold text-(--text-heading) flex items-center gap-2 pb-2 border-b border-(--border-default) mb-4">
            <Clock :size="20" class="text-amber-500" /> Lịch đặt gần đây
          </h3>

          <div v-if="bookings.length === 0" class="flex flex-col items-center justify-center py-20 text-center flex-1">
            <Building :size="36" class="text-placeholder mb-2 opacity-50" />
            <p class="text-sm font-bold text-(--text-heading)">Chưa có lịch đặt nào</p>
            <p class="text-xs text-(--text-muted) mt-1">Các yêu cầu đặt phòng của hệ thống sẽ xuất hiện tại đây.</p>
          </div>

          <div v-else class="space-y-3 overflow-y-auto max-h-[450px] pr-1 flex-1">
            <div v-for="b in bookings" :key="b.id" class="p-4 rounded-xl flex items-center justify-between gap-4 border border-(--border-default) bg-(--surface-solid) hover:border-(--border-focus) transition-all duration-200">
              <div class="space-y-1 min-w-0">
                <p class="text-sm font-bold text-(--text-heading) truncate">{{ b.roomName }}</p>
                <p class="text-xs text-(--text-muted) flex items-center gap-1">
                  <Calendar :size="12" />
                  {{ formatBookingDate(b.startTime) }}
                  <span class="text-placeholder">&bull;</span>
                  <Clock :size="12" />
                  {{ formatBookingTime(b.startTime, b.endTime) }}
                </p>
                <p class="text-[11px] text-(--text-body) italic truncate mt-1">Lý do: {{ b.purpose }}</p>
                <p class="text-[10px] text-(--text-muted)">Người đặt: {{ b.requesterName }}</p>
              </div>
              <div class="shrink-0">
                <GlassBadge :variant="
                  b.status === 'confirmed' ? 'success' : 
                  b.status === 'da_huy' ? 'neutral' : 'warning'
                }" size="sm" class="font-bold">
                  {{ 
                    b.status === 'confirmed' ? 'Đã duyệt' : 
                    b.status === 'da_huy' ? 'Đã hủy' : 'Chờ duyệt' 
                  }}
                </GlassBadge>
              </div>
            </div>
          </div>
        </GlassPanel>
      </div>
    </template>
  </div>
</template>

<style scoped>
</style>
