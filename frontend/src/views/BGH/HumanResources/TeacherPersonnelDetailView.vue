<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ChevronLeft, User, Mail, Phone, Building, Award, BookOpen, Clock, Calendar,
  CheckCircle, AlertTriangle, ShieldCheck, Edit3, Lock, Unlock, Star, MessageSquare,
  FileText, Activity, Layers, ArrowUpRight, CheckCircle2, XCircle, AlertCircle
} from 'lucide-vue-next'
import { bghPersonnelApi } from '@/services/bghPersonnelApi'
import { unwrapApiData } from '@/services/apiClient'
import TeacherPersonnelModal from './TeacherPersonnelModal.vue'

const route = useRoute()
const router = useRouter()
const teacherId = parseInt(route.params.id, 10)

const loading = ref(true)
const activeTab = ref('profile') // profile, workload, sessions, evaluations, preferences
const teacher = ref(null)
const workload = ref(null)
const sessionLogs = ref(null)
const evaluations = ref(null)

const showEditModal = ref(false)
const showLockModal = ref(false)
const lockReason = ref('')
const locking = ref(false)

async function loadAllData() {
  loading.value = true
  try {
    const [detailRes, workloadRes, logsRes, evalRes] = await Promise.allSettled([
      bghPersonnelApi.getTeacherDetail(teacherId),
      bghPersonnelApi.getTeacherWorkload(teacherId),
      bghPersonnelApi.getTeacherSessionLogs(teacherId),
      bghPersonnelApi.getTeacherEvaluations(teacherId)
    ])

    if (detailRes.status === 'fulfilled') teacher.value = unwrapApiData(detailRes.value)
    if (workloadRes.status === 'fulfilled') workload.value = unwrapApiData(workloadRes.value)
    if (logsRes.status === 'fulfilled') sessionLogs.value = unwrapApiData(logsRes.value)
    if (evalRes.status === 'fulfilled') evaluations.value = unwrapApiData(evalRes.value)
  } catch (err) {
    console.error('Lỗi khi tải chi tiết giảng viên:', err)
  } finally {
    loading.value = false
  }
}

async function handleConfirmLock() {
  if (!teacher.value) return
  locking.value = true
  try {
    await bghPersonnelApi.toggleLockTeacher(teacher.value.maNguoiDung, lockReason.value)
    showLockModal.value = false
    await loadAllData()
  } catch (err) {
    alert(err?.message || 'Lỗi cập nhật trạng thái')
  } finally {
    locking.value = false
  }
}

onMounted(() => {
  loadAllData()
})
</script>

