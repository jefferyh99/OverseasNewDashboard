# 海外仓运营监控系统字段映射模板

## 1. 文档信息

- 文档名称：海外仓运营监控系统字段映射模板
- 文档日期：2026-05-04
- 文档状态：V0.1 模板
- 目标读者：产品经理、后端研发、测试、仓库业务负责人
- 关联技术方案：[docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md](docs/superpowers/specs/2026-05-03-overseas-warehouse-operations-monitoring-technical-solution.md)
- 关联接口草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-api-draft.md)
- 关联表结构草案：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md)

## 2. 使用说明

本模板用于冻结“业务系统只读库 -> 监控系统内部快照/结果”的字段映射关系，评审时重点确认以下内容：

- 源对象或视图是否存在
- 源字段名是否准确
- 该字段是否为必填
- 该字段的时区口径是否明确
- 空值时是进入数据异常，还是允许为空，还是可由派生规则补齐
- 派生字段的计算逻辑是否被业务和研发同时认可

建议评审顺序：

1. 先确认公共约定
2. 再确认出库、到仓不齐、上架三类监控字段
3. 最后确认未来 7 天预估字段与数据异常规则

## 3. 公共约定

### 3.1 基础约定

- 当前 MVP 范围：单仓
- 正式时区口径：所有时间按仓库本地时间计算
- 增量抽取优先依据：源系统更新时间字段
- 正式数据库：SQL Server
- 外部数据接入方式：业务系统只读库 + Dapper 抽取

### 3.2 确认状态说明

- `已确认`：产品、后端、QA、业务负责人均认可
- `待业务确认`：字段定义明确，但需业务方确认口径
- `待研发确认`：业务上明确，但源字段或转换方式需研发确认
- `不适用`：该字段不在对应源对象中维护

### 3.3 空值策略说明

- `进入数据异常`：缺失后不得进入业务异常计算
- `允许为空`：缺失不影响当前 MVP 逻辑
- `可派生`：可通过其他字段计算得出
- `阻断同步`：缺失时整条记录不可入快照表

## 4. 出库监控字段映射

目标快照表：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md) 中的 `wm_outbound_order_snapshot`

| 序号 | 业务字段 | 是否必填 | 源对象/表/视图 | 源字段名 | 目标字段 | 类型建议 | 转换或计算规则 | 时区口径 | 空值策略 | 确认状态 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 仓库编号 | 是 | 待填写 | 待填写 | warehouse_id | nvarchar(50) | 原值映射 | 仓库本地标识，不涉及时区换算 | 阻断同步 | 待业务确认 | 单仓 MVP 也保留该字段 |
| 2 | 订单号 | 是 | 待填写 | 待填写 | order_id | nvarchar(64) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | 唯一键之一 |
| 3 | 客户名称 | 否 | 待填写 | 待填写 | customer_name | nvarchar(100) | 原值映射 | 不涉及时区 | 允许为空 | 待业务确认 | 前端展示可与渠道拼接 |
| 4 | 渠道名称 | 否 | 待填写 | 待填写 | channel_name | nvarchar(100) | 原值映射 | 不涉及时区 | 允许为空 | 待业务确认 | 明细页筛选项之一 |
| 5 | 下单时间 | 是 | 待填写 | 待填写 | order_time | datetimeoffset(0) | 原值映射后统一为仓库本地时间 | 必须是仓库本地时间 | 进入数据异常 | 待业务确认 | 出库监控起算时间 |
| 6 | 实际出库时间 | 否 | 待填写 | 待填写 | shipped_time | datetimeoffset(0) | 原值映射后统一为仓库本地时间 | 必须是仓库本地时间 | 允许为空 | 待业务确认 | 未出库时可为空 |
| 7 | 承诺 SLA 小时 | 是 | 待填写 | 待填写 | promised_sla_hours | int | 原值映射，单位统一为小时 | 不涉及时区 | 进入数据异常 | 待业务确认 | 若源系统不是小时，需换算 |
| 8 | 承诺出库截止时间 | 是 | 无需源字段 | 无需源字段 | deadline_at | datetimeoffset(0) | `order_time + promised_sla_hours` | 使用仓库本地时间计算 | 可派生 | 待研发确认 | 建议同步时直接落库 |
| 9 | 仓内当前状态 | 是 | 待填写 | 待填写 | current_status | nvarchar(50) | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | 例如待出库、已出库 |
| 10 | 是否已出库 | 是 | 待填写 | 待填写 | shipped_flag | bit | 由状态字段或出库时间共同判定 | 不涉及时区 | 可派生 | 待研发确认 | 与明细页字段保持一致 |
| 11 | 源系统更新时间 | 是 | 待填写 | 待填写 | source_updated_at | datetimeoffset(0) | 原值映射 | 使用源系统更新时间原口径 | 阻断同步 | 待研发确认 | 增量抽取主依据 |

