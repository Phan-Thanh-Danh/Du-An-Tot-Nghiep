<script setup>
import { computed, ref } from 'vue'
import {
  ArrowRight,
  BookOpenCheck,
  CheckCircle2,
  Clock,
  Flag,
  GitCompare,
  GraduationCap,
  History,
  Layers3,
  Lock,
  Map,
  PlayCircle,
  RefreshCcw,
  Rocket,
  Sparkles,
  Trophy,
} from 'lucide-vue-next'
import { mockStudentCurriculum, mockCurriculumVersionData } from '@/data/studentData.mock.js'

const viewMode = ref('card')
const showEarlyLearningNotification = ref(true)

const studentCurriculum = mockStudentCurriculum
const curriculumVersionData = mockCurriculumVersionData

const activeFilter = ref('all')
const selectedVersionId = ref(curriculumVersionData.availableVersions?.[0]?.programId || 'sd-2026')

const statusConfig = {
  completed: { label: 'Đã hoàn thành', className: 'status-completed', icon: CheckCircle2 },
  current: { label: 'Đang học', className: 'status-current', icon: PlayCircle },
  early_available: { label: 'Có thể học trước', className: 'status-early', icon: Rocket },
  early_completed: { label: 'Đã học trước', className: 'status-early-done', icon: Trophy },
  future_locked: { label: 'Chưa mở', className: 'status-locked', icon: Lock },
  changed_program: { label: 'Đổi chương trình', className: 'status-changed', icon: Flag },
}

const versionStatusConfig = {
  current_program: { label: 'Chương trình hiện tại', className: 'version-current' },
  old_version_completed: { label: 'Đã học ở version cũ', className: 'version-old' },
  replaced: { label: 'Đã được thay thế', className: 'version-replaced' },
  equivalent: { label: 'Có thể áp dụng', className: 'version-equivalent' },
  partial_equivalent: { label: 'Cần học bổ sung', className: 'version-partial' },
  history_only: { label: 'Chỉ lưu lịch sử', className: 'version-history' },
}

const applyStatusConfig = {
  applied: { label: 'Đã áp dụng vào chương trình hiện tại', className: 'version-equivalent' },
  pending_review: { label: 'Chờ xét tương đương', className: 'version-replaced' },
  requires_supplement: { label: 'Cần học bổ sung', className: 'version-partial' },
  history_only: { label: 'Chỉ lưu làm lịch sử tự học', className: 'version-history' },
}

const filterOptions = [
  { key: 'all', label: 'Tất cả' },
  { key: 'current', label: 'Đang học' },
  { key: 'completed', label: 'Đã hoàn thành' },
  { key: 'early_available', label: 'Có thể học trước' },
  { key: 'future_locked', label: 'Chưa mở' },
  { key: 'early_completed', label: 'Đã học trước' },
]

const allSubjects = computed(() =>
  studentCurriculum.semesters.flatMap((semester) =>
    semester.blocks.flatMap((block) =>
      block.subjects.map((item) => ({
        ...item,
        semesterIndex: semester.semesterIndex,
        semesterName: semester.semesterName,
        blockIndex: block.blockIndex,
        blockName: block.blockName,
      }))
    )
  )
)

const filteredSemesters = computed(() => {
  const isOldVersion = selectedVersionId.value && selectedVersionId.value.endsWith('-old')

  return studentCurriculum.semesters
    .map((semester) => ({
      ...semester,
      blocks: semester.blocks
        .map((block) => ({
          ...block,
          subjects: block.subjects
            .filter((item) => activeFilter.value === 'all' || item.status === activeFilter.value)
            .map((item) => {
              if (isOldVersion) {
                const historyItem = curriculumVersionData.earlyLearningHistory.find(
                  (h) => h.newSubjectCode === item.subjectCode
                )
                if (historyItem) {
                  return {
                    ...item,
                    subjectCode: historyItem.oldSubjectCode,
                    subjectName: historyItem.oldSubjectName,
                    newSubjectCode: historyItem.newSubjectCode,
                    newSubjectName: historyItem.newSubjectName,
                    versionStatus: 'current_program', // Hide equivalence notice since we are displaying the old version natively
                  }
                }
              }
              return item
            }),
        }))
        .filter((block) => block.subjects.length),
    }))
    .filter((semester) => semester.blocks.length)
})

const progressPercent = computed(() =>
  Math.round((studentCurriculum.completedCredits / studentCurriculum.totalCredits) * 100)
)

const summaryCards = computed(() => [
  { label: 'Tín chỉ hoàn thành', value: `${studentCurriculum.completedCredits}/${studentCurriculum.totalCredits}`, hint: `${progressPercent.value}% toàn chương trình`, icon: Trophy },
  { label: 'Môn đã hoàn thành', value: `${studentCurriculum.completedSubjects}/${studentCurriculum.totalSubjects}`, hint: 'Theo khung mock hiện tại', icon: BookOpenCheck },
  { label: 'Kỳ hiện tại', value: `Kỳ ${studentCurriculum.currentSemesterIndex}`, hint: `Block ${studentCurriculum.currentBlockIndex}`, icon: Layers3 },
  { label: 'Môn học trước', value: earlySuggestions.value.length, hint: 'Đang mở cho tự học', icon: Rocket },
])

