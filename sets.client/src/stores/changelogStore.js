import { defineStore } from 'pinia'
import { changelogApi } from '@/api/changelogApi'
import { authApi } from '@/api/authApi'

export const useChangelogStore = defineStore('changelog', {
  state: () => ({
    latestEntry: null,
    modalVisible: false,
  }),

  actions: {
    async checkAndShow() {
      try {
        // Fetch version fresh — avoids stale persisted value
        const versionRes = await authApi.getVersion()
        const appVersion = versionRes.data?.version
        if (!appVersion || appVersion === '...') return

        const { exists, entry } = await changelogApi.getLatest()
        if (!exists || !entry) return

        if (entry.version !== appVersion) return

        const { seen } = await changelogApi.hasSeen(entry.version)
        if (seen) return

        this.latestEntry = entry
        this.modalVisible = true
      } catch (err) {
        console.warn('[changelog] checkAndShow failed:', err)
      }
    },

    async dismiss() {
      if (!this.latestEntry) {
        this.modalVisible = false
        return
      }
      try {
        await changelogApi.markSeen(this.latestEntry.version)
      } catch {
        // close regardless
      } finally {
        this.modalVisible = false
      }
    },

    reset() {
      this.latestEntry = null
      this.modalVisible = false
    },
  },

  persist: false,
})
