using System;
using CustomNet;

namespace Clientabc
{
    class Program
    {
        static CustomClient client = new CustomClient();
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting...");
            client.ClientConnected = OnConnected;
            client.ClientDisconnected = OnDisconnected;
            client.PacketReceived = OnPacketReceived;
            //Connected result = client.Connect("127.0.0.1",228);
            ServerData data = client.GetServerData("127.0.0.1",228);
            Console.WriteLine("[ServerData][Players: "+data.server_clients+"][MaxPlayers: "+data.server_max_clients+"][Ping: "+data.server_ping+"]");
            //Console.WriteLine("Result: "+result);
            Console.Read();
        }
        private static void OnConnected()
        {
            Console.WriteLine("[Connected][ID: "+client.id+"]");
        }
        private static void OnDisconnected(Disconnected reason)
        {
            Console.WriteLine("[Disconnected][Reason: "+reason+"]");
        }
        private static void OnPacketReceived(Packet packet)
        {
            Console.WriteLine("[Packet][ActionID: "+packet.action_id+"][Data: "+packet.data+"]");
        }
    }
}
