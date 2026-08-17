<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { AlertTriangle, CheckCircle2, EyeOff, Lock, RotateCcw, Timer } from 'lucide-vue-next'

const props = defineProps({
  lesson: {
    type: Object,
    required: true,
  },
})

const emit = defineEmits(['completed', 'progress'])

const SEEK_TOLERANCE_SECONDS = 2
const SAVE_INTERVAL_MS = 5000

const durationSeconds = ref(props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0)
const videoRef = ref(null)

const allowSeek = computed(() => {
  if (props.lesson?.allowSeek === false || props.lesson?.AllowSeek === false) return false
  return true
})
const pauseOnBlur = computed(() => props.lesson?.pauseOnBlur !== false)
const minWatchPercent = computed(() => props.lesson?.minWatchPercentToComplete || 80)
const hasVideoSource = computed(() => Boolean(props.lesson?.videoUrl))

function getInitialWatchedSeconds(customDuration = null) {
  const p = Number(props.lesson?.progressPercent) || 0
  const d = customDuration || durationSeconds.value || props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0
  const isSeekAllowed = props.lesson?.allowSeek !== false && props.lesson?.AllowSeek !== false

  if (props.lesson?.watchedSeconds && props.lesson.watchedSeconds > 0) {
    return Math.min(props.lesson.watchedSeconds, d > 0 ? d : props.lesson.watchedSeconds)
  }
  if (props.lesson?.maxWatchedSeconds && props.lesson.maxWatchedSeconds > 0) {
    return Math.min(props.lesson.maxWatchedSeconds, d > 0 ? d : props.lesson.maxWatchedSeconds)
  }
  if (d > 0 && p > 0 && isSeekAllowed) {
    if (p >= 80 || props.lesson?.status === 'completed') return d
    return Math.min(d, Math.round((p / 100) * d))
  }
  return 0
}

const initialW = getInitialWatchedSeconds()
const currentTimeSeconds = ref(initialW)
const maxWatchedSeconds = ref(initialW)
const savedProgress = ref(props.lesson?.progressPercent || 0)
const focusPauseMessage = ref('')
const seekGuardMessage = ref('')
const isRestoringSeek = ref(false)
let lastSavedAt = 0

const progressPercent = computed(() => {
  const duration = durationSeconds.value || props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0
  if (!duration) return savedProgress.value
  const watched = allowSeek.value ? Math.max(currentTimeSeconds.value, maxWatchedSeconds.value) : maxWatchedSeconds.value
  return Math.min(100, Math.round((watched / duration) * 100))
})
const displayedProgress = computed(() => Math.max(savedProgress.value, progressPercent.value))
const isCompleted = computed(() => displayedProgress.value >= minWatchPercent.value)

watch(
  () => [props.lesson?.id, props.lesson?.videoUrl],
  ([newId, newUrl], [oldId, oldUrl] = []) => {
    // CHỈ pause và reset khi ID của bài học THỰC SỰ THAY ĐỔI sang bài khác!
    if (oldId && newId && String(oldId) === String(newId)) {
      return
    }

    if (videoRef.value && !videoRef.value.paused) {
      videoRef.value.pause()
    }
    durationSeconds.value = props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0
    savedProgress.value = props.lesson?.progressPercent || 0
    const initW = getInitialWatchedSeconds()
    currentTimeSeconds.value = initW
    maxWatchedSeconds.value = initW
    focusPauseMessage.value = ''
    seekGuardMessage.value = ''
    isRestoringSeek.value = false
    lastSavedAt = 0
  }
)

let previousSeekState = props.lesson?.allowSeek !== false && props.lesson?.AllowSeek !== false
watch(
  () => [props.lesson?.allowSeek, props.lesson?.AllowSeek],
  ([newSeek]) => {
    const isAllowed = newSeek !== false && props.lesson?.AllowSeek !== false
    if (isAllowed === previousSeekState) return
    previousSeekState = isAllowed

    if (!isAllowed) {
      seekGuardMessage.value = '🔒 Giảng viên vừa khóa tua video. Video yêu cầu xem tuần tự.'
    } else {
      seekGuardMessage.value = '🔓 Giảng viên đã cho phép tự do tua video.'
    }
    window.setTimeout(() => {
      seekGuardMessage.value = ''
    }, 3500)
  }
)

