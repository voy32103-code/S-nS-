import'@testing-library/jest-dom/vitest';
import{afterEach,describe,expect,it,vi}from'vitest';
import{fireEvent,render,screen,waitFor}from'@testing-library/react';
import AppV7 from'./AppV7';

afterEach(()=>vi.restoreAllMocks());

describe('AppV7 authenticated workspace',()=>{
  it('logs an Owner in, sends the tenant bearer, and loads onboarding',async()=>{
    const fetchMock=vi.spyOn(globalThis,'fetch')
      .mockResolvedValueOnce(json({token:'opaque-token',principal:{userId:'u1',tenantId:'tenant-an-nhien',role:0,stepUpVerified:true,expiresAt:'2026-08-25T00:00:00Z'}}))
      .mockResolvedValueOnce(json({snapshot:{currentStep:1}}));
    render(<AppV7/>);fillLogin();fireEvent.click(screen.getByRole('button',{name:'Đăng nhập'}));
    expect(await screen.findByText('Truy cập đã xác thực')).toBeInTheDocument();
    expect(await screen.findByText('Thiết lập doanh nghiệp')).toBeInTheDocument();
    const onboarding=fetchMock.mock.calls[1];expect(onboarding[0]).toContain('/api/onboarding');
    const headers=new Headers((onboarding[1] as RequestInit).headers);expect(headers.get('Authorization')).toBe('Bearer opaque-token');expect(headers.get('X-Tenant-Id')).toBe('tenant-an-nhien');
  });

  it('renders a denied state for Viewer instead of onboarding controls',async()=>{
    vi.spyOn(globalThis,'fetch').mockResolvedValueOnce(json({token:'viewer-token',principal:{userId:'u2',tenantId:'tenant-an-nhien',role:6,stepUpVerified:false,expiresAt:'2026-08-25T00:00:00Z'}}));
    render(<AppV7/>);fillLogin();fireEvent.click(screen.getByRole('button',{name:'Đăng nhập'}));
    expect(await screen.findByText('Bạn không có quyền onboarding')).toBeInTheDocument();
    await waitFor(()=>expect(screen.queryByText('Hoàn tất bước hiện tại')).not.toBeInTheDocument());
  });
});

function fillLogin(){fireEvent.change(screen.getByLabelText('Email'),{target:{value:'user@example.invalid'}});fireEvent.change(screen.getByLabelText('Mật khẩu'),{target:{value:'a-long-password'}});fireEvent.change(screen.getByLabelText('Mã MFA 6 số'),{target:{value:'123456'}})}
function json(value:unknown){return Promise.resolve(new Response(JSON.stringify(value),{status:200,headers:{'Content-Type':'application/json'}}))}
