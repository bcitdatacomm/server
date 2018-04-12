CC= g++

NAME=Library
FLAGS= -std=c++11 -Wall -ggdb -pedantic -c -fPIC
LINK= -shared


client.o:
	$(CC) $(FLAGS) client.cpp

server.o:
	$(CC) $(FLAGS) server.cpp

tcpserver.o:
	$(CC) $(FLAGS) tcpserver.cpp

tcpclient.o:
	$(CC) $(FLAGS) tcpclient.cpp

library.o:
	$(CC) $(FLAGS) library.cpp

library: server.o  client.o tcpserver.o tcpclient.o library.o
	$(CC) $(LINK)  tcpserver.o server.o client.o tcpclient.o library.o  -L/lib64/ -o libNetwork.so && cp 'libNetwork.so' ../../Assets/Plugins/Network.so

server: server.o  client.o tcpserver.o tcpclient.o library.o
	$(CC) $(LINK)  tcpserver.o server.o client.o tcpclient.o library.o  -L/lib64/ -o libNetwork.so && cp 'libNetwork.so' /usr/lib/libNetwork.so

#library: server.o library.o client.o tcpserver.o tcpclient.o
# 	$(CC) $(LINK) library.o tcpserver.o server.o client.o -o libNetwork.so && cp 'libNetwork.so' ../../Assets/Plugins/Network.so

clean:
	rm -f *.o & rm -f libNetwork.so