function formatTime(seconds) {
  const safeSeconds = Math.max(0, Math.floor(seconds || 0))
  const minutes = Math.floor(safeSeconds / 60)
  return `${minutes}:${String(safeSeconds % 60).padStart(2, '0')}`
}

function onLoadedMetadata() {
  if (!videoRef.value) return
  const realDuration = Math.round(videoRef.value.duration || durationSeconds.value || 0)
  durationSeconds.value = realDuration

  const initW = getInitialWatchedSeconds(realDuration)
  if (initW > 0) {
    maxWatchedSeconds.value = Math.max(maxWatchedSeconds.value, initW)
    currentTimeSeconds.value = initW
    try {
      videoRef.value.currentTime = initW
    } catch (e) {}
  }
}

function onTimeUpdate() {
  if (!videoRef.value) return
  const currentTime = videoRef.value.currentTime || 0
  currentTimeSeconds.value = currentTime

  // Khi đang phát video bình thường (không phải thao tác kéo tua),
  // maxWatchedSeconds luôn tịnh tiến trơn tru theo thời gian thực tế mà không bị ngắt quãng hay giật cục
  if (!videoRef.value.seeking && !isRestoringSeek.value) {
    if (currentTime > maxWatchedSeconds.value) {
      maxWatchedSeconds.value = currentTime
    }
  }

  persistProgress()

  if (isCompleted.value) {
    emit('completed', buildProgressPayload(true))
  }
}

function onSeeking() {
  if (!videoRef.value || isRestoringSeek.value) return
  
  const requestedTime = videoRef.value.currentTime || 0

  // 1. Nếu giảng viên cho phép tua tự do -> KHÔNG CHẶN
  if (allowSeek.value) {
    seekGuardMessage.value = ''
    currentTimeSeconds.value = requestedTime
    maxWatchedSeconds.value = Math.max(maxWatchedSeconds.value, requestedTime)
    return
  }

  // 2. Nếu giảng viên khóa tua nhanh -> chỉ cho phép tua lại trong phạm vi đã xem
  if (requestedTime > maxWatchedSeconds.value + SEEK_TOLERANCE_SECONDS) {
    isRestoringSeek.value = true
    const safeTarget = maxWatchedSeconds.value
    videoRef.value.currentTime = safeTarget
    currentTimeSeconds.value = safeTarget
    seekGuardMessage.value = `Bài học này yêu cầu xem tuần tự theo cấu hình của giảng viên. Đã chuyển về vị trí đã học (${formatTime(safeTarget)}).`
    
    // Đảm bảo video tiếp tục phát trơn tru từ điểm an toàn nếu người dùng đang phát
    if (!videoRef.value.paused) {
      videoRef.value.play().catch(() => {})
    }

    window.setTimeout(() => {
      isRestoringSeek.value = false
    }, 200)

    window.setTimeout(() => {
      if (seekGuardMessage.value.includes('yêu cầu xem tuần tự')) {
        seekGuardMessage.value = ''
      }
    }, 4000)
  } else {
    seekGuardMessage.value = ''
    currentTimeSeconds.value = requestedTime
  }
}

function onSeeked() {
  if (!videoRef.value) return
  const cur = videoRef.value.currentTime || 0
  currentTimeSeconds.value = cur
  if (allowSeek.value) {
    maxWatchedSeconds.value = Math.max(maxWatchedSeconds.value, cur)
  } else if (cur <= maxWatchedSeconds.value + SEEK_TOLERANCE_SECONDS) {
    maxWatchedSeconds.value = Math.max(maxWatchedSeconds.value, cur)
  }
  persistProgress(true)
}

function pauseVideo(reason) {
  if (!pauseOnBlur.value) return
  if (!videoRef.value || videoRef.value.paused) return
  videoRef.value.pause()
  focusPauseMessage.value = reason
  persistProgress(true)
}

