<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border)">
      <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
           style="background-color: var(--color-primary-soft)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">swap_horiz</span>
      </div>
      <div>
        <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Endorsement Set-Up</h2>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Configure default and additional endorsed-to sites per branch and section.</p>
      </div>
    </div>

    <div class="px-8 py-6 space-y-8">
      <!-- Default Endorsed-To Sites -->
      <div>
        <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted)">Default Endorsed-To Site</p>
        <p class="text-xs mb-4" style="color: var(--color-text-muted)">The default site shown when opening the endorsement form. One per branch/section combination.</p>
        <div class="space-y-3">
          <div v-for="(config, idx) in endorsement.defaults" :key="idx"
               class="grid grid-cols-3 gap-3 p-4 rounded-xl"
               style="background-color: var(--color-surface-low)">
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Branch</label>
              <input v-model="config.branchCode"
                     class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="Branch Code" />
            </div>
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Section</label>
              <input v-model="config.sectionCode"
                     class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="Section Code" />
            </div>
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Default Endorsed-To</label>
              <input v-model="config.defaultSite"
                     class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="Site Code" />
            </div>
          </div>
        </div>
        <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                @click="endorsement.defaults.push({ branchCode: '', sectionCode: '', defaultSite: '' })">
          <span class="material-symbols-outlined text-sm">add</span>Add Row
        </button>
      </div>

      <!-- Additional Endorsed-To Sites -->
      <div>
        <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted)">Additional Endorsed-To Sites</p>
        <p class="text-xs mb-4" style="color: var(--color-text-muted)">Sites available in the dropdown. All other sites are hidden unless configured here.</p>
        <div class="space-y-3">
          <div v-for="(config, idx) in endorsement.additional" :key="idx"
               class="flex gap-3 items-end p-4 rounded-xl"
               style="background-color: var(--color-surface-low)">
            <div class="flex-1">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Branch / Section</label>
              <input v-model="config.branchSection"
                     class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="e.g. WES / HEM" />
            </div>
            <div class="flex-[2]">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Additional Sites (comma-separated)</label>
              <input v-model="config.sites"
                     class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                     style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="e.g. MAIN, NAGA, TABUNOK" />
            </div>
            <button class="w-9 h-9 rounded-xl flex items-center justify-center transition-all active:scale-95 flex-shrink-0"
                    style="background-color: var(--color-error-soft); color: var(--color-error)"
                    @click="endorsement.additional.splice(idx, 1)">
              <span class="material-symbols-outlined text-sm">close</span>
            </button>
          </div>
        </div>
        <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                @click="endorsement.additional.push({ branchSection: '', sites: '' })">
          <span class="material-symbols-outlined text-sm">add</span>Add Row
        </button>
      </div>

      <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border)">
        <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                style="background: var(--color-primary-gradient); color: #ffffff"
                @click="save">
          <span class="material-symbols-outlined text-sm">save</span>Save Changes
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";

const emit = defineEmits(["toast"]);

// TODO: wire to API when Endorsement Set-Up API is ready
const endorsement = ref({
  defaults: [{ branchCode: "WES", sectionCode: "HEM", defaultSite: "MAIN" }],
  additional: [{ branchSection: "WES / HEM", sites: "NAGA, TABUNOK" }],
});

function save() {
  // TODO: call API
  emit("toast", "Endorsement settings saved.");
}
</script>
