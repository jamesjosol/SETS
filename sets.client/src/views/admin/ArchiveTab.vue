<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border)">
      <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
           style="background-color: var(--color-primary-soft)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">archive</span>
      </div>
      <div>
        <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Archive Settings</h2>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Set data retention period. Older batches will be archived and accessible only through the archive viewer.</p>
      </div>
    </div>

    <div class="px-8 py-6 space-y-6">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted)">Data Retention</p>
        <div class="space-y-4">
          <div class="flex items-center justify-between gap-6">
            <div>
              <p class="text-sm font-bold" style="color: var(--color-text)">Active Batch Retention Period</p>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Batches older than this will be moved to archive</p>
            </div>
            <div class="flex items-center gap-3">
              <input v-model.number="archive.retentionDays" type="number" min="7" max="365"
                     class="w-24 px-4 py-2.5 rounded-xl text-sm font-bold text-center outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
              <span class="text-sm font-bold" style="color: var(--color-text-muted)">days</span>
            </div>
          </div>

          <div class="flex items-center justify-between gap-6">
            <div>
              <p class="text-sm font-bold" style="color: var(--color-text)">Allow Archive Review</p>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Users with appropriate rights can browse archived batches</p>
            </div>
            <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                    :style="archive.allowReview ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                    @click="archive.allowReview = !archive.allowReview">
              <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                    :style="archive.allowReview ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
            </button>
          </div>
        </div>
      </div>

      <div class="px-5 py-4 rounded-xl flex items-start gap-3" style="background-color: #fffbeb; border: 1px solid #f59e0b">
        <span class="material-symbols-outlined text-base mt-0.5" style="color: #f59e0b">warning</span>
        <p class="text-xs font-medium" style="color: var(--color-text-muted)">
          Archived data is stored and can be retrieved, but will not appear in standard batch views or reports. Ensure the retention period aligns with your compliance requirements.
        </p>
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

// TODO: wire to API when Archive API is ready
const archive = ref({ retentionDays: 30, allowReview: true });

function save() {
  // TODO: call API
  emit("toast", "Archive settings saved.");
}
</script>