function handleVisibilityChange() {
  if (!pauseOnBlur.value) return
  if (document.visibilityState === 'hidden') {
    pauseVideo('Video đã tạm dừng vì bạn chuyển sang tab khác.')
  }
}

function handleWindowBlur() {
  // Chỉ dừng nếu cấu hình pauseOnBlur bật và tài liệu thực sự bị ẩn
  if (!pauseOnBlur.value) return
  if (document.visibilityState === 'hidden') {
    pauseVideo('Video đã tạm dừng vì bạn chuyển sang tab khác.')
  }
}

function onPlay() {
  focusPauseMessage.value = ''
  seekGuardMessage.value = ''
}

function handleBeforeUnload() {
  persistProgress(true)
}

function persistProgress(force = false) {
  const now = Date.now()
  if (!force && now - lastSavedAt < SAVE_INTERVAL_MS) return
  lastSavedAt = now
  savedProgress.value = Math.max(savedProgress.value, progressPercent.value)

  emit('progress', buildProgressPayload(isCompleted.value))
}

function buildProgressPayload(completed) {
  return {
    lessonId: props.lesson.id,
    currentTimeSeconds: Math.round(currentTimeSeconds.value),
    maxWatchedSeconds: Math.round(maxWatchedSeconds.value),
    progressPercent: displayedProgress.value,
    completed,
  }
}

const videoErrorMessage = ref('')

function onVideoError(e) {
  const err = videoRef.value?.error
  if (err) {
    console.warn('Lesson video stream error:', err.code, err.message)
    videoErrorMessage.value = 'Không thể tải luồng video từ máy chủ. Vui lòng kiểm tra lại kết nối hoặc thử tải lại bài học.'
  }
}

function retryVideoLoad() {
  videoErrorMessage.value = ''
  if (videoRef.value) {
    videoRef.value.load()
  }
}

onMounted(() => {
  document.addEventListener('visibilitychange', handleVisibilityChange)
  window.addEventListener('beforeunload', handleBeforeUnload)
})

onBeforeUnmount(() => {
  if (videoRef.value) {
    videoRef.value.pause()
    videoRef.value.removeAttribute('src')
    videoRef.value.load()
  }
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  window.removeEventListener('beforeunload', handleBeforeUnload)
})
</script>

<template>
  <section class="lesson-video-player" aria-label="Video bài học">
    <div class="video-shell">
      <video
        v-if="hasVideoSource && !videoErrorMessage"
        ref="videoRef"
        class="lesson-video"
        controls
        preload="metadata"
        :src="lesson.videoUrl"
        @loadedmetadata="onLoadedMetadata"
        @timeupdate="onTimeUpdate"
        @seeking="onSeeking"
        @seeked="onSeeked"
        @play="onPlay"
        @pause="persistProgress(true)"
        @error="onVideoError"
      />

      <div v-else-if="videoErrorMessage" class="video-placeholder">
        <div class="video-placeholder-icon">
          <AlertTriangle :size="28" class="text-amber-400" />
        </div>
        <div>
          <strong>Không thể phát video trực tiếp</strong>
          <span>{{ videoErrorMessage }}</span>
          <div class="mt-2">
            <button
              type="button"
              class="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-xs font-semibold"
              @click="retryVideoLoad"
            >
              Thử tải lại video
            </button>
          </div>
        </div>
      </div>

      <div v-else class="video-placeholder">
        <div class="video-placeholder-icon">
          <Timer :size="28" />
        </div>
        <div>
          <strong>Video demo chưa có file phát</strong>
          <span>Player đã sẵn sàng nhận URL video từ backend.</span>
        </div>
      </div>
    </div>

    <div class="video-meta">
      <span class="seek-badge" :class="allowSeek ? 'seek-free' : 'seek-locked'">
        <RotateCcw v-if="allowSeek" :size="13" />
        <Lock v-else :size="13" />
        {{ allowSeek ? 'Cho phép tua' : 'Xem theo trình tự' }}
      </span>
      <span class="duration-chip">{{ formatTime(currentTimeSeconds) }} / {{ formatTime(durationSeconds) }}</span>
    </div>

    <div class="progress-block">
      <div class="progress-copy">
        <span>Tiến độ xem video</span>
        <strong>{{ displayedProgress }}%</strong>
      </div>
      <div class="progress-track" aria-hidden="true">
        <div class="progress-fill" :style="{ width: `${displayedProgress}%` }" />
      </div>
    </div>

    <p class="rule-note">
      <RotateCcw v-if="allowSeek" :size="14" />
      <Lock v-else :size="14" />
      <span v-if="allowSeek">Video này cho phép tua theo cấu hình của giảng viên.</span>
      <span v-else>Video này cần xem theo trình tự. Bạn có thể tua lại phần đã xem, nhưng không thể tua đến phần chưa học.</span>
    </p>

    <p v-if="focusPauseMessage" class="player-message focus-message">
      <EyeOff :size="14" />
      {{ focusPauseMessage }}
    </p>

    <p v-if="seekGuardMessage" class="player-message seek-message">
      <AlertTriangle :size="14" />
      {{ seekGuardMessage }}
    </p>

    <p class="completion-note" :class="{ completed: isCompleted }">
      <CheckCircle2 :size="14" />
      <span v-if="isCompleted">Đã đạt điều kiện hoàn thành video.</span>
      <span v-else>Cần xem tối thiểu {{ minWatchPercent }}% để hoàn thành.</span>
    </p>
  </section>
