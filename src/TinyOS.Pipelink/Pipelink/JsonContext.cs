using System.Text.Json.Serialization;

namespace TinyOS.Pipelink;

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(List<string>))]
internal partial class  JsonContext : JsonSerializerContext
{

}