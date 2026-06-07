<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  User, ShieldCheck, Award, Link as LinkIcon, 
  MapPin, Phone, Mail, GraduationCap, Building,
  Key, Save, Download, Trash2, Plus, AlertCircle,
  ToggleLeft, ToggleRight, CheckCircle2, QrCode
} from 'lucide-vue-next'

const popupStore = usePopupStore()

// Mock Data
const profile = ref({
  studentId: 'SV2026001',
  fullName: 'Nguyễn Văn An',
  email: 'an.nv.sv@student.edu.vn',
  phone: '0901234567',
  address: '123 Nguyễn Hữu Cảnh, P.22, Bình Thạnh, TP.HCM',
  className: 'SE1601',
  major: 'Kỹ thuật Phần mềm',
  campus: 'Cơ sở TP.Hồ Chí Minh',
  status: 'Active', // 'Active', 'First login', 'Locked'
  role: 'Student'
})

const awards = ref([
  { id: 'AW-01', title: 'Sinh viên Giỏi Học kỳ Fall 2025', type: 'Học thuật', gpa: '3.6', date: '15/01/2026' },
  { id: 'AW-02', title: 'Giải Ba Cuộc thi Hackathon 2025', type: 'Thi đấu', gpa: 'N/A', date: '20/11/2025' }
])

const disciplines = ref([
  // Empty for good student
])

const parents = ref([
  { 
    id: 'PR-01', name: 'Nguyễn Văn Bình (Bố)', email: 'binh.nv@gmail.com', status: 'Connected',
    permissions: { grades: true, attendance: true, finance: false, schedule: true }
  }
])

// State
const activeTab = ref('profile')
const tabs = [
  { id: 'profile', label: 'Thông tin cá nhân', icon: User },
  { id: 'security', label: 'Bảo mật tài khoản', icon: ShieldCheck },
  { id: 'awards', label: 'Khen thưởng & Kỷ luật', icon: Award },
  { id: 'parents', label: 'Liên kết phụ huynh', icon: LinkIcon }
]

// Forms
const editPhone = ref(profile.value.phone)
const editAddress = ref(profile.value.address)

const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

const inviteEmail = ref('')
const inviteName = ref('')

// Computed
const isFirstLogin = computed(() => profile.value.status === 'First login')

// Actions
const updateProfile = () => {
  profile.value.phone = editPhone.value
  profile.value.address = editAddress.value
  popupStore.success('Đã cập nhật', 'Thông tin liên lạc đã được cập nhật thành công.')
}

const changePassword = () => {
  if (newPassword.value !== confirmPassword.value) {
    popupStore.error('Lỗi', 'Mật khẩu xác nhận không khớp!')
    return
  }
  if (!oldPassword.value || !newPassword.value) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng điền đủ thông tin!')
    return
  }
  popupStore.success('Đã đổi mật khẩu', 'Mật khẩu của bạn đã được thay đổi.')
  oldPassword.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
  if (profile.value.status === 'First login') {
    profile.value.status = 'Active' // Remove first login constraint
  }
}

const downloadCertificate = (award) => {
  // Simulate PDF download with digital signature & QR
  popupStore.info('Tải bằng khen', `Đang tải file PDF Bằng khen: ${award.title}`)
}

const inviteParent = () => {
  if (parents.value.length >= 3) {
    popupStore.warning('Giới hạn liên kết', 'Chỉ được phép liên kết tối đa 3 phụ huynh/người giám hộ.')
    return
  }
  if (!inviteEmail.value || !inviteName.value) return
  
  parents.value.push({
    id: `PR-0${parents.value.length + 1}`,
    name: inviteName.value,
    email: inviteEmail.value,
    status: 'Pending',
    permissions: { grades: false, attendance: false, finance: false, schedule: false }
  })
  
  inviteEmail.value = ''
  inviteName.value = ''
  popupStore.success('Đã gửi lời mời', 'Email mời liên kết phụ huynh đã được gửi.')
}

const togglePermission = (parent, key) => {
  parent.permissions[key] = !parent.permissions[key]
}

const removeParent = (idx) => {
  if(confirm('Bạn có chắc chắn muốn thu hồi quyền truy cập và hủy liên kết với tài khoản phụ huynh này?')) {
    parents.value.splice(idx, 1)
  }
}

