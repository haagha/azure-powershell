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
using System.Collections.Generic;
using Microsoft.WindowsAzure.Commands.Sync.Upload;
using Xunit;

namespace Microsoft.Azure.Commands.Compute.Test.ScenarioTests
{
    public class VhdUploadMetaDataTests
    {
        private static readonly byte[] Sha256A = CreateHash(0x11);
        private static readonly byte[] Sha256B = CreateHash(0x22);
        private static readonly byte[] LegacyMd5 = new byte[16];

        [Fact]
        public void FileMetaData_RoundTripsSha256Hash()
        {
            var metaData = new LocalMetaData { FileMetaData = CreateFileMetaData(Sha256A) };

            var serialized = SerializationUtil.GetSerializedString(metaData);
            var deserialized = SerializationUtil.GetObjectFromSerializedString<LocalMetaData>(serialized);

            Assert.Equal(Sha256A, deserialized.FileMetaData.Sha256Hash);
            Assert.Null(deserialized.FileMetaData.LegacyMD5Hash);
            Assert.DoesNotContain("<MD5Hash>", serialized);
        }

        [Fact]
        public void FileMetaData_ReadsMetadataWrittenByEarlierVersions()
        {
            // Earlier versions wrote the whole-file MD5 under the "MD5Hash" element and no SHA-256.
            var legacy = CreateFileMetaData(null);
            legacy.LegacyMD5Hash = LegacyMd5;
            var serialized = SerializationUtil.GetSerializedString(new LocalMetaData { FileMetaData = legacy });
            Assert.Contains("<MD5Hash>", serialized);

            var deserialized = SerializationUtil.GetObjectFromSerializedString<LocalMetaData>(serialized);

            Assert.Equal(LegacyMd5, deserialized.FileMetaData.LegacyMD5Hash);
            Assert.Null(deserialized.FileMetaData.Sha256Hash);
        }

        [Fact]
        public void CompareFileMetaData_MatchingSha256_ReturnsNoMessages()
        {
            var messages = BlobCreatorBase.CompareFileMetaData(CreateFileMetaData(Sha256A), CreateFileMetaData(Sha256A));

            Assert.Empty(messages);
        }

        [Fact]
        public void CompareFileMetaData_DifferentSha256_ReturnsMismatch()
        {
            var messages = BlobCreatorBase.CompareFileMetaData(CreateFileMetaData(Sha256A), CreateFileMetaData(Sha256B));

            var message = Assert.Single(messages);
            Assert.StartsWith("SHA-256 hash of VHD file in blob storage", message);
        }

        [Fact]
        public void CompareFileMetaData_UploadStartedByEarlierVersion_AsksForOverWrite()
        {
            var blobFileMetaData = CreateFileMetaData(null);
            blobFileMetaData.LegacyMD5Hash = LegacyMd5;

            var messages = BlobCreatorBase.CompareFileMetaData(blobFileMetaData, CreateFileMetaData(Sha256A));

            var message = Assert.Single(messages);
            Assert.Equal(BlobCreatorBase.LegacyUploadCannotResumeMessage, message);
            Assert.Contains("OverWrite", message);
        }

        [Fact]
        public void IsUploadCompleted_DetectsNewMarkerAndLegacyContentMd5()
        {
            var withMarker = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { CompletedUploadMetaData.MetaDataKey, Convert.ToBase64String(Sha256A) }
            };
            var empty = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Finished by this version: SHA-256 marker, no Content-MD5.
            Assert.True(CloudPageBlobExtensions.IsUploadCompleted(withMarker, null));
            // Finished by an earlier version: Content-MD5, no marker.
            Assert.True(CloudPageBlobExtensions.IsUploadCompleted(empty, Convert.ToBase64String(LegacyMd5)));
            // Not finished.
            Assert.False(CloudPageBlobExtensions.IsUploadCompleted(empty, null));
            Assert.False(CloudPageBlobExtensions.IsUploadCompleted(null, String.Empty));
        }

        [Fact]
        public void GetUploadCompletedSha256Hash_ReturnsMarkerValue()
        {
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { CompletedUploadMetaData.MetaDataKey, Convert.ToBase64String(Sha256A) }
            };

            Assert.Equal(Sha256A, CloudPageBlobExtensions.GetUploadCompletedSha256Hash(metadata));
            Assert.Null(CloudPageBlobExtensions.GetUploadCompletedSha256Hash(new Dictionary<string, string>()));
        }

        private static FileMetaData CreateFileMetaData(byte[] sha256Hash)
        {
            var date = new DateTime(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            return new FileMetaData
            {
                FileFullName = @"C:\vhd\disk.vhd",
                CreatedDateUtc = date,
                LastModifiedDateUtc = date,
                Size = 1024L * 1024L * 1024L,
                VhdSize = 1024L * 1024L * 1024L + 512,
                Sha256Hash = sha256Hash
            };
        }

        private static byte[] CreateHash(byte value)
        {
            var hash = new byte[32];
            for (int i = 0; i < hash.Length; i++)
            {
                hash[i] = value;
            }
            return hash;
        }
    }
}
