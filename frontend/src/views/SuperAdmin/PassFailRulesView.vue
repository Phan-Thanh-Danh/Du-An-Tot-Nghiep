<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  CheckCircle2,
  Loader2,
  Pencil,
  Plus,
  RefreshCw,
  Save,
  Search,
  X,
} from 'lucide-vue-next'
import { passFailRuleApi } from '@/services/passFailRuleApi'
import { academicTermApi } from '@/services/academicTermApi'
import { apiRequest } from '@/services/apiClient'
import GlassButton from '@/components/ui/GlassButton.vue'
import { SkeletonTable } from '@/components/common/skeleton'

const loading = ref(true)
const error = ref('')
const terms = ref([])
const selectedTermId = ref(null)
const search = ref('')
const majors = ref([])
const specializations = ref([])
const selectedMajorId = ref(null)
const selectedSpecializationId = ref(null)
const rules = ref([])
const summary = ref({ tongMonHoc: 0, daCauHinh: 0, chuaCauHinh: 0 })

const modalOpen = ref(false)
const modalSaving = ref(false)
const modalError = ref('')
const editingId = ref(null)

const emptyForm = () => ({
  maMonHoc: null,
  maHocKy: null,
  trongSoQuaTrinh: 60,
  trongSoGiuaKy: 0,
  trongSoCuoiKy: 40,
  nguongDat: 5,
  tiLeChuyenCanToiThieu: 80,
})
const form = ref(emptyForm())

const totalWeight = computed(() => {
  const f = form.value
  return Number(f.trongSoQuaTrinh || 0) + Number(f.trongSoGiuaKy || 0) + Number(f.trongSoCuoiKy || 0)
})

async function loadTerms() {
  try {
    const list = await academicTermApi.list({ pageSize: 100 })
    terms.value = list.sort((a, b) => (a.tenHocKy ?? '').localeCompare(b.tenHocKy ?? ''))
    const preferred = terms.value.find((t) => t.maCodeHocKy === 'HK3_2026')
    selectedTermId.value = preferred?.maHocKy ?? terms.value[0]?.maHocKy ?? null
  } catch {
    // học kỳ không tải được thì giữ trống
  }
}

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

async function loadMajors() {
  try {
    const result = await apiRequest('/api/master-data/majors?pageSize=100')
    majors.value = unwrapList(result).filter((m) => m.conHoatDong !== false)
  } catch {
    majors.value = []
  }
}

async function loadSpecializations() {
  try {
    const params = { pageSize: 100 }
    if (selectedMajorId.value) params.maNganh = selectedMajorId.value
    const qs = new URLSearchParams(params).toString()
    const result = await apiRequest(`/api/master-data/specializations?${qs}`)
    specializations.value = unwrapList(result).filter((s) => s.conHoatDong !== false)
  } catch {
    specializations.value = []
  }
}

function onMajorChange() {
  selectedSpecializationId.value = null
  loadSpecializations()
  loadRules()
}

async function loadRules() {
  loading.value = true
  error.value = ''
  try {
    const result = await passFailRuleApi.getRules({
      maHocKy: selectedTermId.value ?? '',
      search: search.value || '',
      maNganh: selectedMajorId.value ?? '',
      maChuyenNganh: selectedSpecializationId.value ?? '',
      pageIndex: 1,
      pageSize: 200,
    })
    rules.value = result?.items ?? result?.Items ?? []
    summary.value = {
      tongMonHoc: result?.tongMonHoc ?? rules.value.length,
      daCauHinh: result?.daCauHinh ?? 0,
      chuaCauHinh: result?.chuaCauHinh ?? 0,
    }
  } catch (e) {
    error.value = e?.message || 'Không lấy được danh sách quy tắc đạt/rớt.'
  } finally {
    loading.value = false
  }
}

function openCreate(rule) {
  editingId.value = null
  form.value = { ...emptyForm(), maMonHoc: rule?.maMonHoc ?? null, maHocKy: selectedTermId.value }
  modalError.value = ''
  modalOpen.value = true
}

function openEdit(rule) {
  editingId.value = rule.maCauHinhDiem
  form.value = {
    maMonHoc: rule.maMonHoc,
    maHocKy: rule.maHocKy,
    trongSoQuaTrinh: Number(rule.trongSoQuaTrinh),
    trongSoGiuaKy: Number(rule.trongSoGiuaKy),
    trongSoCuoiKy: Number(rule.trongSoCuoiKy),
    nguongDat: Number(rule.nguongDat),
    tiLeChuyenCanToiThieu: Number(rule.tiLeChuyenCanToiThieu),
  }
  modalError.value = ''
  modalOpen.value = true
}

