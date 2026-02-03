using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
namespace ISAI.Lessons.Mobile.Services
{
    public static class LocalAssetInstaller
    {
        const string RemoteRoot = "wwwroot"; // path inside package
        const string ManifestPath = "wwwroot/asset_manifest.txt"; // packaged path
        

        public static async Task EnsureAssetsCopiedAsync()
        {
            var appData = FileSystem.Current.AppDataDirectory;
            var destRoot = Path.Combine(appData, "wwwroot");
            var markerPath = Path.Combine(destRoot, ".assets_installed");

            if (File.Exists(markerPath))
                return; // already installed

            // Make sure parent exists
            Directory.CreateDirectory(destRoot);

            // Open the manifest from the packaged app
            using (var manifestStream = await FileSystem.OpenAppPackageFileAsync(ManifestPath))
            using (var reader = new StreamReader(manifestStream))
            {
                string relative;
                while ((relative = await reader.ReadLineAsync()) != null)
                {
                    relative = relative.Trim();
                    if (string.IsNullOrEmpty(relative) || relative.StartsWith("#"))
                        continue;

                    // Input packaged path (forward slashes)
                    var packagedPath = Path.Combine(RemoteRoot, relative).Replace('\\', '/');

                    // Open packaged file
                    using var inStream = await FileSystem.OpenAppPackageFileAsync(packagedPath);

                    // Destination path on disk (preserve subfolders)
                    var outPath = Path.Combine(destRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                    var outDir = Path.GetDirectoryName(outPath);
                    if (!Directory.Exists(outDir))
                        Directory.CreateDirectory(outDir);

                    // Write the file
                    using var outFs = File.Create(outPath);
                    await inStream.CopyToAsync(outFs);
                }
            }

            // Create marker
            File.WriteAllText(markerPath, DateTime.UtcNow.ToString("o"));
        }
    }
}