const earlySuggestions = computed(() =>
  allSubjects.value.filter((item) => item.status === 'early_available').slice(0, 4)
)

const selectedVersion = computed(() =>
  curriculumVersionData.availableVersions.find((version) => version.programId === selectedVersionId.value)
  || curriculumVersionData.availableVersions[0]
)

const versionChangeCards = computed(() => [
  { label: 'Môn thay thế', value: curriculumVersionData.versionChanges.replacedSubjects, icon: RefreshCcw },
  { label: 'Môn đổi nội dung', value: curriculumVersionData.versionChanges.changedSubjects, icon: GitCompare },
  { label: 'Môn mới thêm', value: curriculumVersionData.versionChanges.addedSubjects, icon: Sparkles },
  { label: 'Chỉ lưu lịch sử', value: curriculumVersionData.versionChanges.historyOnlySubjects, icon: History },
])

function actionLabel(item) {
  if (item.status === 'current') return 'Vào học'
  if (item.status === 'completed' || item.status === 'early_completed') return 'Xem kết quả'
  if (item.status === 'early_available') return 'Học trước'
  return 'Chưa mở'
}

function isActionDisabled(item) {
  return item.status === 'future_locked' || item.status === 'changed_program'
}

function versionLabel(status) {
  return versionStatusConfig[status]?.label || 'Không xác định'
}

function canApplyEarlyResult(item) {
  return ['equivalent', 'partial_equivalent'].includes(item.versionStatus)
}

const isOldVersion = computed(() => selectedVersionId.value && selectedVersionId.value.endsWith('-old'))
</script>

