using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DotNet
{
    /**
     * This is a class that encapsulates a network endpoint.
     */
    class EndPoint
    {
        public IPEndPoint IPEndPoint { get; set; }

        public EndPoint()
        {
            this.IPEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 42069);
        }

        /**
         * Constructor.
         * 
         * string ip:   The ip in dotted decimal format
         * ushort port: The port number.
         */
        public EndPoint(string ip, ushort port)
        {
            this.IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }
    }

    /**
     * The server networking class. This class should be used when you are the 1 in 1..*.
     */
    class Server
    {
        public bool IsLAN { get; set; }
        public string LastError { get; set; }

        private Socket socket;

        public Server()
        {
            this.IsLAN = false;
        } 

        ~Server()
        {
            if (this.socket != null)
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
            }
        }

        /**
         * Called when you want to connect to a port
         * 
         * ushort port: The port number.
         * 
         * Return:  True of the socket was initialized, false otherwise.
         */
        public bool Init(ushort port)
        {
            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
                return true;
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                this.socket = null;
                return false;
            }
        }


        /**
         * Checks if the port has data to be read.
         * 
         * Return:  Will return false if there is no data to read, otherwise, returns true.
         * If true is returned, a Recv() call needs to be made to get the data.
         */
        public bool Poll()
        {
            try
            {
                return this.socket.Poll(1, SelectMode.SelectRead);
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return false;
            }
        }

        /**
         * Reads data from the socket.
         * 
         * EndPoint ep:     The endpoint to read from.
         * byte[] buffer:   The buffer to fill with read bytes. 
         * Int32 len:       The length of the buffer.
         * 
         * Return:  The number of bytes read.
         */
        public Int32 Recv(ref EndPoint ep, byte[] buffer, Int32 len)
        {
            try
            {
                System.Net.EndPoint tmp = (System.Net.EndPoint)ep.IPEndPoint;
                int result = this.socket.ReceiveFrom(buffer, len, SocketFlags.None, ref tmp);
                ep.IPEndPoint = (System.Net.IPEndPoint)tmp;
                return result;
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return 0;
            }
            
        }

        /**
         * Writes data to the socket.
         * 
         * EndPoint ep:     The endpoint to write to.
         * byte[] buffer:   The buffer to write.
         * Int32 len:       The length of the buffer.
         * 
         * Return:  The number of bytes sent.
         */
        public Int32 Send(EndPoint ep, byte[] buffer, Int32 len)
        {
            try
            {
                if (this.IsLAN)
                {
                    return this.socket.SendTo(buffer, len, SocketFlags.Broadcast, ep.IPEndPoint);
                }
                else
                {
                    return this.socket.SendTo(buffer, len, SocketFlags.None, ep.IPEndPoint);
                }
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return 0;
            }
        }

    }


    /**
     * The server networking class. This class should be used when you are the many in 1..*.
     */
    class Client
    {
        public string LastError { get; set; }

        private Socket socket;
        private EndPoint remoteEndPoint;

        public Client() { }

        ~Client()
        {
            if (this.socket != null)
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
            }
        }

        /**
         * Called when you want to connect to a port
         * 
         * string ip: The ip of the server in dotted decimal form.
         * ushort port: The port number of the server.
         * 
         * Return:  True of the socket was initialized, false otherwise.
         */
        public bool Init(string ip, ushort port)
        {
            try
            {
                this.remoteEndPoint = new EndPoint(ip, port);
                this.socket = new Socket(this.remoteEndPoint.IPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                this.socket.Connect(this.remoteEndPoint.IPEndPoint);
                return true;
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                this.socket = null;
                return false;
            }
        }

        /**
         * Checks if the port has data to be read.
         * 
         * Return:  Will return false if there is no data to read, otherwise, returns true.
         * If true is returned, a Recv() call needs to be made to get the data.
         */
        public bool Poll()
        {
            try
            {
                return this.socket.Poll(1, SelectMode.SelectRead);
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return false;
            }
        }

        /**
         * Reads data from the socket.
         * 
         * byte[] buffer:   The buffer to fill with read bytes. 
         * Int32 len:       The length of the buffer.
         * 
         * Return:  The number of bytes read.
         */
        public Int32 Recv(byte[] buffer, Int32 len)
        {
            try
            {
                return this.socket.Receive(buffer, len, SocketFlags.None);
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return 0;
            }
        }

        /**
         * Writes data to the socket.
         * 
         * byte[] buffer:   The buffer to write.
         * Int32 len:       The length of the buffer.
         * 
         * Return:  The number of bytes sent.
         */
        public Int32 Send(Byte[] buffer, Int32 len)
        {
            try
            {
                return this.socket.Send(buffer, len, SocketFlags.None);
            }
            catch (Exception e)
            {
                this.LastError = e.Message;
                return 0;
            }
        }
    }
}


