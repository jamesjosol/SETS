<template>
  <nav class="fixed top-0 right-0 z-50 flex items-center justify-between h-16 px-6"
       style="left: 256px; background: var(--color-bg); backdrop-filter: blur(12px); -webkit-backdrop-filter: blur(12px); border-bottom: 1px solid var(--color-border);">

    <!-- ── Search ─────────────────────────────────────────────────────── -->
    <div class="relative hidden md:block" ref="searchWrapRef">

      <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none"
            style="color: var(--color-text-muted); font-size: 18px;">search</span>

      <input ref="searchInputRef"
             v-model="searchQuery"
             type="text"
             placeholder="Search specimens or patient IDs..."
             class="border-none outline-none rounded-full py-2 pl-10 pr-10 text-sm w-80 transition-colors"
             style="background-color: var(--color-surface-high); color: var(--color-text);"
             @focus="onFocus"
             @blur="(e) => (e.target.style.backgroundColor = 'var(--color-surface-high)')"
             @keydown="onKeydown" />

      <!-- Spinner / Clear -->
      <div class="absolute right-3 top-1/2 -translate-y-1/2 flex items-center">
        <span v-if="isLoading"
              class="material-symbols-outlined animate-spin"
              style="color: var(--color-text-muted); font-size: 16px;">progress_activity</span>
        <button v-else-if="searchQuery"
                class="material-symbols-outlined transition-colors"
                style="color: var(--color-text-muted); font-size: 16px; background: none; border: none; cursor: pointer; padding: 0;"
                @mousedown.prevent="clearSearch">
          close
        </button>
      </div>

      <!-- ── Dropdown ──────────────────────────────────────────────────── -->
      <Transition name="dropdown">
        <div v-if="dropdownVisible"
             class="absolute top-[calc(100%+8px)] left-0 w-[480px] rounded-2xl shadow-2xl border overflow-hidden"
             style="background-color: var(--color-surface); border-color: var(--color-border); z-index: 100;">

          <!-- Loading skeleton -->
          <div v-if="isLoading" class="p-4 flex flex-col gap-2">
            <div v-for="i in 3" :key="i"
                 class="h-10 rounded-xl animate-pulse"
                 style="background-color: var(--color-surface-high);"></div>
          </div>

          <!-- No results -->
          <div v-else-if="results.length === 0"
               class="px-5 py-8 text-center">
            <span class="material-symbols-outlined text-3xl block mb-2"
                  style="color: var(--color-text-muted);">manage_search</span>
            <p class="text-sm font-bold" style="color: var(--color-text-muted);">No results found</p>
            <p class="text-xs mt-1" style="color: var(--color-text-muted);">
              Try a specimen no., lab no., patient name, or batch no.
            </p>
          </div>

          <!-- Results -->
          <template v-else>

            <!-- Specimens group -->
            <div v-if="specimenResults.length">
              <div class="px-4 pt-3 pb-1.5 flex items-center gap-2">
                <span class="material-symbols-outlined" style="color: var(--color-primary); font-size: 14px;">biotech</span>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimens</p>
              </div>
              <button v-for="(item, idx) in specimenResults"
                      :key="item.specimenNo + idx"
                      class="w-full flex items-center gap-3 px-4 py-3 transition-all text-left"
                      :style="focusedIndex === globalIndex(idx, 'specimen')
                        ? 'background-color: var(--color-surface-high);'
                        : 'background-color: transparent;'"
                      @mouseenter="focusedIndex = globalIndex(idx, 'specimen')"
                      @mouseleave="focusedIndex = -1"
                      @mousedown.prevent="navigate(item)">

                <span class="w-2 h-2 rounded-full flex-shrink-0 mt-0.5"
                      :style="statusDotStyle(item.status)"></span>

                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 flex-wrap">
                    <span class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</span>
                    <span v-if="item.labNo"
                          class="text-[10px] font-mono px-1.5 py-0.5 rounded"
                          style="background-color: var(--color-surface-high); color: var(--color-text-muted);">
                      {{ item.labNo }}
                    </span>
                    <span class="text-[10px] px-2 py-0.5 rounded-full font-bold"
                          :style="statusBadgeStyle(item.status)">
                      {{ statusLabel(item.status) }}
                    </span>
                  </div>
                  <div class="flex items-center gap-2 mt-0.5">
                    <span class="text-xs font-semibold truncate" style="color: var(--color-text);">{{ item.patientName || '—' }}</span>
                    <span v-if="item.pid" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.pid }}</span>
                  </div>
                </div>

                <div class="text-right flex-shrink-0">
                  <p class="text-[10px] font-bold" style="color: var(--color-text-muted);">{{ item.batchNo }}</p>
                  <p class="text-[10px]" style="color: var(--color-text-muted);">{{ item.sampleTypeName }}</p>
                </div>

              </button>
            </div>

            <!-- Divider between groups -->
            <div v-if="specimenResults.length && batchResults.length"
                 style="border-top: 1px solid var(--color-border);"></div>

            <!-- Batches group -->
            <div v-if="batchResults.length">
              <div class="px-4 pt-3 pb-1.5 flex items-center gap-2">
                <span class="material-symbols-outlined" style="color: var(--color-primary); font-size: 14px;">package_2</span>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Batches</p>
              </div>
              <button v-for="(item, idx) in batchResults"
                      :key="item.batchNo + idx"
                      class="w-full flex items-center gap-3 px-4 py-3 transition-all text-left"
                      :style="focusedIndex === globalIndex(idx, 'batch')
                        ? 'background-color: var(--color-surface-high);'
                        : 'background-color: transparent;'"
                      @mouseenter="focusedIndex = globalIndex(idx, 'batch')"
                      @mouseleave="focusedIndex = -1"
                      @mousedown.prevent="navigate(item)">

                <span class="w-2 h-2 rounded-full flex-shrink-0 mt-0.5"
                      :style="statusDotStyle(item.status)"></span>

                <div class="flex-1 min-w-0">
                  <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.batchNo }}</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ item.location }}</p>
                </div>

                <div class="text-right flex-shrink-0">
                  <span class="text-[10px] px-2 py-0.5 rounded-full font-bold"
                        :style="statusBadgeStyle(item.status)">
                    {{ statusLabel(item.status) }}
                  </span>
                  <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ formatDate(item.endorsed) }}</p>
                </div>

              </button>
            </div>

            <!-- Footer hint -->
            <div class="px-4 py-2.5 flex items-center gap-3"
                 style="border-top: 1px solid var(--color-border); background-color: var(--color-surface-low, var(--color-surface));">
              <span class="text-[10px]" style="color: var(--color-text-muted);">
                <kbd class="px-1 py-0.5 rounded text-[9px] font-bold"
                     style="background-color: var(--color-surface-high); color: var(--color-text-muted);">↑↓</kbd>
                navigate &nbsp;
                <kbd class="px-1 py-0.5 rounded text-[9px] font-bold"
                     style="background-color: var(--color-surface-high); color: var(--color-text-muted);">Enter</kbd>
                select &nbsp;
                <kbd class="px-1 py-0.5 rounded text-[9px] font-bold"
                     style="background-color: var(--color-surface-high); color: var(--color-text-muted);">Esc</kbd>
                close
              </span>
              <span class="ml-auto text-[10px] font-bold" style="color: var(--color-text-muted);">
                {{ results.length }} result{{ results.length !== 1 ? 's' : '' }}
              </span>
            </div>

          </template>
        </div>
      </Transition>
    </div>

    <!-- ── Right Side ──────────────────────────────────────────────────── -->
    <div class="flex items-center gap-4 ml-auto">

      <!-- Notifications -->
      <div ref="notifRef" class="relative">
        <button class="relative p-2 rounded-full transition-colors"
                style="color: var(--color-text-muted);"
                @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-high)')"
                @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                @click.stop="toggleNotif">
          <span class="material-symbols-outlined">notifications</span>
          <span v-if="notifCount > 0"
                class="absolute top-1 right-1 w-4 h-4 rounded-full text-[9px] font-bold flex items-center justify-center text-white"
                style="background-color: var(--color-error);">
            {{ notifCount > 99 ? '99+' : notifCount }}
          </span>
        </button>

        <!-- Notification Dropdown -->
        <Teleport to="body">
          <Transition name="notif-drop">
            <div v-if="notifOpen"
                 class="fixed z-50 rounded-2xl overflow-hidden"
                 style="
             width: 360px;
             top: 56px;
             right: 16px;
             background-color: var(--color-surface);
             box-shadow: 0 8px 32px rgba(0,0,0,0.2);
             border: 1px solid var(--color-border);
           ">

              <!-- Header -->
              <div class="px-5 py-4 flex items-center justify-between"
                   style="border-bottom: 1px solid var(--color-border);">
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text);">Notifications</p>
                  <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
                    {{ notifCount }} unread
                  </p>
                </div>
                <button v-if="notifStore.items.length > 0"
                        class="text-[10px] font-bold uppercase tracking-widest px-3 py-1.5 rounded-lg transition-all"
                        style="color: var(--color-primary); background-color: var(--color-primary-soft);"
                        @click="notifStore.markAllRead()">
                  Mark all read
                </button>
              </div>

              <!-- List -->
              <div class="overflow-y-auto" style="max-height: 420px;">

                <!-- Loading -->
                <div v-if="notifStore.loading"
                     class="flex items-center justify-center py-10">
                  <span class="material-symbols-outlined animate-spin text-2xl"
                        style="color: var(--color-text-muted);">progress_activity</span>
                </div>

                <!-- Empty -->
                <div v-else-if="notifStore.items.length === 0"
                     class="flex flex-col items-center justify-center py-10 gap-2">
                  <span class="material-symbols-outlined text-3xl"
                        style="color: var(--color-text-muted);">notifications_off</span>
                  <p class="text-xs" style="color: var(--color-text-muted);">No notifications</p>
                </div>

                <!-- Items -->
                <template v-else>
                  <button v-for="notif in notifStore.items"
                          :key="notif.notifID"
                          class="w-full flex items-start gap-3 px-5 py-3.5 transition-all text-left cursor-pointer"
                          :style="!notif.isRead
                      ? 'background-color: var(--color-primary-soft);'
                      : 'background-color: transparent;'"
                          @mouseenter="(e) => { if (notif.isRead) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                          @mouseleave="(e) => { if (notif.isRead) e.currentTarget.style.backgroundColor = 'transparent' }"
                          @click="handleNotifClick(notif)">

                    <!-- Icon -->
                    <div class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0 mt-0.5"
                         style="background-color: var(--color-surface-low);">
                      <span class="material-symbols-outlined text-base"
                            :style="`color: ${notifIconColor(notif.notifType)}; font-size: 16px;`">
                        {{ notifIcon(notif.notifType) }}
                      </span>
                    </div>

                    <!-- Content -->
                    <div class="flex-1 min-w-0">
                      <div class="flex items-start justify-between gap-2">
                        <p class="text-xs font-bold leading-snug"
                           style="color: var(--color-text);">
                          {{ notif.title }}
                        </p>
                        <span class="text-[10px] flex-shrink-0 mt-0.5"
                              style="color: var(--color-text-muted);">
                          {{ formatNotifTime(notif.createdAt) }}
                        </span>
                      </div>
                      <p class="text-[11px] mt-0.5 leading-relaxed"
                         style="color: var(--color-text-muted);">
                        {{ notif.message }}
                      </p>
                    </div>

                    <!-- Unread dot -->
                    <div v-if="!notif.isRead"
                         class="w-2 h-2 rounded-full flex-shrink-0 mt-2"
                         style="background-color: var(--color-primary);"></div>

                  </button>
                </template>
              </div>

              <!-- Footer -->
              <div v-if="notifStore.items.length > 0"
                   class="px-5 py-3 text-center"
                   style="border-top: 1px solid var(--color-border);">
                <p class="text-[10px]" style="color: var(--color-text-muted);">
                  Showing last {{ notifStore.items.length }} notification{{ notifStore.items.length !== 1 ? 's' : '' }}
                </p>
              </div>

            </div>
          </Transition>
        </Teleport>
      </div>

      <!-- Divider -->
      <div class="h-8 w-px" style="background-color: var(--color-border-strong);"></div>

      <!-- User Info -->
      <div ref="profileRef" class="relative flex items-center gap-3">
        <div class="text-right">
          <p class="text-sm font-bold" style="color: var(--color-text);">{{ authStore.userName }}</p>
          <p class="text-[10px] uppercase tracking-widest font-bold" style="color: var(--color-text-muted);">
            {{ authStore.isAdmin ? "Administrator" : roleLabel }}
          </p>
        </div>

        <!-- Avatar Button -->
        <button class="w-10 h-10 rounded-full flex items-center justify-center font-bold text-sm transition-all active:scale-95 overflow-hidden"
                style="background: var(--color-primary-gradient); color: #ffffff;"
                @click.stop="toggleDropdown">
          <img v-if="authStore.profilePicture"
               :src="authStore.profilePicture"
               class="w-full h-full object-cover" />
          <span v-else>{{ userInitials }}</span>
        </button>

        <!-- Dropdown — v-show so GSAP can animate in/out -->
        <div v-show="dropdownOpen"
             ref="dropdownRef"
             class="absolute right-0 top-12 w-64 rounded-2xl shadow-xl border z-50"
             style="background-color: var(--color-surface); border-color: var(--color-border); transform-origin: top right;">

          <div class="px-5 py-4" style="border-bottom: 1px solid var(--color-border);">
            <p class="font-bold text-sm" style="color: var(--color-text);">{{ authStore.userName }}</p>
            <p class="text-[10px] uppercase tracking-widest font-bold mt-0.5" style="color: var(--color-text-muted);">
              {{ authStore.sectionName }}
            </p>
            <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
              {{ authStore.branchCode }} · {{ authStore.sectionCode }}
            </p>
          </div>

          <div class="p-2">

            <button class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all text-left"
                    style="color: var(--color-text-muted);"
                    @click="goToProfile"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')">
              <span class="material-symbols-outlined text-lg">manage_accounts</span>
              My Profile
            </button>

            <!-- Theme Switcher -->
            <div class="px-4 py-3" style="border-top: 1px solid var(--color-border); border-bottom: 1px solid var(--color-border);">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Mode</p>
              <div class="flex items-center gap-2 mb-3">
                <button v-for="t in themes"
                        :key="t.value"
                        class="flex-1 py-2 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="authStore.theme === t.value
                          ? 'background-color: var(--color-primary); color: #ffffff;'
                          : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                        @click="handleThemeChange(t.value)">
                  {{ t.label }}
                </button>
              </div>

              <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Accent</p>
              <div class="flex items-center gap-2">
                <button v-for="a in accents"
                        :key="a.value"
                        class="flex-1 h-7 rounded-xl transition-all relative"
                        :style="`background: ${a.gradient};`"
                        :title="a.label"
                        @click="handleAccentChange(a.value)">
                  <span v-if="authStore.accentColor === a.value"
                        class="material-symbols-outlined text-white absolute inset-0 flex items-center justify-center"
                        style="font-size: 14px; display: flex; align-items: center; justify-content: center;">
                    check
                  </span>
                </button>
              </div>
            </div>

            <button class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all text-left"
                    style="color: var(--color-error);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-error-soft)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="handleLogout">
              <span class="material-symbols-outlined text-lg">logout</span>
              Log Out
            </button>

          </div>
        </div>
      </div>
    </div>

  </nav>
