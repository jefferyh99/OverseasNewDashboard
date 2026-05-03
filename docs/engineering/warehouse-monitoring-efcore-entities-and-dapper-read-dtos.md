# 海外仓运营监控系统 EF Core 实体与 Dapper 读库 DTO 清单

## 1. 文档信息

- 文档名称：海外仓运营监控系统 EF Core 实体与 Dapper 读库 DTO 清单
- 文档日期：2026-05-04
- 文档状态：V0.1 草案
- 目标读者：后端研发、架构负责人、测试
- 关联技术方案：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md)
- 关联表结构草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md)
- 关联字段映射：[docs/engineering/warehouse-monitoring-field-mapping.md](docs/engineering/warehouse-monitoring-field-mapping.md)
- 关联样例数据：[docs/engineering/warehouse-monitoring-sample-data.md](docs/engineering/warehouse-monitoring-sample-data.md)

## 2. 使用说明

本清单用于在尚未拿到业务系统真实表名、真实视图名、真实 SQL 的阶段，先冻结以下研发输入：

- 监控系统内部数据库对应的 EF Core 实体类
- 外部业务系统只读库抽取用的 Dapper DTO
- 推荐的仓储接口命名和输出对象

当前阶段采用“模拟源对象名 + 稳定 DTO 属性名”的策略：

- 真实业务表名、视图名后续由你替换
- Dapper SQL 由你后续补充
- 但 SQL 输出列别名建议严格对齐本文定义的 DTO 属性名，避免后续改动 DTO

## 3. 推荐目录结构

建议后端代码按以下目录组织：

```text
backend/
  src/
    Domain/
      Entities/
    Infrastructure/
      Persistence/
        Configurations/
      BusinessRead/
        Dtos/
        Repositories/
        Sql/
```

说明：

- `Domain/Entities`：放 EF Core 实体类
- `Infrastructure/Persistence/Configurations`：放 `IEntityTypeConfiguration<T>` 配置类
- `Infrastructure/BusinessRead/Dtos`：放 Dapper 读库 DTO
- `Infrastructure/BusinessRead/Repositories`：放 Dapper 读库仓储接口和实现
- `Infrastructure/BusinessRead/Sql`：放你后续补充的 SQL 文件或 SQL 常量

## 4. 模拟外部读库对象命名建议

在真实表名尚未拿到前，建议先使用以下模拟对象命名来统一讨论口径：

| 监控主题 | 模拟源对象名 | 对应 DTO |
| --- | --- | --- |
| 出库订单读库 | `vw_biz_outbound_order_monitor_src` | `BusinessOutboundOrderReadDto` |
| 入库单读库 | `vw_biz_inbound_asn_monitor_src` | `BusinessInboundAsnReadDto` |
| 箱级上架读库 | `vw_biz_carton_shelving_monitor_src` | `BusinessCartonReadDto` |
| 未来 7 天预估读库 | `vw_biz_inbound_forecast_monitor_src` | `BusinessInboundForecastReadDto` |

说明：

- 这四个名称只是模拟命名，不代表最终真实表名或视图名
- 后续真实 SQL 只要把列别名映射到 DTO 属性名即可，不要求继续保留这些模拟对象名

## 5. EF Core 实体清单

### 5.1 实体与表映射

| 实体类名 | 对应表名 | 说明 |
| --- | --- | --- |
| `OutboundOrderSnapshotEntity` | `wm_outbound_order_snapshot` | 出库订单快照 |
| `InboundAsnSnapshotEntity` | `wm_inbound_asn_snapshot` | 入库单快照 |
| `CartonSnapshotEntity` | `wm_carton_snapshot` | 箱级快照 |
| `MonitorRiskResultEntity` | `wm_monitor_risk_result` | 当前风险结果 |
| `DashboardAggregateEntity` | `wm_dashboard_aggregate` | 首页聚合主表 |
| `DashboardForecastDailyEntity` | `wm_dashboard_forecast_daily` | 未来 7 天预估按日聚合 |
| `AlertEventEntity` | `wm_alert_event` | 提醒事件 |
| `AlertSettingEntity` | `wm_alert_setting` | 提醒配置 |
| `SyncJobLogEntity` | `wm_sync_job_log` | 同步与任务日志 |
| `DataAnomalyEntity` | `wm_data_anomaly` | 数据异常 |
| `SyncCheckpointEntity` | `wm_sync_checkpoint` | 增量游标 |