## 5. 到仓不齐监控字段映射

目标快照表：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md) 中的 `wm_inbound_asn_snapshot`

| 序号 | 业务字段 | 是否必填 | 源对象/表/视图 | 源字段名 | 目标字段 | 类型建议 | 转换或计算规则 | 时区口径 | 空值策略 | 确认状态 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 仓库编号 | 是 | 待填写 | 待填写 | warehouse_id | nvarchar(50) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | |
| 2 | 入库单号 | 是 | 待填写 | 待填写 | asn_id | nvarchar(64) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | 唯一键之一 |
| 3 | 预计到仓日期 | 是 | 待填写 | 待填写 | eta_date | date | 原值映射 | 以仓库本地日期解释 | 进入数据异常 | 待业务确认 | 未来 7 天预估依赖 |
| 4 | 计划箱数 | 是 | 待填写 | 待填写 | planned_carton_count | int | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | |
| 5 | 已到仓箱数 | 是 | 待填写 | 待填写 | arrived_carton_count | int | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | |
| 6 | 缺箱数 | 是 | 无需源字段 | 无需源字段 | missing_carton_count | int | `planned_carton_count - arrived_carton_count` | 不涉及时区 | 可派生 | 待研发确认 | 小于 0 时进入数据异常 |
| 7 | 首箱到仓时间 | 否 | 待填写 | 待填写 | first_arrival_time | datetimeoffset(0) | 原值映射后统一为仓库本地时间 | 必须是仓库本地时间 | 允许为空 | 待业务确认 | 为空时仅进入预估，不进入到仓不齐监控 |
| 8 | 是否已到齐 | 是 | 无需源字段 | 无需源字段 | completed_flag | bit | `arrived_carton_count >= planned_carton_count` | 不涉及时区 | 可派生 | 待研发确认 | |
| 9 | 截止时间 | 否 | 无需源字段 | 无需源字段 | deadline_at | datetimeoffset(0) | `first_arrival_time + 72 小时` | 使用仓库本地时间计算 | 可派生 | 待研发确认 | 首箱未到时可为空 |
| 10 | 源系统更新时间 | 是 | 待填写 | 待填写 | source_updated_at | datetimeoffset(0) | 原值映射 | 使用源系统更新时间原口径 | 阻断同步 | 待研发确认 | 增量抽取主依据 |

## 6. 上架监控字段映射

目标快照表：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md) 中的 `wm_carton_snapshot`

| 序号 | 业务字段 | 是否必填 | 源对象/表/视图 | 源字段名 | 目标字段 | 类型建议 | 转换或计算规则 | 时区口径 | 空值策略 | 确认状态 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 仓库编号 | 是 | 待填写 | 待填写 | warehouse_id | nvarchar(50) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | |
| 2 | 箱号 | 是 | 待填写 | 待填写 | carton_id | nvarchar(64) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | 唯一键之一 |
| 3 | 所属入库单号 | 是 | 待填写 | 待填写 | asn_id | nvarchar(64) | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | |
| 4 | 箱子到仓时间 | 是 | 待填写 | 待填写 | arrival_time | datetimeoffset(0) | 原值映射后统一为仓库本地时间 | 必须是仓库本地时间 | 进入数据异常 | 待业务确认 | 上架监控起算时间 |
| 5 | 箱内 SKU 总数 | 是 | 待填写 | 待填写 | sku_count | int | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | |
| 6 | 未上架 SKU 数 | 是 | 待填写 | 待填写 | unshelved_sku_count | int | 原值映射 | 不涉及时区 | 进入数据异常 | 待业务确认 | |
| 7 | 是否已完成上架 | 是 | 待填写 | 待填写 | shelved_flag | bit | 由状态字段或 `unshelved_sku_count <= 0` 共同判定 | 不涉及时区 | 可派生 | 待研发确认 | |
| 8 | 完成上架时间 | 否 | 待填写 | 待填写 | shelved_completed_at | datetimeoffset(0) | 原值映射后统一为仓库本地时间 | 必须是仓库本地时间 | 允许为空 | 待业务确认 | 未完成时可为空 |
| 9 | 截止时间 | 是 | 无需源字段 | 无需源字段 | deadline_at | datetimeoffset(0) | `arrival_time + 72 小时` | 使用仓库本地时间计算 | 可派生 | 待研发确认 | |
| 10 | 源系统更新时间 | 是 | 待填写 | 待填写 | source_updated_at | datetimeoffset(0) | 原值映射 | 使用源系统更新时间原口径 | 阻断同步 | 待研发确认 | |

