<template>
  <aside class="h-screen w-64 fixed left-0 top-0 flex flex-col z-40"
         style="background-color: var(--color-surface); border-right: 1px solid var(--color-border);">

    <!-- Logo -->
    <div class="px-6 py-5 flex items-center gap-3"
         style="border-bottom: 1px solid var(--color-border);">
      <img src="/SETSLOGO.png" alt="SETS Logo" class="w-10 h-10 object-contain" />
      <div>
        <p class="font-extrabold tracking-tighter text-lg" style="color: var(--color-primary);">SETS</p>
        <p class="text-[9px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
          Specimen Endorsement & Tracking System
        </p>
      </div>
    </div>

    <!-- Section Info (regular/TL) or Admin Badge -->
    <div class="px-4 py-4">
      <!-- Regular / Team Lead -->
      <div v-if="!authStore.isAdmin"
           class="rounded-xl p-3"
           style="background-color: var(--color-surface-low);">
        <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">
          Current Section
        </p>
        <p class="font-bold text-sm" style="color: var(--color-text);">{{ authStore.sectionName }}</p>
        <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
          {{ authStore.branchCode }} · {{ authStore.sectionCode }}
        </p>
      </div>

      <!-- Admin -->
      <div v-else
           class="rounded-xl p-3 flex items-center gap-3"
           style="background-color: var(--color-primary-soft);">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">
          admin_panel_settings
        </span>
        <div>
          <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary);">
            Admin Mode
          </p>
          <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
            Full system access
          </p>
        </div>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-4 py-2 overflow-y-auto space-y-1">

      <!-- Regular / Team Lead: flat nav -->
      <template v-if="!authStore.isAdmin">
        <router-link v-for="item in visibleNavItems"
                     :key="item.name"
                     :to="item.path"
                     class="flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all duration-200"
                     :style="isActive(item.path) ? activeStyle : inactiveStyle"
                     @mouseenter="(e) => { if (!isActive(item.path)) applyHover(e) }"
                     @mouseleave="(e) => { if (!isActive(item.path)) removeHover(e) }">
          <span class="material-symbols-outlined text-lg">{{ item.icon }}</span>
          <span>{{ item.name }}</span>
          <div v-if="isActive(item.path)"
               class="ml-auto w-1.5 h-1.5 rounded-full"
               style="background-color: var(--color-primary);"></div>
        </router-link>
      </template>

      <!-- Admin: grouped by category -->
      <template v-else>
        <div v-for="group in adminNavGroups" :key="group.categoryCode" class="mb-2">

          <!-- Group Header -->
          <button class="w-full flex items-center justify-between px-3 py-2 rounded-xl transition-all duration-200"
                  style="color: var(--color-text-muted);"
                  @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                  @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                  @click="uiStore.toggleCategory(group.categoryCode)">
            <div class="flex items-center gap-2">
              <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">
                {{ group.icon }}
              </span>
              <span class="text-[10px] font-bold uppercase tracking-widest">{{ group.label }}</span>
            </div>
            <span class="material-symbols-outlined text-sm transition-transform duration-200"
                  :style="uiStore.sidebarCollapsed[group.categoryCode] ? 'transform: rotate(-90deg)' : ''">
              expand_more
            </span>
          </button>

          <!-- Group Items -->
          <div v-show="!uiStore.sidebarCollapsed[group.categoryCode]"
               class="mt-1 space-y-1 pl-2">
            <router-link v-for="item in group.items"
                         :key="item.name"
                         :to="item.path"
                         class="flex items-center gap-3 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all duration-200"
                         :style="isActive(item.path) ? activeStyle : inactiveStyle"
                         @mouseenter="(e) => { if (!isActive(item.path)) applyHover(e) }"
                         @mouseleave="(e) => { if (!isActive(item.path)) removeHover(e) }">
              <span class="material-symbols-outlined text-base">{{ item.icon }}</span>
              <span>{{ item.name }}</span>
              <div v-if="isActive(item.path)"
                   class="ml-auto w-1.5 h-1.5 rounded-full"
                   style="background-color: var(--color-primary);"></div>
            </router-link>
          </div>

        </div>
      </template>

    </nav>

    <!-- New Endorsement Button (Endorser only, or Admin) -->
    <div v-if="authStore.isEndorser || authStore.isAdmin"
         class="px-4 pb-6 pt-4"
         style="border-top: 1px solid var(--color-border);">
      <router-link to="/endorsement/new">
        <button class="w-full py-4 rounded-xl font-bold text-xs uppercase tracking-widest flex items-center justify-center gap-2 transition-all active:scale-[0.98] shadow-lg"
                style="background: var(--color-primary-gradient); color: #ffffff;">
          <span class="material-symbols-outlined text-lg">add_circle</span>
          New Endorsement
        </button>
      </router-link>
    </div>

  </aside>
