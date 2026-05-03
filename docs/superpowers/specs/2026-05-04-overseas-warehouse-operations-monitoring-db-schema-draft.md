# 海外仓运营监控系统 MVP 核心表结构草案

## 1. 文档信息

- 文档名称：海外仓运营监控系统 MVP 核心表结构草案
- 文档日期：2026-05-04
- 文档状态：V0.1 草案
- 目标读者：后端研发、架构负责人、测试、产品经理
- 关联技术方案：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md)
- 关联接口草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md)
- 关联 PRD：[docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md](docs/prd/2026-05-03-overseas-warehouse-operations-monitoring-prd.md)

## 2. 目标与范围

本草案用于定义监控系统内部数据库的核心表结构，为以下能力提供数据落地基础：

- 标准化快照存储
- 风险结果计算与查询
- 首页聚合读模型
- 提醒事件幂等与审计
- 轻量配置管理
- 同步日志与数据异常记录

本草案不覆盖以下内容：

- 外部业务系统只读库的表结构
- 外部读库 Dapper SQL 明细
- Hangfire 自带表结构
- 非 MVP 范围的历史分析报表、多仓汇总和业务处理闭环

## 3. 设计原则

### 3.1 双库职责分离

- 外部业务系统只读库只负责提供业务事实数据
- 监控系统内部数据库只负责存储标准化结果、计算结果、配置和日志

### 3.2 Snapshot + Result + ReadModel 模式

- Snapshot 表保存标准化后的业务事实
- Result 表保存当前风险判断结果
- ReadModel 表为首页查询提供聚合后的读模型

### 3.3 SQL Server 为正式标准

- 正式目标数据库为 SQL Server
- SQLite 仅用于本地开发辅助，不作为最终数据库行为验证依据

### 3.4 控制 MVP 表数量

- 核心业务表控制在 10 张以内
- 额外系统辅助表单独列出，不计入核心业务表上限

### 3.5 避免过度权限建模

- 认证与权限复用业务系统接口
- 本期不建设本地用户、角色、菜单、按钮权限表
- 权限码映射优先在代码层维护，如后续需要再单独扩展配置表

## 4. 命名与字段约定

### 4.1 表命名约定

- 所有业务表统一使用 `wm_` 前缀
- 快照类表统一以 `_snapshot` 结尾
- 结果类表统一以 `_result` 或 `_aggregate` 结尾
- 日志类表统一以 `_log` 结尾

### 4.2 主键与公共字段约定

除只读聚合明细表外，所有核心表建议至少包含以下公共字段：

- `id`：`bigint`，主键，自增
- `warehouse_id`：`nvarchar(50)`，仓库编号，单仓 MVP 也保留该字段
- `created_at`：`datetimeoffset(0)`，创建时间
- `updated_at`：`datetimeoffset(0)`，更新时间

### 4.3 类型建议

- 标识类：`nvarchar(50)` 到 `nvarchar(64)`
- 名称类：`nvarchar(100)` 到 `nvarchar(200)`
- 枚举类：`nvarchar(32)`
- 说明类：`nvarchar(500)`
- 大文本或 JSON：`nvarchar(max)`
- 时间类：`datetimeoffset(0)`
- 日期类：`date`
- 整数统计类：`int`
- 重量与体积：`decimal(18,3)`
- 布尔：`bit`

### 4.4 JSON 字段使用原则

仅在以下场景允许使用 JSON 字段：

- 首页聚合读模型中的非强结构内容
- 提醒配置中的灵活配置内容
- 提醒消息的发送载荷
- 数据异常的原始上下文快照

对于需要稳定筛选、排序、分页的字段，不应仅存储在 JSON 中。

## 5. 表清单总览

### 5.1 核心业务表 10 张

1. `wm_outbound_order_snapshot`
2. `wm_inbound_asn_snapshot`
3. `wm_carton_snapshot`
4. `wm_monitor_risk_result`
5. `wm_dashboard_aggregate`
6. `wm_dashboard_forecast_daily`
7. `wm_alert_event`
8. `wm_alert_setting`
9. `wm_sync_job_log`
10. `wm_data_anomaly`

### 5.2 系统辅助表 1 张

1. `wm_sync_checkpoint`

说明：

