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
    static class ProcessManager
    {
        private static readonly string debugProcessName = "notepad";
        private static readonly string finalFantasyXIVProcessName = "ffxiv";
        private static readonly bool debugEnabled = false;
        private static IntPtr GameProcessPtr { get; set; }
        public static Process GameProcess;


        public static void LoadProcess()
        {  
            // find all processes
            Process[] foundProcesses;
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


        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr ProcessPtr()
        {
            if (GameProcessPtr == null)
            {
                GameProcessPtr = FindWindow(null, GameProcess.MainWindowTitle);
            }
            return GameProcessPtr;
        }
    }
}
