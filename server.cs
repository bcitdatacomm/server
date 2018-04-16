/*---------------------------------------------------------------------------------------
--    SOURCE FILE:    Server.cs
--
--    PROGRAM:        server
--
--    FUNCTIONS:        
--                    public static void Main()
--                    public static void pregame()
--                    public static void startGame()
--                    private static bool isTick()
--                    private static void gameThreadFunction()
--                    private static void sendThreadFunction()
--                    private static byte generateTickPacketHeader(bool hasPlayer, bool hasBullet, bool hasWeapon, int players)
--                    private static void updateHealthPacket(Player player, byte[] snapshot)
--                    private static void buildSendPacket()
--                    private static void recvThreadFunction()
--                    private static void handleBuffer(byte[] inBuffer, EndPoint ep)
--                    private static void updateExistingPlayer(ref byte[] inBuffer)
--                    private static void handleIncomingBullet(byte playerId, int bulletId, byte bulletType)
--                    private static void handleIncomingWeapon(byte playerId, int weaponId, byte weaponType)
--                    private static void addNewPlayer(EndPoint ep)
--                    private static void sendInitPacket(Player newPlayer)
--                    private static void initTCPServer()
--                    private static void generateInitData()
--                    private static void listenThreadFunc()
--                    private static void transmitThreadFunc(object clientsockfd)
--
--    DATE:           Feb 18, 2018
--
--    REVISIONS:      Mar 18, 2018 - Created separate repo for server
--                    Mar 30, 2018 - Moved the server off unity to a seperate script
--                    Apr 2, 2018 - Added bullet handling
--                    Apr 11, 2018 - Merged in danger zone
--
--    DESIGNERS:      Benny Wang, Tim Bruecker, Haley Booker, Alfred Swinton
--
--    PROGRAMMER:     Benny Wang, Tim Bruecker, Haley Booker, Alfred Swinton
--
-- NOTES:
-- This is the csharp class to start the server. It waits for a maximum of 30 players.
-- After 30 seconds of running the game will be initiated if a minimum of 2 players have joined.
-- The server keeps track of all players, bullets and weapons in the game. It sends the
-- information of the players inventory, health and the other players coordinates to each player.
---------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         Main()
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang
    --
    -- INTERFACE:        public static void Main()
    --
    -- RETURNS:          void
    --
    -- NOTES:
    -- The starting point of the server. Sets up the pregame and starts the game.
    -------------------------------------------------------------------------------------------------*/
    public static void Main()
    {
        Console.WriteLine("Starting server");
        mutex = new Mutex();

        pregame();

        startGame();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         pregame
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang
    --
    -- INTERFACE:        public static void pregame()
    --
    -- RETURNS:          void
    --
    -- NOTES:
    -- Creates everything needed before the game can start.
    -------------------------------------------------------------------------------------------------*/
    public static void pregame()
    {
        players = new Dictionary<byte, Player>();
        initTCPServer();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         startGame
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang, Haley Booker
    --
    -- INTERFACE:        public static void startGame()
    --
    -- RETURNS:         void
    --
    -- NOTES:
    -- Starts the threads for the game.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         isTick
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang
    --
    -- INTERFACE:        public static void isTick()
    --
    -- RETURNS:          Returns true if time has elapsed. Else it returns false
    --
    -- NOTES:
    -- Checks if a new tick has occurred. It’s used to update the send thread.
    -------------------------------------------------------------------------------------------------*/
    private static bool isTick()
    {
        if (DateTime.Now > nextTick)
        {
            nextTick = DateTime.Now.AddMilliseconds(R.Game.TICK_INTERVAL);
            return true;
        }
        return false;
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         gameThreadFunction
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang, Tim Bruecker, Haley Booker
    --
    -- INTERFACE:        private static void gameThreadFunction()
    --
    -- RETURNS:          void
    --
    -- NOTES:
    -- Updates the the players based on collisions and the danger zone. The systems handles
    -- players outside the danger zone, collisions between bullets and players and expired bullets.
    -------------------------------------------------------------------------------------------------*/
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
                            if (bullet.Value.IsColliding(player.Value.x, player.Value.z, R.Game.Players.RADIUS))
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         sendThreadFunction
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang, Tim Bruecker, Haley Booker
    --
    -- INTERFACE:        private static void sendThreadFunction()
    --
    -- RETURNS:          void
    --
    -- NOTES:
    -- Sends and packet to each connected player. The system sends each players
    -- health out after updating it.
    -------------------------------------------------------------------------------------------------*/
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


    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:         generateTickPacketHeader
    --
    -- DATE:             Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:         Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER:       Benny Wang
    --
    -- INTERFACE:        private static byte generateTickPacketHeader(bool hasPlayer, bool hasBullet, bool hasWeapon, int players)
    --                      bool hasPlayer: True if the packet is sending players
    --                      bool hasBullet: True if the packet is sending bullets
    --                      bool hasWeapon: True if the packet is sending weapons
    --                      int players: The number of players in the game
    --
    -- RETURNS:          The header byte generated
    --
    -- NOTES:
    -- Generates a byte for the header based on what it needs to send. The byte value will      
    -- depend on the number of players and whether the packet will have players, bullets and/or
    -- weapons.
    -------------------------------------------------------------------------------------------------*/
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


    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		updateHealthPacket
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker
    --
    -- PROGRAMMER: 	    Benny Wang, Tim Bruecker
    --
    -- INTERFACE:	 	private static void updateHealthPacket(Player player, byte[] snapshot)
    --				        Player player: The player object
    --				        byte[] snapshot: The byte array to be copied to
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Takes a players health value and copies it into a byte array. Used to update player’s health
    -------------------------------------------------------------------------------------------------*/
    private static void updateHealthPacket(Player player, byte[] snapshot)
    {
        int offset = R.Net.Offset.HEALTH;
        mutex.WaitOne();
        Array.Copy(BitConverter.GetBytes(player.h), 0, snapshot, offset, 1);
        mutex.ReleaseMutex();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		buildSendPacket
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:		Mar 27, 2018 - Refactored offsets for new packets
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER: 	    Benny Wang, Tim Bruecker, Haley Booker
    --
    -- INTERFACE:	 	private static void buildSendPacket()
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Builds the send packet with the players ids and coordinates. For any new bullets it adds
    -- them to the packet.The offset of the bullets is based on which player fired the bullet. If a
    -- player’s inventory has changed. The weapons on the map will be updated.
    -------------------------------------------------------------------------------------------------*/
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


    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		recvThreadFunction
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER: 	    Benny Wang, Tim Bruecker, Haley Booker
    --
    -- INTERFACE:	 	private static void recvThreadFunction()
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Receives all incoming data from all clients. Checks to confirm the amount of data
    -- is accurate.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		handleBuffer
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker
    --
    -- PROGRAMMER: 	    Benny Wang
    --
    -- INTERFACE:	 	private static void handleBuffer(byte[] inBuffer, EndPoint ep)
    --				        byte[] inBuffer: The buffer of recieved data
    --				        EndPoint ep: The end point of who sent the data
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Checks to see if the data recieved is from a new or existing client.
    -------------------------------------------------------------------------------------------------*/
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


    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		updateExistingPlayer
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER: 	    Benny Wang, Haley Booker
    --
    -- INTERFACE:	 	private static void updateExistingPlayer(ref byte[] inBuffer)
    --				        byte[] inBuffer: The buffer of recieved data
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Updates the coordinates of a player and handles bullets or weapons switching.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		handleIncomingBullet
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker
    --
    -- PROGRAMMER: 	    Benny Wang
    --
    -- INTERFACE:	 	private static void handleIncomingBullet(byte playerId, int bulletId, byte bulletType)
    --				        byte playerId: The id of the player
    --				        int bulletId: The id of the bullet
    --				        byte bulletType: The type of bullet
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Creates a new bullet and adds it to the bullet array.
    -------------------------------------------------------------------------------------------------*/
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

/*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		handleIncomingWeapon
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker
    --
    -- PROGRAMMER: 	    Benny Wang
    --
    -- INTERFACE:	 	private static void handleIncomingWeapon(byte playerId, int weaponId, byte weaponType)
    --				        byte playerId: The id of the player
    --				        int weaponId: The id of the weapon
    --				        byte weaponType: The type of weapon
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Handles a player picking up a weapon and updating the player’s inventory.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		addNewPlayer
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:		Mar 27, 2018 - Refactored offsets for new packets
    -- 				    Mar 30, 2018 - Implemented better spawn points
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER: 	    Benny Wang, Haley Booker
    --
    -- INTERFACE:	 	private static void addNewPlayer(EndPoint ep)
    --				        EndPoint ep: The end point of a new connection
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Creates a new player and adds it to the player array.
    -------------------------------------------------------------------------------------------------*/
    private static void addNewPlayer(EndPoint ep)
    {
        List<float> spawnPoint = spawnPointGenerator.GetNextSpawnPoint();
        Player newPlayer = new Player(ep, nextPlayerId, spawnPoint[0], spawnPoint[1]);

        mutex.WaitOne();
        nextPlayerId++;
        players[newPlayer.id] = newPlayer;
        mutex.ReleaseMutex();

        sendInitPacket(newPlayer);
    }


    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		sendInitPacket
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:		Mar 27, 2018 - Refactored offsets for new packets
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker, Haley Booker
    --
    -- PROGRAMMER: 	    Haley Booker
    --
    -- INTERFACE:	 	private static void sendInitPacket(Player newPlayer)
    --				        Player newPlayer: The new player to be sent
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Sends an initial packet to client on connection. The packet contains the client’s
    -- player id.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:    initTCPServer
    --
    -- DATE:        Mar. 28, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:    Benny Wang
    --
    -- PROGRAMMER:  Benny Wang
    --
    -- INTERFACE:   private static void initTCPServer()
    --
    -- RETURNS:     void
    --
    -- NOTES: 
    -- This function is called to initialize a TCPServer object which handles TCP connections.
    -- After creating the TCPServer object, it creates a thread which executes listenThreadFunc
    -- and joins on the thread's termination.
    -------------------------------------------------------------------------------------------------*/
    private static void initTCPServer()
    {
        tcpServer = new TCPServer();
        tcpServer.Init(R.Net.PORT, R.Net.TIMEOUT);
        listenThread = new Thread(listenThreadFunc);
        listenThread.Start();
        listenThread.Join();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		generateInitData
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:		Mar 27, 2018 - Refactored offsets for new packets
    --
    -- DESIGNER: 		Benny Wang, Tim Bruecker
    --
    -- PROGRAMMER: 	    Benny Wang
    --				    Roger Zhang
    -- 				    Alfred Swinton
    --
    -- INTERFACE:	 	private static void generateInitData()
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- This function generates the valid initialization for the terrain and weapons.
    -------------------------------------------------------------------------------------------------*/
    private static void generateInitData()
    {
        dangerZone = new DangerZone();

        InitRandomGuns getItems = new InitRandomGuns(R.Net.MAX_PLAYERS);
        itemData = getItems.compressedpcktarray;

        while (!tc.GenerateEncoding()) ;
        int terrainDataLength = tc.CompressedData.Length;
        Array.Copy(tc.CompressedData, 0, mapData, 0, terrainDataLength);
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:    listenThreadFunc
    --
    -- DATE:        Mar. 28, 2018
    --              
    -- REVISIONS:   Apr. 9, 2018
    --                  - Added timeout to listen loop, hard coded to 30s
    --              Apr. 11, 2018
    --                  - Modified timeout to accept a value passed down from
    --                    R.Net.TIMEOUT
    --
    -- DESIGNER:    Wilson Hu, Angus Lam, Benny Wang
    --
    -- PROGRAMMER:  Wilson Hu, Angus Lam, Benny Wang
    --
    -- INTERFACE:   private static void listenThreadFunc()
    --
    -- RETURNS:     void
    --
    -- NOTES: 
    -- This thread function performs a listen loop that continuously accepts up to 
    -- 30 clients. 
    -- 
    -- It calls the game generation function upon timing out or receiving the max
    -- number of clients. 
    -- 
    -- It then creates n threads to handle transmitting game initialization data to each
    -- connected client, and joining on each thread's termination.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION:    transmitThreadFunc
    --
    -- DATE:        Mar. 28, 2018
    --
    -- REVISIONS:
    --
    -- DESIGNER:    Wilson Hu, Angus Lam, Benny Wang
    --
    -- PROGRAMMER:  Angus Lam, Wilson Hu, Benny Wang
    --
    -- INTERFACE:   private static void transmitThreadFunc(object clientsockfd)
    --                  object clientsockfd: a socket descriptor value which is cast to an Int32
                                             within the function
    --
    -- RETURNS:     void
    --
    -- NOTES: 
    -- This thread function sends the game initialization data to its input client socket descriptor.
    -------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 		LogError
    --
    -- DATE: 			Feb 18, 2018
    --
    -- REVISIONS:		
    --
    -- DESIGNER: 		Benny Wang
    --
    -- PROGRAMMER: 	    Benny Wang
    --
    -- INTERFACE:	 	private static void LogError()
    --
    -- RETURNS: 		void
    --
    -- NOTES:
    -- Prints a message to the screen with the timestamp prepended to the message.
    -------------------------------------------------------------------------------------------------*/
    private static void LogError(String s)
    {
        Console.WriteLine(DateTime.Now + " - " + s);
    }
}
