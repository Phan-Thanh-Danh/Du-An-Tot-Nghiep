<script setup>
import { ref, computed } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import { Send, Eye, Save } from 'lucide-vue-next'

const props = defineProps({
  loading: Boolean,
  templates: Array
})

const emit = defineEmits(['preview', 'submit', 'template-selected'])

const form = ref({
  tieuDe: '',
  noiDung: '',
  loaiThongBao: 'HOC_VU',
  doUuTien: 'BINH_THUONG',
  loaiNguoiNhan: 'ALL',
  maDonVi: null,
  maKhoaHoc: null,
  maLopHoc: null,
  maHocKy: null,
  danhSachNguoiNhan: [],
})

const isValid = computed(() => {
  return form.value.tieuDe.trim() !== '' && form.value.noiDung.trim() !== ''
})

const handlePreview = () => {
  if (isValid.value) {
    emit('preview', form.value)
  }
}

const handleSubmit = () => {
  if (isValid.value) {
    emit('submit', form.value)
  }
}
</script>

<template>
  <GlassPanel variant="strong" padding="comfortable">
    <template #header>
      <h2 class="text-lg font-semibold text-[var(--text-heading)]">Soạn thông báo mới</h2>
    </template>
    
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-[var(--text-label)] mb-1">Loại thông báo</label>
        <select v-model="form.loaiThongBao" class="w-full h-10 px-3 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)] text-[var(--text-body)] focus:ring-2 focus:ring-[var(--focus-ring)]">
          <option value="HOC_VU">Học vụ</option>
          <option value="HOC_PHI">Học phí</option>
          <option value="HE_THONG">Hệ thống</option>
          <option value="KHAC">Khác</option>
        </select>
      </div>

      <div>
        <label class="block text-sm font-medium text-[var(--text-label)] mb-1">Độ ưu tiên</label>
        <select v-model="form.doUuTien" class="w-full h-10 px-3 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)] text-[var(--text-body)] focus:ring-2 focus:ring-[var(--focus-ring)]">
          <option value="THAP">Thấp</option>
          <option value="BINH_THUONG">Bình thường</option>
          <option value="QUAN_TRONG">Quan trọng</option>
          <option value="KHAN_CAP">Khẩn cấp</option>
        </select>
      </div>
      
      <div>
        <label class="block text-sm font-medium text-[var(--text-label)] mb-1">Phạm vi người nhận</label>
        <select v-model="form.loaiNguoiNhan" class="w-full h-10 px-3 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)] text-[var(--text-body)] focus:ring-2 focus:ring-[var(--focus-ring)]">
          <option value="ALL">Tất cả người dùng</option>
          <option value="SCOPE">Theo phạm vi (Cơ sở, Khóa, Lớp...)</option>
          <option value="MANUAL">Thủ công (Nhập danh sách)</option>
        </select>
      </div>

      <div v-if="form.loaiNguoiNhan === 'SCOPE'" class="grid grid-cols-2 gap-4">
        <!-- Add scope selectors here in the future -->
        <GlassInput v-model="form.maDonVi" type="number" placeholder="Mã cơ sở (ID)" />
        <GlassInput v-model="form.maHocKy" type="number" placeholder="Mã học kỳ (ID)" />
      </div>

      <div>
        <label class="block text-sm font-medium text-[var(--text-label)] mb-1">Tiêu đề <span class="text-[var(--color-danger-text)]">*</span></label>
        <GlassInput v-model="form.tieuDe" type="text" placeholder="Nhập tiêu đề thông báo" />
      </div>
      
      <div>
        <label class="block text-sm font-medium text-[var(--text-label)] mb-1">Nội dung <span class="text-[var(--color-danger-text)]">*</span></label>
        <textarea 
          v-model="form.noiDung" 
          rows="6" 
          class="w-full p-3 rounded-lg border border-[var(--border-input)] bg-[var(--surface-input)] text-[var(--text-body)] focus:ring-2 focus:ring-[var(--focus-ring)] outline-none"
          placeholder="Nội dung thông báo (hỗ trợ HTML cơ bản)"
        ></textarea>
      </div>
    </div>
    
    <template #footer>
      <div class="flex justify-end gap-3 mt-4">
        <GlassButton variant="secondary" :disabled="!isValid || loading" @click="handlePreview">
          <template #leading><Eye class="w-4 h-4" /></template>
          Preview người nhận
        </GlassButton>
        <GlassButton variant="primary" :disabled="!isValid || loading" :loading="loading" @click="handleSubmit">
          <template #leading><Send class="w-4 h-4" /></template>
          Gửi thông báo
        </GlassButton>
      </div>
    </template>
  </GlassPanel>
</template>
