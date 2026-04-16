<template>
  <nav class="fixed top-0 right-0 z-50 flex items-center justify-between h-16 px-6"
       style="left: 256px; background: var(--color-bg); backdrop-filter: blur(12px); -webkit-backdrop-filter: blur(12px); border-bottom: 1px solid var(--color-border);">
    <!-- Search -->
    <div class="relative hidden md:block">
      <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm" style="color: var(--color-text-muted);">search</span>
      <input v-model="searchQuery"
             type="text"
             placeholder="Search specimens or patient IDs..."
             class="border-none outline-none rounded-full py-2 pl-10 pr-4 text-sm w-80 transition-colors"
             style="background-color: var(--color-surface-high); color: var(--color-text);"
             @focus="(e) => (e.target.style.backgroundColor = 'var(--color-surface-highest)')"
             @blur="(e) => (e.target.style.backgroundColor = 'var(--color-surface-high)')" />
    </div>

    <!-- Right Side -->
    <div class="flex items-center gap-4 ml-auto">

      <!-- Notifications -->
      <button class="relative p-2 rounded-full transition-colors"
              style="color: var(--color-text-muted);"
              @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-high)')"
              @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')">
        <span class="material-symbols-outlined">notifications</span>
        <span v-if="notifCount > 0"
              class="absolute top-1 right-1 w-4 h-4 rounded-full text-[9px] font-bold flex items-center justify-center text-white"
              style="background-color: var(--color-error);">{{ notifCount }}</span>
      </button>

      <!-- Divider -->
      <div class="h-8 w-px" style="background-color: var(--color-border-strong);"></div>

      <!-- User Info -->
      <div class="relative flex items-center gap-3">
        <div class="text-right">
          <p class="text-sm font-bold" style="color: var(--color-text);">{{ authStore.userName }}</p>
          <p class="text-[10px] uppercase tracking-widest font-bold" style="color: var(--color-text-muted);">
            {{ authStore.isAdmin ? "Administrator" : roleLabel }}
          </p>
        </div>

        <!-- Avatar Button -->
        <button class="w-10 h-10 rounded-full flex items-center justify-center font-bold text-sm transition-all active:scale-95"
                style="background: var(--color-primary-gradient); color: #ffffff;"
                @click="toggleDropdown">
          {{ userInitials }}
        </button>

        <!-- Dropdown -->
        <div v-if="dropdownOpen"
             class="absolute right-0 top-12 w-64 rounded-2xl shadow-xl border z-50"
             style="background-color: var(--color-surface); border-color: var(--color-border);">
          <!-- User Info Header -->
          <div class="px-5 py-4" style="border-bottom: 1px solid var(--color-border);">
            <p class="font-bold text-sm" style="color: var(--color-text);">{{ authStore.userName }}</p>
            <p class="text-[10px] uppercase tracking-widest font-bold mt-0.5" style="color: var(--color-text-muted);">
              {{ authStore.sectionName }}
            </p>
            <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
              {{ authStore.branchCode }} · {{ authStore.sectionCode }}
            </p>
          </div>

          <!-- Menu Items -->
          <div class="p-2">

            <!-- My Profile -->
            <button class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all text-left"
                    style="color: var(--color-text-muted);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')">
              <span class="material-symbols-outlined text-lg">manage_accounts</span>
              My Profile
            </button>

            <!-- Theme Switcher -->
            <div class="px-4 py-3" style="border-top: 1px solid var(--color-border); border-bottom: 1px solid var(--color-border);">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Theme</p>
              <div class="flex items-center gap-2">
                <button v-for="t in themes"
                        :key="t.value"
                        class="flex-1 py-2 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="authStore.theme === t.value
                    ? 'background-color: var(--color-primary); color: #ffffff;'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                        @click="handleThemeChange(t.value)">
                  {{ t.label }}
                </button>
              </div>
            </div>

            <!-- Logout -->
            <button class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all text-left"
                    style="color: var(--color-error);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-error-soft)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="handleLogout">
              <span class="material-symbols-outlined text-lg">logout</span>
              Log Out
            </button>

          </div>
        </div>
      </div>

      <!-- Backdrop -->
      <div v-if="dropdownOpen" class="fixed inset-0 z-40" @click="dropdownOpen = false"></div>
    </div>
  </nav>
</template>

<script setup>
  import { ref, computed } from "vue";
  import NProgress from "nprogress";
  import { useRouter } from "vue-router";
  import { useAuthStore } from "@/stores/authStore";
  import { authApi } from "@/api/authApi";
  import { useTheme } from "@/composables/useTheme";
  import { themeApi } from "@/api/themeApi";

  const { applyTheme } = useTheme();
  const authStore = useAuthStore();
  const router = useRouter();
  const searchQuery = ref("");
  const notifCount = ref(3);
  const dropdownOpen = ref(false);

  const themes = [
    { value: 0, label: "☀️ Light" },
    { value: 1, label: "🌙 Dark" },
    { value: 2, label: "🌫️ Dim" },
  ];

  const userInitials = computed(() => {
    const name = authStore.userName ?? "";
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });

  const roleLabel = computed(() => {
    switch (authStore.roleID) {
      case 1: return "Regular";
      case 2: return "Team Lead";
      default: return "Staff";
    }
  });

  function toggleDropdown() {
    dropdownOpen.value = !dropdownOpen.value;
  }

  async function handleThemeChange(themeValue) {
    authStore.setTheme(themeValue);
    applyTheme(themeValue);
    try {
      await themeApi.update(themeValue);
    } catch {
      // silently fail
    }
  }

  async function handleLogout() {
    NProgress.start();
    try {
      await authApi.logout();
    } catch {
    } finally {
      NProgress.done();
      authStore.logout();
      router.push("/");
    }
  }
</script>