// Logic: If First login, force security tab
if (isFirstLogin.value) {
  activeTab.value = 'security'
}

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
              <div class="read-value"><GraduationCap :size="15"/> {{ profile.className }} - {{ profile.major }}</div>
            </div>
            <div class="form-group readonly col-span-2">
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
            <div class="sp-icon"><Key :size="24" class="text-[var(--text-link)]"/></div>
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
          <h3 class="pane-subtitle flex items-center gap-2 text-[var(--color-success-text)]"><Award :size="18"/> Bằng khen & Thành tích</h3>
          <div class="cards-list mb-4">
            <div v-for="aw in awards" :key="aw.id" class="award-card">
              <div class="ac-icon"><Award :size="24" class="text-[var(--color-warning-text)]"/></div>
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
          <h3 class="pane-subtitle flex items-center gap-2 text-[var(--color-danger-text)]"><AlertCircle :size="18"/> Hồ sơ Kỷ luật</h3>
          <div class="cards-list">
            <div v-for="dc in disciplines" :key="dc.id" class="discipline-card">
              <!-- Render disciplines if any -->
            </div>
            <div v-if="disciplines.length === 0" class="empty-state success">
              <CheckCircle2 :size="20" class="text-[var(--color-success-text)] mb-2 mx-auto"/>
              Hồ sơ trong sạch. Không có ghi nhận kỷ luật nào.
            </div>
          </div>
        </div>

        <!-- Tab 4: Parents -->
        <div v-show="activeTab === 'parents'" class="tab-pane">
          <h2 class="pane-title">Quản lý Liên kết Phụ huynh</h2>
          <p class="text-sm text-[var(--text-muted)] mb-4">Bạn có thể cấp quyền truy cập để gia đình/người giám hộ theo dõi tiến độ học tập. Tối đa 3 tài khoản liên kết.</p>

          <div class="parents-list mb-8">
            <div v-for="(parent, idx) in parents" :key="parent.id" class="parent-card">
              <div class="pc-header">
                <div class="flex items-center gap-3">
                  <div class="avatar-sm">{{ parent.name.charAt(0) }}</div>
                  <div>
                    <h4>{{ parent.name }}</h4>
                    <p>{{ parent.email }}</p>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <span class="status-badge-sm" :class="parent.status === 'Connected' ? 'badge-sm-success' : 'badge-sm-inactive'">
                    {{ parent.status === 'Connected' ? 'Đã liên kết' : 'Chờ xác nhận' }}
                  </span>
                  <button class="btn-icon text-[var(--color-danger-text)] hover:bg-[var(--color-danger-bg)]" title="Thu hồi & Hủy liên kết" @click="removeParent(idx)">
                    <Trash2 :size="16"/>
                  </button>
                </div>
              </div>

              <!-- Permission Toggles -->
              <div class="pc-permissions" v-if="parent.status === 'Connected'">
                <div class="perm-title">Cấu hình quyền chia sẻ dữ liệu:</div>
                <div class="perm-grid">
                  <div class="perm-item">
                    <span>Điểm thi & Học tập</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.grades}" @click="togglePermission(parent, 'grades')">
                      <component :is="parent.permissions.grades ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.grades ? 'text-[var(--text-link)]' : 'text-[var(--text-placeholder)]'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Điểm danh & Chuyên cần</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.attendance}" @click="togglePermission(parent, 'attendance')">
                      <component :is="parent.permissions.attendance ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.attendance ? 'text-[var(--text-link)]' : 'text-[var(--text-placeholder)]'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Công nợ & Học phí</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.finance}" @click="togglePermission(parent, 'finance')">
                      <component :is="parent.permissions.finance ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.finance ? 'text-[var(--text-link)]' : 'text-[var(--text-placeholder)]'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Thời khóa biểu</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.schedule}" @click="togglePermission(parent, 'schedule')">
                      <component :is="parent.permissions.schedule ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.schedule ? 'text-[var(--text-link)]' : 'text-[var(--text-placeholder)]'"/>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div v-if="parents.length === 0" class="empty-state">Chưa có liên kết phụ huynh nào.</div>
          </div>

          <!-- Invite Form -->
          <div class="invite-panel" v-if="parents.length < 3">
            <h3 class="pane-subtitle flex items-center gap-2 mb-3"><Plus :size="18"/> Mời liên kết mới</h3>
            <div class="invite-form">
              <div class="form-group flex-1">
                <input v-model="inviteName" type="text" class="input-glass" placeholder="Tên phụ huynh/Người giám hộ" />
              </div>
              <div class="form-group flex-1">
                <input v-model="inviteEmail" type="email" class="input-glass" placeholder="Email nhận lời mời" />
              </div>
              <button class="btn-primary" @click="inviteParent" :disabled="!inviteEmail || !inviteName">Gửi lời mời</button>
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

/* Parents List */
.parent-card { background: var(--surface-card-strong); border: 1px solid var(--border-default); border-radius: 16px; overflow: hidden; margin-bottom: 1rem; }
.pc-header { padding: 1.25rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px dashed var(--border-default); }
.avatar-sm { width: 40px; height: 40px; border-radius: 50%; background: var(--surface-input); color: var(--text-label); display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 1.1rem; }
.pc-header h4 { font-size: 1rem; font-weight: 700; margin: 0; color: var(--text-heading); }
.pc-header p { font-size: .8125rem; color: var(--text-muted); margin: 0; }
.status-badge-sm { font-size: .7rem; font-weight: 700; padding: .2rem .5rem; border-radius: 6px; }

.pc-permissions { padding: 1rem 1.25rem; background: var(--surface-solid); }
.perm-title { font-size: .8125rem; font-weight: 700; color: var(--text-label); margin-bottom: .75rem; }
.perm-grid { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.perm-item { display: flex; justify-content: space-between; align-items: center; padding: .6rem 1rem; background: var(--surface-card-strong); border: 1px solid var(--border-default); border-radius: 8px; font-size: .875rem; font-weight: 500; color: var(--text-body); }
.toggle-btn { background: transparent; border: none; cursor: pointer; padding: 0; display: flex; align-items: center; transition: transform .1s; }
.toggle-btn:active { transform: scale(0.9); }

.invite-panel { background: color-mix(in srgb, var(--accent-primary-soft) 30%, transparent); border: 1px dashed var(--accent-primary-soft); padding: 1.5rem; border-radius: 16px; }
.invite-form { display: flex; gap: 1rem; align-items: flex-end; }

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
