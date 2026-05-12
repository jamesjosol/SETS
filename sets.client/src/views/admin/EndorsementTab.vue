<template>
  <div class="space-y-6">

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted)">
        Cross-branch mapping
      </p>
      <p class="text-xs" style="color: var(--color-text-muted)">
        Register external branches and their sections for outbound endorsement.
      </p>
    </div>

    <!-- Outbound feature toggle -->
    <div class="flex items-center justify-between px-5 py-4 rounded-2xl"
         style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
      <div class="flex items-center gap-3">
        <div class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
             :style="branchSettings.isOutboundEnabled
           ? 'background-color: var(--color-success-soft)'
           : 'background-color: var(--color-surface-low)'">
          <span class="material-symbols-outlined text-base"
                :style="branchSettings.isOutboundEnabled
              ? 'color: var(--color-success)'
              : 'color: var(--color-text-muted)'">alt_route</span>
        </div>
        <div>
          <p class="text-sm font-bold" style="color: var(--color-text)">Outbound Endorsement</p>
          <p class="text-xs" style="color: var(--color-text-muted)">
            {{
 branchSettings.isOutboundEnabled
          ? 'Enabled — endorsers can send batches to partner branches'
          : 'Disabled — endorsers can only endorse to local processing'
            }}
          </p>
        </div>
      </div>

      <!-- Toggle -->
      <button @click="toggleOutbound"
              :disabled="togglingOutbound"
              class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
              :style="branchSettings.isOutboundEnabled
            ? 'background-color: var(--color-success-soft); color: var(--color-success)'
            : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border)'">
        <span class="material-symbols-outlined" style="font-size: 14px">
          {{ togglingOutbound ? 'progress_activity' : branchSettings.isOutboundEnabled ? 'toggle_on' : 'toggle_off' }}
        </span>
        {{ branchSettings.isOutboundEnabled ? 'Enabled' : 'Disabled' }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-16">
      <span class="material-symbols-outlined animate-spin text-2xl" style="color: var(--color-primary)">progress_activity</span>
    </div>



    <!-- Card grid (partners + add card) -->
    <div v-else class="grid gap-4" style="grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))">

      <!-- Partner cards -->
      <div v-for="partner in partners"
           :key="partner.code"
           class="rounded-2xl p-4 space-y-3"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border)">

        <!-- Card header -->
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs font-extrabold flex-shrink-0"
               style="background-color: var(--color-primary-soft); color: var(--color-primary)">
            {{ (partner.code ?? '???').substring(0, 3) }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-bold truncate" style="color: var(--color-text)">{{ partner.name }}</p>
            <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">
              {{ partner.code }} · {{ activeSections(partner).length }} section{{ activeSections(partner).length !== 1 ? 's' : '' }}
            </p>
          </div>

          <!-- Active toggle -->
          <button @click="togglePartner(partner)"
                  class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all flex-shrink-0"
                  :style="partner.active
                    ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border)'">
            <span class="material-symbols-outlined" style="font-size: 13px">
              {{ partner.active ? 'check_circle' : 'cancel' }}
            </span>
            {{ partner.active ? 'Active' : 'Inactive' }}
          </button>

          <!-- Unregister -->
          <button @click="confirmUnregister(partner)"
                  class="w-8 h-8 rounded-xl flex items-center justify-center transition-all active:scale-95 flex-shrink-0"
                  style="background-color: var(--color-error-soft); color: var(--color-error)">
            <span class="material-symbols-outlined" style="font-size: 15px">link_off</span>
          </button>
        </div>

        <!-- Divider -->
        <div style="border-top: 1px solid var(--color-border)"></div>

        <!-- Sections list -->
        <div class="space-y-1.5">
          <div v-if="activeSections(partner).length === 0"
               class="text-xs py-2 text-center rounded-xl"
               style="color: var(--color-text-muted); background-color: var(--color-surface-low)">
            No sections registered
          </div>

          <div v-for="section in activeSections(partner)"
               :key="section.code"
               class="flex items-center gap-2 px-3 py-2 rounded-xl"
               style="background-color: var(--color-surface-low)">

            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold" style="color: var(--color-text)">
                {{ section.code }} — {{ section.name }}
              </p>
            </div>

            <!-- Category pill -->
            <span class="text-[10px] font-bold px-2 py-0.5 rounded-full flex-shrink-0"
                  :style="section.category === '2'
                    ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                    : 'background-color: var(--color-primary-soft); color: var(--color-primary)'">
              {{ section.category === '2' ? 'Processing' : 'Endorser' }}
            </span>

            <!-- Remove -->
            <button @click="confirmRemoveSection(partner, section)"
                    class="w-6 h-6 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    title="Remove section">
              <span class="material-symbols-outlined" style="font-size: 14px">close</span>
            </button>
          </div>
        </div>

        <!-- Add section row -->
        <div class="flex items-center gap-2"
             style="border-top: 1px dashed var(--color-border); padding-top: 10px">
          <input v-model="sectionInputs[partner.code]"
                 type="text"
                 maxlength="10"
                 placeholder="Section code"
                 @keyup.enter="lookupAndAdd(partner)"
                 class="flex-1 px-3 py-2 rounded-xl text-xs outline-none uppercase"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                 @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
          <button @click="lookupAndAdd(partner)"
                  :disabled="lookingUp[partner.code]"
                  class="px-3 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-1.5 flex-shrink-0"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
            <span class="material-symbols-outlined" style="font-size: 14px">
              {{ lookingUp[partner.code] ? 'progress_activity' : 'search' }}
            </span>
            {{ lookingUp[partner.code] ? 'Looking up...' : 'Add' }}
          </button>
        </div>

        <!-- Lookup preview -->
        <Transition name="fade">
          <div v-if="previews[partner.code]"
               class="rounded-xl p-3 flex items-center gap-3"
               style="background-color: var(--color-primary-soft); border: 1px solid var(--color-primary)">
            <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">check_circle</span>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold" style="color: var(--color-primary)">
                {{ previews[partner.code].code }} — {{ previews[partner.code].name }}
              </p>
              <p class="text-[10px]" style="color: var(--color-primary)">
                {{ previews[partner.code].category === '2' ? 'Processing section' : 'Endorser section' }}
              </p>
            </div>
            <button @click="confirmAddSection(partner)"
                    class="px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                    style="background: var(--color-primary-gradient); color: #ffffff">
              Confirm
            </button>
            <button @click="clearPreview(partner.code)"
                    class="w-6 h-6 rounded-lg flex items-center justify-center"
                    style="color: var(--color-primary)">
              <span class="material-symbols-outlined" style="font-size: 14px">close</span>
            </button>
          </div>
        </Transition>

      </div>

      <!-- Add partner branch — dashed card -->
      <button @click="openAddPartner"
              class="rounded-2xl flex flex-col items-center justify-center gap-2 transition-all active:scale-95 min-h-40 cursor-pointer"
              style="border: 1.5px dashed var(--color-text-muted); color: var(--color-text-muted); background: transparent; min-height: 160px">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center"
             style="background-color: var(--color-surface-low)">
          <span class="material-symbols-outlined text-xl" style="color: var(--color-text-muted)">add</span>
        </div>
        <span class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">
          Add partner branch
        </span>
      </button>

    </div>

    <!-- Add partner modal -->
    <Transition name="fade">
      <div v-if="addPartnerModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background: rgba(0,0,0,0.4)"
           @click.self="addPartnerModal.visible = false">
        <div class="rounded-2xl p-6 w-full max-w-sm space-y-4"
             style="background-color: var(--color-surface); border: 1px solid var(--color-border)">

          <div class="flex items-center justify-between">
            <p class="text-sm font-bold" style="color: var(--color-text)">Add partner branch</p>
            <button @click="addPartnerModal.visible = false">
              <span class="material-symbols-outlined text-lg" style="color: var(--color-text-muted)">close</span>
            </button>
          </div>

          <p class="text-xs" style="color: var(--color-text-muted)">
            Select a branch from Branch Master to register as an outbound endorsement partner.
          </p>

          <div v-if="eligibleBranches.length === 0"
               class="text-xs text-center py-4 rounded-xl"
               style="color: var(--color-text-muted); background-color: var(--color-surface-low)">
            All branches are already registered as partners.
          </div>

          <div v-else class="space-y-2">
            <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                   style="color: var(--color-text-muted)">Select branch</label>
            <select v-model="addPartnerModal.selectedCode"
                    class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                    style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border)">
              <option value="" disabled>Choose a branch...</option>
              <option v-for="b in eligibleBranches" :key="b.code" :value="b.code">
                {{ b.name }} ({{ b.code }})
              </option>
            </select>
          </div>

          <div class="flex justify-end gap-2 pt-2">
            <button @click="addPartnerModal.visible = false"
                    class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
              Cancel
            </button>
            <button @click="registerPartner"
                    :disabled="!addPartnerModal.selectedCode || addPartnerModal.saving"
                    class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                    style="background: var(--color-primary-gradient); color: #ffffff">
              <span class="material-symbols-outlined text-sm">
                {{ addPartnerModal.saving ? 'progress_activity' : 'add' }}
              </span>
              {{ addPartnerModal.saving ? 'Registering...' : 'Register' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Alert modal -->
    <AlertModal :is-visible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false" />

    <!-- Confirm modal -->
    <ConfirmModal :is-visible="confirm.isVisible"
                  :title="confirm.title"
                  :message="confirm.message"
                  :confirm-label="confirm.confirmLabel"
                  :type="confirm.type"
                  @confirm="confirm.onConfirm"
                  @cancel="confirm.isVisible = false" />

  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import endorsementSetupApi from '@/api/endorsementSetupApi'
  import AlertModal from '@/components/common/AlertModal.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'

  const emit = defineEmits(['toast'])

  // ── State ──────────────────────────────────────────────────────────────────

  const loading = ref(false)
  const partners = ref([])
  const eligibleBranches = ref([])

  const sectionInputs = ref({})
  const lookingUp = ref({})
  const previews = ref({})

  const addPartnerModal = ref({
    visible: false,
    selectedCode: '',
    saving: false
  })

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  const confirm = ref({
    isVisible: false,
    title: '',
    message: '',
    confirmLabel: 'Confirm',
    type: 'warning',
    onConfirm: () => { }
  })

  // ── Helpers ────────────────────────────────────────────────────────────────

  function activeSections(partner) {
    return (partner.sections ?? []).filter(s => s.active)
  }

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  function showConfirm(title, message, confirmLabel, type, onConfirm) {
    confirm.value = { isVisible: true, title, message, confirmLabel, type, onConfirm }
  }

  function clearPreview(branchCode) {
    previews.value[branchCode] = null
    sectionInputs.value[branchCode] = ''
  }

  // ── Load ───────────────────────────────────────────────────────────────────

  async function load() {
    loading.value = true
    try {
      partners.value = await endorsementSetupApi.getPartners()
      console.log('partners response:', JSON.stringify(partners.value))  // ← add this
      partners.value.forEach(p => {
        sectionInputs.value[p.code] = ''
        lookingUp.value[p.code] = false
        previews.value[p.code] = null
      })
    } catch {
      showAlert('error', 'Load Failed', 'Unable to load partner branches.')
    } finally {
      loading.value = false
    }
  }
  async function loadEligible() {
    try {
      eligibleBranches.value = await endorsementSetupApi.getEligibleBranches()
    } catch {
      eligibleBranches.value = []
    }
  }


  // ── Add partner ────────────────────────────────────────────────────────────

  async function openAddPartner() {
    await loadEligible()
    addPartnerModal.value = { visible: true, selectedCode: '', saving: false }
  }

  async function registerPartner() {
    if (!addPartnerModal.value.selectedCode) return
    addPartnerModal.value.saving = true
    try {
      await endorsementSetupApi.registerPartner(addPartnerModal.value.selectedCode)
      addPartnerModal.value.visible = false
      emit('toast', 'Partner branch registered.')
      await load()
    } catch (err) {
      showAlert('error', 'Registration Failed', err.response?.data?.message ?? 'Could not register branch.')
    } finally {
      addPartnerModal.value.saving = false
    }
  }

  // ── Toggle partner ─────────────────────────────────────────────────────────

  async function togglePartner(partner) {
    try {
      await endorsementSetupApi.togglePartner(partner.code)
      partner.active = !partner.active
      emit('toast', `${partner.name} ${partner.active ? 'activated' : 'deactivated'}.`)
    } catch (err) {
      showAlert('error', 'Toggle Failed', err.response?.data?.message ?? 'Could not update status.')
    }
  }

  // ── Unregister partner ─────────────────────────────────────────────────────

  function confirmUnregister(partner) {
    showConfirm(
      'Unregister partner',
      `Remove ${partner.name} as an outbound endorsement partner? All registered sections will be deactivated.`,
      'Unregister',
      'error',
      () => unregisterPartner(partner)
    )
  }

  async function unregisterPartner(partner) {
    confirm.value.isVisible = false
    try {
      await endorsementSetupApi.unregisterPartner(partner.code)
      emit('toast', `${partner.name} unregistered.`)
      await load()
    } catch (err) {
      showAlert('error', 'Unregister Failed', err.response?.data?.message ?? 'Could not unregister branch.')
    }
  }

  // ── Section lookup + add ───────────────────────────────────────────────────

  async function lookupAndAdd(partner) {
    const input = (sectionInputs.value[partner.code] ?? '').trim().toUpperCase()
    if (!input) return

    if (activeSections(partner).some(s => s.code === input)) {
      showAlert('warning', 'Already Registered', `Section ${input} is already registered for ${partner.name}.`)
      return
    }

    lookingUp.value[partner.code] = true
    previews.value[partner.code] = null

    try {
      const result = await endorsementSetupApi.lookupSection(partner.code, input)
      previews.value[partner.code] = result
    } catch (err) {
      const status = err.response?.status
      if (status === 404) {
        showAlert('warning', 'Not Found', err.response.data.message)
      } else if (status === 503) {
        showAlert('error', 'Connection Error', err.response.data.message)
      } else {
        showAlert('error', 'Lookup Failed', 'Could not look up section.')
      }
      sectionInputs.value[partner.code] = ''
    } finally {
      lookingUp.value[partner.code] = false
    }
  }

  async function confirmAddSection(partner) {
    const preview = previews.value[partner.code]
    if (!preview) return

    try {
      await endorsementSetupApi.addSection(partner.code, {
        code: preview.code,
        name: preview.name,
        category: preview.category
      })

      partner.sections.push({
        code: preview.code,
        name: preview.name,
        category: preview.category,
        active: true
      })

      clearPreview(partner.code)
      emit('toast', `Section ${preview.code} added to ${partner.name}.`)
    } catch (err) {
      showAlert('error', 'Save Failed', err.response?.data?.message ?? 'Could not save section.')
    }
  }

  // ── Remove section ─────────────────────────────────────────────────────────

  function confirmRemoveSection(partner, section) {
    showConfirm(
      'Remove section',
      `Remove ${section.code} — ${section.name} from ${partner.name}?`,
      'Remove',
      'warning',
      () => removeSection(partner, section)
    )
  }

  async function removeSection(partner, section) {
    confirm.value.isVisible = false
    try {
      await endorsementSetupApi.removeSection(partner.code, section.code)
      section.active = false
      emit('toast', `Section ${section.code} removed.`)
    } catch (err) {
      showAlert('error', 'Remove Failed', err.response?.data?.message ?? 'Could not remove section.')
    }
  }

  // ── Branch settings ────────────────────────────────────────────────────────
  const branchSettings = ref({ isOutboundEnabled: false, updatedAt: null, updatedBy: null })
  const togglingOutbound = ref(false)

  async function loadSettings() {
    try {
      branchSettings.value = await endorsementSetupApi.getSettings()
    } catch {
      branchSettings.value = { isOutboundEnabled: false }
    }
  }

  async function toggleOutbound() {
    togglingOutbound.value = true
    try {
      await endorsementSetupApi.setOutbound(!branchSettings.value.isOutboundEnabled)
      branchSettings.value.isOutboundEnabled = !branchSettings.value.isOutboundEnabled
      emit('toast', `Outbound endorsement ${branchSettings.value.isOutboundEnabled ? 'enabled' : 'disabled'}.`)
    } catch (err) {
      showAlert('error', 'Toggle Failed', err.response?.data?.message ?? 'Could not update setting.')
    } finally {
      togglingOutbound.value = false
    }
  }

  // Add loadSettings() to onMounted:
  onMounted(async () => {
    await load()
    await loadSettings()
  })


</script>

<style scoped>
  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.2s ease;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }
</style>
