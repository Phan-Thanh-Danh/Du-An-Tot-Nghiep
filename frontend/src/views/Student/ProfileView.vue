<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { studentApi } from '@/services/studentApi'
import { unwrapApiData } from '@/services/apiClient'
import {
  User, Users, ShieldCheck, Award, Link as LinkIcon, 
  MapPin, Phone, Mail, GraduationCap, Building,
  Key, Save, Download, Trash2, Plus, AlertCircle,
  ToggleLeft, ToggleRight, CheckCircle2
} from 'lucide-vue-next'
const popupStore = usePopupStore()

const emptyProfile = {
  fullName: '',
  studentId: '',
  email: '',
  phone: '',
  address: '',
  className: 'Chưa có dữ liệu học vụ',
  major: 'Chưa có dữ liệu học vụ',
  campus: 'Chưa có dữ liệu học vụ',
  status: '',
}

const profile = ref({ ...emptyProfile })
const awards = ref([])
const disciplines = ref([])
const loading = ref(false)

// State
const activeTab = ref('profile')
const tabs = [
  { id: 'profile', label: 'Thông tin cá nhân', icon: User },
  { id: 'security', label: 'Bảo mật tài khoản', icon: ShieldCheck },
  { id: 'awards', label: 'Khen thưởng & Kỷ luật', icon: Award }
]

// Forms
const editPhone = ref(profile.value.phone)
const editAddress = ref(profile.value.address)

const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

// Computed
const isFirstLogin = computed(() => profile.value.status === 'First login')

// Actions
const loadProfile = async () => {
  loading.value = true
  try {
    const savedLocalAddress = localStorage.getItem('student_profile_address') || ''

    const [profileRes, rewardsRes, disciplinesRes] = await Promise.allSettled([
      studentApi.getProfile(),
      studentApi.getRewards({ pageIndex: 1, pageSize: 50 }),
      studentApi.getDisciplines({ pageIndex: 1, pageSize: 50 })
    ])

    if (profileRes.status === 'fulfilled') {
      const data = unwrapApiData(profileRes.value) || {}
      profile.value = { ...profile.value, ...data }
      editPhone.value = profile.value.phone || ''
      editAddress.value = profile.value.address || savedLocalAddress
      profile.value.address = editAddress.value
    }

    if (rewardsRes.status === 'fulfilled') {
      const rwData = unwrapApiData(rewardsRes.value)
      const items = rwData?.items ?? rwData?.Items ?? []
      awards.value = items.map(r => ({
        id: r.maKhenThuong ?? r.MaKhenThuong,
        title: r.danhHieuSnapshot ?? r.DanhHieuSnapshot ?? r.tenLoaiKhenThuong ?? 'Khen thưởng',
        type: r.tenLoaiKhenThuong ?? r.TenLoaiKhenThuong ?? 'Học tập',
        gpa: r.diemXet ?? r.DiemXet ?? 'N/A',
        date: r.ngayDuyet ?? r.NgayDuyet ?? r.capLuc ?? 'Đã ghi nhận'
      }))
    }

    if (disciplinesRes.status === 'fulfilled') {
      const dcData = unwrapApiData(disciplinesRes.value)
      const items = dcData?.items ?? dcData?.Items ?? []
      disciplines.value = items.map(d => ({
        id: d.maHoSoKyLuat ?? d.MaHoSoKyLuat,
        title: d.tieuDe ?? d.TieuDe ?? 'Kỷ luật',
        level: d.mucDoKyLuat ?? d.MucDoKyLuat ?? 'Nhắc nhở',
        status: d.trangThai ?? d.TrangThai ?? 'Hiệu lực',
        date: d.ngayViPham ?? d.NgayViPham ?? ''
      }))
    }
  } catch (error) {
    popupStore.error('Không thể tải hồ sơ', error?.message || 'Không thể tải hồ sơ cá nhân.')
  } finally {
    loading.value = false
  }
}

