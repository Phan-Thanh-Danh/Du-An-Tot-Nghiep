import * as signalR from '@microsoft/signalr'

const getApiBaseUrl = () => {
  return (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
}

export class ExamProctoringHub {
  constructor() {
    this.connection = null
    this._token = null
    this.eventHandlers = {
      onJoinedRoom: null,
      onScreenShareStatusChanged: null,
      onViolationDetected: null,
      onStudentStatusUpdated: null,
      onWarningReceived: null,
      // WebRTC — payload giờ là DTO object (có fromConnectionId, targetConnectionId, offer/answer/candidate)
      onReceiveOffer: null,
      onReceiveAnswer: null,
      onReceiveIceCandidate: null,
      onStudentConnectionIdBroadcast: null,
      onProctorRequestedConnections: null,
      onProctorAcknowledged: null,
    }
  }

  async connect(token) {
    // Nếu đã connected thì không tạo lại
    if (
      this.connection &&
      this.connection.state === signalR.HubConnectionState.Connected
    ) {
      return this.connection
    }

    this._token = token
    const url = `${getApiBaseUrl()}/hubs/exam-monitoring`

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        // accessTokenFactory dùng cho LongPolling & SSE (header Authorization)
        // OnMessageReceived ở backend đọc ?access_token= cho WebSocket
        accessTokenFactory: () => this._token,
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.ServerSentEvents |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build()

    // ── Register events ────────────────────────────────────────────────────

    this.connection.on('JoinedRoom', (payload) => {
      if (import.meta.env.DEV) console.debug('[Hub] JoinedRoom', payload)
      this.eventHandlers.onJoinedRoom?.(payload)
    })

    this.connection.on('ScreenShareStatusChanged', (payload) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ScreenShareStatusChanged', payload)
      this.eventHandlers.onScreenShareStatusChanged?.(payload)
    })

    this.connection.on('ViolationDetected', (payload) => {
      this.eventHandlers.onViolationDetected?.(payload)
    })

    this.connection.on('StudentStatusUpdated', (payload) => {
      this.eventHandlers.onStudentStatusUpdated?.(payload)
    })

    this.connection.on('WarningReceived', (payload) => {
      this.eventHandlers.onWarningReceived?.(payload)
    })

    // WebRTC — giờ nhận DTO object thay vì hai tham số rời
    this.connection.on('ReceiveOffer', (dto) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ReceiveOffer from', dto?.fromConnectionId)
      this.eventHandlers.onReceiveOffer?.(dto)
    })

    this.connection.on('ReceiveAnswer', (dto) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ReceiveAnswer from', dto?.fromConnectionId)
      this.eventHandlers.onReceiveAnswer?.(dto)
    })

    this.connection.on('ReceiveIceCandidate', (dto) => {
      this.eventHandlers.onReceiveIceCandidate?.(dto)
    })

    this.connection.on('StudentConnectionIdBroadcast', (payload) => {
      if (import.meta.env.DEV)
        console.debug(
          '[Hub] StudentConnectionIdBroadcast',
          payload?.maHocSinh,
          payload?.connectionId
        )
      this.eventHandlers.onStudentConnectionIdBroadcast?.(payload)
    })

    this.connection.on('ProctorRequestedConnections', (payload) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ProctorRequestedConnections received', payload)
      this.eventHandlers.onProctorRequestedConnections?.(payload)
    })

    this.connection.on('ProctorAcknowledged', (payload) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ProctorAcknowledged received', payload)
      this.eventHandlers.onProctorAcknowledged?.(payload)
    })

    this.connection.on('ViolationDetected', (payload) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] ViolationDetected received', payload)
      this.eventHandlers.onViolationDetected?.(payload)
    })

    this.connection.on('StudentUnlocked', (payload) => {
      if (import.meta.env.DEV)
        console.debug('[Hub] StudentUnlocked received', payload)
      this.eventHandlers.onStudentUnlocked?.(payload)
    })

    // ── Reconnect lifecycle ────────────────────────────────────────────────

    this.connection.onreconnecting(() => {
      console.warn('[Hub] SignalR reconnecting...')
    })

    this.connection.onreconnected((connectionId) => {
      console.info('[Hub] SignalR reconnected. connectionId:', connectionId)
    })

    this.connection.onclose((error) => {
      if (error) console.error('[Hub] SignalR connection closed with error:', error)
      else console.info('[Hub] SignalR connection closed.')
    })

    // ── Start ──────────────────────────────────────────────────────────────

    try {
      await this.connection.start()
      console.info('[Hub] Connected to ExamMonitoringHub via', this.connection.transport)
    } catch (err) {
      console.error('[Hub] Error connecting to ExamMonitoringHub:', err?.message || err)
      throw err
    }

    return this.connection
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
  }

  // ── Helper ─────────────────────────────────────────────────────────────────

  get isConnected() {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  /** Connection ID hiện tại — dùng để lọc own candidate/offer. */
  get connectionId() {
    return this.connection?.connectionId ?? null
  }

  async _invoke(methodName, ...args) {
    if (!this.isConnected) {
      console.warn(`[Hub] Cannot invoke '${methodName}': not connected`)
      return
    }
    await this.connection.invoke(methodName, ...args)
  }

  // ── Room methods ───────────────────────────────────────────────────────────

  async joinExamRoom(maCaThi) {
    await this._invoke('JoinExamRoom', maCaThi)
  }

  async leaveExamRoom(maCaThi) {
    await this._invoke('LeaveExamRoom', maCaThi)
  }

  /**
   * Học sinh tham gia phòng thi và broadcast connectionId.
   * Backend: JoinAsStudent(int maCaThi, int maHocSinh)
   */
  async joinAsStudent(maCaThi, maHocSinh) {
    await this._invoke('JoinAsStudent', maCaThi, maHocSinh)
  }

  async acknowledgeStudent(studentConnectionId) {
    await this._invoke('AcknowledgeStudent', studentConnectionId)
  }

  // ── Screen share ───────────────────────────────────────────────────────────

  async screenShareStarted(maCaThi, maHocSinh) {
    await this._invoke('ScreenShareStarted', maCaThi, maHocSinh)
  }

  async screenShareStopped(maCaThi, maHocSinh) {
    await this._invoke('ScreenShareStopped', maCaThi, maHocSinh)
  }

  // ── Violation ─────────────────────────────────────────────────────────────

  async sendViolationLog(maCaThi, maHocSinh, loaiViPham, chiTiet) {
    await this._invoke('SendViolationLog', maCaThi, maHocSinh, loaiViPham, chiTiet)
  }

  // ── WebRTC signaling ───────────────────────────────────────────────────────
  // QUAN TRỌNG: Backend giờ nhận DTO object, KHÔNG nhận tham số rời.
  // Frontend gửi: connection.invoke('SendOffer', { maCaThi, maHocSinh, targetConnectionId, offer })

  /**
   * Student gửi SDP Offer tới Proctor.
   * @param {{ maCaThi: number, maHocSinh: number, targetConnectionId: string, offer: RTCSessionDescriptionInit }} payload
   */
  async sendOffer(payload) {
    if (import.meta.env.DEV)
      console.debug(
        '[Hub] sendOffer →',
        payload.maHocSinh,
        'target:',
        payload.targetConnectionId
      )
    await this._invoke('SendOffer', payload)
  }

  /**
   * Proctor gửi SDP Answer về Student.
   * @param {{ maCaThi: number, maHocSinh: number, targetConnectionId: string, answer: RTCSessionDescriptionInit }} payload
   */
  async sendAnswer(payload) {
    if (import.meta.env.DEV)
      console.debug(
        '[Hub] sendAnswer →',
        payload.maHocSinh,
        'target:',
        payload.targetConnectionId
      )
    await this._invoke('SendAnswer', payload)
  }

  /**
   * Gửi ICE Candidate (cả hai chiều).
   * candidate có thể là RTCIceCandidate object hoặc RTCIceCandidateInit plain object.
   * @param {{ maCaThi: number, maHocSinh: number, targetConnectionId: string, candidate: RTCIceCandidate|RTCIceCandidateInit }} payload
   */
  async sendIceCandidate(payload) {
    // Chuẩn hóa candidate về plain object
    const raw = payload.candidate
    const candidateInit = typeof raw?.toJSON === 'function' ? raw.toJSON() : raw
    if (!candidateInit?.candidate) return // bỏ qua end-of-candidates
    await this._invoke('SendIceCandidate', {
      maCaThi: payload.maCaThi,
      maHocSinh: payload.maHocSinh,
      targetConnectionId: payload.targetConnectionId,
      candidate: {
        candidate: candidateInit.candidate,
        sdpMid: candidateInit.sdpMid ?? null,
        sdpMLineIndex: candidateInit.sdpMLineIndex ?? 0,
        usernameFragment: candidateInit.usernameFragment ?? null,
      },
    })
  }

  // ── Warning ────────────────────────────────────────────────────────────────

  async sendWarningToStudent(studentConnectionId, message) {
    await this._invoke('SendWarningToStudent', studentConnectionId, message)
  }

  async unlockStudent(studentConnectionId) {
    await this._invoke('UnlockStudent', studentConnectionId)
  }
}

export const examProctoringHub = new ExamProctoringHub()
