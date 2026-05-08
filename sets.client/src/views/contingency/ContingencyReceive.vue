<template>
  <AppLayout>
    <div class="p-6 space-y-6">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-3xl" style="color: var(--color-warning)">offline_bolt</span>
        <div>
          <h1 class="text-xl font-bold" style="color: var(--color-text)">Contingency Receiving</h1>
          <p class="text-xs" style="color: var(--color-text-muted)">Receive specimens from contingency endorsements</p>
        </div>
        <div class="ml-auto flex items-center gap-3">
          <div class="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest"
               style="background-color: rgba(217,119,6,0.15); color: var(--color-warning)">
            ⚡ Contingency Mode
          </div>
          <label class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest cursor-pointer transition-all active:scale-95 flex items-center gap-2"
                 style="background-color: var(--color-surface); color: var(--color-text); border: 1px solid var(--color-border)">
            <span class="material-symbols-outlined text-sm">upload_file</span>
            Import Excel
            <input type="file" accept=".xlsx" class="hidden" @change="importExcel" />
          </label>
        </div>
      </div>

      <!-- HCLAB Warning Banner -->
      <Transition name="banner-slide">
        <div v-if="showWarning"
             class="rounded-2xl p-5 flex items-start gap-4"
             style="background-color: rgba(186,26,26,0.08); border: 1.5px solid rgba(186,26,26,0.35)">
          <span class="material-symbols-outlined text-2xl flex-shrink-0 mt-0.5"
                style="color: #ba1a1a">warning</span>
          <div class="flex-1">
            <p class="font-bold text-sm" style="color: #ba1a1a">Contingency Mode Warning</p>
            <p class="text-xs mt-1 leading-relaxed" style="color: #ba1a1a">
              This feature is intended <strong>only for emergency use</strong> when HCLAB is down or offline.
              Using this outside of an actual HCLAB outage may cause data inconsistencies and tracking issues.
              Please close this page if HCLAB is currently accessible.
            </p>
          </div>
          <button @click="showWarning = false"
                  class="flex-shrink-0 w-7 h-7 rounded-lg flex items-center justify-center transition-all active:scale-95"
                  style="background-color: rgba(186,26,26,0.15); color: #ba1a1a">
            <span class="material-symbols-outlined text-sm">close</span>
          </button>
        </div>
      </Transition>

      <!-- Tabs -->
      <div class="flex gap-1 p-1 rounded-xl w-fit"
           style="background-color: var(--color-surface-low)">
        <button v-for="tab in tabs" :key="tab.key"
                @click="activeTab = tab.key"
                class="px-5 py-2 rounded-lg text-xs font-bold uppercase tracking-widest transition-all"
                :style="activeTab === tab.key
                  ? 'background-color: var(--color-surface); color: var(--color-text); box-shadow: 0 1px 3px var(--color-shadow)'
                  : 'color: var(--color-text-muted)'">
          {{ tab.label }}
          <span v-if="tab.key === 'pending'" class="ml-1.5 px-1.5 py-0.5 rounded-full text-[10px]"
                :style="activeTab === 'pending'
                  ? 'background-color: var(--color-warning-soft); color: var(--color-warning)'
                  : 'background-color: var(--color-surface); color: var(--color-text-muted)'">
            {{ pendingBatches.length }}
          </span>
          <span v-if="tab.key === 'received'" class="ml-1.5 px-1.5 py-0.5 rounded-full text-[10px]"
                :style="activeTab === 'received'
                  ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                  : 'background-color: var(--color-surface); color: var(--color-text-muted)'">
            {{ completedBatches.length }}
          </span>
        </button>
      </div>

      <!-- ── PENDING TAB ─────────────────────────────────────────────────── -->
      <div v-if="activeTab === 'pending'" class="grid grid-cols-12 gap-6">

        <!-- Batch list -->
        <div class="col-span-4 space-y-2">
          <h2 class="text-xs font-bold uppercase tracking-widest px-1"
              style="color: var(--color-text-muted)">
            Pending Batches
            <span class="ml-2 px-2 py-0.5 rounded-full text-xs"
                  style="background-color: var(--color-surface-low)">{{ pendingBatches.length }}</span>
          </h2>

          <div v-if="batchesLoading" class="text-center py-8 text-xs"
               style="color: var(--color-text-muted)">Loading...</div>

          <div v-else-if="pendingBatches.length === 0"
               class="rounded-xl p-6 text-center"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted)">inventory</span>
            <p class="text-xs mt-2" style="color: var(--color-text-muted)">No pending batches.</p>
          </div>

          <TransitionGroup name="batch-list" tag="div" class="space-y-2">
            <div v-for="batch in pendingBatches" :key="batch.id"
                 @click="selectBatch(batch)"
                 class="rounded-xl p-4 cursor-pointer transition-all"
                 :style="selectedBatch?.id === batch.id
                   ? 'background-color: var(--color-primary-soft); border: 1.5px solid var(--color-primary)'
                   : 'background-color: var(--color-surface); border: 1px solid var(--color-border)'">
              <div class="flex items-start justify-between">
                <div>
                  <p class="font-bold text-sm font-mono" style="color: var(--color-text)">{{ batch.batchNo }}</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">From: {{ batch.endorsingBranch }} / {{ batch.endorsingSection }}</p>
                  <p class="text-xs" style="color: var(--color-text-muted)">By: {{ batch.endorsedBy }}</p>
                  <p class="text-xs" style="color: var(--color-text-muted)">{{ formatDate(batch.endorsedAt) }}</p>
                </div>
                <div class="text-right">
                  <span class="px-2 py-0.5 rounded-full text-xs font-bold"
                        style="background-color: rgba(217,119,6,0.15); color: var(--color-warning)">
                    Endorsed
                  </span>
                  <p class="text-xs mt-1 font-bold" style="color: var(--color-text)">
                    {{ batch.receivedCount }}/{{ batch.totalSpecimens }}
                  </p>
                  <div class="mt-1 h-1.5 rounded-full overflow-hidden w-16 ml-auto"
                       style="background-color: var(--color-surface-low)">
                    <div class="h-full rounded-full transition-all"
                         :style="`width: ${batch.totalSpecimens > 0 ? (batch.receivedCount / batch.totalSpecimens * 100) : 0}%; background-color: var(--color-success)`"></div>
                  </div>
                  <span v-if="batch.isImported" class="text-xs" style="color: var(--color-text-muted)">📥 Imported</span>
                </div>
              </div>
            </div>
          </TransitionGroup>
        </div>

        <!-- Receiving panel -->
        <div class="col-span-8">

          <!-- Batch just completed — countdown banner -->
          <Transition name="fade">
            <div v-if="completingBatch"
                 class="rounded-2xl p-5 mb-4 flex items-center gap-4"
                 style="background-color: var(--color-success-soft); border: 1.5px solid var(--color-success)">
              <span class="material-symbols-outlined text-2xl" style="color: var(--color-success)">check_circle</span>
              <div class="flex-1">
                <p class="font-bold text-sm" style="color: var(--color-success)">
                  Batch {{ completingBatch.batchNo }} — All specimens received!
                </p>
                <p class="text-xs mt-0.5" style="color: var(--color-success)">
                  Moving to Received tab in {{ countdown }}s...
                </p>
              </div>
              <div class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm"
                   style="background-color: var(--color-success); color: #fff">
                {{ countdown }}
              </div>
            </div>
          </Transition>

          <div v-if="!selectedBatch"
               class="rounded-2xl flex items-center justify-center h-64"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <div class="text-center">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted)">touch_app</span>
              <p class="text-sm mt-2" style="color: var(--color-text-muted)">Select a batch to start receiving</p>
            </div>
          </div>

          <div v-else class="space-y-4">
            <!-- Batch info -->
            <div class="rounded-2xl p-5"
                 style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
              <div class="flex items-center justify-between mb-3">
                <div>
                  <p class="font-bold font-mono" style="color: var(--color-text)">{{ selectedBatch.batchNo }}</p>
                  <p class="text-xs" style="color: var(--color-text-muted)">
                    {{ selectedBatch.endorsingBranch }} → {{ selectedBatch.endorsedTo }}
                  </p>
                </div>
                <div class="text-right">
                  <p class="text-2xl font-bold" style="color: var(--color-text)">
                    {{ receivedCount }}
                    <span class="text-sm font-normal" style="color: var(--color-text-muted)">
                      /{{ detail?.specimens?.length ?? 0 }}
                    </span>
                  </p>
                  <p class="text-xs" style="color: var(--color-text-muted)">received</p>
                </div>
              </div>
              <div class="h-2 rounded-full overflow-hidden" style="background-color: var(--color-surface-low)">
                <div class="h-full rounded-full transition-all duration-500"
                     :style="`width: ${progressPct}%; background-color: var(--color-success)`"></div>
              </div>
            </div>

            <!-- Scan input -->
            <div v-if="selectedBatch.status !== 'Completed'"
                 class="rounded-2xl p-5"
                 style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                     style="color: var(--color-text-muted)">Scan Specimen</label>
              <div class="flex items-end gap-4">
                <div class="flex-1">
                  <div class="relative flex items-center">
                    <span class="material-symbols-outlined absolute left-4 text-lg"
                          style="color: var(--color-text-muted)">qr_code_scanner</span>
                    <input ref="receiveScanRef"
                           v-model="receiveScanValue"
                           @keyup.enter="scanReceive"
                           type="text"
                           placeholder="Scan specimen barcode..."
                           class="w-full border-none outline-none rounded-xl py-4 pl-12 pr-4 text-sm transition-colors"
                           style="background-color: var(--color-surface-low); color: var(--color-text);"
                           @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                           @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'" />
                  </div>
                </div>
                <button @click="scanReceive"
                        :disabled="scanning || !receiveScanValue.trim()"
                        class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                        style="background: var(--color-primary-gradient); color: #fff">
                  <span v-if="scanning"
                        class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                  <span v-else class="material-symbols-outlined text-sm">send</span>
                  {{ scanning ? 'Receiving...' : 'Receive' }}
                </button>
              </div>
            </div>

            <!-- Specimen list -->
            <div class="rounded-2xl overflow-hidden"
                 style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
              <table class="w-full text-sm">
                <thead>
                  <tr style="background-color: var(--color-surface-low)">
                    <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Specimen No.</th>
                    <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Sample Type</th>
                    <th class="px-4 py-2.5 text-center text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Status</th>
                    <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Received By</th>
                    <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Received At</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sp in detail?.specimens" :key="sp.id"
                      :style="`border-top: 1px solid var(--color-border);
                               background-color: ${sp.isReceived ? 'var(--color-success-soft)' : 'transparent'};
                               opacity: ${sp.isReceived ? 0.6 : 1}`">
                    <td class="px-4 py-2.5 font-mono text-sm font-bold"
                        style="color: var(--color-text)">
                      {{ sp.specimenNo }}
                    </td>
                    <td class="px-4 py-2.5 text-sm" style="color: var(--color-text)">{{ sp.sampleTypeName }}</td>
                    <td class="px-4 py-2.5 text-center">
                      <span class="material-symbols-outlined text-sm"
                            :style="sp.isReceived ? 'color: var(--color-success)' : 'color: var(--color-text-muted)'">
                        {{ sp.isReceived ? 'check_circle' : 'radio_button_unchecked' }}
                      </span>
                    </td>
                    <td class="px-4 py-2.5 text-xs" style="color: var(--color-text-muted)">{{ sp.receivedBy ?? '—' }}</td>
                    <td class="px-4 py-2.5 text-xs" style="color: var(--color-text-muted)">
                      {{ sp.receivedAt ? formatDate(sp.receivedAt) : '—' }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- ── RECEIVED TAB ────────────────────────────────────────────────── -->
      <div v-if="activeTab === 'received'" class="space-y-4">

        <!-- Search -->
        <div class="relative flex items-center max-w-sm">
          <span class="material-symbols-outlined absolute left-4 text-lg"
                style="color: var(--color-text-muted)">search</span>
          <input v-model="receivedSearch"
                 type="text"
                 placeholder="Search batch no. or endorsed by..."
                 class="w-full border-none outline-none rounded-xl py-3 pl-12 pr-4 text-sm transition-colors"
                 style="background-color: var(--color-surface-low); color: var(--color-text);"
                 @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                 @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'" />
        </div>

        <!-- Completed batch list -->
        <div v-if="filteredCompletedBatches.length === 0"
             class="rounded-2xl flex items-center justify-center py-16"
             style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
          <div class="text-center">
            <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted)">inventory</span>
            <p class="text-sm mt-2" style="color: var(--color-text-muted)">No completed batches yet.</p>
          </div>
        </div>

        <div v-else class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
          <table class="w-full text-sm">
            <thead>
              <tr style="background-color: var(--color-surface-low)">
                <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Batch No.</th>
                <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">From</th>
                <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Endorsed By</th>
                <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Endorsed At</th>
                <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Specimens</th>
                <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Imported</th>
                <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">Detail</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="batch in filteredCompletedBatches" :key="batch.id">
                <tr style="border-top: 1px solid var(--color-border)"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-5 py-3 font-mono font-bold text-sm"
                      style="color: var(--color-primary)">
                    {{ batch.batchNo }}
                  </td>
                  <td class="px-5 py-3 text-xs" style="color: var(--color-text-muted)">
                    {{ batch.endorsingBranch }} / {{ batch.endorsingSection }}
                  </td>
                  <td class="px-5 py-3 text-xs" style="color: var(--color-text)">{{ batch.endorsedBy }}</td>
                  <td class="px-5 py-3 text-xs" style="color: var(--color-text-muted)">
                    {{ formatDate(batch.endorsedAt) }}
                  </td>
                  <td class="px-5 py-3 text-center">
                    <span class="px-2 py-0.5 rounded-full text-xs font-bold"
                          style="background-color: var(--color-success-soft); color: var(--color-success)">
                      {{ batch.totalSpecimens }}/{{ batch.totalSpecimens }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-center text-xs" style="color: var(--color-text-muted)">
                    {{ batch.isImported ? '📥 Yes' : '—' }}
                  </td>
                  <td class="px-5 py-3 text-center">
                    <button @click="toggleReceivedDetail(batch.id)"
                            class="w-7 h-7 rounded-lg flex items-center justify-center mx-auto transition-all active:scale-95"
                            style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
                      <span class="material-symbols-outlined text-sm">
                        {{ expandedReceivedId === batch.id ? 'expand_less' : 'expand_more' }}
                      </span>
                    </button>
                  </td>
                </tr>

                <!-- Expandable specimen detail -->
                <tr v-if="expandedReceivedId === batch.id" :key="`detail-${batch.id}`">
                  <td colspan="7" class="px-5 py-0">
                    <div class="py-3">
                      <div v-if="receivedDetailLoading"
                           class="text-xs py-4 text-center"
                           style="color: var(--color-text-muted)">Loading...</div>
                      <table v-else class="w-full text-xs rounded-xl overflow-hidden"
                             style="background-color: var(--color-surface-low)">
                        <thead>
                          <tr>
                            <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                style="color: var(--color-text-muted)">Specimen No.</th>
                            <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                style="color: var(--color-text-muted)">Sample Type</th>
                            <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                style="color: var(--color-text-muted)">Received By</th>
                            <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                style="color: var(--color-text-muted)">Received At</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="sp in receivedDetail?.specimens" :key="sp.id"
                              style="border-top: 1px solid var(--color-border)">
                            <td class="px-4 py-2 font-mono font-bold"
                                style="color: var(--color-primary)">
                              {{ sp.specimenNo }}
                            </td>
                            <td class="px-4 py-2" style="color: var(--color-text)">{{ sp.sampleTypeName }}</td>
                            <td class="px-4 py-2" style="color: var(--color-text-muted)">{{ sp.receivedBy ?? '—' }}</td>
                            <td class="px-4 py-2" style="color: var(--color-text-muted)">
                              {{ sp.receivedAt ? formatDate(sp.receivedAt) : '—' }}
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>

    </div>

    <AlertModal :isVisible="alert.isVisible" :type="alert.type" :title="alert.title" :message="alert.message"
                @close="alert.isVisible = false" @confirm="alert.isVisible = false" />
  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
  import * as signalR from '@microsoft/signalr'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { contingencyApi } from '@/api/contingencyApi'

  const authStore = useAuthStore()
  const showWarning = ref(!authStore.isContingency)
  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) { alert.value = { isVisible: true, type, title, message } }

  // ── Tabs ───────────────────────────────────────────────────────────────────
  const tabs = [
    { key: 'pending', label: 'Pending' },
    { key: 'received', label: 'Received' },
  ]
  const activeTab = ref('pending')

  // ── Batch state ────────────────────────────────────────────────────────────
  const batches = ref([])
  const batchesLoading = ref(false)
  const selectedBatch = ref(null)
  const detail = ref(null)

  // ── Scan ───────────────────────────────────────────────────────────────────
  const receiveScanRef = ref(null)
  const receiveScanValue = ref('')
  const scanning = ref(false)

  // ── Completion countdown ───────────────────────────────────────────────────
  const completingBatch = ref(null)   // batch object being counted down
  const countdown = ref(5)
  let countdownTimer = null

  // ── Received tab ───────────────────────────────────────────────────────────
  const receivedSearch = ref('')
  const expandedReceivedId = ref(null)
  const receivedDetail = ref(null)
  const receivedDetailLoading = ref(false)

  // ── SignalR ────────────────────────────────────────────────────────────────
  let hubConnection = null

  // ── Computed ───────────────────────────────────────────────────────────────
  const pendingBatches = computed(() =>
    batches.value.filter(b => b.status !== 'Completed')
  )

  const completedBatches = computed(() =>
    batches.value.filter(b => b.status === 'Completed')
  )

  const filteredCompletedBatches = computed(() => {
    const q = receivedSearch.value.trim().toLowerCase()
    if (!q) return completedBatches.value
    return completedBatches.value.filter(b =>
      b.batchNo.toLowerCase().includes(q) ||
      b.endorsedBy.toLowerCase().includes(q) ||
      b.endorsingBranch.toLowerCase().includes(q)
    )
  })

  const receivedCount = computed(() =>
    detail.value?.specimens?.filter(s => s.isReceived).length ?? 0
  )

  const progressPct = computed(() => {
    const total = detail.value?.specimens?.length ?? 0
    return total > 0 ? (receivedCount.value / total * 100) : 0
  })

  // ── Helpers ────────────────────────────────────────────────────────────────
  function formatDate(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', {
      year: 'numeric', month: 'short', day: '2-digit',
      hour: '2-digit', minute: '2-digit'
    })
  }

  // ── Completion countdown logic ─────────────────────────────────────────────
  function startCompletionCountdown(batch) {
    completingBatch.value = batch
    countdown.value = 5

    clearInterval(countdownTimer)
    countdownTimer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        clearInterval(countdownTimer)
        // Remove from pending view + clear panel
        selectedBatch.value = null
        detail.value = null
        completingBatch.value = null
      }
    }, 1000)
  }

  // ── SignalR ────────────────────────────────────────────────────────────────
  async function connectSignalR() {
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/contingency')
      .withAutomaticReconnect()
      .build()

    hubConnection.on('NewContingencyBatch', async () => {
      await loadBatches()
    })

    hubConnection.on('ContingencySpecimenReceived', async (data) => {
      if (selectedBatch.value?.id === data.batchId) {
        await loadDetail(data.batchId)
        if (data.batchCompleted) {
          const b = batches.value.find(b => b.id === data.batchId)
          if (b) { b.status = 'Completed'; b.receivedCount = b.totalSpecimens }
        }
      }
      await loadBatches()
    })

    await hubConnection.start()
    await hubConnection.invoke('JoinBranch', authStore.branchCode)
  }

  // ── Load ───────────────────────────────────────────────────────────────────
  async function loadBatches() {
    batchesLoading.value = true
    try {
      batches.value = await contingencyApi.getBatches()
    } catch { showAlert('error', 'Error', 'Failed to load batches.') }
    finally { batchesLoading.value = false }
  }

  async function loadDetail(batchId) {
    try {
      detail.value = await contingencyApi.getBatchDetail(batchId)
    } catch { showAlert('error', 'Error', 'Failed to load batch detail.') }
  }

  async function selectBatch(batch) {
    selectedBatch.value = batch
    await loadDetail(batch.id)
    await nextTick()
    receiveScanRef.value?.focus()
  }

  // ── Received tab detail expand ─────────────────────────────────────────────
  async function toggleReceivedDetail(batchId) {
    if (expandedReceivedId.value === batchId) {
      expandedReceivedId.value = null
      receivedDetail.value = null
      return
    }
    expandedReceivedId.value = batchId
    receivedDetailLoading.value = true
    try {
      receivedDetail.value = await contingencyApi.getBatchDetail(batchId)
    } catch { showAlert('error', 'Error', 'Failed to load batch detail.') }
    finally { receivedDetailLoading.value = false }
  }

  // ── Scan ───────────────────────────────────────────────────────────────────
  async function scanReceive() {
    const val = receiveScanValue.value.trim().toUpperCase()
    if (!val || !selectedBatch.value) return

    scanning.value = true
    try {
      const result = await contingencyApi.scanSpecimen({
        batchId: selectedBatch.value.id,
        specimenNo: val
      })

      if (!result.found) {
        showAlert('warning', 'Not Found', result.message)
      } else if (result.alreadyReceived) {
        showAlert('info', 'Already Received', result.message)
      } else {
        // Update locally
        if (detail.value) {
          const sp = detail.value.specimens.find(s => s.id === result.specimenId)
          if (sp) {
            sp.isReceived = true
            sp.receivedBy = authStore.userID
            sp.receivedAt = new Date().toISOString()
          }
        }
        const b = batches.value.find(b => b.id === selectedBatch.value.id)
        if (b) {
          b.receivedCount++
          if (result.batchCompleted) {
            b.status = 'Completed'
            selectedBatch.value.status = 'Completed'
            // Start 5s countdown then remove from pending view
            startCompletionCountdown(b)
          }
        }
      }
    } catch (err) {
      showAlert('error', 'Error', err.response?.data?.message ?? 'Scan failed.')
    } finally {
      scanning.value = false
      receiveScanValue.value = ''
      receiveScanRef.value?.focus()
    }
  }

  // ── Import Excel ───────────────────────────────────────────────────────────
  async function importExcel(event) {
    const file = event.target.files[0]
    if (!file) return

    const formData = new FormData()
    formData.append('file', file)
    event.target.value = ''

    try {
      const result = await contingencyApi.importExcel(formData)
      if (result.alreadyExists) {
        showAlert('info', 'Already Exists', result.message)
      } else {
        showAlert('success', 'Imported', result.message)
        await loadBatches()
      }
    } catch (err) {
      showAlert('error', 'Import Failed', err.response?.data?.message ?? 'Import failed.')
    }
  }

  // ── Lifecycle ──────────────────────────────────────────────────────────────
  onMounted(async () => {
    await loadBatches()
    await connectSignalR()
  })

  onUnmounted(async () => {
    clearInterval(countdownTimer)
    if (hubConnection) {
      await hubConnection.invoke('LeaveBranch', authStore.branchCode).catch(() => { })
      await hubConnection.stop()
    }
  })
</script>

<style scoped>
  .fade-enter-active, .fade-leave-active {
    transition: all 0.3s ease;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  .batch-list-enter-active {
    transition: all 0.3s ease;
  }

  .batch-list-leave-active {
    transition: all 0.4s ease;
  }

  .batch-list-enter-from {
    opacity: 0;
    transform: translateX(-10px);
  }

  .batch-list-leave-to {
    opacity: 0;
    transform: translateX(-10px);
    height: 0;
  }

  .banner-slide-enter-active,
  .banner-slide-leave-active {
    transition: all 0.3s ease;
  }

  .banner-slide-enter-from,
  .banner-slide-leave-to {
    opacity: 0;
    transform: translateY(-8px);
  }
</style>
