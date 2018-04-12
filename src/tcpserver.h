#ifndef TCP_DEF
#define TCP_DEF
#include <sys/types.h>
#include <sys/socket.h>
#include <unistd.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <map>
#include <poll.h>
#include <errno.h>
#include <iostream>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <netdb.h>
#include <errno.h>

#include "EndPoint.h"

#ifndef SOCK_NONBLOCK
#include <fcntl.h>
#define SOCK_NONBLOCK O_NONBLOCK
#endif

#define SOCKET_NODATA 0
#define SOCKET_DATA_WAITING 1

#define BUFLEN					1200		//Buffer length
#define MAX_NUM_CLIENTS 		30
#define TRUE					1
#define FALSE 					0



class TCPServer {

public:
	TCPServer();
	int32_t initializeSocket(short port, short timeout);
	int32_t acceptConnection(EndPoint* ep);
	int32_t sendBytes(int clientSocket, char * data, unsigned len);
	int32_t receiveBytes(int clientSocket, char * buffer, unsigned len);
	int32_t closeClientSocket(int32_t clientSocket);
	int32_t closeListenSocket(int32_t sockfd);



private:
	int tcpSocket;
	sockaddr_in serverAddr;
	struct pollfd* poll_events;

};

#endif
