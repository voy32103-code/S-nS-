using System.Security.Cryptography;
using SanSo.Api.Security;
using Xunit;

namespace SanSo.Api.Tests;

public sealed class TenantFieldProtectorTests
{
    private static readonly byte[] Key = SHA256.HashData("unit-test-only-field-key"u8.ToArray());

    [Fact]
    public void RoundTripUsesRandomNonceAndDoesNotContainPlaintext()
    {
        var protector = new TenantFieldProtector(Key, "v1");
        var first = protector.Protect("tenant-a", "tax_identifier", "0123456789");
        var second = protector.Protect("tenant-a", "tax_identifier", "0123456789");
        Assert.NotEqual(first, second);
        Assert.DoesNotContain("0123456789", first);
        Assert.Equal("0123456789", protector.Unprotect("tenant-a", "tax_identifier", first));
    }

    [Fact]
    public void CiphertextIsBoundToTenantAndPurpose()
    {
        var protector = new TenantFieldProtector(Key, "v1");
        var value = protector.Protect("tenant-a", "tax_identifier", "0123456789");
        Assert.Throws<CryptographicException>(() => protector.Unprotect("tenant-b", "tax_identifier", value));
        Assert.Throws<CryptographicException>(() => protector.Unprotect("tenant-a", "address", value));
    }

    [Fact]
    public void TamperingAndWrongKeyVersionAreRejected()
    {
        var protector = new TenantFieldProtector(Key, "v1");
        var value = protector.Protect("tenant-a", "address", "Địa chỉ giả");
        var tampered = value[..^1] + (value[^1] == 'A' ? 'B' : 'A');
        Assert.ThrowsAny<CryptographicException>(() => protector.Unprotect("tenant-a", "address", tampered));
        var rotated = new TenantFieldProtector(SHA256.HashData("another-test-key"u8.ToArray()), "v2");
        Assert.Throws<CryptographicException>(() => rotated.Unprotect("tenant-a", "address", value));
    }

    [Fact]
    public void InvalidKeyMaterialIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new TenantFieldProtector(new byte[16], "v1"));
        Assert.Throws<ArgumentException>(() => new TenantFieldProtector(Key, "bad.version"));
    }
}
