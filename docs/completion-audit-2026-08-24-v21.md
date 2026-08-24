# SànSổ completion audit V21

Date: 2026-08-24  
Verdict: **goal remains active**.

## Newly completed

- Durable AES-256-GCM onboarding repository is now wired to all seven HTTP steps.
- PostgreSQL-without-key behavior is fail-closed and live-tested.
- Restart persistence, masked output, encrypted-at-rest fields and authenticated disclaimer actor are live-tested.
- OpenAPI V9 documents 31 paths and all onboarding operations.
- Canonical backend passes 98/98 tests with 0 build warnings/errors.
- `.env.v9.example` documents required variable names without committing a secret.

## External inputs still required

1. Managed IdP versus in-house identity decision and production configuration.
2. Approved Vietnamese tax rules, legal citations and golden outcomes.
3. Official marketplace/email/Zalo/billing sandbox credentials and scopes.
4. Actual field key and key version injected from the selected production secret manager.
5. Pilot interview and willingness-to-pay evidence.

## Engineering gaps still open

- production identity persistence/real-IdP E2E;
- complete settlement and marketplace fee ingestion;
- full report/export APIs and E2E;
- real provider adapter contract tests;
- Docker runtime verification in an environment with Docker;
- expert-approved tax golden regression suite.

