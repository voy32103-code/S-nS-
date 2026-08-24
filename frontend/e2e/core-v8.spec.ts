import{createHmac}from'node:crypto';import{expect,test,Page}from'@playwright/test';

const email='e2e.owner@example.invalid',password='Long-Safe-E2E-Owner-Password-2026!',tenant='tenant-an-nhien',secret='3132333435363738393031323334353637383930';

function totp(){const counter=Math.floor(Date.now()/30_000);const bytes=Buffer.alloc(8);bytes.writeBigUInt64BE(BigInt(counter));const hash=createHmac('sha1',Buffer.from(secret,'hex')).update(bytes).digest();const offset=hash[19]&15;return(((hash[offset]&127)<<24|(hash[offset+1]&255)<<16|(hash[offset+2]&255)<<8|hash[offset+3]&255)%1_000_000).toString().padStart(6,'0')}
async function login(page:Page){await page.goto('/index-v8.html');await page.getByLabel('Email').fill(email);await page.getByLabel('Mật khẩu').fill(password);await page.getByLabel('Mã tenant').fill(tenant);await page.getByLabel('Mã MFA 6 số').fill(totp());await page.getByRole('button',{name:'Đăng nhập'}).click();await expect(page.getByText('SànSổ')).toBeVisible()}

test('1 owner logs in and reaches the first demo reconciliation',async({page})=>{await login(page);await expect(page.getByText('SET-2026-08')).toBeVisible();await expect(page.getByText('Không suy đoán thuế suất.')).toBeVisible()});

test('2 invalid credentials stay outside the authenticated shell',async({page})=>{await page.goto('/index-v8.html');const inputs=page.locator('form input');await inputs.nth(0).fill(email);await inputs.nth(1).fill('Definitely-Wrong-Password!');await inputs.nth(2).fill(tenant);await inputs.nth(3).fill(totp());await page.locator('form button').click();await expect(page.getByRole('alert')).toContainText('Invalid credentials');await expect(page.locator('aside')).toHaveCount(0)});

test('3 controlled CSV import requires preview and explicit confirmation',async({page})=>{await login(page);await page.getByRole('button',{name:'Import dữ liệu'}).click();await page.getByLabel('Chọn file import').setInputFiles({name:'orders.csv',mimeType:'text/csv',buffer:Buffer.from('order_code,amount,occurred_at\nE2E-ORDER,765000,2026-08-24T10:00:00+07:00')});await page.getByRole('button',{name:'Tạo bản xem trước'}).click();await expect(page.getByText(/SHA-256:/)).toBeVisible();await page.getByRole('checkbox').check();await page.getByRole('button',{name:'Xác nhận import'}).click();await expect(page.getByText(/Đã xác nhận import|File đã được xác nhận trước đó/)).toBeVisible()});

test('4 owner can enter the resumable onboarding workflow',async({page})=>{await login(page);await page.getByRole('button',{name:'Onboarding'}).click();await expect(page.getByText('Thiết lập 7 bước')).toBeVisible();await expect(page.getByText('Hồ sơ doanh nghiệp')).toBeVisible();await page.getByRole('button',{name:'Hoàn tất bước hiện tại'}).click();await expect(page.getByText('Nguồn dữ liệu')).toBeVisible()});

test('5 controlled tax workflow remains evidence-first',async({page})=>{await login(page);await page.getByRole('button',{name:'Workflow'}).click();await page.getByRole('button',{name:'Kiểm tra thuế an toàn'}).click();await expect(page.locator('.v8-result')).toContainText(/NEEDS_REVIEW|missing|rule/i)});

test('6 logout returns to login and removes the authenticated shell',async({page})=>{await login(page);await page.getByRole('button',{name:'Đăng xuất'}).click();await expect(page.getByRole('button',{name:'Đăng nhập'})).toBeVisible();await expect(page.getByText('SET-2026-08')).toHaveCount(0)});

