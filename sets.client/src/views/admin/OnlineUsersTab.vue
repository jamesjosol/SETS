<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">groups</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
            Online Users
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Real-time list of staff currently logged in to this branch.
          </p>
        </div>
      </div>

      <!-- Live count badge -->
      <div class="flex items-center gap-2 px-3 py-1.5 rounded-xl text-xs font-bold"
           style="background-color: var(--color-primary-soft); color: var(--color-primary)">
        <span class="w-2 h-2 rounded-full animate-pulse"
              style="background-color: var(--color-primary)"></span>
        {{ deduped.length }} Online
      </div>
    </div>

    <!-- Search bar -->
    <div class="px-8 pt-6 pb-2">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="search"
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="
            background-color: var(--color-surface-low);
            color: var(--color-text);
            border: 1.5px solid var(--color-border);
          "
               placeholder="Search by User ID, Name, or Section..."
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>
    </div>

    <!-- Table -->
    <div class="px-8 pb-8 pt-4">
      <AppTable :rows="filtered"
                :columns="columns"
                row-key="userID"
                :page-size="15"
                empty-text="No users are currently online."
                empty-icon="person_off">
        <!-- User (avatar + ID) -->
        <template #cell-userID="{ row }">
          <div class="flex items-center gap-3">
            <!-- Avatar -->
            <div class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 overflow-hidden"
                 style="background-color: var(--color-primary-soft)">
              <img v-if="row.profilePicture"
                   :src="row.profilePicture"
                   class="w-full h-full object-cover cursor-zoom-in"
                   @mouseenter="showAvatarPreview($event, row)"
                   @mouseleave="hideAvatarPreview" />
              <span v-else
                    class="material-symbols-outlined text-sm"
                    style="color: var(--color-primary)">person</span>
            </div>
            <span class="font-mono font-bold text-xs" style="color: var(--color-text)">
              {{
              row.userID
              }}
            </span>
          </div>
        </template>

        <!-- Name -->
        <template #cell-userName="{ value }">
          <span class="text-sm font-medium" style="color: var(--color-text)">{{ value }}</span>
        </template>

        <!-- Section -->
        <template #cell-sectionName="{ row }">
          <div>
            <p class="text-sm font-medium" style="color: var(--color-text)">
              {{ row.isAdmin ? '—' : row.sectionName || row.sectionCode || '—' }}
            </p>
            <p v-if="!row.isAdmin && row.sectionCode"
               class="font-mono text-[10px] mt-0.5"
               style="color: var(--color-text-muted)">
              {{ row.sectionCode }}
            </p>
          </div>
        </template>

        <!-- Role / Category badge -->
        <template #cell-category="{ row }">
          <span class="px-2.5 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest"
                :style="categoryStyle(row)">
            {{ categoryLabel(row) }}
          </span>
        </template>

        <!-- Connected at -->
        <template #cell-connectedAt="{ value }">
          <span class="text-xs font-medium" style="color: var(--color-text-muted)">
            {{ formatTime(value) }}
          </span>
        </template>
      </AppTable>
    </div>

    <!-- SignalR status footer -->
    <div class="px-8 py-3 flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest"
         style="border-top: 1px solid var(--color-border); color: var(--color-text-muted)">
      <span class="w-1.5 h-1.5 rounded-full"
            :style="
          presenceStore.hubStatus === 'connected'
            ? 'background-color: var(--color-success)'
            : 'background-color: var(--color-warning)'
        "></span>
      Live updates
      {{ presenceStore.hubStatus === 'connected' ? 'active' : presenceStore.hubStatus }}
    </div>
  </div>

  <!-- Avatar zoom preview (Teleport — same pattern as UserManagementTab) -->
  <Teleport to="body">
    <div v-if="avatarPreview.visible"
         ref="avatarPreviewRef"
         class="fixed z-[9999] flex flex-col items-center gap-2 pointer-events-none"
         :style="`top: ${avatarPreview.y}px; left: ${avatarPreview.x}px; transform: translate(-50%, -50%);`">
      <div class="rounded-2xl overflow-hidden shadow-2xl"
           style="width: 160px; height: 160px; border: 2.5px solid var(--color-border)">
        <img :src="avatarPreview.src" class="w-full h-full object-cover" />
      </div>
      <span class="text-xs font-bold px-3 py-1 rounded-full"
            style="
          background-color: var(--color-surface);
          color: var(--color-text);
          border: 1px solid var(--color-border);
          box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
        ">
        {{ avatarPreview.name }}
      </span>
    </div>
  </Teleport>