### 5.2 推荐基础实体

```csharp
namespace WarehouseMonitoring.Domain.Entities;

/// <summary>
/// 监控系统内部数据库实体基类。
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// 主键，自增。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 仓库编号，单仓 MVP 也保留该字段，便于后续扩展多仓。
    /// </summary>
    public string WarehouseId { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### 5.3 EF Core 实体定义草案

```csharp
namespace WarehouseMonitoring.Domain.Entities;

/// <summary>
/// 出库订单快照实体，对应 wm_outbound_order_snapshot。
/// </summary>
public sealed class OutboundOrderSnapshotEntity : EntityBase
{
    /// <summary>
    /// 出库订单号，业务唯一键。
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 客户名称。
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 渠道名称。
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// 下单时间，已统一转换为仓库本地时间。
    /// </summary>
    public DateTimeOffset OrderTime { get; set; }

    /// <summary>
    /// 实际出库时间，未出库时为空。
    /// </summary>
    public DateTimeOffset? ShippedTime { get; set; }

    /// <summary>
    /// 承诺 SLA，单位小时。
    /// </summary>
    public int PromisedSlaHours { get; set; }

    /// <summary>
    /// 承诺出库截止时间。
    /// </summary>
    public DateTimeOffset DeadlineAt { get; set; }

    /// <summary>
    /// 仓内当前状态，例如待出库、已出库。
    /// </summary>
    public string CurrentStatus { get; set; } = string.Empty;

    /// <summary>
    /// 是否已出库。
    /// </summary>
    public bool ShippedFlag { get; set; }

    /// <summary>
    /// 源系统记录最后更新时间，用于增量抽取。
    /// </summary>
    public DateTimeOffset SourceUpdatedAt { get; set; }

    /// <summary>
    /// 最近一次同步任务日志编号。
    /// </summary>
    public long? SyncJobLogId { get; set; }
}

/// <summary>
/// 入库单快照实体，对应 wm_inbound_asn_snapshot。
/// </summary>
public sealed class InboundAsnSnapshotEntity : EntityBase
{
    /// <summary>
    /// 入库单号，业务唯一键。
    /// </summary>
    public string AsnId { get; set; } = string.Empty;

    /// <summary>
    /// 预计到仓日期。
    /// </summary>
    public DateOnly EtaDate { get; set; }

    /// <summary>
    /// 计划箱数。
    /// </summary>
    public int PlannedCartonCount { get; set; }

    /// <summary>
    /// 已到仓箱数。
    /// </summary>
    public int ArrivedCartonCount { get; set; }

    /// <summary>
    /// 缺箱数，建议同步时直接落库。
    /// </summary>
    public int MissingCartonCount { get; set; }

    /// <summary>
    /// 首箱到仓时间，未到首箱时为空。
    /// </summary>
    public DateTimeOffset? FirstArrivalTime { get; set; }

    /// <summary>
    /// 是否已完全到齐。
    /// </summary>
    public bool CompletedFlag { get; set; }

    /// <summary>
    /// 首箱到仓后的 72 小时截止时间，未首箱到仓时可为空。
    /// </summary>
    public DateTimeOffset? DeadlineAt { get; set; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset SourceUpdatedAt { get; set; }

    /// <summary>
    /// 最近一次同步任务日志编号。
    /// </summary>
    public long? SyncJobLogId { get; set; }
}

/// <summary>
/// 箱级快照实体，对应 wm_carton_snapshot。
/// </summary>
public sealed class CartonSnapshotEntity : EntityBase
{
    /// <summary>
    /// 箱号，业务唯一键。
    /// </summary>
    public string CartonId { get; set; } = string.Empty;

    /// <summary>
    /// 所属入库单号。
    /// </summary>
    public string AsnId { get; set; } = string.Empty;

    /// <summary>
    /// 箱子到仓时间。
    /// </summary>
    public DateTimeOffset ArrivalTime { get; set; }

    /// <summary>
    /// 箱内 SKU 总数。
    /// </summary>
    public int SkuCount { get; set; }

    /// <summary>
    /// 未上架 SKU 数。
    /// </summary>
    public int UnshelvedSkuCount { get; set; }

    /// <summary>
    /// 是否已完成上架。
    /// </summary>
    public bool ShelvedFlag { get; set; }