- Hangfire 自带表不在本草案范围内
- 若后续需要权限映射配置，再单独新增权限映射表，不在本期核心表内

## 6. 核心表结构草案

### 6.1 `wm_outbound_order_snapshot`

用途：保存标准化后的出库订单事实数据，作为出库风险计算和明细查询基础。

关键字段建议：

- `id`：主键
- `warehouse_id`：仓库编号
- `order_id`：订单号
- `customer_name`：客户名称
- `channel_name`：渠道名称
- `order_time`：下单时间
- `shipped_time`：实际出库时间
- `promised_sla_hours`：承诺时效，单位小时
- `deadline_at`：承诺出库截止时间
- `current_status`：仓内当前状态
- `shipped_flag`：是否已出库
- `source_updated_at`：源系统最后更新时间
- `sync_job_log_id`：最近同步任务日志编号
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, order_id)`
- 普通索引：`(warehouse_id, deadline_at)`
- 普通索引：`(warehouse_id, source_updated_at)`

说明：

- `deadline_at` 可同步时直接计算并落库，避免后续重复计算
- `customer_name` 与 `channel_name` 分开存储，前端展示时可拼接

### 6.2 `wm_inbound_asn_snapshot`

用途：保存标准化后的入库单事实数据，支撑到仓不齐计算与查询。

关键字段建议：

- `id`
- `warehouse_id`
- `asn_id`：入库单号
- `eta_date`：预计到仓日期
- `planned_carton_count`：计划箱数
- `arrived_carton_count`：已到仓箱数
- `missing_carton_count`：缺箱数
- `first_arrival_time`：首箱到仓时间
- `completed_flag`：是否已到齐
- `deadline_at`：首箱到仓时间 + 72 小时
- `source_updated_at`
- `sync_job_log_id`
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, asn_id)`
- 普通索引：`(warehouse_id, eta_date)`
- 普通索引：`(warehouse_id, first_arrival_time)`

说明：

- `missing_carton_count` 建议冗余存储，便于列表展示和风险计算
- 未首箱到仓的 ASN 仍可落快照，但不会进入到仓不齐风险结果

### 6.3 `wm_carton_snapshot`

用途：保存标准化后的箱级事实数据，支撑上架风险计算和箱级明细查询。

关键字段建议：

- `id`
- `warehouse_id`
- `carton_id`：箱号
- `asn_id`：所属入库单号
- `arrival_time`：箱子到仓时间
- `sku_count`：箱内 SKU 总数
- `unshelved_sku_count`：未上架 SKU 数
- `shelved_flag`：是否完成上架
- `shelved_completed_at`：完成上架时间
- `deadline_at`：到仓时间 + 72 小时
- `source_updated_at`
- `sync_job_log_id`
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, carton_id)`
- 普通索引：`(warehouse_id, asn_id)`
- 普通索引：`(warehouse_id, arrival_time)`

说明：

- 箱级是上架异常的最小监控单位，不按 SKU 行级建独立业务表

### 6.4 `wm_monitor_risk_result`

用途：统一存储三类监控对象的当前风险结果，是首页、明细页和提醒的核心结果表。

关键字段建议：

- `id`
- `warehouse_id`
- `monitor_type`：`outbound`、`inbound`、`shelving`
- `business_key`：订单号、ASN 号或箱号
- `parent_business_key`：父级业务键，上架场景可存 ASN 号
- `risk_status`：`imminent` 或 `overdue`
- `time_status`：`remaining` 或 `overdue`
- `remaining_minutes`：剩余分钟数
- `overdue_minutes`：超时分钟数
- `deadline_at`：截止时间
- `object_status`：对象当前业务状态
- `completed_flag`：是否已完成
- `current_flag`：是否为当前有效风险结果
- `risk_entered_at`：进入风险窗口时间
- `overdue_entered_at`：进入超时状态时间
- `resolved_at`：风险解除时间
- `last_calculated_at`：最后一次计算时间
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, monitor_type, business_key, current_flag)`，其中 `current_flag = 1` 应保证唯一
- 普通索引：`(warehouse_id, monitor_type, risk_status, deadline_at)`
- 普通索引：`(warehouse_id, current_flag, last_calculated_at)`

说明：

