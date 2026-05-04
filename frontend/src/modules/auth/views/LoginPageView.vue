<script setup lang="ts">
import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  username: 'admin',
  password: 'admin',
})

function submit() {
  if (form.username === 'admin' && form.password === 'admin') {
    authStore.signIn()
    router.push('/dashboard')
  }
}
</script>

<template>
  <section class="login-card">
    <h1>Ops Monitor</h1>
    <p>Monitoring-system template login shell</p>
    <input v-model="form.username" placeholder="username" />
    <input v-model="form.password" type="password" placeholder="password" />
    <button @click="submit">登录</button>
  </section>
</template>

<style scoped>
.login-card {
  width: min(420px, calc(100vw - 32px));
  padding: 32px;
  border-radius: 18px;
  background: #ffffff;
  box-shadow: 0 18px 48px rgba(15, 23, 42, 0.12);
  display: grid;
  gap: 12px;
}

input,
button {
  min-height: 44px;
  border-radius: 10px;
  border: 1px solid #cbd5e1;
  padding: 0 12px;
}

button {
  background: #16324f;
  color: #ffffff;
  cursor: pointer;
}
</style>
