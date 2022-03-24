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

        /// <summary>
        /// Create the Console handle and prepare the buffer for rendering.
        /// </summary>
        public static void Initialize()
        {
            consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (consoleHandle.IsInvalid)
            {
                Log.Write("Failed to create console handle", "printer", important: true);
                PrintLineSimple("Printer failed to create console handle", Styler.ErrorColor);
                Environment.Exit(1); //kinda temporary
                return;
            }

            Log.Write("Successfully created console handle", "printer");

            bufWidth = (short)Console.WindowWidth;
            bufHeight = (short)Console.WindowHeight;

            buf = new CharInfo[bufWidth * bufHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
        }

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
        private static short GetAttributeFromColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            return (short)((short)(GetAttributeFromColor(backgroundColor) << 4) | GetAttributeFromColor(foregroundColor));
        }

        /// <summary>
        /// Generate a buffer attribute value representing the given color. The attribute represents the color as the foreground color with a black background.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>A buffer attrivute representing the color.</returns>
        private static short GetAttributeFromColor(ConsoleColor color)
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

        static Cursor printerCursor = new Cursor(0, 0);

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
            }

            printerCursor.SX += word.Length;
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
            printerCursor.Move(x, y);
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
        public static void DrawFooter(String content, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            ClearFooter();
            WriteToFooter(content, 0, foregroundColor, backgroundColor);
        }

        /// <summary>
        /// Clear the Editor's footer region (bottom line of the Console).
        /// </summary>
        public static void ClearFooter()
        {
            ClearLine(Cursor.MaxScreenY);
        }

        /// <summary>
        /// Write to the Editor's footer region (bottom line of the Console).
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="x">The </param>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        public static void WriteToFooter(String text, int x = -1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            if (x != -1)
                printerCursor.Move(x, Cursor.MaxScreenY);
            int index = GetBufferIndex(printerCursor);

            char[] textChars = text.ToCharArray();
            short attribute = GetAttributeFromColor(foregroundColor);

            for (int i = index; i < index + text.Length; i++)
            {
                if (i >= buf.Length) //overflow
                {
                    buf[buf.Length - 1].Char.UnicodeChar = '…';
                    buf[buf.Length - 1].Attributes = GetAttributeFromColor(ConsoleColor.Black, Styler.AccentColor);
                    break;
                }

                buf[i].Char.UnicodeChar = textChars[i - index];
                buf[i].Attributes = attribute;
            }

            printerCursor.SX += text.Length;
        }

        /// <summary>
        /// Clear the given line.
        /// </summary>
        /// <param name="line">The line index.</param>
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

        /// <summary>
        /// Clear the line to the right of the internal printer Cursor.
        /// </summary>
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