- 建议仅保留当前有效结果为 `current_flag = 1`
- 历史提醒审计由 `wm_alert_event` 承担，不依赖保留多版本风险结果
- 明细页字段通过本表与对应快照表按 `business_key` 逻辑关联生成

### 6.5 `wm_dashboard_aggregate`

用途：保存首页顶部状态与今日总览的聚合结果，作为首页读模型主表。

关键字段建议：

- `id`
- `warehouse_id`
- `snapshot_at`：本次聚合生成时间
- `last_sync_time`：最近同步完成时间
- `sync_status`：同步状态
- `today_reminder_count`：今日提醒总数
- `today_overdue_count`：今日已超时总数
- `outbound_total`
- `outbound_imminent_count`
- `outbound_overdue_count`
- `inbound_total`
- `inbound_imminent_count`
- `inbound_overdue_count`
- `shelving_total`
- `shelving_imminent_count`
- `shelving_overdue_count`
- `volume_pressure_level`：今日货量压力等级
- `operation_tip`：今日运营提示
- `alert_channel_summary_json`：提醒渠道状态摘要
- `delayed_data_flag`：是否数据延迟
- `delayed_reason`：延迟原因
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, snapshot_at)`
- 普通索引：`(warehouse_id, created_at desc)`

说明：

- 此表建议保留历史快照，便于排查首页聚合结果变化
- 首页三类异常预览清单建议仍通过 `wm_monitor_risk_result + 快照表` 查询生成，不单独落 JSON 预览列表

### 6.6 `wm_dashboard_forecast_daily`

用途：保存未来 7 天到仓预估的按日聚合结果。

关键字段建议：

- `id`
- `warehouse_id`
- `snapshot_at`：本批预估生成时间
- `forecast_date`：预测日期
- `carton_count`：预计到仓箱数
- `weight`：预计总重量
- `volume`：预计总体积
- `is_peak_day`：是否高峰日
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, snapshot_at, forecast_date)`
- 普通索引：`(warehouse_id, forecast_date)`

说明：

- 未来 7 天预估建议单独建表，而不是塞进首页聚合 JSON，便于后续扩展趋势查询

### 6.7 `wm_alert_event`

用途：保存提醒事件，用于幂等控制、消息发送审计与失败重试追踪。

关键字段建议：

- `id`
- `warehouse_id`
- `monitor_type`
- `business_key`
- `event_type`：`imminent-first`、`overdue-first`、`overdue-summary`
- `alert_channel`：`wechat`、`email` 等
- `dedup_key`：幂等键
- `payload_json`：发送载荷
- `scheduled_at`：计划发送时间
- `sent_at`：实际发送时间
- `send_status`：`pending`、`success`、`failed`、`canceled`
- `retry_count`
- `response_message`：外部渠道返回摘要
- `error_message`
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(dedup_key)`
- 普通索引：`(warehouse_id, send_status, scheduled_at)`
- 普通索引：`(warehouse_id, monitor_type, business_key)`

说明：

- `dedup_key` 建议由 `warehouse_id + monitor_type + business_key + event_type + channel + round` 组成
- 此表承担提醒历史和幂等审计，不建议复用风险结果表记录提醒发送状态

### 6.8 `wm_alert_setting`

用途：保存预警提前量、严重阈值、接收人、渠道开关等轻量配置。

关键字段建议：

- `id`
- `warehouse_id`
- `setting_category`：`lead-time`、`severity-threshold`、`receivers`、`channels`
- `setting_code`：配置编码
- `setting_name`：配置名称
- `setting_value_json`：配置值 JSON
- `enabled`
- `version_no`
- `updated_by`
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, setting_category, setting_code)`

说明：

- 采用“分类 + JSON 值”模式，避免为 MVP 拆过多配置子表
- 若后续配置复杂度提升，再拆分为规则配置表、接收人表、渠道表

### 6.9 `wm_sync_job_log`

用途：保存同步、重算、聚合、提醒扫描等后台任务执行日志。

关键字段建议：

- `id`
- `warehouse_id`
- `job_name`
- `job_type`：`sync`、`recalculate`、`aggregate`、`alert-scan`、`alert-summary`
- `started_at`
- `finished_at`
- `status`：`running`、`success`、`partial-failed`、`failed`
- `source_range_start`：本次抽取起始游标
- `source_range_end`：本次抽取结束游标
- `extracted_count`
- `inserted_count`
- `updated_count`
- `anomaly_count`
- `error_message`
- `trace_id`
- `created_at`
- `updated_at`

