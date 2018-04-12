#ifndef CLIENT_DEF
#define CLIENT_DEF

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <poll.h>
#include <iostream>
#include <string.h>
#include "EndPoint.h"
#ifndef SOCK_NONBLOCK
#include <fcntl.h>
#define SOCK_NONBLOCK O_NONBLOCK
#endif

#define MAX_FD 1
#define SOCKET_NODATA 0
#define SOCKET_DATA_WAITING 1




class Client 
{

public:
	Client();
	int32_t initializeSocket(EndPoint ep);
	int32_t sendBytes(char * data, uint32_t len);
	int32_t receiveBytes(char * buffer, uint32_t size);
	int32_t UdpPollSocket();

private:
	int clientSocket;
	sockaddr_in serverAddr;

};

#endif
