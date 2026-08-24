CREATE TABLE import_batches (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  organization_id uuid NOT NULL REFERENCES organizations(id),
  checksum text NOT NULL,
  token_hash text NOT NULL UNIQUE,
  format text NOT NULL CHECK (format IN ('CSV','XLSX')),
  template_version text NOT NULL,
  status text NOT NULL CHECK (status IN ('PREVIEWED','CONFIRMED','EXPIRED')),
  expires_at timestamptz NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now(),
  confirmed_at timestamptz,
  confirmed_by uuid REFERENCES users(id),
  CHECK ((status = 'CONFIRMED') = (confirmed_at IS NOT NULL))
);

CREATE UNIQUE INDEX ux_import_batches_confirmed_checksum
  ON import_batches(organization_id, checksum)
  WHERE status = 'CONFIRMED';

CREATE TABLE import_rows (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  organization_id uuid NOT NULL REFERENCES organizations(id),
  batch_id uuid NOT NULL REFERENCES import_batches(id) ON DELETE RESTRICT,
  row_number integer NOT NULL CHECK (row_number > 0),
  event_id text NOT NULL,
  normalized_payload jsonb NOT NULL,
  errors jsonb NOT NULL DEFAULT '[]'::jsonb,
  created_at timestamptz NOT NULL DEFAULT now(),
  UNIQUE(batch_id, row_number),
  UNIQUE(organization_id, event_id)
);

ALTER TABLE import_batches ENABLE ROW LEVEL SECURITY;
ALTER TABLE import_batches FORCE ROW LEVEL SECURITY;
ALTER TABLE import_rows ENABLE ROW LEVEL SECURITY;
ALTER TABLE import_rows FORCE ROW LEVEL SECURITY;

CREATE POLICY import_batches_tenant ON import_batches
  USING (organization_id = current_setting('app.current_organization_id', true)::uuid)
  WITH CHECK (organization_id = current_setting('app.current_organization_id', true)::uuid);

CREATE POLICY import_rows_tenant ON import_rows
  USING (organization_id = current_setting('app.current_organization_id', true)::uuid)
  WITH CHECK (organization_id = current_setting('app.current_organization_id', true)::uuid);

CREATE INDEX ix_import_batches_tenant_status_expiry
  ON import_batches(organization_id, status, expires_at);
CREATE INDEX ix_import_rows_batch ON import_rows(batch_id, row_number);
