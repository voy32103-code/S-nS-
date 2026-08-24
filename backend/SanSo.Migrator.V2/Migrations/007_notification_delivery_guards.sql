ALTER TABLE notification_deliveries ENABLE ROW LEVEL SECURITY; ALTER TABLE notification_deliveries FORCE ROW LEVEL SECURITY;
CREATE POLICY notification_deliveries_tenant ON notification_deliveries USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
ALTER TABLE notification_deliveries ADD COLUMN IF NOT EXISTS dedupe_key text, ADD COLUMN IF NOT EXISTS notification_type text, ADD COLUMN IF NOT EXISTS resource_ref text;
CREATE UNIQUE INDEX ux_notification_delivery_dedupe ON notification_deliveries(organization_id,dedupe_key,channel) WHERE dedupe_key IS NOT NULL;
CREATE INDEX ix_notification_delivery_due ON notification_deliveries(organization_id,status,next_attempt_at) WHERE status IN('PENDING','RETRY_SCHEDULED');
