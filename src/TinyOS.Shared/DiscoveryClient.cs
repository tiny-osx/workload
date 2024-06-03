using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace TinyOS.Client
{
    public class DiscoveryClient : IDisposable
    {
        public string Host { get; set; } = "unknown";
        public string BoardType { get; set; } = "unknown";
        public int ReceiveTimeout { get; set; } = 200;
        public List<AdaptorInterface> AdaptorInterfaces { get; set; }
        private readonly UdpClient _udpClient;

        public DiscoveryClient()
            : this(8920)
        { }

        public DiscoveryClient(int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);

            _udpClient = new UdpClient(endpoint)
            {
                EnableBroadcast = true
            };

            _udpClient.Client.ReceiveTimeout = ReceiveTimeout;

            AdaptorInterfaces = new List<AdaptorInterface>()
            { new AdaptorInterface()
                {
                    Name = "unknown",
                    IPv4Address =  new List<string>() { "192.168.7.1" },
                    IPv6Address =  new List<string>() { "::" },
                    Priority = 0
                }
            };

            var task = Task.Run(() =>
            {
                try
                {
                    var response = _udpClient.Receive(ref endpoint);
                    var hostInterface = JsonSerializer.Deserialize(response, InterfaceJsonContext.Default.HostInterface);

                    if (hostInterface != null)
                    {
                        Host = hostInterface.Host;
                        BoardType = hostInterface.BoardType;
                        AdaptorInterfaces = hostInterface.AdaptorInterfaces;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            });

            AdaptorInterfaces.Add( new AdaptorInterface()
                {
                    Name = "unknown",
                    IPv4Address =  new List<string>() { "192.168.7.1" },
                    IPv6Address =  new List<string>() { "::" },
                    Priority = 3
                }
            );
            SendToken(port);

            task.Wait();
        }

        private void SendToken(int port)
        {
            var token = Encoding.UTF8.GetBytes("aa832bc6");

            try
            {
                _udpClient.Send(token, token.Length, new IPEndPoint(IPAddress.Broadcast, port));
            }
            catch (Exception)
            {
                return;
            }
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }

       public static string GetIPv4Address()
        {
            return GetIPv4Address(8920);
        }

        public static string GetIPv4Address(int port)
        {
            string address;

            using (var client = new DiscoveryClient(port))
            {
                address = client.AdaptorInterfaces.OrderBy(i => i.Priority).First().IPv4Address.First();
            }

            return address;
        }
        
    }
    
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(HostInterface))]
    [JsonSerializable(typeof(List<AdaptorInterface>))]
    internal partial class InterfaceJsonContext : JsonSerializerContext
    {
    }

    public class HostInterface()
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public required string BoardType { get; set; }
        public required List<AdaptorInterface> AdaptorInterfaces { get; set; }
    }

    public class AdaptorInterface()
    {
        public required string Name { get; set; }
        public required List<string> IPv4Address { get; set; }
        public required List<string> IPv6Address { get; set; }
        public required int  Priority  { get; set; }
    }
}