using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using ISAI.Lessons.Models.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ISAI.Lessons.Core.Services
{
    public class AzureMediaService
    {

        string _subscriptionId = "eb45aa6b-06eb-4540-8be2-b69950fe9924";
        string _resourceGroup = "Lessons";
        string _accountName = "lessonsmedia";
        string _aadTenantId = "72b8f033-67c4-4524-964e-d3c01efa1acd";
        string _aadClientId = "88030882-6822-4b8d-b8ae-19fe43c38f89";
        string _aadSecret = "6~0ER7Cs_1BBVWLpOgTj~B93VE_lV1~UFC";
        //string _armAadAudienceUrl = "https://management.core.windows.net";
        string _armEndpoint = "https://management.azure.com";
        string _adaptiveStreamingTransformName = "ScottishOnlineLessonsWithAdaptiveStreamingPreset";


        string _issuer = "ScottishOnlineLessons";
        string _audience = "ScottishOnlineLessonsUsers";
        string _primaryKey = "BcrcZvgOBUx1H6JsiIN5lS98NaYxBRAMPwB3PYwqIVv5UoisjMRs0g==";


        public async Task CreateAESLocator(IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName, 
            string locatorName, 
            string contentPolicyName)
        {

            //IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();

            //// Set the polling interval for long running operations to 2 seconds.
            //// The default value is 30 seconds for the .NET client SDK
            //client.LongRunningOperationRetryTimeout = 5;

            var asset = await client.Assets.GetAsync(_resourceGroup, _accountName, assetName);

            if(asset != null)
            {
                Console.WriteLine(asset.AssetId);
                Console.WriteLine(asset.Name);

                var currentLocator = await client.StreamingLocators.GetAsync(_resourceGroup, _accountName, locatorName);

                if (currentLocator != null)
                {
                    await client.StreamingLocators.DeleteAsync(_resourceGroup, _accountName, locatorName);
                    Console.WriteLine("Locator Deleted.");
                }

                var locator = await CreateStreamingLocatorAsync(client, _resourceGroup, _accountName, asset.Name, locatorName, false, true, contentPolicyName);
                Console.WriteLine("Locator created.");
            } 
          

         


        }

            public async Task<EncodeVideoOutput> EncodeFile(string jobId, string inputFilePath, string outputFileName)
        {

            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 5;

            // Creating a unique suffix so that we don't have name collisions if you run the sample
            // multiple times without cleaning up.
            string jobName = $"job-{jobId}";
            string aesStreamingLocatorName = $"aes-streaming-locator-{jobId}";
            string streamingLocatorName = $"streaming-locator-{jobId}";
            string downloadLocatorName = $"download-locator-{jobId}";
            string outputAssetName = $"output-{jobId}";
            string inputAssetName = $"input-{jobId}";

            // Ensure that you have the desired encoding Transform. This is really a one time setup operation.
            var transform = await GetOrCreateTransformAsync(client, _resourceGroup, _accountName, _adaptiveStreamingTransformName);

            // Create a new input Asset and upload the specified local video file into it.
            var asset = await CreateInputAssetAsync(client, _resourceGroup, _accountName, inputAssetName, inputFilePath);

            // Use the name of the created input asset to create the job input.
            var jobInputAsset = new JobInputAsset(assetName: inputAssetName);

            // Output from the encoding Job must be written to an Asset, so let's create one
            Asset outputAsset = await CreateOutputAssetAsync(client, _resourceGroup, _accountName, outputAssetName);

            var job = await SubmitJobAsync(client, _resourceGroup, _accountName, _adaptiveStreamingTransformName, jobName, inputAssetName, outputAsset.Name);
            // In this demo code, we will poll for Job status
            // Polling is not a recommended best practice for production applications because of the latency it introduces.
            // Overuse of this API may trigger throttling. Developers should instead use Event Grid.
            Job waitingJob = await WaitForJobToFinishAsync(client, _resourceGroup, _accountName, _adaptiveStreamingTransformName, jobName);

            if (waitingJob.State == JobState.Finished)
            {
                Console.WriteLine("Job finished.");

                //Dont need download assets here
                //if (!Directory.Exists(outputFileName))
                //{
                //    Directory.CreateDirectory(outputFileName);
                //}
                //await DownloadOutputAssetAsync(client, _resourceGroup, _accountName, outputAsset.Name, outputFileName);

                StreamingLocator streamingLocator = await CreateStreamingLocatorAsync(client, _resourceGroup, _accountName, outputAsset.Name, streamingLocatorName, false);
                StreamingLocator downloadLocator = await CreateStreamingLocatorAsync(client, _resourceGroup, _accountName, outputAsset.Name, downloadLocatorName, true);
                await CreateAESLocator(client, _resourceGroup, _accountName, outputAsset.Name, streamingLocatorName, "scottishonlinelessons");

                return new EncodeVideoOutput()
                {
                    DownloadLocatorId = streamingLocatorName,
                    StreamingLocatorId = downloadLocatorName
                };

                //Delete Input asset
                //await client.Assets.DeleteAsync(_resourceGroup, _accountName, inputAssetName);

                // Note that the URLs returned by this method include a /manifest path followed by a (format=)
                // parameter that controls the type of manifest that is returned. 
                // The /manifest(format=m3u8-aapl) will provide Apple HLS v4 manifest using MPEG TS segments.
                // The /manifest(format=mpd-time-csf) will provide MPEG DASH manifest.
                // And using just /manifest alone will return Microsoft Smooth Streaming format.
                // There are additional formats available that are not returned in this call, please check the documentation
                // on the dynamic packager for additional formats - see https://docs.microsoft.com/azure/media-services/latest/dynamic-packaging-overview
                //IList<string> urls = await GetStreamingUrlsAsync(client, _resourceGroup, _accountName, streamingLocator.Name);
                //return urls;
            }

            return null;

        }

        /// <summary>
        /// Create the ServiceClientCredentials object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        // <GetCredentialsAsync>
        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            // Use ApplicationTokenProvider.LoginSilentWithCertificateAsync or UserTokenProvider.LoginSilentAsync to get a token using service principal with certificate
            //// ClientAssertionCertificate
            //// ApplicationTokenProvider.LoginSilentWithCertificateAsync

            // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symetric key
            ClientCredential clientCredential = new ClientCredential(_aadClientId, _aadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(_aadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }
        // </GetCredentialsAsync>

        /// <summary>
        /// Creates the AzureMediaServicesClient object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        // <CreateMediaServicesClient>
        private async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            var credentials = await GetCredentialsAsync();

            return new AzureMediaServicesClient(new Uri(_armEndpoint), credentials)
            {
                SubscriptionId = _subscriptionId,
            };
        }
        // </CreateMediaServicesClient>

        /// <summary>
        /// Creates a new input Asset and uploads the specified local video file into it.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The asset name.</param>
        /// <param name="fileToUpload">The file you want to upload into the asset.</param>
        /// <returns></returns>
        // <CreateInputAsset>
        private async Task<Asset> CreateInputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string assetName,
            string fileToUpload)
        {
            // In this example, we are assuming that the asset name is unique.
            //
            // If you already have an asset with the desired name, use the Assets.Get method
            // to get the existing asset. In Media Services v3, the Get method on entities returns null 
            // if the entity doesn't exist (a case-insensitive check on the name).

            // Call Media Services API to create an Asset.
            // This method creates a container in storage for the Asset.
            // The files (blobs) associated with the asset will be stored in this container.
            Asset asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions 
            // and the exparation time for the SAS URL.
            var response = await client.Assets.ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWriteDelete,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.  
            BlobContainerClient container = new BlobContainerClient(sasUri);
            BlobClient blob = container.GetBlobClient(Path.GetFileName(fileToUpload));

            // Use Strorage API to upload the file into the container in storage.
            await blob.UploadAsync(fileToUpload, true);

            return asset;
        }
        // </CreateInputAsset>

        /// <summary>
        /// Creates an ouput asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <returns></returns>
        // <CreateOutputAsset>
        private async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            // Check if an Asset already exists
            Asset outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);
            Asset asset = new Asset();
            string outputAssetName = assetName;

            if (outputAsset != null)
            {
                // Name collision! In order to get the sample to work, let's just go ahead and create a unique asset name
                // Note that the returned Asset can have a different name than the one specified as an input parameter.
                // You may want to update this part to throw an Exception instead, and handle name collisions differently.
                string uniqueness = $"-{Guid.NewGuid():N}";
                outputAssetName += uniqueness;

                Console.WriteLine("Warning – found an existing Asset with name = " + assetName);
                Console.WriteLine("Creating an Asset with this name instead: " + outputAssetName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
        }
        // </CreateOutputAsset>

        /// <summary>
        /// If the specified transform exists, get that transform.
        /// If the it does not exist, creates a new transform with the specified output. 
        /// In this case, the output is set to encode a video using one of the built-in encoding presets.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <returns></returns>
        // <EnsureTransformExists>
        private async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName)
        {
            // Does a Transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                // You need to specify what you want it to produce as an output
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput
                    {
                        // The preset for the Transform is set to one of Media Services built-in sample presets.
                        // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            // This sample uses the built-in encoding preset for Adaptive Bitrate Streaming.
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }

            return transform;
        }
        // </EnsureTransformExists>

        /// <summary>
        /// Submits a request to Media Services to apply the specified Transform to a given input video.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The (unique) name of the job.</param>
        /// <param name="inputAssetName">The name of the input asset.</param>
        /// <param name="outputAssetName">The (unique) name of the  output asset that will store the result of the encoding job. </param>
        // <SubmitJob>
        private async Task<Job> SubmitJobAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            string inputAssetName,
            string outputAssetName)
        {
            // Use the name of the created input asset to create the job input.
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName);

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            // In this example, we are assuming that the job name is unique.
            //
            // If you already have a job with the desired name, use the Jobs.Get method
            // to get the existing job. In Media Services v3, the Get method on entities returns null 
            // if the entity doesn't exist (a case-insensitive check on the name).
            Job job = await client.Jobs.CreateAsync(
                resourceGroupName,
                accountName,
                transformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            return job;
        }
        // </SubmitJob>

        /// <summary>
        /// Polls Media Services for the status of the Job.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The name of the job you submitted.</param>
        /// <returns></returns>
        // <WaitForJobToFinish>
        private async Task<Job> WaitForJobToFinishAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName)
        {
            const int SleepIntervalMs = 20 * 1000;

            Job job;
            do
            {
                job = await client.Jobs.GetAsync(resourceGroupName, accountName, transformName, jobName);

                Console.WriteLine($"Job is '{job.State}'.");
                for (int i = 0; i < job.Outputs.Count; i++)
                {
                    JobOutput output = job.Outputs[i];
                    Console.Write($"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == JobState.Processing)
                    {
                        Console.Write($"  Progress (%): '{output.Progress}'.");
                    }

                    Console.WriteLine();
                }

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }
        // </WaitForJobToFinish>

        /// <summary>
        /// Creates a StreamingLocator for the specified asset and with the specified streaming policy name.
        /// Once the StreamingLocator is created the output asset is available to clients for playback.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The name of the output asset.</param>
        /// <param name="locatorName">The StreamingLocator name (unique in this case).</param>
        /// <returns></returns>
        // <CreateStreamingLocator>
        private async Task<StreamingLocator> CreateStreamingLocatorAsync(
            IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName,
            string locatorName, 
            bool isDownload,
            bool isEncrypted = false, 
            string contentPolicyName = null)
        {


            var streamingLocator = new StreamingLocator
            {
                AssetName = assetName,
                StreamingPolicyName = PredefinedStreamingPolicy.DownloadOnly
            };

            if(!isDownload)
            {


                if (isEncrypted)
                {
                    streamingLocator = new StreamingLocator
                    {
                        AssetName = assetName,
                        StreamingPolicyName = PredefinedStreamingPolicy.ClearKey,
                        DefaultContentKeyPolicyName = contentPolicyName
                    };

                } else
                {
                    streamingLocator = new StreamingLocator
                    {
                        AssetName = assetName,
                        StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                    };
                }

            }

         

            StreamingLocator locator = await client.StreamingLocators.CreateAsync(
                resourceGroup,
                accountName,
                locatorName,
                streamingLocator);

            return locator;
        }
        // </CreateStreamingLocator>

        /// <summary>
        /// Checks if the "default" streaming endpoint is in the running state,
        /// if not, starts it.
        /// Then, builds the streaming URLs.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="locatorName">The name of the StreamingLocator that was created.</param>
        /// <returns></returns>
        // <GetStreamingURLs>
        public async Task<IList<string>> GetStreamingUrlsAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            String locatorName,
            StreamingPolicyStreamingProtocol protocol)
        {

            if(client == null) client = await CreateMediaServicesClientAsync();
            if (resourceGroupName == null) resourceGroupName = _resourceGroup;
            if (accountName == null) accountName = _accountName;

            const string DefaultStreamingEndpointName = "default";

            IList<string> streamingUrls = new List<string>();

            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);

            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await client.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
                }
            }

            ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);


   

            if (locatorName.Contains("download"))
            {

                foreach (string path in paths.DownloadPaths)
                {
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Scheme = "https",
                        Host = streamingEndpoint.HostName,

                        Path = path
                    };

                    streamingUrls.Add(uriBuilder.ToString());

                }

            } else
            {

                foreach (StreamingPath path in paths.StreamingPaths)
                {
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Scheme = "https",
                        Host = streamingEndpoint.HostName,

                        Path = path.Paths[0]
                    };

                    if (path.StreamingProtocol == protocol)
                    {
                        streamingUrls.Add(uriBuilder.ToString());
                    }

                    //if (protocol != null)
                    //{
                    //    if (path.StreamingProtocol == protocol)
                    //    {
                    //        streamingUrls.Add(uriBuilder.ToString());
                    //    }
                    //}
                    //else
                    //{
                    //    streamingUrls.Add(uriBuilder.ToString());
                    //}


                }
            }
      

           

            return streamingUrls;
        }
        // </GetStreamingURLs>

        public async Task<Tuple<string, string>> GetEncryptedStreamingUrlsAsync(
           IAzureMediaServicesClient client,
           string resourceGroupName,
           string accountName,
           string locatorName)
        {

            if (client == null) client = await CreateMediaServicesClientAsync();
            if (resourceGroupName == null) resourceGroupName = _resourceGroup;
            if (accountName == null) accountName = _accountName;


            var locator = await client.StreamingLocators.GetAsync(resourceGroupName, accountName, locatorName);

            var tokenSigningKey = Convert.FromBase64String(_primaryKey);
            string keyIdentifier = locator.ContentKeys.First().Id.ToString();

            string token = GetTokenAsync(_issuer, _audience, keyIdentifier, tokenSigningKey);

            string dashPath = await GetDASHStreamingUrlAsync(client, _resourceGroup, _accountName, locatorName);

            return new Tuple<string, string>(dashPath, token);

            //return string.Format("{0}&aes=true&aestoken=Bearer%3D{1}", dashPath, token);

        }



        /// <summary>
        ///  Downloads the results from the specified output asset, so you can see what you got.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset.</param>
        /// <param name="outputFolderName">The name of the folder into which to download the results.</param>
        // <DownloadResults>
        private async Task DownloadOutputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName,
            string outputFolderName)
        {
            if (!Directory.Exists(outputFolderName))
            {
                Directory.CreateDirectory(outputFolderName);
            }

            AssetContainerSas assetContainerSas = await client.Assets.ListContainerSasAsync(
                resourceGroup,
                accountName,
                assetName,
                permissions: AssetContainerPermission.Read,
                expiryTime: DateTime.UtcNow.AddHours(1).ToUniversalTime());

            Uri containerSasUrl = new Uri(assetContainerSas.AssetContainerSasUrls.FirstOrDefault());
            BlobContainerClient container = new BlobContainerClient(containerSasUrl);

            string directory = Path.Combine(outputFolderName, assetName);
            Directory.CreateDirectory(directory);

            Console.WriteLine($"Downloading output results to '{directory}'...");

            string continuationToken = null;
            IList<Task> downloadTasks = new List<Task>();

            do
            {
                var resultSegment = container.GetBlobs().AsPages(continuationToken);

                foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        var blobClient = container.GetBlobClient(blobItem.Name);
                        string filename = Path.Combine(directory, blobItem.Name);

                        downloadTasks.Add(blobClient.DownloadToAsync(filename));
                    }
                    // Get the continuation token and loop until it is empty.
                    continuationToken = blobPage.ContinuationToken;
                }

            } while (continuationToken != "");

            await Task.WhenAll(downloadTasks);

            Console.WriteLine("Download complete.");
        }
        // </DownloadResults>

        /// <summary>
        /// Deletes the jobs, assets and potentially the content key policy that were created.
        /// Generally, you should clean up everything except objects 
        /// that you are planning to reuse (typically, you will reuse Transforms, and you will persist output assets and StreamingLocators).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="accountName"></param>
        /// <param name="transformName"></param>
        /// <param name="jobName"></param>
        /// <param name="assetNames"></param>
        /// <param name="contentKeyPolicyName"></param>
        /// <returns></returns>
        // <CleanUp>
        private async Task CleanUpAsync(
           IAzureMediaServicesClient client,
           string resourceGroupName,
           string accountName,
           string transformName,
           string jobName,
           List<string> assetNames,
           string contentKeyPolicyName = null
           )
        {
            await client.Jobs.DeleteAsync(resourceGroupName, accountName, transformName, jobName);

            foreach (var assetName in assetNames)
            {
                await client.Assets.DeleteAsync(resourceGroupName, accountName, assetName);
            }

            if (contentKeyPolicyName != null)
            {
                client.ContentKeyPolicies.Delete(resourceGroupName, accountName, contentKeyPolicyName);
            }
        }
        // </CleanUp>

        /// <summary>
        /// Checks if the "default" streaming endpoint is in the running state,
        /// if not, starts it.
        /// Then, builds the streaming URLs.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="locatorName">The name of the StreamingLocator that was created.</param>
        /// <returns></returns>
        // <GetMPEGStreamingUrl>
        private static async Task<string> GetDASHStreamingUrlAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string locatorName)
        {
            const string DefaultStreamingEndpointName = "default";

            string dashPath = "";

            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);

            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await client.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
                }
            }

            ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName
                };

                // Look for just the DASH path and generate a URL for the Azure Media Player to playback the content with the AES token to decrypt.
                // Note that the JWT token is set to expire in 1 hour. 
                if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                {
                    uriBuilder.Path = path.Paths[0];

                    dashPath = uriBuilder.ToString();

                }
            }

            return dashPath;
        }
        // </GetMPEGStreamingUrl>

        /// <summary>
        /// Create a token that will be used to protect your stream.
        /// Only authorized clients would be able to play the video.  
        /// </summary>
        /// <param name="issuer">The issuer is the secure token service that issues the token. </param>
        /// <param name="audience">The audience, sometimes called scope, describes the intent of the token or the resource the token authorizes access to. </param>
        /// <param name="keyIdentifier">The content key ID.</param>
        /// <param name="tokenVerificationKey">Contains the key that the token was signed with. </param>
        /// <returns></returns>
        // <GetToken>
        private static string GetTokenAsync(string issuer, string audience, string keyIdentifier, byte[] tokenVerificationKey)
        {
            var tokenSigningKey = new SymmetricSecurityKey(tokenVerificationKey);

            SigningCredentials cred = new SigningCredentials(
                tokenSigningKey,
                // Use the  HmacSha256 and not the HmacSha256Signature option, or the token will not work!
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);

            Claim[] claims = new Claim[]
            {
                new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, keyIdentifier)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.Now.AddMinutes(-5),
                expires: DateTime.Now.AddMinutes(40),
                signingCredentials: cred);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }
        // </GetToken>


    }
}
