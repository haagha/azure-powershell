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


using Microsoft.WindowsAzure.Commands.Sync.IO;
using Microsoft.WindowsAzure.Commands.Tools.Vhd;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Sync.Upload
{
    [DataContract]
    public class FileMetaData
    {
        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string FileFullName { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 20)]
        private string InternalCreatedDateUtc { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 30)]
        private string InternalLastModifiedDateUtc { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 40)]
        public long Size { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 50)]
        public long VhdSize { get; set; }

        /// <summary>
        /// Whole-file MD5 hash written by earlier versions of Add-AzVhd. It is no longer calculated;
        /// it is kept only so that upload metadata written by those versions can still be read.
        /// </summary>
        [DataMember(Name = "MD5Hash", EmitDefaultValue = false, Order = 60)]
        public byte[] LegacyMD5Hash { get; set; }

        /// <summary>
        /// Whole-file SHA-256 hash of the local VHD. Used to check that a resumed or patched upload
        /// is for the same file, and as the completion marker of a finished blob upload.
        /// </summary>
        [DataMember(EmitDefaultValue = false, Order = 70)]
        public byte[] Sha256Hash { get; set; }

        public DateTime CreatedDateUtc
        {
            get { return DateTime.Parse(this.InternalCreatedDateUtc, DateTimeFormatInfo.InvariantInfo); }
            set { this.InternalCreatedDateUtc = value.ToString(DateTimeFormatInfo.InvariantInfo); }
        }

        public DateTime LastModifiedDateUtc
        {
            get { return DateTime.Parse(this.InternalLastModifiedDateUtc, DateTimeFormatInfo.InvariantInfo); }
            set { this.InternalLastModifiedDateUtc = value.ToString(DateTimeFormatInfo.InvariantInfo); }
        }

        public static FileMetaData Create(string filePath)
        {
            return Create(filePath, calculateHash: true);
        }

        /// <summary>
        /// Creates the metadata for a local VHD file.
        /// </summary>
        /// <param name="filePath">Path of the local VHD file.</param>
        /// <param name="calculateHash">
        /// When false, the whole-file hash is not calculated. The managed disk upload does not use the
        /// hash, so it skips the extra full read of the file.
        /// </param>
        public static FileMetaData Create(string filePath, bool calculateHash)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filePath);
            }

            using (var stream = new VirtualDiskStream(filePath))
            {
                return new FileMetaData
                {
                    FileFullName = fileInfo.FullName,
                    CreatedDateUtc = DateTime.SpecifyKind(fileInfo.CreationTimeUtc, DateTimeKind.Utc),
                    LastModifiedDateUtc = DateTime.SpecifyKind(fileInfo.LastWriteTimeUtc, DateTimeKind.Utc),
                    Size = fileInfo.Length,
                    VhdSize = stream.Length,
                    Sha256Hash = calculateHash ? CalculateSha256Hash(stream, filePath) : null
                };
            }
        }

        private static byte[] CalculateSha256Hash(Stream stream, string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                Program.SyncOutput.MessageCalculatingMD5Hash(filePath);
                ProgressStatus hashStatus = new ProgressStatus(0, stream.Length, new ComputeStats());

                using (ProgressTracker progressTracker = new ProgressTracker(hashStatus,
                                                                             Program.SyncOutput.ProgressOperationStatus,
                                                                             Program.SyncOutput.ProgressOperationComplete))
                using (var swrp = new StreamWithReadProgress(stream, hashStatus))
                {
                    Task<byte[]> task = Task<byte[]>.Factory.StartNew(() =>
                    {
                        var bs = new BufferedStream(swrp);
                        return sha256.ComputeHash(bs);
                    });

                    while (!task.Wait(TimeSpan.FromSeconds(1)))
                    {
                        progressTracker.Update();
                    }

                    Program.SyncOutput.MessageMD5HashCalculationFinished();
                    return task.Result;
                }
            }
        }
    }

    [DataContract]
    public class SystemInformation
    {
        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string MachineName { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 20)]
        public int CsUploadProcessId { get; set; }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static SystemInformation Create()
        {
            return new SystemInformation
            {
                MachineName = Environment.MachineName,
                CsUploadProcessId = Process.GetCurrentProcess().Id
            };
        }
    }


    [DataContract]
    public class LocalMetaData
    {
        public static string MetaDataKey = "localmetadata";

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public FileMetaData FileMetaData { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 20)]
        public SystemInformation SystemInformation { get; set; }
    }

    /// <summary>
    /// Blob metadata entry that marks a finished blob upload. Its value is the Base64-encoded SHA-256
    /// hash of the uploaded VHD file. It replaces the Content-MD5 property that earlier versions set
    /// when an upload finished.
    /// </summary>
    public static class CompletedUploadMetaData
    {
        public const string MetaDataKey = "uploadsha256";
    }

    [DataContract]
    public class LeaseMetaData
    {
        public static string MetaDataKey = "leasemetadata";

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public Guid LeaseId { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 20)]
        public DateTime LeaseExpirationDateUtc { get; set; }
    }

    [DataContract]
    public class BlobMetaData
    {
        public static string MetaDataKey = "blobmetadata";

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string ETag { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 20)]
        public DateTime LastModifiedUtc { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 30)]
        public byte[] MD5Hash { get; set; }
    }
}