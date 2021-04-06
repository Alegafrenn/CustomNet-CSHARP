using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CustomNet.Packet
{
    [Serializable]
    public class CustomPacket
    {
        public int action_id = -1;
        public object data = "";
    }

    public static class CustomPacketHandler
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