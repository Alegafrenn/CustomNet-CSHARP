using System;
using CustomNet;
using CustomNet.Packet;

namespace Client
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
            Connected result = client.Connect("127.0.0.1",228);
            Console.WriteLine("Result: "+result);
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
        private static void OnPacketReceived(CustomPacket packet)
        {
            Console.WriteLine("[Packet][ActionID: "+packet.action_id+"][Data: "+packet.data+"]");
        }
    }
}
