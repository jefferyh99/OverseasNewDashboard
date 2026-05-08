# Context Handoff

> 更新时间：2026-05-05
> 用途：给接手开发的 Agent 快速建立上下文，减少重新摸索成本。

## 0. 5 分钟速读

- 这是一个海外仓运营监控系统 MVP，前端是 Vue 3 + Vite，后端是 .NET 10 Web API。
- 项目骨架、登录、路由、Dashboard、三类异常页、提醒配置页、通知系统都已经跑通。
- 当前最关键的问题不是页面，而是业务数据仍然主要是 Mock。
- 三类异常接口目前还没有接真实业务库，Dashboard 的主要统计也还是 Mock。
- 通知系统已经实现企业微信和 SMTP 邮件，并有后台扫描、提醒日志、防重复发送和手动触发接口。
- 通知流程是通的，但通知对象仍来自 `MockAnomalyDataSource`，不是真实风险对象。
- 当前没有安装 Hangfire，定时提醒依赖 .NET 内置 `BackgroundService`。
- 本地开发环境使用 SQLite，后端启动时会自动 `EnsureCreated()`。
- 目前最重要的下一步是：把 Mock 风险计算替换成真实监控计算逻辑。
- 次优先任务是：补“数据异常列表”和“提醒接收配置持久化”。
- 本地运行方式：`backend\\Api` 下 `dotnet run`，`frontend` 下 `npm run dev`。
- 最近一次确认可用地址：后端 `http://localhost:5092`，前端 `http://localhost:5175`。
- 关键待办已整理在 [toDoTask.md](toDoTask.md)，接手时先读它，再从 [backend/Api/Controllers/AnomaliesController.cs](backend/Api/Controllers/AnomaliesController.cs) 和 [backend/Api/Controllers/DashboardController.cs](backend/Api/Controllers/DashboardController.cs) 开始。

## 1. 项目定位

- 项目名：OpsMonitor
- 目标：海外仓运营监控系统 MVP
- 依据文档：[docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md](docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md)
- 当前仓库分支：`main`
- 当前最新提交：`859dc89 add:todotask`
- 重要上一阶段提交：`aebb045 feat: implement notification system with WeChat webhook and SMTP email`

## 2. 当前技术栈

### 前端

- Vue 3
- TypeScript
- Vite 8
- Element Plus
- Pinia
- Vue Router 5
- Axios

前端入口目录：`frontend/`

### 后端

- .NET 10
- ASP.NET Core Web API
- EF Core 10
- SQLite（开发环境）
- Dapper
- Serilog
- xUnit + WebApplicationFactory

后端入口目录：`backend/`

## 3. 当前系统状态

### 已完成的业务能力

- 登录与权限 Mock 流程已完成
- 前端基础布局、导航、路由已完成
- 首页 Dashboard 已完成
- 三个异常明细页已完成：出库异常、到仓不齐、上架异常
- 轻量提醒配置页已完成
- 外部消息提醒系统已完成：企业微信 Webhook + SMTP 邮件
- 后端提醒后台扫描服务已完成
- 后端集成测试已通过（此前验证为 8/8）

### 当前仍然是 Mock 的部分

- Dashboard 主要业务数据仍是 Mock
- 三类异常接口仍是 Mock
- 通知扫描数据源仍使用 Mock 风险对象
- 提醒配置目前仍以静态内存和配置文件为主，不是完整持久化方案

## 4. 已实现页面与路由

当前前端路由定义在 [frontend/src/router/routes.ts](frontend/src/router/routes.ts)。

已实现页面：

- `/login`
- `/dashboard`
- `/anomalies/outbound`
- `/anomalies/inbound`
- `/anomalies/shelving`
- `/settings/alerts`

## 5. 已实现后端接口

### 认证

- `POST /api/auth/login`
- `GET /api/auth/validate`
- `GET /api/auth/permissions`

### Dashboard

- `GET /api/dashboard`

### 异常明细

