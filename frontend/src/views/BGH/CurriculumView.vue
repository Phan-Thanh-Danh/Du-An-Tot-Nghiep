<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Khung chương trình</h2>
        <p class="text-xs text-muted mt-1">Chi tiết môn học theo từng học kỳ trong chương trình đào tạo</p>
      </div>
      <select v-model="selectedProgram" class="px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary) min-w-[240px]">
        <option v-for="p in programs" :key="p.maChuongTrinh" :value="p.maChuongTrinh">{{ p.tenChuongTrinh }}</option>
      </select>
    </div>

    <div v-if="currentProgram" class="surface-card border border-card rounded-2xl p-5 shadow-sm">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h2 class="text-lg font-bold text-heading">{{ currentProgram.tenChuongTrinh }}</h2>
          <p class="text-xs text-muted mt-0.5">{{ currentProgram.tenChuyenNganh }} · {{ currentProgram.soHocKy }} học kỳ · {{ totalCredits }} tín chỉ</p>
        </div>
        <span :class="statusBadge(currentProgram.trangThai)" class="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider">
          {{ statusLabel(currentProgram.trangThai) }}
        </span>
      </div>
    </div>

    <div v-for="semester in semesters" :key="semester.maChuongTrinhHocKy" class="surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div class="px-5 py-3 bg-(--surface-input)/30 border-b border-default flex items-center justify-between">
        <h3 class="text-sm font-bold text-heading flex items-center gap-2">
          <span class="h-6 w-6 rounded-lg bg-(--lg-primary) text-white flex items-center justify-center text-[11px] font-bold">{{ semester.thuTuHocKy }}</span>
          Học kỳ {{ semester.thuTuHocKy }}
        </h3>
        <span class="text-xs text-muted">{{ semester.subjects.length }} môn · {{ semester.totalCredits }} tín chỉ</span>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="bg-(--surface-card) border-b border-default">
            <tr>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Mã môn</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Tên môn học</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Số tín chỉ</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Loại môn</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Bắt buộc</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Thứ tự</th>
              <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="sub in semester.subjects" :key="sub.maChuongTrinhMonHoc" class="hover:bg-(--surface-input)/50 transition-colors">
              <td class="px-4 py-2.5">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ sub.maCodeMonHoc }}</span>
              </td>
              <td class="px-4 py-2.5 font-semibold text-heading">{{ sub.tenMonHoc }}</td>
              <td class="px-4 py-2.5">
                <span class="font-bold text-heading">{{ sub.soTinChi }}</span>
              </td>
              <td class="px-4 py-2.5">
                <span class="text-xs px-2 py-0.5 rounded bg-(--surface-input) text-muted border border-default">{{ sub.loaiMonHoc }}</span>
              </td>
              <td class="px-4 py-2.5">
                <span v-if="sub.batBuoc" class="text-(--color-success-text) bg-(--color-success-bg) px-2 py-0.5 rounded text-xs font-bold">Bắt buộc</span>
                <span v-else class="text-muted bg-(--surface-input) px-2 py-0.5 rounded text-xs">Tự chọn</span>
              </td>
              <td class="px-4 py-2.5 text-muted">{{ sub.thuTu }}</td>
              <td class="px-4 py-2.5">
                <span :class="sub.conHoatDong ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="text-xs font-bold">
                  {{ sub.conHoatDong ? 'Hoạt động' : 'Ngừng' }}
                </span>
              </td>
            </tr>
          </tbody>
          <tfoot v-if="semester.ghiChu" class="border-t border-default">
            <tr>
              <td colspan="7" class="px-4 py-2 text-xs text-muted italic">
                📝 {{ semester.ghiChu }}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>

    <div v-if="!selectedProgram" class="text-center py-12 text-muted">
      <Library :size="40" class="mx-auto mb-3 opacity-50" />
      <p>Vui lòng chọn chương trình đào tạo để xem khung chương trình.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Library, CheckCircle2, FileText, AlertCircle, Eye, Archive } from 'lucide-vue-next'

const selectedProgram = ref(1)

const programs = [
  { maChuongTrinh: 1, tenChuongTrinh: 'Công nghệ thông tin - Khóa 19', tenChuyenNganh: 'Công nghệ thông tin (Ứng dụng phần mềm)', soHocKy: 7, trangThai: 'active' },
  { maChuongTrinh: 3, tenChuongTrinh: 'Quản trị Kinh doanh - Khóa 19', tenChuyenNganh: 'Quản trị Kinh doanh (Kế toán)', soHocKy: 6, trangThai: 'active' },
  { maChuongTrinh: 4, tenChuongTrinh: 'Thiết kế Đồ họa - Khóa 19', tenChuyenNganh: 'Thiết kế Đồ họa', soHocKy: 6, trangThai: 'approved' },
]

