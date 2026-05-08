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
  padding: 24px;
  background:
    radial-gradient(circle at 10% 0%, rgba(15, 94, 168, 0.22), transparent 40%),
    radial-gradient(circle at 100% 10%, rgba(15, 94, 168, 0.16), transparent 35%),
    linear-gradient(180deg, #f4f8fd 0%, #e9f0fa 100%);
}
</style>
