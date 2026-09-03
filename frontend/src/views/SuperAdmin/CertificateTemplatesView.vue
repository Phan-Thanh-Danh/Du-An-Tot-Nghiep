<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Code2, Eye, Plus, Power, Search, Sparkles } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { certificateTemplateApi } from '@/services/certificateTemplateApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const isBgh = computed(() => route.path.startsWith('/bgh'))

const loading = ref(true)
const templates = ref([])
const searchQuery = ref('')
const confirmDisable = ref(null)
const confirmDelete = ref(null)

const filteredTemplates = computed(() => {
  if (!searchQuery.value) return templates.value
  const q = searchQuery.value.toLowerCase()
  return templates.value.filter((t) => (t.tenMau || '').toLowerCase().includes(q))
})

async function loadTemplates() {
  loading.value = true
  try {
    const data = await certificateTemplateApi.getTemplates({ pageIndex: 1, pageSize: 100 })
    templates.value = data?.items ?? data?.Items ?? []
  } catch (err) {
    templates.value = []
    popupStore.error('Lỗi', err?.message || 'Không tải được danh sách mẫu giấy khen.')
  } finally {
    loading.value = false
  }
}

function openCreate() {
  if (isBgh.value) {
    router.push('/bgh/awards/certificate-templates/new')
  } else {
    router.push('/super-admin/awards/certificate-templates/new')
  }
}

function openEdit(template) {
  if (isBgh.value) {
    router.push(`/bgh/awards/certificate-templates/${template.maMauBangKhen}/edit`)
  } else {
    router.push(`/super-admin/awards/certificate-templates/${template.maMauBangKhen}/edit`)
  }
}

function openAiEdit(template) {
  if (isBgh.value) {
    router.push(`/bgh/awards/certificate-templates/${template.maMauBangKhen}/edit?aiPrompt=true`)
  } else {
    router.push(`/super-admin/awards/certificate-templates/${template.maMauBangKhen}/edit?aiPrompt=true`)
  }
}

async function toggleActive(template) {
  try {
    await certificateTemplateApi.disableTemplate(template.maMauBangKhen)
    popupStore.success('Thành công', 'Đã vô hiệu hóa mẫu giấy khen.')
    await loadTemplates()
    confirmDisable.value = null
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Vô hiệu hóa mẫu thất bại.')
  }
}

async function deleteTemplate(template) {
  try {
    await certificateTemplateApi.deleteTemplate(template.maMauBangKhen)
    popupStore.success('Thành công', 'Đã xóa mẫu giấy khen.')
    await loadTemplates()
    confirmDelete.value = null
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Xóa mẫu thất bại.')
  }
}

onMounted(loadTemplates)

