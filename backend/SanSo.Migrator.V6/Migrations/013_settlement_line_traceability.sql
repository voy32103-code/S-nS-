ALTER TABLE ledger_lines
  ADD COLUMN IF NOT EXISTS settlement_id uuid REFERENCES settlements(id);

ALTER TABLE reconciliation_lines
  ADD COLUMN IF NOT EXISTS source_line_id text,
  ADD COLUMN IF NOT EXISTS source_order_code text;

CREATE UNIQUE INDEX IF NOT EXISTS ux_reconciliation_run_input
  ON reconciliation_runs(organization_id,settlement_id,input_checksum);

CREATE UNIQUE INDEX IF NOT EXISTS ux_reconciliation_line_source
  ON reconciliation_lines(organization_id,run_id,source_line_id)
  WHERE source_line_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_ledger_settlement
  ON ledger_lines(organization_id,settlement_id,occurred_at);

