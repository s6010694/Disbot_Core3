using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Disbot.Marshal
{
    internal static class ConsoleEvent
    {
        internal delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}
