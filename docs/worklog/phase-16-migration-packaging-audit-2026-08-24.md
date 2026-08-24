# Phase 16 — Migration packaging audit

Ngày: 2026-08-24

## Phát hiện

`SanSo.Migrator.csproj` chỉ đóng gói migration từ:

```text
../SanSo.Api/Persistence/Migrations/*.sql
```

Các migration 005–007 ban đầu được tạo dưới thư mục project migrator, vì vậy build vẫn xanh nhưng binary output chỉ có 001–004. Đây là lỗi bằng chứng: source tồn tại không đồng nghĩa migrator có thể chạy source đó.

## Khắc phục

- Đặt 005 import confirmation, 006 onboarding profiles và 007 notification RLS guard vào nguồn migration canonical mà project đóng gói.
- Build lại `SanSo.Migrator`.
- Kiểm tra trực tiếp `bin/Debug/net9.0/Migrations`.
- Thêm `scripts/verify-migration-manifest.ps1` để kiểm tra:
  - tên file chuẩn;
  - sequence liên tục từ 001;
  - các guard RLS bắt buộc;
  - migration mới thật sự có trong binary output.

## Bằng chứng

Binary manifest sau build:

```text
001_initial.sql
002_tenant_guards.sql
003_demo_seed.sql
004_workflow_security.sql
005_import_confirmation.sql
006_onboarding_profiles.sql
007_notification_delivery_guards.sql
```

Build migrator: 0 warnings, 0 errors.

Đây chỉ chứng minh packaging. SQL execution, PostgreSQL compatibility và RLS runtime vẫn chưa được chứng minh khi Docker daemon chưa chạy.
