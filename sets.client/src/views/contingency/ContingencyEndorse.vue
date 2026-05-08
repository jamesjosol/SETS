<template>
    <AppLayout>
      <div class="p-6 max-w-5xl mx-auto space-y-6">

        <!-- Header -->
        <div class="flex items-center gap-3">
          <span class="material-symbols-outlined text-3xl" style="color: var(--color-warning)">offline_bolt</span>
          <div>
            <h1 class="text-xl font-bold" style="color: var(--color-text)">Contingency Endorsement</h1>
            <p class="text-xs" style="color: var(--color-text-muted)">HCLAB offline mode — specimen no. only, sample type auto-detected</p>
          </div>
          <div class="ml-auto px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest"
               style="background-color: rgba(217,119,6,0.15); color: var(--color-warning)">
            ⚡ Contingency Mode
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
            <span v-if="tab.key === 'endorsed'" class="ml-1.5 px-1.5 py-0.5 rounded-full text-[10px]"
                  :style="activeTab === 'endorsed'
                  ? 'background-color: var(--color-primary-soft); color: var(--color-primary)'
                  : 'background-color: var(--color-surface); color: var(--color-text-muted)'">
              {{ endorsedBatches.length }}
            </span>
          </button>
        </div>

        <!-- ── NEW ENDORSEMENT TAB ─────────────────────────────────────────── -->
        <template v-if="activeTab === 'new'">

          <!-- Endorsement Details -->
          <div class="rounded-2xl p-5 space-y-4"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <h2 class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Endorsement Details</h2>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                       style="color: var(--color-text-muted)">Endorsed To</label>
                <select v-model="endorsedTo"
                        :disabled="endorsed"
                        class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm transition-colors"
                        style="background-color: var(--color-surface-low); color: var(--color-text);"
                        @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                        @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'">
                  <option value="" disabled>Select destination branch</option>
                  <option v-for="b in branches" :key="b.code" :value="b.code">
                    {{ b.name ?? b.code }}{{ b.code === authStore.branchCode ? ' (Local)' : '' }}
                  </option>
                </select>
              </div>
              <div>
                <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                       style="color: var(--color-text-muted)">Endorsed By</label>
                <input :value="authStore.userID"
                       disabled
                       class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm"
                       style="background-color: var(--color-surface-low); color: var(--color-text-muted);" />
              </div>
            </div>
          </div>

          <!-- Scan Area -->
          <div class="rounded-2xl p-5 space-y-4"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <h2 class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Scan Specimens</h2>

            <div class="flex items-end gap-4">
              <div class="flex-1">
                <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                       style="color: var(--color-text-muted)">Specimen No.</label>
                <div class="relative flex items-center">
                  <span class="material-symbols-outlined absolute left-4 text-lg"
                        style="color: var(--color-text-muted)">qr_code_scanner</span>
                  <input ref="scanInputRef"
                         v-model="scanValue"
                         :disabled="endorsed"
                         type="text"
                         placeholder="Scan specimen barcode..."
                         class="w-full border-none outline-none rounded-xl py-4 pl-12 pr-4 text-sm transition-colors"
                         style="background-color: var(--color-surface-low); color: var(--color-text);"
                         @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                         @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'"
                         @keyup.enter="addSpecimen" />
                </div>
              </div>
              <button @click="addSpecimen"
                      :disabled="endorsed || !scanValue.trim()"
                      class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background: var(--color-primary-gradient); color: #fff">
                <span class="material-symbols-outlined text-sm">add</span>
                Add
              </button>
            </div>

            <!-- Feedback -->
            <Transition name="fade">
              <div v-if="lastScanFeedback"
                   class="px-4 py-2.5 rounded-xl text-xs font-bold flex items-center gap-2"
                   :style="lastScanFeedback.type === 'ok'
                   ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                   : 'background-color: var(--color-error-soft); color: var(--color-error)'">
                <span class="material-symbols-outlined text-sm">
                  {{ lastScanFeedback.type === 'ok' ? 'check_circle' : 'error' }}
                </span>
                {{ lastScanFeedback.message }}
              </div>
            </Transition>

            <!-- Specimen Table -->
            <div v-if="specimens.length > 0"
                 class="rounded-xl overflow-hidden"
                 style="border: 1px solid var(--color-border)">
              <table class="w-full text-sm">
                <thead>
                  <tr style="background-color: var(--color-surface-low)">
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">#</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Specimen No.</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Lab No.</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Sample Type</th>
                    <th class="px-4 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">Remove</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(sp, idx) in specimens" :key="idx"
                      style="border-top: 1px solid var(--color-border)"
                      @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                      @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                    <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted)">{{ idx + 1 }}</td>
                    <td class="px-4 py-3 font-mono text-sm font-bold" style="color: var(--color-primary)">
                      {{ sp.specimenNo }}
                    </td>
                    <td class="px-4 py-3 font-mono text-xs" style="color: var(--color-text-muted)">
                      {{ sp.labNo }}
                    </td>
                    <td class="px-4 py-3 text-sm" style="color: var(--color-text)">
                      <span class="px-2 py-0.5 rounded-lg text-xs font-bold"
                            style="background-color: var(--color-surface-low); color: var(--color-text)">
                        {{ sp.sampleTypeCode }}
                      </span>
                      <span class="ml-2" style="color: var(--color-text-muted)">{{ sp.sampleTypeName }}</span>
                    </td>
                    <td class="px-4 py-3 text-center">
                      <button v-if="!endorsed"
                              @click="removeSpecimen(idx)"
                              class="w-7 h-7 rounded-lg flex items-center justify-center mx-auto transition-all active:scale-95"
                              style="background-color: var(--color-error-soft); color: var(--color-error)">
                        <span class="material-symbols-outlined text-sm">close</span>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <p v-else class="text-xs text-center py-6" style="color: var(--color-text-muted)">
              No specimens added yet. Scan a barcode above.
            </p>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-between">
            <span class="text-sm" style="color: var(--color-text-muted)">
              {{ specimens.length }} specimen(s) added
            </span>
            <div class="flex gap-3">
              <button v-if="endorsed && lastBatchNo"
                      @click="downloadExcel"
                      :disabled="downloading"
                      class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-success-soft); color: var(--color-success)">
                <span class="material-symbols-outlined text-sm">download</span>
                {{ downloading ? 'Downloading...' : 'Download Excel' }}
              </button>
              <button v-if="!endorsed"
                      @click="endorse"
                      :disabled="endorsing || specimens.length === 0 || !endorsedTo"
                      class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #fff">
                <span class="material-symbols-outlined text-sm">send</span>
                {{ endorsing ? 'Endorsing...' : 'Endorse' }}
              </button>
              <button v-if="endorsed"
                      @click="reset"
                      class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
                <span class="material-symbols-outlined text-sm">refresh</span>
                New Batch
              </button>
            </div>
          </div>

          <!-- Success Banner -->
          <Transition name="fade">
            <div v-if="endorsed && lastBatchNo"
                 class="rounded-2xl p-5 flex items-center gap-4"
                 style="background-color: var(--color-success-soft); border: 1px solid var(--color-success)">
              <span class="material-symbols-outlined text-2xl" style="color: var(--color-success)">check_circle</span>
              <div>
                <p class="font-bold text-sm" style="color: var(--color-success)">Endorsed successfully!</p>
                <p class="text-xs mt-0.5" style="color: var(--color-success)">
                  Batch No: <strong>{{ lastBatchNo }}</strong> — Download the Excel to transmit to the receiving branch if cross-branch.
                </p>
              </div>
            </div>
          </Transition>

        </template>

        <!-- ── ENDORSED BATCHES TAB ───────────────────────────────────────── -->
        <template v-if="activeTab === 'endorsed'">

          <!-- Search -->
          <div class="relative flex items-center max-w-sm">
            <span class="material-symbols-outlined absolute left-4 text-lg"
                  style="color: var(--color-text-muted)">search</span>
            <input v-model="batchSearch"
                   type="text"
                   placeholder="Search batch no. or endorsed to..."
                   class="w-full border-none outline-none rounded-xl py-3 pl-12 pr-4 text-sm transition-colors"
                   style="background-color: var(--color-surface-low); color: var(--color-text);"
                   @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                   @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'" />
          </div>

          <!-- Empty -->
          <div v-if="filteredEndorsedBatches.length === 0"
               class="rounded-2xl flex items-center justify-center py-16"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <div class="text-center">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted)">inventory</span>
              <p class="text-sm mt-2" style="color: var(--color-text-muted)">No endorsed batches yet.</p>
            </div>
          </div>

          <!-- Table -->
          <div v-else class="rounded-2xl overflow-hidden"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
            <table class="w-full text-sm">
              <thead>
                <tr style="background-color: var(--color-surface-low)">
                  <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Batch No.</th>
                  <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Endorsed To</th>
                  <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Endorsed By</th>
                  <th class="px-5 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Endorsed At</th>
                  <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Specimens</th>
                  <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Status</th>
                  <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Excel</th>
                  <th class="px-5 py-3 text-center text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted)">Detail</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="batch in filteredEndorsedBatches" :key="batch.id">
                  <tr style="border-top: 1px solid var(--color-border)"
                      @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                      @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                    <td class="px-5 py-3 font-mono font-bold text-sm"
                        style="color: var(--color-primary)">
                      {{ batch.batchNo }}
                    </td>
                    <td class="px-5 py-3 text-xs font-bold" style="color: var(--color-text)">
                      {{ batch.endorsedTo }}
                      <span v-if="batch.endorsedTo === authStore.branchCode"
                            class="ml-1 text-[10px]" style="color: var(--color-text-muted)">(Local)</span>
                    </td>
                    <td class="px-5 py-3 text-xs" style="color: var(--color-text-muted)">{{ batch.endorsedBy }}</td>
                    <td class="px-5 py-3 text-xs" style="color: var(--color-text-muted)">
                      {{ formatDate(batch.endorsedAt) }}
                    </td>
                    <td class="px-5 py-3 text-center text-xs font-bold" style="color: var(--color-text)">
                      {{ batch.totalSpecimens }}
                    </td>
                    <td class="px-5 py-3 text-center">
                      <span class="px-2 py-0.5 rounded-full text-xs font-bold"
                            :style="batch.status === 'Completed'
                            ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                            : 'background-color: rgba(217,119,6,0.15); color: var(--color-warning)'">
                        {{ batch.status }}
                      </span>
                    </td>
                    <td class="px-5 py-3 text-center">
                      <button @click="redownloadExcel(batch.batchNo)"
                              :disabled="redownloading === batch.batchNo"
                              class="w-7 h-7 rounded-lg flex items-center justify-center mx-auto transition-all active:scale-95"
                              style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                              title="Download Excel">
                        <span class="material-symbols-outlined text-sm"
                              :class="redownloading === batch.batchNo ? 'animate-spin' : ''">
                          {{ redownloading === batch.batchNo ? 'progress_activity' : 'download' }}
                        </span>
                      </button>
                    </td>
                    <td class="px-5 py-3 text-center">
                      <button @click="toggleBatchDetail(batch.id)"
                              class="w-7 h-7 rounded-lg flex items-center justify-center mx-auto transition-all active:scale-95"
                              style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
                        <span class="material-symbols-outlined text-sm">
                          {{ expandedBatchId === batch.id ? 'expand_less' : 'expand_more' }}
                        </span>
                      </button>
                    </td>
                  </tr>

                  <!-- Expandable specimen rows -->
                  <tr v-if="expandedBatchId === batch.id" :key="`detail-${batch.id}`">
                    <td colspan="8" class="px-5 py-0">
                      <div class="py-3">
                        <div v-if="batchDetailLoading"
                             class="text-xs py-4 text-center"
                             style="color: var(--color-text-muted)">Loading...</div>
                        <table v-else class="w-full text-xs rounded-xl overflow-hidden"
                               style="background-color: var(--color-surface-low)">
                          <thead>
                            <tr>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                  style="color: var(--color-text-muted)">Specimen No.</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                  style="color: var(--color-text-muted)">Lab No.</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest"
                                  style="color: var(--color-text-muted)">Sample Type</th>
                              <th class="px-4 py-2 text-center text-[10px] font-bold uppercase tracking-widest"
                                  style="color: var(--color-text-muted)">Received</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="sp in batchDetail?.specimens" :key="sp.id"
                                style="border-top: 1px solid var(--color-border)">
                              <td class="px-4 py-2 font-mono font-bold"
                                  style="color: var(--color-primary)">
                                {{ sp.specimenNo }}
                              </td>
                              <td class="px-4 py-2 font-mono"
                                  style="color: var(--color-text-muted)">
                                {{ sp.specimenNo.substring(0, 10) }}
                              </td>
                              <td class="px-4 py-2" style="color: var(--color-text)">
                                <span class="px-1.5 py-0.5 rounded text-[10px] font-bold mr-1"
                                      style="background-color: var(--color-surface); color: var(--color-text)">
                                  {{ sp.sampleTypeCode }}
                                </span>
                                {{ sp.sampleTypeName }}
                              </td>
                              <td class="px-4 py-2 text-center">
                                <span class="material-symbols-outlined text-sm"
                                      :style="sp.isReceived
                                      ? 'color: var(--color-success)'
                                      : 'color: var(--color-text-muted)'">
                                  {{ sp.isReceived ? 'check_circle' : 'radio_button_unchecked' }}
                                </span>
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

        </template>

      </div>

        <AlertModal :isVisible="alert.isVisible"
                    :type="alert.type"
                    :title="alert.title"
                    :message="alert.message"
                    @close="alert.isVisible = false"
                    @confirm="alert.isVisible = false" />
    </AppLayout>
