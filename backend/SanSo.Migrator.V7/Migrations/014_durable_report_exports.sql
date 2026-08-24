ALTER TABLE exports
  ADD COLUMN IF NOT EXISTS content_type text,
  ADD COLUMN IF NOT EXISTS content_bytes bytea,
  ADD COLUMN IF NOT EXISTS content_checksum text,
  ADD COLUMN IF NOT EXISTS expires_at timestamptz,
  ADD COLUMN IF NOT EXISTS confirmed_at timestamptz,
  ADD COLUMN IF NOT EXISTS download_count int NOT NULL DEFAULT 0;

ALTER TABLE exports
  ADD CONSTRAINT exports_status_check
  CHECK (status IN ('PREVIEWED','READY','EXPIRED','REVOKED')) NOT VALID;

CREATE INDEX IF NOT EXISTS ix_exports_download
  ON exports(organization_id,status,expires_at)
  WHERE status IN ('PREVIEWED','READY');

