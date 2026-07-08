<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft,
  User,
  AlertTriangle,
  Calendar,
  BookOpen,
  GraduationCap,
  TrendingDown,
  CheckCircle2,
  XCircle,
  Clock,
  Award,
  Loader2,
  AlertCircle,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const route = useRoute()
const router = useRouter()
const studentId = Number(route.params.studentId)
const loading = ref(false)
const error = ref(null)

const student = ref(null)
const academicHistory = ref([])
const attendanceHistory = ref([])

function getRiskBadge(risk) {
  switch (risk) {
    case 'critical': return 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20'
    case 'high': return 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20'
    case 'medium': return 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20'
    default: return 'surface-solid text-muted border-default'
  }
}

function deriveRisk(s) {
  if (!s) return 'unknown'
  return s.failCount >= 3 ? 'critical' : s.failCount >= 2 ? 'high' : 'medium'
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getAtRiskStudents()
    const data = unwrapApiData(res)
    if (data?.students) {
      const found = data.students.find(s => s.id === studentId)
      if (found) {
        student.value = {
          id: found.id,
          name: found.name,
          code: found.email || '—',
          class: found.classCode || '—',
          subject: '',
          grade: found.avgGpa,
          attendance: null,
          risk: deriveRisk(found),
          reason: `Đã rớt ${found.failCount} môn. GPA hiện tại: ${found.avgGpa}.`,
          email: found.email || '',
          failCount: found.failCount,
        }
      }
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

function goBack() {
  router.push('/bgh/academic/at-risk')
}
</script>

<template>
  <div v-if="loading" class="p-8">
    <div class="flex items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-placeholder" />
    </div>
  </div>
  <div v-else-if="error" class="p-8">
    <div class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
  </div>
  <PageContainer
    v-else-if="student"
    :title="`Lịch sử học tập — ${student.name}`"
    :subtitle="`Mã SV: ${student.code} • Lớp: ${student.class}`"
  >
    <template #actions>
      <button @click="goBack" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
        <ArrowLeft :size="18" /> Quay lại
      </button>
    </template>

    <div class="space-y-6">

      <!-- ── Student Summary Card ── -->
      <div class="surface-card border border-card rounded-2xl p-5">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div class="flex items-center gap-4">
            <div class="h-14 w-14 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center border border-(--color-danger-text)/20">
              <User :size="28" class="text-(--color-danger-text)" />
            </div>
            <div>
              <h2 class="text-lg font-semibold text-heading">{{ student.name }}</h2>
              <div class="flex items-center gap-3 mt-1">
                <span class="text-xs font-bold text-muted">{{ student.code }}</span>
                <span class="text-xs font-bold text-placeholder">|</span>
                <span class="text-xs font-bold text-muted">Lớp {{ student.class }}</span>
              </div>
            </div>
          </div>
          <div :class="['px-3 py-1.5 rounded-full text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getRiskBadge(student.risk)]">
            {{ student.risk }}
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

        <!-- ── GPA Trend ── -->
        <div class="lg:col-span-2 space-y-6">

          <!-- ── Academic History by Semester ── -->
          <div v-if="academicHistory.length">
            <div v-for="sem in academicHistory" :key="sem.semester" class="surface-card border border-card rounded-2xl overflow-hidden">
              <div class="px-5 py-4 bg-(--color-info-bg) border-b border-(--color-info-text)/20 flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <Calendar :size="16" class="text-(--color-info-text)" />
                  <h4 class="text-xs font-bold text-(--color-info-text) uppercase tracking-widest">{{ sem.semester }}</h4>
                </div>
                <span class="text-[10px] font-semibold text-label">
                  TB: {{ (sem.courses.reduce((s, c) => s + c.grade, 0) / sem.courses.length).toFixed(1) }}
                </span>
              </div>
              <div class="overflow-x-auto">
                <table class="w-full text-left">
                  <thead>
                    <tr class="surface-solid">
                      <th class="px-5 py-3 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Môn học</th>
                      <th class="px-5 py-3 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tín chỉ</th>
                      <th class="px-5 py-3 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Điểm</th>
                      <th class="px-5 py-3 text-[10px] font-semibold text-placeholder uppercase tracking-widest">Kết quả</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="course in sem.courses" :key="course.name" class="hover:bg-(--surface-input) transition-colors">
                      <td class="px-5 py-3.5">
                        <div class="flex items-center gap-2">
                          <BookOpen :size="14" class="text-placeholder shrink-0" />
                          <span class="text-xs font-semibold text-label">{{ course.name }}</span>
                        </div>
                      </td>
                      <td class="px-5 py-3.5">
                        <span class="text-xs font-bold text-muted">{{ course.credit }}</span>
                      </td>
                      <td class="px-5 py-3.5">
                        <span :class="['text-xs font-bold', course.grade >= 5 ? 'text-(--color-success-text)' : course.grade >= 4 ? 'text-(--color-warning-text)' : 'text-(--color-danger-text)']">
                          {{ course.grade }}
                        </span>
                      </td>
                      <td class="px-5 py-3.5">
                        <div v-if="course.passed" class="flex items-center gap-1 text-[10px] font-bold text-(--color-success-text)">
                          <CheckCircle2 :size="14" /> Pass
                        </div>
                        <div v-else class="flex items-center gap-1 text-[10px] font-bold text-(--color-danger-text)">
                          <XCircle :size="14" /> Fail
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          <div v-else class="surface-card border border-card rounded-2xl p-8 text-center">
            <GraduationCap :size="36" class="text-placeholder mx-auto mb-3" />
            <p class="text-sm font-semibold text-muted">Chưa có dữ liệu lịch sử học tập</p>
            <p class="text-xs text-placeholder mt-1">Thông tin chi tiết sẽ được cập nhật từ hệ thống.</p>
          </div>

        </div>

        <!-- ── Right Sidebar ── -->
        <div class="space-y-6">

          <!-- ── Attendance History ── -->
          <div v-if="attendanceHistory.length" class="surface-card border border-card rounded-2xl p-5">
            <h4 class="text-xs font-semibold text-heading uppercase tracking-widest mb-5 flex items-center gap-2">
              <Clock :size="16" /> Chuyên cần theo kỳ
            </h4>
            <div class="space-y-4">
              <div v-for="att in attendanceHistory" :key="att.semester">
                <div class="flex items-center justify-between mb-1.5">
                  <span class="text-[10px] font-semibold text-muted">{{ att.semester }}</span>
                  <span :class="['text-[10px] font-bold', att.rate >= 85 ? 'text-(--color-success-text)' : att.rate >= 75 ? 'text-(--color-warning-text)' : 'text-(--color-danger-text)']">
                    {{ att.rate }}%
                  </span>
                </div>
                <div class="h-2 w-full bg-(--surface-input) rounded-full overflow-hidden">
                  <div
                    :style="{ width: `${att.rate}%` }"
                    :class="['h-full rounded-full transition-all', att.rate >= 85 ? 'bg-(--color-success-text)' : att.rate >= 75 ? 'bg-(--color-warning-text)' : 'bg-(--color-danger-text)']"
                  ></div>
                </div>
              </div>
            </div>
          </div>

          <!-- ── AI Risk Summary ── -->
          <div class="surface-card border border-card rounded-2xl p-5">
            <h4 class="text-xs font-semibold text-heading uppercase tracking-widest mb-4 flex items-center gap-2">
              <AlertTriangle :size="16" class="text-(--color-danger-text)" /> Phân tích AI
            </h4>
            <div class="flex items-start gap-3 p-4 bg-(--color-danger-bg) rounded-2xl border border-(--color-danger-text)/20 mb-4">
              <TrendingDown :size="16" class="text-(--color-danger-text) shrink-0 mt-0.5" />
              <div>
                <p class="text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest">Cảnh báo</p>
                <p class="text-[11px] text-body font-medium mt-1">Sinh viên có nguy cơ rớt môn. Cần can thiệp sớm.</p>
              </div>
            </div>
            <p class="text-[11px] text-body font-medium leading-relaxed">{{ student.reason }}</p>
          </div>

          <!-- ── Quick Stats ── -->
          <div class="grid grid-cols-2 gap-3">
            <div class="surface-card border border-card rounded-2xl p-4 text-center">
              <GraduationCap :size="20" class="text-(--color-info-text) mx-auto mb-2" />
              <p class="text-lg font-bold text-heading">{{ student.failCount || 0 }}</p>
              <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mt-1">Môn đã rớt</p>
            </div>
            <div class="surface-card border border-card rounded-2xl p-4 text-center">
              <Award :size="20" class="text-(--color-success-text) mx-auto mb-2" />
              <p class="text-lg font-bold text-heading">{{ student.grade }}</p>
              <p class="text-[9px] font-semibold text-muted uppercase tracking-widest mt-1">GPA hiện tại</p>
            </div>
          </div>

        </div>
      </div>

    </div>
  </PageContainer>

  <!-- ── Not Found ── -->
  <PageContainer v-else title="Không tìm thấy" subtitle="Sinh viên không tồn tại.">
    <template #actions>
      <button @click="goBack" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
        <ArrowLeft :size="18" /> Quay lại
      </button>
    </template>
  </PageContainer>
</template>
