using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace SanSo.Api.Modules;

public enum NotificationType { SyncFailure, LargeDiscrepancy, LowStock, PeriodNeedsReview }
public enum DeliveryChannel { InApp, Email }
public enum DeliveryStatus { Pending, Delivered, RetryScheduled, DeadLetter, Acknowledged }

public sealed record NotificationDelivery(
    string Id,
    string TenantId,
    NotificationType Type,
    DeliveryChannel Channel,
    string RecipientMasked,
    string ResourceRef,
    string DedupeKey,
    string Title,
    string Body,
    DeliveryStatus Status,
    int Attempt,
    DateTimeOffset NextAttemptAt,
    string? LastErrorCode,
    DateTimeOffset CreatedAt);

public sealed class NotificationCenter(TimeProvider? timeProvider = null)
{
    private readonly ConcurrentDictionary<string, NotificationDelivery> deliveries = new();
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;

    public NotificationDelivery Raise(string tenant, NotificationType type, DeliveryChannel channel, string recipient, string resourceRef, DateTimeOffset windowStart)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant);
        if (string.IsNullOrWhiteSpace(resourceRef) || resourceRef.Length > 100) throw new ArgumentException("RESOURCE_REF_INVALID");
        var dedupe = Hash($"{tenant}|{type}|{resourceRef}|{windowStart:yyyyMMddHH}");
        var key = $"{tenant}|{dedupe}|{channel}";
        return deliveries.GetOrAdd(key, _ =>
        {
            var copy = Copy(type);
            var now = clock.GetUtcNow();
            return new(Guid.NewGuid().ToString(), tenant, type, channel, Mask(recipient, channel), resourceRef, dedupe, copy.Title, copy.Body, DeliveryStatus.Pending, 0, now, null, now);
        });
    }

    public NotificationDelivery RecordSuccess(string tenant, string id)
    {
        var current = Owned(tenant, id);
        return Replace(current, current with { Status = DeliveryStatus.Delivered, Attempt = current.Attempt + 1, LastErrorCode = null });
    }

    public NotificationDelivery RecordFailure(string tenant, string id, string safeErrorCode, bool transient)
    {
        var current = Owned(tenant, id);
        var attempt = current.Attempt + 1;
        var dead = !transient || attempt >= 5;
        var delay = TimeSpan.FromSeconds(Math.Min(900, Math.Pow(2, attempt) * 10));
        return Replace(current, current with
        {
            Status = dead ? DeliveryStatus.DeadLetter : DeliveryStatus.RetryScheduled,
            Attempt = attempt,
            NextAttemptAt = clock.GetUtcNow().Add(delay),
            LastErrorCode = SafeError(safeErrorCode)
        });
    }

    public NotificationDelivery Acknowledge(string tenant, string id)
    {
        var current = Owned(tenant, id);
        if (current.Channel != DeliveryChannel.InApp) throw new InvalidOperationException("ONLY_IN_APP_CAN_BE_ACKNOWLEDGED");
        return Replace(current, current with { Status = DeliveryStatus.Acknowledged });
    }

    public IReadOnlyList<NotificationDelivery> List(string tenant) => deliveries.Values.Where(x => x.TenantId == tenant).OrderByDescending(x => x.CreatedAt).ToList();

    private NotificationDelivery Owned(string tenant, string id) => deliveries.Values.FirstOrDefault(x => x.Id == id && x.TenantId == tenant) ?? throw new KeyNotFoundException("NOTIFICATION_NOT_FOUND");
    private NotificationDelivery Replace(NotificationDelivery current, NotificationDelivery next)
    {
        var key = $"{current.TenantId}|{current.DedupeKey}|{current.Channel}";
        deliveries[key] = next;
        return next;
    }

    private static (string Title, string Body) Copy(NotificationType type) => type switch
    {
        NotificationType.SyncFailure => ("Đồng bộ cần xử lý", "Kết nối đang gián đoạn. Mở Trung tâm tích hợp để xem mã lỗi an toàn và thao tác khôi phục."),
        NotificationType.LargeDiscrepancy => ("Có chênh lệch đối soát lớn", "Một kỳ thanh toán vượt ngưỡng cảnh báo. Mở chi tiết để truy đến đơn và dòng giao dịch nguồn."),
        NotificationType.LowStock => ("Tồn khả dụng sắp hết", "Một SKU đã xuống dưới ngưỡng cảnh báo. Kiểm tra tồn thực, tồn giữ chỗ và quarantine trước khi hành động."),
        NotificationType.PeriodNeedsReview => ("Kỳ cần hoàn tất", "Kỳ hiện còn ngoại lệ cần xem xét. SànSổ không tự nộp hoặc điều chỉnh hồ sơ khi chưa có xác nhận."),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private static string Mask(string value, DeliveryChannel channel)
    {
        if (channel == DeliveryChannel.InApp) return "authorized-members";
        var at = value.IndexOf('@');
        if (at <= 0 || at == value.Length - 1) throw new ArgumentException("EMAIL_INVALID");
        return $"{value[0]}***@{value[(at + 1)..].ToLowerInvariant()}";
    }

    private static string SafeError(string code) => code is "SMTP_TIMEOUT" or "PROVIDER_RATE_LIMIT" or "ADDRESS_REJECTED" ? code : "DELIVERY_FAILED";
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
