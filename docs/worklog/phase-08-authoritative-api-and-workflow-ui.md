# Phase 8 — Authoritative API composition and interactive workflow UI

**Date:** 2026-08-24 · **Status:** in progress

## API composition V3

- Replaced API V2 composition file through MSBuild target without altering the project identity.
- Maps login/logout/me and all core read endpoints.
- Maps durable raw/inventory, refund, period freeze/lock, team invitation, support grant, billing trial/transition, tax calculation, Copilot and traceable export.
- Production resolves PostgreSQL commerce/lifecycle stores; Development uses explicit in-memory domain only where demo behavior is safe.
- Production bearer/tenant middleware remains active via secure composition extension.

## API verification

- Five new HTTP tests cover refund/freeze/lock, billing expiry, support grant, Copilot tax refusal and Production MFA login response.
- Backend/domain/API total: **45 passed, 0 failed**.
- Solution build after composition: **0 warnings, 0 errors**.

## Frontend V3

- Added accessible pilot workflow drawer over the existing operations UI.
- Interactive actions: partial refund, Tax safety check, time-bound accountant invite, trial start and Copilot refusal.
- Development copy explicitly states Production requires bearer/role/preview/diff/audit.
- Frontend scripts now build V3 by default.
- Component suite: **9 passed**; production V3 bundle build pass (1.807 modules, JS 211.73 kB, CSS 9.43 kB); npm audit 0.

## Browser verification

- Added authoritative API V3 + frontend V3 Chromium tests.
- First Tax test exposed ambiguous text locator; corrected to inspect `.workflow-result pre`, proving API response rather than static copy.
- Workflow browser suite: **3 passed** (Tax result `NEEDS_REVIEW` with null amount, Copilot `REFUSED`, refund `PARTIAL_REFUND`).
- Existing operations browser suite remains **4 passed**. Total browser coverage now 7 passing flows when run as the combined default script.

## Remaining

- PostgreSQL execution/RLS integration remains unverified locally.
- Invitation acceptance, authenticated support, billing expiration and period amendment need browser flows.
- Preview/diff/two-step confirmation UI for sensitive production commands remains incomplete.
