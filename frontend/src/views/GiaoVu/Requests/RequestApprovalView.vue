<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft, CheckCircle2, XCircle, Loader2, AlertCircle, FileText, User, Clock, MessageSquare
} from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'


const popupStore = usePopupStore()
const route = useRoute()
const router = useRouter()
const requestId = route.params.id || 'DON-001'

const loading = ref(true)
const apiError = ref('')
const approving = ref(false)
const rejecting = ref(false)
const request = ref(null)

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getRequestById(requestId)
    request.value = res ?? null
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function handleApprove() {
  approving.value = true
  try {
    await staffApi.approveRequest(requestId, { note: 'Đã duyệt bởi giáo vụ.' })
    popupStore.success('Đã duyệt đơn', `Đơn ${requestId} đã được phê duyệt.`)
    if (request.value) request.value.status = 'approved'
  } catch (err) {
    popupStore.error('Duyệt thất bại', err?.message || 'Không thể duyệt đơn, vui lòng thử lại.')
  } finally {
    approving.value = false
  }
}

async function handleReject() {
  rejecting.value = true
  try {
    await staffApi.rejectRequest(requestId, { reason: 'Từ chối bởi giáo vụ.' })
    popupStore.error('Đã từ chối đơn', `Đơn ${requestId} đã bị từ chối.`)
    if (request.value) request.value.status = 'rejected'
  } catch (err) {
    popupStore.error('Từ chối thất bại', err?.message || 'Không thể từ chối đơn, vui lòng thử lại.')
  } finally {
    rejecting.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-7xl mx-auto w-full">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <CheckCircle2 class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Phê duyệt đơn</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Xem xét và phê duyệt hoặc từ chối đơn từ sinh viên.</p>
      </div>
      <router-link to="/staff/requests" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
        <ArrowLeft :size="18" /> Quay lại
      </router-link>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else-if="request">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Left: Request Preview -->
        <div class="lg:col-span-2 space-y-4">
          <div class="surface-card border border-(--border-card) rounded-2xl p-6">
            <div class="flex items-center gap-4 mb-5">
              <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body shadow-sm border-default">
                <FileText :size="24" />
              </div>
              <div>
                <h3 class="text-lg font-semibold text-(--text-heading) leading-tight">{{ request.title }}</h3>
                <p class="text-xs font-bold text-(--text-label) mt-0.5 uppercase tracking-tighter">{{ request.type }}</p>
              </div>
              <div class="ml-auto">
                <GlassBadge :variant="request.status === 'approved' ? 'success' : request.status === 'rejected' ? 'danger' : 'warning'" size="sm">
                  {{ request.status === 'approved' ? 'Đã duyệt' : request.status === 'rejected' ? 'Từ chối' : 'Chờ duyệt' }}
                </GlassBadge>
              </div>
            </div>

            <div class="surface-solid p-4 rounded-2xl border-default">
              <p class="text-sm text-(--text-body) leading-relaxed font-medium">{{ request.content }}</p>
            </div>

            <div class="mt-5 flex items-center gap-6 text-xs text-(--text-muted)">
              <span class="flex items-center gap-1.5"><User :size="14" /> {{ request.student }} ({{ request.studentCode }})</span>
              <span class="flex items-center gap-1.5"><Clock :size="14" /> {{ request.date }}</span>
            </div>
          </div>
        </div>

        <!-- Right: Approval Panel -->
        <div class="space-y-4">
          <div class="surface-card border border-(--border-card) rounded-2xl p-6">
            <h4 class="text-xs font-semibold text-(--text-label) uppercase tracking-widest mb-5">Xác nhận phê duyệt</h4>

            <div class="space-y-4" v-if="request.status !== 'approved' && request.status !== 'rejected'">
              <button
                :disabled="approving"
                @click="handleApprove"
                class="w-full lg-button-primary py-4 text-sm font-semibold flex items-center justify-center gap-2"
              >
                <Loader2 v-if="approving" class="animate-spin" :size="20" />
                <CheckCircle2 v-else :size="20" />
                {{ approving ? 'Đang xử lý...' : 'DUYỆT ĐƠN' }}
              </button>
              <button
                :disabled="rejecting"
                @click="handleReject"
                class="w-full lg-btn-danger py-4 text-sm font-semibold flex items-center justify-center gap-2"
              >
                <Loader2 v-if="rejecting" class="animate-spin" :size="20" />
                <XCircle v-else :size="20" />
                {{ rejecting ? 'Đang xử lý...' : 'TỪ CHỐI' }}
              </button>
            </div>

            <div v-else class="text-center py-4">
              <p class="text-sm font-semibold text-(--text-heading)">
                Đơn này đã được {{ request.status === 'approved' ? 'phê duyệt' : 'từ chối' }}.
              </p>
            </div>
          </div>

          <div class="surface-card border border-(--border-card) rounded-2xl p-4">
            <h4 class="text-xs font-semibold text-(--text-label) uppercase tracking-widest mb-3">Thông tin sinh viên</h4>
            <div class="space-y-2 text-sm">
              <p class="flex justify-between"><span class="text-(--text-muted)">Họ tên</span> <span class="font-semibold text-(--text-heading)">{{ request.student }}</span></p>
              <p class="flex justify-between"><span class="text-(--text-muted)">Mã SV</span> <span class="font-semibold text-(--text-heading)">{{ request.studentCode }}</span></p>
              <p class="flex justify-between"><span class="text-(--text-muted)">Loại đơn</span> <span class="font-semibold text-(--text-heading)">{{ request.type }}</span></p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
