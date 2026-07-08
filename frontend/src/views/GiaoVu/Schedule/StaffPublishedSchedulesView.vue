<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  CheckCircle, ArrowLeftRight, X, AlertTriangle, Users, BookOpen, Loader2,
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { staffApi } from '@/services/staffApi'


const loading = ref(true)
const apiError = ref('')
const schedules = ref([])
const changes = ref([])
const selected = ref(null)
const filterHocKy = ref('')
const activeTab = ref('lichHoc')

import { formatDate } from '@/utils/dateFormat'

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getSchedules()
    schedules.value = res?.items ?? res ?? []
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })

const hocKyList = computed(() => [...new Set(schedules.value.map(s => s.tenHocKy))])

const filteredSchedules = computed(() => {
  if (!filterHocKy.value) return schedules.value
  return schedules.value.filter(s => s.tenHocKy === filterHocKy.value)
})

const loaiThayDoiLabel = l => ({
  day_thay: 'Dạy thay', doi_phong: 'Đổi phòng', doi_ca: 'Đổi ca', huy_buoi: 'Hủy buổi',
}[l] || l)
const loaiThayDoiVariant = l => ({
  day_thay: 'info', doi_phong: 'warning', doi_ca: 'warning', huy_buoi: 'danger',
}[l] || 'neutral')
const ttLabel = s => ({ da_xac_nhan: 'Đã xác nhận', cho_xac_nhan: 'Chờ xác nhận' }[s] || s)
const ttVariant = s => ({ da_xac_nhan: 'success', cho_xac_nhan: 'warning' }[s] || 'neutral')

const stats = computed(() => ({
  total: schedules.value.length,
  lopTong: schedules.value.reduce((a, b) => a + b.soLop, 0),
  thayDoi: changes.value.length,
  huyChua: changes.value.filter(c => c.loaiThayDoi === 'huy_buoi').length,
}))
</script>

