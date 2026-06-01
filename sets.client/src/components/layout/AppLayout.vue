<template>
  <div class="min-h-screen" style="background-color: var(--color-bg);">
    <AppSidebar />
    <AppTopbar />
    <div style="margin-left: 256px;">
      <div class="pt-16">
        <AnnouncementBanner />
      </div>
      <main>
        <div class="p-8">
          <slot />
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
  import { onMounted, onUnmounted } from 'vue'
  import AppSidebar from './AppSidebar.vue'
  import AppTopbar from './AppTopbar.vue'
  import AnnouncementBanner from '@/components/common/AnnouncementBanner.vue'
  import { useAnnouncementStore } from '@/stores/announcementStore'
  import { useNotificationStore } from '@/stores/notificationStore'
  import { usePresenceStore } from '@/stores/presenceStore'
  import { useAuthStore } from '@/stores/authStore'

  const announcementStore = useAnnouncementStore()
  const notificationStore = useNotificationStore()
  const presenceStore = usePresenceStore()
  const authStore = useAuthStore()

  let poller = null

  onMounted(async () => {
    // ── Announcement ─────────────────────────────────────────────────────────
    await announcementStore.fetch()
    await announcementStore.connectSignalR(authStore.branchCode)
    poller = setInterval(() => announcementStore.fetch(), 60000)

    // ── Notifications ─────────────────────────────────────────────────────────
    await notificationStore.connectSignalR()

    // ── Presence ──────────────────────────────────────────────────────────────
    await presenceStore.connectSignalR()
  })

  onUnmounted(async () => {
    clearInterval(poller)
    await announcementStore.disconnectSignalR(authStore.branchCode)
    await notificationStore.disconnectSignalR()
    await presenceStore.disconnectSignalR()
  })
</script>

<style>
  body {
    font-family: "Manrope", sans-serif;
    background-color: var(--color-bg);
    color: var(--color-text);
  }
</style>
