<template>
  <div class="prototype-page">
    <div class="toolbar">
      <div class="title">企微通知原型</div>
      <a
        class="open-link"
        href="/prototypes/wecom-notification-5.4.html"
        target="_blank"
        rel="noopener noreferrer"
      >
        新窗口打开
      </a>
    </div>
    <iframe
      class="prototype-frame"
      title="企微通知原型"
      src="/prototypes/wecom-notification-5.4.html"
    />
  </div>
</template>

<style scoped>
.prototype-page {
  display: flex;
  flex-direction: column;
  gap: 12px;
  height: calc(100vh - 176px);
  min-height: 620px;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.title {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

.open-link {
  color: #2563eb;
  text-decoration: none;
  font-size: 13px;
}

.open-link:hover {
  text-decoration: underline;
}

.prototype-frame {
  width: 100%;
  height: 100%;
  border: 1px solid #dbe2ea;
  border-radius: 10px;
  background: #fff;
}
</style>
