# Phase 29 — OpenAPI V7 canonical contract

Date: 2026-08-24  
Status: generated and verified.

## Artifacts

- Contract: `docs/openapi-v7-canonical.json`
- Canonical generator: `scripts/generate-openapi-v7-v2.ps1`
- Canonical verifier: `scripts/verify-openapi-v7-v2.ps1`

The earlier `generate-openapi-v7.ps1` and `verify-openapi-v7.ps1` are superseded diagnostic iterations and must not be used by CI.

## V7 contract changes

- canonical entry point is explicitly recorded;
- import preview documents the 10 MiB limit, CSV/XLSX allowlist and UTF-8 requirement;
- durable inventory GET, reserve and release operations are documented;
- inventory snapshot and idempotent mutation schemas are included;
- authentication, authorization, validation, conflict and dependency failure responses are explicit.

## Verification gate

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-openapi-v7-v2.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-openapi-v7-v2.ps1
```

The verifier checks OpenAPI version, API version, canonical entry point, mandatory routes, route presence in source, upload limit, inventory schemas and every internal component reference.
