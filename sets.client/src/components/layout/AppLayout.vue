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
  import { useAuthStore } from '@/stores/authStore'

  const announcementStore = useAnnouncementStore()
  const notificationStore = useNotificationStore()
  const authStore = useAuthStore()

  let poller = null

  onMounted(async () => {
    // ── Announcement ─────────────────────────────────────────────────────────
    await announcementStore.fetch()
    await announcementStore.connectSignalR(authStore.branchCode)
    poller = setInterval(() => announcementStore.fetch(), 60000)

    // ── Notifications ─────────────────────────────────────────────────────────
    await notificationStore.connectSignalR()
  })

  onUnmounted(async () => {
    clearInterval(poller)
    await announcementStore.disconnectSignalR(authStore.branchCode)
    await notificationStore.disconnectSignalR()
  })
</script>

<style>
  body {
    font-family: "Manrope", sans-serif;
    background-color: var(--color-bg);
    color: var(--color-text);
  }
</style>
