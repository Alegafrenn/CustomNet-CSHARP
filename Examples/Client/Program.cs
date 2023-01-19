using System;
using CustomNet;

namespace Clientabc
{
    class Program
    {
        static CustomClient client = new CustomClient();

        static void Main(string[] args)
        {
            client.ClientConnected = OnConnected;
            client.ClientDisconnected = OnDisconnected;
            client.PacketReceived = OnPacketReceived;
            Handle();
        }
        static void Handle()
        {
            string input = Console.ReadLine();

            if(input == "connect") client.Connect("127.0.0.1",228);
            else if(input == "disconnect") client.Disconnect();
            else if(input == "send") client.SendPacket(new Packet(){action_id=0,data=(object)"Hello World!"});
            else if(input == "serverdata")
            {
                ServerData data = client.GetServerData("127.0.0.1",228);
                Console.WriteLine("[ServerData][MaxClients: " + data.server_max_clients + "][Clients: " + data.server_clients + "][Ping: " + data.server_ping + "]");
            }
            Handle();
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
