# Final Low-Fidelity Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create one final low-fidelity HTML prototype that consolidates the approved homepage and three detail pages for stakeholder confirmation.

**Architecture:** Add one standalone wireframe-style HTML file under mockups. Reuse the confirmed information architecture from the approved homepage and detail-page prototypes, but reduce the visual fidelity into grayscale panels, placeholder blocks, and lightweight annotations so the user can validate page structure instead of polished styling.

**Tech Stack:** HTML, CSS

---

### Task 1: Create Final Low-Fidelity Shell

**Files:**
- Create: `mockups/warehouse-monitor-final-lowfi.html`
- Reference: `mockups/warehouse-monitor-prototypes.html`
- Reference: `mockups/warehouse-monitor-detail-pages.html`

- [ ] **Step 1: Create the standalone HTML file with the final page title**

```html
<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>海外仓运营监控 MVP 最终低保真原型</title>
</head>
<body>
  <div class="page"></div>
</body>
</html>
```

- [ ] **Step 2: Add grayscale wireframe tokens instead of the warmer semi-polished visual language**

```css
:root {
  --bg: #f3f3f1;
  --panel: #ffffff;
  --line: #222222;
  --muted: #666666;
  --soft: #ececea;
  --alert: #d9d9d9;
}
```

- [ ] **Step 3: Verify the new file has no HTML diagnostics**

Run: VS Code diagnostics on `mockups/warehouse-monitor-final-lowfi.html`
Expected: No HTML diagnostics errors

### Task 2: Add Homepage Wireframe

**Files:**
- Modify: `mockups/warehouse-monitor-final-lowfi.html`
- Reference: `mockups/warehouse-monitor-prototypes.html`

- [ ] **Step 1: Add the approved homepage as the first board in low-fidelity form**

```html
<article class="wireframe">
  <div class="wireframe-head">
    <h2>01 首页 / 轻总览 + 异常清单优先</h2>
  </div>
</article>
```

- [ ] **Step 2: Keep these homepage sections in order: top status, today overview, anomaly lists, 7-day forecast**

```html
<section class="block-group">
  <div class="block">顶部状态区</div>
  <div class="block">今日总览区</div>
  <div class="block">三类异常清单区</div>
  <div class="block">近 7 天到仓预估区</div>
</section>
```

- [ ] **Step 3: Use lightweight mock data labels rather than rich cards**

Run: Visual inspection in browser
Expected: Homepage reads as a wireframe, not a polished dashboard

### Task 3: Add Three Detail Page Wireframes

**Files:**
- Modify: `mockups/warehouse-monitor-final-lowfi.html`
- Reference: `mockups/warehouse-monitor-detail-pages.html`
- Reference: `docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-design.md`

- [ ] **Step 1: Add the outbound detail page section**

```html
<article class="wireframe">
  <div class="wireframe-head">
    <h2>02 子页面 / 出库异常明细页</h2>
  </div>
</article>
```

- [ ] **Step 2: Add the inbound mismatch detail page section**

```html
<article class="wireframe">
  <div class="wireframe-head">
    <h2>03 子页面 / 到仓不齐明细页</h2>
  </div>
</article>
```

- [ ] **Step 3: Add the shelving delay detail page section**

```html
<article class="wireframe">
  <div class="wireframe-head">
    <h2>04 子页面 / 上架异常明细页</h2>
  </div>
</article>
```

- [ ] **Step 4: For each detail page, keep only summary area, filter area, table area, and a right-side note area**

```html
<div class="layout-two-col">
  <div class="table-zone">表格区</div>
  <div class="note-zone">说明区</div>
</div>
```

- [ ] **Step 5: Verify each page keeps the approved spec fields and avoids closure actions**

Run: Compare against `## 11. 明细页设计`
Expected: All fields and filters are present, with no task assignment or processed-state controls

### Task 4: Render Verification

**Files:**
- Verify: `mockups/warehouse-monitor-final-lowfi.html`

- [ ] **Step 1: Open the final low-fidelity prototype in the browser**

Run: Open `mockups/warehouse-monitor-final-lowfi.html` in the VS Code browser
Expected: Four stacked wireframes render clearly

- [ ] **Step 2: Confirm all page titles are visible**

Run: Read browser snapshot
Expected: Visible headings for homepage and three detail pages

- [ ] **Step 3: Confirm no diagnostics errors remain**

Run: VS Code diagnostics on `mockups/warehouse-monitor-final-lowfi.html`
Expected: No errors