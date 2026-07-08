<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  AlertTriangle,
  ChevronDown,
  AlertOctagon,
  CheckCircle,
  HelpCircle,
  PhoneCall,
  User,
  ChevronLeft,
  AlertCircle
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')))
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref(null)
const children = ref([])
const warningItems = ref([])

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || {
    id: activeChildId.value,
    name: 'Học sinh',
    className: ''
  }
})

function selectChild(id) {
  activeChildId.value = id
  localStorage.setItem('parent_active_student_id', id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
  loading.value = true
  error.value = null
  parentApi.getChildAlerts(id).then(res => {
    warningItems.value = mapAlertsToWarnings(res.data)
  }).catch(e => {
    error.value = e.message
  }).finally(() => {
    loading.value = false
  })
}

function mapAlertsToWarnings(data) {
  const alerts = data?.alerts || []
  return alerts.map((a, idx) => ({
    id: idx + 1,
    type: a.severity === 'danger' ? 'danger' : 'warning',
    subject: a.type === 'attendance' ? 'Chuyên cần' : a.type === 'grade' ? 'Học tập' : 'Hệ thống',
    reason: a.message || '',
    advice: a.severity === 'danger'
      ? 'Phụ huynh vui lòng liên hệ gấp với Giảng viên chủ nhiệm để có biện pháp hỗ trợ kịp thời.'
      : 'Phụ huynh vui lòng nhắc nhở con em học tập và rèn luyện đúng quy định.',
    date: new Date().toLocaleDateString('vi-VN'),
    confirmed: false,
    confirmedAt: null
  }))
}

function confirmWarning(warnId) {
  const warn = warningItems.value.find(w => w.id === warnId)
  if (warn) {
    warn.confirmed = true
    warn.confirmedAt = new Date().toLocaleString('vi-VN')
    popupStore.success('Xác nhận thành công', 'Hệ thống đã ghi nhận phản hồi của phụ huynh.')
  }
}

function goBack() {
  router.push('/parent/dashboard')
}

onMounted(async () => {
  try {
    const [childrenRes, alertsRes] = await Promise.all([
      parentApi.getChildren().catch(() => ({ data: [] })),
      parentApi.getChildAlerts(activeChildId.value)
    ])
    children.value = childrenRes.data || []
    warningItems.value = mapAlertsToWarnings(alertsRes.data)
  } catch (e) {
    error.value = e.message || 'Không thể tải dữ liệu cảnh báo'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="space-y-6">
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="text-xs text-muted">Đang tải dữ liệu...</div>
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 gap-3">
      <AlertCircle :size="28" class="text-red-500" />
      <p class="text-xs text-red-600 font-semibold">{{ error }}</p>
      <button @click="goBack" class="px-3 py-1.5 text-xs font-bold rounded-lg surface-card border border-card text-label hover:text-orange-600">Quay lại</button>
    </div>
    <template v-else>
      <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="flex items-center gap-2">
          <button
            @click="goBack"
            class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
            title="Quay lại"
          >
            <ChevronLeft :size="18" />
          </button>
          <div>
            <h2 class="text-lg font-bold text-heading flex items-center gap-2">
              <AlertTriangle :size="20" class="text-red-500" />
              Cảnh báo học tập & rèn luyện
            </h2>
            <p class="text-xs text-body">Nhận thông báo rủi ro về điểm số kém, vắng quá số buổi hoặc thiếu bài tập của con</p>
          </div>
        </div>

        <div class="relative min-w-[220px]">
          <button
            type="button"
            class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
            @click="dropdownOpen = !dropdownOpen"
          >
            <div class="flex items-center gap-2">
              <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
                {{ currentChild.name.split(' ').pop().charAt(0) }}
              </div>
              <span>{{ currentChild.name }}</span>
            </div>
            <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
          </button>

          <Transition
            enter-active-class="transition-all duration-200 ease-out"
            enter-from-class="opacity-0 translate-y-2 scale-95"
            enter-to-class="opacity-100 translate-y-0 scale-100"
            leave-active-class="transition-all duration-150 ease-in"
            leave-from-class="opacity-100 translate-y-0 scale-100"
            leave-to-class="opacity-0 translate-y-2 scale-95"
          >
            <div
              v-if="dropdownOpen"
              class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-(--lg-shadow-md)"
            >
              <button
                v-for="child in children"
                :key="child.id"
                type="button"
                class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
                @click="selectChild(child.id)"
              >
                <span>{{ child.name }} ({{ child.className }})</span>
              </button>
            </div>
          </Transition>
        </div>
      </div>

      <!-- ── DANH SÁCH CẢNH BÁO CHÍNH ── -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div class="lg:col-span-2 space-y-4">
          <div class="lg-card-glass p-5 space-y-4">
            <div class="flex items-center justify-between pb-3 border-b border-card">
              <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
                Danh sách cảnh báo hiện tại
              </h3>
              <span class="text-[10px] font-bold text-red-500 bg-red-50 dark:bg-red-950/20 px-2 py-0.5 rounded">
                {{ warningItems.filter(w => !w.confirmed).length }} cảnh báo chưa đọc
              </span>
            </div>

            <div v-if="warningItems.length === 0" class="text-center py-16 text-muted text-xs flex flex-col items-center justify-center gap-2">
              <CheckCircle :size="30" class="text-emerald-500" />
              <span>Tuyệt vời! Con em hiện tại không có cảnh báo rèn luyện hoặc học tập nào.</span>
            </div>
            <div v-else class="space-y-4">
              <div
                v-for="warn in warningItems"
                :key="warn.id"
                class="p-4 rounded-2xl border transition relative overflow-hidden flex flex-col justify-between gap-4"
                :class="
                  warn.type === 'danger' ? 'border-red-200 bg-red-50/20 dark:border-red-950/20 dark:bg-red-950/5' :
                  'border-amber-200 bg-amber-50/20 dark:border-amber-950/20 dark:bg-amber-950/5'
                "
              >
                <div
                  class="absolute left-0 top-0 bottom-0 w-1"
                  :class="warn.type === 'danger' ? 'bg-red-500' : 'bg-amber-500'"
                />

                <div class="flex items-start justify-between gap-3 pl-2">
                  <div class="flex items-start gap-2.5">
                    <span class="mt-0.5 flex-shrink-0" :class="warn.type === 'danger' ? 'text-red-600' : 'text-amber-600'">
                      <component :is="warn.type === 'danger' ? AlertOctagon : AlertTriangle" :size="18" />
                    </span>
                    <div>
                      <h4 class="text-xs font-bold text-heading">
                        Môn học: {{ warn.subject }}
                      </h4>
                      <p class="text-[10px] text-muted font-semibold mt-0.5">
                        Ngày cảnh báo: {{ warn.date }}
                      </p>
                    </div>
                  </div>

                  <span
                    class="px-2 py-0.5 rounded text-[9px] font-bold uppercase"
                    :class="warn.type === 'danger' ? 'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400' : 'bg-amber-100 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400'"
                  >
                    {{ warn.type === 'danger' ? 'Mức độ: Cao' : 'Mức độ: Thấp' }}
                  </span>
                </div>

                <div class="space-y-2 pl-7 text-xs leading-relaxed text-body">
                  <p><strong>Nội dung rủi ro:</strong> {{ warn.reason }}</p>
                  <div class="p-3 surface-input rounded-xl border border-dashed border-card">
                    <span class="font-bold text-[10px] uppercase text-orange-600 block mb-1">Lời khuyên / Yêu cầu từ nhà trường:</span>
                    <p>{{ warn.advice }}</p>
                  </div>
                </div>

                <div class="pl-7 pt-3 border-t border-card/60 flex items-center justify-between gap-3">
                  <span v-if="warn.confirmed" class="text-[10px] text-emerald-600 dark:text-emerald-400 font-bold flex items-center gap-1">
                    <CheckCircle :size="12" /> Đã xác nhận lúc {{ warn.confirmedAt }}
                  </span>
                  <button
                    v-else
                    @click="confirmWarning(warn.id)"
                    class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1.5 ml-auto"
                  >
                    <CheckCircle :size="13" /> Đã đọc và xác nhận
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="space-y-6">
          <div class="lg-card-glass p-5 space-y-4">
            <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card">
              Giảng viên chủ nhiệm (GVCN)
            </h3>
            <div class="space-y-4 text-xs font-semibold">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-100 dark:bg-orange-950/30 text-orange-700 font-bold text-sm">
                  GV
                </div>
                <div>
                  <h4 class="text-xs font-bold text-heading">TS. Nguyễn Văn Giáo Vụ</h4>
                  <p class="text-[10px] text-muted font-normal mt-0.5">Khoa Công nghệ thông tin</p>
                </div>
              </div>

              <div class="border-t border-card my-3"></div>

              <div class="space-y-2.5 text-[11px] text-body">
                <p class="flex items-center gap-2">
                  <PhoneCall :size="13" class="text-muted" />
                  <span>Hotline: <strong class="text-heading">0901.234.567</strong></span>
                </p>
                <p class="flex items-center gap-2">
                  <User :size="13" class="text-muted" />
                  <span>Giờ làm việc: 8:00 - 17:00 (Thứ 2 - Thứ 6)</span>
                </p>
              </div>

              <a
                href="tel:0901234567"
                class="w-full py-2 surface-input border border-card hover:bg-(--surface-card-hover) text-label text-xs font-bold rounded-xl flex items-center justify-center gap-1.5 transition"
              >
                <PhoneCall :size="13" /> Gọi điện trực tiếp
              </a>
            </div>
          </div>

          <div class="lg-card-glass p-5 space-y-3">
            <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card flex items-center gap-1.5">
              <HelpCircle :size="15" class="text-orange-600" />
              Thông tin cảnh báo
            </h3>
            <div class="text-[11px] text-body leading-relaxed space-y-2">
              <p>
                Hệ thống cảnh báo tự động chạy hàng ngày dựa trên các dữ liệu về chuyên cần, tiến độ nộp bài và kết quả kiểm tra định kỳ của học sinh.
              </p>
              <p>
                <strong>Mức độ Cao (Đỏ):</strong> Rủi ro lớn có thể dẫn đến việc bị cấm thi hoặc trượt môn học. Cần phối hợp gấp với nhà trường.
              </p>
              <p>
                <strong>Mức độ Thấp (Vàng):</strong> Nhắc nhở nhắc con rèn luyện học tập trước khi tình trạng nghiêm trọng hơn.
              </p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
</style>