class Server
{
    private const double tickInterval = (double)1000 / (double)64;
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

        players = new Dictionary<byte, connectionData>();
        
        initNetworking();
        startThreads();
    }

    public static void initNetworking()
    {
        server = new DotNet.Server();
        server.Init(R.Net.PORT);
    }

    public static void startThreads()
    {
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

                mutex.WaitOne();
                byte[] snapshot = sendBuffer;
                foreach (KeyValuePair<byte, connectionData> pair in players)
                {
                    server.Send(pair.Value.ep, snapshot, snapshot.Length);
                }
                mutex.ReleaseMutex();
            }
        }

        return;
    }

    private static void buildSendPacket()
    {
        mutex.WaitOne();
        foreach (KeyValuePair<byte, connectionData> pair in players)
        {
            byte id = pair.Key;
            connectionData player = pair.Value;

            int positionOffset = (R.Net.Offset.PLAYER_POSITIONS + (id * 8)) - 8;
            int rotationOffset = (R.Net.Offset.PLAYER_ROTATIONS + (id * 4)) - 4;

            sendBuffer[R.Net.Offset.PLAYER_IDS + id - 1] = id;

            Array.Copy(BitConverter.GetBytes(player.x), 0, sendBuffer, positionOffset, 4);
            Array.Copy(BitConverter.GetBytes(player.z), 0, sendBuffer, positionOffset + 4, 4);
            Array.Copy(BitConverter.GetBytes(player.r), 0, sendBuffer, rotationOffset, 4);
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
            case R.Net.Header.NEW_CLIENT:
                addNewPlayer(ref inBuffer, ep);
                break;

            case R.Net.Header.TICK:
                updateExistingPlayer(ref inBuffer);
                break;

            default:
                LogError("Server received a valid amount of data but the header is incorrect.");
                break;
        }
    }

    private static void addNewPlayer(ref byte[] inBuffer, DotNet.EndPoint ep)
    {
        connectionData newPlayer = new connectionData();
        newPlayer.id = nextPlayerId;
        nextPlayerId++;

        newPlayer.x = spawnPoint * 5;
        newPlayer.z = spawnPoint * 5;
        newPlayer.r = 0;
        newPlayer.h = 100;
        newPlayer.ep = ep;

        spawnPoint++;

        mutex.WaitOne();
        players[newPlayer.id] = newPlayer;
        mutex.ReleaseMutex();

        sendInitPacket(newPlayer.id, newPlayer.x, newPlayer.z);
    }

    private static void updateExistingPlayer(ref byte[] inBuffer)
    {
        byte id = inBuffer[R.Net.Offset.PID];
        float x = BitConverter.ToSingle(inBuffer, R.Net.Offset.X);
        float z = BitConverter.ToSingle(inBuffer, R.Net.Offset.Z);
        float r = BitConverter.ToSingle(inBuffer, R.Net.Offset.R);

        int positionOffset = (R.Net.Offset.PLAYER_POSITIONS + (id * 8)) - 8;
        int rotationOffset = (R.Net.Offset.PLAYER_ROTATIONS + (id * 4)) - 4;

        mutex.WaitOne();
        players[id].x = x;
        players[id].z = z;
        players[id].r = r;
        Array.Copy(inBuffer, R.Net.Offset.X, sendBuffer, positionOffset, 4);
        Array.Copy(inBuffer, R.Net.Offset.Z, sendBuffer, positionOffset + 4, 4);
        Array.Copy(inBuffer, R.Net.Offset.R, sendBuffer, rotationOffset, 4);
        mutex.ReleaseMutex();
    }

    private static void sendInitPacket(byte id, float x, float z)
    {
        byte[] buffer = new byte[R.Net.Size.SERVER_TICK];

        int positionOffset = (R.Net.Offset.PLAYER_POSITIONS + (id * 8)) - 8;

        buffer[0] = 0;
        buffer[R.Net.Offset.PLAYER_IDS] = id;
        Array.Copy(BitConverter.GetBytes(x), 0, buffer, positionOffset, 4);
        Array.Copy(BitConverter.GetBytes(z), 0, buffer, positionOffset + 4, 4);

        mutex.WaitOne();
        DotNet.EndPoint ep = players[id].ep;
        mutex.ReleaseMutex();

        server.Send(ep, buffer, buffer.Length);
    }

    private static void LogError(string s)
    {
        Console.WriteLine(DateTime.Now + " - " + s);
    }
}
