/*---------------------------------------------------------------------------------------
--	SOURCE FILE:	library.cpp -   Static exports of interface functions for use in C#
--
--	PROGRAM:		libNetwork.so (dynamically loaded networking library)
--
--	FUNCTIONS:		Server* Server_CreateServer()
--					int32_t Server_initServer(void *serverPtr, short port)
--					int32_t Server_PollSocket(void *serverPtr)
--					int32_t Server_sendBytes(void *serverPtr, EndPoint ep, char *data, uint32_t len)
--					int32_t Server_recvBytes(void *serverPtr, EndPoint *addr, char *buffer, uint32_t bufSize)
--
--                  Client* Client_CreateClient()
--                  int32_t Client_sendBytes(void *clientPtr, char *buffer, uint32_t len)
--                  int32_t Client_recvBytes(void *clientPtr, char *buffer, uint32_t len)
--                  int32_t Client_PollSocket(void *clientPtr)
--                  int32_t Client_initClient(void *clientPtr, EndPoint ep)
--
--                  TCPServer* TCPServer_CreateServer()
--                  int32_t TCPServer_initServer(void * serverPtr, short port)
--                  int32_t TCPServer_acceptConnection(void * serverPtr, EndPoint * ep)
--                  int32_t TCPServer_sendBytes(void * serverPtr, int32_t clientSocket, char * data, uint32_t len)
--                  int32_t TCPServer_recvBytes(void * serverPtr, int32_t clientSocket, char * buffer, uint32_t bufSize)
--                  int32_t TCPServer_closeClientSocket(void* serverPtr, int32_t clientSocket)
--                  void TCPServer_closeListenSocket(void* serverPtr, int32_t sockfd)
--
--                  TCPClient* TCPClient_CreateClient()
--                  int32_t TCPClient_initClient(void *clientPtr, EndPoint ep)
--                  int32_t TCPClient_sendBytes(void *clientPtr, char *buffer, uint32_t len)
--                  int32_t TCPClient_recvBytes(void *clientPtr, char *buffer, uint32_t len)
--                  int32_t TCPClient_closeConnection(void *clientPtr, int32_t sockfd)
--
--	DATE:			March 10th, 2018
--
--	REVISIONS:		
--                  March 17th, 2018: added TCP server functions - Wilson Hu
--
--	DESIGNERS:		Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee, Matthew Shew, Calvin Lai, William Murphy
--
--	PROGRAMMER:		Delan Elliot, Wilson Hu
--
--	NOTES:
--		library.cpp provides an external interface to call our networking code through the use of a 
--      shared library file. These are all the functions that can be called in any environment that
--      can do a "DllImport". In our case, it is imported into C# through interop libraries. 
--  
--	    The function returns and parameters are all chosen because they are reliably interoperable
--      with C# types. All int types have a width specifier so we know the size of the binary 
--      representation. 
--
--      The general form for the interface is: 
--  
--      extern "C" [Return Type] [Class]_[FunctionName](void * [Class Ptr], params...)
--      {
--          ((Class * cast)classPtr)->[FunctionName](params);
--      }
--      
--      Each of the functions essentially conform to this structure. 
---------------------------------------------------------------------------------------*/

#include "tcpserver.h"
#include "client.h"
#include "server.h"
#include "tcpclient.h"



// UDP SERVER
extern "C" Server *Server_CreateServer()
{
    return new Server();
}

extern "C" int32_t Server_initServer(void *serverPtr, short port)
{
    return ((Server *)serverPtr)->initializeSocket(port);
}

extern "C" int32_t Server_PollSocket(void *serverPtr)
{
    return ((Server *)serverPtr)->UdpPollSocket();
}


extern "C" int32_t Server_sendBytes(void *serverPtr, EndPoint ep, char *data, uint32_t len)
{
    return ((Server *)serverPtr)->sendBytes(ep, data, len);
}

extern "C" int32_t Server_recvBytes(void *serverPtr, EndPoint *addr, char *buffer, uint32_t bufSize)
{

    int32_t result = ((Server *)serverPtr)->UdpRecvFrom(buffer, bufSize, addr);
    return result;
}


//UDP CLIENT
extern "C" Client *Client_CreateClient()
{
    return new Client();
}

extern "C" int32_t Client_sendBytes(void *clientPtr, char *buffer, uint32_t len)
{
    Client *p = (Client *)clientPtr;
    return p->sendBytes(buffer, len);
}

extern "C" int32_t Client_recvBytes(void *clientPtr, char *buffer, uint32_t len)
{
    return ((Client *)clientPtr)->receiveBytes(buffer, len);
}

extern "C" int32_t Client_PollSocket(void *clientPtr)
{
    return ((Client *)clientPtr)->UdpPollSocket();
}


extern "C" int32_t Client_initClient(void *clientPtr, EndPoint ep)
{
    return ((Client *)clientPtr)->initializeSocket(ep);
}


//TCP SERVER
extern "C" TCPServer * TCPServer_CreateServer()
{
	return new TCPServer();
}

extern "C" int32_t TCPServer_initServer(void * serverPtr, short port, short timeout)
{
	return ((TCPServer *)serverPtr)->initializeSocket(port, timeout);
}

extern "C" int32_t TCPServer_acceptConnection(void * serverPtr, EndPoint * ep)
{
	return ((TCPServer *)serverPtr)->acceptConnection(ep);
}

extern "C" int32_t TCPServer_sendBytes(void * serverPtr, int32_t clientSocket, char * data, uint32_t len)
{
	return ((TCPServer *)serverPtr)->sendBytes(clientSocket, data, len);
}

extern "C" int32_t TCPServer_recvBytes(void * serverPtr, int32_t clientSocket, char * buffer, uint32_t bufSize)
{
	return ((TCPServer *)serverPtr)->receiveBytes(clientSocket, buffer, bufSize);
}

extern "C" int32_t TCPServer_closeClientSocket(void* serverPtr, int32_t clientSocket)
{
    return ((TCPServer*)serverPtr)->closeClientSocket(clientSocket);
}

extern "C" int32_t TCPServer_closeListenSocket(void* serverPtr, int32_t sockfd)
{
    return ((TCPServer*)serverPtr)->closeListenSocket(sockfd);
}



//TCP CLIENT
extern "C" TCPClient *TCPClient_CreateClient()
{
    return new TCPClient();
}

extern "C" int32_t TCPClient_initClient(void *clientPtr, EndPoint ep)
{
    return ((TCPClient *)clientPtr)->initializeSocket(ep);
}

extern "C" int32_t TCPClient_sendBytes(void *clientPtr, char *buffer, uint32_t len)
{
    return ((TCPClient *)clientPtr)->sendBytes(buffer, len);
}

extern "C" int32_t TCPClient_recvBytes(void *clientPtr, char *buffer, uint32_t len)
{
    return ((TCPClient *)clientPtr)->receiveBytes(buffer, len);
}

extern "C" int32_t TCPClient_closeConnection(void *clientPtr, int32_t sockfd)
{
    return ((TCPClient *)clientPtr)->closeConnection(sockfd);
}



