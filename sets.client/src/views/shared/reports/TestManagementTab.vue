<template>
  <div class="flex-1 flex flex-col overflow-hidden">

    <!-- Header -->
    <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
         style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
      <div>
        <h1 class="text-base font-bold" style="color: var(--color-text);">Test Management Report</h1>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
          Running days per test · grouped by laboratory section · Excel export
        </p>
      </div>
      <button ref="exportBtnRef"
              :disabled="!rows.length || exporting"
              class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
              style="background: var(--color-primary-gradient); color: #ffffff;"
              @click="handleExport">
        <span ref="exportIconRef"
              class="material-symbols-outlined text-sm"
              :class="exporting ? 'animate-spin' : ''">
          {{ exporting ? 'progress_activity' : exportDone ? 'check_circle' : 'download' }}
        </span>
        {{ exporting ? 'Exporting…' : exportDone ? 'Exported!' : 'Export Excel' }}
      </button>
    </div>

    <!-- Filters -->
    <div ref="filterRef"
         class="px-7 py-4 flex items-end gap-4 flex-shrink-0"
         style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

      <!-- Section filter (admin only) -->
      <div v-if="authStore.isAdmin" class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Section</label>
        <select v-model="filters.sectionCode"
                class="text-xs rounded-xl px-3 h-9 pr-8"
                style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 200px;">
          <option value="">All Sections</option>
          <option v-for="s in labSections" :key="s.code" :value="s.code">{{ s.name }}</option>
        </select>
      </div>

      <!-- Locked section display (TL) -->
      <div v-else class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Section</label>
        <div class="text-xs rounded-xl px-3 h-9 flex items-center gap-2"
             style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 200px;">
          <span class="material-symbols-outlined" style="font-size: 14px; color: var(--color-text-muted);">lock</span>
          {{ authStore.sectionName }}
        </div>
      </div>

      <!-- Generate button -->
      <button ref="generateBtnRef"
              :disabled="loading"
              class="flex items-center gap-2 px-5 h-9 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
              style="background: var(--color-primary-gradient); color: #ffffff;"
              @click="handleGenerate">
        <span class="material-symbols-outlined text-sm"
              :class="loading ? 'animate-spin' : ''">
          {{ loading ? 'progress_activity' : 'search' }}
        </span>
        {{ loading ? 'Loading…' : 'Generate' }}
      </button>
    </div>

    <!-- Content -->
    <div class="flex-1 overflow-y-auto px-7 py-6">

      <!-- Empty state — not yet generated -->
      <div v-if="!generated && !loading"
           class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
           style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">biotech</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">Select a section and click Generate</p>
        <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
      </div>

      <!-- Skeleton -->
      <div v-else-if="loading" class="flex flex-col gap-3">
        <div v-for="i in 8" :key="i" class="h-10 rounded-xl animate-pulse"
             style="background-color: var(--color-surface);"></div>
      </div>

      <!-- No results -->
      <div v-else-if="generated && !rows.length"
           ref="emptyRef"
           class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
           style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No tests found</p>
        <p class="text-xs" style="color: var(--color-text-muted);">No running day setup for this section</p>
      </div>

      <!-- Results -->
      <template v-else-if="rows.length">

        <!-- Admin: grouped by section -->
        <template v-if="authStore.isAdmin && !filters.sectionCode">
          <div v-for="(group, sectionCode) in groupedRows" :key="sectionCode" class="mb-6">

            <!-- Section header -->
            <div class="flex items-center gap-2 mb-2">
              <span class="material-symbols-outlined" style="font-size: 15px; color: var(--color-primary);">science</span>
              <span class="text-xs font-bold" style="color: var(--color-text);">{{ group.sectionName }}</span>
              <span class="text-[10px] px-2 py-0.5 rounded-full font-medium"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
                {{ group.rows.length }} test{{ group.rows.length !== 1 ? 's' : '' }}
              </span>
            </div>

            <!-- Table -->
            <div class="rounded-2xl overflow-hidden"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
              <div class="overflow-x-auto">
                <table class="w-full text-xs">
                  <thead>
                    <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                          style="color: var(--color-text-muted);">Test code</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Test name</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                          style="color: var(--color-text-muted);">Running days</th>
                    </tr>
                  </thead>
                  <tbody :ref="el => { if (el) groupBodyRefs[sectionCode] = el }">
                    <tr v-for="(row, idx) in group.rows" :key="idx"
                        style="border-bottom: 0.5px solid var(--color-border);"
                        @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                        @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                      <td class="px-4 py-2.5 font-mono font-medium whitespace-nowrap"
                          style="color: var(--color-primary);">
                        {{ row.testCode }}
                      </td>
                      <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.testName }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.runningDays ?? '—' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                   style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low); color: var(--color-text-muted);">
                {{ group.rows.length }} test{{ group.rows.length !== 1 ? 's' : '' }}
              </div>
            </div>
          </div>
        </template>

        <!-- Single section: flat table (admin filtered or TL) -->
        <template v-else>
          <div ref="tableRef" class="rounded-2xl overflow-hidden"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="overflow-x-auto">
              <table class="w-full text-xs">
                <thead>
                  <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                        style="color: var(--color-text-muted);">Test code</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted);">Test name</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                        style="color: var(--color-text-muted);">Running days</th>
                  </tr>
                </thead>
                <tbody ref="tableBodyRef">
                  <tr v-for="(row, idx) in rows" :key="idx"
                      style="border-bottom: 0.5px solid var(--color-border);"
                      @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                      @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                    <td class="px-4 py-2.5 font-mono font-medium whitespace-nowrap"
                        style="color: var(--color-primary);">
                      {{ row.testCode }}
                    </td>
                    <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.testName }}</td>
                    <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.runningDays ?? '—' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                 style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low); color: var(--color-text-muted);">
              {{ rows.length }} test{{ rows.length !== 1 ? 's' : '' }}
            </div>
          </div>
        </template>

      </template>
    </div>

    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />

  </div>
