<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Running Specimens</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          <span v-if="authStore.isAdmin" style="color: var(--color-primary); font-weight: 700;">ADMINISTRATOR</span>
          <span v-else>{{ authStore.sectionName }} · {{ authStore.branchCode }} · Assigned to {{ authStore.userName }}</span>
        </p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
              style="background: var(--color-primary-gradient); color: #fff;"
              @click="load">
        <span class="material-symbols-outlined text-sm">refresh</span>
        Refresh
      </button>
    </div>

    <!-- Search + Count (shared) -->
    <div class="mb-4 flex gap-3 flex-wrap items-center">
      <div class="relative flex-1 min-w-[200px]">
        <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
              style="color: var(--color-text-muted);">search</span>
        <input v-model="searchQuery" type="text" placeholder="Search specimen no., patient name..."
               class="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition-all"
               style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);" />
      </div>
      <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
           style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
        <span class="material-symbols-outlined text-sm" style="color: var(--color-warning);">labs</span>
        {{ totalSpecimens }} specimen{{ totalSpecimens !== 1 ? 's' : '' }}
      </div>
      <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
           style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
        <span class="material-symbols-outlined text-sm" style="color: var(--color-warning);">biotech</span>
        {{ totalTests }} test{{ totalTests !== 1 ? 's' : '' }} running
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════════════
       REGULAR / TEAM LEAD VIEW
  ══════════════════════════════════════════════════════════════════════ -->
    <template v-if="!authStore.isAdmin">

      <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div v-if="loading" class="p-6 flex flex-col gap-3">
          <div v-for="i in 5" :key="i" class="h-14 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
        </div>
        <div v-else-if="!filteredSpecimens.length" class="p-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">labs</span>
          <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
          <p class="text-xs" style="color: var(--color-text-muted);">You have no tests currently marked as running.</p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1.5px solid var(--color-border);">
                <th class="w-8 px-4 py-3"></th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tests Running</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="item in filteredSpecimens" :key="item.headerId">
                <tr class="transition-colors cursor-pointer"
                    :style="expandedId === item.headerId ? 'background-color: var(--color-primary-soft);' : 'background-color: transparent;'"
                    @mouseenter="e => { if (expandedId !== item.headerId) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                    @mouseleave="e => { if (expandedId !== item.headerId) e.currentTarget.style.backgroundColor = 'transparent' }"
                    @click="toggleExpand(item)"
                    style="border-top: 1px solid var(--color-border);">
                  <td class="px-4 py-3">
                    <span class="material-symbols-outlined text-sm transition-transform duration-200"
                          :style="{ color: 'var(--color-text-muted)', display: 'block', transform: expandedId === item.headerId ? 'rotate(90deg)' : 'rotate(0deg)' }">
                      chevron_right
                    </span>
                  </td>
                  <td class="px-4 py-3">
                    <div class="flex items-center gap-1.5">
                      <span class="font-bold font-mono" style="color: var(--color-text);">{{ item.specimenNo }}</span>
                      <span v-if="item.isOnSite"
                            class="material-symbols-outlined cursor-default"
                            style="color: var(--color-warning); font-size: 16px;"
                            title="On-Site / Mission">
                        location_on
                      </span>
                    </div>
                  </td>
                  <td class="px-4 py-3">
                    <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName ?? '—' }}</p>
                    <p v-if="item.patientID" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.patientID }}</p>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                    <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                  </td>
                  <td class="px-4 py-3">
                    <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                    <span v-else class="text-xs italic" style="color: var(--color-text-muted);">—</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                          style="background-color: rgba(217,119,6,0.1); color: var(--color-warning);">
                      {{ item.tests.length }} running
                    </span>
                  </td>
                  <td class="px-4 py-3" @click.stop>
                    <button class="p-1.5 rounded-lg transition-all"
                            :style="item.remarks ? 'color: var(--color-warning);' : 'color: var(--color-text-muted); opacity: 0.4;'"
                            :disabled="!item.remarks" @click="openRemarks(item)">
                      <span class="material-symbols-outlined text-sm">{{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}</span>
                    </button>
                  </td>
                </tr>
                <Transition name="expand">
                  <tr v-if="expandedId === item.headerId" :key="`exp-${item.headerId}`">
                    <td colspan="7" class="px-0 py-0">
                      <div class="mx-4 mb-3 rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border);">
                        <table class="w-full text-xs">
                          <thead>
                            <tr style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border);">
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Run Date &amp; Time</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="test in item.tests" :key="test.id" style="border-top: 1px solid var(--color-border);">
                              <td class="px-4 py-2.5"><span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span></td>
                              <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                              <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ formatDt(test.assigned) }}</td>
                              <td class="px-4 py-2.5">
                                <span v-if="test.runAt" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(test.runAt) }}</span>
                                <span v-else class="text-xs italic" style="color: var(--color-text-muted);">—</span>
                              </td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </td>
                  </tr>
                </Transition>
              </template>
            </tbody>
          </table>
        </div>
      </div>

    </template>

    <!-- ══════════════════════════════════════════════════════════════════════
       ADMIN VIEW — all sections grouped
  ══════════════════════════════════════════════════════════════════════ -->
    <template v-else>

      <div v-if="adminLoading" class="flex flex-col gap-4">
        <div v-for="i in 3" :key="i" class="rounded-2xl p-6 animate-pulse"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow); height: 120px;"></div>
      </div>

      <div v-else-if="!adminFilteredGroups.length" class="rounded-2xl p-16 flex flex-col items-center gap-3"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">labs</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
        <p class="text-xs" style="color: var(--color-text-muted);">No tests are currently running across all sections.</p>
      </div>

      <div v-else class="flex flex-col gap-5">
        <div v-for="group in adminFilteredGroups" :key="group.sectionCode"
             class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Section header -->
          <div class="px-6 py-3 flex items-center gap-3 cursor-pointer select-none"
               style="background-color: var(--color-primary-soft); border-bottom: 1.5px solid var(--color-border);"
               @click="toggleCollapse(group.sectionCode)">
            <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">science</span>
            <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</h2>
            <span class="ml-auto text-[10px] font-bold px-2.5 py-1 rounded-full"
                  style="background-color: rgba(217,119,6,0.15); color: var(--color-warning);">
              {{ group.specimens.reduce((sum, s) => sum + s.tests.length, 0) }} running
            </span>
            <span class="material-symbols-outlined text-sm transition-transform"
                  :style="collapsedSections.has(group.sectionCode)
          ? 'color: var(--color-primary); transform: rotate(-90deg);'
          : 'color: var(--color-primary); transform: rotate(0deg);'">
              expand_more
            </span>
          </div>
          <!-- Table -->
          <Transition name="expand">
            <div v-show="!collapsedSections.has(group.sectionCode)" class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr style="border-bottom: 1.5px solid var(--color-border);">
                    <th class="w-8 px-4 py-3"></th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tests Running</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
                  </tr>
                </thead>
                <tbody>
                  <template v-for="item in group.filteredSpecimens" :key="item.headerId">
                    <tr class="transition-colors cursor-pointer"
                        :style="adminExpandedKey === `${group.sectionCode}-${item.headerId}` ? 'background-color: var(--color-primary-soft);' : 'background-color: transparent;'"
                        @mouseenter="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.headerId}`) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                        @mouseleave="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.headerId}`) e.currentTarget.style.backgroundColor = 'transparent' }"
                        @click="adminToggleExpand(group.sectionCode, item)"
                        style="border-top: 1px solid var(--color-border);">
                      <td class="px-4 py-3">
                        <span class="material-symbols-outlined text-sm transition-transform duration-200"
                              :style="{ color: 'var(--color-text-muted)', display: 'block', transform: adminExpandedKey === `${group.sectionCode}-${item.headerId}` ? 'rotate(90deg)' : 'rotate(0deg)' }">
                          chevron_right
                        </span>
                      </td>
                      <td class="px-4 py-3">
                        <div class="flex items-center gap-1.5">
                          <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                          <span v-if="item.isOnSite"
                                class="material-symbols-outlined cursor-default"
                                style="color: var(--color-warning); font-size: 16px;"
                                title="On-Site / Mission">
                            location_on
                          </span>
                        </div>
                      </td>
                      <td class="px-4 py-3">
                        <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName ?? '—' }}</p>
                        <p v-if="item.patientID" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.patientID }}</p>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                        <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                      </td>
                      <td class="px-4 py-3">
                        <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                        <span v-else class="text-xs italic" style="color: var(--color-text-muted);">—</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                              style="background-color: rgba(217,119,6,0.1); color: var(--color-warning);">
                          {{ item.tests.length }} running
                        </span>
                      </td>
                      <td class="px-4 py-3" @click.stop>
                        <button class="p-1.5 rounded-lg transition-all"
                                :style="item.remarks ? 'color: var(--color-warning);' : 'color: var(--color-text-muted); opacity: 0.4;'"
                                :disabled="!item.remarks" @click="openRemarks(item)">
                          <span class="material-symbols-outlined text-sm">{{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}</span>
                        </button>
                      </td>
                    </tr>
                    <Transition name="expand">
                      <tr v-if="adminExpandedKey === `${group.sectionCode}-${item.headerId}`"
                          :key="`exp-${group.sectionCode}-${item.headerId}`">
                        <td colspan="7" class="px-0 py-0">
                          <div class="mx-4 mb-3 rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border);">
                            <table class="w-full text-xs">
                              <thead>
                                <tr style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border);">
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Run Date &amp; Time</th>
                                </tr>
                              </thead>
                              <tbody>
                                <tr v-for="test in item.tests" :key="test.id" style="border-top: 1px solid var(--color-border);">
                                  <td class="px-4 py-2.5"><span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span></td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                                  <td class="px-4 py-2.5">
                                    <span v-if="test.assignedRMT" class="text-xs font-bold px-2 py-0.5 rounded-lg"
                                          style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                                      {{ test.assignedRMT }}
                                    </span>
                                    <span v-else class="text-xs" style="color: var(--color-text-muted);">—</span>
                                  </td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ formatDt(test.assigned) }}</td>
                                  <td class="px-4 py-2.5">
                                    <span v-if="test.runAt" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(test.runAt) }}</span>
                                    <span v-else class="text-xs italic" style="color: var(--color-text-muted);">—</span>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </div>
                        </td>
                      </tr>
                    </Transition>
                  </template>
                </tbody>
              </table>
            </div>
          </Transition>

        </div>
      </div>

    </template>

    <RemarkViewer :isVisible="remarkViewer.visible"
                  title="Specimen Remarks"
                  :text="remarkViewer.text"
                  @close="remarkViewer.visible = false" />

    <!-- Alert Modal -->
    <AlertModal :isVisible="alert.isVisible" :type="alert.type" :title="alert.title" :message="alert.message"
                @close="alert.isVisible = false" @confirm="alert.isVisible = false" />
  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import RemarkViewer from '@/components/common/RemarkViewer.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'

  const authStore = useAuthStore()
  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) { alert.value = { isVisible: true, type, title, message } }
  const remarkViewer = ref({ visible: false, text: '' })

  function openRemarks(item) {
    remarkViewer.value = { visible: true, text: item.remarks ?? '' }
  }


  // ══════════════════════════════════════════════════════════════════════════
  // REGULAR / TEAM LEAD
  // ══════════════════════════════════════════════════════════════════════════

  const loading = ref(true)
  const specimens = ref([])
  const expandedId = ref(null)

  // ══════════════════════════════════════════════════════════════════════════
  // ADMIN
  // ══════════════════════════════════════════════════════════════════════════

  const adminLoading = ref(true)
  const adminGroups = ref([])
  const adminExpandedKey = ref(null)   // format: `${sectionCode}-${headerId}`
  const collapsedSections = ref(new Set())

  // ══════════════════════════════════════════════════════════════════════════
  // SHARED — search applies to both views
  // ══════════════════════════════════════════════════════════════════════════

  const searchQuery = ref('')

  const filteredSpecimens = computed(() => {
    const q = searchQuery.value.toLowerCase()
    if (!q) return specimens.value
    return specimens.value.filter(s =>
      s.specimenNo?.toLowerCase().includes(q) ||
      s.patientName?.toLowerCase().includes(q) ||
      s.patientID?.toLowerCase().includes(q)
    )
  })

    watch(searchQuery, (q) => {
        if (!q) return
        const next = new Set(collapsedSections.value)
        adminGroups.value.forEach(group => {
            const hasMatch = group.specimens.some(s =>
                s.specimenNo?.toLowerCase().includes(q.toLowerCase()) ||
                s.patientName?.toLowerCase().includes(q.toLowerCase()) ||
                s.patientID?.toLowerCase().includes(q.toLowerCase())
            )
            if (hasMatch) next.delete(group.sectionCode)
        })
        collapsedSections.value = next
    })

  const adminFilteredGroups = computed(() => {
    const q = searchQuery.value.toLowerCase()
    return adminGroups.value.map(group => ({
      ...group,
      filteredSpecimens: q
        ? group.specimens.filter(s =>
          s.specimenNo?.toLowerCase().includes(q) ||
          s.patientName?.toLowerCase().includes(q) ||
          s.patientID?.toLowerCase().includes(q)
        )
        : group.specimens,
    })).filter(g => g.filteredSpecimens.length > 0)
  })

  // Counts — drive the shared badges in the toolbar
  const totalSpecimens = computed(() =>
    authStore.isAdmin
      ? adminFilteredGroups.value.reduce((sum, g) => sum + g.filteredSpecimens.length, 0)
      : filteredSpecimens.value.length
  )

  const totalTests = computed(() =>
    authStore.isAdmin
      ? adminFilteredGroups.value.reduce((sum, g) => sum + g.filteredSpecimens.reduce((s2, sp) => s2 + sp.tests.length, 0), 0)
      : filteredSpecimens.value.reduce((sum, s) => sum + s.tests.length, 0)
  )

  // ══════════════════════════════════════════════════════════════════════════
  // EXPAND / COLLAPSE
  // ══════════════════════════════════════════════════════════════════════════

  function toggleExpand(item) {
    expandedId.value = expandedId.value === item.headerId ? null : item.headerId
  }

  function adminToggleExpand(sectionCode, item) {
    const key = `${sectionCode}-${item.headerId}`
    adminExpandedKey.value = adminExpandedKey.value === key ? null : key
  }

    function toggleCollapse(sectionCode) {
        const next = new Set(collapsedSections.value)
        if (next.has(sectionCode)) next.delete(sectionCode)
        else next.add(sectionCode)
        collapsedSections.value = next
    }

  // ══════════════════════════════════════════════════════════════════════════
  // LOAD
  // ══════════════════════════════════════════════════════════════════════════

  async function load() {
    if (!authStore.isAdmin) {
      loading.value = true; expandedId.value = null
      try { const d = await runnerApi.getRunningSpecimens(authStore.sectionCode); specimens.value = Array.isArray(d) ? d : [] }
      catch (e) { showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load running specimens.') }
      finally { loading.value = false }
    } else {
      adminLoading.value = true; adminExpandedKey.value = null
      try { const d = await runnerApi.getAdminRunning(); adminGroups.value = Array.isArray(d) ? d : [] }
      catch (e) { showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load running specimens.') }
      finally { adminLoading.value = false }
    }
  }

  async function silentRefresh() {
    try {
      if (!authStore.isAdmin) { const d = await runnerApi.getRunningSpecimens(authStore.sectionCode); specimens.value = Array.isArray(d) ? d : [] }
      else { const d = await runnerApi.getAdminRunning(); adminGroups.value = Array.isArray(d) ? d : [] }
    } catch { }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // FORMATTERS
  // ══════════════════════════════════════════════════════════════════════════

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' })
  }

  // ══════════════════════════════════════════════════════════════════════════
  // MOUNT / UNMOUNT
  // ══════════════════════════════════════════════════════════════════════════

  let refreshInterval = null
  onMounted(() => { load(); refreshInterval = setInterval(silentRefresh, 10000) })
  onUnmounted(() => { clearInterval(refreshInterval) })
</script>
