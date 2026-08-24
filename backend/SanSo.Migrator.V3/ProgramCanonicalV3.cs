using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

var connectionString=Environment.GetEnvironmentVariable("SANSO_POSTGRES")??throw new InvalidOperationException("SANSO_POSTGRES is required; secrets must come from environment/secret manager.");
var migrationPath=Path.Combine(AppContext.BaseDirectory,"Migrations");
await using var connection=new NpgsqlConnection(connectionString);await connection.OpenAsync();
await using(var bootstrap=connection.CreateCommand()){bootstrap.CommandText="SELECT pg_advisory_lock(7482152027); CREATE TABLE IF NOT EXISTS schema_migrations_v3(version text PRIMARY KEY,checksum text NOT NULL,applied_at timestamptz NOT NULL DEFAULT now());";await bootstrap.ExecuteNonQueryAsync();}
try
{
 foreach(var file in Directory.GetFiles(migrationPath,"*.sql").OrderBy(Path.GetFileName,StringComparer.Ordinal))
 {
  var version=Path.GetFileName(file);var original=await File.ReadAllTextAsync(file,Encoding.UTF8);var checksum=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(original))).ToLowerInvariant();
  await using var check=connection.CreateCommand();check.CommandText="SELECT checksum FROM schema_migrations_v3 WHERE version=$1";check.Parameters.AddWithValue(version);var existing=await check.ExecuteScalarAsync()as string;
  if(existing is not null){if(existing!=checksum)throw new InvalidOperationException($"Migration checksum changed after apply: {version}");Console.WriteLine($"SKIP {version}");continue;}
  var sql=RemoveOuterTransaction(original);
  await using var transaction=await connection.BeginTransactionAsync();
  await using var apply=connection.CreateCommand();apply.Transaction=transaction;apply.CommandText=sql;await apply.ExecuteNonQueryAsync();
  await using var record=connection.CreateCommand();record.Transaction=transaction;record.CommandText="INSERT INTO schema_migrations_v3(version,checksum) VALUES($1,$2)";record.Parameters.AddWithValue(version);record.Parameters.AddWithValue(checksum);await record.ExecuteNonQueryAsync();
  await transaction.CommitAsync();Console.WriteLine($"APPLIED {version} {checksum}");
 }
}
finally{await using var unlock=connection.CreateCommand();unlock.CommandText="SELECT pg_advisory_unlock(7482152027)";await unlock.ExecuteNonQueryAsync();}

static string RemoveOuterTransaction(string sql)
{
 var withoutBegin=Regex.Replace(sql,@"\A\s*BEGIN\s*;",string.Empty,RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
 return Regex.Replace(withoutBegin,@"COMMIT\s*;\s*\z",string.Empty,RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
}
