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
        #region private fields and properties
        private static readonly string finalFantasyXIVProcessName = "ffxiv_dx11";
        private static readonly string finalFantasyXIVProcessNameDX9 = "ffxiv";
        private static IntPtr GameProcessPtr { get; set; }
        #endregion

        #region user32.dll imports
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlockIt);
        #endregion

        #region Structs
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
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


        public static void DisableInputs()
        {
            var verify = VerifyGameState();
            if (verify)
                BlockInput(true);
        }

        public static void EnableInputs()
        {
            BlockInput(false);
        }

        private static bool VerifyGameState()
        {
            // Return TRUE IF: 
            // 1. Game is in focus OR
            // 2. Mouse over the game
            return (GameInFocus() || MouseOverGame());
        }

        private static bool MouseOverGame()
        {
            // Get window of game, if failed return false
            // See if mouse is within game window region

            Rect windowArea = new Rect();
            var getRect = GetWindowRect(GameProcessPtr, ref windowArea);

            if (!getRect)
                return false;

            var mousePos = System.Windows.Forms.Cursor.Position;

            var mouseHorizontalCheck = (mousePos.X > windowArea.Left && mousePos.Y < windowArea.Right);
            var mouseVerticalCheck = (mousePos.Y > windowArea.Top && mousePos.Y < windowArea.Bottom);
            return (mouseHorizontalCheck && mouseVerticalCheck);
        }

        private static bool GameInFocus()
        {
            try
            {
                var foregroundPtr = GetForegroundWindow();
                var check = IntPtr.Equals(foregroundPtr, GameProcessPtr);
                return check;
            }
            catch
            {
                return false;
            }
        }
    }
}
