<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertTriangle, ShieldAlert, Scale } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { studentApi } from '@/services/studentApi'
import { formatDate } from '@/utils/dateFormat'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()
const records = ref([])
const loading = ref(false)
const selectedRecord = ref(null)
const appealReason = ref('')
const confirmAppeal = ref(false)

const fetchRecords = async () => {
  loading.value = true
  try {
    const res = await studentApi.getDisciplineRecords()
    records.value = Array.isArray(res) ? res : (res?.items ?? res?.data ?? [])
  } catch (error) {
    console.error(error)
    popup.error('Lỗi', 'Không thể tải hồ sơ kỷ luật.')
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchRecords())

const activeCount = computed(() => records.value.filter(r => r.trangThai === 'active').length)
const pendingAppealsCount = computed(() => records.value.filter(r => r.appealStatus === 'pending').length)
const canAppealCount = computed(() => records.value.filter(r => r.coTheKhieuNai && r.appealStatus !== 'pending' && r.trangThai === 'active').length)

const submitAppeal = async () => {
  if (!appealReason.value.trim()) {
    popup.error('Lỗi', 'Vui lòng nhập lý do khiếu nại.')
    return
  }
  confirmAppeal.value = false
  try {
    const res = await studentApi.submitAppeal(selectedRecord.value.id, { reason: appealReason.value })
    selectedRecord.value.appealStatus = 'pending'
    popup.success('Thành công', 'Đã gửi khiếu nại. Vui lòng chờ phản hồi.')
    appealReason.value = ''
  } catch (_err) {
    console.error(_err)
    popup.error('Lỗi', 'Không thể gửi khiếu nại.')
  }
}
</script>

<template>
  <div class="student-discipline max-w-7xl mx-auto space-y-6">
    <div class="bg-(--surface-modal) border border-(--border-default) rounded-xl p-5 flex items-start gap-4 shadow-sm">
      <ShieldAlert :size="24" class="text-(--text-muted) mt-1 shrink-0" />
      <div>
        <h2 class="text-base font-bold text-(--text-heading) mb-1">Hồ sơ kỷ luật cá nhân</h2>
        <p class="text-sm text-(--text-muted) leading-relaxed">
          Thông tin kỷ luật mang tính bảo mật và chỉ hiển thị cho bạn cùng các cấp quản lý có thẩm quyền. Hệ thống ghi nhận mọi vi phạm nhằm theo dõi quá trình rèn luyện.
        </p>
      </div>
    </div>

    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-l-[var(--color-danger-bg, #ef4444)] relative overflow-hidden group">
        <div class="absolute right-0 top-0 opacity-10 transform translate-x-4 -translate-y-4 group-hover:scale-110 transition-transform">
          <AlertTriangle :size="80" />
        </div>
        <p class="text-sm font-semibold text-(--text-muted) mb-1 relative z-10">Đang hiệu lực</p>
        <strong class="text-2xl text-(--text-heading) relative z-10">{{ activeCount }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-l-(--border-default)">
        <p class="text-sm font-semibold text-(--text-muted) mb-1">Đã gỡ bỏ</p>
        <strong class="text-2xl text-(--text-heading)">{{ records.length - activeCount }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-l-(--lg-primary)">
        <p class="text-sm font-semibold text-(--text-muted) mb-1">Khiếu nại chờ xử lý</p>
        <strong class="text-2xl text-(--text-heading)">{{ pendingAppealsCount }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-l-amber-500">
        <p class="text-sm font-semibold text-(--text-muted) mb-1">Có thể khiếu nại</p>
        <strong class="text-2xl text-(--text-heading)">{{ canAppealCount }}</strong>
      </GlassPanel>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 space-y-4">
        <LoadingSkeleton v-if="loading" :lines="5" />
        <EmptyState v-else-if="records.length === 0" title="Hồ sơ trống" description="Bạn không có hồ sơ kỷ luật nào." />

        <div v-else class="space-y-4 min-h-[450px] content-start">
          <GlassPanel
            v-for="r in records" :key="r.id"
            variant="flat"
            class="cursor-pointer transition-all duration-200 border"
            :class="[
              selectedRecord?.id === r.id && r.trangThai === 'active' 
                ? 'ring-2 ring-red-500/80 border-red-500/50 shadow-md' 
                : '',
              selectedRecord?.id === r.id && r.trangThai !== 'active'
                ? 'ring-2 ring-(--border-focus) shadow-md' 
                : '',
              selectedRecord?.id !== r.id ? 'border-(--border-default)' : ''
            ]"
            @click="selectedRecord = r"
          >
            <div class="flex justify-between items-start mb-2 gap-2">
              <h3 class="font-bold text-(--text-heading) text-lg line-clamp-2">{{ r.tieuDe }}</h3>
              <div class="shrink-0 pt-0.5">
                <GlassBadge :variant="r.trangThai === 'active' ? 'danger' : 'neutral'" size="sm" class="font-semibold">
                  <template v-if="r.trangThai === 'active'">
                    <span class="flex items-center gap-1.5"><AlertTriangle :size="12" /> Đang hiệu lực</span>
                  </template>
                  <template v-else>Đã gỡ bỏ</template>
                </GlassBadge>
              </div>
            </div>
            <div class="text-sm text-(--text-body) mb-2">
              <p>Mức độ: <strong>{{ r.mucDoKyLuat }}</strong> - Hình thức: <strong>{{ r.hinhThucXuLy }}</strong></p>
            </div>
            <div class="text-xs text-(--text-muted) border-t border-(--border-default) pt-3 flex justify-between">
              <span>Vi phạm: {{ formatDate(r.ngayViPham) }}</span>
              <span>Hạn hiệu lực: {{ formatDate(r.thoiHanHieuLuc) }}</span>
            </div>
          </GlassPanel>
        </div>
      </div>

      <div class="lg:col-span-1">
        <div class="sticky top-6 flex flex-col min-h-[450px] lg:w-[380px] xl:w-[420px] bg-(--surface-card) rounded-2xl border border-(--border-default) shadow-lg overflow-hidden mx-auto lg:mx-0 w-full">
          <div class="h-1.5" :class="selectedRecord?.trangThai === 'active' ? 'bg-(--color-danger-bg)' : 'bg-(--border-focus)'"></div>
          <div class="p-6 flex flex-col flex-1">
            <EmptyState v-if="!selectedRecord" title="Chưa chọn hồ sơ" description="Nhấp vào thẻ bên trái để xem chi tiết vi phạm." />
            <div v-else class="flex flex-col h-full relative">
              <div class="mb-6 pb-5 border-b border-dashed border-(--border-default)">
                <div class="flex items-center gap-2 mb-3">
                  <ShieldAlert v-if="selectedRecord.trangThai === 'active'" :size="18" class="text-(--color-danger-bg)" />
                  <h3 class="text-xl font-bold text-(--text-heading) leading-tight">{{ selectedRecord.tieuDe }}</h3>
                </div>
                <div class="bg-(--surface-input) p-4 rounded-lg font-mono text-sm text-(--text-body) leading-relaxed border border-(--border-default)">
                  {{ selectedRecord.moTaCongKhai }}
                </div>
              </div>

              <div class="flex-1 space-y-4 mb-4">
                <div class="flex justify-between items-center py-2 border-b border-(--border-default) text-sm">
                  <span class="text-(--text-muted)">Mức độ vi phạm</span>
                  <strong class="text-(--text-heading)">{{ selectedRecord.mucDoKyLuat }}</strong>
                </div>
                <div class="flex justify-between items-center py-2 border-b border-(--border-default) text-sm">
                  <span class="text-(--text-muted)">Hình thức xử lý</span>
                  <strong class="font-bold uppercase tracking-tight" :class="selectedRecord.trangThai === 'active' ? 'text-red-600 dark:text-red-400' : 'text-(--text-heading)'">{{ selectedRecord.hinhThucXuLy }}</strong>
                </div>
                <div class="flex justify-between items-center py-2 border-b border-(--border-default) text-sm">
                  <span class="text-(--text-muted)">Thời gian vi phạm</span>
                  <strong class="text-(--text-heading)">{{ formatDate(selectedRecord.ngayViPham, 'DD/MM/YYYY HH:mm') }}</strong>
                </div>
              </div>

              <div v-if="selectedRecord.coTheKhieuNai && selectedRecord.appealStatus !== 'pending' && selectedRecord.trangThai === 'active'" class="pt-6 mt-auto">
                <div class="bg-(--surface-modal) p-4 rounded-xl border border-(--border-default) shadow-sm">
                  <p class="text-sm font-semibold mb-3 flex items-center gap-2 text-(--text-heading)"><Scale :size="16"/> Nộp đơn khiếu nại</p>
                  <textarea v-model="appealReason" class="w-full bg-(--surface-input) border border-(--border-default) text-(--text-body) rounded-lg p-3 min-h-[80px] mb-3 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-(--border-focus)" placeholder="Trình bày lý do khiếu nại của bạn..."></textarea>
                  <GlassButton variant="primary" class="w-full justify-center" @click="confirmAppeal = true">
                    Gửi đơn khiếu nại
                  </GlassButton>
                </div>
              </div>
              <div v-else-if="selectedRecord.appealStatus === 'pending'" class="pt-6 mt-auto">
                <div class="flex items-center justify-center gap-2 bg-(--surface-modal) border border-(--border-default) rounded-xl p-4 text-center text-sm font-medium text-(--text-muted) shadow-sm">
                  <GlassBadge variant="info">Chờ xử lý</GlassBadge>
                  <span>Đơn khiếu nại đang được xem xét.</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="confirmAppeal"
      :show="true"
      title="Xác nhận gửi khiếu nại"
      message="Bạn chắc chắn muốn gửi khiếu nại cho hồ sơ này? Hội đồng kỷ luật sẽ xem xét lại vụ việc."
      confirmLabel="Gửi đi"
      variant="primary"
      @confirm="submitAppeal"
      @cancel="confirmAppeal = false"
    />
  </div>
</template>
