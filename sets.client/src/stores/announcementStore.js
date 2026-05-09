import { defineStore } from 'pinia'
import { announcementApi } from '@/api/announcementApi'
import { createSignalRConnection } from '@/composables/useSignalR'

export const useAnnouncementStore = defineStore('announcement', {
  state: () => ({
    announcement: null,
    dismissed: false,
    loading: false,
    _connection: null,
  }),

  getters: {
    isVisible: (state) => !!state.announcement && !state.dismissed,
  },

  actions: {
    async fetch() {
      try {
        this.loading = true
        const data = await announcementApi.getActive()
        this.announcement = data ?? null
      } catch {
        this.announcement = null
      } finally {
        this.loading = false
      }
    },

    dismiss() {
      this.dismissed = true
    },

    reset() {
      this.announcement = null
      this.dismissed = false
    },

    // ── SignalR ──────────────────────────────────────────────────────────────

    async connectSignalR(branchCode) {
      if (this._connection) return

      const conn = createSignalRConnection('/hubs/announcement', branchCode, () => this.fetch())

      conn.on('AnnouncementUpdated', (data) => {
        this.announcement = data ?? null
        this.dismissed = false
      })

      await conn.start()
      this._connection = conn
    },

    async disconnectSignalR(branchCode) {
      if (!this._connection) return
      await this._connection.stop(branchCode)
      this._connection = null
    },
  },

  persist: false,
})