const updateProfile = async () => {
  try {
    localStorage.setItem('student_profile_address', editAddress.value)
    const response = await studentApi.updateProfile({
      fullName: profile.value.fullName,
      email: profile.value.email,
      phone: editPhone.value,
    })
    const data = unwrapApiData(response) || {}
    profile.value = { ...profile.value, ...data, phone: editPhone.value, address: editAddress.value }
    popupStore.success('Đã cập nhật', 'Thông tin liên lạc đã được cập nhật thành công.')
  } catch (error) {
    popupStore.error('Không thể cập nhật', error?.message || 'Không thể cập nhật thông tin liên lạc.')
  }
}

const changePassword = async () => {
  if (newPassword.value !== confirmPassword.value) {
    popupStore.error('Lỗi', 'Mật khẩu xác nhận không khớp!')
    return
  }
  if (!oldPassword.value || !newPassword.value) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng điền đủ thông tin!')
    return
  }
  try {
    await studentApi.changePassword({
      currentPassword: oldPassword.value,
      newPassword: newPassword.value,
      confirmPassword: confirmPassword.value,
    })
    popupStore.success('Đã đổi mật khẩu', 'Mật khẩu của bạn đã được thay đổi.')
    oldPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
    if (profile.value.status === 'First login') {
      profile.value.status = 'Active'
    }
  } catch (error) {
    popupStore.error('Không thể đổi mật khẩu', error?.message || 'Không thể đổi mật khẩu.')
  }
}

const downloadCertificate = (award) => {
  popupStore.info('Tải bằng khen', `Đang tải file PDF Bằng khen: ${award.title}`)
}

// Logic: If First login, force security tab
if (isFirstLogin.value) {
  activeTab.value = 'security'
}

onMounted(loadProfile)
</script>

