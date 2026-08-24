# SànSổ completion audit V16

Date: 2026-08-24  
Verdict: **not fully complete**. Durable inventory read/reserve/release is now complete and live-proven.

## Newly proven since V15

| Requirement | Evidence | Status |
|---|---|---|
| PostgreSQL inventory read | API V6 live HTTP test | Proven |
| PostgreSQL reserve with ATP/version/source-key guards | Store V4 and live test | Proven |
| PostgreSQL release guard and idempotency | API V6 live test | Proven |
| Anonymous inventory access rejected in Development and Production composition | live 401 assertion plus `IdentityEndpoints.Require` | Proven |
| Canonical solution V6 | build 0 warnings/errors; aggregate 90/90 | Proven for build/unit scope |

## Canonical stack

- Solution: `SanSo.V6.slnx`
- API: `backend/SanSo.Api.V6`
- Worker: `backend/SanSo.Worker.V2`
- Migrator: `backend/SanSo.Migrator.V4` (001–011)
- Frontend: V8
- CI: `ci-v12-canonical.yml`

## Remaining blockers requiring user/domain input

1. Choose a production identity approach: managed IdP or database-backed in-house password/session storage.
2. Supply expert-approved tax rules and golden expected results; otherwise positive tax amounts must remain unavailable.
3. Supply official sandbox credentials/authorization before enabling marketplace, email, or Zalo adapters.

## Remaining engineering after those decisions

- durable onboarding repository using field encryption/key management;
- durable notification delivery repository/provider;
- browser-to-real-IdP acceptance;
- OpenAPI V6 refresh before external publication;
- settlement/fee ingestion for full reconciliation projection;
- disable/remove superseded CI V10 in repository administration.

