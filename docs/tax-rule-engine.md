# Tax rule engine specification

## Safety invariant

Generative AI never selects a rate, threshold, taxable subject or final amount. The deterministic engine accepts structured input and an `APPROVED` effective-dated rule version. Missing profile/category, no rule, multiple overlapping rules or conflicting evidence returns `NEEDS_REVIEW` with no amount.

## Rule schema

```json
{
  "code": "LEGAL-COUNSEL-ASSIGNED-CODE",
  "version": 1,
  "jurisdiction": "VN",
  "subjectType": "expert-classified subject",
  "channelScope": "verified capability classification",
  "categoryScope": "approved category taxonomy",
  "effectiveFrom": "YYYY-MM-DD",
  "effectiveTo": null,
  "legalSource": "official URL",
  "legalArticle": "exact article/clause",
  "formula": {"operator":"RATE_TIMES_BASIS","rate":null},
  "rounding": "expert-approved explicit policy",
  "requiredInputs": ["taxProfile","category","eventType"],
  "status": "DRAFT",
  "approvedBy": null,
  "approvedAt": null
}
```

The `rate` is intentionally `null`; this sample demonstrates structure without fabricating law. Transition to `APPROVED` is rejected unless rate/formula, official source and human approval are present.

## Selection

Filter by jurisdiction → subject/profile effective on transaction date → channel function/class → event/category → effective date. Require exactly one approved version. Snapshot input IDs/checksum, selected rule version, legal basis, effective date, calculation time, rounding steps and reviewer.

## Reproducibility

Same input snapshot + rule version + engine version must return identical basis/result/explanation. Rule updates create a new version and never rewrite historical calculations. Locked periods require amendment/next-period adjustment.

## Golden release gate

- `negative-cases.json` contains executable no-rate safety cases and is currently green.
- Positive/boundary/rate/threshold cases may only be added after a named tax expert approves exact source/article, input, expected basis/result and explanation.
- Any approved rule change runs all golden cases; CI failure blocks release.
