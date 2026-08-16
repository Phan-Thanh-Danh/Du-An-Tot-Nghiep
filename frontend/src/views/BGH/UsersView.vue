<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <!-- Header & Actions -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Quản lý Người Dùng</h2>
        <p class="text-xs text-muted mt-1">Danh sách tài khoản trong cơ sở trực thuộc</p>
      </div>
      <div class="flex items-center gap-2">
        <button v-if="canImport" @click="openImportModal" class="flex items-center gap-2 px-4 py-2 border border-input bg-(--surface-input) hover:bg-(--surface-input-hover) text-body text-sm font-bold rounded-xl transition-all shadow-sm">
          <FileSpreadsheet :size="18" class="text-emerald-600" /> <span>Nhập từ Excel</span>
        </button>
        <button v-if="canEdit" @click="openCreateModal" class="flex items-center gap-2 px-4 py-2 bg-(--lg-primary) hover:bg-(--lg-primary-dark) text-white text-sm font-bold rounded-xl transition-all shadow-sm">
          <Plus :size="18" /> <span>Thêm người dùng</span>
        </button>
      </div>
    </div>

    <!-- Filter Bar: Luôn hiển thị để người dùng có thể đổi cơ sở / lọc dữ liệu bất kỳ lúc nào -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-4 items-end">
      <div class="flex-1 min-w-[200px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Tìm kiếm</label>
        <div class="relative">
          <Loader2 v-if="searchLoading" class="absolute left-3 top-1/2 -translate-y-1/2 text-(--lg-primary) animate-spin" :size="16" />
          <Search v-else class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input v-model="keyword" @input="onSearchInput" @keyup.enter="handleFilter" type="text" placeholder="Tên, Email, SĐT..." class="w-full pl-9 pr-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
        </div>
      </div>
      <div class="w-full sm:w-48">
        <label class="block text-xs font-bold text-heading mb-1.5">Cơ sở / Đơn vị con</label>
        <LmsSelect v-model="orgFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả cơ sở con</option>
          <option v-for="org in orgsList" :key="org.maDonVi" :value="org.maDonVi">{{ org.tenDonVi }}</option>
        </LmsSelect>
      </div>
      <div class="w-full sm:w-40">
        <label class="block text-xs font-bold text-heading mb-1.5">Vai trò</label>
        <LmsSelect v-model="roleFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả vai trò</option>
          <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
        </LmsSelect>
      </div>
      <div class="w-full sm:w-40">
        <label class="block text-xs font-bold text-heading mb-1.5">Trạng thái</label>
        <LmsSelect v-model="statusFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả trạng thái</option>
          <option value="hoat_dong">Hoạt động</option>
          <option value="bi_khoa">Bị khóa</option>
        </LmsSelect>
      </div>
      <button @click="handleFilter" class="px-4 py-2 bg-(--surface-input) border border-input hover:bg-(--surface-input-hover) text-heading text-sm font-bold rounded-lg transition-colors h-10">Lọc dữ liệu</button>
    </div>

    <!-- Data Table Container -->
    <div class="flex-1 surface-card border border-card rounded-2xl shadow-sm flex flex-col overflow-hidden">
      <div class="flex-1 overflow-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="sticky top-0 bg-(--surface-card) z-10 backdrop-blur-[12px]">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">Mã / ID</th>
              <th class="px-4 py-3 font-bold text-heading">Họ tên</th>
              <th class="px-4 py-3 font-bold text-heading">Email</th>
              <th class="px-4 py-3 font-bold text-heading">Vai trò</th>
              <th class="px-4 py-3 font-bold text-heading">Đơn vị</th>
              <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
              <th v-if="canEdit" class="px-4 py-3 font-bold text-heading text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="relative">
            <!-- Loading Row -->
            <tr v-if="loading">
              <td colspan="7" class="py-16 text-center text-muted">
                <div class="flex flex-col items-center justify-center gap-2">
                  <Loader2 class="animate-spin text-(--lg-primary)" :size="24" />
                  <span class="text-xs font-medium">Đang tải dữ liệu người dùng...</span>
                </div>
              </td>
            </tr>
            <!-- In-Table Error Row (Thông báo bên trong bảng để không che mất bộ lọc và combobox cơ sở) -->
            <tr v-else-if="error">
              <td colspan="7" class="py-14 text-center">
                <div class="flex flex-col items-center justify-center gap-2 max-w-md mx-auto px-4">
                  <div class="h-10 w-10 rounded-full bg-amber-500/10 flex items-center justify-center text-amber-600 dark:text-amber-400">
                    <AlertCircle :size="22" />
                  </div>
                  <p class="text-xs font-bold text-heading mt-1">{{ error }}</p>
                  <p class="text-[11px] text-muted leading-relaxed">
                    Bạn có thể chọn cơ sở khác từ danh sách cơ sở phía trên để tiếp tục làm việc.
                  </p>
                  <button @click="loadData()" class="mt-2 px-3 py-1.5 bg-(--surface-input) hover:bg-(--surface-input-hover) border border-input rounded-lg text-xs font-bold text-heading transition-colors">
                    Thử tải lại
                  </button>
                </div>
              </td>
            </tr>
            <!-- Empty Row -->
            <tr v-else-if="filteredUsers.length === 0" class="bg-transparent">
              <td colspan="7" class="py-12 text-center text-muted">
                <p class="text-xs">Không tìm thấy người dùng nào trong cơ sở đã chọn.</p>
              </td>
            </tr>
            <!-- Data Rows -->
            <tr v-else v-for="user in pagedUsers" :key="user.maNguoiDung" class="hover:bg-(--surface-input)/50 transition-colors">
              <td class="px-4 py-3 font-medium">{{ user.maNguoiDung }}</td>
              <td class="px-4 py-3 font-bold text-heading">{{ user.hoTen }}</td>
              <td class="px-4 py-3">{{ user.email }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ user.tenVaiTro }}</span>
              </td>
              <td class="px-4 py-3 text-xs">{{ user.tenDonVi || 'N/A' }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center gap-1 px-2 py-1 rounded-md text-[10px] font-bold uppercase tracking-wider" :class="user.trangThai === 'hoat_dong' ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)'">
                  <CheckCircle2 v-if="user.trangThai === 'hoat_dong'" :size="12" />
                  <Lock v-else :size="12" />
                  {{ user.trangThai === 'hoat_dong' ? 'Hoạt động' : 'Bị khóa' }}
                </span>
              </td>
              <td v-if="canEdit" class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEditModal(user)" class="p-1.5 text-muted hover:text-(--lg-primary) hover:bg-(--lg-primary)/10 rounded-lg transition-colors" title="Chỉnh sửa"><Edit2 :size="16" /></button>
                  <button v-if="user.trangThai === 'hoat_dong'" @click="handleToggleLock(user)" class="p-1.5 text-muted hover:text-(--color-danger-text) hover:bg-(--color-danger-bg) rounded-lg transition-colors" title="Khóa tài khoản"><Lock :size="16" /></button>
                  <button v-else @click="handleToggleLock(user)" class="p-1.5 text-(--color-danger-text) hover:text-(--color-success-text) hover:bg-(--color-success-bg) rounded-lg transition-colors" title="Mở khóa tài khoản"><Unlock :size="16" /></button>
                  <button @click="handleResetPassword(user)" class="p-1.5 text-muted hover:text-(--color-warning-text) hover:bg-(--color-warning-bg) rounded-lg transition-colors" title="Đặt lại mật khẩu"><Key :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="p-4 bg-(--surface-card) flex items-center justify-between text-sm">
        <span class="text-muted">Hiển thị {{ pagedUsers.length }} / {{ totalItems }} người dùng</span>
        <div class="flex items-center gap-2">
          <button @click="prevPage" :disabled="currentPage === 1" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang trước</button>
          <span class="px-2 font-bold text-heading">Trang {{ currentPage }} / {{ totalPages }}</span>
          <button @click="nextPage" :disabled="currentPage >= totalPages" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang sau</button>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-lg surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-full">
        <div class="p-4 border-b border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">{{ modalMode === 'create' ? 'Thêm Người Dùng Mới' : 'Chỉnh Sửa Người Dùng' }}</h3>
          <button @click="closeModal" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted"><X :size="20" /></button>
        </div>
        <form @submit.prevent="submitForm" class="p-6 overflow-y-auto space-y-4">
          <div v-if="apiError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start">
            <AlertTriangle :size="16" class="shrink-0 mt-0.5" /><span>{{ apiError }}</span>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Họ và tên <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.hoTen" type="text" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Email <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.email" type="email" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Số điện thoại</label>
            <input v-model="formData.soDienThoai" type="text" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div v-if="modalMode === 'create'">
            <label class="block text-xs font-bold text-heading mb-1.5">Mật khẩu <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.matKhau" type="password" required minlength="8" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Vai trò <span class="text-(--color-danger-text)">*</span></label>
            <LmsSelect v-model="formData.maCodeVaiTro" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none">
              <option value="" disabled>-- Chọn vai trò --</option>
              <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
            </LmsSelect>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Đơn vị <span class="text-(--color-danger-text)">*</span></label>
            <LmsSelect v-model="formData.maDonVi" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none">
              <option value="" disabled>-- Chọn đơn vị --</option>
              <option v-for="org in orgsList" :key="org.maDonVi" :value="org.maDonVi">{{ org.tenDonVi }} ({{ org.capDonVi }})</option>
            </LmsSelect>
          </div>
        </form>
        <div class="p-4 border-t border-default bg-(--surface-card) flex justify-end gap-3">
          <button @click="closeModal" type="button" class="px-4 py-2 text-sm font-bold border border-input rounded-lg hover:bg-(--surface-input) transition-colors">Hủy</button>
          <button @click="submitForm" class="flex items-center justify-center gap-2 px-6 py-2 bg-(--lg-primary) text-white text-sm font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors min-w-[100px]">Lưu lại</button>
        </div>
      </div>
    </div>
    <!-- Import Modal -->
    <div v-if="showImportModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-lg surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-[90vh]">
        <div class="p-4 border-b border-default flex justify-between items-center bg-(--surface-card)">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <FileSpreadsheet :size="20" class="text-emerald-600" /> Nhập hoặc cập nhật người dùng từ Excel / CSV
          </h3>
          <button @click="closeImportModal" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted"><X :size="20" /></button>
        </div>
        <div class="p-6 space-y-4 overflow-y-auto">
          <!-- Import Success Detailed Card -->
          <div v-if="importResult && importResult.daLuu" class="p-4 bg-emerald-500/10 border border-emerald-500/30 rounded-xl space-y-3">
            <div class="flex items-center gap-2.5 text-emerald-600 dark:text-emerald-400 font-bold text-xs">
              <CheckCircle2 :size="18" class="shrink-0" />
              <span>Đã lưu thành công dữ liệu vào hệ thống!</span>
            </div>
            <div class="grid grid-cols-3 gap-2 text-center pt-1">
              <div class="p-2.5 rounded-lg bg-(--surface-card) border border-default">
                <p class="text-[11px] text-muted font-medium">Tổng số dòng</p>
                <p class="text-base font-extrabold text-heading">{{ importResult.soDongDaNhap }}</p>
              </div>
              <div class="p-2.5 rounded-lg bg-(--surface-card) border border-default">
                <p class="text-[11px] text-emerald-600 font-medium">Tạo mới</p>
                <p class="text-base font-extrabold text-emerald-600">+{{ importResult.soDongTaoMoi }}</p>
              </div>
              <div class="p-2.5 rounded-lg bg-(--surface-card) border border-default">
                <p class="text-[11px] text-blue-600 font-medium">Cập nhật</p>
                <p class="text-base font-extrabold text-blue-600">{{ importResult.soDongCapNhat }}</p>
              </div>
            </div>
          </div>

          <div v-else-if="importSuccessMsg" class="p-3 bg-(--color-success-bg) text-(--color-success-text) text-xs rounded-lg flex gap-2 items-center">
            <CheckCircle2 :size="16" class="shrink-0" /> <span>{{ importSuccessMsg }}</span>
          </div>
          <div v-if="importApiError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start">
            <AlertTriangle :size="16" class="shrink-0 mt-0.5" /> <span>{{ importApiError }}</span>
          </div>

          <p class="text-xs text-muted leading-relaxed">
            Email là khóa đối chiếu: email mới sẽ tạo tài khoản, email đã có trong phạm vi quản lý sẽ cập nhật các thông tin hợp lệ.
            Hệ thống hỗ trợ Giảng viên, Sinh viên và Giáo vụ từ file <code>.xlsx</code> hoặc <code>.csv</code>.
          </p>

          <div class="flex items-center justify-between p-3 bg-(--surface-input) rounded-xl border border-card text-xs">
            <span class="text-muted font-medium">Chưa có file chuẩn mẫu?</span>
            <button type="button" @click="downloadSampleTemplate" class="text-emerald-600 font-bold hover:underline flex items-center gap-1">
              <Download :size="14" /> Tải file mẫu (.csv)
            </button>
          </div>

          <div>
            <label class="block text-xs font-bold text-heading mb-1">Cơ sở trực thuộc áp dụng</label>
            <LmsSelect
              v-model="importTargetOrg"
              placeholder="Cơ sở mặc định của bạn"
              :options="importOrgOptions"
            />
          </div>

          <!-- File Upload Box -->
          <div
            class="border-2 border-dashed border-card hover:border-(--lg-primary) transition-colors rounded-xl p-6 flex flex-col items-center justify-center text-center cursor-pointer surface-input"
            @click="$refs.fileInput.click()"
          >
            <UploadCloud :size="36" class="text-muted mb-2" />
            <p class="text-xs font-bold text-heading">{{ importFile ? importFile.name : 'Nhấp để chọn file Excel (.xlsx, .csv)' }}</p>
            <span class="text-[11px] text-muted mt-1">{{ importFile ? `${(importFile.size / 1024).toFixed(1)} KB` : 'Hỗ trợ .xlsx, .csv (tối đa 10MB)' }}</span>
            <input ref="fileInput" type="file" accept=".xlsx,.csv" class="hidden" @change="handleFileUpload" />
          </div>

          <!-- Import Errors List if any -->
          <div v-if="importResult && importResult.chiTietLoi && importResult.chiTietLoi.length > 0" class="space-y-2 border border-(--color-danger-bg) rounded-xl p-3 bg-(--color-danger-bg)/10">
            <div class="flex items-center justify-between text-xs font-bold text-(--color-danger-text)">
              <span>Danh sách lỗi phát hiện ({{ importResult.chiTietLoi.length }} lỗi):</span>
            </div>
            <div class="max-h-36 overflow-y-auto space-y-1 pr-1 text-[11px]">
              <div v-for="(err, idx) in importResult.chiTietLoi" :key="idx" class="p-1.5 rounded bg-(--surface-card) border border-default text-body flex justify-between gap-2">
                <span class="font-bold text-heading shrink-0">Dòng {{ err.dong }}:</span>
                <span class="truncate text-muted" :title="err.email">Bản ghi: {{ err.email || 'N/A' }}</span>
                <span class="text-(--color-danger-text) text-right shrink-0">{{ err.lyDo }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="p-4 border-t border-default bg-(--surface-card) flex justify-between items-center gap-3">
          <button @click="closeImportModal" type="button" class="px-4 py-2 text-sm font-bold border border-input rounded-lg hover:bg-(--surface-input) transition-colors">Đóng</button>
          <div class="flex items-center gap-2">
            <button
              @click="submitImport(true)"
              :disabled="!importFile || importing"
              class="px-4 py-2 text-xs font-bold border border-emerald-600 text-emerald-600 rounded-lg hover:bg-emerald-50 dark:hover:bg-emerald-950/30 transition-colors disabled:opacity-50"
            >
              <Loader2 v-if="importing && isDryRun" class="animate-spin inline mr-1" :size="14" />
              Kiểm tra trước (Dry-run)
            </button>
            <button
              @click="submitImport(false)"
              :disabled="!importFile || importing"
              class="flex items-center justify-center gap-2 px-5 py-2 bg-emerald-600 text-white text-sm font-bold rounded-lg hover:bg-emerald-700 transition-colors disabled:opacity-50 shadow-sm"
            >
              <Loader2 v-if="importing && !isDryRun" class="animate-spin" :size="16" />
              <FileSpreadsheet v-else :size="16" />
              <span>{{ importing && !isDryRun ? 'Đang tải lên...' : 'Tải lên' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast Notification -->
    <transition name="fade">
      <div
        v-if="toastMsg"
        class="fixed bottom-6 right-6 z-50 flex items-center gap-3 px-5 py-3.5 bg-emerald-600 text-white font-medium text-sm rounded-2xl shadow-2xl backdrop-blur-md border border-emerald-400/30 animate-in slide-in-from-bottom-5 duration-300"
      >
        <div class="p-1 bg-white/20 rounded-full">
          <CheckCircle2 :size="20" class="text-white" />
        </div>
        <div class="flex flex-col">
          <span class="font-bold text-white text-xs">Thao tác thành công</span>
          <span class="text-emerald-50 text-xs">{{ toastMsg }}</span>
        </div>
        <button @click="toastMsg = ''" class="ml-3 p-1 hover:bg-white/20 rounded-lg text-emerald-100 transition-colors">
          <X :size="16" />
        </button>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, onUnmounted } from 'vue'
import { Search, Loader2, Plus, Edit2, Lock, Unlock, Key, CheckCircle2, AlertTriangle, AlertCircle, X, FileSpreadsheet, UploadCloud, Download } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { bghApi } from '@/services/bghApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole(['SuperAdmin', 'Admin', 'Principal']))
const canImport = computed(() => authStore.hasRole(['Principal', 'AcademicStaff']))

const loading = ref(false)
const searchLoading = ref(false)
const error = ref(null)

const keyword = ref('')
const orgFilter = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = 15
const totalItems = ref(0)
const serverTotalPages = ref(1)
let searchTimer = null

const showModal = ref(false)
const showImportModal = ref(false)
const importFile = ref(null)
const importTargetOrg = ref('')
const importSuccessMsg = ref('')
const importApiError = ref('')
const importing = ref(false)
const isDryRun = ref(false)
const importResult = ref(null)
const modalMode = ref('create')
const apiError = ref('')
const saving = ref(false)
const formData = ref({ maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' })

const toastMsg = ref('')
let toastTimer = null

function showToast(msg) {
  toastMsg.value = msg
  if (toastTimer) clearTimeout(toastTimer)
  toastTimer = setTimeout(() => {
    toastMsg.value = ''
  }, 4500)
}

onUnmounted(() => {
  if (toastTimer) clearTimeout(toastTimer)
  if (searchTimer) clearTimeout(searchTimer)
})

function downloadSampleTemplate() {
  const orgNames = orgsList.value.map(org => org.tenDonVi).filter(Boolean)
  const firstOrg = orgNames[0] || 'Dong Nai'
  const secondOrg = orgNames[1] || 'Ha Noi'
  const csvContent = '\uFEFF' +
    'Email,HoTen,MatKhau,MaCodeVaiTro,TenDonVi,SoDienThoai\n' +
    `giangvien.sample01@edulms.local,Nguyễn Văn Giảng,Teacher@123,Teacher,${firstOrg},0912345678\n` +
    `giangvien.sample02@edulms.local,Trần Thị Hướng,Teacher@123,Teacher,${secondOrg},0987654321\n` +
    `nhanvien.sample01@edulms.local,Lê Hoàng Nam,Staff@123,AcademicStaff,${firstOrg},0901234567\n` +
    `hocsinh.sample01@edulms.local,Phạm Minh Tuấn,Student@123,Student,,0933445566\n`
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', 'Mau_Import_NguoiDung.csv')
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const rolesList = ref([])
const orgsList = ref([])
const users = ref([])

const importOrgOptions = computed(() => [
  { value: '', label: 'Cơ sở mặc định của bạn' },
  ...orgsList.value.map(org => ({
    value: org.maDonVi,
    label: org.tenDonVi
  }))
])

async function loadData(isInitial = false) {
  if (isInitial) loading.value = true
  error.value = null
  try {
    const promises = [
      bghApi.getUsers({
        pageIndex: currentPage.value,
        pageSize,
        keyword: keyword.value.trim(),
        role: roleFilter.value,
        status: statusFilter.value,
        maDonVi: orgFilter.value || undefined,
      }),
    ]
    if (rolesList.value.length === 0) promises.push(bghApi.getRoles())
    if (orgsList.value.length === 0) promises.push(bghApi.getOrganizations())

    const results = await Promise.allSettled(promises)
    const userRes = results[0]

    if (userRes.status === 'fulfilled') {
      users.value = unwrapApiData(userRes.value) || []
      totalItems.value = userRes.value?.pagination?.totalItems ?? users.value.length
      serverTotalPages.value = Math.max(1, userRes.value?.pagination?.totalPages ?? 1)
    } else {
      users.value = []
      totalItems.value = 0
      serverTotalPages.value = 1
      error.value = userRes.reason?.message || 'Bạn không có quyền truy cập dữ liệu của cơ sở này.'
    }

    if (results.length > 1 && results[1].status === 'fulfilled') {
      rolesList.value = (unwrapApiData(results[1].value) || []).map(r => ({
        maVaiTro: r.maVaiTro,
        maCodeVaiTro: r.maCodeVaiTro,
        tenVaiTro: r.tenVaiTro,
      }))
    }
    if (results.length > 2 && results[2].status === 'fulfilled') {
      orgsList.value = (unwrapApiData(results[2].value) || []).map(o => ({
        maDonVi: o.id,
        tenDonVi: o.name,
        capDonVi: o.organizationLevel,
      }))
    }
  } catch (e) {
    users.value = []
    error.value = e?.message || 'Bạn không có quyền truy cập dữ liệu của cơ sở này.'
  } finally {
    loading.value = false
    searchLoading.value = false
  }
}

function onSearchInput(e) {
  searchLoading.value = true
  if (e?.target?.value !== undefined) {
    keyword.value = e.target.value
  }
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    handleFilter()
  }, 250)
}

watch([keyword, orgFilter, roleFilter, statusFilter], () => {
  searchLoading.value = true
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    handleFilter()
  }, 250)
})

const filteredUsers = computed(() => users.value)

const totalPages = computed(() => serverTotalPages.value)
const pagedUsers = computed(() => filteredUsers.value)

function handleFilter() {
  currentPage.value = 1
  bghApi.invalidate('/api/bgh/users')
  loadData()
}
function prevPage() {
  if (currentPage.value > 1) {
    currentPage.value--
    loadData()
  }
}
function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
    loadData()
  }
}



