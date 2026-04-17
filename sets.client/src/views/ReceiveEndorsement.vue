<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">
          Receive Endorsement
        </h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
    </div>

    <!-- Main Card -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Tabs -->
      <div class="flex" style="border-bottom: 1px solid var(--color-surface-low);">
        <button v-for="tab in tabs"
                :key="tab.key"
                class="px-8 py-4 text-xs font-bold uppercase tracking-widest transition-all relative"
                :style="activeTab === tab.key
                  ? 'color: var(--color-primary);'
                  : 'color: var(--color-text-muted);'"
                @click="activeTab = tab.key">
          {{ tab.label }}
          <span v-if="activeTab === tab.key"
                class="absolute bottom-0 left-0 w-full h-0.5"
                style="background-color: var(--color-primary);"></span>
          <!-- Non-barcoded pending count badge -->
          <span v-if="tab.key === 'nonbarcoded' && pendingNonBarcoded.length > 0"
                class="ml-2 px-2 py-0.5 rounded-full text-[10px] font-bold"
                style="background-color: var(--color-warning-soft); color: var(--color-warning);">
            {{ pendingNonBarcoded.length }}
          </span>
        </button>
      </div>

      <!-- Tab Content -->
      <div class="p-8">

        <!-- ===== BARCODED TAB ===== -->
        <div v-if="activeTab === 'barcoded'">

          <!-- Active Batch Info -->
          <div class="mb-6 rounded-xl p-4 flex items-center gap-4"
               :style="activeBatchNo
         ? 'background-color: var(--color-primary-soft);'
         : 'background-color: var(--color-surface-low);'">
            <span class="material-symbols-outlined"
                  :style="activeBatchNo ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
              inventory_2
            </span>
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest"
                 :style="activeBatchNo ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
                {{ activeBatchNo ? 'Active Batch' : 'No Active Batch' }}
              </p>
              <p class="text-sm font-bold"
                 :style="activeBatchNo ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
                {{ activeBatchNo ?? 'Scan a specimen to begin' }}
                <span v-if="activeBatchName" class="font-normal text-xs ml-2"
                      style="color: var(--color-text-muted);">· {{ activeBatchName }}</span>
              </p>
            </div>
          </div>

          <!-- Temp / BagNo Fields — shown only when active batch is set -->
          <!-- Temp / BagNo Fields — shown only when active batch is set -->
          <div v-if="activeBatchNo"
               class="mb-6 rounded-xl overflow-hidden"
               style="background-color: var(--color-surface-low); border: 1px solid var(--color-border);">

            <!-- Collapsible Header -->
            <button class="w-full flex items-center justify-between px-5 py-3 transition-all"
                    :style="batchDetailsExpanded ? 'border-bottom: 1px solid var(--color-border);' : ''"
                    @click="batchDetailsExpanded = !batchDetailsExpanded">
              <p class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Batch Details</p>
              <span class="material-symbols-outlined text-sm transition-transform duration-200"
                    :style="`color: var(--color-text-muted); transform: rotate(${batchDetailsExpanded ? '0' : '-90'}deg);`">
                expand_more
              </span>
            </button>

            <!-- Collapsible Content -->
            <div v-show="batchDetailsExpanded" class="p-5">
              <div class="grid grid-cols-3 gap-4 mb-4">

                <!-- Temperature -->
                <div>
                  <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                         style="color: var(--color-text-muted);">Temperature</label>
                  <input v-model="temp"
                         type="text"
                         placeholder="e.g. 2-8°C"
                         class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm transition-colors"
                         style="background-color: var(--color-surface); color: var(--color-text);"
                         @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                         @blur="e => e.target.style.backgroundColor = 'var(--color-surface)'" />
                </div>

                <!-- Temp Remarks -->
                <div>
                  <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                         style="color: var(--color-text-muted);">Temp Remarks</label>
                  <input v-model="tempRemarks"
                         type="text"
                         placeholder="e.g. Cold chain maintained"
                         class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm transition-colors"
                         style="background-color: var(--color-surface); color: var(--color-text);"
                         @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                         @blur="e => e.target.style.backgroundColor = 'var(--color-surface)'" />
                </div>

                <!-- Bag No -->
                <div>
                  <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                         style="color: var(--color-text-muted);">Bag No.</label>
                  <input v-model="bagNo"
                         type="text"
                         placeholder="e.g. BAG-001"
                         class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm transition-colors"
                         style="background-color: var(--color-surface); color: var(--color-text);"
                         @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                         @blur="e => e.target.style.backgroundColor = 'var(--color-surface)'" />
                </div>
              </div>

              <!-- Save Button -->
              <div class="flex justify-end">
                <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                        :disabled="isSavingTemp"
                        style="background-color: var(--color-primary); color: #ffffff;"
                        @click="handleSaveTemp">
                  <span v-if="isSavingTemp"
                        class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                  <span v-else class="material-symbols-outlined text-sm">save</span>
                  {{ isSavingTemp ? 'Saving...' : 'Save' }}
                </button>
              </div>
            </div>
          </div>

          <!-- Scan Input -->
          <div class="flex items-end gap-4 mb-6">
            <div class="flex-1">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                     style="color: var(--color-text-muted);">Specimen No.</label>
              <div class="relative flex items-center">
                <span class="material-symbols-outlined absolute left-4 text-lg"
                      style="color: var(--color-text-muted);">qr_code_scanner</span>
                <input ref="specimenInput"
                       v-model="specimenNoInput"
                       type="text"
                       :placeholder="isScanning ? 'Looking up specimen...' : 'Scan or enter specimen number...'"
                       :disabled="isScanning"
                       class="w-full border-none outline-none rounded-xl py-4 pl-12 pr-4 text-sm transition-colors"
                       style="background-color: var(--color-surface-low); color: var(--color-text);"
                       @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                       @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'"
                       @keyup.enter="handleScan" />
              </div>
            </div>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background-color: var(--color-primary); color: #ffffff;"
                    :disabled="isScanning"
                    @click="handleScan">
              <span v-if="isScanning"
                    class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">send</span>
              {{ isScanning ? 'Receiving...' : 'Receive' }}
            </button>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    :disabled="receivedSpecimens.length === 0"
                    :style="receivedSpecimens.length === 0
              ? 'background-color: var(--color-surface-low); color: var(--color-text-muted); cursor: not-allowed;'
              : 'background-color: var(--color-error-soft); color: var(--color-error);'"
                    @click="clearReceivedList">
              <span class="material-symbols-outlined text-sm">delete_sweep</span>
              Clear
            </button>
          </div>

          <!-- Received Specimens Table -->
          <div class="rounded-xl overflow-hidden"
               style="border: 1px solid var(--color-border);">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-surface-low);">
                  <th class="px-6 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Specimen No.</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Location</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Patient ID</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Patient Name</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Sample Type</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">
                    <span title="Endorsement Remarks">End. Remarks</span>
                  </th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">
                    <span title="Receiving Remarks">Rec. Remarks</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="receivedSpecimens.length === 0">
                  <td colspan="7" class="px-6 py-12 text-center">
                    <div class="flex flex-col items-center gap-3">
                      <span class="material-symbols-outlined text-4xl opacity-20"
                            style="color: var(--color-text-muted);">qr_code_scanner</span>
                      <p class="text-sm font-bold"
                         style="color: var(--color-text-muted);">No specimens received yet</p>
                      <p class="text-xs"
                         style="color: var(--color-text-muted);">Scan or enter a specimen number above</p>
                    </div>
                  </td>
                </tr>
                <tr v-for="(item, index) in receivedSpecimens"
                    :key="item.specimenNo"
                    class="transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-6 py-4 font-mono text-xs font-bold"
                      style="color: var(--color-primary);">
                    {{ item.specimenNo }}
                  </td>
                  <td class="px-4 py-4 text-sm"
                      style="color: var(--color-text-muted);">
                    {{ item.location }}
                  </td>
                  <td class="px-4 py-4 text-sm"
                      style="color: var(--color-text-muted);">
                    {{ item.pid }}
                  </td>
                  <td class="px-4 py-4 text-sm font-medium"
                      style="color: var(--color-text);">
                    {{ item.patientName }}
                  </td>
                  <td class="px-4 py-4 text-sm"
                      style="color: var(--color-text-muted);">
                    {{ item.sampleTypeName }}
                  </td>

                  <!-- Endorsement Remarks (read-only) -->
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.endorsementRemarks
                              ? 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.endorsementRemarks || 'No endorsement remarks'"
                            @click="viewEndorsementRemark(item.endorsementRemarks)">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.endorsementRemarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>

                  <!-- Receiving Remarks (editable) -->
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.receivingRemarks
                              ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.receivingRemarks || 'Add receiving remarks'"
                            @click="openReceivingRemark(index, 'barcoded')">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.receivingRemarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

        </div>

        <!-- ===== NON-BARCODED TAB ===== -->
        <div v-if="activeTab === 'nonbarcoded'">

          <!-- Filter + Actions Row -->
          <div class="flex items-end justify-between gap-4 mb-6">

            <!-- Branch Filter -->
            <div style="width: 260px;">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2"
                     style="color: var(--color-text-muted);">Filter by Location</label>
              <select v-model="locationFilter"
                      class="w-full border-none outline-none rounded-xl py-3 px-4 text-sm font-bold transition-colors"
                      style="background-color: var(--color-surface-low); color: var(--color-text);">
                <option value="">All Locations</option>
                <option v-for="loc in availableLocations"
                        :key="loc.code"
                        :value="loc.code">
                  {{ loc.name }}
                </option>
              </select>
            </div>

            <!-- Select All + Receive Button -->
            <div class="flex items-center gap-3">
              <button class="px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                      @click="toggleSelectAll">
                {{ isAllSelected ? 'Deselect All' : 'Select All' }}
              </button>
              <button class="px-6 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      :disabled="selectedItemIDs.length === 0 || isReceivingNonBarcoded"
                      :style="selectedItemIDs.length === 0 || isReceivingNonBarcoded
                        ? 'background-color: var(--color-surface-low); color: var(--color-text-muted); cursor: not-allowed;'
                        : 'background: var(--color-primary-gradient); color: #ffffff;'"
                      @click="handleReceiveNonBarcoded">
                <span v-if="isReceivingNonBarcoded"
                      class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                <span v-else class="material-symbols-outlined text-sm">done_all</span>
                {{ isReceivingNonBarcoded ? 'Receiving...' : `Receive (${selectedItemIDs.length})` }}
              </button>
            </div>

          </div>

          <!-- Non-Barcoded Table -->
          <div class="rounded-xl overflow-hidden"
               style="border: 1px solid var(--color-border);">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-surface-low);">
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">
                    <input type="checkbox"
                           :checked="isAllSelected"
                           :indeterminate="isSomeSelected"
                           class="rounded"
                           @change="toggleSelectAll" />
                  </th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Batch No.</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Location</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Description</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">Qty</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">Endorsed By</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">End. Remarks</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center"
                      style="color: var(--color-text-muted);">Rec. Remarks</th>
                </tr>
              </thead>
              <tbody>

                <!-- Loading -->
                <tr v-if="nonBarcodedLoading">
                  <td colspan="8" class="px-6 py-12 text-center">
                    <div class="flex items-center justify-center gap-3">
                      <span class="material-symbols-outlined animate-spin"
                            style="color: var(--color-text-muted);">progress_activity</span>
                      <p class="text-xs font-bold uppercase tracking-widest"
                         style="color: var(--color-text-muted);">Loading...</p>
                    </div>
                  </td>
                </tr>

                <!-- Empty -->
                <tr v-else-if="filteredNonBarcoded.length === 0">
                  <td colspan="8" class="px-6 py-12 text-center">
                    <div class="flex flex-col items-center gap-3">
                      <span class="material-symbols-outlined text-4xl opacity-20"
                            style="color: var(--color-text-muted);">inventory_2</span>
                      <p class="text-sm font-bold"
                         style="color: var(--color-text-muted);">No pending non-barcoded items</p>
                    </div>
                  </td>
                </tr>

                <!-- Rows -->
                <tr v-else
                    v-for="(item, index) in filteredNonBarcoded"
                    :key="item.itemID"
                    class="transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-4 py-4 text-center">
                    <input type="checkbox"
                           :checked="selectedItemIDs.includes(item.itemID)"
                           class="rounded"
                           @change="toggleItem(item.itemID)" />
                  </td>
                  <td class="px-4 py-4 font-mono text-xs font-bold"
                      style="color: var(--color-primary);">
                    {{ item.batchNo }}
                  </td>
                  <td class="px-4 py-4 text-sm"
                      style="color: var(--color-text-muted);">
                    {{ item.locationName }}
                  </td>
                  <td class="px-4 py-4 text-sm font-medium"
                      style="color: var(--color-text);">
                    {{ item.description }}
                  </td>
                  <td class="px-4 py-4 text-sm text-center font-bold"
                      style="color: var(--color-text);">
                    {{ item.quantity }}
                  </td>
                  <td class="px-4 py-4 text-sm"
                      style="color: var(--color-text-muted);">
                    {{ item.endorsedBy }}
                  </td>

                  <!-- Endorsement Remarks (read-only) -->
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.remarks
                              ? 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.remarks || 'No endorsement remarks'"
                            @click="viewEndorsementRemark(item.remarks)">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>

                  <!-- Receiving Remarks (editable) -->
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.receivingRemarks
                              ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.receivingRemarks || 'Add receiving remarks'"
                            @click="openReceivingRemark(index, 'nonbarcoded')">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.receivingRemarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>
                </tr>

              </tbody>
            </table>
          </div>

        </div>

      </div>
    </div>

    <!-- ── Endorsement Remark Viewer ──────────────────────────────────── -->
    <div v-if="endorsementRemarkViewer.visible"
         class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
           @click="endorsementRemarkViewer.visible = false"></div>
      <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4"
           style="background-color: var(--color-surface);">
        <div class="flex items-center gap-2">
          <div class="p-2 rounded-xl"
               style="background-color: rgba(217,119,6,0.1);">
            <span class="material-symbols-outlined text-sm"
                  style="color: var(--color-warning);">chat_bubble</span>
          </div>
          <h3 class="text-sm font-bold"
              style="color: var(--color-text);">
            Endorsement Remarks
          </h3>
        </div>
        <p class="text-sm rounded-xl p-4 whitespace-pre-wrap"
           style="background-color: var(--color-surface-low); color: var(--color-text);">
          {{ endorsementRemarkViewer.text }}
        </p>
        <button class="w-full py-3 rounded-xl text-xs font-bold uppercase tracking-widest"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="endorsementRemarkViewer.visible = false">
          Close
        </button>
      </div>
    </div>

    <!-- Remark Modal (reused for receiving remarks) -->
    <RemarkModal :isVisible="remarkModal.visible"
                 title="Receiving Remarks"
                 :initialText="remarkModal.text"
                 @save="saveReceivingRemark"
                 @cancel="remarkModal.visible = false"
                 @close="remarkModal.visible = false" />

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
import { ref, computed, onMounted, nextTick } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import RemarkModal from '@/components/common/RemarkModal.vue'
import { useAuthStore } from '@/stores/authStore'
import { receivingApi } from '@/api/receivingApi'
import NProgress from 'nprogress'