<template>
  <div class="curriculum-page">
    <section class="curriculum-hero">
      <div class="hero-copy">
        <span class="eyebrow">
          <Map :size="14" />
          Khung chương trình
        </span>
        <h1>{{ studentCurriculum.majorName }}</h1>
        <p>
          {{ studentCurriculum.facultyName }} · {{ studentCurriculum.programCode }} ·
          {{ studentCurriculum.programVersion }}
        </p>
        <div class="hero-note">
          <Sparkles :size="15" />
          Bạn có thể xem trước lộ trình toàn khóa và học sớm các môn được mở.
        </div>
      </div>

      <div class="program-progress" aria-label="Tiến độ toàn chương trình">
        <strong>{{ progressPercent }}%</strong>
        <span>{{ studentCurriculum.completedCredits }}/{{ studentCurriculum.totalCredits }} tín chỉ</span>
        <div class="progress-track">
          <div class="progress-fill" :style="{ width: `${progressPercent}%` }" />
        </div>
      </div>
    </section>

    <!-- Banner Thông báo Học trước & Bảo lưu kết quả -->
    <div v-if="showEarlyLearningNotification" class="early-notification-banner glass-card">
      <div class="banner-icon">
        <Rocket :size="22" class="text-[var(--text-link)] animate-bounce" />
      </div>
      <div class="banner-body">
        <h3>Nhắc nhở học trước & Bảo lưu kết quả</h3>
        <p>
          Hệ thống ghi nhận bạn đang có kết quả tự học trước của môn <strong>Kiểm thử phần mềm (TEST101)</strong> đạt <strong>8.4/10</strong> (hoàn thành 64% chương trình).
          Kết quả này sẽ tự động được bảo lưu và áp dụng chính thức khi bạn bước vào Kỳ 2 - Block 2!
        </p>
        <div class="supplement-info">
          <span class="supplement-badge">Cập nhật môn học</span>
          <span>Đối với môn <strong>Lập trình C# nâng cấp (NET102)</strong>, kết quả học trước môn tương đương cũ <strong>NET101 (8.5/10)</strong> đã được áp dụng và bảo lưu thành công, bạn chỉ cần học bổ sung 30% nội dung cập nhật mới.</span>
        </div>
      </div>
      <button type="button" class="banner-close-btn" @click="showEarlyLearningNotification = false">Đóng</button>
    </div>

    <section class="version-panel" aria-label="Version chương trình">
      <div class="version-heading">
        <div>
          <span class="eyebrow small">
            <GitCompare :size="13" />
            Version chương trình
          </span>
          <h2>{{ curriculumVersionData.currentProgram.majorName }} - {{ curriculumVersionData.currentProgram.versionName }}</h2>
          <p>
            Áp dụng cho khóa {{ curriculumVersionData.currentProgram.studentCohort }} ·
            hiệu lực từ {{ curriculumVersionData.currentProgram.effectiveFromYear }}
          </p>
        </div>

        <label class="version-switcher">
          <span>Xem version</span>
          <select v-model="selectedVersionId">
            <option
              v-for="version in curriculumVersionData.availableVersions"
              :key="version.programId"
              :value="version.programId"
            >
              {{ version.versionName }}{{ version.hasEarlyLearningHistory ? ' - có lịch sử học trước' : '' }}
            </option>
          </select>
        </label>
      </div>

      <div class="version-context">
        <div>
          <strong>{{ selectedVersion.versionName }}</strong>
          <p v-if="selectedVersion.isCurrent">Đây là chương trình hiện tại của sinh viên.</p>
          <p v-else>Kết quả học trước ở version cũ vẫn được giữ lại để xét tương đương.</p>
        </div>
        <span class="version-pill" :class="{ current: selectedVersion.isCurrent }">
          {{ selectedVersion.isCurrent ? 'Chương trình hiện tại' : 'Version cũ' }}
        </span>
      </div>

      <div class="change-grid" aria-label="Thay đổi so với version trước">
        <article v-for="item in versionChangeCards" :key="item.label" class="change-card">
          <component :is="item.icon" :size="16" />
          <div>
            <strong>{{ item.value }}</strong>
            <span>{{ item.label }}</span>
          </div>
        </article>
      </div>
    </section>

    <section class="summary-grid" aria-label="Tổng quan chương trình">
      <article v-for="card in summaryCards" :key="card.label" class="summary-card">
        <component :is="card.icon" :size="18" />
        <div>
          <span>{{ card.label }}</span>
          <strong>{{ card.value }}</strong>
          <p>{{ card.hint }}</p>
        </div>
      </article>
    </section>

    <section class="early-section">
      <div class="section-heading">
        <div>
          <span class="eyebrow small">
            <Rocket :size="13" />
            Tự học sớm
          </span>
          <h2>Gợi ý học trước</h2>
        </div>
        <p>Kết quả học trước sẽ được lưu lại khi hệ thống hỗ trợ áp dụng chính thức.</p>
      </div>

      <div class="early-grid">
        <article v-for="item in earlySuggestions" :key="item.id" class="early-card">
          <div>
            <span class="subject-code">{{ item.subjectCode }}</span>
            <h3>{{ item.subjectName }}</h3>
            <p>{{ item.semesterName }} · {{ item.blockName }} · {{ item.credits }} tín chỉ</p>
          </div>
          <router-link class="early-cta" :to="'/student/courses/' + item.subjectCode">
            Bắt đầu học trước
            <ArrowRight :size="13" />
          </router-link>
        </article>
      </div>
    </section>


    <section class="filter-strip" aria-label="Bộ lọc trạng thái">
      <div class="filter-chips">
        <button
          v-for="filter in filterOptions"
          :key="filter.key"
          type="button"
          :class="['filter-chip', { active: activeFilter === filter.key }]"
          @click="activeFilter = filter.key"
        >
          {{ filter.label }}
        </button>
      </div>

      <!-- Switcher chế độ xem Bảng/Thẻ -->
      <div class="view-mode-switcher">
        <button
          type="button"
          :class="['switcher-btn', { active: viewMode === 'card' }]"
          @click="viewMode = 'card'"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
          </svg>
          Dạng thẻ
        </button>
        <button
          type="button"
          :class="['switcher-btn', { active: viewMode === 'table' }]"
          @click="viewMode = 'table'"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
          Dạng bảng
        </button>
      </div>
    </section>

    <section v-if="filteredSemesters.length && viewMode === 'card'" class="semester-timeline" aria-label="Lộ trình học tập">
      <article
        v-for="semester in filteredSemesters"
        :key="semester.semesterIndex"
        :class="['semester-card', { 'current-semester': semester.semesterIndex === studentCurriculum.currentSemesterIndex }]"
      >
        <header class="semester-header">
          <div>
            <h2>{{ semester.semesterName }}</h2>
          </div>
          <span
            class="semester-state"
            :class="{ active: semester.semesterIndex === studentCurriculum.currentSemesterIndex }"
          >
            <span v-if="semester.semesterIndex === studentCurriculum.currentSemesterIndex" class="pulse-dot" />
            {{ semester.semesterIndex === studentCurriculum.currentSemesterIndex ? 'Kỳ hiện tại' : 'Lộ trình' }}
          </span>
        </header>

        <div class="block-grid">
          <section v-for="block in semester.blocks" :key="block.blockIndex" class="block-card">
            <div class="block-header">
              <div>
                <span>{{ block.blockName }}</span>
                <strong>{{ block.subjects.length }} môn</strong>
              </div>
              <div class="block-header-right">
                <span 
                  v-if="semester.semesterIndex === studentCurriculum.currentSemesterIndex && block.blockIndex === studentCurriculum.currentBlockIndex" 
                  class="block-current-badge"
                >
                  <span class="pulse-dot" />
                  Block hiện tại
                </span>
                <Clock :size="15" />
              </div>
            </div>

            <div class="subject-list">
              <article 
                v-for="item in block.subjects" 
                :key="item.id" 
                :class="['subject-card', { 'old-version-card': isOldVersion }]"
              >
                <!-- CARD PHIÊN BẢN CŨ (CHỈ XEM THAY ĐỔI) -->
                <template v-if="isOldVersion">
                  <div class="subject-main">
                    <span class="subject-code old">{{ item.subjectCode }}</span>
                    <h3>{{ item.subjectName }}</h3>
                    <p>{{ semester.semesterName }} · {{ block.blockName }} · {{ item.credits }} tín chỉ</p>
                  </div>
                  
                  <div class="compare-transition">
                    <div class="compare-flow">
                      <div class="flow-node">
                        <span class="node-label">Version cũ</span>
                        <strong class="font-mono text-heading">{{ item.subjectCode }}</strong>
                      </div>
                      <ArrowRight :size="14" class="flow-arrow text-link" />
                      <div class="flow-node highlight">
                        <span class="node-label">Chương trình mới</span>
                        <strong class="font-mono text-link">{{ item.newSubjectCode || item.subjectCode }}</strong>
                      </div>
                    </div>
                    <p class="compare-text">
                      Môn học này tương đương với <strong>{{ item.newSubjectName || item.subjectName }}</strong> ({{ item.newSubjectCode || item.subjectCode }}) trong chương trình mới.
                    </p>
                  </div>
                </template>

                <!-- CARD PHIÊN BẢN MỚI (XEM BÌNH THƯỜNG) -->
                <template v-else>
                  <div class="subject-main">
                    <span class="subject-code">{{ item.subjectCode }}</span>
                    <h3>{{ item.subjectName }}</h3>
                    <p>{{ semester.semesterName }} · {{ block.blockName }} · {{ item.credits }} tín chỉ</p>
                  </div>

                  <div class="subject-meta">
                    <span class="status-badge" :class="statusConfig[item.status]?.className">
                      <component :is="statusConfig[item.status]?.icon" :size="12" />
                      {{ statusConfig[item.status]?.label }}
                    </span>
                    <span class="version-badge" :class="versionStatusConfig[item.versionStatus]?.className">
                      {{ versionLabel(item.versionStatus) }}
                    </span>
                    <span v-if="item.quizScore" class="score-chip">Quiz {{ item.quizScore }}/10</span>
                    <span v-if="item.score" class="score-chip">Điểm {{ item.score }}/10</span>
                    <span v-if="item.earlyScore" class="score-chip">Học trước {{ item.earlyScore }}/10</span>
                    <span v-if="item.earlyScoreFromOldVersion" class="score-chip">Version cũ {{ item.earlyScoreFromOldVersion }}/10</span>
                  </div>

                  <div class="subject-progress">
                    <div class="progress-copy">
                      <span>Tiến độ</span>
                      <strong>{{ item.earlyProgressPercent ?? item.progressPercent }}%</strong>
                    </div>
                    <div class="mini-track">
                      <div class="mini-fill" :style="{ width: `${item.earlyProgressPercent ?? item.progressPercent}%` }" />
                    </div>
                  </div>

                  <p v-if="item.status === 'early_available'" class="subject-note">
                    Bạn có thể học trước môn này. Kết quả sẽ được lưu khi backend hỗ trợ.
                  </p>
                  <p v-else-if="item.status === 'early_completed'" class="subject-note">
                    Đã học trước {{ item.earlyCompletedAt }} · tiến độ {{ item.earlyProgressPercent }}%.
                  </p>
                  <p v-else-if="item.status === 'future_locked'" class="subject-note muted">
                    Mở ở {{ semester.semesterName }} - {{ block.blockName }}.
                  </p>
                  <p v-else-if="item.status === 'changed_program'" class="subject-note muted">
                    {{ item.note }}
                  </p>
                  <p
                    v-if="item.versionStatus && item.versionStatus !== 'current_program'"
                    class="equivalence-notice compact"
                    :class="{ partial: item.versionStatus === 'partial_equivalent', history: item.versionStatus === 'history_only' }"
                  >
                    <GitCompare :size="13" />
                    <span v-if="item.versionStatus === 'partial_equivalent'">
                      Môn này đã thay đổi ở chương trình mới. Kết quả {{ item.replacedSubjectCode }} được giữ lại;
                      {{ canApplyEarlyResult(item) ? 'có thể xét áp dụng điểm cũ' : 'chỉ lưu lịch sử' }}.
                    </span>
                    <span v-else-if="item.versionStatus === 'history_only'">Chỉ lưu làm lịch sử tự học.</span>
                    <span v-else>{{ versionLabel(item.versionStatus) }}.</span>
                  </p>

                  <router-link
                    v-if="!isActionDisabled(item)"
                    class="subject-action"
                    :to="'/student/courses/' + item.subjectCode"
                  >
                    {{ actionLabel(item) }}
                    <ArrowRight :size="13" />
                  </router-link>
                  <button v-else class="subject-action disabled" type="button" disabled>
                    {{ actionLabel(item) }}
                  </button>
                </template>
              </article>
            </div>
          </section>
        </div>
      </article>
    </section>

    <!-- DẠNG BẢNG (TABLE VIEW) -->
    <section v-else-if="filteredSemesters.length && viewMode === 'table'" class="curriculum-table-section glass-card">
      <div class="table-container">
        <table class="curriculum-table">
          <thead>
            <tr v-if="isOldVersion">
              <th>Học kỳ & Block</th>
              <th>Mã môn cũ</th>
              <th>Tên môn học cũ</th>
              <th>Số tín chỉ</th>
              <th class="text-link">Mã môn mới tương đương</th>
              <th>Tên môn mới tương đương</th>
            </tr>
            <tr v-else>
              <th>Học kỳ & Block</th>
              <th>Mã môn</th>
              <th>Tên môn học</th>
              <th>Số tín chỉ</th>
              <th>Trạng thái học</th>
              <th>Kết quả / Điểm</th>
              <th>Tiến độ</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="semester in filteredSemesters" :key="semester.semesterIndex">
              <template v-for="block in semester.blocks" :key="block.blockIndex">
                <tr v-for="(item, idx) in block.subjects" :key="item.id" class="table-row">
                  <td
                    v-if="idx === 0"
                    :rowspan="block.subjects.length"
                    class="semester-block-cell"
                  >
                    <div class="sem-block-badge">
                      Kỳ {{ semester.semesterIndex }} - {{ block.blockName }}
                    </div>
                  </td>
                  
                  <template v-if="isOldVersion">
                    <td class="code-cell font-mono font-semibold text-placeholder">{{ item.subjectCode }}</td>
                    <td class="name-cell font-semibold text-placeholder">{{ item.subjectName }}</td>
                    <td class="credits-cell">{{ item.credits }} tín chỉ</td>
                    <td class="code-cell font-mono font-bold text-link">{{ item.newSubjectCode || item.subjectCode }}</td>
                    <td class="name-cell font-bold text-heading">{{ item.newSubjectName || item.subjectName }}</td>
                  </template>
                  
                  <template v-else>
                    <td class="code-cell font-mono font-semibold text-link">{{ item.subjectCode }}</td>
                    <td class="name-cell font-semibold text-heading">{{ item.subjectName }}</td>
                    <td class="credits-cell">{{ item.credits }} tín chỉ</td>
                    <td class="status-cell">
                      <span class="status-badge" :class="statusConfig[item.status]?.className">
                        <component :is="statusConfig[item.status]?.icon" :size="12" />
                        {{ statusConfig[item.status]?.label }}
                      </span>
                    </td>
                    <td class="score-cell">
                      <div class="score-container">
                        <span v-if="item.score" class="score-text">Chính thức: {{ item.score }}</span>
                        <span v-else-if="item.earlyScore" class="early-score-text">Học trước: {{ item.earlyScore }}</span>
                        <span v-else-if="item.earlyScoreFromOldVersion" class="early-score-text text-purple-600">Bảo lưu: {{ item.earlyScoreFromOldVersion }}</span>
                        <span v-else class="text-placeholder">--</span>
                      </div>
                    </td>
                    <td class="progress-cell">
                      <div class="table-progress-bar">
                        <span class="progress-num">{{ item.earlyProgressPercent ?? item.progressPercent }}%</span>
                        <div class="track">
                          <div class="fill" :style="{ width: `${item.earlyProgressPercent ?? item.progressPercent}%` }" />
                        </div>
                      </div>
                    </td>
                    <td class="action-cell">
                      <router-link
                        v-if="!isActionDisabled(item)"
                        class="table-action-btn"
                        :to="'/student/courses/' + item.subjectCode"
                      >
                        {{ actionLabel(item) }}
                      </router-link>
                      <button v-else class="table-action-btn disabled" type="button" disabled>
                        {{ actionLabel(item) }}
                      </button>
                    </td>
                  </template>
                </tr>
              </template>
            </template>
          </tbody>
        </table>
      </div>
    </section>

    <section v-else class="empty-state">
      <GraduationCap :size="34" />
      <h2>Không có môn phù hợp</h2>
      <p>Không tìm thấy môn học phù hợp với bộ lọc hiện tại.</p>
    </section>
  </div>
