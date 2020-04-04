﻿using System;
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
        #region private fields and properties
        private static readonly string finalFantasyXIVProcessName = "ffxiv_dx11";
        private static readonly string finalFantasyXIVProcessNameDX9 = "ffxiv";
        private static IntPtr GameProcessPtr { get; set; }
        #endregion

        #region user32.dll imports
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        #endregion

        public static Process GameProcess;

        public static void LoadProcess()
        {  
            // find all processes matching either FFXIV or notepad, depending on debugging enabled
            Process[] foundProcesses;
            Process.GetProcesses();
            GameProcessPtr = IntPtr.Zero;

            foundProcesses = Process.GetProcessesByName(finalFantasyXIVProcessName);

            if (foundProcesses.Count() == 0)
                foundProcesses = Process.GetProcessesByName(finalFantasyXIVProcessNameDX9);

            if (foundProcesses.Count() == 0)
                throw new ProcessMissingException();

            GameProcess = foundProcesses.First();
        }


        public static IntPtr ProcessPtr()
        {
            if (GameProcessPtr == IntPtr.Zero)
            {
                GameProcessPtr = FindWindow(null, GameProcess.MainWindowTitle);
            }
             return GameProcessPtr;
        }
    }
}
