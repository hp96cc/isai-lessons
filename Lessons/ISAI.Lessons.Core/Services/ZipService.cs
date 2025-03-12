using ICSharpCode.SharpZipLib.Zip;

namespace ISAI.Lessons.Core.Services
{
    public static class ZipService
    {

        static string _password = "k/u%SaM#D6)vcO|ht&D3%KmBc91w";

        public static void ZipFolder(string fileName, string sourceDirectory, string password = null)
        {

            if (password == null)
            {
                password = _password;
            }

            FastZip zipFile = new FastZip();
            zipFile.Password = password;
            zipFile.CreateEmptyDirectories = true;
            zipFile.CreateZip(fileName, sourceDirectory, true, null);

        }


        public static void ExtractZipFile(string zipFileName, string targetDirectory, string password = null)
        {
            if (password == null)
            {
                password = _password;
            }

            FastZip zipFile = new FastZip();
            zipFile.Password = password;
            zipFile.ExtractZip(zipFileName, targetDirectory, null);

        }

    }
}
