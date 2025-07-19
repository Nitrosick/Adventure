using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class Encryption {
  private static readonly string key = "MySecretKey12345";
  private static readonly string iv = "MyInitVector1234";

  public static string Encrypt(string plainText) {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = Encoding.UTF8.GetBytes(iv);

    using var encryptor = aes.CreateEncryptor();
    using var ms = new MemoryStream();
    using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
    using var sw = new StreamWriter(cs);

    sw.Write(plainText);
    sw.Close();

    return Convert.ToBase64String(ms.ToArray());
  }

  public static string Decrypt(string cipherText) {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = Encoding.UTF8.GetBytes(iv);

    using var decryptor = aes.CreateDecryptor();
    using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);

    return sr.ReadToEnd();
  }
}
