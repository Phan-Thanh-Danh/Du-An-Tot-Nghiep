export const RTC_CONFIG = {
  iceServers: [
    {
      urls: [
        'stun:stun.l.google.com:19302',
        'stun:stun1.l.google.com:19302'
      ]
    }
  ]
}

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
  const pc = new RTCPeerConnection(RTC_CONFIG)

  // Thêm các track từ màn hình vào peer connection
  stream.getTracks().forEach((track) => {
    pc.addTrack(track, stream)
  })

  // Lắng nghe ICE candidates để gửi cho teacher
  pc.onicecandidate = (event) => {
    if (event.candidate) {
      onIceCandidate(event.candidate)
    }
  }

  return pc
}

export function createProctorPeerConnection(onIceCandidate, onTrack) {
  const pc = new RTCPeerConnection(RTC_CONFIG)

  // Lắng nghe stream từ student
  pc.ontrack = (event) => {
    if (event.streams && event.streams[0]) {
      onTrack(event.streams[0])
    }
  }

  // Lắng nghe ICE candidates để gửi cho student
  pc.onicecandidate = (event) => {
    if (event.candidate) {
      onIceCandidate(event.candidate)
    }
  }

  return pc
}

export function stopScreenShare(stream) {
  if (stream) {
    stream.getTracks().forEach(track => track.stop())
  }
}
