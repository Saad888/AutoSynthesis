using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RapidSynthesis
{
    public static class HotkeyProcessor
    {
        private static Dictionary<Key, string> KeyCodeToString;

        static HotkeyProcessor()
        {
            SetDictionaryValues();
        }

        public static string ProcessEventInputs(Key keys, HashSet<Key> modKeys)
        {
            string response = "";
            // Add Shift
            if (modKeys.Contains(Key.LeftShift) || modKeys.Contains(Key.RightShift))
                response += "SHIFT + ";
            // Add Control
            if (modKeys.Contains(Key.LeftCtrl) || modKeys.Contains(Key.RightCtrl))
                response += "CTRL + ";
            // Add Alt
            if (modKeys.Contains(Key.LeftAlt) || modKeys.Contains(Key.RightAlt))
                response += "ALT + ";
            // Add Key
            response += keys.ToString();

            if (KeyCodeToString.ContainsKey(keys))
                return KeyCodeToString[keys];
            else
                return "MISSING SHIT";
        }

        private static void SetDictionaryValues()
        {
            KeyCodeToString = new Dictionary<Key, string>();
            KeyCodeToString.Add(Key.A, "A");
            KeyCodeToString.Add(Key.Add, "+");
            KeyCodeToString.Add(Key.LeftAlt, "Alt");
            KeyCodeToString.Add(Key.B, "B");
            KeyCodeToString.Add(Key.Back, "Backspace");
            KeyCodeToString.Add(Key.C, "C");
            KeyCodeToString.Add(Key.Cancel, "Cancel");
            KeyCodeToString.Add(Key.CapsLock, "Caps");
            KeyCodeToString.Add(Key.Clear, "Clear");
            KeyCodeToString.Add(Key.LeftCtrl, "CTRL1");
            KeyCodeToString.Add(Key.RightCtrl, "CTRL2");
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
            KeyCodeToString.Add(Key.Decimal, ".");
            KeyCodeToString.Add(Key.Delete, "DEL");
            KeyCodeToString.Add(Key.Divide, "/");
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
            KeyCodeToString.Add(Key.Multiply, "*");
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
            KeyCodeToString.Add(Key.OemBackslash, @"\");
            KeyCodeToString.Add(Key.OemClear, "CLEAR");
            KeyCodeToString.Add(Key.OemCloseBrackets, ")");
            KeyCodeToString.Add(Key.OemComma, ",");
            KeyCodeToString.Add(Key.OemMinus, "-");
            KeyCodeToString.Add(Key.OemOpenBrackets, "(");
            KeyCodeToString.Add(Key.OemPeriod, ".");
            KeyCodeToString.Add(Key.OemQuestion, "?");
            KeyCodeToString.Add(Key.OemQuotes, "\"");
            KeyCodeToString.Add(Key.OemSemicolon, ";");
            KeyCodeToString.Add(Key.OemTilde, "`");
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
            KeyCodeToString.Add(Key.Subtract, "-");
            KeyCodeToString.Add(Key.T, "T");
            KeyCodeToString.Add(Key.Tab, "TAB");
            KeyCodeToString.Add(Key.U, "U");
            KeyCodeToString.Add(Key.Up, "UP");
            KeyCodeToString.Add(Key.V, "V");
            KeyCodeToString.Add(Key.W, "W");
            KeyCodeToString.Add(Key.X, "X");
            KeyCodeToString.Add(Key.Y, "Y");
            KeyCodeToString.Add(Key.Z, "Z");

        }

    }
}
