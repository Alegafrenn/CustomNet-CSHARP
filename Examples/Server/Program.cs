using System;
using CustomNet;
using System.Timers;

namespace Server
{
    class Program
    {
        private static int port = 228;
        private static int max_players = 2;
        private static CustomServer server = new CustomServer();
        static void Main(string[] args)
        {
            server.ClientConnected = OnClientConnected;
            server.ClientDisconnected = OnClientDisconnected;
            server.PacketReceived = OnPacketReceived;
            Handle();
        }

        static void Handle()
        {
            string input = Console.ReadLine();

            if(input == "start") {server.Start(port, max_players); Console.WriteLine("Server started on port "+port);}
            else if(input == "stop") server.Stop();
            else if(input == "send") for(int i = server.clients - 1; i >= 0; i--) { server.SendPacket(i,new Packet(){action_id=0,data=(object)"Hello World!"}); }
            Handle();
        }




        private static void OnClientConnected(int id)
        {
            Console.WriteLine("[ClientConnected][ID: "+id+"]");
        }
        private static void OnClientDisconnected(int id,Disconnected reason)
        {
            Console.WriteLine("[ClientDisconnected][ID: "+id+"][Reason: "+reason+"]");
        }
        private static void OnPacketReceived(int id,Packet packet)
        {
            Console.WriteLine("[Packet][ID: "+id+"][ActionID: "+packet.action_id+"][Data: "+packet.data+"]");
        }
    }
}
