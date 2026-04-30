<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">location_on</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">On-Site / Mission</h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Manage On-Site specimen scanning and allowed lab number prefixes.</p>
        </div>
      </div>
    </div>

    <div class="px-8 py-6 space-y-8">
      <!-- Global toggle -->
      <div class="flex items-center justify-between p-5 rounded-2xl"
           style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border);">
        <div>
          <p class="text-sm font-bold" style="color: var(--color-text)">Enable On-Site Scanning</p>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">When enabled, lab section users can scan On-Site / Mission specimens on the Assign RMT page.</p>
        </div>
        <button class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors flex-shrink-0 ml-6"
                :style="onSiteEnabled ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                :disabled="onSiteToggling"
                @click="toggleOnSite">
          <span class="inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform"
                :style="onSiteEnabled ? 'transform: translateX(23px);' : 'transform: translateX(4px);'" />
        </button>
      </div>

      <!-- Allowed Lab No. Prefixes -->
      <div>
        <div class="flex items-center justify-between mb-4">
          <div>
            <p class="text-sm font-extrabold" style="color: var(--color-text)">Allowed Lab No. Prefixes</p>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Characters 3–4 of the lab number must match an active prefix to be allowed for scanning.</p>
          </div>
        </div>

        <!-- Add prefix form -->
        <div class="flex items-start gap-3 mb-5 p-4 rounded-xl"
             style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border);">
          <div class="flex flex-col gap-1">
            <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Prefix (2 digits)</label>
            <input v-model="onSiteNewPrefix" maxlength="2" placeholder="e.g. 52"
                   class="px-3 py-2 rounded-lg text-sm font-mono font-bold outline-none w-24 transition-all"
                   style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);"
                   @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                   @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                   @keyup.enter="addLabNo" />
          </div>
          <div class="flex flex-col gap-1 flex-1">
            <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Description (optional)</label>
            <input v-model="onSiteNewDescription" placeholder="e.g. On-Site Mission A"
                   class="px-3 py-2 rounded-lg text-sm outline-none transition-all"
                   style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);"
                   @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                   @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                   @keyup.enter="addLabNo" />
          </div>
          <div class="flex flex-col gap-1 justify-end">
            <label class="text-[10px] font-bold uppercase tracking-widest opacity-0">Add</label>
            <button class="flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                    style="background: var(--color-primary-gradient); color: #fff"
                    :disabled="onSiteAdding"
                    @click="addLabNo">
              <span v-if="onSiteAdding" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">add</span>
              Add
            </button>
          </div>
        </div>
        <p v-if="onSiteAddError" class="mb-3 text-xs font-bold" style="color: #ba1a1a">{{ onSiteAddError }}</p>

        <!-- Loading -->
        <div v-if="onSiteLoading" class="flex items-center justify-center py-8 gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-primary)">progress_activity</span>
          <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Loading...</p>
        </div>

        <!-- Table -->
        <div v-else-if="onSiteLabNos.length" class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border)">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1px solid var(--color-border)">
                <th v-for="col in ['Prefix','Description','Status','Added By','']" :key="col"
                    class="text-left px-4 py-2.5 text-[11px] font-bold uppercase tracking-widest"
                    style="color: var(--color-text-muted)">{{ col }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="entry in onSiteLabNos" :key="entry.id" style="border-bottom: 1px solid var(--color-border)">
                <td class="px-4 py-3">
                  <span class="font-mono font-extrabold text-base" style="color: var(--color-primary)">{{ entry.prefix }}</span>
                </td>
                <td class="px-4 py-3" style="color: var(--color-text)">{{ entry.description || "—" }}</td>
                <td class="px-4 py-3">
                  <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                        :style="entry.active
                          ? 'background-color: rgba(5,150,105,0.1); color: var(--color-success, #059669);'
                          : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
                    {{ entry.active ? "Active" : "Inactive" }}
                  </span>
                </td>
                <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted)">{{ entry.createdBy }}</td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-2 justify-end">
                    <button class="text-xs font-bold px-3 py-1.5 rounded-lg transition-all"
                            :style="entry.active
                              ? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                              : 'background-color: rgba(5,150,105,0.1); color: #059669;'"
                            @click="toggleLabNo(entry)">
                      {{ entry.active ? "Disable" : "Enable" }}
                    </button>
                    <button class="text-xs font-bold px-3 py-1.5 rounded-lg transition-all"
                            style="background-color: rgba(186,26,26,0.08); color: #ba1a1a"
                            @click="confirmDeleteLabNo(entry)">
                      Remove
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="py-8 text-center text-sm" style="color: var(--color-text-muted)">No prefixes configured yet.</div>
      </div>
    </div>
  </div>

  <!-- Delete confirm -->
  <ConfirmModal :isVisible="onSiteDeleteConfirm.visible"
                type="error" icon="delete" title="Remove Prefix"
                :message="`Remove prefix '${onSiteDeleteConfirm.prefix}'?`"
                confirmText="Yes, Remove" cancelText="No"
                @confirm="executeDeleteLabNo"
                @close="onSiteDeleteConfirm.visible = false" />
