<template>
  <div class="min-h-screen flex items-center justify-center p-6 relative overflow-hidden"
       style="background-color: #f8f9fa; color: #191c1d">
    <!-- Background Image (right half) -->
    <div class="absolute top-0 right-0 w-1/2 h-full hidden lg:block">
      <div class="w-full h-full relative">
        <img src="/loginbg.png"
             alt="Laboratory Background"
             class="w-full h-full object-cover opacity-90" />
        <div class="absolute inset-0"
             style="background: linear-gradient(to right, #f8f9fa, rgba(248, 249, 250, 0.4), transparent);"></div>
      </div>
    </div>

    <!-- Floating Glows -->
    <div class="absolute -bottom-24 -left-24 w-96 h-96 rounded-full pointer-events-none"
         style="background: rgba(70, 21, 153, 0.05); filter: blur(120px)"></div>
    <div class="absolute top-1/4 left-1/4 w-48 h-48 rounded-full pointer-events-none"
         style="background: rgba(205, 230, 244, 0.2); filter: blur(100px)"></div>

    <!-- Login Container -->
    <main class="w-full max-w-md relative z-10">
      <!-- Brand Header -->
      <div class="mb-12 text-left lg:pl-4">
        <h1 class="text-4xl font-bold tracking-tighter mb-2" style="color: #461599">SETS</h1>
        <p class="text-sm font-bold uppercase tracking-widest" style="color: #4a626d">
          Specimen Endorsement &amp; Tracking System
        </p>
      </div>

      <!-- Login Card -->
      <div class="p-10 rounded-2xl shadow-2xl border"
           style="
          background: rgba(248, 249, 250, 0.8);
          backdrop-filter: blur(12px);
          -webkit-backdrop-filter: blur(12px);
          border-color: rgba(203, 195, 213, 0.15);
        ">
        <header class="mb-8">
          <h2 class="text-2xl font-bold" style="color: #191c1d">Login</h2>
          <p class="text-sm mt-1" style="color: #7b7484">
            Please enter your HCLAB credentials to proceed.
          </p>
        </header>

        <form class="space-y-6" @submit.prevent="handleLogin">

          <!-- Branch / Section Display -->
          <div class="p-4 rounded-2xl flex items-center gap-4 transition-all"
               :style="branchFieldStyle"
               :class="{ 'cursor-pointer hover:opacity-90': pcInfo.isRegistered && pcInfo.sections.length > 1 }"
               @click="openSectionPicker">
            <!-- Loading state -->
            <template v-if="pcLoading">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: #e7e8e9;">
                <span class="material-symbols-outlined text-lg animate-spin" style="color: #7b7484">progress_activity</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #7b7484">Detecting...</p>
                <p class="font-bold" style="color: #7b7484">Please wait</p>
              </div>
            </template>

            <!-- Not registered -->
            <template v-else-if="!pcInfo.isRegistered">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: rgba(186,26,26,0.1);">
                <span class="material-symbols-outlined text-lg" style="color: #ba1a1a">warning</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #ba1a1a">Unregistered Device</p>
                <p class="font-bold text-sm" style="color: #ba1a1a">IP {{ pcInfo.ipAddress }} is not registered.</p>
              </div>
            </template>

            <!-- Registered -->
            <template v-else>
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: #cde6f4; color: #4a626d">
                <span class="material-symbols-outlined text-lg">location_on</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #506873">Current Section</p>
                <p class="font-bold" style="color: #191c1d">{{ selectedSection?.name ?? 'Select Section' }}</p>
              </div>
              <div class="ml-auto opacity-40">
                <!-- Show chevron if multiple sections, lock if single -->
                <span class="material-symbols-outlined text-sm" style="color: #191c1d">
                  {{ pcInfo.sections.length > 1 ? 'expand_more' : 'lock' }}
                </span>
              </div>
            </template>
          </div>

          <!-- User ID -->
          <div>
            <label class="block text-[11px] font-bold uppercase tracking-widest mb-2 ml-1"
                   style="color: #4a626d"
                   for="user-id">
              User ID
            </label>
            <div class="relative flex items-center">
              <input id="user-id"
                     v-model="form.userID"
                     type="text"
                     placeholder="Enter User ID"
                     :disabled="!pcInfo.isRegistered || pcLoading"
                     class="w-full border-none outline-none rounded-2xl py-4 px-6 transition-colors"
                     style="background-color: #e7e8e9; color: #191c1d"
                     @focus="(e) => (e.target.style.backgroundColor = '#e1e3e4')"
                     @blur="(e) => (e.target.style.backgroundColor = '#e7e8e9')" />
            </div>
          </div>

          <!-- Password -->
          <div>
            <label class="block text-[11px] font-bold uppercase tracking-widest mb-2 ml-1"
                   style="color: #4a626d"
                   for="password">
              Password
            </label>
            <div class="relative flex items-center">
              <input id="password"
                     v-model="form.password"
                     :type="showPassword ? 'text' : 'password'"
                     placeholder="••••••••"
                     :disabled="!pcInfo.isRegistered || pcLoading"
                     class="w-full border-none outline-none rounded-2xl py-4 pl-6 pr-12 transition-colors"
                     style="background-color: #e7e8e9; color: #191c1d"
                     @focus="(e) => (e.target.style.backgroundColor = '#e1e3e4')"
                     @blur="(e) => (e.target.style.backgroundColor = '#e7e8e9')" />
              <button type="button"
                      class="absolute right-4 transition-colors"
                      style="color: #4a626d"
                      :disabled="!pcInfo.isRegistered || pcLoading"
                      @mouseenter="(e) => (e.currentTarget.style.color = '#461599')"
                      @mouseleave="(e) => (e.currentTarget.style.color = '#4a626d')"
                      @click="showPassword = !showPassword">
                <span class="material-symbols-outlined text-lg">
                  {{ showPassword ? "visibility_off" : "visibility" }}
                </span>
              </button>
            </div>
          </div>

          <!-- Submit Button -->
          <button type="submit"
                  :disabled="!pcInfo.isRegistered || isLoading || pcLoading"
                  class="w-full py-4 px-6 font-bold rounded-2xl shadow-lg flex items-center justify-center gap-2 mt-4 transition-all active:scale-[0.98]"
                  :style="pcInfo.isRegistered ? 'background: linear-gradient(135deg, #461599, #5e35b1); color: #ffffff; cursor: pointer;' : 'background: #e7e8e9; color: #7b7484; cursor: not-allowed;'">
            <span v-if="!isLoading" class="flex items-center gap-2 uppercase tracking-widest">
              Login
              <span class="material-symbols-outlined text-lg">arrow_forward</span>
            </span>
            <span v-else class="uppercase tracking-widest">Authenticating...</span>
          </button>

        </form>

        <footer class="mt-10 pt-8 text-center"
                style="border-top: 1px solid rgba(203, 195, 213, 0.1)">
          <p class="text-[11px] uppercase tracking-widest leading-relaxed" style="color: rgba(73, 68, 83, 0.6)">
            Authorized Personnel Only<br />
            SETS • v1.0.0
          </p>
        </footer>
      </div>

      <div class="mt-8 flex items-center gap-3 justify-center" style="opacity: 0.5">
        <div class="h-px w-10" style="background-color: #cbc3d5"></div>
        <span class="text-[10px] font-bold uppercase tracking-tighter" style="color: #4a626d">HI-Precision Diagnostics Cebu</span>
        <div class="h-px w-10" style="background-color: #cbc3d5"></div>
      </div>
    </main>
  </div>

  <!-- Section Picker Modal -->
  <div v-if="sectionPickerVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="sectionPickerVisible = false"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-8 flex flex-col gap-4 animate-modal"
         style="background-color: #ffffff;">
      <h3 class="text-lg font-bold" style="color: #191c1d">Select Section</h3>
      <p class="text-sm" style="color: #7b7484">Choose the section for this session.</p>

      <div class="flex flex-col gap-2 mt-2">
        <button v-for="section in pcInfo.sections"
                :key="section.code"
                type="button"
                class="w-full p-4 rounded-xl text-left flex items-center gap-3 transition-all"
                :style="selectedSection?.code === section.code
            ? 'background-color: rgba(70,21,153,0.1); border: 1.5px solid #461599;'
            : 'background-color: #f3f4f5; border: 1.5px solid transparent;'"
                @click="selectSection(section)">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold"
               style="background-color: #cde6f4; color: #461599;">
            {{ section.code.charAt(0) }}
          </div>
          <div>
            <p class="font-bold text-sm" style="color: #191c1d">{{ section.name }}</p>
            <p class="text-[11px]" style="color: #7b7484">{{ section.code }}</p>
          </div>
          <div v-if="selectedSection?.code === section.code" class="ml-auto">
            <span class="material-symbols-outlined text-sm" style="color: #461599">check_circle</span>
          </div>
        </button>
      </div>

      <button type="button"
              class="mt-2 w-full py-3 px-6 rounded-xl font-bold uppercase tracking-widest text-sm transition-all active:scale-[0.98]"
              style="background-color: #461599; color: #ffffff;"
              @click="sectionPickerVisible = false">
        Confirm
      </button>
    </div>
  </div>

  <!-- Alert Modal -->
  <AlertModal :isVisible="alert.isVisible"
              :type="alert.type"
              :title="alert.title"
              :message="alert.message"
              @close="alert.isVisible = false"
              @confirm="alert.isVisible = false" />
