using System;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using System.Threading;

namespace AutoSynthesis 
{
    static class KeyInputEngine
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;

        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);

        public static void SendInputTest()
        {
            var key = VirtualKeyCode.VK_W;
            var mods = new VirtualKeyCode[] { VirtualKeyCode.CONTROL , VirtualKeyCode.SHIFT};
            SendKeysToGame(key, mods);
        }

        public static void SendKeysToGame(VirtualKeyCode key, VirtualKeyCode[] modKeys = null)
        {
            LogInputs(key, modKeys);
            // submit mod keys
            if (modKeys != null)
            {
                foreach (var modKey in modKeys)
                {
                    SendMessage(ProcessManager.ProcessPtr(), WM_KEYDOWN, (int)modKey, 0);
                }

                Thread.Sleep(50);
            }

            // send key command 
            SendMessage(ProcessManager.ProcessPtr(), WM_KEYDOWN, (int)key, 0);
            SendMessage(ProcessManager.ProcessPtr(), WM_KEYUP, (int)key, 0);

            // release mod keys
            if (modKeys != null)
            {
                foreach (var modKey in modKeys)
                {
                    SendMessage(ProcessManager.ProcessPtr(), WM_KEYUP, (int)modKey, 0);
                }
            }
        }

        private static void LogInputs(VirtualKeyCode key, VirtualKeyCode[] modKeys)
        {
            string modkeyText = "";
            if (modKeys != null && modKeys.Length >= 1)
            {
                modkeyText += " With Mods";
                foreach (var mod in modKeys)
                    modkeyText += " " + mod.ToString();
            }
            Logger.Write($"Input {key}{modkeyText} to Process {ProcessManager.GameProcess.Id}");
        }
    }
}

