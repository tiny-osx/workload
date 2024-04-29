using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using TinyOS.Build.Device;

#nullable disable

namespace TinyOS.NET.Sdk
{    
    public abstract class DeviceTask : Task
    {
        public RemoteDevice RemoteDevice { get; internal set; }
 
        public DeviceTask()
        {
            RemoteDevice = new RemoteDevice();
            RemoteDevice.MessageReceived += MessageReceived;
        }

        public override bool Execute()
        {
            return !Log.HasLoggedErrors;
        }

        private void MessageReceived(object sender, MessageEventArgs args)
        {
            Log.LogMessage((MessageImportance)args.Importance, args.Message);
        }
    }
}