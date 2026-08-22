<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { AlertTriangle, CheckCircle2, EyeOff, Lock, RotateCcw, Timer, Wifi, WifiOff, ShieldCheck } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({
  lesson: {
    type: Object,
    required: true,
  },
})

const emit = defineEmits(['completed', 'progress', 'duration-change'])

const authStore = useAuthStore()
const SEEK_TOLERANCE_SECONDS = 2
const SAVE_INTERVAL_MS = 5000

const isOnline = ref(typeof navigator !== 'undefined' ? navigator.onLine : true)
const offlineNotice = ref('')

const videoShellRef = ref(null)
const isFullscreen = ref(false)

// 1. Watermark Text: Siêu tinh gọn (chỉ hiển thị mã người dùng/MSSV, không in email dài loằng ngoằng)
const watermarkText = computed(() => {
  const u = authStore.user
  if (!u) return 'LMS'
  const code = u.username || u.mssv || u.studentCode || u.maSinhVien || (u.id ? `ID:${u.id}` : '')
  return code ? `${code}` : 'LMS'
})

// 2. Periodic Ghost Flash: Mặc định ẩn hoàn toàn. Cứ mỗi 25s chỉ hiện mờ 2.5s rồi lại biến mất.
const watermarkPositions = [
  { top: '6%', right: '4%', bottom: 'auto', left: 'auto' },
  { top: 'auto', right: 'auto', bottom: '10%', left: '4%' },
  { top: '6%', left: '4%', bottom: 'auto', right: 'auto' },
  { top: 'auto', right: '4%', bottom: '10%', left: 'auto' },
  { top: '45%', right: '5%', bottom: 'auto', left: 'auto' },
  { top: '45%', left: '5%', bottom: 'auto', right: 'auto' },
]
const currentPosIndex = ref(0)
const isWatermarkVisible = ref(false)
let watermarkTimer = null
let watermarkHideTimer = null

function triggerGhostWatermark() {
  let nextIndex = Math.floor(Math.random() * watermarkPositions.length)
  if (nextIndex === currentPosIndex.value) {
    nextIndex = (nextIndex + 1) % watermarkPositions.length
  }
  currentPosIndex.value = nextIndex
  isWatermarkVisible.value = true

  if (watermarkHideTimer) clearTimeout(watermarkHideTimer)
  watermarkHideTimer = window.setTimeout(() => {
    isWatermarkVisible.value = false
  }, 2500)
}

function startWatermarkCycle() {
  if (watermarkTimer) clearInterval(watermarkTimer)
  triggerGhostWatermark()
  watermarkTimer = setInterval(triggerGhostWatermark, 25000)
}

function handleFullscreenChange() {
  const fsElem = document.fullscreenElement || document.webkitFullscreenElement
  if (fsElem && fsElem === videoRef.value) {
    if (document.exitFullscreen) {
      document.exitFullscreen().then(() => {
        if (videoShellRef.value?.requestFullscreen) {
          videoShellRef.value.requestFullscreen().catch(() => {})
        }
      }).catch(() => {})
    }
  }
  isFullscreen.value = Boolean(fsElem)
}

const durationSeconds = ref(props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0)
const videoRef = ref(null)
const metadataLoaded = ref(false)
const hasPendingOfflineProgress = ref(false)

const allowSeek = computed(() => {
  if (props.lesson?.allowSeek === false || props.lesson?.AllowSeek === false) return false
  return true
})
const pauseOnBlur = computed(() => props.lesson?.pauseOnBlur !== false)
const minWatchPercent = computed(() => props.lesson?.minWatchPercentToComplete || 80)
const hasVideoSource = computed(() => Boolean(props.lesson?.videoUrl))

function getOfflineProgressKey(lessonId) {
  const u = authStore.user
  const uId = u?.id || u?.userId || u?.Id || u?.UserId || u?.maNguoiDung || u?.MaNguoiDung || u?.email || u?.Email || 'guest'
  return `lms_offline_progress_${uId}_${lessonId}`
}

function saveProgressOffline(payload, forcePending = false) {
  try {
    const isOfflineNow = typeof navigator !== 'undefined' && !navigator.onLine
    if (props.lesson?.id && (isOfflineNow || forcePending)) {
      localStorage.setItem(getOfflineProgressKey(props.lesson.id), JSON.stringify({
        ...payload,
        pendingSync: true,
        timestamp: Date.now(),
      }))
    }
  } catch (e) {}
}

function readOfflineProgress(lessonId) {
  try {
    if (!lessonId) return null
    const raw = localStorage.getItem(getOfflineProgressKey(lessonId))
    return raw ? JSON.parse(raw) : null
  } catch (e) {
    return null
  }
}

