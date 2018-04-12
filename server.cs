using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Networking;
using InitGuns;

class Server
{
    private static DateTime nextTick = DateTime.Now;
    private static Thread sendThread;
    private static Thread recvThread;
    private static Thread gameThread;
    private static bool running;
    private static Mutex mutex;

    private static Networking.Server server;

    private static byte[] sendBuffer = new byte[R.Net.Size.SERVER_TICK];

    private static bool overtime = false;
    private static Random random = new Random();

    private static byte nextPlayerId = 1;
    private static Dictionary<byte, Player> players;
    private static HashSet<byte> deadPlayers = new HashSet<byte>();
    private static Stack<Bullet> newBullets = new Stack<Bullet>();
    private static Dictionary<int, Bullet> bullets = new Dictionary<int, Bullet>();
    private static Stack<Tuple<byte, int>> weaponSwapEvents = new Stack<Tuple<byte, int>>();
    private static TerrainController tc = new TerrainController();

    // Game generation variables
    private static Int32[] clientSockFdArr = new Int32[R.Net.MAX_PLAYERS];
    private static Thread[] transmitThreadArr = new Thread[R.Net.MAX_PLAYERS];
    private static Thread listenThread;
    private static byte[] itemData = new byte[R.Net.TCP_BUFFER_SIZE];
    private static byte[] mapData = new byte[R.Net.TCP_BUFFER_SIZE];
    private static Int32 numClients = 0;
    private static bool accepting = false;
    private static TCPServer tcpServer;

