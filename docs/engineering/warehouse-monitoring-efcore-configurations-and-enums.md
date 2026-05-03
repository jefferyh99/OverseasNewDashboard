# 海外仓运营监控系统 EF Core 配置类与枚举常量清单

## 1. 文档信息

- 文档名称：海外仓运营监控系统 EF Core 配置类与枚举常量清单
- 文档日期：2026-05-04
- 文档状态：V0.1 草案
- 目标读者：后端研发、架构负责人、测试
- 关联实体与 DTO 清单：[docs/engineering/warehouse-monitoring-efcore-entities-and-dapper-read-dtos.md](docs/engineering/warehouse-monitoring-efcore-entities-and-dapper-read-dtos.md)
- 关联表结构草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md)
- 关联接口草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md)

## 2. 使用说明

本清单用于补齐后端实现所需的两类稳定输入：

- EF Core Fluent API 配置类命名、职责和关键配置点
- 项目内统一使用的字符串常量类

本项目当前推荐做法如下：

- 数据库存储状态码统一使用字符串编码
- 代码层优先使用 `static class + const string` 形式维护状态常量，而不是直接使用 C# 数值枚举
- EF Core 的表名、索引、字段长度、精度、唯一约束统一放在配置类中，不分散到实体 Data Annotation

## 3. 推荐目录结构

```text
backend/
  src/
    Domain/
      Entities/
      Constants/
    Infrastructure/
      Persistence/
        Configurations/
        Converters/
```

说明：

- `Domain/Constants`：放状态码、权限码、配置分类等常量
- `Infrastructure/Persistence/Configurations`：放 `IEntityTypeConfiguration<T>` 类
- `Infrastructure/Persistence/Converters`：仅在 SQLite 本地调试出现兼容问题时，补充 `DateOnly` 或其他值转换器

## 4. EF Core 配置类清单

### 4.1 配置类总览

| 配置类名 | 对应实体 | 对应表 | 说明 |
| --- | --- | --- | --- |
| `OutboundOrderSnapshotConfiguration` | `OutboundOrderSnapshotEntity` | `wm_outbound_order_snapshot` | 出库订单快照配置 |
| `InboundAsnSnapshotConfiguration` | `InboundAsnSnapshotEntity` | `wm_inbound_asn_snapshot` | 入库单快照配置 |
| `CartonSnapshotConfiguration` | `CartonSnapshotEntity` | `wm_carton_snapshot` | 箱级快照配置 |
| `MonitorRiskResultConfiguration` | `MonitorRiskResultEntity` | `wm_monitor_risk_result` | 风险结果配置 |
| `DashboardAggregateConfiguration` | `DashboardAggregateEntity` | `wm_dashboard_aggregate` | 首页聚合配置 |
| `DashboardForecastDailyConfiguration` | `DashboardForecastDailyEntity` | `wm_dashboard_forecast_daily` | 未来 7 天预估配置 |
| `AlertEventConfiguration` | `AlertEventEntity` | `wm_alert_event` | 提醒事件配置 |
| `AlertSettingConfiguration` | `AlertSettingEntity` | `wm_alert_setting` | 提醒配置配置 |
| `SyncJobLogConfiguration` | `SyncJobLogEntity` | `wm_sync_job_log` | 任务日志配置 |
| `DataAnomalyConfiguration` | `DataAnomalyEntity` | `wm_data_anomaly` | 数据异常配置 |
| `SyncCheckpointConfiguration` | `SyncCheckpointEntity` | `wm_sync_checkpoint` | 增量游标配置 |

### 4.2 推荐公共基类

```csharp
namespace WarehouseMonitoring.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseMonitoring.Domain.Entities;

/// <summary>
/// 所有监控系统实体的公共配置基类。
/// </summary>
public abstract class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WarehouseId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetimeoffset(0)")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetimeoffset(0)")
            .IsRequired();

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
```

## 5. 各配置类关键配置点

### 5.1 `OutboundOrderSnapshotConfiguration`

关键点：

- 表名：`wm_outbound_order_snapshot`
- `OrderId` 最大长度建议 `64`
- `CustomerName`、`ChannelName` 最大长度建议 `100`
- `CurrentStatus` 最大长度建议 `50`
- `DeadlineAt`、`OrderTime`、`ShippedTime` 使用 `datetimeoffset(0)`
- 唯一索引：`WarehouseId + OrderId`
- 普通索引：`WarehouseId + DeadlineAt`
- 普通索引：`WarehouseId + SourceUpdatedAt`

