using System.Security.Cryptography;
using System.Text;
using Npgsql;
using SanSo.Api.Modules;

namespace SanSo.Api.V6;

public sealed class PostgresNotificationStoreV1(NpgsqlDataSource dataSource)
{
    public async Task<NotificationDelivery> RaiseInApp(string tenant, NotificationType type, string resourceRef, DateTimeOffset windowStart, CancellationToken ct)
    {
        var tenantId = ParseTenant(tenant);
        if (string.IsNullOrWhiteSpace(resourceRef) || resourceRef.Length > 100) throw new ArgumentException("RESOURCE_REF_INVALID");
        var dedupe = Hash($"{tenant}|{type}|{resourceRef}|{windowStart:yyyyMMddHH}");
        var copy = Copy(type);
        await using var c = await Open(tenantId, ct);
        await using var q = c.CreateCommand();
        q.CommandText = """
            INSERT INTO notification_deliveries(organization_id,channel,recipient_masked,status,attempt,next_attempt_at,dedupe_key,notification_type,resource_ref,title,body)
            VALUES($1,'IN_APP','authorized-members','PENDING',0,now(),$2,$3,$4,$5,$6)
            ON CONFLICT (organization_id,dedupe_key,channel) WHERE dedupe_key IS NOT NULL DO UPDATE SET dedupe_key=EXCLUDED.dedupe_key
            RETURNING id::text,notification_type,channel,recipient_masked,resource_ref,dedupe_key,title,body,status,attempt,next_attempt_at,last_error_code,created_at
            """;
        q.Parameters.AddWithValue(tenantId);
        q.Parameters.AddWithValue(dedupe);
        q.Parameters.AddWithValue(Type(type));
        q.Parameters.AddWithValue(resourceRef);
        q.Parameters.AddWithValue(copy.Title);
        q.Parameters.AddWithValue(copy.Body);
        await using var r = await q.ExecuteReaderAsync(ct);
        await r.ReadAsync(ct);
        return Read(tenant, r);
    }

    public async Task<IReadOnlyList<NotificationDelivery>> List(string tenant, CancellationToken ct)
    {
        var tenantId = ParseTenant(tenant);
        await using var c = await Open(tenantId, ct);
        await using var q = c.CreateCommand();
        q.CommandText = """
            SELECT id::text,notification_type,channel,recipient_masked,resource_ref,dedupe_key,title,body,status,attempt,next_attempt_at,last_error_code,created_at
            FROM notification_deliveries WHERE organization_id=$1 AND channel='IN_APP' ORDER BY created_at DESC
            """;
        q.Parameters.AddWithValue(tenantId);
        await using var r = await q.ExecuteReaderAsync(ct);
        var items = new List<NotificationDelivery>();
        while (await r.ReadAsync(ct)) items.Add(Read(tenant, r));
        return items;
    }

