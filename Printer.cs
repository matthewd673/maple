using System;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

namespace maple
{
    public static class Printer
    {

        //source: https://stackoverflow.com/a/2754674/3785038
        //this answer was remarkably helpful in improving performance
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutputW(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
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

        private static SafeFileHandle consoleHandle;
        private static short width;
        private static short height;
        private static CharInfo[] buf;
        private static SmallRect rect;

        public static void Initialize()
        {
            consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (consoleHandle.IsInvalid)
            {
                Log.Write("Failed to create console handle", "printer");
                PrintLine("Printer failed to create console handle", Styler.ErrorColor);
                Environment.Exit(1);
                return;
            }

            width = (short)Console.WindowWidth;
            height = (short)Console.WindowHeight;

            buf = new CharInfo[width * height];
            rect = new SmallRect() { Left = 0, Top = 0, Right = width, Bottom = height };
        }

        private static int GetBufferIndex(int x, int y)
        {
            return y * width + x;
        }

        private static short GetAttribute(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            return (short)(GetAttribute(backgroundColor) << 4 | GetAttribute(foregroundColor));
        }

        private static short GetAttribute(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: //bright: 0x008 (dark gray)
                    return 0x0000;
                case ConsoleColor.Blue: //bright: 0x0009
                    return 0x0001;
                case ConsoleColor.Green: //bright: 0x000A
                    return 0x0002;
                case ConsoleColor.Cyan: //bright: 0x000B
                    return 0x0003;
                case ConsoleColor.Red: //bright 0x000C
                    return 0x0004;
                case ConsoleColor.Magenta: //bright: 0x000D
                    return 0x0005;
                case ConsoleColor.Yellow: //bright: 0x000E
                    return 0x0006;
                case ConsoleColor.Gray:
                    return 0x0007; //"foreground"
                case ConsoleColor.White: //"bright gray"
                    return 0x000F;
                case ConsoleColor.DarkGray: //bright black
                    return 0x0008;
            }
            return 0x0000;
        }

        private static int GetBufferIndex(Cursor cursor)
        {
            return GetBufferIndex(cursor.SX, cursor.SY);
        }

        public static void ApplyBuffer()
        {
            bool b = WriteConsoleOutputW(consoleHandle,
                buf,
                new Coord() { X = width, Y = height },
                new Coord() { X = 0, Y = 0 },
                ref rect
                );
        }

        static ConsoleColor defaultForegroundColor = ConsoleColor.Gray;
        static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;

        static Cursor printerCursor = new Cursor(0, 0);

        public static int CursorSX { get { return printerCursor.SX; } }
        public static int CursorSY { get { return printerCursor.SY; } }

        public static void Resize()
        {
            
        }

        public static void PrintWord(String word, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            // Console.ForegroundColor = foregroundColor;
            // Console.BackgroundColor = backgroundColor;
            // Console.Write(word);
            // ResetColors();

            int index = GetBufferIndex(printerCursor);

            char[] wordChars = word.ToCharArray();
            short attribute = GetAttribute(foregroundColor, backgroundColor);

            for (int i = index; i < index + word.Length; i++)
            {
                // buf[i].Char.UnicodeChar = word.ToCharArray()[i - index];
                // buf[i].Attributes = GetAttribute(foregroundColor); //TODO: support background colors
                buf[i].Char.UnicodeChar = wordChars[i - index];
                buf[i].Attributes = attribute;
            }

            printerCursor.SX += word.Length;
        }

        public static void PrintToken(Token token)
        {
            // Console.Write(token.ColorCode);
            // Console.Write(token.Text);
            // ResetColors();
            int index = GetBufferIndex(printerCursor);
            for (int i = index; i < index + token.Text.Length; i++)
            {
                buf[i].Char.UnicodeChar = token.Text.ToCharArray()[i - index];
                buf[i].Attributes = GetAttribute(token.Color);
            }

            printerCursor.SX += token.Text.Length;

            // ApplyBuffer();
        }

        public static void Clear()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
            }

            ApplyBuffer();
        }

        public static void MoveCursor(int x, int y)
        {
            printerCursor.Move(x, y);
        }

        public static void PrintLine(String message, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
            // ResetColors();
        }

        public static void DrawHeader(String content, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray, int offsetTop = 0)
        {
            ClearLine(offsetTop);
            printerCursor.Move(0, offsetTop);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(content);
            // ResetColors();
        }

        public static void DrawFooter(String content, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            ClearFooter();
            WriteToFooter(content, 0, foregroundColor, backgroundColor);
        }

        public static void ClearFooter()
        {
            ClearLine(Cursor.MaxScreenY);
        }

        public static void WriteToFooter(String text, int x = -1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (x != -1)
                printerCursor.Move(x, Cursor.MaxScreenY);
            int index = GetBufferIndex(printerCursor);
            for (int i = index; i < index + text.Length; i++)
            {
                buf[i].Char.UnicodeChar = text.ToCharArray()[i - index];
                buf[i].Attributes = GetAttribute(foregroundColor);
            }
            printerCursor.SX += text.Length;
        }

        public static void ClearLine(int line)
        {
            //don't clear if out of range
            if(line < 0 || line > Cursor.MaxScreenY)
                return;
            
            int startIndex = GetBufferIndex(0, line);
            for (int i = startIndex; i < startIndex + width; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = 0x0000;
            }
        }

        public static void ClearRight()
        {
            //don't clear if out of range
            if (printerCursor.SY < 0 || printerCursor.SY > Cursor.MaxScreenY)
                return;

            int startIndex = GetBufferIndex(printerCursor.SX, printerCursor.SY);
            int width = Cursor.MaxScreenX - printerCursor.SX;
            for (int i = startIndex; i < startIndex + width; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = 0x0000;
            }
        }
    }
}