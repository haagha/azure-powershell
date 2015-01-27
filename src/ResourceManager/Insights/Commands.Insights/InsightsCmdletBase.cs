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

using System;
using Microsoft.Azure.Common.Authorization;
using Microsoft.Azure.Common.Authorization.Models;
using Microsoft.Azure.Insights;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.Insights
{
    /// <summary>
    /// Base class for the Azure Insights SDK Cmdlets
    /// </summary>
    abstract public class InsightsCmdletBase : AzurePSCmdlet, IDisposable
    {
        private IInsightsClient insightsClient;

        private bool disposed;

        /// <summary>
        /// Gets the InsightsClient to use in the Cmdlet
        /// </summary>
        protected IInsightsClient InsightsClient
        {
            get
            {
                if (this.insightsClient == null)
                {
                    // The premise is that a command to establish a context (like Add-AzureAccount) has been called before this command in order to have a correct CurrentContext
                    this.insightsClient = AzureSession.ClientFactory.CreateClient<InsightsClient>(CurrentContext, AzureEnvironment.Endpoint.ResourceManager);
                }

                return this.insightsClient;
            }
        }

        /// <summary>
        /// Dispose method
        /// The implementation of IDispose follows the recommeded pattern
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // The class is not sealed, so this is here in case a derived class is created
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the resources
        /// </summary>
        /// <param name="disposing">Indicates whether the managed resources should be disposed or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (this.insightsClient != null)
                {
                    this.insightsClient.Dispose();
                    this.insightsClient = null;
                }

                this.disposed = true;
            }
        }
    }
}
