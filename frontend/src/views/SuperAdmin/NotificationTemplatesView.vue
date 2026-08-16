<script setup>
import { ref, onMounted, reactive, watch } from 'vue'
import { useRouter } from 'vue-router'
import { Search, Play, Pause, Edit, Trash2, Plus, Loader2 } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'
import { superAdminApi } from '@/services/superAdminApi'

const router = useRouter()
const popupStore = usePopupStore()

const templates = ref([])
const loading = ref(false)
const confirmAction = ref(null)

const queryParams = reactive({
  SearchTerm: '',
  Category: '',
  Status: '',
  PageIndex: 1,
  PageSize: 20
})

const paginationInfo = ref({
  totalItems: 0,
  totalPages: 0
})

const loadTemplates = async () => {
  try {
    loading.value = true
    const res = await superAdminApi.getNotificationTemplates(queryParams)
    if (res.success && res.data) {
      templates.value = res.data.items || []
      paginationInfo.value = {
        totalItems: res.data.totalCount,
        totalPages: res.data.totalPages
      }
    }
  } catch (error) {
    console.error('Lỗi khi tải mẫu thông báo:', error)
    popupStore.error('Lỗi', 'Không thể tải danh sách mẫu thông báo')
  } finally {
    loading.value = false
  }
}

// Watch filters
let searchTimeout
watch(() => queryParams.SearchTerm, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    queryParams.PageIndex = 1
    loadTemplates()
  }, 500)
})

const onFilterChange = () => {
  queryParams.PageIndex = 1
  loadTemplates()
}

onMounted(() => {
  loadTemplates()
})

const toggleStatus = (tpl) => {
  const isPause = tpl.dangHoatDong
  confirmAction.value = {
    title: isPause ? 'Tạm dừng mẫu?' : 'Kích hoạt mẫu?',
    message: isPause ? `Mẫu "${tpl.tenMau}" sẽ không được dùng để gửi tự động nữa.` : `Mẫu "${tpl.tenMau}" sẽ sẵn sàng sử dụng.`,
    label: isPause ? 'Tạm dừng' : 'Kích hoạt',
    variant: isPause ? 'danger' : 'success',
    run: async () => {
      try {
        if (isPause) {
          await superAdminApi.deactivateNotificationTemplate(tpl.maMauThongBao)
        } else {
          await superAdminApi.activateNotificationTemplate(tpl.maMauThongBao)
        }
        popupStore.success('Thành công', 'Trạng thái mẫu đã được cập nhật')
        loadTemplates()
      } catch (error) {
        popupStore.error('Lỗi', 'Cập nhật trạng thái thất bại')
      } finally {
        confirmAction.value = null
      }
    }
  }
}

const deleteTemplate = (tpl) => {
  if (tpl.laHeThong) {
    popupStore.warning('Không hợp lệ', 'Không thể xóa mẫu hệ thống mặc định.')
    return
  }
  
  confirmAction.value = {
    title: 'Xóa mẫu thông báo?',
    message: `Bạn có chắc chắn muốn xóa mẫu "${tpl.tenMau}"? Hành động này không thể hoàn tác.`,
    label: 'Xóa mẫu',
    variant: 'danger',
    run: async () => {
      try {
        await superAdminApi.deleteNotificationTemplate(tpl.maMauThongBao)
        popupStore.success('Thành công', 'Đã xóa mẫu thông báo')
        loadTemplates()
      } catch (error) {
        popupStore.error('Lỗi', 'Xóa mẫu thông báo thất bại')
      } finally {
        confirmAction.value = null
      }
    }
  }
}

const goToCreate = () => {
  router.push('/super-admin/notifications/templates/create')
}

const goToEdit = (id) => {
  router.push(`/super-admin/notifications/templates/${id}/edit`)
}
</script>

