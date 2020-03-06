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
        public int TimerInMiliseconds { get; set; }
        private const int DEFAULT_TIMER = 250;
        public Hotkey(VirtualKeyCode keyCode, int timer)
        {
            KeyCode = keyCode;
            TimerInMiliseconds = timer;
        }
        public Hotkey(VirtualKeyCode keyCode)
        {
            KeyCode = keyCode;
            TimerInMiliseconds = DEFAULT_TIMER;
        }
    }
}