async function saveRule() {
  modalError.value = ''
  if (form.value.maMonHoc == null || form.value.maHocKy == null) {
    modalError.value = 'Vui lòng chọn môn học và học kỳ.'
    return
  }
  if (totalWeight.value !== 100) {
    modalError.value = `Tổng trọng số phải bằng 100% (hiện tại: ${totalWeight.value}%).`
    return
  }
  if (form.value.nguongDat < 0 || form.value.nguongDat > 10) {
    modalError.value = 'Ngưỡng đạt phải nằm trong khoảng 0 - 10.'
    return
  }
  if (form.value.tiLeChuyenCanToiThieu < 0 || form.value.tiLeChuyenCanToiThieu > 100) {
    modalError.value = 'Tỷ lệ chuyên cần tối thiểu phải từ 0 đến 100%.'
    return
  }

  const payload = {
    maMonHoc: form.value.maMonHoc,
    maHocKy: form.value.maHocKy,
    trongSoQuaTrinh: Number(form.value.trongSoQuaTrinh),
    trongSoGiuaKy: Number(form.value.trongSoGiuaKy),
    trongSoCuoiKy: Number(form.value.trongSoCuoiKy),
    nguongDat: Number(form.value.nguongDat),
    tiLeChuyenCanToiThieu: Number(form.value.tiLeChuyenCanToiThieu),
  }

  modalSaving.value = true
  try {
    if (editingId.value) {
      await passFailRuleApi.updateRule(editingId.value, payload)
    } else {
      await passFailRuleApi.createRule(payload)
    }
    modalOpen.value = false
    await loadRules()
  } catch (e) {
    modalError.value = e?.message || 'Lưu quy tắc thất bại.'
  } finally {
    modalSaving.value = false
  }
}

const selectedTermName = computed(() => {
  return terms.value.find((t) => t.maHocKy === selectedTermId.value)?.tenHocKy ?? '—'
})

const formatDateTime = (value) => {
  if (!value) return '—'
  return new Date(value).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })
}