</template>

<script setup>
  import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import NProgress from 'nprogress'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/authStore'
  import { useNotificationStore } from '@/stores/notificationStore'
  import { useTheme } from '@/composables/useTheme'
  import { batchApi } from '@/api/batchApi'
  import { themeApi } from '@/api/themeApi'
  import { authApi } from '@/api/authApi'


  const { applyTheme, applyAccent } = useTheme()
  const authStore = useAuthStore()
  const router = useRouter()

  // ── Search state ───────────────────────────────────────────────────────────
  const searchQuery = ref('')
  const results = ref([])
  const isLoading = ref(false)
  const dropdownVisible = ref(false)
  const focusedIndex = ref(-1)
  const searchInputRef = ref(null)
  const searchWrapRef = ref(null)
  let debounceTimer = null

  const specimenResults = computed(() => results.value.filter(r => r.type === 'specimen'))
  const batchResults = computed(() => results.value.filter(r => r.type === 'batch'))

  function goToProfile() {
    closeDropdown()
    router.push({ name: 'MyProfile' })
  }

  function globalIndex(localIdx, group) {
    return group === 'specimen' ? localIdx : specimenResults.value.length + localIdx
  }

  const flatResults = computed(() => [...specimenResults.value, ...batchResults.value])

  watch(searchQuery, (val) => {
    clearTimeout(debounceTimer)
    focusedIndex.value = -1

    if (val.trim().length < 3) {
      results.value = []
      dropdownVisible.value = false
      isLoading.value = false
      return
    }

    isLoading.value = true
    dropdownVisible.value = true

    debounceTimer = setTimeout(async () => {
      try {
        results.value = await batchApi.searchGlobal(val.trim())
      } catch {
        results.value = []
      } finally {
        isLoading.value = false
      }
    }, 350)
  })

  function onFocus(e) {
    e.target.style.backgroundColor = 'var(--color-surface-highest, var(--color-surface-high))'
    if (searchQuery.value.trim().length >= 3 && results.value.length > 0) {
      dropdownVisible.value = true
    }
  }

  function clearSearch() {
    searchQuery.value = ''
    results.value = []
    dropdownVisible.value = false
    focusedIndex.value = -1
    searchInputRef.value?.focus()
  }

  function onKeydown(e) {
    if (!dropdownVisible.value) return
    const total = flatResults.value.length
    if (total === 0) return

    if (e.key === 'ArrowDown') {
      e.preventDefault()
      focusedIndex.value = (focusedIndex.value + 1) % total
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      focusedIndex.value = (focusedIndex.value - 1 + total) % total
    } else if (e.key === 'Enter') {
      e.preventDefault()
      if (focusedIndex.value >= 0) navigate(flatResults.value[focusedIndex.value])
    } else if (e.key === 'Escape') {
      dropdownVisible.value = false
      focusedIndex.value = -1
      searchInputRef.value?.blur()
    }
  }

  function onClickOutside(e) {
    if (searchWrapRef.value && !searchWrapRef.value.contains(e.target)) {
      dropdownVisible.value = false
      focusedIndex.value = -1
    }
  }

  onMounted(() => {
    document.addEventListener('mousedown', onClickOutside)
    document.addEventListener('click', handleNotifOutside)
    document.addEventListener('click', handleProfileOutside)
  })
  onUnmounted(() => {
    document.removeEventListener('mousedown', onClickOutside)
    document.removeEventListener('click', handleNotifOutside)
    document.removeEventListener('click', handleProfileOutside)
  })

  // ── Navigation ─────────────────────────────────────────────────────────────
  function navigate(item) {
    dropdownVisible.value = false
    searchQuery.value = ''
    results.value = []

    const category = authStore.sectionCategory
    const isAdmin = authStore.isAdmin
    const batchNo = item.batchNo
    const status = item.status

    if (item.type === 'specimen') {
      if (isAdmin || category === '1') {
        router.push({ name: 'Endorsements', query: { highlight: batchNo } })
      } else if (category === '2') {
        if (status === 'P') {
          router.push({ name: 'IncomingSpecimens', query: { highlight: batchNo } })
        } else {
          router.push({ name: 'ReceivedBatches', query: { highlight: batchNo } })
        }
      } else if (category === '3') {
        if (status === 'P') {
          router.push({ name: 'PendingSpecimens', query: { highlight: item.specimenNo } })
        } else if (status === 'S') {
          router.push({ name: 'ScheduledSpecimens', query: { highlight: item.specimenNo } })
        } else if (status === 'R') {
          router.push({ name: 'RunningSpecimens', query: { highlight: item.specimenNo } })
        } else {
          router.push({ name: 'CompletedSpecimens', query: { highlight: item.specimenNo } })
        }
      }
    } else {
      if (isAdmin || category === '1') {
        router.push({ name: 'Endorsements', query: { highlight: batchNo } })
      } else if (category === '2') {
        if (status === 'C') {
          router.push({ name: 'ReceivedBatches', query: { highlight: batchNo } })
        } else {
          router.push({ name: 'IncomingSpecimens', query: { highlight: batchNo } })
        }
      } else if (category === '3') {
        router.push({ name: 'PendingSpecimens', query: { highlight: batchNo } })
      }
    }
  }

  // ── Status helpers ─────────────────────────────────────────────────────────
  function statusLabel(s) {
    return { P: 'Pending', R: 'Received', C: 'Completed', X: 'Cancelled', S: 'Saved', PA: 'Partial' }[s] ?? s
  }

  function statusDotStyle(s) {
    const colors = {
      P: 'var(--color-warning)',
      R: 'var(--color-primary)',
      C: 'var(--color-success, #16a34a)',
      X: 'var(--color-error)',
      S: 'var(--color-info, #0ea5e9)',
      PA: '#2563EB'
    }
    return `background-color: ${colors[s] ?? 'var(--color-text-muted)'};`
  }

  function statusBadgeStyle(s) {
    const map = {
      P: 'background-color: rgba(217,119,6,0.12); color: var(--color-warning);',
      R: 'background-color: rgba(var(--color-primary-rgb,99,102,241),0.12); color: var(--color-primary);',
      C: 'background-color: rgba(22,163,74,0.12); color: var(--color-success, #16a34a);',
      X: 'background-color: rgba(239,68,68,0.12); color: var(--color-error);',
      S: 'background-color: rgba(14,165,233,0.12); color: var(--color-info, #0ea5e9);',
      PA: 'background-color: rgba(37, 99, 235, 0.08); color: var(--color-info, #2563EB);'
    }
    return map[s] ?? ''
  }

  function formatDate(dt) {
    if (!dt) return ''
    return new Date(dt).toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' })
  }

  // ── Profile dropdown ───────────────────────────────────────────────────────
  const dropdownOpen = ref(false)
  const dropdownRef = ref(null)
  const profileRef = ref(null)

  const themes = [
    { value: 0, label: '☀️ Light' },
    { value: 1, label: '🌙 Dark' },
    { value: 2, label: '🌫️ Dim' },
  ]

  const accents = [
    { value: 0, label: 'Purple', gradient: 'linear-gradient(135deg, #461599, #5e35b1)' },
    { value: 1, label: 'Blue', gradient: 'linear-gradient(135deg, #1a6bcc, #3b82f6)' },
    { value: 2, label: 'Teal', gradient: 'linear-gradient(135deg, #0f766e, #14b8a6)' },
    { value: 3, label: 'Rose', gradient: 'linear-gradient(135deg, #be123c, #e11d48)' },
  ]

  const userInitials = computed(() => {
    const name = authStore.userName ?? ''
    const parts = name.trim().split(/\s+/)
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  })

  const roleLabel = computed(() => {
    switch (authStore.roleID) {
      case 1: return 'Regular'
      case 2: return 'Team Lead'
      default: return 'Staff'
    }
  })

  // ── GSAP dropdown open / close ─────────────────────────────────────────────

  async function animateDropdownOpen() {
    await nextTick()
    if (!dropdownRef.value) return
    gsap.killTweensOf(dropdownRef.value)
    gsap.set(dropdownRef.value, { opacity: 0, scale: 0.93, y: -8 })
    gsap.to(dropdownRef.value, {
      opacity: 1,
      scale: 1,
      y: 0,
      duration: 0.22,
      ease: 'back.out(1.6)',
    })
  }

  function animateDropdownClose(onComplete) {
    if (!dropdownRef.value) { onComplete?.(); return }
    gsap.killTweensOf(dropdownRef.value)
    gsap.to(dropdownRef.value, {
      opacity: 0,
      scale: 0.95,
      y: -6,
      duration: 0.14,
      ease: 'power2.in',
      onComplete,
    })
  }

  function toggleDropdown() {
    if (dropdownOpen.value) {
      animateDropdownClose(() => { dropdownOpen.value = false })
    } else {
      dropdownOpen.value = true
      animateDropdownOpen()
    }
  }

  function closeDropdown() {
    if (!dropdownOpen.value) return
    animateDropdownClose(() => { dropdownOpen.value = false })
  }

  function handleProfileOutside(e) {
    if (profileRef.value && !profileRef.value.contains(e.target)) {
      closeDropdown()
    }
  }

  // ── Theme / Accent / Logout ────────────────────────────────────────────────

  async function handleThemeChange(themeValue) {
    authStore.setTheme(themeValue)
    applyTheme(themeValue)
    try { await themeApi.update(themeValue, authStore.accentColor) } catch { }
  }

  async function handleAccentChange(accentValue) {
    authStore.setAccentColor(accentValue)
    applyAccent(accentValue)
    try { await themeApi.update(authStore.theme, accentValue) } catch { }
  }

  async function handleLogout() {
    closeDropdown()
    NProgress.start()
    try {
      await authApi.logout()
    } catch {
    } finally {
      NProgress.done()
      authStore.logout()
      router.push('/')
    }
  }


  // --------------- NOTIFICATION

  const notifStore = useNotificationStore()
  const notifOpen = ref(false)
  const notifRef = ref(null)

  const notifCount = computed(() => notifStore.unreadCount)

  function toggleNotif() {
    notifOpen.value = !notifOpen.value
  }

  async function handleNotifClick(notif) {
    if (!notif.isRead) {
      await notifStore.markRead(notif.notifID)
    }
    notifOpen.value = false
    navigateToRef(notif)
  }

  function navigateToRef(notif) {
    if (!notif.referenceID) return
    const authStore = useAuthStore()

    switch (notif.notifType) {
      case 'BATCH_ENDORSED':
      case 'BATCH_RECEIVED':
      case 'SPECIMEN_REENDORSED':
        if (authStore.sectionCategory === '2')
          router.push('/receiver/incoming')
        else
          router.push('/endorsements')
        break
      case 'SPECIMEN_CANCELLED':
        router.push('/endorsements')
        break
      case 'SPECIMEN_ARRIVED':
      case 'SPECIMEN_COMPLETED':
        router.push('/runner/pending')
        break
      case 'MIDDLEWARE_ISSUE':
        router.push('/admin/settings')
        break
    }
  }

  function notifIcon(type) {
    const map = {
      BATCH_ENDORSED: 'move_to_inbox',
      BATCH_RECEIVED: 'inventory',
      SPECIMEN_CANCELLED: 'cancel',
      SPECIMEN_REENDORSED: 'redo',
      SPECIMEN_ARRIVED: 'science',
      SPECIMEN_COMPLETED: 'task_alt',
      MIDDLEWARE_ISSUE: 'warning',
      SPECIMEN_FLAGGED: 'flag',
      SPECIMEN_ALERT: 'info'
    }
    return map[type] ?? 'notifications'
  }

  function notifIconColor(type) {
    const map = {
      BATCH_ENDORSED: 'var(--color-primary)',
      BATCH_RECEIVED: 'var(--color-success)',
      SPECIMEN_CANCELLED: 'var(--color-error)',
      SPECIMEN_REENDORSED: 'var(--color-warning)',
      SPECIMEN_ARRIVED: 'var(--color-primary)',
      SPECIMEN_COMPLETED: 'var(--color-success)',
      MIDDLEWARE_ISSUE: 'var(--color-error)',
      SPECIMEN_FLAGGED: 'var(--color-error)',
      SPECIMEN_ALERT: '#2563eb',
    }
    return map[type] ?? 'var(--color-text-muted)'
  }

  function formatNotifTime(dt) {
    if (!dt) return ''
    const d = new Date(dt)
    const now = new Date()
    const diffMs = now - d
    const diffMins = Math.floor(diffMs / 60000)
    if (diffMins < 1) return 'just now'
    if (diffMins < 60) return `${diffMins}m ago`
    const diffHrs = Math.floor(diffMins / 60)
    if (diffHrs < 24) return `${diffHrs}h ago`
    return d.toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })
  }

  function handleNotifOutside(e) {
    if (notifRef.value && !notifRef.value.contains(e.target)) {
      notifOpen.value = false
    }
  }

</script>

<style scoped>
  .dropdown-enter-active {
    transition: opacity 0.15s ease, transform 0.15s ease;
  }

  .dropdown-leave-active {
    transition: opacity 0.1s ease, transform 0.1s ease;
  }

  .dropdown-enter-from {
    opacity: 0;
    transform: translateY(-6px);
  }

  .dropdown-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  .animate-spin {
    animation: spin 0.8s linear infinite;
  }

  .notif-drop-enter-active,
  .notif-drop-leave-active {
    transition: all 0.2s ease;
  }

  .notif-drop-enter-from,
  .notif-drop-leave-to {
    opacity: 0;
    transform: translateY(-8px);
  }
</style>
