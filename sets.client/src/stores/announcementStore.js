import { defineStore } from 'pinia'
import { announcementApi } from '@/api/announcementApi'

export const useAnnouncementStore = defineStore('announcement', {
  state: () => ({
    announcement: null,   // active AnnouncementDto or null
    dismissed: false,     // per-session dismiss flag, resets on logout/login
    loading: false,
  }),

  getters: {
    // Returns true if there is an active announcement visible to this user
    isVisible: (state) => {
      return !!state.announcement && !state.dismissed
    },
  },

  actions: {
    async fetch() {
      try {
        this.loading = true
        const data = await announcementApi.getActive()
        this.announcement = data ?? null
        // If the announcement changed (new one posted), reset dismiss so it shows again
      } catch {
        this.announcement = null
      } finally {
        this.loading = false
      }
    },

    dismiss() {
      this.dismissed = true
    },

    // Called on login — resets dismiss so announcement shows fresh each session
    reset() {
      this.announcement = null
      this.dismissed = false
    },
  },

  // Do NOT persist — dismiss resets on every login as per requirement
  persist: false,
})
