using System.Security.Cryptography;
using System.Text;

public static class SecurityHelper
{
    
    private static readonly string Key = "E546C8DF278CD5931069B522E695D4F2"; // 32 chars

    public static string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = new byte[16]; // IV rỗng cho demo (Nên dùng IV động)
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var bytes = Encoding.UTF8.GetBytes(text);
        var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipher)
    {
        if (string.IsNullOrEmpty(cipher)) return cipher;
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = new byte[16];
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var bytes = Convert.FromBase64String(cipher);
        var original = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        return Encoding.UTF8.GetString(original);
    }
}