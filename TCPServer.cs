/************************************************************************************
SOURCE FILE: 	TCPServer.cs

PROGRAM:		game

FUNCTIONS:		TCPServer()
				Init()
				AcceptConnection()
				Recv()
				Send()
				CloseClientSocket()
				CloseListenSocket()

DATE:			Mar. 14, 2018

REVISIONS:

DESIGNER:		Delan Elliot, Wilson Hu, Jeremy Lee, Jeff Chou

PROGRAMMER:		Delan Elliot, Wilson Hu

NOTES:
This class represents a TCP server object that handles
TCP connections. Its methods are C# wrappers that call the
networking library's functions. 
**********************************************************************************/
using System;

namespace Networking
{
	public unsafe class TCPServer
    {

		private IntPtr tcpServer;
        private Int32 serverSocket;

		/************************************************************************************
		FUNCTION:	TCPServer

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Delan Elliot

		PROGRAMMER:	Delan Elliot

		INTERFACE:	TCPServer()

		NOTES:
		This constructor calls the library's TCPServer_CreateServer
		function, which creates a corresponding C++ TCPServer object.
		**********************************************************************************/
		public TCPServer()
		{
			tcpServer = ServerLibrary.TCPServer_CreateServer();
		}

		/************************************************************************************
		FUNCTION:	Init

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Delan Elliot

		PROGRAMMER:	Delan Elliot

		INTERFACE:	public Int32 Init(ushort port)
						ushort port: port number for the listening socket

		RETURNS:	Returns an Int32 indicating success or failure
						- <=0 on failure, >0 listen socket descriptor on success

		NOTES:
		This function is the C# wrapper around the library's TCPServer::initializeSocket
		function.
		**********************************************************************************/
		public Int32 Init(ushort port, ushort timeout)
		{
			serverSocket = ServerLibrary.TCPServer_initServer(tcpServer, port, timeout);

			return serverSocket;
		}

		/************************************************************************************
		FUNCTION:	AcceptConnection

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Delan Elliot

		PROGRAMMER:	Delan Elliot

		INTERFACE:	public Int32 AcceptConnection(ref EndPoint ep)
						ref EndPoint ep: EndPoint struct reference that holds the newly
										 connected client's IP & port.

		RETURNS:	Returns an Int32 value depending on success or failure
						- <= 0 on failure, >0 socket descriptor (client) on success

		NOTES:
		This function is the C# wrapper around the library's TCPServer::acceptConnection
		function.
		**********************************************************************************/
		public Int32 AcceptConnection(ref EndPoint ep)
        {
			fixed(EndPoint* p = &ep)
			{
				return ServerLibrary.TCPServer_acceptConnection(tcpServer, p);
			}
        }

		/************************************************************************************
		FUNCTION:	Recv

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Delan Elliot

		PROGRAMMER: Delan Elliot

		INTERFACE:	public Int32 Recv(Int32 socket, byte[] buffer, Int32 len)
						Int32 socket: client socket descriptor
						byte[] buffer: receive buffer
						Int32 len: number of bytes to receive

		RETURNS:	Returns the number of bytes received
						- len on success, <len on failure

		NOTES:
		This function is the C# wrapper for the library's TCPServer::receiveBytes
		function.

		It reads len bytes from the input socket descriptor into the input buffer.
		**********************************************************************************/
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

		/************************************************************************************
		FUNCTION:	Send

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Delan Elliot

		PROGRAMMER:	Delan Elliot

		INTERFACE:	public Int32 Send(Int32 socket, byte[] buffer, Int32 len)
						Int32 socket: client socket descriptor
						byte[] buffer: buffer to send
						Int32 len: number of bytes to send

		RETURNS:	Returns the number of bytes sent.
						- len on success, <len on failure

		NOTES:
		This function is the C# wrapper for the library's TCPServer::sendBytes
		function.

		It sends len bytes from the buffer to the input socket descriptor. 
		**********************************************************************************/
        public Int32 Send(Int32 socket, byte[] buffer, Int32 len)
		{
			fixed( byte* tmpBuf = buffer)
			{
				UInt32 bufLen = Convert.ToUInt32 (len);
				Int32 ret = ServerLibrary.TCPServer_sendBytes(tcpServer, socket, new IntPtr(tmpBuf), bufLen);
				return ret;
			}
		}

		/************************************************************************************
		FUNCTION:	CloseClientSocket

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Wilson Hu

		PROGRAMMER:	Wilson Hu

		INTERFACE:	public Int32 CloseClientSocket(Int32 sockfd)
						Int32 sockfd: client socket descriptor

		RETURNS:	Returns Int32 value depending on success or failure
						- -1 on failure, 0 on success.

		NOTES:
		This function is the C# wrapper for the library's TCPServer::closeClientSocket
		function.

		It closes the input socket descriptor (clientSocket)
		**********************************************************************************/
		public Int32 CloseClientSocket(Int32 sockfd)
		{
				return ServerLibrary.TCPServer_closeClientSocket(sockfd);
		}

		/************************************************************************************
		FUNCTION:	CloseListenSocket

		DATE:		Mar. 14, 2018

		REVISIONS:

		DESIGNER:	Wilson Hu

		PROGRAMMER:	Wilson Hu

		INTERFACE:	public Int32 CloseListenSocket(Int32 sockfd)

		RETURNS:	Returns Int32 value depending on success or failure
						- -1 on failure, 0 on success.

		NOTES:
		This function is the C# wrapper for the library's TCPServer::closeListenSocket
		function.

		It closes the input socket descriptor (sockfd).
		**********************************************************************************/
		public Int32 CloseListenSocket(Int32 sockfd)
		{
            Int32 result = ServerLibrary.TCPServer_closeListenSocket(serverSocket);
            return result;
		}

	}
}
