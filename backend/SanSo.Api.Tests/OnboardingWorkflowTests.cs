using SanSo.Api.Modules;
using Xunit;

namespace SanSo.Api.Tests;

public sealed class OnboardingWorkflowTests
{
    [Fact]
    public void FullDemoJourneyCompletesOnlyAfterActivationResult()
    {
        var workflow = new OnboardingWorkflow();
        workflow.Start("tenant-a");
        workflow.SaveBusinessProfile("tenant-a", Profile());
        workflow.SelectDataSource("tenant-a", SourceMode.Demo);
        workflow.SelectBackfill("tenant-a", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));
        workflow.ConfirmSkuMapping("tenant-a", 2);
        workflow.SaveOpeningBalances("tenant-a", [new("SKU-01", 10, 125_000)]);
        workflow.ConfirmTaxDisclaimer("tenant-a", "tax-support-disclaimer-v1", true);
        Assert.Equal(OnboardingStep.FirstReconciliation, workflow.Get("tenant-a").CurrentStep);
        Assert.Equal("ACTIVATION_RESULT_REQUIRED", Assert.Throws<OnboardingValidationException>(() => workflow.CompleteFirstReconciliation("tenant-a", "rec-01", false)).Message);
        var completed = workflow.CompleteFirstReconciliation("tenant-a", "rec-01", true);
        Assert.Equal(OnboardingStep.Completed, completed.CurrentStep);
        Assert.Equal("rec-01", completed.FirstReconciliationId);
    }

    [Fact]
    public void CannotSkipStepsOrConfirmDisclaimerImplicitly()
    {
        var workflow = new OnboardingWorkflow();
        workflow.Start("tenant-a");
        Assert.Equal("ONBOARDING_STEP_OUT_OF_ORDER", Assert.Throws<OnboardingValidationException>(() => workflow.SelectDataSource("tenant-a", SourceMode.Demo)).Message);
        workflow.SaveBusinessProfile("tenant-a", Profile());
        workflow.SelectDataSource("tenant-a", SourceMode.Csv);
        workflow.SelectBackfill("tenant-a", DateOnly.FromDateTime(DateTime.UtcNow));
        workflow.ConfirmSkuMapping("tenant-a", 0);
        workflow.SaveOpeningBalances("tenant-a", []);
        Assert.Equal("DISCLAIMER_CONFIRMATION_REQUIRED", Assert.Throws<OnboardingValidationException>(() => workflow.ConfirmTaxDisclaimer("tenant-a", "v1", false)).Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("012345678A")]
    public void TaxIdentifierValidationChecksFormatWithoutClaimingLegalValidity(string value)
    {
        var workflow = new OnboardingWorkflow();
        workflow.Start("tenant-a");
        var profile = Profile() with { TaxIdentifier = value };
        Assert.Equal("TAX_IDENTIFIER_FORMAT_INVALID", Assert.Throws<OnboardingValidationException>(() => workflow.SaveBusinessProfile("tenant-a", profile)).Message);
    }

    [Fact]
    public void TenantDraftsAreIsolated()
    {
        var workflow = new OnboardingWorkflow();
        workflow.Start("tenant-a");
        workflow.Start("tenant-b");
        workflow.SaveBusinessProfile("tenant-a", Profile());
        Assert.Equal(OnboardingStep.DataSource, workflow.Get("tenant-a").CurrentStep);
        Assert.Equal(OnboardingStep.BusinessProfile, workflow.Get("tenant-b").CurrentStep);
        Assert.Null(workflow.Get("tenant-b").BusinessProfile);
    }

    [Fact]
    public void InvalidMoneyInventoryAndDuplicateSkuAreRejected()
    {
        var workflow = AtOpeningBalances();
        Assert.Equal("OPENING_BALANCE_INVALID", Assert.Throws<OnboardingValidationException>(() => workflow.SaveOpeningBalances("tenant-a", [new("SKU", -1, 10)])).Message);
        Assert.Equal("OPENING_BALANCE_INVALID", Assert.Throws<OnboardingValidationException>(() => workflow.SaveOpeningBalances("tenant-a", [new("SKU", 1, -10)])).Message);
        Assert.Equal("DUPLICATE_CANONICAL_SKU", Assert.Throws<OnboardingValidationException>(() => workflow.SaveOpeningBalances("tenant-a", [new("SKU", 1, 10), new("sku", 2, 20)])).Message);
    }

    private static BusinessProfileDraft Profile() => new(BusinessSubjectType.HouseholdBusiness, "Hộ kinh doanh Demo An Nhiên", "0123456789", "Địa chỉ hoàn toàn giả, Việt Nam");
    private static OnboardingWorkflow AtOpeningBalances()
    {
        var workflow = new OnboardingWorkflow();
        workflow.Start("tenant-a");
        workflow.SaveBusinessProfile("tenant-a", Profile());
        workflow.SelectDataSource("tenant-a", SourceMode.Demo);
        workflow.SelectBackfill("tenant-a", DateOnly.FromDateTime(DateTime.UtcNow));
        workflow.ConfirmSkuMapping("tenant-a", 1);
        return workflow;
    }
}
