BEGIN;
-- Application sets `app.current_organization_id` per transaction after authenticated membership lookup.
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;
ALTER TABLE ledger_lines ENABLE ROW LEVEL SECURITY;
ALTER TABLE settlements ENABLE ROW LEVEL SECURITY;
ALTER TABLE tax_periods ENABLE ROW LEVEL SECURITY;
ALTER TABLE inventory_movements ENABLE ROW LEVEL SECURITY;
ALTER TABLE exports ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_orders ON orders USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_ledger ON ledger_lines USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_settlements ON settlements USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_tax_periods ON tax_periods USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_inventory_movements ON inventory_movements USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_exports ON exports USING (organization_id = current_setting('app.current_organization_id',true)::uuid) WITH CHECK (organization_id = current_setting('app.current_organization_id',true)::uuid);
COMMIT;
