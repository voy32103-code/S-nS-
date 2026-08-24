using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using SanSo.Import;

namespace SanSo.Api.V2;

public sealed record PersistentImportPreview(string BatchId, string PreviewToken, DateTimeOffset ExpiresAt, ImportPreview Preview);
public sealed record PersistentImportConfirmation(string BatchId, string Checksum, int AcceptedRows, int RejectedRows, bool Duplicate);

public sealed class PostgresImportStore(NpgsqlDataSource dataSource)
{
    public async Task<PersistentImportPreview> Stage(string tenant, ImportPreview preview, CancellationToken ct)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var tokenHash = Hash(token);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        await using var connection = await Open(tenant, ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var batch = connection.CreateCommand();
        batch.Transaction = transaction;
        batch.CommandText = """
            INSERT INTO import_batches(organization_id,checksum,token_hash,format,template_version,status,expires_at)
            VALUES($1::uuid,$2,$3,$4,$5,'PREVIEWED',$6) RETURNING id::text
            """;
        batch.Parameters.AddWithValue(tenant);
        batch.Parameters.AddWithValue(preview.Checksum);
        batch.Parameters.AddWithValue(tokenHash);
        batch.Parameters.AddWithValue(preview.Format);
        batch.Parameters.AddWithValue(preview.TemplateVersion);
        batch.Parameters.AddWithValue(expiresAt);
        var batchId = (string)(await batch.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("IMPORT_STAGE_FAILED"));
        foreach (var row in preview.Rows)
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO import_rows(organization_id,batch_id,row_number,event_id,normalized_payload,errors)
                VALUES($1::uuid,$2::uuid,$3,$4,$5::jsonb,$6::jsonb)
                """;
            insert.Parameters.AddWithValue(tenant);
            insert.Parameters.AddWithValue(batchId);
            insert.Parameters.AddWithValue(row.RowNumber);
            insert.Parameters.AddWithValue($"file:{preview.Checksum}:row:{row.RowNumber}");
            insert.Parameters.AddWithValue(JsonSerializer.Serialize(new { row.OrderCode, row.Amount, row.OccurredAt, row.Raw }));
            insert.Parameters.AddWithValue(JsonSerializer.Serialize(row.Errors));
            await insert.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
        return new(batchId, token, expiresAt, preview);
    }

    public async Task<PersistentImportConfirmation> Confirm(string tenant, string previewToken, string checksum, string actorId, CancellationToken ct)
    {
        await using var connection = await Open(tenant, ct);
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await using var select = connection.CreateCommand();
        select.Transaction = transaction;
        select.CommandText = """
            SELECT id::text,checksum,status,expires_at FROM import_batches
            WHERE organization_id=$1::uuid AND token_hash=$2 FOR UPDATE
            """;
        select.Parameters.AddWithValue(tenant);
        select.Parameters.AddWithValue(Hash(previewToken));
        await using var reader = await select.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("PREVIEW_NOT_FOUND");
        var batchId = reader.GetString(0);
        var storedChecksum = reader.GetString(1);
        var status = reader.GetString(2);
        var expiresAt = reader.GetFieldValue<DateTimeOffset>(3);
        await reader.CloseAsync();
        if (!string.Equals(storedChecksum, checksum, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("CHECKSUM_MISMATCH");
        if (status != "PREVIEWED") throw new InvalidOperationException("PREVIEW_ALREADY_CONFIRMED");
        if (expiresAt <= DateTimeOffset.UtcNow) throw new InvalidOperationException("PREVIEW_EXPIRED");
        await using var duplicate = connection.CreateCommand();
        duplicate.Transaction = transaction;
        duplicate.CommandText = "SELECT id::text FROM import_batches WHERE organization_id=$1::uuid AND checksum=$2 AND status='CONFIRMED' AND id<>$3::uuid";
        duplicate.Parameters.AddWithValue(tenant);
        duplicate.Parameters.AddWithValue(checksum);
        duplicate.Parameters.AddWithValue(batchId);
        if (await duplicate.ExecuteScalarAsync(ct) is not null)
        {
            await transaction.RollbackAsync(ct);
            return new(batchId, checksum, 0, 0, true);
        }
        await using var post = connection.CreateCommand();
        post.Transaction = transaction;
        post.CommandText = """
            INSERT INTO raw_events(organization_id,source,source_event_id,event_type,schema_version,payload,checksum)
            SELECT organization_id,'file-import',event_id,'ORDER_IMPORTED','1',normalized_payload,
                   encode(sha256(convert_to(normalized_payload::text,'UTF8')),'hex')
            FROM import_rows WHERE organization_id=$1::uuid AND batch_id=$2::uuid AND errors='[]'::jsonb
            ON CONFLICT(organization_id,source,source_event_id) DO NOTHING;
            UPDATE import_batches SET status='CONFIRMED',confirmed_at=now(),confirmed_by=$3::uuid
            WHERE organization_id=$1::uuid AND id=$2::uuid;
            """;
        post.Parameters.AddWithValue(tenant);
        post.Parameters.AddWithValue(batchId);
        post.Parameters.AddWithValue(actorId);
        await post.ExecuteNonQueryAsync(ct);
        await using var count = connection.CreateCommand();
        count.Transaction = transaction;
        count.CommandText = "SELECT count(*) FILTER (WHERE errors='[]'::jsonb),count(*) FILTER (WHERE errors<>'[]'::jsonb) FROM import_rows WHERE organization_id=$1::uuid AND batch_id=$2::uuid";
        count.Parameters.AddWithValue(tenant);
        count.Parameters.AddWithValue(batchId);
        await using var counts = await count.ExecuteReaderAsync(ct);
        await counts.ReadAsync(ct);
        var accepted = counts.GetInt32(0);
        var rejected = counts.GetInt32(1);
        await transaction.CommitAsync(ct);
        return new(batchId, checksum, accepted, rejected, false);
    }

    private async Task<NpgsqlConnection> Open(string tenant, CancellationToken ct)
    {
        var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT set_config('app.current_organization_id',$1,false)";
        command.Parameters.AddWithValue(tenant);
        await command.ExecuteNonQueryAsync(ct);
        return connection;
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
