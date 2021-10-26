
# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

<#
.Synopsis
Creates or updates a Custom Location in the specified Subscription and Resource Group
.Description
Creates or updates a Custom Location in the specified Subscription and Resource Group
.Example
PS C:\> New-AzCustomLocation -ResourceGroupName azps_test_group -Name azps_test_cluster -Location eastus -ClusterExtensionId "/subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/resourceGroups/azps_test_group/providers/Microsoft.Kubernetes/connectedClusters/azps_test_cluster/providers/Microsoft.KubernetesConfiguration/extensions/azps_test_extension" -HostResourceId "/subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/resourceGroups/azps_test_group/providers/Microsoft.Kubernetes/connectedClusters/azps_test_cluster" -DisplayName azps_test_cluster -Namespace arc

Location Name              Type
-------- ----              ----
eastus   azps_test_cluster Microsoft.ExtendedLocation/customLocations

.Outputs
Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Models.Api20210815.ICustomLocation
.Link
https://docs.microsoft.com/powershell/module/az.customlocation/new-azcustomlocation
#>
function New-AzCustomLocation {
    [OutputType([Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Models.Api20210815.ICustomLocation])]
    [CmdletBinding(DefaultParameterSetName='CreateExpanded', PositionalBinding=$false, SupportsShouldProcess, ConfirmImpact='Medium')]
    param(
        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Path')]
        [System.String]
        # Custom Locations name.
        ${Name},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Path')]
        [System.String]
        # The name of the resource group.
        # The name is case insensitive.
        ${ResourceGroupName},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Path')]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Runtime.DefaultInfo(Script='(Get-AzContext).Subscription.Id')]
        [System.String]
        # The ID of the target subscription.
        ${SubscriptionId},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # The geo-location where the resource lives
        ${Location},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # The type of the Custom Locations authentication
        ${AuthenticationType},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # The kubeconfig value.
        ${AuthenticationValue},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String[]]
        # Contains the reference to the add-on that contains charts to deploy CRDs and operators.
        ${ClusterExtensionId},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # Display name for the Custom Locations location.
        ${DisplayName},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # Connected Cluster or AKS Cluster.
        # The Custom Locations RP will perform a checkAccess API for listAdminCredentials permissions.
        ${HostResourceId},

        [Parameter()]
        [ArgumentCompleter([Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Support.ResourceIdentityType])]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Support.ResourceIdentityType]
        # The identity type.
        ${IdentityType},

        [Parameter(Mandatory)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [System.String]
        # Kubernetes namespace that will be created on the specified cluster.
        ${Namespace},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Body')]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Runtime.Info(PossibleTypes=([Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Models.Api20.ITrackedResourceTags]))]
        [System.Collections.Hashtable]
        # Resource tags.
        ${Tag},

        [Parameter()]
        [Alias('AzureRMContext', 'AzureCredential')]
        [ValidateNotNull()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Azure')]
        [System.Management.Automation.PSObject]
        # The credentials, account, tenant, and subscription used for communication with Azure.
        ${DefaultProfile},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Management.Automation.SwitchParameter]
        # Run the command as a job
        ${AsJob},

        [Parameter(DontShow)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Management.Automation.SwitchParameter]
        # Wait for .NET debugger to attach
        ${Break},

        [Parameter(DontShow)]
        [ValidateNotNull()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Runtime.SendAsyncStep[]]
        # SendAsync Pipeline Steps to be appended to the front of the pipeline
        ${HttpPipelineAppend},

        [Parameter(DontShow)]
        [ValidateNotNull()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Runtime.SendAsyncStep[]]
        # SendAsync Pipeline Steps to be prepended to the front of the pipeline
        ${HttpPipelinePrepend},

        [Parameter()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Management.Automation.SwitchParameter]
        # Run the command asynchronously
        ${NoWait},

        [Parameter(DontShow)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Uri]
        # The URI for the proxy server to use
        ${Proxy},

        [Parameter(DontShow)]
        [ValidateNotNull()]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Management.Automation.PSCredential]
        # Credentials for a proxy server to use for the remote call
        ${ProxyCredential},

        [Parameter(DontShow)]
        [Microsoft.Azure.PowerShell.Cmdlets.CustomLocation.Category('Runtime')]
        [System.Management.Automation.SwitchParameter]
        # Use the default credentials for the proxy
        ${ProxyUseDefaultCredentials}
    )

    process {
        try {
            Az.CustomLocation.internal\New-AzCustomLocation @PSBoundParameters
        }
        catch {
            throw
        }
    }
}
