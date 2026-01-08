using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace WhisperNet
{
    internal static class OneDriveDownloader
    {
        private static readonly HttpClient sharedHttpClient = new HttpClient();

        // Fallback constants (used only when env vars are not provided)
        private const string GraphClientIdFallback = "88030882-6822-4b8d-b8ae-19fe43c38f89";
        private const string GraphTenantIdFallback = "72b8f033-67c4-4524-964e-d3c01efa1acd";
        private const string GraphClientSecret = "THt8Q~-AQqzVroyoi9nxjKHRUAfFv3nxC1K2rcgU";
        private const string GraphTargetUser = "craig.champion@scottishonlinelessons.com";

        private sealed record GraphConfig(string ClientId, string TenantId, string ClientSecret, string TargetUser);

        /// <summary>
        /// Downloads a file from OneDrive and extracts it to <paramref name="destinationFolder"/>.
        /// Returns true on success.
        /// </summary>
        public static async Task<bool> DownloadAndUnzipAsync(string remoteFolder, string remoteFileName, string destinationFolder)
        {
            if (string.IsNullOrWhiteSpace(remoteFileName))
            {
                Console.WriteLine("remoteFileName must be specified.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                Console.WriteLine("destinationFolder must be specified.");
                return false;
            }

            string? zipPath = null;
            try
            {
                zipPath = await DownloadFileAsync(remoteFolder, remoteFileName).ConfigureAwait(false);
                if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
                {
                    Console.WriteLine("Download failed or returned no file.");
                    return false;
                }

                Directory.CreateDirectory(destinationFolder);

                // Overwrite existing files by default
                Console.WriteLine($"Extracting '{zipPath}' to '{destinationFolder}' (overwrite enabled).");
                ZipFile.ExtractToDirectory(zipPath, destinationFolder, overwriteFiles: true);

                Console.WriteLine("Extraction completed.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DownloadAndUnzip failed: {ex.Message}");
                return false;
            }
            finally
            {
                // Best-effort cleanup of temporary zip
                try
                {
                    if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                        File.Delete(zipPath);
                }
                catch
                {
                    // ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Downloads the specified file from OneDrive and returns the local temp path.
        /// Returns null on failure.
        /// </summary>
        private static async Task<string?> DownloadFileAsync(string remoteFolder, string remoteFileName)
        {
            GraphConfig config = GetGraphConfig();

            if (string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.TenantId))
            {
                Console.WriteLine("GRAPH_CLIENT_ID and GRAPH_TENANT_ID must be set in environment variables.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(config.TargetUser))
            {
                Console.WriteLine("GRAPH_CLIENT_SECRET is set; GRAPH_TARGET_USER must also be set for app-only downloads.");
                return null;
            }

            bool authOk = await AuthenticateAndSetHeaderAsync(config).ConfigureAwait(false);
            if (!authOk)
                return null;

            try
            {
                var remotePath = NormalizeRemoteFolder(remoteFolder);
                string escapedFile = Uri.EscapeDataString(remoteFileName);
                string downloadUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(config.TargetUser)}/drive/root:{Uri.EscapeDataString(remotePath + "/" + remoteFileName)}:/content";

                Console.WriteLine($"Requesting download URL: {downloadUrl}");

                using var resp = await sharedHttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Download request failed: {resp.StatusCode} - {body}");
                    return null;
                }

                string tempFileName = $"{Path.GetFileNameWithoutExtension(remoteFileName)}_{Guid.NewGuid():N}{Path.GetExtension(remoteFileName)}";
                string tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

                await using (var responseStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await responseStream.CopyToAsync(fs).ConfigureAwait(false);
                }

                Console.WriteLine($"Downloaded file saved to: {tempPath}");
                return tempPath;
            }
            catch (MsalException mex)
            {
                Console.WriteLine($"Authentication error while downloading: {mex.Message}");
                return null;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine($"Network error while downloading: {hex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while downloading: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lists file names in the specified OneDrive remote folder.
        /// Returns a read-only list of file names, or null on failure.
        /// </summary>
        public static async Task<IReadOnlyList<string>?> ListFilesAsync(string remoteFolder)
        {
            GraphConfig config = GetGraphConfig();

            if (string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.TenantId))
            {
                Console.WriteLine("GRAPH_CLIENT_ID and GRAPH_TENANT_ID must be set in environment variables.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(config.TargetUser))
            {
                Console.WriteLine("GRAPH_CLIENT_SECRET is set; GRAPH_TARGET_USER must also be set for app-only operations.");
                return null;
            }

            bool authOk = await AuthenticateAndSetHeaderAsync(config).ConfigureAwait(false);
            if (!authOk)
                return null;

            try
            {
                string remotePath = NormalizeRemoteFolder(remoteFolder);
                string requestUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(config.TargetUser)}/drive/root:{Uri.EscapeDataString(remotePath)}:/children?$select=name,file,folder";

                Console.WriteLine($"Requesting list URL: {requestUrl}");

                using var resp = await sharedHttpClient.GetAsync(requestUrl).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"List request failed: {resp.StatusCode} - {body}");
                    return null;
                }

                using var contentStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var doc = await JsonDocument.ParseAsync(contentStream).ConfigureAwait(false);

                var results = new List<string>();

                if (doc.RootElement.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in valueElement.EnumerateArray())
                    {
                        // Only include actual files (items that have a 'file' property).
                        if (item.TryGetProperty("file", out _))
                        {
                            if (item.TryGetProperty("name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
                            {
                                string? name = nameElement.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    results.Add(name);
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Found {results.Count} file(s) in '{remotePath}'.");
                return results.AsReadOnly();
            }
            catch (MsalException mex)
            {
                Console.WriteLine($"Authentication error while listing files: {mex.Message}");
                return null;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine($"Network error while listing files: {hex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while listing files: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Uploads a file to the specified OneDrive remote folder.
        /// Returns true on success.
        /// </summary>
        public static async Task<bool> UploadFileToOneDriveAsync(string localFilePath, string remoteFolder)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
            {
                Console.WriteLine("localFilePath must be specified.");
                return false;
            }

            if (!File.Exists(localFilePath))
            {
                Console.WriteLine($"Local file not found: {localFilePath}");
                return false;
            }

            GraphConfig config = GetGraphConfig();

            if (string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.TenantId))
            {
                Console.WriteLine("GRAPH_CLIENT_ID and GRAPH_TENANT_ID must be set in environment variables.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.TargetUser))
            {
                Console.WriteLine("GRAPH_CLIENT_SECRET is set; GRAPH_TARGET_USER must also be set for app-only operations.");
                return false;
            }

            bool authOk = await AuthenticateAndSetHeaderAsync(config).ConfigureAwait(false);
            if (!authOk)
                return false;

            try
            {
                string remotePath = NormalizeRemoteFolder(remoteFolder);
                string fileName = Path.GetFileName(localFilePath);
                long fileSize = new FileInfo(localFilePath).Length;

                // If file is small, use simple PUT upload to /content endpoint
                const long SimpleUploadLimit = 4L * 1024L * 1024L; // 4 MB

                if (fileSize <= SimpleUploadLimit)
                {
                    string uploadUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(config.TargetUser)}/drive/root:{Uri.EscapeDataString(remotePath + "/" + fileName)}:/content";
                    Console.WriteLine($"Uploading (simple) to: {uploadUrl}");

                    await using var fs = File.OpenRead(localFilePath);
                    using var content = new StreamContent(fs);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    using var putResp = await sharedHttpClient.PutAsync(uploadUrl, content).ConfigureAwait(false);
                    if (!putResp.IsSuccessStatusCode)
                    {
                        string body = await putResp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Console.WriteLine($"Simple upload failed: {putResp.StatusCode} - {body}");
                        return false;
                    }

                    Console.WriteLine("Simple upload completed.");
                    return true;
                }

                // For larger files use an upload session (resumable)
                // Replace site/path construction with:
                string itemPathForGraph = remotePath == "/" ? $"/{fileName}" : $"{remotePath}/{fileName}";
                string sessionUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(config.TargetUser)}/drive/root:{Uri.EscapeDataString(itemPathForGraph)}:/createUploadSession";

                string sessionRequestBody = "{\"item\": {\"@microsoft.graph.conflictBehavior\": \"replace\"}}";
                using var sessionContent = new StringContent(sessionRequestBody, System.Text.Encoding.UTF8, "application/json");
                using var sessionResp = await sharedHttpClient.PostAsync(sessionUrl, sessionContent).ConfigureAwait(false);

                if (!sessionResp.IsSuccessStatusCode)
                {
                    string body = await sessionResp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Create upload session failed: {sessionResp.StatusCode} - {body}");
                    return false;
                }

                using var sessionStream = await sessionResp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var sessionDoc = await JsonDocument.ParseAsync(sessionStream).ConfigureAwait(false);

                if (!sessionDoc.RootElement.TryGetProperty("uploadUrl", out JsonElement uploadUrlElement) || uploadUrlElement.ValueKind != JsonValueKind.String)
                {
                    Console.WriteLine("Upload session response did not contain an uploadUrl.");
                    return false;
                }

                string uploadUrlResumable = uploadUrlElement.GetString() ?? string.Empty;
                if (string.IsNullOrEmpty(uploadUrlResumable))
                {
                    Console.WriteLine("Upload session returned an empty uploadUrl.");
                    return false;
                }

                Console.WriteLine($"Resumable upload URL acquired. Uploading {fileSize} bytes in chunks.");

                const int ChunkSize = 5 * 1024 * 1024; // 5 MB
                byte[] buffer = new byte[ChunkSize];

                await using var fileStream = File.OpenRead(localFilePath);
                long offset = 0;
                int read;

                while ((read = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
                {
                    long chunkStart = offset;
                    long chunkEnd = offset + read - 1;
                    long total = fileSize;

                    using var chunkContent = new ByteArrayContent(buffer, 0, read);
                    chunkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    chunkContent.Headers.Add("Content-Range", $"bytes {chunkStart}-{chunkEnd}/{total}");

                    using var req = new HttpRequestMessage(HttpMethod.Put, uploadUrlResumable)
                    {
                        Content = chunkContent
                    };

                    using var chunkResp = await sharedHttpClient.SendAsync(req).ConfigureAwait(false);

                    if (chunkResp.IsSuccessStatusCode)
                    {
                        // Completed (201 Created or 200 OK) or accepted (202)
                        if (chunkResp.StatusCode == System.Net.HttpStatusCode.Created ||
                            chunkResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine("Resumable upload completed.");
                            return true;
                        }

                        // 202 Accepted -> continue
                        if (chunkResp.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            offset += read;
                            Console.WriteLine($"Uploaded chunk {chunkStart}-{chunkEnd}. Progress: {offset}/{total}");
                            continue;
                        }
                    }

                    // Non-success -> read body and fail
                    string respBody = await chunkResp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Chunk upload failed: {chunkResp.StatusCode} - {respBody}");
                    return false;
                }

                Console.WriteLine("Resumable upload finished loop without explicit completion response; assuming success.");
                return true;
            }
            catch (MsalException mex)
            {
                Console.WriteLine($"Authentication error while uploading: {mex.Message}");
                return false;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine($"Network error while uploading: {hex.Message}");
                return false;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Upload cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while uploading: {ex.Message}");
                return false;
            }
        }

        private static GraphConfig GetGraphConfig()
        {
            string clientId = Environment.GetEnvironmentVariable("GRAPH_CLIENT_ID") ?? GraphClientIdFallback;
            string tenantId = Environment.GetEnvironmentVariable("GRAPH_TENANT_ID") ?? GraphTenantIdFallback;
            string clientSecret = Environment.GetEnvironmentVariable("GRAPH_CLIENT_SECRET") ?? GraphClientSecret;
            string targetUser = Environment.GetEnvironmentVariable("GRAPH_TARGET_USER") ?? GraphTargetUser;

            return new GraphConfig(clientId, tenantId, clientSecret, targetUser);
        }

        private static async Task<bool> AuthenticateAndSetHeaderAsync(GraphConfig config)
        {
            try
            {
                var confidentialApp = ConfidentialClientApplicationBuilder
                    .Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithTenantId(config.TenantId)
                    .Build();

                string[] appScopes = new[] { "https://graph.microsoft.com/.default" };
                AuthenticationResult authResult = await confidentialApp.AcquireTokenForClient(appScopes).ExecuteAsync().ConfigureAwait(false);

                sharedHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                return true;
            }
            catch (MsalException mex)
            {
                Console.WriteLine($"Authentication error: {mex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected authentication error: {ex.Message}");
                return false;
            }
        }

        private static string NormalizeRemoteFolder(string remoteFolder)
        {
            var remotePath = remoteFolder?.Trim() ?? "/";
            if (!remotePath.StartsWith('/')) remotePath = "/" + remotePath;
            if (remotePath != "/") remotePath = remotePath.TrimEnd('/');

            // Graph path requires leading slash for path notation; return "/" when root
            return remotePath == "/" ? "/" : remotePath;
        }
    }
}