watch(() => route.path, () => {
  if (route.path === '/super-admin/awards/certificate-templates' || route.path === '/bgh/awards/certificate-templates') {
    loadTemplates()
  }
})
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <div class="flex flex-wrap items-center gap-2">
          <h2 class="text-heading text-lg font-bold">Cấu hình giấy khen</h2>
          <GlassBadge variant="secondary">{{ isBgh ? 'BGH Cơ sở' : 'Super Admin' }}</GlassBadge>
        </div>
        <p class="text-label mt-0.5 text-sm">
          {{ isBgh ? 'Xem mẫu chuẩn của Toàn trường hoặc tự thiết kế mẫu giấy khen riêng cho Cơ sở.' : 'Custom mẫu giấy khen bằng HTML/CSS, xem trước trực tiếp và cấp phát chứng nhận theo mẫu.' }}
        </p>
      </div>
      <GlassButton variant="primary" @click="openCreate">
        <template #leading><Plus :size="16" /></template>
        {{ isBgh ? 'Tạo mẫu cho cơ sở' : 'Tạo mẫu mới' }}
      </GlassButton>
    </div>

    <div class="flex flex-wrap gap-3">
      <label class="flex h-10 flex-1 min-w-[220px] items-center gap-2 rounded-lg border border-(--border-input) bg-(--surface-input) px-3 transition-shadow focus-within:ring-2 focus-within:ring-(--border-focus)">
        <Search class="h-4 w-4 text-(--text-muted)" />
        <input v-model="searchQuery" type="text" placeholder="Tìm theo tên mẫu..." class="w-full bg-transparent text-sm text-(--text-body) outline-none" />
      </label>
    </div>

    <LoadingSkeleton v-if="loading" :lines="8" />

    <div v-else-if="filteredTemplates.length === 0" class="border border-dashed border-(--border-card) rounded-2xl">
      <EmptyState
        title="Chưa có mẫu giấy khen"
        description="Tạo mẫu đầu tiên bằng HTML/CSS để tùy biến giấy khen theo ý muốn."
      >
        <GlassButton variant="primary" size="sm" @click="openCreate">
          <template #leading><Plus :size="14" /></template>
          {{ isBgh ? 'Tạo mẫu cho cơ sở' : 'Tạo mẫu mới' }}
        </GlassButton>
      </EmptyState>
    </div>

    <div v-else class="overflow-hidden rounded-2xl border border-(--border-card) shadow-sm">
      <table class="w-full text-left text-sm">
        <thead class="bg-slate-50 text-xs font-bold uppercase text-(--text-muted) dark:bg-slate-800/50">
          <tr>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Tên mẫu</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Phạm vi</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Loại mẫu</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Chế độ</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Kích thước</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Trạng thái</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card)">Cập nhật</th>
            <th scope="col" class="px-4 py-3 border-b border-(--border-card) text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-(--border-card)">
          <tr v-for="t in filteredTemplates" :key="t.maMauBangKhen" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
            <td class="px-4 py-3">
              <div class="font-bold text-(--text-heading)">{{ t.tenMau }}</div>
              <div v-if="t.tenNguoiTao" class="text-xs text-(--text-placeholder)">tạo bởi {{ t.tenNguoiTao }}</div>
            </td>
            <td class="px-4 py-3">
              <GlassBadge v-if="t.isRootTemplate || t.maDonVi === 1 || !t.maDonVi" variant="info" size="sm">
                🌐 Toàn trường
              </GlassBadge>
              <GlassBadge v-else variant="success" size="sm">
                📍 {{ t.tenDonVi || 'Cơ sở' }}
              </GlassBadge>
            </td>
            <td class="px-4 py-3 text-xs font-mono text-(--text-muted)">{{ t.loaiMau }}</td>
            <td class="px-4 py-3">
              <GlassBadge :variant="t.mode === 'html' ? 'info' : 'secondary'" size="sm">
                {{ t.mode === 'html' ? 'HTML/CSS' : 'Vị trí field' }}
              </GlassBadge>
            </td>
            <td class="px-4 py-3 text-xs text-(--text-muted)">
              {{ t.chieuRong }}×{{ t.chieuCao }}px
              <div>{{ t.huongGiay === 'A4_NGANG' ? 'A4 ngang' : 'A4 dọc' }}</div>
            </td>
            <td class="px-4 py-3">
              <GlassBadge :variant="t.conHoatDong ? 'success' : 'secondary'" size="sm">
                {{ t.conHoatDong ? 'Đang hoạt động' : 'Tạm ẩn' }}
              </GlassBadge>
            </td>
            <td class="px-4 py-3 text-xs text-(--text-muted)">{{ t.ngayCapNhat || t.ngayTao ? (t.ngayCapNhat || t.ngayTao).slice(0, 10) : '—' }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center justify-end gap-2">
                <!-- BGH xem mẫu Root -->
                <template v-if="isBgh && (t.isRootTemplate || t.maDonVi === 1 || !t.maDonVi)">
                  <button
                    type="button"
                    @click.stop="openAiEdit(t)"
                    class="px-2.5 py-1 rounded-lg bg-blue-500/10 hover:bg-blue-500/20 text-blue-400 border border-blue-500/30 text-xs font-semibold flex items-center gap-1.5 transition-colors cursor-pointer"
                  >
                    <Sparkles :size="13" />
                    <span>AI Sửa mẫu</span>
                  </button>
                  <GlassButton variant="secondary" size="sm" @click="openEdit(t)">
                    <template #leading><Eye :size="13" /></template>
                    Xem & Sao chép
                  </GlassButton>
                </template>
                <!-- Mẫu của cơ sở hoặc SuperAdmin -->
                <template v-else>
                  <button
                    type="button"
                    @click.stop="openAiEdit(t)"
                    class="px-2.5 py-1 rounded-lg bg-blue-500/10 hover:bg-blue-500/20 text-blue-400 border border-blue-500/30 text-xs font-semibold flex items-center gap-1.5 transition-colors cursor-pointer"
                  >
                    <Sparkles :size="13" />
                    <span>AI Sửa mẫu</span>
                  </button>
                  <GlassButton variant="secondary" size="sm" @click="openEdit(t)">
                    <template #leading><Code2 :size="13" /></template>
                    Sửa
                  </GlassButton>
                  <GlassButton v-if="t.conHoatDong" variant="ghost" size="sm" @click="confirmDisable = t">
                    <template #leading><Power :size="13" /></template>
                    Tạm ẩn
                  </GlassButton>
                  <GlassButton v-else variant="secondary" size="sm" @click="confirmDisable = t">
                    <template #leading><Power :size="13" /></template>
                    Kích hoạt
                  </GlassButton>
                  <GlassButton variant="danger" size="sm" @click="confirmDelete = t">
                    <template #leading>🗑️</template>
                    Xóa
                  </GlassButton>
                </template>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <ConfirmActionDialog
      v-if="confirmDisable"
      :model-value="true"
      title="Tạm ẩn mẫu giấy khen"
      :message="`Bạn có chắc muốn tạm ẩn mẫu &quot;${confirmDisable.tenMau}&quot;? Mẫu bị tạm ẩn sẽ không dùng để cấp phát chứng nhận được.`"
      confirm-label="Tạm ẩn"
      variant="danger"
      @confirm="toggleActive(confirmDisable)"
      @cancel="confirmDisable = null"
    />

    <ConfirmActionDialog
      v-if="confirmDelete"
      :model-value="true"
      title="Xóa mẫu giấy khen"
      :message="`Bạn có chắc muốn xóa mẫu &quot;${confirmDelete.tenMau}&quot;? Hành động này không thể hoàn tác.`"
      confirm-label="Xóa"
      variant="danger"
      @confirm="deleteTemplate(confirmDelete)"
      @cancel="confirmDelete = null"
    />
  </div>
</template>
