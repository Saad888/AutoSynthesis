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
        #region Class Properties and Consts
        private static Dictionary<HKType, Hotkey> HotkeySet { get; set; }
        private static SettingsContainer Settings { get; set; }
        public static bool CraftingActive { get; set; } = false;
        private static bool CraftingSuccessfullyCancelled { get; set; } = false;
        public static DateTime NextFoodUse { get; set; }
        public static DateTime NextSyrupUse { get; set; }
        public static CancellationTokenSource Cts { get; set; }
        private static Action EndCraftCallback { get; set; }

        private const int CONSUMABLE_MARGIN_IN_MINUTES = 2;
        private const int STANDARD_FOOD_TIME = 30;
        private const int EXTENDED_FOOD_TIME = 40;
        private const int STANDARD_SYRUP_TIME = 15;
        private const int STANDARD_MENU_DELAY = 1500;
        private const int STANDARD_ANIMATION_DELAY = 2000;
        private const int STANDARD_TICK_TIME = 50;
        private static int craftCount = 0;
        #endregion

        #region System Methods
        public static void InitiateCraftingEngine(Dictionary<HKType, Hotkey> hotKeyDictionary,
            SettingsContainer userSettings, Action endCraftCallback)
        {
            // Ensure craft is not already happening
            if (CraftingActive)
            {
                throw new DuplicateCraftingAttemptedException();
            }
            CraftingActive = true;

            EndCraftCallback = endCraftCallback;

            // Load process or throw error if process does not exist
            ProcessManager.LoadProcess();

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
            if (!CraftingActive)
                return;
            CraftingSuccessfullyCancelled = false;
            UICommunicator.UpdateStatus("Ending Craft...");
            Cts.Cancel();
            while (!CraftingSuccessfullyCancelled)
            {
                Thread.Sleep(100);
            }
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

            try
            {
                // Set crafting parameters
                UICommunicator.ResetValues();
                UICommunicator.UpdateStatus("Setting up for Crafting...");
                UICommunicator.StartTimedProgressBarUpdates();
                UICommunicator.UpdateCompletedUIInfo(0, Settings.CraftCount);


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
                    UICommunicator.UpdateCraftUIInfo(craftCount, Settings.CraftCount);

                    // Begin Craft Timer:
                    RunCraftProgressBar();

                    // Initiate Macro 1
                    SendMacroInput(HotkeySet[HKType.Macro1], 1);

                    // Initiate Macro 2
                    SendMacroInput(HotkeySet[HKType.Macro2], 2);

                    // Initiate Macro 3
                    SendMacroInput(HotkeySet[HKType.Macro3], 3);

                    // Update UI Message
                    UICommunicator.UpdateCompletedUIInfo(craftCount, Settings.CraftCount);

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
            } 
            catch (Exception e) when (!(e is CraftCancelRequest))
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(e.Message);
                throw e;
            } 
            finally
            {
                EndCraftingProcess();
            }
        }

        private static void EndCraftingProcess()
        {
            // ALL CLEANUP
            CraftingActive = false;
            CraftingSuccessfullyCancelled = true;
            UICommunicator.UpdateStatus("Craft Finished!");
            Thread.Sleep(STANDARD_TICK_TIME * 5); 
            UICommunicator.EndAllProgress();
            EndCraftCallback.Invoke();
        }

        private static void CheckCancelRequest()
        {
            if (Cts.IsCancellationRequested)
                throw new CraftCancelRequest();
        }
        #endregion

        #region Crafting Methods
        private static void SendMacroInput(Hotkey hotkey, int macroNumber)
        {
            if (hotkey == null)
                return;

            // UI message: MACRO NUMBER macroNumber
            UICommunicator.UpdateMacroUIInfo(macroNumber, hotkey.TimerInMiliseconds);
            SendInput(hotkey);
        }

        private static void SendCollectableConfirmationInput()
        {
            if (Settings.CollectableCraft == false)
                return;

            UICommunicator.UpdateStatus("Accepting Collectable Craft...");
            Break(STANDARD_MENU_DELAY);
            SendInput(HotkeySet[HKType.Confirm]);
            SendInput(HotkeySet[HKType.Confirm]);
        }

        private static void SendFoodAndSyrupInput()
        {
            if ((Settings.CraftCount != 0) && (craftCount < Settings.CraftCount))
                return;
                // check if timers is passed
            bool useFood = (HotkeySet[HKType.Food] != null && DateTime.Compare(NextFoodUse, DateTime.Now) <= 0);
            bool useSyrup = (HotkeySet[HKType.Syrup] != null && DateTime.Compare(NextSyrupUse, DateTime.Now) <= 0);

            // leave if neither are to be used
            if (!useFood && !useSyrup)
                return;

            // enter a craft and leave it out
            Break(500);
            SendInput(HotkeySet[HKType.Confirm], 3);
            Break(1000);
            SendInput(HotkeySet[HKType.Cancel]);
            SendInput(HotkeySet[HKType.Confirm]);
            Break(1500);

            // use food and syrup as needed
            if (useFood)
            {
                UICommunicator.UpdateStatus("Using Food...");
                SendInput(HotkeySet[HKType.Food]);
                NextFoodUse = CalculateNextConsumableUse(Settings.FoodTimerFourtyMinutes ? EXTENDED_FOOD_TIME : STANDARD_FOOD_TIME);
            }
            if (useSyrup)
            {
                UICommunicator.UpdateStatus("Using Syrup...");
                SendInput(HotkeySet[HKType.Syrup]);
                NextSyrupUse = CalculateNextConsumableUse(STANDARD_SYRUP_TIME);
            }
        }

        private static void PrepareNextCraftInput()
        {
            if ((Settings.CraftCount == 0) || (craftCount < Settings.CraftCount))
            {
                UICommunicator.UpdateStatus("Preparing Next Craft...");
                SendInput(HotkeySet[HKType.Confirm], 3);
            }
        }

        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes)
        {
            return DateTime.Now.AddMinutes(timeRemainingInMinutes - CONSUMABLE_MARGIN_IN_MINUTES);
        }

        #endregion

        #region UI Methods
        private static void RunCraftProgressBar()
        {
            int totalTime = HotkeySet[HKType.Macro1].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime += HotkeySet[HKType.Macro2].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime += HotkeySet[HKType.Macro3].TimerInMiliseconds;

            UICommunicator.BeginCraftTimer(totalTime);
        }
        #endregion

        #region Timing Methods
        private static void Break(int time)
        {
            SleepThread(time);
        }

        private static void SendInput(Hotkey hotkey, int repeat = 1)
        {
            for (int i = repeat; i > 0; i--)
            {
                KeyInputEngine.SendKeysToGame(hotkey.KeyCode, hotkey.ModKeyCodes);
                SleepThread(hotkey.TimerInMiliseconds);
            }
        }

        private static void SleepThread(int timeInMilliseconds)
        {
            var targetTimeEnd = DateTime.Now.AddMilliseconds(timeInMilliseconds);
            while (DateTime.Now <= targetTimeEnd)
            {
                CheckCancelRequest();
                Thread.Sleep(STANDARD_TICK_TIME);
            }
        }
        #endregion
    }
}