function openImportModal() {
  importFile.value = null
  importTargetOrg.value = orgFilter.value || ''
  importSuccessMsg.value = ''
  importApiError.value = ''
  importResult.value = null
  importing.value = false
  showImportModal.value = true
}

function closeImportModal() {
  showImportModal.value = false
}

function handleFileUpload(e) {
  const files = e.target?.files
  if (files && files.length > 0) {
    importFile.value = files[0]
    importSuccessMsg.value = ''
    importApiError.value = ''
    importResult.value = null
  }
}

async function submitImport(dryRun = false) {
  if (!importFile.value) return
  importing.value = true
  isDryRun.value = dryRun
  importApiError.value = ''
  importSuccessMsg.value = ''
  importResult.value = null

  try {
    const rawRes = await bghApi.importTeacherPersonnel(importFile.value, {
      dryRun,
      defaultMaDonVi: importTargetOrg.value ? parseInt(importTargetOrg.value, 10) : null,
    })
    const res = unwrapApiData(rawRes)
    importResult.value = res

    if (res.daLuu) {
      importSuccessMsg.value = `Hoàn tất ${res.soDongDaNhap} tài khoản: tạo mới ${res.soDongTaoMoi}, cập nhật ${res.soDongCapNhat}.`
      showToast(`Đã import thành công ${res.soDongDaNhap} tài khoản (Thêm mới: ${res.soDongTaoMoi}, Cập nhật: ${res.soDongCapNhat}).`)
      bghApi.invalidate('/api/bgh/users')
      await loadData()
    } else if (dryRun) {
      if (res.soDongLoi === 0) {
        importSuccessMsg.value = `Kiểm tra hợp lệ: ${res.soDongTaoMoi} tài khoản sẽ tạo mới, ${res.soDongCapNhat} tài khoản sẽ cập nhật.`
      } else {
        importApiError.value = `Phát hiện ${res.soDongLoi} / ${res.tongSoDong} dòng dữ liệu không hợp lệ. Vui lòng kiểm tra danh sách bên dưới.`
      }
    } else {
      if (res.soDongLoi > 0) {
        importApiError.value = `File còn ${res.soDongLoi} lỗi. Hệ thống chưa lưu dữ liệu nào.`
      }
    }
  } catch (err) {
    importApiError.value = err?.response?.data?.message || err?.message || 'Lỗi khi nhập dữ liệu từ Excel.'
  } finally {
    importing.value = false
  }
}

