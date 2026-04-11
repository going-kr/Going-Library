using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications
{
    /// <summary>
    /// 소켓 연결/해제 시 발생하는 이벤트의 인자 클래스
    /// </summary>
    /// <param name="client">이벤트와 관련된 소켓 클라이언트</param>
    public class SocketEventArgs(Socket client) : EventArgs
    {
        /// <summary>
        /// 이벤트와 관련된 소켓 클라이언트를 가져옵니다.
        /// </summary>
        public Socket Client { get; private set; } = client;
    }
}