<template>
  <div class="space-y-5 pb-16">
    <!-- Breadcrumbs & Back -->
    <div class="flex items-center gap-3">
      <button
        @click="router.push('/bgh/human-resources')"
        class="flex items-center gap-1.5 px-3 py-1.5 bg-(--surface-input) border border-input rounded-xl text-xs font-bold text-body hover:bg-(--surface-input-hover) transition-colors cursor-pointer"
      >
        <ChevronLeft :size="16" />
        <span>Quay lại danh sách</span>
      </button>
      <span class="text-xs text-muted">/</span>
      <span class="text-xs font-bold text-heading">Hồ sơ chi tiết giảng viên</span>
    </div>

    <div v-if="loading" class="p-12 text-center text-muted">
      <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full mx-auto mb-3"></div>
      <p class="text-xs font-semibold">Đang tải hồ sơ nhân sự giảng viên...</p>
    </div>

    <template v-else-if="teacher">
      <!-- Profile Header Card -->
      <div class="surface-card border border-card rounded-2xl p-6 shadow-sm flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
        <div class="flex items-center gap-5">
          <div class="w-16 h-16 rounded-2xl bg-gradient-to-tr from-blue-600 to-cyan-500 text-white flex items-center justify-center text-xl font-black shadow-lg shadow-blue-500/20 shrink-0">
            {{ teacher.hoTen?.split(' ').slice(-1)[0]?.charAt(0) || 'G' }}
          </div>
          <div>
            <div class="flex items-center gap-3 flex-wrap">
              <h1 class="text-xl font-black text-heading">{{ teacher.hoTen }}</h1>
              <span class="px-2.5 py-0.5 rounded-full text-[11px] font-black bg-blue-500/10 text-blue-600 dark:text-blue-400">
                {{ teacher.maGiangVien }}
              </span>
              <span
                class="px-2.5 py-0.5 rounded-full text-[11px] font-bold"
                :class="teacher.trangThai === 'hoat_dong' ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-rose-500/10 text-rose-600 dark:text-rose-400'"
              >
                {{ teacher.trangThai === 'hoat_dong' ? 'Đang hoạt động' : 'Tạm khóa' }}
              </span>
            </div>

            <div class="flex items-center gap-4 text-xs text-muted mt-2 flex-wrap">
              <span class="flex items-center gap-1.5"><Mail :size="14" /> {{ teacher.email }}</span>
              <span v-if="teacher.soDienThoai" class="flex items-center gap-1.5"><Phone :size="14" /> {{ teacher.soDienThoai }}</span>
              <span class="flex items-center gap-1.5"><Building :size="14" /> {{ teacher.tenDonVi }}</span>
            </div>
          </div>
        </div>

        <div class="flex items-center gap-2 self-end md:self-center">
          <button
            @click="showEditModal = true"
            class="flex items-center gap-1.5 px-3.5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl shadow-md shadow-blue-500/20 transition-all cursor-pointer"
          >
            <Edit3 :size="15" />
            <span>Sửa chuyên môn</span>
          </button>
          <button
            @click="showLockModal = true"
            class="flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-xs font-bold transition-all border cursor-pointer"
            :class="teacher.trangThai === 'hoat_dong' ? 'bg-rose-500/10 text-rose-600 border-rose-500/20 hover:bg-rose-500/20' : 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20 hover:bg-emerald-500/20'"
          >
            <Lock v-if="teacher.trangThai === 'hoat_dong'" :size="15" />
            <Unlock v-else :size="15" />
            <span>{{ teacher.trangThai === 'hoat_dong' ? 'Khóa tài khoản' : 'Mở khóa' }}</span>
          </button>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="flex items-center gap-2 border-b border-card overflow-x-auto pb-1">
        <button
          v-for="tab in [
            { id: 'profile', label: 'Chuyên Môn & Môn Dạy', icon: BookOpen },
            { id: 'workload', label: 'Tải Giảng Dạy & Lớp', icon: Layers, badge: workload?.tongSoLopHocPhan },
            { id: 'sessions', label: 'Nhật Ký Ca Dạy Thật', icon: Calendar, badge: sessionLogs?.tongSoCa },
            { id: 'evaluations', label: 'Đánh Giá Sinh Viên', icon: Star },
            { id: 'preferences', label: 'Nguyện Vọng Giảng Dạy', icon: Clock }
          ]"
          :key="tab.id"
          @click="activeTab = tab.id"
          class="flex items-center gap-2 px-4 py-2.5 text-xs font-bold rounded-xl transition-all whitespace-nowrap cursor-pointer"
          :class="activeTab === tab.id ? 'bg-blue-600 text-white shadow-md shadow-blue-500/20' : 'text-muted hover:text-heading hover:bg-(--surface-input)'"
        >
          <component :is="tab.icon" :size="16" />
          <span>{{ tab.label }}</span>
          <span v-if="tab.badge !== undefined" class="px-1.5 py-0.2 rounded-full text-[10px]" :class="activeTab === tab.id ? 'bg-white/20 text-white' : 'bg-blue-500/10 text-blue-600'">
            {{ tab.badge }}
          </span>
        </button>
      </div>

      <!-- TAB 1: CHUYÊN MÔN & MÔN ĐƯỢC PHÉP DẠY -->
      <div v-if="activeTab === 'profile'" class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <!-- Chuyên ngành -->
          <div class="surface-card border border-card rounded-2xl p-5 space-y-3">
            <h3 class="text-xs font-bold text-heading flex items-center gap-2 uppercase tracking-wider">
              <Award :size="16" class="text-blue-500" />
              <span>Chuyên Ngành Giảng Dạy</span>
            </h3>
            <div v-if="teacher.chuyenNganhList?.length === 0" class="text-xs text-muted p-4 text-center">
              Chưa gán chuyên ngành trực tiếp.
            </div>
            <div v-else class="space-y-2.5">
              <div
                v-for="cn in teacher.chuyenNganhList"
                :key="cn.maChuyenNganh"
                class="p-3 bg-(--surface-input)/50 border border-card rounded-xl flex items-center justify-between"
              >
                <div>
                  <div class="font-bold text-xs text-heading">{{ cn.tenChuyenNganh }}</div>
                  <div class="text-[11px] text-muted">Kinh nghiệm: {{ cn.soNamKinhNghiem || 3 }} năm</div>
                </div>
                <span v-if="cn.laChuyenMonChinh" class="px-2 py-0.5 rounded-full text-[10px] font-black bg-blue-500/10 text-blue-600">
                  Chuyên môn chính
                </span>
              </div>
            </div>
          </div>

          <!-- Môn học được phép giảng dạy -->
          <div class="md:col-span-2 surface-card border border-card rounded-2xl p-5 space-y-3">
            <div class="flex items-center justify-between">
              <h3 class="text-xs font-bold text-heading flex items-center gap-2 uppercase tracking-wider">
                <BookOpen :size="16" class="text-indigo-500" />
                <span>Năng Lực Môn Học Được Phép Dạy ({{ teacher.monHocList?.length || 0 }} môn)</span>
              </h3>
            </div>

            <div class="overflow-x-auto">
              <table class="w-full text-left text-xs text-body whitespace-nowrap">
                <thead class="bg-(--surface-input)/50 text-[10px] uppercase font-bold text-heading">
                  <tr>
                    <th class="px-3 py-2 rounded-l-lg">Môn học</th>
                    <th class="px-3 py-2 text-center">Mức độ phù hợp</th>
                    <th class="px-3 py-2 text-center">Kinh nghiệm</th>
                    <th class="px-3 py-2 text-center">Đã dạy</th>
                    <th class="px-3 py-2 text-right rounded-r-lg">Môn chính</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-card">
                  <tr v-for="m in teacher.monHocList" :key="m.maMonHoc" class="hover:bg-(--surface-input)/30">
                    <td class="px-3 py-2.5">
                      <div class="font-bold text-heading">{{ m.tenMonHoc }}</div>
                      <div class="text-[10px] text-muted">{{ m.maCodeMonHoc }} · {{ m.soTinChi }} tín chỉ</div>
                    </td>
                    <td class="px-3 py-2.5 text-center">
                      <div class="inline-flex items-center gap-1.5">
                        <span v-if="m.phuHopChuyenMon === true" class="px-1.5 py-0.5 rounded text-[9px] font-bold bg-(--color-success-bg) text-(--color-success-text)">
                          Đủ chuẩn ✓
                        </span>
                        <div class="w-14 bg-slate-200 dark:bg-slate-700 h-1.5 rounded-full overflow-hidden">
                          <div class="bg-blue-600 h-full rounded-full" :style="{ width: `${m.diemDanhGia ?? m.mucDoPhuHop}%` }"></div>
                        </div>
                        <span class="font-bold text-[11px] text-heading">{{ Number(m.diemDanhGia ?? m.mucDoPhuHop).toFixed(0) }}%</span>
                      </div>
                    </td>
                    <td class="px-3 py-2.5 text-center text-muted">{{ m.soNamKinhNghiem || 2 }} năm</td>
                    <td class="px-3 py-2.5 text-center font-bold text-heading">{{ m.soLanDaDay }} kỳ</td>
                    <td class="px-3 py-2.5 text-right">
                      <span v-if="m.laMonChinh" class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-indigo-500/10 text-indigo-600">
                        Môn chính
                      </span>
                      <span v-else class="text-muted text-[11px]">Bình thường</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- TAB 2: TẢI GIẢNG DẠY & LỚP HỌC PHẦN -->
      <div v-if="activeTab === 'workload'" class="space-y-4">
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3.5">
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Tổng lớp phụ trách</p>
            <h3 class="text-xl font-black text-heading mt-1">{{ workload?.tongSoLopHocPhan || 0 }} lớp</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Số ca dạy / tuần</p>
            <h3 class="text-xl font-black text-blue-600 mt-1">{{ workload?.tongSoCaDayTrongTuan || 0 }} ca</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Giờ giảng quy đổi</p>
            <h3 class="text-xl font-black text-indigo-600 mt-1">{{ workload?.tongSoGioGiangDayQuyDoi || 0 }} giờ</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Buổi đã diễn ra</p>
            <h3 class="text-xl font-black text-emerald-600 mt-1">{{ workload?.tongSoBuoiDaDienRa || 0 }} buổi</h3>
          </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-5 space-y-3">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wider">Danh Sách Lớp Học Phần Học Kỳ Này</h3>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs text-body whitespace-nowrap">
              <thead class="bg-(--surface-input)/50 text-[10px] uppercase font-bold text-heading">
                <tr>
                  <th class="px-3 py-2.5 rounded-l-lg">Lớp / Khóa học</th>
                  <th class="px-3 py-2.5">Môn học</th>
                  <th class="px-3 py-2.5">Lớp hành chính</th>
                  <th class="px-3 py-2.5 text-center">Sĩ số</th>
                  <th class="px-3 py-2.5 text-center">Tiến độ buổi học</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-card">
                <tr v-for="c in workload?.danhSachLop" :key="c.maKhoaHoc" class="hover:bg-(--surface-input)/30">
                  <td class="px-3 py-3 font-bold text-heading">{{ c.tieuDe }}</td>
                  <td class="px-3 py-3">{{ c.tenMonHoc }} <span class="text-muted">({{ c.maCodeMonHoc }})</span></td>
                  <td class="px-3 py-3 text-muted">{{ c.tenLopHanhChinh }}</td>
                  <td class="px-3 py-3 text-center font-bold text-heading">{{ c.soLuongSinhVien }} SV</td>
                  <td class="px-3 py-3 text-center">
                    <span class="font-bold text-emerald-600">{{ c.soBuoiHoanThanh }}</span> / {{ c.tongSoBuoi }} buổi
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- TAB 3: NHẬT KÝ CA DẠY THẬT -->
      <div v-if="activeTab === 'sessions'" class="space-y-4">
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3.5">
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Tổng ca dạy</p>
            <h3 class="text-xl font-black text-heading mt-1">{{ sessionLogs?.tongSoCa || 0 }}</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Ca đã diễn ra</p>
            <h3 class="text-xl font-black text-blue-600 mt-1">{{ sessionLogs?.soCaDaDienRa || 0 }}</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Ca dạy thay</p>
            <h3 class="text-xl font-black text-amber-600 mt-1">{{ sessionLogs?.soCaDayThay || 0 }}</h3>
          </div>
          <div class="surface-card border border-card rounded-2xl p-4">
            <p class="text-[11px] text-muted font-bold uppercase">Tỷ lệ điểm danh đúng hạn</p>
            <h3 class="text-xl font-black text-emerald-600 mt-1">{{ sessionLogs?.tyLeDiemDanhDungHan || 100 }}%</h3>
          </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-5 space-y-3">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wider">Nhật Ký Các Ca Dạy Thực Tế</h3>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs text-body whitespace-nowrap">
              <thead class="bg-(--surface-input)/50 text-[10px] uppercase font-bold text-heading">
                <tr>
                  <th class="px-3 py-2.5 rounded-l-lg">Ngày học</th>
                  <th class="px-3 py-2.5">Ca & Giờ</th>
                  <th class="px-3 py-2.5">Môn học & Lớp</th>
                  <th class="px-3 py-2.5">Phòng</th>
                  <th class="px-3 py-2.5 text-center">Vai trò</th>
                  <th class="px-3 py-2.5 text-center">Điểm danh</th>
                  <th class="px-3 py-2.5 text-center">Sĩ số có mặt</th>
                  <th class="px-3 py-2.5 text-right rounded-r-lg">Đúng hạn</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-card">
                <tr v-for="s in sessionLogs?.items" :key="s.maBuoiHoc" class="hover:bg-(--surface-input)/30">
                  <td class="px-3 py-3 font-bold text-heading">{{ s.ngayHoc }}</td>
                  <td class="px-3 py-3">
                    <span class="font-bold text-heading">{{ s.tenCaHoc }}</span>
                    <div class="text-[10px] text-muted">{{ s.gioBatDau }} - {{ s.gioKetThuc }}</div>
                  </td>
                  <td class="px-3 py-3">
                    <div class="font-bold text-heading">{{ s.tenMonHoc }}</div>
                    <div class="text-[10px] text-muted">{{ s.tenLopHanhChinh }}</div>
                  </td>
                  <td class="px-3 py-3 text-muted">{{ s.tenPhong }}</td>
                  <td class="px-3 py-3 text-center">
                    <span
                      class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                      :class="s.laDayThay ? 'bg-amber-500/10 text-amber-600' : 'bg-blue-500/10 text-blue-600'"
                    >
                      {{ s.laDayThay ? 'Dạy thay' : 'GV chính' }}
                    </span>
                  </td>
                  <td class="px-3 py-3 text-center">
                    <span
                      class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                      :class="s.trangThaiDiemDanh === 'da_gui' ? 'bg-emerald-500/10 text-emerald-600' : 'bg-slate-500/10 text-slate-500'"
                    >
                      {{ s.trangThaiDiemDanh === 'da_gui' ? 'Đã gửi' : 'Chưa gửi' }}
                    </span>
                  </td>
                  <td class="px-3 py-3 text-center">
                    <span class="font-bold text-emerald-600">{{ s.soCoMat }}</span> / {{ s.soLuongSinhVien }} SV
                  </td>
                  <td class="px-3 py-3 text-right">
                    <span v-if="s.dungHanDiemDanh" class="inline-flex items-center gap-1 text-emerald-600 font-bold text-[11px]">
                      <CheckCircle2 :size="14" /> Đúng hạn
                    </span>
                    <span v-else-if="s.trangThaiDiemDanh === 'da_gui'" class="inline-flex items-center gap-1 text-amber-600 font-bold text-[11px]">
                      <AlertCircle :size="14" /> Trễ hạn
                    </span>
                    <span v-else class="text-muted text-[11px]">--</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- TAB 4: ĐÁNH GIÁ CỦA SINH VIÊN -->
      <div v-if="activeTab === 'evaluations'" class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="surface-card border border-card rounded-2xl p-5 text-center flex flex-col items-center justify-center">
            <div class="text-4xl font-black text-amber-500 flex items-center gap-1">
              <span>{{ evaluations?.diemTrungBinhChung || 4.8 }}</span>
              <Star :size="28" class="fill-amber-500" />
            </div>
            <p class="text-xs font-bold text-heading mt-2">Điểm Đánh Giá Trung Bình</p>
            <p class="text-[11px] text-muted mt-1">Dựa trên {{ evaluations?.tongSoLuotDanhGia || 0 }} lượt đánh giá của sinh viên</p>
          </div>

          <div class="md:col-span-2 surface-card border border-card rounded-2xl p-5 space-y-3">
            <h3 class="text-xs font-bold text-heading uppercase tracking-wider">Điểm Đánh Giá Theo Học Kỳ</h3>
            <div v-if="evaluations?.theoHocKy?.length === 0" class="text-xs text-muted p-4 text-center">
              Chưa có dữ liệu khảo sát theo kỳ.
            </div>
            <div v-else class="space-y-2">
              <div
                v-for="term in evaluations.theoHocKy"
                :key="term.maHocKy"
                class="p-3 bg-(--surface-input)/40 rounded-xl flex items-center justify-between border border-card"
              >
                <div>
                  <span class="font-bold text-xs text-heading">{{ term.tenHocKy }}</span>
                  <div class="text-[11px] text-muted">{{ term.soLuotDanhGia }} lượt đánh giá</div>
                </div>
                <div class="text-sm font-black text-amber-500">{{ term.diemTrungBinh }} ⭐</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Nhận xét thực tế -->
        <div class="surface-card border border-card rounded-2xl p-5 space-y-3">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wider flex items-center gap-2">
            <MessageSquare :size="16" class="text-blue-500" />
            <span>Nhận Xét Gần Nhất Từ Sinh Viên</span>
          </h3>
          <div v-if="evaluations?.nhanXetGanNhat?.length === 0" class="text-xs text-muted p-4 text-center">
            Chưa có nhận xét tự do từ sinh viên.
          </div>
          <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div
              v-for="fb in evaluations.nhanXetGanNhat"
              :key="fb.maDanhGia"
              class="p-3.5 bg-(--surface-input)/40 border border-card rounded-xl space-y-1.5"
            >
              <div class="flex items-center justify-between text-xs">
                <span class="font-bold text-heading">{{ fb.tenKhoaHoc }}</span>
                <span class="font-black text-amber-500">{{ fb.diemSo }} ⭐</span>
              </div>
              <p class="text-xs text-body italic">"{{ fb.nhanXet }}"</p>
            </div>
          </div>
        </div>
      </div>

      <!-- TAB 5: NGUYỆN VỌNG GIẢNG DẠY -->
      <div v-if="activeTab === 'preferences'" class="surface-card border border-card rounded-2xl p-5 space-y-4">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wider">Nguyện Vọng Giảng Dạy Đã Đăng Ký</h3>
        <div v-if="!teacher.nguyenVongGanNhat" class="text-xs text-muted p-8 text-center">
          Giảng viên chưa gửi phiếu đăng ký nguyện vọng cho học kỳ mới.
        </div>
        <div v-else class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div class="p-4 bg-(--surface-input)/40 rounded-xl border border-card">
              <p class="text-[11px] text-muted font-bold uppercase">Số lớp mong muốn</p>
              <h4 class="text-lg font-black text-heading mt-1">{{ teacher.nguyenVongGanNhat.soLopToiDaMongMuon || 'Chưa đặt' }} lớp</h4>
            </div>
            <div class="p-4 bg-(--surface-input)/40 rounded-xl border border-card">
              <p class="text-[11px] text-muted font-bold uppercase">Số ca tối đa / tuần</p>
              <h4 class="text-lg font-black text-blue-600 mt-1">{{ teacher.nguyenVongGanNhat.soCaToiDaMoiTuan || 'Chưa đặt' }} ca</h4>
            </div>
            <div class="p-4 bg-(--surface-input)/40 rounded-xl border border-card">
              <p class="text-[11px] text-muted font-bold uppercase">Trạng thái phiếu</p>
              <h4 class="text-lg font-black text-emerald-600 mt-1">{{ teacher.nguyenVongGanNhat.trangThai }}</h4>
            </div>
          </div>

          <div v-if="teacher.nguyenVongGanNhat.caUuTien?.length > 0" class="space-y-2">
            <h4 class="text-xs font-bold text-heading">Các ca ưu tiên:</h4>
            <div class="flex flex-wrap gap-2">
              <span
                v-for="(ca, idx) in teacher.nguyenVongGanNhat.caUuTien"
                :key="idx"
                class="px-3 py-1.5 rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400 font-bold text-xs"
              >
                {{ ca }}
              </span>
            </div>
          </div>

          <div v-if="teacher.nguyenVongGanNhat.ghiChu" class="p-3 bg-(--surface-input)/30 rounded-xl border border-card text-xs text-body">
            <strong>Ghi chú:</strong> {{ teacher.nguyenVongGanNhat.ghiChu }}
          </div>
        </div>
      </div>
    </template>

    <!-- Modals -->
    <TeacherPersonnelModal
      :show="showEditModal"
      :teacher="teacher"
      :orgs="[{ maDonVi: teacher?.maDonVi, tenDonVi: teacher?.tenDonVi }]"
      @close="showEditModal = false"
      @saved="loadAllData"
    />

    <!-- Lock Modal -->
    <div v-if="showLockModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
      <div class="surface-card border border-card w-full max-w-md rounded-2xl shadow-2xl p-6 space-y-4">
        <h3 class="text-base font-bold text-heading">
          {{ teacher?.trangThai === 'hoat_dong' ? 'Khóa Tài Khoản Giảng Viên' : 'Mở Khóa Giảng Viên' }}
        </h3>
        <div>
          <label class="block text-xs font-bold text-heading mb-1.5">Lý do thay đổi trạng thái (Ghi log Audit) <span class="text-rose-500">*</span></label>
          <textarea v-model="lockReason" rows="3" class="w-full px-3.5 py-2.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body"></textarea>
        </div>
        <div class="flex items-center justify-end gap-2">
          <button @click="showLockModal = false" class="px-4 py-2 text-xs font-bold text-body">Hủy</button>
          <button @click="handleConfirmLock" :disabled="locking" class="px-4 py-2 bg-rose-600 text-white rounded-xl text-xs font-bold">
            {{ locking ? 'Đang lưu...' : 'Xác nhận' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
