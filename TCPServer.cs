using System;

namespace Networking
{
	public unsafe class TCPServer
    {

		private IntPtr tcpServer;
        private Int32 serverSocket;

		public TCPServer()
		{
			tcpServer = ServerLibrary.TCPServer_CreateServer();
		}

		public Int32 Init(ushort port)
		{
			serverSocket = ServerLibrary.TCPServer_initServer(tcpServer, port);

			return serverSocket;
		}


		public Int32 AcceptConnection(ref EndPoint ep)
        {
			fixed(EndPoint* p = &ep)
			{
				return ServerLibrary.TCPServer_acceptConnection(tcpServer, p);
			}
        }


		public Int32 Recv(Int32 socket, byte[] buffer, Int32 len)
		{
			Int32 length;
			fixed (byte* tmpBuf = buffer)
			{
				UInt32 bufLen = Convert.ToUInt32(len);
				length = ServerLibrary.TCPServer_recvBytes(tcpServer, socket, new IntPtr(tmpBuf), bufLen);
				return length;
			}
		}

        public Int32 Send(Int32 socket, byte[] buffer, Int32 len)
		{
			fixed( byte* tmpBuf = buffer)
			{
				UInt32 bufLen = Convert.ToUInt32 (len);
				Int32 ret = ServerLibrary.TCPServer_sendBytes(tcpServer, socket, new IntPtr(tmpBuf), bufLen);
				return ret;
			}
		}

		public Int32 CloseClientSocket(Int32 clientSocket)
		{
				return ServerLibrary.TCPServer_closeClientSocket(clientSocket);
		}

		public Int32 CloseListenSocket(Int32 sockfd)
		{
            Int32 result = ServerLibrary.TCPServer_closeListenSocket(serverSocket);
            return result;
		}

	}
}
