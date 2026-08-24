\set ON_ERROR_STOP on
SELECT set_config('app.current_organization_id','11111111-1111-1111-1111-111111111111',false);
INSERT INTO raw_events(organization_id,source,source_event_id,event_type,schema_version,payload,checksum)
VALUES('11111111-1111-1111-1111-111111111111','file-import','ci:projection:1','ORDER_IMPORTED','1','{"OrderCode":"CI-ORDER-1","Amount":1250000,"OccurredAt":"2026-08-24T10:00:00+07:00"}','ci-projection-checksum')
ON CONFLICT(organization_id,source,source_event_id) DO NOTHING;
DO $$ BEGIN
 IF (SELECT count(*) FROM orders WHERE organization_id='11111111-1111-1111-1111-111111111111' AND source_key='ci:projection:1')<>1 THEN RAISE EXCEPTION 'ORDER_PROJECTION_INVALID'; END IF;
 IF (SELECT count(*) FROM ledger_lines WHERE organization_id='11111111-1111-1111-1111-111111111111' AND source_key='file-sale:ci:projection:1' AND amount=1250000)<>1 THEN RAISE EXCEPTION 'LEDGER_PROJECTION_INVALID'; END IF;
 IF (SELECT count(*) FROM tax_calculations tc JOIN ledger_lines ll ON ll.id=tc.ledger_line_id WHERE ll.source_key='file-sale:ci:projection:1' AND tc.status='NEEDS_REVIEW' AND tc.calculated_amount IS NULL AND tc.rule_version_id IS NULL)<>1 THEN RAISE EXCEPTION 'TAX_NO_GUESS_PROJECTION_INVALID'; END IF;
 IF (SELECT count(*) FROM tax_exceptions te JOIN tax_calculations tc ON tc.id=te.calculation_id JOIN ledger_lines ll ON ll.id=tc.ledger_line_id WHERE ll.source_key='file-sale:ci:projection:1' AND te.code='RULE_OR_CATEGORY_REQUIRED')<>1 THEN RAISE EXCEPTION 'TAX_EXCEPTION_PROJECTION_INVALID'; END IF;
END $$;
INSERT INTO raw_events(organization_id,source,source_event_id,event_type,schema_version,payload,checksum)
VALUES('11111111-1111-1111-1111-111111111111','file-import','ci:projection:1','ORDER_IMPORTED','1','{"OrderCode":"CI-ORDER-1","Amount":1250000,"OccurredAt":"2026-08-24T10:00:00+07:00"}','ci-projection-checksum')
ON CONFLICT(organization_id,source,source_event_id) DO NOTHING;
DO $$ BEGIN IF (SELECT count(*) FROM ledger_lines WHERE source_key='file-sale:ci:projection:1')<>1 THEN RAISE EXCEPTION 'PROJECTION_NOT_IDEMPOTENT'; END IF; END $$;
SELECT 'IMPORT_PROJECTION_VERIFIED order=1 ledger=1 tax=needs_review amount=null idempotent=true' AS result;