    /// <summary>
    /// 完成上架时间，未完成时为空。
    /// </summary>
    public DateTimeOffset? ShelvedCompletedAt { get; set; }

    /// <summary>
    /// 到仓后的 72 小时截止时间。
    /// </summary>
    public DateTimeOffset DeadlineAt { get; set; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset SourceUpdatedAt { get; set; }

    /// <summary>
    /// 最近一次同步任务日志编号。
    /// </summary>
    public long? SyncJobLogId { get; set; }
}

/// <summary>
/// 当前风险结果实体，对应 wm_monitor_risk_result。
/// </summary>
public sealed class MonitorRiskResultEntity : EntityBase
{
    /// <summary>
    /// 监控类型：outbound、inbound、shelving。
    /// </summary>
    public string MonitorType { get; set; } = string.Empty;

    /// <summary>
    /// 业务键，可能是订单号、入库单号或箱号。
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;

    /// <summary>
    /// 父级业务键，上架场景可存所属入库单号。
    /// </summary>
    public string? ParentBusinessKey { get; set; }

    /// <summary>
    /// 风险状态：imminent 或 overdue。
    /// </summary>
    public string RiskStatus { get; set; } = string.Empty;

    /// <summary>
    /// 时间状态：remaining 或 overdue。
    /// </summary>
    public string TimeStatus { get; set; } = string.Empty;

    /// <summary>
    /// 剩余分钟数，仅对即将超时对象有效。
    /// </summary>
    public int? RemainingMinutes { get; set; }

    /// <summary>
    /// 超时分钟数，仅对已超时对象有效。
    /// </summary>
    public int? OverdueMinutes { get; set; }

    /// <summary>
    /// 当前对象的截止时间。
    /// </summary>
    public DateTimeOffset DeadlineAt { get; set; }

    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string ObjectStatus { get; set; } = string.Empty;

    /// <summary>
    /// 对象是否已完成。
    /// </summary>
    public bool CompletedFlag { get; set; }

    /// <summary>
    /// 是否当前有效结果。
    /// </summary>
    public bool CurrentFlag { get; set; }

    /// <summary>
    /// 进入风险窗口时间。
    /// </summary>
    public DateTimeOffset? RiskEnteredAt { get; set; }

    /// <summary>
    /// 首次进入超时状态时间。
    /// </summary>
    public DateTimeOffset? OverdueEnteredAt { get; set; }

    /// <summary>
    /// 风险解除时间。
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>
    /// 最后一次计算时间。
    /// </summary>
    public DateTimeOffset LastCalculatedAt { get; set; }
}

/// <summary>
/// 首页聚合读模型实体，对应 wm_dashboard_aggregate。
/// </summary>
public sealed class DashboardAggregateEntity : EntityBase
{
    /// <summary>
    /// 本次聚合快照生成时间。
    /// </summary>
    public DateTimeOffset SnapshotAt { get; set; }

    /// <summary>
    /// 最近同步完成时间。
    /// </summary>
    public DateTimeOffset LastSyncTime { get; set; }

    /// <summary>
    /// 同步状态：success、partial-failed、failed、delayed。
    /// </summary>
    public string SyncStatus { get; set; } = string.Empty;

    /// <summary>
    /// 今日提醒总数。
    /// </summary>
    public int TodayReminderCount { get; set; }

    /// <summary>
    /// 今日已超时总数。
    /// </summary>
    public int TodayOverdueCount { get; set; }

    /// <summary>
    /// 出库风险总数。
    /// </summary>
    public int OutboundTotal { get; set; }

    /// <summary>
    /// 出库即将超时数量。
    /// </summary>
    public int OutboundImminentCount { get; set; }

    /// <summary>
    /// 出库已超时数量。
    /// </summary>
    public int OutboundOverdueCount { get; set; }

    /// <summary>
    /// 到仓风险总数。
    /// </summary>
    public int InboundTotal { get; set; }

    /// <summary>
    /// 到仓即将超时数量。
    /// </summary>
    public int InboundImminentCount { get; set; }

    /// <summary>
    /// 到仓已超时数量。
    /// </summary>
    public int InboundOverdueCount { get; set; }

    /// <summary>
    /// 上架风险总数。
    /// </summary>
    public int ShelvingTotal { get; set; }

    /// <summary>
    /// 上架即将超时数量。
    /// </summary>
    public int ShelvingImminentCount { get; set; }