<template>
  <div class="profile-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><User :size="15"/>Tài khoản sinh viên</div>
        <h1 class="page-title">Hồ sơ cá nhân</h1>
        <p class="page-sub">Quản lý thông tin định danh, bảo mật, khen thưởng và chia sẻ dữ liệu.</p>
      </div>
      <div class="status-badge" :class="isFirstLogin ? 'badge-warning' : 'badge-success'">
        <CheckCircle2 v-if="!isFirstLogin" :size="16" />
        <AlertCircle v-else :size="16" />
        Trạng thái: <strong>{{ isFirstLogin ? 'Đăng nhập lần đầu' : profile.status }}</strong>
      </div>
    </div>

    <!-- First Login Blocker -->
    <div v-if="isFirstLogin" class="warning-banner blocker-warning mb-4">
      <div class="warning-icon"><AlertCircle :size="24"/></div>
      <div class="warning-content">
        <h3>Bắt buộc đổi mật khẩu</h3>
        <p>Đây là lần đăng nhập đầu tiên của bạn. Để đảm bảo an toàn, hệ thống yêu cầu bạn phải đổi mật khẩu ngay lập tức trước khi sử dụng các chức năng khác.</p>
      </div>
    </div>

    <!-- Layout: Settings Style (Sidebar + Content) -->
    <div class="settings-layout">
      <!-- Sidebar Navigation -->
      <div class="settings-sidebar">
        <div class="profile-summary">
          <div class="avatar-circle">
            {{ profile.fullName.charAt(0) }}
          </div>
          <h3>{{ profile.fullName }}</h3>
          <p>{{ profile.studentId }}</p>
        </div>

        <nav class="settings-nav">
          <button v-for="tab in tabs" :key="tab.id" 
                  class="nav-btn" :class="{'active': activeTab === tab.id}"
                  @click="!isFirstLogin || tab.id === 'security' ? activeTab = tab.id : null"
                  :disabled="isFirstLogin && tab.id !== 'security'">
            <component :is="tab.icon" :size="18"/>
            {{ tab.label }}
          </button>
        </nav>
      </div>

      <!-- Main Content Area -->
      <div class="settings-content">
        
        <!-- Tab 1: Profile Info -->
        <div v-show="activeTab === 'profile'" class="tab-pane">
          <h2 class="pane-title">Thông tin Định danh & Học vụ</h2>
          
          <div class="info-grid">
            <!-- Readonly Fields -->
            <div class="form-group readonly">
              <label>Mã Sinh Viên</label>
              <div class="read-value"><User :size="15"/> {{ profile.studentId }}</div>
            </div>
            <div class="form-group readonly">
              <label>Họ và Tên</label>
              <div class="read-value">{{ profile.fullName }}</div>
            </div>
            <div class="form-group readonly">
              <label>Email Trường cấp</label>
              <div class="read-value"><Mail :size="15"/> {{ profile.email }}</div>
            </div>
            <div class="form-group readonly">
              <label>Lớp Sinh hoạt</label>
              <div class="read-value"><Users :size="15"/> {{ profile.className }}</div>
            </div>
            <div class="form-group readonly">
              <label>Chuyên ngành</label>
              <div class="read-value"><GraduationCap :size="15"/> {{ profile.major }}</div>
            </div>
            <div class="form-group readonly">
              <label>Cơ sở học tập (Campus)</label>
              <div class="read-value"><Building :size="15"/> {{ profile.campus }}</div>
            </div>
          </div>

          <div class="divider"></div>

          <h3 class="pane-subtitle">Thông tin liên lạc (Được phép sửa)</h3>
          <div class="info-grid">
            <div class="form-group">
              <label>Số điện thoại</label>
              <div class="input-icon-wrapper">
                <Phone :size="16" class="input-icon"/>
                <input v-model="editPhone" type="text" class="input-glass pl-9" />
              </div>
            </div>
            <div class="form-group col-span-2">
              <label>Địa chỉ hiện tại</label>
              <div class="input-icon-wrapper">
                <MapPin :size="16" class="input-icon"/>
                <input v-model="editAddress" type="text" class="input-glass pl-9" />
              </div>
            </div>
          </div>
          
          <div class="form-actions mt-4">
            <button class="btn-primary" @click="updateProfile"><Save :size="16"/> Lưu thay đổi</button>
          </div>
        </div>

        <!-- Tab 2: Security -->
        <div v-show="activeTab === 'security'" class="tab-pane">
          <h2 class="pane-title">Bảo mật tài khoản</h2>
          
          <div class="security-panel">
            <div class="sp-icon"><Key :size="24" class="text-(--text-link)"/></div>
            <div class="sp-content">
              <h3>Thay đổi mật khẩu</h3>
              <p>Mật khẩu cần có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số.</p>
              
              <div class="pwd-form mt-4">
                <div class="form-group">
                  <label>Mật khẩu hiện tại</label>
                  <input v-model="oldPassword" type="password" class="input-glass" placeholder="Nhập mật khẩu cũ..." />
                </div>
                <div class="form-group mt-3">
                  <label>Mật khẩu mới</label>
                  <input v-model="newPassword" type="password" class="input-glass" placeholder="Nhập mật khẩu mới..." />
                </div>
                <div class="form-group mt-3">
                  <label>Xác nhận mật khẩu mới</label>
                  <input v-model="confirmPassword" type="password" class="input-glass" placeholder="Nhập lại mật khẩu mới..." />
                </div>
                <div class="mt-4">
                  <button class="btn-primary" @click="changePassword">Cập nhật mật khẩu</button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab 3: Awards & Discipline -->
        <div v-show="activeTab === 'awards'" class="tab-pane">
          <h2 class="pane-title">Hồ sơ Khen thưởng & Kỷ luật</h2>
          
          <!-- Awards -->
          <h3 class="pane-subtitle flex items-center gap-2 text-(--color-success-text)"><Award :size="18"/> Bằng khen & Thành tích</h3>
          <div class="cards-list mb-4">
            <div v-for="aw in awards" :key="aw.id" class="award-card">
              <div class="ac-icon"><Award :size="24" class="text-(--color-warning-text)"/></div>
              <div class="ac-info">
                <h4>{{ aw.title }}</h4>
                <div class="ac-meta">
                  <span>Loại: <strong>{{ aw.type }}</strong></span> • 
                  <span>GPA: <strong>{{ aw.gpa }}</strong></span> • 
                  <span>Ngày: {{ aw.date }}</span>
                </div>
              </div>
              <div class="ac-action">
                <button class="btn-outline-sm" @click="downloadCertificate(aw)">
                  <Download :size="14"/> Tải PDF (Signed)
                </button>
              </div>
            </div>
            <div v-if="awards.length === 0" class="empty-state">Chưa có hồ sơ khen thưởng.</div>
          </div>

          <!-- Disciplines -->
          <h3 class="pane-subtitle flex items-center gap-2 text-(--color-danger-text)"><AlertCircle :size="18"/> Hồ sơ Kỷ luật</h3>
          <div class="cards-list">
            <div v-for="dc in disciplines" :key="dc.id" class="discipline-card">
              <!-- Render disciplines if any -->
            </div>
            <div v-if="disciplines.length === 0" class="empty-state success">
              <CheckCircle2 :size="20" class="text-(--color-success-text) mb-2 mx-auto"/>
              Hồ sơ trong sạch. Không có ghi nhận kỷ luật nào.
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-page {
  padding: 2rem;
  max-width: 1300px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: var(--text-heading);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--text-link); margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; color: var(--text-heading); }
