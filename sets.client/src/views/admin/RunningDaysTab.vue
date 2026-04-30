<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">calendar_month</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Running Days</h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Configure which days of the week each SRD-tagged test is scheduled to run.</p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAddRunningDay">
        <span class="material-symbols-outlined text-sm">add</span>Add Setup
      </button>
    </div>

    <div class="px-8 py-6 space-y-4">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-sm"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="rdSearch"
               placeholder="Search by test code or name..."
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>

      <AppTable :rows="filteredRd" :columns="rdColumns" row-key="id" :page-size="10"
                :loading="rdLoading" loading-text="Loading setups..."
                empty-text="No running day setups yet." empty-icon="calendar_month">
        <template #cell-testName="{ row }">
          <div>
            <p class="font-bold text-sm" style="color: var(--color-text)">{{ row.testName }}</p>
            <p class="font-mono text-[10px] mt-0.5" style="color: var(--color-text-muted)">{{ row.testCode }}</p>
          </div>
        </template>
        <template #cell-runningDays="{ row }">
          <div class="flex flex-wrap gap-1.5">
            <span v-for="day in allDays" :key="day.value"
                  class="px-2.5 py-0.5 rounded-lg text-[10px] font-bold transition-all"
                  :style="(row.dayList ?? []).includes(day.value)
                    ? `background-color: ${day.softColor}; color: ${day.color};`
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); opacity: 0.4;'">
              {{ day.short }}
            </span>
          </div>
        </template>
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="openEditRunningDay(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: #ba1a1a"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="confirmDeleteRd(row)">
              <span class="material-symbols-outlined text-sm">delete</span>
            </button>
          </div>
        </template>
      </AppTable>
    </div>
  </div>

  <!-- Add/Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="rdModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeRdModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24);">
          <!-- Header -->
          <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center" style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">calendar_month</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ rdModal.mode === "add" ? "Add Running Day Setup" : "Edit Running Days" }}
              </h3>
            </div>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeRdModal">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Body -->
          <div class="px-6 py-5 space-y-5">
            <!-- Test search (add only) -->
            <div v-if="rdModal.mode === 'add'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Test *</label>
              <div class="relative">
                <input v-model="rdModal.testSearch"
                       ref="rdTestInputRef"
                       placeholder="Type 3+ characters to search HCLAB..."
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                       autocomplete="off"
                       @input="onRdTestInput"
                       @keydown.down.prevent="rdSelectNext"
                       @keydown.up.prevent="rdSelectPrev"
                       @keydown.enter.prevent="rdConfirmSelected"
                       @keydown.escape="rdModal.dropdownOpen = false"
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="onRdTestBlur" />
                <div class="absolute right-3 top-1/2 -translate-y-1/2">
                  <span v-if="rdModal.testLoading" class="material-symbols-outlined text-sm animate-spin" style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="rdModal.form.testCode" class="material-symbols-outlined text-sm" style="color: var(--color-primary)">check_circle</span>
                  <span v-else class="material-symbols-outlined text-sm" style="color: var(--color-text-muted)">biotech</span>
                </div>
              </div>

              <!-- Selected badge -->
              <div v-if="rdModal.form.testCode" class="mt-1.5 flex items-center gap-2 px-3 py-2 rounded-lg" style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">biotech</span>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-bold truncate" style="color: var(--color-primary)">{{ rdModal.form.testCode }}</p>
                  <p class="text-[10px] truncate" style="color: var(--color-text-muted)">{{ rdModal.form.testName }}</p>
                </div>
                <button class="material-symbols-outlined text-sm" style="color: var(--color-text-muted)" @click="clearRdTest">close</button>
              </div>

              <p v-else-if="rdModal.testSearch.length < 3 && !rdModal.form.testCode" class="mt-1 text-[10px]" style="color: var(--color-text-muted)">
                Search by test code or name (min. 3 characters).
              </p>

              <!-- Dropdown -->
              <Teleport to="body">
                <div v-if="rdModal.dropdownOpen && (rdModal.testResults.length || rdModal.testNoResults)"
                     class="fixed z-[60] rounded-xl overflow-hidden shadow-xl"
                     :style="`top: ${rdDropdownPos.top}px; left: ${rdDropdownPos.left}px; width: ${rdDropdownPos.width}px; background-color: var(--color-surface); border: 1.5px solid var(--color-border);`">
                  <div v-if="rdModal.testNoResults" class="px-4 py-3 text-xs text-center" style="color: var(--color-text-muted)">
                    No tests found for "{{ rdModal.testSearch }}".
                  </div>
                  <div v-else class="overflow-y-auto" style="max-height: 200px">
                    <div v-for="(t, idx) in rdModal.testResults" :key="t.testCode"
                         class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                         :style="idx === rdModal.activeIdx ? 'background-color: var(--color-primary-soft);' : 'border-bottom: 1px solid var(--color-surface-low);'"
                         @mouseenter="rdModal.activeIdx = idx"
                         @mousedown.prevent="selectRdTest(t)">
                      <div class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0" style="background-color: var(--color-surface-low)">
                        <span class="material-symbols-outlined text-sm" style="color: var(--color-text-muted)">biotech</span>
                      </div>
                      <div class="flex-1 min-w-0">
                        <div class="flex items-center gap-2">
                          <p class="text-xs font-bold truncate" style="color: var(--color-text)">{{ t.testCode }}</p>
                          <span v-if="t.hasSetup" class="px-1.5 py-0.5 rounded text-[9px] font-bold uppercase"
                                style="background-color: rgba(186,26,26,0.1); color: #ba1a1a">Already set</span>
                        </div>
                        <p class="text-[10px] truncate" style="color: var(--color-text-muted)">{{ t.testName }}</p>
                      </div>
                      <span class="text-[10px] font-mono font-bold flex-shrink-0" style="color: var(--color-text-muted)">{{ t.testGroup }}</span>
                    </div>
                  </div>
                </div>
              </Teleport>
            </div>

            <!-- Test read-only (edit) -->
            <div v-else>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Test</label>
              <div class="flex items-center gap-3 px-4 py-2.5 rounded-xl" style="background-color: var(--color-surface-low)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">biotech</span>
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text)">{{ rdModal.form.testName }}</p>
                  <p class="text-[10px] font-mono" style="color: var(--color-text-muted)">{{ rdModal.form.testCode }}</p>
                </div>
              </div>
            </div>

            <!-- Day picker -->
            <div>
              <div class="flex items-center justify-between mb-3">
                <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Running Days *</label>
                <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">{{ rdModal.form.days.length }} / 7</span>
              </div>
              <p v-if="rdModal.dayError" class="mb-2 text-xs font-bold" style="color: #ba1a1a">At least one running day must be selected.</p>
              <div class="grid grid-cols-7 gap-1.5">
                <button v-for="day in allDays" :key="day.value"
                        class="flex flex-col items-center py-3 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="rdModal.form.days.includes(day.value)
                          ? `background-color: ${day.color}; color: #ffffff;`
                          : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                        @click="toggleRdDay(day.value)">
                  {{ day.short }}
                </button>
              </div>
              <div v-if="rdModal.form.days.length" class="mt-3 px-3 py-2 rounded-xl text-xs"
                   style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
                <span class="font-bold" style="color: var(--color-text)">Will run on: </span>{{ rdModal.form.days.join(" · ") }}
              </div>
            </div>

            <p v-if="rdModal.error" class="text-xs font-bold" style="color: #ba1a1a">{{ rdModal.error }}</p>
          </div>

          <!-- Footer -->
          <div class="px-6 py-4 flex justify-end gap-3" style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="closeRdModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="rdModal.saving"
                    @click="saveRdModal">
              <span v-if="rdModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ rdModal.mode === "add" ? "Save Setup" : "Update" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Delete Confirm -->
  <ConfirmModal :isVisible="rdDeleteConfirm.visible"
                type="error" icon="delete" title="Remove Setup"
                :message="`Remove running day setup for '${rdDeleteConfirm.itemName}'? Tests tagged SRD with this code will no longer resolve a running date.`"
                confirmText="Yes, Remove" cancelText="No"
                @confirm="executeDeleteRd"
                @close="rdDeleteConfirm.visible = false" />
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from "vue";
import { testRunningDayApi } from "@/api/testRunningDayApi";
import AppTable from "@/components/common/AppTable.vue";
import ConfirmModal from "@/components/common/ConfirmModal.vue";