</template>

<script setup>
import { ref, onMounted } from "vue";
import { runnerApi } from "@/api/runnerApi";
import ConfirmModal from "@/components/common/ConfirmModal.vue";

const emit = defineEmits(["toast"]);

const onSiteEnabled     = ref(false);
const onSiteLabNos      = ref([]);
const onSiteLoading     = ref(false);
const onSiteToggling    = ref(false);
const onSiteNewPrefix   = ref("");
const onSiteNewDescription = ref("");
const onSiteAddError    = ref("");
const onSiteAdding      = ref(false);
const onSiteDeleteConfirm = ref({ visible: false, id: null, prefix: "" });

async function loadOnSiteSettings() {
  onSiteLoading.value = true;
  try {
    const data = await runnerApi.getOnSiteSettings();
    onSiteEnabled.value = data.isEnabled;
    onSiteLabNos.value = data.allowedLabNos ?? [];
  } catch { onSiteLabNos.value = []; }
  finally { onSiteLoading.value = false; }
}

onMounted(() => loadOnSiteSettings());

async function toggleOnSite() {
  onSiteToggling.value = true;
  try {
    await runnerApi.toggleOnSite(!onSiteEnabled.value);
    onSiteEnabled.value = !onSiteEnabled.value;
    emit("toast", `On-Site scanning ${onSiteEnabled.value ? "enabled" : "disabled"}.`);
  } catch (err) {
    emit("toast", err.response?.data?.message || "Failed to update On-Site setting.");
  } finally { onSiteToggling.value = false; }
}

async function addLabNo() {
  onSiteAddError.value = "";
  const prefix = onSiteNewPrefix.value.trim();
  if (!prefix) { onSiteAddError.value = "Prefix is required."; return; }
  if (!/^\d{2}$/.test(prefix)) { onSiteAddError.value = "Prefix must be exactly 2 digits."; return; }
  onSiteAdding.value = true;
  try {
    await runnerApi.addAllowedLabNo({ prefix, description: onSiteNewDescription.value.trim() || null });
    onSiteNewPrefix.value = "";
    onSiteNewDescription.value = "";
    await loadOnSiteSettings();
    emit("toast", `Prefix '${prefix}' added.`);
  } catch (err) {
    onSiteAddError.value = err.response?.data?.message || "Failed to add prefix.";
  } finally { onSiteAdding.value = false; }
}

async function toggleLabNo(entry) {
  try {
    await runnerApi.toggleAllowedLabNo(entry.id);
    entry.active = !entry.active;
  } catch (err) { emit("toast", err.response?.data?.message || "Failed to update status."); }
}

function confirmDeleteLabNo(entry) {
  onSiteDeleteConfirm.value = { visible: true, id: entry.id, prefix: entry.prefix };
}

async function executeDeleteLabNo() {
  try {
    await runnerApi.deleteAllowedLabNo(onSiteDeleteConfirm.value.id);
    onSiteLabNos.value = onSiteLabNos.value.filter((l) => l.id !== onSiteDeleteConfirm.value.id);
    emit("toast", `Prefix '${onSiteDeleteConfirm.value.prefix}' removed.`);
  } catch (err) { emit("toast", err.response?.data?.message || "Failed to delete."); }
  finally { onSiteDeleteConfirm.value.visible = false; }
}
</script>
