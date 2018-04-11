/*---------------------------------------------------------------------------------------
--	SOURCE FILE:	Server.cs -   A C# wrapper class providing UDP server functions
--
--	PROGRAM:		game
--
--	FUNCTIONS:		Init(string ipaddr, ushort port)
--					Poll()
--					Select()
--					Recv(byte[] buffer, Int32 len)
--					Send(byte[] buffer, Int32 len)
--
--	DATE:			February 27th, 2018
--					
--
--	REVISIONS:		(Date and Description)
--
--	DESIGNERS:		Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee
--
--	PROGRAMMER:		Delan Elliot
--
--	NOTES:
--		Server.cs provides a C# wrapper for the C++ functions implemented in the shared library.
--		It uses C# interopservices to interact with unmanaged data at the binary level.
--		
---------------------------------------------------------------------------------------*/
using System;

namespace Networking
{
	public unsafe class Server
	{
		public static Int32 SOCKET_NO_DATA = 0;
		public static Int32 SOCKET_DATA_WAITING = 1;

		private IntPtr server;

		public Server()
		{
			server = ServerLibrary.Server_CreateServer();
		}

/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: Init
--
-- DATE: February 27th, 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Wilson Hu
--
-- PROGRAMMER: Delan Elliot
--
-- INTERFACE: Int32 Init(ushort port)
--								port: open a server on this port
--
-- RETURNS: 0 on success, or -1 if unsuccessfully opened. 
--
-- NOTES:
-- 		Init is called once the unmanaged server has been instantiated, and it creates the socket.
--------------------------------------------------------------------------------------------------------------*/
		public Int32 Init(ushort port)
		{
			Int32 err = ServerLibrary.Server_initServer(server, port);
			return err;
		}

/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: Poll
--
-- DATE: February 27th, 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee
--
-- PROGRAMMER: Delan Elliot
--
-- INTERFACE: Int32 Poll()
--
-- RETURNS: True when data is waiting, false otherwise
--
-- NOTES:
-- 		Call Poll on the unmanaged server and returns true if data is waiting to be recevied, and false otherwise.
--------------------------------------------------------------------------------------------------------------*/
		public bool Poll()
		{
			Int32 p = ServerLibrary.Server_PollSocket(server);
			return Convert.ToBoolean (p);
		}

/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: Select
--
-- DATE: March 5th, 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee
--
-- PROGRAMMER: Jeremy Lee
--
-- INTERFACE: Int32 Select()
--
-- RETURNS: True when data is waiting, false otherwise
--
-- NOTES:
-- 		Call Select on the unmanaged server and returns true if data is waiting to be recevied, and false otherwise.
--------------------------------------------------------------------------------------------------------------*/
        public bool Select()
        {
            Int32 s = ServerLibrary.Server_SelectSocket(server);
            return Convert.ToBoolean (s);
        }

/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: Recv
--
-- DATE: February 27th, 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee
--
-- PROGRAMMER: Delan Elliot
--
-- INTERFACE: Int32 Recv(ref EndPoint ep, byte[] buffer, Int32 len)
--				ep: reference to an EndPoint struct, which is binary address data that can be passed to the unmanaged code
--				buffer: the byte array to write received data into
--				len: the max length of received data (the length of the buffer)
--
-- RETURNS: the number of bytes received
--
-- NOTES:
-- 		Creates two fixed references to ensure the references are not changed by the garbage collector during execution. 
--------------------------------------------------------------------------------------------------------------*/
		public Int32 Recv(ref EndPoint ep, byte[] buffer, Int32 len)
		{
			Int32 length;
			fixed (byte* tmpBuf = buffer)
			{
				fixed(EndPoint * p = &ep) 
				{
					UInt32 bufLen = Convert.ToUInt32(len);
					length = ServerLibrary.Server_recvBytes(server, p, new IntPtr(tmpBuf), bufLen);
				}
				return length;
			}
		}

		public Int32 Send(EndPoint ep, byte[] buffer, Int32 len)
		{
			fixed( byte* tmpBuf = buffer)
			{
				UInt32 bufLen = Convert.ToUInt32 (len);
				Int32 ret = ServerLibrary.Server_sendBytes(server, ep, new IntPtr(tmpBuf), bufLen);
				return ret;
			}
		}
	}
}

