using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput.Native;

namespace AutoSynthesis
{
    public static class HotkeyProcessor
    {
        private static Dictionary<Key, string> KeyCodeToString;
        private static Dictionary<Key, VirtualKeyCode> KeyCodeToVKCode;
        private static HashSet<Key> NumpadKeys;
        private const int MAX_CHARACTER = 18;

        static HotkeyProcessor()
        {
            SetDictionaryValues();
        }


        public static string GetKeyInputText(Key keys, HashSet<Key> modKeys, bool shortenString = false)
        {
            string response = "";
            // Add Shift
            if (modKeys.Contains(Key.LeftShift) || modKeys.Contains(Key.RightShift))
                if (!shortenString)
                    response += "SHIFT + ";
                else
                    response += "SHFT+";
            // Add Control
            if (modKeys.Contains(Key.LeftCtrl) || modKeys.Contains(Key.RightCtrl))
                if (!shortenString)
                    response += "CTRL + ";
                else
                    response += "CTL+";
            // Add Alt
            if (modKeys.Contains(Key.LeftAlt) || modKeys.Contains(Key.RightAlt))
                if (!shortenString)
                    response += "ALT + ";
                else
                    response += "ALT+";
            // Add Key
            response += KeyCodeToString[keys];

            // If string is too long, shorten
            if (response.Length > MAX_CHARACTER && !shortenString)
                return GetKeyInputText(keys, modKeys, true);
            return response;
        }

        public static VirtualKeyCode GetVKCFromKeyCode(Key keys)
        {
            return KeyCodeToVKCode[keys];
        }

