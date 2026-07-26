export const getRtcConfig = () => ({
  iceServers: [
    {
      urls: [
        'stun:stun.l.google.com:19302',
        'stun:stun1.l.google.com:19302'
      ]
    },
    {
      urls: `turn:${window.location.hostname}:3478`,
      username: 'lms_turn_user',
      credential: 'lms_turn_password'
    }
  ]
})

export async function requestScreenShare() {
  try {
    const stream = await navigator.mediaDevices.getDisplayMedia({
      video: {
        cursor: 'always',
        displaySurface: 'monitor',
        logicalSurface: true
      },
      audio: false
    })

    // Bắt buộc sinh viên phải chọn "Toàn bộ màn hình" (monitor), không cho chọn Tab hay Window
    const videoTrack = stream.getVideoTracks()[0]
    const settings = videoTrack.getSettings()
    
    if (settings.displaySurface && settings.displaySurface !== 'monitor') {
      // Dừng stream ngay lập tức nếu chọn sai
      stream.getTracks().forEach(track => track.stop())
      throw new Error('Bạn bắt buộc phải chọn "Toàn bộ màn hình" (Entire Screen). Việc chia sẻ Thẻ (Tab) hoặc Cửa sổ (Window) không được phép.')
    }

    return stream
  } catch (err) {
    console.error('Lỗi khi lấy quyền chia sẻ màn hình:', err)
    throw err
  }
}

export function createStudentPeerConnection(stream, onIceCandidate) {
  const pc = new RTCPeerConnection(getRtcConfig())

  // Thêm các track từ màn hình vào peer connection
  if (stream) {
    stream.getTracks().forEach((track) => {
      track.enabled = true
      pc.addTrack(track, stream)
    })
  }

  // Lắng nghe ICE candidates để gửi cho teacher
  pc.onicecandidate = (event) => {
    if (event.candidate) {
      onIceCandidate(event.candidate)
    }
  }

  return pc
}

export function createProctorPeerConnection(onIceCandidate, onTrack, onReconnectNeeded) {
  const pc = new RTCPeerConnection(getRtcConfig())
  pc.addTransceiver('video', { direction: 'recvonly' })
  let trackFired = false

  // Lắng nghe stream từ student
  pc.ontrack = (event) => {
    const track = event.track
    track.enabled = true

    const publish = () => {
      if (!trackFired) {
        trackFired = true
        const newStream = new MediaStream([track])
        onTrack(newStream)
      }
    }

    if (!track.muted) {
      publish()
    }
    track.onunmute = () => {
      publish()
    }
  }

  // Lắng nghe ICE candidates để gửi cho student
  pc.onicecandidate = (event) => {
    if (event.candidate) {
      onIceCandidate(event.candidate)
    }
  }

  pc.oniceconnectionstatechange = () => {
    if (pc.iceConnectionState === 'failed') {
      console.warn('WebRTC: ICE failed, triggering reconnect...')
      if (onReconnectNeeded) onReconnectNeeded()
    }
  }

  pc.onconnectionstatechange = () => {
    if (pc.connectionState === 'failed' || pc.connectionState === 'disconnected') {
      console.warn('WebRTC: Connection failed/disconnected, triggering reconnect...')
      if (onReconnectNeeded) onReconnectNeeded()
    }
  }

  return pc
}

export function stopScreenShare(stream) {
  if (stream) {
    stream.getTracks().forEach(track => track.stop())
  }
}
