import { defineStore } from 'pinia'
import { notificationApi } from '@/api/notificationApi'
import { createSignalRConnection } from '@/composables/useSignalR'
import { useAuthStore } from '@/stores/authStore'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    items: [],
    loading: false,
    _connection: null,
  }),

  getters: {
    unreadCount: (state) => state.items.filter(n => !n.isRead).length,
  },

  actions: {
    async fetch() {
      try {
        this.loading = true
        const result = await notificationApi.getForUser()
        this.items = result.items ?? []
      } catch {
        this.items = []
      } finally {
        this.loading = false
      }
    },

    async markRead(notifID) {
      try {
        await notificationApi.markRead(notifID)
        const item = this.items.find(n => n.notifID === notifID)
        if (item) item.isRead = true
      } catch {
        // silent
      }
    },

    async markAllRead() {
      try {
        await notificationApi.markAllRead()
        this.items.forEach(n => (n.isRead = true))
      } catch {
        // silent
      }
    },

    // ── SignalR ──────────────────────────────────────────────────────────────

    async connectSignalR() {
      if (this._connection) return

      const authStore = useAuthStore()
      const branchCode = authStore.branchCode
      const category = authStore.sectionCategory
      const sectionCode = authStore.sectionCode
      const userID = authStore.userID
      const isAdmin = authStore.isAdmin

      const conn = createSignalRConnection(
        '/hubs/notification',
        branchCode,
        () => this.fetch()
      )

      conn.on('NewNotification', (notif) => {
        // Prepend — newest first
        const exists = this.items.some(n => n.notifID === notif.notifID)
        if (!exists) this.items.unshift(notif)
      })

      await conn.start()

      // Join all relevant groups after connection
      try {
        // Category group (e.g. all receivers)
        if (category) {
          await conn.invoke('JoinCategory', branchCode, category)
        }
        // Section group (for runner section-scoped notifs)
        if (sectionCode) {
          await conn.invoke('JoinSection', branchCode, sectionCode)
        }
        // Personal group (for user-targeted notifs)
        if (userID) {
          await conn.invoke('JoinUser', userID)
        }
        // Admin gets branch-wide group
        if (isAdmin) {
          await conn.invoke('JoinBranch', branchCode)
        }
      } catch (err) {
        console.warn('[notificationStore] Group join failed:', err)
      }

      this._connection = conn

      // Initial fetch
      await this.fetch()
    },

    async disconnectSignalR() {
      if (!this._connection) return
      const authStore = useAuthStore()
      await this._connection.stop(authStore.branchCode)
      this._connection = null
    },
  },

  persist: false,
})