</template>

<script setup>
    import { ref, computed, onMounted, nextTick } from 'vue'
    import AppLayout from '@/components/layout/AppLayout.vue'
    import AlertModal from '@/components/common/AlertModal.vue'
    import { useAuthStore } from '@/stores/authStore'
    import { contingencyApi } from '@/api/contingencyApi'
    import { branchApi } from '@/api/branchApi'

    const authStore = useAuthStore()
    const showWarning = ref(!authStore.isContingency)
    const scanInputRef = ref(null)

    const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
    function showAlert(type, title, message) { alert.value = { isVisible: true, type, title, message } }

    // ── Tabs ───────────────────────────────────────────────────────────────────
    const tabs = [
        { key: 'new', label: 'New Endorsement' },
        { key: 'endorsed', label: 'Endorsed Batches' },
    ]
    const activeTab = ref('new')

    // ── New endorsement state ──────────────────────────────────────────────────
    const scanValue = ref('')
    const endorsedTo = ref('')
    const specimens = ref([])
    const sampleTypes = ref([])
    const branches = ref([])
    const endorsing = ref(false)
    const endorsed = ref(false)
    const downloading = ref(false)
    const lastBatchNo = ref('')
    const lastScanFeedback = ref(null)
    let feedbackTimer = null

    // ── Endorsed batches state ─────────────────────────────────────────────────
    const endorsedBatches = ref([])
    const batchesLoading = ref(false)
    const batchSearch = ref('')
    const expandedBatchId = ref(null)
    const batchDetail = ref(null)
    const batchDetailLoading = ref(false)
    const redownloading = ref(null)  // batchNo currently being re-downloaded

    // ── Computed ───────────────────────────────────────────────────────────────
    const filteredEndorsedBatches = computed(() => {
        const q = batchSearch.value.trim().toLowerCase()
        if (!q) return endorsedBatches.value
        return endorsedBatches.value.filter(b =>
            b.batchNo.toLowerCase().includes(q) ||
            b.endorsedTo.toLowerCase().includes(q) ||
            b.endorsedBy.toLowerCase().includes(q)
        )
    })

    // ── Helpers ────────────────────────────────────────────────────────────────
    function formatDate(dt) {
        if (!dt) return '—'
        return new Date(dt).toLocaleString('en-PH', {
            year: 'numeric', month: 'short', day: '2-digit',
            hour: '2-digit', minute: '2-digit'
        })
    }

    function showFeedback(type, message) {
        lastScanFeedback.value = { type, message }
        clearTimeout(feedbackTimer)
        feedbackTimer = setTimeout(() => { lastScanFeedback.value = null }, 2500)
    }

    function parseSpecimenNo(raw) {
        const val = raw.trim().toUpperCase()
        if (val.length <= 10) return null
        const labNo = val.substring(0, 10)
        const sampleTypeCode = val.substring(10)
        const sampleType = sampleTypes.value.find(s => s.code.toUpperCase() === sampleTypeCode)
        return { specimenNo: val, labNo, sampleTypeCode, sampleTypeName: sampleType?.name ?? sampleTypeCode }
    }

    // ── Load ───────────────────────────────────────────────────────────────────
    async function loadEndorsedBatches() {
        batchesLoading.value = true
        try {
            // getBatches returns batches where EndorsingBranch = current branch
            // we need a separate endpoint — or reuse getBatches filtered by endorsing branch
            // For now use getEndorsedByBranch (add to API below)
            endorsedBatches.value = await contingencyApi.getEndorsedBatches()
        } catch { showAlert('error', 'Error', 'Failed to load endorsed batches.') }
        finally { batchesLoading.value = false }
    }

    onMounted(async () => {
        try { sampleTypes.value = await contingencyApi.getSampleTypes() }
        catch { showAlert('error', 'Error', 'Failed to load sample types.') }

        try {
            const res = await branchApi.getAll()
            branches.value = res.filter(b => b.active)
            endorsedTo.value = authStore.branchCode
        } catch { /* non-fatal */ }

        await loadEndorsedBatches()
        await nextTick()
        scanInputRef.value?.focus()
    })

    // ── Scan / Add ─────────────────────────────────────────────────────────────
    function addSpecimen() {
        const raw = scanValue.value.trim()
        if (!raw) return

        const parsed = parseSpecimenNo(raw)
        if (!parsed) {
            showFeedback('error', `'${raw}' is too short — must be at least 11 characters.`)
            scanValue.value = ''
            scanInputRef.value?.focus()
            return
        }

        if (specimens.value.some(s => s.specimenNo === parsed.specimenNo)) {
            showFeedback('error', `Duplicate — '${parsed.specimenNo}' is already in the list.`)
            scanValue.value = ''
            scanInputRef.value?.focus()
            return
        }

        const known = sampleTypes.value.some(s => s.code.toUpperCase() === parsed.sampleTypeCode)
        if (!known) {
            showFeedback('error', `Sample type '${parsed.sampleTypeCode}' not found in master — added anyway.`)
        } else {
            showFeedback('ok', `Added — ${parsed.specimenNo} · ${parsed.sampleTypeName}`)
        }

        specimens.value.unshift(parsed)
        scanValue.value = ''
        scanInputRef.value?.focus()
    }

    function removeSpecimen(idx) { specimens.value.splice(idx, 1) }

    // ── Endorse ────────────────────────────────────────────────────────────────
    async function endorse() {
        if (!endorsedTo.value) { showAlert('warning', 'Required', 'Please select a destination branch.'); return }
        if (specimens.value.length === 0) { showAlert('warning', 'Required', 'Add at least one specimen.'); return }

        endorsing.value = true
        try {
            const result = await contingencyApi.endorse({
                endorsedTo: endorsedTo.value,
                specimens: specimens.value.map(s => ({
                    specimenNo: s.specimenNo,
                    sampleTypeCode: s.sampleTypeCode,
                    sampleTypeName: s.sampleTypeName
                }))
            })
            lastBatchNo.value = result.batchNo
            endorsed.value = true
            await loadEndorsedBatches()   // refresh list
            await downloadExcel()
        } catch (err) {
            showAlert('error', 'Error', err.response?.data?.message ?? 'Endorsement failed.')
        } finally {
            endorsing.value = false
        }
    }

    // ── Excel ──────────────────────────────────────────────────────────────────
    async function downloadExcel() {
        if (!lastBatchNo.value) return
        downloading.value = true
        try {
            const response = await contingencyApi.exportExcel(lastBatchNo.value)
            triggerDownload(response.data, `${lastBatchNo.value}.xlsx`)
        } catch { showAlert('error', 'Error', 'Failed to download Excel.') }
        finally { downloading.value = false }
    }

    async function redownloadExcel(batchNo) {
        redownloading.value = batchNo
        try {
            const response = await contingencyApi.exportExcel(batchNo)
            triggerDownload(response.data, `${batchNo}.xlsx`)
        } catch { showAlert('error', 'Error', 'Failed to download Excel.') }
        finally { redownloading.value = null }
    }

    function triggerDownload(data, filename) {
        const url = URL.createObjectURL(new Blob([data]))
        const a = document.createElement('a')
        a.href = url
        a.download = filename
        a.click()
        URL.revokeObjectURL(url)
    }

    // ── Batch detail expand ────────────────────────────────────────────────────
    async function toggleBatchDetail(batchId) {
        if (expandedBatchId.value === batchId) {
            expandedBatchId.value = null
            batchDetail.value = null
            return
        }
        expandedBatchId.value = batchId
        batchDetailLoading.value = true
        try {
            batchDetail.value = await contingencyApi.getBatchDetail(batchId)
        } catch { showAlert('error', 'Error', 'Failed to load batch detail.') }
        finally { batchDetailLoading.value = false }
    }

    // ── Reset ──────────────────────────────────────────────────────────────────
    function reset() {
        specimens.value = []
        scanValue.value = ''
        endorsed.value = false
        lastBatchNo.value = ''
        lastScanFeedback.value = null
        nextTick(() => scanInputRef.value?.focus())
    }
</script>

<style scoped>
    .fade-enter-active, .fade-leave-active {
        transition: all 0.2s ease;
    }

    .fade-enter-from, .fade-leave-to {
        opacity: 0;
        transform: translateY(-4px);
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
