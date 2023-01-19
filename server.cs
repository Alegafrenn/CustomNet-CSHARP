using System;
using System.Net;
using System.Text;
using System.Timers;
using System.Net.Sockets;

namespace CustomNet
{
    public class CustomServer
    {
        private Socket socket;
        private int buffer_size;
        public bool started { get; private set; }
        public int max_clients { get; private set; }
        public int clients { get; private set; }
        public class _client
        {
            public Socket socket = null;
            public byte[] buffer = null;
            public bool cooldown = false;
        }
        public _client[] client;
        private System.Timers.Timer cooldown_timer = new System.Timers.Timer(50){ AutoReset = true };

        public delegate void _ClientConnected(int clientid);
        public delegate void _ClientDisconnected(int clientid, Disconnected reason);
        public delegate void _PacketReceived(int clientid, Packet packet);
        public _ClientConnected ClientConnected = NotAssignedEvent;
        public _ClientDisconnected ClientDisconnected = NotAssignedEvent;
        public _PacketReceived PacketReceived = NotAssignedEvent;
        private static void NotAssignedEvent(int clientid) {}
        private static void NotAssignedEvent(int clientid, Disconnected reason) {}
        private static void NotAssignedEvent(int clientid, Packet packet) {}

        public CustomServer(int _buffer_size = 8192)
        {
            socket = null;
            buffer_size = _buffer_size;
            started = false;
            max_clients = 0;
            clients = 0;
            cooldown_timer.Elapsed += cooldown_turnoff;
        }

        public bool Start(int port, int _max_clients)
        {
            if(started) return false;
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            max_clients = _max_clients;
            client = new _client[max_clients];
            for(int i = 0;i < max_clients;i++) client[i] = new _client();
            socket.Bind(new IPEndPoint(IPAddress.Any,port));
            socket.Listen(max_clients-1);
            socket.BeginAccept(new AsyncCallback(AcceptConnection),null);
            cooldown_timer.Start();
            started = true;
            return true;
        }
        public bool Stop()
        {
            if(!started) return false;
            for(int i = 0;i < max_clients;i++) if(client[i].socket != null) { _SendPacket(i,new Packet(){action_id=-2,data="close"}); }
            started = false;
            max_clients = 0;
            cooldown_timer.Stop();
            client = null;
            socket.Close();
            return true;
        }
        public bool SendPacket(int id,Packet packet)
        {
            if(!started || client[id].socket == null || packet.action_id < 0 || client[id].cooldown == true) return false;
            client[id].socket.Send(PacketHandler.Serialize(packet));
            client[id].cooldown = true;
            return true;
        }

        private void _SendPacket(int id,Packet packet) { client[id].socket.Send(PacketHandler.Serialize(packet)); }
        private void AcceptConnection(IAsyncResult result)
        {
            if(!started) return;
            Socket clientsocket = socket.EndAccept(result);
            byte[] buffer = new byte[64];
            clientsocket.Receive(buffer);
            if(Convert.ToInt32(Encoding.ASCII.GetString(buffer)) == 1)
            {
                clientsocket.Send(PacketHandler.Serialize(new ServerData(){server_clients=clients,server_max_clients=max_clients}));
                clientsocket.Shutdown(SocketShutdown.Both);
                clientsocket.Close();
                socket.BeginAccept(new AsyncCallback(AcceptConnection),null);
                return;
            }
            if(clients >= max_clients)
            {
                clientsocket.Shutdown(SocketShutdown.Both);
                clientsocket.Close();
                socket.BeginAccept(new AsyncCallback(AcceptConnection),null);
                return;
            }
            for(int i = 0;i < max_clients;i++)
            {
                if(client[i].socket == null)
                {
                    client[i].socket = clientsocket;
                    client[i].socket.Send(Encoding.ASCII.GetBytes(Convert.ToString(i)));
                    client[i].buffer = new byte[buffer_size];
                    client[i].socket.BeginReceive(client[i].buffer,0,client[i].buffer.Length,SocketFlags.None,new AsyncCallback(Receive),(object)i);
                    ClientConnected(i);
                    break;
                }
            }
            clients++;
            socket.BeginAccept(new AsyncCallback(AcceptConnection),null);
        }
        private void Receive(IAsyncResult result)
        {
            int id = Convert.ToInt32(result.AsyncState);
        
            try
            {
                client[id].socket.EndReceive(result);
                Packet packet = (Packet)PacketHandler.Deserialize(client[id].buffer);
                if(packet.action_id == -2)//specific actions
                {
                    if(packet.data == (object)"disconnect")
                    {
                        _Disconnect(id,Disconnected.By_Client);
                        return;
                    }
                    if(packet.data == (object)"ping") 
                    {
                        _SendPacket(id,packet);
                    }
                }
                else PacketReceived(id,packet);
                client[id].buffer = new byte[buffer_size];
                client[id].socket.BeginReceive(client[id].buffer,0,client[id].buffer.Length,SocketFlags.None,new AsyncCallback(Receive),(object)id);
            }
            catch { _Disconnect(id,Disconnected.Lost_Connection); }
        }
        private void _Disconnect(int id,Disconnected reason)
        {
            ClientDisconnected(id,reason);
            client[id].socket.Close();
            client[id].buffer = new byte[buffer_size];
            client[id].socket = null;
            clients--;
        }
        private void cooldown_turnoff(object o,ElapsedEventArgs e)
        {
            for(int i = 0;i < max_clients;i++)
            {
                if(client[i].cooldown) client[i].cooldown = false;
            }
        }
    }
}