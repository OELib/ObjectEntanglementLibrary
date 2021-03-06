﻿using System.Runtime.InteropServices;

namespace OELib.LibraryBase.Messages
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageHeader
    {
        public int Length;
        public bool DataIsCompressed;

        public static int HeaderSize => Marshal.SizeOf(typeof(MessageHeader));
    }
}