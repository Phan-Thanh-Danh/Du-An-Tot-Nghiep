<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <div v-if="loading" class="flex-1 p-4">
      <SkeletonTable :rows="8" :columns="6" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Quản lý Người Dùng</h2>
        <p class="text-xs text-muted mt-1">Danh sách tài khoản trong cơ sở trực thuộc</p>
      </div>
    </div>

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
            </tr>
          </thead>
          <tbody class="relative">
            <tr v-if="!loading && !searchLoading && filteredUsers.length === 0" class="bg-transparent">
              <td colspan="7" class="py-12 text-center text-muted"><p>Không tìm thấy người dùng nào.</p></td>
            </tr>
            <tr v-for="user in pagedUsers" :key="user.maNguoiDung" class="hover:bg-(--surface-input)/50 transition-colors">
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

    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, onUnmounted } from 'vue'
import { unwrapApiData } from '@/services/apiClient'
import { Search, Loader2, Lock, CheckCircle2, AlertCircle } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { bghApi } from '@/services/bghApi'

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

const rolesList = ref([])
const orgsList = ref([])
const users = ref([])

async function loadData(isInitial = false) {
  if (isInitial) loading.value = true
  error.value = null
  try {
    const [userRes, roleRes, orgRes] = await Promise.all([
      bghApi.getUsers({
        pageIndex: currentPage.value,
        pageSize,
        keyword: keyword.value.trim(),
        role: roleFilter.value,
        status: statusFilter.value,
      }),
      bghApi.getRoles(),
      bghApi.getOrganizations(),
    ])
    users.value = unwrapApiData(userRes) || []
    totalItems.value = userRes?.pagination?.totalItems ?? users.value.length
    serverTotalPages.value = Math.max(1, userRes?.pagination?.totalPages ?? 1)
    rolesList.value = (unwrapApiData(roleRes) || []).map(r => ({ maVaiTro: r.maVaiTro, maCodeVaiTro: r.maCodeVaiTro, tenVaiTro: r.tenVaiTro }))
    orgsList.value = (unwrapApiData(orgRes) || []).map(o => ({ maDonVi: o.id, tenDonVi: o.name, capDonVi: o.organizationLevel }))
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu người dùng'
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

watch([keyword, roleFilter, statusFilter], () => {
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

onMounted(() => { loadData(true) })
onUnmounted(() => clearTimeout(searchTimer))
</script>
