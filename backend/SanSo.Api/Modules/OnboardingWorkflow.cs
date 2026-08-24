using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace SanSo.Api.Modules;

public enum OnboardingStep { BusinessProfile = 1, DataSource = 2, Backfill = 3, SkuMapping = 4, OpeningBalances = 5, TaxDisclaimer = 6, FirstReconciliation = 7, Completed = 8 }
public enum BusinessSubjectType { Individual, HouseholdBusiness, MicroEnterprise, Company, OtherNeedsReview }
public enum SourceMode { Demo, Csv, Xlsx, OfficialApiPendingAuthorization }

public sealed record BusinessProfileDraft(BusinessSubjectType SubjectType, string LegalName, string TaxIdentifier, string Address, string Currency = "VND", string TimeZone = "Asia/Ho_Chi_Minh");
public sealed record OpeningBalanceDraft(string CanonicalSku, int OnHand, long? UnitCostMinor);
public sealed record OnboardingSnapshot(
    string TenantId,
    OnboardingStep CurrentStep,
    BusinessProfileDraft? BusinessProfile,
    SourceMode? SourceMode,
    DateOnly? BackfillFrom,
    int MappedSkuCount,
    IReadOnlyList<OpeningBalanceDraft> OpeningBalances,
    string? DisclaimerVersion,
    DateTimeOffset? DisclaimerConfirmedAt,
    string? FirstReconciliationId,
    DateTimeOffset UpdatedAt);

public sealed class OnboardingValidationException(string code) : Exception(code);

public sealed class OnboardingWorkflow(TimeProvider? timeProvider = null)
{
    private static readonly Regex TaxIdentifierFormat = new("^(?:[0-9]{10}|[0-9]{13})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly ConcurrentDictionary<string, OnboardingSnapshot> snapshots = new();
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;

    public OnboardingSnapshot Start(string tenant) => snapshots.GetOrAdd(tenant, id => Empty(id));

    public OnboardingSnapshot SaveBusinessProfile(string tenant, BusinessProfileDraft profile)
    {
        RequireStep(tenant, OnboardingStep.BusinessProfile);
        if (string.IsNullOrWhiteSpace(profile.LegalName)) throw new OnboardingValidationException("LEGAL_NAME_REQUIRED");
        if (!TaxIdentifierFormat.IsMatch(profile.TaxIdentifier)) throw new OnboardingValidationException("TAX_IDENTIFIER_FORMAT_INVALID");
        if (string.IsNullOrWhiteSpace(profile.Address)) throw new OnboardingValidationException("ADDRESS_REQUIRED");
        if (profile.Currency != "VND") throw new OnboardingValidationException("MVP_CURRENCY_UNSUPPORTED");
        if (profile.TimeZone != "Asia/Ho_Chi_Minh") throw new OnboardingValidationException("MVP_TIMEZONE_UNSUPPORTED");
        return Save(tenant, current => current with { BusinessProfile = profile, CurrentStep = OnboardingStep.DataSource });
    }

    public OnboardingSnapshot SelectDataSource(string tenant, SourceMode mode)
    {
        RequireStep(tenant, OnboardingStep.DataSource);
        return Save(tenant, current => current with { SourceMode = mode, CurrentStep = OnboardingStep.Backfill });
    }

    public OnboardingSnapshot SelectBackfill(string tenant, DateOnly from)
    {
        RequireStep(tenant, OnboardingStep.Backfill);
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        if (from > today) throw new OnboardingValidationException("BACKFILL_FUTURE_DATE");
        if (from < today.AddYears(-2)) throw new OnboardingValidationException("BACKFILL_RANGE_EXCEEDS_MVP");
        return Save(tenant, current => current with { BackfillFrom = from, CurrentStep = OnboardingStep.SkuMapping });
    }

    public OnboardingSnapshot ConfirmSkuMapping(string tenant, int mappedSkuCount)
    {
        RequireStep(tenant, OnboardingStep.SkuMapping);
        if (mappedSkuCount < 0) throw new OnboardingValidationException("SKU_COUNT_INVALID");
        return Save(tenant, current => current with { MappedSkuCount = mappedSkuCount, CurrentStep = OnboardingStep.OpeningBalances });
    }

    public OnboardingSnapshot SaveOpeningBalances(string tenant, IReadOnlyList<OpeningBalanceDraft> balances)
    {
        RequireStep(tenant, OnboardingStep.OpeningBalances);
        if (balances.Any(x => string.IsNullOrWhiteSpace(x.CanonicalSku) || x.OnHand < 0 || x.UnitCostMinor < 0))
            throw new OnboardingValidationException("OPENING_BALANCE_INVALID");
        if (balances.GroupBy(x => x.CanonicalSku, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new OnboardingValidationException("DUPLICATE_CANONICAL_SKU");
        return Save(tenant, current => current with { OpeningBalances = balances.ToList(), CurrentStep = OnboardingStep.TaxDisclaimer });
    }

    public OnboardingSnapshot ConfirmTaxDisclaimer(string tenant, string disclaimerVersion, bool explicitlyConfirmed)
    {
        RequireStep(tenant, OnboardingStep.TaxDisclaimer);
        if (!explicitlyConfirmed) throw new OnboardingValidationException("DISCLAIMER_CONFIRMATION_REQUIRED");
        if (string.IsNullOrWhiteSpace(disclaimerVersion)) throw new OnboardingValidationException("DISCLAIMER_VERSION_REQUIRED");
        return Save(tenant, current => current with { DisclaimerVersion = disclaimerVersion, DisclaimerConfirmedAt = clock.GetUtcNow(), CurrentStep = OnboardingStep.FirstReconciliation });
    }

    public OnboardingSnapshot CompleteFirstReconciliation(string tenant, string reconciliationId, bool hasMatchedOrExplainedDiscrepancy)
    {
        RequireStep(tenant, OnboardingStep.FirstReconciliation);
        if (string.IsNullOrWhiteSpace(reconciliationId)) throw new OnboardingValidationException("RECONCILIATION_ID_REQUIRED");
        if (!hasMatchedOrExplainedDiscrepancy) throw new OnboardingValidationException("ACTIVATION_RESULT_REQUIRED");
        return Save(tenant, current => current with { FirstReconciliationId = reconciliationId, CurrentStep = OnboardingStep.Completed });
    }

    public OnboardingSnapshot Get(string tenant) => snapshots.TryGetValue(tenant, out var value) ? value : throw new KeyNotFoundException("ONBOARDING_NOT_FOUND");

    private OnboardingSnapshot RequireStep(string tenant, OnboardingStep expected)
    {
        var current = Start(tenant);
        if (current.CurrentStep != expected) throw new OnboardingValidationException("ONBOARDING_STEP_OUT_OF_ORDER");
        return current;
    }

    private OnboardingSnapshot Save(string tenant, Func<OnboardingSnapshot, OnboardingSnapshot> update)
    {
        while (true)
        {
            var current = Get(tenant);
            var next = update(current) with { UpdatedAt = clock.GetUtcNow() };
            if (snapshots.TryUpdate(tenant, next, current)) return next;
        }
    }

    private OnboardingSnapshot Empty(string tenant) => new(tenant, OnboardingStep.BusinessProfile, null, null, null, 0, [], null, null, null, clock.GetUtcNow());
}
