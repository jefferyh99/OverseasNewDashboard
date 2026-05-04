<template>
  <main class="auth-shell">
    <router-view />
  </main>
</template>

<style scoped>
.auth-shell {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: linear-gradient(180deg, #f5f7fb 0%, #e9eef7 100%);
}
</style>