const emit = defineEmits(["toast"]);

// ── Constants ──────────────────────────────────────────────────────────────
const allDays = [
  { value: "Monday",    short: "Mon", color: "#1565c0", softColor: "rgba(21,101,192,0.1)" },
  { value: "Tuesday",   short: "Tue", color: "#6a1b9a", softColor: "rgba(106,27,154,0.1)" },
  { value: "Wednesday", short: "Wed", color: "#2e7d32", softColor: "rgba(46,125,50,0.1)" },
  { value: "Thursday",  short: "Thu", color: "#e65100", softColor: "rgba(230,81,0,0.1)" },
  { value: "Friday",    short: "Fri", color: "#c62828", softColor: "rgba(198,40,40,0.1)" },
  { value: "Saturday",  short: "Sat", color: "#558b2f", softColor: "rgba(85,139,47,0.1)" },
  { value: "Sunday",    short: "Sun", color: "#4e342e", softColor: "rgba(78,52,46,0.1)" },
];

const rdColumns = [
  { key: "testName",    label: "Test" },
  { key: "runningDays", label: "Running Days" },
];

// ── State ──────────────────────────────────────────────────────────────────
const rdList    = ref([]);
const rdLoading = ref(false);
const rdSearch  = ref("");

const rdTestInputRef = ref(null);
const rdDropdownPos  = ref({ top: 0, left: 0, width: 0 });
let rdSearchTimer    = null;

