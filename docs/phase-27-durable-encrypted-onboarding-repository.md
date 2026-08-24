# Phase 27 — Durable encrypted onboarding repository

Date: 2026-08-24  
Status: repository implemented and live-verified; API route wiring pending field-key configuration.

## Implementation

`backend/SanSo.Api.V6/PostgresOnboardingStoreV1.cs` persists the seven-step onboarding state machine in `onboarding_profiles`.

Security and integrity properties:

- tax identifier and address are AES-256-GCM protected with tenant, purpose, and key version in AAD;
- tax identifier reads are masked to last four digits;
- no default/generated production key exists;
- step transitions use `UPDATE ... WHERE current_step=N` and fail atomically when out of order;
- currency/timezone, tax identifier, backfill range, SKU count, balances, disclaimer, and reconciliation activation are validated;
- RLS session context is set before every operation;
- completion requires a real `reconciliation_runs` FK.

## Live verification

An isolated PostgreSQL 17 cluster on port 55435 was migrated through 001–011, tested, stopped, and permanently removed.

The test proved:

- initial step=1 and completion step=8;
- all seven transitions persist;
- a repeated business-profile step raises `ONBOARDING_STEP_OUT_OF_ORDER`;
- tax ID is returned as `******6789`;
- ciphertext columns do not contain the tax ID/address plaintext;
- `field_key_version=test-v1` is stored;
- decrypting tax ciphertext with another tenant fails authentication.

## Remaining wiring

API V6 still exposes the in-memory onboarding composition. A future API composition must register `TenantFieldProtector` from a secret-manager supplied 32-byte key and route onboarding calls to this repository. It must fail startup or return an explicit unavailable state when the key is absent; it must never generate a silent replacement key.

