using System;

using Microsoft.Build.Framework;

using TinyOS.Build.Device;

namespace TinyOS.NET.Sdk
{
    // TEST RUN: dotnet build -t:OnClean
    public class CleanTask : DeviceTask
    {
        [Required]
        public string SourceDirectory { get; set;  } 
        
        [Required]
        public string ProjectName { get; set;  }

        public string ProjectId { get; set; } 
        
        public string DeviceUrl { get; set; } 

        public override bool Execute()
        {            
            try
            {
                RemoteDevice.Initialize(
                    SourceDirectory, ProjectName, ProjectId, DeviceUrl, false
                );

                RemoteDevice.ExecuteClean();
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
