using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTools.Encryption
{
    public class EncrypterDecrypter
    {
        private int[] key;
        private int current = 0;
        private int encryptionLevel = 256;
        private int MaxRange = 256;

        public EncrypterDecrypter()
        {
            int i = 0;
            this.key = new int[this.encryptionLevel];
            Random r = new Random();
            while (i < encryptionLevel)
            {
                this.key[i] = r.Next(1, MaxRange);
                i++;
            }


        }


        public string encrypt(string str)
        {
            string fullyEncryptedValue = "";
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(str.ToCharArray()))
            {
                string strEncrypted = encrypt(b);
                int tmpLength = strEncrypted.Length;
                if (tmpLength <= 15)
                {
                    string hexLength = tmpLength.ToString("X");
                    strEncrypted = hexLength + strEncrypted;
                    
                }
                else
                {
                    //Unexpectedly long value Should never happen as long as the encrypt equation is not altered
                }
                fullyEncryptedValue += strEncrypted;
                
            }
            this.current = 0;

            return fullyEncryptedValue;

        }

        public string decrypt(string str)
        {
            string fullyDecryptedValue = "";
            int totalLength = str.Length;
            while (str.Length > 0)
            {
                int characterLength = int.Parse(str.Substring(0,1), System.Globalization.NumberStyles.HexNumber);
                try
                {
                    int v = int.Parse(str.Substring(1,characterLength));
                    fullyDecryptedValue += decrypt(v);
                }
                catch
                {
                    //Some error about being unable to parse data file
                }
                
                if (str.Length > (characterLength + 1))
                {
                    str = str.Substring(characterLength+1, str.Length - (characterLength+1));
                }
                else
                {
                    str = "";
                }
                 
                
                
            }
            this.current = 0;

            return fullyDecryptedValue;
        }

        private string encrypt(byte bRaw)
        {
            int bEncrypted = (bRaw * key[current]) * 2;

            string sEncrypted = bEncrypted.ToString();

            if (this.current < this.key.Length - 1)
            {
                this.current++;
            }
            else
            {
                this.current = 0;
            }

            return sEncrypted;
        }

        private string decrypt(int iEncrypted)
        {
            int iDecrypted = (iEncrypted/2) / key[current];

            byte bDecrypted = BitConverter.GetBytes(iDecrypted)[0];

            string sDecrypted = (char)bDecrypted + "";

            if (this.current < this.key.Length - 1)
            {
                this.current++;
            }
            else
            {
                this.current = 0;
            }

            return sDecrypted;
        }

        public string getKey()
        {
            int i = 0;
            string skey = "";
            while (i < this.key.Length)
            {
                skey += this.key[i].ToString();
                i++;
            }
            return skey;

        }

        public string getKeyDelimited()
        {
            int i = 0;
            string skey = "";
            while (i < this.key.Length)
            {
                
               skey += this.key[i].ToString() + "-";
               i++;
            }
            skey = skey.Substring(0, skey.Length - 1);
            return skey;
        }

        public void setKey(string s)
        {
            s = s + "-";
            int i = 0;
            List<int> tmpInt = new List<int>();
            string singleChar = "";
            string next ="";
            while (i < s.Length)
            {
                singleChar = s.Substring(i, 1);
                if (singleChar != "-")
                {
                    next += s.Substring(i, 1);
                }
                else
                {
                    tmpInt.Add(int.Parse(next));
                    next ="";
                }
                i++;
            }

            this.key = new int[tmpInt.Count];
            this.key = tmpInt.ToArray();

        }

        public bool encryptToFile(string s, string fileName)
        {
            bool success = false;
            string encryptedS = this.encrypt(s);
            try
            {
                System.IO.File.WriteAllText(fileName, encryptedS);
                success = true;
            }
            catch
            {
                //IO error
            }
            return success;
        }

        public string decryptFile(string fileName)
        {

            string text = "";
            try
            {
                text = System.IO.File.ReadAllText(fileName);
                text = this.decrypt(text);

            }
            catch
            {
                //IO error
            }
            return text;
        }

        public bool saveKeyToFile(string fileName)
        {
            bool success = false;
            string skey = this.getKeyDelimited();
            try
            {
                System.IO.File.WriteAllText(fileName + "k", skey);
                success = true;
            }
            catch
            {
                //IO error
            }
            return success;
        }

    }
}