function getExactWatchState(lessonId = null) {
  const targetId = lessonId || props.lesson?.id
  const cached = readOfflineProgress(targetId)
  const offline = cached?.pendingSync === true ? cached : null
  const hasPropCurrent = props.lesson?.watchedSeconds !== undefined && props.lesson?.watchedSeconds !== null
  const hasPropMax = props.lesson?.maxWatchedSeconds !== undefined && props.lesson?.maxWatchedSeconds !== null
  const current = Number(hasPropCurrent ? props.lesson.watchedSeconds : offline?.currentTimeSeconds) || 0
  const storedMax = Number(hasPropMax ? props.lesson.maxWatchedSeconds : offline?.maxWatchedSeconds) || 0
  const max = Math.max(current, storedMax)

  return {
    currentTimeSeconds: Math.max(0, current),
    maxWatchedSeconds: Math.max(0, max),
    hasExactSeconds: max > 0,
  }
}

function getHighestHistoricalProgress(lessonId = null) {
  const targetId = lessonId || props.lesson?.id
  const rawDbP = props.lesson?.progressPercent ?? props.lesson?.ProgressPercent
  const dbP = typeof rawDbP === 'number' ? rawDbP : 0
  const offline = readOfflineProgress(targetId)
  const offlineP = offline?.pendingSync === true ? (Number(offline.progressPercent) || 0) : 0
  return Math.max(dbP, offlineP)
}

function syncOfflineProgress() {
  if (!props.lesson?.id) return
  try {
    const data = readOfflineProgress(props.lesson.id)
    if (data?.pendingSync === true) {
      hasPendingOfflineProgress.value = true
      if (typeof data.progressPercent === 'number') {
        if (data.progressPercent > highestProgressPercent.value) {
          highestProgressPercent.value = data.progressPercent
          savedProgress.value = data.progressPercent
        }
        const exact = getExactWatchState()
        currentTimeSeconds.value = exact.currentTimeSeconds
        maxWatchedSeconds.value = exact.maxWatchedSeconds
      }
      if (metadataLoaded.value) {
        reconcileWithRealDuration(durationSeconds.value, false)
        if (isOnline.value) persistProgress(true, true)
      }
    }
  } catch (e) {}
}

function updateOnlineStatus() {
  isOnline.value = typeof navigator !== 'undefined' ? navigator.onLine : true
  if (isOnline.value) {
    offlineNotice.value = 'Đã khôi phục kết nối mạng. Đang đồng bộ tiến độ học tập...'
    syncOfflineProgress()
    window.setTimeout(() => {
      offlineNotice.value = ''
    }, 3500)
  } else {
    offlineNotice.value = 'Mất kết nối Internet tạm thời. Tiến độ học đang được lưu an toàn trên thiết bị của bạn.'
  }
}

function getInitialWatchedSeconds(customDuration = null) {
  const d = customDuration || 0
  const p = getHighestHistoricalProgress()
  const exact = getExactWatchState()

  if (exact.hasExactSeconds) {
    return d > 0
      ? Math.min(exact.currentTimeSeconds, d)
      : exact.currentTimeSeconds
  }

  // 1. Nếu bài học đã đạt 100% -> Bắt đầu con trỏ video từ giây 0 để người dùng xem lại
  if (p >= 100) {
    return 0
  }

  // 2. Nếu có mốc giây dở dang cụ thể (và còn cách cuối video ít nhất 5s)
  // 2. Chỉ đổi phần trăm thành giây sau khi đã có duration thật từ metadata.
  const isSeekAllowed = props.lesson?.allowSeek !== false && props.lesson?.AllowSeek !== false
  if (d > 0 && p > 0 && p < 100 && isSeekAllowed) {
    const proportionalSec = Math.round((p / 100) * d)
    if (proportionalSec < d - 5) {
      return proportionalSec
    }
  }

  return 0
}

const highestProgressPercent = ref(getHighestHistoricalProgress())
const initialExactState = getExactWatchState()
const initialW = getInitialWatchedSeconds()
const currentTimeSeconds = ref(initialW)
const maxWatchedSeconds = ref(initialExactState.hasExactSeconds ? initialExactState.maxWatchedSeconds : initialW)
const savedProgress = ref(highestProgressPercent.value)
const focusPauseMessage = ref('')
const seekGuardMessage = ref('')
const isRestoringSeek = ref(false)
let lastSavedAt = 0

