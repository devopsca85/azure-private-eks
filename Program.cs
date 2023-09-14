using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Collections.Generic;
using System;
using Pulumi.Azure.Cdn;
using Pulumi.Azure.AppConfiguration;
using Pulumi.Azure.Network;
using Pulumi.Azure.Network.Inputs;
using Pulumi.Azure.ContainerService;
using Pulumi.Azure.ContainerService.Inputs;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.KeyVault;
using Pulumi.Azure.Compute;
using Pulumi.Azure.Compute.Inputs;


return await Pulumi.Deployment.RunAsync(() =>
{
    // Create an Azure Resource Group
        var config = new Pulumi.Config();
    var resourceGroupName = config.Require("resourceGroupName");
var resourceGroupLocation = config.Require("resourceGroupLocation");

var frontdoorProfileName = config.Require("frontdoorProfileName");
var frontdoorProfileSkuName = config.Require("frontdoorProfileSkuName");
var frontdoorProfileEnvironment = config.Require("frontdoorProfileEnvironment");

var virtualNetworkName = config.Require("virtualNetworkName");
var virtualNetworkAddressPrefix = config.Require("virtualNetworkAddressPrefix");

var aksSubnetName = config.Require("aksSubnetName");
var aksSubnetAddressPrefix = config.Require("aksSubnetAddressPrefix");
var vmSubnetName = config.Require("vmSubnetName");
var vmSubnetAddressPrefix = config.Require("vmSubnetAddressPrefix");

var kcName = config.Require("kcName");
var kcDefaultNodePoolName = config.Require("kcDefaultNodePoolName");
var kcDefaultNodePoolVmSize = config.Require("kcDefaultNodePoolVmSize");
var kcDefaultNodePoolNodeCount = config.Require("kcDefaultNodePoolNodeCount");
var kcVersion = config.Require("kcVersion");
var kcDnsPrefix = config.Require("kcDnsPrefix");
var kcIdentityType = config.Require("kcIdentityType");
var kcPrivateClusterEnabled = config.RequireBoolean("kcPrivateClusterEnabled");
var kcNetworkProfilePlugin = config.Require("kcNetworkProfilePlugin");

var networkInterface = config.Require("networkInterface");

var vmName = config.Require("vmName");
var vmSize = config.Require("vmSize");

var vmStorageImageReferencePublisher = config.Require("vmStorageImageReferencePublisher");
var vmStorageImageReferenceOffer = config.Require("vmStorageImageReferenceOffer");
var vmStorageImageReferenceSku = config.Require("vmStorageImageReferenceSku");
var vmStorageImageReferenceVersion = config.Require("vmStorageImageReferenceVersion");

var vmosProfileComputerName = config.Require("vmosProfileComputerName");
var vmosProfileAdminUsername = config.Require("vmosProfileAdminUsername");
var vmosProfileAdminPassword = config.Require("vmosProfileAdminPassword");
var vmosProfiledDisablePasswordAuthentication = config.RequireBoolean("vmosProfiledDisablePasswordAuthentication");

var vmstorageOsDiskCreateOption = config.Require("vmstorageOsDiskCreateOption");
var vmstorageOsDiskName = config.Require("vmstorageOsDiskName");

var natGatewayPublicIp = config.Require("natGatewayPublicIp");
var natAllocationMethod = config.Require("natAllocationMethod");
var natGatewayName = config.Require("natGatewayName");
int natIdleTimeoutInMinutes =Convert.ToInt32( config.Require("natIdleTimeoutInMinutes"));

var appConfigStoreName = config.Require("appConfigStoreName");

var vaultName = config.Require("vaultName");
var vaultObjectId = config.Require("vaultObjectId");
var vaultTenantId = config.Require("vaultTenantId");

    var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
    {
       Location = resourceGroupLocation,
    });
    var frontdoorProfile = new FrontdoorProfile(frontdoorProfileName, new FrontdoorProfileArgs
   {
       ResourceGroupName = resourceGroup.Name,
       SkuName = frontdoorProfileSkuName,
       Tags = {
        { "environment", frontdoorProfileEnvironment },
         },
    }); 
    var vnet = new VirtualNetwork(virtualNetworkName, new VirtualNetworkArgs
{
    AddressSpaces = new[]
   {
      virtualNetworkAddressPrefix,
   },
    
    ResourceGroupName = resourceGroup.Name,
});
 var aksSubnet = new Subnet(aksSubnetName, new SubnetArgs
 {
     AddressPrefixes = aksSubnetAddressPrefix,
     ResourceGroupName = resourceGroup.Name,
     VirtualNetworkName = vnet.Name,
 });
 var vmSubnet = new Subnet(vmSubnetName, new SubnetArgs
{
    AddressPrefixes = vmSubnetAddressPrefix,
    ResourceGroupName = resourceGroup.Name,
    VirtualNetworkName = vnet.Name,
});
var cluster1 = new KubernetesCluster(kcName, new KubernetesClusterArgs
{
    ResourceGroupName = resourceGroup.Name,
    DefaultNodePool = new KubernetesClusterDefaultNodePoolArgs
    {
        Name = kcDefaultNodePoolName,
        VmSize = kcDefaultNodePoolVmSize,
        NodeCount =Convert.ToInt32( kcDefaultNodePoolNodeCount),
    },
    DnsPrefix = kcDnsPrefix,
    Identity = new KubernetesClusterIdentityArgs
    {
        Type = kcIdentityType,
    },
    PrivateClusterEnabled = kcPrivateClusterEnabled,
    
    KubernetesVersion = kcVersion,
});
var publicIp = new PublicIp(natGatewayPublicIp, new PublicIpArgs
{
    ResourceGroupName = resourceGroup.Name,
    Location = resourceGroupLocation,
    AllocationMethod = natAllocationMethod,
    Sku = "Standard",
});
var natGateway = new NatGateway(natGatewayName, new NatGatewayArgs
{
    ResourceGroupName = resourceGroup.Name,
    Location = resourceGroupLocation,
    IdleTimeoutInMinutes = natIdleTimeoutInMinutes,
});
   var vmNic = new NetworkInterface(networkInterface, new Pulumi.Azure.Network.NetworkInterfaceArgs
   {
       ResourceGroupName = resourceGroup.Name,
       IpConfigurations = 
       {
           new NetworkInterfaceIpConfigurationArgs
           {
               Name = "ipconfig",
               SubnetId = vmSubnet.Id,
               PrivateIpAddressAllocation = "Dynamic",
           },
       },
   });
   var vault = new Vault("vault", new VaultArgs
{
    Location = resourceGroupLocation,
    Properties = new VaultPropertiesArgs
    {
        AccessPolicies = new[]
        {
            new AccessPolicyEntryArgs
            {
                ObjectId = vaultObjectId,
                Permissions = new PermissionsArgs
                {
                    Certificates = { "get", "list", "delete", "create", "import", "update", "managecontacts", "getissuers", "listissuers", "setissuers", "deleteissuers", "manageissuers", "recover", "purge" },
                    Keys = { "encrypt", "decrypt", "wrapKey", "unwrapKey", "sign", "verify", "get", "list", "create", "update", "import", "delete", "backup", "restore", "recover", "purge" },
                    Secrets = { "get", "list", "set", "delete", "backup", "restore", "recover", "purge" },
                },
                TenantId = vaultTenantId,
            },
        },
        EnabledForDeployment = true,
        EnabledForDiskEncryption = true,
        EnabledForTemplateDeployment = true,
        PublicNetworkAccess = "Enabled",
        Sku = new Pulumi.AzureNative.KeyVault.Inputs.SkuArgs
        {
            Family = "A",
            Name = Pulumi.AzureNative.KeyVault.SkuName.Standard,
        },
        TenantId = vaultTenantId,
    },
    ResourceGroupName = resourceGroup.Name,
    VaultName = vaultName,
});

    
    
var networkSecurityGroup = new NetworkSecurityGroup("myNetworkSecurityGroup", new NetworkSecurityGroupArgs
{
    ResourceGroupName = resourceGroup.Name,
    SecurityRules = new[]
     {
         new NetworkSecurityGroupSecurityRuleArgs
         {
             Name = "test123",
             Priority = 100,
             Direction = "Inbound",
             Access = "Allow",
             Protocol = "Tcp",
             SourcePortRange = "*",
             DestinationPortRange = "*",
             SourceAddressPrefix = "*",
            DestinationAddressPrefix = "*",
        },
    }
    
});
var virtualMachine = new VirtualMachine("myVirtualMachine", new VirtualMachineArgs
{
    ResourceGroupName = resourceGroup.Name,
    NetworkInterfaceIds = new[]
    {
         vmNic.Id,
    },
    VmSize=vmSize,
    StorageImageReference = new VirtualMachineStorageImageReferenceArgs
     {
      Publisher = "Canonical",
      Offer = "0001-com-ubuntu-server-focal",
      Sku = "20_04-lts",
      Version = "latest",
     },
    StorageOsDisk = new VirtualMachineStorageOsDiskArgs
     {
        Name = "myosdisk1",
        Caching = "ReadWrite",
        CreateOption = "FromImage",
        ManagedDiskType = "Standard_LRS",
     },
        OsProfile = new VirtualMachineOsProfileArgs
     {
        ComputerName = "hostname",
        AdminUsername = "testadmin",
        AdminPassword = "Password1234!",
     },
         OsProfileLinuxConfig = new VirtualMachineOsProfileLinuxConfigArgs
     {
         DisablePasswordAuthentication = false,
     },
         Tags =
     {
         { "environment", "staging" },
     },
});
});