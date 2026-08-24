using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace SanSo.Import;

public sealed record StagedImport(string PreviewToken, DateTimeOffset ExpiresAt, ImportPreview Preview);
public sealed record ConfirmedImport(string Checksum, int AcceptedRows, int RejectedRows, bool Duplicate, IReadOnlyList<ConfirmedRawEvent> Events);
public sealed record ConfirmedRawEvent(string EventId, string EventType, string SchemaVersion, string Payload);

public sealed class ImportConfirmationWorkflow(TimeProvider? clock = null)
{
    private readonly TimeProvider clock = clock ?? TimeProvider.System;
    private readonly ConcurrentDictionary<string, Pending> pending = new();
    private readonly ConcurrentDictionary<string, byte> committed = new();

    public StagedImport Stage(string tenant, ImportPreview preview)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var expiresAt = clock.GetUtcNow().AddMinutes(30);
        pending[token] = new(tenant, preview, expiresAt);
        return new(token, expiresAt, preview);
    }

    public ConfirmedImport Confirm(string tenant, string previewToken, string checksum)
    {
        if (!pending.TryGetValue(previewToken, out var item)) throw new InvalidOperationException("PREVIEW_NOT_FOUND");
        if (!CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(item.Tenant), System.Text.Encoding.UTF8.GetBytes(tenant)))
            throw new UnauthorizedAccessException("PREVIEW_TENANT_MISMATCH");
        if (item.ExpiresAt <= clock.GetUtcNow()) { pending.TryRemove(previewToken, out _); throw new InvalidOperationException("PREVIEW_EXPIRED"); }
        if (!string.Equals(item.Preview.Checksum, checksum, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("CHECKSUM_MISMATCH");
        if (!pending.TryRemove(previewToken, out _)) throw new InvalidOperationException("PREVIEW_ALREADY_CONFIRMED");
        var key = $"{tenant}|{item.Preview.Checksum}";
        var duplicate = !committed.TryAdd(key, 0);
        if (duplicate) return new(item.Preview.Checksum, 0, item.Preview.Rows.Count, true, []);
        var events = item.Preview.Rows.Where(row => row.Errors.Count == 0).Select(row => new ConfirmedRawEvent(
            $"file:{item.Preview.Checksum}:row:{row.RowNumber}",
            "ORDER_IMPORTED",
            item.Preview.TemplateVersion,
            JsonSerializer.Serialize(new { row.OrderCode, row.Amount, row.OccurredAt, row.Raw }))).ToList();
        return new(item.Preview.Checksum, events.Count, item.Preview.Rows.Count - events.Count, false, events);
    }

    private sealed record Pending(string Tenant, ImportPreview Preview, DateTimeOffset ExpiresAt);
}