    public async Task<NotificationDelivery> Acknowledge(string tenant, string id, CancellationToken ct)
    {
        var tenantId = ParseTenant(tenant);
        if (!Guid.TryParse(id, out var notificationId)) throw new KeyNotFoundException("NOTIFICATION_NOT_FOUND");
        await using var c = await Open(tenantId, ct);
        await using var q = c.CreateCommand();
        q.CommandText = """
            UPDATE notification_deliveries SET status='ACKNOWLEDGED',acknowledged_at=COALESCE(acknowledged_at,now())
            WHERE organization_id=$2 AND id=$1 AND channel='IN_APP'
            RETURNING id::text,notification_type,channel,recipient_masked,resource_ref,dedupe_key,title,body,status,attempt,next_attempt_at,last_error_code,created_at
            """;
        q.Parameters.AddWithValue(notificationId);
        q.Parameters.AddWithValue(tenantId);
        await using var r = await q.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) throw new KeyNotFoundException("NOTIFICATION_NOT_FOUND");
        return Read(tenant, r);
    }

    private async Task<NpgsqlConnection> Open(Guid tenant, CancellationToken ct)
    {
        var c = await dataSource.OpenConnectionAsync(ct);
        await using var q = c.CreateCommand();
        q.CommandText = "SELECT set_config('app.current_organization_id',$1,false)";
        q.Parameters.AddWithValue(tenant.ToString());
        await q.ExecuteNonQueryAsync(ct);
        return c;
    }

    private static NotificationDelivery Read(string tenant, NpgsqlDataReader r) => new(
        r.GetString(0), tenant, Enum.Parse<NotificationType>(r.GetString(1), true),
        r.GetString(2) == "IN_APP" ? DeliveryChannel.InApp : DeliveryChannel.Email,
        r.GetString(3), r.GetString(4), r.GetString(5), r.GetString(6), r.GetString(7),
        Enum.Parse<DeliveryStatus>(ToPascal(r.GetString(8))), r.GetInt32(9), r.GetFieldValue<DateTimeOffset>(10),
        r.IsDBNull(11) ? null : r.GetString(11), r.GetFieldValue<DateTimeOffset>(12));

    private static Guid ParseTenant(string tenant) => Guid.TryParse(tenant, out var id) ? id : throw new UnauthorizedAccessException("TENANT_INVALID");
    private static string Type(NotificationType type) => type.ToString().ToUpperInvariant();
    private static string ToPascal(string value) => string.Concat(value.ToLowerInvariant().Split('_').Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static (string Title, string Body) Copy(NotificationType type) => type switch
    {
        NotificationType.SyncFailure => ("Ă„ÂĂ¡Â»â€œng bĂ¡Â»â„¢ cĂ¡ÂºÂ§n xĂ¡Â»Â­ lÄ‚Â½", "KĂ¡ÂºÂ¿t nĂ¡Â»â€˜i Ă„â€˜ang giÄ‚Â¡n Ă„â€˜oĂ¡ÂºÂ¡n. MĂ¡Â»Å¸ Trung tÄ‚Â¢m tÄ‚Â­ch hĂ¡Â»Â£p Ă„â€˜Ă¡Â»Æ’ xem mÄ‚Â£ lĂ¡Â»â€”i an toÄ‚Â n vÄ‚Â  thao tÄ‚Â¡c khÄ‚Â´i phĂ¡Â»Â¥c."),
        NotificationType.LargeDiscrepancy => ("CÄ‚Â³ chÄ‚Âªnh lĂ¡Â»â€¡ch Ă„â€˜Ă¡Â»â€˜i soÄ‚Â¡t lĂ¡Â»â€ºn", "MĂ¡Â»â„¢t kĂ¡Â»Â³ thanh toÄ‚Â¡n vĂ†Â°Ă¡Â»Â£t ngĂ†Â°Ă¡Â»Â¡ng cĂ¡ÂºÂ£nh bÄ‚Â¡o. MĂ¡Â»Å¸ chi tiĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»Æ’ truy Ă„â€˜Ă¡ÂºÂ¿n Ă„â€˜Ă†Â¡n vÄ‚Â  dÄ‚Â²ng giao dĂ¡Â»â€¹ch nguĂ¡Â»â€œn."),
        NotificationType.LowStock => ("TĂ¡Â»â€œn khĂ¡ÂºÂ£ dĂ¡Â»Â¥ng sĂ¡ÂºÂ¯p hĂ¡ÂºÂ¿t", "MĂ¡Â»â„¢t SKU Ă„â€˜Ä‚Â£ xuĂ¡Â»â€˜ng dĂ†Â°Ă¡Â»â€ºi ngĂ†Â°Ă¡Â»Â¡ng cĂ¡ÂºÂ£nh bÄ‚Â¡o. KiĂ¡Â»Æ’m tra tĂ¡Â»â€œn thĂ¡Â»Â±c, tĂ¡Â»â€œn giĂ¡Â»Â¯ chĂ¡Â»â€” vÄ‚Â  quarantine trĂ†Â°Ă¡Â»â€ºc khi hÄ‚Â nh Ă„â€˜Ă¡Â»â„¢ng."),
        NotificationType.PeriodNeedsReview => ("KĂ¡Â»Â³ cĂ¡ÂºÂ§n hoÄ‚Â n tĂ¡ÂºÂ¥t", "KĂ¡Â»Â³ hiĂ¡Â»â€¡n cÄ‚Â²n ngoĂ¡ÂºÂ¡i lĂ¡Â»â€¡ cĂ¡ÂºÂ§n xem xÄ‚Â©t. SÄ‚Â nSĂ¡Â»â€¢ khÄ‚Â´ng tĂ¡Â»Â± nĂ¡Â»â„¢p hoĂ¡ÂºÂ·c Ă„â€˜iĂ¡Â»Âu chĂ¡Â»â€°nh hĂ¡Â»â€œ sĂ†Â¡ khi chĂ†Â°a cÄ‚Â³ xÄ‚Â¡c nhĂ¡ÂºÂ­n."),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
