using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.ResourceManager.Models;
using ConsoleApp1;

// Generated from example definition: specification/mediaservices/resource-manager/Microsoft.Media/Metadata/stable/2022-08-01/examples/assets-list-all.json
// this example is just showing the usage of "Assets_List" operation, for the dependent resources, they will have to be created separately.

// get your azure access token, for more details of how Azure SDK get your access token, please refer to https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line
var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
// authenticate your client
ArmClient client = new ArmClient(credential);


// this example assumes you already have this MediaServicesAccountResource created on azure
// for more information of creating MediaServicesAccountResource, please refer to the document of MediaServicesAccountResource
string subscriptionId = "eb45aa6b-06eb-4540-8be2-b69950fe9924";
string resourceGroupName = "Lessons";
string accountName = "lessonsmedia";
ResourceIdentifier mediaServicesAccountResourceId = MediaServicesAccountResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, accountName);
MediaServicesAccountResource mediaServicesAccount = client.GetMediaServicesAccountResource(mediaServicesAccountResourceId);

// get the collection of this MediaAssetResource
MediaAssetCollection collection = mediaServicesAccount.GetMediaAssets();



// invoke the operation and iterate over the result
await foreach (MediaAssetResource item in collection.GetAllAsync())
{


    // the variable item is a resource, you could call other operations on this instance as well
    // but just for demo, we get its data from this resource instance
    MediaAssetData resourceData = item.Data;


        // for demo we just print out the id
        Console.WriteLine($"Succeeded on id: {resourceData.Id}");
}

Console.WriteLine($"Succeeded");