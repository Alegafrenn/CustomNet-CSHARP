using System;
using CustomNet;
using System.Timers;
using CustomNet.Packet;

namespace Server
{
    class Program
    {
        private static int port = 228;
        private static int max_players = 100;
        private static CustomServer server = new CustomServer();
        private static Timer timer = new Timer(500){ AutoReset = true };
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            timer.Elapsed += send_pack;
            server.ClientConnected = OnClientConnected;
            server.ClientDisconnected = OnClientDisconnected;
            server.PacketReceived = OnPacketReceived;
            server.Start(port,max_players);
            Console.WriteLine("Server started on port "+port);
            timer.Start();
            Console.Read();
        }
        private static void OnClientConnected(int id)
        {
            Console.WriteLine("[ClientConnected][ID: "+id+"]");
        }
        private static void OnClientDisconnected(int id,Disconnected reason)
        {
            Console.WriteLine("[ClientDisconnected][ID: "+id+"][Reason: "+reason+"]");
        }
        private static void OnPacketReceived(int id,CustomPacket packet)
        {
            Console.WriteLine("[Packet][ID: "+id+"][ActionID: "+packet.action_id+"][Data: "+packet.data+"]");
        }
        private static void send_pack(object o,ElapsedEventArgs e)
        {
            for(int i = 0;i < max_players;i++)
            {
                if(server.client[i].connected) server.SendPacket(i,new CustomPacket(){action_id=228,data="abc"});
            }
        }
    }
}
