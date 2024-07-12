using System.Security.Cryptography;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Models
{
    public class Encryption
    {
        public static string EncryptData(string data)
        {
            try
            {
                byte[] encrypted;
                using (AesManaged aesManaged = new AesManaged())
                {
                    ICryptoTransform encryptor = aesManaged.CreateEncryptor(Convert.FromBase64String(AES.AES256EncryptString), Convert.FromBase64String(AES.AES256IVStringAccID));
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                                streamWriter.Write(data);
                            encrypted = memoryStream.ToArray();
                        }
                    }
                }
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                throw;
            }
        }

    }
}
