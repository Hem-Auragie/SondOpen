using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace Application_Sondage.Models
{
    static class CleUnique
    {
        const uint longueur = 128;
        static RNGCryptoServiceProvider random;
        static CleUnique()
        {
            random = new RNGCryptoServiceProvider();
        }

        static public byte[] Generate()
        {
            Contract.Ensures(Contract.Result<string>().Length == longueur);

            byte[] cle = new byte[longueur];
            random.GetBytes(cle);
            return cle;
        }
    }
}