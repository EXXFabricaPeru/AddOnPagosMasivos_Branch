using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Util
{
    public static class CryptoHelper
    {
        public static string DecryptPgpDataFile(string inputFilePath, byte[] decryptKey, string password)
        {
            // reading in the file as byte[] prevents a "unknown object in stream 47" IO exception thrown by bouncy castle
            var inputData = File.ReadAllBytes(inputFilePath);

            return DecryptPgpBytes(inputData, decryptKey, password);
        }

        public static string DecryptPgpData(string inputData, byte[] decryptKey, string password, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            byte[] bytes = encoding.GetBytes(inputData);
            return DecryptPgpBytes(bytes, decryptKey, password);
        }

        public static string DecryptPgpBytes(byte[] inputData, byte[] keyData, string pass)
        {
            string output;
            using (var inputStream = new MemoryStream(inputData))
            {
                using (var keyIn = new MemoryStream(keyData))
                {
                    output = DecryptPgpData(inputStream, keyIn, pass);
                }
            }
            return output;
        }

        public static string DecryptPgpData(Stream inputStream, Stream privateKeyStream, string passPhrase)
        {
            string output;

            PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));

            // find secret key
            PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

            PgpObject pgp = pgpFactory?.NextPgpObject();

            if (pgp == null)
            {
                var ex0 = new PgpException("The PGP Object is null");
                throw ex0;
            }

            // the first object might be a PGP marker packet.
            PgpEncryptedDataList encryptedData;
            if (pgp is PgpEncryptedDataList list)
            {
                encryptedData = list;
            }
            else
            {
                encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
            }

            if (encryptedData == null)
            {
                var ex1 = new PgpException("The PGP Encrypted Data is null");
                throw ex1;
            }

            // decrypt
            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData pubKeyData = null;
            foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                if (privateKey != null)
                {
                    pubKeyData = pubKeyDataItem;
                    break;
                }
            }

            if (privateKey == null)
            {
                var ex2 = new ArgumentException("Secret key for message not found.");
                throw ex2;
            }

            PgpObjectFactory plainFact = null;
            using (Stream clear = pubKeyData.GetDataStream(privateKey))
            {
                plainFact = new PgpObjectFactory(clear);
            }

            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData compressedData)
            {
                PgpObjectFactory pgpCompressedFactory = null;

                using (Stream compDataIn = compressedData.GetDataStream())
                {
                    pgpCompressedFactory = new PgpObjectFactory(compDataIn);
                }

                message = pgpCompressedFactory.NextPgpObject();
                if (message is PgpOnePassSignatureList)
                {
                    message = pgpCompressedFactory.NextPgpObject();
                }

                PgpLiteralData literalData = (PgpLiteralData)message;
                using (Stream unc = literalData.GetInputStream())
                {
                    output = GetString(unc);
                }
            }
            else if (message is PgpLiteralData literalData)
            {
                using (Stream unc = literalData.GetInputStream())
                {
                    output = GetString(unc);
                }
            }
            else if (message is PgpOnePassSignatureList)
            {
                var ex3 = new PgpException("Encrypted message contains a signed message - not literal data.");
                throw ex3;
            }
            else
            {
                var ex4 = new PgpException("Message is not a simple encrypted file - type unknown.");
                throw ex4;
            }

            return output;
        }

        public static void EncryptPgpFile(string inputFilePath, string outputFilePath, byte[] publicKeyData, bool armor = false, bool withIntegrityCheck = true)
        {
            using (Stream publicKeyStream = new MemoryStream(publicKeyData))
            {
                PgpPublicKey pubKey = ReadPublicKey(publicKeyStream);

                using (MemoryStream outputBytes = new MemoryStream())
                {
                    PgpCompressedDataGenerator dataCompressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                    PgpUtilities.WriteFileToLiteralData(dataCompressor.Open(outputBytes), PgpLiteralData.Binary, new FileInfo(inputFilePath));

                    dataCompressor.Close();
                    PgpEncryptedDataGenerator dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes128, withIntegrityCheck, new SecureRandom());

                    dataGenerator.AddMethod(pubKey);
                    byte[] dataBytes = outputBytes.ToArray();

                    using (Stream outputStream = File.Create(outputFilePath))
                    {
                        if (armor)
                        {
                            using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(outputStream))
                            {
                                WriteStream(dataGenerator.Open(armoredStream, dataBytes.Length), ref dataBytes);
                            }
                        }
                        else
                        {
                            WriteStream(dataGenerator.Open(outputStream, dataBytes.Length), ref dataBytes);
                        }
                    }
                }
            }
        }

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            var ex = new ArgumentException("Can't find encryption key in key ring.");
            throw ex;
        }

        // IO functions: 
        private static string GetString(Stream inputStream)
        {
            string output;
            using (StreamReader reader = new StreamReader(inputStream))
            {
                output = reader.ReadToEnd();
            }
            return output;
        }

        private static void WriteStream(Stream inputStream, ref byte[] dataBytes)
        {
            using (Stream outputStream = inputStream)
            {
                outputStream.Write(dataBytes, 0, dataBytes.Length);
            }
        }
    }
}
