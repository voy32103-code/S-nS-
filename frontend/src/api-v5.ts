export type Principal={userId:string;tenantId:string;role:'Owner'|'Admin'|'Finance'|'Ops'|'Accountant'|'Warehouse'|'Viewer';stepUpVerified:boolean;expiresAt:string};
export type Session={token:string;principal:Principal};

const base=import.meta.env.VITE_API_URL??'http://localhost:5080';

export async function login(email:string,password:string,tenantId:string,totpCode:string):Promise<Session>{
  const response=await fetch(`${base}/api/auth/login`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email,password,tenantId,totpCode:totpCode||null})});
  return decode<Session>(response);
}

export async function logout(session:Session):Promise<void>{
  await fetch(`${base}/api/auth/logout`,{method:'POST',headers:authorizedHeaders(session)});
}

export async function apiJson<T>(session:Session,path:string,init:RequestInit={}):Promise<T>{
  const headers=new Headers(init.headers);for(const[key,value]of Object.entries(authorizedHeaders(session)))headers.set(key,value);
  if(init.body&&!headers.has('Content-Type'))headers.set('Content-Type','application/json');
  return decode<T>(await fetch(base+path,{...init,headers}));
}

export async function apiFile<T>(session:Session,path:string,file:File):Promise<T>{
  const body=new FormData();body.append('file',file);
  return decode<T>(await fetch(base+path,{method:'POST',headers:authorizedHeaders(session),body}));
}

export function authorizedHeaders(session:Session):Record<string,string>{return{'X-Tenant-Id':session.principal.tenantId,Authorization:`Bearer ${session.token}`}}

async function decode<T>(response:Response):Promise<T>{
  const text=await response.text();let body:unknown=text;
  if(text){try{body=JSON.parse(text)}catch{body=text}}
  if(!response.ok){const detail=typeof body==='object'&&body&&('title'in body||'code'in body)?String((body as {title?:string;code?:string}).title??(body as {code?:string}).code):`HTTP ${response.status}`;throw new Error(detail)}
  return body as T;
}
