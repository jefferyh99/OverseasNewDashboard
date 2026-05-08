using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Workload;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/workloads")]
[Authorize]
public class WorkloadsController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard([FromQuery] string warehouseCode = "DE")
    {
        if (!IsSupportedWarehouse(warehouseCode))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "Unsupported warehouseCode"));

        var data = new WorkloadDashboardResponse(
            Warehouse: new WarehouseOption(warehouseCode, warehouseCode == "ON" ? "Ontario Warehouse" : "Germany Warehouse"),
            OverdueTasks:
            [
                new("outbound", "Outbound Overdue", 2),
                new("inbound", "Inbound Overdue", 1),
                new("shelving", "Shelving Overdue", 0)
            ],
            TodayOutboundPackages: new StatusSplit(10, 7, 3),
            TodayShelvingSkus: new StatusSplit(8, 5, 3),
            TodayInboundSummary: new TodayInboundSummary(
                TotalCartons: new MetricSplit(100, 60, 40),
                TotalWeightKg: new MetricSplit(1200, 700, 500),
                TotalVolumeM3: new MetricSplit(30, 18, 12),
                TotalUnits: new MetricSplit(1000, 650, 350),
                TotalSkuCount: new MetricSplit(200, 130, 70),
                SeaContainerCount: 8,
                TruckPalletCount: 12),
            TodayInbound:
            [
                new("sea",     new MetricSplit(40, 20, 20), new MetricSplit(500, 250, 250), new MetricSplit(12, 6, 6),    new MetricSplit(400, 200, 200), new MetricSplit(80, 40, 40),  SeaContainerCount: 8,    TruckPalletCount: null),
                new("truck",   new MetricSplit(30, 20, 10), new MetricSplit(350, 220, 130), new MetricSplit(9, 6, 3),     new MetricSplit(320, 240, 80),  new MetricSplit(70, 50, 20), SeaContainerCount: null, TruckPalletCount: 12),
                new("express", new MetricSplit(20, 15, 5),  new MetricSplit(220, 170, 50),  new MetricSplit(6, 4.5, 1.5), new MetricSplit(200, 160, 40),  new MetricSplit(40, 30, 10), SeaContainerCount: null, TruckPalletCount: null),
                new("air",     new MetricSplit(10, 5, 5),   new MetricSplit(130, 60, 70),   new MetricSplit(3, 1.5, 1.5), new MetricSplit(80, 50, 30),   new MetricSplit(10, 10, 0),  SeaContainerCount: null, TruckPalletCount: null)
            ],
            TomorrowInboundSummary: new TomorrowInboundSummary(120, 1400, 35, TotalUnits: 1200, TotalSkuCount: 220, SeaContainerCount: 9, TruckPalletCount: 29),
            TomorrowInbound:
            [
                new("sea",   60, 700, 18, TotalUnits: 600, TotalSkuCount: 100, SeaContainerCount: 9,    TruckPalletCount: null),
                new("truck", 60, 700, 17, TotalUnits: 600, TotalSkuCount: 120, SeaContainerCount: null, TruckPalletCount: 29)
            ],
            FutureInboundForecast: GenerateWorkdayForecast(7));

        return Ok(ApiResponse<WorkloadDashboardResponse>.Ok(data));
    }

    [HttpGet("future-inbound-volume")]
    public IActionResult GetFutureInboundVolume(
        [FromQuery] string warehouseCode,
        [FromQuery] string dateStart,
        [FromQuery] string dateEnd,
        [FromQuery] string? transportMode = null)
    {
        if (!IsSupportedWarehouse(warehouseCode))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "Unsupported warehouseCode"));

        if (!DateOnly.TryParse(dateStart, out var startDate) || !DateOnly.TryParse(dateEnd, out var endDate))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart/dateEnd must be yyyy-MM-dd"));

        if (startDate > endDate)
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart cannot be greater than dateEnd"));

        var dayCount = endDate.DayNumber - startDate.DayNumber + 1;
        var baseCartons = warehouseCode == "ON" ? 42 : 48;
        var items = new List<FutureInboundVolumeItem>(dayCount * 2);

        for (var i = 0; i < dayCount; i++)
        {
            var date = startDate.AddDays(i).ToString("yyyy-MM-dd");

            items.Add(new FutureInboundVolumeItem(
                ArrivalDate: date,
                TransportMode: "sea",
                TotalCartons: baseCartons + i * 2,
                TotalWeightKg: 900 + i * 35,
                TotalVolumeM3: 22 + i * 0.5,
                TotalUnits: 700 + i * 20,
                TotalSkuCount: 110 + i * 3,
                TruckPalletCount: null,
                SeaContainerCount: 2 + (i % 3)));

            items.Add(new FutureInboundVolumeItem(
                ArrivalDate: date,
                TransportMode: "truck",
                TotalCartons: baseCartons - 6 + i,
                TotalWeightKg: 520 + i * 18,
                TotalVolumeM3: 14 + i * 0.3,
                TotalUnits: 420 + i * 15,
                TotalSkuCount: 74 + i * 2,
                TruckPalletCount: 9 + (i % 4),
                SeaContainerCount: null));
        }

        items = items
            .Where(item => string.IsNullOrWhiteSpace(transportMode) || item.TransportMode == transportMode)
            .ToList();

        var data = new FutureInboundVolumeResponse(
            Summary: new FutureInboundVolumeSummary(
                TotalCartons: items.Sum(i => i.TotalCartons),
                TotalWeightKg: items.Sum(i => i.TotalWeightKg),
                TotalVolumeM3: items.Sum(i => i.TotalVolumeM3),
                TotalUnits: items.Sum(i => i.TotalUnits),
                TotalSkuCount: items.Sum(i => i.TotalSkuCount),
                SeaContainerCount: items.Sum(i => i.SeaContainerCount ?? 0),
                TruckPalletCount: items.Sum(i => i.TruckPalletCount ?? 0)),
            Items: items);

        return Ok(ApiResponse<FutureInboundVolumeResponse>.Ok(data));
    }

    [HttpGet("outbound-packages")]
    public IActionResult GetOutboundPackages(
        [FromQuery] string warehouseCode,
        [FromQuery] string dateStart,
        [FromQuery] string dateEnd,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? keyword = null)
    {
        if (!IsSupportedWarehouse(warehouseCode))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "Unsupported warehouseCode"));

        if (!DateOnly.TryParse(dateStart, out var startDate) || !DateOnly.TryParse(dateEnd, out var endDate))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart/dateEnd must be yyyy-MM-dd"));

        if (startDate > endDate)
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart cannot be greater than dateEnd"));

        var dayStart = startDate.ToDateTime(TimeOnly.MinValue);
        var warehousePrefix = warehouseCode == "ON" ? "ON" : "DE";
        var items = Enumerable.Range(1, 8).Select(i =>
        {
            var pending = i % 3 != 0;
            return new OutboundPackageDetailItem(
                OrderId: $"{warehousePrefix}-SO202605{i:000}",
                Customer: i % 2 == 0 ? "CustomerA" : "CustomerB",
                LogisticsProvider: i % 2 == 0 ? "DHL" : "UPS",
                TrackingNo: $"TRK{warehousePrefix}{10000 + i}",
                ProductService: i % 2 == 0 ? "Standard" : "Priority",
                ShippingRule: i % 2 == 0 ? "Rule-A" : "Rule-B",
                OrderTime: dayStart.AddHours(i),
                DeadlineAt: dayStart.AddHours(12 + i),
                ActualOutboundTime: pending ? null : dayStart.AddHours(13 + i),
                ProcessingStatus: pending ? "pending" : "processed");
        }).ToList();

        items = items
            .Where(item => MatchProcessingStatus(item.ProcessingStatus, processingStatus))
            .Where(item => string.IsNullOrWhiteSpace(keyword) || item.OrderId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var summary = BuildStatusSplit(items.Select(item => item.ProcessingStatus));
        var data = new OutboundPackageDetailResponse(summary, items);
        return Ok(ApiResponse<OutboundPackageDetailResponse>.Ok(data));
    }

    [HttpGet("skus")]
    public IActionResult GetSkus(
        [FromQuery] string warehouseCode,
        [FromQuery] string dateStart,
        [FromQuery] string dateEnd,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? transportMode = null,
        [FromQuery] string? keyword = null)
    {
        if (!IsSupportedWarehouse(warehouseCode))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "Unsupported warehouseCode"));

        if (!DateOnly.TryParse(dateStart, out var startDate) || !DateOnly.TryParse(dateEnd, out var endDate))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart/dateEnd must be yyyy-MM-dd"));

        if (startDate > endDate)
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart cannot be greater than dateEnd"));

        var dayStart = startDate.ToDateTime(TimeOnly.MinValue);
        var items = Enumerable.Range(1, 10).Select(i =>
        {
            var mod = i % 4;
            var mode = mod switch
            {
                0 => "sea",
                1 => "air",
                2 => "express",
                _ => "truck",
            };
            var processedUnits = i % 3 == 0 ? 0 : 10 + i;
            var totalUnits = 20 + i;
            var pendingUnits = totalUnits - processedUnits;
            return new SkuDetailItem(
                InventoryCode: $"INV-{warehouseCode}-{i:000}",
                Sku: $"SKU-{i:000}",
                Customer: i % 2 == 0 ? "CustomerA" : "CustomerB",
                CartonId: $"{warehouseCode}-CTN-{i:000}",
                AsnId: $"{warehouseCode}-ASN-{i:000}",
                TransportMode: mode,
                TotalUnits: totalUnits,
                ProcessedUnits: processedUnits,
                PendingUnits: pendingUnits,
                ArrivalTime: dayStart.AddHours(i),
                DeadlineAt: dayStart.AddHours(18 + i),
                ActualShelvedAt: pendingUnits == 0 ? dayStart.AddHours(20 + i) : null,
                ProcessingStatus: pendingUnits == 0 ? "processed" : "pending");
        }).ToList();

        items = items
            .Where(item => MatchProcessingStatus(item.ProcessingStatus, processingStatus))
            .Where(item => string.IsNullOrWhiteSpace(transportMode) || item.TransportMode == transportMode)
            .Where(item => string.IsNullOrWhiteSpace(keyword)
                || item.InventoryCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Sku.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.CartonId.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.AsnId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var summary = BuildStatusSplit(items.Select(item => item.ProcessingStatus));
        var data = new SkuDetailResponse(summary, items);
        return Ok(ApiResponse<SkuDetailResponse>.Ok(data));
    }

    [HttpGet("cartons")]
    public IActionResult GetCartons(
        [FromQuery] string warehouseCode,
        [FromQuery] string dateStart,
        [FromQuery] string dateEnd,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? transportMode = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string context = "today")
    {
        if (!IsSupportedWarehouse(warehouseCode))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "Unsupported warehouseCode"));

        if (!DateOnly.TryParse(dateStart, out var startDate) || !DateOnly.TryParse(dateEnd, out var endDate))
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart/dateEnd must be yyyy-MM-dd"));

        if (startDate > endDate)
            return BadRequest(ApiResponse<object>.Fail("BAD_REQUEST", "dateStart cannot be greater than dateEnd"));

        var dayStart = startDate.ToDateTime(TimeOnly.MinValue);
        var items = Enumerable.Range(1, 12).Select(i =>
        {
            var mod = i % 4;
            var mode = mod switch
            {
                0 => "sea",
                1 => "air",
                2 => "express",
                _ => "truck",
            };
            var pending = i % 4 == 0 || (context == "tomorrow" && i % 2 == 0);
            return new CartonDetailItem(
                CartonId: $"{warehouseCode}-CTN-{i:000}",
                AsnId: $"{warehouseCode}-ASN-{i:000}",
                TransportMode: mode,
                EtaAt: dayStart.AddHours(i),
                ArrivedAt: pending ? null : dayStart.AddHours(i + 1),
                TotalWeightKg: 30 + i * 1.3,
                TotalVolumeM3: 1.5 + i * 0.1,
                TotalUnits: 80 + i * 3,
                TotalSkuCount: 18 + i,
                ProcessingStatus: pending ? "pending" : "processed");
        }).ToList();

        items = items
            .Where(item => MatchProcessingStatus(item.ProcessingStatus, processingStatus))
            .Where(item => string.IsNullOrWhiteSpace(transportMode) || item.TransportMode == transportMode)
            .Where(item => string.IsNullOrWhiteSpace(keyword)
                || item.CartonId.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.AsnId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var summary = new CartonDetailSummary(
            Total: items.Count,
            Processed: items.Count(item => item.ProcessingStatus == "processed"),
            Pending: items.Count(item => item.ProcessingStatus == "pending"),
            TotalWeightKg: items.Sum(item => item.TotalWeightKg),
            TotalVolumeM3: items.Sum(item => item.TotalVolumeM3));

        var data = new CartonDetailResponse(summary, items);
        return Ok(ApiResponse<CartonDetailResponse>.Ok(data));
    }

    private static bool IsSupportedWarehouse(string warehouseCode) =>
        warehouseCode is "DE" or "ON";

    private static IReadOnlyList<FutureInboundForecastBar> GenerateWorkdayForecast(int count)
    {
        var result = new List<FutureInboundForecastBar>(count);
        var day = DateOnly.FromDateTime(DateTime.Today).AddDays(2); // 从后天开始
        var baseCartons = 100;
        for (var i = 0; result.Count < count; i++, day = day.AddDays(1))
        {
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;
            result.Add(new FutureInboundForecastBar(
                ArrivalDate: day.ToString("yyyy-MM-dd"),
                TotalCartons: baseCartons + i * 10,
                TotalWeightKg: (baseCartons + i * 10) * 12,
                TotalVolumeM3: (baseCartons + i * 10) / 4,
                SeaContainerCount: 3 + i,
                TruckPalletCount: 8 + i));
        }
        return result;
    }

    private static bool MatchProcessingStatus(string actual, string filter) =>
        filter switch
        {
            "processed" => actual == "processed",
            "pending" => actual == "pending",
            _ => true,
        };

    private static StatusSplit BuildStatusSplit(IEnumerable<string> statuses)
    {
        var array = statuses.ToArray();
        return new StatusSplit(
            Total: array.Length,
            Processed: array.Count(status => status == "processed"),
            Pending: array.Count(status => status == "pending"));
    }
}
