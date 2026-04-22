<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Pending Specimens</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span> · {{ authStore.branchCode }}
        </p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
              style="background: var(--color-primary-gradient); color: #fff;"
              @click="load">
        <span class="material-symbols-outlined text-sm">refresh</span>
        Refresh
      </button>
    </div>

    <!-- ═════════════════════════════════════════════════════════════════════
         REGULAR / TEAM LEAD VIEW
    ══════════════════════════════════════════════════════════════════════ -->
    <template v-if="!authStore.isAdmin">

      <!-- Search -->
      <div class="mb-4 flex gap-3 flex-wrap">
        <div class="relative flex-1 min-w-[200px]">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
                style="color: var(--color-text-muted);">search</span>
          <input v-model="searchQuery"
                 type="text"
                 placeholder="Search specimen no., patient name..."
                 class="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                 style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        </div>
        <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
             style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">pending_actions</span>
          {{ filteredSpecimens.length }} specimen{{ filteredSpecimens.length !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- Table -->
      <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <!-- Loading skeleton -->
        <div v-if="loading" class="p-6 flex flex-col gap-3">
          <div v-for="i in 5" :key="i" class="h-14 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
        </div>

        <!-- Empty state -->
        <div v-else-if="!filteredSpecimens.length" class="p-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">inbox</span>
          <p class="text-sm font-bold" style="color: var(--color-text);">No pending specimens</p>
          <p class="text-xs" style="color: var(--color-text-muted);">All specimens for this section have been processed.</p>
        </div>

        <!-- Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1.5px solid var(--color-border);">
                <th class="w-8 px-4 py-3"></th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Routed</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="item in filteredSpecimens" :key="item.id">
                <!-- Main row -->
                <tr class="transition-colors cursor-pointer"
                    :style="expandedId === item.id
                      ? 'background-color: var(--color-primary-soft);'
                      : 'background-color: transparent;'"
                    @mouseenter="e => { if (expandedId !== item.id) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                    @mouseleave="e => { if (expandedId !== item.id) e.currentTarget.style.backgroundColor = 'transparent' }"
                    @click="toggleExpand(item)">
                  <td class="px-4 py-3">
                    <span class="material-symbols-outlined text-sm transition-transform duration-200"
                          :style="{ color: 'var(--color-text-muted)', transform: expandedId === item.id ? 'rotate(90deg)' : 'rotate(0deg)' }">
                      chevron_right
                    </span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</span>
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
                    <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                  </td>
                  <td class="px-4 py-3">
                    <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                    <span v-else class="text-xs italic" style="color: var(--color-text-muted);">Not yet received</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                          :style="headerStatusStyle(item.status)">
                      {{ headerStatusLabel(item.status) }}
                    </span>
                  </td>
                  <td class="px-4 py-3" @click.stop>
                    <button class="p-1.5 rounded-lg transition-all"
                            :style="item.remarks
                              ? 'color: var(--color-warning);'
                              : 'color: var(--color-text-muted); opacity: 0.4;'"
                            :disabled="!item.remarks"
                            @click="openRemarks(item)">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>
                </tr>

                <!-- Expanded test rows -->
                <Transition name="expand">
                  <tr v-if="expandedId === item.id" :key="`exp-${item.id}`">
                    <td colspan="8" class="px-0 py-0">
                      <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                           style="border: 1.5px solid var(--color-border);">
                        <div v-if="testsLoading" class="p-4 flex flex-col gap-2">
                          <div v-for="j in 3" :key="j" class="h-8 rounded-lg animate-pulse"
                               style="background-color: var(--color-surface-low);"></div>
                        </div>
                        <table v-else class="w-full text-xs">
                          <thead>
                            <tr style="background-color: var(--color-surface-low);">
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="test in expandedTests" :key="test.id"
                                style="border-top: 1px solid var(--color-border);">
                              <td class="px-4 py-2.5">
                                <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                              </td>
                              <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                              <td class="px-4 py-2.5">
                                <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                      :style="testStatusStyle(test.status)">
                                  {{ testStatusLabel(test.status) }}
                                </span>
                              </td>
                              <td class="px-4 py-2.5">
                                <span v-if="test.scheduleTag" class="font-bold" :style="scheduleTagStyle(test.scheduleTag)">
                                  {{ test.scheduleTag }}
                                  <span v-if="test.runningDate" style="color: var(--color-text-muted);"> — {{ test.runningDate }}</span>
                                </span>
                                <span v-else style="color: var(--color-text-muted);">—</span>
                              </td>
                              <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT ?? '—' }}</td>
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

      <!-- Search + count -->
      <div class="mb-4 flex gap-3 flex-wrap">
        <div class="relative flex-1 min-w-[200px]">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
                style="color: var(--color-text-muted);">search</span>
          <input v-model="adminSearchQuery"
                 type="text"
                 placeholder="Search specimen no., patient name..."
                 class="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                 style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        </div>
        <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
             style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">pending_actions</span>
          {{ adminTotalCount }} specimen{{ adminTotalCount !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- Loading -->
      <div v-if="adminLoading" class="flex flex-col gap-4">
        <div v-for="i in 3" :key="i" class="rounded-2xl p-6 animate-pulse"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow); height: 120px;"></div>
      </div>

      <!-- Empty -->
      <div v-else-if="!adminFilteredGroups.length"
           class="rounded-2xl p-16 flex flex-col items-center gap-3"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">inbox</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No pending specimens</p>
        <p class="text-xs" style="color: var(--color-text-muted);">All sections are clear.</p>
      </div>

      <!-- Grouped tables -->
      <div v-else class="flex flex-col gap-5">
        <div v-for="group in adminFilteredGroups" :key="group.sectionCode"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Section header -->
          <div class="px-6 py-3 flex items-center gap-3"
               style="background-color: var(--color-primary-soft); border-bottom: 1.5px solid var(--color-border);">
            <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">science</span>
            <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</h2>
            <span class="ml-auto text-[10px] font-bold px-2.5 py-1 rounded-full"
                  style="background-color: rgba(70,21,153,0.15); color: var(--color-primary);">
              {{ group.filteredSpecimens.length }} specimen{{ group.filteredSpecimens.length !== 1 ? 's' : '' }}
            </span>
          </div>

          <!-- Table -->
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr style="border-bottom: 1.5px solid var(--color-border);">
                  <th class="w-8 px-4 py-3"></th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Routed</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="item in group.filteredSpecimens" :key="item.id">

                  <!-- Main row -->
                  <tr class="transition-colors cursor-pointer"
                      :style="adminExpandedKey === `${group.sectionCode}-${item.id}`
                        ? 'background-color: var(--color-primary-soft);'
                        : 'background-color: transparent;'"
                      @mouseenter="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.id}`) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                      @mouseleave="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.id}`) e.currentTarget.style.backgroundColor = 'transparent' }"
                      @click="adminToggleExpand(group.sectionCode, item)"
                      style="border-top: 1px solid var(--color-border);">
                    <td class="px-4 py-3">
                      <span class="material-symbols-outlined text-sm transition-transform duration-200"
                            :style="{ color: 'var(--color-text-muted)', transform: adminExpandedKey === `${group.sectionCode}-${item.id}` ? 'rotate(90deg)' : 'rotate(0deg)' }">
                        chevron_right
                      </span>
                    </td>
                    <td class="px-4 py-3">
                      <span class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</span>
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
                      <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                    </td>
                    <td class="px-4 py-3">
                      <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                      <span v-else class="text-xs italic" style="color: var(--color-text-muted);">Not yet received</span>
                    </td>
                    <td class="px-4 py-3">
                      <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                            :style="headerStatusStyle(item.status)">
                        {{ headerStatusLabel(item.status) }}
                      </span>
                    </td>
                    <td class="px-4 py-3" @click.stop>
                      <button class="p-1.5 rounded-lg transition-all"
                              :style="item.remarks
                                ? 'color: var(--color-warning);'
                                : 'color: var(--color-text-muted); opacity: 0.4;'"
                              :disabled="!item.remarks"
                              @click="openRemarks(item)">
                        <span class="material-symbols-outlined text-sm">
                          {{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                        </span>
                      </button>
                    </td>
                  </tr>

                  <!-- Expanded test rows -->
                  <Transition name="expand">
                    <tr v-if="adminExpandedKey === `${group.sectionCode}-${item.id}`"
                        :key="`exp-${group.sectionCode}-${item.id}`">
                      <td colspan="8" class="px-0 py-0">
                        <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                             style="border: 1.5px solid var(--color-border);">
                          <div v-if="adminTestsLoading" class="p-4 flex flex-col gap-2">
                            <div v-for="j in 3" :key="j" class="h-8 rounded-lg animate-pulse"
                                 style="background-color: var(--color-surface-low);"></div>
                          </div>
                          <table v-else class="w-full text-xs">
                            <thead>
                              <tr style="background-color: var(--color-surface-low);">
                                <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                                <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                                <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                                <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                                <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="test in adminExpandedTests" :key="test.id"
                                  style="border-top: 1px solid var(--color-border);">
                                <td class="px-4 py-2.5">
                                  <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                                </td>
                                <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                                <td class="px-4 py-2.5">
                                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                        :style="testStatusStyle(test.status)">
                                    {{ testStatusLabel(test.status) }}
                                  </span>
                                </td>
                                <td class="px-4 py-2.5">
                                  <span v-if="test.scheduleTag" class="font-bold" :style="scheduleTagStyle(test.scheduleTag)">
                                    {{ test.scheduleTag }}
                                    <span v-if="test.runningDate" style="color: var(--color-text-muted);"> — {{ test.runningDate }}</span>
                                  </span>
                                  <span v-else style="color: var(--color-text-muted);">—</span>
                                </td>
                                <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT ?? '—' }}</td>
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
      </div>

    </template>

    <!-- Remarks Modal (shared) -->
    <RemarkModal :isVisible="remarksModal.visible"
                 title="Specimen Remarks"
                 :initialText="remarksModal.text"
                 @save="remarksModal.visible = false"
                 @cancel="remarksModal.visible = false"
                 @close="remarksModal.visible = false" />

    <!-- Alert Modal -->
    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />
  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import RemarkModal from '@/components/common/RemarkModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'

  const authStore = useAuthStore()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  const remarksModal = ref({ visible: false, text: '' })
  function openRemarks(item) {
    remarksModal.value = { visible: true, text: item.remarks ?? '' }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // REGULAR / TEAM LEAD
  // ══════════════════════════════════════════════════════════════════════════

  const loading = ref(true)
  const testsLoading = ref(false)
  const specimens = ref([])
  const searchQuery = ref('')
  const expandedId = ref(null)
  const expandedTests = ref([])

  const filteredSpecimens = computed(() => {
    const q = searchQuery.value.toLowerCase()
    if (!q) return specimens.value
    return specimens.value.filter(s =>
      s.specimenNo?.toLowerCase().includes(q) ||
      s.patientName?.toLowerCase().includes(q) ||
      s.patientID?.toLowerCase().includes(q)
    )
  })

  async function toggleExpand(item) {
    if (expandedId.value === item.id) {
      expandedId.value = null
      expandedTests.value = []
      return
    }
    expandedId.value = item.id
    expandedTests.value = []
    testsLoading.value = true
    try {
      expandedTests.value = await runnerApi.getTestsByHeader(item.id)
    } catch (e) {
      showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load tests.')
    } finally {
      testsLoading.value = false
    }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // ADMIN
  // ══════════════════════════════════════════════════════════════════════════

  const adminLoading = ref(true)
  const adminTestsLoading = ref(false)
  const adminGroups = ref([])
  const adminSearchQuery = ref('')
  const adminExpandedKey = ref(null)   // format: `${sectionCode}-${itemId}`
  const adminExpandedTests = ref([])

  const adminFilteredGroups = computed(() => {
    const q = adminSearchQuery.value.toLowerCase()
    return adminGroups.value
      .map(group => ({
        ...group,
        filteredSpecimens: q
          ? group.specimens.filter(s =>
            s.specimenNo?.toLowerCase().includes(q) ||
            s.patientName?.toLowerCase().includes(q) ||
            s.patientID?.toLowerCase().includes(q)
          )
          : group.specimens,
      }))
      .filter(group => group.filteredSpecimens.length > 0)
  })

  const adminTotalCount = computed(() =>
    adminFilteredGroups.value.reduce((sum, g) => sum + g.filteredSpecimens.length, 0)
  )

  async function adminToggleExpand(sectionCode, item) {
    const key = `${sectionCode}-${item.id}`
    if (adminExpandedKey.value === key) {
      adminExpandedKey.value = null
      adminExpandedTests.value = []
      return
    }
    adminExpandedKey.value = key
    adminExpandedTests.value = []
    adminTestsLoading.value = true
    try {
      adminExpandedTests.value = await runnerApi.getTestsByHeader(item.id)
    } catch (e) {
      showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load tests.')
    } finally {
      adminTestsLoading.value = false
    }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // LOAD
  // ══════════════════════════════════════════════════════════════════════════

  async function load() {
    if (!authStore.isAdmin) {
      loading.value = true
      expandedId.value = null
      expandedTests.value = []
      try {
        const data = await runnerApi.getPendingSpecimens(authStore.sectionCode)
        specimens.value = Array.isArray(data) ? data : []
      } catch (e) {
        showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load specimens.')
      } finally {
        loading.value = false
      }
    } else {
      adminLoading.value = true
      adminExpandedKey.value = null
      adminExpandedTests.value = []
      try {
        const data = await runnerApi.getAdminPending()
        adminGroups.value = Array.isArray(data) ? data : []
      } catch (e) {
        showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load specimens.')
      } finally {
        adminLoading.value = false
      }
    }
  }

  async function silentRefresh() {
    try {
      if (!authStore.isAdmin) {
        const data = await runnerApi.getPendingSpecimens(authStore.sectionCode)
        specimens.value = Array.isArray(data) ? data : []
      } else {
        const data = await runnerApi.getAdminPending()
        adminGroups.value = Array.isArray(data) ? data : []
      }
    } catch {
      // fail silently
    }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // FORMATTERS
  // ══════════════════════════════════════════════════════════════════════════

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' })
  }

  function headerStatusLabel(s) {
    return { P: 'Pending', S: 'Saved', C: 'Completed' }[s] ?? s
  }

  function headerStatusStyle(s) {
    const map = {
      P: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
      S: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
      C: 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);',
    }
    return map[s] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function testStatusLabel(s) {
    return { P: 'Pending', S: 'Saved', R: 'Running', X: 'Released' }[s] ?? s
  }

  function testStatusStyle(s) {
    const map = {
      P: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
      S: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
      R: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
      X: 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);',
    }
    return map[s] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function scheduleTagStyle(tag) {
    const map = {
      ERD: 'color: var(--color-primary);',
      CRD: 'color: var(--color-warning);',
      SRD: 'color: var(--color-info, #4a626d);',
    }
    return map[tag] ?? 'color: var(--color-text-muted);'
  }

  // ══════════════════════════════════════════════════════════════════════════
  // MOUNT / UNMOUNT
  // ══════════════════════════════════════════════════════════════════════════

  let refreshInterval = null

  onMounted(() => {
    load()
    refreshInterval = setInterval(silentRefresh, 10000)
  })

  onUnmounted(() => {
    clearInterval(refreshInterval)
  })
</script>

<style scoped>
  .expand-enter-active,
  .expand-leave-active {
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    overflow: hidden;
  }

  .expand-enter-from,
  .expand-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  .expand-enter-to,
  .expand-leave-from {
    opacity: 1;
    transform: translateY(0);
  }
</style>
