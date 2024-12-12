using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Core;

class Program
{
    static async Task Main(string[] args)
    {
        string subscriptionId = "";
        string resourceGroupName = "";
        string storageAccountName = ""; // Must be globally unique
        string region = "";

        // Authenticate with Azure using DefaultAzureCredential
        var credential = new DefaultAzureCredential();
        
        // Initialize the ARM client
        var armClient = new ArmClient(credential);
        
        // Get the subscription
        var subscription = await armClient.GetDefaultSubscriptionAsync();
        
        // Create or get the resource group
        var resourceGroup = await CreateResourceGroupIfNeeded(subscription, resourceGroupName, region);
        
        // Create the storage account
        await CreateStorageAccount(resourceGroup, storageAccountName, region);
        
        Console.WriteLine("Storage account created successfully!");
    }

    static async Task<ResourceGroupResource> CreateResourceGroupIfNeeded(SubscriptionResource subscription, string resourceGroupName, string region)
    {
        var resourceGroups = subscription.GetResourceGroups();
        var rgExists = await resourceGroups.ExistsAsync(resourceGroupName);

        if (!rgExists)
        {
            Console.WriteLine($"Creating Resource Group: {resourceGroupName} in {region}...");
            var rgData = new ResourceGroupData(region);
            var operation = await resourceGroups.CreateOrUpdateAsync(Azure.WaitUntil.Completed, resourceGroupName, rgData);
            return operation.Value;
        }
        else
        {
            Console.WriteLine($"Resource Group {resourceGroupName} already exists.");
            return await resourceGroups.GetAsync(resourceGroupName);
        }
    }

    static async Task CreateStorageAccount(ResourceGroupResource resourceGroup, string storageAccountName, string region)
    {
        var storageAccounts = resourceGroup.GetStorageAccounts();

        // Configure the storage account
        var parameters = new StorageAccountCreateOrUpdateContent(
            new StorageSku(StorageSkuName.StandardLrs),
            StorageKind.StorageV2,
            region)
        {
            AccessTier = StorageAccountAccessTier.Hot
        };

        // Create the storage account
        var storageAccountOperation = await storageAccounts.CreateOrUpdateAsync(
            Azure.WaitUntil.Completed,
            storageAccountName,
            parameters);

        var storageAccount = storageAccountOperation.Value;
        Console.WriteLine($"Storage Account '{storageAccount.Data.Name}' created in resource group '{resourceGroup.Data.Name}'!");
    }
}