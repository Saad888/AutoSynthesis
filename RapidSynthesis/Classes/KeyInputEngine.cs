using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


// https://github.com/EasyAsABC123/Keyboard
// https://stackoverflow.com/questions/13200362/how-to-send-ctrl-shift-alt-key-combinations-to-an-application-window-via-sen
// APP NEEDS TO RUN IN ADMIN FFS


namespace RapidSynthesis 
{
    static class KeyInputEngine
    {

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);

        public static void SendInputTest()
        {
            var key = Keys.D2;
            var mods = new Keys[] { Keys.Control };
            SendKeyCombo(key, mods);
        }

        private static void SendKeyCombo(Keys key, Keys[] modKeys = null)
        {
            foreach(Keys modKey in modKeys)
            {
                KeyDown(modKey);
            }
            Thread.Sleep(100);
            KeyDown(key);
            foreach(Keys modKey in modKeys)
            {
                KeyUp(modKey);
            }
        }

        private static void KeyDown(Keys key)
        {
            PostMessage(ProcessManager.ProcessPtr(), WM_KEYDOWN, (int)key, 0);
        }

        private static void KeyUp(Keys key)
        {
            PostMessage(ProcessManager.ProcessPtr(), WM_KEYUP, (int)key, 0);
        }
    }
}
