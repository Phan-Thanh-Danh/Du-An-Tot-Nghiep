<template>
  <div class="webrtc-screen" :class="{ 'stream-active': isStreaming, 'stream-stopped': isStopped }">
    <video ref="videoRef" autoplay playsinline class="webrtc-video" :class="{ compact, large }"></video>
    <div class="webrtc-overlay">
      <span v-if="!isStreaming" class="status-badge">
        {{ isStopped ? 'Đã dừng chia sẻ' : 'Đang kết nối...' }}
      </span>
      <span class="student-name">{{ student.name }} ({{ student.id }})</span>
      <span v-if="violations > 0" class="violation-badge">Cảnh báo</span>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
  student: Object,
  violations: Number,
  compact: Boolean,
  large: Boolean,
  stream: Object // Nhận MediaStream từ ProctoringView
})

const videoRef = ref(null)
const isStreaming = ref(false)
const isStopped = ref(false)

watch(() => props.stream, (newStream) => {
  if (videoRef.value) {
    if (newStream) {
      videoRef.value.srcObject = newStream
      isStreaming.value = true
      isStopped.value = false
    } else {
      videoRef.value.srcObject = null
      isStreaming.value = false
      isStopped.value = props.student.streamStatus === 'stopped'
    }
  }
}, { immediate: true })

watch(() => props.student.streamStatus, (newStatus) => {
  if (newStatus === 'stopped') {
    isStopped.value = true
    isStreaming.value = false
    if (videoRef.value) videoRef.value.srcObject = null
  } else if (newStatus === 'streaming') {
    isStopped.value = false
  }
})

onMounted(() => {
  if (props.stream && videoRef.value) {
    videoRef.value.srcObject = props.stream
    isStreaming.value = true
  }
})
</script>

<style scoped>
.webrtc-screen {
  position: relative;
  width: 100%;
  height: 100%;
  background: #000;
  border-radius: 8px;
  overflow: hidden;
}
.webrtc-video {
  width: 100%;
  height: 100%;
  object-fit: contain;
}
.webrtc-video.compact {
  height: 120px;
}
.webrtc-video.large {
  height: calc(100vh - 200px);
}
.webrtc-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 8px;
}
.student-name {
  color: white;
  text-shadow: 1px 1px 2px black;
  font-size: 0.875rem;
}
.status-badge {
  color: white;
  background: rgba(0,0,0,0.5);
  padding: 4px 8px;
  border-radius: 4px;
  align-self: center;
  margin-top: 20%;
}
.violation-badge {
  color: white;
  background: red;
  padding: 2px 4px;
  border-radius: 4px;
  font-size: 0.75rem;
  align-self: flex-end;
}
</style>
