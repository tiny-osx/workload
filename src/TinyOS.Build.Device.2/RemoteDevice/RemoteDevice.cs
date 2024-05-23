﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Net.Http;

namespace TinyOS.Build.Device
{
    public class RemoteDevice : DeviceClient, IDisposable
    {
        public Guid ProjectId { get; private set; } 
        
        public string SourceDirectory { get; private set;  } = string.Empty;
        
        public string ProjectName { get; private set;  } = string.Empty;

        public bool VerifyHash {get; set; } = true;
        
        private List<FileMeta> LocalFiles { get; set; } = new List<FileMeta>();

        private List<FileMeta> RemoteFiles { get; set; } = new List<FileMeta>();

        public void Initialize(string sourceDirectory, string projectName, string projectId, string deviceUrl, bool verifyHash)
        {                        
            if (!Directory.Exists(sourceDirectory))
            {
                throw new Exception(string.Format($"'{sourceDirectory}' is not a valid directory."));
            }
            
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = GenerateGuid.CreateFromName(projectName).ToString();
            }
            
            if (string.IsNullOrEmpty(deviceUrl))
            {
                deviceUrl = "http://tinyos:8920";
            }

            SourceDirectory = sourceDirectory;
            ProjectName = projectName;
            ProjectId = Guid.Parse(projectId);
            DeviceUrl = new Uri(deviceUrl);
            VerifyHash = verifyHash;

            LogMessage(1, 
                $"{ProjectName} -> ProjectId: {ProjectId}, SourceDirectory:{SourceDirectory}, DeviceUrl: {DeviceUrl}, VerifyHash: {VerifyHash}"
            );

            Connect();
        }

        public void Connect()
        {
            try
            {
                if (IsConnected())
                {
                    LogMessage($"Connected successfully to remote device '{DeviceUrl.Host}:{DeviceUrl.Port}'.");
                    SyncClock();
                    SyncFiles();
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to connect to '{DeviceUrl.Host}:{DeviceUrl.Port}' remote device with error '{ex.Message}'.");
            }
        }

        public bool ExecuteDeploy()
        {            
            if (LocalFiles.Count != RemoteFiles.Count)
            {
                LogMessage($"Cleaning remote files for projectId '{ProjectId}'. Local and remote file count do not match.");
                CleanFiles();
            }
            
            foreach (var file in LocalFiles.Where(x =>
                   !RemoteFiles.Any(z => z.LocalPath == x.LocalPath && z.Hash.SequenceEqual(x.Hash))).ToList())
            {
                // local files added or changed
                using (var fileStream = File.OpenRead(file.LocalPath))
                {
                    LogMessage($"  Transferred file '{file.RemotePath}'.");
                    UploadFile(file);
                }
            }
            
            foreach (var file in RemoteFiles.Where(x =>
                !LocalFiles.Any(z => z.LocalPath == x.LocalPath)).ToList())
            {
                // local files deleted. deleting remote files
                LogMessage($"Deleteing remote file '{file.RemotePath}'. Local files were deleted.");
                DeleteFile(file.RemotePath);
            }

            UploadCacheFile();
            
            return true;
        }

        public bool ExecuteClean()
        {
            if (CleanFiles())
            {
                LogMessage($"Cleaned remote files for projectId '{ProjectId}' successfuly.");
                return true;
            }
            
            return false;
        }

        private bool IsConnected()
        {
            var response = GetStatusAsync().GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        private bool SyncClock()
        {
            var response = SyncClockAsync(DateTime.UtcNow).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        private bool UploadFile(FileMeta metadata)
        {
            var response = UploadFileAsync(ProjectId, metadata, VerifyHash).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        private bool UploadCacheFile()
        {
            var response = UploadCacheFileAsync(LocalFiles, ProjectId).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        private bool DownloadCache()
        {
            RemoteFiles = DownloadCacheFileAsync(ProjectId).GetAwaiter().GetResult();
            
            if (RemoteFiles.Count > 0)
            {
                return true; 
            } 

            return false;
        }

        private bool CleanFiles()
        {
            try 
            {
                var response = CleanFilesAsync(ProjectId).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteFile(string remotePath)
        {
            var response = DeleteFileAsync(ProjectId, remotePath).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }

        private void SyncFiles()
        {
            DownloadCache();
            
            if (File.GetAttributes(SourceDirectory).HasFlag(FileAttributes.Directory))
            {
                var files = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);

                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                var fileHashList = new ConcurrentBag<FileMeta>();

                Parallel.ForEach(files, options, file =>
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = new BufferedStream(File.OpenRead(file), 1200000))
                        {

                            var properties = new FileMeta
                            {
                                LocalPath = Path.GetFullPath(file),
                                RemotePath = MakeRelativePath(SourceDirectory, file),
                                Hash = md5.ComputeHash(stream)
                            };

                            fileHashList.Add(properties);
                        }
                    }
                });

                LocalFiles = fileHashList.ToList();
            }
        }

        private static string MakeRelativePath(string root, string subdirectory)
        {
            if (!subdirectory.StartsWith(root))
            {
                throw new Exception(string.Format("'{0}' is not a subdirectory of '{1}'.", subdirectory, root));
            }

            int chop = root.Length;
            if (subdirectory[chop] == Path.DirectorySeparatorChar)
                ++chop;

            return subdirectory.Substring(chop);
        }
    }  
}