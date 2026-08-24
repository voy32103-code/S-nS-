CREATE TABLE onboarding_profiles (
  organization_id uuid PRIMARY KEY REFERENCES organizations(id) ON DELETE RESTRICT,
  current_step integer NOT NULL DEFAULT 1 CHECK (current_step BETWEEN 1 AND 8),
  subject_type text,
  legal_name text,
  tax_identifier_protected text,
  tax_identifier_last4 text CHECK (tax_identifier_last4 IS NULL OR tax_identifier_last4 ~ '^[0-9]{4}$'),
  address_protected text,
  field_key_version text,
  currency text NOT NULL DEFAULT 'VND' CHECK (currency = 'VND'),
  timezone text NOT NULL DEFAULT 'Asia/Ho_Chi_Minh' CHECK (timezone = 'Asia/Ho_Chi_Minh'),
  source_mode text,
  backfill_from date,
  mapped_sku_count integer NOT NULL DEFAULT 0 CHECK (mapped_sku_count >= 0),
  opening_balances jsonb NOT NULL DEFAULT '[]'::jsonb,
  disclaimer_version text,
  disclaimer_confirmed_at timestamptz,
  disclaimer_confirmed_by uuid REFERENCES users(id),
  first_reconciliation_id uuid REFERENCES reconciliations(id),
  completed_at timestamptz,
  updated_at timestamptz NOT NULL DEFAULT now(),
  CHECK ((disclaimer_confirmed_at IS NULL) = (disclaimer_version IS NULL)),
  CHECK (current_step < 8 OR (first_reconciliation_id IS NOT NULL AND completed_at IS NOT NULL))
);

ALTER TABLE onboarding_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding_profiles FORCE ROW LEVEL SECURITY;
CREATE POLICY onboarding_profiles_tenant ON onboarding_profiles
  USING (organization_id = current_setting('app.current_organization_id', true)::uuid)
  WITH CHECK (organization_id = current_setting('app.current_organization_id', true)::uuid);

CREATE INDEX ix_onboarding_profiles_incomplete
  ON onboarding_profiles(updated_at)
  WHERE current_step < 8;
