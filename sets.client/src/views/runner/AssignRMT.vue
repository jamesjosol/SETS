<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Assign RMT / Section Receiving</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
    </div>

    <!-- Barcode scan field -->
    <div class="mb-5 rounded-2xl p-5 flex items-center gap-4"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <div class="p-3 rounded-xl" style="background-color: var(--color-primary-soft);">
        <span class="material-symbols-outlined text-lg" style="color: var(--color-primary);">barcode_scanner</span>
      </div>
      <div class="flex-1">
        <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Scan Barcode</p>
        <input ref="scanInput"
               v-model="scanValue"
               type="text"
               placeholder="Scan or enter specimen no. here..."
               class="w-full px-3 py-2 rounded-xl text-sm outline-none transition-all"
               style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);"
               @keydown.enter="scanSpecimen" />
      </div>
      <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.97]"
              style="background: var(--color-primary-gradient); color: #fff;"
              :disabled="scanning || !scanValue.trim()"
              @click="scanSpecimen">
        {{ scanning ? 'Scanning...' : 'Add' }}
      </button>
    </div>

    <!-- Scanned list -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
        <div class="flex items-center gap-2">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">assignment_ind</span>
          <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Scanned Specimens</h2>
        </div>
        <span class="text-xs font-bold" style="color: var(--color-text-muted);">
          {{ specimenGroups.length }} specimen{{ specimenGroups.length !== 1 ? 's' : '' }}
        </span>
      </div>

      <!-- Empty state -->
      <div v-if="!specimenGroups.length" class="p-12 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">assignment_ind</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No specimens scanned</p>
        <p class="text-xs" style="color: var(--color-text-muted);">Scan a specimen barcode above to assign an RMT.</p>
      </div>

      <!-- Grouped table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr style="border-bottom: 1.5px solid var(--color-border); background-color: var(--color-surface-low);">
              <th class="w-8 px-4 py-3"></th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Group</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tests</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
              <th class="w-8 px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="group in specimenGroups" :key="group.specimenNo">

              <!-- Specimen header row -->
              <tr class="cursor-pointer transition-colors"
                  :style="expandedSpecimen === group.specimenNo
                    ? 'background-color: var(--color-primary-soft);'
                    : ''"
                  @mouseenter="e => { if (expandedSpecimen !== group.specimenNo) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                  @mouseleave="e => { if (expandedSpecimen !== group.specimenNo) e.currentTarget.style.backgroundColor = '' }"
                  @click="toggleSpecimen(group.specimenNo)"
                  style="border-top: 1px solid var(--color-border);">

                <td class="px-4 py-3">
                  <span class="material-symbols-outlined text-sm transition-transform duration-200"
                        :style="{
                          color: 'var(--color-primary)',
                          display: 'block',
                          transform: expandedSpecimen === group.specimenNo ? 'rotate(90deg)' : 'rotate(0deg)'
                        }">
                    chevron_right
                  </span>
                </td>

                <td class="px-4 py-3">
                  <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ group.specimenNo }}</p>
                  <p v-if="group.firstScan"
                     class="text-[10px] font-bold mt-0.5"
                     style="color: var(--color-success, #16a34a);">
                    ✓ Received now
                  </p>
                </td>

                <td class="px-4 py-3">
                  <span class="font-bold text-xs font-mono px-2 py-0.5 rounded-lg"
                        style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                    {{ group.testGroupCode }}
                  </span>
                </td>

                <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted);">
                  {{ group.sampleTypeCode }}
                </td>

                <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted);">
                  {{ group.firstScan ? 'Just now' : formatDt(group.received) }}
                </td>

                <td class="px-4 py-3">
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">
                    {{ group.tests.length }} test{{ group.tests.length !== 1 ? 's' : '' }}
                  </span>
                </td>

                <!-- Remarks button -->
                <td class="px-4 py-3" @click.stop>
                  <button class="p-1.5 rounded-lg transition-all"
                          :style="group.remarks
            ? 'color: var(--color-warning);'
            : 'color: var(--color-text-muted);'"
                          @mouseenter="e => e.currentTarget.style.color = 'var(--color-primary)'"
                          @mouseleave="e => e.currentTarget.style.color = group.remarks ? 'var(--color-warning)' : 'var(--color-text-muted)'"
                          @click="openRemarks(group)">
                    <span class="material-symbols-outlined text-sm">
                      {{ group.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                    </span>
                  </button>
                </td>

                <td class="px-4 py-3" @click.stop>
                  <button class="p-1.5 rounded-lg transition-all"
                          style="color: var(--color-text-muted);"
                          @mouseenter="e => e.currentTarget.style.color = '#ba1a1a'"
                          @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
                          @click="removeSpecimen(group.specimenNo)">
                    <span class="material-symbols-outlined text-sm">close</span>
                  </button>
                </td>
              </tr>
              <!-- Expanded child test rows -->
              <Transition name="expand">
                <tr v-if="expandedSpecimen === group.specimenNo" :key="`exp-${group.specimenNo}`">
                  <td colspan="8" class="px-0 py-0">
                    <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border);">
                      <table class="w-full text-xs">
                        <thead>
                          <tr style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border);">
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Running Date</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="test in group.tests" :key="test.id"
                              :style="(test.status === 'R' || test.status === 'X')
                              ? 'border-top: 1px solid var(--color-border); opacity: 0.45; pointer-events: none;'
                              : 'border-top: 1px solid var(--color-border);'">

                            <td class="px-4 py-3">
                              <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                            </td>

                            <td class="px-4 py-3" style="color: var(--color-text);">{{ test.testName }}</td>

                            <td class="px-4 py-3">
                              <div class="flex items-center gap-1.5">
                                <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                      :style="testStatusStyle(test.status)">
                                  {{ testStatusLabel(test.status) }}
                                </span>
                                <span v-if="test.status === 'R' || test.status === 'X'"
                                      class="material-symbols-outlined text-xs"
                                      style="color: var(--color-text-muted);"
                                      title="Cannot be changed">
                                  lock
                                </span>
                              </div>
                            </td>

                            <!-- Assigned RMT — auto-filled to current user, editable -->
                            <td class="px-4 py-3">
                              <select v-model="test.selectedRMT"
                                      class="px-2 py-1 rounded-lg text-xs outline-none transition-all"
                                      style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text); min-width: 130px;">
                                <option value="">— None —</option>
                                <option v-for="rmt in availableRMTs" :key="rmt.userID" :value="rmt.userID">
                                  {{ rmt.fullName ?? rmt.userID }}
                                </option>
                              </select>
                            </td>

                            <!-- Schedule tag pills: Today / ERD / CRD / SRD -->
                            <td class="px-4 py-3">
                              <div class="flex items-center gap-1.5 flex-wrap">
                                <label v-for="tag in scheduleTags" :key="tag.value"
                                       class="flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest cursor-pointer px-2.5 py-1 rounded-lg transition-all select-none"
                                       :style="test.scheduleTag === tag.value
                                       ? scheduleTagActiveStyle(tag.value)
                                       : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'">
                                  <input type="radio"
                                         :name="`tag-${test.id}`"
                                         :value="tag.value"
                                         v-model="test.scheduleTag"
                                         class="hidden" />
                                  {{ tag.label }}
                                </label>
                              </div>
                            </td>

                            <!-- Running date — only visible for CRD -->
                            <td class="px-4 py-3">
                              <input v-if="test.scheduleTag === 'CRD'"
                                     type="date"
                                     v-model="test.runningDate"
                                     class="px-2 py-1 rounded-lg text-xs outline-none"
                                     style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);" />
                              <span v-else class="text-[10px]" style="color: var(--color-text-muted);">—</span>
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

        <!-- Save / Clear bar -->
        <div class="px-6 py-4 flex items-center justify-end gap-3"
             style="border-top: 1.5px solid var(--color-border);">
          <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  @click="clearAll">
            Clear All
          </button>
          <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.97]"
                  style="background: var(--color-primary-gradient); color: #fff;"
                  :disabled="saving || !specimenGroups.length"
                  @click="saveAssignments">
            {{ saving ? 'Saving...' : 'Save Assignments' }}
          </button>
        </div>
      </div>
    </div>
    <!-- Remarks Modal -->
    <RemarkModal :isVisible="remarksModal.visible"
                 title="Specimen Remarks"
                 :initialText="remarksModal.text"
                 @save="saveRemarks"
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
  import { ref, onMounted, nextTick } from 'vue'
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

  const scanInput = ref(null)
  const scanValue = ref('')
  const scanning = ref(false)
  const saving = ref(false)
  const expandedSpecimen = ref(null)
  const specimenGroups = ref([])
  const availableRMTs = ref([])

  // Schedule tag options
  // TODAY = no tag in DB, test runs today → status R
  // ERD/CRD/SRD = saved with schedule → status S
  const scheduleTags = [
    { value: 'TODAY', label: 'Today' },
    { value: 'ERD', label: 'ERD' },
    { value: 'CRD', label: 'CRD' },
    { value: 'SRD', label: 'SRD' },
  ]

  // ── Expand / collapse ──────────────────────────────────────────────────────

  function toggleSpecimen(specimenNo) {
    expandedSpecimen.value = expandedSpecimen.value === specimenNo ? null : specimenNo
  }

  // ── Scan ───────────────────────────────────────────────────────────────────

  async function scanSpecimen() {
    const val = scanValue.value.trim()
    if (!val) return

    // Block duplicate scan in same session
    if (specimenGroups.value.some(g => g.specimenNo === val)) {
      showAlert('warning', 'Already Scanned', `Specimen ${val} is already in the list.`)
      scanValue.value = ''
      await nextTick()
      scanInput.value?.focus()
      return
    }

    scanning.value = true
    try {
      const result = await runnerApi.scanSpecimen({
        specimenNo: val,
        sectionCode: authStore.sectionCode,
        userID: authStore.userID,
      })

      const {
        firstScan, tests, specimenNo,
        testGroupCode, sampleTypeCode,
        received, receivedBy
      } = result.data

      if (!tests || !tests.length) {
        showAlert('warning', 'No Tests', `No pending tests found for specimen ${val}.`)
        return
      }

      // Build group entry
      // Auto-assign current user as RMT, default schedule = Today
      specimenGroups.value.push({
        specimenNo,
        headerId: result.data.headerId,
        testGroupCode,
        sampleTypeCode,
        firstScan,
        received,
        receivedBy,
        remarks: result.data.remarks ?? '', 
        tests: tests.map(t => ({
          ...t,
          selectedRMT: authStore.userID,  // ← auto-assign logged-in user
          scheduleTag: 'TODAY',           // ← default to Today
          runningDate: null,
        }))
      })

      // Auto-expand newly scanned specimen
      expandedSpecimen.value = specimenNo

      if (firstScan) {
        showAlert('success', 'Received', `Specimen ${val} marked as received by section.`)
      }

    } catch (e) {
      showAlert('error', 'Scan Failed', e?.response?.data?.message ?? 'Could not scan specimen.')
    } finally {
      scanning.value = false
      scanValue.value = ''
      await nextTick()
      scanInput.value?.focus()
    }
  }

  // ── Remove ─────────────────────────────────────────────────────────────────

  function removeSpecimen(specimenNo) {
    specimenGroups.value = specimenGroups.value.filter(g => g.specimenNo !== specimenNo)
    if (expandedSpecimen.value === specimenNo) expandedSpecimen.value = null
  }

  function clearAll() {
    specimenGroups.value = []
    expandedSpecimen.value = null
  }

  // ── Save ───────────────────────────────────────────────────────────────────

  async function saveAssignments() {
    saving.value = true
    try {
      // Flatten all groups into one assignments array
      // TODAY → scheduleTag = null (runs today, will become status R if RMT set)
      const assignments = specimenGroups.value.flatMap(g =>
        g.tests.map(t => ({
          testId: t.id,
          assignedRMT: t.selectedRMT || null,
          scheduleTag: t.scheduleTag === 'TODAY' ? null : (t.scheduleTag || null),
          runningDate: t.scheduleTag === 'CRD' ? (t.runningDate || null) : null,
        }))
      )
      // Inside saveAssignments(), before the API call:
      for (const group of specimenGroups.value) {
        for (const test of group.tests) {
          if (test.scheduleTag === 'CRD' && !test.runningDate) {
            showAlert('warning', 'Running Date Required',
              `Test ${test.testCode} is tagged CRD but has no running date specified.`)
            saving.value = false
            return
          }
        }
      }
      await runnerApi.saveAssignments({
        userID: authStore.userID,
        assignments,
        specimenRemarks: specimenGroups.value.map(g => ({
          headerId: g.headerId, 
          remarks: g.remarks || null,
        }))
      })

      showAlert('success', 'Saved', 'Assignments saved successfully.')
      specimenGroups.value = []
      expandedSpecimen.value = null

    } catch (e) {
      showAlert('error', 'Save Failed', e?.response?.data?.message ?? 'Could not save assignments.')
    } finally {
      saving.value = false
    }
  }

  const remarksModal = ref({ visible: false, specimenNo: '', text: '' })

  function openRemarks(group) {
    remarksModal.value = {
      visible: true,
      specimenNo: group.specimenNo,
      text: group.remarks ?? '',
    }
  }

  function saveRemarks(text) {
    const group = specimenGroups.value.find(g => g.specimenNo === remarksModal.value.specimenNo)
    if (group) group.remarks = text
    remarksModal.value.visible = false
  }

  // ── Formatters ─────────────────────────────────────────────────────────────

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', {
      month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit'
    })
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

  function scheduleTagActiveStyle(tag) {
    const map = {
      TODAY: 'background-color: rgba(22,163,74,0.12); color: var(--color-success, #16a34a);',
      ERD: 'background-color: rgba(70,21,153,0.12); color: var(--color-primary);',
      CRD: 'background-color: rgba(217,119,6,0.12); color: var(--color-warning);',
      SRD: 'background-color: rgba(74,98,109,0.12); color: var(--color-info, #4a626d);',
    }
    return map[tag] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  // ── Mount ──────────────────────────────────────────────────────────────────

  onMounted(async () => {
    // Seed dropdown with current user for now
    // Replace with API call when RMT list endpoint is ready
    availableRMTs.value = [{
      userID: authStore.userID,
      fullName: authStore.userName ?? authStore.userID,
    }]

    await nextTick()
    scanInput.value?.focus()
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
