using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AES
{
    public class AES128
    {
        public byte[] Key { get { return aeskey; }  set { aeskey = value;  } }
        private static byte[] aeskey;
        private Aes aes;

        public byte[] IV { get { return pr_IV; } set { pr_IV = value; } }
        private static byte[] pr_IV;
        public AES128(){
            aes = Aes.Create();
            aes.Mode = CipherMode.CBC; //Cipher Block Chaining
            aes.Padding = PaddingMode.Zeros;
            aes.KeySize = 128;

            aeskey = aes.Key;
            pr_IV = aes.IV;
        }
        public byte[] ToAes128(byte[] fstream)
        {
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(fstream, encryptor);
            }
        }

        public byte[] FromAes128(byte[] shifr)
        {
            byte[] bytesIv = new byte[16];
            byte[] mess = new byte[shifr.Length - 16];
            //Списываем соль
            for (int i = shifr.Length - 16, j = 0; i < shifr.Length; i++, j++)
                bytesIv[j] = shifr[i];
            //Списываем оставшуюся часть сообщения
            for (int i = 0; i < shifr.Length - 16; i++)
                mess[i] = shifr[i];
            //Объект класса Aes
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            //Задаем тот же ключ, что и для шифрования
            aes.Key = aeskey;
            //Задаем соль
            aes.IV = pr_IV;
            //Строковая переменная для результата
            string text = "";
            byte[] data = mess;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(shifr, decryptor);
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

    }
}