    private static DangerZone dangerZone;
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
        players = new Dictionary<byte, Player>();
        initTCPServer();
    }

    public static void startGame()
    {
        server = new Networking.Server();
        server.Init(R.Net.PORT);

        sendThread = new Thread(sendThreadFunction);
        recvThread = new Thread(recvThreadFunction);
        gameThread = new Thread(gameThreadFunction);

        mutex.WaitOne();
        running = true;
        mutex.ReleaseMutex();

        sendThread.Start();
        recvThread.Start();
        gameThread.Start();
    }

    private static bool isTick()
    {
        if (DateTime.Now > nextTick)
        {
            nextTick = DateTime.Now.AddMilliseconds(R.Game.TICK_INTERVAL);
            return true;
        }
        return false;
    }

    private static void gameThreadFunction()
    {
        try
        {
            while (running)
            {
                if (isTick())
                {
                    dangerZone.Update();

                    Dictionary<int, int> bulletIds = new Dictionary<int, int>();

                    mutex.WaitOne();
                    foreach (KeyValuePair<int, Bullet> bullet in bullets)
                    {
                        if (tc.IsOccupied(bullet.Value))
                        {
                            bulletIds[bullet.Key] = bullet.Key;
                        }
                    }
                    mutex.ReleaseMutex();

                    mutex.WaitOne();
                    foreach (KeyValuePair<byte, Player> player in players)
                    {
                        dangerZone.HandlePlayer(player.Value);
                    }
                    mutex.ReleaseMutex();

                    // Loop through players
                    foreach (KeyValuePair<byte, Player> player in players)
                    {
                        // Loop through bullets
                        mutex.WaitOne();
                        foreach(KeyValuePair<int, Bullet> bullet in bullets)
                        {
                            // If bullet collides
                            if (bullet.Value.PlayerId == player.Value.id)
                            {
                                continue;
                            }
                            if (bullet.Value.isColliding(player.Value.x, player.Value.z, R.Game.Players.RADIUS))
                            {
                                // Subtract health
                                if (player.Value.h < bullet.Value.Damage)
                                {
                                    player.Value.h = 0;
                                }
                                else
                                {
                                    player.Value.TakeDamage(bullet.Value.Damage);
                                }
                                // Signal delete
                                bulletIds[bullet.Key] = bullet.Key;
                            }
                        }
                        mutex.ReleaseMutex();
                    }

                    mutex.WaitOne();
                    foreach (KeyValuePair<int, Bullet> pair in bullets)
                    {
                        // Update bullet positions
                        if (!pair.Value.Update())
                        {
                            // Remove expired bullets
                            bulletIds[pair.Key] = pair.Key;
                        }
                    }
                    mutex.ReleaseMutex();

                    // Remove bullets
                    mutex.WaitOne();
                    foreach (KeyValuePair<int, int> pair in bulletIds)
                    {
                        bullets[pair.Key].Event = R.Game.Bullet.REMOVE;
                        newBullets.Push(bullets[pair.Key]);
                        bullets.Remove(pair.Key);
                    }
                    mutex.ReleaseMutex();
                }
            }
        }
        catch (Exception e)
        {
            LogError("Game Logic Thread Exception");
            LogError(e.ToString());
        }
    }

    private static void sendThreadFunction()
    {
        Console.WriteLine("Starting Sending Thread");
        while (running)
        {
            try
            {
                if (isTick())
                {
                    buildSendPacket();

                    byte[] snapshot = new byte[sendBuffer.Length];
                    Buffer.BlockCopy(sendBuffer, 0, snapshot, 0, sendBuffer.Length);

                    foreach (KeyValuePair<byte, Player> pair in players)
                    {
                        updateHealthPacket(pair.Value, snapshot);
                        server.Send(pair.Value.ep, snapshot, snapshot.Length);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Send Thread Exception");
                LogError(e.ToString());
            }
        }
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

    private static void updateHealthPacket(Player player, byte[] snapshot)
    {
        int offset = R.Net.Offset.HEALTH;
        mutex.WaitOne();
        Array.Copy(BitConverter.GetBytes(player.h), 0, snapshot, offset, 1);
        mutex.ReleaseMutex();
    }

    private static void buildSendPacket()
    {
        int offset = R.Net.Offset.PLAYERS;
        int bulletOffset = R.Net.Offset.BULLETS;
        int weaponOffset = R.Net.Offset.WEAPONS;

        // Header
        mutex.WaitOne();

        sendBuffer[0] = generateTickPacketHeader(true, newBullets.Count > 0, weaponSwapEvents.Count > 0, players.Count - deadPlayers.Count);

        // Danger zone
        Array.Copy(dangerZone.ToBytes(), 0, sendBuffer, R.Net.Offset.DANGER_ZONE, 16);

        // Player data
        foreach (KeyValuePair<byte, Player> pair in players)
        {
            byte id = pair.Key;
            Player player = pair.Value;

            sendBuffer[offset] = id;
            Array.Copy(BitConverter.GetBytes(player.x), 0, sendBuffer, offset + 1, 4);
            Array.Copy(BitConverter.GetBytes(player.z), 0, sendBuffer, offset + 5, 4);
            Array.Copy(BitConverter.GetBytes(player.r), 0, sendBuffer, offset + 9, 4);
            offset += R.Net.Size.PLAYER_DATA;
        }
        mutex.ReleaseMutex();

        if (newBullets.Count > 0)
        {
            // Bullet data
            mutex.WaitOne();
            sendBuffer[bulletOffset] = Convert.ToByte(newBullets.Count);
            bulletOffset++;

            while (newBullets.Count > 0)
            {
                Bullet bullet = newBullets.Pop();
                if (bullet == null)
                {
                    continue;
                }
                sendBuffer[bulletOffset] = bullet.PlayerId;
                Array.Copy(BitConverter.GetBytes(bullet.BulletId), 0, sendBuffer, bulletOffset + 1, 4);
                sendBuffer[bulletOffset + 5] = bullet.Type;
                if (bullet.Event != R.Game.Bullet.IGNORE)
                {
                    sendBuffer[bulletOffset + 6] = bullet.Event;
                }
                else
                {
                    LogError("Bullet event is set to ignore");
                }
                bulletOffset += 7;
            }
            mutex.ReleaseMutex();
            //Console.WriteLine(BitConverter.ToString(sendBuffer));
        }

        if (weaponSwapEvents.Count > 0)
        {
            // Weapon swap event
            mutex.WaitOne();
            sendBuffer[weaponOffset] = Convert.ToByte(weaponSwapEvents.Count);
            weaponOffset++;

            while (weaponSwapEvents.Count > 0)
            {
                Tuple<byte, int> weaponSwap = weaponSwapEvents.Pop();
                sendBuffer[weaponOffset] = weaponSwap.Item1;
                Array.Copy(BitConverter.GetBytes(weaponSwap.Item2), 0, sendBuffer, weaponOffset + 1, 4);
                weaponOffset += 5;
            }
            mutex.ReleaseMutex();
        }
    }

    private static void recvThreadFunction()
    {
        Console.WriteLine("Starting Receive Function");

        try
        {
            while (running)
            {
                // if (isTick())
                // {
                    // Receive from up to 30 clients per tick
                    for (int i = 0; i < R.Net.MAX_PLAYERS; i++)
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
                // }
            }
        }
        catch (Exception e)
        {
            LogError("Receive Thread Exception");
            LogError(e.ToString());
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

        int weaponId = BitConverter.ToInt32(inBuffer, R.Net.Offset.WEAPON_ID);
        byte weaponType = inBuffer[R.Net.Offset.WEAPON_TYPE];
        handleIncomingWeapon(id, weaponId, weaponType);

        int bulletId = BitConverter.ToInt32(inBuffer, R.Net.Offset.BULLET_ID);
        byte bulletType = inBuffer[R.Net.Offset.BULLET_TYPE];
        handleIncomingBullet(id, bulletId, bulletType);

        mutex.WaitOne();
        if (players[id].IsDead())
        {
            deadPlayers.Add(id);
        }

        players[id].x = x; //crashing here
        players[id].z = z;
        players[id].r = r;

        mutex.ReleaseMutex();
    }

    private static void handleIncomingBullet(byte playerId, int bulletId, byte bulletType)
    {
        if (bulletType != 0)
        {
            Player player = players[playerId];
            Bullet bullet = new Bullet(bulletId, bulletType, player);
            bullet.Event = R.Game.Bullet.ADD;
            mutex.WaitOne();
            newBullets.Push(bullet);
            bullets[bulletId] = bullet;
            mutex.ReleaseMutex();
        }
    }

    private static void handleIncomingWeapon(byte playerId, int weaponId, byte weaponType)
    {
        if (weaponId != 0)
        {
            mutex.WaitOne();
            if (players[playerId].currentWeaponId == weaponId)
            {
                mutex.ReleaseMutex();
                return;
            }

            players[playerId].currentWeaponId = weaponId;
            players[playerId].currentWeaponType = weaponType;

            weaponSwapEvents.Push(Tuple.Create(playerId, weaponId));
            mutex.ReleaseMutex();

            Console.WriteLine("Player {0} changed weapon to -> Weapon: ID - {1}, Type - {2}", playerId, weaponId, weaponType);
        }
    }

    private static void addNewPlayer(EndPoint ep)
    {
        List<float> spawnPoint = spawnPointGenerator.GetNextSpawnPoint();

        mutex.WaitOne();
        Player newPlayer = new Player(ep, nextPlayerId, spawnPoint[0], spawnPoint[1]);
        nextPlayerId++;
        players[newPlayer.id] = newPlayer;
        mutex.ReleaseMutex();

        sendInitPacket(newPlayer);
    }

    private static void sendInitPacket(Player newPlayer)
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
        tcpServer.Init(R.Net.PORT, R.Net.TIMEOUT);
        listenThread = new Thread(listenThreadFunc);
        listenThread.Start();
        listenThread.Join();
    }

    private static void generateInitData()
    {
        dangerZone = new DangerZone();

        InitRandomGuns getItems = new InitRandomGuns(R.Net.MAX_PLAYERS);
        itemData = getItems.compressedpcktarray;

        while (!tc.GenerateEncoding()) ;
        int terrainDataLength = tc.CompressedData.Length;
        Array.Copy(tc.CompressedData, 0, mapData, 0, terrainDataLength);
    }

    private static void listenThreadFunc()
    {
        Int32 clientsockfd;
        accepting = true;
		Networking.EndPoint ep = new Networking.EndPoint();

        // Accept loop, accepts incoming client requests if there are <30 clients or loop is broken
        while (accepting && numClients < R.Net.MAX_PLAYERS)
        {
			clientsockfd = tcpServer.AcceptConnection(ref ep);

            // Breaks loop only if there are >1 clients and AcceptConnection call times out
            if (clientsockfd == R.Net.TIMEOUT_ERRNO && numClients > 1)
            {
                LogError("Accept timeout: Breaking out of listen loop");
                accepting = false;
            }
            // If AcceptConnection call returns an error
            if (clientsockfd <= 0)
            {
                LogError("Accept error: " + clientsockfd);
            }
            // AcceptConnection call passes
            else
            {
                clientSockFdArr[numClients] = clientsockfd;
                LogError("Connected client: " + ep.ToString()); //Add toString() for EndPoint
                numClients++;
            }
        }

        // Generate game initialization data - weapon spawns & map data
        generateInitData();

        // Intialize and start transmit threads
        for (int i = 0; i < numClients; i++)
        {
            transmitThreadArr[i] = new Thread(transmitThreadFunc);
            transmitThreadArr[i].Start(clientSockFdArr[i]);
        }

        // Join each transmitThread
        foreach (Thread t in transmitThreadArr)
        {
            if (t != null)
            {
                t.Join();
            }
        }

        LogError("All threads joined, Starting game");
    }

    private static void transmitThreadFunc(object clientsockfd)
    {
        Int32 numSentMap;
        Int32 numSentItem;
        Int32 sockfd = (Int32)clientsockfd;

        // Send item spawn data to the client
        numSentItem = tcpServer.Send(sockfd, itemData, R.Net.TCP_BUFFER_SIZE);
        LogError("Num Item Bytes Sent: " + numSentItem);

        // Send map data to the client
        numSentMap = tcpServer.Send(sockfd, mapData, R.Net.TCP_BUFFER_SIZE);
        LogError("Num Map Bytes Sent: " + numSentMap);

        // Close client TCP socket
        tcpServer.CloseClientSocket(sockfd);
    }

    private static void LogError(String s)
    {
        Console.WriteLine(DateTime.Now + " - " + s);
    }
}
