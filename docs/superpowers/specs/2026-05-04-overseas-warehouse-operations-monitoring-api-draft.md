# 海外仓运营监控系统 MVP 接口清单草案

## 1. 文档信息

- 文档名称：海外仓运营监控系统 MVP 接口清单草案
- 文档日期：2026-05-04
- 文档状态：V0.1 草案
- 目标读者：后端研发、前端研发、测试、产品经理
- 关联技术方案：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md)
- 关联产品设计：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-design.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-design.md)
- 关联 PRD：[docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md](docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md)

## 2. 目标与范围

本接口草案用于明确 MVP 阶段前后端联调边界，覆盖以下能力：

- 认证与权限
- 首页聚合看板
- 三类异常明细页
- 轻量提醒配置
- 系统诊断摘要

本草案不覆盖以下内容：

- 外部业务系统只读库抽取 SQL 细节
- 内部数据库表结构细节
- Hangfire 内部任务管理接口
- 非 MVP 范围的历史报表、多仓汇总、处理闭环功能

## 3. 接口设计约定

### 3.1 基础约定

- 基础路径：`/api`
- 数据格式：`application/json`
- 鉴权方式：`Authorization: Bearer {token}`
- 时间格式：ISO 8601，必须包含时区偏移，例如 `2026-05-04T09:30:00+08:00`
- 所有金额、重量、体积等数值字段使用数值类型，不返回拼接后的展示文案
- 所有需要前端直接展示的时长字段，同时返回原始分钟数与格式化文案

### 3.2 通用响应结构

