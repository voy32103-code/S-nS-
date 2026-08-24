using SanSo.Api.Modules;
using Xunit;

namespace SanSo.Api.Tests;

public sealed class NotificationCenterTests
{
    [Fact]
    public void SameAlertWindowIsDeduplicatedAndEmailIsMasked()
    {
        var center = new NotificationCenter();
        var window = new DateTimeOffset(2026, 8, 24, 9, 0, 0, TimeSpan.FromHours(7));
        var first = center.Raise("tenant-a", NotificationType.SyncFailure, DeliveryChannel.Email, "owner@example.invalid", "connection-01", window);
        var second = center.Raise("tenant-a", NotificationType.SyncFailure, DeliveryChannel.Email, "owner@example.invalid", "connection-01", window.AddMinutes(30));
        Assert.Equal(first.Id, second.Id);
        Assert.Equal("o***@example.invalid", first.RecipientMasked);
        Assert.DoesNotContain("owner@example", first.RecipientMasked);
    }

    [Fact]
    public void TenantCannotReadOrMutateAnotherTenantNotification()
    {
        var center = new NotificationCenter();
        var delivery = center.Raise("tenant-a", NotificationType.LowStock, DeliveryChannel.InApp, "ignored", "SKU-01", DateTimeOffset.UtcNow);
        Assert.Empty(center.List("tenant-b"));
        Assert.Throws<KeyNotFoundException>(() => center.RecordSuccess("tenant-b", delivery.Id));
        Assert.Throws<KeyNotFoundException>(() => center.Acknowledge("tenant-b", delivery.Id));
    }

    [Fact]
    public void TransientDeliveryRetriesThenDeadLettersAtAttemptFive()
    {
        var center = new NotificationCenter();
        var delivery = center.Raise("tenant-a", NotificationType.PeriodNeedsReview, DeliveryChannel.Email, "finance@example.invalid", "period-2026-08", DateTimeOffset.UtcNow);
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            delivery = center.RecordFailure("tenant-a", delivery.Id, "SMTP_TIMEOUT", true);
            Assert.Equal(DeliveryStatus.RetryScheduled, delivery.Status);
            Assert.Equal(attempt, delivery.Attempt);
        }
        delivery = center.RecordFailure("tenant-a", delivery.Id, "SMTP_TIMEOUT", true);
        Assert.Equal(DeliveryStatus.DeadLetter, delivery.Status);
        Assert.Equal(5, delivery.Attempt);
    }

    [Fact]
    public void UnknownProviderErrorIsNotStoredVerbatim()
    {
        var center = new NotificationCenter();
        var delivery = center.Raise("tenant-a", NotificationType.LargeDiscrepancy, DeliveryChannel.Email, "finance@example.invalid", "settlement-01", DateTimeOffset.UtcNow);
        delivery = center.RecordFailure("tenant-a", delivery.Id, "provider said customer-secret-value", false);
        Assert.Equal("DELIVERY_FAILED", delivery.LastErrorCode);
        Assert.Equal(DeliveryStatus.DeadLetter, delivery.Status);
    }

    [Fact]
    public void OnlyInAppNotificationCanBeAcknowledged()
    {
        var center = new NotificationCenter();
        var email = center.Raise("tenant-a", NotificationType.LowStock, DeliveryChannel.Email, "ops@example.invalid", "SKU-02", DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => center.Acknowledge("tenant-a", email.Id));
        var inApp = center.Raise("tenant-a", NotificationType.LowStock, DeliveryChannel.InApp, "ignored", "SKU-02", DateTimeOffset.UtcNow);
        Assert.Equal(DeliveryStatus.Acknowledged, center.Acknowledge("tenant-a", inApp.Id).Status);
    }
}
