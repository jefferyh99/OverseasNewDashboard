<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({ username: '', password: '' })
const errorMsg = ref('')
const loading = ref(false)

async function submit() {
  if (!form.username || !form.password) {
    errorMsg.value = '请输入用户名和密码'
    return
  }
  loading.value = true
  errorMsg.value = ''
  try {
    await authStore.signIn(form.username, form.password)
    router.push('/dashboard')
  } catch {
    errorMsg.value = '用户名或密码错误，请重试'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <section class="login-card">
    <div class="login-logo">
      <span class="logo-icon">📊</span>
    </div>
    <h1>Ops Monitor</h1>
    <p class="subtitle">运营监控管理平台</p>

    <el-form @submit.prevent="submit" label-position="top">
      <el-form-item label="用户名">
        <el-input
          v-model="form.username"
          placeholder="请输入用户名"
          size="large"
          clearable
          @keyup.enter="submit"
        />
      </el-form-item>
      <el-form-item label="密码">
        <el-input
          v-model="form.password"
          type="password"
          placeholder="请输入密码"
          size="large"
          show-password
          @keyup.enter="submit"
        />
      </el-form-item>

      <el-alert
        v-if="errorMsg"
        :title="errorMsg"
        type="error"
        show-icon
        :closable="false"
        style="margin-bottom: 12px"
      />

      <el-button
        type="primary"
        size="large"
        :loading="loading"
        style="width: 100%"
        @click="submit"
      >
        登 录
      </el-button>
    </el-form>

    <p class="hint">测试账号：admin / admin</p>
  </section>
</template>

<style scoped>
.login-card {
  width: min(430px, calc(100vw - 36px));
  padding: 44px 36px 32px;
  border-radius: 20px;
  background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%);
  border: 1px solid #d8e5f3;
  box-shadow:
    0 20px 52px rgba(15, 46, 80, 0.12),
    0 1px 0 rgba(255, 255, 255, 0.72) inset;
}

.login-logo {
  text-align: center;
  margin-bottom: 10px;
}

.logo-icon {
  display: inline-flex;
  width: 56px;
  height: 56px;
  border-radius: 14px;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  background: linear-gradient(140deg, #2388e1 0%, #10599f 100%);
  box-shadow: 0 10px 24px rgba(16, 89, 159, 0.35);
}

h1 {
  text-align: center;
  font-size: 24px;
  font-weight: 700;
  color: #173452;
  margin: 0 0 4px;
  letter-spacing: 0.01em;
}

.subtitle {
  text-align: center;
  color: #69809b;
  font-size: 13px;
  margin: 0 0 26px;
}

.hint {
  text-align: center;
  color: #8496aa;
  font-size: 12px;
  margin: 16px 0 0;
}

:deep(.el-form-item__label) {
  color: #2d4865;
  font-weight: 600;
}
</style>

