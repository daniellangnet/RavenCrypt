using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Raven.Database.Plugins;
using Raven.Json.Linq;

namespace RavenCrypt
{
    public class DocumentCodec : AbstractDocumentCodec
    {
        private const string YOUR_PASSWORD = "super-secret-password";

        private static readonly ICryptoTransform Encryptor;
        private static readonly ICryptoTransform Decryptor;

        static DocumentCodec()
        {
            var passwordBytes = new Rfc2898DeriveBytes(YOUR_PASSWORD, GetSaltFromDocumentKey(Guid.NewGuid().ToString()));
            var cryptoProvider = new RijndaelManaged();

            cryptoProvider.Key = passwordBytes.GetBytes(cryptoProvider.KeySize / 8);
            cryptoProvider.IV = passwordBytes.GetBytes(cryptoProvider.BlockSize / 8);

            Encryptor = cryptoProvider.CreateEncryptor();
            Decryptor = cryptoProvider.CreateDecryptor();
        }

        public override Stream Encode(string key, RavenJObject data, RavenJObject metadata, Stream dataStream)
        {
            return new CryptoStream(dataStream, Encryptor, CryptoStreamMode.Write);
        }

        public override Stream Decode(string key, RavenJObject metadata, Stream dataStream)
        {
            return new CryptoStream(dataStream, Decryptor, CryptoStreamMode.Read);
        }

        private static byte[] GetSaltFromDocumentKey(string key)
        {
            return SHA512.Create().ComputeHash(Encoding.ASCII.GetBytes(key));
        }
    }
}