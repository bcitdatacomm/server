/*---------------------------------------------------------------------------------------
--	SOURCE FILE:	EndPoint.cs -   A C# struct for C++/C# interoperability
--
--	PROGRAM:		game
--
--	CLASSES:        EndPoint
--                  CAddr
--                  EndpointComparer
--
--	DATE:			February 25th, 2018
--
--	REVISIONS:		March 15th, 2018:
--                      Added convenience methods for comparison (Delan Elliot)
--                  April 11th, 2018:
--                      Fixed compile warning
--
--	DESIGNERS:		Delan Elliot
--
--	PROGRAMMER:		Delan Elliot
--
--	NOTES:
--		EndPoint and CAddr are C# structs that are explicitly packed in oder to preserve a
--      predicatble byte order. This way we can populate the binary data in C++ and still 
--      be able to read the data in C#. It also provides a number of convenience methods 
--      for comparison and stringificaion. 
---------------------------------------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Client
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CAddr{

        public static readonly CAddr Loopback = new CAddr("127.0.0.1");

        [FieldOffset(0)]
        public readonly uint Packet;
        [FieldOffset(0)]
        public readonly byte Byte0;
        [FieldOffset(1)]
        public readonly byte Byte1;
        [FieldOffset(2)]
        public readonly byte Byte2;
        [FieldOffset(3)]
        public readonly byte Byte3;


        public CAddr (string ip) {
            string[] parts = ip.Split('.');
            Packet = 0; 
            Byte0 = byte.Parse(parts[3]);
            Byte1 = byte.Parse(parts[2]);
            Byte2 = byte.Parse(parts[1]);
            Byte3 = byte.Parse(parts[0]);
        }

        public CAddr (byte a, byte b, byte c, byte d) {
            Packet = 0;
            Byte0 = d;
            Byte1 = c;
            Byte2 = b;
            Byte3 = a;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EndPoint {
        public readonly CAddr addr;
        public readonly ushort port;


        public EndPoint(string ipAddr, ushort p)
        {
            addr = new CAddr (ipAddr);
            port = p;
        }

        public override int GetHashCode ()
        {
            return (int) (addr.Packet ^ port);
        }

        public override string ToString ()
        {
            return string.Format ("{0}.{1}.{2}.{3}:{4}", addr.Byte3, addr.Byte2, addr.Byte1, addr.Byte0, port);
        }

        public static bool operator == (EndPoint x, EndPoint y)
        {
            return Compare(x, y) == 0;
        }

        public override bool Equals (object x)
        {
            if(x is EndPoint)
            {
                return Compare(this, (EndPoint) x) == 0;            
            }
            else
            {
                return false;
            }
        }
        

        public static bool operator != (EndPoint x, EndPoint y)
        {
            return Compare(x, y) != 0;
        }

        internal static int Compare (EndPoint x, EndPoint y)
        {
            if (x.addr.Packet > y.addr.Packet) return 1;
            if (x.addr.Packet < y.addr.Packet) return -1;

            if (x.port > y.port) return 1;
            if (x.port < y.port) return -1;

            return 0;
        }
    }

    public class EndPointComparer : IEqualityComparer<EndPoint> {
        bool IEqualityComparer<EndPoint>.Equals (EndPoint x, EndPoint y) {
            return EndPoint.Compare(x, y) == 0;
        }

        int IEqualityComparer<EndPoint>.GetHashCode (EndPoint obj) {
            return obj.GetHashCode();
        }
    }


}

