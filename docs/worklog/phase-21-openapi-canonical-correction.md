# Phase 21 correction — Canonical OpenAPI JSON

Self-review phát hiện YAML V4 đặt reusable `parameters` ở root, không đúng OpenAPI structure. File YAML được giữ làm bằng chứng draft nhưng không authoritative.

Authoritative contract mới là `docs/openapi-v4-canonical.json`:

- JSON parse được bằng PowerShell `ConvertFrom-Json`;
- `components.parameters` và `$ref` đúng vị trí;
- OpenAPI 3.1.0;
- bearer security + tenant header;
- 21 required pilot paths;
- import request/preview/confirmation schemas;
- tax/no-submit/persistence limitations trong description.

`scripts/verify-openapi-v4-canonical.ps1` parse document, kiểm tra paths, security, tenant parameter và dangling component refs.

`docs/openapi-v4.yaml` và verifier V1/V2 là superseded drafts, không dùng để generate client hoặc publish contract.