const progressPercent = computed(() => {
  // Trước loadedmetadata, duration từ DTO có thể chỉ là giá trị dự kiến.
  // Không dùng nó để tính lại %, nếu không mỗi lần remount sẽ cộng thêm sai số.
  if (!metadataLoaded.value) return highestProgressPercent.value
  const duration = durationSeconds.value || props.lesson?.durationSeconds || props.lesson?.totalSeconds || 0
  if (!duration) return highestProgressPercent.value
  const watched = allowSeek.value ? Math.max(currentTimeSeconds.value, maxWatchedSeconds.value) : maxWatchedSeconds.value
  const calculated = Math.min(100, Math.round((watched / duration) * 100))
  // Tiến độ chỉ có tiến lên, không bao giờ được thấp hơn mốc cao nhất đã ghi nhận
  return Math.max(highestProgressPercent.value, calculated)
})
const displayedProgress = computed(() => Math.max(highestProgressPercent.value, savedProgress.value, progressPercent.value))
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
    metadataLoaded.value = false
    hasPendingOfflineProgress.value = false
    const initP = getHighestHistoricalProgress(newId)
    highestProgressPercent.value = initP
    savedProgress.value = initP
    const exact = getExactWatchState(newId)
    const initW = getInitialWatchedSeconds()
    currentTimeSeconds.value = exact.hasExactSeconds ? exact.currentTimeSeconds : initW
    maxWatchedSeconds.value = exact.hasExactSeconds ? exact.maxWatchedSeconds : initW
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
  if (realDuration > 0) {
    durationSeconds.value = realDuration
    metadataLoaded.value = true
    reconcileWithRealDuration(realDuration)
    if (hasPendingOfflineProgress.value && isOnline.value) {
      persistProgress(true, true)
    }
    emit('duration-change', {
      lessonId: props.lesson?.id,
      durationSeconds: realDuration,
      durationText: formatTime(realDuration),
    })
  }
}

