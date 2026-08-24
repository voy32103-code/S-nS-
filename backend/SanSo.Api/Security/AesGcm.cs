namespace SanSo.Api.Security;

internal sealed class AesGcm : IDisposable
{
    private readonly System.Security.Cryptography.AesGcm implementation;

    public AesGcm(byte[] key, int tagSizeInBytes) => implementation = new(key, tagSizeInBytes);

    public void Encrypt(byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag, byte[] associatedData) =>
        implementation.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

    public void Decrypt(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext, byte[] associatedData)
    {
        try { implementation.Decrypt(nonce, ciphertext, tag, plaintext, associatedData); }
        catch (System.Security.Cryptography.AuthenticationTagMismatchException error)
        {
            throw new System.Security.Cryptography.CryptographicException("PROTECTED_FIELD_AUTHENTICATION_FAILED", error);
        }
    }

    public void Dispose() => implementation.Dispose();
}
