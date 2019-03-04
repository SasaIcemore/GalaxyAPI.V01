using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Tools.RSA
{
    public static class RSAHelper
    {
        //CreateKey运行报错，直接写死
        public const string PUBLIC_KEY = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCFFdxHTS7310Sxyl/+xmKmY1XU
                                            ZzpVLiGeLQ/sTA4f2Ge9t5HVRdmyVsdMP5egpWiY/GhcgSNLDbLVX8J4LRCvMf4b
                                            jw4CjjAzh7P8ZJQOurFGUIV6btB/+7L8cbA5JrxVuj8r/BTW9Pwkv9kbVH3Hit2y
                                            LcvArcxiC9WtJn+bTwIDAQAB";

        public const string PRIVATE_KEY = @"MIICXAIBAAKBgQCFFdxHTS7310Sxyl/+xmKmY1XUZzpVLiGeLQ/sTA4f2Ge9t5HV
                                            RdmyVsdMP5egpWiY/GhcgSNLDbLVX8J4LRCvMf4bjw4CjjAzh7P8ZJQOurFGUIV6
                                            btB/+7L8cbA5JrxVuj8r/BTW9Pwkv9kbVH3Hit2yLcvArcxiC9WtJn+bTwIDAQAB
                                            AoGAXxr6KfIcaHzr2GoZI8a95M4EHiAO7aRj3deyE1lelp9ds5QloVWjdvDQx8qC
                                            HwlVsE5gfgrDH/WQIS+nl54M1zngcmOvUCKbzPhRvk0ggMTf1A7UlNbz7R6/nd+/
                                            2lt1805V2sMAVJeqQTPa7kChu3aLYMgGdQKCjlgtOEhgG1ECQQCZGfzibDTFCP5V
                                            9CpWAOg6wdi62sxg4MSkIsAEnaQTOqzhPC+AlfAJFMwfHrwRTuixOawSA4DBJYzz
                                            5zqNIdLVAkEA3of7RmrWbDJfNMulVBPYXA1rxvezAmB8hZK5pbP5Z67WmPbeCtrg
                                            iX+B1LAr+IboQBfMwuV83cZ5/cBKDbzfkwJBAIVchNK6jNDhP6KhuEsIKUIdXuuM
                                            8aClycRr3LVol+aEItljssEgWmGesxucixfMk5bk9MUQNhSI4BoJXhBX2S0CQEED
                                            g3MYRZsIwG4BTUUaIy0JssJHneIE5Hx12I1D3orfNs0r2sVbGxywxvKnocETUQlg
                                            qq8KMwfsM7niEwCefMcCQD7jyUyC0rW9AKlunB+cLUlynVfdrv0zFww4YyzJHEm9
                                            dFJEGLIsp09gzh3H7sV+qo9XGxIYCa9xXajRC9xB5Js=";

        //https://www.cnblogs.com/csqb-511612371/p/4898437.html
        //生成公私钥
        public static Dictionary<string, string> CreateKey()
        {
            var keyDic = new Dictionary<string, string>();
            var rsa = new RSACryptoServiceProvider(1024);
            RSAParameters parameter = rsa.ExportParameters(true);
            keyDic.Add("PUBLIC", BytesToHexString(parameter.Exponent) + "," + BytesToHexString(parameter.Modulus));
            keyDic.Add("PRIVATE", rsa.ToXmlString(true));//not support
            return keyDic;
        }

        
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="str_Cypher_Text">密文</param>
        /// <param name="str_Private_Key">私钥</param>
        /// <returns></returns>
        static public string DecryptRSA(string encryptData, string privateKey)
        {
            string decryptData = "";
            try
            {
                var provider = new RSACryptoServiceProvider();
                provider.FromXmlString(privateKey);

                byte[] result = provider.Decrypt(HexStringToBytes(encryptData), false);
                ASCIIEncoding enc = new ASCIIEncoding();
                decryptData = enc.GetString(result);
            }
            catch (Exception e)
            {
                throw new Exception("RSA解密出错!", e);
            }
            return decryptData;
        }

        private static string BytesToHexString(byte[] input)
        {
            StringBuilder hexString = new StringBuilder(64);

            for (int i = 0; i < input.Length; i++)
            {
                hexString.Append(String.Format("{0:X2}", input[i]));
            }
            return hexString.ToString();
        }

        public static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[] { 0 };
            }

            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                result[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            return result;
        }
    }
}
