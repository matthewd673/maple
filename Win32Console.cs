using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace maple
{
    public static class Win32Console
    {
        // DRAWING UTILS

        // source: https://stackoverflow.com/a/2754674/3785038
        // this answer was remarkably helpful in improving performance
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutputW(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short x, short y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public ushort UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        public static SafeFileHandle CreateConsoleHandle()
        {
            try
            {
                SafeFileHandle consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

                if (consoleHandle.IsInvalid)
                {
                    Log.Write("Failed to create console handle", "win32console", important: true);
                    Printer.PrintLineSimple("Failed to create console handle", Settings.Theme.ErrorColor);
                    
                    // get me out
                    Console.ResetColor();
                    Environment.Exit(1);
                    return null;
                }

                Log.Write("Successfully created console handle", "win32console");

                return consoleHandle;
            }
            catch (DllNotFoundException e)
            {
                Printer.PrintLineSimple("Failed to load kernel32.dll - is maple running on a non-Win32 platform?", Settings.Theme.ErrorColor);
                Log.Write("Encountered DLLNotFoundException when initializing console handle: " + e.Message, "win32console", important: true);
                Log.Write("Platform: " + Environment.OSVersion, "win32console");
                Console.ResetColor();
                Environment.Exit(1);
            }

            return null;
        }
    }
}