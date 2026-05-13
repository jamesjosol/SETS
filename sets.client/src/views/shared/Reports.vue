<template>
  <AppLayout>
    <div class="flex h-full" style="min-height: calc(100vh - 64px);">

      <!-- ── Sidebar ──────────────────────────────────────────────────────── -->
      <aside class="w-56 flex-shrink-0 flex flex-col overflow-y-auto"
             style="border-right: 1px solid var(--color-border); background-color: var(--color-surface);">

        <div class="px-4 pt-5 pb-3 text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted); border-bottom: 0.5px solid var(--color-border);">Reports</div>

        <nav ref="navRef" class="flex flex-col gap-0.5 px-2 py-3">
          <template v-for="item in reportItems" :key="item.key">

            <!-- Locked item -->
            <div v-if="!item.hasAccess"
                 class="flex items-start gap-2.5 px-3 py-2.5 rounded-xl cursor-not-allowed select-none"
                 style="opacity: 0.38; filter: grayscale(0.6);">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    style="color: var(--color-text-muted); font-size: 17px;">lock</span>
              <div>
                <div class="text-xs font-medium" style="color: var(--color-text-muted);">{{ item.label }}</div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </div>

            <!-- Accessible item -->
            <button v-else
                    class="w-full flex items-start gap-2.5 px-3 py-2.5 rounded-xl text-left transition-all cursor-pointer"
                    :style="activeReport === item.key
                      ? 'background-color: var(--color-surface-low); border-left: 2.5px solid var(--color-primary);'
                      : 'border-left: 2.5px solid transparent;'"
                    @click="selectReport(item.key)">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    :style="`font-size: 17px; color: ${activeReport === item.key ? 'var(--color-primary)' : 'var(--color-text-muted)'}`">
                {{ item.icon }}
              </span>
              <div>
                <div class="text-xs font-medium"
                     :style="`color: ${activeReport === item.key ? 'var(--color-text)' : 'var(--color-text-muted)'}`">
                  {{ item.label }}
                </div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </button>

          </template>
        </nav>
      </aside>

      <!-- ── Main panel ───────────────────────────────────────────────────── -->
      <main ref="mainRef" class="flex-1 flex flex-col overflow-hidden" style="background-color: var(--color-bg);">

        <BatchSummaryTab v-if="activeReport === 'batch-summary'"
                         :endorsing-sections="endorsingSections"
                         :lab-sections="labSections" />

        <SpecimenReceiptSectionTab v-else-if="activeReport === 'specimen-receipt-section'"
                                   :endorsing-sections="endorsingSections"
                                   :lab-sections="labSections" />

        <DuplicateEndorsementTab v-else-if="activeReport === 'duplicate-endorsement'"
                                 :endorsing-sections="endorsingSections"
                                 :lab-sections="labSections" />

        <Beyond14DaysTab v-else-if="activeReport === 'beyond-14-days'"
                         :endorsing-sections="endorsingSections"
                         :lab-sections="labSections" />

        <SpecimenNotReceivedTab v-else-if="activeReport === 'specimen-not-received'"
                                :endorsing-sections="endorsingSections"
                                :lab-sections="labSections" />

        <ComingSoonTab v-else
                       :label="activeItem?.label"
                       :sublabel="activeItem?.sublabel" />

      </main>
    </div>
  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { sectionApi } from '@/api/sectionApi'

  // ── Tab components ────────────────────────────────────────────────────────────
  import BatchSummaryTab from './reports/BatchSummaryTab.vue'
  import SpecimenReceiptSectionTab from './reports/SpecimenReceiptSectionTab.vue'
  import DuplicateEndorsementTab from './reports/DuplicateEndorsementTab.vue'
  import Beyond14DaysTab from './reports/Beyond14DaysTab.vue'
  import SpecimenNotReceivedTab from './reports/SpecimenNotReceivedTab.vue'
  import ComingSoonTab from './reports/ComingSoonTab.vue'

  const authStore = useAuthStore()

  // ── Access helpers ────────────────────────────────────────────────────────────
  const isTLorAdmin = computed(() => authStore.isAdmin || authStore.roleID === 2)

  // ── Report nav items ──────────────────────────────────────────────────────────
  const reportItems = computed(() => [
    {
      key: 'unprocessed-specimen',
      label: 'Unprocessed Specimen',
      sublabel: 'ERD / CRD / SRD per section',
      icon: 'science',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-endorsed',
      label: 'Specimen Not Endorsed',
      sublabel: 'Flagged during endorsement',
      icon: 'warning',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-received',
      label: 'Specimen Not Received',
      sublabel: 'Pending / unprocessed',
      icon: 'inventory_2',
      roles: ['Processing', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '2',
    },
    {
      key: 'test-management',
      label: 'Test Management',
      sublabel: 'Running days, TAT, status',
      icon: 'biotech',
      roles: ['Lab', 'TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
    {
      key: 'batch-summary',
      label: 'Batch Summary',
      sublabel: 'TAT endorsement & completion',
      icon: 'table_chart',
      roles: ['Endorser/Processing', 'TL', 'Admin'],
      hasAccess: isTLorAdmin.value && (authStore.sectionCategory === '1' || authStore.sectionCategory === '2'),
    },
    {
      key: 'specimen-receipt-section',
      label: 'Specimen Receipt Laboratory Section',
      sublabel: 'Processing → Lab section TAT',
      icon: 'schedule_send',
      roles: ['Processing/Lab', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '2' || authStore.sectionCategory === '3',
    },
    {
      key: 'duplicate-endorsement',
      label: 'Duplicate Endorsement',
      sublabel: 'Re-endorsed specimens',
      icon: 'content_copy',
      roles: ['Endorser/Processing', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '1' || authStore.sectionCategory === '2',
    },
    {
      key: 'beyond-14-days',
      label: 'Beyond 14 Days',
      sublabel: 'Old transaction flags',
      icon: 'calendar_clock',
      roles: ['Endorser/Processing', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '1' || authStore.sectionCategory === '2',
    },
    {
      key: 'monthly-summary',
      label: 'Monthly Summary',
      sublabel: 'Aggregate per location',
      icon: 'bar_chart',
      roles: ['TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
  ])

  const firstAccessible = computed(() =>
    reportItems.value.find(i => i.hasAccess)?.key ?? 'batch-summary'
  )

  const activeReport = ref(null)
  const activeItem = computed(() => reportItems.value.find(i => i.key === activeReport.value))

  function selectReport(key) {
    if (activeReport.value === key) return
    activeReport.value = key
  }

  function roleChipClass(role) {
    switch (role) {
      case 'All roles': return 'bg-teal-50   text-teal-800'
      case 'Endorser': return 'bg-green-50  text-green-800'
      case 'Endorser/Processing': return 'bg-cyan-50   text-cyan-800'
      case 'Processing': return 'bg-blue-50   text-blue-800'
      case 'Processing/Lab': return 'bg-indigo-50 text-indigo-800'
      case 'Lab': return 'bg-rose-50   text-rose-800'
      case 'TL': return 'bg-amber-50  text-amber-800'
      case 'Admin': return 'bg-purple-50 text-purple-800'
      default: return 'bg-gray-100  text-gray-600'
    }
  }

  // ── Reference data — passed down as props to all tab components ───────────────
  const endorsingSections = ref([])
  const labSections = ref([])

  async function loadReferenceData() {
    try {
      const res = await sectionApi.getAll()
      const active = res.data.filter(s => s.active)
      endorsingSections.value = active.filter(s => s.category === '1')
      labSections.value = active.filter(s => s.category === '3')
    } catch (e) {
      console.error(e)
    }
  }

  // ── GSAP refs ─────────────────────────────────────────────────────────────────
  const navRef = ref(null)
  const mainRef = ref(null)

  // ── GSAP — panel swap on every report change ──────────────────────────────────
  watch(activeReport, async () => {
    if (!mainRef.value) return
    await nextTick()
    gsap.set(mainRef.value, { opacity: 0, y: 7 })
    gsap.to(mainRef.value, { opacity: 1, y: 0, duration: 0.22, ease: 'power2.out' })
  })

  // ── Lifecycle ─────────────────────────────────────────────────────────────────
  onMounted(async () => {
    activeReport.value = firstAccessible.value
    loadReferenceData()

    // Sidebar nav items stagger in from the left.
    // Each child is tweened to its own target opacity so locked items
    // (opacity: 0.38) are not overwritten to 1 by the stagger tween.
    await nextTick()
    if (navRef.value?.children?.length) {
      const children = Array.from(navRef.value.children)
      gsap.set(children, { x: -10, opacity: 0 })
      children.forEach((child, i) => {
        const isLocked = child.classList.contains('cursor-not-allowed')
        gsap.to(child, {
          x: 0,
          opacity: isLocked ? 0.38 : 1,
          duration: 0.25,
          delay: i * 0.04,
          ease: 'power2.out',
        })
      })
    }
  })

  onUnmounted(() => {
    gsap.killTweensOf([mainRef.value, navRef.value].filter(Boolean))
  })
</script>