</template>

<style scoped>
.curriculum-page {
  display: grid;
  gap: 1rem;
}

.curriculum-hero,
.early-section,
.history-section,
.semester-card,
.summary-card,
.version-panel,
.filter-strip,
.empty-state {
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
}

.curriculum-hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(16rem, 0.34fr);
  gap: 1rem;
  overflow: hidden;
  border-radius: 24px;
  padding: 1rem;
  background: var(--surface-card);
}

.hero-copy {
  display: grid;
  gap: 0.55rem;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.eyebrow.small {
  font-size: 0.66rem;
}

.hero-copy h1,
.section-heading h2,
.semester-header h2,
.subject-card h3,
.early-card h3 {
  margin: 0;
  color: var(--text-heading);
}

.hero-copy h1 {
  font-size: 1.55rem;
  font-weight: 900;
  line-height: 1.12;
}

.hero-copy p,
.section-heading p,
.early-card p,
.subject-note {
  margin: 0;
  color: var(--text-label);
  font-size: 0.82rem;
  line-height: 1.45;
}

.subject-main p {
  margin: 0;
  color: var(--text-label);
  font-size: 0.72rem;
  line-height: 1.4;
}

.hero-note {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  width: fit-content;
  border-radius: 14px;
  background: var(--color-info-bg);
  color: var(--color-info-text);
  padding: 0.55rem 0.7rem;
  font-size: 0.78rem;
  font-weight: 800;
}

.program-progress {
  display: grid;
  align-content: center;
  gap: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 20px;
  background: var(--surface-input);
  padding: 0.9rem;
}

.program-progress strong {
  color: var(--text-heading);
  font-size: 1.7rem;
  font-weight: 950;
  line-height: 1;
}

.program-progress span {
  color: var(--text-label);
  font-size: 0.8rem;
  font-weight: 780;
}

.progress-track,
.mini-track {
  overflow: hidden;
  border-radius: 999px;
  background: var(--surface-input);
}

.progress-track {
  height: 0.55rem;
  border: 1px solid var(--border-card);
}

.progress-fill,
.mini-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--accent-primary), var(--accent-cyan));
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.version-panel,
.history-section {
  display: grid;
  gap: 0.8rem;
  border-radius: 22px;
  padding: 0.9rem;
}

