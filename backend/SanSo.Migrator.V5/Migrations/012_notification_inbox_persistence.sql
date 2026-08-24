ALTER TABLE notification_deliveries
  ADD COLUMN IF NOT EXISTS title text,
  ADD COLUMN IF NOT EXISTS body text,
  ADD COLUMN IF NOT EXISTS acknowledged_at timestamptz;

ALTER TABLE notification_deliveries
  ADD CONSTRAINT notification_deliveries_channel_check
  CHECK (channel IN ('IN_APP','EMAIL')) NOT VALID;

ALTER TABLE notification_deliveries
  ADD CONSTRAINT notification_deliveries_status_check
  CHECK (status IN ('PENDING','DELIVERED','RETRY_SCHEDULED','DEAD_LETTER','ACKNOWLEDGED')) NOT VALID;

CREATE INDEX IF NOT EXISTS ix_notification_inbox
  ON notification_deliveries(organization_id,created_at DESC)
  WHERE channel='IN_APP';

