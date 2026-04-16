<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="handleBackdropClick"></div>

    <!-- Modal -->
    <div
      class="relative w-full max-w-sm rounded-2xl shadow-2xl p-8 flex flex-col items-center text-center gap-4 animate-modal"
      style="background-color: var(--modal-bg, #ffffff)"
    >
      <!-- Animated Icon -->
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-2"
        :style="iconBgStyle"
      >
        <!-- Error Icon -->
        <svg
          v-if="type === 'error'"
          class="w-10 h-10 animate-icon"
          :style="iconStyle"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <circle cx="12" cy="12" r="10" />
          <line x1="15" y1="9" x2="9" y2="15" />
          <line x1="9" y1="9" x2="15" y2="15" />
        </svg>
        <!-- Warning Icon -->
        <svg
          v-else-if="type === 'warning'"
          class="w-10 h-10 animate-icon"
          :style="iconStyle"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path
            d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"
          />
          <line x1="12" y1="9" x2="12" y2="13" />
          <line x1="12" y1="17" x2="12.01" y2="17" />
        </svg>
        <!-- Success Icon -->
        <svg
          v-else-if="type === 'success'"
          class="w-10 h-10 animate-icon"
          :style="iconStyle"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <polyline points="22 4 12 14.01 9 11.01" />
        </svg>
        <!-- Info Icon -->
        <svg
          v-else-if="type === 'info'"
          class="w-10 h-10 animate-icon"
          :style="iconStyle"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </svg>
      </div>

      <!-- Title -->
      <h3 class="text-xl font-bold" style="color: var(--modal-title, #191c1d)">{{ title }}</h3>

      <!-- Message -->
      <p class="text-sm leading-relaxed" style="color: var(--modal-text, #7b7484)">{{ message }}</p>

      <!-- Confirm Button -->
      <button
        class="mt-2 w-full py-3 px-6 rounded-xl font-bold uppercase tracking-widest text-sm transition-all active:scale-[0.98]"
        :style="buttonStyle"
        @click="handleConfirm"
      >
        {{ confirmText }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: false,
  },
  type: {
    type: String,
    default: "info", // 'error' | 'warning' | 'success' | 'info'
  },
  title: {
    type: String,
    default: "",
  },
  message: {
    type: String,
    default: "",
  },
  confirmText: {
    type: String,
    default: "OK",
  },
  closeOnBackdrop: {
    type: Boolean,
    default: true,
  },
});

const emit = defineEmits(["confirm", "close"]);

const typeConfig = {
  error: {
    iconBg: "rgba(186,26,26,0.1)",
    iconColor: "#ba1a1a",
    buttonBg: "#ba1a1a",
    buttonColor: "#ffffff",
  },
  warning: {
    iconBg: "rgba(255,179,113,0.2)",
    iconColor: "#7b4200",
    buttonBg: "#7b4200",
    buttonColor: "#ffffff",
  },
  success: {
    iconBg: "rgba(70,21,153,0.1)",
    iconColor: "#461599",
    buttonBg: "#461599",
    buttonColor: "#ffffff",
  },
  info: {
    iconBg: "rgba(74,98,109,0.1)",
    iconColor: "#4a626d",
    buttonBg: "#4a626d",
    buttonColor: "#ffffff",
  },
};

const config = computed(() => typeConfig[props.type] || typeConfig.info);

const iconBgStyle = computed(() => ({
  backgroundColor: config.value.iconBg,
}));

const iconStyle = computed(() => ({
  color: config.value.iconColor,
}));

const buttonStyle = computed(() => ({
  backgroundColor: config.value.buttonBg,
  color: config.value.buttonColor,
}));

function handleConfirm() {
  emit("confirm");
  emit("close");
}

function handleBackdropClick() {
  if (props.closeOnBackdrop) {
    emit("close");
  }
}
</script>

<style scoped>
.animate-modal {
  animation: modalIn 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.animate-icon {
  animation: iconPop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) 0.15s both;
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

@keyframes iconPop {
  from {
    opacity: 0;
    transform: scale(0.5);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
