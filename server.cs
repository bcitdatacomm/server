using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;


class Server
{
    private const double tickInterval = (double)1000 / (double)128;
    private static DateTime nextTick = DateTime.Now;
    private static Thread sendThread;
    private static Thread recvThread;
    private static bool running;
    private static Mutex mutex;


    private static DotNet.Server server;

    private static byte[] sendBuffer = new byte[R.Net.Size.SERVER_TICK];

    static byte nextPlayerId = 1;
    static Dictionary<byte, connectionData> players;

    static float spawnPoint = 1.0f;

    public static void Main()
    {
        Console.WriteLine("Starting server");

        pregame();

        startGame();
    }

    public static void pregame()
    {
        players = new Dictionary<byte, connectionData>();

    }

    public static void startGame()
    {
        server = new DotNet.Server();
        server.Init(R.Net.PORT);

        mutex = new Mutex();

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
                Buffer.BlockCopy(sendBuffer, 0, snapshot, 0, sendBuffer.Length)

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

            offset += 14;
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
                    DotNet.EndPoint ep = new DotNet.EndPoint();
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

    private static void handleBuffer(byte[] inBuffer, DotNet.EndPoint ep)
    {
        switch (inBuffer[0])
        {
            case R.Net.Header.TICK:
                updateExistingPlayer(ref inBuffer);
                break;

            default:
                LogError("Server received a valid amount of data but the header is incorrect.");
                break;
        }
    }

    private static void addNewPlayer(DotNet.EndPoint ep)
    {
        mutex.WaitOne();
        players[newPlayer.id] = new connectionData(ep, nextPlayerId++, spawnPoint * 5, spawnPoint * 5);
        spawnPoint++;
        mutex.ReleaseMutex();
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

    private static void sendInitPacket(byte id, float x, float z)
    {
        byte[] buffer = new byte[R.Net.Size.SERVER_TICK];

        buffer[0] = R.Net.Header.INIT_PLAYER;
        buffer[1] = id;
        int offset = 2;

        // sets the coordinates for the new player
        Array.Copy(BitConverter.GetBytes(x), 0, buffer, offset, 4);
        Array.Copy(BitConverter.GetBytes(z), 0, buffer, offset + 4, 4);

        mutex.WaitOne();
        DotNet.EndPoint ep = players[id].ep;
        mutex.ReleaseMutex();

        server.Send(ep, buffer, buffer.Length);

        mutex.WaitOne();

        // Find the offset to add the player to sendBuffer
        offset = R.Net.Offset.PLAYERS;
        while (sendBuffer[offset] != 0)
        {
            offset += R.Net.Size.PLAYER_DATA;
        }

        // Sets the player ID
        sendBuffer[offset] = id;
        offset++;

        // sets the coordinates for each player connected
        Array.Copy(BitConverter.GetBytes(x), 0, sendBuffer, offset, 4);
        Array.Copy(BitConverter.GetBytes(z), 0, sendBuffer, offset + 4, 4);

        mutex.ReleaseMutex();
    }

    private static void LogError(string s)
    {
        Console.WriteLine(DateTime.Now + " - " + s);
    }
}
