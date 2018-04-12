#ifndef TCPCLIENT_DEF
#define TCPCLIENT_DEF
#include <netinet/in.h>
#include <arpa/inet.h>
#include <poll.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <cerrno>
#ifndef SOCK_NONBLOCK
#include <fcntl.h>
#define SOCK_NONBLOCK O_NONBLOCK
#endif

#include "EndPoint.h"

class TCPClient {

public:
	TCPClient();
	int32_t initializeSocket(EndPoint ep);
	int32_t sendBytes(char * data, uint32_t len);
	int32_t receiveBytes(char * buffer, uint32_t size);
	int32_t closeConnection(int32_t sockfd);

private:
	int32_t clientSocket;
	sockaddr_in serverAddr;

};

#endif
