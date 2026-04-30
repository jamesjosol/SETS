<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border)">
      <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
           style="background-color: var(--color-primary-soft)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">tune</span>
      </div>
      <div>
        <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Processing Options</h2>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Configure optional fields shown during specimen receiving.</p>
      </div>
    </div>

    <div class="px-8 py-6">
      <div v-if="procOptionsLoading" class="flex items-center justify-center py-10 gap-3">
        <span class="material-symbols-outlined animate-spin text-xl" style="color: var(--color-primary)">progress_activity</span>
        <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Loading...</p>
      </div>

      <template v-else>
        <div>
          <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted)">Receive by Specimen — Optional Fields</p>
          <p class="text-xs mb-5" style="color: var(--color-text-muted)">
            Toggle which fields are visible during specimen receiving. Disabling a field hides it from the interface but does not delete any existing data.
          </p>

          <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border)">
            <!-- Temperature -->
            <div class="flex items-center justify-between px-5 py-4" style="border-bottom: 1px solid var(--color-border)">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">thermostat</span>
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text)">Temperature Field</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Show temperature input during receiving</p>
                </div>
              </div>
              <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                      :style="procOptions.showTemperature ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                      @click="procOptions.showTemperature = !procOptions.showTemperature">
                <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                      :style="procOptions.showTemperature ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
              </button>
            </div>

            <!-- Temp Remarks -->
            <div class="flex items-center justify-between px-5 py-4" style="border-bottom: 1px solid var(--color-border)">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">comment</span>
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text)">Temperature Remarks</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Show temperature remarks field. If existing data is present, the field will remain visible regardless of this setting.</p>
                </div>
              </div>
              <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                      :style="procOptions.showTempRemarks ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                      @click="procOptions.showTempRemarks = !procOptions.showTempRemarks">
                <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                      :style="procOptions.showTempRemarks ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
              </button>
            </div>

            <!-- Bag No -->
            <div class="flex items-center justify-between px-5 py-4">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">shopping_bag</span>
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text)">Bag Number</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Show bag number field during receiving</p>
                </div>
              </div>
              <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                      :style="procOptions.showBagNo ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                      @click="procOptions.showBagNo = !procOptions.showBagNo">
                <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                      :style="procOptions.showBagNo ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
              </button>
            </div>
          </div>
        </div>

        <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border); margin-top: 1.5rem;">
          <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                  :class="procOptionsSaving ? 'opacity-60 pointer-events-none' : ''"
                  style="background: var(--color-primary-gradient); color: #ffffff;"
                  @click="saveProcessingOptions">
            <span class="material-symbols-outlined text-sm">{{ procOptionsSaving ? 'progress_activity' : 'save' }}</span>
            {{ procOptionsSaving ? 'Saving...' : 'Save Changes' }}
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { processingOptionsApi } from "@/api/processingOptionsApi";

const emit = defineEmits(["toast"]);

const procOptions       = ref({ showTemperature: true, showTempRemarks: true, showBagNo: true });
const procOptionsLoading = ref(false);
const procOptionsSaving  = ref(false);

async function loadProcessingOptions() {
  procOptionsLoading.value = true;
  try {
    const data = await processingOptionsApi.get();
    procOptions.value = { showTemperature: data.showTemperature, showTempRemarks: data.showTempRemarks, showBagNo: data.showBagNo };
  } catch {
    procOptions.value = { showTemperature: true, showTempRemarks: true, showBagNo: true };
  } finally { procOptionsLoading.value = false; }
}

onMounted(() => loadProcessingOptions());

async function saveProcessingOptions() {
  procOptionsSaving.value = true;
  try {
    await processingOptionsApi.upsert({ showTemperature: procOptions.value.showTemperature, showTempRemarks: procOptions.value.showTempRemarks, showBagNo: procOptions.value.showBagNo });
    emit("toast", "Processing options saved.");
  } catch (err) {
    emit("toast", err.response?.data?.message || "Failed to save processing options.");
  } finally { procOptionsSaving.value = false; }
}
</script>
