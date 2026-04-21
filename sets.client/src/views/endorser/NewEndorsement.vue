<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">New Endorsement</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
      <router-link to="/dashboard"
                   class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                   style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                   @mouseenter="e => e.currentTarget.style.color = 'var(--color-primary)'"
                   @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'">
        <span class="material-symbols-outlined text-sm">arrow_back</span>
        Back
      </router-link>
    </div>

    <!-- Endorsement Card -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Card Header: Endorsed To -->
      <div class="px-8 py-5 flex flex-wrap items-center gap-6" style="border-bottom: 1px solid var(--color-surface-low);">

        <!-- Endorsed To: Local (auto-filled) -->
        <div class="flex items-center gap-3">
          <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-primary);">arrow_forward</span>
          </div>
          <div>
            <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed To (Local)</p>
            <p class="font-bold text-sm" style="color: var(--color-text);">Processing — {{ authStore.branchCode }}</p>
          </div>
        </div>

        <!-- Divider -->
        <div class="h-8 w-px hidden md:block" style="background-color: var(--color-border);"></div>

        <!-- Endorsed To: Outbound (disabled for now) -->
        <div class="flex items-center gap-3 opacity-40">
          <div class="p-2 rounded-xl" style="background-color: var(--color-surface-low);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-text-muted);">alt_route</span>
          </div>
          <div>
            <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed To (Outbound)</p>
            <select disabled
                    class="text-sm font-bold border-none outline-none cursor-not-allowed rounded-lg px-2 py-1"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
              <option>Coming Soon</option>
            </select>
          </div>
        </div>

        <!-- Batch No (shown after endorsement) -->
        <div v-if="batchNo" class="ml-auto flex items-center gap-2 px-4 py-2 rounded-xl" style="background-color: var(--color-primary-soft);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">tag</span>
          <span class="text-sm font-bold" style="color: var(--color-primary);">Batch: {{ batchNo }}</span>
        </div>
      </div>

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
          <!-- Item count badge -->
          <span v-if="tab.key === 'barcoded' && barcodedItems.length > 0"
                class="ml-2 px-2 py-0.5 rounded-full text-[10px] font-bold"
                style="background-color: var(--color-primary-soft); color: var(--color-primary);">{{ barcodedItems.length }}</span>
          <span v-if="tab.key === 'nonbarcoded' && nonBarcodedItems.length > 0"
                class="ml-2 px-2 py-0.5 rounded-full text-[10px] font-bold"
                style="background-color: var(--color-primary-soft); color: var(--color-primary);">{{ nonBarcodedItems.length }}</span>
        </button>
      </div>

      <!-- Tab Content -->
      <div class="p-8">

        <!-- ===== BARCODED TAB ===== -->
        <div v-if="activeTab === 'barcoded'">

          <!-- Input Row -->
          <div class="flex items-end gap-4 mb-6">
            <div class="flex-1">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">
                Specimen No.
              </label>
              <div class="relative flex items-center">
                <span class="material-symbols-outlined absolute left-4 text-lg" style="color: var(--color-text-muted);">qr_code_scanner</span>
                <input ref="specimenInput"
                       v-model="specimenNoInput"
                       type="text"
                       :placeholder="isSearching ? 'Looking up specimen...' : 'Scan or enter specimen number...'"
                       :disabled="isSearching || isEndorsed"
                       class="w-full border-none outline-none rounded-xl py-4 pl-12 pr-4 text-sm transition-colors"
                       style="background-color: var(--color-surface-low); color: var(--color-text);"
                       @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                       @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'"
                       @keyup.enter="addBarcodedItem" />
              </div>
            </div>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background-color: var(--color-primary); color: #ffffff;"
                    :disabled="isSearching || isEndorsed"
                    @click="addBarcodedItem">
              <span v-if="isSearching" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">add</span>
              {{ isSearching ? 'Loading...' : 'Add to List' }}
            </button>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="clearBarcodedInput">
              <span class="material-symbols-outlined text-sm">close</span>
              Cancel
            </button>
          </div>

          <!-- Barcoded Table -->
          <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border);">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-surface-low);">
                  <th class="px-6 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Transaction Date</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient ID</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient Name</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center" style="color: var(--color-text-muted);">Remarks</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center" style="color: var(--color-text-muted);">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="barcodedItems.length === 0">
                  <td colspan="7" class="px-6 py-12 text-center">
                    <div class="flex flex-col items-center gap-3">
                      <span class="material-symbols-outlined text-4xl opacity-20" style="color: var(--color-text-muted);">qr_code_scanner</span>
                      <p class="text-sm font-bold" style="color: var(--color-text-muted);">No specimens added yet</p>
                      <p class="text-xs" style="color: var(--color-text-muted);">Scan or enter a specimen number above</p>
                    </div>
                  </td>
                </tr>
                <tr v-for="(item, index) in barcodedItems"
                    :key="item.specimenNo"
                    class="transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-6 py-4 font-mono text-xs font-bold" style="color: var(--color-primary);">{{ item.specimenNo }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ item.transactionDate }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ item.patientID }}</td>
                  <td class="px-4 py-4 text-sm font-medium" style="color: var(--color-text);">{{ item.patientName }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ item.sampleType }}</td>
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.remarks
                        ? 'background-color: var(--color-error-soft); color: var(--color-error);'
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.remarks || 'Add remark'"
                            @click="openRemarkModal(index, 'barcoded')">
                      <span class="material-symbols-outlined text-sm">{{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}</span>
                    </button>
                  </td>
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            style="background-color: var(--color-error-soft); color: var(--color-error);"
                            @click="removeBarcodedItem(index)">
                      <span class="material-symbols-outlined text-sm">close</span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- ===== NON-BARCODED TAB ===== -->
        <div v-if="activeTab === 'nonbarcoded'">

          <!-- Input Row -->
          <div class="flex items-end gap-4 mb-6">
            <div class="flex-1">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">
                Item Description
              </label>
              <input v-model="nonBarcodedInput.description"
                     type="text"
                     placeholder="e.g. Request Form, Slide, Job Order..."
                     class="w-full border-none outline-none rounded-xl py-4 px-6 text-sm transition-colors"
                     style="background-color: var(--color-surface-low); color: var(--color-text);"
                     @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                     @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'" />
            </div>
            <div style="width: 120px;">
              <label class="block text-[11px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">
                Quantity
              </label>
              <input v-model.number="nonBarcodedInput.quantity"
                     type="number"
                     min="1"
                     placeholder="1"
                     class="w-full border-none outline-none rounded-xl py-4 px-4 text-sm transition-colors text-center"
                     style="background-color: var(--color-surface-low); color: var(--color-text);"
                     @focus="e => e.target.style.backgroundColor = 'var(--color-surface-high)'"
                     @blur="e => e.target.style.backgroundColor = 'var(--color-surface-low)'" />
            </div>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background-color: var(--color-primary); color: #ffffff;"
                    @click="addNonBarcodedItem">
              <span class="material-symbols-outlined text-sm">add</span>
              Add to List
            </button>
            <button class="px-6 py-4 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="clearNonBarcodedInput">
              <span class="material-symbols-outlined text-sm">close</span>
              Cancel
            </button>
          </div>

          <!-- Non-Barcoded Table -->
          <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border);">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-surface-low);">
                  <th class="px-6 py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Item Description</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center" style="color: var(--color-text-muted);">Quantity</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center" style="color: var(--color-text-muted);">Remarks</th>
                  <th class="px-4 py-3 text-[10px] font-bold uppercase tracking-widest text-center" style="color: var(--color-text-muted);">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="nonBarcodedItems.length === 0">
                  <td colspan="4" class="px-6 py-12 text-center">
                    <div class="flex flex-col items-center gap-3">
                      <span class="material-symbols-outlined text-4xl opacity-20" style="color: var(--color-text-muted);">inventory_2</span>
                      <p class="text-sm font-bold" style="color: var(--color-text-muted);">No items added yet</p>
                      <p class="text-xs" style="color: var(--color-text-muted);">Add non-barcoded items such as forms or slides</p>
                    </div>
                  </td>
                </tr>
                <tr v-for="(item, index) in nonBarcodedItems"
                    :key="index"
                    class="transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-6 py-4 text-sm font-medium" style="color: var(--color-text);">{{ item.description }}</td>
                  <td class="px-4 py-4 text-sm text-center font-bold" style="color: var(--color-text);">{{ item.quantity }}</td>
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            :style="item.remarks
                        ? 'background-color: var(--color-error-soft); color: var(--color-error);'
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                            :title="item.remarks || 'Add remark'"
                            @click="openRemarkModal(index, 'nonbarcoded')">
                      <span class="material-symbols-outlined text-sm">{{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}</span>
                    </button>
                  </td>
                  <td class="px-4 py-4 text-center">
                    <button class="w-8 h-8 rounded-full flex items-center justify-center mx-auto transition-all hover:scale-110"
                            style="background-color: var(--color-error-soft); color: var(--color-error);"
                            @click="removeNonBarcodedItem(index)">
                      <span class="material-symbols-outlined text-sm">close</span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>

      <!-- Footer Actions -->
      <div class="px-8 py-5 flex items-center justify-between"
           style="border-top: 1px solid var(--color-surface-low);">
        <div class="flex items-center gap-3">
          <span class="text-xs font-bold uppercase tracking-widest"
                style="color: var(--color-text-muted);">
            {{ barcodedItems.length }} barcoded · {{ nonBarcodedItems.length }} non-barcoded
          </span>
        </div>
        <div class="flex items-center gap-3">
          <button class="px-6 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  @click="clearAll">
            {{ isEndorsed ? 'New Endorsement' : 'Clear All' }}
          </button>
          <button class="px-8 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                  :disabled="isEndorsed || isEndorsing || (barcodedItems.length === 0 && nonBarcodedItems.length === 0)"
                  :style="(isEndorsed || isEndorsing || (barcodedItems.length === 0 && nonBarcodedItems.length === 0))
    ? 'background: var(--color-surface-low); color: var(--color-text-muted); cursor: not-allowed;'
    : 'background: var(--color-primary-gradient); color: #ffffff; cursor: pointer;'"
                  @click="handleEndorse">
            <!-- Spinner while saving -->
            <span v-if="isEndorsing"
                  class="material-symbols-outlined text-sm animate-spin">
              progress_activity
            </span>
            <!-- Checkmark once done -->
            <span v-else-if="isEndorsed"
                  class="material-symbols-outlined text-sm">
              check_circle
            </span>
            <!-- Default send icon -->
            <span v-else
                  class="material-symbols-outlined text-sm">
              send
            </span>

            {{ isEndorsing ? 'Endorsing...' : isEndorsed ? 'Endorsed' : 'Endorse' }}
          </button>
        </div>
      </div>

    </div>

    <RemarkModal :isVisible="remarkModal.visible"
                 :title="remarkModal.required ? 'Reason Required' : 'Add Remark'"
                 :required="remarkModal.required"
                 :initialText="remarkModal.text"
                 @save="saveRemark"
                 @cancel="cancelRemark"
                 @close="remarkModal.visible = false" />

    <ConfirmModal :isVisible="confirmPrompt.visible"
                  :title="confirmPrompt.title"
                  :message="confirmPrompt.message"
                  @confirm="handleConfirmYes"
                  @cancel="handleConfirmNo"
                  @close="confirmPrompt.visible = false" />

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
import { ref, nextTick } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import RemarkModal from '@/components/common/RemarkModal.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import { useAuthStore } from '@/stores/authStore'
import { transactionApi } from '@/api/transactionApi'
import NProgress from 'nprogress'

const authStore = useAuthStore()

const activeTab = ref('barcoded')
const specimenInput = ref(null)
const specimenNoInput = ref('')
const batchNo = ref(null)
const isSearching = ref(false)

const tabs = [
  { key: 'barcoded', label: '🔬 Barcoded Items' },
  { key: 'nonbarcoded', label: '📋 Non-Barcoded Items' },
]

// Barcoded items list
const barcodedItems = ref([])

// Non-barcoded items list
const nonBarcodedItems = ref([])
const nonBarcodedInput = ref({ description: '', quantity: 1 })
const isEndorsed = ref(false)
const isEndorsing = ref(false)

// Remark modal
const remarkModal = ref({
  visible: false,
  index: null,
  type: null,
  text: '',
  required: false,
  resolve: null  // ← for programmatic use
})

  function openRequiredRemarkModal(template) {
    return new Promise((resolve) => {
      remarkModal.value = {
        visible: true,
        index: null,
        type: null,
        text: template,
        required: true,
        resolve
      }
    })
  }

  // Manual open from table row (existing behavior)
  function openRemarkModal(index, type) {
    const item = type === 'barcoded' ? barcodedItems.value[index] : nonBarcodedItems.value[index]
    remarkModal.value = {
      visible: true,
      index,
      type,
      text: item.remarks ?? '',
      required: false,
      resolve: null
    }
  }

  function saveRemark(text) {
    const { index, type, required, resolve } = remarkModal.value

    if (required) {
      const lines = text.split('\n')
      const hasReason = lines.every(line => {
        const colonIndex = line.indexOf(':')
        if (colonIndex === -1) return true
        return line.substring(colonIndex + 1).trim().length > 0
      })
      if (!hasReason) {
        showAlert('warning', 'Reason Required', 'Please provide a reason before continuing.')
        return
      }
      remarkModal.value.visible = false
      resolve(text)
      return
    }

    if (type === 'barcoded') {
      barcodedItems.value[index].remarks = text
    } else {
      nonBarcodedItems.value[index].remarks = text
    }
    remarkModal.value.visible = false
  }

  function cancelRemark() {
    const { required, resolve } = remarkModal.value
    remarkModal.value.visible = false
    if (required && resolve) {
      resolve(null)  // user cancelled required remarks
    }
  }


// Alert modal
const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

// ---- Barcoded ----
  async function addBarcodedItem() {
    const input = specimenNoInput.value.trim().toUpperCase()
    if (!input) return

    if (barcodedItems.value.some(i => i.specimenNo === input)) {
      showAlert('warning', 'Duplicate Barcode', `Specimen ${input} has already been added to this batch.`)
      specimenNoInput.value = ''
      return
    }

    isSearching.value = true
    NProgress.start()

    try {
      const response = await transactionApi.getOrder(input)
      const data = response.data.data

      // --- Check conditions ---
      const daysDiff = Math.floor(
        (new Date() - new Date(data.transactionDate)) / (1000 * 60 * 60 * 24)
      )
      const isBeyond14 = daysDiff > 14

      // Check previously endorsed via API
      const checkResponse = await transactionApi.checkSpecimen(input)
      const isPreviouslyEndorsed = checkResponse.data.previouslyEndorsed

      // --- Build remarks template if needed ---
      let requiredRemarks = false
      let remarksTemplate = ''

      if (isBeyond14 && isPreviouslyEndorsed) {
        const confirmed = await showConfirmPrompt(
          'Warning',
          `This specimen has been previously endorsed and its transaction date is beyond 14 days (${daysDiff} days ago). Do you want to continue?`
        )
        if (!confirmed) {
          specimenNoInput.value = ''
          return
        }
        remarksTemplate = 'Reason for late endorsement: \nReason for re-endorsement: '
        requiredRemarks = true

      } else if (isBeyond14) {
        const confirmed = await showConfirmPrompt(
          'Transaction Beyond 14 Days',
          `This specimen's transaction date is beyond 14 days (${daysDiff} days ago). Do you want to continue?`
        )
        if (!confirmed) {
          specimenNoInput.value = ''
          return
        }
        remarksTemplate = 'Reason for late endorsement: '
        requiredRemarks = true

      } else if (isPreviouslyEndorsed) {
        const confirmed = await showConfirmPrompt(
          'Previously Endorsed',
          `Specimen ${input} has already been endorsed in a previous batch. Do you want to continue?`
        )
        if (!confirmed) {
          specimenNoInput.value = ''
          return
        }
        remarksTemplate = 'Reason for re-endorsement: '
        requiredRemarks = true
      }

      // --- If remarks required, open modal and wait ---
      let finalRemarks = ''
      if (requiredRemarks) {
        finalRemarks = await openRequiredRemarkModal(remarksTemplate)
        if (finalRemarks === null) {
          // user cancelled the remarks modal
          specimenNoInput.value = ''
          return
        }
      }

      // --- Push to list ---
      barcodedItems.value.push({
        specimenNo: data.specimenNo,
        labNo: data.labNo,
        trxDate: new Date(data.transactionDate).toISOString(),
        transactionDate: new Date(data.transactionDate).toLocaleDateString('en-PH'),
        patientID: data.patientID,
        patientName: data.patientName,
        sampleTypeCode: data.sampleTypeCode,
        sampleType: data.sampleTypeName,
        remarks: finalRemarks
      })

      specimenNoInput.value = ''

    } catch (err) {
      if (err.response?.status === 404) {
        showAlert('error', 'Not Found', err.response.data.message)
      } else if (err.response?.status === 400) {
        showAlert('warning', 'Invalid Input', err.response.data.message)
      } else if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        showAlert('error', 'Connection Error', 'Unable to connect to the server.')
      }
    } finally {
      isSearching.value = false
      NProgress.done()
      await nextTick()
      specimenInput.value?.focus()
    }
  }

