<script setup>
import { ref, computed } from 'vue'
import {
  Clock, Send, RotateCcw, CheckCircle2, AlertTriangle, X,
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { bghPendingSchedules } from '@/mocks/scheduleAttendanceMockData'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const schedules = ref(bghPendingSchedules.map(s => ({ ...s })))
const selected = ref(null)
const confirmAction = ref(null)
const filterStatus = ref('')

// ── Computed ───────────────────────────────────────────────────
const filtered = computed(() => {
  let list = schedules.value
  if (filterStatus.value) list = list.filter(s => s.trangThai === filterStatus.value)
  return list
})

const ttLabel = s => ({ cho_duyet: 'Chờ duyệt', da_duyet: 'Đã duyệt', tra_ve: 'Đã trả về', da_huy: 'Đã hủy' }[s] || s)
const ttVariant = s => ({ cho_duyet: 'warning', da_duyet: 'success', tra_ve: 'info', da_huy: 'neutral' }[s] || 'neutral')

const statusColor = (s) => ({
  cho_duyet: 'border-l-amber-400',
  da_duyet: 'border-l-emerald-500',
  tra_ve: 'border-l-blue-400',
  da_huy: 'border-l-(--border-default)',
}[s] || 'border-l-(--border-default)')

// ── Stats ──────────────────────────────────────────────────────
const stats = computed(() => ({
  choDuyet: schedules.value.filter(s => s.trangThai === 'cho_duyet').length,
  trVe: schedules.value.filter(s => s.trangThai === 'tra_ve').length,
  daDuyet: schedules.value.filter(s => s.trangThai === 'da_duyet').length,
  xungDot: schedules.value.filter(s => s.xungDot > 0).length,
}))

// ── Actions ────────────────────────────────────────────────────
function recallSubmission(s) {
  confirmAction.value = {
    title: 'Thu hồi bản TKB?',
    message: `Lịch "${s.maTkb}" sẽ được thu hồi về trạng thái bản nháp. Bạn có thể chỉnh sửa rồi gửi lại.`,
    label: 'Thu hồi',
    variant: 'danger',
    run: () => {
      const idx = schedules.value.findIndex(x => x.id === s.id)
      if (idx !== -1) schedules.value.splice(idx, 1)
      if (selected.value?.id === s.id) selected.value = null
      confirmAction.value = null
      popupStore.success('Đã thu hồi', `Lịch ${s.maTkb} đã được thu hồi về nháp.`)
    }
  }
}

function resubmit(s) {
  confirmAction.value = {
    title: 'Gửi lại lịch học?',
    message: `Gửi lại "${s.maTkb}" đến BGH để xét duyệt lần hai?`,
    label: 'Gửi lại',
    variant: 'primary',
    run: () => {
      const idx = schedules.value.findIndex(x => x.id === s.id)
      if (idx !== -1) schedules.value[idx].trangThai = 'cho_duyet'
      if (selected.value?.id === s.id) selected.value = { ...schedules.value[idx] }
      confirmAction.value = null
      popupStore.success('Đã gửi lại', `Lịch ${s.maTkb} đã được gửi đến BGH xét duyệt.`)
    }
  }
}
</script>

<template>
  <div class="pending-view space-y-4 max-w-full">

    <!-- Header -->
    <div class="flex items-start gap-3 justify-between flex-wrap">
      <div>
        <div class="flex items-center gap-2">
          <Clock class="text-amber-500" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Lịch chờ duyệt</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Theo dõi trạng thái các bộ TKB đã gửi BGH phê duyệt.</p>
      </div>
    </div>

    <!-- Stat pills -->
    <div class="flex flex-wrap gap-2">
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-amber-200 dark:border-amber-800 bg-amber-50/80 dark:bg-amber-950/30 text-sm">
        <span class="font-bold text-xl text-amber-600 dark:text-amber-400">{{ stats.choDuyet }}</span>
        <span class="text-(--text-muted)">Chờ duyệt</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-emerald-200 dark:border-emerald-800 bg-emerald-50/80 dark:bg-emerald-950/40 text-sm">
        <span class="font-bold text-xl text-emerald-600 dark:text-emerald-400">{{ stats.daDuyet }}</span>
        <span class="text-(--text-muted)">Đã duyệt</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-blue-200 dark:border-blue-800 bg-blue-50/80 dark:bg-blue-950/30 text-sm">
        <span class="font-bold text-xl text-blue-600 dark:text-blue-400">{{ stats.trVe }}</span>
        <span class="text-(--text-muted)">Trả về</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-red-200 dark:border-red-800 bg-red-50/80 dark:bg-red-950/30 text-sm">
        <AlertTriangle :size="13" class="text-red-500" />
        <span class="font-bold text-red-600 dark:text-red-400">{{ stats.xungDot }}</span>
        <span class="text-(--text-muted)">Có xung đột</span>
      </div>
    </div>

    <!-- Filter row -->
    <div class="flex gap-2 items-center">
      <button
        v-for="f in [
          { v: '', l: 'Tất cả' },
          { v: 'cho_duyet', l: 'Chờ duyệt' },
          { v: 'tra_ve', l: 'Trả về' },
          { v: 'da_duyet', l: 'Đã duyệt' },
        ]"
        :key="f.v"
        class="px-4 py-1.5 rounded-full text-sm font-medium border transition-colors"
        :class="filterStatus === f.v
          ? 'bg-(--lg-primary) text-white border-(--lg-primary)'
          : 'bg-(--surface-input) text-(--text-body) border-(--border-default) hover:bg-(--surface-hover)'"
        @click="filterStatus = f.v"
      >{{ f.l }}</button>
    </div>

    <!-- Main split area -->
    <div class="flex gap-4 items-start">

      <!-- Left: schedule list -->
      <div class="flex-1 min-w-0 space-y-2">
        <div
          v-for="s in filtered" :key="s.id"
          class="surface-card border border-(--border-card) rounded-2xl shadow-sm border-l-4 cursor-pointer transition-all hover:shadow-md"
          :class="[statusColor(s.trangThai), selected?.id === s.id ? 'ring-2 ring-(--lg-primary)' : '']"
          @click="selected = s"
        >
          <div class="p-4 flex items-center gap-4 flex-wrap">
            <!-- Icon -->
            <div
              class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0"
              :class="{
                'bg-amber-50 dark:bg-amber-950/30 text-amber-500': s.trangThai === 'cho_duyet',
                'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-500': s.trangThai === 'da_duyet',
                'bg-blue-50 dark:bg-blue-950/30 text-blue-500': s.trangThai === 'tra_ve',
                'bg-(--surface-input) text-(--text-muted)': !['cho_duyet','da_duyet','tra_ve'].includes(s.trangThai),
              }"
            >
              <CheckCircle2 v-if="s.trangThai === 'da_duyet'" :size="20" />
              <Clock v-else-if="s.trangThai === 'cho_duyet'" :size="20" />
              <RotateCcw v-else :size="20" />
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-sm font-bold text-(--text-heading)">{{ s.maTkb }}</span>
                <GlassBadge :variant="ttVariant(s.trangThai)" size="sm">{{ ttLabel(s.trangThai) }}</GlassBadge>
                <GlassBadge v-if="s.xungDot > 0" variant="danger" size="sm">{{ s.xungDot }} xung đột</GlassBadge>
              </div>
              <p class="text-xs text-(--text-muted) mt-0.5">{{ s.tenDonVi }} · {{ s.tenHocKy }}</p>
              <p class="text-xs text-(--text-muted)">Nộp: {{ s.ngayTao }}</p>
            </div>

            <!-- Stats -->
            <div class="flex gap-4 shrink-0 text-center">
              <div>
                <p class="text-lg font-bold text-(--text-heading)">{{ s.soLop }}</p>
                <p class="text-[10px] text-(--text-muted)">Lớp</p>
              </div>
              <div>
                <p class="text-lg font-bold text-(--text-heading)">{{ s.soGiaoVien }}</p>
                <p class="text-[10px] text-(--text-muted)">GV</p>
              </div>
              <div>
                <p class="text-lg font-bold text-(--text-heading)">{{ s.tongSoTiet }}</p>
                <p class="text-[10px] text-(--text-muted)">Tiết/tuần</p>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex gap-2 shrink-0" @click.stop>
              <GlassButton v-if="s.trangThai === 'cho_duyet'" variant="secondary" size="xs" @click="recallSubmission(s)">
                <RotateCcw :size="13" class="mr-1" /> Thu hồi
              </GlassButton>
              <GlassButton v-if="s.trangThai === 'tra_ve'" variant="primary" size="xs" @click="resubmit(s)">
                <Send :size="13" class="mr-1" /> Gửi lại
              </GlassButton>
            </div>
          </div>

          <!-- Return reason -->
          <div v-if="s.trangThai === 'tra_ve' && s.lyDoTraVe" class="border-t border-(--border-default) mx-4 mb-3 pt-3">
            <div class="flex items-start gap-2 bg-blue-50/80 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 rounded-xl px-3 py-2">
              <AlertTriangle :size="13" class="text-blue-500 mt-0.5 shrink-0" />
              <div>
                <p class="text-[10px] font-bold text-blue-600 dark:text-blue-400 uppercase tracking-wide mb-0.5">Lý do BGH trả về</p>
                <p class="text-xs text-(--text-body)">{{ s.lyDoTraVe }}</p>
              </div>
            </div>
          </div>
        </div>

        <div v-if="filtered.length === 0" class="surface-card border border-(--border-card) rounded-2xl p-12 text-center shadow-sm">
          <CheckCircle2 :size="40" class="mx-auto text-emerald-400 mb-3" />
          <p class="font-semibold text-(--text-heading)">Không có lịch nào</p>
          <p class="text-sm text-(--text-muted) mt-1">Không có lịch học phù hợp với bộ lọc hiện tại.</p>
        </div>
      </div>

      <!-- Right: detail panel -->
      <transition name="panel-slide">
        <div
          v-if="selected"
          class="w-72 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg overflow-hidden"
          style="position: sticky; top: 80px"
        >
          <div class="p-4 border-b border-(--border-default) flex items-center justify-between bg-(--surface-solid)">
            <div class="flex items-center gap-2">
              <GlassBadge :variant="ttVariant(selected.trangThai)">{{ ttLabel(selected.trangThai) }}</GlassBadge>
            </div>
            <button @click="selected = null" class="p-1 rounded-lg hover:bg-(--surface-input) text-(--text-muted) hover:text-(--text-heading) transition-colors">
              <X :size="15" />
            </button>
          </div>

          <div class="p-4 space-y-3">
            <div>
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-0.5">Mã lịch</p>
              <p class="text-base font-bold text-(--text-heading)">{{ selected.maTkb }}</p>
              <p class="text-xs text-(--text-muted)">{{ selected.tenDonVi }}</p>
            </div>

            <div class="grid grid-cols-3 gap-2">
              <div v-for="(val, key) in { 'Lớp': selected.soLop, 'GV': selected.soGiaoVien, 'Tiết': selected.tongSoTiet }" :key="key"
                   class="bg-(--surface-input) rounded-xl p-2 border border-(--border-default) text-center">
                <p class="text-base font-bold text-(--text-heading)">{{ val }}</p>
                <p class="text-[10px] text-(--text-muted)">{{ key }}</p>
              </div>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Thông tin</p>
              <p class="text-xs text-(--text-body)">Học kỳ: <strong>{{ selected.tenHocKy }}</strong></p>
              <p class="text-xs text-(--text-muted) mt-0.5">Nộp: {{ selected.ngayTao }}</p>
            </div>

            <div v-if="selected.xungDot > 0" class="bg-red-50/80 dark:bg-red-950/30 border border-red-200 dark:border-red-800 rounded-xl p-3">
              <p class="text-[10px] font-bold text-red-600 dark:text-red-400 uppercase tracking-wide mb-2">{{ selected.xungDot }} xung đột</p>
              <div v-for="(c, i) in selected.conflicts" :key="i" class="text-xs text-red-700 dark:text-red-300 flex items-start gap-1 mb-1">
                <AlertTriangle :size="11" class="mt-0.5 shrink-0" /> {{ c }}
              </div>
            </div>

            <div v-if="selected.trangThai === 'tra_ve' && selected.lyDoTraVe" class="bg-blue-50/80 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 rounded-xl p-3">
              <p class="text-[10px] font-bold text-blue-600 dark:text-blue-400 uppercase tracking-wide mb-1">Lý do trả về</p>
              <p class="text-xs text-(--text-body)">{{ selected.lyDoTraVe }}</p>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default) text-xs text-(--text-muted)">
              <strong class="text-(--text-heading)">Quy trình:</strong> Sau khi gửi, BGH sẽ xét duyệt trong vòng 1–2 ngày làm việc. Nếu bị trả về, chỉnh sửa rồi gửi lại.
            </div>
          </div>

          <div class="px-4 pb-4 flex flex-col gap-2">
            <GlassButton v-if="selected.trangThai === 'cho_duyet'" variant="secondary" class="w-full h-9 justify-center text-sm" @click="recallSubmission(selected)">
              <RotateCcw :size="14" class="mr-1" /> Thu hồi
            </GlassButton>
            <GlassButton v-if="selected.trangThai === 'tra_ve'" variant="primary" class="w-full h-9 justify-center text-sm" @click="resubmit(selected)">
              <Send :size="14" class="mr-1" /> Gửi lại BGH
            </GlassButton>
          </div>
        </div>
      </transition>
    </div>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>

<style scoped>
.panel-slide-enter-active, .panel-slide-leave-active { transition: opacity .2s ease, transform .2s ease; }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateX(16px); }
</style>
