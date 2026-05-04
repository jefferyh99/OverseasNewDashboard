# 可复用监控系统前后端模板骨架设计

## 1. 文档信息

- 文档名称：可复用监控系统前后端模板骨架设计
- 文档日期：2026-05-04
- 文档状态：V1.0
- 目标读者：架构负责人、前端研发、后端研发、测试、项目负责人
- 关联技术方案：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md)
- 关联接口草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md)
- 关联表结构草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md)
- 关联实体与 DTO 清单：[docs/engineering/warehouse-monitoring-efcore-entities-and-dapper-read-dtos.md](docs/engineering/warehouse-monitoring-efcore-entities-and-dapper-read-dtos.md)
- 关联配置与常量清单：[docs/engineering/warehouse-monitoring-efcore-configurations-and-enums.md](docs/engineering/warehouse-monitoring-efcore-configurations-and-enums.md)

## 2. 背景与目标

当前仓库已经完成产品设计、PRD、技术方案、接口草案、表结构草案以及后端实体与 DTO 清单，但尚未搭建真实前后端工程。由于本项目后续希望把这套框架复用到其他中后台监控类项目，因此首版工程骨架不能只满足“海外仓项目可跑”，还需要满足“低成本复制到其他项目”的目标。

本设计文档用于定义一套模板型前后端脚手架，要求同时满足以下目标：

- 当前海外仓运营监控系统可以在该骨架上直接继续开发
- 前后端的基础设施层尽量中性，不把当前业务领域写死到公共层
- 未来新项目可以复制该骨架，仅替换业务模块、页面、接口和部分命名即可继续使用
- 首版脚手架应优先实现可启动、可扩展、可测试，而不是一次性写完全部业务逻辑

## 3. 设计结论

本次采用“单仓模板版 + 内部共享层”的方案。

核心结论如下：

- 仓库采用单仓结构，同时容纳前端、后端、文档
- 前端使用 Vue 3 + TypeScript + Vite，按“应用壳层 + 共享层 + 业务模块层”分层
- 后端使用 .NET 10 + ASP.NET Core Web API，按“Api + Application + Contracts + Domain + Infrastructure”分层
- 公共基础设施、通用组件、统一配置、统一返回模型、统一异常处理和权限接线放入模板层
- 当前海外仓监控能力作为第一组业务模块落在模板层之上，而不是把“warehouse”写进公共底座命名
- 首版只搭建工程骨架、路由、页面占位、接口占位、依赖注入和基础中间件，不在这一轮实现完整业务逻辑

## 4. 范围定义

### 4.1 本次要完成的内容

- 初始化前端工程与后端工程
- 固化仓库目录结构、命名规范、环境配置和启动方式
- 建立前端基础布局、路由守卫、状态管理、请求封装、权限占位和模块目录
- 建立后端基础分层、统一响应、异常处理中间件、依赖注入扩展、日志接线、数据库接线和模块目录
- 为当前海外仓项目创建首批模块占位，包括认证、首页、异常、提醒配置、诊断接口与页面
- 为后续项目保留可替换点，包括模块目录、服务注册、权限映射和业务查询实现
- 形成第一版可提交的框架基线

### 4.2 本次不做的内容

- 不实现真实业务系统 SQL
- 不实现完整的规则计算逻辑
- 不实现真实业务系统认证联调
- 不实现完整的首页聚合与三类异常明细查询
- 不实现导出、历史报表、多仓汇总等非 MVP 范围能力
- 不做项目生成器或参数化模板引擎

## 5. 架构方案

### 5.1 仓库结构

推荐仓库结构如下：

```text
d:/AI
  frontend/
  backend/
  docs/
  .editorconfig
  .gitignore
  README.md
```

说明：

- `frontend/` 放 Web 前端模板工程
- `backend/` 放 .NET 后端模板工程与测试工程
- `docs/` 继续承载产品、技术和研发文档
- 根目录保留仓库级工程说明和统一约束文件

