<script setup>
import { ref } from 'vue'
import { Search, Play, Pause } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const confirmAction = ref(null)

const templates = ref([
  { id: 'TPL-01', name: 'Nhắc nhở học phí', category: 'tai_chinh', status: 'active', vars: '{{StudentName}}, {{Amount}}', updatedAt: '2026-06-25' },
  { id: 'TPL-02', name: 'Duyệt đơn nghỉ phép', category: 'hoc_vu', status: 'active', vars: '{{StudentName}}, {{Date}}', updatedAt: '2026-06-28' },
  { id: 'TPL-03', name: 'Cảnh báo vắng học', category: 'hoc_vu', status: 'paused', vars: '{{StudentName}}, {{Subject}}, {{Percentage}}', updatedAt: '2026-05-10' },
  { id: 'TPL-04', name: 'Thông báo lịch thi', category: 'hoc_vu', status: 'active', vars: '{{ExamDate}}, {{Room}}', updatedAt: '2026-06-01' }
])

const toggleStatus = (tpl) => {
  const isPause = tpl.status === 'active'
  confirmAction.value = {
    title: isPause ? 'Tạm dừng mẫu?' : 'Kích hoạt mẫu?',
    message: isPause ? `Mẫu "${tpl.name}" sẽ không được dùng để gửi tự động nữa.` : `Mẫu "${tpl.name}" sẽ sẵn sàng sử dụng.`,
    label: isPause ? 'Tạm dừng' : 'Kích hoạt',
    variant: isPause ? 'danger' : 'success',
    run: () => {
      tpl.status = isPause ? 'paused' : 'active'
      confirmAction.value = null
      popupStore.success('Thành công', 'Trạng thái mẫu đã được cập nhật')
    }
  }
}
</script>

<template>
  <div class="templates-view max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact" class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-(--text-heading)">Mẫu thông báo (Templates)</h1>
        <p class="text-(--text-body)">Quản lý các mẫu thông báo gửi tự động từ hệ thống.</p>
      </div>
      <GlassButton variant="primary">Tạo mẫu mới</GlassButton>
    </GlassPanel>

    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Tổng mẫu</p>
        <strong class="text-2xl text-(--text-heading)">{{ templates.length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Đang hoạt động</p>
        <strong class="text-2xl text-(--text-heading)">{{ templates.filter(t => t.status === 'active').length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[80px]">
        <p class="text-sm text-(--text-muted)">Tạm dừng</p>
        <strong class="text-2xl text-(--text-heading)">{{ templates.filter(t => t.status === 'paused').length }}</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex gap-4">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) flex-1">
          <Search :size="16" />
          <input type="text" placeholder="Tìm kiếm mẫu..." class="bg-transparent border-none outline-none w-full" />
        </label>
        <select class="lg-control min-w-[150px]">
          <option>Tất cả danh mục</option>
        </select>
        <select class="lg-control min-w-[150px]">
          <option>Tất cả trạng thái</option>
        </select>
      </div>

      <TableShell>
        <table>
          <thead>
            <tr>
              <th>Mã mẫu</th>
              <th>Tên mẫu</th>
              <th>Danh mục</th>
              <th>Trạng thái</th>
              <th>Biến sử dụng (Variables)</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="tpl in templates" :key="tpl.id">
              <td>{{ tpl.id }}</td>
              <td class="font-medium">{{ tpl.name }}</td>
              <td><GlassBadge variant="info" size="sm">{{ tpl.category }}</GlassBadge></td>
              <td>
                <GlassBadge :variant="tpl.status === 'active' ? 'success' : 'neutral'" size="sm">
                  {{ tpl.status === 'active' ? 'Đang hoạt động' : 'Tạm dừng' }}
                </GlassBadge>
              </td>
              <td class="text-(--text-muted) text-sm">{{ tpl.vars }}</td>
              <td>
                <div class="flex gap-2">
                  <GlassButton variant="ghost" size="sm" @click="toggleStatus(tpl)">
                    <component :is="tpl.status === 'active' ? Pause : Play" :size="14" />
                  </GlassButton>
                  <GlassButton variant="ghost" size="sm">Sửa</GlassButton>
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
