# Phase 28 — Import preview hardening V7

Date: 2026-08-24  
Status: implemented and HTTP-tested.

## Canonical implementation

- Runtime source: `backend/SanSo.Api.V6/ProgramCanonicalV7.cs`
- MSBuild selection: `backend/SanSo.Api.V6/SanSo.Api.V6.csproj.user`
- Regression tests: `backend/SanSo.Api.V6.Tests/ImportPreviewSecurityV7Tests.cs`

The previous `Program.cs` is excluded from compilation. It is retained only as historical evidence because the Windows sandbox helper could not update an existing file reliably.

## Enforced controls

- request must be `multipart/form-data`;
- a `file` part is mandatory;
- maximum upload size is 10 MiB;
- only CSV and XLSX filename/MIME combinations on the allowlist are accepted;
- malformed UTF-8 CSV returns `ENCODING_INVALID` with a remediation hint;
- parsing errors remain explicit and do not persist business projections;
- preview responses include delimiter and normalized headers;
- durable staging remains PostgreSQL-backed when configured.

## Verification

`dotnet build backend/SanSo.Api.V6/SanSo.Api.V6.csproj -c Release`

- 0 warnings
- 0 errors

`dotnet test backend/SanSo.Api.V6.Tests/SanSo.Api.V6.Tests.csproj -c Release --no-restore`

- 6 passed
- 0 failed

Four tests directly cover multipart enforcement, extension/MIME rejection, the 10 MiB limit, and a valid UTF-8 CSV preview.