.page-sub { font-size: .875rem; color: var(--text-muted); margin: 0; }

.status-badge { display: inline-flex; align-items: center; gap: .5rem; font-size: .875rem; padding: .5rem 1rem; border-radius: 99px; }
.badge-success { background: var(--color-success-bg); color: var(--color-success-text); border: 1px solid color-mix(in srgb, var(--color-success-text) 20%, transparent); }
.badge-warning { background: var(--color-warning-bg); color: var(--color-warning-text); border: 1px solid color-mix(in srgb, var(--color-warning-text) 20%, transparent); }
.badge-sm-success { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-sm-inactive { background: var(--surface-solid); color: var(--text-muted); }

/* Warning Banner */
.warning-banner { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 16px; backdrop-filter: blur(12px); box-shadow: 0 4px 20px color-mix(in srgb, var(--color-warning-text) 10%, transparent); }
.blocker-warning { background: var(--color-warning-bg); border: 1px solid color-mix(in srgb, var(--color-warning-text) 20%, transparent); color: var(--color-warning-text); }
.warning-icon { padding-top: .1rem; }
.warning-content h3 { font-size: 1rem; font-weight: 800; margin: 0 0 .25rem; color: var(--text-heading); }
.warning-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

/* Settings Layout */
.settings-layout {
  display: flex; gap: 1.5rem; align-items: flex-start;
}

.settings-sidebar {
  width: 280px; flex-shrink: 0;
  background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; padding: 1.5rem;
  box-shadow: var(--lg-shadow-sm); backdrop-filter: saturate(160%) blur(16px);
}

.profile-summary { text-align: center; margin-bottom: 2rem; }
.avatar-circle { width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, var(--lg-primary), var(--accent-violet)); color: var(--text-inverse); display: flex; align-items: center; justify-content: center; font-size: 2.5rem; font-weight: 800; margin: 0 auto 1rem; box-shadow: 0 8px 24px color-mix(in srgb, var(--text-link) 30%, transparent); }
.profile-summary h3 { font-size: 1.1rem; font-weight: 800; color: var(--text-heading); margin: 0 0 .25rem; }
.profile-summary p { font-size: .875rem; color: var(--text-muted); margin: 0; }

.settings-nav { display: flex; flex-direction: column; gap: .5rem; }
.nav-btn { display: flex; align-items: center; gap: .75rem; width: 100%; padding: .8rem 1rem; border-radius: 12px; font-size: .9rem; font-weight: 600; color: var(--text-label); background: transparent; border: none; cursor: pointer; text-align: left; transition: all .2s; }
.nav-btn:hover:not(:disabled) { background: var(--surface-solid); color: var(--text-heading); }
.nav-btn.active { background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 12px color-mix(in srgb, var(--text-link) 20%, transparent); }
.nav-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.settings-content {
  flex: 1;
  background: var(--surface-card-strong); border: 1px solid var(--border-card);
  border-radius: 20px; padding: 2rem;
  box-shadow: var(--lg-shadow-sm); backdrop-filter: saturate(160%) blur(16px);
  min-height: 500px;
}

.pane-title { font-size: 1.5rem; font-weight: 800; margin: 0 0 1.5rem; border-bottom: 1px solid var(--border-default); padding-bottom: 1rem; color: var(--text-heading); }
.pane-subtitle { font-size: 1.1rem; font-weight: 700; margin: 0 0 1rem; color: var(--text-heading); }

