# Warehouse Monitor Detail Prototypes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a standalone HTML wireframe that shows the three approved detail subpages for the overseas warehouse monitoring MVP.

**Architecture:** Create one new prototype file under mockups that reuses the established visual language from the approved homepage prototype while splitting each business detail page into its own section. Because this is throwaway prototype HTML, verification uses browser rendering and diagnostics instead of automated tests.

**Tech Stack:** HTML, CSS

---

### Task 1: Scaffold Detail Prototype File

**Files:**
- Create: `mockups/warehouse-monitor-detail-pages.html`
- Reference: `mockups/warehouse-monitor-prototypes.html`
- Reference: `docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-design.md`

- [ ] **Step 1: Create the standalone HTML shell with shared visual tokens**

```html
<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>海外仓运营监控 MVP 子页面原型</title>
</head>
<body>
  <div class="page"></div>
</body>
</html>
```

- [ ] **Step 2: Copy and adapt the existing visual language from the homepage prototype**

```css
:root {
  --bg: #f4f1e8;
  --panel: #fffdf8;
  --line: #292522;
  --muted: #6a645d;
  --accent: #d06f3b;
  --warn: #b4432d;
  --soft: #ece5d8;
  --grid: #d9d0c1;
}
```

- [ ] **Step 3: Verify the file is syntactically valid**

Run: VS Code diagnostics on `mockups/warehouse-monitor-detail-pages.html`
Expected: No HTML diagnostics errors

### Task 2: Add The Three Detail Page Wireframes

**Files:**
- Modify: `mockups/warehouse-monitor-detail-pages.html`
- Reference: `docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-design.md`

- [ ] **Step 1: Add the outbound order detail page section**

```html
<section class="detail-board">
  <div class="board-head">
    <div>
      <span class="label">子页面 01</span>
      <h2>出库异常明细页</h2>
    </div>
  </div>
</section>
```

- [ ] **Step 2: Add the inbound mismatch detail page section**

```html
<section class="detail-board">
  <div class="board-head">
    <div>
      <span class="label">子页面 02</span>
      <h2>到仓不齐明细页</h2>
    </div>
  </div>
</section>
```

- [ ] **Step 3: Add the shelving delay detail page section**

```html
<section class="detail-board">
  <div class="board-head">
    <div>
      <span class="label">子页面 03</span>
      <h2>上架异常明细页</h2>
    </div>
  </div>
</section>
```

- [ ] **Step 4: Fill each section with summary cards, filter chips, and a table using realistic mock data from the approved homepage prototype**

```html
<div class="filters">
  <span class="chip active">已超时</span>
  <span class="chip">即将超时</span>
  <span class="chip">Amazon</span>
  <span class="chip">近 24 小时</span>
</div>
```

- [ ] **Step 5: Verify the page content matches the approved spec fields for all three detail pages**

Run: Compare rendered sections with `## 11. 明细页设计` in the spec
Expected: All required fields and filters are present, with no processing-closure controls added

### Task 3: Render Validation

**Files:**
- Verify: `mockups/warehouse-monitor-detail-pages.html`

- [ ] **Step 1: Open the prototype in the browser**

Run: Open `mockups/warehouse-monitor-detail-pages.html` in the VS Code browser
Expected: Three stacked detail-page wireframes render with the same design language as the homepage prototype

- [ ] **Step 2: Confirm each section shows distinct business data**

Run: Read the browser snapshot
Expected: Visible titles for `出库异常明细页`, `到仓不齐明细页`, and `上架异常明细页`

- [ ] **Step 3: Keep the homepage prototype untouched**

Run: Review changed files
Expected: No unrelated edits to `mockups/warehouse-monitor-prototypes.html`