ALTER TABLE outbox_messages ADD COLUMN IF NOT EXISTS error_code text;
CREATE INDEX ix_outbox_worker_due ON outbox_messages(organization_id,status,next_attempt_at,created_at)
  WHERE status IN('PENDING','RETRY_SCHEDULED','PROCESSING');
COMMENT ON COLUMN outbox_messages.next_attempt_at IS 'For PROCESSING rows this is lease expiry; for RETRY_SCHEDULED rows this is retry due time.';