</template>

<style scoped>
.lesson-video-player {
  display: grid;
  gap: 0.75rem;
}

.video-shell {
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  background: var(--surface-app);
  box-shadow: var(--lg-shadow-sm);
}

.lesson-video,
.video-placeholder {
  display: block;
  width: 100%;
  aspect-ratio: 16 / 9;
  max-height: 22rem;
}

.video-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.85rem;
  color: var(--text-inverse);
  background: linear-gradient(135deg, #0f172a, #111827);
  padding: 1rem;
  text-align: left;
}

.video-placeholder-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-lg);
  background: rgba(255, 255, 255, 0.1);
  color: var(--text-inverse);
}

.video-placeholder strong,
.video-placeholder span {
  display: block;
}

.video-placeholder strong {
  font-size: 0.95rem;
  font-weight: 850;
}

.video-placeholder span {
  margin-top: 0.2rem;
  color: rgba(255, 255, 255, 0.68);
  font-size: 0.78rem;
  font-weight: 650;
}

.video-meta {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.seek-badge,
.duration-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  min-height: 1.65rem;
  border-radius: 999px;
  padding: 0.25rem 0.65rem;
  font-size: 0.72rem;
  font-weight: 850;
}

.seek-free {
  color: var(--color-success-text);
  background: var(--color-success-bg);
}

.seek-locked {
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
}

.duration-chip {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-label);
}

.progress-block {
  display: grid;
  gap: 0.35rem;
}

.progress-copy {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 800;
}

.progress-copy strong {
  color: var(--text-heading);
}

.progress-track {
  height: 0.5rem;
  overflow: hidden;
  border-radius: 999px;
  background: var(--surface-input);
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--accent-primary), var(--accent-cyan));
  transition: width 180ms ease;
}

.rule-note,
.player-message,
.completion-note {
  display: flex;
  align-items: flex-start;
  gap: 0.45rem;
  margin: 0;
  border: 1px solid var(--border-card);
  border-radius: 14px;
  padding: 0.55rem 0.65rem;
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 720;
  line-height: 1.45;
}

.rule-note {
  background: var(--surface-input);
}

.focus-message {
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
}

.seek-message {
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
}

.completion-note {
  color: var(--text-placeholder);
  background: var(--surface-input);
}

.completion-note.completed {
  color: var(--color-success-text);
  background: var(--color-success-bg);
}

@media (max-width: 640px) {
  .video-placeholder {
    flex-direction: column;
    text-align: center;
  }
}
</style>