function removeBarcodedItem(index) {
  barcodedItems.value.splice(index, 1)
}

function clearBarcodedInput() {
  specimenNoInput.value = ''
}

// ---- Non-Barcoded ----
function addNonBarcodedItem() {
  if (!nonBarcodedInput.value.description.trim()) {
    showAlert('warning', 'Missing Description', 'Please enter an item description.')
    return
  }
  if (!nonBarcodedInput.value.quantity || nonBarcodedInput.value.quantity < 1) {
    showAlert('warning', 'Invalid Quantity', 'Please enter a valid quantity.')
    return
  }

  nonBarcodedItems.value.push({
    description: nonBarcodedInput.value.description.trim(),
    quantity: nonBarcodedInput.value.quantity,
    remarks: ''
  })

  clearNonBarcodedInput()
}

function removeNonBarcodedItem(index) {
  nonBarcodedItems.value.splice(index, 1)
}

function clearNonBarcodedInput() {
  nonBarcodedInput.value = { description: '', quantity: 1 }
}


// ---- Clear All ----
function clearAll() {
    barcodedItems.value = []
    nonBarcodedItems.value = []
    specimenNoInput.value = ''
    nonBarcodedInput.value = { description: '', quantity: 1 }
    batchNo.value = null
    isEndorsed.value = false   // ← unlock for new endorsement
}

