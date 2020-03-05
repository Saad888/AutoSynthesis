using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using WindowsInput.Native;
//using WindowsInput;


// https://github.com/EasyAsABC123/Keyboard
// https://stackoverflow.com/questions/13200362/how-to-send-ctrl-shift-alt-key-combinations-to-an-application-window-via-sen
// APP NEEDS TO RUN IN ADMIN FFS
// https://archive.codeplex.com/?p=inputsimulator


namespace RapidSynthesis 
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

        private static void SendKeysToGame(VirtualKeyCode key, VirtualKeyCode[] modKeys = null)
        {
            KeyPress(key, modKeys);
        }

        private static void KeyPress(VirtualKeyCode key, VirtualKeyCode[] modKeys)
        {
            Thread.Sleep(5000);
            if (modKeys != null)
            {
                foreach (var modKey in modKeys)
                {
                    SendMessage(ProcessManager.ProcessPtr(), WM_KEYDOWN, (int)modKey, 0);
                }
            }

            SendMessage(ProcessManager.ProcessPtr(), WM_KEYDOWN, (int)key, 0);
            SendMessage(ProcessManager.ProcessPtr(), WM_KEYUP, (int)key, 0);
            if (modKeys != null)
            {
                foreach (var modKey in modKeys)
                {
                    SendMessage(ProcessManager.ProcessPtr(), WM_KEYUP, (int)modKey, 0);
                }
            }
        }


    }
}