const subjectCatalog = [
  { maMonHoc: 1, maCodeMonHoc: 'COM101', tenMonHoc: 'Tin học cơ sở', soTinChi: 3 },
  { maMonHoc: 2, maCodeMonHoc: 'ENG101', tenMonHoc: 'Tiếng Anh 1', soTinChi: 4 },
  { maMonHoc: 3, maCodeMonHoc: 'MATH101', tenMonHoc: 'Toán rời rạc', soTinChi: 3 },
  { maMonHoc: 4, maCodeMonHoc: 'PRF101', tenMonHoc: 'Kỹ năng nghề nghiệp', soTinChi: 2 },
  { maMonHoc: 5, maCodeMonHoc: 'PRO101', tenMonHoc: 'Lập trình cơ bản', soTinChi: 4 },
  { maMonHoc: 6, maCodeMonHoc: 'ENG201', tenMonHoc: 'Tiếng Anh 2', soTinChi: 4 },
  { maMonHoc: 7, maCodeMonHoc: 'MATH201', tenMonHoc: 'Giải tích', soTinChi: 3 },
  { maMonHoc: 8, maCodeMonHoc: 'PRO202', tenMonHoc: 'Lập trình hướng đối tượng', soTinChi: 4 },
  { maMonHoc: 9, maCodeMonHoc: 'DBI201', tenMonHoc: 'Cơ sở dữ liệu', soTinChi: 3 },
  { maMonHoc: 10, maCodeMonHoc: 'WEB101', tenMonHoc: 'Thiết kế Web', soTinChi: 3 },
  { maMonHoc: 11, maCodeMonHoc: 'PRO301', tenMonHoc: 'Lập trình Java', soTinChi: 4 },
  { maMonHoc: 12, maCodeMonHoc: 'MOB201', tenMonHoc: 'Lập trình di động', soTinChi: 4 },
  { maMonHoc: 13, maCodeMonHoc: 'SWP301', tenMonHoc: 'Quản lý dự án phần mềm', soTinChi: 3 },
  { maMonHoc: 14, maCodeMonHoc: 'CAP401', tenMonHoc: 'Đồ án tốt nghiệp', soTinChi: 8 },
  { maMonHoc: 15, maCodeMonHoc: 'PRF301', tenMonHoc: 'Kỹ năng mềm', soTinChi: 2 },
  { maMonHoc: 16, maCodeMonHoc: 'NET301', tenMonHoc: 'Lập trình .NET', soTinChi: 4 },
  { maMonHoc: 17, maCodeMonHoc: 'TST301', tenMonHoc: 'Kiểm thử phần mềm', soTinChi: 3 },
  { maMonHoc: 18, maCodeMonHoc: 'SEC301', tenMonHoc: 'An toàn bảo mật', soTinChi: 3 },
  { maMonHoc: 19, maCodeMonHoc: 'MAS301', tenMonHoc: 'Quản trị mạng', soTinChi: 3 },
  { maMonHoc: 20, maCodeMonHoc: 'PRO401', tenMonHoc: 'Lập trình nâng cao', soTinChi: 4 },
]

const semesterData = [
  { maChuongTrinh: 1, maChuongTrinhHocKy: 1, thuTuHocKy: 1, monHocs: [1, 2, 3, 4, 5], ghiChu: '' },
  { maChuongTrinh: 1, maChuongTrinhHocKy: 2, thuTuHocKy: 2, monHocs: [6, 7, 8, 9, 15], ghiChu: '' },
  { maChuongTrinh: 1, maChuongTrinhHocKy: 3, thuTuHocKy: 3, monHocs: [10, 11, 16, 17], ghiChu: '' },
  { maChuongTrinh: 1, maChuongTrinhHocKy: 4, thuTuHocKy: 4, monHocs: [12, 13, 18, 19], ghiChu: '' },
  { maChuongTrinh: 1, maChuongTrinhHocKy: 5, thuTuHocKy: 5, monHocs: [20], ghiChu: 'Học kỳ thực tập doanh nghiệp' },
  { maChuongTrinh: 1, maChuongTrinhHocKy: 6, thuTuHocKy: 6, monHocs: [14], ghiChu: 'Học kỳ làm đồ án tốt nghiệp' },
]

const currentProgram = computed(() => programs.find(p => p.maChuongTrinh === selectedProgram.value))

const totalCredits = computed(() => {
  let total = 0
  for (const sem of semesters.value) {
    for (const sub of sem.subjects) {
      total += sub.soTinChi
    }
  }
  return total
})

const semesters = computed(() => {
  const filtered = semesterData.filter(s => s.maChuongTrinh === selectedProgram.value)
  return filtered.map(sem => ({
    ...sem,
    subjects: sem.monHocs.map(id => {
      const mon = subjectCatalog.find(m => m.maMonHoc === id)
      return {
        maChuongTrinhMonHoc: id,
        maCodeMonHoc: mon.maCodeMonHoc,
        tenMonHoc: mon.tenMonHoc,
        soTinChi: mon.soTinChi,
        loaiMonHoc: id % 2 === 0 ? 'Cơ sở ngành' : 'Chuyên ngành',
        batBuoc: id !== 15,
        thuTu: sem.monHocs.indexOf(id) + 1,
        conHoatDong: true,
      }
    }),
    totalCredits: sem.monHocs.reduce((sum, id) => sum + (subjectCatalog.find(m => m.maMonHoc === id)?.soTinChi || 0), 0),
  }))
})

function statusBadge(status) {
  switch (status) {
    case 'active': return 'bg-(--color-success-bg) text-(--color-success-text)'
    case 'approved': return 'bg-(--color-info-bg) text-(--color-info-text)'
    default: return 'bg-(--surface-input) text-muted'
  }
}

function statusLabel(status) {
  switch (status) {
    case 'active': return 'Đang áp dụng'
    case 'approved': return 'Đã duyệt'
    default: return status
  }
}
</script>