### 5.2 `InboundAsnSnapshotConfiguration`

关键点：

- 表名：`wm_inbound_asn_snapshot`
- `AsnId` 最大长度建议 `64`
- `EtaDate` 映射为 `date`
- `FirstArrivalTime`、`DeadlineAt` 使用 `datetimeoffset(0)`
- 唯一索引：`WarehouseId + AsnId`
- 普通索引：`WarehouseId + EtaDate`
- 普通索引：`WarehouseId + FirstArrivalTime`

### 5.3 `CartonSnapshotConfiguration`

关键点：

- 表名：`wm_carton_snapshot`
- `CartonId`、`AsnId` 最大长度建议 `64`
- `ArrivalTime`、`ShelvedCompletedAt`、`DeadlineAt` 使用 `datetimeoffset(0)`
- 唯一索引：`WarehouseId + CartonId`
- 普通索引：`WarehouseId + AsnId`
- 普通索引：`WarehouseId + ArrivalTime`

### 5.4 `MonitorRiskResultConfiguration`

关键点：

- 表名：`wm_monitor_risk_result`
- `MonitorType`、`RiskStatus`、`TimeStatus` 最大长度建议 `32`
- `BusinessKey`、`ParentBusinessKey` 最大长度建议 `64`
- `ObjectStatus` 最大长度建议 `50`
- `DeadlineAt`、`RiskEnteredAt`、`OverdueEnteredAt`、`ResolvedAt`、`LastCalculatedAt` 使用 `datetimeoffset(0)`
- 唯一索引：`WarehouseId + MonitorType + BusinessKey + CurrentFlag`
- 普通索引：`WarehouseId + MonitorType + RiskStatus + DeadlineAt`
- 普通索引：`WarehouseId + CurrentFlag + LastCalculatedAt`

说明：

- SQL Server 层如需严格保证 `CurrentFlag = 1` 唯一，可后续补充过滤唯一索引；当前阶段先在应用层和复合唯一索引层面约束即可

### 5.5 `DashboardAggregateConfiguration`

关键点：

- 表名：`wm_dashboard_aggregate`
- `SyncStatus`、`VolumePressureLevel` 最大长度建议 `32`
- `OperationTip` 最大长度建议 `500`
- `AlertChannelSummaryJson` 使用 `nvarchar(max)`
- `DelayedReason` 最大长度建议 `500`
- 唯一索引：`WarehouseId + SnapshotAt`
- 普通索引：`WarehouseId + CreatedAt`

### 5.6 `DashboardForecastDailyConfiguration`

关键点：

- 表名：`wm_dashboard_forecast_daily`
- `ForecastDate` 映射为 `date`
- `Weight`、`Volume` 精度使用 `decimal(18,3)`
- 唯一索引：`WarehouseId + SnapshotAt + ForecastDate`
- 普通索引：`WarehouseId + ForecastDate`

### 5.7 `AlertEventConfiguration`

关键点：

- 表名：`wm_alert_event`
- `MonitorType`、`EventType`、`AlertChannel`、`SendStatus` 最大长度建议 `32`
- `BusinessKey`、`DedupKey` 最大长度建议 `128`
- `PayloadJson` 使用 `nvarchar(max)`
- `ResponseMessage`、`ErrorMessage` 最大长度建议 `500`
- 唯一索引：`DedupKey`
- 普通索引：`WarehouseId + SendStatus + ScheduledAt`
- 普通索引：`WarehouseId + MonitorType + BusinessKey`

### 5.8 `AlertSettingConfiguration`

关键点：

- 表名：`wm_alert_setting`
- `SettingCategory`、`SettingCode` 最大长度建议 `50`
- `SettingName` 最大长度建议 `100`
- `SettingValueJson` 使用 `nvarchar(max)`
- `UpdatedBy` 最大长度建议 `64`
- 唯一索引：`WarehouseId + SettingCategory + SettingCode`

### 5.9 `SyncJobLogConfiguration`

关键点：

- 表名：`wm_sync_job_log`
- `JobName`、`JobType`、`Status` 最大长度建议 `32`
- `SourceRangeStart`、`SourceRangeEnd` 最大长度建议 `100`
- `ErrorMessage` 最大长度建议 `500`
- `TraceId` 最大长度建议 `64`
- 普通索引：`WarehouseId + JobName + StartedAt`
- 普通索引：`WarehouseId + Status + StartedAt`

