using EncryptionHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer
{
    public class ConnictionString
    {
        private string conString = "xxxxx";

        public string GetConnectionString()
        {
            return EncryptionHelper.EncryptionHelper.Decrypt(conString);
        }

        public string GetConnectionString2()
        {
            return EncryptionHelper.EncryptionHelper.Decrypt(conString);
        }
    }
}