const authStore = useAuthStore()

// ── Tabs ───────────────────────────────────────────────────────────────────

const activeTab = ref('barcoded')
const tabs = [
  { key: 'barcoded',    label: '🔬 Receive by Specimen' },
  { key: 'nonbarcoded', label: '📋 Receive Non-Barcoded' },
]

// ── Alert ──────────────────────────────────────────────────────────────────

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

// ── Endorsement Remark Viewer ──────────────────────────────────────────────

const endorsementRemarkViewer = ref({ visible: false, text: '' })

function viewEndorsementRemark(text) {
  if (!text) return
  endorsementRemarkViewer.value = { visible: true, text }
}

// ── Receiving Remark Modal ─────────────────────────────────────────────────

const remarkModal = ref({ visible: false, index: null, type: null, text: '' })

function openReceivingRemark(index, type) {
  const item = type === 'barcoded'
    ? receivedSpecimens.value[index]
    : filteredNonBarcoded.value[index]
  remarkModal.value = { visible: true, index, type, text: item.receivingRemarks ?? '' }
}

  async function saveReceivingRemark(text) {
    const { index, type } = remarkModal.value

    if (type === 'barcoded') {
      const item = receivedSpecimens.value[index]
      try {
        await receivingApi.updateSpecimenRemarks({
          specimenNo: item.specimenNo,
          batchNo: item.batchNo,
          receivingRemarks: text || null
        })
        item.receivingRemarks = text
      } catch (err) {
        showAlert('error', 'Save Failed', 'Unable to save receiving remarks.')
        remarkModal.value.visible = false
        return
      }

    } else {
      // Non-barcoded — now persisted
      const filtered = filteredNonBarcoded.value[index]
      const real = pendingNonBarcoded.value.find(n => n.itemID === filtered.itemID)
      if (!real) {
        remarkModal.value.visible = false
        return
      }
      try {
        await receivingApi.updateNonBarcodedRemarks({
          itemID: real.itemID,
          receivingRemarks: text || null
        })
        real.receivingRemarks = text
      } catch (err) {
        showAlert('error', 'Save Failed', 'Unable to save receiving remarks.')
        remarkModal.value.visible = false
        return
      }
    }

    remarkModal.value.visible = false
  }