/* Grid Forms */
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: .4rem; }
.form-group label { font-size: .8125rem; font-weight: 700; color: var(--text-label); }
.read-value { display: flex; align-items: center; gap: .5rem; padding: .75rem 1rem; background: var(--surface-solid); border-radius: 10px; font-size: .9rem; font-weight: 600; color: var(--text-heading); border: 1px dashed var(--border-default); }

.input-icon-wrapper { position: relative; display: flex; align-items: center; }
.input-icon { position: absolute; left: 1rem; color: var(--text-placeholder); }
.input-glass { width: 100%; padding: .75rem 1rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .9rem; outline: none; transition: border-color .2s; color: var(--text-body); }
.input-icon-wrapper .input-glass { padding-left: 2.5rem; }
.input-glass:focus { border-color: var(--border-input-focus); background: var(--surface-input-focus); }

.divider { height: 1px; background: var(--border-default); margin: 2rem 0; }

/* Security Panel */
.security-panel { display: flex; gap: 1.5rem; background: color-mix(in srgb, var(--accent-primary-soft) 30%, transparent); border: 1px solid var(--accent-primary-soft); padding: 1.5rem; border-radius: 16px; }
.sp-icon { width: 48px; height: 48px; border-radius: 12px; background: var(--accent-primary-soft); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.sp-content h3 { font-size: 1.1rem; font-weight: 800; margin: 0 0 .25rem; color: var(--text-heading); }
.sp-content p { font-size: .875rem; color: var(--text-muted); margin: 0; }
.pwd-form { max-width: 400px; }

/* Cards List (Awards) */
.cards-list { display: flex; flex-direction: column; gap: 1rem; }
.award-card { display: flex; align-items: center; gap: 1rem; background: var(--surface-card-strong); border: 1px solid var(--border-default); padding: 1rem 1.25rem; border-radius: 12px; }
.ac-icon { width: 48px; height: 48px; border-radius: 50%; background: var(--color-warning-bg); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.ac-info { flex: 1; }
.ac-info h4 { font-size: 1rem; font-weight: 700; margin: 0 0 .25rem; color: var(--text-heading); }
.ac-meta { font-size: .8125rem; color: var(--text-muted); }
.btn-outline-sm { display: inline-flex; align-items: center; gap: .3rem; padding: .4rem .8rem; border-radius: 8px; font-size: .75rem; font-weight: 700; color: var(--text-label); border: 1px solid var(--border-default); background: transparent; cursor: pointer; transition: all .15s; }
.btn-outline-sm:hover { border-color: var(--text-link); color: var(--text-link); }

.empty-state { text-align: center; padding: 2rem; color: var(--text-muted); font-size: .9rem; font-style: italic; background: var(--surface-solid); border-radius: 12px; border: 1px dashed var(--border-default); }



/* Buttons */
.btn-primary { display: inline-flex; align-items: center; gap: .4rem; padding: .75rem 1.25rem; border-radius: 10px; font-size: .875rem; font-weight: 700; cursor: pointer; border: none; background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 25%, transparent); transition: all .15s; }
.btn-primary:hover:not(:disabled) { background: var(--lg-primary-dark); transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-icon { width: 32px; height: 32px; border-radius: 8px; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; border: none; background: transparent; transition: all .15s; }
.btn-icon:hover { background: var(--accent-primary-soft); }

@media (max-width: 1024px) {
  .settings-layout { flex-direction: column; }
  .settings-sidebar { width: 100%; display: flex; flex-direction: row; gap: 2rem; align-items: center; }
  .profile-summary { margin-bottom: 0; text-align: left; display: flex; align-items: center; gap: 1rem; }
  .avatar-circle { width: 60px; height: 60px; margin: 0; font-size: 1.8rem; }
  .settings-nav { flex-direction: row; flex: 1; flex-wrap: wrap; }
  .nav-btn { width: auto; }
  .info-grid, .perm-grid { grid-template-columns: 1fr; }
  .form-group.col-span-2 { grid-column: span 1; }
  .invite-form { flex-direction: column; align-items: stretch; }
}
</style>
