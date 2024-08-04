using CryptoNet;
using Effortless.Net.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Key.Creation
{
    class Program
    {
        static void Main(string[] args)
        {
            string privateKeyFile = @"c:\apps\videokey.private";
            string publicKeyFile = @"c:\apps\videokey.public";
            string aesKeyFile = @"c:\apps\videokey.aes";

            ICryptoNet cryptoNet = new CryptoNetRsa();

            cryptoNet.ExportKeyAndSave(new FileInfo(privateKeyFile), true);
            cryptoNet.ExportKeyAndSave(new FileInfo(publicKeyFile), false);


            ICryptoNet cryptoNetAes = new CryptoNetAes();
            var key = cryptoNetAes.ExportKey();
            cryptoNetAes.ExportKeyAndSave(new FileInfo(aesKeyFile));

            var b = true;


        }
    }
}
