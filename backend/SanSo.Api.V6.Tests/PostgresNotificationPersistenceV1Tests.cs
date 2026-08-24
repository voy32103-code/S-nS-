extern alias apiv6;
using Npgsql;
using SanSo.Api.Modules;
using Xunit;
using Store = apiv6::SanSo.Api.V6.PostgresNotificationStoreV1;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresNotificationPersistenceV1Tests
{
    [Fact]
    public async Task InAppDeliveryIsDurableDeduplicatedAcknowledgedAndTenantIsolated()
    {
        var adminCs = Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminCs)) return;
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var role = $"notify_{Guid.NewGuid():N}";
        await SeedAndGrant(adminCs, tenantA, tenantB, role);
        var csb = new NpgsqlConnectionStringBuilder(adminCs) { Username = role };
        await using var source = NpgsqlDataSource.Create(csb.ConnectionString);
        var store = new Store(source);
        var window = new DateTimeOffset(2026, 8, 24, 10, 0, 0, TimeSpan.Zero);

        var first = await store.RaiseInApp(tenantA.ToString(), NotificationType.LowStock, "sku:ABC", window, default);
        var duplicate = await store.RaiseInApp(tenantA.ToString(), NotificationType.LowStock, "sku:ABC", window, default);
        Assert.Equal(first.Id, duplicate.Id);
        Assert.Equal(DeliveryStatus.Pending, first.Status);
        Assert.Equal("authorized-members", first.RecipientMasked);
        Assert.Single(await store.List(tenantA.ToString(), default));
        Assert.Empty(await store.List(tenantB.ToString(), default));

        var acknowledged = await store.Acknowledge(tenantA.ToString(), first.Id, default);
        Assert.Equal(DeliveryStatus.Acknowledged, acknowledged.Status);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => store.Acknowledge(tenantB.ToString(), first.Id, default));

        await using var secondSource = NpgsqlDataSource.Create(csb.ConnectionString);
        var restarted = new Store(secondSource);
        var afterRestart = await restarted.List(tenantA.ToString(), default);
        Assert.Single(afterRestart);
        Assert.Equal(DeliveryStatus.Acknowledged, afterRestart[0].Status);
    }

    private static async Task SeedAndGrant(string cs, Guid tenantA, Guid tenantB, string role)
    {
        await using var c = new NpgsqlConnection(cs);
        await c.OpenAsync();
        await using var q = c.CreateCommand();
        q.CommandText = $"""
            INSERT INTO organizations(id,slug,name) VALUES($1,$2,'Notification A'),($3,$4,'Notification B');
            CREATE ROLE {role} LOGIN;
            GRANT SELECT,INSERT,UPDATE ON notification_deliveries TO {role};
            """;
        q.Parameters.AddWithValue(tenantA);
        q.Parameters.AddWithValue($"notify-a-{tenantA:N}");
        q.Parameters.AddWithValue(tenantB);
        q.Parameters.AddWithValue($"notify-b-{tenantB:N}");
        await q.ExecuteNonQueryAsync();
    }
}
