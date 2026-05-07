<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">computer</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
            PC Registration
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Register workstations by IP address and assign which sections they can access.
          </p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAddPC">
        <span class="material-symbols-outlined text-sm">add</span>
        Register PC
      </button>
    </div>

    <!-- Table -->
    <div class="px-8 py-6 space-y-5">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="pcSearch"
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
               placeholder="Search by IP, description, or section..."
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>

      <AppTable :rows="filteredPCs" :columns="pcColumns" row-key="id" :page-size="10"
                :loading="pcLoading" loading-text="Loading registered PCs..."
                empty-text="No PCs registered yet." empty-icon="computer_off">
        <template #cell-ipAddress="{ value }">
          <span class="font-mono font-bold text-xs" style="color: var(--color-text)">{{ value }}</span>
        </template>
        <template #cell-description="{ value }">
          <span class="text-sm" style="color: var(--color-text-muted)">{{ value || "—" }}</span>
        </template>
        <template #cell-sections="{ row }">
          <div class="flex flex-wrap gap-1">
            <span v-if="!row.sections.length" class="text-xs font-bold" style="color: var(--color-text-muted)">No sections</span>
            <span v-for="sec in row.sections" :key="sec.id"
                  class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
              {{ sec.sectionName }}
            </span>
          </div>
        </template>
        <template #cell-active="{ row }">
          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="row.active
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                  @click.stop="togglePC(row)">
            <span class="material-symbols-outlined text-xs">{{ row.active ? "check_circle" : "cancel" }}</span>
            {{ row.active ? "Active" : "Inactive" }}
          </button>
        </template>
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Edit PC"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditPC(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Delete PC"
                    style="color: #ba1a1a"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="confirmDeletePC(row)">
              <span class="material-symbols-outlined text-sm">delete</span>
            </button>
          </div>
        </template>
      </AppTable>
    </div>
  </div>

  <!-- Add / Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="pcModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closePCModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24);">
          <!-- Modal Header -->
          <div class="px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">computer</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ pcModal.mode === "add" ? "Register New PC" : "Edit PC" }}
              </h3>
            </div>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closePCModal">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Modal Body -->
          <div class="px-6 py-5 space-y-4">
            <!-- IP Address (add only — read-only on edit) -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">IP Address *</label>
              <input v-model="pcModal.form.ipAddress"
                     placeholder="e.g. 192.168.1.100"
                     :readonly="pcModal.mode === 'edit'"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                     :style="pcModal.mode === 'edit'
                       ? 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border); cursor: not-allowed;'
                       : 'background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);'"
                     @focus="(e) => { if (pcModal.mode !== 'edit') e.target.style.borderColor = 'var(--color-primary)' }"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Description (both add and edit) -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">Description</label>
              <input v-model="pcModal.form.description"
                     placeholder="e.g. Hematology workstation"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Section Assignments (both add and edit) -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                     style="color: var(--color-text-muted)">Allowed Sections</label>
              <div class="rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border)">
                <div v-for="sec in availableSections" :key="sec.code"
                     class="flex items-center justify-between px-4 py-2.5 transition-all cursor-pointer"
                     style="border-bottom: 1px solid var(--color-surface-low)"
                     :style="pcModal.form.sectionCodes.includes(sec.code) ? 'background-color: var(--color-primary-soft);' : ''"
                     @click="toggleSectionInModal(sec.code)">
                  <span class="text-sm font-medium" style="color: var(--color-text)">{{ sec.name }}</span>
                  <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all"
                       :style="pcModal.form.sectionCodes.includes(sec.code)
                         ? 'background-color: var(--color-primary);'
                         : 'border: 1.5px solid var(--color-border);'">
                    <span v-if="pcModal.form.sectionCodes.includes(sec.code)"
                          class="material-symbols-outlined text-xs" style="color: #ffffff">check</span>
                  </div>
                </div>
                <p v-if="!availableSections.length" class="px-4 py-3 text-xs text-center"
                   style="color: var(--color-text-muted)">No sections available.</p>
              </div>
            </div>

            <p v-if="pcModal.error" class="text-xs font-bold" style="color: #ba1a1a">{{ pcModal.error }}</p>
          </div>

          <!-- Modal Footer -->
          <div class="px-6 py-4 flex justify-end gap-3" style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="closePCModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="pcModal.saving"
                    @click="savePCModal">
              <span v-if="pcModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ pcModal.mode === "add" ? "Register" : "Save" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Delete Confirm Modal -->
  <ConfirmModal :isVisible="deleteConfirm.visible"
                type="error"
                title="Delete PC"
                :message="`Delete ${deleteConfirm.ipAddress}? This will remove the PC and all its section assignments permanently.`"
                confirm-label="Delete"
                @close="deleteConfirm.visible = false"
                @confirm="executeDeletePC" />
</template>