</template>

<script setup>
  import { ref, computed, onMounted } from "vue";
  import { useRouter } from "vue-router";
  import NProgress from "nprogress";
  import { authApi } from "@/api/authApi";
  import { useAuthStore } from "@/stores/authStore";
  import AlertModal from "@/components/common/AlertModal.vue";
  import { useTheme } from '@/composables/useTheme'

  const { applyTheme } = useTheme()
  const router = useRouter();
  const authStore = useAuthStore();

  const showPassword = ref(false);
  const isLoading = ref(false);
  const pcLoading = ref(true);
  const sectionPickerVisible = ref(false);

  const pcInfo = ref({
    isRegistered: false,
    ipAddress: "",
    sections: [],
  });

  const selectedSection = ref(null);

  const alert = ref({
    isVisible: false,
    type: "error",
    title: "",
    message: "",
  });

  const form = ref({
    userID: "",
    password: "",
  });

  // Branch field style based on PC status
  const branchFieldStyle = computed(() => {
    if (pcLoading.value) return "background-color: #f3f4f5;";
    if (!pcInfo.value.isRegistered) return "background-color: rgba(186,26,26,0.08); border: 1.5px solid rgba(186,26,26,0.3);";
    return "background-color: #f3f4f5;";
  });

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message };
  }

  function openSectionPicker() {
    if (pcInfo.value.isRegistered && pcInfo.value.sections.length > 1) {
      sectionPickerVisible.value = true;
    }
  }

  function selectSection(section) {
    selectedSection.value = section;
  }

  // Load PC info on mount
  onMounted(async () => {
    try {
      const response = await authApi.getPCInfo();
      pcInfo.value = response.data;

      if (pcInfo.value.isRegistered && pcInfo.value.sections.length > 0) {
        selectedSection.value = pcInfo.value.sections[0];
      }
    } catch {
      pcInfo.value = { isRegistered: false, ipAddress: "Unknown", sections: [] };
    } finally {
      pcLoading.value = false;
    }
  });

  async function handleLogin() {
    if (!pcInfo.value.isRegistered) return;

    const selectedBranch = computed(() =>
      pcInfo.value.sections.find(s => s.code === selectedSection.value?.code)?.branchCode ?? "WES"
    )

    if (!form.value.userID || !form.value.password) {
      showAlert("warning", "Missing Fields", "Please enter your User ID and Password.");
      return;
    }

    if (!selectedSection.value) {
      showAlert("warning", "No Section", "Please select a section to proceed.");
      return;
    }

    isLoading.value = true;
    NProgress.start();

    try {
      const response = await authApi.login({
        userID: form.value.userID.toUpperCase(),
        password: form.value.password,
        branch: selectedSection.value.branchCode, 
        sectionCode: selectedSection.value.code,
      });

      const data = response.data

      if (!data.success) {
        showAlert("error", "Login Failed", data.message || "Invalid username or password.")
        NProgress.done()
        return
      }

      authStore.setUser(
        {
          userID: data.data.userID,
          userName: data.data.userName,
          isAdmin: data.data.isAdmin,
          theme: data.data.theme 
        },
        data.data.branchCode,
        {
          code: data.data.sectionCode,
          name: data.data.sectionName,
          roleID: data.data.roleID,
          branchCode: data.data.branchCode,
          category: data.data.sectionCategory
        }
      )

      console.log(authStore.section)
      applyTheme(data.data.theme) 
      router.push("/dashboard")

    } catch (err) {
      if (err.response) {
        const msg = err.response?.data?.message;
        showAlert("error", "Login Failed", msg || "Invalid username or password.");
      } else {
        showAlert("error", "Connection Error", "Unable to connect to the server. Please try again.");
      }
      NProgress.done();
    } finally {
      isLoading.value = false;
    }
  }
</script>

<style>
  body {
    font-family: "Manrope", sans-serif;
  }

  #nprogress .bar {
    background: #461599 !important;
    height: 3px;
  }

  #nprogress .peg {
    box-shadow: 0 0 10px #461599, 0 0 5px #461599 !important;
  }

  #nprogress .spinner-icon {
    border-top-color: #461599 !important;
    border-left-color: #461599 !important;
  }

  .animate-modal {
    animation: modalIn 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
  }

  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.85) translateY(20px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
