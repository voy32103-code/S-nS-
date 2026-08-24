CREATE TABLE settlement_import_previews(
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  organization_id uuid NOT NULL REFERENCES organizations(id),
  checksum text NOT NULL,
  token_hash text NOT NULL,
  format text NOT NULL CHECK(format IN ('CSV','XLSX')),
  normalized_payload jsonb NOT NULL,
  status text NOT NULL CHECK(status IN ('PREVIEWED','CONFIRMED','EXPIRED')),
  expires_at timestamptz NOT NULL,
  confirmed_run_id uuid REFERENCES reconciliation_runs(id),
  created_at timestamptz NOT NULL DEFAULT now(),
  confirmed_at timestamptz,
  UNIQUE(organization_id,checksum)
);
ALTER TABLE settlement_import_previews ENABLE ROW LEVEL SECURITY;
ALTER TABLE settlement_import_previews FORCE ROW LEVEL SECURITY;
CREATE POLICY settlement_import_previews_tenant ON settlement_import_previews
  USING(organization_id=current_setting('app.current_organization_id',true)::uuid)
  WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE INDEX ix_settlement_previews_expiry ON settlement_import_previews(organization_id,status,expires_at);

