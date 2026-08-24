import{test,expect}from'@playwright/test';

test('V4 browser previews 20 rows and requires explicit confirm',async({page})=>{
 const lines=['Mã đơn;Số tiền;Ngày đơn'];for(let index=1;index<=21;index++)lines.push(`BROWSER-${index};${index===2?'bad':100000+index};24/08/2026`);
 await page.goto('/index-v4.html');await page.getByText('Import CSV/XLSX có kiểm soát').click();
 await page.getByLabel('Chọn file import').setInputFiles({name:'orders.csv',mimeType:'text/csv',buffer:Buffer.from(lines.join('\n'),'utf8')});
 await page.getByText('Tạo bản xem trước').click();await expect(page.getByText('Chỉ hiển thị 20/21 dòng. Tất cả dòng vẫn được validation.')).toBeVisible();await expect(page.getByText('AMOUNT_INVALID')).toBeVisible();await expect(page.getByText('BROWSER-21')).toHaveCount(0);
 const confirm=page.getByRole('button',{name:/Xác nhận import/});await expect(confirm).toBeDisabled();await page.getByRole('checkbox').check();await expect(confirm).toBeEnabled();await confirm.click();
 await expect(page.getByText('Đã hoàn tất bước xác nhận')).toBeVisible();await expect(page.getByText('20 dòng được chấp nhận · 1 dòng bị loại.')).toBeVisible();await expect(page.getByText('Demo only · Chưa ghi PostgreSQL.')).toBeVisible();
});

test('V4 browser can abandon preview and choose another file',async({page})=>{
 await page.goto('/index-v4.html');await page.getByText('Import CSV/XLSX có kiểm soát').click();await page.getByLabel('Chọn file import').setInputFiles({name:'first.csv',mimeType:'text/csv',buffer:Buffer.from('order_code,amount,occurred_at\nRESET-1,100,2026-08-24')});await page.getByText('Tạo bản xem trước').click();await expect(page.getByText('RESET-1')).toBeVisible();await page.getByText('Chọn file khác').click();await expect(page.getByText('Chọn file CSV hoặc XLSX')).toBeVisible();
});
