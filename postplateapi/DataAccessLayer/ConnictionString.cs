using EncryptionHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer
{
    public class ConnictionString
    {
        private string conString = "XG070FTV54hiWX5xsoohn2VeSggRwfRJpRQDR45Vtlhxw6riYkdWmWKgEJKoTpzgdrJ1AcoGDrSepBDvt6DUtcxkKR3JQCUBBXUcz0jyH7zfmkcTkEjNxxTOJLtYTbh0b3tt/Yqrg4P7R+b7X5ARKsEbCp13DQ6+JgXNn01S1HLP3pg800jYRKBaO3BKHgc+avN6cp4rck0n6Ga4u2uTToL2DMfZHc7QLkc2w1SwxB9hL8Dg/dUJi3c564JcMAYZJY69uWOyu8DgpqCG5zPp1IAAPlyhhZSAznCvf4Jr32Box7nci9QF2/ryM/+A276a";

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