</template>

<script setup>
  import { ref, computed, watch, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import { useAuthStore } from '@/stores/authStore'
  import { reportApi } from '@/api/reportApi'
  import AlertModal from '@/components/common/AlertModal.vue'

  const props = defineProps({
    labSections: { type: Array, default: () => [] },
  })

  const authStore = useAuthStore()

  // ── State ─────────────────────────────────────────────────────────────────
  const filters = ref({ sectionCode: '' })
  const loading = ref(false)
  const exporting = ref(false)
  const generated = ref(false)
  const rows = ref([])

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Grouped rows (admin all-sections view) ────────────────────────────────
  const groupedRows = computed(() => {
    const groups = {}
    for (const row of rows.value) {
      if (!groups[row.sectionCode]) {
        groups[row.sectionCode] = { sectionName: row.sectionName, rows: [] }
      }
      groups[row.sectionCode].rows.push(row)
    }
    return groups
  })

  // ── GSAP refs ─────────────────────────────────────────────────────────────
  const generateBtnRef = ref(null)
  const exportBtnRef = ref(null)
  const exportIconRef = ref(null)
  const filterRef = ref(null)
  const tableRef = ref(null)
  const tableBodyRef = ref(null)
  const emptyRef = ref(null)
  const groupBodyRefs = ref({})  // keyed by sectionCode for grouped view

  // ── GSAP helpers ──────────────────────────────────────────────────────────
  function punchButton(btnRef) {
    if (!btnRef?.value) return
    gsap.killTweensOf(btnRef.value)
    gsap.set(btnRef.value, { scale: 1 })
    gsap.to(btnRef.value, { scale: 0.93, duration: 0.08, ease: 'power2.in', yoyo: true, repeat: 1 })
  }

  function dimFilter(isLoading) {
    if (!filterRef.value) return
    gsap.killTweensOf(filterRef.value)
    gsap.to(filterRef.value, { opacity: isLoading ? 0.45 : 1, duration: 0.2, ease: 'power1.out' })
  }

  async function animateTableEntrance() {
    if (!tableRef.value) return
    await nextTick()
    const tableRows = tableRef.value.querySelectorAll('tbody tr')
    gsap.set(tableRef.value, { opacity: 0, y: 14 })
    gsap.to(tableRef.value, { opacity: 1, y: 0, duration: 0.22, ease: 'power2.out' })
    if (tableRows.length) {
      gsap.set(tableRows, { opacity: 0, x: -5 })
      gsap.to(tableRows, { opacity: 1, x: 0, duration: 0.15, stagger: 0.02, ease: 'power1.out', delay: 0.12 })
    }
  }

  async function animateGroupedEntrance() {
    await nextTick()
    for (const sectionCode of Object.keys(groupBodyRefs.value)) {
      const tbody = groupBodyRefs.value[sectionCode]
      if (!tbody) continue
      const tableRows = tbody.querySelectorAll('tr')
      if (!tableRows.length) continue
      gsap.set(tableRows, { opacity: 0, x: -5 })
      gsap.to(tableRows, { opacity: 1, x: 0, duration: 0.15, stagger: 0.02, ease: 'power1.out', delay: 0.1 })
    }
  }

  async function animateEmptyState() {
    if (!emptyRef.value) return
    await nextTick()
    gsap.set(emptyRef.value, { scale: 0.92, opacity: 0 })
    gsap.to(emptyRef.value, { scale: 1, opacity: 1, duration: 0.32, ease: 'back.out(1.5)' })
  }

  // ── Export icon swap ──────────────────────────────────────────────────────
  const exportDone = ref(false)
  let exportResetTimer = null

  // ── API ───────────────────────────────────────────────────────────────────
  async function generate() {
    loading.value = true
    generated.value = false
    rows.value = []
    try {
      rows.value = await reportApi.getTestManagement({
        sectionCode: authStore.isAdmin ? (filters.value.sectionCode || null) : null,
      })
      generated.value = true
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      loading.value = false
    }
  }

  async function exportExcel() {
    if (!rows.value.length) return
    exporting.value = true
    try {
      await reportApi.exportTestManagementExcel({
        sectionCode: authStore.isAdmin ? (filters.value.sectionCode || null) : null,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      exporting.value = false
    }
  }

  // ── Action handlers (with GSAP) ───────────────────────────────────────────
  async function handleGenerate() {
    punchButton(generateBtnRef)
    await generate()
  }

  async function handleExport() {
    punchButton(exportBtnRef)
    exportDone.value = false
    let success = false
    try {
      await exportExcel()
      success = true
    } catch { /* exportExcel shows alert internally */ }
    if (success && exportIconRef.value) {
      exportDone.value = true
      await nextTick()
      gsap.set(exportIconRef.value, { scale: 0, opacity: 0 })
      gsap.to(exportIconRef.value, { scale: 1, opacity: 1, duration: 0.35, ease: 'back.out(2.5)' })
      clearTimeout(exportResetTimer)
      exportResetTimer = setTimeout(() => { exportDone.value = false }, 1500)
    }
  }

  // ── Watchers ──────────────────────────────────────────────────────────────
  watch(loading, (v) => dimFilter(v))

  watch(rows, async (val) => {
    if (!generated.value) return
    await nextTick()
    if (!val.length) {
      await animateEmptyState()
      return
    }
    // Admin all-sections grouped view
    if (authStore.isAdmin && !filters.value.sectionCode) {
      await animateGroupedEntrance()
    } else {
      await animateTableEntrance()
    }
  })
</script>
