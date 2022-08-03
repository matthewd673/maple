using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace maple
{
    public static class Printer
    {
        private static SafeFileHandle consoleHandle;
        private static short bufWidth;
        private static short bufHeight;
        private static Win32Console.CharInfo[] buf;
        private static Win32Console.SmallRect rect;

        private const int STD_INPUT_HANDLE = -10;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int ENABLE_WINDOW_INPUT = 0x0008;
        private const int ENABLE_MOUSE_INPUT = 0x0010;

        // to keep track of input handler / mode
        private static IntPtr hStdin;
        private static uint fdwOldMode;

        private static Thread inputThread;
        private const int InputLoopDelay = 10;

        public static Dictionary<ConsoleColor, short> ConsoleColorToAttributeTable = new() {
            { ConsoleColor.Black, 0x0000 },
            { ConsoleColor.DarkBlue, 0x0001 },
            { ConsoleColor.Blue, 0x0009 },
            { ConsoleColor.DarkGreen, 0x0002 },
            { ConsoleColor.Green, 0x000A },
            { ConsoleColor.DarkCyan, 0x0003 },
            { ConsoleColor.Cyan, 0x000B },
            { ConsoleColor.DarkRed, 0x0004 },
            { ConsoleColor.Red, 0x000C },
            { ConsoleColor.DarkMagenta, 0x0005 },
            { ConsoleColor.Magenta, 0x000D },
            { ConsoleColor.DarkYellow, 0x0006 },
            { ConsoleColor.Yellow, 0x000E },
            { ConsoleColor.Gray, 0x0007 },
            { ConsoleColor.DarkGray, 0x0008 },
            { ConsoleColor.White, 0x000F },
        };

        /// <summary>
        /// Create the Console handle and prepare the buffer for rendering.
        /// </summary>
        public static void Initialize()
        {
            consoleHandle = Win32Console.CreateConsoleHandle();
            if (consoleHandle == null) return; // it should Environment.Exit() first

            bufWidth = (short) Console.WindowWidth;
            bufHeight = (short) Console.WindowHeight;

            buf = new Win32Console.CharInfo[bufWidth * bufHeight];
            rect = new Win32Console.SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
        }

        public static int MinScreenX = 0;
        public static int MinScreenY = 0;
        public static int MaxScreenX
        {
            get
            {
                return bufWidth - 1;
            }
        }

        public static int MaxScreenY
        {
            get
            {
                return bufHeight - 1;
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
            return (short)((short)(ConsoleColorToAttributeTable[backgroundColor] << 4) | ConsoleColorToAttributeTable[foregroundColor]);
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
            bool b = Win32Console.WriteConsoleOutputW(consoleHandle,
                buf,
                new Win32Console.Coord() { X = bufWidth, Y = bufHeight },
                new Win32Console.Coord() { X = 0, Y = 0 },
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

            buf = new Win32Console.CharInfo[bufWidth * bufHeight];
            rect = new Win32Console.SmallRect() { Left = 0, Top = 0, Right = bufWidth, Bottom = bufHeight };
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
                    buf[buf.Length - 1].Char.UnicodeChar = Settings.Properties.OverflowIndicatorChar;
                    buf[buf.Length - 1].Attributes = attribute;
                    break;
                }
                if (i == buf.Length || (i > index && i % bufWidth == 0)) //line overflow
                {
                    buf[i - 1].Char.UnicodeChar = Settings.Properties.OverflowIndicatorChar;
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
    }
}