### 5.2 前端分层

推荐前端目录如下：

```text
frontend/
  src/
    app/
    layouts/
    router/
    stores/
    services/
    shared/
      components/
      composables/
      constants/
      styles/
      types/
      utils/
    modules/
      auth/
      dashboard/
      anomalies/
      settings/
    types/
    utils/
```

分层约束：

- `app/`：应用入口、全局 provider、插件注册
- `layouts/`：主布局、登录布局、菜单容器、页头容器
- `router/`：路由声明、权限守卫、动态菜单映射
- `stores/`：全局状态，如登录态、权限态、应用状态
- `services/`：HTTP 基座、请求实例、全局拦截器
- `shared/`：与具体业务无关的通用组件、类型、工具和样式
- `modules/`：按业务域组织页面、局部组件、接口定义、局部 store

### 5.3 后端分层

推荐后端目录如下：

```text
backend/
  src/
    Api/
    Application/
    Contracts/
    Domain/
    Infrastructure/
  tests/
    UnitTests/
    IntegrationTests/
```

分层约束：

- `Api/`：Controller、过滤器、中间件、Swagger、鉴权接线
- `Application/`：查询服务、同步服务、提醒服务、配置服务、编排逻辑
- `Contracts/`：请求模型、响应模型、分页模型、接口 DTO
- `Domain/`：实体、常量、规则接口、领域抽象
- `Infrastructure/`：EF Core、Dapper、Hangfire、Redis、日志、外部认证适配、提醒适配
- `tests/UnitTests/`：纯逻辑单元测试
- `tests/IntegrationTests/`：API 启动、依赖注入、基础路由等集成测试

## 6. 命名与可复用策略

### 6.1 项目命名

首版推荐采用以下命名：

- 前端项目名：`ops-monitor-web`
- 后端解决方案名：`OpsMonitor`
- 后端根命名空间：`OpsMonitor`

命名原则：

- 公共模板层不使用 `warehouse` 作为前缀
- 当前业务领域名称只进入模块、DTO、实体、查询服务等业务层文件
- 以后新项目复用时，可以仅替换业务模块而不改公共基础层

### 6.2 可替换点设计

前端可替换点：

- `src/modules/*`
- `src/services/modules/*`
- `src/router/module-routes.ts`
- `src/shared/styles/theme.css`
- 权限码映射和菜单配置

后端可替换点：

- `Application/*` 下的业务用例实现
- `Domain/*` 下的业务实体、规则与常量扩展
- `Infrastructure/BusinessRead/*` 下的 Dapper SQL 和读库仓储实现
- `Infrastructure/Auth/*` 下的业务系统认证适配实现
- `Infrastructure/Alerts/*` 下的提醒渠道适配实现

## 7. 前端首版设计

### 7.1 首版必须具备的能力

- Vue 3 + TypeScript + Vite 基础工程
- Element Plus、Pinia、Vue Router、Axios、ECharts 接入
- 登录布局和后台主布局
- 登录页、首页、出库异常页、到仓不齐页、上架异常页、提醒配置页占位页面
- 统一加载态、空态、错误态组件
- 权限路由守卫与按钮权限占位指令或工具
- 基础 API 封装与统一错误提示
- 环境变量配置与开发代理配置

### 7.2 首版页面策略

首页与明细页先以“静态结构 + mock 接口”方式跑通页面壳层，目标如下：

- 页面路由完整
- 菜单跳转完整
- 页面基础布局完整
- API 请求链路完整
- 假数据结构与正式接口草案字段对齐

这样可以保证前后端框架先形成联调壳层，再逐步补真实实现。

### 7.3 前端错误处理

前端统一处理以下错误类型：

- 未登录或 token 失效：跳回登录页
- 无权限：显示无权限状态页
- 接口异常：统一 toast 提示并保留页面状态
- 空数据：显示空状态组件而不是报错

