using Npgsql;
using SanSo.Worker;

var connectionString=Environment.GetEnvironmentVariable("SANSO_POSTGRES")??throw new InvalidOperationException("SANSO_POSTGRES is required");
var tenant=Environment.GetEnvironmentVariable("SANSO_WORKER_TENANT")??throw new InvalidOperationException("SANSO_WORKER_TENANT is required; worker is tenant-scoped and must not use BYPASS RLS");
await using var dataSource=NpgsqlDataSource.Create(connectionString);var processor=new OutboxProcessor(new PostgresOutboxStore(dataSource,tenant),new PilotOutboxHandler());
var recovered=await processor.Recover();Console.WriteLine($"OUTBOX_RECOVERED count={recovered} tenant_fingerprint={Fingerprint(tenant)}");
using var stop=new CancellationTokenSource();Console.CancelKeyPress+=(_,eventArgs)=>{eventArgs.Cancel=true;stop.Cancel();};
while(!stop.IsCancellationRequested){try{if(!await processor.Tick(stop.Token))await Task.Delay(TimeSpan.FromSeconds(2),stop.Token);}catch(OperationCanceledException)when(stop.IsCancellationRequested){break;}}
static string Fingerprint(string value)=>Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..12];
