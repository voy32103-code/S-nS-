BEGIN;
CREATE TABLE invitations(
 id uuid PRIMARY KEY DEFAULT gen_random_uuid(),organization_id uuid NOT NULL REFERENCES organizations(id),email text NOT NULL,role text NOT NULL,
 token_hash text NOT NULL UNIQUE,expires_at timestamptz NOT NULL,status text NOT NULL DEFAULT 'PENDING',invited_by uuid REFERENCES users(id),accepted_by uuid REFERENCES users(id),accepted_at timestamptz,created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE sessions(
 token_hash text PRIMARY KEY,user_id uuid NOT NULL REFERENCES users(id),organization_id uuid NOT NULL REFERENCES organizations(id),role text NOT NULL,
 step_up_verified_at timestamptz,expires_at timestamptz NOT NULL,revoked_at timestamptz,created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE support_grants(
 id uuid PRIMARY KEY DEFAULT gen_random_uuid(),organization_id uuid NOT NULL REFERENCES organizations(id),support_user_id uuid NOT NULL REFERENCES users(id),approved_by uuid NOT NULL REFERENCES users(id),
 reason text NOT NULL CHECK(length(trim(reason))>0),expires_at timestamptz NOT NULL,status text NOT NULL DEFAULT 'ACTIVE',created_at timestamptz NOT NULL DEFAULT now(),CHECK(expires_at<=created_at+interval '8 hours')
);
CREATE TABLE feature_flags(
 organization_id uuid NOT NULL REFERENCES organizations(id),code text NOT NULL,enabled boolean NOT NULL DEFAULT false,updated_by uuid REFERENCES users(id),updated_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(organization_id,code)
);
CREATE TABLE notification_deliveries(
 id uuid PRIMARY KEY DEFAULT gen_random_uuid(),organization_id uuid NOT NULL REFERENCES organizations(id),alert_id uuid REFERENCES alerts(id),channel text NOT NULL,recipient_masked text NOT NULL,
 status text NOT NULL,attempt int NOT NULL DEFAULT 0,next_attempt_at timestamptz,last_error_code text,created_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE invitations ENABLE ROW LEVEL SECURITY;ALTER TABLE sessions ENABLE ROW LEVEL SECURITY;ALTER TABLE support_grants ENABLE ROW LEVEL SECURITY;ALTER TABLE feature_flags ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_invitations ON invitations USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_sessions ON sessions USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_support_grants ON support_grants USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE POLICY tenant_feature_flags ON feature_flags USING(organization_id=current_setting('app.current_organization_id',true)::uuid) WITH CHECK(organization_id=current_setting('app.current_organization_id',true)::uuid);
CREATE INDEX ix_sessions_active ON sessions(organization_id,user_id,expires_at) WHERE revoked_at IS NULL;
CREATE INDEX ix_invitations_pending ON invitations(organization_id,email,expires_at) WHERE status='PENDING';
COMMIT;
