# 待办任务清单

> 基于 PRD `docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md`  
> 最后更新：2026-05-04  
> 当前分支：main（最新 commit：aebb045）

---

## 整体进度说明

| 模块 | 状态 |
|---|---|
| 前后端脚手架 + 布局 + 路由 + 认证流程 | ✅ 已完成 |
| 首页 Dashboard（含 5 条预览 / 延迟 Banner / 7 天预测） | ✅ 已完成（Mock 数据） |
| 出库异常明细页 + API | ✅ 已完成（Mock 数据） |
| 到仓不齐明细页 + API | ✅ 已完成（Mock 数据） |
| 上架异常明细页 + API | ✅ 已完成（Mock 数据） |
| 预警提前量配置页 + API | ✅ 已完成（内存存储） |
| 外部消息提醒系统（企微 Webhook + SMTP 邮件） | ✅ 已完成 |
| 后端集成测试 | ✅ 8/8 通过 |
| **真实业务数据接入（监控计算逻辑）** | ❌ 未完成 |
| **数据异常列表** | ❌ 未完成 |
| **提醒接收人配置持久化** | ❌ 未完成 |

---

## MVP 必须完成的任务（PRD §17 验收标准）

### T-01 真实监控计算逻辑（后端）

> PRD §10.2 / §10.3 / §10.4  
> **优先级：最高**。当前三类异常 API 全部返回 Mock 数据，无法基于真实业务数据判断风险。

需要做的事：

1. **设计业务数据库表结构**（或对接现有业务系统接口）：
   - 出库订单表：订单号、下单时间、承诺时效、实际出库时间、当前状态
   - 入库单表：入库单号、预计到仓日期、计划箱数、实际到仓箱数、首箱到仓时间
   - 箱子表：箱号、所属入库单、到仓时间、SKU 数量、未上架 SKU 数
2. **实现出库异常计算查询**（替换 `AnomaliesController.GetOutbound` 的 Mock 数据）：
   - 即将超时：未出库 AND 距截止时间 ≤ 预警提前量（默认 4h）
   - 已超时：未出库 AND 当前时间 > 截止时间
3. **实现到仓不齐计算查询**（替换 `AnomaliesController.GetInbound` 的 Mock 数据）：
   - 监控对象：首箱已到但未完全到齐的入库单
   - 截止时间 = 首箱到仓时间 + 72h
4. **实现上架异常计算查询**（替换 `AnomaliesController.GetShelving` 的 Mock 数据）：
   - 监控对象：已到仓但存在未上架 SKU 的箱子
   - 截止时间 = 箱子到仓时间 + 72h
5. **更新 Dashboard 总览卡数据**：
   - `OutboundRiskCount`、`InboundRiskCount`、`ShelvingRiskCount` 改为从真实查询计算
   - `TodayOverdueCount` 改为真实计数
   - `LastSyncTime` 改为业务数据库最近一条记录时间
6. **替换 `MockAnomalyDataSource`**：
   - 实现 `DbAnomalyDataSource`（查询真实数据库），注册为正式数据源
   - 提醒系统将自动使用真实风险对象

相关文件：
- `backend/Api/Controllers/AnomaliesController.cs`（全部替换为真实查询）
- `backend/Api/Controllers/DashboardController.cs`（总览卡数字 + LastSyncTime）
- `backend/Application/Notifications/MockAnomalyDataSource.cs`（替换为 DbAnomalyDataSource）

---

### T-02 数据异常列表（后端 + 前端）

> PRD §13  
> **优先级：高**。关键字段缺失的对象不能混入正常异常清单，需要单独展示。

需要做的事：

1. **后端**：新增 `GET /api/anomalies/data-errors` 接口：
   - 查询出关键字段缺失的记录：
     - 出库订单缺 `OrderTime`
     - 入库单缺 `PlannedCartonCount`
     - 箱子缺 `ArrivalTime` 但已进入上架流程
   - 返回：对象类型、对象 ID、缺失字段名、最近发现时间
