using System.Net;
using System;
using System.Text;
using System.Timers;
using System.Net.Sockets;

namespace CustomNet
{
    public class CustomClient
    {
        private Socket socket;
        private byte[] buffer;
        public bool connected { get; private set; }
        private int buffer_size;
        public int id { get; private set; }
        public int ping { get; private set; }
        private int ping_counter;
        private Timer ping_timer = new Timer(500){ AutoReset = true };
        private Timer ping_count = new Timer(1){ AutoReset = true };
        private Timer cooldown_timer = new Timer(50){ AutoReset = false };
        public delegate void _ClientConnected();
        public delegate void _ClientDisconnected(Disconnected reason);
        public delegate void _PacketReceived(Packet packet);
        public _ClientConnected ClientConnected = NotAssignedEvent;
        public _ClientDisconnected ClientDisconnected = NotAssignedEvent;
        public _PacketReceived PacketReceived = NotAssignedEvent;
        private static void NotAssignedEvent(Packet packet) {}
        private static void NotAssignedEvent() {}
        private static void NotAssignedEvent(Disconnected reason) {}
        private bool cooldown;

        public CustomClient(int _buffer_size = 8192)
        {
            socket = null;
            buffer = new byte[_buffer_size];
            buffer_size = _buffer_size;
            connected = false;
            ping = 0;
            ping_counter= 0;
            ping_timer.Elapsed += ping_send;
            ping_count.Elapsed += ping_inc;
            cooldown_timer.Elapsed += cooldown_turnoff;
            cooldown = false;
        }

        public Connected Connect(string ip, int port)
        {
            if(connected) return Connected.Already_Connected;
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            try { socket.Connect(ip,port); }
            catch { return Connected.No_Connection; }
            try
            {
                socket.Send(Encoding.ASCII.GetBytes(Convert.ToString(0)));
                socket.Receive(buffer,0,buffer.Length,SocketFlags.None);
                id = Convert.ToInt32(Encoding.ASCII.GetString(buffer));
            }
            catch
            {
                return Connected.Server_Full;
            }
            socket.BeginReceive(buffer,0,buffer.Length,SocketFlags.None,new AsyncCallback(Receive),null);
            ClientConnected();
            connected = true;
            ping_timer.Start();
            return Connected.Success;
        }
        public ServerData GetServerData(string ip, int port)
        {
            Socket checksocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            try { checksocket.Connect(ip,port); }
            catch { return new ServerData(); }
            try
            {
                ping_timer.Start();
                checksocket.Send(Encoding.ASCII.GetBytes(Convert.ToString(1)));
                checksocket.Receive(buffer,0,buffer.Length,SocketFlags.None);
                ping_timer.Stop();
                ServerData data = (ServerData)PacketHandler.Deserialize(buffer);
                data.server_ping = ping_counter / 2;
                ping_counter = 0;
                buffer = new byte[buffer_size];
                checksocket.Disconnect(false);
                return data;
            }
            catch { return new ServerData();}
        }
        public bool Disconnect()
        {
            if(!connected) return false;
            socket.Send(PacketHandler.Serialize(new Packet(){action_id=-2,data="disconnect"}));
            _Disconnect(Disconnected.By_Client);
            return true;
        }
        public bool SendPacket(Packet packet)
        {
            if(!connected || packet.action_id < 0 || cooldown) return false;
            socket.Send(PacketHandler.Serialize(packet));
            cooldown = true;
            cooldown_timer.Start();
            return true;
        }

        private void _SendPacket(Packet packet) { socket.Send(PacketHandler.Serialize(packet)); }
        private void Receive(IAsyncResult result)
        {
            try
            {
                if(socket.EndReceive(result) <= 0) 
                {
                    _Disconnect(Disconnected.Lost_Connection); 
                    return;
                }
            } catch { return; }
            Packet packet = (Packet)PacketHandler.Deserialize(buffer);
            buffer = new byte[buffer_size];
            if(packet.action_id == -2)
            {
                if(packet.data == (object)"ping")
                {
                    ping_count.Stop();
                    ping = ping_counter / 2;
                    ping_counter = 0;
                }
                if(packet.data == (object)"close")
                {
                    _Disconnect(Disconnected.Server_Closed);
                    return;
                }
            }
            else PacketReceived(packet);
            socket.BeginReceive(buffer,0,buffer.Length,SocketFlags.None,new AsyncCallback(Receive),null);
        }
        private void _Disconnect(Disconnected reason)
        {
            if(!connected) return;
            if(ping_timer.Enabled) ping_timer.Stop();
            socket.Disconnect(false);
            socket = null;
            ClientDisconnected(reason);
            connected = false;
            buffer = new byte[buffer_size];
            ping = 0;
            ping_counter = 0;
        }
        private void ping_send(Object source, ElapsedEventArgs e)
        {
            ping_count.Start();
            _SendPacket(new Packet(){action_id=-2,data="ping"});
        }
        private void ping_inc(Object source, ElapsedEventArgs e) { ping_counter++; }
        private void cooldown_turnoff(object o,ElapsedEventArgs e) { cooldown = false; }
    }
}