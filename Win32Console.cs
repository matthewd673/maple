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

        // INPUT UTILS
        // https://www.pinvoke.net/default.aspx/kernel32.getstdhandle
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out uint lpMode
            );
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            uint dwMode
            );

        // [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ReadConsoleInputW")]
        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CallingConvention = CallingConvention.StdCall)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
            );

        public struct WINDOW_BUFFER_SIZE_RECORD {
            public Win32Console.Coord dwSize;

            public WINDOW_BUFFER_SIZE_RECORD(short x, short y)
            {
                dwSize = new Win32Console.Coord(x,y);
            }
        }

        // https://www.codeproject.com/script/Content/ViewAssociatedFile.aspx?rzp=%2FKB%2Fdotnet%2FConsolePasswordInput%2FConsolePasswordInput_src.ZIP&zep=ConsolePasswordInput%2FConsolePasswordInput.cs&obid=8110&obtid=2&ovid=1
        [StructLayout(LayoutKind.Sequential, Pack=8)]        
        public struct KEY_EVENT_RECORD
        {
            public int bKeyDown;
            public ushort wRepeatCount;
            public ushort wVirtualKeyCode;
            public ushort wVirtualScanCode;
            public Win32Console.CharUnion uchar;
            public uint dwControlKeyState;
        }

        public struct MOUSE_EVENT_RECORD
        {
            Win32Console.Coord dwMousePosition;
            uint dwButtonState;
            uint dwControlKeyState;
            uint dwEventFlags;
        }

        public struct MENU_EVENT_RECORD
        {
            uint dwCommandId;
        }

        public struct FOCUS_EVENT_RECORD
        {
            bool bSetFocus;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD
        {
            [FieldOffset(0)]
            public ushort EventType;
            [FieldOffset(4)]
            public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(4)]
            public MOUSE_EVENT_RECORD MouseEvent;
            [FieldOffset(4)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
            [FieldOffset(4)]
            public MENU_EVENT_RECORD MenuEvent;
            [FieldOffset(4)]
            public FOCUS_EVENT_RECORD FocusEvent;
        };

        // CONSTS
        private const int STD_INPUT_HANDLE = -10;
        private const int ENABLE_WINDOW_INPUT = 0x0008;
        private const int ENABLE_MOUSE_INPUT = 0x0010;
        public const int KEY_EVENT = 0x0001;
        public const int MOUSE_EVENT = 0x002;
        public const int WINDOW_BUFFER_SIZE_EVENT = 0x0004;
        public const int MENU_EVENT = 0x0008;
        public const int FOCUS_EVENT = 0x0010;

        private static uint fdwOldMode;
        public static uint InputOldMode { get { return fdwOldMode; } }

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

        public static IntPtr GetInputHandle()
        {
            IntPtr hStdin = GetStdHandle(STD_INPUT_HANDLE);
            uint fdwMode;

            Log.WriteDebug("Got hStdin (=" + hStdin + ")", "printer");
            
            if (!GetConsoleMode(hStdin, out fdwOldMode))
            {
                Log.Write("GetInputHandle() failed to GetConsoleMode()", "win32console", important: true);
            }

            fdwMode = ENABLE_WINDOW_INPUT | ENABLE_MOUSE_INPUT;
            if (!SetConsoleMode(hStdin, fdwMode))
            {
                Log.Write("GetInputHandle() failed to SetConsoleMode()", "win32console", important: true);
            }

            return hStdin;
        }
    }
}