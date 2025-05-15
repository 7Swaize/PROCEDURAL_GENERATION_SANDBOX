#if NEWTONSOFT_JSON
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VS.Utilities.Files
{
    public sealed class AesEncrypter : IEncrypter
    {
        private const string Key = "ggdPhkeOoiv6YMiPWa34kIuOdDUL7NwQFg6l1DVdwN8=";
        private const string Iv = "JZuM0HQsWSBVpRHTeRZMYQ==";
        
        public Task WriteEncryptedData<T>(T data, FileStream fileStream)
        {
            using Aes aesProvider = Aes.Create();
            aesProvider.Key = Convert.FromBase64String(Key);
            aesProvider.IV = Convert.FromBase64String(Iv);

            using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
            using CryptoStream cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Write);
            
            cryptoStream.WriteAsync(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data))); 
            return Task.CompletedTask;
        }

        public T ReadEncryptedData<T>(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            
            using Aes aesProvider = Aes.Create();
            aesProvider.Key = Convert.FromBase64String(Key);
            aesProvider.IV = Convert.FromBase64String(Iv);
            
            using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
            using MemoryStream decryptionStream = new MemoryStream(fileBytes);
            using CryptoStream cryptoStream = new CryptoStream(decryptionStream, cryptoTransform, CryptoStreamMode.Read);
            using StreamReader decryptionStreamReader = new StreamReader(cryptoStream);
            
            string result = decryptionStreamReader.ReadToEnd();
            
            return JsonConvert.DeserializeObject<T>(result);
        }
    }

    public interface IEncrypter
    {
        public Task WriteEncryptedData<T>(T data, FileStream fileStream);
        public T ReadEncryptedData<T>(string path);
    }
}
#endif