// ── Barcoded Tab ───────────────────────────────────────────────────────────

const specimenInput = ref(null)
const specimenNoInput = ref('')
const isScanning = ref(false)
const receivedSpecimens = ref([])

async function handleScan() {
  const input = specimenNoInput.value.trim().toUpperCase()
  if (!input) return

  // Duplicate check in current session only
  if (receivedSpecimens.value.some(s => s.specimenNo === input)) {
    showAlert('warning', 'Already Received', `Specimen ${input} has already been received in this session.`)
    specimenNoInput.value = ''
    return
  }

  isScanning.value = true
  NProgress.start()

  try {
    const response = await receivingApi.receiveSpecimen({
      userID: authStore.userID,
      specimenNo: input,
      currentBatchNo: activeBatchNo.value ?? null,  // ← pass active batch
      receivingRemarks: null
    })

    const data = response.data.data

    // First scan — set active batch and pre-populate temp fields
    if (!activeBatchNo.value) {
      activeBatchNo.value = data.batchNo
      activeBatchName.value = data.locationName
      temp.value = data.temp ?? ''
      tempRemarks.value = data.tempRemarks ?? ''
      bagNo.value = data.bagNo ?? ''
    }

    receivedSpecimens.value.unshift({
      specimenNo: data.specimenNo,
      batchNo: data.batchNo,
      location: data.locationName,
      pid: data.pid,
      patientName: data.patientName,
      sampleTypeName: data.sampleTypeName,
      endorsementRemarks: null,
      receivingRemarks: null,
      batchStatus: data.batchStatus
    })

    specimenNoInput.value = ''

  } catch (err) {
    const msg = err.response?.data?.message
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      // This now catches the batch restriction error from backend too
      showAlert('error', 'Cannot Receive', msg || 'An error occurred.')
    }
  } finally {
    isScanning.value = false
    NProgress.done()
    await nextTick()
    specimenInput.value?.focus()
  }
}

