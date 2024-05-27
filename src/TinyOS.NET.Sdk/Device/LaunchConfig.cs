using System.Text.Json;
using System.Text.Json.Serialization;

namespace TinyOS.Build.Device
{
    public static class LaunchConfig
    {
        public static string Create()
        {
            var launchFileConfig = new RemoteDebugLaunchConfig
            {
                Version = "0.2.1",
                Adapter = "piplink",
                AdapterArgs = "--interpreter=vscode",
                LanguageMappings = new Languagemappings
                {
                    CSharp = new CSharp
                    {
                        LanguageId = "3F5162F8-07C6-11D3-9053-00C04FA302A1",
                        Extensions = new string[] { "*" },
                    },
                },
                ExceptionCategoryMappings = new Exceptioncategorymappings
                {
                    CLR = "449EC4CC-30D2-4032-9256-EE18EB41B62B",
                    MDA = "6ECE07A9-0EDE-45C4-8296-818D8FC401D4",
                },
                Configurations = new Configuration[]
                {
                    new Configuration
                    {
                        Name = ".NET Core Launch",
                        Type = "coreclr",
                        ProcessName = $"/apps/TinyOS.VScode",
                        Request = "attach",
                        JustMyCode = false,
                        Logging = new Logging
                        {
                            EngineLogging = true,
                            ModuleLoad = false
                        },
                    },
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            return JsonSerializer.Serialize(launchFileConfig, options);
        }
    }

    public class RemoteDebugLaunchConfig
    {
        public string? Version { get; set; }
        public string? Adapter { get; set; }
        public string? AdapterArgs { get; set; }
        public Languagemappings? LanguageMappings { get; set; }
        public Exceptioncategorymappings? ExceptionCategoryMappings { get; set; }
        public Configuration[]? Configurations { get; set; }
    }

    public class Languagemappings
    {
        [JsonPropertyName("C#")]
        public CSharp? CSharp { get; set; }
    }

    public class CSharp
    {
        public string? LanguageId { get; set; }
        public string[]? Extensions { get; set; }
    }

    public class Exceptioncategorymappings
    {
        public string? CLR { get; set; } 
        public string? MDA { get; set; } 
    }

    public class Configuration
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? ProcessName { get; set; }
        public string? Request { get; set; }
        public bool? JustMyCode { get; set; }
        public string? Cwd { get; set; }
        public Logging? Logging { get; set; }
    }

    public class Logging
    {
        public bool EngineLogging { get; set; }
        public bool ModuleLoad { get; set; }
    }
}