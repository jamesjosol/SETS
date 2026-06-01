import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '@/stores/authStore'

export const usePresenceStore = defineStore('presence', {
  state: () => ({
    onlineUsers: [],   // populated for admin sessions only
    hubStatus: 'disconnected',
    _connection: null,
  }),

  actions: {
    // ── Connect ──────────────────────────────────────────────────────────────
    // Called by AppLayout.onMounted — runs for every authenticated user.
    async connectSignalR() {
      if (this._connection) return

      const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/presence')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      // ── Status tracking ──────────────────────────────────────────────────
      connection.onreconnecting(() => {
        this.hubStatus = 'reconnecting'
      })

      connection.onreconnected(async () => {
        this.hubStatus = 'connected'
        // Re-register identity after transport restores — group membership is lost
        await this._register(connection)
      })

      connection.onclose(() => {
        this.hubStatus = 'disconnected'
      })

      // ── Event listener ───────────────────────────────────────────────────
      // All connections receive PresenceUpdated, but only admin stores the list.
      connection.on('PresenceUpdated', (list) => {
        if (useAuthStore().isAdmin) {
          this.onlineUsers = list ?? []
        }
      })

      // ── Start ────────────────────────────────────────────────────────────
      this.hubStatus = 'connecting'
      try {
        await connection.start()
        this.hubStatus = 'connected'
        this._connection = connection
        await this._register(connection)
      } catch (err) {
        this.hubStatus = 'disconnected'
        this._connection = null
        console.error('[presenceStore] Failed to connect:', err)
      }
    },

    // ── Register self ────────────────────────────────────────────────────────
    async _register(connection) {
      const authStore = useAuthStore()
      try {
        await connection.invoke(
          'Register',
          authStore.userID,
          authStore.userName,
          authStore.branchCode,
          authStore.sectionCode,
          authStore.sectionName,
          authStore.sectionCategory ?? '',
          authStore.isAdmin,
          authStore.profilePicture ?? null,   // base64 or null
        )
      } catch (err) {
        console.warn('[presenceStore] Register invoke failed:', err)
      }
    },

    // ── Disconnect ───────────────────────────────────────────────────────────
    // Called by AppLayout.onUnmounted — fires on logout / session end.
    async disconnectSignalR() {
      if (!this._connection) return
      const conn = this._connection
      this._connection = null

      const authStore = useAuthStore()
      try {
        if (conn.state === signalR.HubConnectionState.Connected) {
          await conn.invoke('Unregister', authStore.branchCode)
        }
      } catch { /* hub's OnDisconnectedAsync is the fallback */ }

      try {
        await conn.stop()
      } catch { /* ignore */ }

      this.onlineUsers = []
      this.hubStatus = 'disconnected'
    },
  },

  persist: false,
})
