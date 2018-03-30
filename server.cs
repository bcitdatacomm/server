using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
// using System.Net;
// using System.Net.Sockets;
using Networking;
using InitGuns;

class Server
{
    private const double tickInterval = (double)1000 / (double)128;
    private static DateTime nextTick = DateTime.Now;
    private static Thread sendThread;
    private static Thread recvThread;
    private static bool running;
    private static Mutex mutex;

    private static Networking.Server server;

    private static byte[] sendBuffer = new byte[R.Net.Size.SERVER_TICK];

    static byte nextPlayerId = 1;
    static Dictionary<byte, connectionData> players;

    // Game geneartion variables
    private static Int32[] clientSockFdArr = new Int32[R.Net.MAX_PLAYERS];
    private static Thread[] transmitThreadArr = new Thread[R.Net.MAX_PLAYERS];
    private static Thread listenThread;
    private static byte[] itemData  = new byte[R.Net.TCP_BUFFER_SIZE];
    private static byte[] mapData   = new byte[R.Net.TCP_BUFFER_SIZE];
    private static Int32 numClients = 0;
    private static bool accepting = false;
    private static TCPServer tcpServer;

    private static SpawnPointGenerator spawnPointGenerator = new SpawnPointGenerator();

    public static void Main()
    {
        Console.WriteLine("Starting server");
        mutex = new Mutex();

        pregame();

        startGame();
    }

    public static void pregame()
    {
        players = new Dictionary<byte, connectionData>();
        initTCPServer();
    }

    public static void startGame()
    {
        server = new Networking.Server();
        server.Init(R.Net.PORT);

        sendThread = new Thread(sendThreadFunction);
        recvThread = new Thread(recvThreadFunction);

        mutex.WaitOne();
        running = true;
        mutex.ReleaseMutex();

        sendThread.Start();
        recvThread.Start();
    }

    private static bool isTick()
    {
        if (DateTime.Now >= nextTick)
        {
            nextTick = DateTime.Now.AddMilliseconds(tickInterval);
            return true;
        }

        return false;
    }

    private static void sendThreadFunction()
    {
        Console.WriteLine("Starting Sending Thread");

        while (running)
        {
            if (isTick())
            {
                buildSendPacket();

                byte[] snapshot = new byte[sendBuffer.Length];
                Buffer.BlockCopy(sendBuffer, 0, snapshot, 0, sendBuffer.Length);

                mutex.WaitOne();
                foreach (KeyValuePair<byte, connectionData> pair in players)
                {
                    server.Send(pair.Value.ep, snapshot, snapshot.Length);
                }
                mutex.ReleaseMutex();
            }
        }

        return;
    }

    private static byte generateTickPacketHeader(bool hasPlayer, bool hasBullet, bool hasWeapon, int players)
    {
        byte tmp = 0;

        if (hasPlayer)
        {
            tmp += 128;
        }

        if (hasBullet)
        {
            tmp += 64;
        }

        if (hasWeapon)
        {
            tmp += 32;
        }

        tmp += Convert.ToByte(players);

        return tmp;
    }

    private static void buildSendPacket()
    {
        mutex.WaitOne();
        int offset = R.Net.Offset.PLAYERS;

        sendBuffer[0] = generateTickPacketHeader(true, false, false, players.Count);

        foreach (KeyValuePair<byte, connectionData> pair in players)
        {
            byte id = pair.Key;
            connectionData player = pair.Value;

            sendBuffer[offset] = id;
            Array.Copy(BitConverter.GetBytes(player.x), 0, sendBuffer, offset + 1, 4);
            Array.Copy(BitConverter.GetBytes(player.z), 0, sendBuffer, offset + 5, 4);
            Array.Copy(BitConverter.GetBytes(player.r), 0, sendBuffer, offset + 9, 4);
            // Weapon here

            offset += R.Net.Size.PLAYER_DATA;
        }
        mutex.ReleaseMutex();
    }

    private static void recvThreadFunction()
    {
        Console.WriteLine("Starting Receive Funciton");

        while (running)
        {
            if (isTick())
            {
                // Receive from up to 30 clients per tick
                for (int i = 0; i < 30; i++)
                {
                    // If there is not data continue
                    if (!server.Poll())
                    {
                        continue;
                    }

                    // Prepare to receive
                    EndPoint ep = new EndPoint();
                    byte[] recvBuffer = new byte[R.Net.Size.CLIENT_TICK];

                    // Receive
                    int n = server.Recv(ref ep, recvBuffer, R.Net.Size.CLIENT_TICK);

                    // If invalid amount of data was received discard and continue
                    if (n != R.Net.Size.CLIENT_TICK)
                    {
                        LogError("Server received an invalid amount of data.");
                        continue;
                    }

                    // Handle incoming data if it is correct
                    handleBuffer(recvBuffer, ep);
                }
            }
        }

        return;
    }

