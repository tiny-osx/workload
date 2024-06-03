using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TinyOS.Build.Device
{
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(List<FileMeta>))]
    internal partial class DeviceJsonContext : JsonSerializerContext
    {
    }
}