### 5.10 `DataAnomalyConfiguration`

关键点：

- 表名：`wm_data_anomaly`
- `SourceType`、`AnomalyType` 最大长度建议 `32`
- `BusinessKey` 最大长度建议 `64`
- `AnomalyField` 最大长度建议 `64`
- `AnomalyMessage` 最大长度建议 `500`
- `SourcePayloadJson` 使用 `nvarchar(max)`
- 普通索引：`WarehouseId + SourceType + BusinessKey`
- 普通索引：`WarehouseId + CurrentFlag + LastDetectedAt`

### 5.11 `SyncCheckpointConfiguration`

关键点：

- 表名：`wm_sync_checkpoint`
- `SourceName`、`CheckpointField` 最大长度建议 `50`
- `CheckpointValue` 最大长度建议 `100`
- 唯一索引：`WarehouseId + SourceName`

## 6. 推荐配置类代码骨架

以下骨架可直接照着扩展，其余配置类按相同模式实现。

```csharp
namespace WarehouseMonitoring.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseMonitoring.Domain.Entities;

/// <summary>
/// 出库订单快照实体配置。
/// </summary>
public sealed class OutboundOrderSnapshotConfiguration
    : EntityBaseConfiguration<OutboundOrderSnapshotEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OutboundOrderSnapshotEntity> builder)
    {
        builder.ToTable("wm_outbound_order_snapshot");

        builder.Property(x => x.OrderId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(100);

        builder.Property(x => x.ChannelName)
            .HasMaxLength(100);

        builder.Property(x => x.OrderTime)
            .HasColumnType("datetimeoffset(0)")
            .IsRequired();

        builder.Property(x => x.ShippedTime)
            .HasColumnType("datetimeoffset(0)");

        builder.Property(x => x.DeadlineAt)
            .HasColumnType("datetimeoffset(0)")
            .IsRequired();

        builder.Property(x => x.CurrentStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceUpdatedAt)
            .HasColumnType("datetimeoffset(0)")
            .IsRequired();

        builder.HasIndex(x => new { x.WarehouseId, x.OrderId })
            .IsUnique();

        builder.HasIndex(x => new { x.WarehouseId, x.DeadlineAt });

        builder.HasIndex(x => new { x.WarehouseId, x.SourceUpdatedAt });
    }
}
```

## 7. 枚举常量策略

### 7.1 推荐做法

本项目建议统一使用字符串常量类，而不是直接使用 C# 数值枚举，原因如下：

- 数据库中状态码以字符串存储，便于排查和联调
- API 返回值与数据库值保持一致，减少映射层复杂度
- 权限码本身就是字符串，更适合统一为常量类管理

### 7.2 推荐常量类目录

```text
backend/
  src/
    Domain/
      Constants/
        MonitorTypes.cs
        RiskStatuses.cs
        TimeStatuses.cs
        SyncStatuses.cs
        AlertChannels.cs
        AlertChannelStatuses.cs
        AlertEventTypes.cs
        AlertSendStatuses.cs
        JobTypes.cs
        JobStatuses.cs
        SettingCategories.cs
        AnomalyTypes.cs
        AnomalySourceTypes.cs
        VolumePressureLevels.cs
        MenuPermissionCodes.cs
        ButtonPermissionCodes.cs
```

## 8. 常量类清单与定义建议

### 8.1 监控类型

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 三类核心监控类型。
/// </summary>
public static class MonitorTypes
{
    /// <summary>
    /// 出库监控。
    /// </summary>
    public const string Outbound = "outbound";

    /// <summary>
    /// 到仓不齐监控。
    /// </summary>
    public const string Inbound = "inbound";

    /// <summary>
    /// 上架监控。
    /// </summary>
    public const string Shelving = "shelving";
}
```

### 8.2 风险状态与时间状态

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 风险状态常量。
/// </summary>
public static class RiskStatuses
{
    /// <summary>
    /// 即将超时。
    /// </summary>
    public const string Imminent = "imminent";

    /// <summary>
    /// 已超时。
    /// </summary>
    public const string Overdue = "overdue";
}

/// <summary>
/// 时长状态常量。
/// </summary>
public static class TimeStatuses
{
    /// <summary>
    /// 剩余时长。
    /// </summary>
    public const string Remaining = "remaining";

    /// <summary>
    /// 超时时长。
    /// </summary>
    public const string Overdue = "overdue";
}
```

