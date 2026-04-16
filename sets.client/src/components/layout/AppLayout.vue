<template>
  <div class="min-h-screen" style="background-color: var(--color-bg);">
    <AppSidebar />
    <AppTopbar />
    <main class="pt-16" style="margin-left: 256px;">
      <div class="p-8">
        <slot />
      </div>
    </main>
  </div>
</template>

<script setup>
  import AppSidebar from "./AppSidebar.vue";
  import AppTopbar from "./AppTopbar.vue";
</script>

<style>
  body {
    font-family: "Manrope", sans-serif;
    background-color: var(--color-bg);
    color: var(--color-text);
  }
</style>
