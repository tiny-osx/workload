using System;

namespace TinyOS.Build.Device
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message)
        {
            Importance = 0;
            Message = message;
        }

        public MessageEventArgs(int importance, string message)
        {
            Importance = importance;
            Message = message;
        }
        
        public int Importance { get; internal set; }
        public string Message { get; internal set; }
    }
}