### 8.3 同步状态

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 同步状态常量。
/// </summary>
public static class SyncStatuses
{
    public const string Success = "success";
    public const string PartialFailed = "partial-failed";
    public const string Failed = "failed";
    public const string Delayed = "delayed";
}
```

### 8.4 提醒渠道和提醒渠道状态

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 提醒渠道常量。
/// </summary>
public static class AlertChannels
{
    public const string Wechat = "wechat";
    public const string Email = "email";
    public const string DingTalk = "dingtalk";
}

/// <summary>
/// 提醒渠道状态常量。
/// </summary>
public static class AlertChannelStatuses
{
    public const string Healthy = "healthy";
    public const string Degraded = "degraded";
    public const string Down = "down";
}
```

### 8.5 提醒事件与提醒发送状态

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 提醒事件类型常量。
/// </summary>
public static class AlertEventTypes
{
    public const string ImminentFirst = "imminent-first";
    public const string OverdueFirst = "overdue-first";
    public const string OverdueSummary = "overdue-summary";
}

/// <summary>
/// 提醒发送状态常量。
/// </summary>
public static class AlertSendStatuses
{
    public const string Pending = "pending";
    public const string Success = "success";
    public const string Failed = "failed";
    public const string Canceled = "canceled";
}
```

### 8.6 任务类型与任务状态

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 后台任务类型常量。
/// </summary>
public static class JobTypes
{
    public const string Sync = "sync";
    public const string Recalculate = "recalculate";
    public const string Aggregate = "aggregate";
    public const string AlertScan = "alert-scan";
    public const string AlertSummary = "alert-summary";
}

/// <summary>
/// 后台任务执行状态常量。
/// </summary>
public static class JobStatuses
{
    public const string Running = "running";
    public const string Success = "success";
    public const string PartialFailed = "partial-failed";
    public const string Failed = "failed";
}
```

### 8.7 配置分类、异常类型和异常来源类型

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 提醒配置分类常量。
/// </summary>
public static class SettingCategories
{
    public const string LeadTime = "lead-time";
    public const string SeverityThreshold = "severity-threshold";
    public const string Receivers = "receivers";
    public const string Channels = "channels";
}

/// <summary>
/// 数据异常类型常量。
/// </summary>
public static class AnomalyTypes
{
    public const string MissingField = "missing-field";
    public const string InvalidValue = "invalid-value";
    public const string UnsupportedStatus = "unsupported-status";
}

/// <summary>
/// 数据异常来源类型常量。
/// </summary>
public static class AnomalySourceTypes
{
    public const string OutboundOrder = "outbound-order";
    public const string Asn = "asn";
    public const string Carton = "carton";
}
```

### 8.8 货量压力等级

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 首页货量压力等级常量。
/// </summary>
public static class VolumePressureLevels
{
    public const string Low = "low";
    public const string Medium = "medium";
    public const string High = "high";
}
```

### 8.9 权限码常量

```csharp
namespace WarehouseMonitoring.Domain.Constants;

/// <summary>
/// 菜单权限码常量。
/// </summary>
public static class MenuPermissionCodes
{
    public const string Dashboard = "dashboard";
    public const string AnomalyOutbound = "anomaly-outbound";
    public const string AnomalyInbound = "anomaly-inbound";
    public const string AnomalyShelving = "anomaly-shelving";
    public const string SettingsAlerts = "settings-alerts";
}

/// <summary>
/// 按钮权限码常量。
/// </summary>
public static class ButtonPermissionCodes
{
    public const string SettingsAlertsSave = "settings-alerts-save";
    public const string AnomaliesExport = "anomalies-export";
}
```

## 9. SQLite 本地开发注意事项

- `DateOnly`、`DateTimeOffset`、`decimal(18,3)` 在 SQLite 本地调试时应优先使用 provider 原生映射
- 若 SQLite 本地行为与 SQL Server 有明显差异，再单独补 `ValueConverter`，不要让本地兼容逻辑污染正式 SQL Server 设计
- 过滤索引、复杂索引和字符串排序规则，以 SQL Server 为最终验证标准

## 10. 建议你后续直接补的代码项

1. 为 11 个实体创建对应配置类文件
2. 在 `MonitoringDbContext` 中通过 `ApplyConfigurationsFromAssembly` 自动注册配置
3. 为上述常量类建立统一命名空间并避免重复定义
4. 若后续新增导出能力，再补充 `ExportStatuses` 或导出任务相关常量