    /// <summary>
    /// 上架已超时数量。
    /// </summary>
    public int ShelvingOverdueCount { get; set; }

    /// <summary>
    /// 今日货量压力等级，例如 low、medium、high。
    /// </summary>
    public string VolumePressureLevel { get; set; } = string.Empty;

    /// <summary>
    /// 今日运营提示文本。
    /// </summary>
    public string OperationTip { get; set; } = string.Empty;

    /// <summary>
    /// 提醒渠道状态摘要 JSON。
    /// </summary>
    public string? AlertChannelSummaryJson { get; set; }

    /// <summary>
    /// 是否存在数据延迟。
    /// </summary>
    public bool DelayedDataFlag { get; set; }

    /// <summary>
    /// 数据延迟原因说明。
    /// </summary>
    public string? DelayedReason { get; set; }
}

/// <summary>
/// 未来 7 天预估按日聚合实体，对应 wm_dashboard_forecast_daily。
/// </summary>
public sealed class DashboardForecastDailyEntity : EntityBase
{
    /// <summary>
    /// 本批预估生成时间。
    /// </summary>
    public DateTimeOffset SnapshotAt { get; set; }

    /// <summary>
    /// 预测日期。
    /// </summary>
    public DateOnly ForecastDate { get; set; }

    /// <summary>
    /// 预计到仓箱数。
    /// </summary>
    public int CartonCount { get; set; }

    /// <summary>
    /// 预计总重量。
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// 预计总体积。
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// 是否高峰日。
    /// </summary>
    public bool IsPeakDay { get; set; }
}

/// <summary>
/// 提醒事件实体，对应 wm_alert_event。
/// </summary>
public sealed class AlertEventEntity : EntityBase
{
    /// <summary>
    /// 监控类型：outbound、inbound、shelving。
    /// </summary>
    public string MonitorType { get; set; } = string.Empty;

    /// <summary>
    /// 业务键。
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;