function reconcileWithRealDuration(realDuration) {
  if (!videoRef.value || !realDuration) return

  const historicalPercent = highestProgressPercent.value
  const exact = getExactWatchState()
  let resumeAt = 0
  if (exact.hasExactSeconds) {
    const exactMax = Math.min(realDuration, exact.maxWatchedSeconds)
    resumeAt = Math.min(realDuration, exact.currentTimeSeconds)
    const exactPercent = exactMax >= realDuration - 1
      ? 100
      : Math.min(99, Math.round((exactMax / realDuration) * 100))

    highestProgressPercent.value = Math.max(historicalPercent, exactPercent)

    maxWatchedSeconds.value = exactMax
    savedProgress.value = highestProgressPercent.value
  } else {
    resumeAt = getInitialWatchedSeconds(realDuration)
    maxWatchedSeconds.value = resumeAt
  }

  currentTimeSeconds.value = resumeAt
  if (resumeAt > 0) {
    try {
      videoRef.value.currentTime = resumeAt
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
}

function onEnded() {
  if (!videoRef.value) return

  const finalDuration = Math.round(videoRef.value.duration || durationSeconds.value || 0)
  if (finalDuration > 0) {
    durationSeconds.value = finalDuration
    currentTimeSeconds.value = finalDuration
    maxWatchedSeconds.value = finalDuration
  }
  highestProgressPercent.value = 100
  savedProgress.value = 100

  const payload = buildProgressPayload(true)
  saveProgressOffline(payload)
  emit('progress', payload)
  emit('completed', payload)
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

function persistProgress(force = false, forceSave = false) {
  const now = Date.now()
  if (!force && now - lastSavedAt < SAVE_INTERVAL_MS) return
  lastSavedAt = now

  const currentCalc = progressPercent.value
  if (currentCalc > highestProgressPercent.value) {
    highestProgressPercent.value = currentCalc
  }
  savedProgress.value = highestProgressPercent.value

  const payload = buildProgressPayload(false, forceSave)
  saveProgressOffline(payload, forceSave)
  emit('progress', payload)
}

function buildProgressPayload(completed, forceSave = false) {
  return {
    lessonId: props.lesson.id,
    currentTimeSeconds: Math.round(currentTimeSeconds.value),
    maxWatchedSeconds: Math.round(maxWatchedSeconds.value),
    progressPercent: displayedProgress.value,
    // `completed` chỉ có nghĩa là video đã chạy tới cuối. Ngưỡng 80%
    // chỉ dùng để hiển thị điều kiện học tập, không được đổi tiến độ thành 100%.
    completed: completed || displayedProgress.value >= 100,
    forceSave,
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
  document.addEventListener('fullscreenchange', handleFullscreenChange)
  document.addEventListener('webkitfullscreenchange', handleFullscreenChange)
  window.addEventListener('beforeunload', handleBeforeUnload)
  window.addEventListener('online', updateOnlineStatus)
  window.addEventListener('offline', updateOnlineStatus)
  syncOfflineProgress()
  startWatermarkCycle()
})

onBeforeUnmount(() => {
  // Chuyển sang bài khác sẽ tháo player hiện tại; phải chốt đúng số giây
  // trước khi xóa src để cache offline không giữ một mốc cũ.
  persistProgress(true, true)
  if (videoRef.value) {
    videoRef.value.pause()
    videoRef.value.removeAttribute('src')
    videoRef.value.load()
  }
  if (watermarkTimer) clearInterval(watermarkTimer)
  if (watermarkHideTimer) clearTimeout(watermarkHideTimer)
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
  document.removeEventListener('webkitfullscreenchange', handleFullscreenChange)
  window.removeEventListener('beforeunload', handleBeforeUnload)
  window.removeEventListener('online', updateOnlineStatus)
  window.removeEventListener('offline', updateOnlineStatus)
})
</script>

<template>
  <section class="lesson-video-player" aria-label="Video bài học">
    <div ref="videoShellRef" class="video-shell relative group">
      <video
        v-if="hasVideoSource && !videoErrorMessage"
        ref="videoRef"
        class="lesson-video"
        controls
        controlsList="nodownload"
        disablePictureInPicture
        preload="metadata"
        :src="lesson.videoUrl"
        @contextmenu.prevent
        @loadedmetadata="onLoadedMetadata"
        @timeupdate="onTimeUpdate"
        @ended="onEnded"
        @seeking="onSeeking"
        @seeked="onSeeked"
        @play="onPlay"
        @pause="persistProgress(true)"
        @error="onVideoError"
      />

      <!-- Periodic Ghost Flash Watermark (Mờ nhạt, thoắt ẩn thoắt hiện) -->
      <div
        v-if="hasVideoSource && !videoErrorMessage"
        class="video-watermark-overlay"
        aria-hidden="true"
      >
        <div
          class="subtle-watermark-stamp"
          :class="{ 'is-visible': isWatermarkVisible }"
          :style="watermarkPositions[currentPosIndex]"
        >
          <span class="watermark-dot"></span>
          <span>{{ watermarkText }}</span>
        </div>
      </div>

      <div v-else-if="videoErrorMessage" class="video-placeholder">
        <div class="video-placeholder-icon">
          <AlertTriangle :size="28" class="text-amber-400" />
        </div>
        <div>
          <strong>Không thể phát video trực tiếp</strong>
          <span>{{ videoErrorMessage }}</span>
          <div class="mt-2 flex items-center gap-2">
            <button
              type="button"
              class="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-xs font-semibold inline-flex items-center gap-1.5 transition-colors"
              @click="retryVideoLoad"
            >
              <RotateCcw :size="12" /> Thử tải lại video
            </button>
            <span v-if="!isOnline" class="text-[11px] text-amber-300 font-medium">Đang mất kết nối mạng</span>
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

    <!-- Network / Offline State Notice -->
    <div v-if="offlineNotice" class="player-message" :class="isOnline ? 'online-sync-message' : 'offline-message'">
      <Wifi v-if="isOnline" :size="14" class="text-emerald-500 shrink-0" />
      <WifiOff v-else :size="14" class="text-amber-500 shrink-0" />
      <span>{{ offlineNotice }}</span>
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
  position: relative;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  background: var(--surface-app);
  box-shadow: var(--lg-shadow-sm);
}

/* Hỗ trợ chế độ Toàn màn hình (Full Screen) */
.video-shell:fullscreen,
.video-shell:-webkit-full-screen {
  width: 100vw !important;
  height: 100vh !important;
  background: #000 !important;
  border-radius: 0 !important;
  border: none !important;
  display: flex;
  align-items: center;
  justify-content: center;
}

.video-shell:fullscreen .lesson-video,
.video-shell:-webkit-full-screen .lesson-video {
  width: 100%;
  height: 100%;
  max-height: 100vh;
  object-fit: contain;
}

.video-shell:fullscreen .video-watermark-overlay,
.video-shell:-webkit-full-screen .video-watermark-overlay {
  z-index: 2147483647;
}

.lesson-video,
.video-placeholder {
  display: block;
  width: 100%;
  aspect-ratio: 16 / 9;
  max-height: 22rem;
}

/* Periodic Ghost Flash Watermark */
.video-watermark-overlay {
  position: absolute;
  inset: 0;
  pointer-events: none;
  user-select: none;
  z-index: 15;
  overflow: hidden;
}

.subtle-watermark-stamp {
  position: absolute;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.65rem;
  font-weight: 500;
  letter-spacing: 0.1em;
  color: rgba(255, 255, 255, 0.18);
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
  opacity: 0;
  transition: opacity 0.8s cubic-bezier(0.4, 0, 0.2, 1), transform 0.8s cubic-bezier(0.4, 0, 0.2, 1);
  transform: translateY(3px);
}

.subtle-watermark-stamp.is-visible {
  opacity: 1;
  transform: translateY(0);
}

.watermark-dot {
  width: 3.5px;
  height: 3.5px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.22);
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

.offline-message {
  color: var(--color-warning-text);
  background: var(--color-warning-bg);
  border-color: rgba(245, 158, 11, 0.25);
}

.online-sync-message {
  color: var(--color-success-text);
  background: var(--color-success-bg);
  border-color: rgba(16, 185, 129, 0.25);
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