- `GET /api/anomalies/outbound`
- `GET /api/anomalies/inbound`
- `GET /api/anomalies/shelving`

### 设置

- `GET /api/settings/alerts`
- `PUT /api/settings/alerts`

### 通知

- `GET /api/notifications/reminders`
- `POST /api/notifications/trigger`

### 其他

- `GET /health`

## 6. 认证与本地联调方式

### 当前认证机制

- 使用 Mock Bearer 认证
- 登录账号密码：`admin/admin`
- 登录成功后返回 mock token
- 后端认证入口在 [backend/Api/Program.cs](backend/Api/Program.cs)
- 使用 `MockBearerAuthenticationHandler`

### 本地联调地址

最近一次运行时可用地址：

- 后端：http://localhost:5092
- 健康检查：http://localhost:5092/health
- 前端：http://localhost:5175

说明：

- 前端原本通常用 5173 或 5174，但最近运行时这两个端口被占用，所以 Vite 自动切到了 5175
- 如果迁移到新环境，前端端口可能重新回到 5173

## 7. 运行命令

### 后端

在仓库根目录执行：

```powershell
Set-Location 'backend\Api'
dotnet run
```

### 前端

在仓库根目录执行：

```powershell
Set-Location 'frontend'
npm run dev
```

### 验证后端健康状态

```powershell
Invoke-RestMethod http://localhost:5092/health | ConvertTo-Json -Compress
```

返回应类似：

```json
{"status":"healthy"}
```

## 8. 通知系统实现现状

通知系统是最近一轮重点完成的功能，核心事实如下：

- 已实现企业微信机器人渠道
- 已实现 SMTP 邮件渠道
- 已实现提醒日志表 `ReminderLogs`
- 已实现防重复发送与超时重发逻辑
- 已实现后台定时扫描服务 `ReminderScanHostedService`
- 已实现手动触发接口 `POST /api/notifications/trigger`

关键文件：

- [backend/Infrastructure/Notifications/INotificationChannel.cs](backend/Infrastructure/Notifications/INotificationChannel.cs)
- [backend/Infrastructure/Notifications/WechatWebhookChannel.cs](backend/Infrastructure/Notifications/WechatWebhookChannel.cs)
- [backend/Infrastructure/Notifications/SmtpEmailChannel.cs](backend/Infrastructure/Notifications/SmtpEmailChannel.cs)
- [backend/Infrastructure/Notifications/ReminderDispatchService.cs](backend/Infrastructure/Notifications/ReminderDispatchService.cs)
- [backend/Api/BackgroundServices/ReminderScanHostedService.cs](backend/Api/BackgroundServices/ReminderScanHostedService.cs)
- [backend/Api/Controllers/NotificationsController.cs](backend/Api/Controllers/NotificationsController.cs)
- [backend/Domain/ReminderLog.cs](backend/Domain/ReminderLog.cs)

重要限制：

- 当前通知扫描的数据源仍然是 `MockAnomalyDataSource`
- 所以提醒系统流程已经通了，但提醒对象还不是真实业务数据

## 9. 数据库与持久化现状

- 开发环境数据库为 SQLite
- 当前后端启动时会自动 `EnsureCreated()`
- 相关逻辑在 [backend/Api/Program.cs](backend/Api/Program.cs)
- 当前工作区出现了本地数据库文件：
  - `backend/Api/ops-monitor-dev.db`
  - `backend/Api/ops-monitor-dev.db-shm`
  - `backend/Api/ops-monitor-dev.db-wal`

说明：

- 这些是开发运行产生的本地文件，不属于业务代码核心内容
- 迁移环境时可按需删除后重新运行生成

## 10. Hangfire 状态

- 当前项目**没有安装 Hangfire**
- 定时任务使用的是 .NET 内置 `BackgroundService`
- README 中提到基础设施预留了 Hangfire 接入空间，但当前代码未安装、未配置、未启用

## 11. 当前明确未完成项

详细待办见 [toDoTask.md](toDoTask.md)。

最关键的 3 项：

