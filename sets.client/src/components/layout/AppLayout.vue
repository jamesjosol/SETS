<template>
  <div class="min-h-screen" style="background-color: var(--color-bg);">
    <AppSidebar />
    <AppTopbar />

    <div style="margin-left: 256px;">
      <!-- Banner sits right below the fixed topbar -->
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
  import AppSidebar from './AppSidebar.vue'
  import AppTopbar from './AppTopbar.vue'
  import AnnouncementBanner from '@/components/common/AnnouncementBanner.vue'
  import { useAnnouncementStore } from '@/stores/announcementStore'

  const store = useAnnouncementStore()

  import { onMounted, onUnmounted } from 'vue'

  let poller = null
  onMounted(() => {
    store.fetch()
    poller = setInterval(() => store.fetch(), 60000)
  })
  onUnmounted(() => clearInterval(poller))
</script>

<style>
  body {
    font-family: "Manrope", sans-serif;
    background-color: var(--color-bg);
    color: var(--color-text);
  }
</style>
