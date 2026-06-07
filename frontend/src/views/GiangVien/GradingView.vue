<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  AlertCircle,
  ArrowLeft,
  CheckCircle2,
  Clock,
  Download,
  Edit3,
  ExternalLink,
  FileBox,
  FileDigit,
  MessageSquare,
  Save,
  Search,
  Star,
  Users,
  X,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const popupStore = usePopupStore()

const submissions = ref([
  { id: 1, studentId: 'SV16001', name: 'Nguyễn Văn A', file: 'Asm1_NVA.zip', time: '18/05/2026 09:30', score: 8.5, comment: 'Tốt, giao diện sạch sẽ.', status: 'Graded' },
  { id: 2, studentId: 'SV16002', name: 'Trần Thị B', file: 'Asm1_Final_B.rar', time: '19/05/2026 14:15', score: null, comment: '', status: 'Pending' },
  { id: 3, studentId: 'SV16003', name: 'Lê Hoàng C', file: 'LHC_Asm1.pdf', time: '19/05/2026 23:55', score: 9.0, comment: 'Xuất sắc!', status: 'Graded' },
  { id: 4, studentId: 'SV16004', name: 'Phạm Minh D', file: 'asm_java_d.zip', time: '20/05/2026 01:20', score: null, comment: '', status: 'Late' },
])

const selectedSubmission = ref(null)

const gradingStats = computed(() => {
  const graded = submissions.value.filter((submission) => submission.status === 'Graded').length
  const late = submissions.value.filter((submission) => submission.status === 'Late').length
  const pending = submissions.value.length - graded
  const gradedScores = submissions.value
    .filter((submission) => submission.score !== null)
    .map((submission) => Number(submission.score))
  const average = gradedScores.length
    ? (gradedScores.reduce((sum, score) => sum + score, 0) / gradedScores.length).toFixed(1)
    : '--'

  return [
    { label: 'Bài nộp', value: submissions.value.length, tone: 'primary' },
    { label: 'Đã chấm', value: graded, tone: 'success' },
    { label: 'Chờ chấm', value: pending, tone: 'warning' },
    { label: 'Nộp trễ', value: late, tone: 'danger' },
    { label: 'Điểm TB', value: average, tone: 'neutral' },
  ]
})

function selectGrading(sub) {
  selectedSubmission.value = { ...sub }
}

function saveGrade() {
  if (selectedSubmission.value) {
    const idx = submissions.value.findIndex(s => s.id === selectedSubmission.value.id)
    if (idx !== -1) {
      submissions.value[idx] = { ...selectedSubmission.value, status: 'Graded' }
    }
    selectedSubmission.value = null
    popupStore.success('Đã lưu điểm', 'Điểm và nhận xét đã được lưu thành công.')
  }
}

function statusVariant(status) {
  if (status === 'Graded') return 'success'
  if (status === 'Late') return 'danger'
  return 'warning'
}

function statusLabel(status) {
  if (status === 'Graded') return 'Đã chấm'
  if (status === 'Late') return 'Nộp trễ'
  return 'Chờ chấm'
}
</script>