### 1. 真实监控计算逻辑

- 三类异常接口仍返回 Mock 数据
- Dashboard 统计数字也仍是 Mock
- 当前最重要的下一步是接入真实业务数据源，替换异常计算逻辑

### 2. 数据异常列表

- PRD 要求把关键字段缺失对象单独列出来
- 当前尚无对应 API 与前端页面

### 3. 提醒接收配置持久化

- 当前配置接口主要是静态内存实现
- Webhook URL 与邮件接收人还没有真正做到页面化持久配置

## 12. 当前关键代码入口

如果新 Agent 要继续开发，建议从这些文件开始看：

### 后端启动与依赖注入

- [backend/Api/Program.cs](backend/Api/Program.cs)
- [backend/Infrastructure/InfrastructureServiceExtensions.cs](backend/Infrastructure/InfrastructureServiceExtensions.cs)

### 业务接口控制器

- [backend/Api/Controllers/DashboardController.cs](backend/Api/Controllers/DashboardController.cs)
- [backend/Api/Controllers/AnomaliesController.cs](backend/Api/Controllers/AnomaliesController.cs)
- [backend/Api/Controllers/SettingsController.cs](backend/Api/Controllers/SettingsController.cs)
- [backend/Api/Controllers/NotificationsController.cs](backend/Api/Controllers/NotificationsController.cs)

### 通知相关

- [backend/Application/Notifications/IAnomalyDataSource.cs](backend/Application/Notifications/IAnomalyDataSource.cs)
- [backend/Application/Notifications/MockAnomalyDataSource.cs](backend/Application/Notifications/MockAnomalyDataSource.cs)
- [backend/Infrastructure/Notifications/ReminderDispatchService.cs](backend/Infrastructure/Notifications/ReminderDispatchService.cs)

### 前端主要页面

- [frontend/src/modules/dashboard/views/DashboardView.vue](frontend/src/modules/dashboard/views/DashboardView.vue)
- [frontend/src/modules/anomalies/views/OutboundView.vue](frontend/src/modules/anomalies/views/OutboundView.vue)
- [frontend/src/modules/anomalies/views/InboundView.vue](frontend/src/modules/anomalies/views/InboundView.vue)
- [frontend/src/modules/anomalies/views/ShelvingView.vue](frontend/src/modules/anomalies/views/ShelvingView.vue)
- [frontend/src/modules/settings/views/AlertsConfigView.vue](frontend/src/modules/settings/views/AlertsConfigView.vue)

## 13. 最近踩过的坑

- Vite 代理端口曾经配置错过，已修到 5092
- Vue 文件内容替换时曾出现编码损坏，后续创建或编辑源码时要避免错误编码写入
- .NET 构建时若有旧 `dotnet run` 进程占用 DLL，会导致 build 失败，需要先停掉运行中的 API 进程
- 第一次用旧二进制执行测试时曾出现假失败，重新 build 后测试恢复正常

## 14. 当前工作区脏文件状态

当前未提交文件：

- `.vscode/tasks.json`
- `backend/Api/ops-monitor-dev.db`
- `backend/Api/ops-monitor-dev.db-shm`
- `backend/Api/ops-monitor-dev.db-wal`

说明：

- `.vscode/tasks.json` 是为了本地快速启动前后端新增的任务配置
- 三个 SQLite 文件是运行后生成的开发态数据库文件

## 15. 推荐接手顺序

建议新 Agent 接手时按下面顺序推进：

1. 先跑通前后端，确认本地环境一致
2. 阅读 [toDoTask.md](toDoTask.md)
3. 优先做“真实监控计算逻辑”
4. 然后做“数据异常列表”
5. 最后补“提醒接收配置持久化”和首页状态真实性

## 16. 一句话总结

这是一个已经完成前后端骨架、主要页面、Mock 业务接口和通知系统流程的海外仓运营监控 MVP；当前最大的真实缺口不是 UI，而是把 Mock 风险计算替换为真实业务数据计算。