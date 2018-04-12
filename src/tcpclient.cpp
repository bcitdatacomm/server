/**
 * SOURCE FILE:	tcpclient.cpp
 *
 * PROGRAM:		game
 *
 * FUNCTIONS:	TCPClient();
 *				int32_t initializeSocket(EndPoint ep);
 *				int32_t sendBytes(char * data, uint32_t len);
 *				int32_t receiveBytes(char * buffer, uint32_t size);
 *				int32_t closeConnection(int32_t sockfd);
 *
 * DATE:		Mar.
 *
 * REVISIONS:	Mar.
 * 				Apr.
 *
 * DESIGNER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * NOTES:
 * This file is a class wrapper around the client-side TCP functions.
 * The library class uses this class and its methods to send data as a
 * client.
 */
#include "tcpclient.h"


/**
 * FUNCTION:	TCPClient
 *
 * DATE:		Mar. 2018
 *
 * REVISIONS:
 *
 * DESIGNER:	Delan Elliot
 *
 * PROGRAMMER:	Delan Elliot
 *
 * INTERFACE:	int TCPClient::initializeSocket(EndPoint ep)
 *
 * RETURNS:
 *
 * NOTES:
 * Empty constructor for the TCPClient class.
 * This constructor should be called to create a TCPClient, followed
 * by a TCPClient::initializeSocket() call.
 *
 */
TCPClient::TCPClient()
{

}

/**
 * FUNCTION:	initializeSocket
 *
 * DATE:		Mar. 2018
 *
 * REVISIONS:
 *
 * DESIGNER:	Delan Elliot
 *
 * PROGRAMMER:	Delan Elliot, Wilson Hu, Calvin Lai
 *
 * INTERFACE:	int TCPClient::initializeSocket(EndPoint ep)
 *
 * RETURNS:		Returns an int indicating success or failure
 					- -1 on failure, socket descriptor on success
 *
 * NOTES:
 * This function initializes the client socket and connects to the
 * server.
 *
 * It calls socket() and connect() in order to establish the connection.
 */
int TCPClient::initializeSocket(EndPoint ep)
{
	if ((clientSocket = socket(AF_INET, SOCK_STREAM  , 0)) == -1) {
		perror("failed to initialize socket");
		return -1;
	}

	int optFlag = 1;

	if(setsockopt(clientSocket, SOL_SOCKET, SO_REUSEADDR, &optFlag, sizeof(optFlag)) == -1)
	{
		perror("set opts failed");
		return -1;
	}

	memset(&serverAddr, 0, sizeof(struct sockaddr_in));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(ep.port);
	serverAddr.sin_addr.s_addr = htonl(ep.addr);


	if (connect(clientSocket, (struct sockaddr *)&serverAddr, sizeof(serverAddr)) < 0)
	{
		printf("\n Error : Connect Failed \n");
		perror("failure");
		return -1;
	}
	return clientSocket;

}


/**
 * FUNCTION:	closeConnection
 *
 * DATE:		Mar. 2018
 *
 * REVISIONS:	Mar. 2018
 *
 * DESIGNER:	Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Wilson Hu
 *
 * INTERFACE:	int32_t TCPClient::closeConnection(int32_t sockfd)
 *					int32_t sockfd: socket descriptor to close
 *
 * RETURNS:		Returns 0 on success, -1 on failure
 *
 * NOTES:
 * This function is a close() wrapper for the networking library.
 *
 * It closes an input file descriptor.
 */
int32_t TCPClient::closeConnection(int32_t sockfd)
{
	return close(sockfd);
}

/**
 * FUNCTION:	sendBytes
 *
 * DATE:		Mar. 2018
 *
 * REVISIONS:	Mar. 2018
 *
 * DESIGNER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * INTERFACE:	int32_t TCPClient::sendBytes(char* data, uint32_t len)
 *					char* data: pointer to the send buffer
 *					uint32_t len: number of bytes to send
 *
 * RETURNS:		Returns the number of bytes sent, or -1 on failure
 *
 * NOTES:
 * This function is a send() wrapper for the game's networking library.
 */
int32_t TCPClient::sendBytes(char * data, uint32_t len)
{
	int32_t result;
	if ((result = send(clientSocket, data, len , 0 )) == -1) {
		perror("client send error");
	}

	return result;
}

/**
 * FUNCTION:	receiveBytes
 *
 * DATE:		Mar. 2018
 *
 * REVISIONS:	Mar. 2018
 *
 * DESIGNER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * PROGRAMMER:	Calvin Lai, Delan Elliot, Wilson Hu
 *
 * INTERFACE:	int32_t TCPClient::receiveBytes(char * buffer, uint32_t len)
 *					char* buffer: pointer to the receive buffer
 * 					uint32_t len: number of bytes to receive
 *
 * RETURNS:		Returns number of bytes received
 *
 * NOTES:
 * This function wraps the recv() call for the game's networking library.
 *
 * It calls recv until it reads len bytes from the socket.
 */
int32_t TCPClient::receiveBytes(char * buffer, uint32_t len)
{

	size_t n = 0;
	size_t bytesToRead = len;
	while ((n = recv (clientSocket, buffer, bytesToRead, 0)) < bytesToRead)
	{
		buffer += n;
		bytesToRead -= n;
	}
	return (bytesToRead);
}
