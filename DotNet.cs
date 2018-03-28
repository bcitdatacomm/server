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