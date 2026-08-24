namespace SanSo.Api.Security;

internal static class Convert
{
    public static string ToBase64String(byte[] value) => System.Convert.ToBase64String(value);

    public static byte[] FromBase64String(string value)
    {
        var standard = value.Replace('-', '+').Replace('_', '/');
        standard = standard.PadRight(standard.Length + ((4 - standard.Length % 4) % 4), '=');
        try { return System.Convert.FromBase64String(standard); }
        catch (FormatException error) { throw new System.Security.Cryptography.CryptographicException("PROTECTED_FIELD_INVALID", error); }
    }
}
