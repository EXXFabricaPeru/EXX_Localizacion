using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.metodos
{
    public class TripleDESCryptoHelper
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private static byte[] sKey = new byte[16]
        {
            (byte) 192,
            (byte) 0,
            (byte) 142,
            (byte) 109,
            (byte) 122,
            (byte) 177,
            (byte) 77,
            (byte) 182,
            (byte) 130,
            (byte) 87,
            (byte) 81,
            (byte) 122,
            (byte) 122,
            (byte) 154,
            (byte) 106,
            (byte) 195
        };
        private static byte[] sIV = new byte[8]
        {
            (byte) 158,
            (byte) 231,
            (byte) 208,
            (byte) 155,
            (byte) 29,
            (byte) 14,
            (byte) 71,
            (byte) 13
        };

        public static byte[] DecryptFromFile(string FileName)
        {
            using (FileStream fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
                return DecryptFromStream((Stream)fileStream);
        }

        public static byte[] DecryptFromStream(Stream fStream)
        {
            try
            {
                TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();
                cryptoServiceProvider.Key = TripleDESCryptoHelper.sKey;
                cryptoServiceProvider.IV = TripleDESCryptoHelper.sIV;
                using (CryptoStream cryptoStream = new CryptoStream(fStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[32768];
                    int length = 0;
                    int num1;
                    while ((num1 = cryptoStream.Read(buffer, length, buffer.Length - length)) > 0)
                    {
                        length += num1;
                        if (length == buffer.Length)
                        {
                            int num2 = cryptoStream.ReadByte();
                            if (num2 == -1)
                                return buffer;
                            byte[] numArray = new byte[buffer.Length * 2];
                            Array.Copy((Array)buffer, (Array)numArray, buffer.Length);
                            numArray[length] = (byte)num2;
                            buffer = numArray;
                            ++length;
                        }
                    }
                    byte[] numArray1 = new byte[length];
                    Array.Copy((Array)buffer, (Array)numArray1, length);
                    GC.Collect();
                    return numArray1;
                }
            }
            catch (CryptographicException ex)
            {
                logger.Error("DecryptFromStream", ex);
                throw;
            }
        }

        public static void EncryptToFile(string inputFileName, string outputFileName)
        {
            using (FileStream fileStream = File.Open(outputFileName, FileMode.Create))
                EncryptToStream(File.ReadAllBytes(inputFileName), (Stream)fileStream);
        }

        public static void EncryptToStream(byte[] inputBytes, Stream resultStream)
        {
            try
            {
                TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();
                cryptoServiceProvider.Key = TripleDESCryptoHelper.sKey;
                cryptoServiceProvider.IV = TripleDESCryptoHelper.sIV;
                using (CryptoStream cryptoStream = new CryptoStream(resultStream, cryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter((Stream)cryptoStream))
                        binaryWriter.Write(inputBytes);
                }
            }
            catch (Exception ex)
            {
                logger.Error("EncryptToStream", ex);
                throw;
            }
        }
    }
}
