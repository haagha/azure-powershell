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
.SYNOPSIS
Test Add Vhd
#>
function Test-AddVhd
{
    # Setup
    $rgname = Get-ComputeTestResourceName

    try
    {
        # Common
        $loc = Get-ComputeVMLocation;
        New-AzResourceGroup -Name $rgname -Location $loc -Force;

        # Storage Account (SA)
        $stoname = 'sto' + $rgname;
        $stotype = 'Standard_GRS';
        New-AzStorageAccount -ResourceGroupName $rgname -Name $stoname -Location $loc -Type $stotype;
        $stoaccount = Get-AzStorageAccount -ResourceGroupName $rgname -Name $stoname;
        $storageKey = Get-AzStorageAccountKey -ResourceGroupName $rgname -Name $stoname;
        $vhdContainerName = 'vhds';

        $path = (get-item -path ".\").FullName;

        $csvFile = "upload_VHD.csv";
        $csvPath = "..\..\..\..\..\ServiceManagement\Compute\Commands.ServiceManagement.Test\Resources\" + $csvFile;
        $testData = Import-Csv $csvPath;

        foreach ($testItem in $testData)
        {
              $vhdLocalPath = 'f:\vhdstore\' + $testItem.vhdName;
              $vhdName = GetFileNameWithoutExtension $testItem.vhdName;
              $vhdDestUri = [System.String]::Format("{0}{1}/{2}{3}.vhd", $stoaccount.PrimaryEndpoints.Blob, $vhdContainerName, $vhdName, $rgname);
              Write-Output ("Start Uploading... : " + $testItem.vhdName);

              $vhdUploadContext = Add-AzVhd -ResourceGroupName $rgname -Destination $vhdDestUri -LocalFilePath $vhdLocalPath -NumberOfUploaderThreads 1;
              Wait-Seconds 5;
              Write-Output ("Destination Uri :" + $vhdUploadContext.DestinationUri);
              Write-Output ("Local File :" + $vhdUploadContext.LocalFilePath.FullName);
              Write-Output ("Uploading Ended.");

              Assert-NotNull $vhdUploadContext;
              Assert-AreEqual $vhdDestUri $vhdUploadContext.DestinationUri;
              Assert-AreEqual $vhdLocalPath $vhdUploadContext.LocalFilePath.FullName;

              Write-Output ($vhdDestUri);
              Write-Output ($storageKey.Key1);

              $destBlobHandle = GetBlobHandle $vhdDestUri $storageKey.Key1;
              Assert-True {VerifyUploadSha256 $destBlobHandle};

              $vhdDownloadPath = $vhdLocalPath + "-download.vhd";

              $vhdDownloadContext = Save-AzVhd -ResourceGroupName $rgname -SourceUri $vhdDestUri -LocalFilePath $vhdDownloadPath;
              Write-Output ("Source :" +$vhdDownloadContext.Source);
              Write-Output ("LocalFilePath :" +$vhdDownloadContext.LocalFilePath);
              Assert-AreEqual $vhdDestUri $vhdDownloadContext.Source;
              Assert-AreEqual $vhdDownloadPath $vhdDownloadContext.LocalFilePath;

              $vhdDownloadContext = Save-AzVhd -ResourceGroupName $rgname -SourceUri $vhdDestUri -LocalFilePath $vhdDownloadPath -Overwrite;
              Assert-AreEqual $vhdDestUri $vhdDownloadContext.Source;
              Assert-AreEqual $vhdDownloadPath $vhdDownloadContext.LocalFilePath;
        }
    }
    finally
    {
        # Cleanup
        Clean-ResourceGroup $rgname
    }
}

function GetFileNameWithoutExtension ($fileName)
{
    $fileName.Substring(0, $fileName.IndexOf('.'));
}

function GetBlobHandle
{
    param([string] $blobString, [string] $storageKey)

    $blobPath = [Microsoft.WindowsAzure.Commands.Sync.Download.BlobUri] $null;
    [Microsoft.WindowsAzure.Commands.Sync.Download.BlobUri]::TryParseUri($blobString, [REF]$blobPath);
    $blob = [Microsoft.WindowsAzure.Commands.Sync.Download.BlobHandle]::new($blobPath, $storageKey);
    return [Microsoft.WindowsAzure.Commands.Sync.Download.BlobHandle] $blob;
}

function VerifyUploadSha256
{
    param([System.Object] $blobHandle)

    return -not [System.String]::IsNullOrEmpty($blobHandle.Blob.Metadata['uploadsha256']) -and
        [System.String]::IsNullOrEmpty($blobHandle.Blob.Properties.ContentMD5)
}
