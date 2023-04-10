using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSynthesis
{
    class SettingsContainer
    {
        // contains settings metadata
        public int CraftCount { get; set; } // Set to 0 if endless craft
        public int FoodCount { get; set; }
        public int FoodDuration { get; set; }
        public bool CollectableCraft { get; set; }
        public int StartingFoodTime { get; set; }
        public int StartingSyrupTime { get; set; }
        public int StartingDelay { get; set; }
        public int EndingDelay { get; set; }

        public SettingsContainer(int craftCount, int foodCount, bool collectableCraft, int foodDuration, int startingFoodTime, int startingSyrupTime, int startingDelay, int endingDelay)
        {
            if (craftCount < 0)
            {
                throw new ArgumentException("Crafting count less than 0");
            }

            CraftCount = craftCount;
            FoodCount = foodCount;
            CollectableCraft = collectableCraft;
            FoodDuration = foodDuration;
            StartingFoodTime = startingFoodTime;
            StartingSyrupTime = startingSyrupTime;
            StartingDelay = startingDelay;
            EndingDelay = endingDelay;
        }
    }
}
