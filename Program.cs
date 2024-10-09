using System.Runtime.InteropServices;

namespace ResizeWindow
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool repaint);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private enum WindowActionStatus
        {
            Success,
            WindowNotFound,
            InvalidInput,
            ActionFailed
        }

        static void Main(string[] args)
        {
            if (!ValidateArguments(args, out int x, out int y, out int width, out int height, out string windowName))
                return;

            WindowActionStatus status = MoveAndResizeWindow(x, y, width, height, windowName);

            HandleActionResult(status, windowName);
        }

        private static bool ValidateArguments(string[] args, out int x, out int y, out int width, out int height, out string windowName)
        {
            x = y = width = height = 0;
            windowName = string.Empty;

            if (args.Length != 5)
            {
                Console.WriteLine("All arguments must be specified. Usage: my-program.exe x y width height windowName");
                return false;
            }

            if (!int.TryParse(args[0], out x) || !int.TryParse(args[1], out y) ||
                !int.TryParse(args[2], out width) || !int.TryParse(args[3], out height))
            {
                Console.WriteLine("Invalid numerical arguments for x, y, width, or height.");
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                Console.WriteLine("Width and height cant be 0.");
                return false;
            }

            windowName = args[4];
            return true;
        }

        private static WindowActionStatus MoveAndResizeWindow(int x, int y, int width, int height, string windowName)
        {
            IntPtr windowHandle = FindWindow(null, windowName);

            if (windowHandle == IntPtr.Zero)
            {
                return WindowActionStatus.WindowNotFound;
            }

            if (!SetForegroundWindow(windowHandle))
            {
                return WindowActionStatus.ActionFailed;
            }

            if (GetWindowRect(windowHandle, out Rect rect))
            {
                Console.WriteLine($"Current window position: left={rect.Left}, top={rect.Top}, right={rect.Right}, bottom={rect.Bottom}");
            }

            if (!MoveWindow(windowHandle, x, y, width, height, true))
            {
                return WindowActionStatus.ActionFailed;
            }

            return WindowActionStatus.Success;
        }

        private static void HandleActionResult(WindowActionStatus status, string windowName)
        {
            switch (status)
            {
                case WindowActionStatus.Success:
                    Console.WriteLine($"Successfully moved and resized window '{windowName}'");
                    break;
                case WindowActionStatus.WindowNotFound:
                    Console.WriteLine($"Window '{windowName}' not found");
                    break;
                case WindowActionStatus.ActionFailed:
                    Console.WriteLine($"Failed to bring window '{windowName}' to foreground");
                    break;
                default:
                    Console.WriteLine("Unknown error");
                    break;
            }
        }
    }
}
