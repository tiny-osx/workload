namespace TinyOS.Build.Client;

using TinyOS.Build.Device;

internal class Program
{    
    public static string DeviceUrl { get; set; } = "http://localhost:8920";
    public static string ProjectId { get; set; } = "72fa37ca-51b4-7f83-7112-343c5b8d0113";
    public static string SourceDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    public static string ProjectName { get; set; } = "$ProjectName";
    public static bool VerifyHash {get; set; } = true;
    
    private static RemoteDevice RemoteDevice { get; set; } = new RemoteDevice();
    
    private static void Main(string[] args)
    {    
        RemoteDevice.MessageReceived += OnMessageReceived;
        
        RemoteDevice.Initialize(
                SourceDirectory, ProjectName, ProjectId, DeviceUrl, VerifyHash
            );

        RemoteDevice.ExecuteClean();
        RemoteDevice.ExecuteDeploy();
        RemoteDevice.Dispose();
    }

    private static void OnMessageReceived(object sender, MessageEventArgs args)
    {
        Console.WriteLine(args.Message);
    }
}