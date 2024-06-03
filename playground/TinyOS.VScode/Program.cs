using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));

        Debug.WriteLine("Hello World");
        //Debugger.Break();
    }
}