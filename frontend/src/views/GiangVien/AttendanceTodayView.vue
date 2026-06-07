<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  ArrowLeft,
  Calendar,
  CheckCircle2,
  ChevronRight,
  Clock,
  MapPin,
  Save,
  Search,
  Users,
  XCircle,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const popupStore = usePopupStore()

const todayClasses = ref([
  { id: 1, code: 'SE1601', name: 'Lập trình Java', room: 'A201', time: '07:30 - 10:45', students: 45 },
  { id: 2, code: 'SS1402', name: 'Lập trình Web', room: 'B305', time: '12:30 - 15:45', students: 38 },
])

const selectedClass = ref(null)
const students = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', present: true },
  { id: 'SV16002', name: 'Trần Thị B', present: true },
  { id: 'SV16003', name: 'Lê Hoàng C', present: false },
  { id: 'SV16004', name: 'Phạm Minh D', present: true },
])

const attendanceSummary = computed(() => {
  const present = students.value.filter((student) => student.present).length
  const absent = students.value.length - present

  return [
    { label: 'Danh sách', value: students.value.length, tone: 'primary' },
    { label: 'Có mặt', value: present, tone: 'success' },
    { label: 'Vắng', value: absent, tone: 'danger' },
    { label: 'Đi muộn', value: 0, tone: 'warning' },
    { label: 'Có phép', value: 0, tone: 'info' },
    { label: 'Chưa đánh dấu', value: 0, tone: 'neutral' },
  ]
})

function selectClass(cls) {
  selectedClass.value = cls
}

function submitAttendance() {
  popupStore.success('Đã lưu điểm danh', `Đã lưu điểm danh cho lớp ${selectedClass.value.code}`)
  selectedClass.value = null
}
</script>

<template>
  <div class="attendance-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-main">
        <button
          v-if="selectedClass"
          type="button"
          class="back-button"
          aria-label="Quay lại danh sách buổi học"
          @click="selectedClass = null"
        >
          <ArrowLeft :size="18" />
        </button>

        <div class="header-copy">
          <div class="eyebrow">
            <Calendar :size="15" />
            {{ new Date().toLocaleDateString('vi-VN') }}
          </div>
          <div>
            <h1>{{ selectedClass ? `Điểm danh lớp ${selectedClass.code}` : 'Điểm danh hôm nay' }}</h1>
            <p v-if="selectedClass">
              {{ selectedClass.name }} · {{ selectedClass.time }} · Phòng {{ selectedClass.room }}
            </p>
            <p v-else>Chọn buổi học hôm nay để bắt đầu điểm danh và xác nhận sĩ số.</p>
          </div>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge :variant="selectedClass ? 'warning' : 'info'">
          {{ selectedClass ? 'Đang điểm danh' : 'Chưa chọn buổi học' }}
        </GlassBadge>
        <GlassButton v-if="selectedClass" variant="primary" size="sm" @click="submitAttendance">
          <template #leading>
            <Save :size="16" />
          </template>
          Lưu điểm danh
        </GlassButton>
      </div>
    </GlassPanel>

    <template v-if="!selectedClass">
      <GlassPanel variant="flat" density="compact" class="session-panel">
        <div class="panel-title">
          <div>
            <h2>Buổi học hôm nay</h2>
            <p>{{ todayClasses.length }} lớp có lịch giảng dạy trong ngày.</p>
          </div>
          <GlassBadge variant="success">Sẵn sàng</GlassBadge>
        </div>

        <div class="session-grid">
          <article
            v-for="cls in todayClasses"
            :key="cls.id"
            class="session-card"
            @click="selectClass(cls)"
          >
            <div class="session-card-header">
              <span class="session-icon">
                <Calendar :size="20" />
              </span>
              <GlassBadge variant="success">Sắp diễn ra</GlassBadge>
            </div>

            <div>
              <h3>{{ cls.code }}</h3>
              <p>{{ cls.name }}</p>
            </div>

            <div class="session-meta">
              <span>
                <Clock :size="15" />
                {{ cls.time }}
              </span>
              <span>
                <MapPin :size="15" />
                Phòng {{ cls.room }}
              </span>
              <span>
                <Users :size="15" />
                {{ cls.students }} sinh viên
              </span>
            </div>

            <GlassButton variant="secondary" size="sm" block @click.stop="selectClass(cls)">
              Bắt đầu điểm danh
              <template #trailing>
                <ChevronRight :size="14" />
              </template>
            </GlassButton>
          </article>
        </div>
      </GlassPanel>
    </template>

    <template v-else>
      <GlassPanel variant="flat" density="compact" class="context-panel">
        <div class="summary-strip">
          <div v-for="item in attendanceSummary" :key="item.label" :class="['summary-pill', item.tone]">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </div>

        <div class="toolbar-actions">
          <label class="search-field">
            <Search :size="16" />
            <input type="text" placeholder="Tìm sinh viên..." />
          </label>
          <GlassButton variant="secondary" size="sm" @click="students.forEach((sv) => (sv.present = true))">
            Tất cả có mặt
          </GlassButton>
          <GlassButton variant="ghost" size="sm" @click="students.forEach((sv) => (sv.present = false))">
            Reset
          </GlassButton>
        </div>
      </GlassPanel>

      <GlassPanel variant="flat" density="compact" class="attendance-panel">
        <div class="panel-title">
          <div>
            <h2>Danh sách sinh viên</h2>
            <p>Đánh dấu nhanh trạng thái có mặt hoặc vắng cho từng sinh viên.</p>
          </div>
          <GlassBadge variant="warning">Chưa xác nhận</GlassBadge>
        </div>

        <TableShell density="compact">
          <table>
            <thead>
              <tr>
                <th>Sinh viên</th>
                <th>MSSV</th>
                <th>Lớp</th>
                <th>Trạng thái</th>
                <th>Ghi chú</th>
                <th class="text-right">Hành động nhanh</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sv in students" :key="sv.id">
                <td>
                  <div class="student-cell">
                    <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                    <span>
                      <strong>{{ sv.name }}</strong>
                      <small>{{ selectedClass.name }}</small>
                    </span>
                  </div>
                </td>
                <td class="student-code">{{ sv.id }}</td>
                <td>
                  <GlassBadge variant="primary">{{ selectedClass.code }}</GlassBadge>
                </td>
                <td>
                  <GlassBadge :variant="sv.present ? 'success' : 'danger'">
                    {{ sv.present ? 'Có mặt' : 'Vắng' }}
                  </GlassBadge>
                </td>
                <td class="note-cell">
                  {{ sv.present ? 'Đã ghi nhận trong buổi học' : 'Cần xác nhận lý do vắng' }}
                </td>
                <td>
                  <div class="quick-actions">
                    <label :class="['status-choice', sv.present ? 'active success' : '']">
                      <input
                        type="radio"
                        :name="'att-' + sv.id"
                        :checked="sv.present"
                        @change="sv.present = true"
                      />
                      <CheckCircle2 :size="14" />
                      Có mặt
                    </label>

                    <label :class="['status-choice', !sv.present ? 'active danger' : '']">
                      <input
                        type="radio"
                        :name="'att-' + sv.id"
                        :checked="!sv.present"
                        @change="sv.present = false"
                      />
                      <XCircle :size="14" />
                      Vắng
                    </label>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>

        <div class="submit-bar">
          <div class="submit-note">
            <Clock :size="15" />
            Điểm danh sẽ được lưu cho buổi {{ selectedClass.time }} tại phòng {{ selectedClass.room }}.
          </div>
          <GlassButton variant="primary" size="md" @click="submitAttendance">
            <template #leading>
              <Save :size="17" />
            </template>
            Hoàn tất điểm danh
          </GlassButton>
        </div>
      </GlassPanel>
    </template>
  </div>
