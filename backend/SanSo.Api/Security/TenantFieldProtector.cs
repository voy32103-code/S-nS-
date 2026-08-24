using System.Security.Cryptography;
using System.Text;

namespace SanSo.Api.Security;

public sealed record ProtectedField(string KeyVersion, string Nonce, string Ciphertext, string Tag)
{
    public override string ToString() => $"{KeyVersion}.{Nonce}.{Ciphertext}.{Tag}";
}

public sealed class TenantFieldProtector
{
    private readonly byte[] key;
    private readonly string keyVersion;

    public TenantFieldProtector(byte[] key, string keyVersion)
    {
        if (key.Length != 32) throw new ArgumentException("FIELD_KEY_MUST_BE_256_BIT", nameof(key));
        if (string.IsNullOrWhiteSpace(keyVersion) || keyVersion.Contains('.')) throw new ArgumentException("KEY_VERSION_INVALID", nameof(keyVersion));
        this.key = key.ToArray();
        this.keyVersion = keyVersion;
    }

    public string Protect(string tenant, string purpose, string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var source = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[source.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, source, ciphertext, tag, AssociatedData(tenant, purpose, keyVersion));
        CryptographicOperations.ZeroMemory(source);
        return new ProtectedField(keyVersion, Base64(nonce), Base64(ciphertext), Base64(tag)).ToString();
    }

    public string Unprotect(string tenant, string purpose, string serialized)
    {
        var parts = serialized.Split('.');
        if (parts.Length != 4 || parts[0] != keyVersion) throw new CryptographicException("PROTECTED_FIELD_FORMAT_OR_KEY_VERSION_INVALID");
        var nonce = Convert.FromBase64String(parts[1]);
        var ciphertext = Convert.FromBase64String(parts[2]);
        var tag = Convert.FromBase64String(parts[3]);
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, AssociatedData(tenant, purpose, keyVersion));
        try { return Encoding.UTF8.GetString(plaintext); }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    private static byte[] AssociatedData(string tenant, string purpose, string version) => Encoding.UTF8.GetBytes($"sanso|{version}|{tenant}|{purpose}");
    private static string Base64(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
