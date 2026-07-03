<script setup>
import { ref, computed, onMounted } from 'vue'
import { courseApi } from '@/services/courseApi'
import { X, ChevronDown, ChevronRight, FileText, BookOpen, Calendar, Clock, AlertCircle } from 'lucide-vue-next'
import CourseStatusBadge from './CourseStatusBadge.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'

const emit = defineEmits(['close'])
const props = defineProps({
  course: { type: Object, required: true },
})

const activeTab = ref('overview')
const courseDetail = ref(null)
const loading = ref(false)
const error = ref('')

const tabs = [
  { key: 'overview', label: 'Tổng quan' },
  { key: 'content', label: 'Nội dung' },
  { key: 'schedule', label: 'TKB' },
  { key: 'sessions', label: 'Buổi học' },
  { key: 'audit', label: 'Audit' },
]

async function loadDetail() {
  loading.value = true
  error.value = ''
  try {
    const res = await courseApi.getCourseDetail(props.course.maKhoaHoc)
    const data = res.data || res
    courseDetail.value = data
  } catch (err) {
    error.value = err.message || 'Không thể tải chi tiết khóa học'
  } finally {
    loading.value = false
  }
}

const expandedChuong = ref(null)

function toggleChuong(maChuong) {
  expandedChuong.value = expandedChuong.value === maChuong ? null : maChuong
}

const displayCourse = computed(() => courseDetail.value || props.course)

