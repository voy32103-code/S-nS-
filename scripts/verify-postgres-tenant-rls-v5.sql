\set ON_ERROR_STOP on

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname='sanso_ci_rls_reader') THEN
    CREATE ROLE sanso_ci_rls_reader NOLOGIN NOSUPERUSER NOBYPASSRLS;
  END IF;
END $$;
GRANT USAGE ON SCHEMA public TO sanso_ci_rls_reader;
GRANT SELECT ON organizations,raw_events TO sanso_ci_rls_reader;

DO $$
DECLARE missing_count integer;
BEGIN
  SELECT count(*) INTO missing_count
  FROM (VALUES
    ('organizations'),('memberships'),('tax_profiles'),('connections'),('import_batches'),('raw_events'),('inbox_messages'),('outbox_messages'),
    ('products'),('channel_skus'),('orders'),('order_items'),('ledger_lines'),('settlements'),('reconciliation_runs'),('reconciliation_lines'),
    ('tax_periods'),('tax_calculations'),('tax_exceptions'),('inventory_locations'),('inventory_movements'),('inventory_balances'),('alerts'),
    ('subscriptions'),('exports'),('audit_logs'),('invitations'),('sessions'),('support_grants'),('feature_flags'),('notification_deliveries'),
    ('import_staging_batches'),('import_staging_rows'),('onboarding_profiles')
  ) AS expected(name)
  LEFT JOIN pg_class c ON c.relname=expected.name AND c.relnamespace='public'::regnamespace
  WHERE c.oid IS NULL OR NOT c.relrowsecurity OR NOT c.relforcerowsecurity;
  IF missing_count<>0 THEN RAISE EXCEPTION 'RLS_FORCE_INCOMPLETE count=%',missing_count; END IF;
END $$;

SET ROLE sanso_ci_rls_reader;
SET app.current_organization_id='11111111-1111-1111-1111-111111111111';
DO $$ BEGIN
  IF (SELECT count(*) FROM organizations)<>1 THEN RAISE EXCEPTION 'TENANT_A_ORGANIZATION_VISIBILITY_INVALID'; END IF;
  IF (SELECT count(*) FROM raw_events)<>11 THEN RAISE EXCEPTION 'TENANT_A_RAW_EVENT_VISIBILITY_INVALID'; END IF;
  IF EXISTS(SELECT 1 FROM raw_events WHERE organization_id='22222222-2222-2222-2222-222222222222') THEN RAISE EXCEPTION 'TENANT_B_LEAKED_INTO_A'; END IF;
END $$;
SET app.current_organization_id='22222222-2222-2222-2222-222222222222';
DO $$ BEGIN
  IF (SELECT count(*) FROM organizations)<>1 THEN RAISE EXCEPTION 'TENANT_B_ORGANIZATION_VISIBILITY_INVALID'; END IF;
  IF (SELECT count(*) FROM raw_events)<>1 THEN RAISE EXCEPTION 'TENANT_B_RAW_EVENT_VISIBILITY_INVALID'; END IF;
  IF EXISTS(SELECT 1 FROM raw_events WHERE organization_id='11111111-1111-1111-1111-111111111111') THEN RAISE EXCEPTION 'TENANT_A_LEAKED_INTO_B'; END IF;
END $$;
RESET ROLE;
SELECT 'POSTGRES_TENANT_RLS_VERIFIED tenant_a=11 tenant_b=1 forced_tables=34' AS result;