索引建议：

- 普通索引：`(warehouse_id, job_name, started_at desc)`
- 普通索引：`(warehouse_id, status, started_at desc)`

说明：

- 任务排查优先依赖本表，不直接依赖 Hangfire 面板数据替代业务日志

### 6.10 `wm_data_anomaly`

用途：保存关键字段缺失或数据异常对象，避免其进入业务异常清单。

关键字段建议：

- `id`
- `warehouse_id`
- `source_type`：`outbound-order`、`asn`、`carton`
- `business_key`
- `anomaly_type`：`missing-field`、`invalid-value`、`unsupported-status`
- `anomaly_field`
- `anomaly_message`
- `source_payload_json`
- `current_flag`
- `first_detected_at`
- `last_detected_at`
- `resolved_at`
- `sync_job_log_id`
- `created_at`
- `updated_at`

索引建议：

- 普通索引：`(warehouse_id, source_type, business_key)`
- 普通索引：`(warehouse_id, current_flag, last_detected_at desc)`

说明：

- 本表用于支撑“数据可能延迟”与数据异常排查，不直接暴露为业务异常页

## 7. 系统辅助表草案

### 7.1 `wm_sync_checkpoint`

用途：保存每类外部读库抽取任务的最新成功游标，支撑增量同步。

关键字段建议：

- `id`
- `warehouse_id`
- `source_name`：`outbound-order`、`inbound-asn`、`carton`
- `checkpoint_field`：`updated_at`、`id` 等
- `checkpoint_value`：游标值，字符串存储
- `last_success_at`
- `last_job_log_id`
- `created_at`
- `updated_at`

索引建议：

- 唯一索引：`(warehouse_id, source_name)`

说明：

- 虽然也可从最近一次成功日志中反推游标，但单独建表更利于稳定恢复和人工干预

## 8. 逻辑关系说明

- `wm_inbound_asn_snapshot` 与 `wm_carton_snapshot` 通过 `warehouse_id + asn_id` 逻辑关联
- `wm_monitor_risk_result` 与三类快照表通过 `warehouse_id + monitor_type + business_key` 逻辑关联
- `wm_alert_event` 与 `wm_monitor_risk_result` 通过 `warehouse_id + monitor_type + business_key` 逻辑关联
- `wm_dashboard_forecast_daily` 与 `wm_dashboard_aggregate` 通过 `warehouse_id + snapshot_at` 逻辑关联
- `wm_data_anomaly` 与 `wm_sync_job_log` 通过 `sync_job_log_id` 逻辑关联
- `wm_sync_checkpoint` 与 `wm_sync_job_log` 通过 `last_job_log_id` 逻辑关联

说明：

- 为减少外部业务键带来的耦合，本期建议弱化数据库层外键约束，以应用层逻辑保证一致性

## 9. 不纳入本期表结构的内容

- 本地用户表、角色表、菜单表、按钮权限表
- 任务处理记录表
- 历史 SLA 统计报表表
- 多仓总部汇总表
- 导出任务表

## 10. SQLite 本地开发注意事项

- 本地 SQLite 仅用于内部表的基础开发与联调，不作为最终行为验证依据
- `datetimeoffset` 在本地可通过字符串或 provider 映射兼容，但最终以 SQL Server 行为为准
- `rowversion`、复杂索引、过滤索引等能力需在 SQL Server 环境验证
- Hangfire 正式存储不以 SQLite 为准

## 11. 建议先行冻结的字段与约束

- `warehouse_id` 的长度和编码规则
- 订单号、ASN 号、箱号的长度上限
- 时间统一使用 `datetimeoffset(0)` 还是 `datetimeoffset(3)`
- 重量与体积的小数精度
- `risk_status`、`sync_status`、`send_status` 等枚举的最终编码
- `wm_alert_setting.setting_value_json` 的结构约定

## 12. 下一步建议

本表结构草案通过后，建议继续推进：

1. 字段映射与样例数据模板
2. EF Core 实体与配置类清单
3. Dapper 读库 SQL 清单与增量抽取策略