<template>
  <div class="published-view space-y-4 max-w-full">

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

    <template v-else>
      <!-- Header -->
      <div>
        <div class="flex items-center gap-2">
          <CheckCircle class="text-emerald-500" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Lịch đã công bố</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Theo dõi và giám sát các bộ TKB đã được BGH phê duyệt và công bố chính thức.</p>
      </div>

      <!-- Stat pills -->
      <div class="flex flex-wrap gap-2">
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--color-success-bg) text-sm">
          <span class="font-bold text-xl text-(--color-success-text)">{{ stats.total }}</span>
          <span class="text-(--text-muted)">Bộ TKB</span>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
          <Users :size="13" class="text-(--text-muted)" />
          <span class="font-bold text-(--text-heading)">{{ stats.lopTong }}</span>
          <span class="text-(--text-muted)">Lớp học</span>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--color-warning-bg) text-sm">
          <ArrowLeftRight :size="13" class="text-(--color-warning-text)" />
          <span class="font-bold text-(--color-warning-text)">{{ stats.thayDoi }}</span>
          <span class="text-(--text-muted)">Thay đổi</span>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--color-danger-bg) text-sm">
          <AlertTriangle :size="13" class="text-(--color-danger-text)" />
          <span class="font-bold text-(--color-danger-text)">{{ stats.huyChua }}</span>
          <span class="text-(--text-muted)">Buổi hủy</span>
        </div>
      </div>

      <!-- Tab bar -->
      <div class="flex gap-1 bg-(--surface-input) p-1 rounded-xl w-fit border border-(--border-default)">
        <button
          v-for="tab in [{ v: 'lichHoc', l: 'Thời khóa biểu' }, { v: 'thayDoi', l: 'Thay đổi phát sinh' }]" :key="tab.v"
          class="px-4 py-1.5 rounded-lg text-sm font-medium transition-all"
          :class="activeTab === tab.v ? 'bg-(--surface-card) text-(--text-heading) shadow-sm' : 'text-(--text-muted) hover:text-(--text-heading)'"
          @click="activeTab = tab.v"
        >{{ tab.l }}</button>
      </div>

      <!-- ── Tab: Lịch học đã công bố ── -->
      <div v-if="activeTab === 'lichHoc'" class="flex gap-4 items-start">

        <!-- List -->
        <div class="flex-1 min-w-0 space-y-2">
          <!-- Filter -->
          <div class="flex gap-2 items-center mb-1">
            <select v-model="filterHocKy" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
              <option value="">Tất cả học kỳ</option>
              <option v-for="hk in hocKyList" :key="hk">{{ hk }}</option>
            </select>
            <span class="text-xs text-(--text-muted)">{{ filteredSchedules.length }} bộ TKB</span>
          </div>

          <div
            v-for="s in filteredSchedules" :key="s.id"
            class="surface-card border border-(--border-card) rounded-2xl shadow-sm border-l-4 border-l-emerald-500 cursor-pointer transition-all hover:shadow-md"
            :class="selected?.id === s.id ? 'ring-2 ring-(--lg-primary)' : ''"
            @click="selected = s"
          >
            <div class="p-4 flex items-center gap-4 flex-wrap">
              <div class="w-10 h-10 rounded-xl bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center shrink-0">
                <CheckCircle :size="20" />
              </div>

              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="text-sm font-bold text-(--text-heading)">{{ s.maTkb }}</span>
                  <GlassBadge variant="success" size="sm">Đã công bố</GlassBadge>
                  <GlassBadge v-if="s.thayDoiPhatSinh > 0" variant="warning" size="sm">{{ s.thayDoiPhatSinh }} thay đổi</GlassBadge>
                  <GlassBadge v-if="s.buoiHuy > 0" variant="danger" size="sm">{{ s.buoiHuy }} hủy</GlassBadge>
                </div>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ s.tenDonVi }} · {{ s.tenHocKy }}</p>
                <p class="text-xs text-(--text-muted)">Công bố: {{ formatDate(s.ngayXuatBan) }}</p>
              </div>

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
            </div>
          </div>

          <div v-if="filteredSchedules.length === 0" class="surface-card border border-(--border-card) rounded-2xl p-12 text-center shadow-sm">
            <BookOpen :size="36" class="mx-auto text-(--text-muted) mb-3" />
            <p class="font-semibold text-(--text-heading)">Chưa có lịch công bố</p>
            <p class="text-sm text-(--text-muted) mt-1">Chưa có lịch nào được BGH phê duyệt trong học kỳ này.</p>
          </div>
        </div>

        <!-- Detail panel -->
        <transition name="panel-slide">
          <div
            v-if="selected"
            class="w-72 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg overflow-hidden"
            style="position: sticky; top: 80px"
          >
            <div class="p-4 border-b border-(--border-default) flex items-center justify-between bg-(--color-success-bg)">
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
                <span class="text-xs font-bold text-(--color-success-text) uppercase tracking-wide">Đã công bố</span>
              </div>
              <button @click="selected = null" class="p-1 rounded-lg hover:bg-(--surface-input) text-(--text-muted)">
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
                <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Thời gian</p>
                <p class="text-xs">Ngày công bố: <strong>{{ formatDate(selected.ngayXuatBan) }}</strong></p>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ selected.tenHocKy }}</p>
              </div>

              <div class="space-y-2">
                <div class="flex items-center justify-between bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
                  <span class="text-xs text-(--text-body)">Thay đổi phát sinh</span>
                  <GlassBadge :variant="selected.thayDoiPhatSinh > 0 ? 'warning' : 'success'" size="sm">{{ selected.thayDoiPhatSinh }}</GlassBadge>
                </div>
                <div class="flex items-center justify-between bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
                  <span class="text-xs text-(--text-body)">Buổi bị hủy</span>
                  <GlassBadge :variant="selected.buoiHuy > 0 ? 'danger' : 'success'" size="sm">{{ selected.buoiHuy }}</GlassBadge>
                </div>
              </div>

              <div v-if="selected.thayDoiPhatSinh === 0 && selected.buoiHuy === 0" class="flex items-center gap-2 bg-(--color-success-bg) border border-(--border-default) rounded-xl p-3">
                <CheckCircle :size="15" class="text-(--color-success-text) shrink-0" />
                <p class="text-xs text-(--color-success-text) font-medium">Lịch đang vận hành ổn định, không có biến động.</p>
              </div>
            </div>
          </div>
        </transition>
      </div>

      <!-- ── Tab: Thay đổi phát sinh ── -->
      <div v-if="activeTab === 'thayDoi'" class="space-y-2">
        <div
          v-for="c in changes" :key="c.id"
          class="surface-card border border-(--border-card) rounded-2xl shadow-sm p-4 flex items-center gap-4 flex-wrap"
        >
          <div class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0"
            :class="{
              'bg-(--color-danger-bg) text-(--color-danger-text)': c.loaiThayDoi === 'huy_buoi',
              'bg-(--color-warning-bg) text-(--color-warning-text)': ['doi_phong','doi_ca'].includes(c.loaiThayDoi),
              'bg-(--accent-primary-soft) text-(--lg-primary)': c.loaiThayDoi === 'day_thay',
            }">
            <ArrowLeftRight :size="18" />
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 flex-wrap">
              <span class="text-sm font-semibold text-(--text-heading) truncate">{{ c.monHoc }}</span>
              <GlassBadge :variant="loaiThayDoiVariant(c.loaiThayDoi)" size="sm">{{ loaiThayDoiLabel(c.loaiThayDoi) }}</GlassBadge>
              <GlassBadge :variant="ttVariant(c.trangThai)" size="sm">{{ ttLabel(c.trangThai) }}</GlassBadge>
            </div>
            <p class="text-xs text-(--text-muted) mt-0.5">{{ c.lop }} · {{ c.giaoVien }}</p>
            <p class="text-xs text-(--text-muted)">Ngày học: {{ formatDate(c.ngayHoc) }}</p>
          </div>
          <div class="flex flex-col gap-1 text-xs text-right shrink-0 max-w-[200px]">
            <span class="text-(--text-muted) truncate">Trước: {{ c.truoc }}</span>
            <span class="text-(--text-body) font-medium truncate">Sau: {{ c.sau }}</span>
          </div>
        </div>

        <div v-if="changes.length === 0" class="surface-card border border-(--border-card) rounded-2xl p-12 text-center shadow-sm">
          <CheckCircle :size="36" class="mx-auto text-emerald-400 mb-3" />
          <p class="font-semibold text-(--text-heading)">Không có thay đổi</p>
          <p class="text-sm text-(--text-muted) mt-1">Chưa có biến động nào sau khi lịch được công bố.</p>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.panel-slide-enter-active, .panel-slide-leave-active { transition: opacity .2s ease, transform .2s ease; }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateX(16px); }
</style>
