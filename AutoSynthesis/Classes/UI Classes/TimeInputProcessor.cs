using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoSynthesis
{
    static class TimeInputProcessor
    {
        private static Dictionary<Key, int> NumbersKeys { get; set; }

        static TimeInputProcessor()
        {
            // Set NumbersKeys
            NumbersKeys = new Dictionary<Key, int>();
            NumbersKeys.Add(Key.D0, 0);
            NumbersKeys.Add(Key.D1, 1);
            NumbersKeys.Add(Key.D2, 2);
            NumbersKeys.Add(Key.D3, 3);
            NumbersKeys.Add(Key.D4, 4);
            NumbersKeys.Add(Key.D5, 5);
            NumbersKeys.Add(Key.D6, 6);
            NumbersKeys.Add(Key.D7, 7);
            NumbersKeys.Add(Key.D8, 8);
            NumbersKeys.Add(Key.D9, 9);
            NumbersKeys.Add(Key.NumPad0, 0);
            NumbersKeys.Add(Key.NumPad1, 1);
            NumbersKeys.Add(Key.NumPad2, 2);
            NumbersKeys.Add(Key.NumPad3, 3);
            NumbersKeys.Add(Key.NumPad4, 4);
            NumbersKeys.Add(Key.NumPad5, 5);
            NumbersKeys.Add(Key.NumPad6, 6);
            NumbersKeys.Add(Key.NumPad7, 7);
            NumbersKeys.Add(Key.NumPad8, 8);
            NumbersKeys.Add(Key.NumPad9, 9);
        }

        public static bool TryGetNumber(Key input, ref int value)
        {
            // returns true if value exists and modifies value
            // returns false if key is not a value
            if (!NumbersKeys.ContainsKey(input))
                return false;

            value = NumbersKeys[input];
            return true;
        }
    }
}
