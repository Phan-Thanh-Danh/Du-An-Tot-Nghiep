<script setup>
import { ref, computed } from 'vue'
import {
  User, ShieldCheck, Award, Link as LinkIcon, 
  MapPin, Phone, Mail, GraduationCap, Building,
  Key, Save, Download, Trash2, Plus, AlertCircle,
  ToggleLeft, ToggleRight, CheckCircle2, QrCode
} from 'lucide-vue-next'

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
  alert('Đã cập nhật thông tin liên lạc thành công!')
}

const changePassword = () => {
  if (newPassword.value !== confirmPassword.value) {
    alert('Mật khẩu xác nhận không khớp!')
    return
  }
  if (!oldPassword.value || !newPassword.value) {
    alert('Vui lòng điền đủ thông tin!')
    return
  }
  alert('Đổi mật khẩu thành công! Tài khoản đã được bảo mật.')
  oldPassword.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
  if (profile.value.status === 'First login') {
    profile.value.status = 'Active' // Remove first login constraint
  }
}

const downloadCertificate = (award) => {
  // Simulate PDF download with digital signature & QR
  alert(`Đang tải file PDF Bằng khen: ${award.title}\n(File PDF đã được nhúng chữ ký số của Hiệu trưởng và mã QR xác minh trực tuyến)`)
}

