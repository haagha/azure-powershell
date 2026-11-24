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
using System.Linq;
using Microsoft.WindowsAzure.Commands.Sync.Upload;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace Microsoft.Azure.Commands.Compute.Test
{
    public class VhdUploadMetaDataTests
    {
        private static FileMetaData CreateMetadata(byte[] hash)
        {
            return new FileMetaData
            {
                FileFullName = "disk.vhd",
                Size = 1024,
                VhdSize = 1024,
                CreatedDateUtc = new DateTime(2020, 1, 1),
                LastModifiedDateUtc = new DateTime(2020, 1, 2),
                Sha256Hash = hash
            };
        }

        private static CloudPageBlob CreateBlob()
        {
            return new CloudPageBlob(new Uri("https://example.blob.core.windows.net/vhds/disk.vhd"));
        }

        [Fact]
        public void ReadsLegacyMetadata()
        {
            const string xml = "<FileMetaData xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.WindowsAzure.Commands.Sync.Upload\"><MD5Hash>AQID</MD5Hash></FileMetaData>";
            var metadata = SerializationUtil.GetObjectFromSerializedString<FileMetaData>(xml);
            Assert.Equal(new byte[] { 1, 2, 3 }, metadata.LegacyMD5Hash);
            Assert.Null(metadata.Sha256Hash);
        }

        [Fact]
        public void ReadsSha256Metadata()
        {
            var metadata = CreateMetadata(Enumerable.Repeat((byte)42, 32).ToArray());
            var xml = SerializationUtil.GetSerializedString(metadata);
            var restored = SerializationUtil.GetObjectFromSerializedString<FileMetaData>(xml);
            Assert.Equal(metadata.Sha256Hash, restored.Sha256Hash);
            Assert.DoesNotContain("MD5Hash", xml);
        }

        [Fact]
        public void LegacyResumeRequiresOverwrite()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => BlobCreatorBase.AssertSha256Metadata(new FileMetaData { LegacyMD5Hash = new byte[16] }));
            Assert.Contains("-OverWrite", exception.Message);
        }

        [Fact]
        public void MatchingSha256AllowsResume()
        {
            var hash = Enumerable.Repeat((byte)1, 32).ToArray();
            Assert.Empty(BlobCreatorBase.CompareFileMetaData(CreateMetadata(hash), CreateMetadata(hash.ToArray())));
        }

        [Fact]
        public void DifferentSha256RejectsResume()
        {
            var messages = BlobCreatorBase.CompareFileMetaData(
                CreateMetadata(new byte[32]), CreateMetadata(Enumerable.Repeat((byte)1, 32).ToArray()));
            Assert.Single(messages);
            Assert.Contains("SHA-256", messages[0]);
            Assert.Contains("-OverWrite", messages[0]);
        }

        [Fact]
        public void NewCompletionMarkerIsDetected()
        {
            var blob = CreateBlob();
            Assert.False(blob.IsUploadComplete());
            blob.SetUploadSha256Hash(new byte[32]);
            Assert.True(blob.IsUploadComplete());
            Assert.Equal(Convert.ToBase64String(new byte[32]), blob.Metadata[CloudPageBlobExtensions.UploadSha256MetadataKey]);
        }

        [Fact]
        public void LegacyContentMd5CompletionIsDetected()
        {
            var blob = CreateBlob();
            blob.Properties.ContentMD5 = Convert.ToBase64String(new byte[16]);
            Assert.True(blob.IsUploadComplete());
        }
    }
}
