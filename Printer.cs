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
        private static short bufWidth;
        private static short bufHeight;
        private static CharInfo[] buf;
        private static SmallRect rect;

        public static void Initialize()
        {
            consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (consoleHandle.IsInvalid)
            {
                Log.Write("Failed to create console handle", "printer");
                PrintLineSimple("Printer failed to create console handle", Styler.ErrorColor);
                Environment.Exit(1);
                return;
            }

            bufWidth = (short)Console.WindowWidth;
            bufHeight = (short)Console.WindowHeight;

            buf = new CharInfo[bufWidth * bufHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
        }

        private static int GetBufferIndex(int x, int y)
        {
            return y * bufWidth + x;
        }

        private static short GetAttribute(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            return (short)((short)(GetAttribute(backgroundColor) << 4) | GetAttribute(foregroundColor));
        }

        private static short GetAttribute(ConsoleColor color)
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

        private static int GetBufferIndex(Cursor cursor)
        {
            return GetBufferIndex(cursor.SX, cursor.SY);
        }

        public static void ApplyBuffer()
        {
            bool b = WriteConsoleOutputW(consoleHandle,
                buf,
                new Coord() { X = bufWidth, Y = bufHeight },
                new Coord() { X = 0, Y = 0 },
                ref rect
                );
        }

        static Cursor printerCursor = new Cursor(0, 0);

        public static void Resize()
        {
            bufWidth = (short)Console.WindowWidth;
            bufHeight = (short)Console.WindowHeight;

            buf = new CharInfo[bufWidth * bufHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
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
        }

        public static void PrintManually(char c, int sX, int sY, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            int index = GetBufferIndex(sX, sY);
            buf[index].Char.UnicodeChar = c;
            buf[index].Attributes = GetAttribute(foregroundColor, backgroundColor);
        }

        public static void Clear()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i].Char.UnicodeChar = 0x0020;
                buf[i].Attributes = 0x0000;
            }

            ApplyBuffer();
        }

        public static void MoveCursor(int x, int y)
        {
            printerCursor.Move(x, y);
        }

        public static void PrintLineSimple(String message, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(message);
        }

        public static void DrawHeader(String content, ConsoleColor foregroundColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Gray, int offsetTop = 0)
        {
            ClearLine(offsetTop);
            printerCursor.Move(0, offsetTop);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(content);
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

            char[] textChars = text.ToCharArray();
            short attribute = GetAttribute(foregroundColor);

            for (int i = index; i < index + text.Length; i++)
            {
                if (i >= buf.Length) //overflow
                {
                    buf[buf.Length - 1].Char.UnicodeChar = '…';
                    buf[buf.Length - 1].Attributes = GetAttribute(ConsoleColor.Black, Styler.AccentColor);
                    break;
                }

                buf[i].Char.UnicodeChar = textChars[i - index];
                buf[i].Attributes = attribute;
            }

            printerCursor.SX += text.Length;
        }

        public static void ClearLine(int line)
        {
            //don't clear if out of range
            if(line < 0 || line > Cursor.MaxScreenY)
                return;
            
            int startIndex = GetBufferIndex(0, line);
            for (int i = startIndex; i < startIndex + bufWidth; i++)
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