2. **后端**：监控计算逻辑中过滤掉关键字段缺失对象，防止其出现在正常异常清单
3. **前端**：在"系统设置"或诊断区新增"数据异常"页面：
   - 路由：`/diagnostics/data-errors`
   - 展示表格：对象类型、对象 ID、缺失字段、发现时间
   - 在导航侧边栏中添加入口

相关文件：
- `backend/Api/Controllers/`（新增 DiagnosticsController 或在现有控制器增加端点）
- `frontend/src/modules/`（新增 diagnostics 模块）
- `frontend/src/router/routes.ts`（新增路由）

---

### T-03 提醒接收配置持久化（后端）

> PRD §12  
> **优先级：中**。当前 `SettingsController` 用静态内存变量存储，重启后配置丢失；Webhook URL / 邮件收件人也只能改 `appsettings.json` 文件，无法通过页面配置。

需要做的事：

1. **后端**：在 SQLite（开发）中新建 `AlertsConfig` 表，替代内存变量存储
2. **后端**：当用户在页面保存配置（`PUT /api/settings/alerts`）时，将 Webhook URL 和 SMTP 收件人写入数据库
3. **后端**：`WechatWebhookChannel` 和 `SmtpEmailChannel` 在发送时从数据库读取动态配置，而非只读 `appsettings.json`
4. **前端**：在 `AlertsConfigView.vue` 增加企微 Webhook URL 输入框和邮件收件人输入框，保存时调用 `PUT /api/settings/alerts`

相关文件：
- `backend/Api/Controllers/SettingsController.cs`
- `backend/Infrastructure/Notifications/WechatWebhookChannel.cs`
- `backend/Infrastructure/Notifications/SmtpEmailChannel.cs`
- `frontend/src/modules/settings/views/AlertsConfigView.vue`

---

## MVP 锦上添花（不影响验收，可选做）

### T-04 首页顶部"提醒渠道状态"真实检测

> PRD §8.1  
> 当前 Dashboard 的 `AlertChannels` 字段硬编码为 `healthy`，不反映真实情况。

- 后端：`GET /api/dashboard` 中通过检测（ping Webhook URL / SMTP 连接）填入真实状态
- 或：基于最近一次提醒日志中是否有 `Success=false` 推断渠道异常

---

### T-05 今日提醒总数 / 今日已超时总数（真实计数）

> PRD §8.1  
> 当前 `TodayReminderCount` 和 `TodayOverdueCount` 为硬编码数字。

- 后端：`TodayReminderCount` 改为当天 `ReminderLog` 表中 `Success=true` 的记录数
- 后端：`TodayOverdueCount` 改为真实已超时对象总数（T-01 完成后自然可实现）

---

### T-06 运营提示文字生成优化

> PRD §8.2  
> 当前 `OperationTip` 字段硬编码字符串。

- 后端：根据三类风险数量的对比，用简单规则生成动态运营提示文案
- 例如：哪类数量最多 → "今天主要风险集中在 X"，是否高峰日 → "建议提前安排人力"

---

## 非 MVP（阶段二 / 三）

> PRD §15 阶段二、三，本期不做。

- [ ] 历史数据复盘报表
- [ ] 多仓总览
- [ ] 任务分派与处理记录闭环
- [ ] 钉钉通知渠道（PRD §11.1 提到但 MVP 未强制要求）
- [ ] 客户通知记录联动
- [ ] 明细页分页性能与筛选体验优化（T-01 完成后才有意义优化）

---

## 建议执行顺序

```
T-01（真实监控计算）
  ↓ 完成后 T-05 自然完成一半
T-02（数据异常列表）
T-03（配置持久化）
  ↓ 完成后 T-04 自然可做
T-04 / T-05 / T-06（首页状态完善）
```

T-01 是核心依赖，建议优先完成。T-02 和 T-03 可独立并行推进。