.version-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.version-heading h2,
.history-main h3 {
  margin: 0.35rem 0 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.version-heading p,
.version-context p,
.history-main p {
  margin: 0.25rem 0 0;
  color: var(--text-label);
  font-size: 0.78rem;
}

.version-switcher {
  display: grid;
  gap: 0.35rem;
  min-width: min(16rem, 100%);
}

.version-switcher span {
  color: var(--text-placeholder);
  font-size: 0.68rem;
  font-weight: 850;
  text-transform: uppercase;
}

.version-switcher select {
  min-height: 2.35rem;
  border: 1px solid var(--border-input);
  border-radius: 14px;
  background: var(--surface-input);
  color: var(--text-label);
  outline: 0;
  padding: 0 0.7rem;
  font-size: 0.8rem;
  font-weight: 760;
}

.version-context {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-input);
  padding: 0.7rem;
}

.version-context strong {
  color: var(--text-heading);
  font-size: 0.9rem;
}

.version-pill {
  display: inline-flex;
  flex-shrink: 0;
  border-radius: 999px;
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 0.25rem 0.65rem;
  font-size: 0.68rem;
  font-weight: 850;
}

.version-pill.current {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.change-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.6rem;
}

.change-card {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  border: 1px solid var(--border-card);
  border-radius: 15px;
  background: var(--surface-input);
  padding: 0.65rem;
}

