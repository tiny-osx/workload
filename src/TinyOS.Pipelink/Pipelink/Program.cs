using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.Http;

namespace TinyOS.Pipelink
{

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            bool showHelp = false;
            int port = 8920;
            string device = "tinyos";
            var extraArgs = new List<string>();

            foreach (var a in args)
            {
                if (a == "-h" || a == "--help" || a == "-?")
                {
                    showHelp = true;
                }
                else if (a.StartsWith("--device="))
                {
                    device = a.Substring("--device=".Length);
                }
                else if (a.StartsWith("--port="))
                {
                    int.TryParse(a.Substring("--port=".Length), out port);
                }
                else
                {
                    extraArgs.Add(a);
                }
            }

            if (showHelp)
            {
                Console.WriteLine("pipelink");
                Console.WriteLine("--------");
                Console.WriteLine("Usage: pipelink [options] <arguments>");
                Console.WriteLine();
                Console.WriteLine("Arguments:");
                Console.WriteLine("   <arguments>  Arguments passed to the debug service.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --device=<name/ip>     Device name/ip of the device to debug.");
                Console.WriteLine("  --port=<port>          Device port number of the device to debug.");
                Console.WriteLine("  -h, -?, --help         Show command line help.");
                Console.WriteLine();

                return (int)ExitCodes.ArgumentFailure;
            }

            if (string.IsNullOrEmpty(device))
            {
                Console.WriteLine("pipelink: Device option name cannot be empty.");
                return (int)ExitCodes.ArgumentFailure;
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                Console.WriteLine($"pipelink: Port number must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.");
                return (int)ExitCodes.ArgumentFailure;
            }

            try
            {
                int tcpPort = -1;

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri($"http://{device}:{port}/");

                    var json = JsonSerializer.Serialize(extraArgs, JsonContext.Default.ListString);

                    using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        var response = await httpClient.PutAsync($"/debug", jsonContent);
                        response.EnsureSuccessStatusCode();

                        var content = await response.Content.ReadAsStreamAsync();
                        try
                        {
                            tcpPort = JsonSerializer.Deserialize(content, JsonContext.Default.Int32);
                        }
                        catch
                        {
                            throw new InvalidOperationException("pipelink: Failed to get debugger port number.");
                        }
                    }
                }

                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(device, tcpPort);

                    if (!client.Connected)
                    {
                        throw new InvalidOperationException("pipelink: Failed to open connection to the device.");
                    }

                    using (NetworkStream clientStream = client.GetStream())
                    {
                        Task.WhenAny(
                            Console.OpenStandardInput().CopyToAsync(clientStream),
                            clientStream.CopyToAsync(Console.OpenStandardOutput())
                        ).Wait();
                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"pipelink: {se.Message}.");
                return se.ErrorCode;
            }
            catch (HttpRequestException re)
            {
                Console.WriteLine($"pipelink: {re.Message}.");
                if (re.StatusCode.HasValue)
                {
                   return (int)re.StatusCode;
                }

                return (int)ExitCodes.UnknownFailure;
            }
            catch (Exception exception)
                when (exception is OperationCanceledException
                    || exception is ObjectDisposedException
                    || exception is InvalidOperationException)
            {
                Console.WriteLine($"pipelink error: {exception.Message}.");
                return (int)ExitCodes.UnknownFailure;
            }

            return (int)ExitCodes.Success;
        }

        [Flags]
        private enum ExitCodes : int
        {
            Success = 0,
            ArgumentFailure = 1,
            UnknownFailure = 2
        }
    }
}