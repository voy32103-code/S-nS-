# Phase 24 — Unified authenticated operations UI V8

Date: 2026-08-24  
Status: implemented, typechecked, component-tested, browser-render tested, and bundled.

## What changed

V8 replaces the split between the broad unauthenticated V4 operations UI and the narrow authenticated V7 onboarding UI. One in-memory authenticated session now drives:

- dashboard metrics and current reconciliation;
- CSV/XLSX preview and explicit confirmation;
- ordered seven-step onboarding for Owner/Admin;
- tax safety, partial refund, and Copilot safety workflows;
- role-aware locked states for actions the principal cannot perform;
- logout and removal of the in-memory token.

All requests use the tenant returned by the authenticated principal plus the opaque bearer token. There is no synthetic Owner fallback, persistent browser token storage, embedded credential, or tax-rate guess.

## Canonical artifacts

- `frontend/index-v8.html`
- `frontend/main-v8.tsx`
- `frontend/vite.v8.config.js`
- `frontend/tsconfig.v8.json`
- `frontend/src/AppV8.tsx`
- `frontend/src/api-v8.ts`
- `frontend/src/ImportV8.tsx`
- `frontend/src/OnboardingV8.tsx`
- `frontend/src/WorkflowV9.tsx`
- `frontend/src/styles-v8.css`
- `frontend/src/AppV8.test.tsx`
- `e2e/tests/authenticated-ui-v8.spec.ts`
- `e2e/playwright.auth-ui-v8.config.ts`
- `.github/workflows/ci-v8-authenticated-frontend.yml`

`WorkflowV9` is the authoritative workflow component because it adds explicit per-role lock states over the initial V8 draft.

## Verification

- `npm exec tsc -- --project tsconfig.v8.json`: passed.
- `npm exec vite -- build --config vite.v8.config.js`: passed; 1,807 modules; JS 207.90 kB; CSS 4.32 kB.
- `npm exec vitest -- run --config vitest.v8.config.ts`: 17/17 passed across 6 files.
- `npx playwright test --config playwright.auth-ui-v8.config.ts`: 2/2 passed.
- `scripts/scan-client-secrets-v2.ps1`: passed; 3 artifacts, 6 rules.

The two V8 Playwright cases mock API responses at the browser routing layer. They prove rendering, navigation, and Owner/Viewer permission states, but they are not evidence of real backend login. Backend identity/RBAC remains covered by the 7 API V4 HTTP tests and core identity tests; full browser-to-real-auth requires a provisioned known test identity.

## Remaining dependency

Live PostgreSQL verification remains pending because the local server requires a password not present in the project. See completion audit V14.