// ── Non-Barcoded Tab ───────────────────────────────────────────────────────

const nonBarcodedLoading = ref(false)
const pendingNonBarcoded = ref([])
const selectedItemIDs = ref([])
const locationFilter = ref('')
const isReceivingNonBarcoded = ref(false)

const availableLocations = computed(() => {
  const seen = new Map()
  for (const item of pendingNonBarcoded.value) {
    if (!seen.has(item.location)) {
      seen.set(item.location, { code: item.location, name: item.locationName })
    }
  }
  return Array.from(seen.values()).sort((a, b) => a.name.localeCompare(b.name))
})

const filteredNonBarcoded = computed(() => {
  if (!locationFilter.value) return pendingNonBarcoded.value
  return pendingNonBarcoded.value.filter(n => n.location === locationFilter.value)
})

const isAllSelected = computed(() =>
  filteredNonBarcoded.value.length > 0 &&
  filteredNonBarcoded.value.every(n => selectedItemIDs.value.includes(n.itemID))
)

const isSomeSelected = computed(() =>
  filteredNonBarcoded.value.some(n => selectedItemIDs.value.includes(n.itemID)) &&
  !isAllSelected.value
)

function toggleSelectAll() {
  if (isAllSelected.value) {
    // Deselect all filtered items
    const filteredIDs = filteredNonBarcoded.value.map(n => n.itemID)
    selectedItemIDs.value = selectedItemIDs.value.filter(id => !filteredIDs.includes(id))
  } else {
    // Select all filtered items
    const filteredIDs = filteredNonBarcoded.value.map(n => n.itemID)
    const merged = new Set([...selectedItemIDs.value, ...filteredIDs])
    selectedItemIDs.value = Array.from(merged)
  }
}

