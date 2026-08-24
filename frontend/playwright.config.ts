import{defineConfig}from'@playwright/test';

const email='e2e.owner@example.invalid';
const password='Long-Safe-E2E-Owner-Password-2026!';
const tenant='tenant-an-nhien';
const secret='3132333435363738393031323334353637383930';

export default defineConfig({
  testDir:'./e2e',fullyParallel:false,workers:1,retries:0,timeout:30_000,
  use:{baseURL:'http://127.0.0.1:5176',channel:process.env.CI?undefined:'msedge',headless:true,trace:'retain-on-failure'},
  webServer:[
    {command:'dotnet run --project ../backend/SanSo.Api.V6/SanSo.Api.V6.csproj --no-launch-profile --urls http://127.0.0.1:5080',url:'http://127.0.0.1:5080/health',reuseExistingServer:false,timeout:120_000,env:{ASPNETCORE_ENVIRONMENT:'Development',SANSO_E2E_EMAIL:email,SANSO_E2E_PASSWORD:password,SANSO_E2E_TENANT:tenant,SANSO_E2E_TOTP_SECRET:secret,SANSO_E2E_VIEWER_EMAIL:'e2e.viewer@example.invalid',SANSO_E2E_VIEWER_PASSWORD:'Long-Safe-E2E-Viewer-Password-2026!'}},
    {command:'npm run dev -- --host 127.0.0.1 --port 5176',url:'http://127.0.0.1:5176/index-v8.html',reuseExistingServer:false,timeout:120_000,env:{VITE_API_URL:'http://127.0.0.1:5080'}}
  ],
  reporter:[['list']]
});

