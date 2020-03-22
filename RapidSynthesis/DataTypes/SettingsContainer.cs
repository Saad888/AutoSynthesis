﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidSynthesis
{
    class SettingsContainer
    {
        // contains settings metadata
        public int CraftCount { get; set; } // Set to 0 if endless craft
        public bool FoodTimerFourtyMinutes { get; set; } // if 40 min food, true
        public bool CollectableCraft { get; set; }
        public int StartingFoodTime { get; set; }
        public int StartingSyrupTime { get; set; }

        public SettingsContainer(int craftCount, bool collectableCraft, bool thirtyMinuteFood, int startingFoodTime, int startingSyrupTime)
        {
            if (craftCount < 0)
            {
                throw new ArgumentException("Crafting count less than 0");
            }

            CraftCount = craftCount;
            CollectableCraft = collectableCraft;
            FoodTimerFourtyMinutes = thirtyMinuteFood;
            StartingFoodTime = startingFoodTime;
            StartingSyrupTime = startingSyrupTime;
        }
    }
}