    private static void handleBuffer(byte[] inBuffer, EndPoint ep)
    {
        switch (inBuffer[0])
        {
            case R.Net.Header.ACK:
                LogError("ACK from " + ep.ToString());
                addNewPlayer(ep);
                break;

            case R.Net.Header.TICK:
                updateExistingPlayer(ref inBuffer);
                break;

            default:
                LogError("Server received a valid amount of data but the header is incorrect.");
                break;
        }
    }

    private static void updateExistingPlayer(ref byte[] inBuffer)
    {
        byte id = inBuffer[R.Net.Offset.PID];
        float x = BitConverter.ToSingle(inBuffer, R.Net.Offset.X);
        float z = BitConverter.ToSingle(inBuffer, R.Net.Offset.Z);
        float r = BitConverter.ToSingle(inBuffer, R.Net.Offset.R);

        mutex.WaitOne();
        players[id].x = x;
        players[id].z = z;
        players[id].r = r;
        mutex.ReleaseMutex();
    }

    private static void addNewPlayer(EndPoint ep)
    {
        List<float> spawnPoint = spawnPointGenerator.GetNextSpawnPoint();

        mutex.WaitOne();
        connectionData newPlayer = new connectionData(ep, nextPlayerId, spawnPoint[0], spawnPoint[1]);
        nextPlayerId++;
        players[newPlayer.id] = newPlayer;
        mutex.ReleaseMutex();

        sendInitPacket(newPlayer);
    }

    private static void sendInitPacket(connectionData newPlayer)
    {
        byte[] buffer = new byte[R.Net.Size.SERVER_TICK];

        buffer[0] = R.Net.Header.INIT_PLAYER;
        buffer[1] = newPlayer.id;
        int offset = 2;

        // sets the coordinates for the new player
        Array.Copy(BitConverter.GetBytes(newPlayer.x), 0, buffer, offset, 4);
        Array.Copy(BitConverter.GetBytes(newPlayer.z), 0, buffer, offset + 4, 4);

        server.Send(newPlayer.ep, buffer, buffer.Length);
    }

    private static void initTCPServer()
    {
        tcpServer = new TCPServer();
        tcpServer.Init(R.Net.PORT);
        listenThread = new Thread(listenThreadFunc);
        listenThread.Start();
        listenThread.Join();
    }

    private static void generateInitData()
    {
        InitRandomGuns getItems = new InitRandomGuns(R.Net.MAX_PLAYERS);
        itemData = getItems.compressedpcktarray;

        TerrainController tc = new TerrainController();
        while (!tc.GenerateEncoding());
        int terrainDataLength = tc.CompressedData.Length;
        Array.Copy(tc.CompressedData, 0, mapData, 0, terrainDataLength);
    }

    private static void listenThreadFunc()
    {
        Int32 clientsockfd;
        accepting = true;
        //Int32 result;
		Networking.EndPoint ep = new Networking.EndPoint ();

        while (accepting && numClients < R.Net.MAX_PLAYERS)
        {
			clientsockfd = tcpServer.AcceptConnection(ref ep);
            if (clientsockfd <= 0)
            {
                LogError("Accept error: " + clientsockfd);
            }
            else
            {
                clientSockFdArr[numClients] = clientsockfd;
                LogError("Connected client: " + ep.ToString()); //Add toString() for EndPoint
                numClients++;
            }
        }

        generateInitData();

        // Intialize and start transmit threads
        for (int i = 0; i < numClients; i++)
        {
            transmitThreadArr[i] = new Thread(transmitThreadFunc);
            transmitThreadArr[i].Start(clientSockFdArr[i]);
        }
    }

    private static void transmitThreadFunc(object clientsockfd)
    {
        Int32 numSentMap;
        Int32 numSentItem;
        Int32 sockfd = (Int32)clientsockfd;

		numSentItem = tcpServer.Send(sockfd, itemData, R.Net.TCP_BUFFER_SIZE);
        LogError("Num Item Bytes Sent: " + numSentItem);
		numSentMap = tcpServer.Send(sockfd, mapData, R.Net.TCP_BUFFER_SIZE);
        LogError("Num Map Bytes Sent: " + numSentMap);

        tcpServer.CloseClientSocket(sockfd);
    }


    private static void LogError(string s)
    {
        Console.WriteLine(DateTime.Now + " - " + s);
    }
}
