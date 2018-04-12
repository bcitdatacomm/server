/*---------------------------------------------------------------------------------------
--	SOURCE FILE:	server.cpp -   
--
--	PROGRAM:		libNetwork.so (dynamically loaded networking library)
--
--	FUNCTIONS:		Server();
--					int initializeSocket(short port);
--					int32_t sendBytes(EndPoint ep, char *data, unsigned len);
--					int32_t UdpPollSocket();
--					int32_t UdpRecvFrom(char *buffer, uint32_t size, EndPoint *addr);
--		
--	DATE:			February 27th, 2018
--
--	REVISIONS:		March 17th, 2018
--						Delan Elliot: fixed issue with Select causing seg fault - moved back to Poll
--                  
--
--	DESIGNERS:		Delan Elliot, Wilson Hu, Jeff Chou, Jeremy Lee, Matthew Shew, Calvin Lai, William Murphy
--
--	PROGRAMMER:		Delan Elliot, Matthew Shew
--
--	NOTES:
--		This class provides UDP server functionality. 
--		
---------------------------------------------------------------------------------------*/
#ifndef SERVER_DEF
#include "server.h"
#define SERVER_DEF
#endif

Server::Server()
{
	poll_events = new pollfd;
}


/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: initializeSocket
--
-- DATE: March 7th 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Matthew Shew, Calvin Lai
--
-- PROGRAMMER: Delan Elliot, Matthew Shew
--
-- INTERFACE: int32_t initializeSocket(short port)
--								port: open a server on this port
--
-- RETURNS: 0 on success, or -1 if unsuccessfully opened. 
--
-- NOTES:
-- 		init is called once the unmanaged server has been instantiated, and it creates the socket, binds it, and 
--		thus begins listening for datagrams. 
--------------------------------------------------------------------------------------------------------------*/
int32_t Server::initializeSocket(short port)
{
	int optFlag = 1;
	if ((udpSocket = socket(AF_INET, SOCK_DGRAM, 0)) == -1)
	{
		perror("failed to initialize socket");
		return -1;
	}

	if (setsockopt(udpSocket, SOL_SOCKET, SO_REUSEADDR, &optFlag, sizeof(int)) == -1)
	{
		perror("set opts failed");
		return -1;
	}

	memset(&serverAddr, 0, sizeof(struct sockaddr_in));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(port);
	serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	int error = -1;

	if ((error = bind(udpSocket, (struct sockaddr *)&serverAddr, sizeof(serverAddr)) == -1))

	{
		perror("bind error: ");
		return error;
	}

	return 0;
}


/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: sendBytes
--
-- DATE: March 7th 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Matthew Shew, Calvin Lai
--
-- PROGRAMMER: Delan Elliot, Matthew Shew
--
-- INTERFACE: int32_t sendBytes(EndPoint ep, char* data, unsigned len)
--								ep: EndPoint struct with the address and port of the receiving client
--								data: array of binary data to send
--								len: length of data to send in bytes
--
-- RETURNS: the number of bytes sent, or -1 if there is an error.
--
-- NOTES:
-- 		Sends bytes of length len to the address specified by the EndPoint struct. The endpoint is host byte order.
--		The EndPoint struct is filled in C# and the binary data is interpreted reliably because of fixed width types. 
--------------------------------------------------------------------------------------------------------------*/
int32_t Server::sendBytes(EndPoint ep, char *data, unsigned len)
{
	struct sockaddr_in temp;

	memset(&temp, 0, sizeof(sockaddr_in));
	temp.sin_family = AF_INET;
	temp.sin_addr.s_addr = htonl(ep.addr);
	temp.sin_port = htons(ep.port);

	int32_t result = sendto(udpSocket, data, len, 0, (struct sockaddr *)&temp, sizeof(sockaddr_in));
	return result;
}

sockaddr_in Server::getServerAddr()
{
	return serverAddr;
}


/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: UdpRecvFrom
--
-- DATE: March 7th 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Matthew Shew, Calvin Lai
--
-- PROGRAMMER: Delan Elliot, Matthew Shew
--
-- INTERFACE: int32_t UdpRecvFrom(char * buffer, uint32_t size, EndPoint * addr)
--								buffer: the buffer that will be filled with the received datagram
--								size: the size of the buffer ( max recv length)
--								addr: a pointer to an EndPoint struct that will be filled with the 
-									address and port of the sending client
--
-- RETURNS: the number of bytes recv, or -1 if there is an error.
--
-- NOTES:
-- 		Receives datagram of max size "size". The address of the client that sent the datagram is saved into the 
--		EndPoint referenced by addr. 
--------------------------------------------------------------------------------------------------------------*/
int32_t Server::UdpRecvFrom(char *buffer, uint32_t size, EndPoint *addr)
{
	sockaddr_in clientAddr;
	socklen_t addrSize = sizeof(clientAddr);
	memset(&clientAddr, 0, addrSize);

	int32_t result = recvfrom(udpSocket, buffer, size, 0, (struct sockaddr *)&clientAddr, &addrSize);

	addr->port = ntohs(clientAddr.sin_port);
	addr->addr = ntohl(clientAddr.sin_addr.s_addr);

	return result;
}

void Server::setEndPointIp(EndPoint *ep, char zero, char one, char two, char three)
{
	char *tmp = (char *)&(ep->addr);

	tmp[0] = zero;
	tmp[1] = one;
	tmp[2] = two;
	tmp[3] = three;
}


/*------------------------------------------------------------------------------------------------------------
-- FUNCTION: UdpPollSocket
--
-- DATE: February 28th 2018
--
-- REVISIONS:
--
-- DESIGNER: Delan Elliot, Matthew Shew, Calvin Lai
--
-- PROGRAMMER: Delan Elliot, Matthew Shew
--
-- INTERFACE: int32_t UdpPollSocket()
--
-- RETURNS: 1 if data is waiting, 0 if not.
--
-- NOTES:
-- 		Calls Poll() on the server socket.
--------------------------------------------------------------------------------------------------------------*/
int32_t Server::UdpPollSocket()
{
	int numfds = 1;
	struct pollfd pollfds;
	pollfds.fd = udpSocket;

	pollfds.events = POLLIN;

	int retVal = poll(&pollfds, numfds, 0);
	if (retVal == -1)
	{
		perror("poll failed with error: ");
	}

	if (pollfds.revents & POLLIN)
	{
		return SOCKET_DATA_WAITING;
	}

	return SOCKET_NODATA;
}


