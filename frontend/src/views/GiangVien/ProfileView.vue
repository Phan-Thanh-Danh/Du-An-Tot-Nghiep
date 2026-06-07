<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  Briefcase,
  Calendar,
  Camera,
  Lock,
  Mail,
  MapPin,
  Phone,
  Save,
  ShieldCheck,
  User,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const authStore = useAuthStore()
const popupStore = usePopupStore()

const user = ref({
  name: 'TS. Nguyễn Minh Khoa',
  email: 'khoa.nm@lms.edu.vn',
  phone: '0901 234 567',
  department: 'Khoa Công nghệ thông tin',
  position: 'Giảng viên cấp cao',
  joinDate: '15/01/2020',
  address: 'Quận 7, TP. Hồ Chí Minh',
})

const teachingStats = computed(() => [
  { label: 'Lớp đang dạy', value: 4, variant: 'primary' },
  { label: 'Môn phụ trách', value: 3, variant: 'info' },
  { label: 'Học kỳ', value: 'Spring', variant: 'neutral' },
])

function handleUpdate() {
  popupStore.success('Đã cập nhật', 'Thông tin cá nhân đã được cập nhật thành công.')
}
</script>

<template>
  <div class="teacher-profile-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <User :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Teacher account</div>
          <h1 class="page-title">Hồ sơ giảng viên</h1>
          <p class="page-subtitle">
            Quản lý thông tin cá nhân, hồ sơ công tác và cài đặt bảo mật tài khoản.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <router-link to="/teacher/change-password" class="link-button">
          <Lock :size="14" />
          Đổi mật khẩu
        </router-link>
        <GlassButton size="sm" variant="primary" @click="handleUpdate">
          <template #leading>
            <Save :size="14" />
          </template>
          Cập nhật hồ sơ
        </GlassButton>
      </div>
    </GlassPanel>

    <div class="profile-layout">
      <GlassPanel variant="readable" density="compact" class="profile-summary" :clip="false">
        <div class="avatar-wrap">
          <div class="avatar-frame">
            <img v-if="authStore.user?.avatar" :src="authStore.user.avatar" alt="" />
            <span v-else>{{ authStore.initials || 'TN' }}</span>
          </div>
          <button type="button" class="avatar-action" title="Đổi ảnh đại diện">
            <Camera :size="15" />
          </button>
        </div>

        <div class="profile-title">
          <h2>{{ user.name }}</h2>
          <p>{{ user.email }}</p>
          <div class="badge-line">
            <GlassBadge variant="primary" size="sm">{{ user.position }}</GlassBadge>
            <GlassBadge variant="success" size="sm">Đã xác minh</GlassBadge>
          </div>
        </div>

        <div class="identity-list">
          <div class="identity-item">
            <Briefcase :size="15" />
            <span>Bộ môn</span>
            <strong>{{ user.department }}</strong>
          </div>
          <div class="identity-item">
            <Calendar :size="15" />
            <span>Ngày tham gia</span>
            <strong>{{ user.joinDate }}</strong>
          </div>
          <div class="identity-item">
            <MapPin :size="15" />
            <span>Khu vực</span>
            <strong>{{ user.address }}</strong>
          </div>
        </div>
      </GlassPanel>

      <div class="main-column">
        <GlassPanel variant="surface" density="compact" class="stats-panel">
          <div class="mini-stats">
            <div v-for="item in teachingStats" :key="item.label" class="mini-stat">
              <span>{{ item.label }}</span>
              <div>
                <strong>{{ item.value }}</strong>
                <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
              </div>
            </div>
          </div>
        </GlassPanel>

        <GlassPanel variant="surface" density="compact">
          <template #header>
            <div class="panel-heading">
              <div>
                <h2>Thông tin cá nhân</h2>
                <p>Cập nhật dữ liệu liên hệ và hồ sơ công tác.</p>
              </div>
              <GlassBadge variant="neutral" size="sm">Mã GV: GV-1024</GlassBadge>
            </div>
          </template>

          <div class="form-grid">
            <label class="field">
              <span>Họ tên</span>
              <div class="input-wrap">
                <User :size="15" />
                <input v-model="user.name" type="text" />
              </div>
            </label>
            <label class="field">
              <span>Email</span>
              <div class="input-wrap">
                <Mail :size="15" />
                <input v-model="user.email" type="email" />
              </div>
            </label>
            <label class="field">
              <span>Số điện thoại</span>
              <div class="input-wrap">
                <Phone :size="15" />
                <input v-model="user.phone" type="text" />
              </div>
            </label>
            <label class="field">
              <span>Khoa/Bộ môn</span>
              <div class="input-wrap">
                <Briefcase :size="15" />
                <input v-model="user.department" type="text" />
              </div>
            </label>
            <label class="field">
              <span>Chức danh</span>
              <div class="input-wrap">
                <ShieldCheck :size="15" />
                <input v-model="user.position" type="text" />
              </div>
            </label>
            <label class="field">
              <span>Địa chỉ</span>
              <div class="input-wrap">
                <MapPin :size="15" />
                <input v-model="user.address" type="text" />
              </div>
            </label>
          </div>
        </GlassPanel>

        <div class="info-grid">
          <GlassPanel variant="surface" density="compact">
            <template #header>
              <div class="panel-heading compact">
                <h2>Thông tin xác thực</h2>
                <ShieldCheck :size="17" />
              </div>
            </template>
            <div class="info-list">
              <div class="info-row">
                <span>Trạng thái tài khoản</span>
                <GlassBadge variant="success" size="sm">Đã xác minh</GlassBadge>
              </div>
              <div class="info-row">
                <span>Lần đăng nhập cuối</span>
                <strong>Hôm nay, 08:30 AM</strong>
              </div>
            </div>
          </GlassPanel>

          <GlassPanel variant="surface" density="compact">
            <template #header>
              <div class="panel-heading compact">
                <h2>Địa chỉ công tác</h2>
                <MapPin :size="17" />
              </div>
            </template>
            <div class="info-list">
              <div class="info-row stacked">
                <span>Cơ sở chính</span>
                <strong>Cơ sở 1 - Đống Đa, Hà Nội</strong>
              </div>
              <div class="info-row stacked">
                <span>Văn phòng</span>
                <strong>Tầng 4, Phòng Giảng viên 402</strong>
              </div>
            </div>
          </GlassPanel>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.teacher-profile-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.header-main,
