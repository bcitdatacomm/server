/**
 * SOURCE FILE:	tcpserver.cpp
 *
 * PROGRAM: 	game.exe
 *
 * FUNCTIONS: 	TCPServer();
 *				int32_t initializeSocket(short port);
 *				int32_t acceptConnection(EndPoint* ep);
 *				int32_t sendBytes(int clientSocket, char * data, unsigned len);
 *				int32_t receiveBytes(int clientSocket, char * buffer, unsigned len);
 *				int32_t closeClientSocket(int32_t clientSocket);
 *				int32_t closeListenSocket(int32_t sockfd);
 *
 * DATE:		Apr. 10, 2018
 *
 * REVISIONS:	Feb.
 * 				March.
 * 				Apr.
 *
 * DESIGNER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * PROGRAMMER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * NOTES:
 * This class contains the C TCP/IP socket calls used by the game's networking
 * library.
 *
 */

#include "tcpserver.h"
#include <cerrno>

// #define BUFLEN					1200		//Buffer length
// #define MAX_NUM_CLIENTS 			30
// #define TRUE						1
// #define FALSE 					0

/**
 * FUNCTION:	TCPServer
 *
 * DATE:		Mar.
 *
 * REVISIONS:
 *
 * DESIGNER:	Delan Elliot
 *
 * PROGRAMMER:	Delan Elliot
 *
 * INTERFACE:	TCPServer()
 *
 * RETURNS:
 *
 * NOTES:
 * This is the TCPServer class' constructor. It does not accept any
 * arguments or initialize any values.
 *
 * This constructor should be called only to create a new TCPServer object.
 * initializeServer should be called immediately afterwards to initialize
 * the socket.
 */
TCPServer::TCPServer()
{

}

/**
 * FUNCTION:	initializeSocket
 *
 * DATE:		Mar.
 *
 * REVISIONS:
 *
 * DESIGNER:	Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Delan Elliot, Wilson Hu
 *
 * INTERFACE:	int32_t TCPServer::initializeSocket	(short port)
 * 					short port: port number
 *
 * RETURNS:		Returns a negative value on failure, otherwise returns
 * 				the socket's file descriptor.
 *
 * NOTES:
 * This function is used to initialize the socket from the input port number.
 *
 * It wraps the C socket(), bind(), and listen() calls for the networking library.
 * This function performs the above socket calls in order and listens for incoming
 * connection requests.
 *
 * The sockopts are set to a timeout of 30s, reuse addr, and reuse port.
 *
 * The function programmer defined error values when the socket fails to
 * initialize, errno when the socket fails to bind, and the server's
 * listen socket descriptor on success.
 */
