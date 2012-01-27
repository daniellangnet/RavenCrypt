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
        private const string YourPassword = "super-secret-password";

        public override Stream Encode(string key, RavenJObject data, RavenJObject metadata, Stream dataStream)
        {
            var passwordBytes = new Rfc2898DeriveBytes(YourPassword, GetSaltFromDocumentKey(key));
            var cryptoProvider = new TripleDESCryptoServiceProvider
            {
                Key = passwordBytes.GetBytes(24), 
                IV = passwordBytes.GetBytes(8)
            };

            return new CryptoStream(dataStream, cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public override Stream Decode(string key, RavenJObject metadata, Stream dataStream)
        {
            var passwordBytes = new Rfc2898DeriveBytes(YourPassword, GetSaltFromDocumentKey(key));
            var cryptoProvider = new TripleDESCryptoServiceProvider
            {
                Key = passwordBytes.GetBytes(24),
                IV = passwordBytes.GetBytes(8)
            };

            return new CryptoStream(dataStream, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
        }

        private static byte[] GetSaltFromDocumentKey(string key)
        {
            return MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(key));
        }
    }
}