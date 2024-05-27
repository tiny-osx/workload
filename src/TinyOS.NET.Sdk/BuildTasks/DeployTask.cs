using System;
using Microsoft.Build.Framework;
using TinyOS.Build.Device;

namespace TinyOS.NET.Sdk
{
    // TEST RUN: dotnet build -t:OnDeploy
    public class DeployTask : DeviceTask
    {
        [Required]
        public string? SourceDirectory { get; set;  } 
        
        [Required]
        public string? ProjectName { get; set;  }
        
        [Required]
        public string? AssemblyName { get; set; } 
        
        public string? DeviceUrl { get; set; } 

        public bool IsPublished { get; set; } = false; 
        
        public bool VerifyHash {get; set; } = true;

        public override bool Execute()
        {            
            try
            {
                RemoteDevice.Initialize(
                    SourceDirectory!, ProjectName!, AssemblyName!, DeviceUrl!, VerifyHash
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