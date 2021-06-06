using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CustomNet
{
    [Serializable] public class Packet
    {
        public int action_id = -1;
        public object data = "";
    }

    [Serializable] public class ServerData
    {
        public int server_clients = -1;
        public int server_max_clients = -1;
        public int server_ping = -1;
    }

    public static class PacketHandler
    {
        private static MemoryStream ms;
        private static BinaryFormatter bf = new BinaryFormatter();
        public static byte[] Serialize(object packet)
        {
            ms = new MemoryStream();
            bf.Serialize(ms,packet);
            return ms.ToArray();         
        }
        public static object Deserialize(byte[] packet)
        {
            ms = new MemoryStream(packet,0,packet.Length);
            return bf.Deserialize(ms);
        }
    }
}