int32_t TCPServer::initializeSocket	(short port, short timeout)
{
	struct	sockaddr_in server;
	if ((tcpSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1)
	{
		perror ("Can't create a socket");
		return -5;
	}
	int optFlag = 1;

	// Sets server receive timeout to input timeout seconds.
	struct timeval tv;
	tv.tv_sec = timeout;
	tv.tv_usec = 0;

	if (setsockopt(tcpSocket, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(struct timeval)) == -1)
	{
		perror("Failed to setsockopt: timeout");
	}
	fprintf(stderr, "Timeout set to: %ld\n", tv.tv_sec);

	// Set socket to reuse address
	if (setsockopt(tcpSocket, SOL_SOCKET, SO_REUSEADDR, &optFlag, sizeof(optFlag)) == -1)
	{
		perror("Failed to setsockopt: reuseaddr");
		return -4;
	}

	// Set socket to reuse port
	if (setsockopt(tcpSocket, SOL_SOCKET, SO_REUSEPORT, &optFlag, sizeof(optFlag)) == -1)
	{
		perror("Failed to setsockopt: reuseport");
		return -8;
	}

	// Zero memory of server sockaddr_in struct
	memset(&server, 0, sizeof(struct sockaddr_in));

	server.sin_family = AF_INET;
	server.sin_port = htons(port);
	server.sin_addr.s_addr = htonl(INADDR_ANY); // Accept connections from any client

	if (bind(tcpSocket, (struct sockaddr *)&server, sizeof(server)) == -1)
	{
		perror("Can't bind name to socket");
		perror("failed bind.");
		return errno;
	}
	if (listen(tcpSocket, MAX_NUM_CLIENTS) == -1)
	{
		return errno;
	}
	return tcpSocket;
}

/**
 * FUNCTION:	acceptConnection
 *
 * DATE:		Mar.
 *
 * REVISIONS:	Mar.
 * 				Apr. 9 (added timeout), 10, 11 (init takes timeout)
 *
 * DESIGNER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * PROGRAMMER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * INTERFACE:	int32_t TCPServer::acceptConnection(EndPoint* ep)
 * 					EndPoint* ep: pointer to EndPoint struct that will
 * 									hold the Port and IP Address of the
 * 									newly connected client
 *
 * RETURNS:		Returns int value indicating result of accept() call
 * 					- <= 0 on failure, client socket descriptor on success (>0)
 *
 * NOTES:
 * This function is the accept() wrapper for the game's networking library.
 *
 * It takes an EndPoint* and writes the new connection's port and address to the
 * input EndPoint.
 *
 */
int32_t TCPServer::acceptConnection(EndPoint* ep)
{
	int clientSocket;
	sockaddr_in clientAddr;
	socklen_t addrSize = sizeof(clientAddr);
	memset(&clientAddr, 0, addrSize);
	errno = 0;

	if ((clientSocket = accept(tcpSocket, (struct sockaddr*) &clientAddr, &addrSize)) == -1)
	{
		// Accept call times out
		if (errno == EAGAIN)
		{
			return -errno;
		}
		return 0;
	}

	ep->port = ntohs(clientAddr.sin_port);
	ep->addr = ntohl(clientAddr.sin_addr.s_addr);

	return clientSocket;
}

/**
 * FUNCTION:	sendBytes
 *
 * DATE:		Mar.
 *
 * REVISIONS:	Mar.
 *
 * DESIGNER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * PROGRAMMER:	Matthew Shew
 *
 * INTERFACE:	int32_t TCPServer::sendBytes(int clientSocket, char * data, unsigned len)
 * 					int clientSocket: file descriptor of the client socket
 * 					char* data: pointer to the buffer this function sends from
 * 					unsigned len: number of bytes to write
 *
 * RETURNS:		Returns the number of bytes written to the client socket
 *
 * NOTES:
 * This function is the send() wrapper for the game's networking library.
 */
int32_t TCPServer::sendBytes(int clientSocket, char* data, unsigned len)
{
	return send(clientSocket, data, len, 0);
}

/**
 * FUNCTION:	receiveBytes
 *
 * DATE:		Mar. 
 *
 * REVISIONS:	Mar.
 * 				Apr.
 *
 * DESIGNER:	Delan Elliot, Wilson Hu, Matthew Shew
 *
 * PROGRAMMER:	Delan Elliot, Matthew Shew
 *
 * INTERFACE:	int32_t TCPServer::receiveBytes(int clientSocket, char * buffer, unsigned len)
 *
 * RETURNS:		Returns the number of bytes NOT received before timeout
 * 					- 0 on success, >0 on timeout
 *
 * NOTES:
 * This function is a recv() wrapper for the game's networking library.
 * It calls recv() in a loop until it reads len bytes into the buffer.
 *
 * On completion or timeout, it returns the difference between the number of
 * bytes passed in and the number of bytes read from the socket.
 */
int32_t TCPServer::receiveBytes(int clientSocket, char * buffer, unsigned len)
{
	size_t n = 0;
	size_t bytesToRead = len;
	while ((n = recv (clientSocket, buffer, bytesToRead, 0)) < len)
	{
		buffer += n;
		bytesToRead -= n;
	}
	return (len - bytesToRead);
}

/**
 * FUNCTION: 	closeClientSocket
 *
 * DATE:		Mar.
 *
 * REVISIONS:
 *
 * DESIGNER:	Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Wilson Hu
 *
 * INTERFACE:	int32_t TCPServer::closeClientSocket(int32_t clientSocket)
 * 					int32_t clientSocket: client socket's descriptor
 *
 * RETURNS:		Returns the result of the close() call.
 * 					- 0 on success, -1 on failure
 *
 * NOTES:
 * This function is a close() wrapper for the game's networking library.
 * It calls close() on the input socket descriptor.
 */
int32_t TCPServer::closeClientSocket(int32_t clientSocket)
{
	return close(clientSocket);
}

/**
 * FUNCTION:	closeListenSocket
 *
 * DATE:		Mar.
 *
 * REVISIONS:	Mar.
 *
 * DESIGNER:	Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Wilson Hu
 *
 * INTERFACE:	int32_t TCPServer::closeListenSocket(int32_t sockfd)
 * 					int32_t sockfd: server's listen socket descriptor
 *
 * RETURNS:		Returns the result of the close() call.
 * 					- 0 on success, -1 on failure.
 *
 * NOTES:
 * This function is a close() wrpaper for the game's networking library.
 * It calls close() on the input socket descriptor.
 */
int32_t TCPServer::closeListenSocket(int32_t sockfd)
{
	int32_t result = close(sockfd);
	return result;
}