</template>

<style scoped>
.attendance-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.page-header,
.header-main,
.header-actions,
.panel-title,
.session-card-header,
.session-meta span,
.context-panel,
.summary-strip,
.toolbar-actions,
.student-cell,
.quick-actions,
.status-choice,
.submit-bar,
.submit-note {
  display: flex;
  align-items: center;
}

.page-header,
.panel-title,
.context-panel,
.submit-bar {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  align-items: flex-start;
  gap: 0.75rem;
  min-width: 0;
}

.back-button {
  display: inline-flex;
  width: 2.25rem;
  height: 2.25rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.back-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.header-copy {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.5rem;
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

.header-copy h1,
.panel-title h2,
.session-card h3 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

.header-copy h1 {
  font-size: 1.45rem;
  line-height: 1.15;
}

.panel-title h2 {
  font-size: 1rem;
}

.session-card h3 {
  font-size: 1.1rem;
}

.header-copy p,
.panel-title p,
.session-card p,
.session-meta span,
.summary-pill span,
.student-cell small,
.student-code,
.note-cell,
.submit-note {
  color: var(--text-muted);
}

.header-copy p,
.panel-title p,
.session-card p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-actions,
.summary-strip,
.toolbar-actions {
  justify-content: flex-end;
  gap: 0.55rem;
  flex-wrap: wrap;
}

.session-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;
}

.session-card {
  display: flex;
  cursor: pointer;
  flex-direction: column;
  gap: 0.85rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.9rem;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    transform 0.2s ease;
}

.session-card:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  transform: translateY(-1px);
}

.session-card-header {
  justify-content: space-between;
  gap: 0.75rem;
}

.session-icon,
.student-avatar {
  display: inline-flex;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.session-icon {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
}

.session-meta {
  display: grid;
  gap: 0.45rem;
}

.session-meta span {
  gap: 0.45rem;
  font-size: 0.8rem;
  font-weight: 750;
}

.context-panel {
  align-items: center;
}

.summary-strip {
  justify-content: flex-start;
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

.summary-pill.primary,
.summary-pill.info {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
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

.search-field:focus-within {
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

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.attendance-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.panel-title {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.student-cell {
  min-width: 13rem;
  gap: 0.65rem;
}

.student-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
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

.student-code,
.note-cell {
  font-size: 0.8rem;
  font-weight: 750;
}

.quick-actions {
  justify-content: flex-end;
  gap: 0.4rem;
  min-width: 11rem;
}

.status-choice {
  min-height: var(--control-height-sm);
  cursor: pointer;
  gap: 0.35rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0 0.65rem;
  font-size: 0.76rem;
  font-weight: 850;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.status-choice input {
  display: none;
}

.status-choice:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.status-choice.active.success {
  border-color: var(--border-card);
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.status-choice.active.danger {
  border-color: var(--border-card);
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.submit-bar {
  align-items: center;
  border-top: 1px solid var(--border-card);
  padding-top: 0.75rem;
}

.submit-note {
  gap: 0.45rem;
  font-size: 0.78rem;
  font-weight: 750;
}

@media (max-width: 1024px) {
  .session-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .context-panel,
  .submit-bar {
    flex-direction: column;
    align-items: stretch;
  }

  .toolbar-actions {
    justify-content: flex-start;
  }
}

@media (max-width: 768px) {
  .page-header,
  .panel-title {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .session-grid,
  .summary-strip,
  .toolbar-actions,
  .quick-actions {
    display: grid;
    grid-template-columns: 1fr;
  }

  .summary-pill,
  .search-field {
    width: 100%;
  }

  .quick-actions {
    justify-content: flex-start;
  }
}
</style>
