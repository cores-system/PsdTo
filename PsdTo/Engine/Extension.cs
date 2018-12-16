using System;
using System.IO;
using System.Net;

namespace PsdTo
{
    public static class Extension
    {
        static public UInt16 ReadUInt16BE(this BinaryReader br)
        {
            return (UInt16)IPAddress.NetworkToHostOrder(br.ReadInt16());
        }
        static public UInt32 ReadUInt32BE(this BinaryReader br)
        {
            return (UInt32)IPAddress.NetworkToHostOrder(br.ReadInt32());
        }
    }
}
