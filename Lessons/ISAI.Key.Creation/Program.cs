using Effortless.Net.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Key.Creation
{
    class Program
    {
        static void Main(string[] args)
        {

            byte[] key = Bytes.GenerateKey();
            byte[] iv = Bytes.GenerateIV();

            var stringKey = Convert.ToBase64String(key);
            var stringIv = Convert.ToBase64String(iv);

            var b = true;


        }
    }
}
