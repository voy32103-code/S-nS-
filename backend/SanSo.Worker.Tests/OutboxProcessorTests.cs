using SanSo.Worker;
using Xunit;

namespace SanSo.Worker.Tests;

public sealed class OutboxProcessorTests
{
    [Fact]public async Task StartupRecoversExpiredProcessingLease(){var store=new FakeStore();store.RecoverCount=2;var processor=new OutboxProcessor(store,new Handler());Assert.Equal(2,await processor.Recover());Assert.Equal(1,store.RecoverCalls);}
    [Fact]public async Task SuccessfulClaimCompletesExactlyOnce(){var store=new FakeStore(Work(attempt:1));var handler=new Handler();var processor=new OutboxProcessor(store,handler);Assert.True(await processor.Tick());Assert.Equal(1,handler.Calls);Assert.Equal(1,store.Completed);Assert.False(await processor.Tick());}
    [Fact]public async Task TransientFailureRetriesThenDeadLettersAtAttemptFive(){var retryStore=new FakeStore(Work(attempt:4));var retry=new OutboxProcessor(retryStore,new Handler(new OutboxHandlingException("TIMEOUT",true)));await retry.Tick();Assert.False(retryStore.Dead);Assert.Equal("TIMEOUT",retryStore.Error);var deadStore=new FakeStore(Work(attempt:5));var dead=new OutboxProcessor(deadStore,new Handler(new OutboxHandlingException("TIMEOUT",true)));await dead.Tick();Assert.True(deadStore.Dead);}
    [Fact]public async Task UnknownExternalTypeFailsClosedWithoutCallingNetwork(){var store=new FakeStore(Work("SHOPEE_SYNC",1));var processor=new OutboxProcessor(store,new PilotOutboxHandler());await processor.Tick();Assert.True(store.Dead);Assert.Equal("PARTNER_ADAPTER_NOT_CONFIGURED",store.Error);}
    [Fact]public async Task UnexpectedExceptionStoresSafeCodeNotMessage(){var store=new FakeStore(Work(attempt:1));var processor=new OutboxProcessor(store,new Handler(new Exception("customer-secret-payload")));await processor.Tick();Assert.Equal("UNEXPECTED_HANDLER_FAILURE",store.Error);Assert.DoesNotContain("secret",store.Error!);}
    private static OutboxEnvelope Work(string type="NOOP_AUDIT",int attempt=1)=>new(Guid.NewGuid(),"tenant-a",type,"{\"private\":true}",Guid.NewGuid().ToString(),attempt,DateTimeOffset.UtcNow);
    private sealed class Handler(Exception? failure=null):IOutboxHandler{public int Calls;public Task Handle(OutboxEnvelope work,CancellationToken ct){Calls++;return failure is null?Task.CompletedTask:Task.FromException(failure);}}
    private sealed class FakeStore(params OutboxEnvelope[] work):IOutboxStore
    {private readonly Queue<OutboxEnvelope> queue=new(work);public int RecoverCount;public int RecoverCalls;public int Completed;public bool Dead;public string? Error;public Task<int> RecoverExpiredLeases(CancellationToken ct){RecoverCalls++;return Task.FromResult(RecoverCount);}public Task<OutboxEnvelope?> Claim(TimeSpan lease,CancellationToken ct)=>Task.FromResult(queue.Count>0?queue.Dequeue():null);public Task Complete(Guid id,CancellationToken ct){Completed++;return Task.CompletedTask;}public Task Fail(Guid id,WorkerFailure failure,TimeSpan delay,bool deadLetter,CancellationToken ct){Error=failure.Code;Dead=deadLetter;return Task.CompletedTask;}}
}