onMounted(async () => {
  await Promise.all([loadTerms(), loadMajors()])
  await loadSpecializations()
  await loadRules()
})
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 class="text-heading text-2xl font-bold">Quy tắc đạt/rớt</h1>
        <p class="text-label mt-1 text-sm">
          Cấu hình trọng số điểm, ngưỡng đạt và tỷ lệ chuyên cần tối thiểu theo môn học & học kỳ.
        </p>
      </div>
      <div class="flex items-center gap-3">
        <label class="text-label text-sm">Học kỳ</label>
        <select
          v-model="selectedTermId"
          class="border-input surface-input text-body rounded-lg border px-3 py-2 text-sm focus:outline-none"
          @change="loadRules"
        >
          <option v-for="term in terms" :key="term.maHocKy" :value="term.maHocKy">
            {{ term.tenHocKy }}
          </option>
        </select>
        <label class="text-label text-sm">Ngành</label>
        <select
          v-model="selectedMajorId"
          class="border-input surface-input text-body rounded-lg border px-3 py-2 text-sm focus:outline-none"
          @change="onMajorChange"
        >
          <option :value="null">Tất cả ngành</option>
          <option v-for="major in majors" :key="major.maNganh" :value="major.maNganh">
            {{ major.tenNganh }}
          </option>
        </select>
        <label class="text-label text-sm">Chuyên ngành</label>
        <select
          v-model="selectedSpecializationId"
          class="border-input surface-input text-body rounded-lg border px-3 py-2 text-sm focus:outline-none"
          @change="loadRules"
        >
          <option :value="null">Tất cả chuyên ngành</option>
          <option
            v-for="spec in specializations"
            :key="spec.maChuyenNganh"
            :value="spec.maChuyenNganh"
          >
            {{ spec.tenChuyenNganh }}
          </option>
        </select>
        <div class="relative">
          <Search class="text-placeholder absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2" />
          <input
            v-model="search"
            class="border-input surface-input text-body rounded-lg border py-2 pl-8 pr-3 text-sm focus:outline-none"
            placeholder="Tìm theo tên/mã môn…"
            @keyup.enter="loadRules"
          />
        </div>
        <GlassButton variant="ghost" @click="loadRules">
          <RefreshCw class="h-4 w-4" />
        </GlassButton>
      </div>
    </div>

    <div class="flex flex-wrap gap-3">
      <div class="surface-card border-card rounded-xl border px-4 py-3">
        <p class="text-placeholder text-xs">Tổng môn</p>
        <p class="text-heading text-xl font-bold">{{ summary.tongMonHoc }}</p>
      </div>
      <div class="surface-card border-card rounded-xl border px-4 py-3">
        <p class="text-placeholder text-xs">Đã cấu hình</p>
        <p class="text-xl font-bold text-emerald-600 dark:text-emerald-400">{{ summary.daCauHinh }}</p>
      </div>
      <div class="surface-card border-card rounded-xl border px-4 py-3">
        <p class="text-placeholder text-xs">Chưa cấu hình</p>
        <p class="text-xl font-bold text-amber-600 dark:text-amber-400">{{ summary.chuaCauHinh }}</p>
      </div>
      <div class="surface-card border-card rounded-xl border px-4 py-3">
        <p class="text-placeholder text-xs">Kỳ đang chọn</p>
        <p class="text-heading text-xl font-bold">{{ selectedTermName }}</p>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:border-rose-800 dark:bg-rose-950 dark:text-rose-300">
      {{ error }}
    </div>

    <div class="surface-card border-card overflow-hidden rounded-xl border">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="text-placeholder border-b text-xs uppercase">
            <tr>
              <th class="px-4 py-3 font-semibold">Môn học</th>
              <th class="px-4 py-3 font-semibold">Ngành / Chuyên ngành</th>
              <th class="px-4 py-3 font-semibold">QT %</th>
              <th class="px-4 py-3 font-semibold">GK %</th>
              <th class="px-4 py-3 font-semibold">CK %</th>
              <th class="px-4 py-3 font-semibold">Ngưỡng đạt</th>
              <th class="px-4 py-3 font-semibold">Chuyên cần tối thiểu</th>
              <th class="px-4 py-3 font-semibold">Cập nhật lúc</th>
              <th class="px-4 py-3 text-right font-semibold">Thao tác</th>
            </tr>
          </thead>
          <tbody class="text-body divide-y">
            <template v-if="loading">
              <tr v-for="i in 8" :key="i">
                <td colspan="9" class="px-4 py-2"><SkeletonTable :rows="1" /></td>
              </tr>
            </template>
            <template v-else-if="rules.length === 0">
              <tr>
                <td colspan="9" class="text-placeholder px-4 py-10 text-center">
                  Chưa có dữ liệu cho bộ lọc này.
                </td>
              </tr>
            </template>
            <tr v-for="rule in rules" v-else :key="rule.maMonHoc">
              <td class="px-4 py-3">
                <p class="font-medium">{{ rule.tenMonHoc }}</p>
                <p class="text-placeholder text-xs">{{ rule.maCodeMonHoc }}</p>
              </td>
              <td class="px-4 py-3 text-xs">
                <p class="text-body">{{ rule.tenNganh ?? '—' }}</p>
                <p class="text-placeholder">{{ rule.tenChuyenNganh ?? '—' }}</p>
              </td>
              <td class="px-4 py-3">{{ rule.trongSoQuaTrinh }}</td>
              <td class="px-4 py-3">{{ rule.trongSoGiuaKy }}</td>
              <td class="px-4 py-3">{{ rule.trongSoCuoiKy }}</td>
              <td class="px-4 py-3">
                <span class="font-semibold text-indigo-600 dark:text-indigo-400">{{ rule.nguongDat }}</span>
              </td>
              <td class="px-4 py-3">
                <span
                  v-if="rule.maCauHinhDiem === 0"
                  class="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-700 dark:bg-amber-900/30 dark:text-amber-300"
                >
                  Chưa cấu hình
                </span>
                <span v-else class="text-body">{{ rule.tiLeChuyenCanToiThieu }}%</span>
              </td>
              <td class="text-placeholder px-4 py-3 text-xs">
                <template v-if="rule.capNhatLuc">{{ formatDateTime(rule.capNhatLuc) }}</template>
                <template v-else>—</template>
              </td>
              <td class="px-4 py-3 text-right">
                <GlassButton
                  v-if="rule.maCauHinhDiem === 0"
                  variant="primary"
                  size="sm"
                  @click="openCreate(rule)"
                >
                  <Plus class="h-4 w-4" />
                  Tạo cấu hình
                </GlassButton>
                <GlassButton v-else variant="ghost" size="sm" @click="openEdit(rule)">
                  <Pencil class="h-4 w-4" />
                  Chỉnh sửa
                </GlassButton>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div
      v-if="modalOpen"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      @click.self="modalOpen = false"
    >
      <div class="lg-glass-strong border-card w-full max-w-lg rounded-2xl border p-6 shadow-2xl">
        <div class="mb-5 flex items-center justify-between">
          <h2 class="text-heading text-lg font-bold">
            {{ editingId ? 'Chỉnh sửa quy tắc đạt/rớt' : 'Tạo quy tắc đạt/rớt' }}
          </h2>
          <button class="text-placeholder hover:text-body" @click="modalOpen = false">
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="space-y-4">
          <div>
            <label class="text-label mb-1 block text-sm">Môn học</label>
            <select
              v-if="!editingId"
              v-model="form.maMonHoc"
              class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none"
            >
              <option :value="null" disabled>Chọn môn học…</option>
              <option
                v-for="rule in rules.filter((r) => r.maCauHinhDiem === 0)"
                :key="rule.maMonHoc"
                :value="rule.maMonHoc"
              >
                {{ rule.tenMonHoc }} ({{ rule.maCodeMonHoc }})
              </option>
            </select>
            <p v-else class="text-body border-input surface-input w-full rounded-lg border px-3 py-2 text-sm">
              {{ rules.find((r) => r.maMonHoc === form.maMonHoc)?.tenMonHoc ?? form.maMonHoc }}
            </p>
          </div>

          <div class="grid grid-cols-3 gap-3">
            <div>
              <label class="text-label mb-1 block text-sm">Quá trình %</label>
              <input v-model.number="form.trongSoQuaTrinh" type="number" min="0" max="100"
                class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none" />
            </div>
            <div>
              <label class="text-label mb-1 block text-sm">Giữa kỳ %</label>
              <input v-model.number="form.trongSoGiuaKy" type="number" min="0" max="100"
                class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none" />
            </div>
            <div>
              <label class="text-label mb-1 block text-sm">Cuối kỳ %</label>
              <input v-model.number="form.trongSoCuoiKy" type="number" min="0" max="100"
                class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none" />
            </div>
          </div>
          <p class="text-xs" :class="totalWeight === 100 ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-600 dark:text-rose-400'">
            Tổng trọng số: {{ totalWeight }}% {{ totalWeight === 100 ? '✓' : '(phải bằng 100%)' }}
          </p>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="text-label mb-1 block text-sm">Ngưỡng đạt (0 - 10)</label>
              <input v-model.number="form.nguongDat" type="number" step="0.25" min="0" max="10"
                class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none" />
            </div>
            <div>
              <label class="text-label mb-1 block text-sm">Chuyên cần tối thiểu %</label>
              <input v-model.number="form.tiLeChuyenCanToiThieu" type="number" step="5" min="0" max="100"
                class="border-input surface-input text-body w-full rounded-lg border px-3 py-2 text-sm focus:outline-none" />
            </div>
          </div>

          <div v-if="modalError" class="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700 dark:border-rose-800 dark:bg-rose-950 dark:text-rose-300">
            {{ modalError }}
          </div>
        </div>

        <div class="mt-6 flex justify-end gap-3">
          <GlassButton variant="ghost" @click="modalOpen = false">Hủy</GlassButton>
          <GlassButton variant="primary" :disabled="modalSaving" @click="saveRule">
            <Loader2 v-if="modalSaving" class="h-4 w-4 animate-spin" />
            <Save v-else class="h-4 w-4" />
            {{ editingId ? 'Lưu thay đổi' : 'Tạo quy tắc' }}
          </GlassButton>
        </div>
      </div>
    </div>

    <p class="text-placeholder flex items-center gap-1.5 text-xs">
      <CheckCircle2 class="h-3.5 w-3.5" />
      Ngưỡng chuyên cần chỉ áp dụng khi cấu hình lớn hơn 0%. Mỗi lần lưu được ghi vào nhật ký hệ thống.
    </p>
  </div>
</template>