.change-card svg {
  color: var(--text-link);
}

.change-card strong {
  display: block;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.change-card span {
  display: block;
  color: var(--text-label);
  font-size: 0.7rem;
  font-weight: 800;
}

.summary-card {
  display: flex;
  gap: 0.7rem;
  border-radius: 18px;
  padding: 0.8rem;
}

.summary-card svg {
  flex-shrink: 0;
  color: var(--text-link);
}

.summary-card span,
.progress-copy span,
.semester-header span,
.block-header span {
  color: var(--text-placeholder);
  font-size: 0.68rem;
  font-weight: 850;
  text-transform: uppercase;
}

.summary-card strong {
  display: block;
  margin-top: 0.2rem;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
}

.summary-card p {
  margin: 0.15rem 0 0;
  color: var(--text-label);
  font-size: 0.72rem;
}

.early-section {
  display: grid;
  gap: 0.8rem;
  border-radius: 22px;
  padding: 0.9rem;
}

.history-list {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.65rem;
}

.history-card {
  display: grid;
  gap: 0.6rem;
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-input);
  padding: 0.75rem;
}

.history-transfer {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--text-placeholder);
  font-size: 0.72rem;
  font-weight: 850;
}

.history-transfer strong {
  color: var(--text-link);
}

.section-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
}

.section-heading h2 {
  margin-top: 0.35rem;
  font-size: 1rem;
  font-weight: 900;
}

.early-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.65rem;
}

.early-card {
  display: grid;
  gap: 0.65rem;
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-input);
  padding: 0.75rem;
}

.subject-code {
  color: var(--text-link);
  font-size: 0.65rem;
  font-weight: 950;
}

.early-card h3,
.subject-card h3 {
  margin-top: 0.1rem;
  font-size: 0.75rem;
  font-weight: 800;
  line-height: 1.25;
}

.early-cta,
.subject-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
  min-height: 1.6rem;
  border-radius: 8px;
  background: var(--accent-primary);
  color: var(--text-inverse);
  font-size: 0.68rem;
  font-weight: 800;
  text-decoration: none;
}

.filter-strip {
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  border-radius: 18px;
  padding: 0.65rem;
}

.filter-chip {
  min-height: 2rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-label);
  cursor: pointer;
  padding: 0 0.75rem;
  font-size: 0.74rem;
  font-weight: 850;
}

.filter-chip.active {
  border-color: var(--border-input-focus);
  background: var(--color-info-bg);
  color: var(--color-info-text);
}

.semester-timeline {
  display: grid;
  gap: 0.9rem;
}

.semester-card {
  border-radius: 22px;
  overflow: hidden;
}

.semester-card.current-semester {
  border-color: var(--accent-primary);
  box-shadow: 0 0 0 1.5px var(--accent-primary), 0 8px 30px color-mix(in srgb, var(--accent-primary) 12%, transparent), var(--lg-shadow-sm);
}

.semester-card.current-semester .semester-header {
  border-bottom-color: color-mix(in srgb, var(--accent-primary) 30%, var(--border-card));
  background: color-mix(in srgb, var(--accent-primary) 4%, transparent);
}

.semester-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  border-bottom: 1px solid var(--border-card);
  padding: 0.8rem 0.9rem;
}

.semester-header h2 {
  margin-top: 0.2rem;
  font-size: 1.05rem;
  font-weight: 900;
}

.semester-state {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-placeholder);
  padding: 0.25rem 0.65rem;
  font-size: 0.7rem;
  font-weight: 850;
}

.pulse-dot {
  display: inline-block;
  width: 6px;
  height: 6px;
  background-color: var(--color-success-text);
  border-radius: 50%;
  margin-right: 6px;
  position: relative;
}

.pulse-dot::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background-color: var(--color-success-text);
  animation: dot-pulse 1.8s infinite ease-in-out;
}

