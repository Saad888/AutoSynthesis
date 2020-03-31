using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace RapidSynthesis
{
    class Hotkey
    {
        public VirtualKeyCode KeyCode { get; set; }
        public VirtualKeyCode[] ModKeyCodes { get; set; }
        public int TimerInMiliseconds { get; set; }
        public string Text { get; set; }
        private const int DEFAULT_TIMER = 250;



        public Hotkey(VirtualKeyCode keyCode, int timer = DEFAULT_TIMER)
        {
            KeyCode = keyCode;
            ModKeyCodes = null;
            TimerInMiliseconds = timer;
        }
        public Hotkey(VirtualKeyCode keyCode, VirtualKeyCode[] modKeyCodes, string text, int timer = DEFAULT_TIMER)
        {
            KeyCode = keyCode;
            ModKeyCodes = modKeyCodes;
            TimerInMiliseconds = timer;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

    }
}
