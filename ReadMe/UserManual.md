# OpsMonitor 前后端框架开发使用手册

> 版本：1.0 · 更新日期：2026-05-04  
> 受众：前端工程师 / 后端工程师  
> 目标：用这套脚手架在新项目中 **30 分钟内** 跑通前后端联调

---

## 目录

1. [框架概览](#1-框架概览)
2. [技术栈与版本要求](#2-技术栈与版本要求)
3. [快速启动（新成员上手）](#3-快速启动新成员上手)
4. [目录结构详解](#4-目录结构详解)
5. [前端开发指南](#5-前端开发指南)
6. [后端开发指南](#6-后端开发指南)
7. [前后端联调说明](#7-前后端联调说明)
8. [在新项目中复用本框架](#8-在新项目中复用本框架)
9. [测试规范](#9-测试规范)
10. [常见问题](#10-常见问题)

---

## 1. 框架概览

本框架是一套**可复用的运营监控类系统脚手架**，前后端独立分层，均可作为新项目的起点直接克隆使用。

### 设计原则

| 原则 | 说明 |
|---|---|
| **开发/生产分离** | 本地开发默认使用 SQLite + Mock 认证，无需配置数据库；生产切换 SQL Server + 真实认证，只改配置文件 |
| **零依赖启动** | 前端 `npm run dev`，后端 `dotnet run`，两条命令跑通本地联调 |
| **模块化扩展** | 新业务页面按约定放入 `modules/` 子目录，不影响框架骨架 |
| **Mock 先行** | 认证和数据均有 Mock 实现，前后端可独立并行开发 |
| **测试覆盖框架层** | 路由守卫、健康检查、认证接口均有自动化测试，保障脚手架本身的稳定性 |

---

## 2. 技术栈与版本要求

### 本地环境要求

| 工具 | 最低版本 | 说明 |
|---|---|---|
| Node.js | 22.x | 推荐 LTS 版本 |
| npm | 10.x | 随 Node.js 附带 |
| .NET SDK | 10.0.x | `dotnet --version` 确认 |
| Git | 2.x | 任意现代版本 |

### 前端依赖

| 包 | 用途 |
|---|---|
| Vue 3 + TypeScript | 核心框架，Composition API |
| Vite 8 | 构建工具，开发服务器 |
| Element Plus | UI 组件库 |
| Pinia | 状态管理 |
| Vue Router 5 | 客户端路由 |
| Axios | HTTP 客户端 |
| ECharts 6 | 图表库（按需引入） |
| Vitest + @vue/test-utils | 单元 / 集成测试 |

### 后端依赖

| 包 | 用途 |
|---|---|
| ASP.NET Core 10 | Web API 框架 |
| EF Core 10 | ORM，支持 SQL Server / SQLite |
| Dapper | 高性能读查询 |
| Serilog | 结构化日志 |
| FluentValidation | 请求参数校验 |
| xUnit + WebApplicationFactory | 集成测试 |

---

## 3. 快速启动（新成员上手）

### 3.1 克隆仓库

```bash
git clone <仓库地址>
cd <项目目录>
```

### 3.2 启动后端

```powershell
cd backend\Api

# 第一次启动（自动创建本地 SQLite 数据库）
dotnet run
```

后端默认监听 `http://localhost:5092`，启动后可访问：

- `GET http://localhost:5092/health` → `{ "status": "healthy" }`
- `POST http://localhost:5092/api/auth/login` → 返回 Mock Token

> **无需安装 SQL Server。** 本地开发环境（`ASPNETCORE_ENVIRONMENT=Development`）自动使用 SQLite。

### 3.3 启动前端

```powershell
cd frontend
npm install
npm run dev
```

浏览器访问 `http://localhost:5173`，使用 **admin / admin** 登录。

### 3.4 验证联调

登录后前端会调用 `/api/auth/login`（通过 Vite dev server 代理到后端 `:5092`），返回 `mock-jwt-token` 后跳转到 Dashboard 页面。整个流程通说明前后端联调正常。

---

## 4. 目录结构详解

```
d:\AI\
├── frontend/                   # Vue 3 前端
│   ├── src/
│   │   ├── App.vue             # 根组件（只含 <router-view />）
│   │   ├── main.ts             # 应用入口，挂载 Pinia / Router / ElementPlus
│   │   ├── layouts/
│   │   │   ├── AppShell.vue    # 主布局：侧边栏 + 顶栏 + 内容区
│   │   │   └── AuthShell.vue   # 认证布局：居中登录卡片
│   │   ├── modules/            # ★ 业务模块目录（按功能划分）
│   │   │   └── auth/
│   │   │       ├── views/LoginPageView.vue
│   │   │       └── __tests__/router-guard.spec.ts
│   │   ├── router/
│   │   │   ├── index.ts        # 路由工厂函数 + 全局守卫
│   │   │   └── routes.ts       # 所有路由定义
│   │   ├── services/
│   │   │   └── http.ts         # Axios 实例（含 Auth 拦截器）
│   │   ├── shared/
│   │   │   ├── constants/permissions.ts  # 权限码常量
│   │   │   └── views/ScaffoldPageView.vue # 占位页（待替换）
│   │   └── stores/
│   │       └── auth.ts         # Pinia 认证 Store
│   ├── vite.config.ts          # 构建配置（alias + dev proxy）
│   ├── vitest.config.ts        # 测试配置（独立文件，避免 vue-tsc 冲突）
│   ├── .env.development        # 开发环境变量
│   └── .env.production         # 生产环境变量
│
├── backend/                    # .NET 10 后端
│   ├── Api/                    # ★ Web API 入口层
│   │   ├── Program.cs          # 应用启动配置
│   │   ├── Auth/MockBearerAuthenticationHandler.cs  # Mock 认证处理器
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs      # 登录接口
│   │   │   └── DashboardController.cs # 示例业务接口
│   │   ├── Middleware/GlobalExceptionMiddleware.cs
│   │   ├── appsettings.json           # 生产配置模板
│   │   └── appsettings.Development.json  # 本地开发配置（SQLite + Mock）
│   ├── Application/            # 应用层（用例、接口定义）
│   ├── Domain/                 # 领域层（实体、值对象）
│   ├── Infrastructure/         # 基础设施层
│   │   ├── Persistence/
│   │   │   ├── OpsMonitorDbContext.cs
│   │   │   └── BusinessReadConnectionFactory.cs  # Dapper 连接工厂
│   │   └── InfrastructureServiceExtensions.cs    # DI 注册入口
│   ├── Contracts/              # 请求 / 响应 DTO（前后端共同契约）
│   │   ├── Auth/AuthContracts.cs
│   │   └── Dashboard/DashboardContracts.cs
│   ├── Api.Tests/              # 集成测试
│   └── Directory.Build.props  # 全局 MSBuild 属性（net10.0 / nullable / rootNamespace）
│
└── docs/                       # 文档
```

---

## 5. 前端开发指南

### 5.1 路由与权限

路由定义在 `src/router/routes.ts`，每条需要认证的路由需声明 `meta.requiresAuth: true`：

```ts
{
  path: 'your-page',
  name: 'your-page',
  component: YourPageView,
  meta: { title: '页面标题', requiresAuth: true, permission: 'your-permission-code' },
}
```

全局守卫在 `src/router/index.ts` 中，逻辑：
- 访问需要认证的路由 → 未登录 → 自动跳转 `/login`
- 已登录访问 `/login` → 自动跳转 `/dashboard`

### 5.2 添加新业务模块

按以下目录约定创建新模块，不影响框架骨架：

```
src/modules/<模块名>/
├── views/          # 页面视图组件（命名规范：XxxPageView.vue）
├── components/     # 该模块私有组件
├── composables/    # 该模块私有 composable
└── __tests__/      # 测试文件
```

**步骤：**

1. 在 `src/modules/<模块名>/views/` 创建页面组件
2. 在 `src/router/routes.ts` 的 `AppShell` children 中添加路由
3. 在 `src/layouts/AppShell.vue` 的 `menuItems` 数组中添加菜单项
4. 在 `src/shared/constants/permissions.ts` 添加权限码

### 5.3 权限码规范

权限码定义在 `src/shared/constants/permissions.ts`：

```ts
export const menuPermissions = {
  yourModule: 'your-module',       // 菜单显示权限
} as const

export const buttonPermissions = {
  yourModuleSave: 'your-module-save',  // 按钮级权限
} as const
```

在组件中使用：

```ts
import { menuPermissions } from '@/shared/constants/permissions'

// 根据权限码判断是否显示
const canView = authStore.menuPermissionCodes.includes(menuPermissions.yourModule)
```

### 5.4 HTTP 请求

使用 `src/services/http.ts` 中已配置的 Axios 实例，自动携带 `Authorization: Bearer <token>` 请求头：

```ts
import http from '@/services/http'

// GET 请求
const { data } = await http.get<DashboardSummary>('/api/dashboard/summary')

// POST 请求
const { data } = await http.post<LoginResponse>('/api/auth/login', { username, password })
```

> 开发环境中，所有 `/api/*` 请求通过 Vite proxy 转发到后端 `http://localhost:5092`，无需处理跨域。

### 5.5 状态管理

认证状态统一由 `src/stores/auth.ts` 的 `useAuthStore` 管理：

```ts
const authStore = useAuthStore()

authStore.isAuthenticated    // 是否已登录（computed）
authStore.token              // 当前 Token
authStore.displayName        // 用户显示名
authStore.menuPermissionCodes    // 菜单权限码列表
authStore.buttonPermissionCodes  // 按钮权限码列表

authStore.signIn()   // 登录（当前为 Mock 实现）
authStore.signOut()  // 退出登录
```

### 5.6 环境变量

| 文件 | 环境 | 说明 |
|---|---|---|
| `.env.development` | `npm run dev` | Vite 开发服务器 |
| `.env.production` | `npm run build` | 生产构建 |

当前配置的变量：

```env
VITE_API_BASE_URL=/api   # API 基础路径，生产环境可改为完整域名
```

在代码中通过 `import.meta.env.VITE_API_BASE_URL` 访问。

### 5.7 常用命令

```powershell
npm run dev      # 启动开发服务器（热重载）
npm run build    # 生产构建（先 vue-tsc 类型检查，再 vite build）
npm run test     # 运行所有 Vitest 测试
npm run preview  # 本地预览生产构建产物
```

---

## 6. 后端开发指南

### 6.1 分层架构

```
请求 → Api（Controllers）→ Application（用例）→ Domain（实体）
                              ↓
                        Infrastructure（DbContext / Dapper）
```

| 层 | 职责 | 不应包含 |
|---|---|---|
| **Api** | 接收 HTTP 请求，校验，调用 Application，返回 HTTP 响应 | 业务逻辑 |
| **Application** | 编排业务用例，调用 Domain 和 Infrastructure 接口 | EF Core 直接操作 |
| **Domain** | 实体、值对象、领域事件、业务规则 | 框架依赖 |
| **Infrastructure** | EF Core / Dapper 实现、第三方服务适配器 | 业务规则 |
| **Contracts** | 请求/响应 DTO（与前端共享契约） | 任何逻辑 |

### 6.2 添加新业务接口

**第一步：** 在 `Contracts/` 中定义请求和响应 DTO：

```csharp
// Contracts/YourModule/YourModuleContracts.cs
namespace OpsMonitor.Contracts.YourModule;

public record YourRequest(string Field1, int Field2);
public record YourResponse(Guid Id, string Result, DateTimeOffset CreatedAt);
```

**第二步：** 在 `Api/Controllers/` 新建 Controller：

```csharp
// Api/Controllers/YourModuleController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts.YourModule;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class YourModuleController : ControllerBase
{
    [HttpGet]
    public IActionResult GetList()
    {
        // TODO: 注入 Application 层服务替换此占位实现
        return Ok(new YourResponse(Guid.NewGuid(), "placeholder", DateTimeOffset.UtcNow));
    }
}
```

**第三步：** 在 `Application/` 中实现业务用例（参考 Clean Architecture 模式）。

### 6.3 数据库访问

框架同时支持 EF Core（写操作）和 Dapper（读查询）。

**使用 EF Core（写操作）：**

```csharp
// 在 Application 或 Infrastructure 中
public class YourService(OpsMonitorDbContext db)
{
    public async Task<Guid> CreateAsync(YourEntity entity)
    {
        db.Set<YourEntity>().Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}
```

**使用 Dapper（读查询）：**

```csharp
// 在 Infrastructure 中
public class YourReadService(BusinessReadConnectionFactory connFactory)
{
    public async Task<IEnumerable<YourDto>> QueryListAsync()
    {
        using var conn = connFactory.Create();
        return await conn.QueryAsync<YourDto>("SELECT * FROM YourTable WHERE IsActive = 1");
    }
}
```

### 6.4 配置文件说明

**`appsettings.json`（生产模板）：**

```json
{
  "Persistence": {
    "Provider": "SqlServer"          // 数据库驱动：SqlServer 或 Sqlite
  },
  "ConnectionStrings": {
    "DefaultConnection": "...",      // EF Core 写库连接串
    "BusinessRead": "..."            // Dapper 读库连接串（可指向只读副本）
  },
  "Auth": {
    "MockEnabled": false             // 生产环境务必为 false
  }
}
```

**`appsettings.Development.json`（本地覆盖）：**

```json
{
  "Persistence": {
    "Provider": "Sqlite"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ops-monitor-dev.db",
    "BusinessRead": "Data Source=ops-monitor-dev.db"
  },
  "Auth": {
    "MockEnabled": true
  }
}
```

> **优先级：** `appsettings.Development.json` 的值会覆盖 `appsettings.json`，只在 `ASPNETCORE_ENVIRONMENT=Development` 时加载。

### 6.5 认证机制

**开发阶段（Mock 认证）：**

`MockBearerAuthenticationHandler` 会识别 `Authorization: Bearer mock-jwt-token` 请求头，免去实际认证系统的依赖。前端登录 admin/admin 后自动获得此 Token。

**生产阶段（接入真实认证）：**

在 `Program.cs` 中，将 Mock 方案替换为实际的 JWT Bearer 或 SSO 方案：

```csharp
// 替换以下代码：
builder.Services
    .AddAuthentication(MockBearerAuthenticationHandler.SchemeName)
    .AddScheme<...>(MockBearerAuthenticationHandler.SchemeName, _ => { });

// 改为（以 JWT 为例）：
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Auth:Authority"];
        options.Audience  = configuration["Auth:Audience"];
    });
```

### 6.6 常用命令

```powershell
# 在 backend/ 目录下执行

dotnet build              # 构建整个解决方案
dotnet test               # 运行所有测试
dotnet run --project Api  # 启动 API（Development 环境）

# 添加 EF Core 迁移（需先安装 dotnet-ef 工具）
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api
```

---

## 7. 前后端联调说明

### 7.1 端口约定

| 服务 | 地址 |
|---|---|
| 前端开发服务器 | `http://localhost:5173` |
| 后端 API | `http://localhost:5092` |

### 7.2 代理配置

前端 `vite.config.ts` 中已配置 dev proxy：

```ts
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5092',
      changeOrigin: true,
    },
  },
}
```

前端发出的所有 `/api/*` 请求会被 Vite 自动转发到后端，**无需前端修改基础 URL，无需开启后端 CORS**。

### 7.3 认证流程（端到端）

```
前端 Login 表单
  → POST /api/auth/login { username: "admin", password: "admin" }
  → 后端 AuthController.Login()
  → 返回 { token: "mock-jwt-token", displayName: "Administrator", permissions: [...] }
  → 前端 authStore.signIn() 存储 token
  → 后续请求自动附加 Authorization: Bearer mock-jwt-token
  → 后端 MockBearerAuthenticationHandler 验证通过
  → 返回业务数据
```

### 7.4 健康检查

后端提供 `/health` 无需认证的健康检查接口，可用于监控探活：

```powershell
Invoke-RestMethod http://localhost:5092/health
# 返回: { status: "healthy" }
```

---

## 8. 在新项目中复用本框架

### 8.1 项目命名替换清单

复用框架时，需要将 `OpsMonitor` 相关命名替换为新项目名称（假设新项目叫 `SupplyTrack`）：

**后端（全局搜索替换）：**

| 替换前 | 替换后 |
|---|---|
| `OpsMonitor` | `SupplyTrack` |
| `OpsMonitor.slnx` | `SupplyTrack.slnx`（重命名文件） |
| 所有 `.csproj` 文件名 | 改为 `SupplyTrack.*` |

```powershell
# 快速重命名 csproj 和 sln（在 backend/ 目录执行）
Get-ChildItem -Recurse -Filter "OpsMonitor*.csproj" | Rename-Item -NewName { $_.Name -replace "OpsMonitor", "SupplyTrack" }
Rename-Item OpsMonitor.slnx SupplyTrack.slnx
```

**前端（无需修改框架代码，只需更新以下内容）：**

| 文件 | 修改项 |
|---|---|
| `package.json` | `name` 字段改为新项目名 |
| `index.html` | `<title>` 标签 |
| `AppShell.vue` | Brand 名称（`.brand` 文字） |
| `LoginPageView.vue` | 页面标题文字 |

### 8.2 菜单和路由替换

1. **清空** `src/router/routes.ts` 中 `AppShell` 的 children（保留结构，替换内容）
2. **清空** `src/layouts/AppShell.vue` 中的 `menuItems` 数组
3. **清空** `src/shared/constants/permissions.ts` 中的权限码定义
4. 按新项目的业务模块重新填入以上三处

### 8.3 认证系统替换

**后端：** 编辑 `Program.cs`，将 `MockBearerAuthenticationHandler` 替换为实际认证方案（见 [6.5 认证机制](#65-认证机制)）。

**前端：** 编辑 `src/stores/auth.ts` 的 `signIn()` 函数，将 Mock 赋值替换为调用真实登录接口：

```ts
async function signIn(username: string, password: string) {
  const { data } = await http.post<LoginResponse>('/api/auth/login', { username, password })
  token.value = data.token
  displayName.value = data.displayName
  menuPermissionCodes.value = data.permissions   // 根据实际接口字段调整
}
```

### 8.4 数据库替换

1. 在 `Infrastructure/Persistence/` 中，用 EF Core Fluent API 定义新实体的配置类
2. 在 `OpsMonitorDbContext` 中注册 `DbSet<YourEntity>`
3. 运行 `dotnet ef migrations add InitialCreate` 生成首次迁移
4. 更新 `appsettings.json` 中的 `ConnectionStrings`

### 8.5 框架不需要改动的部分

以下是框架的通用骨架，**复用时无需修改**：

- `Program.cs` 基础结构（只修改认证方案）
- `GlobalExceptionMiddleware.cs`
- `BusinessReadConnectionFactory.cs`
- `InfrastructureServiceExtensions.cs`（只在需要新基础设施服务时扩展）
- `Directory.Build.props`
- `src/router/index.ts`（路由守卫逻辑）
- `src/services/http.ts`（Axios 拦截器）
- `src/layouts/AuthShell.vue`（登录布局）
- `vitest.config.ts` / `vite.config.ts`

---

## 9. 测试规范

### 9.1 前端测试

测试文件放在对应模块的 `__tests__/` 目录，命名格式为 `*.spec.ts`。

```powershell
cd frontend
npm run test       # 运行所有测试
```

**现有测试：**

| 文件 | 测试内容 |
|---|---|
| `src/modules/auth/__tests__/router-guard.spec.ts` | 验证未认证用户访问受保护路由时被重定向到 `/login` |

**编写新测试示例：**

```ts
import { beforeEach, describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import YourComponent from '@/modules/your-module/components/YourComponent.vue'

describe('YourComponent', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('renders correctly', () => {
    const wrapper = mount(YourComponent)
    expect(wrapper.text()).toContain('期望的文本')
  })
})
```

### 9.2 后端测试

测试项目为 `Api.Tests/`，使用 `WebApplicationFactory<Program>` 启动完整的 ASP.NET Core 管道进行集成测试。

```powershell
cd backend
dotnet test
```

**现有测试：**

| 文件 | 测试内容 |
|---|---|
| `Api.Tests/HealthCheckTests.cs` | `/health` 返回 200 |
| `Api.Tests/AuthControllerTests.cs` | 正确凭据登录返回 mock-jwt-token；错误凭据返回 401；Dashboard 无 Token 返回 401；带 Token 返回 200 |

**编写新集成测试示例：**

```csharp
public class YourControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task YourEndpoint_returns_expected_data()
    {
        var client = factory.CreateClient();
        // 携带 Mock Token
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/yourmodule");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## 10. 常见问题

### Q1：后端启动报错找不到数据库连接串

**原因：** 生产配置 `appsettings.json` 中的 `ConnectionStrings` 使用的是 SQL Server，而本地没有 SQL Server。

**解决：** 确认 `ASPNETCORE_ENVIRONMENT` 环境变量为 `Development`。使用 `dotnet run` 时会自动加载 `appsettings.Development.json`（SQLite），无需额外操作。

```powershell
# 手动确认环境变量
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project Api
```

---

### Q2：前端 `npm run test` 报错 `'vitest' 不是内部或外部命令`

**原因：** Windows 下 `node_modules/.bin/` 中的 Shell 脚本无法直接被 cmd.exe 执行。

**解决：** 本框架已将 `test` 脚本改为使用 `.cmd` 文件：

```json
"test": "node_modules\\.bin\\vitest.cmd run --config vitest.config.ts"
```

若 `npm run test` 仍报错，检查 `vitest` 是否已安装：

```powershell
Test-Path frontend/node_modules/vitest   # 应输出 True
```

若为 `False`，重新安装：

```powershell
cd frontend
npm install
```

---

### Q3：前端构建报 `vue-tsc` 类型错误

**原因：** `vitest.config.ts` 和 `vite.config.ts` 被混用。本框架已将两个配置文件分离——`vite.config.ts` 仅用于构建，`vitest.config.ts` 仅用于测试，`vue-tsc` 只扫描构建配置不会遇到 Vitest 专属类型。

**若遇到新的类型错误，** 请检查：

1. 确认 `vite.config.ts` 中**没有** `test:` 配置块
2. 确认 `vitest.config.ts` 中使用的是 `import { defineConfig } from 'vitest/config'`（不是 `vite`）

---

### Q4：新增路由后跳转正常但菜单不高亮

检查 `AppShell.vue` 中的 `menuItems` 数组，确认 `path` 与 `routes.ts` 中定义的路径完全一致（包括斜杠前缀）：

```ts
// routes.ts
path: 'your-page'          // 相对路径（不含 /）

// AppShell.vue menuItems
path: '/your-page'         // 绝对路径（含 /），用于 router.push 和高亮比对
```

---

### Q5：后端新加 Controller 后调用返回 401

**检查项：**

1. Controller 或 Action 上是否有 `[Authorize]` 特性，且请求没有携带 Token
2. 请求头格式是否正确：`Authorization: Bearer mock-jwt-token`（注意大小写和空格）
3. 若是集成测试，确认已添加请求头：
   ```csharp
   client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");
   ```

---

### Q6：如何在生产部署时更改后端监听端口

修改 `Api/Properties/launchSettings.json`（开发用）或通过环境变量控制（推荐生产使用）：

```powershell
# 指定端口启动
dotnet run --urls "http://0.0.0.0:5080"
```

同时更新前端 `vite.config.ts` 的 proxy target 端口（仅影响开发环境）：

```ts
proxy: {
  '/api': {
    target: 'http://localhost:5080',  // 与后端端口保持一致
  },
}
```

---

## 附录 A：AI 编程助手使用指南

> 本节专为 GitHub Copilot、Cursor 等 AI 编程助手提供精确约束。人工阅读可跳过。

### A.1 命名约定（严格遵守）

**前端**

| 类型 | 命名规范 | 示例 |
|---|---|---|
| 页面级组件 | `XxxPageView.vue`，放在 `modules/<模块>/views/` | `DashboardPageView.vue` |
| 布局组件 | `XxxShell.vue`，放在 `layouts/` | `AppShell.vue` |
| 普通组件 | `XxxCard.vue` / `XxxTable.vue` 等，放在 `modules/<模块>/components/` | `AlertSummaryCard.vue` |
| Pinia Store | `use<Domain>Store`，文件名 `<domain>.ts`，放在 `stores/` | `useAuthStore` → `auth.ts` |
| Composable | `use<Feature>`，放在 `modules/<模块>/composables/` | `useAlertList` |
| 权限码 | kebab-case，菜单权限用模块名，按钮权限加动词后缀 | `anomaly-outbound`、`anomaly-outbound-export` |
| 路由 name | kebab-case，与模块路径对应 | `anomaly-outbound` |
| 测试文件 | `*.spec.ts`，放在 `modules/<模块>/__tests__/` | `router-guard.spec.ts` |

**后端**

| 类型 | 命名规范 | 示例 |
|---|---|---|
| Controller | `<Domain>Controller`，放在 `Api/Controllers/` | `DashboardController` |
| DTO（请求） | `<Action>Request`，放在 `Contracts/<Domain>/` | `LoginRequest` |
| DTO（响应） | `<Domain>Response` 或 `<Action>Response` | `LoginResponse`、`DashboardSummaryResponse` |
| EF 实体配置 | `<Entity>Configuration`，放在 `Infrastructure/Persistence/Configurations/` | `AlertConfiguration` |
| 应用服务接口 | `I<Domain>Service`，放在 `Application/<Domain>/` | `IDashboardService` |
| 迁移 | PascalCase 描述性名称 | `AddAlertTable` |

### A.2 严禁事项（禁止生成以下代码）

**前端**

- ❌ 禁止在 `vite.config.ts` 中添加 `test:` 配置块——测试配置只放 `vitest.config.ts`
- ❌ 禁止在 `vitest.config.ts` 中使用 `import { defineConfig } from 'vite'`——必须用 `vitest/config`
- ❌ 禁止在组件内直接使用 `axios`——必须通过 `@/services/http` 中的实例
- ❌ 禁止在 `router/index.ts` 以外的地方写路由守卫逻辑
- ❌ 禁止硬编码权限字符串——必须从 `@/shared/constants/permissions` 引用常量
- ❌ 禁止在 `npm` 脚本中直接写 `vitest`（不含路径）——Windows 下 PATH 不包含 `node_modules/.bin`，必须使用 `node_modules\\.bin\\vitest.cmd`

**后端**

- ❌ 禁止在 `Infrastructure.csproj` 中使用 `<FrameworkReference Include="Microsoft.AspNetCore.App" />`——该项目 SDK 为 `Microsoft.NET.Sdk`（非 Web），应直接 using 已由 EF Core 传递引入的命名空间
- ❌ 禁止在 `Program.cs` 中使用 `try { ... } finally { Log.CloseAndFlush() }` 包裹整个启动流程——会导致 `WebApplicationFactory` 测试失败（IHost 无法正常构建）
- ❌ 禁止在 Controller 中直接调用 `DbContext`——Controller 只调用 Application 层服务
- ❌ 禁止在 `Domain` 项目中引用任何框架包（EF Core、ASP.NET Core 等）
- ❌ 禁止在 `Contracts` 项目中写任何业务逻辑
- ❌ 禁止手动指定已由 EF Core 传递引入的 NuGet 包版本（如 `Microsoft.Data.SqlClient`、`Microsoft.Extensions.DependencyInjection.Abstractions`）——会引发 NU1605 降级冲突

### A.3 框架骨架文件（不得修改，只能扩展）

以下文件是框架约定的骨架，**不得替换或重写，只能在其扩展点处追加代码**：

| 文件 | 允许的修改 |
|---|---|
| `backend/Api/Program.cs` | 只在注释标记处添加新的 `builder.Services.AddXxx()` 和 `app.UseXxx()` |
| `backend/Infrastructure/InfrastructureServiceExtensions.cs` | 在 `AddInfrastructure()` 方法末尾追加新服务注册 |
| `backend/Infrastructure/Persistence/OpsMonitorDbContext.cs` | 添加 `DbSet<T>` 属性 |
| `backend/Directory.Build.props` | 不修改 |
| `frontend/src/router/index.ts` | 不修改守卫逻辑，只修改 `routes.ts` |
| `frontend/src/services/http.ts` | 不修改，若需多实例则新建文件 |
| `frontend/vite.config.ts` | 只修改 `server.proxy` 端口或新增 `alias` |
| `frontend/vitest.config.ts` | 不修改 |

### A.4 添加一个完整业务功能的操作顺序

AI 在生成完整功能时，**必须按以下顺序**操作，不得跳步：

```
1. 后端 Contracts：定义 XxxRequest / XxxResponse（record 类型）
2. 后端 Domain：定义实体和值对象（若需要）
3. 后端 Infrastructure：添加 EF 实体配置 + DbSet（若需要）
4. 后端 Application：定义服务接口 IXxxService + 实现类
5. 后端 Infrastructure：在 InfrastructureServiceExtensions 注册服务
6. 后端 Api：新建 XxxController，注入 IXxxService
7. 后端 Api.Tests：为新 Controller 编写集成测试，使用 WebApplicationFactory
8. 前端 permissions.ts：添加新的菜单/按钮权限码常量
9. 前端 routes.ts：在 AppShell children 中添加路由定义
10. 前端 AppShell.vue：在 menuItems 中添加菜单项
11. 前端 modules/<模块>/views/：创建 XxxPageView.vue
12. 前端 modules/<模块>/__tests__/：编写组件/路由测试
```

### A.5 已知的项目特定坑（生成代码前先核对）

| 坑 | 原因 | 正确做法 |
|---|---|---|
| `vitest` 在 Windows 终端中无法作为命令直接运行 | PATH 不含 `node_modules/.bin` | 用 `node_modules\\.bin\\vitest.cmd` |
| `vue-tsc -b` 构建时扫描到 `vitest/config` import 会报 TS2307 | `vue-tsc` 会 typecheck `vitest.config.ts`，而 vitest 的类型仅作为 devDependency 存在 | 保持 `vitest.config.ts` 与 `vite.config.ts` 完全分离，不要在 `vite.config.ts` 中引入 vitest |
| `tsconfig.app.json` 中 `baseUrl` 在 TypeScript 6.x 报 TS5101 | TypeScript 6 弃用了 `baseUrl` | 已添加 `"ignoreDeprecations": "6.0"`，不要移除此配置 |
| `WebApplicationFactory` 无法启动，报 "entry point exited without ever building an IHost" | `Program.cs` 中 Serilog 的 `try/finally` 包裹会在测试环境下干扰 Host 构建 | `Program.cs` 不使用 `try/finally`，直接顶层语句启动 |
| Infrastructure 类库引用 `IConfiguration`/`IServiceCollection` 报 CS0246 | 类库使用 `Microsoft.NET.Sdk`（非 Web），不自动引入 ASP.NET Core 命名空间 | 添加显式 `using Microsoft.Extensions.Configuration;` 等——这些类型已由 EF Core 传递引入，只需 using |
| `.slnx` 而非 `.sln` | .NET 10 默认生成新格式解决方案文件 | 所有 `dotnet` 命令指定 `OpsMonitor.slnx`，或在 `backend/` 目录下执行（自动发现） |
| EF Core 10 启动报 `RelationalCommandBuilderDependencies registered multiple times` | EF Core 10 严格校验：同一项目中同时安装 SqlServer 和 Sqlite 两个 Provider 包，即使代码只调用其中一个，启动时也会冲突 | 框架模板只保留 Sqlite；需要 SQL Server 时先 `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`，再修改 `InfrastructureServiceExtensions.cs` 中对应分支 |

### A.6 测试中使用 Mock Token 的标准写法

后端集成测试中，所有需要认证的请求必须使用以下固定写法，不得自行生成其他 Token：

```csharp
client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");
```

前端测试中，需要认证状态时使用：

```ts
const authStore = useAuthStore()
authStore.signIn()   // 调用 Mock signIn，不要直接修改 store 内部状态
```