onMounted(loadDetail)
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50">
      <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm" @click="emit('close')" />
      <div class="fixed inset-y-0 right-0 z-50 w-full sm:w-[680px] lg:w-[780px]
                  transform transition-transform duration-300 translate-x-0
                  surface-card border-l border-card shadow-2xl flex flex-col">
        <div class="flex items-center justify-between px-6 py-4 border-b border-default shrink-0">
          <div class="flex items-center gap-3">
            <button class="h-8 w-8 rounded-lg hover:bg-(--surface-input) flex items-center justify-center text-muted"
              @click="emit('close')">
              <X :size="18" />
            </button>
            <h2 class="text-base font-bold text-heading">Chi tiết khóa học</h2>
          </div>
          <div v-if="!loading">
            <CourseStatusBadge :status="displayCourse.trangThai" />
          </div>
        </div>

        <div class="flex items-center gap-1 px-6 pt-3 border-b border-default shrink-0 overflow-x-auto no-scrollbar">
          <button v-for="tab in tabs" :key="tab.key"
            class="relative px-4 py-2.5 text-xs font-bold uppercase tracking-wide whitespace-nowrap transition-colors"
            :class="activeTab === tab.key ? 'text-(--lg-primary)' : 'text-muted hover:text-heading'"
            @click="activeTab = tab.key">
            {{ tab.label }}
            <span v-if="activeTab === tab.key"
              class="absolute bottom-0 left-1/2 -translate-x-1/2 w-8 h-0.5 rounded-full bg-(--lg-primary)" />
          </button>
        </div>

        <div class="flex-1 overflow-y-auto p-6">
          <div v-if="loading" class="space-y-4">
            <LoadingSkeleton :lines="6" />
          </div>

          <div v-else-if="error" class="flex flex-col items-center justify-center gap-3 py-12">
            <AlertCircle :size="32" class="text-(--color-danger-text)" />
            <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
            <p class="text-xs text-muted">{{ error }}</p>
          </div>

          <div v-else-if="activeTab === 'overview'" class="space-y-6">
            <div v-if="displayCourse.urlAnhBia"
              class="h-40 rounded-2xl bg-cover bg-center"
              :style="{ backgroundImage: `url(${displayCourse.urlAnhBia})` }" />

            <div class="grid grid-cols-2 gap-x-6 gap-y-4">
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Mã khóa học</label>
                <p class="text-sm font-bold text-heading font-mono">#{{ displayCourse.maKhoaHoc }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Tiêu đề</label>
                <p class="text-sm font-bold text-heading">{{ displayCourse.tieuDe || '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Môn học</label>
                <p class="text-sm text-body">{{ displayCourse.tenMonHoc || '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Mã môn học</label>
                <p class="text-sm text-body font-mono">{{ displayCourse.maMonHocCode || '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Giảng viên</label>
                <p class="text-sm text-body">{{ displayCourse.tenGiaoVien || '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Lớp</label>
                <GlassBadge variant="primary" size="sm">{{ displayCourse.tenLop || '—' }}</GlassBadge>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Học kỳ</label>
                <p class="text-sm text-body">{{ displayCourse.tenHocKy || 'Chưa phân bổ' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Cơ sở</label>
                <p class="text-sm text-body">{{ displayCourse.tenDonVi || '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Ngày tạo</label>
                <p class="text-sm text-body">{{ displayCourse.ngayTao ? new Date(displayCourse.ngayTao).toLocaleDateString('vi-VN') : '—' }}</p>
              </div>
              <div>
                <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-1">Trạng thái</label>
                <CourseStatusBadge :status="displayCourse.trangThai" />
              </div>
            </div>

            <div v-if="displayCourse.moTa">
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-wide mb-2">Mô tả</label>
              <p class="text-sm text-body leading-relaxed">{{ displayCourse.moTa }}</p>
            </div>
          </div>

          <div v-else-if="activeTab === 'content'" class="space-y-3">
            <div v-if="courseDetail?.chuongs?.length > 0">
              <div v-for="chuong in courseDetail.chuongs" :key="chuong.maChuong"
                class="surface-card border border-card rounded-xl overflow-hidden">
                <button
                  class="w-full flex items-center gap-3 px-4 py-3 text-left hover:bg-(--surface-hover) transition-colors"
                  @click="toggleChuong(chuong.maChuong)">
                  <ChevronDown v-if="expandedChuong === chuong.maChuong" :size="16" class="text-muted shrink-0" />
                  <ChevronRight v-else :size="16" class="text-muted shrink-0" />
                  <span class="flex-1 text-sm font-bold text-heading">{{ chuong.tenChuong }}</span>
                  <span class="text-[11px] text-muted whitespace-nowrap">{{ chuong.baiHocs?.length || 0 }} bài</span>
                  <span class="text-[11px] text-muted bg-(--surface-input) rounded-full px-2 py-0.5 whitespace-nowrap">{{ chuong.soTiet }} tiết</span>
                </button>
                <div v-if="expandedChuong === chuong.maChuong" class="border-t border-default">
                  <div v-for="bai in chuong.baiHocs" :key="bai.maBaiHoc"
                    class="flex items-center gap-3 px-4 py-2.5 pl-11 text-sm text-body hover:bg-(--surface-hover) transition-colors">
                    <FileText :size="14" class="text-muted shrink-0" />
                    <span class="flex-1">{{ bai.tenBaiHoc }}</span>
                    <GlassBadge variant="neutral" size="sm">{{ bai.loaiBai === 'ly_thuyet' ? 'Lý thuyết' : 'Thực hành' }}</GlassBadge>
                  </div>
                </div>
              </div>
            </div>
            <div v-else class="flex flex-col items-center justify-center gap-2 py-12">
              <BookOpen :size="32" class="text-muted" />
              <p class="text-sm font-bold text-heading">Chưa có nội dung</p>
              <p class="text-xs text-muted">Khóa học chưa được cập nhật nội dung giảng dạy.</p>
            </div>
          </div>

          <div v-else-if="activeTab === 'schedule'" class="flex flex-col items-center justify-center gap-2 py-12">
            <Calendar :size="32" class="text-muted" />
            <p class="text-sm font-bold text-heading">Chưa có thời khóa biểu</p>
            <p class="text-xs text-muted text-center max-w-xs">Thời khóa biểu cho khóa học này chưa được thiết lập.</p>
          </div>

          <div v-else-if="activeTab === 'sessions'" class="flex flex-col items-center justify-center gap-2 py-12">
            <Clock :size="32" class="text-muted" />
            <p class="text-sm font-bold text-heading">Chưa có buổi học</p>
            <p class="text-xs text-muted text-center max-w-xs">Danh sách buổi học sẽ xuất hiện sau khi có thời khóa biểu.</p>
          </div>

          <div v-else-if="activeTab === 'audit'" class="flex flex-col items-center justify-center gap-2 py-12">
            <Clock :size="32" class="text-muted" />
            <p class="text-sm font-bold text-heading">Chưa có dữ liệu</p>
            <p class="text-xs text-muted text-center max-w-xs">Nhật ký thay đổi sẽ được ghi lại khi có hoạt động trên khóa học này.</p>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