## 8. 后端首版设计

### 8.1 首版必须具备的能力

- .NET 10 Web API 解决方案和项目分层
- Serilog 结构化日志接入
- FluentValidation 注册入口
- EF Core DbContext、配置类自动注册入口
- Dapper 读库连接工厂和查询服务占位
- Redis、Hangfire 注册入口与配置占位
- 统一响应结构、中间件异常处理、健康检查基础入口
- Auth、Dashboard、Anomalies、Settings、Diagnostics 五组接口占位

### 8.2 后端接口策略

首版接口以“可启动 + 可返回 mock 结构 + 可对齐正式 contracts”为目标：

- Controller 路由与接口草案保持一致
- 返回结构与已确认 API 草案字段一致
- Application 层使用 mock service 或 in-memory 数据占位
- 后续替换 Application 和 Infrastructure 实现时，不改 API 路由和 Contracts

### 8.3 后端错误处理

后端统一处理以下错误类型：

- 参数校验失败：返回统一校验错误结构
- 未认证：返回 401
- 无权限：返回 403
- 业务异常：返回统一错误码和 traceId
- 未处理异常：进入全局异常中间件并输出结构化日志

## 9. 数据与配置策略

### 9.1 配置文件层次

后端建议至少包含：

- `appsettings.json`
- `appsettings.Development.json`

建议配置节包括：

- `ConnectionStrings`
- `BusinessReadDb`
- `Redis`
- `Hangfire`
- `Serilog`
- `AuthAdapter`
- `Alerting`

前端建议至少包含：

- `.env.development`
- `.env.production`

建议配置项包括：

- `VITE_API_BASE_URL`
- `VITE_APP_TITLE`
- `VITE_ENABLE_MOCK`

### 9.2 数据实现边界

首版后端虽然接入 EF Core 和 Dapper 基础设施，但不在本轮实现真实持久化与真实查询逻辑。

原因如下：

- 当前目标是先建立稳定工程骨架
- 真实 SQL、真实表名与真实认证接口尚未落定
- 先完成框架基线更利于团队并行推进页面和接口开发

## 10. 测试策略

### 10.1 前端测试

首版至少覆盖以下验证：

- 工程可启动
- 路由跳转可用
- 基础页面渲染成功
- 权限守卫逻辑可运行

### 10.2 后端测试

首版至少覆盖以下验证：

- 解决方案可恢复依赖并编译
- Web API 可启动
- 关键路由返回 200 或符合预期的授权响应
- 依赖注入和基础服务注册不报错

### 10.3 验证原则

首版框架提交前，必须至少完成：

- 前端安装依赖并启动一次
- 前端构建一次
- 后端还原依赖并编译一次
- 后端测试项目至少运行最小集成测试一次

## 11. 风险与约束

### 11.1 主要风险

- 若首版就引入过多抽象，会拖慢当前项目落地
- 若公共层写入过多业务命名，会降低后续复用价值
- 若前后端接口占位结构与正式草案不一致，后续会造成重复调整

### 11.2 风险应对

- 基础层只保留稳定抽象，业务逻辑留在模块层
- 所有 mock contracts 严格对齐现有 API 草案
- 先交付可运行骨架，再按业务优先级逐步替换占位实现

## 12. 交付结果

本设计通过后，首版脚手架交付应至少包含：

- 可运行的前端模板工程
- 可运行的后端模板工程
- 前后端基础目录与命名规范
- 已接线但未完全落业务的基础能力
- 一次独立 git 提交，作为后续研发的框架基线

## 13. 实施建议

建议实施顺序如下：

1. 先创建仓库根级约束文件和 README
2. 初始化前端模板工程并跑通页面与路由
3. 初始化后端模板工程并跑通 API 与依赖注入
4. 接入统一配置、日志、异常处理、权限占位
5. 接入首批业务模块壳层
6. 完成构建与启动验证后，提交第一版框架基线
