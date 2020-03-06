using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RapidSynthesis
{
    static class CraftingEngine
    {
        private static Dictionary<HKType, Hotkey> HotkeySet { get; set; }
        private static SettingsContainer Settings { get; set; }
        private static bool CraftingActive { get; set; } = false;
        public static DateTime NextFoodUse { get; set; }
        public static DateTime NextSyrupUse { get; set; }
        public static CancellationTokenSource Cts { get; set; }

        private const int CONSUMABLE_MARGIN_IN_MINUTES = 2;
        private const int STANDARD_FOOD_TIME = 30;
        private const int EXTENDED_FOOD_TIME = 40;
        private const int STANDARD_SYRUP_TIME = 15;
        private const int STANDARD_MENU_DELAY = 1000;
        private const int STANDARD_ANIMATION_DELAY = 3000;
        private static int craftCount = 0;


        public static void InitiateCraftingEngine(Dictionary<HKType, Hotkey> hotKeyDictionary,
            SettingsContainer userSettings)
        {
            // Ensure craft is not already happening
            if (CraftingActive)
            {
                throw new DuplicateCraftingAttemptedException();
            }
            CraftingActive = true;

            // Verify hotkeys are built correctly
            if (hotKeyDictionary[HKType.Macro1] == null)
            {
                throw new InvalidUserParametersException("Macro 1 is Null");
            }
            if (hotKeyDictionary[HKType.Confirm] == null)
            {
                throw new InvalidUserParametersException("Confirm is Null");
            }
            if ((hotKeyDictionary[HKType.Food] != null ||
                hotKeyDictionary[HKType.Syrup] != null) &&
                hotKeyDictionary[HKType.Cancel] == null)
            {
                throw new InvalidUserParametersException("Cancel is Null");
            }

            // Store values
            HotkeySet = hotKeyDictionary;
            Settings = userSettings;



            // Run crafting system on a new thread
            Cts = new CancellationTokenSource();
            var token = Cts.Token;
            Task.Run(() => RunCraftingEngine(token), token);

        }

        public static void CancelCrafting()
        {
            Console.WriteLine("Sending Cancel");
            Cts.Cancel();
        }

        private static void RunCraftingEngine(CancellationToken token)
        {
            // PROCESS:
            // set queue parameters
            // calcualte overall time for full process
            // macro 1
            // macro 2
            // macro 3
            // collectable if needed
            // food if needed
            // pot if needed
            // begin next craft

            // Set crafting parameters
            craftCount = 1;
            if (HotkeySet[HKType.Food] != null)
                NextFoodUse = CalculateNextConsumableUse(Settings.StartingFoodTime);
            if (HotkeySet[HKType.Syrup] != null)
                NextSyrupUse = CalculateNextConsumableUse(Settings.StartingSyrupTime);


            // If crafts remaining was 0, loop infinitley
            // If not, craft until quota is met
            while ((Settings.CraftCount == 0) || (craftCount <= Settings.CraftCount))
            {
                // UI MESSAGE: Set timer for overall craft

                // Initiate Macro 1
                SendMacroInput(HotkeySet[HKType.Macro1], 1);

                // Initiate Macro 2
                SendMacroInput(HotkeySet[HKType.Macro2], 2);

                // Initiate Macro 3
                SendMacroInput(HotkeySet[HKType.Macro3], 3);

                // Collectable Menu Option
                SendCollectableConfirmationInput();

                // Standard delay for menus
                Break(STANDARD_MENU_DELAY);

                // Use Food and Syrup
                SendFoodAndSyrupInput();

                // Prepare next craft if crafting is not finished
                PrepareNextCraftInput();
                Break(STANDARD_ANIMATION_DELAY);

                craftCount += 1;
            }
            EndCraftingProcess();
        }

        #region crafting methods
        private static void SendMacroInput(Hotkey hotkey, int macroNumber)
        {
            if (hotkey == null)
                return;

            // UI message: MACRO NUMBER macroNumber
            SendInput(hotkey);
        }

        private static void SendCollectableConfirmationInput()
        {
            if (Settings.CollectableCraft == false)
                return;

            SendInput(HotkeySet[HKType.Confirm]);
        }

        private static void SendFoodAndSyrupInput()
        {
            // check if timers is passed
            bool useFood = (HotkeySet[HKType.Food] != null && DateTime.Compare(NextFoodUse, DateTime.Now) <= 0);
            bool useSyrup = (HotkeySet[HKType.Syrup] != null && DateTime.Compare(NextSyrupUse, DateTime.Now) <= 0);

            // leave if neither are to be used
            if (!useFood && !useSyrup)
                return;

            // enter a craft and leave it out
            SendInput(HotkeySet[HKType.Confirm], 3);
            Break(500);
            SendInput(HotkeySet[HKType.Cancel]);
            SendInput(HotkeySet[HKType.Confirm]);
            Break(1000);

            // use food and syrup as needed
            if (useFood)
            {
                SendInput(HotkeySet[HKType.Food]);
                NextFoodUse = CalculateNextConsumableUse(Settings.FoodTimerFourtyMinutes ? EXTENDED_FOOD_TIME : STANDARD_FOOD_TIME);
            }
            if (useSyrup)
            {
                SendInput(HotkeySet[HKType.Syrup]);
                NextSyrupUse = CalculateNextConsumableUse(STANDARD_SYRUP_TIME);
            }
        }

        private static void PrepareNextCraftInput()
        {
            if ((Settings.CraftCount == 0) || (craftCount < Settings.CraftCount))
                SendInput(HotkeySet[HKType.Confirm], 3);
        }
        #endregion

        private static void Break(int time)
        {
            Thread.Sleep(time);
        }
        private static void SendInput(Hotkey hotkey, int repeat = 1)
        {
            for (int i = repeat; i > 0; i--)
            {
                KeyInputEngine.SendKeysToGame(hotkey.KeyCode, hotkey.ModKeyCodes);
                Thread.Sleep(hotkey.TimerInMiliseconds);
            }
        }

        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes)
        {
            return DateTime.Now.AddMinutes(timeRemainingInMinutes - CONSUMABLE_MARGIN_IN_MINUTES);
        }

        private static void EndCraftingProcess()
        {
            // ALL CLEANUP
            CraftingActive = false;
        }
    }
}