<template>
  <div class="grading-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-main">
        <router-link to="/teacher/assignments" class="back-link" aria-label="Quay lại bài tập">
          <ArrowLeft :size="18" />
        </router-link>

        <div class="header-copy">
          <div class="context-tags">
            <GlassBadge variant="primary">Assignment 1</GlassBadge>
            <GlassBadge variant="neutral">SE1601</GlassBadge>
          </div>
          <h1>Chấm bài tập</h1>
          <p>Chấm điểm, phản hồi và theo dõi trạng thái bài nộp của lớp SE1601.</p>
        </div>
      </div>

      <div class="header-meta">
        <span>
          <Clock :size="14" />
          Hạn nộp: 20/05/2026 23:59
        </span>
        <span>
          <CheckCircle2 :size="14" />
          Đã chấm: {{ submissions.filter((sub) => sub.status === 'Graded').length }}/{{ submissions.length }}
        </span>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="notice">
        <span class="notice-icon">
          <AlertCircle :size="17" />
        </span>
        <div>
          <h2>Lưu ý bài nộp trễ</h2>
          <p>Hệ thống đánh dấu các bài nộp sau ngày 20/05/2026 là nộp trễ để giảng viên cân nhắc khi chấm.</p>
        </div>
      </div>

      <div class="summary-strip">
        <div v-for="item in gradingStats" :key="item.label" :class="['summary-pill', item.tone]">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
    </GlassPanel>

    <div class="grading-layout">
      <GlassPanel variant="flat" density="compact" class="submission-panel">
        <div class="panel-toolbar">
          <div>
            <h2>
              <Users :size="17" />
              Danh sách bài nộp
            </h2>
            <p>Chọn một sinh viên để nhập điểm và phản hồi.</p>
          </div>

          <label class="search-field">
            <Search :size="16" />
            <input type="text" placeholder="Tìm sinh viên, MSSV..." />
          </label>
        </div>

        <TableShell density="compact">
          <table>
            <thead>
              <tr>
                <th>Sinh viên</th>
                <th>File</th>
                <th>Nộp lúc</th>
                <th>Trạng thái</th>
                <th>Điểm</th>
                <th class="text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="sub in submissions"
                :key="sub.id"
                :class="selectedSubmission?.id === sub.id ? 'active-row' : ''"
                @click="selectGrading(sub)"
              >
                <td>
                  <div class="student-cell">
                    <span class="student-avatar">{{ sub.name.split(' ').pop()[0] }}</span>
                    <span>
                      <strong>{{ sub.name }}</strong>
                      <small>{{ sub.studentId }}</small>
                    </span>
                  </div>
                </td>
                <td>
                  <span class="file-cell">
                    <FileBox :size="14" />
                    {{ sub.file }}
                  </span>
                </td>
                <td>
                  <span class="time-cell">
                    <Clock :size="13" />
                    {{ sub.time }}
                  </span>
                </td>
                <td>
                  <GlassBadge :variant="statusVariant(sub.status)">
                    {{ statusLabel(sub.status) }}
                  </GlassBadge>
                </td>
                <td>
                  <strong v-if="sub.score !== null" class="score-cell">{{ sub.score.toFixed(1) }}</strong>
                  <span v-else class="empty-score">--</span>
                </td>
                <td>
                  <div class="row-actions">
                    <GlassButton variant="ghost" size="sm">
                      <template #leading>
                        <Download :size="13" />
                      </template>
                      File
                    </GlassButton>
                    <GlassButton
                      :variant="selectedSubmission?.id === sub.id ? 'primary' : 'secondary'"
                      size="sm"
                      @click.stop="selectGrading(sub)"
                    >
                      <template #leading>
                        <Edit3 :size="13" />
                      </template>
                      Chấm
                    </GlassButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>
      </GlassPanel>

      <GlassPanel
        v-if="selectedSubmission"
        variant="flat"
        density="compact"
        class="grading-panel"
        :clip="false"
      >
        <div class="grading-panel-header">
          <div class="grading-title">
            <span class="grading-icon">
              <FileDigit :size="20" />
            </span>
            <span>
              <h2>Chấm điểm</h2>
              <p>{{ selectedSubmission.name }} · {{ selectedSubmission.studentId }}</p>
            </span>
          </div>
          <button type="button" class="icon-button" @click="selectedSubmission = null">
            <X :size="16" />
          </button>
        </div>

        <div class="grading-form">
          <section class="form-block">
            <label>
              <FileBox :size="14" />
              File bài nộp
            </label>
            <a href="#" class="attachment-link">
              <span class="attachment-icon">
                <Download :size="17" />
              </span>
              <span>
                <strong>{{ selectedSubmission.file }}</strong>
                <small>{{ selectedSubmission.time }}</small>
              </span>
              <ExternalLink :size="15" />
            </a>
          </section>

          <section class="form-block">
            <label>
              <Star :size="14" />
              Điểm số (0-10)
            </label>
            <input
              v-model="selectedSubmission.score"
              type="number"
              max="10"
              min="0"
              step="0.1"
              placeholder="0.0"
              class="score-input"
            />
          </section>

          <section class="form-block">
            <label>
              <MessageSquare :size="14" />
              Nhận xét
            </label>
            <textarea
              v-model="selectedSubmission.comment"
              rows="5"
              placeholder="Nhập phản hồi cho sinh viên..."
              class="comment-input"
            />
          </section>

          <section class="feedback-note">
            <AlertCircle :size="15" />
            <p>Phản hồi sẽ hiển thị cho sinh viên sau khi giảng viên lưu kết quả.</p>
          </section>

          <GlassButton variant="primary" size="md" block @click="saveGrade">
            <template #leading>
              <Save :size="17" />
            </template>
            Lưu kết quả
          </GlassButton>
        </div>
      </GlassPanel>

      <GlassPanel v-else variant="flat" density="compact" class="empty-panel">
        <span class="empty-icon">
          <Edit3 :size="28" />
        </span>
        <h2>Chưa chọn bài</h2>
        <p>Nhấp vào một sinh viên trong danh sách để bắt đầu chấm điểm.</p>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.grading-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.page-header,