        private static void SetDictionaryValues()
        {
            // KeyCode to String
            #region KeyCodeToString
            KeyCodeToString = new Dictionary<Key, string>();
            KeyCodeToString.Add(Key.A, "A");
            KeyCodeToString.Add(Key.Add, "NUM +");
            KeyCodeToString.Add(Key.LeftAlt, "ALT");
            KeyCodeToString.Add(Key.B, "B");
            KeyCodeToString.Add(Key.Back, "BACKSPACE");
            KeyCodeToString.Add(Key.C, "C");
            KeyCodeToString.Add(Key.Cancel, "CANCEL");
            KeyCodeToString.Add(Key.CapsLock, "CAPS LOCK");
            KeyCodeToString.Add(Key.Clear, "CLEAR");
            KeyCodeToString.Add(Key.LeftCtrl, "CTRL");
            KeyCodeToString.Add(Key.RightCtrl, "CTRL");
            KeyCodeToString.Add(Key.D, "D");
            KeyCodeToString.Add(Key.D0, "0");
            KeyCodeToString.Add(Key.D1, "1");
            KeyCodeToString.Add(Key.D2, "2");
            KeyCodeToString.Add(Key.D3, "3");
            KeyCodeToString.Add(Key.D4, "4");
            KeyCodeToString.Add(Key.D5, "5");
            KeyCodeToString.Add(Key.D6, "6");
            KeyCodeToString.Add(Key.D7, "7");
            KeyCodeToString.Add(Key.D8, "8");
            KeyCodeToString.Add(Key.D9, "9");
            KeyCodeToString.Add(Key.Decimal, "NUM .");
            KeyCodeToString.Add(Key.Delete, "DEL");
            KeyCodeToString.Add(Key.Divide, "NUM /");
            KeyCodeToString.Add(Key.Down, "DOWN");
            KeyCodeToString.Add(Key.E, "E");
            KeyCodeToString.Add(Key.End, "END");
            KeyCodeToString.Add(Key.Enter, "ENTER");
            KeyCodeToString.Add(Key.Escape, "ESC");
            KeyCodeToString.Add(Key.F, "F");
            KeyCodeToString.Add(Key.F1, "F1");
            KeyCodeToString.Add(Key.F10, "F10");
            KeyCodeToString.Add(Key.F11, "F11");
            KeyCodeToString.Add(Key.F12, "F12");
            KeyCodeToString.Add(Key.F2, "F2");
            KeyCodeToString.Add(Key.F3, "F3");
            KeyCodeToString.Add(Key.F4, "F4");
            KeyCodeToString.Add(Key.F5, "F5");
            KeyCodeToString.Add(Key.F6, "F6");
            KeyCodeToString.Add(Key.F7, "F7");
            KeyCodeToString.Add(Key.F8, "F8");
            KeyCodeToString.Add(Key.F9, "F9");
            KeyCodeToString.Add(Key.G, "G");
            KeyCodeToString.Add(Key.H, "H");
            KeyCodeToString.Add(Key.Home, "HOME");
            KeyCodeToString.Add(Key.I, "I");
            KeyCodeToString.Add(Key.Insert, "INS");
            KeyCodeToString.Add(Key.J, "J");
            KeyCodeToString.Add(Key.K, "K");
            KeyCodeToString.Add(Key.L, "L");
            KeyCodeToString.Add(Key.Left, "LEFT");
            KeyCodeToString.Add(Key.LeftShift, "SHIFT");
            KeyCodeToString.Add(Key.M, "M");
            KeyCodeToString.Add(Key.Multiply, "NUM *");
            KeyCodeToString.Add(Key.N, "N");
            KeyCodeToString.Add(Key.NumLock, "NUM LOCK");
            KeyCodeToString.Add(Key.NumPad0, "NUM 0");
            KeyCodeToString.Add(Key.NumPad1, "NUM 1");
            KeyCodeToString.Add(Key.NumPad2, "NUM 2");
            KeyCodeToString.Add(Key.NumPad3, "NUM 3");
            KeyCodeToString.Add(Key.NumPad4, "NUM 4");
            KeyCodeToString.Add(Key.NumPad5, "NUM 5");
            KeyCodeToString.Add(Key.NumPad6, "NUM 6");
            KeyCodeToString.Add(Key.NumPad7, "NUM 7");
            KeyCodeToString.Add(Key.NumPad8, "NUM 8");
            KeyCodeToString.Add(Key.NumPad9, "NUM 9");
            KeyCodeToString.Add(Key.O, "O");
            //KeyCodeToString.Add(Key.OemBackslash, @"\");
            KeyCodeToString.Add(Key.OemClear, "CLEAR");
            KeyCodeToString.Add(Key.OemCloseBrackets, "]");
            KeyCodeToString.Add(Key.OemComma, ",");
            KeyCodeToString.Add(Key.OemMinus, "-");
            KeyCodeToString.Add(Key.OemOpenBrackets, "[");
            KeyCodeToString.Add(Key.OemPeriod, ".");
            KeyCodeToString.Add(Key.OemPlus, "=");
            KeyCodeToString.Add(Key.OemQuestion, "/");
            KeyCodeToString.Add(Key.OemQuotes, "'");
            KeyCodeToString.Add(Key.OemSemicolon, ";");
            KeyCodeToString.Add(Key.OemTilde, "`");
            KeyCodeToString.Add(Key.Oem5, "\\");
            KeyCodeToString.Add(Key.P, "P");
            KeyCodeToString.Add(Key.PageDown, "PAGE DOWN");
            KeyCodeToString.Add(Key.PageUp, "PAGE UP");
            KeyCodeToString.Add(Key.PrintScreen, "PRINT SCREEN");
            KeyCodeToString.Add(Key.Q, "Q");
            KeyCodeToString.Add(Key.R, "R");
            KeyCodeToString.Add(Key.Right, "RIGHT");
            KeyCodeToString.Add(Key.RightAlt, "ALT");
            KeyCodeToString.Add(Key.RightShift, "SHIFT");
            KeyCodeToString.Add(Key.S, "S");
            KeyCodeToString.Add(Key.Scroll, "SCROLL LOCK");
            KeyCodeToString.Add(Key.Space, "SPACE");
            KeyCodeToString.Add(Key.Subtract, "NUM -");
            KeyCodeToString.Add(Key.T, "T");
            KeyCodeToString.Add(Key.Tab, "TAB");
            KeyCodeToString.Add(Key.U, "U");
            KeyCodeToString.Add(Key.Up, "UP");
            KeyCodeToString.Add(Key.V, "V");
            KeyCodeToString.Add(Key.W, "W");
            KeyCodeToString.Add(Key.X, "X");
            KeyCodeToString.Add(Key.Y, "Y");
            KeyCodeToString.Add(Key.Z, "Z");
            KeyCodeToString.Add(Key.None, "");
            #endregion

            // KeyCode to VirtualKeyCode
            #region KeycodeToVKCode
            KeyCodeToVKCode = new Dictionary<Key, VirtualKeyCode>();
            KeyCodeToVKCode.Add(Key.A, VirtualKeyCode.VK_A);
            KeyCodeToVKCode.Add(Key.Add, VirtualKeyCode.ADD);
            KeyCodeToVKCode.Add(Key.LeftAlt, VirtualKeyCode.MENU);
            KeyCodeToVKCode.Add(Key.B, VirtualKeyCode.VK_B);
            KeyCodeToVKCode.Add(Key.Back, VirtualKeyCode.BACK);
            KeyCodeToVKCode.Add(Key.C, VirtualKeyCode.VK_C);
            KeyCodeToVKCode.Add(Key.Cancel, VirtualKeyCode.CANCEL);
            KeyCodeToVKCode.Add(Key.CapsLock, VirtualKeyCode.CAPITAL);
            KeyCodeToVKCode.Add(Key.Clear, VirtualKeyCode.CLEAR);
            KeyCodeToVKCode.Add(Key.LeftCtrl, VirtualKeyCode.CONTROL);
            KeyCodeToVKCode.Add(Key.RightCtrl, VirtualKeyCode.CONTROL);
            KeyCodeToVKCode.Add(Key.D, VirtualKeyCode.VK_D);
            KeyCodeToVKCode.Add(Key.D0, VirtualKeyCode.VK_0);
            KeyCodeToVKCode.Add(Key.D1, VirtualKeyCode.VK_1);
            KeyCodeToVKCode.Add(Key.D2, VirtualKeyCode.VK_2);
            KeyCodeToVKCode.Add(Key.D3, VirtualKeyCode.VK_3);
            KeyCodeToVKCode.Add(Key.D4, VirtualKeyCode.VK_4);
            KeyCodeToVKCode.Add(Key.D5, VirtualKeyCode.VK_5);
            KeyCodeToVKCode.Add(Key.D6, VirtualKeyCode.VK_6);
            KeyCodeToVKCode.Add(Key.D7, VirtualKeyCode.VK_7);
            KeyCodeToVKCode.Add(Key.D8, VirtualKeyCode.VK_8);
            KeyCodeToVKCode.Add(Key.D9, VirtualKeyCode.VK_9);
            KeyCodeToVKCode.Add(Key.Decimal, VirtualKeyCode.DECIMAL);
            KeyCodeToVKCode.Add(Key.Delete, VirtualKeyCode.DELETE);
            KeyCodeToVKCode.Add(Key.Divide, VirtualKeyCode.DIVIDE);
            KeyCodeToVKCode.Add(Key.Down, VirtualKeyCode.DOWN);
            KeyCodeToVKCode.Add(Key.E, VirtualKeyCode.VK_E);
            KeyCodeToVKCode.Add(Key.End, VirtualKeyCode.END);
            KeyCodeToVKCode.Add(Key.Enter, VirtualKeyCode.RETURN);
            KeyCodeToVKCode.Add(Key.Escape, VirtualKeyCode.END);
            KeyCodeToVKCode.Add(Key.F, VirtualKeyCode.VK_F);
            KeyCodeToVKCode.Add(Key.F1, VirtualKeyCode.F1);
            KeyCodeToVKCode.Add(Key.F10, VirtualKeyCode.F10);
            KeyCodeToVKCode.Add(Key.F11, VirtualKeyCode.F11);
            KeyCodeToVKCode.Add(Key.F12, VirtualKeyCode.F12);
            KeyCodeToVKCode.Add(Key.F2, VirtualKeyCode.F2);
            KeyCodeToVKCode.Add(Key.F3, VirtualKeyCode.F3);
            KeyCodeToVKCode.Add(Key.F4, VirtualKeyCode.F4);
            KeyCodeToVKCode.Add(Key.F5, VirtualKeyCode.F5);
            KeyCodeToVKCode.Add(Key.F6, VirtualKeyCode.F6);
            KeyCodeToVKCode.Add(Key.F7, VirtualKeyCode.F7);
            KeyCodeToVKCode.Add(Key.F8, VirtualKeyCode.F8);
            KeyCodeToVKCode.Add(Key.F9, VirtualKeyCode.F9);
            KeyCodeToVKCode.Add(Key.G, VirtualKeyCode.VK_G);
            KeyCodeToVKCode.Add(Key.H, VirtualKeyCode.VK_H);
            KeyCodeToVKCode.Add(Key.Home, VirtualKeyCode.HOME);
            KeyCodeToVKCode.Add(Key.I, VirtualKeyCode.VK_I);
            KeyCodeToVKCode.Add(Key.Insert, VirtualKeyCode.INSERT);
            KeyCodeToVKCode.Add(Key.J, VirtualKeyCode.VK_J);
            KeyCodeToVKCode.Add(Key.K, VirtualKeyCode.VK_K);
            KeyCodeToVKCode.Add(Key.L, VirtualKeyCode.VK_L);
            KeyCodeToVKCode.Add(Key.Left, VirtualKeyCode.LEFT);
            KeyCodeToVKCode.Add(Key.LeftShift, VirtualKeyCode.SHIFT);
            KeyCodeToVKCode.Add(Key.M, VirtualKeyCode.VK_M);
            KeyCodeToVKCode.Add(Key.Multiply, VirtualKeyCode.MULTIPLY);
            KeyCodeToVKCode.Add(Key.N, VirtualKeyCode.VK_N);
            KeyCodeToVKCode.Add(Key.NumLock, VirtualKeyCode.NUMLOCK);
            KeyCodeToVKCode.Add(Key.NumPad0, VirtualKeyCode.NUMPAD0);
            KeyCodeToVKCode.Add(Key.NumPad1, VirtualKeyCode.NUMPAD1);
            KeyCodeToVKCode.Add(Key.NumPad2, VirtualKeyCode.NUMPAD2);
            KeyCodeToVKCode.Add(Key.NumPad3, VirtualKeyCode.NUMPAD3);
            KeyCodeToVKCode.Add(Key.NumPad4, VirtualKeyCode.NUMPAD4);
            KeyCodeToVKCode.Add(Key.NumPad5, VirtualKeyCode.NUMPAD5);
            KeyCodeToVKCode.Add(Key.NumPad6, VirtualKeyCode.NUMPAD6);
            KeyCodeToVKCode.Add(Key.NumPad7, VirtualKeyCode.NUMPAD7);
            KeyCodeToVKCode.Add(Key.NumPad8, VirtualKeyCode.NUMPAD8);
            KeyCodeToVKCode.Add(Key.NumPad9, VirtualKeyCode.NUMPAD9);
            KeyCodeToVKCode.Add(Key.O, VirtualKeyCode.VK_O);
            KeyCodeToVKCode.Add(Key.OemBackslash, VirtualKeyCode.OEM_5);
            KeyCodeToVKCode.Add(Key.OemClear, VirtualKeyCode.OEM_CLEAR);
            KeyCodeToVKCode.Add(Key.OemCloseBrackets, VirtualKeyCode.OEM_6);
            KeyCodeToVKCode.Add(Key.OemComma, VirtualKeyCode.OEM_COMMA);
            KeyCodeToVKCode.Add(Key.OemMinus, VirtualKeyCode.OEM_MINUS);
            KeyCodeToVKCode.Add(Key.OemOpenBrackets, VirtualKeyCode.OEM_4);
            KeyCodeToVKCode.Add(Key.OemPeriod, VirtualKeyCode.OEM_PERIOD);
            KeyCodeToVKCode.Add(Key.OemPlus, VirtualKeyCode.OEM_PLUS);
            KeyCodeToVKCode.Add(Key.OemQuestion, VirtualKeyCode.OEM_2);
            KeyCodeToVKCode.Add(Key.OemQuotes, VirtualKeyCode.OEM_7);
            KeyCodeToVKCode.Add(Key.OemSemicolon, VirtualKeyCode.OEM_1);
            KeyCodeToVKCode.Add(Key.OemTilde, VirtualKeyCode.OEM_3);
            KeyCodeToVKCode.Add(Key.Oem5, VirtualKeyCode.OEM_5);
            KeyCodeToVKCode.Add(Key.P, VirtualKeyCode.VK_P);
            KeyCodeToVKCode.Add(Key.PageDown, VirtualKeyCode.NEXT);
            KeyCodeToVKCode.Add(Key.PageUp, VirtualKeyCode.PRIOR);
            KeyCodeToVKCode.Add(Key.PrintScreen, VirtualKeyCode.SNAPSHOT);
            KeyCodeToVKCode.Add(Key.Q, VirtualKeyCode.VK_Q);
            KeyCodeToVKCode.Add(Key.R, VirtualKeyCode.VK_R);
            KeyCodeToVKCode.Add(Key.Right, VirtualKeyCode.RIGHT);
            KeyCodeToVKCode.Add(Key.RightAlt, VirtualKeyCode.MENU);
            KeyCodeToVKCode.Add(Key.RightShift, VirtualKeyCode.SHIFT);
            KeyCodeToVKCode.Add(Key.S, VirtualKeyCode.VK_S);
            KeyCodeToVKCode.Add(Key.Scroll, VirtualKeyCode.SCROLL);
            KeyCodeToVKCode.Add(Key.Space, VirtualKeyCode.SPACE);
            KeyCodeToVKCode.Add(Key.Subtract, VirtualKeyCode.SUBTRACT);
            KeyCodeToVKCode.Add(Key.T, VirtualKeyCode.VK_T);
            KeyCodeToVKCode.Add(Key.Tab, VirtualKeyCode.TAB);
            KeyCodeToVKCode.Add(Key.U, VirtualKeyCode.VK_U);
            KeyCodeToVKCode.Add(Key.Up, VirtualKeyCode.UP);
            KeyCodeToVKCode.Add(Key.V, VirtualKeyCode.VK_V);
            KeyCodeToVKCode.Add(Key.W, VirtualKeyCode.VK_W);
            KeyCodeToVKCode.Add(Key.X, VirtualKeyCode.VK_X);
            KeyCodeToVKCode.Add(Key.Y, VirtualKeyCode.VK_Y);
            KeyCodeToVKCode.Add(Key.Z, VirtualKeyCode.VK_Z);
            #endregion

            // Numpad keys
            #region NumpadKeys
            NumpadKeys = new HashSet<Key>
            {
                Key.NumPad0,
                Key.NumPad1,
                Key.NumPad2,
                Key.NumPad3,
                Key.NumPad4,
                Key.NumPad5,
                Key.NumPad6,
                Key.NumPad7,
                Key.NumPad8,
                Key.NumPad9,
                Key.Decimal
            };
            #endregion
        }

        public static bool NumpadKey(Key numkey)
        {
            return NumpadKeys.Contains(numkey);
        }

    }
}
