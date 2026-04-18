<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">
        Incoming Specimens
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span> · {{ authStore.branchCode }}
      </p>
    </div>

    <!-- Main Card -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Tabs + Filter Row -->
      <div class="px-8 py-4 flex items-center justify-between gap-6"
           style="border-bottom: 1px solid var(--color-surface-low);">

        <!-- View Tabs -->
        <div class="flex">
          <button v-for="tab in tabs"
                  :key="tab.key"
                  class="px-6 py-3 text-xs font-bold uppercase tracking-widest transition-all relative"
                  :style="activeTab === tab.key
                    ? 'color: var(--color-primary);'
                    : 'color: var(--color-text-muted);'"
                  @click="switchTab(tab.key)">
            {{ tab.label }}
            <span v-if="activeTab === tab.key"
                  class="absolute bottom-0 left-0 w-full h-0.5"
                  style="background-color: var(--color-primary);"></span>
            <!-- Count badge -->
            <span class="ml-2 px-2 py-0.5 rounded-full text-[10px] font-bold"
                  :style="activeTab === tab.key
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
              {{ tab.key === 'batch' ? filteredBatches.length : filteredSpecimens.length }}
            </span>
          </button>
        </div>

        <!-- Filters -->
        <div class="flex items-center gap-3">

          <!-- Location Filter -->
          <div>
            <DropdownSelect v-model="locationFilter"
                            icon="location_on"
                            placeholder="All Locations"
                            :options="locationOptions"
                            @change="onLocationFilterChange" />


          </div>

          <!-- Batch Filter (specimen view only) -->
          <div v-if="activeTab === 'specimen'">
            <DropdownSelect v-model="batchFilter"
                            icon="tag"
                            placeholder="All Batches"
                            :options="batchOptions" />
          </div>

          <!-- Refresh -->
          <button class="p-2.5 rounded-xl transition-all"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  :disabled="loading"
                  @mouseenter="e => e.currentTarget.style.color = 'var(--color-primary)'"
                  @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
                  @click="loadData">
            <span class="material-symbols-outlined text-sm"
                  :class="loading ? 'animate-spin' : ''">refresh</span>
          </button>

        </div>
      </div>

      <!-- ===== BATCH VIEW ===== -->
      <div v-if="activeTab === 'batch'">
        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Batch No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Location</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Date & Time Endorsed</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Endorsed By</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest text-center"
                    style="color: var(--color-text-muted);">Specimens</th>
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Status</th>
              </tr>
            </thead>
            <tbody>

              <!-- Loading -->
              <tr v-if="loading">
                <td colspan="6" class="px-8 py-16 text-center">
                  <div class="flex items-center justify-center gap-3">
                    <span class="material-symbols-outlined animate-spin"
                          style="color: var(--color-text-muted);">progress_activity</span>
                    <p class="text-xs font-bold uppercase tracking-widest"
                       style="color: var(--color-text-muted);">Loading...</p>
                  </div>
                </td>
              </tr>

              <!-- Empty -->
              <tr v-else-if="filteredBatches.length === 0">
                <td colspan="6" class="px-8 py-16 text-center">
                  <div class="flex flex-col items-center gap-3">
                    <span class="material-symbols-outlined text-4xl opacity-20"
                          style="color: var(--color-text-muted);">move_to_inbox</span>
                    <p class="text-sm font-bold"
                       style="color: var(--color-text-muted);">No incoming batches</p>
                  </div>
                </td>
              </tr>

              <!-- Rows -->
              <tr v-else
                  v-for="batch in filteredBatches"
                  :key="batch.batchNo"
                  class="cursor-pointer transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                  @click="openDrawer(batch.batchNo)">
                <td class="px-8 py-4 font-mono text-xs font-bold"
                    style="color: var(--color-primary);">
                  {{ batch.batchNo }}
                </td>
                <td class="px-4 py-4 text-sm"
                    style="color: var(--color-text-muted);">
                  {{ batch.locationName }}
                </td>
                <td class="px-4 py-4 text-sm"
                    style="color: var(--color-text-muted);">
                  {{ formatDateTime(batch.endorsed) }}
                </td>
                <td class="px-4 py-4 text-sm font-bold"
                    style="color: var(--color-text);">
                  {{ batch.endorsedBy }}
                </td>
                <td class="px-4 py-4 text-center">
                  <span class="text-xs font-bold px-3 py-1 rounded-full"
                        :style="batch.receivedSpecimens === batch.totalSpecimens && batch.totalSpecimens >
                    0
                    ? 'background-color: var(--color-success-soft); color: var(--color-success);'
                    : batch.receivedSpecimens > 0
                    ? 'background-color: rgba(37,99,235,0.08); color: #2563eb;'
                    : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                    {{ batch.receivedSpecimens }}/{{ batch.totalSpecimens }}
                  </span>
                </td>
                <td class="px-8 py-4">
                  <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                        :style="getBatchStatusStyle(batch.status)">
                    <span class="w-1.5 h-1.5 rounded-full"
                          :style="`background-color: ${getBatchStatusDot(batch.status)}`"></span>
                    {{ getBatchStatusLabel(batch.status) }}
                  </span>
                </td>
              </tr>

            </tbody>
          </table>
        </div>
      </div>

      <!-- ===== SPECIMEN VIEW ===== -->
      <div v-if="activeTab === 'specimen'">
        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Batch No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Location</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Specimen No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Patient ID</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Patient Name</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted);">Sample Type</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest text-center"
                    style="color: var(--color-text-muted);">Remarks</th>
              </tr>
            </thead>
            <tbody>

              <!-- Loading -->
              <tr v-if="loading">
                <td colspan="7" class="px-8 py-16 text-center">
                  <div class="flex items-center justify-center gap-3">
                    <span class="material-symbols-outlined animate-spin"
                          style="color: var(--color-text-muted);">progress_activity</span>
                    <p class="text-xs font-bold uppercase tracking-widest"
                       style="color: var(--color-text-muted);">Loading...</p>
                  </div>
                </td>
              </tr>

              <!-- Empty -->
              <tr v-else-if="filteredSpecimens.length === 0">
                <td colspan="7" class="px-8 py-16 text-center">
                  <div class="flex flex-col items-center gap-3">
                    <span class="material-symbols-outlined text-4xl opacity-20"
                          style="color: var(--color-text-muted);">biotech</span>
                    <p class="text-sm font-bold"
                       style="color: var(--color-text-muted);">No incoming specimens</p>
                  </div>
                </td>
              </tr>

              <!-- Rows -->
              <tr v-else
                  v-for="sp in filteredSpecimens"
                  :key="`${sp.batchNo}-${sp.specimenNo}`"
                  class="transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                <td class="px-8 py-4 font-mono text-xs font-bold"
                    style="color: var(--color-primary);">
                  {{ sp.batchNo }}
                </td>
                <td class="px-4 py-4 text-sm"
                    style="color: var(--color-text-muted);">
                  {{ sp.locationName }}
                </td>
                <td class="px-4 py-4 font-mono text-xs font-bold"
                    style="color: var(--color-text);">
                  {{ sp.specimenNo }}
                </td>
                <td class="px-4 py-4 text-sm"
                    style="color: var(--color-text-muted);">
                  {{ sp.pid }}
                </td>
                <td class="px-4 py-4 text-sm font-medium"
                    style="color: var(--color-text);">
                  {{ sp.patientName }}
                </td>
                <td class="px-4 py-4 text-sm"
                    style="color: var(--color-text-muted);">
                  {{ sp.sampleTypeName }}
                </td>
                <td class="px-4 py-4 text-center">
                  <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                          :style="sp.remarks
                            ? 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                          :title="sp.remarks || 'No remarks'"
                          :disabled="!sp.remarks"
                          @click="viewRemark(sp.remarks)">
                    <span class="material-symbols-outlined text-sm">
                      {{ sp.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                    </span>
                  </button>
                </td>
              </tr>

            </tbody>
          </table>
        </div>
      </div>

    </div>

    <!-- Remark Viewer -->
    <div v-if="remarkViewer.visible"
         class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
           @click="remarkViewer.visible = false"></div>
      <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4"`
           style="background-color: var(--color-surface);">
        <div class="flex items-center gap-2">
          <div class="p-2 rounded-xl"
               style="background-color: rgba(217,119,6,0.1);">
            <span class="material-symbols-outlined text-sm"
                  style="color: var(--color-warning);">chat_bubble</span>
          </div>
          <h3 class="text-sm font-bold" style="color: var(--color-text);">Endorsement Remarks</h3>
        </div>
        <p class="text-sm rounded-xl p-4 whitespace-pre-wrap"
           style="background-color: var(--color-surface-low); color: var(--color-text);">
          {{ remarkViewer.text }}
        </p>
        <button class="w-full py-3 rounded-xl text-xs font-bold uppercase tracking-widest"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="remarkViewer.visible = false">
          Close
        </button>
      </div>
    </div>

    <!-- Batch Detail Drawer -->
    <BatchDetailDrawer :isOpen="drawerOpen"
                       :loading="drawerLoading"
                       :data="drawerData"
                       @close="closeDrawer" />

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
import { ref, computed, onMounted } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import BatchDetailDrawer from '@/components/common/BatchDetailDrawer.vue'
import DropdownSelect from '@/components/common/DropdownSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { receivingApi } from '@/api/receivingApi'
import { batchApi } from '@/api/batchApi'

const authStore = useAuthStore()

// ── Alert ──────────────────────────────────────────────────────────────────

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

// ── Tabs ───────────────────────────────────────────────────────────────────

const activeTab = ref('specimen')
const tabs = [
  { key: 'specimen', label: '🔬 View by Specimen' },
  { key: 'batch',    label: '📦 View by Batch'    },
]

function switchTab(key) {
  activeTab.value = key
  batchFilter.value = ''
}

// ── Filters ────────────────────────────────────────────────────────────────

const locationFilter = ref('')
const batchFilter    = ref('')

const availableLocations = computed(() => {
  const source = activeTab.value === 'batch'
    ? (Array.isArray(batches.value) ? batches.value : [])
    : (Array.isArray(specimens.value) ? specimens.value : [])

  const seen = new Map()
  for (const item of source) {
    if (!seen.has(item.location)) {
      seen.set(item.location, { code: item.location, name: item.locationName })
    }
  }
  return Array.from(seen.values()).sort((a, b) => a.name.localeCompare(b.name))
})

  const locationOptions = computed(() => [
    { value: '', label: 'All Locations' },
    ...availableLocations.value.map(loc => ({
      value: loc.code,
      label: loc.name,
    }))
  ])


// Batch nos available in specimen view — filtered by location if set
const availableBatchNos = computed(() => {
  const source = Array.isArray(specimens.value) ? specimens.value : []
  const filtered = locationFilter.value
    ? source.filter(s => s.location === locationFilter.value)
    : source
  return [...new Set(filtered.map(s => s.batchNo))].sort()
})

  const batchOptions = computed(() => [
    { value: '', label: 'All Batches' },
    ...availableBatchNos.value.map(batchNo => ({
      value: batchNo,
      label: batchNo,
    }))
  ])

function onLocationFilterChange() {
  // Reset batch filter when location changes
  batchFilter.value = ''
}

const filteredBatches = computed(() => {
  if (!locationFilter.value) return batches.value
  return batches.value.filter(b => b.location === locationFilter.value)
})

const filteredSpecimens = computed(() => {
  let result = specimens.value
  if (locationFilter.value)
    result = result.filter(s => s.location === locationFilter.value)
  if (batchFilter.value)
    result = result.filter(s => s.batchNo === batchFilter.value)
  return result
})

// ── Data ───────────────────────────────────────────────────────────────────

const loading   = ref(false)
const batches   = ref([])
const specimens = ref([])

async function loadData() {
  loading.value = true
  try {
    const [batchData, specimenData] = await Promise.all([
      receivingApi.getIncomingBatches(authStore.sectionCode),
      receivingApi.getIncomingSpecimens(authStore.sectionCode),
    ])
    batches.value = Array.isArray(batchData) ? batchData : []
    specimens.value = Array.isArray(specimenData) ? specimenData : []

    console.log('batches:', batches.value.length)
    console.log('specimens:', specimens.value.length)
    console.log('first specimen:', specimens.value[0])
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Load Failed', 'Unable to load incoming specimens.')
    }
  } finally {
    loading.value = false
  }
}

// ── Remark Viewer ──────────────────────────────────────────────────────────

const remarkViewer = ref({ visible: false, text: '' })

function viewRemark(text) {
  if (!text) return
  remarkViewer.value = { visible: true, text }
}

// ── Drawer ─────────────────────────────────────────────────────────────────

const drawerOpen    = ref(false)
const drawerLoading = ref(false)
const drawerData    = ref(null)

async function openDrawer(batchNo) {
  drawerOpen.value    = true
  drawerLoading.value = true
  drawerData.value    = null
  try {
    drawerData.value = await batchApi.getBatchDetail(batchNo)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load batch details.')
    }
  } finally {
    drawerLoading.value = false
  }
}

function closeDrawer() {
  drawerOpen.value = false
  drawerData.value = null
}

// ── Helpers ────────────────────────────────────────────────────────────────

function formatDateTime(dt) {
  if (!dt) return '—'
  return new Date(dt).toLocaleString('en-US', {
    month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', hour12: true
  })
}

function getBatchStatusLabel(status) {
  const map = { P: 'Pending', PA: 'Partial', C: 'Completed' }
  return map[status] ?? status
}

function getBatchStatusStyle(status) {
  const map = {
    P:  'background-color: var(--color-warning-soft); color: var(--color-warning);',
    PA: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
    C:  'background-color: var(--color-success-soft); color: var(--color-success);',
  }
  return map[status] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
}

function getBatchStatusDot(status) {
  const map = { P: 'var(--color-warning)', PA: '#2563eb', C: 'var(--color-success)' }
  return map[status] ?? 'var(--color-text-muted)'
}

// ── Lifecycle ──────────────────────────────────────────────────────────────

onMounted(loadData)
</script>