function openCreateModal() {
  if (!canEdit.value) return
  modalMode.value = 'create'
  formData.value = { maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' }
  apiError.value = ''
  showModal.value = true
}

function openEditModal(user) {
  if (!canEdit.value) return
  modalMode.value = 'edit'
  apiError.value = ''
  formData.value = {
    maNguoiDung: user.maNguoiDung,
    hoTen: user.hoTen,
    email: user.email,
    soDienThoai: user.soDienThoai || '',
    matKhau: '',
    maCodeVaiTro: user.vaiTroChinh,
    maDonVi: user.maDonVi
  }
  showModal.value = true
}

function closeModal() { showModal.value = false }

async function submitForm() {
  if (!canEdit.value) return
  if (!formData.value.hoTen || !formData.value.email || !formData.value.maCodeVaiTro || !formData.value.maDonVi) {
    apiError.value = 'Vui lòng điền đầy đủ các trường bắt buộc (*).'
    return
  }
  if (modalMode.value === 'create' && !formData.value.matKhau) {
    apiError.value = 'Vui lòng nhập mật khẩu.'
    return
  }
  apiError.value = ''
  saving.value = true
  try {
    if (modalMode.value === 'create') {
      await apiRequest('/api/admin/users', {
        method: 'POST',
        body: JSON.stringify({
          hoTen: formData.value.hoTen,
          email: formData.value.email,
          soDienThoai: formData.value.soDienThoai,
          matKhau: formData.value.matKhau,
          maCodeVaiTro: formData.value.maCodeVaiTro,
          maDonVi: parseInt(formData.value.maDonVi),
        })
      })
    } else {
      await apiRequest(`/api/admin/users/${formData.value.maNguoiDung}`, {
        method: 'PUT',
        body: JSON.stringify({
          hoTen: formData.value.hoTen,
          email: formData.value.email,
          soDienThoai: formData.value.soDienThoai,
          maCodeVaiTro: formData.value.maCodeVaiTro,
          maDonVi: parseInt(formData.value.maDonVi),
        })
      })
    }
    bghApi.invalidate('/api/bgh/users')
    closeModal()
    await loadData()
  } catch (e) {
    apiError.value = e?.message || 'Lỗi lưu dữ liệu'
  } finally {
    saving.value = false
  }
}

async function handleToggleLock(user) {
  if (!canEdit.value) return
  const isLocking = user.trangThai === 'hoat_dong'
  const actionText = isLocking ? 'khóa' : 'mở khóa'
  if (!confirm(`Bạn có chắc chắn muốn ${actionText} tài khoản "${user.email}"?`)) return

  try {
    const endpoint = `/api/admin/users/${user.maNguoiDung}/${isLocking ? 'lock' : 'unlock'}`
    const body = isLocking ? JSON.stringify({ reason: 'Quản trị viên thao tác khóa tài khoản' }) : null
    await apiRequest(endpoint, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body
    })
    bghApi.invalidate('/api/bgh/users')
    await loadData()
  } catch (e) {
    alert(e?.message || `Lỗi ${actionText} tài khoản`)
  }
}

async function handleResetPassword(user) {
  if (!canEdit.value) return
  const newPassword = prompt(`Nhập mật khẩu mới cho ${user.email} (tối thiểu 8 ký tự):`)
  if (!newPassword) return
  if (newPassword.length < 8) {
    alert('Mật khẩu phải có tối thiểu 8 ký tự.')
    return
  }
  try {
    await apiRequest(`/api/admin/users/${user.maNguoiDung}/reset-password`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ matKhauMoi: newPassword })
    })
    alert('Đặt lại mật khẩu thành công!')
    bghApi.invalidate('/api/bgh/users')
  } catch (e) {
    alert(e?.message || 'Lỗi đặt lại mật khẩu')
  }
}

onMounted(() => { loadData(true) })
onUnmounted(() => clearTimeout(searchTimer))
</script>