const inviteParent = () => {
  if (parents.value.length >= 3) {
    alert('Chỉ được phép liên kết tối đa 3 phụ huynh/người giám hộ.')
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
  alert('Đã gửi email lời mời liên kết thành công!')
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
      <div class="status-badge" :class="isFirstLogin ? 'bg-amber-100 text-amber-700 border border-amber-300' : 'bg-green-100 text-green-700 border border-green-300'">
        <CheckCircle2 v-if="!isFirstLogin" :size="16" />
        <AlertCircle v-else :size="16" />
        Trạng thái: <strong>{{ isFirstLogin ? 'Đăng nhập lần đầu' : profile.status }}</strong>
      </div>
    </div>

    <!-- First Login Blocker -->
    <div v-if="isFirstLogin" class="warning-banner blocker-warning mb-6">
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
            <div class="sp-icon"><Key :size="24" class="text-blue-600"/></div>
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
          <h3 class="pane-subtitle text-green-700 flex items-center gap-2"><Award :size="18"/> Bằng khen & Thành tích</h3>
          <div class="cards-list mb-6">
            <div v-for="aw in awards" :key="aw.id" class="award-card">
              <div class="ac-icon"><Award :size="24" class="text-amber-500"/></div>
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
          <h3 class="pane-subtitle text-red-700 flex items-center gap-2"><AlertCircle :size="18"/> Hồ sơ Kỷ luật</h3>
          <div class="cards-list">
            <div v-for="dc in disciplines" :key="dc.id" class="discipline-card">
              <!-- Render disciplines if any -->
            </div>
            <div v-if="disciplines.length === 0" class="empty-state success">
              <CheckCircle2 :size="20" class="text-green-500 mb-2 mx-auto"/>
              Hồ sơ trong sạch. Không có ghi nhận kỷ luật nào.
            </div>
          </div>
        </div>

        <!-- Tab 4: Parents -->
        <div v-show="activeTab === 'parents'" class="tab-pane">
          <h2 class="pane-title">Quản lý Liên kết Phụ huynh</h2>
          <p class="text-sm text-slate-600 mb-6">Bạn có thể cấp quyền truy cập để gia đình/người giám hộ theo dõi tiến độ học tập. Tối đa 3 tài khoản liên kết.</p>

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
                  <span class="status-badge-sm" :class="parent.status === 'Connected' ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-600'">
                    {{ parent.status === 'Connected' ? 'Đã liên kết' : 'Chờ xác nhận' }}
                  </span>
                  <button class="btn-icon text-red-500 hover:bg-red-50" title="Thu hồi & Hủy liên kết" @click="removeParent(idx)">
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
                      <component :is="parent.permissions.grades ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.grades ? 'text-blue-600' : 'text-slate-400'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Điểm danh & Chuyên cần</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.attendance}" @click="togglePermission(parent, 'attendance')">
                      <component :is="parent.permissions.attendance ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.attendance ? 'text-blue-600' : 'text-slate-400'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Công nợ & Học phí</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.finance}" @click="togglePermission(parent, 'finance')">
                      <component :is="parent.permissions.finance ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.finance ? 'text-blue-600' : 'text-slate-400'"/>
                    </button>
                  </div>
                  <div class="perm-item">
                    <span>Thời khóa biểu</span>
                    <button class="toggle-btn" :class="{'active': parent.permissions.schedule}" @click="togglePermission(parent, 'schedule')">
                      <component :is="parent.permissions.schedule ? ToggleRight : ToggleLeft" :size="24" :class="parent.permissions.schedule ? 'text-blue-600' : 'text-slate-400'"/>
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
  color: #0f172a;
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: #2563eb; margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; }
.page-sub { font-size: .875rem; color: #64748b; margin: 0; }

.status-badge { display: inline-flex; align-items: center; gap: .5rem; font-size: .875rem; padding: .5rem 1rem; border-radius: 99px; }

/* Warning Banner */
.warning-banner { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 16px; backdrop-filter: blur(12px); box-shadow: 0 4px 20px rgba(217,119,6,.1); }
.blocker-warning { background: rgba(217,119,6,.1); border: 1px solid rgba(217,119,6,.2); color: #b45309; }
.warning-icon { padding-top: .1rem; }
.warning-content h3 { font-size: 1rem; font-weight: 800; margin: 0 0 .25rem; }
.warning-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

/* Settings Layout */
.settings-layout {
  display: flex; gap: 1.5rem; align-items: flex-start;
}

.settings-sidebar {
  width: 280px; flex-shrink: 0;
  background: rgba(255,255,255,.72); border: 1px solid rgba(255,255,255,.6);
  border-radius: 20px; padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(15,23,42,.05); backdrop-filter: saturate(160%) blur(16px);
}

.profile-summary { text-align: center; margin-bottom: 2rem; }
.avatar-circle { width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, #3b82f6, #8b5cf6); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 2.5rem; font-weight: 800; margin: 0 auto 1rem; box-shadow: 0 8px 24px rgba(59,130,246,.3); }
.profile-summary h3 { font-size: 1.1rem; font-weight: 800; color: #0f172a; margin: 0 0 .25rem; }
.profile-summary p { font-size: .875rem; color: #64748b; margin: 0; }

.settings-nav { display: flex; flex-direction: column; gap: .5rem; }
.nav-btn { display: flex; align-items: center; gap: .75rem; width: 100%; padding: .8rem 1rem; border-radius: 12px; font-size: .9rem; font-weight: 600; color: #475569; background: transparent; border: none; cursor: pointer; text-align: left; transition: all .2s; }
.nav-btn:hover:not(:disabled) { background: rgba(248,250,252,.8); color: #0f172a; }
.nav-btn.active { background: #2563eb; color: #fff; box-shadow: 0 4px 12px rgba(37,99,235,.2); }
.nav-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.settings-content {
  flex: 1;
  background: rgba(255,255,255,.85); border: 1px solid rgba(255,255,255,.6);
  border-radius: 20px; padding: 2rem;
  box-shadow: 0 4px 20px rgba(15,23,42,.05); backdrop-filter: saturate(160%) blur(16px);
  min-height: 500px;
}

.pane-title { font-size: 1.5rem; font-weight: 800; margin: 0 0 1.5rem; border-bottom: 1px solid rgba(148,163,184,.2); padding-bottom: 1rem; }
.pane-subtitle { font-size: 1.1rem; font-weight: 700; margin: 0 0 1rem; color: #0f172a; }

/* Grid Forms */
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: .4rem; }
.form-group label { font-size: .8125rem; font-weight: 700; color: #475569; }
.read-value { display: flex; align-items: center; gap: .5rem; padding: .75rem 1rem; background: rgba(248,250,252,.5); border-radius: 10px; font-size: .9rem; font-weight: 600; color: #0f172a; border: 1px dashed rgba(148,163,184,.3); }

.input-icon-wrapper { position: relative; display: flex; align-items: center; }
.input-icon { position: absolute; left: 1rem; color: #94a3b8; }
.input-glass { width: 100%; padding: .75rem 1rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.4); background: rgba(255,255,255,.6); font-size: .9rem; outline: none; transition: border-color .2s; }
.input-icon-wrapper .input-glass { padding-left: 2.5rem; }
.input-glass:focus { border-color: #2563eb; background: #fff; }

.divider { height: 1px; background: rgba(148,163,184,.2); margin: 2rem 0; }

/* Security Panel */
.security-panel { display: flex; gap: 1.5rem; background: rgba(37,99,235,.03); border: 1px solid rgba(37,99,235,.15); padding: 1.5rem; border-radius: 16px; }
.sp-icon { width: 48px; height: 48px; border-radius: 12px; background: rgba(37,99,235,.1); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.sp-content h3 { font-size: 1.1rem; font-weight: 800; margin: 0 0 .25rem; }
.sp-content p { font-size: .875rem; color: #64748b; margin: 0; }
.pwd-form { max-width: 400px; }

/* Cards List (Awards) */
.cards-list { display: flex; flex-direction: column; gap: 1rem; }
.award-card { display: flex; align-items: center; gap: 1rem; background: #fff; border: 1px solid rgba(148,163,184,.2); padding: 1rem 1.25rem; border-radius: 12px; }
.ac-icon { width: 48px; height: 48px; border-radius: 50%; background: rgba(245,158,11,.1); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.ac-info { flex: 1; }
.ac-info h4 { font-size: 1rem; font-weight: 700; margin: 0 0 .25rem; color: #0f172a; }
.ac-meta { font-size: .8125rem; color: #64748b; }
.btn-outline-sm { display: inline-flex; align-items: center; gap: .3rem; padding: .4rem .8rem; border-radius: 8px; font-size: .75rem; font-weight: 700; color: #475569; border: 1px solid rgba(148,163,184,.3); background: transparent; cursor: pointer; transition: all .15s; }
.btn-outline-sm:hover { border-color: #2563eb; color: #2563eb; }

.empty-state { text-align: center; padding: 2rem; color: #64748b; font-size: .9rem; font-style: italic; background: rgba(248,250,252,.5); border-radius: 12px; border: 1px dashed rgba(148,163,184,.3); }

/* Parents List */
.parent-card { background: #fff; border: 1px solid rgba(148,163,184,.2); border-radius: 16px; overflow: hidden; margin-bottom: 1rem; }
.pc-header { padding: 1.25rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px dashed rgba(148,163,184,.2); }
.avatar-sm { width: 40px; height: 40px; border-radius: 50%; background: #e2e8f0; color: #475569; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 1.1rem; }
.pc-header h4 { font-size: 1rem; font-weight: 700; margin: 0; }
.pc-header p { font-size: .8125rem; color: #64748b; margin: 0; }
.status-badge-sm { font-size: .7rem; font-weight: 700; padding: .2rem .5rem; border-radius: 6px; }

.pc-permissions { padding: 1rem 1.25rem; background: rgba(248,250,252,.5); }
.perm-title { font-size: .8125rem; font-weight: 700; color: #475569; margin-bottom: .75rem; }
.perm-grid { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.perm-item { display: flex; justify-content: space-between; align-items: center; padding: .6rem 1rem; background: #fff; border: 1px solid rgba(148,163,184,.2); border-radius: 8px; font-size: .875rem; font-weight: 500; }
.toggle-btn { background: transparent; border: none; cursor: pointer; padding: 0; display: flex; align-items: center; transition: transform .1s; }
.toggle-btn:active { transform: scale(0.9); }

.invite-panel { background: rgba(37,99,235,.03); border: 1px dashed rgba(37,99,235,.3); padding: 1.5rem; border-radius: 16px; }
.invite-form { display: flex; gap: 1rem; align-items: flex-end; }

/* Buttons */
.btn-primary { display: inline-flex; align-items: center; gap: .4rem; padding: .75rem 1.25rem; border-radius: 10px; font-size: .875rem; font-weight: 700; cursor: pointer; border: none; background: #2563eb; color: #fff; box-shadow: 0 4px 14px rgba(37,99,235,.25); transition: all .15s; }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-icon { width: 32px; height: 32px; border-radius: 8px; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; border: none; background: transparent; transition: all .15s; }

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
