<script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue'
  import { useChangelogStore } from '@/stores/changelogStore'
  import gsap from 'gsap'

  const changelogStore = useChangelogStore()

  const modalRef = ref(null)
  const backdropRef = ref(null)

  // ── Tag config ─────────────────────────────────────────────────────────────

  const TAG_CONFIG = {
    NEW: { label: 'New', icon: 'add_circle', color: '#16a34a', bg: 'rgba(22,163,74,0.10)' },
    IMPROVED: { label: 'Improved', icon: 'trending_up', color: '#2563eb', bg: 'rgba(37,99,235,0.10)' },
    FIXED: { label: 'Fixed', icon: 'build_circle', color: '#d97706', bg: 'rgba(217,119,6,0.10)' },
    REMOVED: { label: 'Removed', icon: 'remove_circle', color: '#dc2626', bg: 'rgba(220,38,38,0.10)' },
  }

  function tagConfig(tag) {
    return TAG_CONFIG[tag?.toUpperCase()] ?? TAG_CONFIG.NEW
  }

  const entry = computed(() => changelogStore.latestEntry)

  // ── Animation ──────────────────────────────────────────────────────────────

  onMounted(() => {
    if (!modalRef.value) return

    gsap.set(modalRef.value, { opacity: 0, scale: 0.90, y: 20 })
    gsap.to(modalRef.value, {
      opacity: 1, scale: 1, y: 0,
      duration: 0.32, ease: 'back.out(1.6)',
    })

    if (backdropRef.value) {
      gsap.fromTo(backdropRef.value,
        { opacity: 0 },
        { opacity: 1, duration: 0.22, ease: 'power2.out' }
      )
    }

    window.addEventListener('keydown', onKeyDown)
  })

  onUnmounted(() => {
    window.removeEventListener('keydown', onKeyDown)
  })

  function onKeyDown(e) {
    if (e.key === 'Escape') dismiss()
  }

  // ── Dismiss ────────────────────────────────────────────────────────────────

  const dismissing = ref(false)

  async function dismiss() {
    if (dismissing.value) return
    dismissing.value = true

    if (modalRef.value) {
      gsap.to(modalRef.value, {
        opacity: 0, scale: 0.93, y: 12,
        duration: 0.20, ease: 'power2.in',
      })
    }
    if (backdropRef.value) {
      gsap.to(backdropRef.value, { opacity: 0, duration: 0.20, ease: 'power2.in' })
    }

    // Wait for animation then commit
    setTimeout(async () => {
      await changelogStore.dismiss()
      dismissing.value = false
    }, 210)
  }

  // ── Date format ────────────────────────────────────────────────────────────

  function formatDate(iso) {
    if (!iso) return ''
    return new Date(iso).toLocaleDateString('en-PH', {
      year: 'numeric', month: 'long', day: 'numeric'
    })
  }
</script>

<template>
  <Teleport to="body">
    <div v-if="changelogStore.modalVisible && entry"
         class="fixed inset-0 z-[200] flex items-center justify-center p-4">

      <!-- Backdrop -->
      <div ref="backdropRef"
           class="absolute inset-0 bg-black/50 backdrop-blur-sm"
           @click="dismiss" />

      <!-- Modal -->
      <div ref="modalRef"
           class="relative w-full max-w-lg rounded-2xl shadow-2xl flex flex-col overflow-hidden"
           style="background-color: var(--color-surface); max-height: 88vh;">

        <!-- ── Header ──────────────────────────────────────────────────────── -->
        <div class="px-7 pt-7 pb-5 flex-shrink-0"
             style="border-bottom: 1px solid var(--color-border);">

          <!-- Version badge + icon -->
          <div class="flex items-start justify-between gap-4 mb-3">
            <div class="flex items-center gap-3">
              <div class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft);">
                <span class="material-symbols-outlined text-xl"
                      style="color: var(--color-primary); font-variation-settings: 'FILL' 1;">
                  new_releases
                </span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-0.5"
                   style="color: var(--color-primary);">What's New</p>
                <h2 class="text-lg font-extrabold tracking-tight leading-tight"
                    style="color: var(--color-text);">
                  {{ entry.title }}
                </h2>
              </div>
            </div>

            <!-- Close button -->
            <button class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0 transition-all active:scale-90"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="dismiss">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Version + date pill row -->
          <div class="flex items-center gap-2 flex-wrap">
            <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold"
                  style="background-color: var(--color-primary); color: #ffffff;">
              <span class="material-symbols-outlined text-[13px]">tag</span>
              v{{ entry.version }}
            </span>
            <span class="text-[11px]" style="color: var(--color-text-muted);">
              Released {{ formatDate(entry.releasedAt) }}
            </span>
          </div>
        </div>

        <!-- ── Items (scrollable) ──────────────────────────────────────────── -->
        <div class="px-7 py-5 overflow-y-auto flex-1 space-y-2.5">
          <div v-for="item in entry.items"
               :key="item.id"
               class="flex items-start gap-3 p-3 rounded-xl"
               :style="`background-color: ${tagConfig(item.tag).bg};`">

            <!-- Tag icon -->
            <span class="material-symbols-outlined text-lg mt-0.5 flex-shrink-0"
                  :style="`color: ${tagConfig(item.tag).color}; font-variation-settings: 'FILL' 1;`">
              {{ tagConfig(item.tag).icon }}
            </span>

            <div class="flex-1 min-w-0">
              <!-- Tag label -->
              <span class="inline-block text-[9px] font-extrabold uppercase tracking-widest px-1.5 py-0.5 rounded-md mb-1"
                    :style="`color: ${tagConfig(item.tag).color}; background-color: ${tagConfig(item.tag).bg}; border: 1px solid ${tagConfig(item.tag).color}33;`">
                {{ tagConfig(item.tag).label }}
              </span>
              <!-- Description -->
              <p class="text-xs leading-relaxed"
                 style="color: var(--color-text);">
                {{ item.description }}
              </p>
            </div>
          </div>

          <!-- Empty state (shouldn't happen but safe) -->
          <div v-if="!entry.items || entry.items.length === 0"
               class="text-center py-6">
            <p class="text-sm" style="color: var(--color-text-muted);">No details available.</p>
          </div>
        </div>

        <!-- ── Footer ─────────────────────────────────────────────────────── -->
        <div class="px-7 py-5 flex-shrink-0 flex items-center justify-between gap-3"
             style="border-top: 1px solid var(--color-border);">
          <p class="text-[11px]" style="color: var(--color-text-muted);">
            SETS · HI-Precision Diagnostics Cebu
          </p>
          <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                  style="background-color: var(--color-primary); color: #ffffff;"
                  :disabled="dismissing"
                  @click="dismiss">
            <span class="material-symbols-outlined text-sm">check</span>
            Got it
          </button>
        </div>

      </div>
    </div>
  </Teleport>
</template>
