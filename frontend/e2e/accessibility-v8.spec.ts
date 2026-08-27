import{createHmac}from'node:crypto';import AxeBuilder from'@axe-core/playwright';import{expect,test}from'@playwright/test';

const email='e2e.owner@example.invalid',password='Long-Safe-E2E-Owner-Password-2026!',tenant='tenant-an-nhien',secret='3132333435363738393031323334353637383930';
function reportAxe(violations:Array<{id:string;impact?:string|null;nodes:Array<{target:unknown}>}>){
  if(!process.env.CI||violations.length===0)return;
  const raw=violations.map(v=>v.id+' ('+(v.impact??'unknown')+'): '+v.nodes.map(n=>JSON.stringify(n.target)).join(', ')).join(' | ').slice(0,3000);
  const safe=raw.replaceAll('%','%25').replaceAll('\r','%0D').replaceAll('\n','%0A');
  console.log('::error file=frontend/e2e/accessibility-v8.spec.ts,line=6,title=Axe accessibility violation::'+safe);
}
function totp(){const counter=Math.floor(Date.now()/30_000);const bytes=Buffer.alloc(8);bytes.writeBigUInt64BE(BigInt(counter));const hash=createHmac('sha1',Buffer.from(secret,'hex')).update(bytes).digest();const offset=hash[19]&15;return(((hash[offset]&127)<<24|(hash[offset+1]&255)<<16|(hash[offset+2]&255)<<8|hash[offset+3]&255)%1_000_000).toString().padStart(6,'0')}

test('login has no WCAG A/AA violations',async({page})=>{await page.goto('/index-v8.html');const result=await new AxeBuilder({page}).withTags(['wcag2a','wcag2aa','wcag21a','wcag21aa']).analyze();reportAxe(result.violations);expect(result.violations).toEqual([])});

test('authenticated overview has no WCAG A/AA violations',async({page})=>{await page.goto('/index-v8.html');const inputs=page.locator('form input');await inputs.nth(0).fill(email);await inputs.nth(1).fill(password);await inputs.nth(2).fill(tenant);await inputs.nth(3).fill(totp());await page.locator('form button').click();await expect(page.locator('aside')).toBeVisible();await expect(page.getByText(/SET-/)).toBeVisible();const result=await new AxeBuilder({page}).withTags(['wcag2a','wcag2aa','wcag21a','wcag21aa']).analyze();reportAxe(result.violations);expect(result.violations).toEqual([])});