function toggleItem(itemID) {
  if (selectedItemIDs.value.includes(itemID)) {
    selectedItemIDs.value = selectedItemIDs.value.filter(id => id !== itemID)
  } else {
    selectedItemIDs.value.push(itemID)
  }
}

async function loadPendingNonBarcoded() {
  nonBarcodedLoading.value = true
  try {
    const data = await receivingApi.getPendingNonBarcoded(authStore.sectionCode)
    // ReceivingRemarks now comes from API — no need to override with null
    pendingNonBarcoded.value = Array.isArray(data) ? data : []
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Load Failed', 'Unable to load pending non-barcoded items.')
    }
  } finally {
    nonBarcodedLoading.value = false
  }
}

async function handleReceiveNonBarcoded() {
  if (selectedItemIDs.value.length === 0) return

  isReceivingNonBarcoded.value = true
  NProgress.start()

  try {
    // Attach receiving remarks per selected item
    // For now remarks are stored locally — future enhancement
    await receivingApi.receiveNonBarcoded({
      userID: authStore.userID,
      itemIDs: selectedItemIDs.value
    })

    // Remove received items from the list
    pendingNonBarcoded.value = pendingNonBarcoded.value
      .filter(n => !selectedItemIDs.value.includes(n.itemID))

    selectedItemIDs.value = []

    showAlert('success', 'Received', 'Selected non-barcoded items have been marked as received.')

  } catch (err) {
    const msg = err.response?.data?.message
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Receive Failed', msg || 'Unable to receive selected items.')
    }
  } finally {
    isReceivingNonBarcoded.value = false
    NProgress.done()
  }
}

  // ── Active Batch ───────────────────────────────────────────────────────────
  const activeBatchNo = ref(null)
  const activeBatchName = ref(null)
  const temp = ref('')
  const tempRemarks = ref('')
  const bagNo = ref('')
  const isSavingTemp = ref(false)
  const batchDetailsExpanded = ref(true)

  async function handleSaveTemp() {
    if (!activeBatchNo.value) return

    isSavingTemp.value = true
    try {
      await receivingApi.updateBatchTemp({
        batchNo: activeBatchNo.value,
        temp: temp.value || null,
        tempRemarks: tempRemarks.value || null,
        bagNo: bagNo.value || null
      })
      showAlert('success', 'Saved', 'Temperature and bag details have been saved.')
    } catch (err) {
      showAlert('error', 'Save Failed', 'Unable to save temperature details.')
    } finally {
      isSavingTemp.value = false
    }
  }

// ── Lifecycle ──────────────────────────────────────────────────────────────

onMounted(async () => {
  await loadPendingNonBarcoded()
  await nextTick()
  specimenInput.value?.focus()
})

  function clearReceivedList() {
    receivedSpecimens.value = []
    activeBatchNo.value = null
    activeBatchName.value = null
    temp.value = ''
    tempRemarks.value = ''
    bagNo.value = ''
    batchDetailsExpanded = ref(true)
  }
</script>
