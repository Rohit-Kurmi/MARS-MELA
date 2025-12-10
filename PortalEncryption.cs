using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using MPOEncrypt;
using MPODecrypt;
using System.Data;

namespace PortalLib.Framework.Utilities
{
	/// <summary>
	/// Summary description for PortalEncryption.
	/// </summary>
    /// 
    #region New Class definition
    public class PortalEncryption
    {
        public static string EncryptPassword(string password)
        {
            return PortalEncyptionManager.EncryptionManager.EncryptData(password);
        }



        public static string DecryptPassword(string password)
        {
            try
            {
            return PortalEncyptionManager.EncryptionManager.DecryptData(password);
            }
            catch { return password; }
        }



        public string testthis(string password)
        {
            return PortalEncyptionManager.EncryptionManager.EncryptData(password);
        }



        public static string EncryptPasswordNew(string password)
        {
            return PortalEncyptionManager.EncryptionManager.EncryptDataNew(password);
        }


        public static string DecryptPasswordNew(string password)
        {
            return PortalEncyptionManager.EncryptionManager.DecryptDataNew(password);
        }



        public static string Encrypto(string PlainText)
        {
            
            return CustomEncode.EncryptStr(PlainText, "FDA");
        }


        public static DataTable Encrypto(DataTable dataTable, string[] CoumnNamesArray)
        {
            return CustomEncode.EncryptDataTable(dataTable, "FDA", CoumnNamesArray);
        }


        public static string Decrypto(string CypherText)
        {
            return CustomDecode.DecryptStr(CypherText, "FDA");
        }


        public static string Decrypto_KIOSK(string CypherText)
        {
            return CustomDecode.DecryptStr(CypherText, "KRF");
        }


        public static DataTable Decrypto(DataTable dataTable, string[] ColumnNamesArray)
        {
            return CustomDecode.DecryptDataTable(dataTable, "FDA", ColumnNamesArray);
            //return PortalEncryption.Decrypto(dataTable, ColumnNamesArray);

        }



        #region new code added on 03-May-2022 for SHA 512 Encryption
        public static string GetSHA512(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }


        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
        #endregion
        
    }

    #endregion

    #region OLD class definition
    //public class PortalEncryption
    //{
    //    const string KEY = "1$F%C&4^5@xXp4p#783sd&%^5ngj!s";  // Secret key
    //    const int KEY_SIZE = 32;  //256 bit --  vary depending on Crypto Service Provider
    //    const int BLOCK_SIZE = 16; // 128 bit -- vary depending on Crypto Service Provider
    //    static private Byte[] m_Key = new Byte[KEY_SIZE];  // KEY USED FOR ENCRYPTION
    //    static private Byte[] m_IV = new Byte[BLOCK_SIZE]; // IV USED AS INITIALIZATION VECTOR

    //    /// <summary>
    //    /// Function to encrypt data and returns cipher text
    //    /// </summary>
    //    /// <param name="data">plaintext</param>
    //    /// <returns>cipher text</returns>
    //    private static string EncryptData(string data)
    //    {
    //        string strResult= null;
    //        try
    //        {
    //            //1.Initialise the keys.
    //            if (!InitKey())
    //            {
    //                throw new ApplicationException("Error. Fail to generate key for encryption");				
    //            }			
    //            //2. Create CryptoServiceProvider
    //            RijndaelManaged rijndalCSP = new RijndaelManaged();			
    //            ICryptoTransform rijndalEncrypt = rijndalCSP.CreateEncryptor(m_Key, m_IV); 

    //            //3. Encode string to bytes									
    //            byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(data);  // input data bytes.
    //            MemoryStream memoryStream = new MemoryStream();              // memorySream is the output stream.
    //            CryptoStream cs = new CryptoStream(memoryStream, rijndalEncrypt, CryptoStreamMode.Write); //	 cs is the transformation stream.

    //            //5. Start performing the encryption
    //            cs.Write(dataBytes,0,dataBytes.Length);
    //            cs.FlushFinalBlock();				

    //            //6. Returns the encrypted result after it is base64 encoded
    //            //	In this case, the actual result is converted to base64 so that it can be stored in database and can be passed through any channel.			
    //            strResult = Convert.ToBase64String(memoryStream.ToArray());	
    //            cs.Close();
    //            memoryStream.Close();
    //        }
    //        catch(Exception e)
    //        {
    //            throw new ApplicationException("Server temporarily down.Try later.");
    //        }
    //        return strResult;
    //    }

    //    /// <summary>
    //    /// Function to decrypt data takes encoded cipher text
    //    /// </summary>
    //    /// <param name="data">cipher text</param>
    //    /// <returns>returns plaintext</returns>
    //    private static string DecryptData(string data)
    //    {
    //        string strResult;

    //        try
    //        {
    //            //1.Initialise the keys.
    //            if(!InitKey())
    //            {
    //                throw new ApplicationException("Error. Fail to generate key for decryption");				
    //            }

    //            //2. Initialize the service provider
    //            RijndaelManaged rijndalCSP = new RijndaelManaged();				
    //            ICryptoTransform rijndalDecrypt = rijndalCSP.CreateDecryptor(m_Key, m_IV);

    //            //3. Prepare the streams:
    //            // Remember to revert the base64 encoding into a byte array to restore the original encrypted data stream
    //            byte[] dateBytes = Convert.FromBase64String(data);
    //            MemoryStream memoryStream = new MemoryStream(dateBytes); //	memoryStream is the input stream. 
    //            CryptoStream cs = new CryptoStream(memoryStream, rijndalDecrypt, CryptoStreamMode.Read);	 //	cs is the transformation stream.

    //            //4. read from stream to string
    //            strResult = new StreamReader(cs).ReadToEnd();
    //            cs.Close();
    //            memoryStream.Close();
    //        }
    //        catch(Exception e)
    //        {
    //            throw new ApplicationException("Server temporarily down.Try later.");
    //        }
    //        return strResult;
    //    }


    //    /////////////////////////////////////////////////////////////
    //    //Private function to generate the keys into member variables
    //    static private bool InitKey()
    //    {
    //        try
    //        {		
    //            //1.Convert Key to byte array
    //            byte[] keyBytes = ASCIIEncoding.ASCII.GetBytes(KEY);

    //            //2.Hash the key using SHA256 
    //            SHA256Managed sha256Managed = new SHA256Managed();					
    //            byte[] keyBytesHash = sha256Managed.ComputeHash(keyBytes); // generate 256 bit hash value
    //            keyBytesHash = sha256Managed.ComputeHash(keyBytesHash);    // generates again hash for hash value

    //            Array.Copy(keyBytesHash,0,m_Key,0,KEY_SIZE);	 
    //            Array.Copy(m_Key,0,m_IV,0,BLOCK_SIZE);		
    //            Array.Sort(m_IV);

    //            return true;
    //        }
    //        catch (Exception e)
    //        {
    //            //Error Performing Operations
    //            return false;
    //        }
    //    }

    //    public static string EncryptPassword(string password)
    //    {
    //        return	EncryptData(password);
    //    }
    //    public static string DecryptPassword(string password)
    //    {
    //        return	DecryptData(password);
    //    }

    //    public string testthis(string password)
    //    {
    //        return EncryptData(password);
    //    }
    //}

    #endregion

}
