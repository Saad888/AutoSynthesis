using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidSynthesis
{
    class TimeInputContainer
    {
        public bool FreshFocus { get; set; } = true;
        public int Timer { get; set; } = 0;
        public bool Limit { get; set; } = true; // Prevent timer from going beyond 99
        public TimeInputContainer() { }
        public TimeInputContainer(bool limit) { Limit = limit; }
    }
}