const rdModal = ref({
  visible: false, mode: "add", id: null,
  form: { testCode: "", testName: "", days: [] },
  testSearch: "", testResults: [], testLoading: false,
  dropdownOpen: false, testNoResults: false, activeIdx: -1,
  dayError: false, saving: false, error: "",
});

const rdDeleteConfirm = ref({ visible: false, itemId: null, itemName: "" });

const filteredRd = computed(() => {
  const q = rdSearch.value.toLowerCase();
  if (!q) return rdList.value;
  return rdList.value.filter((r) => r.testCode.toLowerCase().includes(q) || r.testName.toLowerCase().includes(q));
});

// ── Data loading ───────────────────────────────────────────────────────────
async function loadRunningDays() {
  rdLoading.value = true;
  try {
    const res = await testRunningDayApi.getAll();
    rdList.value = res.data;
  } catch { rdList.value = []; }
  finally { rdLoading.value = false; }
}

onMounted(() => loadRunningDays());

// ── Modal actions ──────────────────────────────────────────────────────────
function openAddRunningDay() {
  rdModal.value = {
    visible: true, mode: "add", id: null,
    form: { testCode: "", testName: "", days: [] },
    testSearch: "", testResults: [], testLoading: false,
    dropdownOpen: false, testNoResults: false, activeIdx: -1,
    dayError: false, saving: false, error: "",
  };
}

function openEditRunningDay(item) {
  rdModal.value = {
    visible: true, mode: "edit", id: item.id,
    form: { testCode: item.testCode, testName: item.testName, days: [...item.dayList] },
    testSearch: "", testResults: [], testLoading: false,
    dropdownOpen: false, testNoResults: false, activeIdx: -1,
    dayError: false, saving: false, error: "",
  };
}

function closeRdModal() {
  clearTimeout(rdSearchTimer);
  rdModal.value.visible = false;
}

function toggleRdDay(val) {
  const idx = rdModal.value.form.days.indexOf(val);
  if (idx === -1) rdModal.value.form.days.push(val);
  else rdModal.value.form.days.splice(idx, 1);
  rdModal.value.dayError = false;
}

