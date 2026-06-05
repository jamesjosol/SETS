<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border)">
      <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
           style="background-color: var(--color-primary-soft)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">timer</span>
      </div>
      <div>
        <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">TAT Set-Up</h2>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Configure turnaround time thresholds for endorsing sections and processing.</p>
      </div>
    </div>

    <div class="px-8 py-6 space-y-8">
      <!-- Endorsement TAT -->
      <div>
        <div class="flex items-center gap-2 mb-4">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">arrow_upward</span>
          <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Batch Endorsement — Per Endorsing Section</p>
        </div>

        <div v-if="tatLoading" class="flex items-center justify-center py-10 gap-3">
          <span class="material-symbols-outlined animate-spin text-xl" style="color: var(--color-primary)">progress_activity</span>
          <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Loading...</p>
        </div>

        <div v-else-if="!tatList.length" class="flex flex-col items-center justify-center py-10 gap-2">
          <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted)">timer_off</span>
          <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">No active endorsing sections found.</p>
        </div>

        <template v-else>
          <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border)">
            <div class="grid grid-cols-[1fr_160px_160px_180px] px-5 py-3 text-[10px] font-bold uppercase tracking-widest"
                 style="background-color: var(--color-surface-low); color: var(--color-text-muted); border-bottom: 1px solid var(--color-border);">
              <span>Section</span><span class="text-center">TAT Threshold</span><span class="text-center">Appeal Window</span><span></span>
            </div>
            <div v-for="(row, idx) in tatList" :key="row.sectionCode"
                 class="grid grid-cols-[1fr_160px_160px_180px] items-center px-5 py-4 gap-4"
                 :style="idx < tatList.length - 1 ? 'border-bottom: 1px solid var(--color-border);' : ''">
              <div>
                <p class="text-sm font-bold" style="color: var(--color-text)">{{ row.name }}</p>
                <p class="text-[10px] mt-0.5 font-mono" style="color: var(--color-text-muted)">{{ row.sectionCode }}</p>
              </div>
              <div class="flex items-center justify-center gap-1.5">
                <input v-model.number="row.hours" type="number" min="0" max="23"
                       class="w-14 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">hr</span>
                <input v-model.number="row.minutes" type="number" min="0" max="59"
                       class="w-14 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">min</span>
              </div>
              <div class="flex items-center justify-center">
                <div class="flex rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border)">
                  <button class="px-3 py-1.5 text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="row.appealWindow === 'Before'
                            ? 'background-color: var(--color-primary); color: #ffffff;'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                          @click="row.appealWindow = 'Before'">
                    Before
                  </button>
                  <button class="px-3 py-1.5 text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="row.appealWindow === 'After'
                            ? 'background-color: var(--color-primary); color: #ffffff;'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                          @click="row.appealWindow = 'After'">
                    After
                  </button>
                </div>
              </div>
              <div class="space-y-1">
                <p class="text-[10px]" style="color: var(--color-text-muted)">
                  <template v-if="row.appealWindow === 'Before'">
                    Endorser can appeal
                    <span class="font-bold" style="color: var(--color-text)">anytime</span> while the timer is still running.
                  </template>
                  <template v-else>
                    Endorser can only appeal
                    <span class="font-bold" style="color: var(--color-text)">after</span> the TAT has already been breached.
                  </template>
                </p>
                <p v-if="formatSavedLog(row.updatedBy, row.updated)"
                   class="text-[10px] flex items-center gap-1"
                   style="color: var(--color-text-muted)">
                  <span class="material-symbols-outlined" style="font-size: 11px;">history</span>
                  {{ formatSavedLog(row.updatedBy, row.updated) }}
                </p>
                <p v-else class="text-[10px] italic" style="color: var(--color-text-muted)">Never saved</p>
              </div>
            </div>
          </div>
          <div class="flex justify-end pt-4">
            <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    :class="tatSaving ? 'opacity-60 pointer-events-none' : ''"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    @click="saveTat">
              <span class="material-symbols-outlined text-sm">{{ tatSaving ? "progress_activity" : "save" }}</span>
              {{ tatSaving ? "Saving..." : "Save Endorsement TAT" }}
            </button>
          </div>
        </template>
      </div>

      <!-- Processing TAT -->
      <div style="border-top: 1px solid var(--color-border); padding-top: 2rem">
        <div class="flex items-center gap-2 mb-4">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">arrow_downward</span>
          <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Batch Completion — Processing</p>
        </div>
        <p class="text-xs mb-5" style="color: var(--color-text-muted)">
          Time allowed from first specimen received to batch fully completed. Batches exceeding this threshold will be flagged
          <span class="font-bold" style="color: #ef4444">red</span>; batches within TAT will be
          <span class="font-bold" style="color: #3b82f6">blue</span>.
        </p>

        <div v-if="procTatLoading" class="flex items-center justify-center py-10 gap-3">
          <span class="material-symbols-outlined animate-spin text-xl" style="color: var(--color-primary)">progress_activity</span>
          <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Loading...</p>
        </div>

        <template v-else>
          <div class="flex items-center gap-4 p-5 rounded-xl"
               style="background-color: var(--color-surface-low); border: 1px solid var(--color-border);">
            <span class="material-symbols-outlined text-2xl flex-shrink-0" style="color: var(--color-primary)">hourglass_bottom</span>
            <div class="flex-1">
              <p class="text-sm font-bold mb-0.5" style="color: var(--color-text)">Completion Threshold</p>
              <p class="text-[10px]" style="color: var(--color-text-muted)">Applied branch-wide to all incoming batches.</p>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <input v-model.number="procTat.hours" type="number" min="0" max="23"
                     class="w-16 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
              <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">hr</span>
              <input v-model.number="procTat.minutes" type="number" min="0" max="59"
                     class="w-16 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
              <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">min</span>
            </div>
          </div>
          <div class="flex justify-end pt-4">
            <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    :class="procTatSaving ? 'opacity-60 pointer-events-none' : ''"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    @click="saveProcTat">
              <span class="material-symbols-outlined text-sm">{{ procTatSaving ? "progress_activity" : "save" }}</span>
              {{ procTatSaving ? "Saving..." : "Save Processing TAT" }}
            </button>
          </div>
        </template>
      </div>
      <p v-if="formatSavedLog(procTat.updatedBy, procTat.updated)"
         class="text-[10px] flex items-center gap-1 pt-2"
         style="color: var(--color-text-muted)">
        <span class="material-symbols-outlined" style="font-size: 11px;">history</span>
        Last saved by {{ formatSavedLog(procTat.updatedBy, procTat.updated) }}
      </p>
      <p v-else class="text-[10px] italic pt-2" style="color: var(--color-text-muted)">Never saved</p>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from "vue";
  import { sectionApi } from "@/api/sectionApi";
  import { tatApi } from "@/api/tatApi";
  import { useAuthStore } from "@/stores/authStore";

  const emit = defineEmits(["toast"]);
  const authStore = useAuthStore();

  const tatList = ref([]);
  const tatLoading = ref(false);
  const tatSaving = ref(false);
  const procTat = ref({ hours: 0, minutes: 30, updatedBy: null, updated: null });
  const procTatLoading = ref(false);
  const procTatSaving = ref(false);

  async function loadTat() {
    tatLoading.value = true;
    try {
      const [sectionsRes, tatRes] = await Promise.all([sectionApi.getAll(), tatApi.getAll()]);
      const endorsers = sectionsRes.data.filter(
        (s) => s.category === "1" && s.active && s.branchCode === authStore.branchCode
      );
      tatList.value = endorsers.map((s) => {
        const existing = tatRes.find((t) => t.sectionCode === s.code);
        return {
          sectionCode: s.code,
          name: s.name,
          hours: existing?.hours ?? 0,
          minutes: existing?.minutes ?? 30,
          appealWindow: existing?.appealWindow ?? "Before",
          updatedBy: existing?.updatedBy ?? null,
          updated: existing?.updated ?? null,
        };
      });
    } catch { tatList.value = []; }
    finally { tatLoading.value = false; }
  }

  async function loadProcTat() {
    procTatLoading.value = true;
    try {
      const data = await tatApi.getProcessing();
      procTat.value = {
        hours: data.data.hours ?? 0,
        minutes: data.data.minutes ?? 30,
        updatedBy: data.data.updatedBy ?? null,
        updated: data.data.updated ?? null,
      };
    } catch { procTat.value = { hours: 0, minutes: 30, updatedBy: null, updated: null }; }
    finally { procTatLoading.value = false; }
  }

  onMounted(() => { loadTat(); loadProcTat(); });

  function formatSavedLog(updatedBy, updated) {
    if (!updatedBy || !updated) return null;
    const d = new Date(updated);
    const date = d.toLocaleDateString("en-PH", { month: "short", day: "numeric", year: "numeric" });
    const time = d.toLocaleTimeString("en-PH", { hour: "2-digit", minute: "2-digit" });
    return `${updatedBy} · ${date}, ${time}`;
  }

  async function saveTat() {
    tatSaving.value = true;
    try {
      const payload = tatList.value.map((t) => ({
        sectionCode: t.sectionCode,
        hours: t.hours ?? 0,
        minutes: t.minutes ?? 0,
        appealWindow: t.appealWindow ?? "Before",
      }));
      await tatApi.upsert(payload);
      emit("toast", "Endorsement TAT settings saved.");
      await loadTat(); // reload to get fresh updatedBy/updated
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to save endorsement TAT settings.");
    } finally { tatSaving.value = false; }
  }

  async function saveProcTat() {
    procTatSaving.value = true;
    try {
      await tatApi.upsertProcessing({ hours: procTat.value.hours ?? 0, minutes: procTat.value.minutes ?? 0 });
      emit("toast", "Processing TAT saved.");
      await loadProcTat(); // reload to get fresh updatedBy/updated
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to save processing TAT.");
    } finally { procTatSaving.value = false; }
  }
</script>
