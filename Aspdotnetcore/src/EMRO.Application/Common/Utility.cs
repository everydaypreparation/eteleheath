using Abp.Runtime.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EMRO.Common
{
    public class Utility
    {
        private static Random random = new Random();

        public const string
            UPLOAD_FILE_PATH = "UplodedFilePath:DocumentPath",
            ENCRYPTION_KEY = "FileEncrptionKey:EncrptionKey";

        public static readonly string[] DATE_FORMATE_ARRAY = { "yyyy/MM/dd", "MM/dd/yyyy", "yyyy-MM-dd", "MM-dd-yyyy", "yyyy.MM.dd", "MM.dd.yyyy", "yyyyMMdd", "yyyy/MM/dd HH:mm", "MM/dd/yyyy HH:mm", "yyyy-MM-dd HH:mm", "MM-dd-yyyy HH:mm", "yyyy.MM.dd HH:mm", "MM.dd.yyyy HH:mm", "yyyy/MM/dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "MM-dd-yyyy HH:mm:ss", "yyyy.MM.dd HH:mm:ss", "MM.dd.yyyy HH:mm:ss", "yyyy/M/d", "M/d/yyyy", "yyyy-M-d", "M-d-yyyy", "yyyy.M.d", "M.d.yyyy", "yyyyMd", "yyyy/M/d HH:mm", "M/d/yyyy HH:mm", "yyyy-M-d HH:mm", "M-d-yyyy HH:mm", "yyyy.M.d HH:mm", "M.d.yyyy HH:mm", "yyyy/M/d HH:mm:ss", "M/d/yyyy HH:mm:ss", "yyyy/MM/dd H:mm", "MM/dd/yyyy H:mm", "yyyy-MM-dd H:mm", "MM-dd-yyyy H:mm", "yyyy.MM.dd H:mm", "MM.dd.yyyy H:mm", "yyyy/MM/dd H:mm:ss", "MM/dd/yyyy H:mm:ss", "yyyy-MM-dd H:mm:ss", "MM-dd-yyyy H:mm:ss", "yyyy.MM.dd H:mm:ss", "MM.dd.yyyy H:mm:ss", "yyyy/M/d H:mm", "M/d/yyyy H:mm", "yyyy-M-d H:mm", "M-d-yyyy H:mm", "yyyy.M.d H:mm", "M.d.yyyy H:mm", "yyyy/M/d H:mm:ss", "M/d/yyyy H:mm:ss", "MM/D/YYYY", "MM-D-YYYY", "MM.D.YYYY", "M/DD/YYYY", "M-DD-YYYY", "M.DD.YYYY", "MM/D/YYYY HH:mm:ss", "MM/D/YYYY H:mm:ss", "MM/D/YYYY H:mm", "MM/D/YYYY HH:mm", "MM-D-YYYY HH:mm:ss", "MM-D-YYYY H:mm:ss", "MM-D-YYYY H:mm", "MM-D-YYYY HH:mm", "MM.D.YYYY HH:mm:ss", "MM.D.YYYY H:mm:ss", "MM.D.YYYY HH:mm", "MM.D.YYYY H:mm", "M/DD/YYYY HH:mm", "M/DD/YYYY H:mm", "M/DD/YYYY HH:mm:ss", "M/DD/YYYY H:mm:ss", "M-DD-YYYY HH:mm:ss", "M-DD-YYYY H:mm:ss", "M-DD-YYYY HH:mm", "M-DD-YYYY H:mm", "M.DD.YYYY HH:mm:ss", "M.DD.YYYY H:mm:ss", "M.DD.YYYY HH:mm", "M.DD.YYYY H:mm", "yyyy.M.d HH:mm:ss", "M-d-yyyy H:mm:ss", "M.d.yyyy H:mm:ss" };

        public static bool EncryptFile(string inputFilePath, string outputfilePath, string EncryptionKey)
        {
            bool fileEncryptionStatus = false;
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (FileStream fsOutput = new FileStream(outputfilePath, FileMode.Create))
                    {
                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                            {
                                int data;
                                while ((data = fsInput.ReadByte()) != -1)
                                {
                                    cs.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
                fileEncryptionStatus = true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                fileEncryptionStatus = false;
            }
            return fileEncryptionStatus;
        }



        public static byte[] DecryptFile(string inputFilePath, string EncryptionKey)
        {
            MemoryStream ms1 = new MemoryStream();
            try
            {
                string fileDecryptionStatus = string.Empty;
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                    {
                        using (CryptoStream cs = new CryptoStream(fsInput, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var ms = new MemoryStream())
                            {
                                int data;
                                while ((data = cs.ReadByte()) != -1)
                                {
                                    ms.WriteByte((byte)data);
                                }

                                return ms.ToArray();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string stackTrace = ex.StackTrace;
                return ms1.ToArray();
            }
        }

        public static string CreateRandomPassword(int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }

        public static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".txt": return "text/plain";
                case ".pdf": return "application/pdf";
                case ".doc": return "application/vnd.ms-word";
                case ".docx": return "application/vnd.ms-word";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.ms-excel";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".csv": return "text/csv";
                default: return "";
            }
        }

        public static bool CheckInternetConnection()
        {
            bool IsInternet = false;
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    IsInternet = true;
                }
            }
            catch
            {
                IsInternet = false;
            }

            return IsInternet;
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

    }

    public enum UserType
    {
        Patient,
        FamilyDoctor,
        Consultant,
        Diagnostic,
        Insurance,
        MedicalLegal,
        EmroAdmin
    }
}
