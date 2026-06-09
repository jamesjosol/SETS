<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text)">
        Global Settings
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted)">
        <span style="color: var(--color-primary); font-weight: 700">ADMINISTRATOR</span>
        · System Configuration
      </p>
    </div>

    <!-- Settings Layout: Sidebar + Content -->
    <div class="flex gap-6">
      <!-- Left Nav Sidebar -->
      <aside class="w-56 flex-shrink-0">
        <div class="rounded-2xl overflow-hidden sticky top-6"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="p-2 space-y-0.5">
            <button v-for="tab in settingsTabs"
                    :key="tab.key"
                    class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all text-left"
                    :style="
                activeTab === tab.key
                  ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid var(--color-primary); padding-left: calc(1rem - 3px);'
                  : 'color: var(--color-text-muted); border-left: 3px solid transparent; padding-left: calc(1rem - 3px);'
              "
                    @click="activeTab = tab.key">
              <span class="material-symbols-outlined text-base">{{ tab.icon }}</span>
              {{ tab.label }}
            </button>
          </div>
        </div>
      </aside>

      <!-- Right Content Panel -->
      <div class="flex-1 min-w-0">
        <PcRegistrationTab v-if="activeTab === 'pc'" @toast="showToast" />
        <UserManagementTab v-if="activeTab === 'users'" @toast="showToast" />
        <SectionTab v-if="activeTab === 'sections'" @toast="showToast" />
        <RunningDaysTab v-if="activeTab === 'runningDays'" @toast="showToast" />
        <TatSetUpTab v-if="activeTab === 'tat'" @toast="showToast" />
        <OutboundTatTab v-if="activeTab === 'outboundTat'" @toast="showToast" />
        <OnSiteTab v-if="activeTab === 'onsite'" @toast="showToast" />
        <ProcessingTab v-if="activeTab === 'processing'" @toast="showToast" />
        <BranchTab v-if="activeTab === 'branch'" @toast="showToast" />
        <ContingencyTab v-if="activeTab === 'contingency'" @toast="showToast" />
        <AnnouncementTab v-if="activeTab === 'announcement'" @toast="showToast" />
        <EndorsementTab v-if="activeTab === 'endorsement'" @toast="showToast" />
        <ArchiveTab v-if="activeTab === 'archive'" @toast="showToast" />
        <OnlineUsersTab v-if="activeTab === 'onlineUsers'" @toast="showToast" />
        <ChangelogTab v-if="activeTab === 'changelog' && authStore.isDeveloper" @toast="showToast" />
      </div>
    </div>

    <!-- Toast Notification -->
    <Transition name="toast">
      <div v-if="toast.visible"
           class="fixed bottom-6 right-6 flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl z-50"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">check_circle</span>
        <p class="text-sm font-bold" style="color: var(--color-text)">{{ toast.message }}</p>
      </div>
    </Transition>
  </AppLayout>
</template>

<script setup>
  import { ref, computed } from "vue";
  import { useAuthStore } from '@/stores/authStore'
  import AppLayout from "@/components/layout/AppLayout.vue";
  import PcRegistrationTab from "./PcRegistrationTab.vue";
  import UserManagementTab from "./UserManagementTab.vue";
  import SectionTab from "./SectionTab.vue";
  import RunningDaysTab from "./RunningDaysTab.vue";
  import TatSetUpTab from "./TatSetUpTab.vue";
  import OnSiteTab from "./OnSiteTab.vue";
  import ProcessingTab from "./ProcessingTab.vue";
  import BranchTab from "./BranchTab.vue";
  import ContingencyTab from "./ContingencyTab.vue";
  import AnnouncementTab from '@/views/admin/AnnouncementTab.vue';
  import EndorsementTab from "./EndorsementTab.vue";
  import ArchiveTab from "./ArchiveTab.vue";
  import OnlineUsersTab from './OnlineUsersTab.vue'
  import OutboundTatTab from './OutboundTatTab.vue'
  import ChangelogTab from './ChangelogTab.vue'

  const activeTab = ref("pc");
  const authStore = useAuthStore()

  const settingsTabs = computed(() => {
    const tabs = [
      { key: "pc", label: "PC Registration", icon: "computer" },
      { key: "users", label: "Users", icon: "manage_accounts" },
      { key: "sections", label: "Section", icon: "apartment" },
      { key: "runningDays", label: "Running Days", icon: "calendar_month" },
      { key: "tat", label: "TAT Set-Up", icon: "timer" },
      { key: 'outboundTat', label: 'Outbound TAT', icon: 'alt_route' },
      { key: "onsite", label: "On-Site", icon: "location_on" },
      { key: "processing", label: "Processing", icon: "tune" },
      { key: "branch", label: "Branch", icon: "location_city" },
      { key: "contingency", label: "Contingency", icon: "offline_bolt" },
      { key: 'announcement', label: 'Announcements', icon: 'campaign' },
      { key: "endorsement", label: "Endorsement", icon: "swap_horiz" },
      { key: "archive", label: "Archive", icon: "archive" },
      { key: 'onlineUsers', label: 'Online Users', icon: 'groups' },
    ]

    // Only append Changelog tab for developer users
    if (authStore.isDeveloper) {
      tabs.push({ key: 'changelog', label: 'Changelog', icon: 'new_releases' })
    }

    return tabs
  })

  const toast = ref({ visible: false, message: "" });
  let toastTimer = null;

  function showToast(msg) {
    clearTimeout(toastTimer);
    toast.value = { visible: true, message: msg };
    toastTimer = setTimeout(() => {
      toast.value.visible = false;
    }, 2500);
  }
</script>

<style scoped>
  .toast-enter-active,
  .toast-leave-active {
    transition: opacity 0.25s ease, transform 0.25s ease;
  }

  .toast-enter-from,
  .toast-leave-to {
    opacity: 0;
    transform: translateY(8px);
  }
</style>
