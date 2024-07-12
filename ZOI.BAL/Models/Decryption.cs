using System.Security.Cryptography;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Models
{
    public class Decryption
    {
        public static string DecryptData(string data)
        {
            try
            {
                using (AesManaged aesManaged = new AesManaged())
                {
                    ICryptoTransform decryptor = aesManaged.CreateDecryptor(Convert.FromBase64String(AES.AES256EncryptString), Convert.FromBase64String(AES.AES256IVStringAccID));
                    using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cryptoStream))
                                data = reader.ReadToEnd();
                        }
                    }
                }
                return data;
            }
            catch
            {
                throw;
            }
        }
    }
}