## 7. 未来 7 天到仓预估字段映射

目标读模型表：[docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md](docs/superpowers/specs/2026-05-04-overseas-warehouse-operations-monitoring-db-schema-draft.md) 中的 `wm_dashboard_forecast_daily`

| 序号 | 业务字段 | 是否必填 | 源对象/表/视图 | 源字段名 | 目标字段 | 类型建议 | 转换或计算规则 | 时区口径 | 空值策略 | 确认状态 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 仓库编号 | 是 | 待填写 | 待填写 | warehouse_id | nvarchar(50) | 原值映射 | 不涉及时区 | 阻断同步 | 待业务确认 | |
| 2 | 预计到仓日期 | 是 | 待填写 | 待填写 | forecast_date | date | 使用 ETA 日期聚合 | 以仓库本地日期解释 | 进入数据异常 | 待业务确认 | |
| 3 | 剩余待到仓箱数 | 是 | 待填写 | 待填写 | carton_count | int | 未来 7 天范围内按 ETA 聚合 | 不涉及时区 | 进入数据异常 | 待业务确认 | 首箱未到或未完全到仓对象均可进入预估 |
| 4 | 剩余重量 | 否 | 待填写 | 待填写 | weight | decimal(18,3) | 优先使用剩余重量；若无，则按 `总重量 * 剩余箱数 / 计划箱数` 估算 | 不涉及时区 | 可派生 | 待研发确认 | 需在评审时确认是否接受比例估算 |
| 5 | 剩余体积 | 否 | 待填写 | 待填写 | volume | decimal(18,3) | 优先使用剩余体积；若无，则按 `总体积 * 剩余箱数 / 计划箱数` 估算 | 不涉及时区 | 可派生 | 待研发确认 | |
| 6 | 是否高峰日 | 是 | 无需源字段 | 无需源字段 | is_peak_day | bit | 当天箱数为未来 7 天最大值，且高于日均值 30% 时标记 | 不涉及时区 | 可派生 | 待研发确认 | |

## 8. 数据异常判定模板

以下规则建议在首次联合评审时冻结，作为 `wm_data_anomaly` 的首版识别清单。

| 序号 | 源对象类型 | 异常字段 | 异常条件 | 异常类型 | 处理方式 | 备注 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 出库订单 | 下单时间 | 为空 | missing-field | 进入数据异常，不进入业务异常 | 出库监控起算时间缺失 |
| 2 | 出库订单 | 承诺 SLA 小时 | 为空或小于等于 0 | invalid-value | 进入数据异常 | 无法计算截止时间 |
| 3 | 入库单 | 计划箱数 | 为空或小于等于 0 | invalid-value | 进入数据异常 | 无法计算缺箱数 |
| 4 | 入库单 | 已到仓箱数 | 小于 0 | invalid-value | 进入数据异常 | |
| 5 | 箱级 | 到仓时间 | 为空且 `unshelved_sku_count > 0` | missing-field | 进入数据异常 | 无法计算上架时效 |
| 6 | 箱级 | SKU 总数 | 小于 0 | invalid-value | 进入数据异常 | |

## 9. 联合评审结论页

### 9.1 需签字确认项

- 业务系统只读库对象名称和字段名
- 增量抽取依据字段
- 时区转换规则
- 派生字段计算规则
- 空值与异常处理策略
- 未来 7 天预估中的重量、体积兜底规则

### 9.2 评审记录

| 角色 | 姓名 | 结论 | 日期 | 备注 |
| --- | --- | --- | --- | --- |
| 产品经理 | 待填写 | 待填写 | 待填写 | |
| 后端研发 | 待填写 | 待填写 | 待填写 | |
| QA | 待填写 | 待填写 | 待填写 | |
| 仓库业务负责人 | 待填写 | 待填写 | 待填写 | |
