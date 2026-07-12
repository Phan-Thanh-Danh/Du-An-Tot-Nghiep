<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { Plus, X, Pencil, Loader2, Calendar } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { blockApi } from '@/services/blockApi'
import { academicTermApi } from '@/services/academicTermApi'

const loading = ref(true); const error = ref(null); const blocks = ref([])
const terms = ref([]); const selectedTerm = ref(null)

const showFormModal = ref(false); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ ngayBatDau: '', ngayKetThuc: '' })
const formErrors = ref({})

onMounted(fetchTerms)

async function fetchTerms() {
  try { 
    const res = await academicTermApi.list({ PageSize: 100 }); 
    terms.value = Array.isArray(res) ? res : (res?.items || res?.data || []) 
    if (terms.value.length > 0) {
      selectedTerm.value = terms.value[0].maHocKy;
    }
  }
  catch (e) { error.value = e.message || 'Không thể tải danh sách học kỳ' }
}

watch(selectedTerm, (newVal) => {
  if (newVal) fetchBlocks(newVal)
})

async function fetchBlocks(termId) {
  loading.value = true; error.value = null
  try { 
    const res = await blockApi.getByTerm(termId); 
    blocks.value = Array.isArray(res) ? res : (res?.items || res?.data || []) 
  }
  catch (e) { error.value = e.message || 'Không thể tải danh sách block' }
  finally { loading.value = false }
}

// ── Form ──
function resetForm() { formData.value = { ngayBatDau: '', ngayKetThuc: '' }; formErrors.value = {} }

function openEdit(b) {
  editingId.value = b.maBlock
  // Format DateOnly (YYYY-MM-DD) for input type="date"
  const format = (dateStr) => {
    if (!dateStr) return '';
    return dateStr.split('T')[0];
  }
  formData.value = { 
    ngayBatDau: format(b.ngayBatDau), 
    ngayKetThuc: format(b.ngayKetThuc) 
  }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.ngayBatDau) e.ngayBatDau = 'Ngày bắt đầu là bắt buộc'
  if (!formData.value.ngayKetThuc) e.ngayKetThuc = 'Ngày kết thúc là bắt buộc'
  if (formData.value.ngayBatDau && formData.value.ngayKetThuc && formData.value.ngayBatDau >= formData.value.ngayKetThuc) {
    e._api = 'Ngày bắt đầu phải trước ngày kết thúc'
  }
  formErrors.value = e; return Object.keys(e).length === 0
}

async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    const payload = { ngayBatDau: formData.value.ngayBatDau, ngayKetThuc: formData.value.ngayKetThuc }
    await blockApi.update(editingId.value, payload)
    closeForm(); await fetchBlocks(selectedTerm.value)
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu block' }
  finally { submitting.value = false }
}

const formatDate = (d) => {
  if (!d) return ''
  const date = new Date(d)
  return date.toLocaleDateString('vi-VN')
}
</script>

<template>
  <div class="space-y-4">
    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng số Block (HK)</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ blocks.length }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <Calendar :size="20" class="text-(--color-info-text)" />
          </div>
        </div>
      </div>
    </div>

    <!-- Main Card -->
    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="p-4 border-b border-default bg-(--surface-input)">
        <div class="flex flex-wrap items-center gap-3">
          <div class="flex items-center gap-2 w-64">
            <label class="text-xs font-semibold text-muted whitespace-nowrap">Học kỳ:</label>
            <select v-model="selectedTerm" class="h-10 px-3 flex-1 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
              <option v-for="t in terms" :key="t.maHocKy" :value="t.maHocKy">{{ t.tenHocKy }}</option>
            </select>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="5" /></div>
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchBlocks(selectedTerm)">Thử lại</GlassButton>
        </div>
      </div>
      <div v-else-if="blocks.length === 0 && selectedTerm" class="p-6">
        <EmptyState title="Học kỳ này chưa có block nào" description="Block sẽ được sinh tự động thông qua seeder." />
      </div>
      <div v-else-if="!selectedTerm" class="p-6">
        <EmptyState title="Chưa chọn học kỳ" description="Vui lòng chọn học kỳ để xem danh sách Block." />
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold text-center w-24">Thứ tự</th>
              <th class="px-3 py-4 font-semibold text-center">Ngày bắt đầu</th>
              <th class="px-3 py-4 font-semibold text-center">Ngày kết thúc</th>
              <th class="px-3 py-4 font-semibold text-center w-32">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="b in blocks" :key="b.maBlock" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 text-center font-bold text-(--lg-primary)">Block {{ b.thuTuBlock }}</td>
              <td class="px-3 py-3.5 text-center text-body">{{ formatDate(b.ngayBatDau) }}</td>
              <td class="px-3 py-3.5 text-center text-body">{{ formatDate(b.ngayKetThuc) }}</td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Chỉnh sửa ngày" @click.stop="openEdit(b)">
                    <Pencil :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Edit Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-sm lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">Chỉnh sửa thời gian Block</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày Bắt Đầu <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.ngayBatDau" type="date" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.ngayBatDau ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.ngayBatDau" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.ngayBatDau }}</p>
              </div>
              
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày Kết Thúc <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.ngayKetThuc" type="date" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.ngayKetThuc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.ngayKetThuc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.ngayKetThuc }}</p>
              </div>

            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>Cập nhật</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
