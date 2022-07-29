
# ----------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# Code generated by Microsoft (R) AutoRest Code Generator.Changes may cause incorrect behavior and will be lost if the code
# is regenerated.
# ----------------------------------------------------------------------------------

<#
.Synopsis
Create an in-memory object for AlertRuleLeafCondition.
.Description
Create an in-memory object for AlertRuleLeafCondition.

.Outputs
Microsoft.Azure.PowerShell.Cmdlets.Monitor.Models.Api20201001.AlertRuleLeafCondition
.Link
https://docs.microsoft.com/powershell/module/az./new-AzAlertRuleLeafConditionObject
#>
function New-AzAlertRuleLeafConditionObject {
    [OutputType('Microsoft.Azure.PowerShell.Cmdlets.Monitor.Models.Api20201001.AlertRuleLeafCondition')]
    [CmdletBinding(PositionalBinding=$false)]
    Param(

        [Parameter(HelpMessage="The value of the event's field will be compared to the values in this array (case-insensitive) to determine if the condition is met.")]
        [string[]]
        $ContainsAny,
        [Parameter(HelpMessage="The value of the event's field will be compared to this value (case-insensitive) to determine if the condition is met.")]
        [string]
        $Equal,
        [Parameter(HelpMessage="The name of the Activity Log event's field that this condition will examine.
        The possible values for this field are (case-insensitive): 'resourceId', 'category', 'caller', 'level', 'operationName', 'resourceGroup', 'resourceProvider', 'status', 'subStatus', 'resourceType', or anything beginning with 'properties'.")]
        [string]
        $Field
    )

    process {
        $Object = [Microsoft.Azure.PowerShell.Cmdlets.Monitor.Models.Api20201001.AlertRuleLeafCondition]::New()

        if ($PSBoundParameters.ContainsKey('ContainsAny')) {
            $Object.ContainsAny = $ContainsAny
        }
        if ($PSBoundParameters.ContainsKey('Equal')) {
            $Object.Equal = $Equal
        }
        if ($PSBoundParameters.ContainsKey('Field')) {
            $Object.Field = $Field
        }
        return $Object
    }
}