    /// <summary>
    /// 事件类型：imminent-first、overdue-first、overdue-summary。
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 提醒渠道，例如 wechat、email。
    /// </summary>
    public string AlertChannel { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键。
    /// </summary>
    public string DedupKey { get; set; } = string.Empty;

    /// <summary>
    /// 发送载荷 JSON。
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// 计划发送时间。
    /// </summary>
    public DateTimeOffset ScheduledAt { get; set; }

    /// <summary>
    /// 实际发送时间。
    /// </summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>
    /// 发送状态：pending、success、failed、canceled。
    /// </summary>
    public string SendStatus { get; set; } = string.Empty;

    /// <summary>
    /// 重试次数。
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 外部渠道返回摘要。
    /// </summary>
    public string? ResponseMessage { get; set; }

    /// <summary>
    /// 错误信息。
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 提醒配置实体，对应 wm_alert_setting。
/// </summary>
public sealed class AlertSettingEntity : EntityBase
{
    /// <summary>
    /// 配置分类：lead-time、severity-threshold、receivers、channels。
    /// </summary>
    public string SettingCategory { get; set; } = string.Empty;

    /// <summary>
    /// 配置编码。
    /// </summary>
    public string SettingCode { get; set; } = string.Empty;

    /// <summary>
    /// 配置名称。
    /// </summary>
    public string SettingName { get; set; } = string.Empty;

    /// <summary>
    /// 配置值 JSON。
    /// </summary>
    public string SettingValueJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 配置版本号。
    /// </summary>
    public int VersionNo { get; set; }

    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// 任务执行日志实体，对应 wm_sync_job_log。
/// </summary>
public sealed class SyncJobLogEntity : EntityBase
{
    /// <summary>
    /// 任务名称。
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型：sync、recalculate、aggregate、alert-scan、alert-summary。
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// 开始执行时间。
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// 执行完成时间。
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// 执行状态。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 本次抽取起始游标。
    /// </summary>
    public string? SourceRangeStart { get; set; }

    /// <summary>
    /// 本次抽取结束游标。
    /// </summary>
    public string? SourceRangeEnd { get; set; }

    /// <summary>
    /// 抽取记录数。
    /// </summary>
    public int ExtractedCount { get; set; }

    /// <summary>
    /// 新增记录数。
    /// </summary>
    public int InsertedCount { get; set; }

    /// <summary>
    /// 更新记录数。
    /// </summary>
    public int UpdatedCount { get; set; }

    /// <summary>
    /// 数据异常数量。
    /// </summary>
    public int AnomalyCount { get; set; }

    /// <summary>
    /// 错误信息摘要。
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 跟踪标识。
    /// </summary>
    public string? TraceId { get; set; }
}

/// <summary>
/// 数据异常实体，对应 wm_data_anomaly。
/// </summary>
public sealed class DataAnomalyEntity : EntityBase
{
    /// <summary>
    /// 源对象类型：outbound-order、asn、carton。
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// 业务键。
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;

    /// <summary>
    /// 异常类型：missing-field、invalid-value、unsupported-status。
    /// </summary>
    public string AnomalyType { get; set; } = string.Empty;

    /// <summary>
    /// 异常字段名。
    /// </summary>
    public string? AnomalyField { get; set; }

    /// <summary>
    /// 异常说明。
    /// </summary>
    public string AnomalyMessage { get; set; } = string.Empty;

    /// <summary>
    /// 原始上下文 JSON。
    /// </summary>
    public string? SourcePayloadJson { get; set; }

    /// <summary>
    /// 是否当前仍有效。
    /// </summary>
    public bool CurrentFlag { get; set; }

    /// <summary>
    /// 首次发现时间。
    /// </summary>
    public DateTimeOffset FirstDetectedAt { get; set; }

    /// <summary>
    /// 最近一次发现时间。
    /// </summary>
    public DateTimeOffset LastDetectedAt { get; set; }

    /// <summary>
    /// 解除时间。
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>
    /// 最近同步任务日志编号。
    /// </summary>
    public long? SyncJobLogId { get; set; }
}

/// <summary>
/// 增量同步游标实体，对应 wm_sync_checkpoint。
/// </summary>
public sealed class SyncCheckpointEntity : EntityBase
{
    /// <summary>
    /// 数据源名称，例如 outbound-order、inbound-asn、carton。
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// 游标字段名，例如 updated_at。
    /// </summary>
    public string CheckpointField { get; set; } = string.Empty;

    /// <summary>
    /// 游标值，字符串存储。
    /// </summary>
    public string CheckpointValue { get; set; } = string.Empty;

    /// <summary>
    /// 最近一次成功时间。
    /// </summary>
    public DateTimeOffset? LastSuccessAt { get; set; }

    /// <summary>
    /// 最近一次成功任务日志编号。
    /// </summary>
    public long? LastJobLogId { get; set; }
}
```

### 5.4 推荐配置类命名

| 实体类 | 建议配置类名 |
| --- | --- |
| `OutboundOrderSnapshotEntity` | `OutboundOrderSnapshotConfiguration` |
| `InboundAsnSnapshotEntity` | `InboundAsnSnapshotConfiguration` |
| `CartonSnapshotEntity` | `CartonSnapshotConfiguration` |
| `MonitorRiskResultEntity` | `MonitorRiskResultConfiguration` |
| `DashboardAggregateEntity` | `DashboardAggregateConfiguration` |
| `DashboardForecastDailyEntity` | `DashboardForecastDailyConfiguration` |
| `AlertEventEntity` | `AlertEventConfiguration` |
| `AlertSettingEntity` | `AlertSettingConfiguration` |
| `SyncJobLogEntity` | `SyncJobLogConfiguration` |
| `DataAnomalyEntity` | `DataAnomalyConfiguration` |
| `SyncCheckpointEntity` | `SyncCheckpointConfiguration` |

说明：

- 列类型、索引、唯一约束、JSON 字段长度等建议全部放到 Fluent API 配置类中，不建议用 Data Annotation 分散配置

## 6. Dapper 读库 DTO 清单

### 6.1 DTO 与模拟源对象映射

| DTO 类名 | 模拟源对象名 | 说明 |
| --- | --- | --- |
| `BusinessOutboundOrderReadDto` | `vw_biz_outbound_order_monitor_src` | 出库订单抽取 DTO |
| `BusinessInboundAsnReadDto` | `vw_biz_inbound_asn_monitor_src` | 入库单抽取 DTO |
| `BusinessCartonReadDto` | `vw_biz_carton_shelving_monitor_src` | 箱级上架抽取 DTO |
| `BusinessInboundForecastReadDto` | `vw_biz_inbound_forecast_monitor_src` | 未来 7 天预估抽取 DTO |

### 6.2 Dapper DTO 定义草案

```csharp
namespace WarehouseMonitoring.Infrastructure.BusinessRead.Dtos;

/// <summary>
/// 业务系统出库订单读库 DTO。
/// 后续补 SQL 时，请将真实字段别名映射到本类属性名。
/// 建议模拟源对象名：vw_biz_outbound_order_monitor_src。
/// </summary>
public sealed class BusinessOutboundOrderReadDto
{
    /// <summary>
    /// 仓库编号。
    /// </summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>
    /// 出库订单号。
    /// </summary>
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// 客户名称。
    /// </summary>
    public string? CustomerName { get; init; }

    /// <summary>
    /// 渠道名称。
    /// </summary>
    public string? ChannelName { get; init; }

    /// <summary>
    /// 下单时间。
    /// </summary>
    public DateTimeOffset? OrderTime { get; init; }

    /// <summary>
    /// 实际出库时间。
    /// </summary>
    public DateTimeOffset? ShippedTime { get; init; }

    /// <summary>
    /// 承诺 SLA 小时数。
    /// </summary>
    public int? PromisedSlaHours { get; init; }

    /// <summary>
    /// 当前仓内状态。
    /// </summary>
    public string? CurrentStatus { get; init; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset? SourceUpdatedAt { get; init; }
}

/// <summary>
/// 业务系统入库单读库 DTO。
/// 后续补 SQL 时，请将真实字段别名映射到本类属性名。
/// 建议模拟源对象名：vw_biz_inbound_asn_monitor_src。
/// </summary>
public sealed class BusinessInboundAsnReadDto
{
    /// <summary>
    /// 仓库编号。
    /// </summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>
    /// 入库单号。
    /// </summary>
    public string AsnId { get; init; } = string.Empty;

    /// <summary>
    /// ETA 日期。
    /// </summary>
    public DateOnly? EtaDate { get; init; }

    /// <summary>
    /// 计划箱数。
    /// </summary>
    public int? PlannedCartonCount { get; init; }

    /// <summary>
    /// 已到仓箱数。
    /// </summary>
    public int? ArrivedCartonCount { get; init; }

    /// <summary>
    /// 首箱到仓时间。
    /// </summary>
    public DateTimeOffset? FirstArrivalTime { get; init; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset? SourceUpdatedAt { get; init; }
}

/// <summary>
/// 业务系统箱级上架读库 DTO。
/// 后续补 SQL 时，请将真实字段别名映射到本类属性名。
/// 建议模拟源对象名：vw_biz_carton_shelving_monitor_src。
/// </summary>
public sealed class BusinessCartonReadDto
{
    /// <summary>
    /// 仓库编号。
    /// </summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>
    /// 箱号。
    /// </summary>
    public string CartonId { get; init; } = string.Empty;

    /// <summary>
    /// 所属入库单号。
    /// </summary>
    public string AsnId { get; init; } = string.Empty;

    /// <summary>
    /// 箱子到仓时间。
    /// </summary>
    public DateTimeOffset? ArrivalTime { get; init; }

    /// <summary>
    /// 箱内 SKU 总数。
    /// </summary>
    public int? SkuCount { get; init; }

    /// <summary>
    /// 未上架 SKU 数。
    /// </summary>
    public int? UnshelvedSkuCount { get; init; }

    /// <summary>
    /// 箱级上架完成标记。
    /// </summary>
    public bool? ShelvedFlag { get; init; }

    /// <summary>
    /// 完成上架时间。
    /// </summary>
    public DateTimeOffset? ShelvedCompletedAt { get; init; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset? SourceUpdatedAt { get; init; }
}

/// <summary>
/// 业务系统未来 7 天预估读库 DTO。
/// 后续补 SQL 时，请将真实字段别名映射到本类属性名。
/// 建议模拟源对象名：vw_biz_inbound_forecast_monitor_src。
/// </summary>
public sealed class BusinessInboundForecastReadDto
{
    /// <summary>
    /// 仓库编号。
    /// </summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>
    /// 入库单号。
    /// </summary>
    public string AsnId { get; init; } = string.Empty;

    /// <summary>
    /// ETA 日期。
    /// </summary>
    public DateOnly? EtaDate { get; init; }

    /// <summary>
    /// 计划箱数。
    /// </summary>
    public int? PlannedCartonCount { get; init; }

    /// <summary>
    /// 已到仓箱数。
    /// </summary>
    public int? ArrivedCartonCount { get; init; }

    /// <summary>
    /// 剩余待到仓箱数。
    /// </summary>
    public int? RemainingCartonCount { get; init; }

    /// <summary>
    /// 剩余重量。
    /// </summary>
    public decimal? RemainingWeight { get; init; }

    /// <summary>
    /// 剩余体积。
    /// </summary>
    public decimal? RemainingVolume { get; init; }

    /// <summary>
    /// 源系统记录最后更新时间。
    /// </summary>
    public DateTimeOffset? SourceUpdatedAt { get; init; }
}
```

## 7. Dapper 读库仓储接口清单

建议先按监控主题拆分读库仓储接口，后续 SQL 由你自行补充。

### 7.1 接口清单

| 接口名 | 建议方法 | 输出 DTO |
| --- | --- | --- |
| `IOutboundBusinessReadRepository` | `GetChangedOutboundOrdersAsync` | `BusinessOutboundOrderReadDto` |
| `IInboundAsnBusinessReadRepository` | `GetChangedInboundAsnsAsync` | `BusinessInboundAsnReadDto` |
| `ICartonBusinessReadRepository` | `GetChangedCartonsAsync` | `BusinessCartonReadDto` |
| `IForecastBusinessReadRepository` | `GetForecastInputsAsync` | `BusinessInboundForecastReadDto` |

### 7.2 接口草案

```csharp
namespace WarehouseMonitoring.Infrastructure.BusinessRead.Repositories;

using WarehouseMonitoring.Infrastructure.BusinessRead.Dtos;

/// <summary>
/// 出库订单读库仓储接口。
/// </summary>
public interface IOutboundBusinessReadRepository
{
    /// <summary>
    /// 按增量游标获取发生变化的出库订单。
    /// </summary>
    Task<IReadOnlyList<BusinessOutboundOrderReadDto>> GetChangedOutboundOrdersAsync(
        DateTimeOffset? lastCursor,
        CancellationToken cancellationToken);
}

/// <summary>
/// 入库单读库仓储接口。
/// </summary>
public interface IInboundAsnBusinessReadRepository
{
    /// <summary>
    /// 按增量游标获取发生变化的入库单。
    /// </summary>
    Task<IReadOnlyList<BusinessInboundAsnReadDto>> GetChangedInboundAsnsAsync(
        DateTimeOffset? lastCursor,
        CancellationToken cancellationToken);
}

/// <summary>
/// 箱级上架读库仓储接口。
/// </summary>
public interface ICartonBusinessReadRepository
{
    /// <summary>
    /// 按增量游标获取发生变化的箱级记录。
    /// </summary>
    Task<IReadOnlyList<BusinessCartonReadDto>> GetChangedCartonsAsync(
        DateTimeOffset? lastCursor,
        CancellationToken cancellationToken);
}

/// <summary>
/// 未来 7 天预估读库仓储接口。
/// </summary>
public interface IForecastBusinessReadRepository
{
    /// <summary>
    /// 获取用于未来 7 天预估聚合的输入记录。
    /// </summary>
    Task<IReadOnlyList<BusinessInboundForecastReadDto>> GetForecastInputsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);
}
```

## 8. SQL 后续补充建议

由于当前没有业务系统真实表名和真实 SQL，本阶段建议采用以下约束：

- SQL 文件名先按 DTO 名称命名，例如：
  - `GetChangedOutboundOrders.sql`
  - `GetChangedInboundAsns.sql`
  - `GetChangedCartons.sql`
  - `GetForecastInputs.sql`
- SQL 输出列名必须与 DTO 属性名一致，或通过 `AS` 别名统一到 DTO 属性名
- SQL 中真实表名、视图名和过滤条件由你后续补齐
- SQL 中时间字段需要在查询或映射阶段统一成仓库本地时间口径

## 9. 当前阶段你后续需要补的内容

1. 真实业务系统表名或视图名
2. 增量抽取游标字段
3. Dapper SQL 语句
4. EF Core Fluent API 配置细节
5. 枚举常量类或枚举映射策略

## 10. 实施建议

本清单通过后，建议按以下顺序落地：

1. 先创建 EF Core 实体类和配置类骨架
2. 再创建 Dapper 读库 DTO 和仓储接口
3. 最后按真实表名补 SQL，并将输出列别名对齐 DTO 属性名
