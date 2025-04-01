using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications
{
    public class SocketEventArgs(Socket client) : EventArgs
    {
        public Socket Client { get; private set; } = client;
    }
}
