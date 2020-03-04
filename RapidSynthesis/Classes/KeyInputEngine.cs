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
// APP NEEDS TO RUN IN ADMIN FFS


namespace RapidSynthesis 
{
    static class KeyInputEngine
    {


        #region Imports
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        #endregion
        public static void SendInputTest()
        {


            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;
            var key = Keys.C;

            IntPtr ffxiv = FindWindow(null, "FINAL FANTASY XIV");
            IntPtr editx = FindWindowEx(ffxiv, IntPtr.Zero, "FFXIVGAME", null);

            PostMessage(ffxiv, WM_KEYDOWN, (int)key, 0x001F0001);
            Thread.Sleep(100);
            PostMessage(ffxiv, WM_KEYUP, (int)key, 0xC01F0001);


            //    const uint WM_KEYDOWN = 0x100;
            //    const uint WM_SYSCOMMAND = 0x018;
            //    const uint SC_CLOSE = 0x053;

            //    IntPtr WindowToFind = FindWindow(null, "FINAL FANTASY XIV");

            //    IntPtr result3 = PostMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)Keys.Space), (IntPtr)0);
            //IntPtr result3 = SendMessage(WindowToFind, WM_KEYUP, ((IntPtr)c), (IntPtr)0);
        }
    }
}
