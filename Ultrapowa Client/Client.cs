using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ultrapowa_Client
{
    class Client
    {
        public static Socket _Socket = null;

        public Client()
        {
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string _HostName, int _Port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(_HostName);

            // Prefer IPv4 address
            IPAddress _IpAddress = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (_IpAddress == null)
                throw new Exception("No IPv4 address found for host.");

            IPEndPoint _IPEndPoint = new IPEndPoint(_IpAddress, _Port);

            // Connect, don't bind
            _Socket.Connect(_IPEndPoint);

            //Crypto...
        }

    }
}