.context-panel,
.header-main,
.header-meta,
.summary-strip,
.panel-toolbar,
.row-actions,
.student-cell,
.file-cell,
.time-cell,
.grading-panel-header,
.grading-title,
.attachment-link,
.form-block label,
.feedback-note {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.panel-toolbar,
.grading-panel-header {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  align-items: flex-start;
  gap: 0.75rem;
  min-width: 0;
}

.back-link,
.icon-button,
.student-avatar,
.grading-icon,
.attachment-icon,
.notice-icon,
.empty-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: none;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.back-link,
.icon-button {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.back-link:hover,
.icon-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.header-copy {
  min-width: 0;
}

.context-tags {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
  margin-bottom: 0.45rem;
}

.header-copy h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  line-height: 1.15;
  font-weight: 900;
}

.header-copy p,
.header-meta,
.notice p,
.panel-toolbar p,
.student-cell small,
.time-cell,
.empty-score,
.grading-title p,
.attachment-link small,
.feedback-note p,
.empty-panel p,
.summary-pill span {
  color: var(--text-muted);
}

.header-copy p,
.notice p,
.panel-toolbar p,
.grading-title p,
.empty-panel p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-meta {
  justify-content: flex-end;
  gap: 0.75rem;
  flex-wrap: wrap;
  font-size: 0.78rem;
  font-weight: 800;
}

.header-meta span,
.notice {
  display: flex;
  align-items: center;
  gap: 0.45rem;
}

.notice {
  align-items: flex-start;
  max-width: 34rem;
}

.notice-icon {
  width: 2.1rem;
  height: 2.1rem;
  border-radius: var(--radius-md);
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
}

.notice h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9rem;
  font-weight: 900;
}

.summary-strip {
  justify-content: flex-end;
  gap: 0.45rem;
  flex-wrap: wrap;
}

.summary-pill {
  display: grid;
  min-width: 5rem;
  gap: 0.05rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.primary {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.grading-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(18rem, 24rem);
  gap: 0.875rem;
  align-items: start;
}

.submission-panel,
.grading-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.panel-toolbar {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.panel-toolbar h2 {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.search-field {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  width: min(18rem, 100%);
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.7rem;
  color: var(--text-placeholder);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.search-field:focus-within,
.score-input:focus,
.comment-input:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-field input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.search-field input::placeholder,
.score-input::placeholder,
.comment-input::placeholder {
  color: var(--text-placeholder);
}

.active-row {
  background: var(--accent-primary-soft);
}

.student-cell {
  min-width: 12rem;
  gap: 0.65rem;
}

.student-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
}

.student-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.file-cell,
.time-cell {
  gap: 0.35rem;
  min-width: 8rem;
  font-size: 0.78rem;
  font-weight: 750;
}

.file-cell {
  color: var(--text-heading);
}

.score-cell {
  color: var(--color-success-text);
  font-size: 0.9rem;
  font-weight: 900;
}

.empty-score {
  font-size: 0.8rem;
  font-weight: 850;
}

.row-actions {
  justify-content: flex-end;
  gap: 0.35rem;
  min-width: 8rem;
}

.grading-panel,
.empty-panel {
  position: sticky;
  top: 1rem;
}

.grading-panel-header {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.grading-title {
  gap: 0.65rem;
}

.grading-icon {
  width: 2.35rem;
  height: 2.35rem;
  border-radius: var(--radius-lg);
  color: var(--text-link);
}

.grading-title h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.grading-form {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.form-block {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.form-block label {
  gap: 0.35rem;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 900;
  text-transform: uppercase;
}

.attachment-link {
  justify-content: space-between;
  gap: 0.65rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.7rem;
  text-decoration: none;
}

.attachment-icon {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
}

.attachment-link strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.82rem;
}

.attachment-link small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.7rem;
  font-weight: 700;
}

.attachment-link > svg {
  color: var(--text-placeholder);
}

.score-input,
.comment-input {
  width: 100%;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-heading);
  outline: none;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.score-input {
  min-height: 3rem;
  padding: 0 0.8rem;
  text-align: center;
  color: var(--text-link);
  font-size: 1.6rem;
  font-weight: 900;
}

.comment-input {
  min-height: 7rem;
  padding: 0.75rem;
  resize: none;
  font-size: 0.84rem;
  font-weight: 650;
  line-height: 1.5;
}

.feedback-note {
  align-items: flex-start;
  gap: 0.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.65rem;
  color: var(--text-link);
}

.feedback-note p {
  margin: 0;
  font-size: 0.76rem;
  line-height: 1.45;
}

.empty-panel {
  min-height: 24rem;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.empty-icon {
  width: 3.5rem;
  height: 3.5rem;
  border-radius: var(--radius-xl);
  color: var(--text-placeholder);
  margin: 0 auto 0.8rem;
}

.empty-panel h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.empty-panel p {
  max-width: 14rem;
}

@media (max-width: 1180px) {
  .grading-layout {
    grid-template-columns: 1fr;
  }

  .grading-panel,
  .empty-panel {
    position: static;
  }
}

@media (max-width: 768px) {
  .page-header,
  .context-panel,
  .panel-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip {
    justify-content: flex-start;
  }

  .search-field {
    width: 100%;
  }
}

@media (max-width: 640px) {
  .header-meta,
  .summary-strip,
  .row-actions {
    display: grid;
    grid-template-columns: 1fr;
  }

  .summary-pill {
    width: 100%;
  }

  .row-actions {
    justify-content: flex-start;
  }
}
</style>
