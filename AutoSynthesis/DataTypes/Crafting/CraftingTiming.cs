using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSynthesis
{
    // Used to keep track of the lenght of crafts for the UI
    class CraftingTiming
    {
        Dictionary<HKType, int> CraftingTimingInMiliseconds { get; set; }
        int TotalTimeInMiliseconds { get; set; }

        public void Add(HKType key, int timing)
        {
            CraftingTimingInMiliseconds.Add(key, timing);
            TotalTimeInMiliseconds += timing;
        }

        public void AddToTotal(int timing)
        {
            TotalTimeInMiliseconds += timing;
        }
    }
}
