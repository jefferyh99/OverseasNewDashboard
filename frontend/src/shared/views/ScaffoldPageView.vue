<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <section class="page-card">
    <h2>{{ route.meta.title }}</h2>
    <p>当前页面已经接入模板壳层，后续在此替换为真实业务模块。</p>
  </section>
</template>

<style scoped>
.page-card {
  padding: 24px;
  border-radius: 16px;
  background: #ffffff;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
}
</style>
