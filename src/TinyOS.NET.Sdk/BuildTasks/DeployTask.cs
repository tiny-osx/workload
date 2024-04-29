using System;
using Microsoft.Build.Framework;

namespace TinyOS.NET.Sdk
{
    // TEST RUN: dotnet build -t:OnDeploy
    public class DeployTask : DeviceTask
    {
        [Required]
        public string SourceDirectory { get; set;  } 
        
        [Required]
        public string ProjectName { get; set;  }

        public string ProjectId { get; set; } 
        
        public string DeviceUrl { get; set; } 
        
        public bool VerifyHash {get; set; } = true;

        public override bool Execute()
        {
            try
            {
                RemoteDevice.Initialize(
                    SourceDirectory, ProjectName, ProjectId, DeviceUrl, VerifyHash
                );

                RemoteDevice.ExecuteDeploy();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, showStackTrace: false);
                RemoteDevice.Dispose();
                return false;
            }

            RemoteDevice.Dispose();
            return base.Execute();
        }
    }
}