async function handleEndorse() {
    if (barcodedItems.value.length === 0 && nonBarcodedItems.value.length === 0) {
        showAlert('warning', 'No Items', 'Please add at least one item before endorsing.')
        return
    }

    isEndorsing.value = true 
    NProgress.start()

    try {
        const payload = {
            sectionCode: authStore.sectionCode,
            userID: authStore.userID,
            procDestination: authStore.branchCode,
            specimens: barcodedItems.value.map(i => ({
                specimenNo: i.specimenNo,
                labNo: i.labNo,
                trxDate: i.trxDate,
                pid: i.patientID,
                patientName: i.patientName,
                sampleTypeCode: i.sampleTypeCode,
                sampleTypeName: i.sampleType,
                remarks: i.remarks || null
            })),
            nonBarcoded: nonBarcodedItems.value.map(i => ({
                description: i.description,
                quantity: i.quantity,
                remarks: i.remarks || null
            }))
        }

        const response = await transactionApi.endorse(payload)
        batchNo.value = response.data.batchNo
        isEndorsed.value = true   // ← lock the form

        showAlert('success', 'Endorsement Successful', `Batch ${batchNo.value} has been created.`)

    } catch (err) {
        if (err.response?.status === 401) {
            showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
        } else if (err.response?.status === 400) {
            showAlert('warning', 'Invalid Request', err.response.data.message)
        } else {
            showAlert('error', 'Endorsement Failed', 'Unable to complete endorsement. Please try again.')
        }
    } finally {
        NProgress.done()
        isEndorsing.value = false
    }
}

const confirmPrompt = ref({
  visible: false,
  title: '',
  message: '',
  resolve: null
})

function showConfirmPrompt(title, message) {
  return new Promise((resolve) => {
    confirmPrompt.value = { visible: true, title, message, resolve }
  })
}

function handleConfirmYes() {
  confirmPrompt.value.visible = false
  confirmPrompt.value.resolve(true)
}

function handleConfirmNo() {
  confirmPrompt.value.visible = false
  confirmPrompt.value.resolve(false)
}
</script>

<style scoped>
  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.9) translateY(10px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
