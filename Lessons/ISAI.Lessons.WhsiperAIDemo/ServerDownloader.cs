using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISAI.Lessons.WhsiperAIDemo
{
    internal sealed class ServerDownloader
    {
        private readonly OneDriveDownloader _oneDrive;

        public ServerDownloader(OneDriveDownloader oneDriveDownloader)
        {
            _oneDrive = oneDriveDownloader ?? throw new ArgumentNullException(nameof(oneDriveDownloader));
        }

        /// <summary>
        /// Downloads ZIP files one-by-one from the Azure folder "/automateduploads/",
        /// extracts each to a temporary folder, copies all files (except .mp4 and .mp3)
        /// into "F:\Content\{lessonId} - {lessonName}" where {lessonId} is the leading
        /// integer in the zip filename and {lessonName} is the zip filename without extension.
        /// Afterwards the ZIP is moved to "/automateduploads/processed/" and temporary files are removed.
        /// Files that do not have a leading integer in their filename are skipped.
        /// </summary>
        public async Task DownloadAndCopy()
        {
            const string remoteFolder = "automateduploads";
            const string processedFolder = "automateduploads/processed";
            const string destinationRoot = @"F:\Content";
            //const string destinationRoot = @"C:\Temp\SOL Video Processing\temproot\";

            var files = await _oneDrive.ListFilesAsync(remoteFolder).ConfigureAwait(false);
            if (files == null || files.Count == 0)
            {
                Console.WriteLine($"No files returned from '{remoteFolder}'.");
                return;
            }

            // Only process .zip files one at a time
            foreach (var fileName in files.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()))
            {
                try
                {
                    if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipping '{fileName}': not a .zip");
                        continue;
                    }

                    var baseName = Path.GetFileNameWithoutExtension(fileName);

                    // Extract leading integer lesson id (must appear at start)
                    var m = Regex.Match(baseName, @"^(\d+)");
                    if (!m.Success)
                    {
                        Console.WriteLine($"Skipping '{fileName}': does not start with an integer lesson id.");
                        continue;
                    }

                    var lessonId = m.Groups[1].Value;
                    var destinationFolder = Path.Combine(destinationRoot, $"{lessonId}");

                    // Prepare temporary extraction folder
                    var tempRoot = Path.Combine(Path.GetTempPath(), "ServerDownloader");

                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, recursive: true);

                    Directory.CreateDirectory(tempRoot);
                    var extractFolder = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(extractFolder);

                    string? localZipPath = null;
                    try
                    {
                        // Download the ZIP to a temp path (DownloadFileAsync returns the local path)
                        localZipPath = await _oneDrive.DownloadFileAsync(remoteFolder, fileName).ConfigureAwait(false);

                        if (string.IsNullOrEmpty(localZipPath) || !File.Exists(localZipPath))
                        {
                            Console.WriteLine($"Failed to download remote file '{fileName}'. Aborting further processing.");
                            return;
                        }

                        Console.WriteLine($"Downloaded '{fileName}' -> '{localZipPath}'");

                        // Extract zip to extractFolder (overwrite enabled)
                        Console.WriteLine($"Extracting '{localZipPath}' -> '{extractFolder}'");
                        ZipFile.ExtractToDirectory(localZipPath, extractFolder, overwriteFiles: true);

                        // Ensure destination root exists
                        Directory.CreateDirectory(destinationFolder);

                        // First: recreate directory structure so empty folders are preserved
                        Console.WriteLine($"Creating directory structure under '{destinationFolder}' (preserving empty folders).");
                        foreach (var srcDir in Directory.EnumerateDirectories(extractFolder, "*", SearchOption.AllDirectories))
                        {
                            var relativeDir = Path.GetRelativePath(extractFolder, srcDir);
                            var targetDir = Path.Combine(destinationFolder, relativeDir);
                            Directory.CreateDirectory(targetDir);
                        }

                        // Copy files to destination, excluding .mp4 and .mp3
                        Console.WriteLine($"Copying files to '{destinationFolder}' (excluding .mp4 and .mp3).");
                        foreach (var srcFile in Directory.EnumerateFiles(extractFolder, "*", SearchOption.AllDirectories))
                        {
                            var ext = Path.GetExtension(srcFile);
                            if (ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                ext.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                            {
                                // skip media files
                                continue;
                            }

                            // determine relative path and destination path
                            var relative = Path.GetRelativePath(extractFolder, srcFile);
                            var destPath = Path.Combine(destinationFolder, relative);
                            var destDir = Path.GetDirectoryName(destPath) ?? destinationFolder;
                            Directory.CreateDirectory(destDir);

                            File.Copy(srcFile, destPath, overwrite: true);
                        }

                        Console.WriteLine($"Copy completed for '{fileName}' to '{destinationFolder}'.");

                        // Move remote zip to processed
                        var moved = await _oneDrive.MoveFileAsync(remoteFolder, fileName, processedFolder, overwrite: true).ConfigureAwait(false);
                        if (!moved)
                        {
                            Console.WriteLine($"Failed to move remote file '{fileName}' to '{processedFolder}'. Aborting further processing.");
                            return;
                        }

                        Console.WriteLine($"Moved remote file '{fileName}' to '{processedFolder}'.");
                    }
                    finally
                    {
                        // Cleanup local downloaded zip
                        try
                        {
                            if (!string.IsNullOrEmpty(localZipPath) && File.Exists(localZipPath))
                            {
                                File.Delete(localZipPath);
                            }
                        }
                        catch
                        {
                            // best-effort
                        }

                        // Cleanup extracted temp folder
                        try
                        {
                            if (Directory.Exists(extractFolder))
                                Directory.Delete(extractFolder, recursive: true);
                        }
                        catch
                        {
                            // best-effort
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error processing file '{fileName}': {ex.Message}. Stopping.");
                    return; // stop processing on failure
                }
            }
        }
    }
}