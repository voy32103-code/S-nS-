$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6.Tests/PostgresSettlementPreviewWorkflowV13Tests.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='var tenant=Guid.NewGuid();await Execute(cs,"INSERT INTO organizations'
$new='var tenant=Guid.NewGuid();var actor=Guid.NewGuid();await Execute(cs,"INSERT INTO organizations'
if(-not $text.Contains($old)){throw 'PREVIEW_ACTOR_DECLARATION_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
$old='await Execute(cs,"INSERT INTO orders(organization_id,channel,source_key,code,status,gross_amount,occurred_at)VALUES($1,''CSV'',$2,''ORD-PREVIEW'',''COMPLETED'',1000,now())",tenant,$"order-{tenant:N}");'
$new=$old+'await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,''Preview actor'',''managed'',true)",actor,$"preview-{actor:N}@example.invalid");'
if(-not $text.Contains($old)){throw 'PREVIEW_ACTOR_SEED_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
$text=$text.Replace('"tester",default','actor.ToString(),default')
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'PREVIEW_WORKFLOW_TEST_ACTOR_FIXED=1'
