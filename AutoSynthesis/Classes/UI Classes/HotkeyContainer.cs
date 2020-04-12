using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput.Native;

namespace AutoSynthesis
{
    class HotkeyContainer
    {
        public bool AcceptingInputs { get; set; } = false;
        public HashSet<Key> ActiveNonModKeys { get; set; } = new HashSet<Key>();
        // Key vaues:
        public Key LastPressedKey { get; set; } = Key.None;
        public HashSet<Key> ActiveModKeys { get; set; } = new HashSet<Key>();

        public Key LastSetKey { get; set; } = Key.None;
        public HashSet<Key> LastSetModKeys { get; set; } = new HashSet<Key>();
        
        public HotkeyContainer() { }
        public HotkeyContainer(Key pressedKey, HashSet<Key> modKeys)
        {
            LastPressedKey = pressedKey;
            ActiveModKeys = modKeys;
        }

        public VirtualKeyCode[] ModKeys()
        {
            return ActiveModKeys.Select((k) => HotkeyProcessor.GetVKCFromKeyCode(k)).ToArray();
        }
        public VirtualKeyCode Keys()
        {
            return HotkeyProcessor.GetVKCFromKeyCode(LastPressedKey);
        }

        public override string ToString()
        {
            // Create comma delinated string of the hotkeys
            var output = LastPressedKey.ToString();
            foreach (var modkey in ActiveModKeys)
            {
                output += "," + modkey.ToString();
            }
            return output;
        }

        public static HotkeyContainer FromString(string input)
        {
            // Takes the comma delinated string and sets it
            List<string> inputs = input.Split(',').ToList();
            Key baseKey = Key.None;
            var modKeys = new HashSet<Key>();
            foreach (var keypress in inputs)
            {
                if (keypress == inputs.First())
                    baseKey = (Key)Enum.Parse(typeof(Key), keypress);
                else
                    modKeys.Add((Key)Enum.Parse(typeof(Key), keypress));
            }
            return new HotkeyContainer(baseKey, modKeys);
        }
    }
}
