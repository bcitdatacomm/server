#ifndef SERVER_DEF
#include <sys/types.h>
#define SERVER_DEF
#include <sys/socket.h>
#include <unistd.h>
#include <netinet/in.h>
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

class Server
{
  public:
	Server();
	int initializeSocket(short port);
	int32_t sendBytes(EndPoint ep, char *data, unsigned len);
	int32_t UdpPollSocket();
	int32_t UdpRecvFrom(char *buffer, uint32_t size, EndPoint *addr);
	sockaddr_in getServerAddr();
	void setEndPointIp(EndPoint *ep, char zero, char one, char two, char three);

  private:
	int udpSocket;
	sockaddr_in serverAddr;
	struct pollfd *poll_events;
};

#endif