.header-actions,
.badge-line,
.identity-item,
.mini-stat div,
.panel-heading,
.input-wrap,
.info-row,
.link-button {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading,
.info-row {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon,
.avatar-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.header-icon {
  flex: 0 0 auto;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.eyebrow,
.page-subtitle,
.profile-title p,
.identity-item span,
.mini-stat span,
.panel-heading p,
.field span,
.info-row span {
  color: var(--text-muted);
}

.eyebrow {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  color: var(--text-heading);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  font-weight: 900;
}

.page-subtitle {
  margin: 0.25rem 0 0;
  max-width: 42rem;
  font-size: 0.875rem;
  line-height: 1.5;
}

.header-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
}

.link-button {
  justify-content: center;
  gap: 0.375rem;
  min-height: var(--control-height-sm);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  color: var(--text-body);
  padding: 0 0.75rem;
  font-size: 0.78125rem;
  font-weight: 800;
  text-decoration: none;
}

.link-button:hover {
  color: var(--text-link);
}

.profile-layout {
  display: grid;
  grid-template-columns: minmax(18rem, 0.34fr) minmax(0, 1fr);
  gap: 1rem;
  align-items: start;
}

.profile-summary {
  display: grid;
  gap: 1rem;
}

.avatar-wrap {
  position: relative;
  width: max-content;
}

.avatar-frame {
  display: grid;
  place-items: center;
  width: 6rem;
  height: 6rem;
  overflow: hidden;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 1.75rem;
  font-weight: 900;
}

.avatar-frame img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-action {
  position: absolute;
  right: -0.375rem;
  bottom: -0.375rem;
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  cursor: pointer;
}

.profile-title h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.125rem;
  font-weight: 900;
}

.profile-title p {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  font-weight: 650;
}

.badge-line {
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.identity-list,
.info-list {
  display: grid;
  gap: 0.625rem;
}

.identity-item {
  align-items: flex-start;
  gap: 0.625rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 0.75rem;
}

.identity-item span {
  min-width: 5rem;
  font-size: 0.75rem;
  font-weight: 700;
}

.identity-item strong,
.info-row strong {
  color: var(--text-heading);
  font-size: 0.8125rem;
  font-weight: 900;
}

.main-column {
  display: grid;
  gap: 1rem;
  min-width: 0;
}

.mini-stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.625rem;
}

.mini-stat {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
}

.mini-stat span {
  display: block;
  font-size: 0.6875rem;
  font-weight: 700;
}

.mini-stat div {
  justify-content: space-between;
  gap: 0.5rem;
  margin-top: 0.375rem;
}

.mini-stat strong {
  color: var(--text-heading);
  font-size: 1.125rem;
  font-weight: 900;
}

.panel-heading h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
}

.panel-heading p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.panel-heading.compact h2 {
  font-size: 0.875rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.875rem;
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span,
.info-row span {
  font-size: 0.75rem;
  font-weight: 800;
}

.input-wrap {
  gap: 0.5rem;
  min-height: 2.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.75rem;
}

.input-wrap input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 650;
}

.input-wrap:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.info-row {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.75rem;
}

.info-row.stacked {
  display: grid;
  justify-content: stretch;
  gap: 0.25rem;
}

@media (max-width: 1024px) {
  .page-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .profile-layout,
  .info-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .header-actions,
  .form-grid,
  .mini-stats {
    grid-template-columns: 1fr;
    width: 100%;
  }

  .header-actions :deep(.glass-button),
  .link-button {
    width: 100%;
  }
}
</style>