成功响应建议统一为：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "traceId": "4ac5d4f7d8b54b8c9b7d90a1a3cb7f11",
  "data": {}
}
```

失败响应建议统一为：

```json
{
  "success": false,
  "code": "UNAUTHORIZED",
  "message": "token invalid or expired",
  "traceId": "6d1b7737c1f4482f9b2d3ee73a2a7a44",
  "errors": []
}
```

### 3.3 通用分页结构

列表接口建议统一返回：

```json
{
  "pageNo": 1,
  "pageSize": 20,
  "total": 128,
  "items": []
}
```

### 3.4 通用枚举定义

- `riskStatus`
  - `imminent`：即将超时
  - `overdue`：已超时
- `timeStatus`
  - `remaining`：剩余时长
  - `overdue`：超时时长
- `alertChannelStatus`
  - `healthy`：正常
  - `degraded`：降级
  - `down`：不可用
- `syncStatus`
  - `success`：同步成功
  - `partial-failed`：部分失败
  - `failed`：同步失败
  - `delayed`：同步延迟

## 4. 权限与页面映射

### 4.1 菜单权限码

- `dashboard`
- `anomaly-outbound`
- `anomaly-inbound`
- `anomaly-shelving`
- `settings-alerts`

### 4.2 按钮权限码

- `settings-alerts-save`
- `anomalies-export`

说明：

- `anomalies-search` 和 `anomalies-reset` 若保留，仅用于前端显隐，不单独绑定后端权限接口。
- `anomalies-export` 作为预留能力，本期若未启用导出功能，可暂不开发对应接口。

### 4.3 页面与接口映射

- 登录页
  - `POST /api/auth/login`
  - `GET /api/auth/validate`
  - `GET /api/auth/permissions`
- 首页
  - `GET /api/dashboard`
- 出库异常明细页
  - `GET /api/anomalies/outbound`
- 到仓不齐明细页
  - `GET /api/anomalies/inbound`
- 上架异常明细页
  - `GET /api/anomalies/shelving`
- 提醒配置页
  - `GET /api/settings/alerts`
  - `PUT /api/settings/alerts`
- 系统诊断摘要
  - `GET /api/diagnostics/overview`

## 5. 认证与权限接口

### 5.1 登录接口

- 方法：`POST`
- 路径：`/api/auth/login`
- 鉴权：否
- 说明：测试阶段固定使用 `admin/admin` 登录；正式阶段由后端认证适配层转调业务系统登录接口。

请求体：

```json
{
  "username": "admin",
  "password": "admin"
}
```

响应体 `data`：

```json
{
  "token": "mock-jwt-token",
  "tokenType": "Bearer",
  "expiresIn": 7200,
  "userId": "admin",
  "userName": "admin",
  "displayName": "系统管理员"
}
```

状态码建议：

- `200`：登录成功
- `400`：参数不合法
- `401`：用户名或密码错误

### 5.2 令牌校验接口

- 方法：`GET`
- 路径：`/api/auth/validate`
- 鉴权：是
- 说明：用于页面刷新后校验登录态是否仍然有效。

响应体 `data`：

```json
{
  "valid": true,
  "userId": "admin",
  "userName": "admin",
  "expiresAt": "2026-05-04T18:00:00+08:00"
}
```

状态码建议：

- `200`：token 有效
- `401`：token 无效或已过期

### 5.3 权限获取接口

- 方法：`GET`
- 路径：`/api/auth/permissions`
- 鉴权：是
- 说明：前端登录成功后调用，用于获取菜单权限和按钮权限。

响应体 `data`：

```json
{
  "menuPermissions": [
    "dashboard",
    "anomaly-outbound",
    "anomaly-inbound",
    "anomaly-shelving",
    "settings-alerts"
  ],
  "buttonPermissions": [
    "settings-alerts-save"
  ]
}
```

状态码建议：

- `200`：获取成功
- `401`：token 无效
- `403`：认证通过但权限不可获取

## 6. 首页接口

### 6.1 首页聚合接口

- 方法：`GET`
- 路径：`/api/dashboard`
- 鉴权：是
- 菜单权限：`dashboard`
- 说明：首页接口采用聚合返回，减少前端多次请求。

请求参数：

- `warehouseId`：可选，当前 MVP 为单仓，可由后端默认推断

响应体 `data`：

```json
{
  "warehouse": {
    "warehouseId": "WH-US-001",
    "warehouseName": "美国 1 号仓"
  },
  "baseStatus": {
    "lastSyncTime": "2026-05-04T10:00:00+08:00",
    "syncStatus": "success",
    "todayReminderCount": 16,
    "todayOverdueCount": 9,
    "alertChannels": [
      {
        "channelCode": "wechat",
        "channelName": "企业微信",
        "status": "healthy"
      },
      {
        "channelCode": "email",
        "channelName": "邮件",
        "status": "healthy"
      }
    ],
    "delayedDataFlag": false,
    "delayedReason": null
  },
  "todayOverview": {
    "outboundRiskCount": 23,
    "inboundRiskCount": 7,
    "shelvingRiskCount": 15,
    "todayVolumePressureLevel": "medium",
    "operationTip": "今天主要风险集中在出库时效，建议优先处理渠道 A 的待出库订单。"
  },
  "anomalyPreview": {
    "outbound": {
      "total": 23,
      "imminentCount": 10,
      "overdueCount": 13,
      "items": []
    },
    "inbound": {
      "total": 7,
      "imminentCount": 2,
      "overdueCount": 5,
      "items": []
    },
    "shelving": {
      "total": 15,
      "imminentCount": 4,
      "overdueCount": 11,
      "items": []
    }
  },
  "forecast7Days": [
    {
      "date": "2026-05-05",
      "cartonCount": 120,
      "weight": 1532.5,
      "volume": 32.8,
      "isPeakDay": false
    }
  ]
}
```

`anomalyPreview.outbound.items` 字段建议：

- `orderId`
- `customerOrChannel`
- `orderTime`
- `deadlineAt`
- `currentStatus`
- `riskStatus`
- `timeStatus`
- `timeValueMinutes`
- `timeValueLabel`

`anomalyPreview.inbound.items` 字段建议：

- `asnId`
- `firstArrivalTime`
- `plannedCartonCount`
- `arrivedCartonCount`
- `missingCartonCount`
- `deadlineAt`
- `riskStatus`
- `timeStatus`
- `timeValueMinutes`
- `timeValueLabel`

`anomalyPreview.shelving.items` 字段建议：

- `cartonId`
- `asnId`
- `arrivalTime`
- `skuCount`
- `unshelvedSkuCount`
- `deadlineAt`
- `riskStatus`
- `timeStatus`
- `timeValueMinutes`
- `timeValueLabel`

## 7. 三类异常明细接口

### 7.1 出库异常明细接口

- 方法：`GET`
- 路径：`/api/anomalies/outbound`
- 鉴权：是
- 菜单权限：`anomaly-outbound`

请求参数：

- `riskStatus`：可选，`imminent` 或 `overdue`
- `channel`：可选
- `customer`：可选
- `orderTimeStart`：可选
- `orderTimeEnd`：可选
- `pageNo`：必填
- `pageSize`：必填
- `sortBy`：可选，默认 `deadlineAt`
- `sortDirection`：可选，`asc` 或 `desc`

响应体 `data`：

```json
{
  "summary": {
    "total": 128,
    "imminentCount": 48,
    "overdueCount": 80
  },
  "filterOptions": {
    "channels": ["渠道A", "渠道B"],
    "customers": ["客户A", "客户B"]
  },
  "list": {
    "pageNo": 1,
    "pageSize": 20,
    "total": 128,
    "items": [
      {
        "orderId": "SO20260504001",
        "customerOrChannel": "客户A / 渠道A",
        "orderTime": "2026-05-03T08:30:00+08:00",
        "deadlineAt": "2026-05-04T08:30:00+08:00",
        "timeStatus": "overdue",
        "timeValueMinutes": 180,
        "timeValueLabel": "超时 3 小时",
        "currentStatus": "待出库",
        "shipped": false,
        "riskStatus": "overdue"
      }
    ]
  }
}
```

### 7.2 到仓不齐明细接口

- 方法：`GET`
- 路径：`/api/anomalies/inbound`
- 鉴权：是
- 菜单权限：`anomaly-inbound`

请求参数：

- `riskStatus`：可选，`imminent` 或 `overdue`
- `etaDateStart`：可选
- `etaDateEnd`：可选
- `pageNo`：必填
- `pageSize`：必填
- `sortBy`：可选，默认 `deadlineAt`
- `sortDirection`：可选，`asc` 或 `desc`

响应体 `data`：

```json
{
  "summary": {
    "total": 32,
    "imminentCount": 11,
    "overdueCount": 21
  },
  "list": {
    "pageNo": 1,
    "pageSize": 20,
    "total": 32,
    "items": [
      {
        "asnId": "ASN20260504001",
        "etaDate": "2026-05-02",
        "plannedCartonCount": 50,
        "arrivedCartonCount": 41,
        "missingCartonCount": 9,
        "firstArrivalTime": "2026-05-02T11:00:00+08:00",
        "deadlineAt": "2026-05-05T11:00:00+08:00",
        "timeStatus": "remaining",
        "timeValueMinutes": 360,
        "timeValueLabel": "剩余 6 小时",
        "riskStatus": "imminent"
      }
    ]
  }
}
```

### 7.3 上架异常明细接口

- 方法：`GET`
- 路径：`/api/anomalies/shelving`
- 鉴权：是
- 菜单权限：`anomaly-shelving`

请求参数：

- `riskStatus`：可选，`imminent` 或 `overdue`
- `arrivalDateStart`：可选
- `arrivalDateEnd`：可选
- `pageNo`：必填
- `pageSize`：必填
- `sortBy`：可选，默认 `deadlineAt`
- `sortDirection`：可选，`asc` 或 `desc`

响应体 `data`：

```json
{
  "summary": {
    "total": 54,
    "imminentCount": 18,
    "overdueCount": 36
  },
  "list": {
    "pageNo": 1,
    "pageSize": 20,
    "total": 54,
    "items": [
      {
        "cartonId": "CTN20260504001",
        "asnId": "ASN20260503008",
        "arrivalTime": "2026-05-01T14:00:00+08:00",
        "skuCount": 18,
        "unshelvedSkuCount": 6,
        "deadlineAt": "2026-05-04T14:00:00+08:00",
        "timeStatus": "overdue",
        "timeValueMinutes": 90,
        "timeValueLabel": "超时 1.5 小时",
        "riskStatus": "overdue"
      }
    ]
  }
}
```

### 7.4 预留导出接口

说明：若本期确认需要导出能力，可启用以下预留接口；若本期不做导出，可暂不开发。

- 方法：`POST`
- 路径：`/api/anomalies/{type}/export`
- 鉴权：是
- 按钮权限：`anomalies-export`

请求参数沿用对应明细页筛选条件。

## 8. 提醒配置接口

### 8.1 获取提醒配置接口

- 方法：`GET`
- 路径：`/api/settings/alerts`
- 鉴权：是
- 菜单权限：`settings-alerts`

响应体 `data`：

```json
{
  "warehouseId": "WH-US-001",
  "leadTimes": [
    {
      "monitorType": "outbound",
      "leadTimeHours": 4
    },
    {
      "monitorType": "inbound",
      "leadTimeHours": 12
    },
    {
      "monitorType": "shelving",
      "leadTimeHours": 12
    }
  ],
  "severityThresholds": [
    {
      "monitorType": "outbound",
      "metricCode": "overdue-count",
      "thresholdValue": 20
    }
  ],
  "receivers": {
    "userIds": ["u001", "u002"],
    "groupIds": ["g001"],
    "emails": ["ops@example.com"]
  },
  "channels": [
    {
      "channelCode": "wechat",
      "enabled": true
    },
    {
      "channelCode": "email",
      "enabled": true
    }
  ],
  "updatedAt": "2026-05-04T10:30:00+08:00",
  "updatedBy": "admin"
}
```

### 8.2 保存提醒配置接口

- 方法：`PUT`
- 路径：`/api/settings/alerts`
- 鉴权：是
- 按钮权限：`settings-alerts-save`

请求体结构与获取接口响应体 `data` 基本一致，可省略只读字段 `updatedAt`、`updatedBy`。

状态码建议：

- `200`：保存成功
- `400`：参数不合法
- `401`：未登录
- `403`：无保存权限

## 9. 诊断接口

### 9.1 诊断摘要接口

- 方法：`GET`
- 路径：`/api/diagnostics/overview`
- 鉴权：是
- 说明：供系统诊断摘要、顶部健康状态、联调和排查使用。

响应体 `data`：

```json
{
  "warehouse": {
    "warehouseId": "WH-US-001",
    "warehouseName": "美国 1 号仓"
  },
  "sync": {
    "lastSyncTime": "2026-05-04T10:00:00+08:00",
    "syncStatus": "success",
    "delayedDataFlag": false,
    "delayedReason": null
  },
  "alerts": {
    "channels": [
      {
        "channelCode": "wechat",
        "status": "healthy"
      },
      {
        "channelCode": "email",
        "status": "healthy"
      }
    ],
    "todayReminderCount": 16
  },
  "dataAnomalies": {
    "count": 3,
    "topExamples": [
      "出库订单缺少下单时间",
      "入库单缺少计划箱数"
    ]
  }
}
```

## 10. 错误码建议

建议首版统一以下错误码：

- `OK`
- `BAD_REQUEST`
- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `VALIDATION_FAILED`
- `SYNC_DELAYED`
- `BUSINESS_SYSTEM_UNAVAILABLE`
- `INTERNAL_ERROR`

## 11. 联调前需冻结的事项

- 业务系统登录接口、token 校验接口、权限接口的字段契约
- 外部业务只读库的字段清单与增量抽取依据
- 菜单权限码与按钮权限码最终编码
- 提醒配置中严重阈值的具体口径
- 是否在 MVP 首版启用导出接口

## 12. 下一步建议

本接口草案通过后，建议继续补充以下研发输入：

1. 核心表结构草案
2. 字段映射与样例数据模板
3. 前后端 DTO 与枚举定义清单