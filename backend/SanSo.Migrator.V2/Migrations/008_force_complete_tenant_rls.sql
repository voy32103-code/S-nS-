DO $$
DECLARE table_name text;
BEGIN
  FOREACH table_name IN ARRAY ARRAY[
    'organizations','memberships','tax_profiles','connections','import_batches','raw_events','inbox_messages','outbox_messages',
    'products','channel_skus','orders','order_items','ledger_lines','settlements','reconciliation_runs','reconciliation_lines',
    'tax_periods','tax_calculations','tax_exceptions','inventory_locations','inventory_movements','inventory_balances','alerts',
    'subscriptions','exports','audit_logs','invitations','sessions','support_grants','feature_flags','notification_deliveries',
    'import_staging_batches','import_staging_rows','onboarding_profiles'
  ] LOOP
    EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY',table_name);
    EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY',table_name);
  END LOOP;
END $$;

CREATE POLICY tenant_organizations_v2 ON organizations USING(id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_memberships_v2 ON memberships USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_tax_profiles_v2 ON tax_profiles USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_connections_v2 ON connections USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_import_batches_v2 ON import_batches USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_raw_events_v2 ON raw_events USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_inbox_v2 ON inbox_messages USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_outbox_v2 ON outbox_messages USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_products_v2 ON products USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_channel_skus_v2 ON channel_skus USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_order_items_v2 ON order_items USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_reconciliation_runs_v2 ON reconciliation_runs USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_reconciliation_lines_v2 ON reconciliation_lines USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_tax_calculations_v2 ON tax_calculations USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_tax_exceptions_v2 ON tax_exceptions USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_inventory_locations_v2 ON inventory_locations USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_inventory_balances_v2 ON inventory_balances USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_alerts_v2 ON alerts USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_subscriptions_v2 ON subscriptions USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_audit_logs_v2 ON audit_logs USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