</template>

<script setup>
  import { computed } from 'vue'
  import { useRoute } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import { useUiStore } from '@/stores/uiStore'

  const route = useRoute()
  const authStore = useAuthStore()
  const uiStore = useUiStore()

  // ── Nav item definitions per category ──────────────────────────────────────

  const endorserItems = [
    { name: 'Dashboard', path: '/dashboard', icon: 'dashboard' },
    { name: 'Batch Monitoring', path: '/batch-monitoring', icon: 'monitoring' },
    { name: 'Audit Trail', path: '/audit-trail', icon: 'manage_search' },
  ]

  const endorserRestrictedItems = [
    { name: 'Reports', path: '/reports', icon: 'description' },
    { name: 'Settings', path: '/settings', icon: 'settings' },
  ]

  const receiverItems = [
    { name: 'Dashboard', path: '/receiver/dashboard', icon: 'dashboard' },
    { name: 'Receive Specimens', path: '/receiver/receive', icon: 'inbox' },
    { name: 'Incoming Specimens', path: '/receiver/incoming', icon: 'move_to_inbox' },
    { name: 'Audit Trail', path: '/receiver/audit-trail', icon: 'manage_search' },
    { name: 'Contingency', path: '/receiver/contingency', icon: 'offline_bolt' },
  ]

  const receiverRestrictedItems = [
    { name: 'Reports', path: '/receiver/reports', icon: 'description' },
    { name: 'Settings', path: '/receiver/settings', icon: 'settings' },
  ]

  const runnerItems = [
    { name: 'Dashboard', path: '/runner/dashboard', icon: 'dashboard' },
    { name: 'Pending Specimens', path: '/runner/pending', icon: 'pending_actions' },
    { name: 'Saved Specimens', path: '/runner/saved', icon: 'bookmark' },
    { name: 'Assign RMT', path: '/runner/assign', icon: 'assignment_ind' },
    { name: 'Audit Trail', path: '/runner/audit-trail', icon: 'manage_search' },
  ]

  const runnerRestrictedItems = [
    { name: 'Reports', path: '/runner/reports', icon: 'description' },
    { name: 'Settings', path: '/runner/settings', icon: 'settings' },
  ]

  // ── Computed: visible items for regular/TL user ────────────────────────────

  const isTLorAdmin = computed(() => authStore.isAdmin || authStore.roleID === 2)

  const visibleNavItems = computed(() => {
    let base = []
    let restricted = []

    if (authStore.isEndorser) {
      base = endorserItems
      restricted = endorserRestrictedItems
    } else if (authStore.isReceiver) {
      base = receiverItems
      restricted = receiverRestrictedItems
    } else if (authStore.isRunner) {
      base = runnerItems
      restricted = runnerRestrictedItems
    }

    return isTLorAdmin.value ? [...base, ...restricted] : base
  })

  // ── Computed: grouped nav for admin ────────────────────────────────────────

  const adminNavGroups = computed(() => [
    {
      categoryCode: '1',
      label: 'PHLEBO / SEND-IN',
      icon: 'vaccines',
      items: [...endorserItems, ...endorserRestrictedItems],
    },
    {
      categoryCode: '2',
      label: 'PROCESSING',
      icon: 'labs',
      items: [...receiverItems, ...receiverRestrictedItems],
    },
    {
      categoryCode: '3',
      label: 'LABORATORY',
      icon: 'science',
      items: [...runnerItems, ...runnerRestrictedItems],
    },
  ])

  // ── Helpers ────────────────────────────────────────────────────────────────

  function isActive(path) {
    return route.path === path
  }

  const activeStyle = 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid var(--color-primary); padding-left: calc(1rem - 3px);'
  const inactiveStyle = 'color: var(--color-text-muted); border-left: 3px solid transparent; padding-left: calc(1rem - 3px);'

  function applyHover(e) {
    e.currentTarget.style.cssText = 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid transparent; padding-left: calc(1rem - 3px); transform: translateX(4px);'
  }
  function removeHover(e) {
    e.currentTarget.style.cssText = inactiveStyle
  }
</script>
