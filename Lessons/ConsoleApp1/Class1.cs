using Azure.ResourceManager.Media.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Azure.Storage.Shared.Protocol;
using Azure.ResourceManager;


namespace ConsoleApp1
{

    public class Download {

        public async Task DownloadAssetAsync(ArmClient client, Uri uri, string assetName, string outputFolderName)
        {

            CloudBlobContainer container = new(uri);



            Console.WriteLine($"Downloading blobs to '{outputFolderName}'...");

            BlobContinuationToken continuationToken = null;
            IList<Task> downloadTasks = new List<Task>();
            long totalBytesToBeDownloaded = 0;

            // Setup the transfer context and track the upload progress
            SingleTransferContext context = new();

            bool Error = false;

            try
            {
                Console.WriteLine("Listing blobs'...");

                // listing blobs
                do
                {
                    BlobResultSegment segment = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, null, continuationToken, null, null);
                    foreach (IListBlobItem blobItem in segment.Results)
                    {
                        if (blobItem is CloudBlockBlob blob)
                        {
                            totalBytesToBeDownloaded += blob.Properties.Length;
                        }
                    }
                    continuationToken = segment.ContinuationToken;
                }
                while (continuationToken != null);

                Console.WriteLine($"Downloading blobs to '{outputFolderName}'...");



                //context.ProgressHandler = new Progress<TransferStatus>((progress) =>
                //{
                //    double percentComplete = 100d * progress.BytesTransferred / totalBytesToBeDownloaded;
                //    DoGridTransferUpdateProgress(percentComplete, response.Id);
                //});

                List<string> listDir = new();

                do
                {
                    BlobResultSegment segment = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, null, continuationToken, null, null);

                    foreach (IListBlobItem blobItem in segment.Results)
                    {
                        if (blobItem is CloudBlockBlob blob)
                        {
                            if (blob.Parent is CloudBlobDirectory blobDir && !string.IsNullOrEmpty(blobDir.Prefix) && !listDir.Contains(blobDir.Prefix))
                            {
                                listDir.Add(blobDir.Prefix); // let's create the directory only one time :-)
                                string pathString = System.IO.Path.Combine(outputFolderName, blobDir.Prefix);

                                Directory.CreateDirectory(pathString);
                            }

                            string filePath = System.IO.Path.Combine(outputFolderName, blob.Name.Replace('/', '\\'));

                            if (File.Exists(filePath))
                            {
                                try
                                {
                                    Console.WriteLine($"File {filePath} already exists. It will be overwritten.");
                                    File.Delete(filePath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);

                                }
                            }

                            await blob.FetchAttributesAsync();
                            DownloadOptions dataMovementDownloadOptions = new();
                            var downloadOptionsCopy = dataMovementDownloadOptions;

                            //// if the MD5 is not existent in the blob, let's disable MD5 verification.
                            if (blob.Properties.ContentMD5 == null)
                            {
                                downloadOptionsCopy.DisableContentMD5Validation = true;
                            }

                            // let's save the operations to restore the upload if needed
                            //listTransferDownloadOperations.Add(new(response.Id, filePath, blob, downloadOptionsCopy));

                            // Download blob
                            downloadTasks.Add(TransferManager.DownloadAsync(blob, filePath, downloadOptionsCopy, context));
                        }
                    }

                    continuationToken = segment.ContinuationToken;
                }
                while (continuationToken != null);

                await Task.WhenAll(downloadTasks);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Download of blobs from asset '{0}' failed !", assetName), true);
                Console.WriteLine(ex);
                Console.WriteLine("IMPORTANT : If you have a low bitrate connection, set the number of parallel operations from Auto to 2. Go to Options/Options/Storage Data Movement Library to change this setting.");

                //DoGridTransferDeclareError(response.Id, ex);
                Error = true;
            }
            // listTransferDownloadCheckpoints.Add(new(response.Id, context.LastCheckpoint, totalBytesToBeDownloaded, outputFolderName));

        }
        
    }

}
