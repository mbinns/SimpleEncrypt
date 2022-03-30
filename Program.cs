using System;
using System.IO;
using System.Security.Cryptography;

namespace SimpleEncrypt
{

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 4 || args[0] == "help")
            {
                Console.WriteLine("This program will Encrypt or Decrypt a file specified using AES-CBC and a provided key.");
                Console.WriteLine("SimpleEncrypt.exe <source file> <destination file> <encryption key> <action>\n");
                Console.WriteLine("[*] Source - Specifies the source file ");
                Console.WriteLine("[*] Destination - Specifies the destination file.");
                Console.WriteLine("[*] Key - The Symmetric key.");
                Console.WriteLine("[*] Action - Action to perform on file {encrypt or decrypt}");
                Console.WriteLine("[*] Examples:");
                Console.WriteLine("[*] Encryption:");
                Console.WriteLine("\t./SimpleEncrypt.exe C:\\Users\\Mack\\Documents\\test.txt C:\\Users\\Mack\\Documents\\test.txt-encrypted wty5Bi%65FTwL&amLAKyqtoQPyBhjmVk encrypt");
                Console.WriteLine("[*]Decryption:");
                Console.WriteLine("\t./SimpleEncrypt.exe C:\\Users\\Mack\\Documents\\test.txt-encrypted C:\\Users\\Mack\\Documents\\test-decrypted.txt wty5Bi%65FTwL&amLAKyqtoQPyBhjmVk decrypt");
                return;
            }

            var sourceFilename = args[0];
            var destinationFilename = args[1];
            var symmKey = System.Text.Encoding.ASCII.GetBytes(args[2]);
            var action = args[3];

            if (action == "encrypt")
            {
                Console.WriteLine("[*] Encrypting File: {0}", sourceFilename);
                Console.WriteLine("[*] Using Key: {0}", args[2]);

                using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
                {
                    //specify known key but let it make up a random IV
                    try
                    {
                        provider.Key = symmKey;
                    }
                    catch (CryptographicException e)
                    {
                        Console.WriteLine("[!] ERROR {0} Please use a key that is one of the following lengths in size:", e.Message);
                        Console.WriteLine("\t16\n\t24\n\t32");
                        System.Environment.Exit(1);
                    }

                    //Uses the current key and IV for the encryptor
                    //Since we set the key it will use our provided key
                    ICryptoTransform encryptor = provider.CreateEncryptor();
                    using (var fsSourceFile = File.OpenRead(sourceFilename))
                    {
                        using (var fsDestFile = File.Create(destinationFilename))
                        {
                            //Write the IV to the file so we can recover it.
                            fsDestFile.Write(provider.IV, 0, provider.IV.Length);
                            using (var csEncrypt = new CryptoStream(fsDestFile, encryptor, CryptoStreamMode.Write))
                            {
                                fsSourceFile.CopyTo(csEncrypt);
                                Console.WriteLine("[!] Success! File is located at {0}", destinationFilename);
                            }
                        }
                    }
                }
            }
            if (action == "decrypt")
            {
                Console.WriteLine("[*] Decrypting File: {0}", sourceFilename);
                Console.WriteLine("[*] Using Key: {0}", args[2]);

                using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
                {
                    //specify known key but let it make up a random IV
                    //We will be replacing this soon.
                    try
                    {
                        provider.Key = symmKey;
                    }
                    catch (CryptographicException e)
                    {
                        Console.WriteLine("[!] ERROR {0} Please use a key that is one of the following lengths in size:", e.Message);
                        Console.WriteLine("\t16\n\t24\n\t32");
                        System.Environment.Exit(1);
                    }

                    //Uses the current key and IV for the encryptor
                    //Since we set the key it will use our provided key
                    using (var fsSourceFile = File.OpenRead(sourceFilename))
                    {
                        //Need a temp variable for IV, cannot read straight into provider for some reason.
                        //Read the IV file from the encrypted file.
                        var IV = new byte[provider.IV.Length];
                        fsSourceFile.Read(IV, 0, provider.IV.Length);
                        provider.IV = IV;
                        ICryptoTransform decryptor = provider.CreateDecryptor();
                        using (var fsDestFile = File.Create(destinationFilename))
                        {
                            using (var csDecrypt = new CryptoStream(fsSourceFile, decryptor, CryptoStreamMode.Read))
                            {
                                csDecrypt.CopyTo(fsDestFile);
                                Console.WriteLine("[!] Success! File is located at {0}", destinationFilename);
                            }
                        }
                    }
                }
            }
        }
    }
}