</template>

<script setup>
  import { ref, computed, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import { usePresenceStore } from '@/stores/presenceStore'
  import AppTable from '@/components/common/AppTable.vue'

  defineEmits(['toast'])

  const presenceStore = usePresenceStore()
  const search = ref('')

  // ── Columns ────────────────────────────────────────────────────────────────
  const columns = [
    { key: 'userID', label: 'User' },
    { key: 'userName', label: 'Name' },
    { key: 'sectionName', label: 'Section' },
    { key: 'category', label: 'Role' },
    { key: 'connectedAt', label: 'Connected At' },
  ]

  // ── Computed ───────────────────────────────────────────────────────────────
  // One row per physical user — keep the most-recently connected entry when
  // a user has multiple tabs open simultaneously.
  const deduped = computed(() => {
    const map = new Map()
    for (const u of presenceStore.onlineUsers) {
      const existing = map.get(u.userID)
      if (!existing || u.connectedAt > existing.connectedAt) {
        map.set(u.userID, u)
      }
    }
    return [...map.values()]
  })

  const filtered = computed(() => {
    const q = search.value.toLowerCase()
    if (!q) return deduped.value
    return deduped.value.filter(
      (u) =>
        u.userID.toLowerCase().includes(q) ||
        u.userName.toLowerCase().includes(q) ||
        (u.sectionName || '').toLowerCase().includes(q),
    )
  })

  // ── Category helpers ───────────────────────────────────────────────────────
  function categoryLabel(row) {
    if (row.isAdmin) return 'Admin'
    if (row.category === '1') return 'Endorser'
    if (row.category === '2') return 'Processing'
    if (row.category === '3') return 'Lab Section'
    return row.category ?? '—'
  }

  function categoryStyle(row) {
    if (row.isAdmin)
      return 'background-color: rgba(124,58,237,0.12); color: #7c3aed;'
    if (row.category === '1')
      return 'background-color: var(--color-primary-soft); color: var(--color-primary);'
    if (row.category === '2')
      return 'background-color: rgba(5,150,105,0.1); color: #059669;'
    if (row.category === '3')
      return 'background-color: rgba(202,138,4,0.1); color: #ca8a04;'
    return 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function formatTime(iso) {
    if (!iso) return '—'
    return new Date(iso).toLocaleTimeString('en-PH', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    })
  }

  // ── Avatar zoom preview ────────────────────────────────────────────────────
  const avatarPreviewRef = ref(null)
  const avatarPreview = ref({ visible: false, x: 0, y: 0, src: '', name: '' })

  function showAvatarPreview(e, row) {
    const rect = e.currentTarget.getBoundingClientRect()
    const previewH = 200

    let y
    if (rect.top >= previewH + 12) {
      y = rect.top - 12
    } else if (window.innerHeight - rect.bottom >= previewH + 12) {
      y = rect.bottom + 12 + previewH / 2
    } else {
      y = window.innerHeight / 2
    }

    avatarPreview.value = {
      visible: true,
      x: rect.left + rect.width / 2,
      y,
      src: row.profilePicture,
      name: row.userName,
    }

    nextTick(() => {
      if (!avatarPreviewRef.value) return
      gsap.set(avatarPreviewRef.value, { scale: 0.6, opacity: 0 })
      gsap.to(avatarPreviewRef.value, {
        scale: 1,
        opacity: 1,
        duration: 0.25,
        ease: 'back.out(1.4)',
      })
    })
  }

  function hideAvatarPreview() {
    if (!avatarPreviewRef.value) return
    gsap.to(avatarPreviewRef.value, {
      scale: 0.6,
      opacity: 0,
      duration: 0.15,
      ease: 'power2.in',
      onComplete: () => {
        avatarPreview.value.visible = false
      },
    })
  }
</script>
