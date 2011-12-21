using System.Net;
using Lidgren.Network;


namespace MineWorld
{
    public class MineWorldClient
    {
        private readonly NetClient _client;
            
        public MineWorldClient(NetConfiguration config)
        {
            _client = new NetClient(config);
        }

        public NetConnectionStatus GetStatus()
        {
            return _client.Status;
        }

        public void SendMsg(NetBuffer msg,NetChannel channel)
        {
            if(_client.Status != NetConnectionStatus.Connected)
            {
                return;
            }
            _client.SendMessage(msg,channel);
        }

        public bool ReadMsg(NetBuffer msg,out NetMessageType type)
        {
            return _client.ReadMessage(msg,out type);
        }

        public void Connect(IPEndPoint ip, NetBuffer connect)
        {
            _client.Connect(ip,connect.ToArray());
        }

        public void Shutdown()
        {
            _client.Shutdown("");
        }

        public void Shutdown(string shutdownmsg)
        {
            _client.Shutdown(shutdownmsg);
        }

        public void Disconnect()
        {
            _client.Disconnect("");
        }

        public void Disconnect(string disconnectmsg)
        {
            _client.Disconnect(disconnectmsg);
        }

        public NetBuffer CreateBuffer()
        {
            return _client.CreateBuffer();
        }

        public void Start()
        {
            _client.Start();
        }

        public void DiscoverLocalServer(int port)
        {
            _client.DiscoverLocalServers(port);
        }

        public void EnableMessages(NetMessageType type)
        {
            _client.SetMessageTypeEnabled(type,true);
        }

        public void DisableMessages(NetMessageType type)
        {
            _client.SetMessageTypeEnabled(type,false);
        }
    }
}