<script setup>
  import { ref, computed, onMounted } from "vue";
  import { pcApi } from "@/api/pcApi";
  import { settingsApi } from "@/api/settingsApi";
  import AppTable from "@/components/common/AppTable.vue";
  import ConfirmModal from "@/components/common/ConfirmModal.vue";

  const emit = defineEmits(["toast"]);

  // ── State ──────────────────────────────────────────────────────────────────
  const pcList = ref([]);
  const pcLoading = ref(false);
  const availableSections = ref([]);
  const pcSearch = ref("");

  const pcColumns = [
    { key: "ipAddress", label: "IP Address" },
    { key: "description", label: "Description" },
    { key: "sections", label: "Allowed Sections" },
    { key: "active", label: "Status" },
  ];

  const pcModal = ref({
    visible: false,
    mode: "add",       // "add" | "edit"
    pcId: null,
    form: { ipAddress: "", description: "", sectionCodes: [] },
    saving: false,
    error: "",
  });

  const deleteConfirm = ref({ visible: false, pcId: null, ipAddress: "" });

  const filteredPCs = computed(() => {
    const q = pcSearch.value.toLowerCase();
    if (!q) return pcList.value;
    return pcList.value.filter(
      (pc) =>
        pc.ipAddress.toLowerCase().includes(q) ||
        (pc.description ?? "").toLowerCase().includes(q) ||
        pc.sections.some((s) => s.sectionName.toLowerCase().includes(q)),
    );
  });

  // ── Data loading ───────────────────────────────────────────────────────────
  async function loadPCs() {
    pcLoading.value = true;
    try {
      const res = await pcApi.getAll();
      pcList.value = res.data;
    } catch {
      pcList.value = [];
    } finally {
      pcLoading.value = false;
    }
  }

  async function loadSections() {
    try {
      const res = await settingsApi.getSections();
      availableSections.value = res.data.filter((s) => s.active);
    } catch {
      availableSections.value = [];
    }
  }

  onMounted(() => {
    loadPCs();
    loadSections();
  });

  // ── Modal: Add ─────────────────────────────────────────────────────────────
  function openAddPC() {
    pcModal.value = {
      visible: true,
      mode: "add",
      pcId: null,
      form: { ipAddress: "", description: "", sectionCodes: [] },
      saving: false,
      error: "",
    };
  }

  // ── Modal: Edit (description + sections combined) ──────────────────────────
  function openEditPC(pc) {
    pcModal.value = {
      visible: true,
      mode: "edit",
      pcId: pc.id,
      form: {
        ipAddress: pc.ipAddress,
        description: pc.description ?? "",
        sectionCodes: pc.sections.map((s) => s.sectionCode),
      },
      saving: false,
      error: "",
    };
  }

  function closePCModal() {
    pcModal.value.visible = false;
  }

  function toggleSectionInModal(code) {
    const idx = pcModal.value.form.sectionCodes.indexOf(code);
    if (idx === -1) pcModal.value.form.sectionCodes.push(code);
    else pcModal.value.form.sectionCodes.splice(idx, 1);
  }

  async function savePCModal() {
    pcModal.value.error = "";
    pcModal.value.saving = true;
    try {
      if (pcModal.value.mode === "add") {
        if (!pcModal.value.form.ipAddress.trim()) {
          pcModal.value.error = "IP Address is required.";
          return;
        }
        await pcApi.add({
          ipAddress: pcModal.value.form.ipAddress.trim(),
          description: pcModal.value.form.description.trim(),
          sectionCodes: pcModal.value.form.sectionCodes,
        });
        emit("toast", "PC registered successfully.");
      } else {
        // Update description + active in one call, then update sections
        await pcApi.update(pcModal.value.pcId, {
          ipAddress: pcModal.value.form.ipAddress,
          description: pcModal.value.form.description.trim(),
          active: pcList.value.find((p) => p.id === pcModal.value.pcId)?.active ?? true,
        });
        await pcApi.updateSections(pcModal.value.pcId, {
          sectionCodes: pcModal.value.form.sectionCodes,
        });
        emit("toast", "PC updated successfully.");
      }
      closePCModal();
      await loadPCs();
    } catch (err) {
      pcModal.value.error = err.response?.data?.message || "An error occurred.";
    } finally {
      pcModal.value.saving = false;
    }
  }

  // ── Toggle active ──────────────────────────────────────────────────────────
  async function togglePC(pc) {
    try {
      await pcApi.toggle(pc.id);
      pc.active = !pc.active;
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to update status.");
    }
  }

  // ── Delete ─────────────────────────────────────────────────────────────────
  function confirmDeletePC(pc) {
    deleteConfirm.value = { visible: true, pcId: pc.id, ipAddress: pc.ipAddress };
  }

  async function executeDeletePC() {
    try {
      await pcApi.delete(deleteConfirm.value.pcId);
      pcList.value = pcList.value.filter((p) => p.id !== deleteConfirm.value.pcId);
      emit("toast", "PC deleted successfully.");
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to delete PC.");
    } finally {
      deleteConfirm.value.visible = false;
    }
  }
</script>
