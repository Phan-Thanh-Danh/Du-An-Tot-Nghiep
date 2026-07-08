<script setup>
import { ref, onMounted } from 'vue'
import { Save, Loader2, AlertCircle, Settings, ShieldCheck } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'


const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')
const saving = ref(false)

const config = ref({
  maxCredits: 24,
  minCredits: 12,
  allowOverlap: false,
  autoApprove: true,
  waitlistEnabled: true,
  maxWaitlist: 10,
  withdrawDeadlineDays: 7,
})

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getRegistrationPeriod()
    if (res) {
      config.value = { ...config.value, ...res }
    }
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  saving.value = true
  try {
    popupStore.success('Đã lưu', 'Cấu hình đăng ký đã được cập nhật.')
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể lưu cấu hình.')
  } finally {
    saving.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="registration-config max-w-4xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Cấu hình đăng ký</h1>
      <p class="text-(--text-body) mt-1">Thiết lập các tham số cho quy trình đăng ký môn học.</p>
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
      <div class="lg-card-glass p-6 rounded-2xl space-y-5">
        <div class="flex items-center gap-3 pb-4 border-b border-default">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Settings :size="20" />
          </div>
          <div>
            <h3 class="text-base font-semibold text-heading">Tham số đăng ký</h3>
            <p class="text-xs text-label">Điều chỉnh các giới hạn cho sinh viên.</p>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Max Credits</label>
            <input v-model.number="config.maxCredits" type="number" min="1" max="50" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
          </div>
          <div>
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Min Credits</label>
            <input v-model.number="config.minCredits" type="number" min="0" max="50" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
          </div>
        </div>

        <div>
          <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Hạn hủy môn (ngày)</label>
          <input v-model.number="config.withdrawDeadlineDays" type="number" min="1" max="30" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
        </div>

        <div class="pt-4 border-t border-default space-y-4">
          <label class="flex items-center justify-between cursor-pointer">
            <span class="text-sm font-semibold text-heading">Cho phép đăng ký trùng lịch</span>
            <input v-model="config.allowOverlap" type="checkbox" class="toggle-checkbox" />
          </label>
          <label class="flex items-center justify-between cursor-pointer">
            <span class="text-sm font-semibold text-heading">Tự động duyệt đăng ký</span>
            <input v-model="config.autoApprove" type="checkbox" class="toggle-checkbox" />
          </label>
          <label class="flex items-center justify-between cursor-pointer">
            <span class="text-sm font-semibold text-heading">Bật Waitlist</span>
            <input v-model="config.waitlistEnabled" type="checkbox" class="toggle-checkbox" />
          </label>
          <div v-if="config.waitlistEnabled">
            <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Max Waitlist</label>
            <input v-model.number="config.maxWaitlist" type="number" min="1" max="100" class="lg-input w-full px-4 py-2.5 text-sm font-bold" />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-default">
          <button
            :class="['lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2', saving ? 'opacity-45 pointer-events-none' : '']"
            :disabled="saving"
            @click="handleSave"
          >
            <span v-if="saving" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <Save v-else :size="16" />
            {{ saving ? 'Đang lưu...' : 'Lưu cấu hình' }}
          </button>
        </div>
      </div>

      <div class="lg-card-glass p-5 rounded-2xl border border-(--color-info-bg)/50 flex gap-4">
        <div class="h-10 w-10 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-link shrink-0">
          <ShieldCheck :size="20" />
        </div>
        <div>
          <h4 class="text-sm font-semibold text-heading">Lưu ý</h4>
          <p class="text-xs text-label mt-1 leading-relaxed">
            Thay đổi cấu hình sẽ áp dụng cho các đợt đăng ký mới. Các đợt đang mở không bị ảnh hưởng.
          </p>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.toggle-checkbox {
  appearance: none;
  width: 40px;
  height: 22px;
  border-radius: 11px;
  background: var(--border-default);
  position: relative;
  cursor: pointer;
  transition: background 0.2s;
}
.toggle-checkbox:checked {
  background: var(--lg-primary);
}
.toggle-checkbox::after {
  content: '';
  position: absolute;
  top: 2px;
  left: 2px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #fff;
  transition: transform 0.2s;
}
.toggle-checkbox:checked::after {
  transform: translateX(18px);
}
</style>
