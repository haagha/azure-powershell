﻿// ----------------------------------------------------------------------------------
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

using System.IO;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.Azure.Common.Authorization.Models;
using Microsoft.Azure.Common.Authorization;

namespace Microsoft.Azure.Commands.StreamAnalytics.Models
{
    public partial class StreamAnalyticsClient
    {
        public IStreamAnalyticsManagementClient StreamAnalyticsManagementClient { get; private set; }

        public StreamAnalyticsClient(AzureContext context)
        {
            StreamAnalyticsManagementClient = AzureSession.ClientFactory.CreateClient<StreamAnalyticsManagementClient>(
                context, AzureEnvironment.Endpoint.ResourceManager);
        }

        /// <summary>
        /// Parameterless constructor for Mocking.
        /// </summary>
        public StreamAnalyticsClient()
        {
        }

        public virtual string ReadJsonFileContent(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            using (TextReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
