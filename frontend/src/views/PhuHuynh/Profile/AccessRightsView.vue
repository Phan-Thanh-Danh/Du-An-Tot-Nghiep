<script setup>
import {
  ShieldCheck,
  CheckCircle2,
  Lock,
  FileText,
  Calendar,
  CreditCard,
  UserCheck,
  Info
} from 'lucide-vue-next'
import { childrenData } from '@/components/PhuHuynh/data/parentData.js'

// Mock access rights data
const accessCategories = [
  {
    id: 1,
    name: 'Quản lý học thuật',
    description: 'Quyền xem và giám sát kết quả học tập, chuyên cần',
    icon: FileText,
    rights: [
      { name: 'Xem bảng điểm chi tiết (Điểm thành phần, Điểm thi)', granted: true },
      { name: 'Xem thông tin điểm danh & lịch sử vắng mặt', granted: true },
      { name: 'Xem cảnh báo học vụ, nguy cơ đình chỉ học', granted: true },
      { name: 'Đề nghị phúc khảo điểm thay sinh viên', granted: false }
    ]
  },
  {
    id: 2,
    name: 'Lịch trình & Lên lớp',
    description: 'Quyền theo dõi thời khóa biểu và các hoạt động',
    icon: Calendar,
    rights: [
      { name: 'Xem thời khóa biểu hàng tuần', granted: true },
      { name: 'Xem lịch thi, phòng thi, số báo danh', granted: true },
      { name: 'Đăng ký môn học thay sinh viên', granted: false }
    ]
  },
  {
    id: 3,
    name: 'Quản lý tài chính',
    description: 'Quyền xem thông tin công nợ và đóng học phí',
    icon: CreditCard,
    rights: [
      { name: 'Xem chi tiết các khoản thu, học phí từng kỳ', granted: true },
      { name: 'Thanh toán học phí trực tuyến', granted: true },
      { name: 'Xem lịch sử giao dịch và biên lai (Hóa đơn điện tử)', granted: true },
      { name: 'Yêu cầu hoàn trả học phí, bảo lưu', granted: false }
    ]
  },
  {
    id: 4,
    name: 'Hồ sơ cá nhân & Hệ thống',
    description: 'Quyền truy cập thông tin liên lạc và cập nhật',
    icon: UserCheck,
    rights: [
      { name: 'Xem thông tin hồ sơ sinh viên (Ngành học, Cố vấn học tập)', granted: true },
      { name: 'Thay đổi thông tin liên lạc của sinh viên', granted: false },
      { name: 'Nhận thông báo tự động qua SMS/Zalo/Email', granted: true }
    ]
  }
]
</script>

<template>
  <div class="space-y-6 pb-6">
    <!-- Header Banner -->
    <div class="relative rounded-[24px] bg-gradient-to-br from-orange-600 to-amber-500 p-6 sm:p-8 overflow-hidden shadow-(--lg-shadow-md) flex items-center justify-between">
      <div class="absolute inset-0 opacity-10 bg-[url('data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cg fill=\'none\' fill-rule=\'evenodd\'%3E%3Cg fill=\'%23ffffff\' fill-opacity=\'1\'%3E%3Cpath d=\'M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E')]"></div>
      
      <div class="relative z-10 text-white max-w-2xl">
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight mb-2 flex items-center gap-3">
          <ShieldCheck :size="32" />
          Quyền truy cập được cấp
        </h1>
        <p class="text-orange-100 font-medium text-sm sm:text-base leading-relaxed">
          Phụ huynh được Nhà trường cấp các quyền giám sát nhằm đồng hành và quản lý tiến trình học tập của con em. Một số quyền thao tác trực tiếp đã bị khóa để đảm bảo tính tự lập của sinh viên.
        </p>
      </div>
    </div>

    <!-- Student Scope Note -->
    <div class="flex items-start gap-3 p-4 rounded-xl border border-orange-200 dark:border-orange-950/30 bg-orange-50 dark:bg-orange-950/10">
      <Info :size="20" class="text-orange-600 mt-0.5 shrink-0" />
      <div>
        <p class="text-sm font-bold text-heading">Phạm vi áp dụng</p>
        <p class="text-xs text-body mt-1 leading-relaxed">
          Các quyền truy cập dưới đây được áp dụng chung cho tất cả các sinh viên mà bạn đang giám sát trên hệ thống EduLMS (bao gồm: <span class="font-bold">{{ childrenData.map(c => c.name).join(', ') }}</span>). 
        </p>
      </div>
    </div>

    <!-- Rights List Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div
        v-for="category in accessCategories"
        :key="category.id"
        class="lg-glass p-6 rounded-[24px]"
      >
        <div class="flex items-center gap-3 border-b border-card pb-4 mb-5">
          <div class="h-10 w-10 rounded-xl bg-orange-100 dark:bg-orange-950/30 text-orange-600 flex items-center justify-center shadow-sm">
            <component :is="category.icon" :size="20" stroke-width="2.5" />
          </div>
          <div>
            <h2 class="text-base font-bold text-heading">{{ category.name }}</h2>
            <p class="text-[11px] text-muted">{{ category.description }}</p>
          </div>
        </div>
        
        <div class="space-y-3">
          <div
            v-for="(right, idx) in category.rights"
            :key="idx"
            class="flex items-start gap-3 p-2 rounded-lg transition-colors hover:bg-(--surface-input)"
          >
            <div class="mt-0.5 shrink-0">
              <CheckCircle2 v-if="right.granted" :size="16" class="text-emerald-500" />
              <Lock v-else :size="16" class="text-slate-400 dark:text-slate-500" />
            </div>
            <div>
              <p :class="[
                'text-sm font-semibold',
                right.granted ? 'text-heading' : 'text-muted line-through opacity-70'
              ]">
                {{ right.name }}
              </p>
              <p class="text-[10px] mt-0.5" :class="right.granted ? 'text-emerald-600 dark:text-emerald-500 font-medium' : 'text-slate-500'">
                {{ right.granted ? 'Được phép' : 'Không được phép' }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
