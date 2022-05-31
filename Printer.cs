using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace maple
{
    public static class Printer
    {

        //source: https://stackoverflow.com/a/2754674/3785038
        //this answer was remarkably helpful in improving performance
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
        private static extern bool WriteConsoleOutputW(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion
        );

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord
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
        private struct CharUnion
        {
            [FieldOffset(0)] public ushort UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static SafeFileHandle consoleHandle;
        private static short bufWidth;
        private static short bufHeight;
        private static CharInfo[] buf;
        private static SmallRect rect;

        // https://www.pinvoke.net/default.aspx/kernel32.getstdhandle
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out uint lpMode
            );
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            uint dwMode
            );

        // [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ReadConsoleInputW")]
        [DllImport("Kernel32.DLL", EntryPoint = "ReadConsoleInputW", CallingConvention = CallingConvention.StdCall)]
        static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
            );

        struct WINDOW_BUFFER_SIZE_RECORD {
            public Coord dwSize;

            public WINDOW_BUFFER_SIZE_RECORD(short x, short y)
            {
                dwSize = new Coord(x,y);
            }
        }

        // https://www.codeproject.com/script/Content/ViewAssociatedFile.aspx?rzp=%2FKB%2Fdotnet%2FConsolePasswordInput%2FConsolePasswordInput_src.ZIP&zep=ConsolePasswordInput%2FConsolePasswordInput.cs&obid=8110&obtid=2&ovid=1
        [StructLayout(LayoutKind.Sequential, Pack=8)]        
        struct KEY_EVENT_RECORD
        {
            public int bKeyDown;
            public ushort wRepeatCount;
            public ushort wVirtualKeyCode;
            public ushort wVirtualScanCode;
            public CharUnion uchar;
            public uint dwControlKeyState;
        }

        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        enum VirtualKeyCode
        {
            VK_LBUTTON = 0x01, // left mouse
            VK_RBUTTON = 0x02, // right mouse
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04, // middle mouse
            VK_XBUTTON1 = 0x05, // x1 mouse
            VK_XBUTTON2 = 0x06, // x2 mouse
            VK_BACK = 0x08, // backspace
            VK_TAB = 0x09, // tab
            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D, // enter
            VK_SHIFT = 0x10, // shift
            VK_CONTROL = 0x11, // ctrl
            VK_MENU = 0x12, // alt
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14, // caps lock
            VK_KANA = 0x15,
            VK_HANGUEL = 0x15, // use VK_HANGUL
            VK_HANGUL = 0x15,
            VK_IME_ON = 0x16,
            VK_JUNJA = 0x17,
            VK_FINAL = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,
            VK_IME_OFF = 0x1A,
            VK_ESCAPE = 0x1B, // escape
            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,
            VK_SPACE = 0x20, // space
            VK_PRIOR = 0x21, // page up
            VK_NEXT = 0x22, // page down
            VK_END = 0x23, // end
            VK_HOME = 0x24, // home
            VK_LEFT = 0x25, // left arrow
            VK_UP = 0x26, // up arrow
            VK_RIGHT = 0x27, // right arrow
            VK_DOWN = 0x28, // down arrow
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C, // print screen
            VK_INSERT = 0x2D, // insert
            VK_DELETE = 0x2E, // delete
            VK_HELP = 0x2F, // help
            D0 = 0x30,
            D1 = 0x31,
            D2 = 0x32,
            D3 = 0x33,
            D4 = 0x34,
            D5 = 0x35,
            D6 = 0x36,
            D7 = 0x37,
            D8 = 0x38,
            D9 = 0x39,
            A = 0x41,
            B = 0x42,
            C = 0x43,
            D = 0x44,
            E = 0x45,
            F = 0x46,
            G = 0x47,
            H = 0x48,
            I = 0x49,
            J = 0x4A,
            K = 0x4B,
            L = 0x4C,
            M = 0x4D,
            N = 0x4E,
            O = 0x4F,
            P = 0x50,
            Q = 0x51,
            R = 0x52,
            S = 0x53,
            T = 0x54,
            U = 0x55,
            V = 0x56,
            W = 0x57,
            X = 0x58,
            Y = 0x59,
            Z = 0x5A,
            VK_LWIN = 0x5B, // left windows
            VK_RWIN = 0x5C, // right windows
            VK_APPS = 0x5D,
            VK_SLEEP = 0x5F,
            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A, // multiply
            VK_ADD = 0x6B, // add
            VK_SEPARATOR = 0x6C, // separator key (?)
            VK_SUBTRACT = 0x6D, // subtract
            VK_DECIMAL = 0x6E, // decimal
            VK_DIVIDE = 0x6F, // divide
            VK_F1 = 0x70, // F1
            VK_F2 = 0x71, // F2
            VK_F3 = 0x72, // F3
            VK_F4 = 0x73, // F4
            VK_F5 = 0x74, // F5
            VK_F6 = 0x75, // F6
            VK_F7 = 0x76, // F7
            VK_F8 = 0x77, // F8
            VK_F9 = 0x78, // F9
            VK_F10 = 0x79, // F10
            VK_F11 = 0x7A, // F11
            VK_F12 = 0x7B, // F12
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,
            VK_NUMLOCK = 0x90, // num lock
            VK_SCROLL = 0x91, // scroll lock
            VK_LSHIFT = 0xA0, // left shift
            VK_RSHIFT = 0xA1, // right shift
            VK_LCONTROL = 0xA2, // left control
            VK_RCONTROL = 0xA3, // right control
            VK_LMENU = 0xA4, // left menu (alt?)
            VK_RMENU = 0xA5, // right menu (alt?)
            VK_BROWSER_BACK = 0xA6
            // TODO: UNFINISHED
        }

        struct MOUSE_EVENT_RECORD
        {
            Coord dwMousePosition;
            uint dwButtonState;
            uint dwControlKeyState;
            uint dwEventFlags;
        }

        struct MENU_EVENT_RECORD
        {
            uint dwCommandId;
        }

        struct FOCUS_EVENT_RECORD
        {
            bool bSetFocus;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT_RECORD
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

        private const int STD_INPUT_HANDLE = -10;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int ENABLE_WINDOW_INPUT = 0x0008;
        private const int ENABLE_MOUSE_INPUT = 0x0010;

        private const int KEY_EVENT = 0x0001;
        private const int MOUSE_EVENT = 0x002;
        private const int WINDOW_BUFFER_SIZE_EVENT = 0x0004;
        private const int MENU_EVENT = 0x0008;
        private const int FOCUS_EVENT = 0x0010;

        // to keep track of input handler / mode
        private static IntPtr hStdin;
        private static uint fdwOldMode;

        // keep track of keyboard state
        private static bool shiftKeyDown = false;
        private static bool controlKeyDown = false;
        private static bool altKeyDown = false;

        private static Thread inputThread;
        private const int InputLoopDelay = 200;

        private static int keyEvents = 0;

        /// <summary>
        /// Create the Console handle and prepare the buffer for rendering.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // create handle to console buffer
                consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
    
                if (consoleHandle.IsInvalid)
                {
                    Log.Write("Failed to create console handle", "printer", important: true);
                    PrintLineSimple("Printer failed to create console handle", Styler.ErrorColor);
                    Console.ResetColor();
                    Environment.Exit(1); // kinda temporary
                    return;
                }
    
                Log.Write("Successfully created console handle", "printer");
    
                bufWidth = (short)Console.WindowWidth;
                bufHeight = (short)Console.WindowHeight;
    
                buf = new CharInfo[bufWidth * bufHeight];
                rect = new SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
            
                // set signal handler
                SetConsoleCtrlHandler(SigHandler, true);

                // create resize handler
                // https://docs.microsoft.com/en-us/windows/console/reading-input-buffer-events
                hStdin = GetStdHandle(STD_INPUT_HANDLE);
                uint fdwMode;

                Log.WriteDebug("Got hStdin (=" + hStdin + ")", "printer");

                // if (hStdin == INVALID_HANDLE_VALUE)
                // {
                //     Log.Write("Failed to create input handle", "printer", important: true);
                // }

                if (!GetConsoleMode(hStdin, out fdwOldMode))
                {
                    Log.Write("Failed to GetConsoleMode", "printer", important: true);
                }

                fdwMode = ENABLE_WINDOW_INPUT | ENABLE_MOUSE_INPUT;
                if (!SetConsoleMode(hStdin, fdwMode))
                {
                    Log.Write("Failed to SetConsoleMode", "printer", important: true);
                }

                // begin input listener thread
                Log.Write("Creating Win32 console input listener", "printer");
                inputThread = new Thread(new ThreadStart(ConsoleInputListener));
            }
            catch (DllNotFoundException e)
            {
                PrintLineSimple("Printer failed to load kernel32.dll - is maple running on a non-Windows platform?", Styler.ErrorColor);
                Log.Write("Encountered DLLNotFoundException when initializing printer: " + e.Message, "printer", important: true);
                Log.Write("Platform: " + Environment.OSVersion, "printer");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        public static void StartInputThread()
        {
            inputThread.Start();
        }

        public static void ConsoleInputListener()
        {
            Log.Write("Hello from the console input listener thread!", "printer");

            while (true)
            {
                INPUT_RECORD[] irInBuf = new INPUT_RECORD[128];
                uint cNumRead = 0;

                if (!ReadConsoleInput(
                    hStdin,
                    irInBuf,
                    128,
                    out cNumRead
                ))
                {
                    Log.Write("Printer input handler failed ReadConsoleInput", "printer", important: true);
                    break;
                }

                for (int i = 0; i < cNumRead; i++)
                {
                    if (irInBuf[i].EventType == KEY_EVENT)
                    {
                        KeyEventProc(irInBuf[i].KeyEvent);
                    }
                    if (irInBuf[i].EventType == WINDOW_BUFFER_SIZE_EVENT)
                    {
                        ResizeEventProc(irInBuf[i].WindowBufferSizeEvent);
                    }
                }

                // delay loop
                Thread.Sleep(InputLoopDelay);
            }
        }

        public static void RestorePreviousConsoleMode()
        {
            SetConsoleMode(hStdin, fdwOldMode);
        }

        private static void KeyEventProc(KEY_EVENT_RECORD record)
        {
            // ignore initial enter keypress
            if (keyEvents == 0)
            {
                if ((ConsoleKey)record.wVirtualKeyCode == ConsoleKey.Enter)
                {
                    keyEvents++;
                    return;
                }
                keyEvents++;
            }

            // update state of modifier keys
            switch (record.wVirtualKeyCode)
            {
                case 16:
                    shiftKeyDown = !shiftKeyDown;
                    break;
                case 17:
                    controlKeyDown = !controlKeyDown;
                    break;
                case 18:
                    altKeyDown = !altKeyDown;
                    break;
            }
            ConsoleKey key = (ConsoleKey)record.wVirtualKeyCode;
            char keyChar = (char)record.uchar.UnicodeChar;
            Log.WriteDebug("Key press: " + ((ConsoleKey)record.wVirtualKeyCode).ToString() + " = " + (char)record.uchar.UnicodeChar + " (" + record.wRepeatCount + ")", "printer");

            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo(
                keyChar: keyChar,
                key: key,
                shift: shiftKeyDown,
                alt: altKeyDown,
                control: controlKeyDown
                );
            Input.AcceptInput(keyInfo);
        }

        private static void ResizeEventProc(WINDOW_BUFFER_SIZE_RECORD record)
        {
            Log.WriteDebug("Window buffer size event", "printer");
        }

        public static int MinScreenX = 0;
        public static int MinScreenY = 0;
        private static int _oldMaxScreenX = 0;
        public static int MaxScreenX
        {
            get
            {
                int newMaxScreenX = Console.WindowWidth - 1;
                if (newMaxScreenX != _oldMaxScreenX)
                    Printer.Resize();
                _oldMaxScreenX = newMaxScreenX;
                return newMaxScreenX;
            }
        }

        private static int _oldMaxScreenY = 0;
        public static int MaxScreenY
        {
            get
            {
                Log.WriteDebug("WindowHeight: " + Console.WindowHeight, "printer");
                return Console.WindowHeight - 1;
            }
        }

        static Cursor printerCursor = new Cursor(0, 0);
        public static int CursorSX { get { return printerCursor.SX; } }
        public static int CursorSY { get { return printerCursor.SY; } }

        /// <summary>
        /// Get the 1 dimensional buffer index given 2 dimensional coordinates.
        /// </summary>
        /// <param name="x">The X coordinate (screen).</param>
        /// <param name="y">The Y coordinate (screen).</param>
        /// <returns>The buffer index.</returns>
        private static int GetBufferIndex(int x, int y)
        {
            return y * bufWidth + x;
        }

        /// <summary>
        /// Get the 1 dimensional buffer index from a Cursor's screen position.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        /// <returns>The buffer index.</returns>
        private static int GetBufferIndex(Cursor cursor)
        {
            return GetBufferIndex(cursor.SX, cursor.SY);
        }

        /// <summary>
        /// Generate a buffer attribute value from a given set of ConsoleColors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <returns>A buffer attribute representing the given colors.</returns>
        public static short GetAttributeFromColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            return (short)((short)(GetAttributeFromColor(backgroundColor) << 4) | GetAttributeFromColor(foregroundColor));
        }

        /// <summary>
        /// Generate a buffer attribute value representing the given color. The attribute represents the color as the foreground color with a black background.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>A buffer attrivute representing the color.</returns>
        public static short GetAttributeFromColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return 0x0000;
                case ConsoleColor.DarkBlue:
                    return 0x0001;
                case ConsoleColor.Blue:
                    return 0x0009;
                case ConsoleColor.DarkGreen:
                    return 0x0002;
                case ConsoleColor.Green:
                    return 0x000A;
                case ConsoleColor.DarkCyan:
                    return 0x0003;
                case ConsoleColor.Cyan:
                    return 0x000B;
                case ConsoleColor.DarkRed:
                    return 0x0004;
                case ConsoleColor.Red:
                    return 0x000C;
                case ConsoleColor.DarkMagenta:
                    return 0x0005;
                case ConsoleColor.Magenta:
                    return 0x000D;
                case ConsoleColor.DarkYellow:
                    return 0x0006;
                case ConsoleColor.Yellow:
                    return 0x000E;
                case ConsoleColor.Gray:
                    return 0x0007;
                case ConsoleColor.DarkGray:
                    return 0x0008;
                case ConsoleColor.White:
                    return 0x000F;
            }
            return 0x0000;
        }

        /// <summary>
        /// Get the buffer attribute at a given position on the buffer.
        /// </summary>
        /// <param name="x">The X coordinate (screen).</param>
        /// <param name="y">The Y coordinate (screen).</param>
        /// <returns>The buffer attribute.</returns>
        public static short GetAttributeAtPosition(int x, int y)
        {
            return buf[GetBufferIndex(x, y)].Attributes;
        }

        /// <summary>
        /// Render the current buffer contents to the screen. The more frequently this function is called, the less smooth the rendering will appear.
        /// </summary>
        public static void ApplyBuffer()
        {
            bool b = WriteConsoleOutputW(consoleHandle,
                buf,
                new Coord() { X = bufWidth, Y = bufHeight },
                new Coord() { X = 0, Y = 0 },
                ref rect
                );
        }

        /// <summary>
        /// Recreate the buffer array and bounds according to the current Console dimensions.
        /// </summary>
        public static void Resize()
        {
            bufWidth = (short)Console.WindowWidth;
            bufHeight = (short)Console.WindowHeight;

            buf = new CharInfo[bufWidth * bufHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
        }

        /// <summary>
        /// Print a string to the buffer in the given colors.
        /// </summary>
        /// <param name="word">The string to print.</param>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="backgroundColor">The background color.</param>
        public static void PrintWord(string word, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            PrintWord(word, GetAttributeFromColor(foregroundColor, backgroundColor));
        }

        /// <summary>
        /// Print a string to the buffer and apply the given attribute to each character.
        /// </summary>
        /// <param name="word">The string to print.</param>
        /// <param name="attribute">The attribute to apply to the entire string.</param>
        public static void PrintWord(string word, short attribute)
        {
            int index = GetBufferIndex(printerCursor);

            char[] wordChars = word.ToCharArray();

            for (int i = index; i < index + word.Length; i++)
            {
                buf[i].Char.UnicodeChar = wordChars[i - index];
                buf[i].Attributes = attribute;
                printerCursor.SX++;
            }

            // printerCursor.SX += word.Length;
        }

        /// <summary>
        /// Manually write character and attribute data to the buffer.
        /// </summary>
        /// <param name="c">The character to write.</param>
        /// <param name="sX">The X coordinate (screen).</param>
        /// <param name="sY">The Y coordinate (screen).</param>
        /// <param name="attribute">The buffer attribute info.</param>
        public static void PrintManually(char c, int sX, int sY, short attribute)
        {
            int index = GetBufferIndex(sX, sY);
            buf[index].Char.UnicodeChar = c;
            buf[index].Attributes = attribute;
        }

        /// <summary>
        /// Clear the entire buffer and render it.
        /// </summary>
        public static void Clear()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = 0x0000;
            }

            ApplyBuffer();
        }

        /// <summary>
        /// Move the internal printer cursor to the given screen coordinates.
        /// </summary>
        /// <param name="x">The X coordinate (screen).</param>
        /// <param name="y">The Y coordinate (screen).</param>
        public static void MoveCursor(int x, int y)
        {
            printerCursor.Move(x, y, applyPosition: false);
        }

        /// <summary>
        /// Print a line of colored text to the Console. Not for use within the Editor.
        /// </summary>
        /// <param name="text">The text to print.</param>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="backgroundColor">The background color.</param>
        public static void PrintLineSimple(String text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
        }

        /// <summary>
        /// Clear the footer and write colored text to it.
        /// </summary>
        /// <param name="content">The text to write.</param>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="backgroundColor">The background color.</param>
        public static void DrawFooter(String content, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black, int yOffset = 0)
        {
            ClearLine(MaxScreenY - yOffset, backgroundColor);
            WriteToFooter(content, 0, foregroundColor, backgroundColor, yOffset);
        }

        /// <summary>
        /// Write to the Editor's footer region (bottom line of the Console).
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="x">The </param>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        public static void WriteToFooter(String text, int x = -1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black, int yOffset = 0)
        {
            WriteToFooter(text, x, GetAttributeFromColor(foregroundColor, backgroundColor), yOffset);
        }

        public static void WriteToFooter(String text, int x = -1, short attribute = 0x0007, int yOffset = 0)
        {
            if (x != -1)
            {
                printerCursor.Move(x, MaxScreenY - yOffset, applyPosition: false);
            }

            int index = GetBufferIndex(printerCursor);

            char[] textChars = text.ToCharArray();

            for (int i = index; i < index + text.Length; i++)
            {
                if (i > buf.Length) //complete overflow
                {
                    buf[buf.Length - 1].Char.UnicodeChar = Settings.OverflowIndicator;
                    buf[buf.Length - 1].Attributes = attribute;
                    break;
                }
                if (i == buf.Length || (i > index && i % bufWidth == 0)) //line overflow
                {
                    buf[i - 1].Char.UnicodeChar = Settings.OverflowIndicator;
                    buf[i - 1].Attributes = attribute;
                    break;
                }

                buf[i].Char.UnicodeChar = textChars[i - index];
                buf[i].Attributes = attribute;
            }

            printerCursor.SX += text.Length;
        }

        /// <summary>
        /// Clear the given line (screen coordinates).
        /// </summary>
        /// <param name="line">The line index.</param>
        public static void ClearLine(int line, ConsoleColor clearColor = ConsoleColor.Black)
        {
            //don't clear if out of range
            if(line < 0 || line > MaxScreenY)
                return;
            
            int startIndex = GetBufferIndex(0, line);
            
            short clearAttributes = GetAttributeFromColor(ConsoleColor.Black, clearColor);            
            for (int i = startIndex; i < startIndex + bufWidth; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = clearAttributes;
            }
        }

        /// <summary>
        /// Clear the line to the right of the internal printer Cursor.
        /// </summary>
        public static void ClearRight()
        {
            //don't clear if out of range
            if (printerCursor.SY < 0 || printerCursor.SY > MaxScreenY)
                return;

            int startIndex = GetBufferIndex(printerCursor.SX, printerCursor.SY);
            int width = MaxScreenX - printerCursor.SX;
            for (int i = startIndex; i <= startIndex + width; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = 0x0000;
            }
        }
        
        private static bool SigHandler(CtrlType sig)
        {
            //TODO: this may be implemented further later
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                    return false;
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    return false;
                default:
                    return false;
            }
        }
    }
}
