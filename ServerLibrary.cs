using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Networking {

    internal static unsafe class ServerLibrary {

        [DllImport ("Network")]
        public static extern IntPtr Server_CreateServer ();

        [DllImport ("Network")]
        public static extern Int32 Server_sendBytes (IntPtr serverPtr, EndPoint ep, IntPtr buffer, UInt32 len);

        [DllImport ("Network")]
        public static extern Int32 Server_recvBytes (IntPtr serverPtr, EndPoint * ep, IntPtr buffer, UInt32 len);

        [DllImport ("Network")]
        public static extern Int32 Server_PollSocket (IntPtr serverPtr);

        [DllImport ("Network")]
        public static extern Int32 Server_SelectSocket (IntPtr serverPtr);

        [DllImport ("Network")]
        public static extern Int32 Server_initServer (IntPtr serverPtr, ushort port);

        [DllImport ("Network")]
        public static extern IntPtr Client_CreateClient ();

        [DllImport ("Network")]
        public static extern Int32 Client_sendBytes (IntPtr clientPtr, IntPtr buffer, UInt32 len);

        [DllImport ("Network")]
        public static extern Int32 Client_recvBytes (IntPtr clientPtr, IntPtr buffer, UInt32 len);

        [DllImport ("Network")]
        public static extern Int32 Client_PollSocket (IntPtr clientPtr);

        [DllImport("Network")]
        public static extern Int32 Client_SelectSocket(IntPtr clientPtr);

        [DllImport ("Network")]
        public static extern Int32 Client_initClient (IntPtr clientPtr, EndPoint ep);

        [DllImport("Network")]
        public static extern IntPtr TCPServer_CreateServer();

        [DllImport("Network")]
        public static extern Int32 TCPServer_initServer(IntPtr serverPtr, ushort port);

        [DllImport("Network")]
        public static extern Int32 TCPServer_acceptConnection(IntPtr serverPtr, EndPoint * ep);

        [DllImport("Network")]
        public static extern Int32 TCPServer_sendBytes(IntPtr serverPtr, Int32 clientSocket, IntPtr data, UInt32 len);

        [DllImport("Network")]
        public static extern Int32 TCPServer_recvBytes(IntPtr serverPtr, Int32 clientSocket, IntPtr data, UInt32 len);

        [DllImport("Network")]
        public static extern Int32 TCPServer_closeClientSocket(Int32 clientSocket);

        [DllImport("Network")]
        public static extern Int32 TCPServer_closeListenSocket(Int32 sockfd);

        [DllImport("Network")]
        public static extern IntPtr TCPClient_CreateClient();

        [DllImport("Network")]
        public static extern Int32 TCPClient_initClient(IntPtr serverPtr, EndPoint ep);

        [DllImport("Network")]
        public static extern Int32 TCPClient_sendBytes(IntPtr serverPtr, IntPtr data, UInt32 len);

        [DllImport("Network")]
        public static extern Int32 TCPClient_recvBytes(IntPtr serverPtr, IntPtr data, UInt32 len);

        [DllImport("Network")]
        public static extern Int32 TCPClient_closeConnection(Int32 sockfd);

    }

}
