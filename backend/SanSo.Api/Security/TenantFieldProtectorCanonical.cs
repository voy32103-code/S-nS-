using System.Security.Cryptography;
using System.Text;

namespace SanSo.Api.Security;

public sealed record ProtectedField(string KeyVersion,string Nonce,string Ciphertext,string Tag)
{
    public override string ToString()=>$"{KeyVersion}.{Nonce}.{Ciphertext}.{Tag}";
}

public sealed class TenantFieldProtector
{
    private readonly byte[] key;private readonly string keyVersion;
    public TenantFieldProtector(byte[] key,string keyVersion){if(key.Length!=32)throw new ArgumentException("FIELD_KEY_MUST_BE_256_BIT",nameof(key));if(string.IsNullOrWhiteSpace(keyVersion)||keyVersion.Contains('.'))throw new ArgumentException("KEY_VERSION_INVALID",nameof(keyVersion));this.key=key.ToArray();this.keyVersion=keyVersion;}
    public string Protect(string tenant,string purpose,string plaintext){ArgumentException.ThrowIfNullOrWhiteSpace(tenant);ArgumentException.ThrowIfNullOrWhiteSpace(purpose);var nonce=RandomNumberGenerator.GetBytes(12);var source=Encoding.UTF8.GetBytes(plaintext);var ciphertext=new byte[source.Length];var tag=new byte[16];try{using var aes=new System.Security.Cryptography.AesGcm(key,16);aes.Encrypt(nonce,source,ciphertext,tag,Aad(tenant,purpose,keyVersion));return new ProtectedField(keyVersion,Encode(nonce),Encode(ciphertext),Encode(tag)).ToString();}finally{CryptographicOperations.ZeroMemory(source);}}
    public string Unprotect(string tenant,string purpose,string serialized){try{var parts=serialized.Split('.');if(parts.Length!=4||parts[0]!=keyVersion)throw new CryptographicException("PROTECTED_FIELD_INVALID");var nonce=DecodeCanonical(parts[1]);var ciphertext=DecodeCanonical(parts[2]);var tag=DecodeCanonical(parts[3]);var plaintext=new byte[ciphertext.Length];try{using var aes=new System.Security.Cryptography.AesGcm(key,16);try{aes.Decrypt(nonce,ciphertext,tag,plaintext,Aad(tenant,purpose,keyVersion));}catch(AuthenticationTagMismatchException error){throw new CryptographicException("PROTECTED_FIELD_AUTHENTICATION_FAILED",error);}return Encoding.UTF8.GetString(plaintext);}finally{CryptographicOperations.ZeroMemory(plaintext);}}catch(Exception error)when(error is FormatException or ArgumentException){throw new CryptographicException("PROTECTED_FIELD_INVALID",error);}}
    private static byte[] Aad(string tenant,string purpose,string version)=>Encoding.UTF8.GetBytes($"sanso|{version}|{tenant}|{purpose}");
    private static string Encode(byte[] value)=>System.Convert.ToBase64String(value).TrimEnd('=').Replace('+','-').Replace('/','_');
    private static byte[] DecodeCanonical(string value){if(value.Contains('=')||value.Length%4==1)throw new CryptographicException("PROTECTED_FIELD_NON_CANONICAL");var standard=value.Replace('-','+').Replace('_','/');standard=standard.PadRight(standard.Length+((4-standard.Length%4)%4),'=');var bytes=System.Convert.FromBase64String(standard);if(!string.Equals(Encode(bytes),value,StringComparison.Ordinal))throw new CryptographicException("PROTECTED_FIELD_NON_CANONICAL");return bytes;}
}