<template>
  <div class="templates-view max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact" class="flex flex-col sm:flex-row justify-between sm:items-center gap-4">
      <div>
        <h1 class="text-2xl font-bold text-(--text-heading)">Mẫu thông báo</h1>
        <p class="text-(--text-body)">Quản lý các mẫu thông báo gửi tự động từ hệ thống.</p>
      </div>
      <GlassButton variant="primary" @click="goToCreate" class="flex items-center gap-2">
        <Plus :size="18" /> Tạo mẫu mới
      </GlassButton>
    </GlassPanel>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Tổng mẫu</p>
        <strong class="text-2xl text-(--text-heading)">{{ paginationInfo.totalItems }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Đang hoạt động</p>
        <strong class="text-2xl text-(--text-heading)">{{ templates.filter(t => t.dangHoatDong).length }} (trên trang)</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Tạm dừng</p>
        <strong class="text-2xl text-(--text-heading)">{{ templates.filter(t => !t.dangHoatDong).length }} (trên trang)</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-col md:flex-row gap-4">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) flex-1">
          <Search :size="16" class="text-(--text-muted)" />
          <input 
            v-model="queryParams.SearchTerm" 
            type="text" 
            placeholder="Tìm kiếm mã hoặc tên mẫu..." 
            class="bg-transparent border-none outline-none w-full text-(--text-primary)" 
          />
        </label>
        
        <select v-model="queryParams.Category" @change="onFilterChange" class="lg-control min-w-[150px]">
          <option value="">Tất cả danh mục</option>
          <option value="hoc_vu">Học vụ</option>
          <option value="tai_chinh">Tài chính</option>
          <option value="he_thong">Hệ thống</option>
        </select>
        
        <select v-model="queryParams.Status" @change="onFilterChange" class="lg-control min-w-[150px]">
          <option value="">Tất cả trạng thái</option>
          <option value="active">Đang hoạt động</option>
          <option value="inactive">Tạm dừng</option>
        </select>
      </div>

      <div v-if="loading" class="p-12 flex justify-center items-center text-(--text-muted)">
        <Loader2 class="w-8 h-8 animate-spin" />
      </div>

      <TableShell v-else>
        <table>
          <thead>
            <tr>
              <th>Mã mẫu</th>
              <th>Tên mẫu</th>
              <th>Danh mục</th>
              <th>Trạng thái</th>
              <th>Loại</th>
              <th class="w-[120px] text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="templates.length === 0">
              <td colspan="6" class="text-center py-8 text-(--text-muted)">
                Không tìm thấy mẫu thông báo nào.
              </td>
            </tr>
            <tr v-for="tpl in templates" :key="tpl.maMauThongBao">
              <td class="font-mono text-sm text-(--text-muted)">{{ tpl.maMau || `ID-${tpl.maMauThongBao}` }}</td>
              <td class="font-medium">
                {{ tpl.tenMau }}
                <div class="text-xs text-(--text-muted) font-normal mt-1">{{ tpl.tieuDeMau }}</div>
              </td>
              <td><GlassBadge variant="info" size="sm">{{ tpl.loaiThongBao }}</GlassBadge></td>
              <td>
                <GlassBadge :variant="tpl.dangHoatDong ? 'success' : 'neutral'" size="sm">
                  {{ tpl.dangHoatDong ? 'Đang hoạt động' : 'Tạm dừng' }}
                </GlassBadge>
              </td>
              <td>
                <GlassBadge v-if="tpl.laHeThong" variant="warning" size="sm">Hệ thống</GlassBadge>
                <GlassBadge v-else variant="neutral" size="sm">Tùy chỉnh</GlassBadge>
              </td>
              <td>
                <div class="flex justify-end gap-1">
                  <GlassButton variant="ghost" size="sm" @click="toggleStatus(tpl)" :title="tpl.dangHoatDong ? 'Tạm dừng' : 'Kích hoạt'">
                    <component :is="tpl.dangHoatDong ? Pause : Play" :size="16" class="text-(--text-muted)" />
                  </GlassButton>
                  <GlassButton variant="ghost" size="sm" @click="goToEdit(tpl.maMauThongBao)" title="Sửa">
                    <Edit :size="16" class="text-blue-500" />
                  </GlassButton>
                  <GlassButton 
                    v-if="!tpl.laHeThong" 
                    variant="ghost" 
                    size="sm" 
                    @click="deleteTemplate(tpl)" 
                    title="Xóa"
                  >
                    <Trash2 :size="16" class="text-red-500" />
                  </GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>

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
