csharp
// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.Compute.Automation.Models;
using Microsoft.Azure.Commands.Compute.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.Compute.Automation
{
    [Cmdlet(VerbsCommon.New, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "RestorePoint", DefaultParameterSetName = "DefaultParameter", SupportsShouldProcess = true)]
    [OutputType(typeof(PSRestorePoint))]
    public class NewAzureRestorePoint : ComputeAutomationBaseCmdlet
    {

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Resource group name this resource belongs to")]
        [ResourceGroupCompleter]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the restore point collection this restore point is part of")]
        public string RestorePointCollectionName{ get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the restore point")]
        [Alias("RestorePointName")]
        public string Name { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = false,
            ValueFromPipelineByPropertyName = false,
            HelpMessage = "Set the region of the restore point")]
        public string Location { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "ARM ID of the source OS disk/restore point")]
        public string SourceOSResource { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "ARM ID of the OS disk encryption set")]
        public string OSRestorePointEncryptionSetId { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Type of OS disk encryption")]
        public string OSRestorePointEncryptionType { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "ARM IDs of the source data disks/restore points")]
        public string[] SourceDataDiskResource { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "ARM IDs of the data disk encryption sets")]
        public string[] DataDiskRestorePointEncryptionSetId { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Types of data disk encryption")]
        public string[] DataDiskRestorePointEncryptionType { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            ExecuteClientAction(() =>
            {
                if (ShouldProcess(this.Name, VerbsCommon.New))
                {
                    string resourceGroup = this.ResourceGroupName;
                    string restorePointName = this.Name;
                    string restorePointCollectionName = this.RestorePointCollectionName;
                    string location = this.Location;
                    string sourceOSResource = this.SourceOSResource;
                    string osRestorePointEncryptionSetId = this.OSRestorePointEncryptionSetId;
                    string osRestorePointEncryptionType = this.OSRestorePointEncryptionType;
                    string[] sourceDataDiskResource = this.SourceDataDiskResource;
                    string[] dataDiskRestorePointEncryptionSetId = this.DataDiskRestorePointEncryptionSetId;
                    string[] dataDiskRestorePointEncryptionType = this.DataDiskRestorePointEncryptionType;

                    RestorePoint restorePoint = new RestorePoint();

                    restorePoint.SourceOSResource = new ApiEntityReference { Id = sourceOSResource };
                    restorePoint.OSRestorePointEncryptionSetId = osRestorePointEncryptionSetId;
                    restorePoint.OSRestorePointEncryptionType = osRestorePointEncryptionType;

                    for (int i = 0; i < sourceDataDiskResource.Length; i++)
                    {
                        restorePoint.SourceDataDiskResource.Add(new ApiEntityReference { Id = sourceDataDiskResource[i] });
                        restorePoint.DataDiskRestorePointEncryptionSetId.Add(dataDiskRestorePointEncryptionSetId[i]);
                        restorePoint.DataDiskRestorePointEncryptionType.Add(dataDiskRestorePointEncryptionType[i]);
                    }

                    var result = RestorePointClient.Create(resourceGroup, restorePointCollectionName, restorePointName, restorePoint);
                        
                    var psObject = new PSRestorePoint();
                    ComputeAutomationAutoMapperProfile.Mapper.Map<RestorePoint, PSRestorePoint>(result, psObject);
                    WriteObject(psObject);
                }
            });
        }
    }
}
