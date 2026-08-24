using SanSo.Import;
using Xunit;

namespace SanSo.Import.Tests;

public sealed class ImportConfirmationWorkflowTests
{
    [Fact]
    public void ConfirmCreatesRawEventsOnlyForValidRows()
    {
        var workflow = new ImportConfirmationWorkflow();
        var preview = Preview("abc", [Row(2), Row(3, ["AMOUNT_INVALID"])]);
        var staged = workflow.Stage("tenant-a", preview);
        var result = workflow.Confirm("tenant-a", staged.PreviewToken, "abc");
        Assert.False(result.Duplicate);
        Assert.Equal(1, result.AcceptedRows);
        Assert.Equal(1, result.RejectedRows);
        Assert.Equal("file:abc:row:2", result.Events[0].EventId);
        Assert.Contains("VN-2", result.Events[0].Payload);
    }

    [Fact]
    public void ConfirmRejectsChecksumChangeAndTokenReuse()
    {
        var workflow = new ImportConfirmationWorkflow();
        var staged = workflow.Stage("tenant-a", Preview("abc", [Row(2)]));
        Assert.Equal("CHECKSUM_MISMATCH", Assert.Throws<InvalidOperationException>(() => workflow.Confirm("tenant-a", staged.PreviewToken, "tampered")).Message);
        workflow.Confirm("tenant-a", staged.PreviewToken, "abc");
        Assert.Equal("PREVIEW_NOT_FOUND", Assert.Throws<InvalidOperationException>(() => workflow.Confirm("tenant-a", staged.PreviewToken, "abc")).Message);
    }

    [Fact]
    public void PreviewCannotBeConfirmedByAnotherTenant()
    {
        var workflow = new ImportConfirmationWorkflow();
        var staged = workflow.Stage("tenant-a", Preview("abc", [Row(2)]));
        Assert.Equal("PREVIEW_TENANT_MISMATCH", Assert.Throws<UnauthorizedAccessException>(() => workflow.Confirm("tenant-b", staged.PreviewToken, "abc")).Message);
    }

    [Fact]
    public void RestagingCommittedChecksumDoesNotCreateEventsTwice()
    {
        var workflow = new ImportConfirmationWorkflow();
        var preview = Preview("same", [Row(2)]);
        var first = workflow.Stage("tenant-a", preview);
        Assert.False(workflow.Confirm("tenant-a", first.PreviewToken, "same").Duplicate);
        var second = workflow.Stage("tenant-a", preview);
        var duplicate = workflow.Confirm("tenant-a", second.PreviewToken, "same");
        Assert.True(duplicate.Duplicate);
        Assert.Empty(duplicate.Events);
        Assert.Equal(0, duplicate.AcceptedRows);
    }

    private static ImportPreview Preview(string checksum, IReadOnlyList<ImportRow> rows) =>
        new("CSV", "csv-v1", checksum, ',', ["order_code", "amount", "occurred_at"], rows, [], false);

    private static ImportRow Row(int number, IReadOnlyList<string>? errors = null) =>
        new(number, $"VN-{number}", 100_000, DateTimeOffset.Parse("2026-08-24T09:00:00+07:00"), new Dictionary<string, string>(), errors ?? []);
}