@keyframes dot-pulse {
  0% {
    transform: scale(1);
    opacity: 0.85;
  }
  100% {
    transform: scale(2.8);
    opacity: 0;
  }
}

.semester-state.active {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.block-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
  padding: 0.85rem;
}

.block-card {
  border: 1px solid var(--border-card);
  border-radius: 18px;
  background: color-mix(in srgb, var(--surface-card) 74%, var(--surface-input));
  overflow: hidden;
}

.block-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.6rem;
  border-bottom: 1px solid var(--border-card);
  padding: 0.65rem 0.75rem;
  color: var(--text-link);
}

.block-header strong {
  display: block;
  margin-top: 0.1rem;
  color: var(--text-heading);
  font-size: 0.86rem;
}

.subject-list {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.5rem;
  padding: 0.5rem;
}

.subject-card {
  display: grid;
  gap: 0.35rem;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-card);
  padding: 0.45rem 0.55rem;
}

.subject-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.status-badge,
.version-badge,
.score-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  min-height: 1.3rem;
  border-radius: 999px;
  padding: 0.15rem 0.45rem;
  font-size: 0.62rem;
  font-weight: 850;
  line-height: 1;
}

.version-badge {
  border: 1px solid var(--border-card);
}

.score-chip {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.status-completed { color: var(--color-success-text); background: var(--color-success-bg); }
.status-current { color: var(--color-info-text); background: var(--color-info-bg); }
.status-early,
.status-early-done { color: var(--accent-violet); background: var(--accent-violet-soft); }
.status-locked { color: var(--text-placeholder); background: var(--surface-input); }
.status-changed { color: var(--color-warning-text); background: var(--color-warning-bg); }

.version-current { color: var(--color-info-text); background: var(--color-info-bg); }
.version-old { color: var(--accent-violet); background: var(--accent-violet-soft); }
.version-replaced { color: var(--color-warning-text); background: var(--color-warning-bg); }
.version-equivalent { color: var(--color-success-text); background: var(--color-success-bg); }
.version-partial { color: var(--color-warning-text); background: var(--color-warning-bg); }
.version-history { color: var(--text-placeholder); background: var(--surface-input); }

.subject-progress {
  display: grid;
  gap: 0.3rem;
}

.progress-copy {
  display: flex;
  justify-content: space-between;
  gap: 0.8rem;
}

.progress-copy strong {
  color: var(--text-heading);
  font-size: 0.72rem;
}

.mini-track {
  height: 0.42rem;
}

.subject-note {
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.45rem 0.55rem;
  font-size: 0.72rem;
}

.subject-note.muted {
  color: var(--text-placeholder);
}

.equivalence-notice {
  display: flex;
  align-items: flex-start;
  gap: 0.45rem;
  margin: 0;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  background: var(--color-info-bg);
  color: var(--color-info-text);
  padding: 0.55rem 0.65rem;
  font-size: 0.74rem;
  font-weight: 760;
  line-height: 1.45;
}

.equivalence-notice svg {
  flex-shrink: 0;
  margin-top: 0.05rem;
}

.equivalence-notice.partial {
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
}

.equivalence-notice.history {
  background: var(--surface-card);
  color: var(--text-label);
}

.equivalence-notice.compact {
  padding: 0.45rem 0.55rem;
  font-size: 0.7rem;
}

.subject-action.disabled {
  border: 0;
  background: var(--surface-input);
  color: var(--text-placeholder);
  cursor: not-allowed;
}

.empty-state {
  display: grid;
  place-items: center;
  min-height: 14rem;
  border-radius: 22px;
  padding: 1rem;
  text-align: center;
}

.empty-state h2 {
  margin: 0.5rem 0 0;
  color: var(--text-heading);
  font-size: 1rem;
}

.empty-state p {
  margin: 0.25rem 0 0;
  color: var(--text-label);
  font-size: 0.82rem;
}

@media (max-width: 1180px) {
  .summary-grid,
  .early-grid,
  .change-grid,
  .history-list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .subject-list {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 920px) {
  .curriculum-hero {
    grid-template-columns: 1fr;
  }

  .subject-list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .section-heading {
    align-items: flex-start;
    flex-direction: column;
  }

  .version-heading,
  .version-context {
    align-items: flex-start;
    flex-direction: column;
  }
}

@media (max-width: 640px) {
  .summary-grid,
  .early-grid,
  .change-grid,
  .history-list,
  .subject-list {
    grid-template-columns: 1fr;
  }

  .curriculum-hero,
  .early-section {
    border-radius: 18px;
  }
}

/* Styles cho Banner Thông báo Học trước */
.early-notification-banner {
  display: flex;
  align-items: flex-start;
  gap: 1.25rem;
  padding: 1.25rem;
  border-radius: 20px;
  border: 1px solid var(--accent-primary-soft);
  background: linear-gradient(135deg, var(--color-info-bg), var(--surface-card));
  box-shadow: 0 10px 30px color-mix(in srgb, var(--text-link) 5%, transparent);
}

.banner-icon {
  flex-shrink: 0;
  display: grid;
  place-items: center;
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 14px;
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.banner-body {
  flex: 1;
}

.banner-body h3 {
  margin: 0 0 0.25rem;
  font-size: 0.9rem;
  font-weight: 850;
  color: var(--text-heading);
}

.banner-body p {
  margin: 0;
  font-size: 0.8rem;
  color: var(--text-body);
  line-height: 1.5;
}

.supplement-info {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  margin-top: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--border-card);
  font-size: 0.75rem;
  color: var(--text-label);
}

.supplement-badge {
  flex-shrink: 0;
  font-size: 0.625rem;
  font-weight: 900;
  background: var(--accent-violet-soft);
  color: var(--accent-violet);
  padding: 0.1rem 0.4rem;
  border-radius: 6px;
  text-transform: uppercase;
}

.banner-close-btn {
  border: 0;
  background: transparent;
  color: var(--text-placeholder);
  font-size: 0.75rem;
  font-weight: 850;
  cursor: pointer;
}

.banner-close-btn:hover {
  color: var(--text-heading);
}

/* Styles cho View Mode Switcher */
.filter-strip {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
}

.view-mode-switcher {
  display: flex;
  border: 1px solid var(--border-card);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.2rem;
  gap: 0.15rem;
}

.switcher-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  border: 0;
  border-radius: 9px;
  background: transparent;
  color: var(--text-label);
  padding: 0.35rem 0.65rem;
  font-size: 0.72rem;
  font-weight: 800;
  cursor: pointer;
  transition: all 0.15s ease;
}

.switcher-btn:hover {
  color: var(--text-heading);
}

.switcher-btn.active {
  background: var(--surface-card);
  color: var(--text-link);
  box-shadow: var(--lg-shadow-sm);
}

/* Styles cho Dạng bảng (Table View) */
.curriculum-table-section {
  padding: 1rem;
}

.table-container {
  overflow-x: auto;
  width: 100%;
}

.curriculum-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.curriculum-table th {
  border-bottom: 1.5px solid var(--border-card);
  padding: 0.75rem 1rem;
  color: var(--text-placeholder);
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.curriculum-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--border-card);
  font-size: 0.8rem;
  vertical-align: middle;
}