function computeRdDropdownPos() {
  const el = rdTestInputRef.value;
  if (!el) return;
  const rect = el.getBoundingClientRect();
  rdDropdownPos.value = { top: rect.bottom + 6, left: rect.left, width: rect.width };
}

function onRdTestInput() {
  rdModal.value.form.testCode = "";
  rdModal.value.form.testName = "";
  rdModal.value.dropdownOpen = false;
  rdModal.value.testNoResults = false;
  rdModal.value.activeIdx = -1;
  clearTimeout(rdSearchTimer);
  const q = rdModal.value.testSearch.trim();
  if (q.length < 3) { rdModal.value.testResults = []; return; }
  rdModal.value.testLoading = true;
  rdSearchTimer = setTimeout(async () => {
    try {
      const res = await testRunningDayApi.search(q);
      rdModal.value.testResults = res.data ?? [];
      rdModal.value.testNoResults = rdModal.value.testResults.length === 0;
      rdModal.value.dropdownOpen = true;
      await nextTick();
      computeRdDropdownPos();
    } catch { rdModal.value.testResults = []; rdModal.value.testNoResults = true; }
    finally { rdModal.value.testLoading = false; }
  }, 350);
}

function onRdTestBlur(e) { setTimeout(() => { e.target.style.borderColor = "var(--color-border)"; }, 150); }

function selectRdTest(t) {
  rdModal.value.form.testCode = t.testCode;
  rdModal.value.form.testName = t.testName;
  rdModal.value.testSearch = `${t.testCode} — ${t.testName}`;
  rdModal.value.dropdownOpen = false;
  rdModal.value.activeIdx = -1;
}

function clearRdTest() {
  rdModal.value.form.testCode = "";
  rdModal.value.form.testName = "";
  rdModal.value.testSearch = "";
  rdModal.value.testResults = [];
  rdModal.value.dropdownOpen = false;
  nextTick(() => rdTestInputRef.value?.focus());
}

function rdSelectNext() {
  if (!rdModal.value.testResults.length) return;
  rdModal.value.activeIdx = (rdModal.value.activeIdx + 1) % rdModal.value.testResults.length;
}
function rdSelectPrev() {
  if (!rdModal.value.testResults.length) return;
  rdModal.value.activeIdx = (rdModal.value.activeIdx - 1 + rdModal.value.testResults.length) % rdModal.value.testResults.length;
}
function rdConfirmSelected() {
  const idx = rdModal.value.activeIdx;
  if (idx >= 0 && rdModal.value.testResults[idx]) selectRdTest(rdModal.value.testResults[idx]);
}

async function saveRdModal() {
  rdModal.value.error = "";
  rdModal.value.dayError = false;
  if (rdModal.value.mode === "add" && !rdModal.value.form.testCode) { rdModal.value.error = "Please select a test."; return; }
  if (!rdModal.value.form.days.length) { rdModal.value.dayError = true; return; }

  rdModal.value.saving = true;
  try {
    if (rdModal.value.mode === "add") {
      await testRunningDayApi.add({ testCode: rdModal.value.form.testCode, testName: rdModal.value.form.testName, days: rdModal.value.form.days });
      emit("toast", "Running day setup saved.");
    } else {
      await testRunningDayApi.update(rdModal.value.id, { days: rdModal.value.form.days });
      emit("toast", "Running days updated.");
    }
    closeRdModal();
    await loadRunningDays();
  } catch (err) {
    rdModal.value.error = err.response?.data?.message || "An error occurred.";
  } finally {
    rdModal.value.saving = false;
  }
}

function confirmDeleteRd(item) {
  rdDeleteConfirm.value = { visible: true, itemId: item.id, itemName: item.testName };
}

async function executeDeleteRd() {
  try {
    await testRunningDayApi.delete(rdDeleteConfirm.value.itemId);
    rdList.value = rdList.value.filter((r) => r.id !== rdDeleteConfirm.value.itemId);
    emit("toast", "Running day setup removed.");
  } catch (err) {
    emit("toast", err.response?.data?.message || "Failed to remove setup.");
  } finally {
    rdDeleteConfirm.value.visible = false;
  }
}
</script>
