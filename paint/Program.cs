using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace paint
{
    class Program
    {
        [DllImport("user32")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32")]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32")]
        static extern int GetSystemMetrics(int smIndex);

        [DllImport("user32")]
        static extern bool GetWindowRect(IntPtr hDc, [In, Out] ref RECT rect);

        [DllImport("user32")]
        static extern bool FillRect(IntPtr hdc, [In] ref RECT rect, IntPtr hbrush);

        [DllImport("user32")]
        static extern IntPtr LoadImage(IntPtr hDc, string name, int type, int cx, int cy, int fuLoad);

        [DllImport("user32")]
        static extern IntPtr LoadIcon(HandleRef hInst, IntPtr iconId);

        [DllImport("user32")]
        static extern bool DrawIconEx(IntPtr hDc, int x, int y, IntPtr hIcon, int width, int height, int iStepIfAniCursor, HandleRef hBrushFlickerFree, int diFlags);

        [DllImport("user32")]
        static extern int DrawText(IntPtr hDC, string lpszString, int nCount, ref RECT lpRect, int nFormat);

        [DllImport("gdi32")]
        static extern IntPtr GetStockObject(int nIndex);

        [DllImport("gdi32")]
        static extern int GetObject(IntPtr hObject, int nSize, [In, Out] LOGFONT lf);

        [DllImport("gdi32")]
        static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32")]
        static extern int SetTextColor(IntPtr hDC, int crColor);

        [DllImport("gdi32")]
        static extern int SetBkColor(IntPtr hDC, int clr);

        [DllImport("gdi32")]
        static extern int SetDCBrushColor(IntPtr hDC, int color);

        [DllImport("gdi32")]
        static extern bool Rectangle(IntPtr hDc, int left, int top, int right, int bottom);

        [DllImport("gdi32")]
        static extern bool Ellipse(IntPtr hDc, int x1, int y1, int x2, int y2);

        [DllImport("gdi32")]
        static extern IntPtr CreateFontIndirect(LOGFONT lf);

        [DllImport("gdi32")]
        static extern IntPtr CreatePatternBrush(IntPtr hbmp);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfCharSet;

            public LOGFONT()
            {
            }
        }

        static int ToWin32(Color c)
        {
            return c.R << 0 | c.G << 8 | c.B << 16;
        }

        static readonly string version = "v0.1.0";
        static HandleRef Null = new HandleRef(null, IntPtr.Zero);
        private static readonly int err = 32513, warn = 32515, info = 32516;
        static readonly int height = GetSystemMetrics(1);
        static readonly string[] brush = { "square", "" };
        static string colour;
        static int size = 20;
        static int time = 10;

        static void Main(string[] args)
        {
            Console.WriteLine($"BadPaint {version} by fxys\n");
            if (args.Length == 0)
                Help();
            for (int i = 0; i < args.Length; i += 2) 
            {
               switch(args[i])
               {
                    case "-help":
                    case "-h":
                        Help();
                        break;
                    case "-brush":
                    case "-b":
                        if (args[i + 1] == "-f" || args[i + 1] == "-file")
                        {
                            brush[0] = args[i + 1];
                            brush[1] = args[i + 2];
                            i++;
                            break;
                        }
                        brush[0] = args[i + 1];
                        break;
                    case "-size":
                    case "-s":
                        if (!int.TryParse(args[i + 1], out size))
                            Error($"Invalid brush size: {args[i + 1]}");
                        break;
                    case "-colour":
                    case "-c":
                        colour = args[i+1];
                        break;
                    case "-time":
                    case "-t":
                        if (!int.TryParse(args[i + 1], out time))
                            Error($"Invalid time length: {args[i + 1]}");
                        break;
                    default:
                        Error($"Invalid argument: {args[i]}");
                        break;
               }
            }
            IntPtr hdc = GetDC(IntPtr.Zero);
            Thread t;
            Color c = Color.White;
            int cf = ToWin32(c);
            if (colour != null)
            {
                c = Color.FromName(colour);
                if (!c.IsKnownColor)
                    Error($"Color, {colour} is not valid");
                cf = ToWin32(c);
            }
            if (size == 0)
                Error($"{size} is not a valid brush size");
            switch (brush[0])
            {
                case "square":
                    t = new Thread(() => DrawShapes(hdc, 0, size, cf));
                    break;
                case "circle":
                    t = new Thread(() => DrawShapes(hdc, 1, size, cf));
                    break;
                case "error":
                    t = new Thread(() => DrawIcons(hdc, new IntPtr(err), size));
                    break;
                case "warning":
                    t = new Thread(() => DrawIcons(hdc, new IntPtr(warn), size));
                    break;
                case "info":
                    t = new Thread(() => DrawIcons(hdc, new IntPtr(info), size));
                    break;
                case "-f":
                case "-file":
                    t = new Thread(() => DrawImages(hdc, brush[1], 2 * size));
                    break;
                default:
                    Color bg = Color.FromArgb(c.ToArgb() ^ 0xffffff);
                    int bf = ToWin32(bg);
                    t = new Thread(() => DrawTexts(brush[0], size, 100, cf, bf));
                    break;

            }
            t.Start();
            time = time > 30 ? 30 : time;
            Console.WriteLine($"You have {time} seconds to paint!");
            Thread.Sleep(time * 1000);
            Environment.Exit(0);
        }

        private static void Help()
        {
            Console.WriteLine("paint [ARGUMENTS]\n");
            Console.WriteLine("Description: \n  Allows user to choose their settings to paint onto the display.\n");
            Console.WriteLine("Parameters list: \n  -help, -h  :  Displays this message.\n\n  -brush, -b  :  Choose the brush shape - if left empty brush will be square shaped.\n");
            Console.WriteLine("  -size, -s  :  Change the radius of the brush in pixels.\n\n  -colour, -c  :  Change the colour of the brush - most of the common colours will work.\n");
            Console.WriteLine("  -time, -t  :  Change the amount of time you will be able to paint for - max 30.\n");
            Environment.Exit(0);
        }

        static void Error(string err)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {err}!");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }

        static void DrawIcons(IntPtr hdc, IntPtr icon, int rad)
        {
            while (true)
            {
                GetCursorPos(out Point cur);
                DrawIconEx(hdc, cur.X, cur.Y, LoadIcon(Null, icon), 2 * rad, 2 * rad, 0, Null, 0x000003);
            }
        }

        static void DrawShapes(IntPtr hdc, int shape, int rad, int col)
        {
            while (true)
            {
                GetCursorPos(out Point cur);
                SelectObject(hdc, GetStockObject(8));
                SelectObject(hdc, GetStockObject(18));
                SetDCBrushColor(hdc, col);
                if (shape == 0)
                    Rectangle(hdc, cur.X, cur.Y, cur.X + 2 * rad, cur.Y + 2 * rad);
                else
                    Ellipse(hdc, cur.X, cur.Y, cur.X + 2 * rad, cur.Y + 2 * rad);
            }
        }

        static void DrawTexts(string text, int h, int w, int tc, int bc)
        {
            IntPtr Hfont = GetStockObject(17);
            LOGFONT lf = new LOGFONT();
            GetObject(Hfont, Marshal.SizeOf(lf), lf);
            lf.lfHeight = h;
            lf.lfWeight = w;
            lf.lfItalic = 1;
            while (true)
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                IntPtr hNew = CreateFontIndirect(lf);
                IntPtr hOld = SelectObject(hdc, hNew);
                GetCursorPos(out Point cur);
                RECT r = new RECT(2 * cur.X, cur.Y, 0, height);
                SetTextColor(hdc, tc);
                SetBkColor(hdc, bc);
                DrawText(hdc, text, text.Length + 1, ref r, 0x00000003);
                SelectObject(hdc, hOld);
                DeleteObject(hNew);
            }
        }

        static void DrawImages(IntPtr hdc, string file, int size)
        {
            Image i;
            try
            {
                i = Image.FromFile(file.Replace(@"\", @"\\"));
                i.Save("temp.bmp", ImageFormat.Bmp);
            }
            catch
            {
                Error($"Cannot find file: {file}");
            }
            while (true)
            {
                IntPtr brush = CreatePatternBrush(LoadImage(hdc, "temp.bmp", 0, size, size, 0x00000010));
                GetCursorPos(out Point cur);
                RECT r = new RECT(cur.X, cur.Y, cur.X + size, cur.Y + size);
                GetWindowRect(hdc, ref r);
                FillRect(hdc, ref r, brush);
                DeleteObject(brush);
            }
        }
    }
}
