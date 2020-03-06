using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidSynthesis
{
    // TODO: 
    // implement logic for handling multiple processes
    // handle no process found
    static class ProcessManager
    {
        #region private fields and properties
        private static readonly string debugProcessName = "notepad";
        private static readonly string finalFantasyXIVProcessName = "ffxiv_dx11";
        private static readonly bool debugEnabled = false;
        private static IntPtr GameProcessPtr { get; set; }
        #endregion

        #region user32.dll imports
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        public static Process GameProcess;

        static ProcessManager()
        {
            LoadProcess();
        }

        public static void LoadProcess()
        {  
            // find all processes matching either FFXIV or notepad, depending on debugging enabled
            Process[] foundProcesses;
            var lol = Process.GetProcesses();
            if (!debugEnabled)
            {
                foundProcesses = Process.GetProcessesByName(finalFantasyXIVProcessName);
            }
            else
            {
                foundProcesses = Process.GetProcessesByName(debugProcessName);
            }

            // handle multiple processes
            GameProcess = foundProcesses[0];
        }


        public static IntPtr ProcessPtr()
        {
            if (GameProcessPtr == IntPtr.Zero)
            {
                GameProcessPtr = FindWindow(null, GameProcess.MainWindowTitle);
                // if debug is enabled, child process of notepad needs to be grabbed
                if (debugEnabled)
                {
                    GameProcessPtr = FindWindowEx(GameProcessPtr, IntPtr.Zero, "edit", null);
                }
            }
             return GameProcessPtr;
        }

        public static void FocusGame()
        {
            SetForegroundWindow(ProcessPtr());
        }
    }
}