.semester-block-cell {
  background: var(--surface-input);
  font-weight: 900;
  border-right: 1px solid var(--border-card);
  vertical-align: middle !important;
}

.sem-block-badge {
  font-size: 0.75rem;
  color: var(--text-heading);
  font-weight: 850;
  line-height: 1.25;
}

.code-cell {
  font-size: 0.8rem;
}

.name-cell {
  font-size: 0.825rem;
}

.credits-cell {
  color: var(--text-label);
}

.score-container {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  font-size: 0.75rem;
}

.score-text {
  font-weight: 800;
  color: var(--color-success-text);
}

.early-score-text {
  font-weight: 850;
  color: var(--text-link);
}

.table-progress-bar {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  width: 100px;
}

.progress-num {
  font-size: 0.7rem;
  font-weight: 800;
  color: var(--text-heading);
}

.table-progress-bar .track {
  height: 0.35rem;
  background: var(--surface-input);
  border-radius: 99px;
  overflow: hidden;
}

.table-progress-bar .fill {
  height: 100%;
  background: var(--text-link);
  border-radius: inherit;
}

.table-action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0.4rem 0.75rem;
  font-size: 0.72rem;
  font-weight: 850;
  cursor: pointer;
  text-decoration: none;
  transition: all 0.15s ease;
}

.table-action-btn:hover:not(.disabled) {
  background: var(--surface-card-strong);
  color: var(--text-heading);
  border-color: var(--border-input-focus);
}

.table-action-btn.disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .curriculum-table th,
  .curriculum-table td {
    padding: 0.5rem;
  }
  .semester-block-cell {
    display: none;
  }
}

/* Styling for old version comparison view */
.old-version-card {
  border-style: dashed !important;
  border-color: var(--border-input-focus) !important;
  background: var(--surface-sidebar) !important;
  opacity: 0.95;
}

.subject-code.old {
  color: var(--text-placeholder);
  background: var(--surface-input);
}

.compare-transition {
  margin-top: 0.75rem;
  padding: 0.75rem;
  border-radius: 12px;
  background: var(--surface-input);
  border: 1px solid var(--border-card);
}

.compare-flow {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.flow-node {
  display: flex;
  flex-direction: column;
  background: var(--surface-card);
  padding: 0.35rem 0.6rem;
  border-radius: 8px;
  border: 1px solid var(--border-card);
  min-width: 5.5rem;
}

.flow-node.highlight {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 8px rgba(37, 99, 235, 0.08);
}

.node-label {
  font-size: 0.6rem;
  color: var(--text-placeholder);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.flow-arrow {
  flex-shrink: 0;
  animation: pulse-arrow 2s infinite ease-in-out;
}

.compare-text {
  font-size: 0.72rem;
  color: var(--text-label);
  line-height: 1.4;
  margin: 0;
}

@keyframes pulse-arrow {
  0%, 100% { transform: translateX(0); opacity: 0.6; }
  50% { transform: translateX(3px); opacity: 1; }
}

.block-header-right {
  display: flex;
  align-items: center;
  gap: 0.55rem;
}

.block-current-badge {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  background: var(--color-success-bg);
  color: var(--color-success-text);
  padding: 0.18rem 0.55rem;
  font-size: 0.62rem;
  font-weight: 850;
  line-height: 1;
}
</style>
